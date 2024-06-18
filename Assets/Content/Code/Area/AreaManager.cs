using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using CustomRendering;
using PhantomBrigade;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Area
{
    [ExecuteInEditMode, SelectionBase]
    public class AreaManager : MonoBehaviour
    {
        // Singleton reference

        private static AreaManager instance
        {
            get;
            set;
        }


        public bool collisionsUsed = true;
        public bool visualizeCollisions = false;
        public bool bakeReflections = false;

        public bool logShaderUpdates = false;
        public bool logEvents = false;
        public bool logTilesetSearch = false;
        public bool resetOffsetOnLoad = false;
        public bool showSlopeDetection = false;
        
        public static bool displayOnlyVolume = false;
        public static bool displayProps = true;
        
        public static bool sliceEnabled = false;
        public static int sliceDepth = 0;
        public static float sliceColliderHeightBias = -1.5f;

        // Area data

        public string areaName = string.Empty;

        [NonSerialized]
        public List<AreaVolumePoint> points = new List<AreaVolumePoint> ();

        [NonSerialized]
        public Vector3Int boundsFull = new Vector3Int (2, 2, 2);

        [NonSerialized]
        public List<AreaPlacementProp> placementsProps = new List<AreaPlacementProp> ();

        [NonSerialized]
        public List<AreaPlacementProp> placementsPropsDestroyed = new List<AreaPlacementProp> ();

        [NonSerialized]
        public List<ReflectionProbe> reflectionProbes = new List<ReflectionProbe> ();

        [NonSerialized]
        public Dictionary<int, AreaPlacementProp> propLookupByColliderID = new Dictionary<int, AreaPlacementProp> ();

        [NonSerialized]
        public Dictionary<int, List<AreaPlacementProp>> indexesOccupiedByProps = new Dictionary<int, List<AreaPlacementProp>> ();

        [NonSerialized]
        public Dictionary<int, AreaDataNavOverride> navOverrides = new Dictionary<int, AreaDataNavOverride> ();

        public const string blockTag = "SceneHolder";
        public TilesetVertexProperties vertexPropertiesSelected = new TilesetVertexProperties ();
        
        public TilesetVertexProperties vertexPropertiesDebug = 
            new TilesetVertexProperties (0f, 0f, 0.5f, 0f, 0f, 0.5f, 0f, 0f);

        public TilesetVertexProperties vertexPropertiesDebugTerrain = 
            new TilesetVertexProperties (0.25f, 0.4f, 0.25f, 0.25f, 0.4f, 0.25f, -5f, 0f);


        public int damageRestrictionDepth = 8;
        public int damagePenetrationDepth = 32;

        private int areaChangeTracker = 0;
        public int animationChanges = 0;
        public int versionChanges = 0;

		public int ChangeTracker => areaChangeTracker;

        // private int[] r_IntArray_UpdateVolume;
        private int[] r_IntArray_UpdateSpotsAroundIndex;
        private int[] r_IntArray_UpdateDamageAroundIndex;
        private int[] r_IntArray_SetPointDamaged;

        public static bool ecsInitialized = false;
        public static World world;

        //private static EntityArchetype attachEntityArchetype;
        private static EntityArchetype pointMainArchetype;
        private static EntityArchetype pointInteriorArchetype;
        private static EntityArchetype pointEdgeArchetype;
        private static EntityArchetype propRootArchetype;
        private static EntityArchetype propChildArchetype;
        private static EntityArchetype simulationRootArchetype;

        public static Entity[] pointEntitiesMain;
        public static Entity[] pointEntitiesInterior;

        private static ComponentType componentTypeModel;
        private static ComponentType componentTypeSimulated;
        private static ComponentType componentTypeFrozen;
        private static ComponentType componentTypeStatic;
        private static ComponentType componentTypeParent;
        private static ComponentType componentTypeAttach;
        private static ComponentType componentTypeAnimatingTag;
        private static ComponentType componentTypePropertyVersion;



        // This is a region concerned with ECS related tasks

        private static void CheckECSInitialization ()
        {
            if (ecsInitialized)
                return;

            ecsInitialized = true;
            DefaultWorldInitialization.DefaultLazyEditModeInitialize();

            world = World.DefaultGameObjectInjectionWorld;
            var entityManager = world.EntityManager;
            Debug.LogWarning ("Initializing ECS");

            pointMainArchetype = entityManager.CreateArchetype
            (
                typeof (ECS.BlockID),
                typeof (PointExternalTag),

                typeof (Translation),
                typeof (Rotation),
                typeof (LocalToWorld),
                typeof (PropertyVersion),

                typeof (ScaleShaderProperty),
                typeof (HSBOffsetProperty),
                typeof (DamageProperty),
                typeof (IntegrityProperty)
            );

            pointInteriorArchetype = entityManager.CreateArchetype
            (
                typeof (ECS.BlockID),
                typeof (PointInternalTag),

                typeof (Translation),
                typeof (Rotation),
                typeof(LocalToWorld),
                typeof (PropertyVersion),

                typeof (ScaleShaderProperty),
                typeof(HSBOffsetProperty)
            );

            //Delete ME
            pointEdgeArchetype = entityManager.CreateArchetype
            (
                typeof (ECS.BlockID),
                typeof (ECS.BlockDamageEdge),
                // typeof (PointEdgeTag),

                typeof (Translation),
                typeof (Rotation),
                //typeof (Static),
                typeof(LocalToWorld),

                typeof (ScaleShaderProperty)
            );

            simulationRootArchetype = entityManager.CreateArchetype
            (
                typeof (ECS.SimulatedChunkRoot),
                typeof (Translation),
                typeof (Rotation),
                typeof (LocalToWorld)
            );

            propRootArchetype = entityManager.CreateArchetype
            (
                typeof (ECS.PropRoot),
                typeof (Translation),
                typeof (Rotation),
                typeof (Scale),
                typeof (LocalToWorld)
                //typeof (Static)
            );

            propChildArchetype = entityManager.CreateArchetype
            (
                typeof (ECS.PropChild),
                typeof (PropTag),

                typeof (Translation),
                typeof (Rotation),
                typeof (Scale),
                typeof (LocalToWorld),
                typeof (PropertyVersion),

                typeof (ScaleShaderProperty),
                typeof (HSBOffsetProperty),
                typeof (PackedPropShaderProperty)
            );

            //attachEntityArchetype = entityManager.CreateArchetype (typeof (Attach));

            componentTypeModel = typeof (InstancedMeshRenderer);
            componentTypeSimulated = typeof (ECS.BlockSimulated);
            //Remove frozen, not here anymore
            componentTypeStatic = typeof (Static);
            componentTypeParent = typeof (Parent);
            componentTypeAnimatingTag = typeof (AnimatingTag);
            componentTypePropertyVersion = typeof (PropertyVersion);
            //componentTypeAttach = typeof (Attach);

            UtilityECS.ScheduleUpdate ();
        }

        public static bool IsECSSafe ()
        {
            return ecsInitialized && world != null && world == World.DefaultGameObjectInjectionWorld;
        }




        // This is a region concerned with Unity events
        // This region contains [...]

        private void Awake ()
        {
            if (logEvents)
                Debug.Log
                (
                    "Awake | Application.isPlaying: " + Application.isPlaying +
                    #if UNITY_EDITOR
                    " | IP/CPM: " + EditorApplication.isPlayingOrWillChangePlaymode +
                    #endif
                    " | World.Active: " + (World.DefaultGameObjectInjectionWorld != null ? World.DefaultGameObjectInjectionWorld.Name : "null")
                );

            if (instance == null)
                instance = this;
        }

        private void OnEnable ()
        {
            if (instance == null)
                instance = this;

            if (logEvents)
                Debug.Log
                (
                    "OnEnable | Application.isPlaying: " + Application.isPlaying +
                    #if UNITY_EDITOR
                    " | IP/CPM: " + EditorApplication.isPlayingOrWillChangePlaymode +
                    #endif
                    " | World.Active: " + (World.DefaultGameObjectInjectionWorld != null ? World.DefaultGameObjectInjectionWorld.Name : "null")
                );

            if (!UtilityECS.IsSafeToUseWorld ())
                return;

            CheckECSInitialization ();
            ShaderGlobalHelper.CheckGlobals ();
        }

        public void OnDestroy ()
        {
            if (logEvents)
                Debug.Log
                (
                    "OnDestroy | Application.isPlaying: " + Application.isPlaying +
                    #if UNITY_EDITOR
                    " | IP/CPM: " + EditorApplication.isPlayingOrWillChangePlaymode +
                    #endif
                    " | World.Active: " + (World.DefaultGameObjectInjectionWorld != null ? World.DefaultGameObjectInjectionWorld.Name : "null")
                );

            UnloadArea (false);

            DestroyImmediate (GetHolderColliders ().gameObject);
            DestroyImmediate (GetHolderSimulatedParent ().gameObject);
            DestroyImmediate (GetHolderSimulatedLeftovers ().gameObject);

            // Clear statics for correct mode transfer
            ecsInitialized = false;
            world = null;
        }

        public void OnApplicationQuit ()
        {
            // This event is useful for testing whether we are exiting play mode, since OnDestroy can't tell it apart from level unloading
            UnloadArea (true);

            // Clear statics for correct mode transfer
            ecsInitialized = false;
            world = null;
        }

        private bool updatePlayerLoop = false;
        //private bool useTimer = true;
        //private float timer = 0f;

        private void Update ()
        {
            if (instance == null)
                instance = this;

            if (!IsECSSafe ())
                return;


        }

        //public static int pointTestIndex = -1;
        //private static int pointTestIndexLast = -1;

        private static Entity pointTestEntity = Entity.Null;
        private float pointEntityTestRotation;






        // This is a region concerned with Building

        // This region contains [...]

        public int destructiblePointCount = 0;
        public int rebuildCount = 0;
        private bool terrainUsed = false;

        [ContextMenu ("Rebuild everything")]
        public void RebuildEverything ()
        {
            terrainUsed = false;
            rebuildCount += 1;

            /*
            if (rebuildCount > 10)
            {
                Debug.Log ("AM | Exiting rebuild due to rebuild call count being at " + rebuildCount);
                return;
            }
            */

            UpdateVolume (false);

            UpdateAllSpots (false);

            RebuildAllBlocks ();

            RebuildAllBlockDamage ();

            ExecuteAllPropPlacements ();

            ApplyShaderPropertiesEverywhere ();

            RebuildCollisions ();

            UpdateStructureAnalysis ();

            RecheckNavOverrides ();

            UpdateShaderGlobals ();

            UpdateTerrain (terrainUsed, true);

            UpdateDestructionCounters ();
        }

        public void UpdateDestructionCounters ()
        {
            destructiblePointCount = 0;
            for (int i = 0; i < points.Count; ++i)
            {
                var point = points[i];
                if (point == null)
                    continue;

                if (point.pointState == AreaVolumePointState.Empty)
                    continue;

                bool indestructible = IsPointIndestructible (point, true, true, true, true, true);
                if (indestructible)
                    continue;

                destructiblePointCount += 1;
            }
        }

        public void UpdateShaderGlobals ()
        {
            ShaderGlobalHelper.SetOcclusionShift (boundsFull.y * 3f + 4.5f);
            AreaTilesetHelper.ReapplyMaterialOverrides ();
        }

        public void UpdateTerrain (bool rebuildRequired, bool activationAllowed)
        {
            var sceneHelper = CombatSceneHelper.ins;
            if (sceneHelper == null)
            {
                return;
            }

            var terrain = sceneHelper.terrain;
            if (activationAllowed && terrainUsed != terrain.gameObject.activeSelf)
            {
                terrain.gameObject.SetActive (terrainUsed);
            }
            if (rebuildRequired)
            {
                #if PB_MODSDK
                Debug.Log ("Rebuilding terrain for combat area " + areaName);
                #endif
                terrain.Rebuild ();
            }
        }


        /// <summary>
        /// Fairly straightforward method, makes sure we have the right volume point array size and clears it/creates a floor in it when needed
        /// </summary>
        /// <param name="clearVolume">Whether we need to clear all edits and return the volume point array to the initial state</param>

        public void UpdateVolume (bool forceReset)
        {
            // Since a volume below 2x2x2 won't contain any Spots, we have to limit the minimum size
            boundsFull = new Vector3Int ((int)Mathf.Max (boundsFull.x, 2), (int)Mathf.Max (boundsFull.y, 2), (int)Mathf.Max (boundsFull.z, 2));
            int volumeLength = boundsFull.x * boundsFull.y * boundsFull.z;

            if
            (
                points == null ||
                points.Count == 0 ||
                points.Count != volumeLength ||
                forceReset
            )
            {
                if (points != null && points.Count != 0)
                    ResetVolumeDamage ();

                points = new List<AreaVolumePoint> (new AreaVolumePoint[volumeLength]);
				++areaChangeTracker;

                for (int i = 0; i < points.Count; ++i)
                {
                    AreaVolumePoint point = new AreaVolumePoint ();

                    point.spotIndex = i;
                    point.pointPositionIndex = AreaUtility.GetVolumePositionFromIndex (i, boundsFull, log: false);
                    point.pointPositionLocal =
                        new Vector3 (point.pointPositionIndex.x, -point.pointPositionIndex.y, point.pointPositionIndex.z) *
                        TilesetUtility.blockAssetSize;

                    point.instancePosition = point.pointPositionLocal + new Vector3 (1f, -1f, 1f) * (TilesetUtility.blockAssetSize / 2f);

                    if (point.pointPositionIndex.y >= boundsFull.y - 2)
                        point.pointState = AreaVolumePointState.Full;
                    else
                        point.pointState = AreaVolumePointState.Empty;

                    point.terrainOffset = 0f;
                    point.integrityForDestructionAnimation = 1f;

                    points[i] = point;
                }

                for (int i = 0; i < points.Count; ++i)
                {
                    AreaVolumePoint scenePoint = points[i];
                    var neighbourIndexes = AreaUtility.GetNeighbourIndexesInXxYxZ (i, Vector3Int.size2x2x2, Vector3Int.size0x0x0, boundsFull);
                    if (scenePoint.pointsInSpot == null)
                    {
                        scenePoint.pointsInSpot = new AreaVolumePoint[8];
                        scenePoint.pointsInSpot[0] = scenePoint;
                    }
                    for (int n = 1; n < 8; ++n)
                    {
                        int neighbourIndex = neighbourIndexes[n];
                        if (neighbourIndex != -1)
                            scenePoint.pointsInSpot[n] = points[neighbourIndex];
                        else
                        {
                            scenePoint.spotPresent = false;
                            scenePoint.pointsInSpot[n] = null;
                        }
                    }

                    neighbourIndexes = AreaUtility.GetNeighbourIndexesInXxYxZ (i, Vector3Int.size2x2x2, Vector3Int.size1x1x1Neg, boundsFull);
                    if (scenePoint.pointsWithSurroundingSpots == null)
                    {
                        scenePoint.pointsWithSurroundingSpots = new AreaVolumePoint[8];
                        scenePoint.pointsWithSurroundingSpots[7] = scenePoint;
                    }
                    for (int n = 0; n < 7; ++n)
                    {
                        int neighbourIndex = neighbourIndexes[n];
                        scenePoint.pointsWithSurroundingSpots[n] = neighbourIndex != -1 ? points[neighbourIndex] : null;
                    }
                }
            }
        }




        public void UpdateAllSpots (bool triggerNavigationUpdate)
        {
            for (int i = 0; i < points.Count; ++i)
                UpdateSpotAtPoint (points[i], triggerNavigationUpdate);
        }

        public void UpdateSpotsAroundIndex (int index, bool triggerNavigationUpdate, bool log = false)
        {
            if (!index.IsValidIndex (points))
            {
                if (log)
                    Debug.Log ("AM | UpdateSpotsAroundIndex | Index " + index + " is not valid for point collection sized at " + points.Count);
                return;
            }

            var point = points[index];
            for (int i = 0; i < point.pointsWithSurroundingSpots.Length; ++i)
            {
                var pointAround = point.pointsWithSurroundingSpots[i];
                UpdateSpotAtPoint (pointAround, triggerNavigationUpdate);
            }
        }

        public void UpdateSpotAtIndex (int index, bool triggerNavigationUpdate, bool log = false, bool resetSubtypeOnChange = false)
        {
            if (!index.IsValidIndex (points))
            {
                if (log)
                    Debug.Log ("AM | UpdateSpotAtIndex | Index " + index + " is not valid for point collection sized at " + points.Count);
                return;
            }

            var point = points[index];
            UpdateSpotAtPoint (point, triggerNavigationUpdate, log, resetSubtypeOnChange);
        }

        public void UpdateSpotAtPoint (AreaVolumePoint point, bool triggerNavigationUpdate, bool log = false, bool resetSubtypeOnChange = false)
        {
            if (point == null)
            {
                return;
            }

            if (log)
            {
                Debug.LogFormat
                (
                    "AM | UpdateSpotAtIndex | Starting spot update on point {0} with current configuration {1}",
                    point.spotIndex,
                    point.spotConfiguration
                );
            }

            // We want to avoid starting the evaluation on last levels on X, Y and Z axes, since we are evaluating two levels deep
            // if (points.Count <= index || index < 0 || position.x == boundsFull.x - 1 || position.y == boundsFull.y - 1 || position.z == boundsFull.z - 1)
            //     return;

            if (!point.spotPresent)
            {
                if (log)
                {
                    Debug.Log ("AM | UpdateSpotAtIndex | Point has no spot, aborting");
                }
                return;
            }

            // Move this to direct modification of a byte with bitwise operations, per-spot bool arrays should not be used at all costs
            // Noticed this comment, and decided to change it while I was looking for other things [AJ] (Insert Hal gif here)

            var configurationOld = point.spotConfiguration;
            var configuration = 0;
            var configurationWithDamage = 0;

            for (var i = 0; i < point.pointsInSpot.Length; ++i)
            {
                var pointInSpot = point.pointsInSpot[AreaUtility.configurationIndexRemapping[i]];
                if (pointInSpot == null)
                {
                    continue;
                }
                if (pointInSpot.pointState == AreaVolumePointState.Empty)
                {
                    continue;
                }

                // Dealing with bytes is a bit tricky (pun intended), any non bit operations will cause them to convert to ints
                // If we use a bitshift, we can shift 1 << i number of spaces, so we get a 1 in the correct bit position. Since the order is actually reversed, I'm going from the right index to the left (7 - i)
                // Finally if we use logical OR, we can combine the result with our existing byte data, if either is 1 in each byte, the result will be 1 in that index
                var bit = 1 << (7 - i);
                configuration |= bit;

                if (pointInSpot.pointState == AreaVolumePointState.FullDestroyed)
                {
                    continue;
                }

                configurationWithDamage |= bit;
            }

            // Update primary configuration
            point.spotConfiguration = (byte)(configuration & 0xFF);

            // Determine if spot has damaged points, but do not write it to point just yet, to allow for comparison a bit later
            // Before that, for some additional filtering at the trigger point, update configuration with damage
            var spotHasDamagedPoints = configuration != configurationWithDamage;
            point.spotConfigurationWithDamage = spotHasDamagedPoints
                ? (byte)(configurationWithDamage & 0xFF)
                : point.spotConfiguration;

            // If the spot only just became damaged, record this
            if (spotHasDamagedPoints && !point.spotHasDamagedPoints)
            {
                point.instanceVisibilityInterior = 1f;
            }

            // Overwrite point field now that comparison is done
            point.spotHasDamagedPoints = spotHasDamagedPoints;

            if (resetSubtypeOnChange && point.spotConfiguration != configurationOld)
            {
                point.blockFlippedHorizontally = false;
                point.blockGroup = 0;
                point.blockSubtype = 0;
            }
        }

        public void RebuildAllBlocks ()
        {
            if (AreaTilesetHelper.database.tilesets == null || AreaTilesetHelper.database.tilesets.Count == 0)
                return;

            for (int i = 0; i < points.Count; ++i)
            {
                if (points[i] != null)
                    RebuildBlock (points[i], false);
            }
        }

        public void RebuildBlocksAroundIndex (int index)
        {
            var point = points[index];
            for (int i = 0; i < point.pointsWithSurroundingSpots.Length; ++i)
            {
                var pointNearby = point.pointsWithSurroundingSpots[i];
                if (pointNearby != null)
                    RebuildBlock (pointNearby, false);
            }
        }




        private void ValidateBlockSubtypes (AreaVolumePoint point, AreaBlockDefinition definition, AreaTileset tileset)
        {
            byte groupUsed = point.blockGroup;
            if (definition == null)
            {
                // Debug.LogWarning ("ValidateBlockSubtypes | Definition is null for point " + point.spotIndex + " with config " + point.spotConfiguration + " | Tileset: " + tileset.name);
                // Debug.DrawLine (point.instancePosition, point.instancePosition + Vector3.up, Color.red, 60f, false);
                return;
            }

            if (definition.subtypeGroups == null)
            {
                // Debug.Log ("ValidateBlockSubtypes | Definition subtype groups are null for point with config " + point.spotConfiguration);
                return;
            }

            bool groupExistsNonNull = definition.subtypeGroups.ContainsKey (groupUsed);

            if (!groupExistsNonNull)
            {
                if (definition.subtypeGroups.ContainsKey (0))
                    groupUsed = 0;
                else
                    groupUsed = definition.subtypeGroups.First ().Key;

                Debug.LogWarning ($"Corrected point {point.spotIndex} group from {point.blockGroup} to {groupUsed} due to original group not being found | Input: {point.blockTileset} / {point.spotConfiguration}");
                point.blockGroup = groupUsed;
            }

            SortedDictionary<byte, GameObject> subtypes = definition.subtypeGroups[groupUsed];
            byte subtypeUsed = point.blockSubtype;
            bool subtypeExistsNonNull = (groupExistsNonNull || groupUsed == 0) ? subtypes.ContainsKey (subtypeUsed) : false;

            if (!subtypeExistsNonNull)
            {
                if (subtypes.ContainsKey (0))
                    subtypeUsed = 0;
                else
                    subtypeUsed = subtypes.First ().Key;

                Debug.LogWarning ($"Corrected point {point.spotIndex} subtype from {point.blockGroup}/{point.blockSubtype} to {point.blockGroup}/{subtypeUsed} due to original subtype not being found | Input: {point.blockTileset} / {point.spotConfiguration}");
                point.blockSubtype = subtypeUsed;
            }
        }

        /// <summary>
        /// This method determines whether or not the point is considered to be a part of the dynamic terrain or not
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool IsPointTerrain (AreaVolumePoint point)
        { 
            if (point == null || displayOnlyVolume)
                return false;
            return point.blockTileset == AreaTilesetHelper.idOfTerrain;
        }

        /// <summary>
        /// This method takes a spot and retrieves its intended vertex position in World Space
        /// Artyom : This is where you would add any offset values to the point position calculations
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Vector3 GetSpotVertexPosition (AreaVolumePoint point)
        {
            if (point == null)
            {
                return Vector3.zero;
            }
            var offset = point.terrainOffset * TilesetUtility.blockAssetSize;
            return new Vector3
            (
                point.pointPositionLocal.x,
                point.instancePosition.y + offset,
                point.pointPositionLocal.z
            );
        }



        private GameObject r_RB_InteriorObject;
        private GameObject r_RB_MainObject;

        private GameObject r_RB_BlockPrefab;

        private Vector3 r_RB_FixedScale = Vector3.one;


        public void RebuildBlock (AreaVolumePoint point, bool log)
        {
            var tilesets = AreaTilesetHelper.database.tilesets;
            if (tilesets == null || tilesets.Count == 0)
            {
                if (log)
                {
                    Debug.Log ("AM | RebuildBlockAtIndex | Aborting due to tilesets not being found");
                }
                return;
            }

            var tilesetMain = !tilesets.ContainsKey (point.blockTileset) || displayOnlyVolume ? AreaTilesetHelper.database.tilesetFallback : tilesets[point.blockTileset]; 
            if (tilesetMain == null)
            {
                if (log)
                    Debug.Log ("AM | RebuildBlockAtIndex | Aborting due to main tileset being null");
                return;
            }

            if (IsPointTerrain (point))
            {
                terrainUsed = true;
                ClearInstancedModel (pointEntitiesInterior, point);
                ClearInstancedModel (pointEntitiesMain, point);
                return;
            }

            if
            (
                point.spotHasDamagedPoints &&
                point.spotConfigurationWithDamage != AreaNavUtility.configEmpty &&
                point.spotConfigurationWithDamage != AreaNavUtility.configFull &&
                tilesetMain.interior != null
            )
            {
                var tilesetInterior = tilesetMain.interior;
                var configurationDataWithDamage = AreaTilesetHelper.database.configurationDataForBlocks[point.spotConfigurationWithDamage];
                var definitionWithDamage = tilesetInterior.blocks[point.spotConfigurationWithDamage];

                UpdatePointTransform
                (
                    configurationDataWithDamage,
                    point.blockRotation,
                    point.blockFlippedHorizontally,
                    ref point.instanceInteriorScaleAndSpin,
                    ref point.instanceInteriorRotation
                );

                UpdateInstancedModel
                (
                    pointEntitiesInterior,
                    tilesetInterior.id,
                    AreaTilesetHelper.assetFamilyBlock,
                    point.spotConfigurationWithDamage,
                    0,
                    0,
                    point.instanceInteriorRotation,
                    point,
                    pointInteriorArchetype,
                    ref point.instanceInteriorScaleAndSpin,
                    log
                );
            }
            else
            {
                ClearInstancedModel (pointEntitiesInterior, point);
                point.instanceVisibilityInterior = 0f;
            }

            if
            (
                point.spotConfiguration != AreaNavUtility.configEmpty &&
                point.spotConfiguration != AreaNavUtility.configFull
            )
            {
                var cfg = point.spotConfiguration;
                var configurationData = AreaTilesetHelper.database.configurationDataForBlocks[cfg];
                var definition = tilesetMain.blocks[point.spotConfiguration];

                int tilesetSafe = point.blockTileset;
                byte groupSafe = point.blockGroup;
                byte subtypeSafe = point.blockSubtype;
                
                byte group = point.blockGroup;
                byte subtype = point.blockSubtype;

                if (displayOnlyVolume)
                {
                    tilesetSafe = AreaTilesetHelper.idOfFallback;
                    groupSafe = group = 0;
                    subtypeSafe = subtype = 0;

                    if (cfg == AreaNavUtility.configRoofEdgeZNeg || 
                        cfg == AreaNavUtility.configRoofEdgeZPos || 
                        cfg == AreaNavUtility.configRoofEdgeXNeg || 
                        cfg == AreaNavUtility.configRoofEdgeXPos)
                    {
                        subtypeSafe = subtype = 3;
                    }
                }
                
                var definitionFound = definition != null;
                if (definitionFound && definition.subtypeGroups != null)
                {
                    var groupPresent = definition.subtypeGroups.ContainsKey (group);
                    if (!groupPresent)
                    {
                        groupSafe = definition.subtypeGroups.Keys.First ();
                        if (log)
                        {
                            Debug.LogFormat
                            (
                                "Group swapped from {0} to {1} which is first group key out of {2} | index: {3}",
                                group,
                                groupSafe,
                                definition.subtypeGroups.Count,
                                point.spotIndex
                            );
                        }
                    }

                    var subtypePresent = groupPresent && definition.subtypeGroups[groupSafe].ContainsKey (subtype);
                    if (!subtypePresent)
                    {
                        subtypeSafe = definition.subtypeGroups[groupSafe].Keys.First ();
                        if (log)
                        {
                            Debug.LogFormat
                            (
                                "Subtype swapped from {0} to {1} which is first subtype key out of {2} | index: {3}",
                                subtype,
                                subtypeSafe,
                                definition.subtypeGroups[groupSafe].Count,
                                point.spotIndex
                            );
                        }
                    }
                }
                else
                {
                    tilesetSafe = AreaTilesetHelper.database.tilesetFallback.id;
                    groupSafe = 0;
                    subtypeSafe = 0;
                    Debug.LogWarningFormat
                    (
                        "Definition for config {0} ({1}) was null in tileset {2}, fallback tileset {3} | index: {4}",
                        point.spotConfiguration,
                        TilesetUtility.GetStringFromConfiguration (point.spotConfiguration),
                        tilesetMain.name,
                        AreaTilesetHelper.idOfFallback,
                        point.spotIndex
                    );

                    point.blockTileset = AreaTilesetHelper.database.tilesetFallback.id;
                    point.blockGroup = 0;
                    point.blockSubtype = 0;
                    point.blockFlippedHorizontally = false;
                }

                UpdatePointTransform
                (
                    configurationData,
                    point.blockRotation,
                    point.blockFlippedHorizontally,
                    ref point.instanceMainScaleAndSpin,
                    ref point.instanceMainRotation
                );

                UpdateInstancedModel
                (
                    pointEntitiesMain,
                    tilesetSafe,
                    AreaTilesetHelper.assetFamilyBlock,
                    cfg,
                    groupSafe, 
                    subtypeSafe, 
                    point.instanceMainRotation,
                    point,
                    pointMainArchetype,
                    ref point.instanceMainScaleAndSpin,
                    log
                );

                if (!definitionFound)
                {
                    Debug.LogWarningFormat
                    (
                        "Definition for config {0} ({1}) was null in tileset {2}, fallback used | index: {3}",
                        point.spotConfiguration,
                        TilesetUtility.GetStringFromConfiguration (point.spotConfiguration),
                        tilesetMain.name,
                        point.spotIndex
                    );
                    // ClearInstancedModel (pointEntitiesMain, point);

                    #if UNITY_EDITOR
                    DrawHighlightSpot (point);
                    #endif
                }
            }
            else
            {
                ClearInstancedModel (pointEntitiesMain, point);
            }

            ApplyShaderPropertiesAtPoint (point, ShaderOverwriteMode.None, true, true, true);
        }




        private void UpdatePointTransform
        (
            AreaConfigurationData configData,
            byte blockRotation,
            bool blockFlippedHorizontally,
            ref Vector4 pointScaleAndSpin,
            ref Quaternion pointRotation
        )
        {
            // Since relation of axes to indexes at block export time might not match the one in our spot coordinate system, we add an easily adjustable
            // rotation shift variable, changing the block rotation from the 90 degree basis by N 90 degree rotations
            float spin =
                TilesetUtility.blockAssetRotationBasis * ((configData.requiredRotation + TilesetUtility.blockAssetRotationShift) % 4) +
                TilesetUtility.blockAssetRotationBasis * (configData.customRotationPossible ? blockRotation : 0f);

            // Since the flipping axis at block export time might not match the one in our spot coordinate system, we use a flipping axis bool that applies flipping to Z instead of X if used
            pointScaleAndSpin = configData.requiredFlippingZ ? TilesetUtility.blockAssetScaleFlipped : Vector4.one;
            pointScaleAndSpin.w = spin;
            pointRotation = Quaternion.Euler (0f, spin, 0f);

            // Next we need to apply additional scaling from flipping
            if (configData.customFlippingMode != -1 && blockFlippedHorizontally)
            {
                int flipMode = configData.customFlippingMode;
                Vector3 flipRotationBase = pointRotation.eulerAngles;

                float flipScaleX = flipMode == 0 ? -1f : 1f;
                float flipScaleY = flipMode == 1 ? -1f : 1f;
                float flipScaleZ = (flipMode == 2 || flipMode == 3 || flipMode == 4) ? -1f : 1f;
                float flipRotationCompensation = flipMode == 3 ? -90f : (flipMode == 4 ? 90f : 0f);

                if (TilesetUtility.blockAssetFlipDifferentAxis && (flipMode == 3 || flipMode == 4))
                {
                    float flipScaleXTemporary = flipScaleX;
                    flipScaleX = flipScaleZ;
                    flipScaleZ = flipScaleXTemporary;
                }

                pointScaleAndSpin = new Vector4 (pointScaleAndSpin.x * flipScaleX, pointScaleAndSpin.y * flipScaleY, pointScaleAndSpin.z * flipScaleZ, 1f);
                pointRotation = Quaternion.Euler (flipRotationBase + new Vector3 (0f, flipRotationCompensation, 0f));
                pointScaleAndSpin.w = flipRotationBase.y + flipRotationCompensation;
            }
        }

        // Mark this entity for a single update, if you want to update an entity for animation, use SetEntityAnimation(Entity entity, bool animating)
        // Artyom, feel free to move this method to a better place -AJ
        public static void MarkEntityDirty (Entity entity)
        {
            var entityManager = world.EntityManager;
            if (!entityManager.Exists (entity))
                return;

            instance.versionChanges += 1;

            if (entityManager.HasComponent (entity, componentTypePropertyVersion))
            {
                int currentVersion = entityManager.GetComponentData<PropertyVersion>(entity).version;
                entityManager.SetComponentData (entity, new PropertyVersion { version = currentVersion + 1});
            }
        }


        // Mark this entity for continued animation updates (per frame / tick). If you want a single update, use MarkEntityDirty(Entity entity)
        // Artyom, feel free to move this to a better place - AJ
        public static void SetEntityAnimation (Entity entity, bool animating)
        {
            var entityManager = world.EntityManager;
            instance.animationChanges += 1;

            if (animating)
            {
                if (!entityManager.HasComponent (entity, componentTypeAnimatingTag))
                    entityManager.AddComponent (entity, componentTypeAnimatingTag);
            }
            else
            {
                if (entityManager.HasComponent (entity, componentTypeAnimatingTag))
                    entityManager.RemoveComponent (entity, componentTypeAnimatingTag);
            }
        }

        private void UpdateInstancedModel
        (
            Entity[] entityList,
            int tileset,
            byte family,
            byte configuration,
            byte group,
            byte subtype,
            Quaternion rotation,
            AreaVolumePoint point,
            EntityArchetype archetype,
            ref Vector4 pointScaleAndSpin,
            bool log = false
        )
        {
            if (!IsECSSafe ())
            {
                if (log)
                {
                    Debug.Log ("Skipping point due to ECS safety check");
                }
                return;
            }

            var entityManager = world.EntityManager;
            var entity = entityList[point.spotIndex];

            if (entity == Entity.Null)
            {
                // Debug.Log ($"Point {point.spotIndex} has registered a new interior model");
                entityList[point.spotIndex] = CreatePointInteriorEntity (entityManager, archetype, point.instancePosition, quaternion.identity, point.spotIndex);
                entity = entityList[point.spotIndex];
                point.instanceVisibilityInterior = 1f;
            }

            var model = AreaTilesetHelper.GetInstancedModel
            (
                tileset,
                family,
                configuration,
                group,
                subtype,
                true,
                true,
                out var invalidVariantDetected,
                out var verticalFlip,
                out var lightData
            );

            // Not sure why this is here
            // TODO: Investigate why this is needed at all when GetInstancedModel already includes checks for invalid variants and null mesh/material

            if (invalidVariantDetected)
            {
                point.blockFlippedHorizontally = false;
                point.blockGroup = 0;
                point.blockSubtype = 0;

                model = AreaTilesetHelper.GetInstancedModel
                (
                    tileset,
                    family,
                    configuration,
                    group,
                    subtype,
                    true,
                    true,
                    out invalidVariantDetected,
                    out verticalFlip,
                    out lightData
                );

                Debug.LogWarning ("Detected invalid customization indexes on point " + point.spotIndex + ", recovered by setting group/subtype to 0-0");
            }

            if (model.mesh == null || model.material == null)
            {
                // Debug.LogWarning ($"Detected null model ({model.mesh == null}) or material ({model.material == null})");
                model = AreaTilesetHelper.GetInstancedModel
                (
                    AreaTilesetHelper.idOfFallback,
                    AreaTilesetHelper.assetFamilyBlock,
                    configuration,
                    0,
                    0,
                    false,
                    true,
                    out invalidVariantDetected,
                    out verticalFlip,
                    out lightData
                );
            }

            // Very quick and dirty way to inject vertical flipping as necessary
            // This is _currently_ safe because UpdateInstancedModel is currently always preceded by UpdatePointTransform where this vector is reset
            // This might not be safe to do in the future, so consider unifying there methods or making UpdatePointTransform separately access model specifics
            if (verticalFlip)
                pointScaleAndSpin = new Vector4 (pointScaleAndSpin.x, -pointScaleAndSpin.y, pointScaleAndSpin.z, pointScaleAndSpin.w);

            if (entityManager.HasComponent (entity, componentTypeModel))
                entityManager.SetSharedComponentData (entity, model);
            else
                entityManager.AddSharedComponentData (entity, model);

            entityManager.SetComponentData (entity, new Rotation { Value = rotation });

            if (point.simulatedHelper != null)
            {
                FreeEntityForSimulation (ref entityManager, entity, point);
            }
            else
            {
                if (entityManager.HasComponent (entity, componentTypeFrozen))
                    entityManager.RemoveComponent (entity, componentTypeFrozen);
            }

            // if (!Application.isPlaying)
            // {
            //     if (!entityManager.HasComponent (entity, componentTypeAnimatingTag))
            //     {
            //         entityManager.AddComponent (entity, componentTypeAnimatingTag);
            //         if (log)
            //             Debug.Log ($"Marking entity {entity.Index} as animated");
            //     }
            // }

            point.lightData = lightData;

            MarkEntityDirty (entity);
            UtilityECS.ScheduleUpdate ();
        }

        private void ClearInstancedModel (Entity[] entityList, AreaVolumePoint point)
        {
            if (!IsECSSafe ())
                return;

            Entity entity = entityList[point.spotIndex];
            ClearInstancedModel (entity);
        }

        private void ClearInstancedModel (Entity entity)
        {
            if (!IsECSSafe ())
                return;

            EntityManager entityManager = world.EntityManager;
            if (entity == Entity.Null)
                return;

            if (entityManager.HasComponent (entity, componentTypeModel))
                entityManager.RemoveComponent (entity, componentTypeModel);

            UtilityECS.ScheduleUpdate ();
        }

        private void FreeEntityForSimulation (ref EntityManager entityManager, Entity entity, AreaVolumePoint point)
        {
            if (point.simulatedHelper == null)
                return;

            if (!entityManager.HasComponent (entity, componentTypeModel))
                return;

            AddPropertyAnimation (point);

            // Tag component just in case we'll want to filter to check these out
            var blockSimulated = new ECS.BlockSimulated { indexOfHelper = point.simulatedHelper.id };
            if (!entityManager.HasComponent (entity, componentTypeSimulated))
                entityManager.AddComponentData (entity, blockSimulated);
            else
                entityManager.SetComponentData (entity, blockSimulated);

            // Falling pieces shouldn't be static
            if (entityManager.HasComponent (entity, componentTypeFrozen))
                entityManager.RemoveComponent (entity, componentTypeFrozen);

            if (entityManager.HasComponent (entity, componentTypeStatic))
                entityManager.RemoveComponent (entity, componentTypeStatic);

            // Since parenting would treat position as local, we have to change it to desired in-chunk local position
            Vector3 positionOfHelperCurrent = point.simulatedHelper.transform.position;
            Vector3 positionOfHelperInitial = point.simulatedHelper.initialPosition;
            Vector3 positionOfPointInitial = point.pointPositionLocal;
            Vector3 positionDifferenceInitial = point.pointPositionLocal - point.simulatedHelper.initialPosition;
            // Vector3 positionDifferenceLocalToWorld = point.simulatedHelper.transform.TransformPoint (positionDifferenceInitial);
            // Vector3 positionDifferenceWorldToLocal = point.simulatedHelper.transform.InverseTransformPoint (positionDifferenceInitial);

            if (entityManager.HasComponent (entity, componentTypeParent))
            {
                Entity parent = entityManager.GetComponentData<Parent> (entity).Value;
                if (parent.Index == point.simulatedHelper.entityRoot.Index)
                    return;

                entityManager.RemoveComponent (entity, componentTypeParent);
            }

            /*
            Debug.Log
            (
                point.spotIndex + " | Moving to simulated helper: " + point.simulatedHelper.name +
                "\nHelper position current: " + positionOfHelperCurrent +
                "\nHelper position initial: " + positionOfHelperInitial +
                "\nPoint position initial: " + positionOfPointInitial +
                "\nDifference between initial positions: " + positionDifferenceInitial//  +
                // "\nDifference transformed from local to world: " + positionDifferenceLocalToWorld +
                // "\nDifference transformed from world to local: " + positionDifferenceWorldToLocal
            );
            */

            Vector3 position = positionDifferenceInitial + new Vector3 (1.5f, -1.5f, 1.5f);
            entityManager.SetComponentData (entity, new Translation { Value = position });

            SetEntityAnimation (entity, true);

            if (entityManager.HasComponent (entity, typeof (Parent)))
                entityManager.SetComponentData (entity, new Parent { Value = point.simulatedHelper.entityRoot });
            else
                entityManager.AddComponentData (entity, new Parent { Value = point.simulatedHelper.entityRoot });

            if (!entityManager.HasComponent (entity, typeof (LocalToParent)))
                entityManager.AddComponent (entity, typeof (LocalToParent));

            UtilityECS.ScheduleUpdate ();
        }

        public void UpdateInstancedPositions ()
        {
            if (simulatedHelpers == null || simulatedHelpers.Count == 0)
                return;

            if (!IsECSSafe ())
                return;

            EntityManager entityManager = world.EntityManager;

            int helperCount = simulatedHelpers.Count;
            for (int i = 0; i < helperCount; ++i)
            {
                var simulatedHelper = simulatedHelpers[i];
                Entity entityRoot = simulatedHelper.entityRoot;

                // Debug.Log (string.Format ("Updated helper {0} position to {1}", r_UIP_Helper.id, r_UIP_Helper.transform.position));
                entityManager.SetComponentData (entityRoot, new Translation { Value = simulatedHelper.transform.position });
                entityManager.SetComponentData (entityRoot, new Rotation { Value = simulatedHelper.transform.rotation });
            }

            UtilityECS.ScheduleUpdate ();
        }

        private readonly List<AreaVolumePoint> pointsDestroyedAll = new List<AreaVolumePoint> ();

        public void RebuildAllBlockDamage ()
        {
            if (AreaTilesetHelper.database.tilesets == null || AreaTilesetHelper.database.tilesets.Count == 0)
                return;

            // First, we remove previously instantiated damage objects and update the damage related field on each point
            // If a spot linked to a point has at least one participating point that is damaged, we'll want to redraw that spot

            pointsDestroyedAll.Clear ();

            for (int i = 0; i < points.Count; ++i)
            {
                AreaVolumePoint point = points[i];
                point.RecheckRubbleHosting ();

                if (point.pointState == AreaVolumePointState.FullDestroyed)
                    pointsDestroyedAll.Add (point);

                if (!point.spotPresent)
                    continue;

                point.RecheckDamage ();
            }
        }


        public void UpdateDamageAroundIndex (int index)
        {
            if (AreaTilesetHelper.database.tilesets == null || AreaTilesetHelper.database.tilesets.Count == 0)
            {
                return;
            }
            if (!IsECSSafe ())
            {
                return;
            }

            // First, we remove previously instantiated damage objects and update the damage related field on each point
            // If a spot linked to a point has at least one participating point that is damaged, we'll want to redraw that spot

            var pointOrigin = points[index];
            foreach (var point in pointOrigin.pointsWithSurroundingSpots)
            {
                if (point == null)
                {
                    continue;
                }
                point.RecheckRubbleHosting ();
                if (!point.spotPresent)
                {
                    continue;
                }
                point.RecheckDamage ();
            }

            foreach (var point in pointOrigin.pointsWithSurroundingSpots)
            {
                if (point == null || !point.spotPresent)
                {
                    continue;
                }
                ApplyShaderPropertiesAtPoint (point, ShaderOverwriteMode.None, false, false, true);
                if (!indexesOccupiedByProps.TryGetValue (point.spotIndex, out var placements))
                {
                    continue;
                }
                foreach (var placement in placements)
                {
                    AreaAnimationSystem.OnRemoval (placement, point.spotHasDamagedPoints);
                }
            }
        }

        // This is a region concerned with Collisions

        public void RebuildCollisions ()
        {
            if (!collisionsUsed)
                return;

            for (int i = 0; i < points.Count; ++i)
                RebuildCollisionForPoint (points[i]);
        }

        public void RebuildCollisionsAroundIndex (int pointIndex)
        {
            if (!collisionsUsed)
                return;

            var point = points[pointIndex];
            if (point == null)
                return;

            for (int i = 0; i < 8; ++i)
            {
                var pointAround = point.pointsWithSurroundingSpots[i];
                if (pointAround != null)
                    RebuildCollisionForPoint (pointAround);
            }
        }

        private static readonly Vector3 colliderSize = new Vector3 (3.1f, 3.1f, 3.1f);

        public void RebuildCollisionForPoint (AreaVolumePoint avp)
        {
            if (!collisionsUsed)
                return;

            if (avp == null)
                return;

            bool boxColliderUsed = avp.pointState == AreaVolumePointState.Full && avp.simulatedHelper == null;
            if (boxColliderUsed && Application.isPlaying)
            {
                // See if it's possible to skip collisions on terrain exclusive blocks (only at runtime to avoid level editor issues)
                bool terrainExclusive = true;
                for (int s = 0; s < 8; ++s)
                {
                    AreaVolumePoint pointWithNeighbourSpot = avp.pointsWithSurroundingSpots[s];

                    if (pointWithNeighbourSpot == null)
                        continue;

                    if (pointWithNeighbourSpot.spotConfiguration == AreaNavUtility.configEmpty || pointWithNeighbourSpot.spotConfiguration == AreaNavUtility.configFull)
                        continue;

                    if (pointWithNeighbourSpot.blockTileset != AreaTilesetHelper.idOfTerrain)
                    {
                        terrainExclusive = false;
                        break;
                    }
                }

                if (terrainExclusive)
                    boxColliderUsed = false;
            }

            if (boxColliderUsed)
            {
                if (avp.instanceCollider == null)
                {
                    GameObject go = new GameObject ("ColliderForPoint");
                    go.hideFlags = HideFlags.DontSave;
                    go.transform.parent = GetHolderColliders ();
                    go.transform.localPosition = avp.pointPositionLocal;
                    go.layer = Constants.environmentLayer;
                    avp.instanceCollider = go;

                    BoxCollider bc = go.AddComponent<BoxCollider> ();
                    bc.size = colliderSize;

                    if (visualizeCollisions && avp.pointPositionIndex.y < damageRestrictionDepth)
                    {
                        GameObject vis = PrimitiveHelper.CreatePrimitive (PrimitiveType.Cube, false);
                        vis.name = "ColliderForPointVisual";
                        vis.hideFlags = HideFlags.DontSave;
                        vis.transform.parent = go.transform;
                        vis.transform.localScale = Vector3.one * 2.5f;
                        vis.transform.localPosition = Vector3.zero;

                        if (visualCollisionMaterial == null)
                            visualCollisionMaterial = Resources.Load<Material> ("Content/Debug/AreaCollision");

                        MeshRenderer mr = vis.GetComponent<MeshRenderer> ();
                        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        mr.sharedMaterial = visualCollisionMaterial;
                    }
                }

                if (!avp.instanceCollider.activeSelf)
                    avp.instanceCollider.SetActive (true);
            }
            else
            {
                if (avp.instanceCollider != null && avp.instanceCollider.activeSelf)
                    avp.instanceCollider.SetActive (false);
            }
        }

        private Material visualCollisionMaterial;
        private Material visualSimulationMaterial;
        private Material visualHolderMaterial;




        // This is a region concerned with EditorReset

        public void ResetVolumeDamage ()
        {
            if (points == null)
                return;

            for (int i = 0; i < points.Count; ++i)
            {
                var point = points[i];

                // Necessary because this method is called from UpdateVolume, where volume points might not have been set up yet
                if (point == null)
                    continue;

                if (point.pointState == AreaVolumePointState.FullDestroyed)
                    point.pointState = AreaVolumePointState.Full;

                point.RecheckRubbleHosting ();
                point.integrity = 1f;
                point.integrityForDestructionAnimation = 1f;
            }

            RebuildEverything ();
        }

        public void EraseAndResetScene ()
        {
            UpdateVolume (true);
            RemoveAllProps ();
            RebuildEverything ();
        }

        public void ResetSubtyping ()
        {
            if (points == null)
                return;

            for (int i = 0; i < points.Count; ++i)
            {
                var point = points[i];
                point.blockFlippedHorizontally = false;
                point.blockGroup = 0;
                point.blockSubtype = 0;
            }

            RebuildEverything ();
        }

        public void ResetPropOffsets ()
        {
            for (int i = 0; i < placementsProps.Count; ++i)
            {
                var placement = placementsProps[i];
                placement.offsetX = 0;
                placement.offsetZ = 0f;
                ExecutePropPlacement (placement);
            }
        }

        public void RemoveAllProps (string filter = null)
        {
            bool filterUsed = !string.IsNullOrEmpty (filter);

            for (int i = placementsProps.Count - 1; i >= 0; --i)
            {
                var placement = placementsProps[i];
                if (filterUsed && !placement.prototype.name.Contains (filter))
                    continue;

                RemovePropPlacement (placement);
            }

            // placementsProps = new List<AreaPlacementProp> ();
            RebuildEverything ();
        }

        public void RemoveAllFloatingProps ()
        {
            for (int i = placementsProps.Count - 1; i >= 0; --i)
            {
                var placement = placementsProps[i];

                Vector3 rootPosition = points[placement.pivotIndex].instancePosition;
                if (placement.prototype.prefab.allowPositionOffset)
                    rootPosition += AreaUtility.GetPropOffsetAsVector (placement, placement.state.cachedRootRotation);

                var rootPositionRaycast = AreaUtility.GroundPoint (rootPosition);
                if (Vector3.Distance (rootPosition, rootPositionRaycast) > 0.5f)
                {
                    Debug.Log ($"Removing prop placement {placement.id} at point {placement.pivotIndex}");
                    Debug.DrawLine (rootPosition, rootPositionRaycast, Color.red, 5f);
                    RemovePropPlacement (placement);
                }
            }

            // placementsProps = new List<AreaPlacementProp> ();
            RebuildEverything ();
        }

        public void RemovePropsWithIds (HashSet<int> propIds)
        {
	        List<AreaPlacementProp> propsToRemove = new List<AreaPlacementProp>();
            foreach (var prop in placementsProps)
	        {
		        if (propIds.Contains (prop.id))
			        propsToRemove.Add (prop);
	        }

	        foreach (var prop in propsToRemove)
                RemovePropPlacement (prop);

            RebuildEverything ();
        }

        public void ResetTextureOverrides (int tilesetFilter = -1)
        {
            if (tilesetFilter != -1)
            {
                for (int i = 0; i < points.Count; ++i)
                {
                    AreaVolumePoint point = points[i];
                    if (point.blockTileset == tilesetFilter)
                        point.customization.overrideIndex = 0f;
                }
            }
            else
            {
                for (int i = 0; i < points.Count; ++i)
                {
                    AreaVolumePoint point = points[i];
                    point.customization.overrideIndex = 0f;
                }
            }


            ApplyShaderPropertiesEverywhere ();
        }



        // This is a region concerned with EditorVertexColors
        // This region contains all vertex-color related methods

        public void FixBrightnessValues ()
        {
            for (int i = 0; i < points.Count; ++i)
            {
                AreaVolumePoint avp = points[i];
                if (avp.customization.brightnessPrimary == 0f || avp.customization.brightnessPrimary == 1f)
                    avp.customization = new TilesetVertexProperties (avp.customization.huePrimary, avp.customization.saturationPrimary, 0.5f, avp.customization.hueSecondary, avp.customization.saturationSecondary, avp.customization.brightnessSecondary, avp.customization.overrideIndex, avp.customization.damageIntensity);
                if (avp.customization.brightnessSecondary == 0f || avp.customization.brightnessSecondary == 1f)
                    avp.customization = new TilesetVertexProperties (avp.customization.huePrimary, avp.customization.saturationPrimary, avp.customization.brightnessPrimary, avp.customization.hueSecondary, avp.customization.saturationSecondary, 0.5f, avp.customization.overrideIndex, avp.customization.damageIntensity);
            }

            ApplyShaderPropertiesEverywhere ();
        }

        public void SetAllBlocksToDefault ()
        {
            for (int i = 0; i < points.Count; ++i)
            {
                AreaVolumePoint avp = points[i];
                avp.customization = TilesetVertexProperties.defaults;
            }

            ApplyShaderPropertiesEverywhere ();
        }

        [ContextMenu ("Drop saturation")]
        public void FixSaturation ()
        {
            for (int i = 0; i < points.Count; ++i)
            {
                AreaVolumePoint avp = points[i];
                if (avp.customization.saturationPrimary > 0.5f)
                    avp.customization = new TilesetVertexProperties (avp.customization.huePrimary, avp.customization.saturationPrimary * 0.9f, avp.customization.brightnessPrimary, avp.customization.hueSecondary, avp.customization.saturationSecondary, avp.customization.brightnessSecondary, avp.customization.overrideIndex, avp.customization.damageIntensity);
                if (avp.customization.saturationSecondary > 0.5f)
                    avp.customization = new TilesetVertexProperties (avp.customization.huePrimary, avp.customization.saturationPrimary, avp.customization.brightnessPrimary, avp.customization.hueSecondary, avp.customization.saturationSecondary * 0.9f, avp.customization.brightnessSecondary, avp.customization.overrideIndex, avp.customization.damageIntensity);
            }

            ApplyShaderPropertiesEverywhere ();
        }

        [ContextMenu ("Replace all empty colors")]
        public void ReplaceAllEmptyColors ()
        {
            for (int i = 0; i < points.Count; ++i)
            {
                AreaVolumePoint avp = points[i];
                if (avp.spotConfiguration == 0)
                    avp.customization = vertexPropertiesSelected;
            }

            ApplyShaderPropertiesEverywhere ();
        }

        [ContextMenu ("Replace all interior colors")]
        public void ReplaceAllInteriorColors ()
        {
            for (int i = 0; i < points.Count; ++i)
            {
                AreaVolumePoint avp = points[i];
                if (avp.spotConfiguration == 255)
                    avp.customization = vertexPropertiesSelected;
            }

            ApplyShaderPropertiesEverywhere ();
        }

        public void ApplyShaderPropertiesEverywhere ()
        {
            if (logShaderUpdates)
                Debug.Log ($"Applying shader properties everywhere...");

            for (int i = 0, count = points.Count; i < count; ++i)
            {
                AreaVolumePoint avp = points[i];
                if (avp == null || avp.spotConfiguration == AreaNavUtility.configEmpty || avp.spotConfiguration == AreaNavUtility.configFull)
                    continue;

                ApplyShaderPropertiesAtPoint (avp, ShaderOverwriteMode.None, true, true, true);
            }
        }

        public void ApplyShaderPropertiesToIndexes (List<int> indexes)
        {
            if (logShaderUpdates)
                Debug.Log ($"Applying shader properties to {indexes.Count} indexes...");

            for (int i = 0, count = indexes.Count; i < count; ++i)
            {
                var index = indexes[i];
                AreaVolumePoint avp = points[index];
                if (avp == null || avp.spotConfiguration == AreaNavUtility.configEmpty || avp.spotConfiguration == AreaNavUtility.configFull)
                    continue;

                ApplyShaderPropertiesAtPoint (avp, ShaderOverwriteMode.None, true, true, true);
            }
        }


		public enum ShaderOverwriteMode
		{
			None,
			All,
			AllExceptOverrideIndex
		}

        // private Vector4 r_ASPP_ColorA;
        // private Vector4 r_ASPP_ColorB;
        // private Vector4 r_ASPP_DamageTop;
        // private Vector4 r_ASPP_DamageBottom;
        // private Vector4 r_ASPP_IntegrityTop;
        // private Vector4 r_ASPP_IntegrityBottom;
        // private float r_ASPP_Destruction;
        // private int r_ASPP_Frame;
        // private int r_ASPP_Count;

        public void ApplyShaderPropertiesAtPoint
        (
            AreaVolumePoint point,
            ShaderOverwriteMode overwriteMode,
            bool updateCore,
            bool updateIntegrity,
            bool updateDamage
        )
        {
	        if (point == null || !point.spotPresent)
		        return;

	        if (!IsECSSafe ())
		        return;

			TilesetVertexProperties vertexProps = point.customization;

			if (overwriteMode == ShaderOverwriteMode.All)
				vertexProps = vertexPropertiesSelected;
			else if (overwriteMode == ShaderOverwriteMode.AllExceptOverrideIndex)
			{
				vertexProps = vertexPropertiesSelected;
				vertexProps.overrideIndex = point.customization.overrideIndex;
			}

			ApplyShaderPropertiesAtPoint (point, vertexProps, updateCore, updateIntegrity, updateDamage);
	    }

        public void ApplyShaderPropertiesAtPoint
        (
            AreaVolumePoint point,
            TilesetVertexProperties customization,
            bool updateCore,
            bool updateIntegrity,
            bool updateDamage
        )
        {
            if (point == null || !point.spotPresent)
                return;

            if (!IsECSSafe ())
                return;

            int pointCount = pointEntitiesMain.Length;
            int pointIndex = point.spotIndex;
            bool pointIndexValid = pointIndex >= 0 && pointIndex < pointCount;
            if (!pointIndexValid)
                return;

            point.customization = customization;
            if (displayOnlyVolume)
            {
                customization = vertexPropertiesDebug;
                bool terrain = point.blockTileset == AreaTilesetHelper.idOfTerrain;
                if (terrain)
                    customization = vertexPropertiesDebugTerrain;
            }

            EntityManager em = world.EntityManager;

            Entity entityMain = pointEntitiesMain[point.spotIndex];
            bool entityMainPresent = entityMain != Entity.Null && em.HasComponent<InstancedMeshRenderer> (entityMain);

            Entity entityInterior = pointEntitiesInterior[point.spotIndex];
            bool entityInteriorPresent = entityInterior != Entity.Null && em.HasComponent<InstancedMeshRenderer> (entityInterior);

            if (!entityMainPresent && !entityInteriorPresent)
                return;

            Vector4 damageTop = Vector4.zero;
            Vector4 damageBottom = Vector4.zero;
            float damageCritical = 0f;

            if (updateDamage)
                GetShaderDamage (point, out damageTop, out damageBottom, out damageCritical);

            Vector4 integrityTop = Vector4.one;
            Vector4 integrityBottom = Vector4.one;
            if (updateIntegrity)
                GetShaderIntegrity (point, out integrityTop, out integrityBottom);

            Vector4 hsbPrimaryEmission = new Vector4 (customization.huePrimary, customization.saturationPrimary, customization.brightnessPrimary, customization.overrideIndex);
            Vector4 hsbSecondaryDestruction = new Vector4 (customization.hueSecondary, customization.saturationSecondary, customization.brightnessSecondary, damageCritical);

            if (entityMainPresent)
            {
                ApplyShaderPropertiesToEntity
                (
                    em,
                    entityMain,
                    updateCore,
                    updateIntegrity,
                    updateDamage,
                    point.instanceMainScaleAndSpin,
                    hsbPrimaryEmission,
                    hsbSecondaryDestruction,
                    integrityTop,
                    integrityBottom,
                    damageTop,
                    damageBottom,
                    false
                );

                if (logShaderUpdates)
                    Debug.Log ($"- P{point.spotIndex} | Integrity: {integrityTop}/{integrityBottom} | Damage: {damageTop}/{damageBottom}");
            }

            if (entityInteriorPresent)
            {
                var cs = hsbSecondaryDestruction;
                cs = new Vector4 (cs.x, cs.y, cs.z, Mathf.Clamp01 (cs.w + (1f - point.instanceVisibilityInterior)));

                ApplyShaderPropertiesToEntity
                (
                    em,
                    entityInterior,
                    updateCore,
                    false,
                    updateDamage,
                    point.instanceInteriorScaleAndSpin,
                    hsbPrimaryEmission,
                    cs,
                    integrityTop,
                    integrityBottom,
                    damageTop,
                    damageBottom,
                    true
                );
            }

            //UtilityECS.ScheduleUpdate ();
        }

        private void ApplyShaderPropertiesToEntity
        (
            EntityManager em,
            Entity entity,
            bool updateCore,
            bool updateIntegrity,
            bool updateDamage,
            Vector4 scale,
            Vector4 hsbPrimaryEmission,
            Vector4 hsbSecondaryDestruction,
            Vector4 integrityTop,
            Vector4 integrityBottom,
            Vector4 damageTop,
            Vector4 damageBottom,
            bool interior
        )
        {
            //

            var hsbData = em.GetComponentData<HSBOffsetProperty>(entity);

            //I changed this around, because we need to update the entire packed vector8 for HSB regardless of if core is updated or damage is, and it's much faster to blit the entire thing
            //We need to make sure we don't overwrite what's already in the component data when we shouldn't, such as if we're only updating damage
            if (updateCore || updateDamage)
            {
                em.SetComponentData (entity, new ScaleShaderProperty { property = new HalfVector4(scale) });

                var hsbPrimary = updateCore ? new HalfVector4(hsbPrimaryEmission) : hsbData.property.primary;
                var hsbSecondary = new HalfVector4(hsbSecondaryDestruction);
                em.SetComponentData (entity, new HSBOffsetProperty { property =  new HalfVector8(hsbPrimary, hsbSecondary)});
            }

            if (updateIntegrity)
            {
                em.SetComponentData(entity, new IntegrityProperty {property = new FixedVector8(integrityTop, integrityBottom)});
            }

            if (updateDamage && !interior)
            {
                em.SetComponentData(entity, new DamageProperty {property = new HalfVector8(damageTop, damageBottom)});
            }

            MarkEntityDirty (entity);
        }



        private static Vector4 floatToVectorEncodingMultipliers = new Vector4 (1.0f, 255.0f, 65025.0f, 160581375.0f);
        private static float floatToVectorEncodeBit = 1.0f / 255.0f;

        private Vector4 UnpackVectorFromFloat (float v)
        {
            var result = floatToVectorEncodingMultipliers * v;
            result = new Vector4
            (
                result.x - Mathf.Floor (result.x),
                result.y - Mathf.Floor (result.y),
                result.z - Mathf.Floor (result.z),
                result.w - Mathf.Floor (result.w)
            );

            result -= new Vector4 (result.y, result.z, result.w, result.w) * floatToVectorEncodeBit;
            return result;
        }

        private static Vector4 vectorToFloatDecodeDot = new Vector4 (1.0f, 1f / 255.0f, 1f / 65025.0f, 1f / 160581375.0f);

        private float EncodeVectorToFloat (Vector4 enc)
        {
            vectorToFloatDecodeDot = new Vector4 (1.0f, 1f / 255.0f, 1f / 65025.0f, 1f / 160581375.0f);
            var result = Vector4.Dot (new Vector4 (ToByteStep (enc.x), ToByteStep (enc.y), ToByteStep (enc.z), ToByteStep (enc.w)), vectorToFloatDecodeDot);
            return result;
        }

        private const float byteStepMultiplierA = 255f;
        private const float byteStepMultiplierB = 1f / 255f;

        private float ToByteStep (float v)
        {
            return Mathf.Floor (v * byteStepMultiplierA) * byteStepMultiplierB;
            // return (float)((byte)(v * 255f)) * (1f / 255f);
        }

        /// <summary>
        /// Given a starting index, check a block of 4 spots, to determine if they are damaged, and neighbored by empty space
        /// </summary>
        /// <returns>Final color, that is a representation of the final states of all 4 points stored into rgba </returns>

        public static void GetShaderDamage (AreaVolumePoint point, out Vector4 damageTop, out Vector4 damageBottom, out float damageCritical)
        {
            damageTop = new Vector4
            (
                GetShaderDamageForPoint (point.pointsInSpot[0]),
                GetShaderDamageForPoint (point.pointsInSpot[1]),
                GetShaderDamageForPoint (point.pointsInSpot[2]),
                GetShaderDamageForPoint (point.pointsInSpot[3])
            );

            damageBottom = new Vector4
            (
                GetShaderDamageForPoint (point.pointsInSpot[4]),
                GetShaderDamageForPoint (point.pointsInSpot[5]),
                GetShaderDamageForPoint (point.pointsInSpot[6]),
                GetShaderDamageForPoint (point.pointsInSpot[7])
            );

            var animatedIntegrityLowest = 1f;

            for (var i = 0; i < point.pointsInSpot.Length; ++i)
            {
                var pointInSpot = point.pointsInSpot[i];
                if (pointInSpot == null)
                {
                    continue;
                }
                if (pointInSpot.pointState == AreaVolumePointState.Empty)
                {
                    continue;
                }

                if (pointInSpot.pointState == AreaVolumePointState.Full || pointInSpot.integrityForDestructionAnimation >= 1f)
                {
                    animatedIntegrityLowest = 1f;
                    break;
                };

                animatedIntegrityLowest = Mathf.Min (animatedIntegrityLowest, pointInSpot.integrityForDestructionAnimation);
            }

            damageCritical = 1f - animatedIntegrityLowest;
        }

        private static float GetShaderDamageForPoint (AreaVolumePoint point)
        {
            return point != null ? point.integrityForDestructionAnimation : 0f;
        }

        private void GetShaderIntegrity (AreaVolumePoint point, out Vector4 integrityTop, out Vector4 integrityBottom)
        {
            integrityTop = new Vector4
            (
                GetShaderIntegrityForPoint (point.pointsInSpot[0]),
                GetShaderIntegrityForPoint (point.pointsInSpot[1]),
                GetShaderIntegrityForPoint (point.pointsInSpot[2]),
                GetShaderIntegrityForPoint (point.pointsInSpot[3])
            );

            integrityBottom = new Vector4
            (
                GetShaderIntegrityForPoint (point.pointsInSpot[4]),
                GetShaderIntegrityForPoint (point.pointsInSpot[5]),
                GetShaderIntegrityForPoint (point.pointsInSpot[6]),
                GetShaderIntegrityForPoint (point.pointsInSpot[7])
            );
        }

        private float GetShaderIntegrityForPoint (AreaVolumePoint point)
        {
            return point != null ? point.integrity : 1f;
            // return point.state == AreaVolumePointState.FullDestroyed ? 0 : 1;
        }




        public bool debugPropClaims = true;
        public GameObject debugPropVisualPivot;
        public GameObject debugPropVisualPeriphery;
        public GameObject debugPropVisualCursor;

        public void ExecuteAllPropPlacements ()
        {
            indexesOccupiedByProps.Clear ();
            propLookupByColliderID.Clear ();

            for (int i = placementsProps.Count - 1; i >= 0; --i)
            {
                var placement = placementsProps[i];
                var point = points[placement.pivotIndex];
                var prototype = AreaAssetHelper.GetPropPrototype (placement.id);

                if (IsPropPlacementValid (placement, point, prototype))
                {
                    if (!indexesOccupiedByProps.ContainsKey (placement.pivotIndex))
                        indexesOccupiedByProps.Add (placement.pivotIndex, new List<AreaPlacementProp> ());

                    indexesOccupiedByProps[placement.pivotIndex].Add (placement);
                    ExecutePropPlacement (placement);
                }
                else
                {
                    Debug.Log ("AM | ExecuteAllPropPlacements | Removing prop placement " + i + " due to unavailability of one or more cells");
                    placementsProps.RemoveAt (i);
                }
            }
        }
        public bool IsPropPlacementValid (AreaPlacementProp placement, AreaVolumePoint pointForPivot, AreaPropPrototypeData prototype, bool checkConfiguration = false)
        {
            var configurationMask = AreaAssetHelper.GetPropMask (prototype.prefab.compatibility);
            var result = pointForPivot.spotPresent && (!checkConfiguration || configurationMask.Contains (pointForPivot.spotConfiguration));

            if (!result)
            {
                Debug.Log
                (
                    "AM | IsPropPlacementValid | PV-I: " + placement.pivotIndex +
                    ", R:" + placement.rotation +
                    ", F: " + placement.flipped +
                    " | Spot present: " + pointForPivot.spotPresent +
                    " | Compatible config: " + configurationMask.Contains (pointForPivot.spotConfiguration)
                );
            }

            return result;
        }


        public void ExecutePropPlacement (AreaPlacementProp placement, bool ignoreDamage = false, bool ignoreConfiguration = true, bool log = false)
        {
            if (placement == null)
                return;

            var point = points[placement.pivotIndex];
            if (point.spotPresent && !ignoreDamage && point.spotHasDamagedPoints)
            {
                if (log)
                    Debug.Log ($"Bailing due to damaged points");
                return;
            }

            if (!IsECSSafe ())
            {
                if (log)
                    Debug.Log ($"Bailing due to ECS safety");
                return;
            }

            bool displayEnabled = displayProps && !displayOnlyVolume;
            var entityManager = world.EntityManager;
            
            bool prototypePresent = placement.prototype != null;
            bool prototypePresentAndWrong = prototypePresent && placement.id != placement.prototype.id;
            
            bool entityDeletionRequired = prototypePresentAndWrong || !displayEnabled;
            bool entityCreationRequired = displayEnabled && (prototypePresentAndWrong || !entityManager.ExistsNonNull (placement.entityRoot));

            // Destroy entities from wrong prototype
            if (entityDeletionRequired)
            {
                if (placement.entitiesChildren != null)
                {
                    if (entityManager.ExistsNonNull (placement.entityRoot))
                        entityManager.DestroyEntity (placement.entityRoot);
                    for (int e = 0; e < placement.entitiesChildren.Count; ++e)
                    {
                        if (entityManager.ExistsNonNull (placement.entitiesChildren[e]))
                            entityManager.DestroyEntity (placement.entitiesChildren[e]);
                    }
                    
                    placement.entitiesChildren.Clear ();
                }

                if (placement.instanceCollision != null)
                {
                    int colliderID = placement.instanceCollision.GetInstanceID ();
                    if (propLookupByColliderID.ContainsKey (placement.instanceCollision.GetInstanceID ()))
                        propLookupByColliderID.Remove (colliderID);
                    Destroy (placement.instanceCollision);
                    placement.instanceCollision = null;
                }
            }

            if (entityCreationRequired)
            {
                placement.prototype = AreaAssetHelper.GetPropPrototype (placement.id);
                if (placement.prototype == null)
                {
                    Debug.Log ($"Failed to find find prop prototype with ID {placement.id} for placement at point {placement.pivotIndex}");
                    return;
                }

                // Abort if configuration mask doesn't work
                var configurationMask = AreaAssetHelper.GetPropMask (placement.prototype.prefab.compatibility);
                if (point.spotPresent && !ignoreConfiguration && !configurationMask.Contains (point.spotConfiguration))
                {
                    if (log)
                        Debug.Log ($"Bailing due to config incompatibility {placement.id}");
                    return;
                }

                // Proceed to creating new entities now that all the checks are done
                int subObjectCount = placement.prototype.subObjects.Count;
                if (placement.entitiesChildren == null)
                    placement.entitiesChildren = new List<Entity> (subObjectCount);
                else
                    placement.entitiesChildren.Clear ();

                Entity entityRoot = entityManager.CreateEntity (propRootArchetype);
                entityManager.SetComponentData (entityRoot, new ECS.PropRoot { id = placement.id, pivotIndex = placement.pivotIndex });
                placement.entityRoot = entityRoot;

                // if (log)
                //     Debug.Log ($"Sub-objects spawned: {subObjectCount}");

                for (int s = 0; s < subObjectCount; ++s)
                {
                    AreaPropSubObject subObject = placement.prototype.subObjects[s];
                    Entity entityChild = entityManager.CreateEntity (propChildArchetype);
                    placement.entitiesChildren.Add (entityChild);

                    entityManager.SetComponentData (entityChild, new ECS.PropChild { id = placement.id, pivotIndex = placement.prototype.id, subObjectIndex = s });
                    entityManager.AddSharedComponentData (entityChild, AreaAssetHelper.GetInstancedModel (subObject.modelID));

                    entityManager.SetComponentData (entityChild, new ScaleShaderProperty { property = new HalfVector4 (1, 1, 1, 1) });
                    entityManager.SetComponentData (entityChild, new HSBOffsetProperty { property = new HalfVector8 (new HalfVector4 (0f, 0.5f, 0.5f, 1), new HalfVector4 (0f, 0.5f, 0.5f, 1)) });
                    entityManager.SetComponentData (entityChild, new PackedPropShaderProperty { property = new HalfVector4 (1, 0, 1, 0) });
                    entityManager.SetComponentData (entityChild, new PropertyVersion { version = 1 });

                    if (!Application.isPlaying)
                        SetEntityAnimation (entityChild, true);
                    else
                    {
                        MarkEntityDirty (entityChild);
                    }

                    if (entityManager.HasComponent (entityChild, typeof (Parent)))
                    {
                        entityManager.SetComponentData (entityChild, new Parent { Value = entityRoot });
                    }
                    else
                    {
                        entityManager.AddComponentData (entityChild, new Parent { Value = entityRoot });
                        entityManager.AddComponent (entityChild, typeof (LocalToParent));
                    }

                    //Entity entityAttachment = entityManager.CreateEntity (attachEntityArchetype);
                    //EntityManager.SetComponentData (entityAttachment, new Attach { Parent = entityRoot, Child = entityChild });
                }

                // Prop collider is a basic GameObject hosting only a collider for registering impacts (no legacy prop component, meshes etc.)
                // Probably not worth pooling these since we only need to do this on level loading 
                // and pre-generating 2-5k of each collider type used on props would be wasteful

                if (placement.prototype.prefabCollider != null && Application.isPlaying)
                {
                    placement.instanceCollision = Instantiate (placement.prototype.prefabCollider, GetHolderColliders (), true);
                    placement.instanceCollision.name = "ColliderForProp";
                    placement.instanceCollision.name = placement.prototype.prefabCollider.name;
                    placement.instanceCollision.gameObject.layer = Constants.propLayer;
                    placement.instanceCollision.gameObject.SetFlags (HideFlags.DontSave);
                    propLookupByColliderID.Add (placement.instanceCollision.GetInstanceID (), placement);
                }

                // Time for other changes which are done regardless of whether new entities had to be generated
                // First we need to check rotation byte since certain position calculations use transformations dependent on rotation
                // if (point.spotPresent && placement.prototype.prefab.linkRotationToConfiguration)
                //     placement.rotation = (byte)configurationMask.IndexOf (point.spotConfiguration);
                
                // Next we tell the placement to finish setting up the prop (no point/config info needed further)
                placement.Setup (this, point);
            }

            UtilityECS.ScheduleUpdate ();
        }

        public void SnapPropRotation (AreaPlacementProp placement)
        {
            var configurationMask = AreaAssetHelper.GetPropMask (placement.prototype.prefab.compatibility);
            var point = points[placement.pivotIndex];

            if (point.spotPresent && placement.prototype.prefab.linkRotationToConfiguration)
                placement.rotation = (byte)configurationMask.IndexOf (point.spotConfiguration);

            placement.Setup (this, point);
        }






        // This is a region concerned with UtilityObjectCleanup

        public void RemovePropPlacement (AreaPlacementProp placement)
        {
            if (placement == null)
                return;

            if (indexesOccupiedByProps.ContainsKey (placement.pivotIndex))
            {
                var placementsAtSpot = indexesOccupiedByProps[placement.pivotIndex];
                if (placementsAtSpot.Count == 1)
                {
                    placementsAtSpot.Clear ();
                    indexesOccupiedByProps.Remove (placement.pivotIndex);
                }
                else
                    placementsAtSpot.Remove (placement);

            }

            placementsProps.Remove (placement);
            placement.Cleanup ();

            UtilityECS.ScheduleUpdate ();
        }

        public void RemovePropPlacement (int spotIndex)
        {
	        if (!indexesOccupiedByProps.ContainsKey (spotIndex))
				return;

	        var placementsAtSpot = indexesOccupiedByProps[spotIndex];

	        foreach (var prop in placementsAtSpot)
	        {
		        placementsProps.Remove(prop);
		        prop.Cleanup();
	        }

	        placementsAtSpot.Clear();
	        indexesOccupiedByProps.Remove (spotIndex);

	        UtilityECS.ScheduleUpdate ();
        }


        // This is a region concerned with HolderManagement
        // This region contains helper variables and methods to safely fetch or create transforms used by other methods

        private Transform holderColliders;
        private Transform holderSimulatedLeftovers;
        private Transform holderSimulatedParent;

        private string holderCollidersName = "SceneHolder_Colliders";
        private string holderSimulatedLeftoversName = "SceneHolder_SimulatedLeftovers";
        private string holderSimulatedParentName = "SceneHolder_SimulatedParent";


        public Transform GetHolderColliders ()
        {
            return UtilityGameObjects.GetTransformSafely (ref holderColliders, holderCollidersName, HideFlags.DontSave, transform.localPosition);
        }

        public Transform GetHolderSimulatedLeftovers ()
        {
            return UtilityGameObjects.GetTransformSafely (ref holderSimulatedLeftovers, holderSimulatedLeftoversName, HideFlags.DontSave, transform.localPosition, "SceneHolder");
        }

        public Transform GetHolderSimulatedParent ()
        {
            return UtilityGameObjects.GetTransformSafely (ref holderSimulatedParent, holderSimulatedParentName, HideFlags.DontSave, transform.localPosition, "SceneHolder");
        }


        public void ApplyDamageToPosition (Vector3 position, float impact, bool forceOverkillEffect = false)
        {
            var point = GetPoint (position);
            if (point != null)
				ApplyDamageToPoint (point, impact, forceOverkillEffect);
        }

        public void ApplyDamageUniformlyToGroup (Vector3 position, float impact, int reach)
        {
	        if (impact <= 0 || reach <= 0)
		        return;

            float reachSqr = reach * reach;
            for (int z = -reach; z <= reach; ++z)
	        {
		        for (int y = -reach; y <= reach; ++y)
		        {
			        for (int x = -reach; x <= reach; ++x)
			        {
				        var delta = new Vector3 (x, y, z) * TilesetUtility.blockAssetSize;
                        if (delta.sqrMagnitude > reachSqr)
							continue;

						var point = GetPoint (position + delta);
						if (point == null)
							continue;

						ApplyDamageToPoint (point, impact);
			        }
		        }
	        }
        }

        private Collider[] overlapColliders = new Collider[64];

        public void ApplyDamageToRadius (Vector3 origin, float impactDamage, float radius, float exponent, out int overlapCount)
        {
            overlapCount = Physics.OverlapSphereNonAlloc (origin, radius, overlapColliders, LayerMasks.environmentMask);
            if (overlapCount == 0)
                return;

            if (radius <= 0)
            {
                Debug.LogWarning ($"Radius of projectile splash impact was set to 0, make sure to correct this in the data");
                radius = 1f;
            }

            // if (overlapCount > 50)
            //     Debug.LogWarning ($"More than 50 points ({overlapCount}) damaged at once, consider reducing environment splash impact radius");

            var originMin = origin;
            var originMax = origin;

            originMin.y -= radius;
            originMax.y += radius;

            bool draw = DataShortcuts.sim.environmentCollisionDebug;

            // Debug.Log ($"Splash impact reached {overlapCount} environment points");
            for (int a = 0; a < overlapCount; ++a)
            {
                var col = overlapColliders[a];
                if (col == null)
                    continue;

                var hitPosition = col.transform.position;
                var hitPoint = GetPoint (hitPosition);

                // Exponent allows us to make the bulk of the splash area to be hit fairly uniformly, removing some unpredictability of AoE
                var distanceNormalized = Vector3.Distance (origin, hitPosition) / radius;
                distanceNormalized = Mathf.Pow (distanceNormalized, exponent);

                if (draw)
                    Debug.DrawLine (origin, hitPosition, Color.green, 5);

                var impact = DataLinkerSettingsSimulation.data.impactDamageFalloff.GetCurveSample (distanceNormalized) * impactDamage;
                ApplyDamageToPoint (hitPoint, impact);
            }
        }

        public bool IsDamageCriticalToPosition (Vector3 position, int hitDamage)
        {
            AreaVolumePoint point = GetPoint (position);

            if (point == null)
                return false;

            float finalDamage = hitDamage * DataLinkerSettingsArea.data.damageScalar;
            float finalIntegrity = Mathf.Clamp01 (point.integrity - finalDamage);
            return finalIntegrity > 0f;
        }

        private AreaVolumePoint GetPoint (Vector3 position)
        {
            int index = AreaUtility.GetIndexFromWorldPosition (position, GetHolderColliders ().position, boundsFull);
            if (index < 0 || index > points.Count - 1)
            {
                // Debug.LogWarning ("AM | ApplyDamageToPosition | Volume point index calculated from hit position " + position + " equals " + index + ", which is an invalid number out of bounds of the volume | Aborting");
                return null;
            }

            return points[index];
        }

        public float GetPointIntegrity (Vector3 position)
        {
            var point = GetPoint (position);

            if (point == null)
                return 0;

            return point.integrity;
        }

        public static bool IsPointIndestructible
        (
            Vector3 position,
            bool checkFlag = true,
            bool checkHeight = true,
            bool checkEdges = true,
            bool checkTilesets = true,
            bool checkFullSurroundings = true
        )
        {
            var point = instance.GetPoint (position);
            return IsPointIndestructible (point, checkFlag, checkHeight, checkEdges, checkTilesets, checkFullSurroundings);
        }

        public static bool IsPointIndestructible
        (
            AreaVolumePoint point,
            bool checkFlag,
            bool checkHeight,
            bool checkEdges,
            bool checkTilesets,
            bool checkFullSurroundings
        )
        {
            if (point == null)
                return true;

            if (checkFlag && (!point.destructible || point.indestructibleIndirect))
                return true;

            if (checkHeight && point.pointPositionIndex.y >= instance.damageRestrictionDepth)
                return true;

            if (checkEdges)
            {
                if (point.pointPositionIndex.x == 0 || point.pointPositionIndex.x == (instance.boundsFull.x - 1))
                    return true;

                if (point.pointPositionIndex.z == 0 || point.pointPositionIndex.z == (instance.boundsFull.z - 1))
                    return true;
            }

            if (checkTilesets && IsPointInvolvingIndestructibleTilesets (point))
                return true;

            if (checkFullSurroundings && point.pointPositionIndex.y >= instance.damagePenetrationDepth && IsPointSurroundedBySpotConfiguration (point, AreaNavUtility.configFull))
                return true;

            return false;
        }

        public static bool IsPointInvolvingIndestructibleTilesets (AreaVolumePoint point)
        {
            if (point == null)
                return false;

            bool pointInvolvesIndestructibleTilesets = false;
            if (point.pointsWithSurroundingSpots != null)
            {
                for (int i = 0; i < 8; ++i)
                {
                    var pointOther = point.pointsWithSurroundingSpots[i];
                    if (pointOther == null || !pointOther.spotPresent)
                        continue;

                    if
                    (
                        pointOther.spotConfiguration != AreaNavUtility.configEmpty &&
                        pointOther.spotConfiguration != AreaNavUtility.configFull &&
                        (pointOther.blockTileset == AreaTilesetHelper.idOfForest || pointOther.blockTileset == AreaTilesetHelper.idOfTerrain || pointOther.blockTileset == AreaTilesetHelper.idOfRoad)
                    )
                    {
                        pointInvolvesIndestructibleTilesets = true;
                        break;
                    }
                }
            }

            return pointInvolvesIndestructibleTilesets;
        }

        public static bool IsPointInvolvingTileset (AreaVolumePoint point, int tilesetID)
        {
            if (point == null)
                return false;

            bool pointInvolvesTileset = false;
            if (point.pointsWithSurroundingSpots != null)
            {
                for (int i = 0; i < 8; ++i)
                {
                    var pointOther = point.pointsWithSurroundingSpots[i];
                    if (pointOther == null || !pointOther.spotPresent)
                        continue;

                    if
                    (
                        pointOther.spotConfiguration != AreaNavUtility.configEmpty &&
                        pointOther.spotConfiguration != AreaNavUtility.configFull &&
                        pointOther.blockTileset == tilesetID
                    )
                    {
                        pointInvolvesTileset = true;
                        break;
                    }
                }
            }

            return pointInvolvesTileset;
        }

        public static bool IsPointSurroundedBySpotConfiguration (AreaVolumePoint point, byte config)
        {
            if (point == null)
                return false;

            bool pointSurroundedByConfiguration = true;
            if (point.pointsWithSurroundingSpots != null)
            {
                for (int i = 0; i < 8; ++i)
                {
                    var pointOther = point.pointsWithSurroundingSpots[i];
                    if (pointOther == null || !pointOther.spotPresent)
                        continue;

                    if (pointOther.spotConfiguration != config)
                    {
                        pointSurroundedByConfiguration = false;
                        break;
                    }
                }
            }

            // if (pointSurroundedByConfiguration)
            //     Debug.Log ($"{point.spotIndex} is surrounded by configuration {config}");

            return pointSurroundedByConfiguration;
        }


        private List<int> tilesetsIndexesEncountered = new List<int> (8);

        public void ApplyDamageToPoint (AreaVolumePoint point, float hitDamage, bool forceOverkillEffect = false)
        {

        }

        public AreaTileset GetClosestTileset (AreaVolumePoint point)
        {
            // Determining which tileset should influence the effect
            tilesetsIndexesEncountered.Clear ();

            for (int i = 0; i < 8; ++i)
            {
                var pointHostingSpot = point.pointsWithSurroundingSpots[i];
                if (point.blockTileset == 0 || point.spotConfiguration == AreaNavUtility.configEmpty)
                    continue;

                tilesetsIndexesEncountered.Add (pointHostingSpot.blockTileset);
            }

            if (tilesetsIndexesEncountered.Count > 0)
            {
                tilesetsIndexesEncountered.Sort ();

                // Find the max frequency using linear traversal
                int maxCount = 1;
                int currCount = 1;
                int dominantTilesetID = tilesetsIndexesEncountered[0];

                for (int i = 1; i < tilesetsIndexesEncountered.Count; i++)
                {
                    if (tilesetsIndexesEncountered[i] == tilesetsIndexesEncountered[i - 1])
                        currCount++;
                    else
                    {
                        if (currCount > maxCount)
                        {
                            maxCount = currCount;
                            dominantTilesetID = tilesetsIndexesEncountered[i - 1];
                        }

                        currCount = 1;
                    }
                }

                return AreaTilesetHelper.GetTileset (dominantTilesetID);
            }

            return AreaTilesetHelper.database.tilesetFallback;
        }

        /// <summary>
        /// Returns a tileset belonging to a spot a given position resides in, useful for doing on-hit checks
        /// </summary>
        /// <param name="position">Position in world</param>
        /// <returns></returns>

        public AreaTileset GetExactTileset (Vector3 position)
        {
            var result = AreaTilesetHelper.database.tilesetFallback;
            int index = AreaUtility.GetIndexFromWorldPosition (position + AreaUtility.spotOffset, GetHolderColliders ().position, boundsFull);
            if (index < 0 || index > points.Count - 1)
                return result;

            AreaVolumePoint point = points[index];
            if (point.blockTileset == 0 || point.spotConfiguration == AreaNavUtility.configEmpty)
                return result;

            result = AreaTilesetHelper.GetTileset (point.blockTileset);
            return result;
        }

        /// <summary>
        /// Returns a closest tileset for a given position among spots of a closest point, potentially useful for something like destruction
        /// </summary>
        /// <param name="position">Position in world</param>
        /// <returns></returns>

        public AreaTileset GetClosestTileset (Vector3 position)
        {
            var result = AreaTilesetHelper.database.tilesetFallback;
            // if (logTilesetSearch)
            //     Debug.Log ($"Looking for tileset from position {position} | Fallback tileset: {result.name}");

            int index = AreaUtility.GetIndexFromWorldPosition (position, GetHolderColliders ().position, boundsFull);
            if (index < 0 || index > points.Count - 1)
                return result;

            AreaVolumePoint point = points[index];
            //bool neighborFound = false;
            float neighborMinSqrDistance = 100f;
            // if (logTilesetSearch)
            //     Debug.Log ($"Found closest point {point.spotIndex} at local coords {point.pointPositionLocal}, inspecting influenced spots");

            for (int i = 0; i < 8; ++i)
            {
                var pointHostingSpot = point.pointsWithSurroundingSpots[i];
                if (pointHostingSpot == null || !pointHostingSpot.spotPresent || pointHostingSpot.spotConfiguration == AreaNavUtility.configEmpty)
                {
                    // if (logTilesetSearch)
                    //     Debug.Log ($"Skipping empty point {i}");
                    continue;
                }

                //neighborFound = true;
                float sqr = Vector3.SqrMagnitude (pointHostingSpot.instancePosition - position);
                if (sqr < neighborMinSqrDistance)
                {
                    if (point.blockTileset == 0 || point.spotConfiguration == AreaNavUtility.configEmpty)
                        continue;

                    result = AreaTilesetHelper.GetTileset(pointHostingSpot.blockTileset);
                    neighborMinSqrDistance = sqr;
                    // if (logTilesetSearch)
                    //     Debug.Log ($"New shortest sqr. dist on point {i}: {sqr} is smaller than {neighborMinSqrDistance} | Tileset: {result.name}");
                }
                else
                {
                    // if (logTilesetSearch)
                    //     Debug.Log ($"Not a shortest sqr. dist on point {i}: {sqr} which is more than {neighborMinSqrDistance}");
                }
            }

            if (logTilesetSearch)
                Debug.Log ($"Looked for tileset from position {position} | Result: {result.name}");

            return result;
        }

        private List<AreaVolumePoint> pointsToAnimate = new List<AreaVolumePoint> ();
        private float destructionAnimationRate = 0.20f;

        private void StartDestructionAnimation (AreaVolumePoint point)
        {
            pointsToAnimate.Add (point);
            for (int n = 0; n < point.pointsWithSurroundingSpots.Length; ++n)
            {
                var pointNeighbour = point.pointsWithSurroundingSpots[n];
                AddPropertyAnimation (pointNeighbour);
            }
        }

        private void UpdatePlayerLoop ()
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying && updatePlayerLoop)
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate ();
            #endif
        }

        private void AddPropertyAnimation(AreaVolumePoint point)
        {
            Entity entityMain = pointEntitiesMain[point.spotIndex];
            if (entityMain != Entity.Null)
            {
                SetEntityAnimation (entityMain, true);
            }

            Entity entityInterior = pointEntitiesInterior[point.spotIndex];
            if (entityInterior != Entity.Null)
            {
                SetEntityAnimation (entityInterior, true);
            }
        }

        private void RemovePropertyAnimation(AreaVolumePoint point)
        {
            if (point.spotIndex.IsValidIndex (pointEntitiesMain))
            {
                Entity entityMain = pointEntitiesMain[point.spotIndex];
                if (entityMain != Entity.Null)
                {
                    SetEntityAnimation (entityMain, false);
                }
            }

            if (point.spotIndex.IsValidIndex (pointEntitiesInterior))
            {
                Entity entityInterior = pointEntitiesInterior[point.spotIndex];
                if (entityInterior != Entity.Null)
                {
                    SetEntityAnimation (entityInterior, false);
                }
            }
        }

        public void UpdateDamageAnimation ()
        {
            if (pointsToAnimate == null)
                return;

            for (int i = pointsToAnimate.Count - 1; i >= 0; --i)
            {
                var point = pointsToAnimate[i];
                point.integrityForDestructionAnimation = Mathf.Max
                (
                    0f,
                    point.integrityForDestructionAnimation -
                    destructionAnimationRate * Time.deltaTime *
                    Mathf.Lerp (1f, 4f, point.integrityForDestructionAnimation)
                );
            }

            for (int i = pointsToAnimate.Count - 1; i >= 0; --i)
            {
                var point = pointsToAnimate[i];
                for (int n = 0; n < point.pointsWithSurroundingSpots.Length; ++n)
                {
                    AreaVolumePoint pointNeighbour = point.pointsWithSurroundingSpots[n];
                    ApplyShaderPropertiesAtPoint (pointNeighbour, ShaderOverwriteMode.None, false, false, true);
                }
            }

            for (int i = pointsToAnimate.Count - 1; i >= 0; --i)
            {
                var point = pointsToAnimate[i];
                if (point.integrityForDestructionAnimation <= 0f)
                {
                    point.integrityForDestructionAnimation = 0f;
                    pointsToAnimate.RemoveAt (i);

                    for (int n = 0; n < point.pointsWithSurroundingSpots.Length; ++n)
                    {
                        AreaVolumePoint pointNeighbour = point.pointsWithSurroundingSpots[n];
                        RemovePropertyAnimation (pointNeighbour);
                    }
                }
            }
        }




        public void ForceStructureUpdate ()
        {
            UpdateStructureAnalysis ();
            SimulateIsolatedStructures ();
        }

        private List<List<AreaVolumePoint>> structureGroups = new List<List<AreaVolumePoint>> ();
        private Queue<AreaVolumePoint> structurePointsQueue = new Queue<AreaVolumePoint> ();
        private List<AreaVolumePoint> structurePointsIsolated = new List<AreaVolumePoint> ();

        public void UpdateStructureAnalysis ()
        {
            // Debug.Log ("Updating structural analysis");

            structurePointsQueue.Clear ();
            structurePointsQueue.Enqueue (points[points.Count - 1]);

            int iterations = 0;
            int limit = points.Count + 1000;
            int defaultValue = -1;

            for (int i = 0; i < points.Count; ++i)
            {
                var point = points[i];
                if (point.simulatedHelper != null)
                    continue;

                point.structuralGroup = 0;
                point.structuralValue = defaultValue;
                point.structuralParentCandidate = null;
                point.structuralParent = null;
            }

            ProcessQueue (structurePointsQueue, ref iterations, limit, defaultValue);

            structurePointsIsolated.Clear ();
            if (structureGroups == null)
                structureGroups = new List<List<AreaVolumePoint>> ();

            for (int i = 0; i < points.Count; ++i)
            {
                var point = points[i];
                if (point.structuralValue == -1 && point.pointState == AreaVolumePointState.Full)
                    structurePointsIsolated.Add (point);
            }

            if (structurePointsIsolated.Count > 0)
            {
                int isolationIterations = 0;
                while (structurePointsIsolated.Count > 0)
                {
                    if (isolationIterations > 100)
                    {
                        // Debug.Log ("Isolated point search passes crossed 100, something is wrong");
                        break;
                    }

                    var point = structurePointsIsolated[structurePointsIsolated.Count - 1];
                    if (point.simulatedHelper != null)
                        continue;

                    point.structuralGroup = isolationIterations + 1;
                    var isolatedGroup = new List<AreaVolumePoint> ();
                    structureGroups.Add (isolatedGroup);

                    // Debug.Log ("Attempting to fill remaining " + isolatedPoints.Count + " isolated points; iteration " + isolationIterations + ", point group: " + point.structuralGroup);
                    iterations = 0;
                    structurePointsQueue.Enqueue (structurePointsIsolated[structurePointsIsolated.Count - 1]);
                    ProcessQueue (structurePointsQueue, ref iterations, limit, defaultValue, structurePointsIsolated, isolatedGroup);
                    isolationIterations++;
                }
            }

            // Debug.Log ("Isolation resolution iterations done: " + iterations + " | Isolated groups found: " + isolatedGroups.Count);
        }

        private void ProcessQueue (Queue<AreaVolumePoint> q, ref int iterations, int limit, int defaultValue, List<AreaVolumePoint> listToTrim = null, List<AreaVolumePoint> listToFill = null)
        {
            while (q.Count > 0)
            {
                var point = q.Dequeue ();

                if (q.Count > limit)
                {
                    throw new Exception ("The algorithm is probably looping. Queue size: " + q.Count);
                }

                if (point.simulatedHelper != null)
                    continue;

                if (point.structuralValue != defaultValue)
                    continue;

                point.structuralValue = iterations;
                point.structuralParent = point.structuralParentCandidate;
                point.structuralChildrenPresent = false;

                if (point.structuralParentCandidate != null)
                    point.structuralParentCandidate.structuralChildrenPresent = true;

                if (point.structuralParent != null)
                    point.structuralGroup = point.structuralParent.structuralGroup;

                if (listToFill != null)
                    listToFill.Add (point);

                if (listToTrim != null)
                {
                    if (listToTrim.Contains (point))
                        listToTrim.Remove (point);
                }

                // spotpoints: 1 (X+) 2 (Z+) 4 (Y+)
                // spotsAroundThisPoint: 3 (Y-), 5 (Z-), 6 (X-)

                var pointYPos = point.pointsInSpot[4];
                if (CheckValidity (pointYPos, point))
                    q.Enqueue (pointYPos);

                var pointXPos = point.pointsInSpot[1];
                if (CheckValidity (pointXPos, point))
                    q.Enqueue (pointXPos);

                var pointXNeg = point.pointsWithSurroundingSpots[6];
                if (CheckValidity (pointXNeg, point))
                    q.Enqueue (pointXNeg);

                var pointZPos = point.pointsInSpot[2];
                if (CheckValidity (pointZPos, point))
                    q.Enqueue (pointZPos);

                var pointZNeg = point.pointsWithSurroundingSpots[5];
                if (CheckValidity (pointZNeg, point))
                    q.Enqueue (pointZNeg);

                var pointYNeg = point.pointsWithSurroundingSpots[3];
                if (CheckValidity (pointYNeg, point))
                    q.Enqueue (pointYNeg);

                iterations++;
            }
        }

        private bool CheckValidity (AreaVolumePoint point, AreaVolumePoint structuralParentCandidate)
        {
            if (point == null || point.simulatedHelper != null)
            {
                return false;
            }
            else
            {
                bool result = point.structuralValue == -1 && point.pointState == AreaVolumePointState.Full;
                point.structuralParentCandidate = structuralParentCandidate;
                return result;
            }
        }

        public List<AreaSimulatedChunk> simulatedHelpers = null;
        public List<AreaVolumePoint> pointsDestroyedSinceLastSimulation = null;
        private int simulatedHelperCounter = 0;

        public void SimulateIsolatedStructures ()
        {
            if (structureGroups == null || structureGroups.Count == 0)
                return;

            if (!IsECSSafe ())
                return;

            //Debug.Log (string.Format ("Starting simulation of {0} isolated groups", isolatedGroups.Count));

            if (simulatedHelpers == null)
                simulatedHelpers = new List<AreaSimulatedChunk> ();

            // ECS preparation

            EntityManager entityManager = world.EntityManager;

            for (int i = structureGroups.Count - 1; i >= 0; --i)
            {
                List<AreaVolumePoint> group = structureGroups[i];
                structureGroups.RemoveAt (i);

                List<GameObject> objectsInGroup = new List<GameObject> (group.Count);

                GameObject simulatedHolder = new GameObject ("!SimulatedHolder_" + i);
                simulatedHolder.transform.position = group[0].pointPositionLocal;
                simulatedHolder.layer = Constants.debrisLayer;

                Transform simulatedParent = GetHolderSimulatedParent ();
                simulatedHolder.transform.parent = simulatedParent;

                AreaSimulatedChunk simulatedHelper = simulatedHolder.AddComponent<AreaSimulatedChunk> ();
                simulatedHelper.colliderToPointMap = new Dictionary<Collider, AreaVolumePoint> ();
                simulatedHelper.initialPosition = simulatedHolder.transform.position;
                simulatedHelpers.Add (simulatedHelper);
                simulatedHelper.pointsAffected = new List<AreaVolumePoint> (group.Count);

                simulatedHelper.id = simulatedHelperCounter;
                simulatedHelperCounter += 1;

                // var pos = simulatedHelper.gameObject.AddComponent<TranslationProxy> ();
                // pos.Value = new Translation { Value = simulatedHelper.transform.position };

                // var rot = simulatedHelper.gameObject.AddComponent<RotationProxy> ();
                // rot.Value = new Rotation { Value = simulatedHelper.transform.rotation };

                Entity entityRoot = entityManager.CreateEntity (simulationRootArchetype);
                entityManager.SetComponentData (entityRoot, new ECS.SimulatedChunkRoot { id = simulatedHelper.id });
                entityManager.SetComponentData (entityRoot, new Translation { Value = simulatedHelper.transform.position });
                entityManager.SetComponentData (entityRoot, new Rotation { Value = simulatedHelper.transform.rotation });
                simulatedHelper.entityRoot = entityRoot;

                List<AreaVolumePoint> neighbours = new List<AreaVolumePoint> ();

                for (int g = 0; g < group.Count; ++g)
                {
                    AreaVolumePoint point = group[g];

                    if (pointsDestroyedSinceLastSimulation != null)
                    {
                        for (int a = 0; a < pointsDestroyedSinceLastSimulation.Count; ++a)
                        {
                            AreaVolumePoint potentialNeighbour = pointsDestroyedSinceLastSimulation[a];
                            if (potentialNeighbour.IsNeighbor (point))
                                neighbours.Add (potentialNeighbour);
                        }
                    }

                    if (point.instanceCollider != null)
                        point.instanceCollider.SetActive (false);

                    GameObject simulatedPoint = new GameObject ("!SimulatedPoint_" + g);
                    simulatedPoint.transform.position = point.pointPositionLocal;
                    simulatedPoint.transform.parent = simulatedHolder.transform;
                    simulatedPoint.transform.localScale = Vector3.one * 3f;
                    simulatedPoint.layer = Constants.debrisLayer;

                    SphereCollider simulatedCollider = simulatedPoint.AddComponent<SphereCollider> ();
                    simulatedCollider.radius = 0.5f;
                    simulatedHelper.colliderToPointMap.Add (simulatedCollider, point);

                    if (visualizeCollisions)
                    {
                        GameObject vis = PrimitiveHelper.CreatePrimitive (PrimitiveType.Cube, false);
                        vis.name = "ColliderForSimulatedPointVisual";
                        vis.hideFlags = HideFlags.DontSave;
                        vis.transform.parent = simulatedPoint.transform;
                        vis.transform.localScale = Vector3.one * 0.5f;
                        vis.transform.localPosition = Vector3.zero;

                        if (visualSimulationMaterial == null)
                            visualSimulationMaterial = Resources.Load<Material> ("Content/Debug/AreaCollisionSimulated");

                        MeshRenderer mr = vis.GetComponent<MeshRenderer> ();
                        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        mr.sharedMaterial = visualSimulationMaterial;
                    }

                    for (int a = 0; a < 8; ++a)
                    {
                        AreaVolumePoint pointAround = point.pointsWithSurroundingSpots[a];
                        if (pointAround == null || !pointAround.spotPresent)
                            continue;

                        // This is very sub-optimal, optimize in the future if possible - we really don't want to iterate through all helper points
                        if (simulatedHelper.pointsAffected.Contains (pointAround))
                            continue;

                        simulatedHelper.pointsAffected.Add (pointAround);
                        pointAround.simulatedHelper = simulatedHelper;

                        if (indexesOccupiedByProps.ContainsKey (pointAround.spotIndex))
                        {
                            List<AreaPlacementProp> placements = indexesOccupiedByProps[pointAround.spotIndex];
                            for (int p = 0; p < placements.Count; ++p)
                                AreaAnimationSystem.OnRemoval (placements[p], true);
                        }

                        FreeEntityForSimulation (ref entityManager, pointEntitiesMain[pointAround.spotIndex], pointAround);
                        FreeEntityForSimulation (ref entityManager, pointEntitiesInterior[pointAround.spotIndex], pointAround);
                    }
                }

                Rigidbody simulatedRigidbody = simulatedHolder.AddComponent<Rigidbody> ();

                simulatedRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                simulatedRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

                simulatedHelper.simulatedRigidbody = simulatedRigidbody;
                simulatedHelper.UpdateRigidbody ();
                simulatedHelper.initialPointCount = simulatedHelper.colliderToPointMap.Count;
                // entityManager.AddComponent (entityRoot, typeof (Rigidbody));

                if (visualizeCollisions)
                {
                    GameObject vis = PrimitiveHelper.CreatePrimitive (PrimitiveType.Cube, false);
                    vis.name = "SimulatedHolderVisual";
                    vis.hideFlags = HideFlags.DontSave;
                    vis.transform.parent = simulatedHolder.transform;
                    vis.transform.localScale = Vector3.one * 3f;
                    vis.transform.localPosition = simulatedRigidbody.centerOfMass;

                    if (visualHolderMaterial == null)
                        visualHolderMaterial = Resources.Load<Material> ("Content/Debug/AreaCollisionHolder");

                    MeshRenderer mr = vis.GetComponent<MeshRenderer> ();
                    mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    mr.sharedMaterial = visualHolderMaterial;
                }

                for (int o = 0; o < objectsInGroup.Count; ++o)
                {
                    GameObject go = objectsInGroup[o];
                    objectsInGroup[o].transform.parent = simulatedHolder.transform;
                }

                if (neighbours.Count > 0)
                {
                    //Debug.LogWarning ("Simulated group " + i + " has " + neighbours.Count + " neighbours out of recently destroyed " + pointsDestroyedSinceLastSimulation.Count + " points");
                    Vector3 positionCenter = simulatedHolder.transform.TransformPoint (simulatedRigidbody.centerOfMass);
                    Vector3 positionNeighbor = neighbours[0].pointPositionLocal;

                    Vector3 difference = positionCenter - positionNeighbor;
                    Vector3 direction = Vector3.Normalize (difference);

                    /*
                    List<Vector3> linePoints = new List<Vector3> ();
                    linePoints.Add (positionNeighbor);
                    linePoints.Add (positionCenter);

                    Vectrosity.VectorLine line = new Vectrosity.VectorLine ("Line_Neighbor", linePoints, 6f, Vectrosity.LineType.Continuous);
                    line.layer = Constants.EnvironmentLayer;
                    line.material = UtilityMaterial.GetDefaultMaterial ();
                    line.continuousTexture = true;
                    line.joins = Vectrosity.Joins.None;
                    line.Draw3DAuto ();
                    */

                    // Vector3 push = new Vector3 (difference.x, 0f, difference.z);
                    // simulatedRigidbody.centerOfMass = positionNeighbor;

                    simulatedHelper.timerForForce = simulatedHelper.durationForForce = 3f;
                    simulatedHelper.positionForForce = positionNeighbor;
                    simulatedHelper.useVerticalForce = true;

                    // Vector3 push = new Vector3 (0f, new Vector2 (difference.x, difference.z).magnitude * Tweakables.data.areaSystem.crashPushForceMultiplier * group.Count, 0f);
                    // simulatedRigidbody.AddForceAtPosition (push, positionNeighbor, ForceMode.Impulse);
                }
                else
                {
                    //Debug.Log ("Simulated group " + i + " has no neighbours out of recently destroyed " + pointsDestroyedSinceLastSimulation.Count + " points");
                }
            }

            UtilityECS.ScheduleUpdate ();
        }

        public void OnSimulatedHelperFinish (AreaSimulatedChunk simulatedHelper)
        {
            if (simulatedHelpers == null)
                return;

            if (simulatedHelpers.Contains (simulatedHelper))
            {
                simulatedHelpers.Remove (simulatedHelper);
            }
        }

        public bool IsSimulationInProgress ()
        {
            return simulatedHelpers != null && simulatedHelpers.Count > 0;
        }

        private static Vector3 spotShiftVertical = Vector3.up * (TilesetUtility.blockAssetSize * 0.5f);
        private static float slopeOffsetThresholdPos = 0.3f;
        private static float slopeOffsetThresholdNeg = -0.3f;

        private bool IsPointUsableForNavOverrides (AreaVolumePoint point)
        {
            return
                point != null &&
                !point.spotHasDamagedPoints &&
                point.spotConfiguration != AreaNavUtility.configEmpty &&
                point.spotConfiguration != AreaNavUtility.configFull &&
                point.blockTileset == AreaTilesetHelper.idOfTerrain;
        }

        private void TryAddNavOverride (AreaVolumePoint point, float offset)
        {
            if (point == null)
                return;

            var index = point.spotIndex;
            if (navOverrides.ContainsKey (index))
                return;

            navOverrides.Add
            (
                index,
                new AreaDataNavOverride
                {
                    pivotIndex = index,
                    offsetY = offset
                }
            );
        }

        [ContextMenu ("Navigation override test")]
        public void RecheckNavOverrides ()
        {
            AreaNavUtility.GenerateNavOverrides (navOverrides, points, showSlopeDetection);
        }


        public void SetVisible (bool visible)
        {
            GetHolderColliders ().gameObject.SetActive (visible);
            // GetHolderBackgrounds ().gameObject.SetActive (visible);
        }
        
        public void SetVolumeDisplayMode (bool value)
        {
            if (Application.isPlaying)
                return;
            
            var area = DataMultiLinkerCombatArea.selectedArea;
            if (area == null)
            {
                Debug.LogWarning ($"Can't update display mode, no selected area available");
                return;
            }
            
            displayOnlyVolume = value;
            RebuildEverything ();
            
            if (CombatSceneHelper.ins != null)
            {
                var cs = CombatSceneHelper.ins;
                if (cs.boundary != null)
                {
                    bool backgroundTerrainUsed = area.coreProc != null && area.coreProc.backgroundTerrainUsed;
                    cs.boundary.gameObject.SetActive (backgroundTerrainUsed && !displayOnlyVolume);
                }

                if (cs.segmentHelper != null)
                    cs.segmentHelper.gameObject.SetActive (!displayOnlyVolume);
                
                if (cs.materialHelper != null)
                {
                    var biomeKey = "mossy_neutral";
                    var biomeData = DataMultiLinkerCombatBiome.GetEntry (biomeKey, false);
                    if (biomeData != null)
                        cs.materialHelper.ApplyBiome (biomeData, true);
                }

                if (cs.fieldHelper != null)
                {
                    bool fieldsUsed = area.fieldsProc != null;
                    cs.fieldHelper.gameObject.SetActive (fieldsUsed && !displayOnlyVolume);
                }
            }
        }
        
        private void ResetEditingModes ()
        {
            if (CombatSceneHelper.ins != null && CombatSceneHelper.ins.materialHelper != null)
                CombatSceneHelper.ins.materialHelper.SetupSlicingForLevelEditor (false, 0);

            displayProps = true;
            displayOnlyVolume = false;
            sliceEnabled = false;
            sliceDepth = 0;
        }

        public void UpdateSlicing ()
        {
            if (Application.isPlaying || CombatSceneHelper.ins == null || CombatSceneHelper.ins.materialHelper == null)
                return;
            
            var m = CombatSceneHelper.ins.materialHelper;
            m.SetupSlicingForLevelEditor (sliceEnabled, sliceDepth);

            if (points != null)
            {
                float yMax = sliceEnabled ? (-sliceDepth * 3f + sliceColliderHeightBias) : 1000f;
                var configEmpty = AreaNavUtility.configEmpty;
                var configFull = AreaNavUtility.configFull;
                
                for (int i = 0, iLimit = points.Count; i < iLimit; ++i)
                {
                    var point = points[i];
                    if (point != null && point.instanceCollider != null)
                    {
                        var col = point.instanceCollider;
                        var y = point.instanceCollider.transform.position.y;
                        bool active = true;
                        
                        if (sliceEnabled)
                        {
                            active = y < yMax;
                            if (active)
                            {
                                bool surfaceNear = false;
                                for (int p = 0; p < 8; ++p)
                                {
                                    var pointAround = point.pointsWithSurroundingSpots[p];
                                    if (pointAround == null)
                                        continue;
                                    
                                    if (pointAround.spotConfiguration != configEmpty && pointAround.spotConfiguration != configFull)
                                    {
                                        surfaceNear = true;
                                        break;
                                    }
                                }

                                if (!surfaceNear)
                                    active = false;
                            }
                        }
                        
                        if (col.activeSelf != active)
                            col.SetActive (active);
                    }
                }
            }
        }

        private byte terrainOffsetByteThreshold = 128;

        public void LoadArea (string key, AreaDataCore core, AreaDataContainer data)
        {
            rebuildCount = 0;
            ResetEditingModes ();
            UnloadArea (false);

            if (data == null || core == null)
            {
                Debug.LogWarning ("AM | LoadArea | Failed to load area from container, null reference received");
                return;
            }

            if (!ecsInitialized)
                CheckECSInitialization ();

            if (!IsECSSafe ())
            {
                Debug.LogWarning ($"Can't load level due to ECS safety check returning false | ECS initialized: {ecsInitialized} | ECS world: {world != null} | ECS world match: {world == World.DefaultGameObjectInjectionWorld}");
                return;
            }

            Debug.Log ("Loading combat area " + key);

            areaName = key;
            boundsFull = core.bounds;
            damageRestrictionDepth = core.damageRestrictionDepth;
            damagePenetrationDepth = core.damagePenetrationDepth;

            ShaderGlobalHelper.CheckGlobals ();
            AreaTilesetHelper.CheckResources ();
            AreaAssetHelper.CheckResources ();

            #if !PB_MODSDK
            if (Application.isPlaying)
            {
                var game = Contexts.sharedInstance.game;
                game.ReplaceAreaActive (name);
            }
            #endif

            if (damagePenetrationDepth == 0)
            {
                damagePenetrationDepth = boundsFull.y - 1;
                Debug.LogWarning ($"Using fallback value for damage penetration depth as it was loaded at 0: {damagePenetrationDepth}");
            }

            UpdateVolume (true);

            if (points.Count != data.points.Length)
            {
                Debug.LogWarning ($"Failed to load level {key}: point count based on bounds {core.bounds} is {points.Count}, but loaded data has {data.points.Length} points");
                return;
            }

            // Base data

            for (int i = 0; i < points.Count; ++i)
            {
                AreaVolumePoint point = points[i];
                point.pointState = data.points[i] == true ? AreaVolumePointState.Full : AreaVolumePointState.Empty;

                if (point.pointState == AreaVolumePointState.FullDestroyed)
                {
                    point.integrity = 0;
                    point.integrityForDestructionAnimation = 0f;
                }
            }

            UpdateAllSpots (false);

            HashSet<int> detectedMissingTilesets = new HashSet<int> ();
            for (int i = 0; i < data.spots.Count; ++i)
            {
                AreaDataSpot spotLoaded = data.spots[i];
                AreaVolumePoint point = points[spotLoaded.index];

                point.blockTileset = spotLoaded.tileset;
                point.blockGroup = spotLoaded.group;
                point.blockSubtype = spotLoaded.subtype;
                point.blockRotation = spotLoaded.rotation;
                point.blockFlippedHorizontally = spotLoaded.flip;

                //  0     1       2   3   4      5   6   (byte)
                // -3    -2      -1   0   1      2   3   (shifted int)
                // -1   -0.66  -0.33  0  0.33  0.66  1   (final float)
                // point.terrainOffset = ((int)spotLoaded.offset - 3) / 3f;

                // 253 (byte) -> (253 - 256) = -3 (int)
                // 254 (byte) -> (254 - 256) = -2 (int)
                // 255 (byte) -> (255 - 256) = -1 (int)
                // 0   (byte) ->                0 (int)
                // 1   (byte) ->                1 (int)
                // 2   (byte) ->                2 (int)
                // 2   (byte) ->                3 (int)

                byte offsetByte = spotLoaded.offset;
                int offsetInt = (int) offsetByte;
                if (offsetByte >= terrainOffsetByteThreshold)
                    offsetInt = offsetInt - 256;
                point.terrainOffset = (offsetInt / 3f);

                if (!AreaTilesetHelper.database.tilesets.ContainsKey (point.blockTileset))
                {
                    #if PB_MODSDK && UNITY_EDITOR
                    if (ignoreUnresolvedTilesetOnLoad)
                    {
                        continue;
                    }
                    #endif

                    Debug.Log (string.Format ("AM | LoadArea | Resetting tileset, group and subtype on point {0} due to missing tileset {1}", point.spotIndex, point.blockTileset));
                    #if UNITY_EDITOR
                    DrawHighlightSpot (point);
                    #endif

                    point.blockTileset = AreaTilesetHelper.database.tilesetFallback.id;
                    point.blockFlippedHorizontally = false;
                    point.blockGroup = 0;
                    point.blockSubtype = 0;

                    detectedMissingTilesets.Add (point.blockTileset);
                }
                else // if (point.spotConfiguration != AreaNavUtility.configEmpty && point.spotConfiguration != AreaNavUtility.configFull)
                    ValidateBlockSubtypes (point, AreaTilesetHelper.database.tilesets[point.blockTileset].blocks[point.spotConfiguration], AreaTilesetHelper.database.tilesets[point.blockTileset]);
            }

            bool customizationLoading = !Application.isPlaying || !DataShortcuts.debug.combatColorSkipped;
            if (customizationLoading)
            {
                for (int i = 0; i < data.customizations.Count; ++i)
                {
                    AreaDataCustomization customizationLoaded = data.customizations[i];
                    AreaVolumePoint point = points[customizationLoaded.index];

                    point.customization = new TilesetVertexProperties
                    (
                        customizationLoaded.h1,
                        customizationLoaded.s1,
                        customizationLoaded.b1,
                        customizationLoaded.h2,
                        customizationLoaded.s2,
                        customizationLoaded.b2,
                        customizationLoaded.emission,
                        0f
                    );
                }
            }
            else
            {
                for (int i = 0; i < points.Count; ++i)
                {
                    AreaVolumePoint point = points[i];
                    point.customization.saturationPrimary = 0;
                    point.customization.saturationSecondary = 0;
                }
            }

            if (data.indestructibleIndexes != null)
            {
                int pointsCount = points.Count;
                for (int i = 0; i < data.indestructibleIndexes.Count; ++i)
                {
                    var index = data.indestructibleIndexes[i];
                    if (index >= 0 && index < pointsCount)
                    {
                        var point = points[index];
                        if (point != null)
                            point.destructible = false;
                    }
                }
            }

            /*
            if (data.untrackedIndexes != null)
            {
                int pointsCount = points.Count;
                for (int i = 0; i < data.untrackedIndexes.Count; ++i)
                {
                    var index = data.untrackedIndexes[i];
                    if (index >= 0 && index < pointsCount)
                    {
                        var point = points[index];
                        if (point != null)
                            point.destructionUntracked = true;
                    }
                }
            }
            */

            // Debug.Log ("Found integrity data for " + description.pointIntegrities.Count + " points");
            for (int i = 0; i < data.integrities.Count; ++i)
            {
                AreaDataIntegrity integrityLoaded = data.integrities[i];
                AreaVolumePoint point = points[integrityLoaded.index];

                if (point.pointState == AreaVolumePointState.Empty)
                {
                    Debug.LogWarning ("Area data somehow contains damage data for empty point " + point.spotIndex + " | Recheck serialization implementation");
                    continue;
                }

                point.integrity = integrityLoaded.integrity;

                if (point.destructible && point.integrity <= 0f)
                {
                    point.integrityForDestructionAnimation = 0f;
                    point.pointState = AreaVolumePointState.FullDestroyed;
                }
            }

            bool propsLoading = !Application.isPlaying || !DataShortcuts.debug.combatPropsSkipped;
            if (propsLoading)
            {
                int detectedMissingProps = 0;
                placementsProps = new List<AreaPlacementProp> (data.props.Count);
                for (int i = 0; i < data.props.Count; ++i)
                {
                    AreaDataProp placementLoaded = data.props[i];
                    AreaPropPrototypeData prototype = AreaAssetHelper.GetPropPrototype (placementLoaded.id);

                    if (prototype == null)
                        ++detectedMissingProps;

                    if
                    (
                        placementLoaded.pivotIndex.IsValidIndex (points) &&
                        prototype != null
                    )
                    {
                        AreaPlacementProp placement = new AreaPlacementProp ();
                        placementsProps.Add (placement);

                        placement.id = placementLoaded.id;

                        placement.pivotIndex = placementLoaded.pivotIndex;
                        placement.rotation = placementLoaded.rotation;
                        placement.flipped = placementLoaded.flip;
                        placement.offsetX = placementLoaded.offsetX;
                        placement.offsetZ = placementLoaded.offsetZ;

                        if (customizationLoading)
                        {
                            placement.hsbPrimary = new Vector4 (placementLoaded.h1, placementLoaded.s1, placementLoaded.b1, 0f);
                            placement.hsbSecondary = new Vector4 (placementLoaded.h2, placementLoaded.s2, placementLoaded.b2, 0f);
                        }

                        placement.state = new AreaPropState ();
                        placement.destroyed = placementLoaded.status != 0;
                        placement.destructionTime = -100f;
                        if (placement.destroyed)
                            placementsPropsDestroyed.Add (placement);
                    }
                    else
                        Debug.LogWarning ("AM | LoadArea | Skipping prop placement " + i + " with ID " + placementLoaded.id + " | Prototype is " + prototype.ToStringNullCheck ());
                }

                if (detectedMissingProps > 0)
                    Debug.LogWarning ("AM | LoadArea | Found " + detectedMissingProps + " missing prop IDs");
            }

            navOverrides = new Dictionary<int, AreaDataNavOverride> (navOverrides.Count);
            if (data.navOverrides != null)
            {
                for (int i = 0; i < data.navOverrides.Count; ++i)
                {
                    var navOverrideLoaded = data.navOverrides[i];
                    if (navOverrides.ContainsKey (navOverrideLoaded.pivotIndex))
                        continue;

                    var navOverride = new AreaDataNavOverride
                    {
                        pivotIndex = navOverrideLoaded.pivotIndex,
                        offsetY = navOverrideLoaded.offsetY
                    };
                    navOverrides.Add (navOverrideLoaded.pivotIndex, navOverride);
                }
            }

            // Marking roads: this is not guaranteed to be correct and shouldn't be used in gameplay, but is useful for editing
            for (int i = 0; i < points.Count; ++i)
            {
                var point = points[i];
                if (point.pointState == AreaVolumePointState.Empty)
                    continue;

                bool clear = true;
                for (int a = 0; a < 4; ++a)
                {
                    var pointAbove = point.pointsWithSurroundingSpots[a];
                    if
                    (
                        pointAbove == null ||
                        pointAbove.spotConfiguration != AreaNavUtility.configFloor ||
                        pointAbove.blockTileset != AreaTilesetHelper.idOfRoad
                    )
                    {
                        clear = false;
                        break;
                    }

                    if
                    (
                        pointAbove.blockGroup == 1 ||

                        (pointAbove.blockGroup > 50 && pointAbove.blockGroup < 100)
                    )
                    {
                        clear = false;
                        break;
                    }
                }

                if (clear)
                    point.road = true;

                if (resetOffsetOnLoad)
                    point.terrainOffset = 0f;
            }

            // Allocating new reused collection for damage algo
            structurePointsQueue = new Queue<AreaVolumePoint> (points.Count);

            SetupPointEntities ();
            RebuildEverything ();
            UpdateBoundsInSystems ();

            // Refresh indirect damage flags
            for (int i = 0; i < points.Count; ++i)
            {
                var point = points[i];

                // Skip empty volume
                if (point.pointState == AreaVolumePointState.Empty)
                    continue;

                // Set flag based on height, edges and adjacency to indestructible tilesets
                point.indestructibleIndirect = IsPointIndestructible
                (
                    point,
                    false,
                    true,
                    true,
                    true,
                    true
                );
            }

            if (Application.isPlaying)
            {
                Debug.Log ($"Finished loading combat area {key}");
                ApplyShaderPropertiesEverywhere ();
            }

            #if PB_MODSDK && UNITY_EDITOR
            var combatArea = DataMultiLinkerCombatArea.data[key];
            if (combatArea != null)
            {
                combatArea.errorsCorrectedOnLoad = !ignoreUnresolvedTilesetOnLoad;
            }
            #endif

            OnAfterAreaLoaded ();
        }

        private void OnAfterAreaLoaded ()
        {
        }

        public float boundsBoostForCamera = 20f;
        public float boundsBoostForGrid = -1f;
        public float boundsBoostForCursor = -1f;

        [ContextMenu ("Update bounds in systems")]
        public void UpdateBoundsInSystems ()
        {
            Bounds boundsForCamera = new Bounds (GetBoundsCenter (), GetBoundsExtents (boundsBoostForCamera));
            Bounds boundsForGrid = new Bounds (GetBoundsCenter (), GetBoundsExtents (boundsBoostForGrid));
            Bounds boundsForCursor = new Bounds (GetBoundsCenter (), GetBoundsExtents (boundsBoostForCursor));

            // Debug.Log ("AM | UpdateBoundsInSystems | Camera: " + boundsForCamera + " | Grid: " + boundsForGrid + " | Cursor: " + boundsForCursor);
        }

        private Vector3 GetBoundsCenter ()
        {
            return new Vector3 ((float)(boundsFull.x - 1) / 2f, -(float)(boundsFull.y - 1) / 2f, (float)(boundsFull.z - 1) / 2f) * TilesetUtility.blockAssetSize;
        }

        private Vector3 GetPositionForBackground ()
        {
            return new Vector3 ((float)(boundsFull.x - 1) / 2f, -boundsFull.y, (float)(boundsFull.z - 1) / 2f) * TilesetUtility.blockAssetSize;
        }

        public Vector3 GetBoundsExtents (float offset)
        {
            // Debug.Log ("AM | GetBoundsExtents | Offset: " + offset + " | X: " + boundsFull.x + " | After offset: " + (boundsFull.x + offset * 2) + " | After sizing: " + (Mathf.Max (4, boundsFull.x + offset * 2) * TilesetUtility.blockAssetSize));
            Vector3 result = new Vector3 (boundsFull.x + offset * 2, boundsFull.y + offset * 2, boundsFull.z + offset * 2) * TilesetUtility.blockAssetSize;
            result = new Vector3 (Mathf.Max (3f, result.x), Mathf.Max (3f, result.y), Mathf.Max (3f, result.z));
            return result;
        }

        private List<int> GetIndexesToCheck (AreaProp prefab, int rotation, bool flipped, Vector3Int pivotPosition)
        {
            List<int> indexesToCheck = new List<int> (prefab.pointsToCheck.Count);
            for (int i = 0; i < prefab.pointsToCheck.Count; ++i)
            {
                Vector3Int offset = prefab.pointsToCheck[i];
                Vector3Int offsetRotated = offset.RotateByIndex (rotation);
                Vector3Int offsetFlipped = flipped ? (prefab.mirrorOnZAxis ? offsetRotated.FlipOnZ () : offsetRotated.FlipOnX ()) : offsetRotated;
                Vector3Int internalPosition = pivotPosition + offsetFlipped;

                int indexToCheck = AreaUtility.GetIndexFromVolumePosition (internalPosition, boundsFull);
                indexesToCheck.Add (indexToCheck);
            }

            return indexesToCheck;
        }

        public void UpdateReflections ()
        {
            if (!bakeReflections)
                return;

            for (int i = 0; i < reflectionProbes.Count; ++i)
            {
                // Debug.Log ("AM | Refreshing probe " + i);
                ReflectionProbe probe = reflectionProbes[i];
                probe.RenderProbe ();
            }
        }

        public void UnloadArea (bool forDestruction)
        {
            ResetEditingModes ();
            UnloadEntities (forDestruction);

            ++areaChangeTracker;

            points = new List<AreaVolumePoint> ();
            placementsProps = new List<AreaPlacementProp> ();
            navOverrides = new Dictionary<int, AreaDataNavOverride> ();
            pointsToAnimate.Clear ();

            UtilityGameObjects.ClearChildren (GetHolderColliders ());

            if (Application.isPlaying)
            {
                simulatedHelpers.Clear();
                UtilityGameObjects.ClearChildren (GetHolderSimulatedParent ());
                UtilityGameObjects.ClearChildren (GetHolderSimulatedLeftovers ());
            }

            #if UNITY_EDITOR
            Debug.Log ("Unloaded combat area " + areaName);
            areaName = "";
            #endif
        }

        private void UnloadEntities (bool forDestruction)
        {
            if (IsECSSafe () && !forDestruction)
            {
                var entityManager = world.EntityManager;

                if (pointEntitiesMain != null)
                {
                    for (int i = 0; i < pointEntitiesMain.Length; ++i)
                    {
                        var entity = pointEntitiesMain[i];
                        if (entity != Entity.Null && entityManager.ExistsNonNull (entity))
                            entityManager.DestroyEntity (entity);
                    }
                }

                if (pointEntitiesInterior != null)
                {
                    for (int i = 0; i < pointEntitiesInterior.Length; ++i)
                    {
                        var entity = pointEntitiesInterior[i];
                        if (entity != Entity.Null && entityManager.ExistsNonNull (entity))
                            entityManager.DestroyEntity (entity);
                    }
                }

                if (entityManager.ExistsNonNull (pointTestEntity))
                    entityManager.DestroyEntity (pointTestEntity);
                pointTestEntity = Entity.Null;

                for (int i = 0; i < placementsProps.Count; ++i)
                    placementsProps[i].Cleanup ();

                UtilityECS.ScheduleUpdate ();
            }
            else
            {
                pointEntitiesMain = null;
                pointEntitiesInterior = null;
                pointTestEntity = Entity.Null;
            }
        }


        [ContextMenu ("Clear cache")]
        public void ClearCache ()
        {
            AreaUtility.ClearCache ();
        }

        [ContextMenu ("Check point use")]
        public void CheckPointUse ()
        {
            int pointsEmpty = 0;
            int pointsEmptyWithSpotEmpty = 0;
            int pointsFull = 0;
            int pointsFullWithSpotFull = 0;

            for (int i = 0; i < points.Count; ++i)
            {
                AreaVolumePoint point = points[i];
                if (point.pointState == AreaVolumePointState.Empty)
                {
                    ++pointsEmpty;
                    if (point.spotConfiguration == (byte)0)
                        ++pointsEmptyWithSpotEmpty;
                }
                else
                {
                    ++pointsFull;
                    if (point.spotConfiguration == (byte)255)
                        ++pointsFullWithSpotFull;
                }
            }

            Debug.Log ("AM | CheckPointUse | Empty points: " + pointsEmpty + " (empty spots on " + pointsEmptyWithSpotEmpty + ") | Full points: " + pointsFull + " (full spots on " + pointsFullWithSpotFull + ")");
        }

        public void RemapLevel (Vector3Int boundsFullNew)
        {
            if (points == null || points.Count == 0)
                return;

            // Since a volume below 2x2x2 won't contain any Spots, we have to limit the minimum size
            boundsFullNew = new Vector3Int ((int)Mathf.Max (boundsFullNew.x, 2), (int)Mathf.Max (boundsFullNew.y, 2), (int)Mathf.Max (boundsFullNew.z, 2));
            int volumeLengthNew = boundsFullNew.x * boundsFullNew.y * boundsFullNew.z;

            if (boundsFullNew.x == boundsFull.x && boundsFullNew.y == boundsFull.y && boundsFullNew.z == boundsFull.z)
                return;

            // Proper remapping is broken, crude rebuilding is done here instead

            UnloadArea (false);

            boundsFull = boundsFullNew;
            UpdateVolume (true);
            UpdateAllSpots (false);

            structurePointsQueue = new Queue<AreaVolumePoint> (points.Count);

            SetupPointEntities ();
            RebuildEverything ();
            UpdateBoundsInSystems ();
            ShaderGlobalHelper.SetOcclusionShift (boundsFull.y * 3f + 4.5f);


            /*
            UnloadEntities (false);
            Debug.LogWarning ("AM | RemapArea | Starting new volume mapping with new bounds " + boundsFullNew);
            List<AreaVolumePoint> pointsNew = new List<AreaVolumePoint> (new AreaVolumePoint[volumeLengthNew]);
            for (int i = 0; i < pointsNew.Count; ++i)
            {
                AreaVolumePoint pointNew = new AreaVolumePoint ();
                pointsNew[i] = pointNew;

                pointNew.pointPositionIndex = TilesetUtility.GetVolumePositionFromIndex (i, boundsFullNew);
                pointNew.pointPositionLocal = AreaUtility.GetLocalPositionFromGridPosition (pointNew.pointPositionIndex);
                pointNew.pointsInSpot = new AreaVolumePoint[8];
                pointNew.pointsInSpot[0] = pointNew;
                pointNew.pointsWithSurroundingSpots = new AreaVolumePoint[8];
                pointNew.pointsWithSurroundingSpots[7] = pointNew;

                if (pointNew.pointPositionIndex.y >= boundsFullNew.y - 2) pointNew.pointState = AreaVolumePointState.Full;
                else pointNew.pointState = AreaVolumePointState.Empty;
            }

            for (int i = 0; i < points.Count; ++i)
            {
                AreaVolumePoint pointOld = points[i];
                Vector3Int positionOld = pointOld.pointPositionIndex;
                for (int a = 0; a < pointsNew.Count; ++a)
                {
                    AreaVolumePoint pointNew = pointsNew[a];
                    Vector3Int positionNew = pointNew.pointPositionIndex;
                    if (positionNew == positionOld)
                    {
                        pointsNew[a] = pointOld;
                        pointOld.pointsInSpot = new AreaVolumePoint[8];
                        pointOld.pointsInSpot[0] = pointNew;
                        pointOld.pointsWithSurroundingSpots = new AreaVolumePoint[8];
                        pointOld.pointsWithSurroundingSpots[7] = pointNew;
                    }
                }
            }

            r_IntArray_UpdateVolume = new int[8];
            for (int i = 0; i < pointsNew.Count; ++i)
            {
                AreaVolumePoint pointNew = pointsNew[i];

                Array.Copy (AreaUtility.GetNeighbourIndexesInXxYxZ (i, Vector3Int.size2x2x2, Vector3Int.size0x0x0, boundsFullNew), r_IntArray_UpdateVolume, 8);
                for (int n = 1; n < 8; ++n)
                {
                    int neighbourIndex = r_IntArray_UpdateVolume[n];
                    if (neighbourIndex != -1)
                    {
                        if (!neighbourIndex.IsValidIndex (pointsNew))
                        {
                            Debug.LogWarning ("AM | RemapArea | Encountered invalid neighbour index for point " + i + " with coords " + pointNew.pointPositionIndex + ": " + neighbourIndex);
                            return;
                        }

                        AreaVolumePoint pointToAdd = pointsNew[neighbourIndex];
                        pointNew.pointsInSpot[n] = pointToAdd;
                    }
                    else
                    {
                        pointNew.spotPresent = false;
                        pointNew.pointsInSpot[n] = null;
                    }
                }

                Array.Copy (AreaUtility.GetNeighbourIndexesInXxYxZ (i, Vector3Int.size2x2x2, Vector3Int.size1x1x1Neg, boundsFullNew), r_IntArray_UpdateVolume, 8);
                for (int n = 0; n < 7; ++n)
                {
                    int neighbourIndex = r_IntArray_UpdateVolume[n];
                    pointNew.pointsWithSurroundingSpots[n] = neighbourIndex != -1 ? pointsNew[neighbourIndex] : null;
                }
            }

            points = pointsNew;
            boundsFull = boundsFullNew;

            UpdateAllSpots (false);
            SetupPointEntities ();
            RebuildEverything ();
            */
        }




        public void SetupPointEntities ()
        {
            if (!IsECSSafe ())
                return;

            int pointCount = points.Count;
            var entityManager = world.EntityManager;

            if (pointEntitiesMain != null)
            {
                for (int i = 0; i < pointEntitiesMain.Length; ++i)
                {
                    var entity = pointEntitiesMain[i];
                    if (entity != Entity.Null)
                    {
                        pointEntitiesMain[i] = Entity.Null;
                        if (entityManager.ExistsNonNull (entity))
                            entityManager.DestroyEntity (entity);
                    }
                }
            }

            if ((pointEntitiesMain != null && pointEntitiesMain.Length != pointCount) || pointEntitiesMain == null)
            {
                pointEntitiesMain = new Entity[pointCount];
                for (int i = 0; i < pointCount; ++i)
                    pointEntitiesMain[i] = Entity.Null;
            }

            if (pointEntitiesInterior != null)
            {
                for (int i = 0; i < pointEntitiesInterior.Length; ++i)
                {
                    var entity = pointEntitiesInterior[i];
                    if (entity != Entity.Null)
                    {
                        pointEntitiesInterior[i] = Entity.Null;
                        if (entityManager.ExistsNonNull (entity))
                            entityManager.DestroyEntity (entity);
                    }
                }
            }

            if ((pointEntitiesInterior != null && pointEntitiesInterior.Length != pointCount) || pointEntitiesInterior == null)
            {
                pointEntitiesInterior = new Entity[pointCount];
                for (int i = 0; i < pointCount; ++i)
                    pointEntitiesInterior[i] = Entity.Null;
            }

            for (int i = 0; i < pointCount; ++i)
            {
                AreaVolumePoint point = points[i];
                if (!point.spotPresent || point.spotConfiguration == AreaNavUtility.configEmpty)
                    continue;

                pointEntitiesMain[i] = CreatePointExteriorEntity (entityManager, pointMainArchetype, point.instancePosition, quaternion.identity, i);
                pointEntitiesInterior[i] = CreatePointInteriorEntity (entityManager, pointInteriorArchetype, point.instancePosition, quaternion.identity, i);
            }

            UtilityECS.ScheduleUpdate ();
        }

        private Entity CreatePointInteriorEntity (EntityManager entityManager, EntityArchetype archetype, Vector3 positionDefault, Quaternion rotationDefault, int index)
        {
            Entity entity = entityManager.CreateEntity (archetype);

            entityManager.SetComponentData (entity, new ECS.BlockID { id = index });
            entityManager.SetComponentData (entity, new Translation { Value = positionDefault });
            entityManager.SetComponentData (entity, new Rotation { Value = rotationDefault });

            entityManager.SetComponentData (entity, new ScaleShaderProperty { property = new HalfVector4 (1, 1, 1, 1) });
            entityManager.SetComponentData (entity, new HSBOffsetProperty { property = new HalfVector8(new HalfVector4(0f, 0f, 0f, 0f), new HalfVector4(0f, 0f, 0f, 0f))});

            entityManager.SetComponentData (entity, new PropertyVersion {version = 1});

            MarkEntityDirty (entity);

            UtilityECS.ScheduleUpdate ();

            return entity;
        }

        private Entity CreatePointExteriorEntity (EntityManager entityManager, EntityArchetype archetype, Vector3 positionDefault, Quaternion rotationDefault, int index)
        {
            Entity entity = entityManager.CreateEntity (archetype);

            entityManager.SetComponentData (entity, new ECS.BlockID { id = index });
            entityManager.SetComponentData (entity, new Translation { Value = positionDefault });
            entityManager.SetComponentData (entity, new Rotation { Value = rotationDefault });

            entityManager.SetComponentData (entity, new ScaleShaderProperty { property = new HalfVector4 (1, 1, 1, 1) });
            entityManager.SetComponentData (entity, new HSBOffsetProperty { property = new HalfVector8(new HalfVector4(0f, 0f, 0f, 0f), new HalfVector4(0f, 0f, 0f, 0f))});

            entityManager.SetComponentData(entity, new DamageProperty { property = new HalfVector8(new HalfVector4(0f, 0f, 0f, 0f), new HalfVector4(0f, 0f, 0f, 0f))});
            entityManager.SetComponentData(entity, new IntegrityProperty { property = new FixedVector8(new FixedVector4(0f, 0f, 0f, 0f), new FixedVector4(0f, 0f, 0f, 0f))});

            entityManager.SetComponentData (entity, new PropertyVersion {version = 1});

            MarkEntityDirty (entity);

            UtilityECS.ScheduleUpdate ();

            return entity;
        }

        public AreaPlacementProp GetPropFromColliderID (int colliderID)
        {
            if (propLookupByColliderID.ContainsKey (colliderID))
            {
                // Debug.Log ($"Located a prop collider for prop {propLookupByColliderID[colliderID].prototype.name} using ID {colliderID}");
                return propLookupByColliderID[colliderID];
            }
            else
            {
                // Debug.LogWarning ($"Failed to find a prop collider using ID {colliderID}");
                return null;
            }
        }

        #region Editor
        #if UNITY_EDITOR
        public readonly AreaClipboard clipboard =  new AreaClipboard();

        public Vector3Int clipboardOrigin;
        public Vector3Int clipboardBoundsRequested;
        public Vector3Int targetOrigin;

        public bool transferVolume = true;
        public bool transferProps;
        public bool combineExports;

        [NonSerialized]
        public bool debugPasteDrawHighlights;

        public void ExportVolume (Vector3Int cornerA, Vector3Int cornerB)
        {
            int indexA = AreaUtility.GetIndexFromVolumePosition (cornerA, boundsFull);
            int indexB = AreaUtility.GetIndexFromVolumePosition (cornerB, boundsFull);

            if (indexA == -1 || indexB == -1)
            {
                Debug.LogWarning (string.Format
                (
                    "CopyVolume | Failed to export due to specified corners {0} and {1} falling outside of source level bounds {2}",
                    cornerA,
                    cornerB,
                    boundsFull
                ));
                return;
            }

            // Bounds are limits, like array sizes - so they will be bigger by 1 than just pure position difference
            // For example, a set of points [(0,0);(1,0);(0,1);(1;1)] has bounds of 2x2, not 1x1!
            int x = cornerB.x - cornerA.x + 1;
            int y = cornerB.y - cornerA.y + 1;
            int z = cornerB.z - cornerA.z + 1;

            if (x < 2 || y < 2 || z < 2)
            {
                Debug.LogWarning (string.Format
                (
                    "CopyVolume | Failed to export due to specified corners not creating valid clip bounds | B - A: {0} - {1} = {2}",
                    cornerB,
                    cornerA,
                    (cornerB - cornerA)
                ));
                return;
            }

            int volumeLength = x * y * z;
            var holder = new GameObject ("AreaExport_" + areaName);
            var scale = new Vector3 (3.05f, 3f, 3.05f);
            var objectsAdded = new List<GameObject> ();

            for (int i = 0; i < volumeLength; ++i)
            {
                Vector3Int clipboardPointPosition = AreaUtility.GetVolumePositionFromIndex (i, clipboardBoundsRequested);
                Vector3Int sourcePointPosition = clipboardPointPosition + cornerA;
                int pointIndex = AreaUtility.GetIndexFromVolumePosition (sourcePointPosition, boundsFull);
                var point = points[pointIndex];

                /*
                if (point.pointState != AreaVolumePointState.Full)
                    continue;

                var block = PrimitiveHelper.CreatePrimitive (PrimitiveType.Cube, false);
                block.transform.parent = holder.transform;
                block.transform.localScale = scale;
                block.transform.localPosition = point.pointPositionLocal;

                var mr = block.GetComponent<MeshRenderer> ();
                mr.sharedMaterial = UtilityMaterial.GetDefaultMaterial ();
                objectsAdded.Add (block);
                */

                if (point.spotConfiguration == AreaNavUtility.configEmpty || point.spotConfiguration == AreaNavUtility.configFull)
                    continue;

                var tilesetMain =
                    AreaTilesetHelper.database.tilesets.ContainsKey (point.blockTileset) ?
                    AreaTilesetHelper.database.tilesets[point.blockTileset] :
                    AreaTilesetHelper.database.tilesetFallback;

                if (tilesetMain == null)
                    continue;

                var configurationData = AreaTilesetHelper.database.configurationDataForBlocks[point.spotConfiguration];
                var definition = tilesetMain.blocks[point.spotConfiguration];

                var model = AreaTilesetHelper.GetInstancedModel
                (
                    tilesetMain.id,
                    AreaTilesetHelper.assetFamilyBlock,
                    point.spotConfiguration,
                    point.blockGroup,
                    point.blockSubtype,
                    true,
                    true,
                    out bool invalidVariantDetected,
                    out bool verticalFlip,
                    out var lightData
                );

                var block = new GameObject ();
                block.transform.parent = holder.transform;
                block.transform.localPosition = point.instancePosition;
                block.transform.localScale = new Vector3 (point.instanceMainScaleAndSpin.x, (verticalFlip ? -1f : 1f) * point.instanceMainScaleAndSpin.y, point.instanceMainScaleAndSpin.z);
                block.transform.localRotation = point.instanceMainRotation;
                objectsAdded.Add (block);

                var mf = block.AddComponent<MeshFilter> ();
                mf.sharedMesh = model.mesh;

                var mr = block.AddComponent<MeshRenderer> ();

                #if UNITY_EDITOR
                    mr.sharedMaterial = UtilityMaterial.GetDefaultMaterial ();
                #endif
            }
        }

        public void DrawHighlightSpot (AreaVolumePoint point)
        {
            DrawHighlightBox (point.instancePosition, Color.red, 15f);
        }

        public void DrawHighlightBox (Vector3 pos, Color col, float duration, float size = 1.5f)
        {
            var ct1 = pos + new Vector3 (size, size, size);
            var ct2 = pos + new Vector3 (-size, size, size);
            var ct3 = pos + new Vector3 (-size, size, -size);
            var ct4 = pos + new Vector3 (size, size, -size);

            var cb1 = pos + new Vector3 (size, -size, size);
            var cb2 = pos + new Vector3 (-size, -size, size);
            var cb3 = pos + new Vector3 (-size, -size, -size);
            var cb4 = pos + new Vector3 (size, -size, -size);

            Debug.DrawLine (ct1, ct2, col, duration);
            Debug.DrawLine (ct2, ct3, col, duration);
            Debug.DrawLine (ct3, ct4, col, duration);
            Debug.DrawLine (ct4, ct1, col, duration);

            Debug.DrawLine (cb1, cb2, col, duration);
            Debug.DrawLine (cb2, cb3, col, duration);
            Debug.DrawLine (cb3, cb4, col, duration);
            Debug.DrawLine (cb4, cb1, col, duration);

            Debug.DrawLine (ct1, cb1, col, duration);
            Debug.DrawLine (ct2, cb2, col, duration);
            Debug.DrawLine (ct3, cb3, col, duration);
            Debug.DrawLine (ct4, cb4, col, duration);
        }

        [ContextMenu ("Flip forest tiles to terrain")]
        public void SetTerrainFlagOn () => SetTerrainFlag (true);

        [ContextMenu ("Flip terrain tiles to forest")]
        public void SetTerrainFlagOff () => SetTerrainFlag (false);

        public void SetTerrainFlag (bool terrainUsed)
        {
            for (int i = 0; i < points.Count; ++i)
            {
                var point = points[i];
                if (!point.spotPresent)
                    continue;

                var blockTileset = point.blockTileset;
                if (terrainUsed && blockTileset == AreaTilesetHelper.idOfForest)
                    point.blockTileset = AreaTilesetHelper.idOfTerrain;
                else if (!terrainUsed && blockTileset == AreaTilesetHelper.idOfTerrain)
                    point.blockTileset = AreaTilesetHelper.idOfForest;
            }

            RebuildEverything ();
        }

        public Texture3D texture3DAO;

        [ContextMenu ("Generate 3D Texture")][Button]
        public void Generate3DTexture ()
        {
            var sizeX = boundsFull.x;
            var sizeY = boundsFull.y;
            var sizeZ = boundsFull.z;

            Color[] colorArray = new Color[sizeX * sizeY * sizeZ];
            texture3DAO = new Texture3D (boundsFull.x, sizeY, sizeZ, TextureFormat.RGBA32, false);

            float r = 1.0f / (sizeX - 1.0f);
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    for (int z = 0; z < sizeZ; z++)
                    {
                        var index = AreaUtility.GetIndexFromVolumePosition (new Vector3Int (x, sizeY - 1 - y, z), boundsFull);
                        var point = points[index];
                        var color = point != null && point.pointState == AreaVolumePointState.Full ? Color.black.WithAlpha (0f) : Color.white.WithAlpha (1f);
                        colorArray[x + (y * sizeX) + (z * sizeX * sizeY)] = color;

                        // Color c = new Color (x * r, y * r, z * r, 1.0f);
                        // colorArray[x + (y * sizeX) + (z * sizeX * sizeY)] = c;
                    }
                }
            }

            texture3DAO.SetPixels (colorArray);
            texture3DAO.Apply ();
            texture3DAO.wrapMode = TextureWrapMode.Clamp;

            // Shader.SetGlobalTexture ("_GlobalEnvironmentAOTex", texture3DAO);
        }

        private int[,] heightfield;
        private Texture2D textureDepthmap;
        private Texture2D textureMaskVegetation;

        public struct DepthColorLink
        {
            public Color color;
            public int depth;
            public int depthScaled;
        }

        [NonSerialized]
        public readonly List<DepthColorLink> heightfieldPalette = new List<DepthColorLink> ();
        readonly HashSet<int> colorValues = new HashSet<int> ();
        Color32[] colorArray;
        int maxDepthScaled;

        public const string standardHeightmapFileName = "heightmap.png";
        const byte minDepthScaled = 10;
        const float terrainOffsetTopRamp = -1f / 3f;

        public class HeightmapSpec
        {
            public int SizeX;
            public int SizeZ;
            public int MaxDepth;
            public int[,] Heightfield;
            public List<AreaVolumePoint> Points;
            public Vector3Int Bounds;
            public StoreHeightmapInfo Store;
        }

        public delegate void StoreHeightmapInfo (int x, int z, int depthProcessed, byte slopeInfo, byte roadInfo);
        public delegate void ProcessDepthValues (HeightmapSpec spec);

        #if !PB_MODSDK
        [ContextMenu ("Save depth to texture (R)")][Button]
        public void ExportToHeightmap ()
        {
            var folderPath = DataMultiLinkerCombatArea.GetCurrentUnpackedLevelPath ();
            if (folderPath == null)
            {
                Debug.Log ("Failed to save heightmap, couldn't get current unpacked level path");
                return;
            }

            var filePath = DataPathHelper.GetCombinedCleanPath (folderPath, standardHeightmapFileName);
            CreateHeightmap (CalculateStandardHeightmapValues, filePath);
        }
        #endif

        public void CalculateStandardHeightmapValues (HeightmapSpec spec)
        {
            for (var x = 0; x < spec.SizeX; x += 1)
            {
                for (var z = 0; z < spec.SizeZ; z += 1)
                {
                    var depth = heightfield[x, z];
                    var sizeY = boundsFull.y;
                    var slopeInfo = byte.MinValue;
                    var roadInfo = byte.MinValue;

                    // Try to hit a surface point
                    var posIndex = new Vector3Int (x, 0, z);
                    var pointIndex = AreaUtility.GetIndexFromVolumePosition (posIndex, boundsFull, skipBoundsCheck: true);
                    var pointCurrent = points[pointIndex];
                    for (var iteration = 0; iteration <= sizeY; iteration += 1)
                    {
                        if (pointCurrent == null)
                        {
                            break;
                        }

                        var pointStateCurrent = pointCurrent.pointState;
                        if (pointStateCurrent == AreaVolumePointState.Full)
                        {
                            var pointAboveStartEmpty = pointCurrent.pointsWithSurroundingSpots[3];
                            if (pointAboveStartEmpty == null)
                            {
                                continue;
                            }
                            if (pointAboveStartEmpty.terrainOffset.RoughlyEqual (terrainOffsetTopRamp))
                            {
                                slopeInfo = byte.MaxValue;
                            }
                            if (pointCurrent.road)
                            {
                                roadInfo = byte.MaxValue;
                            }
                            break;
                        }

                        // Get point below
                        pointCurrent = pointCurrent.pointsInSpot[4];
                    }
                    spec.Store (x, z, depth, slopeInfo, roadInfo);
                }
            }
        }

        public void CreateHeightmap(ProcessDepthValues processDepthValues, string filePath)
        {
            var sizeX = boundsFull.x;
            var sizeZ = boundsFull.z;
            if (heightfield == null || (heightfield.GetLength (0) != sizeX && heightfield.GetLength (1) != sizeZ))
            {
                heightfield = new int[sizeX, sizeZ];
            }

            ProceduralMeshUtilities.CollectSurfacePoints (this, heightfield);

            heightfieldPalette.Clear ();
            colorValues.Clear ();

            colorArray = new Color32[heightfield.Length];

            var sizeY = boundsFull.y;
            var maxDepth = Mathf.Min (sizeY - 1, byte.MaxValue);
            maxDepthScaled = maxDepth * 10;

            // Points on north (sizeZ - 1) and east (sizeX - 1) borders don't have spots so exclude them.
            var spec = new HeightmapSpec ()
            {
                SizeX = sizeX - 1,
                SizeZ = sizeZ - 1,
                MaxDepth = maxDepth,
                Heightfield = heightfield,
                Points = points,
                Bounds = boundsFull,
                Store = StoreHeightmapInfoInternal,
            };
            processDepthValues (spec);
            heightfieldPalette.Sort ((x, y) => x.depth.CompareTo (y.depth));

            var zMax = sizeZ - 1;
            for (var x = 0; x < sizeX; x += 1)
            {
                var index = zMax * boundsFull.z + x;
                colorArray[index] = Color.black;
            }
            var xMax = sizeX - 1;
            for (var z = 0; z < sizeZ; z += 1)
            {
                var index = z * boundsFull.z + xMax;
                colorArray[index] = Color.black;
            }

            textureDepthmap = new Texture2D (sizeX, sizeZ, TextureFormat.RGB24, false);
            textureDepthmap.name = areaName + "_heightmap";
            textureDepthmap.SetPixels32 (colorArray);
            textureDepthmap.Apply ();
            textureDepthmap.filterMode = FilterMode.Point;
            textureDepthmap.wrapMode = TextureWrapMode.Clamp;

            try
            {
                var png = textureDepthmap.EncodeToPNG ();
                System.IO.File.WriteAllBytes (filePath, png);
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat ("Area manager | Encountered an exception while saving heightmap {0}\n{1}", filePath, e);
            }
        }

        void StoreHeightmapInfoInternal(int x, int z, int depthProcessed, byte slopeInfo, byte roadInfo)
        {
            var depthScaled = (byte)Mathf.Max (maxDepthScaled - depthProcessed * 10, minDepthScaled);
            var color = new Color32 (depthScaled, slopeInfo, roadInfo, byte.MaxValue);
            var colorIndex = z * boundsFull.z + x;

            colorArray[colorIndex] = color;
            if (!colorValues.Add (depthScaled))
            {
                return;
            }
            heightfieldPalette.Add (new DepthColorLink ()
            {
                color = color,
                depth = depthProcessed,
                depthScaled = depthScaled,
            });
        }

        #if !PB_MODSDK
        [ContextMenu ("Import height from tex. (R depth)")]
        [Button]
        public void ImportHeightFromStandardTexture ()
        {
            var folderPath = DataMultiLinkerCombatArea.GetCurrentUnpackedLevelPath ();
            if (folderPath == null)
            {
                Debug.Log ("Failed to load heightmap, couldn't get current unpacked level path");
                return;
            }
            var filePath = System.IO.Path.Combine(folderPath, standardHeightmapFileName);
            ImportHeightFromTexture (filePath);
        }
        #endif

        public void ImportHeightFromTexture (string filePath)
        {
            if (!LoadHeightmapFromFile (filePath))
            {
                return;
            }

            var sizeX = boundsFull.x;
            var sizeY = boundsFull.y;
            var sizeZ = boundsFull.z;

            // To ensure that the new terrain matches the existing area segments without any visible seams,
            // don't remove any terrain offsets on the border spots.
            const int fringe = 1;

            colorArray = textureDepthmap.GetPixels32 ();
            var maxDepthScaled = Mathf.Min ((sizeY - 1) * 10, byte.MaxValue);
            // The points on the north (sizeZ - 1) and east (sizeX - 1) borders don't have spots. Shrink the
            // bounds by one to exclude those points.
            var lastX = sizeX - 1;
            var lastZ = sizeZ - 1;

            for (var y = 0; y < sizeY; y += 1)
            {
                for (var z = 0; z < lastZ; z += 1)
                {
                    for (var x = 0; x < lastX; x += 1)
                    {
                        var colorIndex = z * sizeX + x;
                        var color = colorArray[colorIndex];
                        var depthSample = color.r;
                        var depthRestored = Mathf.RoundToInt (Mathf.Clamp (maxDepthScaled - depthSample, 0, maxDepthScaled) * 0.1f);
                        var posIndex = new Vector3Int (x, y, z);
                        var index = AreaUtility.GetIndexFromVolumePosition (posIndex, boundsFull, skipBoundsCheck: true);
                        var spot = points[index];

                        if (x > fringe && x < lastX - fringe && z > fringe && z < lastZ - fringe)
                        {
                            spot.terrainOffset = 0f;
                        }

                        if (y < depthRestored)
                        {
                            ClearSpot (spot);
                        }
                        else if (y == depthRestored)
                        {
                            ChangeSpotToTerrain (spot);
                        }
                        else
                        {
                            ChangeSpotToInterior (spot);
                        }
                        RemovePropPlacement (index);
                    }
                }
            }

            RebuildEverything ();

            // Terrain undergoes a smoothing process that may cause a slope to go through an empty or interior
            // spot. The tileset is 0 for these spots and they're displayed with the fallback tileset. Change
            // the tileset to terrain so they are displayed correctly.

            var fixup = false;
            for (var y = 0; y < sizeY; y += 1)
            {
                for (var z = 0; z < lastZ; z += 1)
                {
                    for (var x = 0; x < lastX; x += 1)
                    {
                        var posIndex = new Vector3Int (x, y, z);
                        var index = AreaUtility.GetIndexFromVolumePosition (posIndex, boundsFull, skipBoundsCheck: true);
                        var spot = points[index];
                        if (spot.pointState == AreaVolumePointState.Full
                            && spot.spotConfiguration != TilesetUtility.configurationFull
                            && spot.blockTileset != AreaTilesetHelper.idOfTerrain)
                        {
                            spot.blockTileset = AreaTilesetHelper.idOfTerrain;
                            fixup = true;
                            continue;
                        }
                        if (spot.pointState == AreaVolumePointState.Empty
                            && spot.spotConfiguration != TilesetUtility.configurationEmpty
                            && spot.blockTileset != AreaTilesetHelper.idOfTerrain)
                        {
                            spot.blockTileset = AreaTilesetHelper.idOfTerrain;
                            fixup = true;
                        }
                    }
                }
            }
            if (fixup)
            {
                RebuildEverything ();
            }
        }

        bool LoadHeightmapFromFile (string filePath)
        {
            if (!System.IO.File.Exists (filePath))
            {
                Debug.LogWarning ("Area manager | File doesn't exist: " + filePath);
                return false;
            }

            var sizeX = boundsFull.x;
            var sizeZ = boundsFull.z;
            try
            {
                var pngBytes = System.IO.File.ReadAllBytes (filePath);
                textureDepthmap = new Texture2D (sizeX, sizeZ, TextureFormat.RGB24, false, false)
                {
                    name = areaName + "_heightmap",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                };
                textureDepthmap.LoadImage (pngBytes);
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat ("Area manager | Encountered an exception while loading heightmap {0}\n{1}", filePath, e);
                return false;
            }

            if (textureDepthmap.width != sizeX || textureDepthmap.height != sizeZ)
            {
                Debug.LogErrorFormat
                (
                    "Area manager | Unexpected heightmap resolution {0}x{1} (expected {2}x{3}) at {4}",
                    textureDepthmap.width,
                    textureDepthmap.height,
                    sizeX,
                    sizeZ,
                    filePath
                );
                return false;
            }
            return true;
        }

        public static void ClearSpot (AreaVolumePoint spot)
        {
            spot.pointState = AreaVolumePointState.Empty;
            spot.spotConfiguration = TilesetUtility.configurationEmpty;
            spot.spotConfigurationWithDamage = spot.spotConfiguration;
            spot.road = false;
            spot.terrainOffset = 0f;
            ClearTileData (spot);
        }

        public static void ClearTileData (AreaVolumePoint spot)
        {
            spot.blockFlippedHorizontally = false;
            spot.blockRotation = 0;
            spot.blockGroup = 0;
            spot.blockSubtype = 0;
            spot.blockTileset = 0;
            spot.customization = TilesetVertexProperties.defaults;
        }

        public static void ChangeSpotToTerrain (AreaVolumePoint spot)
        {
            spot.pointState = AreaVolumePointState.Empty;
            spot.road = false;
            ClearTileData (spot);
            spot.blockTileset = AreaTilesetHelper.idOfTerrain;
        }

        public static void ChangeSpotToInterior (AreaVolumePoint spot)
        {
            spot.pointState = AreaVolumePointState.Full;
            spot.spotConfiguration = TilesetUtility.configurationFull;
            spot.spotConfigurationWithDamage = spot.spotConfiguration;
            spot.road = false;
            ClearTileData (spot);
        }

        #if !PB_MODSDK
        [ContextMenu ("Import slopes from tex. (G=127 remove, G=255 add)")][Button]
        public void ImportRampsFromTexture ()
        {
            var folderPath = DataMultiLinkerCombatArea.GetCurrentUnpackedLevelPath ();
            if (folderPath == null)
            {
                Debug.Log ("Failed to load heightmap, couldn't get current unpacked level path");
                return;
            }
            var filePath = System.IO.Path.Combine(folderPath, standardHeightmapFileName);
            ImportRampsFromTexture (filePath);
        }
        #endif

        public void ImportRampsFromTexture (string filePath)
        {
            if (!LoadHeightmapFromFile (filePath))
            {
                return;
            }

            var sizeX = boundsFull.x;
            var sizeY = boundsFull.y;
            var sizeZ = boundsFull.z;

            colorArray = textureDepthmap.GetPixels32 ();
            int slopeAdditionsFound = 0;
            int slopeRemovalsFound = 0;

            // Apply slopes once all volume changes are applied
            for (int x = 0; x < sizeX; x++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    var colorIndex = z * sizeX + x;
                    var color = colorArray[colorIndex];
                    bool slopeAdditionDesired = color.g >= (byte)255;
                    bool slopeRemovalDesired = color.g > (byte)117 && color.g < (byte)137;
                    bool slopeChangeDesired = slopeAdditionDesired || slopeRemovalDesired;

                    if (!slopeChangeDesired)
                        continue;

                    var posIndex = new Vector3Int (x, 0, z);
                    var index = AreaUtility.GetIndexFromVolumePosition (posIndex, boundsFull, skipBoundsCheck: true);
                    var pointCurrent = points[index];
                    int iteration = 0;

                    while (true)
                    {
                        if (pointCurrent == null)
                            break;

                        var pointStateCurrent = pointCurrent.pointState;
                        if (pointStateCurrent == AreaVolumePointState.Full)
                        {
                            TrySettingSlope (pointCurrent, slopeAdditionDesired, false);

                            if (slopeAdditionDesired)
                                slopeAdditionsFound += 1;

                            if (slopeRemovalDesired)
                                slopeRemovalsFound += 1;

                            break;
                        }

                        // Get point below
                        pointCurrent = pointCurrent.pointsInSpot[4];
                        iteration += 1;

                        if (iteration > sizeY)
                            break;
                    }
                }
            }

            RebuildEverything ();

            Debug.Log ($"Slope import completed. Slope additions requested (G=255): {slopeAdditionsFound} | Slope removals requested (G=127): {slopeRemovalsFound}");
        }

        #if !PB_MODSDK
        [ContextMenu ("Import roads from tex. (B)")][Button]
        public void ImportRoadsFromStandardTexture ()
        {
            var folderPath = DataMultiLinkerCombatArea.GetCurrentUnpackedLevelPath ();
            if (folderPath == null)
            {
                Debug.Log ("Failed to load heightmap, couldn't get current unpacked level path");
                return;
            }
            var filePath = System.IO.Path.Combine(folderPath, standardHeightmapFileName);
            ImportRoadsFromTexture (filePath);
        }
        #endif

        public void ImportRoadsFromTexture(string filePath)
        {
            if (!LoadHeightmapFromFile (filePath))
            {
                return;
            }
            var sizeX = boundsFull.x;
            var sizeY = boundsFull.y;
            var sizeZ = boundsFull.z;

            colorArray = textureDepthmap.GetPixels32 ();
            List<AreaVolumePoint> pointsModified = new List<AreaVolumePoint> ();

            int roadAdditions = 0;
            int roadRemovals = 0;

            // Apply slopes once all volume changes are applied
            for (int x = 0; x < sizeX; x++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    var colorIndex = z * sizeX + x;
                    var color = colorArray[colorIndex];
                    bool roadDesired = color.b == (byte)255;

                    var posIndex = new Vector3Int (x, 0, z);
                    var index = AreaUtility.GetIndexFromVolumePosition (posIndex, boundsFull, skipBoundsCheck: true);
                    var pointCurrent = points[index];
                    int iteration = 0;

                    while (true)
                    {
                        if (pointCurrent == null)
                            break;

                        var pointStateCurrent = pointCurrent.pointState;
                        if (pointStateCurrent == AreaVolumePointState.Full)
                        {
                            bool mismatch = pointCurrent.road != roadDesired;
                            if (mismatch)
                            {
                                pointCurrent.road = roadDesired;
                                pointsModified.Add (pointCurrent);

                                if (roadDesired)
                                    roadAdditions += 1;
                                else
                                    roadRemovals += 1;
                            }
                            else if (roadDesired)
                                pointsModified.Add (pointCurrent);

                            break;
                        }

                        // Get point below
                        pointCurrent = pointCurrent.pointsInSpot[4];
                        iteration += 1;

                        if (iteration > sizeY)
                            break;
                    }
                }
            }

            for (int i = 0; i < pointsModified.Count; ++i)
                UpdateRoadConfigurations (pointsModified[i], roadSubtype);

            RebuildEverything ();

            Debug.Log ($"Road import completed. Road additions requested (B=255): {roadAdditions} | Road removals: {roadRemovals} | Total points modified: {pointsModified.Count}");

        }

        public enum RoadEditingOperation
        {
            None = 0,
            Add = 1,
            Remove = 2,
            FloodFill = 3,
            SubtypeNext = 10,
            SubtypePrev = 11
        }

        public enum EditingVolumeBrush
        {
            Point,
            Square2x2,
            Square3x3,
            Circle,
            //Sphere,
            //Cube3x3x3
        }

        private static readonly List<AreaVolumePoint> pointsToEdit = new List<AreaVolumePoint> ();

        public static List<AreaVolumePoint> CollectPointsInBrush (AreaVolumePoint pointStart, EditingVolumeBrush brush)
        {
            pointsToEdit.Clear ();
            pointsToEdit.Add (pointStart);

            if (brush == EditingVolumeBrush.Circle || brush == EditingVolumeBrush.Square3x3)
            {
                // X+ : east
                if (pointStart.pointsInSpot[1] != null)
                    pointsToEdit.Add (pointStart.pointsInSpot[1]);

                // Z+ : north
                if (pointStart.pointsInSpot[2] != null)
                    pointsToEdit.Add (pointStart.pointsInSpot[2]);

                // X- : west
                if (pointStart.pointsWithSurroundingSpots[6] != null)
                    pointsToEdit.Add (pointStart.pointsWithSurroundingSpots[6]);

                // Z- : south
                if (pointStart.pointsWithSurroundingSpots[5] != null)
                    pointsToEdit.Add (pointStart.pointsWithSurroundingSpots[5]);
            }

            if (brush == EditingVolumeBrush.Square3x3)
            {
                // X+ & Z+ : northeast
                if (pointStart.pointsInSpot[3] != null)
                    pointsToEdit.Add (pointStart.pointsInSpot[3]);

                // X- & Z+ : northwest
                var nw = pointStart.pointsWithSurroundingSpots[6]?.pointsInSpot[2];
                if (nw != null)
                    pointsToEdit.Add (nw);

                // X- & Z- : southwest
                if (pointStart.pointsWithSurroundingSpots[4] != null)
                    pointsToEdit.Add (pointStart.pointsWithSurroundingSpots[4]);

                // X+ & Z- : southeast
                var se = pointStart.pointsInSpot[1]?.pointsWithSurroundingSpots[5];
                if (se != null)
                    pointsToEdit.Add (se);
            }

            if (brush == EditingVolumeBrush.Square2x2)
            {
                // X- : west
                if (pointStart.pointsWithSurroundingSpots[6] != null)
                    pointsToEdit.Add (pointStart.pointsWithSurroundingSpots[6]);

                // Z- : south
                if (pointStart.pointsWithSurroundingSpots[5] != null)
                    pointsToEdit.Add (pointStart.pointsWithSurroundingSpots[5]);

                // X- & Z- : southwest
                if (pointStart.pointsWithSurroundingSpots[4] != null)
                    pointsToEdit.Add (pointStart.pointsWithSurroundingSpots[4]);
            }

            #if PB_MODSDK
            pointsToEdit.Sort (OrderPointsToEditByIndex);
            #endif

            return pointsToEdit;
        }

        #if PB_MODSDK
        static int OrderPointsToEditByIndex (AreaVolumePoint x, AreaVolumePoint y) => x.spotIndex.CompareTo (y.spotIndex);
        #endif

        public bool rampImportOnGeneration = false;
        public bool propImportOverrides = false;
        public string propImportOverrideRed = "l3_tall_fir";
        public string propImportOverrideYellow = "l2_mid_fir_mix";
        public string propImportOverrideGreen = "l1_low_fir_mix";

        private static string propImportOverrideRedHex = UtilityColor.ToHexRGB (new Color (1f, 0f, 0f));
        private static string propImportOverrideYellowHex = UtilityColor.ToHexRGB (new Color (1f, 1f, 0f));
        private static string propImportOverrideGreenHex = UtilityColor.ToHexRGB (new Color (0f, 1f, 0f));


        private DataBlockAreaPropGroup GetPropGroupFromHex (string hex)
        {
            if (string.IsNullOrEmpty (hex))
                return null;

            if (propImportOverrides)
            {
                if (string.Equals (hex, propImportOverrideRedHex, StringComparison.InvariantCultureIgnoreCase))
                {
                    DataLinkerCombatBiomes.data.propGroups.TryGetValue (propImportOverrideRed, out var group);
                    return group;
                }
                if (string.Equals (hex, propImportOverrideYellowHex, StringComparison.InvariantCultureIgnoreCase))
                {
                    DataLinkerCombatBiomes.data.propGroups.TryGetValue (propImportOverrideYellow, out var group);
                    return group;
                }
                if (string.Equals (hex, propImportOverrideGreenHex, StringComparison.InvariantCultureIgnoreCase))
                {
                    DataLinkerCombatBiomes.data.propGroups.TryGetValue (propImportOverrideGreen, out var group);
                    return group;
                }
            }

            DataLinkerCombatBiomes.data.propGroupsByColor.TryGetValue (hex, out var groupFromColor);
            return groupFromColor;
        }

        public const string standardPropMaskVegetationFileName = "mask_vegetation.png";

        #if !PB_MODSDK
        [ContextMenu ("Import props from texture")][Button]
        public void ImportPropsFromTexture ()
        {
            var folderPath = DataMultiLinkerCombatArea.GetCurrentUnpackedLevelPath ();
            if (folderPath == null)
            {
                Debug.Log ($"Failed to load props, couldn't get current unpacked level path");
                return;
            }
            var filePath = DataPathHelper.GetCombinedCleanPath (folderPath, standardPropMaskVegetationFileName);
            ImportPropsFromTexture (filePath);
        }
        #endif

        public void ImportPropsFromTexture (string filePath)
        {
            var sizeX = boundsFull.x;
            var sizeZ = boundsFull.z;
            var sizeXDouble = sizeX * 2;
            var sizeZDouble = sizeZ * 2;

            if (!System.IO.File.Exists (filePath))
            {
                Debug.LogWarning ("Area manager | File doesn't exist: " + filePath);
                return;
            }

            try
            {
                var pngBytes = System.IO.File.ReadAllBytes (filePath);
                textureMaskVegetation = new Texture2D (sizeXDouble, sizeZDouble, TextureFormat.RGB24, false, false);
                textureMaskVegetation.name = areaName + "_mask_vegetation";
                textureMaskVegetation.filterMode = FilterMode.Point;
                textureMaskVegetation.wrapMode = TextureWrapMode.Clamp;
                textureMaskVegetation.LoadImage (pngBytes);
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat ($"Area manager | Encountered an exception while loading vegetation mask {filePath}\n{0}", e);
            }

            if (textureMaskVegetation.width != sizeXDouble || textureMaskVegetation.height != sizeZDouble)
            {
                Debug.LogError ($"Area manager | Unexpected heightmap resolution {textureMaskVegetation.width}x{textureMaskVegetation.height} (expected {sizeXDouble}x{sizeZDouble}) at {filePath}\n{0}");
                return;
            }

            Color[] colorArray = textureMaskVegetation.GetPixels ();
            int gridSize = TilesetUtility.blockAssetSize;
            float gridSizeHalf = gridSize * 0.5f;
            var dualGridOffset = new Vector3 (-gridSizeHalf, 0f, -gridSizeHalf) * 0.5f;

            var offsetDiag1 = new Vector3 (0.27f, 0f, 0.27f);
            var offsetDiag2 = new Vector3 (-0.27f, 0f, 0.27f);
            var offsetDiag3 = new Vector3 (-0.27f, 0f, -0.27f);
            var offsetDiag4 = new Vector3 (0.27f, 0f, -0.27f);

            var colorLink = Color.white.WithAlpha (0.1f);
            var spotRaycastHitOffset = new Vector3 (-1.5f, 1.5f, -1.5f);
            var volumePos = GetHolderColliders ().position;

            for (int x = 0; x < sizeXDouble; x++)
            {
                for (int z = 0; z < sizeZDouble; z++)
                {
                    var colorIndex = z * sizeXDouble + x;
                    var color = colorArray[colorIndex];
                    var posRaycast = new Vector3 (x * gridSizeHalf, 25f, z * gridSizeHalf) + dualGridOffset;

                    var posGround = AreaUtility.GroundPoint (posRaycast);
                    if (posGround.y > 0f)
                    {
                        // Debug.Log ($"Failed to find position for pixel {x}x{z}");
                        continue;
                    }

                    var hex = UtilityColor.ToHexRGB (color);
                    var group = GetPropGroupFromHex (hex);
                    if (group == null)
                        continue;

                    var height = group.debugHeight;
                    var colorFaded = color.WithAlpha (0.5f);
                    var colorDark = Color.Lerp (color, Color.black, 0.5f);

                    var posGroundShifted = posGround + spotRaycastHitOffset;
                    int indexForSpot = AreaUtility.GetIndexFromWorldPosition (posGroundShifted, volumePos, boundsFull);
                    if (!indexForSpot.IsValidIndex (points))
                        continue;

                    var point = points[indexForSpot];
                    if (point.blockTileset != AreaTilesetHelper.idOfTerrain)
                        continue;

                    Debug.DrawLine (posGround, point.pointPositionLocal, colorLink, 15f);

                    Debug.DrawLine (posGround, posGround + Vector3.up * (height * 0.5f), colorDark, 15f);
                    Debug.DrawLine (posGround + Vector3.up * (height * 0.5f), posGround + Vector3.up * height, color, 15f);
                    Debug.DrawLine (posGround + offsetDiag1, posGround + offsetDiag3, colorFaded, 15f);
                    Debug.DrawLine (posGround + offsetDiag2, posGround + offsetDiag4, colorFaded, 15f);

                    int propSelectionID = group.propsIDs.GetRandomEntry ();
                    AreaPlacementProp placement = new AreaPlacementProp ();
                    AreaPropPrototypeData prototype = AreaAssetHelper.GetPropPrototype (propSelectionID);

                    if (prototype == null)
                        continue;

                    var posLocal = posGroundShifted - point.pointPositionLocal;
                    var pointIndex = point.spotIndex;

                    placement.id = propSelectionID;
                    placement.pivotIndex = pointIndex;
                    placement.rotation = 0;
                    placement.flipped = false;
                    placement.offsetX = posLocal.x;
                    placement.offsetZ = posLocal.z;
                    placement.hsbPrimary = Constants.defaultHSBOffset;
                    placement.hsbSecondary = Constants.defaultHSBOffset;

                    if (IsPropPlacementValid (placement, point, prototype, false))
                    {
                        if (!indexesOccupiedByProps.ContainsKey (pointIndex))
                            indexesOccupiedByProps.Add (pointIndex, new List<AreaPlacementProp> ());

                        indexesOccupiedByProps[pointIndex].Add (placement);
                        placementsProps.Add (placement);

                        ExecutePropPlacement (placement);
                        SnapPropRotation (placement);
                    }
                }
            }
        }

        public void RemoveRampsEverywhere ()
        {
            for (int i = 0; i < points.Count; ++i)
            {
                var point = points[i];
                if (point != null)
                    point.terrainOffset = 0f;
            }

            RebuildEverything ();
        }

        #if !PB_MODSDK
        public void SetRampsEverywhere (SlopeProximityCheck proximityCheck)
        {
            var folderPath = DataMultiLinkerCombatArea.GetCurrentUnpackedLevelPath ();
            if (folderPath == null)
            {
                Debug.Log ("Failed to load heightmap, couldn't get current unpacked level path");
                return;
            }
            var filePath = System.IO.Path.Combine (folderPath, standardHeightmapFileName);
            SetRampsEverywhere (filePath, proximityCheck);
        }
        #endif

        public void SetRampsEverywhere (string filePath, SlopeProximityCheck proximityCheck)
        {
            bool IsPointOnSurface (AreaVolumePoint point)
            {
                if (point == null || point.pointState != AreaVolumePointState.Full)
                    return false;

                var pointAbove = point.pointsWithSurroundingSpots[3];
                if (pointAbove == null || pointAbove.pointState != AreaVolumePointState.Empty)
                    return false;

                return true;
            }

            for (int i = 0, limit = points.Count; i < limit; ++i)
            {
                var point = points[i];
                if (!IsPointOnSurface (point))
                    continue;

                TrySettingSlope (point, true, false, proximityCheck);
            }

            if (rampImportOnGeneration)
            {
                ImportRampsFromTexture (filePath);
                return;
            }
            RebuildEverything ();
        }

        private List<AreaVolumePoint> slopePointNeighbors = new List<AreaVolumePoint> ();
        private List<AreaVolumePoint> slopePointNeighborsLeft = new List<AreaVolumePoint> ();
        private List<AreaVolumePoint> slopePointNeighborsRight = new List<AreaVolumePoint> ();
        private List<AreaVolumePoint> slopePointNeighborsLeft2 = new List<AreaVolumePoint> ();
        private List<AreaVolumePoint> slopePointNeighborsRight2 = new List<AreaVolumePoint> ();
        private List<AreaVolumePoint> slopePointsAffected = new List<AreaVolumePoint> ();

        public enum SlopeProximityCheck
        {
            None,
            LateralSingle,
            LateralDouble
        }

        public void TrySettingSlope
        (
            AreaVolumePoint pointStartFull,
            bool slopeDesired,
            bool selectiveUpdates = true,
            SlopeProximityCheck proximityCheck = SlopeProximityCheck.None,
            bool log = false
        )
        {
            var pointAboveStartEmpty = pointStartFull?.pointsWithSurroundingSpots[3];

            bool IsPointUsable (AreaVolumePoint tstPoint, AreaVolumePointState stateExpected, bool log)
            {
                if (tstPoint == null || tstPoint.pointState != stateExpected)
                    return false;

                int xMax = boundsFull.x - 1;
                int zMax = boundsFull.z - 1;

                // Only iterate over top points in neighbor list
                for (int p = 0; p < 4; ++p)
                {
                    var pt = tstPoint.pointsWithSurroundingSpots[p];
                    if (pt == null)
                        return false;

                    // Skip cases where any spot in the surrounding 2x2x2 volume is not all terrain
                    if (pt.pointState != AreaVolumePointState.Empty && pt.blockTileset != 0 && pt.blockTileset != AreaTilesetHelper.idOfTerrain)
                    {
                        if (log)
                        {
                            DebugExtensions.DrawCube (pt.pointPositionLocal, Vector3.one, Color.red, 1f);
                            Debug.DrawLine (pt.pointPositionLocal, tstPoint.pointPositionLocal, Color.red, 1f);
                        }
                        return false;
                    }

                    if (pt.pointPositionIndex.x <= 0 || pt.pointPositionIndex.z <= 0)
                    {
                        if (log)
                        {
                            DebugExtensions.DrawCube (pt.pointPositionLocal, Vector3.one, Color.red, 1f);
                            Debug.DrawLine (pt.pointPositionLocal, tstPoint.pointPositionLocal, Color.red, 1f);
                        }
                        return false;
                    }

                    if (pt.pointPositionIndex.x >= xMax || pt.pointPositionIndex.z >= zMax)
                    {
                        if (log)
                        {
                            DebugExtensions.DrawCube (pt.pointPositionLocal, Vector3.one, Color.red, 1f);
                            Debug.DrawLine (pt.pointPositionLocal, tstPoint.pointPositionLocal, Color.red, 1f);
                        }
                        return false;
                    }
                }

                return true;
            }

            if (!IsPointUsable (pointStartFull, AreaVolumePointState.Full, log) || !IsPointUsable (pointAboveStartEmpty, AreaVolumePointState.Empty, log))
            {
                if (log)
                {
                    Debug.Log ($"Point {pointStartFull.ToLog ()} (origin, expected full) or {pointAboveStartEmpty.ToLog ()} (above origin, expected empty) is not compatible with ramps");

                    if (pointStartFull != null)
                        DebugExtensions.DrawCube (pointStartFull.pointPositionLocal, Vector3.one, Color.yellow, 1f);

                    if (pointAboveStartEmpty != null)
                        DebugExtensions.DrawCube (pointAboveStartEmpty.pointPositionLocal, Vector3.one, Color.yellow, 1f);
                }

                return;
            }

            // Debug.Log ($"Slope set to {slopeDesired} | Corners allowed: {cornersAllowed} | Point start: {pointStartFull.ToLog ()} | Point above: {pointAboveStartEmpty.ToLog ()}");

            slopePointNeighbors.Clear ();
            slopePointsAffected.Clear ();

            // X+
            var neighborXPos = pointStartFull.pointsInSpot[1];
            slopePointNeighbors.Add (neighborXPos);

            // Z+
            var neighborZPos = pointStartFull.pointsInSpot[2];
            slopePointNeighbors.Add (neighborZPos);

            // X-
            var neighborXNeg = pointStartFull.pointsWithSurroundingSpots[6];
            slopePointNeighbors.Add (neighborXNeg);

            // Z-
            var neighborZNeg = pointStartFull.pointsWithSurroundingSpots[5];
            slopePointNeighbors.Add (neighborZNeg);

            if (proximityCheck == SlopeProximityCheck.LateralSingle || proximityCheck == SlopeProximityCheck.LateralDouble)
            {
                slopePointNeighborsLeft.Clear ();
                slopePointNeighborsRight.Clear ();

                slopePointNeighborsRight.Add (neighborXPos?.pointsInSpot[2]);
                slopePointNeighborsLeft.Add (neighborXPos?.pointsWithSurroundingSpots[5]);

                slopePointNeighborsRight.Add (neighborZPos?.pointsInSpot[1]);
                slopePointNeighborsLeft.Add (neighborZPos?.pointsWithSurroundingSpots[6]);

                slopePointNeighborsRight.Add (neighborXNeg?.pointsWithSurroundingSpots[5]);
                slopePointNeighborsLeft.Add (neighborXNeg?.pointsInSpot[2]);

                slopePointNeighborsRight.Add (neighborZNeg?.pointsInSpot[1]);
                slopePointNeighborsLeft.Add (neighborZNeg?.pointsWithSurroundingSpots[6]);

                if (proximityCheck == SlopeProximityCheck.LateralDouble)
                {
                    slopePointNeighborsLeft2.Clear ();
                    slopePointNeighborsRight2.Clear ();

                    slopePointNeighborsRight2.Add (slopePointNeighborsRight[0]?.pointsInSpot[2]);
                    slopePointNeighborsLeft2.Add (slopePointNeighborsLeft[0]?.pointsWithSurroundingSpots[5]);

                    slopePointNeighborsRight2.Add (slopePointNeighborsRight[1]?.pointsInSpot[1]);
                    slopePointNeighborsLeft2.Add (slopePointNeighborsLeft[1]?.pointsWithSurroundingSpots[6]);

                    slopePointNeighborsRight2.Add (slopePointNeighborsRight[2]?.pointsWithSurroundingSpots[5]);
                    slopePointNeighborsLeft2.Add (slopePointNeighborsLeft[2]?.pointsInSpot[2]);

                    slopePointNeighborsRight2.Add (slopePointNeighborsRight[3]?.pointsInSpot[1]);
                    slopePointNeighborsLeft2.Add (slopePointNeighborsLeft[3]?.pointsWithSurroundingSpots[6]);
                }
            }

            for (int i = 0, limit = slopePointNeighbors.Count; i < limit; ++i)
            {
                // For each of these horizontal neighbors, find the point under them
                // A horizontal neighbor must be empty, a point under them must be full
                var pointNeighbor = slopePointNeighbors[i];
                var pointNeighborUnder = pointNeighbor?.pointsInSpot[4];

                // Validate each point for state, proximity to edges, missing spots or wrong tilesets
                if (!IsPointUsable (pointNeighbor, AreaVolumePointState.Empty, log) || !IsPointUsable (pointNeighborUnder, AreaVolumePointState.Full, log))
                {
                    if (log)
                    {
                        Debug.Log ($"Point {pointNeighbor.ToLog ()} (neighbor {i}, expected empty) or {pointNeighborUnder.ToLog ()} (under it, expected full) is not compatible with ramps");

                        if (pointNeighbor != null)
                            DebugExtensions.DrawCube (pointNeighbor.pointPositionLocal, Vector3.one, Color.red, 1f);

                        if (pointNeighborUnder != null)
                            DebugExtensions.DrawCube (pointNeighborUnder.pointPositionLocal, Vector3.one, Color.red, 1f);
                    }
                    continue;
                }

                if (proximityCheck == SlopeProximityCheck.LateralSingle || proximityCheck == SlopeProximityCheck.LateralDouble)
                {
                    var pointNeighborLeft = slopePointNeighborsLeft[i];
                    var pointNeighborLeftUnder = pointNeighborLeft?.pointsInSpot[4];

                    if (!IsPointUsable (pointNeighborLeft, AreaVolumePointState.Empty, log) || !IsPointUsable (pointNeighborLeftUnder, AreaVolumePointState.Full, log))
                    {
                        if (log)
                        {
                            Debug.Log ($"Point {pointNeighborLeft.ToLog ()} (neighbor left {i}, expected empty) or {pointNeighborLeftUnder.ToLog ()} (under it, expected full) is not compatible with ramps");

                            if (pointNeighborLeft != null)
                                DebugExtensions.DrawCube (pointNeighborLeft.pointPositionLocal, Vector3.one, Color.red, 1f);

                            if (pointNeighborLeftUnder != null)
                                DebugExtensions.DrawCube (pointNeighborLeftUnder.pointPositionLocal, Vector3.one, Color.red, 1f);
                        }
                        continue;
                    }

                    var pointNeighborRight = slopePointNeighborsRight[i];
                    var pointNeighborRightUnder = pointNeighborRight?.pointsInSpot[4];

                    if (!IsPointUsable (pointNeighborRight, AreaVolumePointState.Empty, log) || !IsPointUsable (pointNeighborRightUnder, AreaVolumePointState.Full, log))
                    {
                        if (log)
                        {
                            Debug.Log ($"Point {pointNeighborRight.ToLog ()} (neighbor right {i}, expected empty) or {pointNeighborRightUnder.ToLog ()} (under it, expected full) is not compatible with ramps");

                            if (pointNeighborRight != null)
                                DebugExtensions.DrawCube (pointNeighborRight.pointPositionLocal, Vector3.one, Color.red, 1f);

                            if (pointNeighborRightUnder != null)
                                DebugExtensions.DrawCube (pointNeighborRightUnder.pointPositionLocal, Vector3.one, Color.red, 1f);
                        }

                        continue;
                    }

                    if (proximityCheck == SlopeProximityCheck.LateralDouble)
                    {
                        var pointNeighborLeft2 = slopePointNeighborsLeft2[i];
                        var pointNeighborLeft2Under = pointNeighborLeft2?.pointsInSpot[4];

                        if (!IsPointUsable (pointNeighborLeft2, AreaVolumePointState.Empty, log) || !IsPointUsable (pointNeighborLeft2Under, AreaVolumePointState.Full, log))
                            continue;

                        var pointNeighborRight2 = slopePointNeighborsRight2[i];
                        var pointNeighborRight2Under = pointNeighborRight2?.pointsInSpot[4];

                        if (!IsPointUsable (pointNeighborRight2, AreaVolumePointState.Empty, log) || !IsPointUsable (pointNeighborRight2Under, AreaVolumePointState.Full, log))
                            continue;
                    }
                }

                // At this point we're ready to apply changes
                if (slopeDesired)
                {
                    pointNeighbor.terrainOffset = 1f / 3f;
                    pointAboveStartEmpty.terrainOffset = -1f / 3f;
                }
                else
                {
                    pointNeighbor.terrainOffset = 0f;
                    pointAboveStartEmpty.terrainOffset = 0f;
                }

                slopePointsAffected.Add (pointAboveStartEmpty);
            }

            if (slopePointsAffected.Count > 0)
            {
                slopePointsAffected.Add (pointAboveStartEmpty);

                bool terrainModified = true;
                for (int i = 0; i < slopePointsAffected.Count; ++i)
                {
                    AreaVolumePoint point = slopePointsAffected[i];

                    for (int s = 0; s < 8; ++s)
                    {
                        AreaVolumePoint pointWithNeighbourSpot = point.pointsWithSurroundingSpots[s];
                        if (pointWithNeighbourSpot == null)
                            continue;

                        if (pointWithNeighbourSpot.blockTileset == AreaTilesetHelper.idOfTerrain)
                        {
                            pointWithNeighbourSpot.blockFlippedHorizontally = false;
                            pointWithNeighbourSpot.blockRotation = 0;
                            pointWithNeighbourSpot.blockGroup = 0;
                            pointWithNeighbourSpot.blockSubtype = 0;
                        }

                        if (selectiveUpdates)
                        {
                            UpdateSpotAtIndex (pointWithNeighbourSpot.spotIndex, false);
                            RebuildBlock (pointWithNeighbourSpot, false);
                            RebuildCollisionForPoint (pointWithNeighbourSpot);
                        }
                    }

                    if (selectiveUpdates)
                        UpdateDamageAroundIndex (pointStartFull.spotIndex);
                }

                if (selectiveUpdates)
                {
                    var sceneHelper = CombatSceneHelper.ins;
                    sceneHelper.terrain.Rebuild (true);
                }
            }
        }

        public void EditRoad (int indexStart, RoadEditingOperation operation)
        {
            var pointStart = points[indexStart];
            CollectPointsInBrush (pointStart, editingVolumeBrush);
            EditRoadPoints (operation);
        }

        public void EditRoadPoints (List<AreaVolumePoint> roadPoints, RoadEditingOperation operation)
        {
            pointsToEdit.Clear ();
            pointsToEdit.AddRange (roadPoints);
            EditRoadPoints (operation);
        }

        void EditRoadPoints (RoadEditingOperation operation)
        {
            var terrainModified = false;
            var roadAdded = operation == RoadEditingOperation.Add;
            var roadRemoved = operation == RoadEditingOperation.Remove;
            var roadFloodFill = operation == RoadEditingOperation.FloodFill;
            var roadSubtypeNext = operation == RoadEditingOperation.SubtypeNext;
            var roadSubtypePrev = operation == RoadEditingOperation.SubtypePrev;

            if (roadAdded || roadRemoved)
            {
                for (var i = 0; i < pointsToEdit.Count; i += 1)
                {
                    var point = pointsToEdit[i];
                    if (point.pointState != AreaVolumePointState.Full)
                    {
                        Debug.Log ("AM (I) | EditRoad | One of the core points (" + i + ") is not full, aborting");
                        return;
                    }

                    for (var a = 0; a < 4; a += 1)
                    {
                        var pointAbove = point.pointsWithSurroundingSpots[a];
                        if (pointAbove == null)
                        {
                            Debug.Log ("AM (I) | EditRoad | One of the surface points (" + a + ") above the starting one is null, aborting");
                            return;
                        }

                        if (pointAbove.spotConfiguration != AreaNavUtility.configFloor)
                        {
                            Debug.Log ("AM (I) | EditRoad | One of the surface points (" + a + ") above has non-00001111 configuration, aborting");
                            return;
                        }
                    }

                    if (!terrainModified)
                    {
                        for (var s = 0; s < 8; s += 1)
                        {
                            AreaVolumePoint pointWithNeighbourSpot = point.pointsWithSurroundingSpots[s];
                            if (pointWithNeighbourSpot == null)
                            {
                                continue;
                            }
                            if (pointWithNeighbourSpot.blockTileset == AreaTilesetHelper.idOfTerrain)
                            {
                                terrainModified = true;
                                break;
                            }
                        }
                    }
                }
            }

            if (roadAdded)
            {
                for (var i = 0; i < pointsToEdit.Count; i += 1)
                {
                    pointsToEdit[i].road = true;
                }
                for (var i = 0; i < pointsToEdit.Count; i += 1)
                {
                    UpdateRoadConfigurations (pointsToEdit[i], roadSubtype);
                }
            }
            else if (roadRemoved)
            {
                for (var i = 0; i < pointsToEdit.Count; i += 1)
                {
                    pointsToEdit[i].road = false;
                }
                for (var i = 0; i < pointsToEdit.Count; i += 1)
                {
                    UpdateRoadConfigurations (pointsToEdit[i], roadSubtype);
                }
            }
            else if (roadFloodFill)
            {
                FloodFillRoadSubtype (pointsToEdit, roadSubtype);
                terrainModified = true;
            }
            else if (roadSubtypeNext || roadSubtypePrev)
            {
                var roadSubtypeInt = (int)roadSubtype;
                roadSubtypeInt += roadSubtypeNext ? 10 : -10;
                if (roadSubtypeInt > 30)
                {
                    roadSubtypeInt = 0;
                }
                else if (roadSubtypeInt < 0)
                {
                    roadSubtypeInt = 30;
                }
                roadSubtype = (RoadSubtype)roadSubtypeInt;
            }

            if (terrainModified)
            {
                var sceneHelper = CombatSceneHelper.ins;
                sceneHelper.terrain.Rebuild (true);
            }
        }

        public static EditingVolumeBrush editingVolumeBrush = EditingVolumeBrush.Point;
        public static RoadSubtype roadSubtype = RoadSubtype.GrassDirt;

        public enum RoadConfigType
        {
	        Empty,
	        Full,
	        Straight,
	        InCorner,
	        OutCorner,
	        BiDiagonal
        }

        public class AreaRoadData
        {
            public bool[] configurationAsArray = new bool[4];
            public byte usedGroup = 0;
            public byte usedRotation = 0;
			public RoadConfigType configType;

            public AreaRoadData (RoadConfigType configType, bool a, bool b, bool c, bool d, byte usedGroup, byte usedRotation)
            {
                this.configurationAsArray = new bool[] { a, b, c, d };
                this.usedGroup = usedGroup;
                this.usedRotation = usedRotation;
				this.configType = configType;
            }
        }

        private static AreaRoadData[] cachedRoadData;

		public class AreaRoadCurveData
		{
			public struct SpotData
			{
				public bool reqActive;

				public int neighbourIndex;
				public RoadConfigType reqConfigType;
				public byte reqRotation;

				public bool editActive;

				public byte group;
				public byte rotationShift;
				public bool flip;

				public SpotData(int neighbourIndex, RoadConfigType reqConfigType, byte reqRotation, byte group, byte rotationShift, bool flip)
				{
					this.reqActive = true;
					this.editActive = true;

					this.neighbourIndex = neighbourIndex;
					this.reqConfigType = reqConfigType;
					this.reqRotation = reqRotation;

					this.group = group;
					this.rotationShift = rotationShift;
					this.flip = flip;
				}

				public SpotData(int neighbourIndex, RoadConfigType reqConfigType, byte reqRotation)
				{
					this.reqActive = true;
					this.editActive = false;

					this.neighbourIndex = neighbourIndex;
					this.reqConfigType = reqConfigType;
					this.reqRotation = reqRotation;

					this.group = 0;
					this.rotationShift = 0;
					this.flip = false;
				}
			}

			public SpotData spot1;
			public SpotData spot2;
			public SpotData spot3;

			public AreaRoadCurveData(SpotData spot1)
			{
				this.spot1 = spot1;
			}

			public AreaRoadCurveData(SpotData spot1, SpotData spot2)
			{
				this.spot1 = spot1;
				this.spot2 = spot2;
			}

			public AreaRoadCurveData(SpotData spot1, SpotData spot2, SpotData spot3)
			{
				this.spot1 = spot1;
				this.spot2 = spot2;
				this.spot3 = spot3;
			}

			public SpotData GetData(int i)
			{
				switch(i)
				{
					case 0:	return spot1;
					case 1:	return spot2;
					case 2:	return spot3;
				}

				return default(SpotData);
			}

			public int DataCount => 3;
		}

		private static AreaRoadCurveData[] cachedRoadCurveData;





		// The road curve tool matches against a library of patterns to detect if we can switch blocks to a smooth turn
		// the pattern spec includes the road configuration type and the rotation of several blocks
		// if all match, we replace them with whatever the pattern specifies
		public void EditRoadCurves (int spotIndex, KeyCode mouseButton, bool shift)
		{
			AreaVolumePoint startingPoint = points[spotIndex];

			var roadDataStarting = GetRoadDataForPoint(startingPoint);
			var roadCurveData = GetRoadCurveData();

			if(mouseButton == KeyCode.Mouse0 || mouseButton == KeyCode.Mouse1)
			{
				//Debug.Log($"{roadDataStarting?.configType??RoadConfigType.Empty} {roadDataStarting.usedRotation} {startingPoint.pointPositionIndex}");

				var pointList = new List<(AreaVolumePoint pt, AreaRoadData data)>();

				void AddPoint(AreaVolumePoint pt)
				{
					if(pt == null)
						pointList.Add((null, null));
					else
						pointList.Add((pt, GetRoadDataForPoint(pt)));
				}

				//Index 0 is the center point; 1-4 are in rotation order
				AddPoint(startingPoint);
				AddPoint(startingPoint.pointsInSpot[1]);
				AddPoint(startingPoint.pointsWithSurroundingSpots[5]);
				AddPoint(startingPoint.pointsWithSurroundingSpots[6]);
				AddPoint(startingPoint.pointsInSpot[2]);


				//go through patterns
				foreach(var curveTemplate in roadCurveData)
				{
					//Try all four rotations of each pattern
					for(int r = 0;r < 4;++r)
					{
						bool allReqsMet = true;
						//Go through individual spot requirements of a pattern
						for(int i = 0;i < curveTemplate.DataCount;++i)
						{
							var spotData = curveTemplate.GetData(i);
							if(!spotData.reqActive)
								continue;

							var rotatedNeighbourIndex = spotData.neighbourIndex == 0?0:((spotData.neighbourIndex-1+4+r)%4+1);

							if(rotatedNeighbourIndex < 0 || rotatedNeighbourIndex >= pointList.Count)
							{
								allReqsMet = false;
								break;
							}

							var pointInfo = pointList[rotatedNeighbourIndex];
							if(pointInfo.pt == null)
							{
								allReqsMet = false;
								break;
							}

							var reqMet = pointInfo.data.configType == spotData.reqConfigType && (pointInfo.data.usedRotation + r)%4 == spotData.reqRotation;

							allReqsMet &= reqMet;

							if(!allReqsMet)
								break;
						}

						if(!allReqsMet)
							continue;

						bool anyModified = false;
						//Apply modifications
						for(int i = 0;i < curveTemplate.DataCount;++i)
						{
							var spotData = curveTemplate.GetData(i);
							var rotatedNeighbourIndex = spotData.neighbourIndex == 0?0:((spotData.neighbourIndex-1+4+r)%4+1);
							var pointInfo = pointList[rotatedNeighbourIndex];

							if(!spotData.reqActive || !spotData.editActive)
								continue;

							bool modified = false;
							if(mouseButton == KeyCode.Mouse0)
							{
								var rotationVal = (byte)((pointInfo.data.usedRotation + spotData.rotationShift + 4) % 4);

								modified = (pointInfo.pt.blockGroup != spotData.group || pointInfo.pt.blockRotation != rotationVal || pointInfo.pt.blockFlippedHorizontally != spotData.flip);

								pointInfo.pt.blockGroup = spotData.group;
								pointInfo.pt.blockRotation = rotationVal;
								pointInfo.pt.blockFlippedHorizontally = spotData.flip;
							}
							else
							{
								pointInfo.pt.blockGroup = pointInfo.data.usedGroup;
								pointInfo.pt.blockRotation = pointInfo.data.usedRotation;
								pointInfo.pt.blockFlippedHorizontally = false;
								modified = true;
							}

							anyModified |= modified;

							if(modified)
								RebuildBlock (pointInfo.pt, false);
						}

						if(anyModified)
							goto donePatternMatching;
					}
				}

				donePatternMatching:;
			}
		}

		void FloodFillRoadSubtype(List<AreaVolumePoint> startPoints, RoadSubtype roadSubtype)
		{
			Queue<AreaVolumePoint> candidates = new Queue<AreaVolumePoint>();
			HashSet<AreaVolumePoint> processedSet = new HashSet<AreaVolumePoint>();
			List<AreaVolumePoint> resultList = new List<AreaVolumePoint>();

			foreach (var pt in startPoints)
				candidates.Enqueue(pt);

			while(candidates.Count > 0)
			{
				var pt = candidates.Dequeue();

				if(pt == null || !pt.road || processedSet.Contains(pt))
					continue;

				resultList.Add(pt);
				processedSet.Add(pt);

				candidates.Enqueue(pt.pointsInSpot[1]);
				candidates.Enqueue(pt.pointsWithSurroundingSpots[6]);
				candidates.Enqueue(pt.pointsInSpot[2]);
				candidates.Enqueue(pt.pointsWithSurroundingSpots[5]);
			}

			foreach (var pt in resultList)
			{
				for (int i = 0; i < 4; ++i)
				{
					AreaVolumePoint pointAbove = pt.pointsWithSurroundingSpots[i];
					if (pointAbove == null)
						break;

					if (pointAbove.spotConfiguration != (byte)15)
						break;

					pointAbove.blockSubtype = (byte)(int)roadSubtype;
					RebuildBlock (pointAbove, false);
				}
			}
		}

        private AreaRoadCurveData[] GetRoadCurveData()
		{
			if(cachedRoadCurveData == null)
			{
				cachedRoadCurveData = new[]
				{
					//90 degree turns (inner)
					new AreaRoadCurveData(
						new AreaRoadCurveData.SpotData(0, RoadConfigType.InCorner, 2, 8, 0, false),
						new AreaRoadCurveData.SpotData(1, RoadConfigType.Straight, 2),
						new AreaRoadCurveData.SpotData(4, RoadConfigType.Straight, 1)),

					new AreaRoadCurveData(
						new AreaRoadCurveData.SpotData(0, RoadConfigType.InCorner, 2, 5, 0, false),
						new AreaRoadCurveData.SpotData(1, RoadConfigType.Straight, 2),
						new AreaRoadCurveData.SpotData(4, RoadConfigType.Straight, 1)),

					new AreaRoadCurveData(
						new AreaRoadCurveData.SpotData(0, RoadConfigType.InCorner, 2, 8, 0, false),
						new AreaRoadCurveData.SpotData(1, RoadConfigType.Straight, 2),
						new AreaRoadCurveData.SpotData(4, RoadConfigType.InCorner, 1)),

					new AreaRoadCurveData(
						new AreaRoadCurveData.SpotData(0, RoadConfigType.InCorner, 2, 5, 0, false),
						new AreaRoadCurveData.SpotData(1, RoadConfigType.Straight, 2),
						new AreaRoadCurveData.SpotData(4, RoadConfigType.InCorner, 1)),

					new AreaRoadCurveData(
						new AreaRoadCurveData.SpotData(0, RoadConfigType.InCorner, 2, 8, 0, false),
						new AreaRoadCurveData.SpotData(1, RoadConfigType.InCorner, 3),
						new AreaRoadCurveData.SpotData(4, RoadConfigType.InCorner, 1)),

					new AreaRoadCurveData(
						new AreaRoadCurveData.SpotData(0, RoadConfigType.InCorner, 2, 5, 0, false),
						new AreaRoadCurveData.SpotData(1, RoadConfigType.InCorner, 3),
						new AreaRoadCurveData.SpotData(4, RoadConfigType.InCorner, 1)),

					new AreaRoadCurveData(
						new AreaRoadCurveData.SpotData(0, RoadConfigType.InCorner, 2, 8, 0, false),
						new AreaRoadCurveData.SpotData(1, RoadConfigType.InCorner, 3),
						new AreaRoadCurveData.SpotData(4, RoadConfigType.Straight, 1)),

					new AreaRoadCurveData(
						new AreaRoadCurveData.SpotData(0, RoadConfigType.InCorner, 2, 5, 0, false),
						new AreaRoadCurveData.SpotData(1, RoadConfigType.InCorner, 3),
						new AreaRoadCurveData.SpotData(4, RoadConfigType.Straight, 1)),

					//90 degree turns (outer)
					new AreaRoadCurveData(
						new AreaRoadCurveData.SpotData(0, RoadConfigType.OutCorner, 0, 7, 0, false),
						new AreaRoadCurveData.SpotData(1, RoadConfigType.Straight, 0),
						new AreaRoadCurveData.SpotData(4, RoadConfigType.Straight, 3)),

					new AreaRoadCurveData(
						new AreaRoadCurveData.SpotData(0, RoadConfigType.OutCorner, 0, 6, 0, false),
						new AreaRoadCurveData.SpotData(1, RoadConfigType.Straight, 0),
						new AreaRoadCurveData.SpotData(4, RoadConfigType.Straight, 3)),

					new AreaRoadCurveData(
						new AreaRoadCurveData.SpotData(0, RoadConfigType.OutCorner, 0, 7, 0, false),
						new AreaRoadCurveData.SpotData(1, RoadConfigType.OutCorner, 1),
						new AreaRoadCurveData.SpotData(4, RoadConfigType.OutCorner, 3)),

					new AreaRoadCurveData(
						new AreaRoadCurveData.SpotData(0, RoadConfigType.OutCorner, 0, 6, 0, false),
						new AreaRoadCurveData.SpotData(1, RoadConfigType.OutCorner, 1),
						new AreaRoadCurveData.SpotData(4, RoadConfigType.OutCorner, 3)),

					new AreaRoadCurveData(
						new AreaRoadCurveData.SpotData(0, RoadConfigType.OutCorner, 0, 7, 0, false),
						new AreaRoadCurveData.SpotData(1, RoadConfigType.Straight, 0),
						new AreaRoadCurveData.SpotData(4, RoadConfigType.OutCorner, 3)),

					new AreaRoadCurveData(
						new AreaRoadCurveData.SpotData(0, RoadConfigType.OutCorner, 0, 6, 0, false),
						new AreaRoadCurveData.SpotData(1, RoadConfigType.Straight, 0),
						new AreaRoadCurveData.SpotData(4, RoadConfigType.OutCorner, 3)),

					new AreaRoadCurveData(
						new AreaRoadCurveData.SpotData(0, RoadConfigType.OutCorner, 0, 7, 0, false),
						new AreaRoadCurveData.SpotData(1, RoadConfigType.OutCorner, 1),
						new AreaRoadCurveData.SpotData(4, RoadConfigType.Straight, 3)),

					new AreaRoadCurveData(
						new AreaRoadCurveData.SpotData(0, RoadConfigType.OutCorner, 0, 6, 0, false),
						new AreaRoadCurveData.SpotData(1, RoadConfigType.OutCorner, 1),
						new AreaRoadCurveData.SpotData(4, RoadConfigType.Straight, 3)),

					//45 degree turns, from the reference of the straight edge
					new AreaRoadCurveData(
						new AreaRoadCurveData.SpotData(0, RoadConfigType.Straight, 2, 13, 2, true),
						new AreaRoadCurveData.SpotData(1, RoadConfigType.InCorner, 3, 12, 1, true)),

					new AreaRoadCurveData(
						new AreaRoadCurveData.SpotData(0, RoadConfigType.Straight, 0, 13, 0, false),
						new AreaRoadCurveData.SpotData(1, RoadConfigType.InCorner, 0, 12, 0, false)),

					new AreaRoadCurveData(
						new AreaRoadCurveData.SpotData(0, RoadConfigType.Straight, 2, 10, 0, false),
						new AreaRoadCurveData.SpotData(1, RoadConfigType.OutCorner, 2, 11, 0, false)),

					new AreaRoadCurveData(
						new AreaRoadCurveData.SpotData(0, RoadConfigType.Straight, 0, 10, 2, true),
						new AreaRoadCurveData.SpotData(1, RoadConfigType.OutCorner, 1, 11, 1, true)),

					//45 degree turns, from the reference of the corner
					new AreaRoadCurveData(
						new AreaRoadCurveData.SpotData(3, RoadConfigType.Straight, 2, 13, 2, true),
						new AreaRoadCurveData.SpotData(0, RoadConfigType.InCorner, 3, 12, 1, true)),

					new AreaRoadCurveData(
						new AreaRoadCurveData.SpotData(3, RoadConfigType.Straight, 0, 13, 0, false),
						new AreaRoadCurveData.SpotData(0, RoadConfigType.InCorner, 0, 12, 0, false)),

					new AreaRoadCurveData(
						new AreaRoadCurveData.SpotData(3, RoadConfigType.Straight, 2, 10, 0, false),
						new AreaRoadCurveData.SpotData(0, RoadConfigType.OutCorner, 2, 11, 0, false)),

					new AreaRoadCurveData(
						new AreaRoadCurveData.SpotData(3, RoadConfigType.Straight, 0, 10, 2, true),
						new AreaRoadCurveData.SpotData(0, RoadConfigType.OutCorner, 1, 11, 1, true)),
				};
			}

			return cachedRoadCurveData;
		}

		private AreaRoadData[] GetRoadData()
		{
			if (cachedRoadData == null)
			{
				cachedRoadData = new AreaRoadData[16];

				// no road (plain terrain)
				cachedRoadData[0] = new AreaRoadData (RoadConfigType.Empty, false, false, false, false, 1, 0);

				// road in a single corner (inner turn edge)
				cachedRoadData[1] = new AreaRoadData (RoadConfigType.InCorner, true, false, false, false, 4, 0);
				cachedRoadData[2] = new AreaRoadData (RoadConfigType.InCorner, false, true, false, false, 4, 1);
				cachedRoadData[3] = new AreaRoadData (RoadConfigType.InCorner, false, false, true, false, 4, 3);
				cachedRoadData[4] = new AreaRoadData (RoadConfigType.InCorner, false, false, false, true, 4, 2);

				// road in two corners (straight road edge)
				cachedRoadData[5] = new AreaRoadData (RoadConfigType.Straight, true, true, false, false, 2, 0);
				cachedRoadData[6] = new AreaRoadData (RoadConfigType.Straight, true, false, true, false, 2, 3);
				cachedRoadData[7] = new AreaRoadData (RoadConfigType.Straight, false, false, true, true, 2, 2);
				cachedRoadData[8] = new AreaRoadData (RoadConfigType.Straight, false, true, false, true, 2, 1);

				// road in two corners (diagonal passage)
				cachedRoadData[9] = new AreaRoadData (RoadConfigType.BiDiagonal, true, false, true, false, 9, 0);
				cachedRoadData[10] = new AreaRoadData (RoadConfigType.BiDiagonal, false, true, false, true, 9, 1);

				// road in three corners (outer turn edge)
				cachedRoadData[11] = new AreaRoadData (RoadConfigType.OutCorner, true, false, true, true, 3, 3);
				cachedRoadData[12] = new AreaRoadData (RoadConfigType.OutCorner,false, true, true, true, 3, 2);
				cachedRoadData[13] = new AreaRoadData (RoadConfigType.OutCorner,true, true, false, true, 3, 1);
				cachedRoadData[14] = new AreaRoadData (RoadConfigType.OutCorner,true, true, true, false, 3, 0);

				// road in all corners (plain asphalt)
				cachedRoadData[15] = new AreaRoadData (RoadConfigType.Full, true, true, true, true, 0, 0);
			}

			return cachedRoadData;
		}

		private AreaRoadData GetRoadDataForPoint(AreaVolumePoint pointAbove)
		{
			var roadData = GetRoadData();

            bool[] config = new bool[4];
            for (int a = 0; a < 4; ++a)
            {
                AreaVolumePoint pointInRoadConfiguration = pointAbove.pointsInSpot[a + 4];
                config[a] = pointInRoadConfiguration?.road??false;
            }

            int dataIndex = -1;
            for (int a = 0; a < roadData.Length; ++a)
            {
                bool[] candidate = roadData[a].configurationAsArray;
                bool equal = true;
                for (int b = 0; b < 4; ++b)
                {
                    if (config[b] != candidate[b])
                    {
                        equal = false;
                        break;
                    }
                }

                if (equal)
                {
                    dataIndex = a;
                    break;
                }
            }

            if (dataIndex <= -1)
	            return null;

            return roadData[dataIndex];
		}

        public enum RoadSubtype
        {
            GrassDirt = 0,
            GrassCurb = 10,
            ConcreteCurb = 20,
            TileCurb = 30,
        }

        private void UpdateRoadConfigurations (AreaVolumePoint pointStart, RoadSubtype subType)
        {
	        if (pointStart.pointState != AreaVolumePointState.Full)
            {
                Debug.Log ("AM (I) | EditRoad | Selected starting point is not full, aborting");
                return;
            }

            for (int i = 0; i < 4; ++i)
            {
                AreaVolumePoint pointAbove = pointStart.pointsWithSurroundingSpots[i];
                if (pointAbove == null)
                {
                    Debug.Log ("AM (I) | EditRoad | One of the points (" + i + ") above the starting one is null, aborting");
                    return;
                }

                if (pointAbove.spotConfiguration != (byte)15)
                {
                    Debug.Log ("AM (I) | EditRoad | One of the points (" + i + ") above has non-00001111 configuration, aborting");
                    return;
                }

                AreaRoadData data = GetRoadDataForPoint(pointAbove);
				if(data == null)
				{
					Debug.Log ("AM (I) | EditRoad | One of the spots above (" + i + ") has a configuration that could not be found.");
					return;
				}

                if (pointAbove.blockTileset != AreaTilesetHelper.database.tilesetRoad.id || pointAbove.blockGroup != data.usedGroup || pointAbove.blockRotation != data.usedRotation)
                {
                    // Debug.Log ("AM (I) | EditRoad | Updating block " + pointAbove.spotIndex + " using road " + dataIndex);
                    pointAbove.blockTileset = AreaTilesetHelper.database.tilesetRoad.id;
                    pointAbove.blockGroup = data.usedGroup;
                    pointAbove.blockSubtype = (byte)(int)subType;
                    pointAbove.blockFlippedHorizontally = false;
                    pointAbove.blockRotation = data.usedRotation;
                    RebuildBlock (pointAbove, false);
                }
            }
        }

        #if PB_MODSDK
        public static bool IsSpotInterior (AreaVolumePoint spot) =>
            spot != null
            && spot.spotPresent
            && spot.pointState == AreaVolumePointState.Full
            && spot.spotConfiguration == TilesetUtility.configurationFull
            && spot.blockTileset == 0;

        public static bool IsSpotRoad (AreaVolumePoint spot) => spot != null && spot.blockTileset == AreaTilesetHelper.idOfRoad;

        public void ApplyPropVisibilityEverywhere (int cutoffLayer)
        {
            // This is the counterpart for props to hiding/showing blocks during layer editing.
            // Any props above layer will be hidden, those on or below layer will be shown.

            var hideStop = boundsFull.x * boundsFull.z * cutoffLayer;
            for (var i = 0; i < points.Count; i += 1)
            {
                if (!indexesOccupiedByProps.TryGetValue (i, out var placements))
                {
                    continue;
                }

                var visible = i >= hideStop;
                var halfValues = visible ? propVisible : propInvisible;
                foreach (var placement in placements)
                {
                    if (AreaAssetHelper.propsHiddenWithECS.Contains (placement.prototype.id))
                    {
                        placement.UpdateVisibilityWithECS (visible, componentTypeModel);
                        continue;
                    }
                    placement.UpdateVisibility(halfValues);
                }
            }
        }

        static readonly HalfVector4 propVisible = new HalfVector4(1f, 0f, 1f, 1f);
        static readonly HalfVector4 propInvisible = new HalfVector4(0f, 1f, 1f, 1f);
        
        public static bool ignoreUnresolvedTilesetOnLoad;
        #endif

        #endif // UNITY_EDITOR
        #endregion // Editor
        
        // Toggle to quickly switch between the old and the new terrain mesh algorithms.
        [NonSerialized]
        public bool useNewTerrainMeshAlgorithm = true;
    }
}
