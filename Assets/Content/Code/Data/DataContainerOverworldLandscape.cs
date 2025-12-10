using System;
using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;
using Random = UnityEngine.Random;

#if !PB_MODSDK
using Pathfinding;
#endif

namespace PhantomBrigade.Data
{
    public class DataBlockLandscapePointGroup
    {
        [YamlIgnore]
        public string key;
        public List<Vector3> points = new List<Vector3> ();
        
        
        #if UNITY_EDITOR
        
        [Button ("$" + nameof(GetSelectLabel)), PropertyOrder (-2), ButtonGroup ("T")]
        public void SelectForEditing ()
        {
            if (DataMultiLinkerOverworldLandscape.selectionPointGroupKey == key)
            {
                DataMultiLinkerOverworldLandscape.selectionPointGroupKey = null;
                DataMultiLinkerOverworldLandscape.selectionPointIndex = -1;
            }
            else
            {
                DataMultiLinkerOverworldLandscape.selectionPointGroupKey = key;
                DataMultiLinkerOverworldLandscape.selectionPointIndex = points != null && points.Count > 0 ? 0 : -1;
            }
        }

        private string GetSelectLabel => DataMultiLinkerOverworldLandscape.selectionPointGroupKey == key ? "Deselect" : "Select";
        
        #endif
    }

    public class DataBlockOverworldLandscapeBiome : DataBlockFilterLinked<DataContainerCombatBiome>
    {
        public override IEnumerable<string> GetTags () => DataMultiLinkerCombatBiome.GetTags ();
        public override SortedDictionary<string, DataContainerCombatBiome> GetData () => DataMultiLinkerCombatBiome.data;
    }

    public class DataBlockOverworldLandscapeLayer
    {
        [PropertyOrder (-1)]
        [PropertyRange (0f, 1f)]
        [OnValueChanged ("OnHeightChange")]
        public float heightNormalized;

        [DropdownReference (true)]
        public DataBlockFloat temperature;
        
        [DropdownReference (true)]
        public DataBlockVector3 precipitationFactors;
        
        [DropdownReference (true)]
        public DataBlockOverworldLandscapeBiome biome;
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldLandscapeLayer () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        private void OnHeightChange ()
        {
            if (DataMultiLinkerOverworldLandscape.Presentation.showLayerHeight)
                VisualizeHeight ();
        }
        
        private void VisualizeHeight ()
        {
            OverworldLandscapeManager.shaderDimensionSlice = heightNormalized;
            OverworldLandscapeManager.RefreshGlobals ();
        }
        
        #endif
        #endregion
    }
    
    public class DataContainerOverworldLandscape : DataContainer
    {
        [ValueDropdown ("$GetAssetKeys")]
        public string assetKey;
        
        public float heightFull = 100f;

        [FoldoutGroup ("Height test")]
        [YamlIgnore, OnValueChanged ("OnHeightTest")]
        [PropertyRange (0f, 1f), HideLabel]
        public float heightTest = 0f;

        [FoldoutGroup ("Height test")]
        [YamlIgnore, ReadOnly, TextArea (2, 10), HideLabel]
        public string heightOutput = string.Empty;

        public float spawnPaddingEdge = 100f;
        public float spawnPaddingGroup = 25f;
        public float spawnInterval = 25f;
        public float spawnNormalThreshold = 0.5f;
        
        [PropertyRange (0f, 1f)]
        public float spawnHeightMin = 0f;
        
        [PropertyRange (0f, 1f)]
        public float spawnHeightMax = 1f;
        
        [PropertyRange (0f, 90f)]
        public float navSlopeLimit = 40f;
        
        [OnValueChanged ("OnPropNormalChange")]
        [MinMaxSlider (0f, 1f, true)]
        public Vector2 propNormalDotRange = new Vector2 (0.1f, 0.98f);

        [ListDrawerSettings (DefaultExpandedState = false, ShowPaging = false)]
        [DropdownReference]
        public List<Vector3> spawnsGeneral = new List<Vector3> ();

