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

        public bool displayOnlyVolume = false;
        public bool displayProps = true;

        public Mesh fallbackMesh;
        public Material fallbackMaterial;

        [NonSerialized]
        public bool sliceEnabled = false;

        [NonSerialized]
        public int sliceDepth = 0;

        [NonSerialized]
        public float sliceColliderHeightBias = -1.5f;

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

        [NonSerialized]
        public Dictionary<int, AreaDataNavOverride> navOverridesSaved = new Dictionary<int, AreaDataNavOverride> ();

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

        private static Entity[] pointEntitiesMain;
        private static Entity[] pointEntitiesInterior;

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
            componentTypeFrozen = typeof (Frozen);
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

            #if !PB_MODSDK
            distanceSort += SortByDistance;
            #endif
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
            #if !PB_MODSDK
            DestroyImmediate (GetHolderSimulatedParent ().gameObject);
            DestroyImmediate (GetHolderSimulatedLeftovers ().gameObject);
            #endif

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

            #if !PB_MODSDK
            if (isolatedStructureSimRequested)
            {
                isolatedStructureSimRequested = false;
                SimulateIsolatedStructures ();
            }
            #endif
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

            UpdateVolume (false);

            UpdateAllSpots (false);

            RebuildAllBlocks ();

            RebuildAllBlockDamage ();

            ExecuteAllPropPlacements ();

            ApplyShaderPropertiesEverywhere ();

            RebuildCollisions ();

            UpdateDestructionFlags ();

            // UpdateStructureAnalysis ();

            GenerateNavOverrides ();

            UpdateShaderGlobals ();

            UpdateTerrain (terrainUsed, true);

            UpdateDestructionCounters ();
        }

        private void UpdateDestructionFlags ()
        {
            // Refresh indirect damage flags
            for (var i = 0; i < points.Count; i += 1)
            {
                var point = points[i];
                if (point == null)
                    continue;

                var check = point.pointState != AreaVolumePointState.Empty | point.blockTileset != 0;
                point.indestructibleIndirect = check && IsPointIndestructible
                (
                    point,
                    false,
                    true,
                    true,
                    true,
                    true
                );
            }
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
                if (point.indestructibleAny)
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
            if (sceneHelper != null)
            {
                var terrain = sceneHelper.terrain;

                if (activationAllowed && terrainUsed != terrain.gameObject.activeSelf)
                    terrain.gameObject.SetActive (terrainUsed);

                if (rebuildRequired)
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
            boundsFull = new Vector3Int (Mathf.Max (boundsFull.x, 2), Mathf.Max (boundsFull.y, 2), Mathf.Max (boundsFull.z, 2));
            var volumeLength = boundsFull.x * boundsFull.y * boundsFull.z;

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

                points = new List<AreaVolumePoint> (volumeLength);
                areaChangeTracker += 1;

                var instanceOffset = -AreaUtility.spotOffset;
                for (var i = 0; i < volumeLength; i += 1)
                {
                    var point = new AreaVolumePoint ()
                    {
                        spotIndex = i,
                        pointPositionIndex = TilesetUtility.GetVolumePositionFromIndex (i, boundsFull),
                    };
                    point.pointPositionLocal = new Vector3
                    (
                        point.pointPositionIndex.x,
                        -point.pointPositionIndex.y,
                        point.pointPositionIndex.z
                    ) * TilesetUtility.blockAssetSize;
                    point.instancePosition = point.pointPositionLocal + instanceOffset;
                    point.pointState = point.pointPositionIndex.y < boundsFull.y - 2
                        ? point.pointState = AreaVolumePointState.Empty
                        : point.pointState = AreaVolumePointState.Full;

                    points.Add (point);
                }

                for (var i = 0; i < points.Count; i += 1)
                {
                    var scenePoint = points[i];
                    var neighbourIndexes = AreaUtility.GetNeighbourIndexesInXxYxZ (i, Vector3Int.size2x2x2, Vector3Int.size0x0x0, boundsFull);
                    if (scenePoint.pointsInSpot == null)
                    {
                        scenePoint.pointsInSpot = new AreaVolumePoint[8];
                        scenePoint.pointsInSpot[WorldSpace.Compass.PointSelf] = scenePoint;
                    }
                    var stop = scenePoint.pointsInSpot.Length;
                    for (var n = 1; n < stop; n += 1)
                    {
                        var neighbourIndex = neighbourIndexes[n];
                        if (neighbourIndex != AreaUtility.invalidIndex)
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
                        scenePoint.pointsWithSurroundingSpots[WorldSpace.Compass.SpotSelf] = scenePoint;
                    }
                    stop = scenePoint.pointsWithSurroundingSpots.Length - 1;
                    for (var n = 0; n < stop; n += 1)
                    {
                        var neighbourIndex = neighbourIndexes[n];
                        scenePoint.pointsWithSurroundingSpots[n] = neighbourIndex != AreaUtility.invalidIndex ? points[neighbourIndex] : null;
                    }
                }
            }
        }




        public void UpdateAllSpots (bool triggerNavigationUpdate)
        {
            for (int i = 0; i < points.Count; ++i)
                UpdateSpotAtPoint (points[i], triggerNavigationUpdate);
        }

        #if UNITY_EDITOR
        public void UpdateSpotsAroundIndex (int index, bool triggerNavigationUpdate, bool log = false)
        {
            if (!index.IsValidIndex (points))
            {
                if (log)
                    Debug.Log ("AM | UpdateSpotsAroundIndex | Index " + index + " is not valid for point collection sized at " + points.Count);
                return;
            }

            var point = points[index];
            for (int i = 0; i < 8; ++i)
            {
                var pointAround = point.pointsWithSurroundingSpots[i];
                UpdateSpotAtPoint (pointAround, triggerNavigationUpdate);
            }
        }
        #endif

        private void UpdateSpotsAroundPoint (AreaVolumePoint point, bool triggerNavigationUpdate)
        {
            for (int i = 0; i < 8; ++i)
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
                return;

            if (log)
                Debug.Log ($"AM | UpdateSpotAtIndex | Starting spot update on point {point.spotIndex} with current configuration {point.spotConfiguration}");

            if (!point.spotPresent)
            {
                if (log)
                    Debug.Log ("AM | UpdateSpotAtIndex | Point has no spot, aborting");
                return;
            }

            var configurationOld = point.spotConfiguration;
            var configurationWithDamageLast = point.spotConfigurationWithDamage;
            byte configurationByte = 0;
            byte configurationDamaged = 0;
            int damagedNeighbours = 0;

            for (var i = 0; i < point.pointsInSpot.Length; i += 1)
            {
                configurationByte <<= 1;
                configurationDamaged <<= 1;

                var pointInSpot = point.pointsInSpot[AreaUtility.configurationIndexRemapping[i]];
                if (pointInSpot == null)
                    continue;

                var psi = (int)pointInSpot.pointState;
                var damaged = psi & (int)AreaVolumePointState.FullDestroyed;
                var full = psi / (int)AreaVolumePointState.Full;
                configurationByte |= (byte)(damaged | full);
                configurationDamaged |= (byte)full;

                damagedNeighbours += damaged;
            }

            var spotHasDamagedPoints = damagedNeighbours > 0;
            if (spotHasDamagedPoints && !point.spotHasDamagedPoints)
            {
                point.instanceVisibilityInterior = 1f;

                // if (point.spotConfiguration == AreaNavUtility.configFull)
                //     Debug.LogWarning ($"Point {point.spotIndex} has detected damage on spot");

                #if !PB_MODSDK
                if (Application.isPlaying && IDUtility.IsGameState (GameStates.combat))
                    CombatReplayHelper.OnLevelInteriorReveal (point.spotIndex);
                #endif
            }

            point.spotConfiguration = configurationByte;
            point.spotConfigurationWithDamage = spotHasDamagedPoints ? configurationDamaged : point.spotConfiguration;
            point.spotHasDamagedPoints = spotHasDamagedPoints;

            if (resetSubtypeOnChange && configurationByte != configurationOld)
            {
                point.blockFlippedHorizontally = false;
                point.blockGroup = 0;
                point.blockSubtype = 0;
            }

            if (!triggerNavigationUpdate)
                return;
            if (!Application.isPlaying)
                return;
            #if !PB_MODSDK
            Contexts.sharedInstance.combat.isPathRescanRequest = configurationWithDamageLast == AreaNavUtility.configFloor
                & configurationWithDamageLast != point.spotConfigurationWithDamage;
            #endif
        }




        public void RebuildAllBlocks ()
        {
            if (AreaTilesetHelper.database.tilesets == null || AreaTilesetHelper.database.tilesets.Count == 0)
                return;

            for (int i = 0; i < points.Count; ++i)
            {
                if (points[i] != null)
                    RebuildBlock (points[i]);
            }
        }

        #if UNITY_EDITOR
        public void RebuildBlocksAroundIndex (int index)
        {
            if (!index.IsValidIndex (points))
                return;

            var point = points[index];
            for (int i = 0; i < point.pointsWithSurroundingSpots.Length; ++i)
            {
                var pointNearby = point.pointsWithSurroundingSpots[i];
                if (pointNearby != null)
                    RebuildBlock (pointNearby);
            }
        }
        #endif

        private void RebuildBlocksAroundPoint (AreaVolumePoint point)
        {
            for (int i = 0; i < point.pointsWithSurroundingSpots.Length; ++i)
            {
                var pointNearby = point.pointsWithSurroundingSpots[i];
                if (pointNearby != null)
                    RebuildBlock (pointNearby);
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
        public bool IsPointTerrain (AreaVolumePoint point)
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

            float offset = point.terrainOffset * TilesetUtility.blockAssetSize;

            return new Vector3
            (
                point.instancePosition.x - TilesetUtility.blockAssetHalfSize,
                point.instancePosition.y + offset,
                point.instancePosition.z - TilesetUtility.blockAssetHalfSize
            );
        }

        private GameObject r_RB_InteriorObject;
        private GameObject r_RB_MainObject;

        private GameObject r_RB_BlockPrefab;

        private Vector3 r_RB_FixedScale = Vector3.one;


        public void RebuildBlock (AreaVolumePoint point, bool log = false, bool applyShaderProperties = true)
        {
            var tilesets = AreaTilesetHelper.database.tilesets;
            if (tilesets == null || tilesets.Count == 0)
            {
                if (log)
                    Debug.Log ("AM | RebuildBlockAtIndex | Aborting due to tilesets not being found");
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
                ClearInstancedModel (pointEntitiesInterior[point.spotIndex]);
                ClearInstancedModel (pointEntitiesMain[point.spotIndex]);
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
                ClearInstancedModel (pointEntitiesInterior[point.spotIndex]);
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

                bool definitionFound = definition != null;
                if (definitionFound)
                {
                    bool groupPresent = definition.subtypeGroups.ContainsKey (group);
                    if (!groupPresent && definition.subtypeGroups != null)
                    {
                        groupSafe = definition.subtypeGroups.Keys.First ();
                        Debug.Log ($"Group swapped from {group} to {groupSafe} which is first group key out of {definition.subtypeGroups.Count}");
                    }

                    bool subtypePresent = groupPresent && definition.subtypeGroups[group].ContainsKey (subtype);
                    if (!subtypePresent && definition.subtypeGroups != null && definition.subtypeGroups.ContainsKey (group))
                    {
                        subtypeSafe = groupPresent ? definition.subtypeGroups[group].Keys.First () : (byte)0;
                        Debug.Log ($"Subtype swapped from {subtype} to {subtypeSafe} which is first group key out of {definition.subtypeGroups[group].Count}");
                    }
                }
                else
                {
                    tilesetSafe = AreaTilesetHelper.database.tilesetFallback.id;
                    groupSafe = 0;
                    subtypeSafe = 0;
                    Debug.LogWarning ($"Definition for config {cfg} ({TilesetUtility.GetStringFromConfiguration (cfg)}) was null in tileset {tilesetMain.name}, fallback tileset {AreaTilesetHelper.idOfFallback}");

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
                    Debug.LogWarning ($"Definition for config {cfg} ({TilesetUtility.GetStringFromConfiguration (cfg)}) was null in tileset {tilesetMain.name}, fallback used");
                    // ClearInstancedModel (pointEntitiesMain, point);

                    DrawHighlightSpot (point);
                }
            }
            else
                ClearInstancedModel (pointEntitiesMain[point.spotIndex]);

            if (applyShaderProperties)
                ApplyShaderPropertiesAtPoint (point, true, true, true);
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
                    Debug.Log ($"Skipping point due to ECS safety check");
                return;
            }

            EntityManager entityManager = world.EntityManager;
            Entity entity = entityList[point.spotIndex];

            if (entity == Entity.Null)
            {
                // Debug.Log ($"Point {point.spotIndex} has registered a new interior model");
                entityList[point.spotIndex] = CreatePointInteriorEntity (entityManager, archetype, point.instancePosition, quaternion.identity, point.spotIndex);
                entity = entityList[point.spotIndex];
                point.instanceVisibilityInterior = 1f;

                #if !PB_MODSDK
                if (Application.isPlaying && IDUtility.IsGameState (GameStates.combat))
                    CombatReplayHelper.OnLevelInteriorReveal (point.spotIndex);
                #endif
            }

            bool invalidVariantDetected = false;
            bool verticalFlip = false;

            var model = AreaTilesetHelper.GetInstancedModel
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
                out var lightData
            );

            if (invalidVariantDetected)
            {
                point.blockFlippedHorizontally = false;
                point.blockGroup = 0;
                point.blockSubtype = 0;
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

            #if !PB_MODSDK
            if (point.simulatedChunkPresent)
                FreeEntityForSimulation (ref entityManager, entity, point);
            else if (entityManager.HasComponent (entity, componentTypeFrozen))
                entityManager.RemoveComponent (entity, componentTypeFrozen);
            #else
            if (entityManager.HasComponent (entity, componentTypeFrozen))
                entityManager.RemoveComponent (entity, componentTypeFrozen);
            #endif

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

        private static void ClearInstancedModel (Entity entity)
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

        #if UNITY_EDITOR
        public static bool InstancedModelExists (int index, bool interior = false)
        {
            if (world == null)
                return false;
            var entityManager = world.EntityManager;
            if (entityManager == null)
                return false;

            var entityList = interior ? pointEntitiesInterior : pointEntitiesMain;
            if (!index.IsValidIndex((entityList)))
                return false;
            var entity = entityList[index];
            return entity != Entity.Null && entityManager.HasComponent<InstancedMeshRenderer> (entity);
        }
        #endif

        #if !PB_MODSDK
        private void FreeEntityForSimulation (ref EntityManager entityManager, Entity entity, AreaVolumePoint point)
        {
            if (!point.simulatedChunkPresent)
                return;

            if (!entityManager.HasComponent (entity, componentTypeModel))
                return;

            AddPropertyAnimation (point);

            // Tag component just in case we'll want to filter to check these out
            var blockSimulated = new ECS.BlockSimulated { indexOfHelper = point.simulatedChunkComponent.id };
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
            Vector3 positionOfHelperCurrent = point.simulatedChunkComponent.transform.position;
            Vector3 positionOfHelperInitial = point.simulatedChunkComponent.initialPosition;
            Vector3 positionOfPointInitial = point.pointPositionLocal;
            Vector3 positionDifferenceInitial = point.pointPositionLocal - point.simulatedChunkComponent.initialPosition;
            // Vector3 positionDifferenceLocalToWorld = point.simulatedHelper.transform.TransformPoint (positionDifferenceInitial);
            // Vector3 positionDifferenceWorldToLocal = point.simulatedHelper.transform.InverseTransformPoint (positionDifferenceInitial);

            if (entityManager.HasComponent (entity, componentTypeParent))
            {
                Entity parent = entityManager.GetComponentData<Parent> (entity).Value;
                if (parent.Index == point.simulatedChunkComponent.entityRoot.Index)
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
                entityManager.SetComponentData (entity, new Parent { Value = point.simulatedChunkComponent.entityRoot });
            else
                entityManager.AddComponentData (entity, new Parent { Value = point.simulatedChunkComponent.entityRoot });

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

                CombatReplayHelper.OnSimulatedStructures
                (
                    simulatedHelper.id,
                    simulatedHelper.transform.position,
                    simulatedHelper.transform.rotation,
                    simulatedHelper.pointsAffected
                );
            }

            UtilityECS.ScheduleUpdate ();
        }

        #endif

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

            CheckScenePointsDestroyed (pointsDestroyedAll);
        }

        #if UNITY_EDITOR
        public void UpdateDamageAroundIndex (int index)
        {
            if (!index.IsValidIndex (points))
                return;
            var pointOrigin = points[index];
            UpdateDamageAroundPoint (pointOrigin);
        }
        #endif

        private List<AreaVolumePoint> pointsDestroyedLocal = new List<AreaVolumePoint> (8);

        private void UpdateDamageAroundPoint (AreaVolumePoint pointOrigin)
        {
            if (AreaTilesetHelper.database.tilesets == null || AreaTilesetHelper.database.tilesets.Count == 0)
                return;

            if (!IsECSSafe ())
                return;

            // First, we remove previously instantiated damage objects and update the damage related field on each point
            // If a spot linked to a point has at least one participating point that is damaged, we'll want to redraw that spot

            pointsDestroyedLocal.Clear ();

            for (int i = 0; i < 8; ++i)
            {
                var point = pointOrigin.pointsWithSurroundingSpots[i];
                if (point == null)
                    continue;

                point.RecheckRubbleHosting ();

                if (point.pointState == AreaVolumePointState.FullDestroyed)
                    pointsDestroyedLocal.Add (point);

                if (!point.spotPresent)
                    continue;

                point.RecheckDamage ();
            }

            for (int i = 0; i < 8; ++i)
            {
                var point = pointOrigin.pointsWithSurroundingSpots[i];
                if (point == null || !point.spotPresent)
                    continue;

                ApplyShaderPropertiesAtPoint (point, ShaderOverwriteMode.None, false, false, true);
                if (indexesOccupiedByProps.TryGetValue (point.spotIndex, out var placements))
                {
                    for (int p = 0; p < placements.Count; ++p)
                    {
                        var placement = placements[p];
                        AreaAnimationSystem.OnRemoval (placement, point.spotHasDamagedPoints);
                    }
                }
            }

            CheckScenePointsDestroyed (pointsDestroyedLocal);
        }

        private void CheckScenePointsDestroyed (List<AreaVolumePoint> scenePointsDestroyed)
        {
            if (scenePointsDestroyed == null)
                return;

            for (int p = 0; p < scenePointsDestroyed.Count; ++p)
            {
                AreaVolumePoint pointDestroyed = scenePointsDestroyed[p];
                if (pointDestroyed == null)
                    continue;

                AreaVolumePoint pointForRubble = AreaUtility.GetRubblePointBelow (pointDestroyed.instancePosition, points, boundsFull); // AreaUtility.GetRubblePointBelow (pointDestroyed, 0);
                OnEnvironmentDestruction (pointDestroyed, pointForRubble);
            }
        }

        public void OnEnvironmentDestruction (AreaVolumePoint pointDestroyed, AreaVolumePoint pointBelow)
        {
            if (!Application.isPlaying)
                return;

            if (pointDestroyed == null || pointBelow == null)
                return;

            if (pointBelow.propsRubble != null)
                return;

            pointBelow.propsRubble = new List<int> (2);
            var dominantTileset = GetClosestTileset (pointDestroyed);
            if (logTilesetSearch)
                Debug.Log ($"Destruction of point {pointDestroyed.spotIndex} would use props {dominantTileset.propIDDebrisClumps.ToStringFormatted ()} (clump) and {dominantTileset.propIDDebrisPile} (pile) from tileset {dominantTileset.name}");

            int randomIndex = UnityEngine.Random.Range (0, dominantTileset.propIDDebrisClumps.Count);
            int propIDDebrisClump = dominantTileset.propIDDebrisClumps[randomIndex];
            var prototypeDebrisClump = AreaAssetHelper.GetPropPrototype (propIDDebrisClump);
            if (prototypeDebrisClump != null)
            {
                var placement = new AreaPlacementProp ();

                placement.id = prototypeDebrisClump.id;
                placement.pivotIndex = pointBelow.spotIndex;

                if (!indexesOccupiedByProps.ContainsKey (pointBelow.spotIndex))
                    indexesOccupiedByProps.Add (pointBelow.spotIndex, new List<AreaPlacementProp> ());

                indexesOccupiedByProps[pointBelow.spotIndex].Add (placement);
                placementsProps.Add (placement);
                ExecutePropPlacement (placement, true, true, false);

                var position = pointBelow.pointPositionLocal + new Vector3 (UnityEngine.Random.Range (-1f, 1f), 2.1f, UnityEngine.Random.Range (-1f, 1f));
                Debug.DrawLine (pointBelow.pointPositionLocal, position, Color.cyan);
                Debug.DrawLine (pointBelow.pointPositionLocal, pointBelow.instancePosition, pointBelow.spotPresent ? Color.green : Color.red);

                var rotation = UnityEngine.Random.rotationUniform;
                var scale = Vector3.one * UnityEngine.Random.Range (1.6f, 1.9f);

                placement.OverrideAndApplyTransformations (position, rotation, scale);
                AreaAnimationSystem.OnReveal (placement, true, 0.5f, Vector3.up);
            }
            else
                Debug.LogWarning ($"Failed to find a debris clump prop with ID {propIDDebrisClump} for tileset {dominantTileset.name} destruction");

            var prototypeDebrisPile = AreaAssetHelper.GetPropPrototype (dominantTileset.propIDDebrisPile);
            if (prototypeDebrisPile != null)
            {
                var placement = new AreaPlacementProp ();

                placement.id = prototypeDebrisPile.id;
                placement.pivotIndex = pointBelow.spotIndex;

                if (!indexesOccupiedByProps.ContainsKey (pointBelow.spotIndex))
                    indexesOccupiedByProps.Add (pointBelow.spotIndex, new List<AreaPlacementProp> ());

                indexesOccupiedByProps[pointBelow.spotIndex].Add (placement);
                placementsProps.Add (placement);
                ExecutePropPlacement (placement, true, true, false);

                var position = pointBelow.pointPositionLocal + new Vector3 (UnityEngine.Random.Range (-1f, 1f), 1.51f, UnityEngine.Random.Range (-1f, 1f));
                var rotation = Quaternion.Euler (0f, UnityEngine.Random.Range (0f, 360f), 0f);
                var scale = Vector3.one * UnityEngine.Random.Range (0.9f, 1.2f);

                placement.OverrideAndApplyTransformations (position, rotation, scale);
                AreaAnimationSystem.OnReveal (placement, true, 0.5f, Vector3.down);
            }
            else
                Debug.LogWarning ($"Failed to find a debris pile prop with ID {dominantTileset.propIDDebrisPile} for tileset {dominantTileset.name} destruction");

            #if !PB_MODSDK
            var fxEnvironmentFireChance = DataShortcuts.sim.environmentFireChance;
            if (fxEnvironmentFireChance > 0f && pointBelow.instanceFire == null)
            {
                float fxEnvironmentFireRoll = UnityEngine.Random.Range (0f, 1f);
                if (fxEnvironmentFireRoll < fxEnvironmentFireChance)
                {
                    var fxPosOffset = new Vector3 (UnityEngine.Random.Range (-0.75f, 0.75f), 1.51f, UnityEngine.Random.Range (-0.75f, 0.75f));
                    var fxPos = pointBelow.pointPositionLocal + fxPosOffset;
                    var fxDir = Vector3.up;
                    var fx = AssetPoolUtility.ActivateInstance (DataShortcuts.sim.environmentFireAsset, fxPos, fxDir);
                    pointBelow.instanceFire = fx;
                }
            }
            #endif
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

        private static readonly Vector3 colliderSize = Vector3.one * (TilesetUtility.blockAssetSize + AreaUtility.pointColliderInflation);

        public void RebuildCollisionForPoint (AreaVolumePoint avp)
        {
            if (!collisionsUsed)
                return;

            if (avp == null)
                return;

            #if !PB_MODSDK
            bool boxColliderUsed = avp.pointState == AreaVolumePointState.Full && !avp.simulatedChunkPresent;
            #else
            bool boxColliderUsed = avp.pointState == AreaVolumePointState.Full;
            #endif
            if (boxColliderUsed && Application.isPlaying)
            {
                // See if it's possible to skip collisions on terrain exclusive blocks (only at runtime to avoid level editor issues)
                bool terrainExclusive = true;
                for (int s = 0; s < 8; ++s)
                {
                    AreaVolumePoint pointWithNeighbourSpot = avp.pointsWithSurroundingSpots[s];

                    if (pointWithNeighbourSpot == null)
                        continue;

                    if (pointWithNeighbourSpot.spotConfiguration == AreaNavUtility.configEmpty || pointWithNeighbourSpot.spotConfigurationWithDamage == AreaNavUtility.configFull)
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
                    var ppi = avp.pointPositionIndex;
                    GameObject go = new GameObject ($"PC_Y{ppi.y:00}_X{ppi.x:000}_Z{ppi.z:000}");
                    go.hideFlags = HideFlags.DontSave;
                    go.transform.parent = GetHolderColliders ();
                    go.transform.localPosition = avp.pointPositionLocal;
                    go.layer = Constants.EnvironmentLayer;
                    avp.instanceCollider = go;

                    BoxCollider bc = go.AddComponent<BoxCollider> ();
                    bc.size = colliderSize;

                    if (visualizeCollisions && avp.pointPositionIndex.y < damageRestrictionDepth)
                    {
                        GameObject vis = PrimitiveHelper.CreatePrimitive (PrimitiveType.Cube, false);
                        vis.name = go.name + "_Vis";
                        vis.hideFlags = HideFlags.DontSave;
                        vis.transform.parent = go.transform;
                        vis.transform.localScale = Vector3.one * (TilesetUtility.blockAssetSize - 0.5f);
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

            #if !PB_MODSDK
            if (Application.isPlaying)
            {
                var combat = Contexts.sharedInstance.combat;
                combat.ReplaceDestructionCount (0);

                simulatedHelperCounter = 0;
                if (simulatedHelpers != null)
                {
                    foreach (var sim in simulatedHelpers)
                    {
                        if (sim != null)
                            DestroyImmediate (sim.gameObject);
                    }
                    simulatedHelpers.Clear ();
                }
            }

            pointsToAnimate.Clear ();
            #endif

            for (int i = 0; i < points.Count; ++i)
            {
                var point = points[i];

                // Necessary because this method is called from UpdateVolume, where volume points might not have been set up yet
                if (point == null)
                    continue;

                if (point.pointState == AreaVolumePointState.FullDestroyed)
                    point.pointState = AreaVolumePointState.Full;

                #if !PB_MODSDK
                if (Application.isPlaying)
                {
                    point.simulatedChunkComponent = null;
                    point.simulatedChunkPresent = false;
                    point.simulationRequested = false;
                    RemovePropertyAnimation (point);
                }
                #endif

                point.integrity = 1f;
                point.integrityForDestructionAnimation = 1f;
                point.RecheckRubbleHosting ();
            }

            #if !PB_MODSDK
            crashSpotIndexes.Clear ();
            #endif

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
            if (!IsECSSafe ())
                return;

            if (logShaderUpdates)
                Debug.Log ("Applying shader properties everywhere...");

            for (var i = 0; i < points.Count; i += 1)
            {
                AreaVolumePoint point = points[i];
                if (point == null)
                    continue;
                if (!point.spotPresent)
                    continue;
                if (point.spotConfiguration == AreaNavUtility.configEmpty | point.spotConfiguration == AreaNavUtility.configFull)
                    continue;
                ApplyShaderPropertiesAtPoint (point, true, true, true);
            }
        }

        public void ApplyShaderPropertiesToIndexes (List<int> indexes)
        {
            if (!IsECSSafe ())
                return;

            if (logShaderUpdates)
                Debug.Log ($"Applying shader properties to {indexes.Count} indexes...");

            for (int i = 0, count = indexes.Count; i < count; ++i)
            {
                var index = indexes[i];
                AreaVolumePoint point = points[index];
                if (point == null)
                    continue;
                if (!point.spotPresent)
                    continue;
                ApplyShaderPropertiesAtPoint (point, true, true, true);
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

            var vertexProps = point.customization;
            if (overwriteMode == ShaderOverwriteMode.All)
                vertexProps = vertexPropertiesSelected;
            else if (overwriteMode == ShaderOverwriteMode.AllExceptOverrideIndex)
            {
                vertexProps = vertexPropertiesSelected;
                vertexProps.overrideIndex = point.customization.overrideIndex;
            }
            point.customization = vertexProps;
            ApplyShaderPropertiesAtPoint (point, updateCore, updateIntegrity, updateDamage);
	    }

        #if UNITY_EDITOR
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
            point.customization = customization;
            ApplyShaderPropertiesAtPoint (point, updateCore, updateIntegrity, updateDamage);
        }
        #endif

        private void ApplyShaderPropertiesAtPoint
        (
            AreaVolumePoint point,
            bool updateCore,
            bool updateIntegrity,
            bool updateDamage
        )
        {
            if (!point.spotIndex.IsValidIndex (pointEntitiesMain))
                return;

            var customization = point.customization;
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

            float animatedIntegrityLowest = 1f;

            for (var i = 0; i < point.pointsInSpot.Length; i += 1)
            {
                var pointInSpot = point.pointsInSpot[i];
                if (pointInSpot == null || pointInSpot.pointState == AreaVolumePointState.Empty)
                    continue;

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

            // Handling of entity creation and clearing of mismatched entities
            if (entityCreationRequired || entityDeletionRequired)
            {
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
                }
            }

            // Next we tell the placement to finish setting up the prop (no point/config info needed further)
            if (!entityDeletionRequired)
                placement.Setup (this, point);

            UtilityECS.ScheduleUpdate ();
        }

        public void SnapPropRotation (AreaPlacementProp placement)
        {
            var configurationMask = AreaAssetHelper.GetPropMask (placement.prototype.prefab.compatibility);
            var point = points[placement.pivotIndex];

            if (point.spotPresent && placement.prototype.prefab.linkRotationToConfiguration)
            {
                placement.rotation = (byte)configurationMask.IndexOf (point.spotConfiguration);
                if (placement.flipped)
                    placement.rotation = (byte)((placement.rotation + 2) % 4);
            }

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

        #if !PB_MODSDK

        public Transform GetHolderSimulatedLeftovers ()
        {
            return UtilityGameObjects.GetTransformSafely (ref holderSimulatedLeftovers, holderSimulatedLeftoversName, HideFlags.DontSave, transform.localPosition, "SceneHolder");
        }

        public Transform GetHolderSimulatedParent ()
        {
            return UtilityGameObjects.GetTransformSafely (ref holderSimulatedParent, holderSimulatedParentName, HideFlags.DontSave, transform.localPosition, "SceneHolder");
        }

        public void ApplyDamageToPosition (Vector3 position, float impact, bool forceOverkillEffect = false, bool forceDestructible = false)
        {
            var point = GetPoint (position);
            if (point != null)
				ApplyDamageToPoint (point, impact, forceOverkillEffect, forceDestructible);
        }

        private Collider[] overlapColliders = new Collider[256];
        private List<ColliderDistanceSqr> collidersByDistance = new List<ColliderDistanceSqr> ();
        private Comparison<ColliderDistanceSqr> distanceSort;

        private struct ColliderDistanceSqr
        {
            public Collider collider;
            public float sqrDist;
        }

        private List<(AreaVolumePoint, float)> pointsImpactRadius = new List<(AreaVolumePoint, float)> ();
        private Dictionary<int, AreaVolumePoint> pointsDamaged = new Dictionary<int, AreaVolumePoint> ();
        private Dictionary<int, AreaVolumePoint> pointsDestructionUpdate = new Dictionary<int, AreaVolumePoint> ();
        private Dictionary<int, AreaVolumePoint> pointsDestroyedRadius = new Dictionary<int, AreaVolumePoint> ();

        private static int SortByDistance (ColliderDistanceSqr a, ColliderDistanceSqr b) => a.sqrDist.CompareTo (b.sqrDist);

        public int ApplyDamageToRadius (Vector3 origin, float impactDamage, float radius, float exponent, bool forceDestructible = false)
        {
            if (impactDamage <= 0f)
                return 0;

            var overlapCount = Physics.OverlapSphereNonAlloc (origin, radius, overlapColliders, LayerMasks.environmentMask);
            if (overlapCount == 0)
                return 0;

            if (radius <= 0f)
            {
                Debug.LogWarning ($"Radius of projectile splash impact was set to 0, make sure to correct this in the data");
                radius = 1f;
            }

            const int limit = 50;
            // if (overlapCount > limit)
            //     Debug.LogWarning ($"More than 50 points ({overlapCount}) damaged at once, consider reducing environment splash impact radius");

            float radiusSqr = radius * radius;
            bool draw = DataShortcuts.sim.environmentCollisionDebug;

            // Sorting overlaps by distance
            collidersByDistance.Clear ();
            for (var i = 0; i < overlapCount; i += 1)
            {
                var col = overlapColliders[i];
                var hitPosition = col.transform.position;
                var sqrDist = (hitPosition - origin).sqrMagnitude;
                collidersByDistance.Add (new ColliderDistanceSqr { collider = col, sqrDist = sqrDist });

                if (draw)
                    Debug.DrawLine (origin, hitPosition, Color.gray.WithAlpha (0.5f), 5);
            }

            collidersByDistance.Sort (distanceSort);
            if (overlapCount > limit)
                overlapCount = limit;

            for (var i = 0; i < overlapCount; i += 1)
                overlapColliders[i] = collidersByDistance[i].collider;

            pointsImpactRadius.Clear ();
            pointsDamaged.Clear ();
            pointsDestructionUpdate.Clear ();
            for (var i = 0; i < overlapCount; i += 1)
            {
                var col = overlapColliders[i];
                var hitPosition = col.transform.position;
                var hitPoint = GetPoint (hitPosition);

                var distanceSqr = Vector3.SqrMagnitude (origin - hitPosition);
                if (distanceSqr >= radiusSqr)
                    continue;

                var distanceNormalizedSqr = distanceSqr / radiusSqr;
                if (distanceNormalizedSqr >= 0.95f)
                    continue;

                var damageScaling = 1f - distanceNormalizedSqr;
                var impact = damageScaling * impactDamage;

                if (draw)
                {
                    var color = Color.Lerp (Color.yellow, Color.red, damageScaling);
                    Debug.DrawLine (origin, hitPosition, color, 5f);
                    DebugExtensions.DrawCube (hitPosition, Vector3.one * 0.25f, color, 5f);
                    var impactScale = DataLinkerSettingsArea.data.damageScalar;
                    Debug.Log ($"AOE {i:00}/{overlapCount} | Point: {hitPoint.spotIndex} ({hitPoint.integrity:0.##}) | Distance/radius (sqr.): {distanceSqr:0.##}/{radiusSqr:0.##} | Damage: x{damageScaling:0.##} ({impact:F0}/{impactDamage:F0}) | Norm. damage: -{(impact * impactScale):0.##}");
                }

                if (!TryUpdatePointIntegrity (hitPoint, impact, forceDestructible))
                    continue;

                if (hitPoint.integrity == 0f)
                {
                    pointsImpactRadius.Add ((hitPoint, impact));
                    pointsToAnimate.Add (hitPoint);
                }
                var neighbours = hitPoint.pointsWithSurroundingSpots;
                for (var j = 0; j < neighbours.Length; j += 1)
                {
                    var neighbour = neighbours[j];
                    if (neighbour == null)
                        continue;
                    var map = hitPoint.integrity > 0f ? pointsDamaged : pointsDestructionUpdate;
                    map[neighbour.spotIndex] = neighbour;
                }
            }

            // Two passes on destruction candidates.
            // First pass evaluates all the spots to determine what has been damaged.
            // Second pass rebuilds all the blocks with updated damage info.
            pointsDestroyedRadius.Clear ();
            foreach (var k in pointsDestructionUpdate.Keys)
            {
                var point = pointsDestructionUpdate[k];
                AddPropertyAnimation (point);
                UpdateSpotAtPoint (point, false);
                point.RecheckRubbleHosting ();
                if (point.pointState == AreaVolumePointState.FullDestroyed)
                    pointsDestroyedRadius[point.spotIndex] = point;
                if (point.spotPresent)
                    point.RecheckDamage ();
            }

            foreach (var k in pointsDestructionUpdate.Keys)
            {
                var point = pointsDestructionUpdate[k];
                if (point.spotPresent && indexesOccupiedByProps.TryGetValue (point.spotIndex, out var placements))
                {
                    for (var p = 0; p < placements.Count; p += 1)
                    {
                        var placement = placements[p];
                        AreaAnimationSystem.OnRemoval (placement, point.spotHasDamagedPoints);
                    }
                }
                RebuildBlock (point);
                pointsDamaged.Remove (point.spotIndex);
            }

            foreach (var k in pointsDestroyedRadius.Keys)
            {
                var point = pointsDestroyedRadius[k];
                var pointForRubble = AreaUtility.GetRubblePointBelow (point.instancePosition, points, boundsFull);
                OnEnvironmentDestruction (point, pointForRubble);
            }

            for (var i = 0; i < pointsImpactRadius.Count; i += 1)
            {
                var (point, impact) = pointsImpactRadius[i];
                ApplyDestructionToPoint (point, impact, false, audioFX: i == 0);
                ScanForDisconnect (point);
            }

            foreach (var k in pointsDamaged.Keys)
            {
                var point = pointsDamaged[k];
                ApplyShaderPropertiesAtPoint (point, ShaderOverwriteMode.None, false, true, false);
            }

            return overlapCount;
        }

        #endif

        public AreaVolumePoint GetPoint (Vector3 position)
        {
            int index = AreaUtility.GetIndexFromWorldPosition (position, GetHolderColliders ().position, boundsFull);
            if (index < 0 || index > points.Count - 1)
            {
                // Debug.LogWarning ("AM | ApplyDamageToPosition | Volume point index calculated from hit position " + position + " equals " + index + ", which is an invalid number out of bounds of the volume | Aborting");
                return null;
            }

            return points[index];
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

            if (checkFlag && point.indestructibleAny)
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

            if (checkFullSurroundings
                && point.pointPositionIndex.y >= instance.damagePenetrationDepth
                && IsPointSurroundedBySpotConfiguration (point, AreaNavUtility.configFull))
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

        public static bool IsPointInvolvingTileset (AreaVolumePoint point, int tilesetID, int groupIDRequired = -1)
        {
            if (point == null)
                return false;

            bool pointInvolvesTileset = false;
            bool groupIDUnchecked = groupIDRequired == -1;

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
                        pointOther.blockTileset == tilesetID &&
                        (groupIDUnchecked || pointOther.blockGroup == groupIDRequired)
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

        public (bool, int) IsSamePoint (Vector3 position1, Vector3 position2)
        {
            var chPos = GetHolderColliders ().position;
            var index1 = AreaUtility.GetIndexFromWorldPosition (position1, chPos, boundsFull);
            var index2 = AreaUtility.GetIndexFromWorldPosition (position2, chPos, boundsFull);
            return (index1 == index2, index2);
        }

        public static bool IsPointInterior (AreaVolumePoint point) =>
            point != null
            && point.spotPresent
            && point.pointState == AreaVolumePointState.Full
            && point.spotConfiguration == TilesetUtility.configurationFull
            && point.blockTileset == 0;

        #if !PB_MODSDK

        public enum VoxelCheck
        {
            Invalid = 0,
            Full,
            Empty,
            Slot,
            Bottom,
        }

        private static readonly Collider[] voxelOverlapBuffer = new Collider[7];
        private static readonly List<Vector3> tempHits = new List<Vector3> ();

        public VoxelCheck InspectVoxelAtPoint (int indexPoint)
        {
            if (!indexPoint.IsValidIndex (points))
                return VoxelCheck.Invalid;

            var point = points[indexPoint];
            var bottomPoint = point.pointPositionIndex.y == boundsFull.y - 1;
            if (bottomPoint)
                return VoxelCheck.Bottom;

            var positionCenter = point.pointPositionLocal;
            positionCenter.y -= TilesetUtility.blockAssetSize;

            var hits = Physics.OverlapSphereNonAlloc
            (
                positionCenter,
                TilesetUtility.blockAssetHalfSize,
                voxelOverlapBuffer,
                LayerMasks.environmentAndDebrisMask
            );

            var result = VoxelCheck.Empty;
            tempHits.Clear ();
            for (var i = 0; i < hits; i += 1)
            {
                var colliderHit = voxelOverlapBuffer[i];
                var positionHit = colliderHit.bounds.center;
                if (positionHit == positionCenter)
                    return VoxelCheck.Full;
                if (positionHit.y != positionCenter.y)
                    continue;
                for (var j = 0; j < tempHits.Count; j += 1)
                {
                    var pos = tempHits[j];
                    var dx = Mathf.Abs (positionHit.x - pos.x);
                    if (dx.RoughlyEqual (TilesetUtility.blockAssetSize * 2f))
                        result = VoxelCheck.Slot;
                    var dz = Mathf.Abs (positionHit.z - pos.z);
                    if (dz.RoughlyEqual (TilesetUtility.blockAssetSize * 2f))
                        result = VoxelCheck.Slot;
                }
                tempHits.Add (positionHit);
            }

            return result;
        }

        private static readonly HashSet<int> crashSpotIndexes = new HashSet<int> ();
        private static int[] resolvedSpotIndexes = new int[3];

        public (bool, int[]) ResolveCrashSpotIndexes (Vector3[] positions, int length)
        {
            // Positive integers : indexes of destroyed spots
            // Negative integers : indexes of undestroyed spots
            // Indexes for undestroyed spots are shifted up by one so that an undestroyed spot at index 0 is represented
            // as -1. This means -1 can't be used as a sentinel for an invalid index as it usually is.
            var invalidIndex = -(points.Count + 1);
            var crashable = false;
            length = Mathf.Clamp (length, 0, positions.Length);
            if (length > resolvedSpotIndexes.Length)
                resolvedSpotIndexes = new int[length];
            var chPos = GetHolderColliders ().position;
            var len = Mathf.Min (resolvedSpotIndexes.Length, length);
            for (var i = 0; i < len; i += 1)
            {
                var index = AreaUtility.GetIndexFromWorldPosition (positions[i], chPos, boundsFull);
                if (index == -1)
                {
                    resolvedSpotIndexes[i] = invalidIndex;
                    continue;
                }
                if (crashSpotIndexes.Contains (index))
                {
                    resolvedSpotIndexes[i] = index;
                    crashable = true;
                    continue;
                }
                resolvedSpotIndexes[i] = -(index + 1);
            }
            for (var i = len; i < resolvedSpotIndexes.Length; i += 1)
                resolvedSpotIndexes[i] = invalidIndex;
            return (crashable, resolvedSpotIndexes);
        }

        public void ApplyDamageToPoint (AreaVolumePoint point, float hitDamage, bool forceOverkillEffect = false, bool forceDestructible = false)
        {
            if (!TryUpdatePointIntegrity (point, hitDamage, forceDestructible))
                return;

            if (point.integrity > 0f)
            {
                /*
                if (DataShortcuts.sim.environmentCollisionDebug)
                {
                    Debug.DrawLine (point.pointPositionLocal, point.pointPositionLocal + Vector3.up * (1.5f * point.integrity), Color.white, 5);
                    Debug.DrawLine (point.pointPositionLocal + Vector3.up * (1.5f * point.integrity), point.pointPositionLocal + Vector3.up * 1.5f, Color.yellow, 5);
                }
                */

                for (int i = 0; i < 8; ++i)
                {
                    var pointAffected = point.pointsWithSurroundingSpots[i];
                    if (pointAffected != null)
                        ApplyShaderPropertiesAtPoint (pointAffected, ShaderOverwriteMode.None, false, true, false);
                }
                return;
            }

            // if (DataShortcuts.sim.environmentCollisionDebug)
            //     Debug.DrawLine (point.pointPositionLocal, point.pointPositionLocal + Vector3.up * 1.5f, Color.red, 5);

            // Debug.Log ("AM | ApplyDamageToPosition | Integrity of volume point " + avp.spotIndex + " (" + positionIndex + ") has reached 0, redrawing the surrounding area");

            UpdatePointForDestruction (point);
            ApplyDestructionToPoint (point, hitDamage, forceOverkillEffect);
            ScanForDisconnect (point);
        }

        private bool TryUpdatePointIntegrity (AreaVolumePoint point, float hitDamage, bool forceDestructible = false)
        {
            if (!Application.isPlaying)
                return false;
            if (hitDamage <= 0.01f)
                return false;
            if (point == null)
                return false;
            if (point.pointState != AreaVolumePointState.Full)
                return false;
            if (point.integrity <= 0)
                return false;

            // Point is indestructible either based on designer input or based on height, tileset or level edge
            // Option to override indestructibility set by designer input
            if (point.indestructibleAny & !forceDestructible)
                return false;

            if (forceDestructible && IsPointInvolvingIndestructibleTilesets (point))
                return false;

            // if (point.simulatedHelper != null)
            //     return;

            var integrity = Mathf.Clamp01 (point.integrity - hitDamage * DataLinkerSettingsArea.data.damageScalar);
            point.integrity = integrity;
            if (integrity == 0f)
            {
                point.pointState = AreaVolumePointState.FullDestroyed;
                point.integrityForDestructionAnimation = 1f;
                crashSpotIndexes.Add (point.spotIndex);
            }

            CombatReplayHelper.OnLevelDamageChange (point.spotIndex, integrity);

            return true;
        }

        private void UpdatePointForDestruction (AreaVolumePoint point)
        {
            StartDestructionAnimation (point);
            UpdateSpotsAroundPoint (point, false);
            UpdateDamageAroundPoint (point);
            RebuildBlocksAroundPoint (point);
        }

        private static readonly List<AreaVolumePoint> pointsToDestroy = new List<AreaVolumePoint> ();

        private void ScanForDisconnect (AreaVolumePoint point)
        {
            if (point.simulatedChunkPresent)
                return;

            pointsToDestroy.Clear ();
            AreaUtility.ScanNeighborsDisconnectedOnDestruction (point, points, pointsToDestroy: pointsToDestroy);
            foreach (var p in pointsToDestroy)
            {
                p.pointState = AreaVolumePointState.FullDestroyed;
                p.integrity = 0f;
                p.integrityForDestructionAnimation = 1f;
                crashSpotIndexes.Add (p.spotIndex);
                CombatReplayHelper.OnLevelDamageChange (p.spotIndex, p.integrity);
                UpdatePointForDestruction (p);
                ApplyDestructionToPoint (p, 0f, false, audioFX: false);
            }
        }

        private void ApplyDestructionToPoint (AreaVolumePoint point, float hitDamage, bool forceOverkillEffect, bool audioFX = true)
        {
            if (pointsDestroyedSinceLastSimulation == null)
                pointsDestroyedSinceLastSimulation = new List<AreaVolumePoint> ();
            pointsDestroyedSinceLastSimulation.Add (point);

            RebuildCollisionForPoint (point);

            // Destruction event position
            var fxPos = point.simulatedChunkPresent
                ? point.simulatedChunkComponent.transform.TransformPoint (point.pointPositionLocal - point.simulatedChunkComponent.initialPosition)
                : point.pointPositionLocal;

            var damageExtreme = hitDamage > 50 || forceOverkillEffect;
            if (audioFX)
            {
                var audioKey = damageExtreme ? PhantomBrigade.Game.AudioEvents.BuildingExplosionLarge : PhantomBrigade.Game.AudioEvents.BuildingCrumble;
                AudioUtility.CreateAudioEvent (audioKey, fxPos);
            }

            var dominantTileset = GetClosestTileset (point);
            var fxKey = dominantTileset.fxNameExplosion; // damageExtreme ? DataShortcuts.sim.environmentExplosionAsset : dominantTileset.fxNameExplosion;
            if (logTilesetSearch)
                Debug.Log ($"Spawning explosion effect {dominantTileset.fxNameExplosion} at position {fxPos} for point {point.spotIndex}");
            AssetPoolUtility.ActivateInstance (fxKey, fxPos, Vector3.forward);
            if (damageExtreme)
                AssetPoolUtility.ActivateInstance (DataShortcuts.sim.environmentExplosionAsset, fxPos, Vector3.forward);

            OverlapUtility.CheckUnitsOnDestroyedPoint (point.pointPositionLocal);

            if (point.destructionUntracked)
                return;

            var combat = Contexts.sharedInstance.combat;
            int destructionCount = combat.hasDestructionCount ? combat.destructionCount.i : 0;
            combat.ReplaceDestructionCount (destructionCount + 1);
        }

        public void ApplyDamageFromList (List<DataBlockAreaIntegrity> integrities)
        {
            if (points == null || integrities == null)
                return;

            if (logShaderUpdates)
                Debug.Log ($"Applying damage from list ({integrities.Count})");

            if (pointsDestroyedSinceLastSimulation == null)
                pointsDestroyedSinceLastSimulation = new List<AreaVolumePoint> ();
            else
                pointsDestroyedSinceLastSimulation.Clear ();

            for (int i = 0; i < integrities.Count; ++i)
            {
                DataBlockAreaIntegrity integrityLoaded = integrities[i];
                if (!integrityLoaded.i.IsValidIndex (points))
                    continue;

                AreaVolumePoint point = points[integrityLoaded.i];
                if (point.pointState == AreaVolumePointState.Empty)
                    continue;

                point.integrity = integrityLoaded.v;
                if (point.integrity <= 0f)
                {
                    point.integrityForDestructionAnimation = 0f;
                    point.pointState = AreaVolumePointState.FullDestroyed;
                    pointsDestroyedSinceLastSimulation.Add (point);

                    UpdateSpotsAroundPoint (point, false);
                    UpdateDamageAroundPoint (point);
                    RebuildBlocksAroundPoint (point);
                    RebuildCollisionForPoint (point);
                }
                else
                {
                    for (int p = 0; p < 8; ++p)
                    {
                        var pointNeighbor = point.pointsWithSurroundingSpots[p];
                        if (pointNeighbor != null)
                            ApplyShaderPropertiesAtPoint (pointNeighbor, ShaderOverwriteMode.None, true, true, true);
                    }
                }
            }

            ApplyShaderPropertiesEverywhere ();


        }

        #endif

        private List<int> tilesetsIndexesEncountered = new List<int> (8);

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

        #if !PB_MODSDK

        private List<AreaVolumePoint> pointsToAnimate = new List<AreaVolumePoint> ();
        private float destructionAnimationRate = 0.20f;

        private void StartDestructionAnimation (AreaVolumePoint point)
        {
            pointsToAnimate.Add (point);
            for (int n = 0; n < point.pointsWithSurroundingSpots.Length; ++n)
            {
                var pointNeighbour = point.pointsWithSurroundingSpots[n];
                if (pointNeighbour == null)
                    continue;
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

        private static readonly List<List<AreaVolumePoint>> isolatedStructureGroups = new List<List<AreaVolumePoint>> ();
        private static bool isolatedStructureSimRequested;

        public static void OnIsolatedStructureDiscovery (List<AreaVolumePoint> pointsIsolated)
        {
            if (pointsIsolated == null || pointsIsolated.Count == 0)
                return;

            foreach (var point in pointsIsolated)
            {
                point.simulationRequested = true;
                crashSpotIndexes.Add (point.spotIndex);
            }

            var pointsIsolatedCopy = new List<AreaVolumePoint> (pointsIsolated);
            isolatedStructureGroups.Add (pointsIsolatedCopy);
            isolatedStructureSimRequested = true;

            if (DataShortcuts.sim.debugCombatStructureAnalysis)
                Debug.Log ($"New isolated structure group consisting of {pointsIsolated.Count} points requests simulation");

        }

        public List<AreaSimulatedChunk> simulatedHelpers = null;
        public List<AreaVolumePoint> pointsDestroyedSinceLastSimulation = null;
        private int simulatedHelperCounter = 0;

        private List<GameObject> simObjectsToReparent = new List<GameObject> ();

        public void SimulateIsolatedStructures ()
        {
            if (isolatedStructureGroups.Count == 0)
                return;

            if (!IsECSSafe ())
                return;

            var logSimulation = DataShortcuts.sim.debugCombatStructureAnalysis;
            if (!Application.isPlaying)
            {
                if (logSimulation)
                    Debug.LogWarning ($"Attempted to start simulation of {isolatedStructureGroups.Count} isolated groups, not supported in edit mode");
                isolatedStructureGroups.Clear ();
                return;
            }

            if (logSimulation)
                Debug.Log ($"Starting simulation of {isolatedStructureGroups.Count} isolated groups");

            if (simulatedHelpers == null)
                simulatedHelpers = new List<AreaSimulatedChunk> ();

            var entityManager = world.EntityManager;
            for (var i = isolatedStructureGroups.Count - 1; i >= 0; i -= 1)
            {
                var group = isolatedStructureGroups[i];
                isolatedStructureGroups.RemoveAt (i);

                var simulatedHolder = new GameObject ("!SimulatedHolder_" + i);
                simulatedHolder.transform.position = group[0].pointPositionLocal;
                simulatedHolder.layer = Constants.debrisLayer;
                var simulatedParent = GetHolderSimulatedParent ();
                simulatedHolder.transform.parent = simulatedParent;
                var simulatedHelper = CreateSimulatedHelper (entityManager, simulatedHolder, group.Count);

                var countNeighbours = 0;
                var positionNeighbour = Vector3.zero;
                for (var g = 0; g < group.Count; g += 1)
                {
                    var point = group[g];
                    countNeighbours = CreateSimulatedPoint
                    (
                        ref entityManager,
                        simulatedHolder.transform,
                        simulatedHelper,
                        g,
                        point,
                        countNeighbours,
                        ref positionNeighbour
                    );
                }

                CreateSimulatedRigidbody (simulatedHolder, simulatedHelper);

                CombatReplayHelper.OnSimulatedStructures
                (
                    simulatedHelper.id,
                    simulatedHelper.transform.position,
                    simulatedHelper.transform.rotation,
                    simulatedHelper.pointsAffected,
                    simulatedHelper.initialPosition
                );

                if (countNeighbours > 0)
                {
                    if (logSimulation)
                        Debug.Log ($"Simulated group {i} has {countNeighbours} neighbours out of recently destroyed {pointsDestroyedSinceLastSimulation.Count} points");
                    var positionCenter = simulatedHolder.transform.TransformPoint (simulatedHelper.simulatedRigidbody.centerOfMass);
                    var difference = positionCenter - positionNeighbour;
                    var direction = Vector3.Normalize (difference);

                    /*
                    List<Vector3> linePoints = new List<Vector3> ();
                    linePoints.Add (positionNeighbour);
                    linePoints.Add (positionCenter);

                    Vectrosity.VectorLine line = new Vectrosity.VectorLine ("Line_Neighbor", linePoints, 6f, Vectrosity.LineType.Continuous);
                    line.layer = Constants.EnvironmentLayer;
                    line.material = UtilityMaterial.GetDefaultMaterial ();
                    line.continuousTexture = true;
                    line.joins = Vectrosity.Joins.None;
                    line.Draw3DAuto ();
                    */

                    // Vector3 push = new Vector3 (difference.x, 0f, difference.z);
                    // simulatedHelper.simulatedRigidbody.centerOfMass = positionNeighbour;

                    simulatedHelper.timerForForce = simulatedHelper.durationForForce = 3f;
                    simulatedHelper.positionForForce = positionNeighbour;
                    simulatedHelper.useVerticalForce = true;

                    // Vector3 push = new Vector3 (0f, new Vector2 (difference.x, difference.z).magnitude * Tweakables.data.areaSystem.crashPushForceMultiplier * group.Count, 0f);
                    // simulatedHelper.simulatedRigidbody.AddForceAtPosition (push, positionNeighbour, ForceMode.Impulse);
                }
                else if (logSimulation)
                    Debug.Log ($"Simulated group {i} has no neighbours out of recently destroyed {pointsDestroyedSinceLastSimulation.Count} points");
            }

            UtilityECS.ScheduleUpdate ();
        }

        private AreaSimulatedChunk CreateSimulatedHelper (EntityManager entityManager, GameObject simulatedHolder, int groupCount)
        {
            var simulatedHelper = simulatedHolder.AddComponent<AreaSimulatedChunk> ();
            simulatedHelper.colliderToPointMap = new Dictionary<Collider, AreaVolumePoint> ();
            simulatedHelper.initialPosition = simulatedHolder.transform.position;
            simulatedHelpers.Add (simulatedHelper);
            simulatedHelper.pointsAffected = new HashSet<int> ();

            simulatedHelper.id = simulatedHelperCounter;
            simulatedHelperCounter += 1;

            // var pos = simulatedHelper.gameObject.AddComponent<TranslationProxy> ();
            // pos.Value = new Translation { Value = simulatedHelper.transform.position };

            // var rot = simulatedHelper.gameObject.AddComponent<RotationProxy> ();
            // rot.Value = new Rotation { Value = simulatedHelper.transform.rotation };

            var entityRoot = entityManager.CreateEntity (simulationRootArchetype);
            entityManager.SetComponentData (entityRoot, new ECS.SimulatedChunkRoot { id = simulatedHelper.id });
            entityManager.SetComponentData (entityRoot, new Translation { Value = simulatedHelper.transform.position });
            entityManager.SetComponentData (entityRoot, new Rotation { Value = simulatedHelper.transform.rotation });
            simulatedHelper.entityRoot = entityRoot;

            return simulatedHelper;
        }

        private int CreateSimulatedPoint
        (
            ref EntityManager entityManager,
            Transform holder,
            AreaSimulatedChunk simulatedHelper,
            int indexGroup,
            AreaVolumePoint point,
            int countNeighbours,
            ref Vector3 positionNeighbour
        )
        {
            if (pointsDestroyedSinceLastSimulation != null)
            {
                for (var i = 0; i < pointsDestroyedSinceLastSimulation.Count; i += 1)
                {
                    var potentialNeighbour = pointsDestroyedSinceLastSimulation[i];
                    if (potentialNeighbour.IsNeighbor (point))
                    {
                        if (countNeighbours == 0)
                            positionNeighbour = potentialNeighbour.pointPositionLocal;
                        countNeighbours += 1;
                    }
                }
            }

            if (point.instanceCollider != null)
                point.instanceCollider.SetActive (false);

            OverlapUtility.CheckUnitsOnDestroyedPoint (point.pointPositionLocal);

            var simulatedPoint = new GameObject ("!SimulatedPoint_" + indexGroup);
            simulatedPoint.transform.position = point.pointPositionLocal;
            simulatedPoint.transform.parent = holder;
            simulatedPoint.transform.localScale = Vector3.one * 3f;
            simulatedPoint.layer = Constants.debrisLayer;

            var simulatedCollider = simulatedPoint.AddComponent<SphereCollider> ();
            simulatedCollider.radius = 0.5f;
            simulatedHelper.colliderToPointMap.Add (simulatedCollider, point);

            if (visualizeCollisions)
            {
                var vis = PrimitiveHelper.CreatePrimitive (PrimitiveType.Cube, false);
                vis.name = "ColliderForSimulatedPointVisual";
                vis.hideFlags = HideFlags.DontSave;
                vis.transform.parent = simulatedPoint.transform;
                vis.transform.localScale = Vector3.one * 0.5f;
                vis.transform.localPosition = Vector3.zero;

                if (visualSimulationMaterial == null)
                    visualSimulationMaterial = Resources.Load<Material> ("Content/Debug/AreaCollisionSimulated");

                var mr = vis.GetComponent<MeshRenderer> ();
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.sharedMaterial = visualSimulationMaterial;
            }

            for (var i = 0; i < 8; i += 1)
            {
                var pointAround = point.pointsWithSurroundingSpots[i];
                if (pointAround == null || !pointAround.spotPresent)
                    continue;
                if (!simulatedHelper.pointsAffected.Add (pointAround.spotIndex))
                    continue;

                pointAround.simulatedChunkComponent = simulatedHelper;
                pointAround.simulatedChunkPresent = true;
                pointAround.simulationRequested = false;

                if (indexesOccupiedByProps.ContainsKey (pointAround.spotIndex))
                {
                    var placements = indexesOccupiedByProps[pointAround.spotIndex];
                    for (var p = 0; p < placements.Count; p += 1)
                        AreaAnimationSystem.OnRemoval (placements[p], true);
                }

                FreeEntityForSimulation (ref entityManager, pointEntitiesMain[pointAround.spotIndex], pointAround);
                FreeEntityForSimulation (ref entityManager, pointEntitiesInterior[pointAround.spotIndex], pointAround);
            }

            return countNeighbours;
        }

        private void CreateSimulatedRigidbody (GameObject simulatedHolder, AreaSimulatedChunk simulatedHelper)
        {
            var simulatedRigidbody = simulatedHolder.AddComponent<Rigidbody> ();
            simulatedRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            simulatedRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            simulatedHelper.simulatedRigidbody = simulatedRigidbody;
            simulatedHelper.UpdateRigidbody ();
            simulatedHelper.initialPointCount = simulatedHelper.colliderToPointMap.Count;
            // entityManager.AddComponent (entityRoot, typeof (Rigidbody));

            if (visualizeCollisions)
            {
                var vis = PrimitiveHelper.CreatePrimitive (PrimitiveType.Cube, false);
                vis.name = "SimulatedHolderVisual";
                vis.hideFlags = HideFlags.DontSave;
                vis.transform.parent = simulatedHolder.transform;
                vis.transform.localScale = Vector3.one * 3f;
                vis.transform.localPosition = simulatedRigidbody.centerOfMass;

                if (visualHolderMaterial == null)
                    visualHolderMaterial = Resources.Load<Material> ("Content/Debug/AreaCollisionHolder");

                var mr = vis.GetComponent<MeshRenderer> ();
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.sharedMaterial = visualHolderMaterial;
            }
        }

        public void OnSimulatedHelperFinish (AreaSimulatedChunk simulatedHelper)
        {
            if (simulatedHelpers == null)
                return;

            if (simulatedHelpers.Contains (simulatedHelper))
            {
                CombatReplayHelper.OnSimulatedStructuresEnd (simulatedHelper.id);
                simulatedHelpers.Remove (simulatedHelper);
            }
        }

        public bool IsSimulationInProgress ()
        {
            return simulatedHelpers != null && simulatedHelpers.Count > 0;
        }

        #endif

        private static Vector3 spotShiftVertical = Vector3.up * TilesetUtility.blockAssetHalfSize;
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

        [ContextMenu ("Generate combined navigation overrides")]
        public void GenerateNavOverrides ()
        {
            AreaNavUtility.GenerateNavOverrides (navOverrides, points, showSlopeDetection, navOverridesSaved, false);
        }

        [ContextMenu ("Clear conflicting navigation overrides")]
        public void ClearConflictingNavOverrides ()
        {
            AreaNavUtility.GenerateNavOverrides (navOverrides, points, showSlopeDetection, navOverridesSaved, true);
        }


        public void SetVisible (bool visible)
        {
            GetHolderColliders ().gameObject.SetActive (visible);
            // GetHolderBackgrounds ().gameObject.SetActive (visible);
        }

        public void SetVolumeDisplayMode (bool displayOnlyVolume)
        {
            if (Application.isPlaying)
                return;

            var area = DataMultiLinkerCombatArea.selectedArea;
            if (area == null)
            {
                Debug.LogWarning ($"Can't update display mode, no selected area available");
                return;
            }

            this.displayOnlyVolume = displayOnlyVolume;
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

            Debug.Log ($"Loading combat area {key}");

            areaName = key;
            boundsFull = core.bounds;
            damageRestrictionDepth = core.damageRestrictionDepth;
            damagePenetrationDepth = core.damagePenetrationDepth;

            ShaderGlobalHelper.CheckGlobals ();
            AreaTilesetHelper.CheckResources ();
            AreaAssetHelper.CheckResources ();

            #if !PB_MODSDK
            AreaUtility.structureAnalysisCounter = 0;

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

            List<int> detectedMissingTilesets = new List<int> ();
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
                    Debug.Log (string.Format ("AM | LoadArea | Resetting tileset, group and subtype on point {0} due to missing tileset {1}", point.spotIndex, point.blockTileset));
                    DrawHighlightSpot (point);

                    point.blockTileset = AreaTilesetHelper.database.tilesetFallback.id;
                    point.blockFlippedHorizontally = false;
                    point.blockGroup = 0;
                    point.blockSubtype = 0;

                    if (!detectedMissingTilesets.Contains (point.blockTileset))
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

            navOverrides.Clear ();
            navOverridesSaved.Clear ();
            int pointsCountTotal = points.Count;

            if (data.navOverrides != null)
            {
                // Debug.Log ($"Applying loaded nav overrides: {data.navOverrides.Count}");
                for (int i = 0; i < data.navOverrides.Count; ++i)
                {
                    var navOverrideLoaded = data.navOverrides[i];

                    if (navOverrideLoaded.pivotIndex < 0 || navOverrideLoaded.pivotIndex >= pointsCountTotal)
                        continue;

                    var point = points[navOverrideLoaded.pivotIndex];
                    if (point.spotConfiguration == AreaNavUtility.configEmpty)
                        continue;

                    var navOverride = new AreaDataNavOverride
                    {
                        pivotIndex = navOverrideLoaded.pivotIndex,
                        offsetY = navOverrideLoaded.offsetY
                    };

                    // Nav overrides loaded from the data only go into navOverridesSaved collection.
                    // On nav rebuild, navOverrides collection is filled with autogenerated overrides (based on terrain & tilesets) + these saved overrides.
                    navOverridesSaved[navOverrideLoaded.pivotIndex] = navOverride;
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
                        pointAbove.blockGroup > 50
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

            SetupPointEntities ();
            RebuildEverything ();
            #if !PB_MODSDK
            UpdateBoundsInSystems ();
            #endif

            if (Application.isPlaying)
            {
                Debug.Log ($"Finished loading combat area {key}");
                ApplyShaderPropertiesEverywhere ();
            }

            OnAfterAreaLoaded ();
        }

        private void OnAfterAreaLoaded ()
        {

        }

        public void LoadAreaSnippet (string key, AreaDataCore core, AreaDataContainer dataLoaded)
        {
            if (dataLoaded == null || core == null || string.IsNullOrEmpty (key))
            {
                Debug.LogWarning ("AM | LoadArea | Failed to load area snippet from container, null reference received");
                return;
            }

            if (clipboard == null)
                clipboard = new AreaClipboard ();

            if (clipboard.clipboardPointsSaved == null)
                clipboard.clipboardPointsSaved = new List<AreaVolumePoint> ();
            else
                clipboard.clipboardPointsSaved.Clear ();

            if (clipboard.clipboardPropsSaved == null)
                clipboard.clipboardPropsSaved = new List<AreaPlacementProp> ();
            else
                clipboard.clipboardPropsSaved.Clear ();

            clipboard.name = key;
            clipboard.clipboardBoundsSaved = core.bounds;
            clipboardBoundsRequested = core.bounds;

            ShaderGlobalHelper.CheckGlobals ();
            AreaTilesetHelper.CheckResources ();
            AreaAssetHelper.CheckResources ();

            // Base data

            for (int i = 0; i < dataLoaded.points.Length; ++i)
            {
                AreaVolumePoint point = new AreaVolumePoint ();
                clipboard.clipboardPointsSaved.Add (point);

                point.pointState = dataLoaded.points[i] == true ? AreaVolumePointState.Full : AreaVolumePointState.Empty;
                if (point.pointState == AreaVolumePointState.FullDestroyed)
                {
                    point.integrity = 0;
                    point.integrityForDestructionAnimation = 0f;
                }
            }

            for (int i = 0; i < clipboard.clipboardPointsSaved.Count; ++i)
            {
                AreaVolumePoint point = clipboard.clipboardPointsSaved[i];

                point.spotIndex = i;
                point.pointPositionIndex = TilesetUtility.GetVolumePositionFromIndex (i, clipboard.clipboardBoundsSaved);
                point.pointPositionLocal = new Vector3 (point.pointPositionIndex.x, -point.pointPositionIndex.y, point.pointPositionIndex.z) * TilesetUtility.blockAssetSize;
                point.instancePosition = point.pointPositionLocal + new Vector3 (1f, -1f, 1f) * (TilesetUtility.blockAssetSize / 2f);

                if (point.pointPositionIndex.y >= boundsFull.y - 2)
                    point.pointState = AreaVolumePointState.Full;
                else
                    point.pointState = AreaVolumePointState.Empty;

                clipboard.clipboardPointsSaved[i] = point;
            }

            for (int i = 0; i < dataLoaded.spots.Count; ++i)
            {
                AreaDataSpot spotLoaded = dataLoaded.spots[i];
                AreaVolumePoint point = clipboard.clipboardPointsSaved[spotLoaded.index];

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
            }

            for (int i = 0; i < dataLoaded.customizations.Count; ++i)
            {
                AreaDataCustomization customizationLoaded = dataLoaded.customizations[i];
                AreaVolumePoint point = clipboard.clipboardPointsSaved[customizationLoaded.index];

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

            if (dataLoaded.indestructibleIndexes != null)
            {
                int pointsCount = clipboard.clipboardPointsSaved.Count;
                for (int i = 0; i < dataLoaded.indestructibleIndexes.Count; ++i)
                {
                    var index = dataLoaded.indestructibleIndexes[i];
                    if (index >= 0 && index < pointsCount)
                    {
                        var point = clipboard.clipboardPointsSaved[index];
                        if (point != null)
                            point.destructible = false;
                    }
                }
            }

            /*
            if (dataLoaded.untrackedIndexes != null)
            {
                int pointsCount = clipboard.clipboardPointsSaved.Count;
                for (int i = 0; i < dataLoaded.untrackedIndexes.Count; ++i)
                {
                    var index = dataLoaded.untrackedIndexes[i];
                    if (index >= 0 && index < pointsCount)
                    {
                        var point = clipboard.clipboardPointsSaved[index];
                        if (point != null)
                            point.destructionUntracked = true;
                    }
                }
            }
            */

            for (int i = 0; i < dataLoaded.integrities.Count; ++i)
            {
                AreaDataIntegrity integrityLoaded = dataLoaded.integrities[i];
                AreaVolumePoint point = clipboard.clipboardPointsSaved[integrityLoaded.index];

                if (point.pointState == AreaVolumePointState.Empty)
                    continue;

                point.integrity = integrityLoaded.integrity;
                if (point.destructible && point.integrity <= 0f)
                {
                    point.integrityForDestructionAnimation = 0f;
                    point.pointState = AreaVolumePointState.FullDestroyed;
                }
            }

            int detectedMissingProps = 0;
            placementsProps = new List<AreaPlacementProp> (dataLoaded.props.Count);
            for (int i = 0; i < dataLoaded.props.Count; ++i)
            {
                AreaDataProp placementLoaded = dataLoaded.props[i];
                AreaPropPrototypeData prototype = AreaAssetHelper.GetPropPrototype (placementLoaded.id);

                if (prototype == null)
                    ++detectedMissingProps;

                if
                (
                    placementLoaded.pivotIndex.IsValidIndex (clipboard.clipboardPointsSaved) &&
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

                    placement.hsbPrimary = new Vector4 (placementLoaded.h1, placementLoaded.s1, placementLoaded.b1, 0f);
                    placement.hsbSecondary = new Vector4 (placementLoaded.h2, placementLoaded.s2, placementLoaded.b2, 0f);

                    placement.state = new AreaPropState ();
                    placement.destroyed = placementLoaded.status != 0;
                    placement.destructionTime = -100f;
                    if (placement.destroyed)
                        placementsPropsDestroyed.Add (placement);
                }
                else
                    Debug.LogWarning ("AM | LoadAreaSnippet | Skipping prop placement " + i + " with ID " + placementLoaded.id + " | Prototype is " + prototype.ToStringNullCheck ());
            }

            if (detectedMissingProps > 0)
                Debug.LogWarning ("AM | LoadAreaSnippet | Found " + detectedMissingProps + " missing prop IDs");
        }

        #if !PB_MODSDK

        public float boundsBoostForCamera = 20f;
        public float boundsBoostForGrid = -1f;
        public float boundsBoostForCursor = -1f;

        [ContextMenu ("Update bounds in systems")]
        public void UpdateBoundsInSystems ()
        {
            if (Contexts.sharedInstance == null)
                return;
            var center = new Vector3 ((boundsFull.x - 1) / 2f, -(boundsFull.y - 1) / 2f, (boundsFull.z - 1) / 2f) * TilesetUtility.blockAssetSize;
            var boundsForGrid = new Bounds (center, GetBoundsExtents (boundsFull, boundsBoostForGrid));
            Contexts.sharedInstance.combat.ReplaceMapBounds (boundsForGrid);
        }

        private static Vector3 GetBoundsExtents (Vector3Int bounds, float offset)
        {
            // Debug.Log ("AM | GetBoundsExtents | Offset: " + offset + " | X: " + boundsFull.x + " | After offset: " + (boundsFull.x + offset * 2) + " | After sizing: " + (Mathf.Max (4, boundsFull.x + offset * 2) * TilesetUtility.blockAssetSize));
            var x = Mathf.Max (bounds.x + offset * 2f, 1f) * TilesetUtility.blockAssetSize;
            var y = Mathf.Max (bounds.y + offset, 1f) * TilesetUtility.blockAssetSize;
            var z = Mathf.Max (bounds.z + offset * 2f, 1f) * TilesetUtility.blockAssetSize;
            return new Vector3 (x, y, z);
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

                int indexToCheck = AreaUtility.GetIndexFromInternalPosition (internalPosition, boundsFull);
                indexesToCheck.Add (indexToCheck);
            }

            return indexesToCheck;
        }

        #endif

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
            AreaNavUtility.OnLevelUnload ();

            ++areaChangeTracker;

            points = new List<AreaVolumePoint> ();
            placementsProps = new List<AreaPlacementProp> ();
            navOverrides = new Dictionary<int, AreaDataNavOverride> ();
            #if !PB_MODSDK
            pointsToAnimate.Clear ();
            crashSpotIndexes.Clear ();
            collidersByDistance.Clear ();
            #endif

            UtilityGameObjects.ClearChildren (GetHolderColliders ());

            #if !PB_MODSDK
            if (Application.isPlaying)
            {
                var game = Contexts.sharedInstance.game;
                if (game.hasAreaActive)
                    game.RemoveAreaActive ();

                simulatedHelpers.Clear();
                UtilityGameObjects.ClearChildren (GetHolderSimulatedParent ());
                UtilityGameObjects.ClearChildren (GetHolderSimulatedLeftovers ());
            }
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

            SetupPointEntities ();
            RebuildEverything ();
            #if !PB_MODSDK
            UpdateBoundsInSystems ();
            #endif
            ShaderGlobalHelper.SetOcclusionShift (boundsFull.y * 3f + 4.5f);
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

        #if !PB_MODSDK

        public static void OnPropImpactFromUnit (int colliderID, Vector3 position, Vector3 direction, float damage, float speed)
        {
            if (instance == null)
                return;

            var prop = instance.GetPropFromColliderID (colliderID);
            if (prop == null)
                return;

            AreaAnimationSystem.OnDestruction (prop, true, position, direction, damage, speed);
        }

        public static void OnPropImpactFromProjectile (int colliderID, Vector3 position, Vector3 direction, float damage, float force)
        {
            if (instance == null)
                return;

            var prop = instance.GetPropFromColliderID (colliderID);
            AreaAnimationSystem.OnDestruction (prop, true, position, direction, damage, force);
        }

        #endif

        public AreaPlacementProp GetPropFromColliderID (int colliderID)
        {
            if (propLookupByColliderID != null && propLookupByColliderID.TryGetValue (colliderID, out var prop))
            {
                // Debug.Log ($"Located a prop collider for prop {propLookupByColliderID[colliderID].prototype.name} using ID {colliderID}");
                return prop;
            }

            return null;
        }


        public AreaClipboard clipboard =  new AreaClipboard();

        public Vector3Int clipboardOrigin;
        public Vector3Int clipboardBoundsRequested;
        public Vector3Int targetOrigin;

		public (int topY,int bottomY) GetShrinkwrapBounds(Vector3Int cornerA, Vector3Int cornerB)
		{
			cornerA.y = 0;
			cornerB.y = boundsFull.y-1;

			int topY = int.MaxValue;
			int bottomY = int.MaxValue;

			for(int y = cornerA.y;y <= cornerB.y;++y)
			{
				bool allFull = true;
				bool allEmpty = true;

				for(int z = cornerA.z;z <= cornerB.z;++z)
				{
					for(int x = cornerA.x;x <= cornerB.x;++x)
					{
						var coord = new Vector3Int(x,y,z);

						int sourcePointIndex = AreaUtility.GetIndexFromInternalPosition (coord, boundsFull);
						var sourcePoint = points[sourcePointIndex];

						if(sourcePoint.pointState != AreaVolumePointState.Empty)
							allEmpty = false;

						if(sourcePoint.pointState != AreaVolumePointState.Full)
							allFull = false;

						if(!allFull && !allEmpty)
							break;
					}

					if(!allFull && !allEmpty)
						break;
				}

				if(!allEmpty)
					topY = Mathf.Min(y-1, topY);

				if(allFull)
					bottomY = Mathf.Min(y, bottomY);
			}

			if(topY < cornerA.y || topY > cornerB.y)
				topY = cornerA.y;

			if(bottomY < cornerA.y || bottomY > cornerB.y)
				bottomY = cornerB.y;

			return (topY, bottomY);
		}

        public void CopyVolume (Vector3Int cornerA, Vector3Int cornerB)
        {
            int indexA = AreaUtility.GetIndexFromInternalPosition (cornerA, boundsFull);
            int indexB = AreaUtility.GetIndexFromInternalPosition (cornerB, boundsFull);

            if (indexA == -1 || indexB == -1)
            {
                Debug.LogWarning (string.Format
                (
                    "CopyVolume | Failed to copy due to specified corners {0} and {1} falling outside of source level bounds {2}",
                    cornerA,
                    cornerB,
                    boundsFull
                ));
                return;
            }

            // Bounds are limits, like array sizes - so they will be bigger by 1 than just pure position difference
            // For example, a set of points [(0,0);(1,0);(0,1);(1;1)] has bounds of 2x2, not 1x1!
            var size = cornerB - cornerA + Vector3Int.size1x1x1;

            if (size.x < 2 || size.y < 2 || size.z < 2)
            {
                Debug.LogWarning (string.Format
                (
                    "CopyVolume | Failed to copy due to specified corners not creating valid clip bounds | B - A: {0} - {1} = {2}",
                    cornerB,
                    cornerA,
                    (cornerB - cornerA)
                ));
                return;
            }


            clipboard.CopyFromArea(this, cornerA, size);
	    }


        public bool combineExports = false;
		public bool transferProps = false;
        public bool transferVolume = true;

        public void ExportVolume (Vector3Int cornerA, Vector3Int cornerB)
        {
            int indexA = AreaUtility.GetIndexFromInternalPosition (cornerA, boundsFull);
            int indexB = AreaUtility.GetIndexFromInternalPosition (cornerB, boundsFull);

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
                int pointIndex = AreaUtility.GetIndexFromInternalPosition (sourcePointPosition, boundsFull);
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

            if (!combineExports)
                return;

            #if !PB_MODSDK
            var combiner = new DigitalOpus.MB.Core.MB3_MeshCombinerSingle ();
            combiner.doNorm = true;
            combiner.doTan = true;
            combiner.doUV = true;
            combiner.renderType = DigitalOpus.MB.Core.MB_RenderType.meshRenderer;

            combiner.AddDeleteGameObjects (objectsAdded.ToArray (), null, true);
            combiner.Apply ();
            combiner.resultSceneObject.name = "AreaExport_" + areaName;
            #endif

            DestroyImmediate (holder);
        }

        public enum BrushApplicationMode
        {
            Overwrite,
            Additive,
            Subtractive
        }

        public BrushApplicationMode brushApplicationMode = BrushApplicationMode.Overwrite;

        public void PasteVolume (Vector3Int cornerA, BrushApplicationMode mode)
        {
            if (!clipboard.IsValid)
            {
                Debug.Log ("PasteVolume | Clipboard is empty");
                return;
            }

            // Bounds are not positions, they are limits, like array sizes - so we need to subtract 1 from bounds axes to get second corner
            Vector3Int cornerB = cornerA + clipboard.clipboardBoundsSaved + Vector3Int.size1x1x1Neg;

            int indexA = AreaUtility.GetIndexFromInternalPosition (cornerA, boundsFull);
            int indexB = AreaUtility.GetIndexFromInternalPosition (cornerB, boundsFull);

            if (indexA == -1 || indexB == -1)
            {
                Debug.LogWarning (string.Format
                (
                    "PasteVolume | Failed to paste due to specified corner {0} or calculated corner {1} falling outside of target level bounds {2}",
                    cornerA,
                    cornerB,
                    boundsFull
                ));
                return;
            }

            // Since we are directly modifying boundary points, some spots outside of captured area would be affected - time to collect all points
            int affectedOriginX = Mathf.Max (0, cornerA.x - 1);
            int affectedOriginY = Mathf.Max (0, cornerA.y - 1);
            int affectedOriginZ = Mathf.Max (0, cornerA.z - 1);
            var cornerAShifted = new Vector3Int (affectedOriginX, affectedOriginY, affectedOriginZ);

            // Bounds are limits, like array sizes - so they will be bigger by 1 than just pure position difference
            // For example, a set of points [(0,0);(1,0);(0,1);(1;1)] has bounds of 2x2, not 1x1!
            int affectedBoundsX = cornerB.x - cornerAShifted.x + 1;
            int affectedBoundsY = cornerB.y - cornerAShifted.y + 1;
            int affectedBoundsZ = cornerB.z - cornerAShifted.z + 1;
            int affectedVolumeLength = affectedBoundsX * affectedBoundsY * affectedBoundsZ;
            var affectedBounds = new Vector3Int (affectedBoundsX, affectedBoundsY, affectedBoundsZ);
            var affectedPoints = new List<AreaVolumePoint> (affectedVolumeLength);

            for (int i = 0; i < affectedVolumeLength; ++i)
            {
                Vector3Int affectedPointPosition = AreaUtility.GetVolumePositionFromIndex (i, affectedBounds);
                Vector3Int sourcePointPosition = affectedPointPosition + cornerAShifted;
                int sourcePointIndex = AreaUtility.GetIndexFromInternalPosition (sourcePointPosition, boundsFull);
                var sourcePoint = points[sourcePointIndex];
                affectedPoints.Add (sourcePoint);
                // Debug.DrawLine (sourcePoint.pointPositionLocal, sourcePoint.instancePosition, Color.white, 10f);
            }

            Debug.Log (string.Format
            (
                "Pasting volume | Target corner/bounds/points: {0}/{1}/{2} | Affected origin/bounds/points: {3}/{4}/{5}",
                cornerA,
                clipboard.clipboardBoundsSaved,
                clipboard.clipboardPointsSaved.Count,
                cornerAShifted,
                affectedBounds,
                affectedPoints.Count
            ));

            int pointsCountTotal = points.Count;

            if (transferVolume)
            {
                // First, override only point state - we want to update all the configurations before looking into applying other spot-related data
                for (int i = 0; i < clipboard.clipboardPointsSaved.Count; ++i)
                {
                    var clipboardPoint = clipboard.clipboardPointsSaved[i];
                    var targetPointPosition = clipboardPoint.pointPositionIndex + cornerA;
                    int targetPointIndex = AreaUtility.GetIndexFromInternalPosition (targetPointPosition, boundsFull);

                    if (targetPointIndex < 0 || targetPointIndex >= pointsCountTotal)
                    {
                        Debug.LogWarning ($"Failed to apply clipboard point {i} to area point {targetPointIndex}: out of bounds | Point position: {clipboardPoint.pointPositionIndex}");
                        continue;
                    }

                    var targetPoint = points[targetPointIndex];

                    // if (clipboardPoint.pointState != AreaVolumePointState.Empty)
                    //     DrawHighlightBox (targetPoint.pointPositionLocal, Color.cyan, 10f);

                    if (mode == BrushApplicationMode.Additive)
                    {
                        if (targetPoint.pointState == AreaVolumePointState.Empty && clipboardPoint.pointState != AreaVolumePointState.Empty)
                            targetPoint.pointState = clipboardPoint.pointState;
                    }
                    else if (mode == BrushApplicationMode.Subtractive)
                    {
                        if (targetPoint.pointState != AreaVolumePointState.Empty && clipboardPoint.pointState == AreaVolumePointState.Empty)
                            targetPoint.pointState = clipboardPoint.pointState;
                    }
                    else
                        targetPoint.pointState = clipboardPoint.pointState;
                }

                // Update all spots
                for (int i = 0; i < affectedPoints.Count; ++i)
                {
                    UpdateSpotAtIndex (affectedPoints[i].spotIndex, false, false, true);
                    // DrawHighlightBox (affectedPoints[i].instancePosition, Color.red, 10f, 0.5f);
                }

                // Now we can take a look at where spot data can be overwritten
                for (int i = 0; i < clipboard.clipboardPointsSaved.Count; ++i)
                {
                    var clipboardPoint = clipboard.clipboardPointsSaved[i];
                    var targetPointPosition = clipboardPoint.pointPositionIndex + cornerA;
                    int targetPointIndex = AreaUtility.GetIndexFromInternalPosition (targetPointPosition, boundsFull);

                    if (targetPointIndex < 0 || targetPointIndex >= pointsCountTotal)
                    {
                        // Debug.LogWarning ($"Failed to apply clipboard point {i} to area point {targetPointIndex}: out of bounds");
                        continue;
                    }

                    var targetPoint = points[targetPointIndex];

                    // We don't want to affect areas outside of clipboard bounds
                    // (points on positive edges of bounds cube control spots that lie outside of our volume)
                    bool inside =
                        clipboardPoint.pointPositionIndex.x < (clipboard.clipboardBoundsSaved.x - 1) &&
                        clipboardPoint.pointPositionIndex.y < (clipboard.clipboardBoundsSaved.y - 1) &&
                        clipboardPoint.pointPositionIndex.z < (clipboard.clipboardBoundsSaved.z - 1);

                    // We also don't want to write to empty points or points with different configurations
                    bool overwriteSpot =
                        inside &&
                        targetPoint.spotPresent &&
                        clipboardPoint.spotConfiguration == targetPoint.spotConfiguration;

                    if (!overwriteSpot)
                    {
                        // DrawHighlightBox (targetPoint.instancePosition, Color.yellow, 10f, 1.5f);
                        continue;
                    }

                    targetPoint.blockFlippedHorizontally = clipboardPoint.blockFlippedHorizontally;
                    targetPoint.blockGroup = clipboardPoint.blockGroup;
                    targetPoint.blockRotation = clipboardPoint.blockRotation;
                    targetPoint.blockSubtype = clipboardPoint.blockSubtype;
                    targetPoint.blockTileset = clipboardPoint.blockTileset;
                    targetPoint.customization = clipboardPoint.customization;
                    targetPoint.terrainOffset = clipboardPoint.terrainOffset;

                    // Debug.DrawLine (targetPoint.pointPositionLocal, targetPoint.instancePosition, Color.white, 10f);
                }

                // Finally, it's time to push full updates
                for (int i = 0; i < affectedPoints.Count; ++i)
                {
                    RebuildBlock (affectedPoints[i]);
                    RebuildCollisionsAroundIndex (affectedPoints[i].spotIndex);
                }
            }

			if (transferProps && (mode == BrushApplicationMode.Overwrite || mode == BrushApplicationMode.Additive))
			{
				if (mode == BrushApplicationMode.Overwrite)
				{
					for (int i = 0; i < affectedPoints.Count; ++i)
					{
						var index = affectedPoints[i].spotIndex;
						RemovePropPlacement(index);
					}
				}

				for (int i = 0; i < clipboard.clipboardPropsSaved.Count; ++i)
				{
					var savedProp = clipboard.clipboardPropsSaved[i];
                    var targetPointPosition = savedProp.clipboardPosition + cornerA;
					int targetPointIndex = AreaUtility.GetIndexFromInternalPosition (targetPointPosition, boundsFull);

					AreaPlacementProp placement = new AreaPlacementProp();
					AreaVolumePoint pointTargeted = points[targetPointIndex];
					AreaPropPrototypeData prototype = AreaAssetHelper.GetPropPrototype (savedProp.id);

					placement.id = savedProp.id;
					placement.pivotIndex = targetPointIndex;
					placement.rotation = savedProp.rotation;
					placement.flipped = savedProp.flipped;
					placement.offsetX = savedProp.offsetX;
					placement.offsetZ = savedProp.offsetZ;
					placement.hsbPrimary = savedProp.hsbPrimary;
					placement.hsbSecondary = savedProp.hsbSecondary;

					if (IsPropPlacementValid (placement, pointTargeted, prototype, false))
					{
						if (!indexesOccupiedByProps.ContainsKey (targetPointIndex))
						{
							indexesOccupiedByProps.Add (targetPointIndex, new List<AreaPlacementProp> ());
						}

						indexesOccupiedByProps[targetPointIndex].Add (placement);
						placementsProps.Add (placement);
						ExecutePropPlacement (placement);
					}
				}
			}

            var sceneHelper = CombatSceneHelper.ins;
            sceneHelper.terrain.Rebuild (true);
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
                        var index = AreaUtility.GetIndexFromInternalPosition (new Vector3Int (x, sizeY - 1 - y, z), boundsFull);
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
        public List<DepthColorLink> heightfieldPalette = new List<DepthColorLink> ();
        public HashSet<int> colorValues = new HashSet<int> ();

        [ContextMenu ("Save depth to texture (R)")][Button]
        public void ExportToHeightmap ()
        {
            var folderPath = DataMultiLinkerCombatArea.GetCurrentUnpackedLevelPath ();
            if (folderPath == null)
            {
                Debug.Log ($"Failed to save heightmap, couldn't get current unpacked level path");
                return;
            }

            var sizeX = boundsFull.x;
            var sizeY = boundsFull.y;
            var sizeZ = boundsFull.z;

            if (heightfield == null || (heightfield.GetLength (0) != sizeX && heightfield.GetLength (1) != sizeZ))
                heightfield = new int[sizeX, sizeZ];

            ProceduralMeshUtilities.CollectSurfacePoints (this, heightfield);

            heightfieldPalette.Clear ();

            Color32[] colorArray = new Color32[heightfield.Length];
            textureDepthmap = new Texture2D (sizeX, sizeZ, TextureFormat.RGB24, false);

            float terrainOffsetTopRamp = -1f / 3f;

            int sizeYMinusOne = sizeY - 1;
            for (int x = 0; x < sizeX; x++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    var colorIndex = z * sizeX + x;

                    int depth = heightfield[x, z];
                    byte depthScaled = (byte)(255 - Mathf.Clamp (depth * 10, 0, 255) - (255 - sizeYMinusOne * 10));

                    byte slopeInfo = (byte)0;
                    byte roadInfo = (byte)0;

                    // Try to hit a surface point
                    var posIndex = new Vector3Int (x, 0, z);
                    var pointIndex = TilesetUtility.GetIndexFromVolumePosition (posIndex, boundsFull);
                    var pointCurrent = points[pointIndex];
                    int iteration = 0;

                    while (true)
                    {
                        if (pointCurrent == null)
                            break;

                        var pointStateCurrent = pointCurrent.pointState;
                        if (pointStateCurrent == AreaVolumePointState.Full)
                        {
                            var pointAboveStartEmpty = pointCurrent?.pointsWithSurroundingSpots[3];
                            if (pointAboveStartEmpty != null && pointAboveStartEmpty.terrainOffset.RoughlyEqual (terrainOffsetTopRamp))
                                slopeInfo = (byte)255;

                            if (pointCurrent.road)
                                roadInfo = (byte)255;

                            break;
                        }

                        // Get point below
                        pointCurrent = pointCurrent.pointsInSpot[4];
                        iteration += 1;

                        if (iteration > sizeY)
                            break;
                    }

                    var color = new Color32 (depthScaled, slopeInfo, roadInfo, (byte)255);
                    colorArray[colorIndex] = color;

                    if (!colorValues.Contains (depthScaled))
                    {
                        colorValues.Add (depthScaled);
                        heightfieldPalette.Add (new DepthColorLink
                        {
                            color = color,
                            depth = depth,
                            depthScaled = depthScaled
                        });
                    }
                }
            }

            heightfieldPalette.Sort ((x, y) => x.depth.CompareTo (y.depth));

            /*
            for (int i = 0; i < sizeY; ++i)
            {
                int depth = i;
                var depthNormalized = 1f - Mathf.Clamp01 ((float)depth / sizeYMinusOne);
                var color = new Color (depthNormalized, depthNormalized, depthNormalized, 1f);
                heightfieldPalette.Add (new DepthColorLink
                {
                    color = color,
                    depth = depth
                });
            }
            */

            textureDepthmap.name = $"{areaName}_heightmap";
            textureDepthmap.SetPixels32 (colorArray);
            textureDepthmap.Apply ();
            textureDepthmap.filterMode = FilterMode.Point;
            textureDepthmap.wrapMode = TextureWrapMode.Clamp;

            var texturePath = $"{folderPath}/heightmap.png";

            try
            {
                byte[] png = textureDepthmap.EncodeToPNG ();
                System.IO.File.WriteAllBytes ($"{texturePath}", png);
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat ($"Area manager | Encountered an exception while saving heightmap {texturePath}\n{0}", e);
            }
        }

        [ContextMenu ("Import height from tex. (R depth)")][Button]
        public void ImportHeightFromTexture ()
        {
            var folderPath = DataMultiLinkerCombatArea.GetCurrentUnpackedLevelPath ();
            if (folderPath == null)
            {
                Debug.Log ($"Failed to import height, couldn't get current unpacked level path");
                return;
            }

            var sizeX = boundsFull.x;
            var sizeY = boundsFull.y;
            var sizeZ = boundsFull.z;
            var texturePath = $"{folderPath}/heightmap.png";

            if (!System.IO.File.Exists (texturePath))
            {
                Debug.LogWarning ($"Area manager | File doesn't exist: {texturePath}");
                return;
            }

            try
            {
                byte[] pngBytes = System.IO.File.ReadAllBytes (texturePath);
                textureDepthmap = new Texture2D (sizeX, sizeZ, TextureFormat.RGB24, false, false);
                textureDepthmap.name = $"{areaName}_heightmap";
                textureDepthmap.filterMode = FilterMode.Point;
                textureDepthmap.wrapMode = TextureWrapMode.Clamp;
                textureDepthmap.LoadImage (pngBytes);
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat ($"Area manager | Encountered an exception while loading heightmap {texturePath}\n{0}", e);
            }

            if (textureDepthmap.width != sizeX || textureDepthmap.height != sizeZ)
            {
                Debug.LogError ($"Area manager | Unexpected heightmap resolution {textureDepthmap.width}x{textureDepthmap.height} (expected {sizeX}x{sizeZ}) at {texturePath}\n{0}");
                return;
            }

            Color32[] colorArray = textureDepthmap.GetPixels32 ();
            StringBuilder sb = new StringBuilder ();
            List<int> pointIndexesModified = new List<int> (points.Count);
            int sizeYMinusOne = sizeY - 1;

            for (int x = 0; x < sizeX; x++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    var colorIndex = z * sizeX + x;
                    var color = colorArray[colorIndex];
                    int depthSample = color.r;
                    int depthSampleShifted = depthSample + (255 - sizeYMinusOne * 10);
                    var depthRestored = Mathf.RoundToInt ((255 - depthSampleShifted) * 0.1f);

                    var posIndex = new Vector3Int (x, 0, z);
                    var index = TilesetUtility.GetIndexFromVolumePosition (posIndex, boundsFull);
                    var pointCurrent = points[index];
                    int iteration = 0;

                    bool surfacePointChecked = false;
                    while (true)
                    {
                        if (pointCurrent == null)
                            break;

                        int depthCurrent = pointCurrent.pointPositionIndex.y;
                        bool aboveHeightmap = depthCurrent <= depthRestored;
                        var pointStateExpected = aboveHeightmap ? AreaVolumePointState.Empty : AreaVolumePointState.Full;
                        var pointStateCurrent = pointCurrent.pointState;

                        if (pointStateCurrent != pointStateExpected)
                        {
                            pointIndexesModified.Add (pointCurrent.spotIndex);
                            pointCurrent.pointState = pointStateExpected;
                            sb.Append ($"\n- {iteration} | Depth: {depthCurrent} | Above heightmap: {aboveHeightmap} | State expected: {pointStateExpected}");
                        }
                        else if (pointStateExpected == AreaVolumePointState.Full)
                        {
                            sb.Append ($"\n- {iteration} | Depth: {depthCurrent} | Full point at expected depth, no need to proceed further");
                            break;
                        }
                        else
                        {
                            sb.Append ($"\n- {iteration} | Depth: {depthCurrent} | Empty point at expected depth");
                        }

                        // Get point below
                        pointCurrent = pointCurrent.pointsInSpot[4];
                        iteration += 1;

                        if (iteration > sizeY)
                        {
                            sb.Append ($"\n- {iteration} | Depth: {depthCurrent} | Depth exceeded limit of {sizeY}, breaking out");
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < pointIndexesModified.Count; ++i)
            {
                int index = pointIndexesModified[i];
                AreaVolumePoint point = points[index];

                if (point == null)
                    continue;

                for (int s = 0; s < 8; ++s)
                {
                    AreaVolumePoint pointWithNeighbourSpot = point.pointsWithSurroundingSpots[s];
                    if (pointWithNeighbourSpot == null)
                        continue;

                    pointWithNeighbourSpot.blockFlippedHorizontally = false;
                    pointWithNeighbourSpot.blockRotation = 0;
                    pointWithNeighbourSpot.blockGroup = 0;
                    pointWithNeighbourSpot.blockSubtype = 0;
                    pointWithNeighbourSpot.blockTileset = AreaTilesetHelper.idOfTerrain;

                    RemovePropPlacement (pointWithNeighbourSpot.spotIndex);
                }
            }

            RebuildEverything ();
            Debug.Log ($"Height import completed. Points altered based on height (R): {pointIndexesModified.Count}");
        }

        [ContextMenu ("Import slopes from tex. (G=127 remove, G=255 add)")][Button]
        public void ImportRampsFromTexture ()
        {
            var folderPath = DataMultiLinkerCombatArea.GetCurrentUnpackedLevelPath ();
            if (folderPath == null)
            {
                Debug.Log ($"Failed to import slopes, couldn't get current unpacked level path");
                return;
            }

            var sizeX = boundsFull.x;
            var sizeY = boundsFull.y;
            var sizeZ = boundsFull.z;
            var texturePath = $"{folderPath}/heightmap.png";

            if (!System.IO.File.Exists (texturePath))
            {
                Debug.LogWarning ($"Area manager | File doesn't exist: {texturePath}");
                return;
            }

            try
            {
                byte[] pngBytes = System.IO.File.ReadAllBytes (texturePath);
                textureDepthmap = new Texture2D (sizeX, sizeZ, TextureFormat.RGB24, false, false);
                textureDepthmap.name = $"{areaName}_heightmap";
                textureDepthmap.filterMode = FilterMode.Point;
                textureDepthmap.wrapMode = TextureWrapMode.Clamp;
                textureDepthmap.LoadImage (pngBytes);
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat ($"Area manager | Encountered an exception while loading heightmap {texturePath}\n{0}", e);
            }

            if (textureDepthmap.width != sizeX || textureDepthmap.height != sizeZ)
            {
                Debug.LogError ($"Area manager | Unexpected heightmap resolution {textureDepthmap.width}x{textureDepthmap.height} (expected {sizeX}x{sizeZ}) at {texturePath}\n{0}");
                return;
            }

            Color32[] colorArray = textureDepthmap.GetPixels32 ();
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
                    var index = TilesetUtility.GetIndexFromVolumePosition (posIndex, boundsFull);
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

        [ContextMenu ("Import roads from tex. (B)")][Button]
        public void ImportRoadsFromTexture ()
        {
            var folderPath = DataMultiLinkerCombatArea.GetCurrentUnpackedLevelPath ();
            if (folderPath == null)
            {
                Debug.Log ($"Failed to import roads, couldn't get current unpacked level path");
                return;
            }

            var sizeX = boundsFull.x;
            var sizeY = boundsFull.y;
            var sizeZ = boundsFull.z;
            var texturePath = $"{folderPath}/heightmap.png";

            if (!System.IO.File.Exists (texturePath))
            {
                Debug.LogWarning ($"Area manager | File doesn't exist: {texturePath}");
                return;
            }

            try
            {
                byte[] pngBytes = System.IO.File.ReadAllBytes (texturePath);
                textureDepthmap = new Texture2D (sizeX, sizeZ, TextureFormat.RGB24, false, false);
                textureDepthmap.name = $"{areaName}_heightmap";
                textureDepthmap.filterMode = FilterMode.Point;
                textureDepthmap.wrapMode = TextureWrapMode.Clamp;
                textureDepthmap.LoadImage (pngBytes);
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat ($"Area manager | Encountered an exception while loading heightmap {texturePath}\n{0}", e);
            }

            if (textureDepthmap.width != sizeX || textureDepthmap.height != sizeZ)
            {
                Debug.LogError ($"Area manager | Unexpected heightmap resolution {textureDepthmap.width}x{textureDepthmap.height} (expected {sizeX}x{sizeZ}) at {texturePath}\n{0}");
                return;
            }

            Color32[] colorArray = textureDepthmap.GetPixels32 ();
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
                    var index = TilesetUtility.GetIndexFromVolumePosition (posIndex, boundsFull);
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
            Circle3x3,
            Circle5x5
        }

        private static List<AreaVolumePoint> pointsToEdit = new List<AreaVolumePoint> ();

        private static void TryAddingPointInRange (AreaVolumePoint p, Vector2Int depthRange)
        {
            if (p == null)
                return;

            bool depthChecked = depthRange.y > depthRange.x;
            if (depthChecked)
            {
                if (p.pointPositionIndex.y < depthRange.x || p.pointPositionIndex.y > depthRange.y)
                    return;
            }

            pointsToEdit.Add (p);
        }

        public static List<AreaVolumePoint> CollectPointsInBrush (AreaVolumePoint pointStart, EditingVolumeBrush brush, int depth = 1, bool depthUp = false, Vector2Int depthRange = default)
        {
            pointsToEdit.Clear ();
            TryAddingPointInRange (pointStart, depthRange);

            depth = Mathf.Clamp (depth, 1, 20);

            if (brush == EditingVolumeBrush.Circle3x3 || brush == EditingVolumeBrush.Square3x3 || brush == EditingVolumeBrush.Circle5x5)
            {
                // X+
                TryAddingPointInRange (pointStart.pointsInSpot[1], depthRange);

                // Z+
                TryAddingPointInRange (pointStart.pointsInSpot[2], depthRange);

                // X-
                TryAddingPointInRange (pointStart.pointsWithSurroundingSpots[6], depthRange);

                // Z-
                TryAddingPointInRange (pointStart.pointsWithSurroundingSpots[5], depthRange);
            }

            if (brush == EditingVolumeBrush.Square3x3 || brush == EditingVolumeBrush.Circle5x5)
            {
                // X+ & Z+
                TryAddingPointInRange (pointStart.pointsInSpot[1]?.pointsInSpot[2], depthRange);

                // X- & Z+
                TryAddingPointInRange (pointStart.pointsWithSurroundingSpots[6]?.pointsInSpot[2], depthRange);

                // X- & Z-
                TryAddingPointInRange (pointStart.pointsWithSurroundingSpots[6]?.pointsWithSurroundingSpots[5], depthRange);

                // X+ & Z-
                TryAddingPointInRange (pointStart.pointsInSpot[1]?.pointsWithSurroundingSpots[5], depthRange);
            }

            if (brush == EditingVolumeBrush.Circle5x5)
            {
                // 2X+
                var p1 = pointStart.pointsInSpot[1]?.pointsInSpot[1];
                TryAddingPointInRange (p1, depthRange);
                TryAddingPointInRange (p1?.pointsInSpot[2], depthRange);
                TryAddingPointInRange (p1?.pointsWithSurroundingSpots[5], depthRange);

                // 2Z+
                var p2 = pointStart.pointsInSpot[2]?.pointsInSpot[2];
                TryAddingPointInRange (p2, depthRange);
                TryAddingPointInRange (p2?.pointsInSpot[1], depthRange);
                TryAddingPointInRange (p2?.pointsWithSurroundingSpots[6], depthRange);

                // 2X-
                var p3 = pointStart.pointsWithSurroundingSpots[6]?.pointsWithSurroundingSpots[6];
                TryAddingPointInRange (p3, depthRange);
                TryAddingPointInRange (p3?.pointsInSpot[2], depthRange);
                TryAddingPointInRange (p3?.pointsWithSurroundingSpots[5], depthRange);

                // 2Z-
                var p4 = pointStart.pointsWithSurroundingSpots[5]?.pointsWithSurroundingSpots[5];
                TryAddingPointInRange (p4, depthRange);
                TryAddingPointInRange (p4?.pointsInSpot[1], depthRange);
                TryAddingPointInRange (p4?.pointsWithSurroundingSpots[6], depthRange);
            }

            if (brush == EditingVolumeBrush.Square2x2)
            {
                // X-
                TryAddingPointInRange (pointStart.pointsWithSurroundingSpots[6], depthRange);

                // Z-
                TryAddingPointInRange (pointStart.pointsWithSurroundingSpots[5], depthRange);

                // X- & Z-
                TryAddingPointInRange (pointStart.pointsWithSurroundingSpots[6]?.pointsWithSurroundingSpots[5], depthRange);
            }

            if (depth > 1)
            {
                for (int p = 0, pLimit = pointsToEdit.Count; p < pLimit; ++p)
                {
                    var pointTop = pointsToEdit[p];
                    for (int i = 0, iLimit = depth - 1; i < iLimit; ++i)
                    {
                        var pointBelow = depthUp ? pointTop.pointsWithSurroundingSpots[3] : pointTop.pointsInSpot[4];
                        if (pointBelow == null)
                            break;

                        TryAddingPointInRange (pointBelow, depthRange);
                        pointTop = pointBelow;
                    }
                }
            }

            return pointsToEdit;
        }

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

        [ContextMenu ("Import props from texture")][Button]
        public void ImportPropsFromTexture ()
        {
            var folderPath = DataMultiLinkerCombatArea.GetCurrentUnpackedLevelPath ();
            if (folderPath == null)
            {
                Debug.Log ($"Failed to import props, couldn't get current unpacked level path");
                return;
            }

            var sizeX = boundsFull.x;
            var sizeY = boundsFull.y;
            var sizeZ = boundsFull.z;
            var sizeXDouble = sizeX * 2;
            var sizeZDouble = sizeZ * 2;
            var texturePath = $"{folderPath}/mask_vegetation.png";

            if (!System.IO.File.Exists (texturePath))
            {
                Debug.LogWarning ($"Area manager | File doesn't exist: {texturePath}");
                return;
            }

            try
            {
                byte[] pngBytes = System.IO.File.ReadAllBytes (texturePath);
                textureMaskVegetation = new Texture2D (sizeXDouble, sizeZDouble, TextureFormat.RGB24, false, false);
                textureMaskVegetation.name = $"{areaName}_mask_vegetation";
                textureMaskVegetation.filterMode = FilterMode.Point;
                textureMaskVegetation.wrapMode = TextureWrapMode.Clamp;
                textureMaskVegetation.LoadImage (pngBytes);
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat ($"Area manager | Encountered an exception while loading vegetation mask {texturePath}\n{0}", e);
            }

            if (textureMaskVegetation.width != sizeXDouble || textureMaskVegetation.height != sizeZDouble)
            {
                Debug.LogError ($"Area manager | Unexpected heightmap resolution {textureMaskVegetation.width}x{textureMaskVegetation.height} (expected {sizeXDouble}x{sizeZDouble}) at {texturePath}\n{0}");
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

        public void SetRampsEverywhere (SlopeProximityCheck proximityCheck)
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
                ImportRampsFromTexture ();
            else
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
                            RebuildBlock (pointWithNeighbourSpot);
                            RebuildCollisionForPoint (pointWithNeighbourSpot);
                        }
                    }

                    if (selectiveUpdates)
                        UpdateDamageAroundPoint (pointStartFull);
                }

                if (selectiveUpdates)
                {
                    var sceneHelper = CombatSceneHelper.ins;
                    sceneHelper.terrain.Rebuild (true);
                }
            }
        }

        public void EditRoad (int indexStart, RoadEditingOperation operation, EditingVolumeBrush editingVolumeBrush)
        {
            var pointStart = points[indexStart];
            var pointsToEdit = CollectPointsInBrush (pointStart, editingVolumeBrush);

            bool terrainModified = false;
            bool roadAdded = operation == RoadEditingOperation.Add;
            bool roadRemoved = operation == RoadEditingOperation.Remove;
            bool roadFloodFill = operation == RoadEditingOperation.FloodFill;
            bool roadSubtypeNext = operation == RoadEditingOperation.SubtypeNext;
            bool roadSubtypePrev = operation == RoadEditingOperation.SubtypePrev;

            if (roadAdded || roadRemoved)
            {
                for (int i = 0; i < pointsToEdit.Count; ++i)
                {
                    var point = pointsToEdit[i];
                    if (point.pointState != AreaVolumePointState.Full)
                    {
                        Debug.Log ("AM (I) | EditRoad | One of the core points (" + i + ") is not full, aborting");
                        return;
                    }

                    for (int a = 0; a < 4; ++a)
                    {
                        var pointAbove = pointStart.pointsWithSurroundingSpots[a];
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
                        for (int s = 0; s < 8; ++s)
                        {
                            AreaVolumePoint pointWithNeighbourSpot = point.pointsWithSurroundingSpots[s];
                            if (pointWithNeighbourSpot == null)
                                continue;

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
                for (int i = 0; i < pointsToEdit.Count; ++i)
                    pointsToEdit[i].road = true;

                for (int i = 0; i < pointsToEdit.Count; ++i)
                    UpdateRoadConfigurations (pointsToEdit[i], roadSubtype);
            }
            else if (roadRemoved)
            {
                for (int i = 0; i < pointsToEdit.Count; ++i)
                    pointsToEdit[i].road = false;

                for (int i = 0; i < pointsToEdit.Count; ++i)
                    UpdateRoadConfigurations (pointsToEdit[i], roadSubtype);
            }
            else if (roadFloodFill)
            {
                FloodFillRoadSubtype (pointsToEdit, roadSubtype);
                terrainModified = true;
            }

            else if (roadSubtypeNext || roadSubtypePrev)
            {
                int roadSubtypeInt = (int)roadSubtype;
                roadSubtypeInt += roadSubtypeNext ? 10 : -10;

                if (roadSubtypeInt > 30)
                    roadSubtypeInt = 0;
                else if (roadSubtypeInt < 0)
                    roadSubtypeInt = 30;

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
								RebuildBlock (pointInfo.pt);
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
					RebuildBlock (pointAbove);
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
                    RebuildBlock (pointAbove);
                }
            }
        }
    }
}
