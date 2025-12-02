using System;
using System.Collections.Generic;
using Area;
using PhantomBrigade.Functions;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [Serializable, HideReferenceObjectPicker]
    public class DataBlockEnvironmentBoundary
    {
        public float groundHeight;

        public float innerSkirtSize = 60f;
        
        public float curveBoundsOffset = 100f;

        public float curveBoundsHeight = 50f;
        
        [LabelText("North (Z+)")][YamlIgnore, HideReferenceObjectPicker]
        public AnimationCurveContainer north;
        
        [YamlMember (Alias = "north"), HideInInspector] 
        public AnimationCurveSerialized northSerialized;
        
        [LabelText("South (Z-)")][YamlIgnore, HideReferenceObjectPicker]
        public AnimationCurveContainer south;
        
        [YamlMember (Alias = "south"), HideInInspector] 
        public AnimationCurveSerialized southSerialized;
        
        [LabelText("East (X+)")][YamlIgnore, HideReferenceObjectPicker]
        public AnimationCurveContainer east;
        
        [YamlMember (Alias = "east"), HideInInspector] 
        public AnimationCurveSerialized eastSerialized;
        
        [LabelText("West (X-)")][YamlIgnore, HideReferenceObjectPicker]
        public AnimationCurveContainer west;
        
        [YamlMember (Alias = "west"), HideInInspector] 
        public AnimationCurveSerialized westSerialized;
        
        public void OnBeforeSerialization ()
        {
            if (north != null)
                northSerialized = (AnimationCurveSerialized) north.curve;
          
            if (south != null)
                southSerialized = (AnimationCurveSerialized) south.curve;
                
            if (east != null)
                eastSerialized = (AnimationCurveSerialized) east.curve;
            
            if (west != null)
                westSerialized = (AnimationCurveSerialized) west.curve;
        }

        public void OnAfterDeserialization ()
        {
            if (northSerialized != null)
                north = new AnimationCurveContainer ((AnimationCurve) northSerialized);
                
            if (southSerialized != null)
                south = new AnimationCurveContainer ((AnimationCurve) southSerialized);
            
            if (eastSerialized != null)
                east = new AnimationCurveContainer ((AnimationCurve) eastSerialized);
            
            if (westSerialized != null)
                west = new AnimationCurveContainer ((AnimationCurve) westSerialized);
        }

        [Button ("Update Boundary Terrain")]
        public void UpdateBoundary ()
        {
            var sceneHelper = CombatSceneHelper.ins;
            if (sceneHelper == null)
                return;
        
            if (sceneHelper.boundary != null)
                sceneHelper.LoadBoundary (this);
        }
    }
    
    public static class SpawnGroupKeys
    {
        public const string Player = "player";
    }
    
    [Serializable, HideReferenceObjectPicker]
    public class DataBlockAreaPoint
    {
        [InlineButton ("SnapToGrid", "Snap")]
        public Vector3 point;
        public Vector3 rotation;

        [YamlIgnore, HideInInspector]
        public int index = -1;
        
        #if UNITY_EDITOR

        public void SnapToGrid ()
        {
            point = AreaUtility.SnapPointToGrid (point);
            UnityEditor.SceneView.RepaintAll ();
        }
        
        public void SnapRotation ()
        {
            rotation = new Vector3
            (
                0,
                RoundToStep (rotation.y, 45),
                0
            );
            UnityEditor.SceneView.RepaintAll ();
        }
        
        private float RoundToStep (float input, float step)
        {
            if (step.RoughlyEqual (0f))
                return input;
            
            return Mathf.Round (input / step) * step;
        }

        public void Ground ()
        {
            point = AreaUtility.GroundPoint (point);
            UnityEditor.SceneView.RepaintAll ();
        }
        
        #endif
    }
    
    [Serializable, HideReferenceObjectPicker]
    public class DataBlockAreaSpawnGroup
    {
        [YamlIgnore, HideInInspector, NonSerialized]
        public string key;
        
        [ValueDropdown ("GetTags")]
        public HashSet<string> tags = new HashSet<string> ();
        
        [ListDrawerSettings (DefaultExpandedState = false, AlwaysAddDefaultValue = true)]
        public List<DataBlockAreaPoint> points = new List<DataBlockAreaPoint> ();
        
        [YamlIgnore, ReadOnly] 
        public Vector3 averagePosition;

        public void RefreshAveragePosition ()
        {
            averagePosition = Vector3.zero;
            if (points == null || points.Count == 0)
                return;
                
            foreach (var pointGroup in points)
                averagePosition += pointGroup.point;
            averagePosition /= points.Count;
        }

        #region Editor
        #if UNITY_EDITOR
        
        private IEnumerable<string> GetTags =>
            DataShortcuts.sim.combatSpawnTags;
        
        [Button ("@GetSelectLabel"), PropertyOrder (-2), ButtonGroup ("T")]
        public void SelectForEditing ()
        {
            if (DataMultiLinkerCombatArea.selectedSpawnGroup == this)
            {
                DataMultiLinkerCombatArea.selectedSpawnGroup = null;
                DataMultiLinkerCombatArea.selectedSpawnPoint = null;
            }
            else
            {
                DataMultiLinkerCombatArea.selectedSpawnGroup = this;
                if (points != null && points.Count > 0)
                    DataMultiLinkerCombatArea.selectedSpawnPoint = points[0];
            }
        }

        private string GetSelectLabel => DataMultiLinkerCombatArea.selectedSpawnGroup == this ? "Deselect" : "Select";

        [Button ("Snap"), PropertyOrder (-2), ButtonGroup ("T")]
        public void SnapToGrid ()
        {
            if (points == null)
                return;

            foreach (var point in points)
                point.SnapToGrid ();
        }

        [Button ("Ground"), PropertyOrder (-2), ButtonGroup ("T")]
        public void Ground ()
        {
            if (points == null)
                return;

            foreach (var point in points)
                point.Ground ();
        }

        [Button ("Linearize"), PropertyOrder (-2), ButtonGroup ("T")]
        public void Linearize (string keyNew)
        {
            if (points == null || points.Count <= 2)
                return;

            var p1 = points[0];
            var p2 = points[points.Count - 1];

            for (int i = 1, limit = points.Count - 1; i < limit; ++i)
            {
                var p = points[i];
                var factor = (float)i / (limit);
                p.point = Vector3.Lerp (p1.point, p2.point, factor);
                p.rotation = Vector3.Lerp (p1.rotation, p2.rotation, factor);
            }
            
            UnityEditor.SceneView.RepaintAll ();
        }
        
        [Button ("Remove"), PropertyOrder (-2), ButtonGroup ("T")]
        public void Remove ()
        {
            if 
            (
                string.IsNullOrEmpty (key) || 
                DataMultiLinkerCombatArea.selectedArea?.spawnGroups == null ||
                !DataMultiLinkerCombatArea.selectedArea.spawnGroups.ContainsValue (this)
            )
            {
                return;
            }

            DataMultiLinkerCombatArea.selectedArea.spawnGroups.Remove (key);
        }
        
        [ShowIf ("@DataMultiLinkerCombatArea.selectedArea != null")]
        [Button ("Rename/Duplicate"), PropertyOrder (-2)]
        public void RenameDuplicate (bool duplicate, string keyNew)
        {
            if (DataMultiLinkerCombatArea.selectedArea == null)
                return;

            var groups = DataMultiLinkerCombatArea.selectedArea.spawnGroups;
            if (groups.ContainsKey (keyNew))
            {
                Debug.LogWarning ($"Key {keyNew} is already taken, can't duplicate");
                return;
            }
            
            if (duplicate)
            {
                var copy = UtilitiesYAML.CloneThroughYaml (this);
                copy.key = keyNew;
                DataMultiLinkerCombatArea.selectedArea.spawnGroups.Add (keyNew, copy);
                DataMultiLinkerCombatArea.selectedSpawnGroup = copy;
                if (copy.points != null && copy.points.Count > 0)
                    DataMultiLinkerCombatArea.selectedSpawnPoint = copy.points[0];
            }
            else
            {
                DataMultiLinkerCombatArea.selectedArea.spawnGroups.Remove (key);
                DataMultiLinkerCombatArea.selectedArea.spawnGroups.Add (keyNew, this);
                key = keyNew;
            }
            
            UnityEditor.SceneView.RepaintAll ();
        }
        
        [Button ("Tag as road"), PropertyOrder (-1), ButtonGroup ("A")]
        public void TagAsRoad ()
        {
            string tagDir = null;
            foreach (var tag in tags)
            {
                if (tag.StartsWith ("direction_"))
                {
                    tagDir = tag;
                    break;
                }
            }
            
            tags = new HashSet<string> ();
            tags.Add ("context_road");

            if (tagDir != null)
                tags.Add (tagDir);
        }

        #endif
        #endregion
    }
    
     [Serializable, HideReferenceObjectPicker]
    public class DataBlockAreaWaypointGroup
    {
        [YamlIgnore, HideInInspector, NonSerialized]
        public string key;

        [ListDrawerSettings (DefaultExpandedState = false, AlwaysAddDefaultValue = true)]
        public List<DataBlockAreaPoint> points = new List<DataBlockAreaPoint> ();

        #region Editor
        #if UNITY_EDITOR
        
        [Button ("@GetSelectLabel"), PropertyOrder (-2), ButtonGroup ("T")]
        public void SelectForEditing ()
        {
            if (DataMultiLinkerCombatArea.selectedWaypointGroup == this)
            {
                DataMultiLinkerCombatArea.selectedWaypointGroup = null;
                DataMultiLinkerCombatArea.selectedWaypointGroup = null;
            }
            else
            {
                DataMultiLinkerCombatArea.selectedWaypointGroup = this;
                if (points != null && points.Count > 0)
                    DataMultiLinkerCombatArea.selectedWaypoint = points[0];
            }
        }

        private string GetSelectLabel => DataMultiLinkerCombatArea.selectedWaypointGroup == this ? "Deselect" : "Select";
        
        [Button ("Remove"), PropertyOrder (-2), ButtonGroup ("T")]
        public void Remove ()
        {
            if 
            (
                string.IsNullOrEmpty (key) || 
                DataMultiLinkerCombatArea.selectedArea?.waypointGroups == null ||
                !DataMultiLinkerCombatArea.selectedArea.waypointGroups.ContainsValue (this)
            )
            {
                return;
            }

            DataMultiLinkerCombatArea.selectedArea.waypointGroups.Remove (key);
        }
        
        [ShowIf ("@DataMultiLinkerCombatArea.selectedArea != null")]
        [Button ("Rename/Duplicate"), PropertyOrder (-2)]
        public void RenameDuplicate (bool duplicate, string keyNew)
        {
            if (DataMultiLinkerCombatArea.selectedArea == null)
                return;

            var groups = DataMultiLinkerCombatArea.selectedArea.waypointGroups;
            if (groups.ContainsKey (keyNew))
            {
                Debug.LogWarning ($"Key {keyNew} is already taken, can't duplicate");
                return;
            }
            
            if (duplicate)
            {
                var copy = UtilitiesYAML.CloneThroughYaml (this);
                copy.key = keyNew;
                DataMultiLinkerCombatArea.selectedArea.waypointGroups.Add (keyNew, copy);
                DataMultiLinkerCombatArea.selectedWaypointGroup = copy;
                if (copy.points != null && copy.points.Count > 0)
                    DataMultiLinkerCombatArea.selectedWaypoint = copy.points[0];
            }
            else
            {
                DataMultiLinkerCombatArea.selectedArea.waypointGroups.Remove (key);
                DataMultiLinkerCombatArea.selectedArea.waypointGroups.Add (keyNew, this);
                key = keyNew;
            }
            
            UnityEditor.SceneView.RepaintAll ();
        }

        #endif
        #endregion
    }
    
    
    
    public interface IDataBlockAreaLocationProvider
    {
        bool TryGetLocationData
        (
            CombatDescription cd,
            DataContainerCombatArea area,
            string stateKeyResolved,
            HashSet<string> keysOccupied,
            out string locationKey
        );
    }
    
    [Serializable, HideReferenceObjectPicker]
    public class DataBlockAreaLocationFromState : IDataBlockAreaLocationProvider
    {
        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerScenario\", \"GetStateKeys\")")]
        public string stateKey;

        public bool TryGetLocationData 
        (
            CombatDescription cd, 
            DataContainerCombatArea area, 
            string stateKeyResolved, 
            HashSet<string> keysOccupied, 
            out string locationKey
        )
        {
            locationKey = null;

            #if !PB_MODSDK

            if (!Application.isPlaying)
                return false;

            if (string.IsNullOrEmpty (stateKey) || cd?.stateLocations == null || !cd.stateLocations.ContainsKey (stateKey))
            {
                Debug.LogWarning ($"State {stateKeyResolved} resolving location | Failed to copy existing location: nothing registered for state [{stateKey}]");
                return false;
            }

            locationKey = cd.stateLocations[stateKey];

            #endif
            return true;
        }
        
        public override string ToString ()
        {
            return $"Volume search from state: {stateKey}";
        }
    }
    
    [Serializable, HideReferenceObjectPicker]
    public class DataBlockAreaLocationTagFilter : IDataBlockAreaLocationProvider
    {
        [PropertyOrder (-1)]
        public bool includeOverlaps = false;
        
        [PropertyOrder (-1)]
        [DictionaryKeyDropdown ("@DataShortcuts.sim.combatLocationTags")]
        public SortedDictionary<string, bool> tagRequirements = new SortedDictionary<string, bool> { { "example", true } };

        [DropdownReference]
        public List<ICombatPositionValidationFunction> filters;
        
        private static List<DataBlockAreaLocationTagged> locationsFoundIntermediate = new List<DataBlockAreaLocationTagged> ();
        private static List<DataBlockAreaLocationTagged> locationsFound = new List<DataBlockAreaLocationTagged> ();

        public bool TryGetLocationData 
        (
            CombatDescription cd, 
            DataContainerCombatArea area, 
            string stateKeyResolved, 
            HashSet<string> keysOccupied, 
            out string locationKey
        )
        {
            locationKey = null;

            #if !PB_MODSDK

            if (!Application.isPlaying)
                return false;
            
            string context = $"State {stateKeyResolved} resolving location";
 
            if (tagRequirements == null || tagRequirements.Count == 0)
            {
                Debug.LogWarning ($"{context} | Failed tag search: no tag filter provided");
                return false;
            }

            if (area.locationsProc == null || area.locationsProc.Count == 0)
            {
                Debug.LogWarning ($"{context} | Failed tag search: area {area.key} has no locations");
                return false;
            }
            
            var keysOccupiedPresent = keysOccupied != null && keysOccupied.Count > 0;
            locationsFound.Clear ();
            locationsFoundIntermediate.Clear ();
            
            bool log = DataShortcuts.sim.logScenarioGeneration;
            if (log)
                Debug.Log ($"{context} | Tag search: {tagRequirements.ToStringFormattedKeyValuePairs ()} | Locations: {area.locationsProc.Count}");

            foreach (var kvp in area.locationsProc)
            {
                // Skip all locations already claimed by other states
                var locationCandidateKey = kvp.Key;
                if (keysOccupiedPresent && keysOccupied.Contains (locationCandidateKey))
                {
                    if (log)
                        Debug.Log ($"{context} | Skipping already occupied location {locationCandidateKey}");
                    continue;
                }

                var locationCandidate = kvp.Value;
                if (locationCandidate == null || locationCandidate.data == null)
                    continue;
                
                bool invalid = false;
                var tagsInLocation = locationCandidate.tags;
                bool tagsInLocationPresent = tagsInLocation != null;

                foreach (var kvp2 in tagRequirements)
                {
                    string tag = kvp2.Key;
                    bool required = kvp2.Value;
                    bool present = tagsInLocationPresent && tagsInLocation.Contains (tag);

                    if (present != required)
                    {
                        // if (log)
                        //     Debug.Log ($"State {stateKeyResolved} resolving location | Skipping location {locationCandidateKey} due to tag mismatch | Tag {tag} {(required ? "absent (required)" : "present (blocked)")}");
                        invalid = true;
                        break;
                    }
                }

                if (invalid)
                    continue;

                locationsFoundIntermediate.Add (locationCandidate);
            }
            
            if (log)
                Debug.Log ($"{context} | Tag search found {locationsFoundIntermediate.Count} intermediate candidates: {locationsFoundIntermediate.ToStringFormatted (toStringOverride: (x) => x.key)}");

            foreach (var locationCandidate in locationsFoundIntermediate)
            {
                var locationCandidateKey = locationCandidate.key;
                var locationPoint = locationCandidate.data.point;
                
                // Scan for overlaps with already allocated locations
                if (keysOccupiedPresent && !includeOverlaps)
                {
                    bool overlap = false;
                    var radiusCandidate = locationCandidate.data.GetRadius ();
                    
                    foreach (var keyOccupied in keysOccupied)
                    {
                        var locationOccupiedFound = area.locationsProc.TryGetValue (keyOccupied, out var locationOccupied);
                        if (!locationOccupiedFound || locationOccupied == null || locationOccupied.data == null)
                            continue;
                        
                        var radiusOccupied = locationOccupied.data.GetRadius ();
                        var threshold = radiusCandidate + radiusOccupied;
                        var dist = Vector3.Distance (locationOccupied.data.point, locationPoint);
                        if (dist < threshold)
                        {
                            if (log)
                                Debug.Log ($"{context} | Skipping tag-compatible location {locationCandidateKey} due to overlap with an assigned location {keyOccupied} | Distance: {dist} | Threshold: {threshold}");
                            overlap = true;
                            break;
                        }
                    }

                    if (overlap)
                        continue;
                }

                if (filters != null)
                {
                    bool filtersPassed = true;
                    foreach (var function in filters)
                    {
                        if (function == null)
                            continue;

                        bool valid = function.IsPositionValid (cd, locationPoint, context);
                        if (!valid)
                        {
                            filtersPassed = false;
                            break;
                        }
                    }

                    if (!filtersPassed)
                    {
                        if (log)
                            Debug.LogWarning ($"{context} | Skipping tag-compatible location {locationCandidateKey} due to failed function based filtering");
                        continue;
                    }
                }
                
                locationsFound.Add (locationCandidate);
            }

            if (locationsFound.Count == 0)
            {
                if (locationsFoundIntermediate.Count > 0)
                    Debug.LogWarning ($"{context} | All {locationsFoundIntermediate.Count} tag-compatible locations were discarded based on additional filtering.");
                return false;
            }
            
            var location = locationsFound.GetRandomEntry ();
            var data = location.data;
            if (data == null || string.IsNullOrEmpty (location.key))
                return false;

            if (log)
                Debug.Log ($"{context} | Final selection: {location.key} | Candidates: {locationsFound.ToStringFormatted (toStringOverride: (x) => x.key)}");
                    
            locationKey = location.key;

            #endif
            return true;
        }
        
        public override string ToString ()
        {
            return $"Location search by tag filter: {tagRequirements.ToStringFormattedKeyValuePairs ()}";
        }
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockAreaLocationTagFilter () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
    
    [Serializable, HideReferenceObjectPicker]
    public class DataBlockAreaLocationKey : IDataBlockAreaLocationProvider
    {
        [PropertyOrder (-1)]
        public string key;

        public bool TryGetLocationData 
        (
            CombatDescription cd, 
            DataContainerCombatArea area, 
            string stateKeyResolved, 
            HashSet<string> keysOccupied, 
            out string locationKey
        )
        {
            locationKey = null;

            #if !PB_MODSDK

            if (!Application.isPlaying || string.IsNullOrEmpty (key))
                return false;

            var locationFound = 
                area.locationsProc != null && 
                !string.IsNullOrEmpty (key) && 
                area.locationsProc.TryGetValue (key, out var location) && 
                location != null && 
                location.data != null;

            if (!locationFound)
            {
                Debug.LogWarning ($"State {stateKeyResolved} resolving location | Failed to get location {key}: no such location is registered in area {area.key}");
                return false;
            }
            
            var keysOccupiedPresent = keysOccupied != null && keysOccupied.Count > 0;
            if (keysOccupiedPresent && keysOccupied.Contains (key))
            {
                Debug.LogWarning ($"State {stateKeyResolved} resolving location | Failed to get location {key}: this location is already occupied");
                return false;
            }

            locationKey = key;

            #endif
            return true;
        }
        
        public override string ToString ()
        {
            return $"Location search by key: {key}";
        }
    }
    
    [Serializable, HideReferenceObjectPicker]
    public class DataBlockAreaLocation
    {
        [HideLabel]
        public Vector3 point;

        // [HideLabel]
        [PropertyRange (-180f, 180f)]
        public float rotation;
        
        // [HideLabel]
        [PropertyRange (1f, 300f)]
        [LabelText ("@rect ? \"Size (X)\" : \"Radius\"")]
        public float sizeX = 9f;
        
        // [HideLabel]
        [ShowIf ("rect")]
        [PropertyRange (1f, 300f)]
        [LabelText ("Size (Y)")]
        public float sizeY = 9f;

        [HideInInspector]
        public bool rect = false;

        [Button ("@rect ? \"Switch to circle\" : \"Switch to rect\"")]
        private void Toggle ()
        {
            rect = !rect;
        }

        public float GetRadius ()
        {
            return rect ? Mathf.Max (sizeX, sizeY) * 0.5f : sizeX;
        }
    }

    [Serializable]
    public class DataBlockAreaShotKeyframe
    {
        public Vector3 position;
        public Vector3 rotation;
        public float fov;

        #region Editor
        #if UNITY_EDITOR

        [Button ("Grab from camera"), ButtonGroup]
        private void GrabFromCamera ()
        {
            if (UnityEditor.SceneView.lastActiveSceneView == null || UnityEditor.SceneView.lastActiveSceneView.camera == null) 
                return;

            var c = UnityEditor.SceneView.lastActiveSceneView.camera;
            var t = c.transform;
            
            position = t.position;
            position = new Vector3 ((float)Math.Round (position.x, 2), (float)Math.Round (position.y, 2), (float)Math.Round (position.z, 2));
            
            rotation = t.rotation.eulerAngles;
            rotation = new Vector3 ((float)Math.Round (rotation.x, 2), (float)Math.Round (rotation.y, 2), (float)Math.Round (rotation.z, 2));
            
            fov = c.fieldOfView;
        }

        [Button ("Apply to camera"), ButtonGroup]
        private void ApplyToCamera ()
        {
            var scene = UnityEditor.SceneView.lastActiveSceneView;
            if (scene == null || scene.camera == null) 
                return;
            
            var c = scene.camera;
            var t = c.transform;
            
            // Debug.Log ($"Scene info before application\nSize: {scene.size} | Pivot / camera position: {scene.pivot} / {t.position} | Rotation / camera rotation: {scene.rotation.eulerAngles} / {t.rotation.eulerAngles}");
            
            t.position = position;
            t.rotation = Quaternion.Euler (rotation);
            
            scene.cameraSettings.fieldOfView = c.fieldOfView;
            scene.AlignViewToObject (t);
            
            // Debug.Log ($"Scene info after application\nSize: {scene.size} | Pivot / camera position: {scene.pivot} / {t.position} | Rotation / camera rotation: {scene.rotation.eulerAngles} / {t.rotation.eulerAngles}");
        }
        
        #endif
        #endregion
    }

    public class DataBlockAreaShotFade
    {
        public float duration;
    }
    
    public class DataBlockIntroCameraFinal
    {
        public TargetFromSource target = new TargetFromSource ();
        public float rotationX = 45f;
        public float rotationY = 180f;
        public float zoom = 0.5f;
    }

    public class DataBlockAreaIntro
    {
        [YamlIgnore, ShowInInspector, ToggleGroup ("preview", "Preview")] 
        [OnValueChanged ("UpdatePreview")]
        private bool preview = false;
        
        [YamlIgnore, ShowInInspector, ToggleGroup ("preview"), PropertyRange (0, "GetShotIndexLimit")] 
        [OnValueChanged ("UpdatePreview")]
        private int previewShot = 0;
        
        [YamlIgnore, ShowInInspector, ToggleGroup ("preview"), PropertyRange (0f, 1f)] 
        [OnValueChanged ("UpdatePreview")]
        private float previewProgress = 0f;
        
        
        public DataBlockIntroCameraFinal cameraInputsFinal;
        
        [ListDrawerSettings (DefaultExpandedState = true, CustomAddFunction = "@new DataBlockAreaShot ()")]
        [OnValueChanged ("UpdatePreview", true)]
        public List<DataBlockAreaShot> shots = new List<DataBlockAreaShot> ();
        
        #region Editor
        #if UNITY_EDITOR

        private int GetShotIndexLimit => shots != null ? Mathf.Max (0, shots.Count - 1) : 0;

        private void UpdatePreview ()
        {
            if (!preview || shots == null || shots.Count < 1 || !previewShot.IsValidIndex (shots))
                return;

            var shot = shots[previewShot];
            if (shot == null || shot.from == null || shot.to == null)
                return;
            
            var scene = UnityEditor.SceneView.lastActiveSceneView;
            if (scene == null || scene.camera == null) 
                return;

            var progress = Mathf.Clamp01 (previewProgress);
            var position = Vector3.Lerp (shot.from.position, shot.to.position, progress);
            var rotation = Quaternion.Lerp (Quaternion.Euler (shot.from.rotation), Quaternion.Euler (shot.to.rotation), progress);
            
            var c = scene.camera;
            var t = c.transform;
            
            t.position = position;
            t.rotation = rotation;
            
            scene.cameraSettings.fieldOfView = c.fieldOfView;
            scene.AlignViewToObject (t);
        }

        #endif

        #endregion
    }
    
    public class DataBlockAreaShot
    {
        [PropertyRange (0.1f, 10f)]
        public float duration = 2f;
        public LeanTweenType easing = LeanTweenType.linear;
        
        [HorizontalGroup]
        public bool fadeOnEnd;
        
        [HorizontalGroup, ShowIf ("fadeOnEnd"), HideLabel]
        public float fadeDuration = 0.5f;
        
        public DataBlockAreaShotKeyframe from = new DataBlockAreaShotKeyframe ();
        public DataBlockAreaShotKeyframe to = new DataBlockAreaShotKeyframe ();

        #region Editor
        #if UNITY_EDITOR

        #endif
        #endregion
    }
    
    [Serializable]
    public class DataBlockAreaLocationTagged
    {
        [YamlIgnore, HideInInspector, NonSerialized]
        public string key;
        
        [ValueDropdown ("GetTags")]
        public HashSet<string> tags = new HashSet<string> ();

        // [HideReferenceObjectPicker]
        public DataBlockAreaLocation data = new DataBlockAreaLocation ();

        #region Editor
        #if UNITY_EDITOR

        private IEnumerable<string> GetTags =>
            DataShortcuts.sim.combatLocationTags;
        
        [Button ("@GetSelectLabel"), PropertyOrder (-2), ButtonGroup ("T")]
        public void SelectForEditing ()
        {
            if (DataMultiLinkerCombatArea.selectedLocation == this)
                DataMultiLinkerCombatArea.selectedLocation = null;
            else
                DataMultiLinkerCombatArea.selectedLocation = this;
        }

        private string GetSelectLabel => DataMultiLinkerCombatArea.selectedLocation == this ? "Deselect" : "Select";

        [Button ("Snap"), PropertyOrder (-2), ButtonGroup ("T")]
        public void SnapToGrid ()
        {
            if (data == null)
                return;
        
            data.point = AreaUtility.SnapPointToGrid (data.point);
            UnityEditor.SceneView.RepaintAll ();
        }

        [Button ("Ground"), PropertyOrder (-2), ButtonGroup ("T")]
        public void Ground ()
        {
            if (data == null)
                return;
            
            data.point = AreaUtility.GroundPoint (data.point);
            UnityEditor.SceneView.RepaintAll ();
        }
        
        [Button ("Remove"), PropertyOrder (-2), ButtonGroup ("T")]
        public void Remove ()
        {
            if 
            (
                string.IsNullOrEmpty (key) || 
                DataMultiLinkerCombatArea.selectedArea?.locations == null ||
                !DataMultiLinkerCombatArea.selectedArea.locations.ContainsValue (this)
            )
            {
                return;
            }

            DataMultiLinkerCombatArea.selectedArea.locations.Remove (key);
        }
        
        [ShowIf ("@DataMultiLinkerCombatArea.selectedArea != null")]
        [Button ("Rename/Duplicate"), PropertyOrder (-2)]
        public void Duplicate (bool duplicate, string keyNew)
        {
            if (data == null || DataMultiLinkerCombatArea.selectedArea == null)
                return;

            var locations = DataMultiLinkerCombatArea.selectedArea.locations;
            if (locations.ContainsKey (keyNew))
            {
                Debug.LogWarning ($"Key {keyNew} is already taken, can't duplicate");
                return;
            }
            
            if (duplicate)
            {
                var copy = UtilitiesYAML.CloneThroughYaml (this);
                copy.key = keyNew;
                DataMultiLinkerCombatArea.selectedArea.locations.Add (keyNew, copy);
                DataMultiLinkerCombatArea.selectedLocation = copy;
            }
            else
            {
                DataMultiLinkerCombatArea.selectedArea.locations.Remove (key);
                DataMultiLinkerCombatArea.selectedArea.locations.Add (keyNew, this);
                key = keyNew;
            }
            
            UnityEditor.SceneView.RepaintAll ();
        }
        
        [Button ("Tag as escape"), PropertyOrder (-1), ButtonGroup ("A")]
        public void TagAsEscape ()
        {
            string tagDir = null;
            foreach (var tag in tags)
            {
                if (tag.StartsWith ("direction_"))
                {
                    tagDir = tag;
                    break;
                }
            }
            
            tags = new HashSet<string> ();
            tags.Add ("obj_escape");

            if (tagDir != null)
                tags.Add (tagDir);
        }
        
        [Button ("Tag as def. trigger"), PropertyOrder (-1), ButtonGroup ("A")]
        public void TagAsDefenseTrigger ()
        {
            tags = new HashSet<string> ();
            tags.Add ("obj_defense_navigable");
        }
        
        [Button ("Tag as def. origin"), PropertyOrder (-1), ButtonGroup ("A")]
        public void TagAsDefenseOrigin ()
        {
            tags = new HashSet<string> ();
            tags.Add ("obj_defense_origin");

            var go = GameObject.Find ("comp_crawler_01");
            if (go != null && go.transform.parent == null)
            {
                Debug.Log ($"Updating position/rotation of point {key} from in-scene root instance of comp_crawler_01 visual prefab");
                data.point = go.transform.position;
                data.rotation = go.transform.rotation.eulerAngles.y;
            }
        }

        #endif
        #endregion
    }
    
    [Serializable]
    public class DataBlockAreaField
    {
        [YamlIgnore, HideInInspector, NonSerialized]
        public string key;
        
        public string type;

        public bool visible = true;
        
        public Vector3 origin;

        public Vector3 size;

        public Vector4 force;

        // [HideLabel]
        [PropertyRange (-180f, 180f)]
        public float rotation;
        
        public bool IsIntersectingPoint (Vector3 point)
        {
            Vector3 half = size * 0.5f;
            return point.x >= origin.x - half.x && point.x <= origin.x + half.x &&
                   point.y >= origin.y - size.y && point.y <= origin.y &&
                   point.z >= origin.z - half.z && point.z <= origin.z + half.z;
        }

        #region Editor
        #if UNITY_EDITOR
        
        [Button ("@GetSelectLabel"), PropertyOrder (-2), ButtonGroup ("T")]
        public void SelectForEditing ()
        {
            if (DataMultiLinkerCombatArea.selectedField == this)
                DataMultiLinkerCombatArea.selectedField = null;
            else
                DataMultiLinkerCombatArea.selectedField = this;
        }

        private string GetSelectLabel => DataMultiLinkerCombatArea.selectedField == this ? "Deselect" : "Select";
        
        [Button ("Remove"), PropertyOrder (-2), ButtonGroup ("T")]
        public void Remove ()
        {
            if 
            (
                string.IsNullOrEmpty (key) || 
                DataMultiLinkerCombatArea.selectedArea?.fields == null ||
                !DataMultiLinkerCombatArea.selectedArea.fields.Contains (this)
            )
            {
                return;
            }

            DataMultiLinkerCombatArea.selectedArea.fields.Remove (this);
        }
        
        [ShowIf ("@DataMultiLinkerCombatArea.selectedArea != null")]
        [Button ("Duplicate"), PropertyOrder (-2), ButtonGroup ("T")]
        public void Duplicate ()
        {
            if (DataMultiLinkerCombatArea.selectedField == null)
                return;
            
            var copy = UtilitiesYAML.CloneThroughYaml (this);
            DataMultiLinkerCombatArea.selectedArea.fields.Add (copy);
            DataMultiLinkerCombatArea.selectedField = copy;
            UnityEditor.SceneView.RepaintAll ();
        }

        #endif
        #endregion
    }
    
    [Serializable, HideReferenceObjectPicker]
    public class DataBlockEnvironmentSegment
    {
        [YamlIgnore, HideReferenceObjectPicker, OnValueChanged ("OnBeforeSerialization")]
        [AssetsOnly]
        public GameObject prefab;
        
        [ReadOnly]
        public string path;

        public void OnBeforeSerialization ()
        {
            #if UNITY_EDITOR && !PB_MODSDK
            if (prefab == null)
            {
                path = string.Empty;
                return;
            };

            var fullPath = UnityEditor.AssetDatabase.GetAssetPath (prefab);
            string extension = System.IO.Path.GetExtension (fullPath);
            
            fullPath = fullPath.ReplaceFirst ("Assets/Resources/", string.Empty);
            fullPath = fullPath.Substring(0, fullPath.Length - extension.Length);
            path = fullPath;
            #endif
        }

        public void OnAfterDeserialization ()
        {
            #if !PB_MODSDK
            prefab = !string.IsNullOrEmpty (path) ? Resources.Load<GameObject> (path) : null;
            #endif
        }
    }

    public class DataBlockAreaForce
    {
        
    }

    public class DataBlockAreaProjectileCeiling
    {
        public bool guidedOnly;
        
        public float height;

        [ValueDropdown("@DataMultiLinkerAssetPools.data.Keys"), InlineButtonClear]
        public string fxKey;
        
        public bool fxRotationCustom = false;
        
        [ShowIf ("fxRotationCustom")]
        public Vector3 fxRotation;
    }

    public class DataBlockAreaCore
    {
        public bool backgroundTerrainUsed = true;
        public bool sliceShadingUsed = false;

        [NonSerialized, YamlIgnore, ShowInInspector, ShowIf ("sliceShadingUsed")]
        private bool sliceLiveRefresh = false;
        
        [OnValueChanged ("RefreshSliceShading", true)]
        [ShowIf ("sliceShadingUsed")]
        [PropertyTooltip ("X: height, Y: shadow size, Z: glow size, W: inversion")]
        public Vector4 sliceShadingInputs = new Vector4 (0f, 0f, 0f, 0f);

        [OnValueChanged ("RefreshSliceShading", true)]
        [ShowIf ("sliceShadingUsed")]
        public Color sliceColor = new Color (0f, 0f, 0f, 0f);

        [DropdownReference (true)]
        public DataBlockVector2 cameraHeightRange;
        
        [DropdownReference (true)]
        public DataBlockFloat timeCustom;
        
        [DropdownReference (true)]
        public CombatChangeFogCustom fogCustom;
        
        [DropdownReference (true)]
        public DataBlockAreaProjectileCeiling projectileCeiling;

        private void RefreshSliceShading ()
        {
            if (!sliceLiveRefresh)
                return;
            
            var sceneHelper = CombatSceneHelper.ins;
            if (sceneHelper == null)
                return;
            
            var materialHelper = sceneHelper.materialHelper;
            if (materialHelper == null)
                return;
            
            if (sliceShadingUsed)
            {
                materialHelper.sliceInputs = sliceShadingInputs;
                materialHelper.sliceColor = sliceColor;
                materialHelper.sliceShadingEnabled = true;
            }
            else
                materialHelper.sliceShadingEnabled = false;
            materialHelper.ApplyGlobals ();
        }
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockAreaCore () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
    
    public class DataBlockAreaAtmosphereSettings
    {
        public float groundHeight = 0f;
        
        [PropertyTooltip ("X-Y (amount of destroyed blocks) mapped to a 0-1 factor that is used to interpolate between Z-W (atmospheric smoke intensity)")]
        public Vector4 destructionMapping = new Vector4(10, 20, 0, 1);

        [ShowInInspector, ReadOnly, PropertyTooltip ("Number of destructible points reported by the AreaManager for the currently loaded level")]
        private int destructiblePoints
        {
            get
            {
                var sceneHelper = CombatSceneHelper.ins;
                if (sceneHelper == null || sceneHelper.areaManager == null)
                    return -1;

                var am = sceneHelper.areaManager;
                return am.destructiblePointCount;
            }
        }
    }
    
    public class DataBlockAreaContent
    {
        [LabelText ("Path")]
        [YamlIgnore, ShowInInspector, DisplayAsString]
        public string pathLast = null;

        [LabelText ("Size")]
        [YamlIgnore, ShowInInspector, DisplayAsString]
        public string memoryLoaded = null;
        
        [BoxGroup]
        [YamlIgnore, ShowInInspector]
        public AreaDataCore core;

        [YamlIgnore, ShowInInspector]
        public Dictionary<string, ILevelDataChannel> channels;
        
        [YamlIgnore, HideInInspector]
        public DataContainerCombatArea parent;
    }
    
    [HideReferenceObjectPicker]
    public class DataContainerCombatAreaParent
    {
        [GUIColor ("GetKeyColor")]
        [ValueDropdown ("@DataMultiLinkerCombatArea.data.Keys")]
        [SuffixLabel ("@hierarchyProperty"), HideLabel]
        public string key;

        [YamlIgnore, ReadOnly, HideInInspector]
        private string hierarchyProperty => DataMultiLinkerCombatArea.Presentation.showHierarchy ? hierarchy : string.Empty;
        
        [YamlIgnore, ReadOnly, HideInInspector]
        public string hierarchy;

        #region Editor
        #if UNITY_EDITOR

        private static Color colorError = Color.Lerp (Color.red, Color.white, 0.5f);
        private static Color colorNormal = Color.white;
        
        private Color GetKeyColor ()
        {
            if (string.IsNullOrEmpty (key))
                return colorError;

            bool present = DataMultiLinkerCombatArea.data.ContainsKey (key);
            return present ? colorNormal : colorError;
        }
        
        #endif
        #endregion
    }
    
    [Serializable][LabelWidth (180f)]
    public class DataContainerCombatArea : DataContainer, IDataContainerTagged
    {
        [GUIColor ("GetSelectedColor")]
        [ToggleLeft]
        public bool hidden = false;

        
        [GUIColor ("GetSelectedColor")]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, CustomAddFunction = "@new DataContainerCombatAreaParent ()")]
        [DropdownReference]
        public List<DataContainerCombatAreaParent> parents = new List<DataContainerCombatAreaParent> ();
        
        [GUIColor ("GetSelectedColor")]
        [YamlIgnore, LabelText ("Children"), ReadOnly]
        [ShowIf ("@children != null && children.Count > 0")]
        [ListDrawerSettings (DefaultExpandedState = false)]
        public List<string> children = new List<string> ();
        
        
        [DropdownReference (true)]
        [ValueDropdown ("@TextureManager.GetExposedTextureKeys (TextureGroupKeys.CombatAreas)"), HideLabel]
        [OnInspectorGUI ("@DropdownUtils.DrawTexturePreview ($value, TextureGroupKeys.CombatAreas, 128)", false)]
        public string image;
        
        [YamlIgnore, ReadOnly, HideDuplicateReferenceBox, ShowIf ("IsInheritanceVisible"), HideLabel]
        [OnInspectorGUI ("@DropdownUtils.DrawTexturePreview ($value, TextureGroupKeys.CombatAreas, 128)", false)]
        public string imageProc;
        
        
        [GUIColor ("GetSelectedColor")]
        [DropdownReference (true)]
        public DataBlockAreaCore core;
        
        [YamlIgnore, ReadOnly, HideDuplicateReferenceBox, ShowIf ("IsInheritanceVisible")]
        public DataBlockAreaCore coreProc; 
        
        
        [GUIColor ("GetSelectedColor")]
        [DropdownReference (true)]
        public DataBlockAreaContent content;
        
        [YamlIgnore, ReadOnly, HideDuplicateReferenceBox, ShowIf ("IsInheritanceVisible")]
        public DataBlockAreaContent contentProc;
        
        
        [DropdownReference (true)]
        public DataBlockAreaAtmosphereSettings atmosphere;
        
        [YamlIgnore, ReadOnly, HideDuplicateReferenceBox, ShowIf ("IsInheritanceVisible")]
        public DataBlockAreaAtmosphereSettings atmosphereProc;
        
        
        [DropdownReference]
        [GUIColor (0.85f, 0.9f, 0.8f, 1.0f)]
        [ValueDropdown ("GetAreaTags")]
        public HashSet<string> tags = new HashSet<string> ();
        
        [YamlIgnore, ReadOnly, HideDuplicateReferenceBox, ShowIf ("IsInheritanceVisible")]
        public HashSet<string> tagsProc = new HashSet<string> ();
        
        
        [DropdownReference]
        [ShowIf ("AreSpawnsVisible")]
        [GUIColor (0.98f, 1f, 0.91f)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        [OnValueChanged ("RefreshParentsInSpawnGroups", true)]
        public SortedDictionary<string, DataBlockAreaSpawnGroup> spawnGroups;
        
        [YamlIgnore, ReadOnly, HideDuplicateReferenceBox, ShowIf ("@IsInheritanceVisible && AreSpawnsVisible")]
        public SortedDictionary<string, DataBlockAreaSpawnGroup> spawnGroupsProc;
        
        
        [DropdownReference]
        [ShowIf ("AreLocationsVisible")]
        [GUIColor (1f, 0.98f, 0.9f)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        [OnValueChanged ("RefreshParentsInLocations", true)]
        public SortedDictionary<string, DataBlockAreaLocationTagged> locations;
        
        [YamlIgnore, ReadOnly, HideDuplicateReferenceBox, ShowIf ("@IsInheritanceVisible && AreLocationsVisible")]
        public SortedDictionary<string, DataBlockAreaLocationTagged> locationsProc;
        
        
        [DropdownReference]
        [ShowIf ("AreVolumesVisible")]
        [GUIColor (1f, 0.945f, 0.929f)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        [OnValueChanged ("RefreshParentsInVolumes", true)]
        public SortedDictionary<string, DataBlockAreaVolumeTagged> volumes;
        
        [YamlIgnore, ReadOnly, HideDuplicateReferenceBox, ShowIf ("@IsInheritanceVisible && AreVolumesVisible")]
        public SortedDictionary<string, DataBlockAreaVolumeTagged> volumesProc;
        
        
        [DropdownReference]
        [ShowIf ("AreFieldsVisible")]
        [GUIColor (0.95f, 0.95f, 1.0f, 1.0f)]
        [ListDrawerSettings (DefaultExpandedState = false)]
        public List<DataBlockAreaField> fields;
        
        [YamlIgnore, ReadOnly, HideDuplicateReferenceBox, ShowIf ("@IsInheritanceVisible && AreFieldsVisible")]
        public List<DataBlockAreaField> fieldsProc;
        
        
        [DropdownReference]
        [ShowIf ("AreWaypointsVisible")]
        [GUIColor (1f, 0.95f, 1.0f, 1.0f)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        [OnValueChanged ("RefreshParentsInSpawnGroups", true)]
        public SortedDictionary<string, DataBlockAreaWaypointGroup> waypointGroups;
        
        [YamlIgnore, ReadOnly, HideDuplicateReferenceBox, ShowIf ("@IsInheritanceVisible && AreWaypointsVisible")]
        public SortedDictionary<string, DataBlockAreaWaypointGroup> waypointGroupsProc;
        
        
        [DropdownReference]
        public List<DataBlockScenarioBriefingGroup> briefingGroupsInjected;

        [YamlIgnore, ReadOnly, HideDuplicateReferenceBox, ShowIf ("IsInheritanceVisible")]
        public List<DataBlockScenarioBriefingGroup> briefingGroupsInjectedProc;
        
        
        [DropdownReference (true)]
        public DataBlockAreaIntro intro;
        
        [YamlIgnore, ReadOnly, HideDuplicateReferenceBox, ShowIf ("IsInheritanceVisible")]
        public DataBlockAreaIntro introProc;
        
        
        [DropdownReference (true)]
        public DataBlockEnvironmentBoundary boundary;
        
        [YamlIgnore, ReadOnly, HideDuplicateReferenceBox, ShowIf ("IsInheritanceVisible")]
        public DataBlockEnvironmentBoundary boundaryProc;
        
        
        [DropdownReference]
        public List<DataBlockEnvironmentSegment> segments;
        
        [YamlIgnore, ReadOnly, HideDuplicateReferenceBox, ShowIf ("IsInheritanceVisible")]
        public List<DataBlockEnvironmentSegment> segmentsProc;

        
        [DropdownReference (true)]
        public DataBlockOverworldLandscapeBiome biomeFilter;
        
        [YamlIgnore, ReadOnly, HideDuplicateReferenceBox, ShowIf ("IsInheritanceVisible")]
        public DataBlockOverworldLandscapeBiome biomeFilterProc;
        
        
        [DropdownReference (true)]
        public List<DataBlockProvinceScenarioChange> scenarioChanges;
        
        [YamlIgnore, ReadOnly, HideDuplicateReferenceBox, ShowIf ("IsInheritanceVisible")]
        public List<DataBlockProvinceScenarioChange> scenarioChangesProc;

        [YamlIgnore, ReadOnly]
        public Vector3 positionCenter = new Vector3 (150f, 0f, 150f);
        

        public HashSet<string> GetTags (bool processed) => 
            processed ? tagsProc : tags;

        public bool IsHidden () => hidden;

        public override void OnBeforeSerialization ()
        {
            base.OnBeforeSerialization ();
            
            SaveLevelContent ();
            
            if (boundary != null)
                boundary.OnBeforeSerialization ();
            
            if (segments != null)
            {
                foreach (var segment in segments)
                {
                    if (segment != null)
                        segment.OnBeforeSerialization ();
                }
            }
        }

        public void OnBeforeProcessing ()
        {
            
        }

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);

            if (content != null)
                content.parent = this;

            LoadLevelContentFromDisk ();

            if (boundary != null)
                boundary.OnAfterDeserialization ();

            if (segments != null)
            {
                foreach (var segment in segments)
                {
                    if (segment == null)
                        continue;
                    
                    segment.OnAfterDeserialization ();
                    
                    #if !PB_MODSDK
                    if (segment.prefab == null)
                        Debug.LogWarning ($"Area {key} | Failed to load environment prefab from path {segment.path}");
                    #endif
                }
            }

            RefreshParentsInSpawnGroups ();
            RefreshParentsInLocations ();
            RefreshParentsInVolumes ();
            CalculateAndRefreshAveragePointInSpawnGroup();
        }

        private void LoadLevelContentFromDisk ()
        {
            if (content == null)
                return;
            
            content.pathLast = null;
            content.memoryLoaded = null;
            
            content.core = null;
            content.channels = null;

            // Load root config. To support cases where a config override mod overrides that config, try DataContainer.path first
            var contentPath = $"{path}{key}/";
            var contentCore = UtilitiesYAML.LoadDataFromFile<AreaDataCore> (contentPath, "core.yaml", false);
            
            // If root config failed to load and DB path we used wasn't coming from the main DB, attempt a fallback from another path
            if (contentCore == null && !contentPath.Contains (DataMultiLinkerCombatArea.path))
            {
                var contentPathFallback = $"{DataMultiLinkerCombatArea.path}{key}/";
                Debug.Log ($"Area {key} | Switched to loading content to source folder\n- Failed path: {contentPath}\n- Fallback path: {contentPathFallback}");
                
                contentPath = contentPathFallback;
                contentCore = UtilitiesYAML.LoadDataFromFile<AreaDataCore> (contentPath, "core.yaml", false);
            }
            
            if (contentCore == null)
            {
                Debug.LogWarning ($"Area {key} | Failed to load core level data from {contentPath}");
                return;
            }

            // Update the path for reference: some editor utils will try to use it to follow the level file location
            content.pathLast = contentPath;
            content.core = contentCore;

            long m1 = GC.GetTotalMemory (false);

            LevelContentHelper.LoadDataFromDisk (content, contentPath);
            
            long m2 = GC.GetTotalMemory (false);
            content.memoryLoaded = UtilityString.FormatByteCount (m2 - m1);

            OnAfterLevelContentLoad ();
        }

        public void LoadLevelContentFromScene ()
        {
            if (content == null)
                return;

            content.core = null;
            content.channels = null;
            
            var sceneHelper = CombatSceneHelper.ins;
            if (sceneHelper == null || sceneHelper.areaManager == null)
                return;

            var am = sceneHelper.areaManager;
            content.core = new AreaDataCore
            {
                bounds = am.boundsFull,
                damageRestrictionDepth = am.damageRestrictionDepth,
                damagePenetrationDepth = am.damagePenetrationDepth,
            };
            
            LevelContentHelper.LoadDataFromScene (content);
        }
        

        private void OnAfterLevelContentLoad ()
        {
            
        }
        
        private void SaveLevelContent ()
        {
            if (content == null || content.channels == null || content.core == null)
                return;

            // Save root config
            var databasePath = DataMultiLinkerCombatArea.path;
            var contentPath = $"{databasePath}{key}/";
            UtilitiesYAML.SaveDataToFile (contentPath, "core.yaml", content.core);
            
            // Save channels
            LevelContentHelper.SaveData (content, contentPath);
        }

        private bool TryApplyLevelContentToScene ()
        {
            if (string.IsNullOrEmpty (key))
                return false;
            
            var sceneHelper = CombatSceneHelper.ins;
            if (sceneHelper == null)
                return false;
            
            if (contentProc == null || contentProc.channels == null || contentProc.core == null)
            {
                Debug.LogWarning ($"Area {key} has no level content and can't be applied to scene");
                return false;
            }
            
            LevelContentHelper.ApplyDataToScene (contentProc);

            bool backgroundTerrainUsed = coreProc != null && coreProc.backgroundTerrainUsed;
            sceneHelper.boundary.gameObject.SetActive (backgroundTerrainUsed);
            if (backgroundTerrainUsed)
                sceneHelper.LoadBoundary (boundaryProc);
                            
            if (sceneHelper.materialHelper != null)
                sceneHelper.materialHelper.ApplyAll ();
            
            sceneHelper.segmentHelper.LoadSegments (segmentsProc);
            sceneHelper.fieldHelper.LoadFields (fieldsProc);
            
            if (sceneHelper.background != null)
            {
                if (backgroundTerrainUsed)
                    sceneHelper.background.Rebuild ("default");
                else
                    sceneHelper.background.RebuildOnlyBoundaryDecal ();
            }

            var materialHelper = sceneHelper.materialHelper;
            if (coreProc != null && coreProc.sliceShadingUsed)
            {
                materialHelper.sliceInputs = coreProc.sliceShadingInputs;
                materialHelper.sliceColor = coreProc.sliceColor;
                materialHelper.sliceShadingEnabled = true;
            }
            else
                materialHelper.sliceShadingEnabled = false;
            materialHelper.ApplyGlobals ();
            
            #if !PB_MODSDK
            var ambientLight = sceneHelper.ambientLight;
            if (ambientLight != null)
            {
                ambientLight.gameObject.SetActive (true);
                ambientLight.OnLevelLoad ();
            }
            #endif
            
            bool additionalChangesSuccessful = OnAfterLevelContentApplication ();
            if (!additionalChangesSuccessful)
                return false;

            return true;
        }

        private bool OnAfterLevelContentApplication ()
        {
            return true;
        }
        
        
        private void RefreshParentsInSpawnGroups ()
        {
            if (spawnGroups == null)
                return;

            foreach (var kvp in spawnGroups)
            {
                var spawnGroupKey = kvp.Key;
                var spawnGroup = kvp.Value;
                if (spawnGroup == null)
                    continue;

                spawnGroup.key = spawnGroupKey;
                if (spawnGroup.points != null)
                {
                    for (int i = 0; i < spawnGroup.points.Count; ++i)
                    {
                        var point = spawnGroup.points[i];
                        if (point == null)
                            continue;

                        point.index = i;
                    }
                }
            }
        }
        
        private void RefreshParentsInLocations ()
        {
            if (locations == null)
                return;

            foreach (var kvp in locations)
            {
                var locationKey = kvp.Key;
                var location = kvp.Value;
                if (location == null)
                    continue;

                location.key = locationKey;
            }
        }

        private void RefreshParentsInVolumes ()
        {
            if (volumes == null)
                return;

            foreach (var kvp in volumes)
            {
                var volumeKey = kvp.Key;
                var volume = kvp.Value;
                if (volume == null)
                    continue;

                volume.key = volumeKey;
            }
        }

        private void CalculateAndRefreshAveragePointInSpawnGroup ()
        {
            if (spawnGroups == null)
                return;
            
            foreach (var kvp in spawnGroups)
            {
                var group = kvp.Value;
                if (group == null || group.points == null || group.points.Count == 0)
                    continue;

                group.RefreshAveragePosition ();
            }
        }
        
        public DataBlockAreaSpawnGroup GetSpawnGroup (string spawnKey, bool logIfMissing)
        {
            if (spawnGroupsProc == null || spawnGroupsProc.Count == 0)
            {
                if (logIfMissing)
                    Debug.LogWarning ($"Failed to get spawn from area {key}: area has no spawn collection");
                return null;
            }

            if (string.IsNullOrEmpty (spawnKey) || !spawnGroupsProc.ContainsKey (spawnKey))
            {
                if (logIfMissing)
                    Debug.LogWarning ($"Failed to get spawn from area {key}: key {spawnKey} returned nothing");
                return null;
            }

            var spawnGroup = spawnGroupsProc[spawnKey];
            return spawnGroup;
        }
        
        public Vector3 GetSpawnGroupCenter (string spawnKey, bool logIfMissing)
        {
            var spawnGroup = GetSpawnGroup (spawnKey, logIfMissing);
            return spawnGroup != null ? spawnGroup.averagePosition : new Vector3 (150f, 0f, 150f);
        }

        public DataBlockAreaVolume GetVolume (string volumeKey)
        {
            if (volumesProc == null || volumesProc.Count == 0)
            {
                Debug.LogWarning ($"Failed to get volume from area {key}: area has no volume collection");
                return null;
            }

            if (string.IsNullOrEmpty (volumeKey) || !volumesProc.ContainsKey (volumeKey))
            {
                Debug.LogWarning ($"Failed to get volume from area {key}: key {volumeKey} returned nothing");
                return null;
            }

            var volume = volumesProc[volumeKey];
            return volume?.data;
        }

        public DataBlockAreaLocation GetLocation (string locationKey)
        {
            if (locationsProc == null || locationsProc.Count == 0)
            {
                Debug.LogWarning ($"Failed to get location from area {key}: area has no location collection");
                return null;
            }

            if (string.IsNullOrEmpty (locationKey) || !locationsProc.ContainsKey (locationKey))
            {
                Debug.LogWarning ($"Failed to get location from area {key}: key {locationKey} returned nothing");
                return null;
            }

            var location = locationsProc[locationKey];
            return location?.data;
        }
        
        public DataBlockAreaWaypointGroup GetWaypointGroup (string waypointGroupKey)
        {
            if (waypointGroupsProc == null || waypointGroupsProc.Count == 0)
            {
                Debug.LogWarning ($"Failed to get waypoint group from area {key}: area has no waypoint group collection");
                return null;
            }

            if (string.IsNullOrEmpty (waypointGroupKey) || !waypointGroupsProc.ContainsKey (waypointGroupKey))
            {
                Debug.LogWarning ($"Failed to get waypoint group from area {key}: key {waypointGroupKey} returned nothing");
                return null;
            }

            var waypointGroup = waypointGroupsProc[waypointGroupKey];
            return waypointGroup;
        }
        
        
        
        private void DeselectAll ()
        {
            DataMultiLinkerCombatArea.selectedArea = null;
            DataMultiLinkerCombatArea.selectedWaypointGroup = null;
            DataMultiLinkerCombatArea.selectedWaypoint = null;
            DataMultiLinkerCombatArea.selectedSpawnGroup = null;
            DataMultiLinkerCombatArea.selectedSpawnPoint = null;
            DataMultiLinkerCombatArea.selectedLocation = null;
            DataMultiLinkerCombatArea.selectedVolume = null;
        }
        
        [HideInPlayMode]
        [GUIColor ("GetSelectedColor")]
        [Button ("@GetSelectLabel ()", ButtonSizes.Large), PropertyOrder (-3), ButtonGroup ("S")]
        public void SelectAndApplyToScene ()
        {
            if (Application.isPlaying)
                return;
            
            if (IsSelected)
                UnloadFromScene ();
            else
                TrySelection (true);
        }

        public bool TrySelection (bool applyToScene)
        {
            DeselectAll ();

            if (applyToScene)
            {
                #if !PB_MODSDK

                if (!Application.isPlaying && OverworldSceneHelper.ins != null)
                    OverworldSceneHelper.ins.SetActiveDirectly (false);

                #endif

                bool success = TryApplyLevelContentToScene ();
                if (!success)
                    return false;
            }
            
            DataMultiLinkerCombatArea.selectedArea = this;
            return true;
        }

        public void UnloadFromScene ()
        {
            if (!IsSelected)
                return;
            
            DeselectAll ();
            
            var sceneHelper = CombatSceneHelper.ins;
            if (sceneHelper != null)
                sceneHelper.areaManager.UnloadArea (false);

            #if !PB_MODSDK

            if (OverworldSceneHelper.ins != null)
                OverworldSceneHelper.ins.SetActiveDirectly (true);

            #endif

            if (sceneHelper != null)
            {
                sceneHelper.DestroyTerrainMeshes ();
                sceneHelper.segmentHelper.ClearSegments ();
                sceneHelper.fieldHelper.ClearFields ();
                
                #if !PB_MODSDK
                sceneHelper.ambientLight.OnLevelUnload ();
                #endif
            }
        }
        
        private bool IsSelected => DataMultiLinkerCombatArea.selectedArea == this;

        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerCombatArea () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        private static bool IsInheritanceVisible => DataMultiLinkerCombatArea.Presentation.showInheritance;
        private static bool AreSpawnsVisible => DataMultiLinkerCombatArea.Presentation.showSpawns;
        private static bool AreLocationsVisible => DataMultiLinkerCombatArea.Presentation.showLocations;
        private static bool AreVolumesVisible => DataMultiLinkerCombatArea.Presentation.showVolumes;
        private static bool AreFieldsVisible => DataMultiLinkerCombatArea.Presentation.showFields;
        private static bool AreWaypointsVisible => DataMultiLinkerCombatArea.Presentation.showWaypoints;
        private bool AreSelectionUtilsVisible => DataMultiLinkerCombatArea.Presentation.showSelectionUtils && IsSelected;

        private static Color colorNeutral = Color.white.WithAlpha (1f);
        private static Color colorSelected = Color.HSVToRGB (0.55f, 0.3f, 1f).WithAlpha (1f);
        
        private Color GetSelectedColor => IsSelected ? colorSelected : colorNeutral;
        
        private string GetSelectLabel ()
        {
            if (content != null)
            {
                if (IsSelected)
                    return "Deselect (and unload scene)";
                else
                    return "Select (and load scene)";
            }
            else
            {
                if (IsSelected)
                    return "Deselect";
                else
                    return "Select";
            }
        }
        
        [ShowIf ("IsSelected")]
        [Button ("Open level editor", ButtonSizes.Large), PropertyOrder (-3), ButtonGroup ("S")]
        public void OpenLevelEditor ()
        {
            var sceneHelper = CombatSceneHelper.ins;
            if (sceneHelper != null && sceneHelper.areaManager.gameObject != null)
                UnityEditor.Selection.activeGameObject = sceneHelper.areaManager.gameObject;
        }
        
        [ShowIf ("AreSelectionUtilsVisible")]
        [Button ("Snap all"), PropertyOrder (-2), ButtonGroup ("T")]
        public void Snap ()
        {
            if (spawnGroups != null)
            {
                foreach (var kvp in spawnGroups)
                {
                    if (kvp.Value != null)
                        kvp.Value.SnapToGrid ();
                }
            }

            if (locations != null)
            {
                foreach (var kvp in locations)
                {
                    if (kvp.Value != null)
                        kvp.Value.SnapToGrid ();
                }
            }
        }
        
        [ShowIf ("AreSelectionUtilsVisible")]
        [Button ("Ground all"), PropertyOrder (-2), ButtonGroup ("T")]
        public void Ground ()
        {
            if (spawnGroups != null)
            {
                foreach (var kvp in spawnGroups)
                {
                    if (kvp.Value != null)
                        kvp.Value.Ground ();
                }
            }

            if (locations != null)
            {
                foreach (var kvp in locations)
                {
                    if (kvp.Value != null)
                        kvp.Value.Ground ();
                }
            }
        }

        // [Button ("Grab boundary"), PropertyOrder (-2), ButtonGroup ("T")]
        public void GrabBoundary ()
        {
            var sceneHelper = CombatSceneHelper.ins;
            if (sceneHelper == null)
                return;

            var bn = sceneHelper.boundary;
            if (boundary == null)
                boundary = new DataBlockEnvironmentBoundary ();

            boundary.east = new AnimationCurveContainer (CopyCurve (bn.interpolationCurves.easternCurve));
            boundary.west = new AnimationCurveContainer (CopyCurve (bn.interpolationCurves.westernCurve));
            boundary.north = new AnimationCurveContainer (CopyCurve (bn.interpolationCurves.northernCurve));
            boundary.south = new AnimationCurveContainer (CopyCurve (bn.interpolationCurves.southernCurve));

            boundary.innerSkirtSize = bn.innerSkirtSize;
            boundary.groundHeight = bn.interpolationCurves.groundHeight;
            boundary.curveBoundsHeight = bn.interpolationCurves.boundsCurve.extents.y - bn.interpolationCurves.boundsLevel.extents.y;
            boundary.curveBoundsOffset = bn.interpolationCurves.boundsCurve.extents.x - bn.interpolationCurves.boundsLevel.extents.x;
        }

        private AnimationCurve CopyCurve (AnimationCurve source)
        {
            if (source == null)
                return null;
            
            var result = new AnimationCurve (source.keys);
            result.preWrapMode = source.preWrapMode;
            result.postWrapMode = source.postWrapMode;
            return result;
        }

        private IEnumerable<string> GetAreaTags =>
            DataMultiLinkerCombatArea.tags;

        #endif
        #endregion
        
        #if PB_MODSDK
        [YamlIgnore]
        [NonSerialized]
        public bool errorsCorrectedOnLoad;
        #endif
    }

    public interface IDataBlockAreaVolumeProvider
    {
        bool TryGetVolumeData (DataContainerCombatArea area, HashSet<string> keysOccupied, SortedDictionary<string, string> stateVolumes, out string volumeKey);
    }

    [Serializable]
    public class DataBlockAreaVolumeTagged
    {
        [YamlIgnore, HideInInspector, NonSerialized]
        public string key;

        //[ValueDropdown("GetTags")]
        public HashSet<string> tags = new HashSet<string> ();

        [BoxGroup, HideLabel]
        public DataBlockAreaVolume data = new DataBlockAreaVolume ();

        #region Editor
        #if UNITY_EDITOR

        private IEnumerable<string> GetTags =>
            DataShortcuts.sim.combatVolumeTags;
        
        [Button ("@GetSelectLabel"), PropertyOrder (-2), ButtonGroup ("T")]
        public void SelectForEditing ()
        {
            if (DataMultiLinkerCombatArea.selectedVolume == this)
                DataMultiLinkerCombatArea.selectedVolume = null;
            else
                DataMultiLinkerCombatArea.selectedVolume = this;
        }
        
        [Button ("Remove"), PropertyOrder (-2), ButtonGroup ("T")]
        public void Remove ()
        {
            if 
            (
                string.IsNullOrEmpty (key) || 
                DataMultiLinkerCombatArea.selectedArea?.volumes == null ||
                !DataMultiLinkerCombatArea.selectedArea.volumes.ContainsValue (this)
            )
            {
                return;
            }

            DataMultiLinkerCombatArea.selectedArea.volumes.Remove (key);
        }

        private string GetSelectLabel => DataMultiLinkerCombatArea.selectedVolume == this ? "Deselect" : "Select";

        [ShowIf ("@DataMultiLinkerCombatArea.selectedArea != null")]
        [Button ("Rename/Duplicate"), PropertyOrder (-2)]
        public void Duplicate (bool duplicate, string keyNew)
        {
            if (data == null || DataMultiLinkerCombatArea.selectedArea == null)
                return;

            var volumes = DataMultiLinkerCombatArea.selectedArea.volumes;
            if (volumes.ContainsKey (keyNew))
            {
                Debug.LogWarning ($"Key {keyNew} is already taken, can't duplicate");
                return;
            }
            
            if (duplicate)
            {
                var copy = UtilitiesYAML.CloneThroughYaml (this);
                copy.key = keyNew;
                DataMultiLinkerCombatArea.selectedArea.volumes.Add (keyNew, copy);
                DataMultiLinkerCombatArea.selectedVolume = copy;
            }
            else
            {
                DataMultiLinkerCombatArea.selectedArea.volumes.Remove (key);
                DataMultiLinkerCombatArea.selectedArea.volumes.Add (keyNew, this);
                key = keyNew;
            }
            
            UnityEditor.SceneView.RepaintAll ();
        }

        #endif
        #endregion
    }

    [Serializable, HideReferenceObjectPicker]
    public class DataBlockAreaVolume
    {
        [OnValueChanged ("ValidateOrigin")]
        public Vector3Int origin = new Vector3Int (50, 2, 50);

        [OnValueChanged ("ValidateSize")]
        public Vector3Int size = new Vector3Int (4, 4, 4);

        public bool TryGetVolumeData (out string volumeKey, out Vector3Int volumeOrigin, out Vector3Int volumeSize)
        {
            volumeKey = null;
            volumeOrigin = origin;
            volumeSize = size;
            return true;
        }

        /*
        [Button("Shrinkwrap"), PropertyOrder(-2), ButtonGroup("T")]
        private void Shrinkwrap ()
        {
            var am = CombatSceneHelper.ins.areaManager;
            var shrinkBounds = am.GetShrinkwrapBounds (origin, origin + size + Vector3Int.size1x1x1Neg);

            origin.y = shrinkBounds.topY;
            size.y = shrinkBounds.bottomY - shrinkBounds.topY + 1;
        }
        */

        public void ValidateSize ()
        {
            size = new Vector3Int
            (
                Mathf.Max (1, size.x),
                Mathf.Max (1, size.y),
                Mathf.Max (1, size.z)
            );
        }
        
        public void ValidateOrigin ()
        {
            origin = new Vector3Int
            (
                Mathf.Clamp (origin.x, 0, 100),
                Mathf.Clamp (origin.y, 0, 30),
                Mathf.Clamp (origin.z, 0, 100)
            );
        }

        // [Button("Show points"), PropertyOrder(-2), ButtonGroup("T")]
        private void ValidatePoints ()
        {
            /*
            ScenarioUtility.GetVolumeState 
            (
                this, 
                out float volumeIntegrity, 
                out int volumePointsFull,
                out int volumePointsDestructible, 
                out int volumePointsDestroyed
            );
            */
            
            float integrity = 0f;
            int pointsFull = 0;
            int pointsDestructible = 0;
            int pointsDestroyed = 0;

            var am = CombatSceneHelper.ins.areaManager;
            if (am == null || am.points == null || am.points.Count < 8)
                return;

            Vector3Int boundsFull = am.boundsFull;
            Vector3Int boundsLocal = size;
            int volumeLengthLocal = boundsLocal.x * boundsLocal.y * boundsLocal.z;

            var points = am.points;
            int pointsTotal = am.points.Count;

            for (int i = 0; i < volumeLengthLocal; ++i)
            {
                Vector3Int internalPositionLocal = AreaUtility.GetVolumePositionFromIndex (i, boundsLocal);
                Vector3Int internalPositionFull = internalPositionLocal + origin;
                int sourcePointIndex = AreaUtility.GetIndexFromInternalPosition (internalPositionFull, boundsFull);

                // Skip if index is invalid
                if (sourcePointIndex < 0 || sourcePointIndex >= pointsTotal)
                    continue;

                // Fetch the point, skip if it's empty
                var sourcePoint = points[sourcePointIndex];
                Debug.Log ($"P{i} | Local: {internalPositionLocal} | Full: {internalPositionFull} | Index: {sourcePointIndex} | State: {sourcePoint.pointState}");
                
                if (sourcePoint.pointState == AreaVolumePointState.Empty)
                    continue;

                // Increment count of all non-empty points
                pointsFull++;
                
                // Skip all points that designers specified as indestructible,
                // or points that are indestructible due to factors like height or tileset
                if (sourcePoint.indestructibleAny)
                    continue;

                // Increment count of destroyed points
                if (sourcePoint.pointState == AreaVolumePointState.FullDestroyed)
                {
                    pointsDestructible++;
                    pointsDestroyed++;
                    DebugExtensions.DrawCube (sourcePoint.pointPositionLocal, Vector3.forward, Vector3.up, Vector3.right, Vector3.one, Color.yellow, 5f);
                }
                else
                {
                    pointsDestructible++;
                    DebugExtensions.DrawCube (sourcePoint.pointPositionLocal, Vector3.forward, Vector3.up, Vector3.right, Vector3.one, Color.green, 5f);
                }
            }

            // This value is 1 when nothing destructible is destroyed and 0 when everything destructible has been destroyed
            if (pointsDestructible > 0)
                integrity = Mathf.Clamp01 (1f - (float)pointsDestroyed / pointsDestructible);

            Debug.Log ($"Origin: {origin} | Size: {size} | Length: {volumeLengthLocal} | Full points: {pointsFull} | Destructible points: {pointsDestructible} | Destroyed points: {pointsDestroyed} | Integrity: {integrity}");
        }

        //private IEnumerable<string> GetTags =>
        //   DataShortcuts.sim.volumeTrackTags;
    }

    [Serializable, HideReferenceObjectPicker]
    public class DataBlockAreaVolumeFromState : IDataBlockAreaVolumeProvider
    {
        public string stateKey;

        public bool TryGetVolumeData (DataContainerCombatArea area, HashSet<string> keysOccupied, SortedDictionary<string, string> stateVolumes, out string volumeKey)
        {
            volumeKey = null;

            if (!Application.isPlaying || string.IsNullOrEmpty (stateKey))
                return false;

            if (!stateVolumes.ContainsKey (stateKey))
                return false;

            volumeKey = stateVolumes[stateKey];
            return true;
        }
        
        public override string ToString ()
        {
            return $"Volume search from state: {stateKey}";
        }
    }

    [Serializable, HideReferenceObjectPicker]
    public class DataBlockAreaVolumeTagFilter : IDataBlockAreaVolumeProvider
    {
        // [PropertyOrder (-1)]
        // public bool ignoreConflicts;
        
        [PropertyOrder (-1)]
        //[DictionaryKeyDropdown("@DataShortcuts.sim.combatVolumeTags")]
        public SortedDictionary<string, bool> tagRequirements = new SortedDictionary<string, bool> { { "example", true } };

        private static List<DataBlockAreaVolumeTagged> volumesFound = new List<DataBlockAreaVolumeTagged> ();

        public bool TryGetVolumeData (DataContainerCombatArea area, HashSet<string> keysOccupied, SortedDictionary<string, string> stateVolumes, out string volumeKey)
        {
            volumeKey = null;

            #if !PB_MODSDK

            if (!Application.isPlaying || tagRequirements == null || tagRequirements.Count == 0)
                return false;

            if (area == null || area.volumesProc == null)
                return false;
            
            var keysOccupiedChecked = keysOccupied != null && keysOccupied.Count > 0; // !ignoreConflicts && 
            volumesFound.Clear ();

            foreach (var kvp in area.volumesProc)
            {
                // Skip all volumes already claimed by other states
                var key = kvp.Key;
                if (keysOccupiedChecked && keysOccupied.Contains (key))
                    continue;

                var volumeCandidate = kvp.Value;

                bool invalid = false;
                var tagsInVolume = volumeCandidate.tags;
                bool tagsInVolumePresent = tagsInVolume != null;

                foreach (var kvp2 in tagRequirements)
                {
                    string tag = kvp2.Key;
                    bool required = kvp2.Value;
                    bool present = tagsInVolumePresent && tagsInVolume.Contains (tag);

                    if (present != required)
                    {
                        invalid = true;
                        break;
                    }
                }

                if (invalid)
                    continue;

                volumesFound.Add (volumeCandidate);
            }

            if (volumesFound.Count == 0)
                return false;

            var volume = volumesFound.GetRandomEntry ();
            var data = volume.data;
            if (data == null)
                return false;

            volumeKey = volume.key;

            #endif
            return true;
        }
        
        public override string ToString ()
        {
            return $"Volume search by tag filter: {tagRequirements.ToStringFormattedKeyValuePairs ()}";
        }
    }

    [Serializable, HideReferenceObjectPicker]
    public class DataBlockAreaVolumeKey : IDataBlockAreaVolumeProvider
    {
        // [PropertyOrder (-1)]
        // public bool ignoreConflicts;
        
        [PropertyOrder (-1)]
        public string key;

        public bool TryGetVolumeData (DataContainerCombatArea area, HashSet<string> keysOccupied, SortedDictionary<string, string> stateVolumes, out string volumeKey)
        {
            volumeKey = null;
            #if !PB_MODSDK

            if (!Application.isPlaying || string.IsNullOrEmpty (key))
                return false;

            if (area == null || area.volumesProc == null || !area.volumesProc.ContainsKey (key))
                return false;

            var volume = area.volumesProc[key];
            var data = volume.data;
            if (data == null)
                return false;
            
            var keysOccupiedChecked = keysOccupied != null && keysOccupied.Count > 0; // !ignoreConflicts && 
            if (keysOccupiedChecked && keysOccupied.Contains (key))
                return false;

            volumeKey = volume.key;

            #endif
            return true;
        }

        public override string ToString ()
        {
            return $"Volume search by key: {key}";
        }
    }
}