        [OnValueChanged (nameof(OnGroupChange), true)]
        [DropdownReference]
        public SortedDictionary<string, DataBlockLandscapePointGroup> pointGroups = new SortedDictionary<string, DataBlockLandscapePointGroup> ();

        [DropdownReference]
        public List<DataBlockOverworldLandscapeLayer> layers;
        

        private static List<Vector3> pointsBuffer = new List<Vector3> ();
        private static List<Vector3> pointsFiltered = new List<Vector3> ();
        private static List<string> keysToFill = new List<string> ();
        private static float navNodeDistanceLimitSqr = 25f;
        
        #if !PB_MODSDK
        private static NNConstraint navConstraint = new NNConstraint { walkable = true, constrainWalkability = true };
        #endif

        private void OnPropNormalChange ()
        {
            var x = Mathf.RoundToInt (propNormalDotRange.x * 100) / 100f;
            var y = Mathf.RoundToInt (propNormalDotRange.y * 100) / 100f;
            propNormalDotRange = new Vector2 (x, y);
        }

        public void GetLayerDataAtHeight (float height, out float temperature, out Vector3 precipitationFactors, out DataBlockOverworldLandscapeBiome biome)
        {
            temperature = 1f;
            precipitationFactors = new Vector3 (0.5f, 0f, 1f);
            biome = null;
            
            if (layers == null)
                return;

            for (int i = 0; i < layers.Count; ++i)
            {
                var layer = layers[i];
                if ((layer.heightNormalized * heightFull) > height)
                    continue;
                
                if (layer.temperature != null)
                    temperature = layer.temperature.f;
                
                if (layer.precipitationFactors != null)
                    precipitationFactors = layer.precipitationFactors.v;

                if (layer.biome != null)
                    biome = layer.biome;
            }
        }

        private void OnGroupChange ()
        {
            #if !PB_MODSDK
            if (pointGroups != null)
            {
                foreach (var kvp in pointGroups)
                {
                    if (kvp.Value == null)
                        keysToFill.Add (kvp.Key);
                    else
                        kvp.Value.key = kvp.Key;
                }

                if (keysToFill.Count > 0)
                {
                    foreach (var key1 in keysToFill)
                        pointGroups[key1] = new DataBlockLandscapePointGroup { key = key1 };
                    keysToFill.Clear ();
                }
            }
            #endif
        }

        [PropertyOrder (-1)]
        [Button ("Add standard groups"), ButtonGroup ("P")]
        private void FillStandardGroups ()
        {
            TryAddPointGroup ("poi");
            TryAddPointGroup ("border");
        }

        private void TryAddPointGroup (string groupKey)
        {
            if (string.IsNullOrEmpty (groupKey))
                return;
            
            if (pointGroups == null)
                pointGroups = new SortedDictionary<string, DataBlockLandscapePointGroup> ();
            
            if (!pointGroups.ContainsKey (groupKey))
                pointGroups.Add (groupKey, new DataBlockLandscapePointGroup { key = groupKey, points = new List<Vector3> () });
        }
        
        [PropertyOrder (-1)]
        [Button ("Refresh height"), ButtonGroup ("P")]
        private void RefreshHeight ()
        {
            DataMultiLinkerOverworldLandscape.selection = this;

            bool assetLoaded = OverworldLandscapeManager.TryLoadingVisual (assetKey);
            if (!assetLoaded)
                return;
            
            var bounds = OverworldLandscapeManager.GetBounds ();
            heightFull = bounds.y;
        }

        private static float normalTestingScale = 3f;
        private static List<Vector3> normalTestingOffsets = new List<Vector3>
        {
            new Vector3 (0f, 0f, 0f),
            new Vector3 (0.7f, 0f, 0.7f),
            new Vector3 (-0.7f, 0f, 0.7f),
            new Vector3 (-0.7f, 0f, -0.7f),
            new Vector3 (0.7f, 0f, -0.7f),
            new Vector3 (1f, 0f, 0f),
            new Vector3 (0, 0f, 1f),
            new Vector3 (-1f, 0f, 0f),
            new Vector3 (0f, 0f, -1f)
        };
        
        [PropertyOrder (-1)]
        [Button ("Generate gen. points"), ButtonGroup ("P")]
        private void RegenerateSpawnsGeneral ()
        {
            RefreshHeight ();
            
            spawnsGeneral = new List<Vector3> ();
            DataMultiLinkerOverworldLandscape.selection = this;

            bool assetLoaded = OverworldLandscapeManager.TryLoadingVisual 
            (
                assetKey, 
                true, 
                navSlopeLimit,
                propNormalDotRange
            );
            
            if (!assetLoaded)
                return;

            var bounds = OverworldLandscapeManager.GetBounds ();
            
            float xMax = bounds.x * 0.5f - spawnPaddingEdge;
            float xMin = -xMax;
            
            float zMax = bounds.z * 0.5f - spawnPaddingEdge;
            float zMin = -zMax;

            float xSize = xMax - xMin;
            float zSize = zMax - zMin;
            
            if (xSize < 0 || zSize < 0)
                return;

            spawnInterval = Mathf.Clamp (spawnInterval, 5f, 100f);
            spawnPaddingEdge = Mathf.Clamp (spawnPaddingEdge, 5f, 100f);
            spawnNormalThreshold = Mathf.Clamp (spawnNormalThreshold, 0f, 1f);
            
            float spawnPaddingEdgeSqr = spawnPaddingEdge * spawnPaddingEdge;
            float spawnPaddingGroupSqr = spawnPaddingGroup * spawnPaddingGroup;
            
            int xSteps = Mathf.FloorToInt (xSize / spawnInterval);
            int zSteps = Mathf.FloorToInt (zSize / spawnInterval);
            int count = xSteps * zSteps;
            var posBase = new Vector3 (xMin, 0f, zMin);
            
            var radius = Mathf.Min (xSize, zSize) * 0.5f;
            var radiusSqr = radius * radius;

            var spawnHeightMinScaled = spawnHeightMin * heightFull;
            var spawnHeightMaxScaled = spawnHeightMax * heightFull;
            
            for (int xStep = 0; xStep < xSteps; ++xStep)
            {
                for (int zStep = 0; zStep < zSteps; ++zStep)
                {
                    var posChecked = posBase + new Vector3 (xStep * spawnInterval, 0f, zStep * spawnInterval);
                    float xMul = (float)xStep / xSteps;
                    float yMul = (float)zStep / zSteps;

                    var groundingRayOrigin = new Vector3 (posChecked.x, 200f, posChecked.z);
                    var groundingRay = new Ray (groundingRayOrigin, Vector3.down);
                    if (Physics.Raycast (groundingRay, out var hit, 400f, LayerMasks.environmentMask))
                        posChecked = hit.point;

                    if (Vector3.Dot (Vector3.up, hit.normal) < spawnNormalThreshold)
                        continue;
                    
                    var distRadialSqr = new Vector2 (posChecked.x, posChecked.z).sqrMagnitude;
                    if (distRadialSqr > radiusSqr)
                        continue;
                    
                    if (posChecked.y < spawnHeightMinScaled || posChecked.y > spawnHeightMaxScaled)
                        continue;

                    int normalAverageCount = 0;
                    var normalAverage = Vector3.zero;
                    bool dotPassed = true;

                    for (int n = 0, nLimit = normalTestingOffsets.Count; n < nLimit; ++n)
                    {
                        var posOffset = normalTestingOffsets[n] * normalTestingScale + posChecked;
                        var rayOffset = new Ray (new Vector3 (posOffset.x, 200f, posOffset.z), Vector3.down);
                        if (Physics.Raycast (rayOffset, out var hitOffset, 400f, LayerMasks.environmentMask))
                        {
                            normalAverage += hitOffset.normal;
                            normalAverageCount += 1;

                            dotPassed = Vector3.Dot (Vector3.up, hitOffset.normal) > spawnNormalThreshold;
                            Debug.DrawLine (hitOffset.point, hitOffset.point + hitOffset.normal, dotPassed ? Color.HSVToRGB (xMul, yMul * 0.5f, 1f) : Color.red, 5f);
                            
                            if (!dotPassed)
                                break;
                        }
                    }
                    
                    if (!dotPassed)
                        continue;

                    if (normalAverageCount > 0)
                    {
                        normalAverage = normalAverage / (float)normalAverageCount;
                        if (Vector3.Dot (Vector3.up, normalAverage) < spawnNormalThreshold)
                            continue;
                    }

                    if (pointGroups != null)
                    {
                        bool groupNear = false;
                        foreach (var kvp in pointGroups)
                        {
                            var points = kvp.Value?.points;
                            if (points == null || points.Count == 0)
                                continue;
                            
                            for (int i = 0, iLimit = points.Count; i < iLimit; ++i)
                            {
                                var spawnGroupPos = points[i];
                                if ((spawnGroupPos - posChecked).sqrMagnitude < spawnPaddingGroupSqr)
                                {
                                    groupNear = true;
                                    break;
                                }
                            }
                            
                            if (groupNear)
                                break;
                        }

                        if (groupNear)
                            continue;
                    }

                    #if !PB_MODSDK
                    if (Application.isPlaying)
                    {
                        // Discard positions with no nearby node
                        var nnInfo = AstarPath.active.GetNearest (posChecked, navConstraint);
                        if (nnInfo.node == null)
                            continue;

                        // Discard positions with nodes that are too far
                        Vector3 navNodeDelta = nnInfo.position.Flatten () - posChecked.Flatten ();
                        if (navNodeDelta.sqrMagnitude > navNodeDistanceLimitSqr)
                            continue;
                    }
                    #endif

                    Debug.DrawLine (posChecked, posChecked + Vector3.up * 20f, Color.HSVToRGB (xMul, yMul, 1f), 5f);
                    spawnsGeneral.Add (posChecked);
                }
            }

            Debug.Log ($"Province {key} dimensions: {xSize:0.##} x {zSize:0.##} | X: {xMin:0.##} <-> {xMax:0.##} | Z: {xMin:0.##} <-> {xMax:0.##} | Points: {xSteps} x {zSteps} = {count} | Points after trimming: {spawnsGeneral.Count}");
        }
        
        [Button ("Generate navmesh"), ButtonGroup ("RT"), HideInEditorMode]
        private void RegenerateNav ()
        {
            RefreshHeight ();
            OverworldLandscapeManager.TryGeneratingNav (navSlopeLimit);
        }
        
        [Button ("Generate props"), ButtonGroup ("RT"), HideInEditorMode]
        private void RegenerateProps ()
        {
            RefreshHeight ();
            OverworldLandscapeManager.TryGeneratingProps (propNormalDotRange);
        }
        
        public List<Vector3> TryGetPointGroup (string groupKey)
        {
            if (pointGroups == null || pointGroups.Count == 0)
                return null;

            if (string.IsNullOrEmpty (groupKey) || !pointGroups.TryGetValue (groupKey, out var group) || group == null || group.points == null || group.points.Count == 0)
            {
                Debug.LogWarning ($"Landscape {key} has no point group {groupKey}");
                return null;
            }

            return group.points;
        }

        public bool TryGetPointInGroup (string groupKey, int pointIndex, out Vector3 result)
        {
            result = Vector3.zero;
            
            if (pointGroups == null || pointGroups.Count == 0)
                return false;

            if (string.IsNullOrEmpty (groupKey) || !pointGroups.TryGetValue (groupKey, out var group) || group == null || group.points == null || group.points.Count == 0)
            {
                Debug.LogWarning ($"Landscape {key} has no point group {groupKey}, can't return a point");
                return false;
            }
            
            var points = group.points;
            if (pointIndex < 0)
            {
                result = points.GetRandomEntry ();
                return true;
            }

            if (pointIndex >= points.Count)
            {
                Debug.LogWarning ($"Landscape {key} has the point group {groupKey} ({points.Count} points), but doesn't contain index {pointIndex}");
                return false;
            }

            result = points[pointIndex];
            return true;
        }

        public bool TryGetPointAnywhere
        (
            out Vector3 result,
            string groupKey = null,
            PointDistancePriority distancePriority = PointDistancePriority.None
        )
        {
            return TryGetPointInRange (Vector3.zero, new Vector2 (0f, 10000f), out result, groupKey, distancePriority);
        }

        public bool TryGetPointInRange 
        (
            Vector3 origin, 
            Vector2 rangeBand, 
            out Vector3 result,
            string groupKey = null,
            PointDistancePriority distancePriority = PointDistancePriority.None,
            bool allowFallback = true
        )
        {
            result = default;
            
            float rangeMin = Mathf.Max (0f, rangeBand.x);
            float rangeMax = Mathf.Max (rangeMin, rangeBand.y);
            bool rangeChecked = rangeMax > rangeMin;
            
            var points = TryGetPointsInRange (origin, rangeBand, 1, groupKey, distancePriority, allowFallback: allowFallback);
            if (points != null && points.Count > 0)
            {
                result = points[0];
                return true;
            }

            if (rangeChecked)
            {
                var pointsWithoutRange = TryGetPointsInRange (origin, default, 1, groupKey, distancePriority, allowFallback: allowFallback);
                if (pointsWithoutRange != null && pointsWithoutRange.Count > 0)
                {
                    result = pointsWithoutRange[0];
                    return true;
                }
            }

            return false;
        }

        public List<Vector3> TryGetPointsInRange 
        (
            Vector3 origin, 
            Vector2 rangeBand, 
            int countRequired, 
            string groupKey = null,
            PointDistancePriority distancePriority = PointDistancePriority.None,
            List<Vector3> exclusionPoints = null,
            bool allowFallback = true
            // float distFromOriginLimit = -1f
            // bool separationNeeded = true
        )
        {
            pointsBuffer.Clear ();
            pointsFiltered.Clear ();

            if (countRequired <= 0)
                return pointsFiltered;
            
            bool distFromOriginCheck = true;
            bool separationNeeded = true;
            var pointList = spawnsGeneral;
            
            if (!string.IsNullOrEmpty (groupKey))
            {
                DataBlockLandscapePointGroup group = null;
                bool groupFound = pointGroups != null && pointGroups.TryGetValue (groupKey, out group);
                if (groupFound && group != null && group.points != null && group.points.Count > 0)
                {
                    pointList = group.points;
                    separationNeeded = group.points.Count > 1;
                    distFromOriginCheck = false;
                }
                else
                {
                    if (allowFallback)
                        Debug.LogWarning ($"Issue finding points in range in landscape {key} | Group {groupKey} does not exist, falling back to default group");
                    else
                    {
                        Debug.LogWarning ($"Issue finding points in range in landscape {key} | Group {groupKey} does not exist, fallback not allowed, returning nothing");
                        return pointsFiltered;
                    }
                }
            }

            bool exclusionChecks = exclusionPoints != null && exclusionPoints.Count > 0;
            if (pointList == null || pointList.Count == 0)
            {
                Debug.LogWarning ($"Failed to find points in range in landscape {key}, falling back to default group");
                return pointsFiltered;
            }
            
            float rangeMin = Mathf.Max (0f, rangeBand.x);
            float rangeMax = Mathf.Max (rangeMin, rangeBand.y);
            bool rangeChecked = rangeMax > rangeMin;
            float rangeMinSqr = 0f, rangeMaxSqr = 0f;
            
            if (rangeChecked)
            {
                rangeMinSqr = rangeMin * rangeMin;
                rangeMaxSqr = rangeMax * rangeMax;
            }

            float entitySeparation = DataShortcuts.overworld.pointSeparationDefault;
            float entitySeparationSqr = entitySeparation * entitySeparation;

            int baseOverworldID = IDUtility.invalidID;
            List<OverworldEntity> entitiesExisting = null;
            
            #if !PB_MODSDK
            if (separationNeeded && Application.isPlaying && entitySeparation > 0f)
            {
                entitiesExisting = OverworldPointUtility.GetActivePoints (false, false);
                var baseOverworld = IDUtility.playerBaseOverworld;
                if (baseOverworld != null && baseOverworld.hasId)
                    baseOverworldID = baseOverworld.id.id;
            }
            #endif
            
            float distFromOriginMax = 200f; // distFromOriginLimit < 1f ? 200f : 0f;
            float distFromOriginMaxSqr = distFromOriginMax * distFromOriginMax;
            
            for (int i = 0, iLimit = pointList.Count; i < iLimit; ++i)
            {
                var spawnCandidate = pointList[i];
                
                // Skip points outside of min/max range band
                var distSqrOrigin = (spawnCandidate - origin).sqrMagnitude;
                if (distSqrOrigin < rangeMinSqr || distSqrOrigin > rangeMaxSqr)
                    continue;
                
                if (distFromOriginCheck)
                {
                    var distFromOrigin2DSqr = new Vector2 (spawnCandidate.x, spawnCandidate.z).sqrMagnitude;
                    if (distFromOrigin2DSqr > distFromOriginMaxSqr)
                        continue;
                }
                
                // Skip any points near existing sites
                #if !PB_MODSDK
                if (separationNeeded && entitiesExisting != null && entitySeparation > 0f)
                {
                    bool eliminated = false;
                    for (int e = 0, eLimit = entitiesExisting.Count; e < eLimit; ++e)
                    {
                        var entityOverworld = entitiesExisting[e];
                        if (entityOverworld == null || entityOverworld.isDestroyed || !entityOverworld.hasPosition || entityOverworld.isHidden || !entityOverworld.hasId)
                            continue;
                        
                        if (entityOverworld.id.id == baseOverworldID)
                            continue;

                        var entityPos = entityOverworld.position.v;
                        var distSqrEntity = (spawnCandidate - entityPos).sqrMagnitude;
                        if (distSqrEntity < entitySeparationSqr)
                        {
                            eliminated = true;
                            break;
                        }
                    }

                    if (eliminated)
                        continue;
                }
                #endif

                if (exclusionChecks)
                {
                    bool eliminated = false;
                    for (int e = 0, eLimit = exclusionPoints.Count; e < eLimit; ++e)
                    {
                        var exclusionPoint = exclusionPoints[e];
                        var distSqrExclusionPoint = (spawnCandidate - exclusionPoint).sqrMagnitude;
                        if (distSqrExclusionPoint < 100)
                        {
                            eliminated = true;
                            break;
                        }
                    }

                    if (eliminated)
                        continue;
                }
                    
                pointsBuffer.Add (spawnCandidate);
            }

            if (pointsBuffer.Count == 0)
                return pointsFiltered;

            if (distancePriority == PointDistancePriority.Closest)
                pointsBuffer.Sort ((x, y) => (x - origin).sqrMagnitude.CompareTo ((y - origin).sqrMagnitude));
            else if (distancePriority == PointDistancePriority.Furthest)
                pointsBuffer.Sort ((x, y) => (y - origin).sqrMagnitude.CompareTo ((x - origin).sqrMagnitude));

            for (int i = 0; i < countRequired; ++i)
            {
                int indexSelected;
                if (distancePriority == PointDistancePriority.Closest)
                    indexSelected = 0;
                else if (distancePriority == PointDistancePriority.Furthest)
                    indexSelected = 0;
                else
                    indexSelected = Random.Range (0, pointsBuffer.Count);
                
                var pointSelected = pointsBuffer[indexSelected];
                // Debug.Log ($"Selected point {i+1}/{countRequired} | Index {indexSelected+1}/{pointsBuffer.Count} | Distance: {(pointSelected - origin).magnitude:0.#} | Mode: {distancePriority}");
                
                pointsBuffer.RemoveAt (indexSelected);
                pointsFiltered.Add (pointSelected);

                // Eliminate all points in the buffer near the picked one
                for (int b = pointsBuffer.Count - 1; b >= 0; --b)
                {
                    var pointChecked = pointsBuffer[b];
                    var distSqr = (pointChecked - pointSelected).sqrMagnitude;
                    if (distSqr < entitySeparationSqr)
                    {
                        pointsBuffer.RemoveAt (b);
                        if (pointsBuffer.Count == 0)
                            break;
                    }
                }

                if (pointsBuffer.Count == 0 && i < (countRequired - 1))
                {
                    Debug.LogWarning ($"Failed to get {countRequired} points from buffer in province {key} | Point last attempted: {i} | Last picked index: {indexSelected} | Points found: {pointsFiltered.Count}");
                    break;
                }
            }
            
            return pointsFiltered;
        }

        private IEnumerable<string> GetAssetKeys ()
        {
            return OverworldLandscapeManager.GetAssetKeys ();
        }

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
            OnGroupChange ();
            
            if (layers != null)
            {
                layers.Sort ((x, y) => x.heightNormalized.CompareTo (y.heightNormalized));
                foreach (var layer in layers)
                {
                    if (layer != null && layer.biome != null)
                        layer.biome.Refresh ();
                }
            }
            
            #if UNITY_EDITOR
            #endif
        }
        
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;

        public DataContainerOverworldLandscape () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        private static StringBuilder sb = new StringBuilder ();

        #if PB_MODSDK

        private string GetVisualWarning () => AssetPackageHelper.landscapeAssetWarning;
        private bool AreVisualsUnavailable () => !AssetPackageHelper.AreLandscapeAssetsInstalled ();
        
        [InfoBox ("$GetVisualWarning", InfoMessageType.Warning, VisibleIf = "$AreVisualsUnavailable")]
        #endif
        [Button ("Select"), PropertyOrder (-3), HideIf (nameof(IsSelectedInInspector))]
        public void SelectToInspector ()
        {
            DataMultiLinkerOverworldLandscape.selection = this;

            OverworldLandscapeManager.TryLoadingVisual 
            (
                assetKey, 
                true, 
                navSlopeLimit,
                propNormalDotRange
            );
        }
        
        [Button ("Deselect"), PropertyOrder (-3), ShowIf (nameof(IsSelectedInInspector))]
        public void DeselectInInspector ()
        {
            DataMultiLinkerOverworldLandscape.selection = null;
        }
        
        private bool IsSelectedInInspector ()
        {
            return DataMultiLinkerOverworldLandscape.selection == this;
        }

        private void OnHeightTest ()
        {
            sb.Clear ();
            
            GetLayerDataAtHeight (heightTest * heightFull, out float temperature, out Vector3 precipitationFactors, out var biome);

            sb.Append ("Temperature: ");
            sb.Append (temperature.ToString ("0.#"));

            sb.Append ("\nPrec. chance: ");
            sb.Append (precipitationFactors.x.ToString ("0.#"));
            
            sb.Append ("\nPrec. range: ");
            sb.Append (precipitationFactors.y.ToString ("0.#"));
            sb.Append (" - ");
            sb.Append (precipitationFactors.z.ToString ("0.#"));
            
            sb.Append ("\nBiome filter: ");
            if (biome != null && biome.configsFiltered != null)
            {
                sb.Append ("\n");
                sb.Append (biome.configsFiltered.ToStringFormatted (true, multilinePrefix: "- ", toStringOverride: (x) => x.key));
            }
            else
                sb.Append ("None");

            heightOutput = sb.ToString ();
        }
        
        #endif
    }
}
