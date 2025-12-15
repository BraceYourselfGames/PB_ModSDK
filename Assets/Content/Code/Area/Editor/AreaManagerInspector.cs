using System;
using UnityEngine;
using UnityEditor;

using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using CustomRendering;
using Pathfinding;
using PhantomBrigade.Data;
using Unity.Entities;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;
using YamlDotNet.Core.Tokens;

namespace Area
{
    [CustomEditor (typeof (AreaManager))]
    public class AreaManagerInspector : Editor
    {
        public enum EditingTarget
        {
            Volume = 0,
            Tileset = 1,
            Spot = 2,
            Transfer = 3,
            Damage = 7,
            Color = 8,
            Props = 9,
            Navigation = 11,
            Roads = 20,
			RoadCurves = 21,
			TerrainRamp = 30
        }

        public static AreaManager am;
        public static List<string> pathsToAreas;

        public static string editingBrushFolder = "Assets/Content/Objects/TilesetTest/TilesetsBrushes";

        public static bool foldoutStats = true;
        public static bool foldoutLoading = false;
        public static bool foldoutUpdates = true;
        public static bool foldoutTilesets = false;
        public static bool foldoutEditing = true;
        public static bool foldoutBrushes = false;
        public static bool foldoutDebug = true;
        public static bool foldoutProps = false;
		public static bool foldoutGenProps = false;
		public static bool foldoutColorPalette = false;
        public static bool foldoutVolumeSnippets = false;
        public static bool foldoutVC = false;
        public static bool foldoutDefault = false;

        private static int overrideIndexTo = 0;
        private static int overrideIndexFrom = 0;

        public static bool blockCopyStrict = false;

        public static bool showLastSearchResults = true;
        public static bool showStructuralAnalysis = false;

        private static int helperLayer = 15; // "Editor" layer
        //private int helperLayerMask = 1 << 15;

        //private int environmentLayer = 8; // "Environment" layer
        private static int environmentLayerMask = 1 << 8;

        private static Color colorLinkHorizontal = new Color (1.0f, 0.25f, 0.25f);
        private static Color colorLinkDiagonal = new Color (1.0f, 0.50f, 0.25f);
        private static Color colorLinkJumpUp = new Color (0.25f, 1.0f, 0.25f);
        private static Color colorLinkJumpDown = new Color (0.25f, 0.45f, 0.9f);
        private static Color colorLinkJumpOverDrop = new Color (0.8f, 0.8f, 0.25f);
        private static Color colorLinkJumpOverClimb = new Color (0.6f, 0.8f, 0.4f);

        private static Color colorAxisXPos = new Color (1f, 0.5f, 0.5f, 1f);
        private static Color colorAxisXNeg = new Color (1f, 0.4f, 0.8f, 1f);

        private static Color colorAxisZPos = new Color (0.6f, 0.8f, 1f, 1f);
        private static Color colorAxisZNeg = new Color (0.2f, 0.8f, 1f, 1f);

        private static Color colorAxisYPos = new Color (0.5f, 1f, 0.6f, 1f);

        private static Color colorOffsetNeutral = new Color (1f, 1f, 1f, 0.25f);
        private static Color colorOffsetPos = new Color (0.25f, 1.0f, 0.25f, 1f);
        private static Color colorOffsetNeg = new Color (1.0f, 0.50f, 0.25f, 1f);

        private static Color colorVolumeMainDestructibleUntracked = new Color (0.65f, 1f, 0.35f, 1f);
        private static Color colorVolumeMainDestructible = new Color (0.5f, 1f, 1f, 1f);
        private static Color colorVolumeMainIndestructibleIndr = new Color (0.7f, 0.7f, 1, 1f);
        private static Color colorVolumeMainIndestructibleFlag = new Color (1f, 0.5f, 0.2f, 1f);
        private static Color colorVolumeMainIndestructibleHard = new Color (1f, 0.35f, 0.55f, 1f);


        private static Color colorVolumeFadedIndestructibleFlag = new Color (1f, 0.5f, 0.2f, 0.1f);
        private static Color colorVolumeFadedIndestructibleHard = new Color (1f, 0.35f, 0.55f, 0.1f);

        private static Color colorVolumeNeighborSecondary = new Color (0.2f, 0.8f, 1f, 0.2f);
        private static Color colorVolumeNeighborPrimaryDestructibleUntracked = new Color (0.65f, 1f, 0.35f, 0.4f);
        private static Color colorVolumeNeighborPrimaryDestructible = new Color (0.5f, 1f, 1f, 0.4f);
        private static Color colorVolumeNeighborPrimaryIndestructibleFlag = new Color (1f, 0.5f, 0.2f, 0.4f);
        private static Color colorVolumeNeighborPrimaryIndestructibleHard = new Color (1f, 0.35f, 0.55f, 0.4f);

        private static string boundsLastCachedForAreaName = string.Empty;
        private static bool boundsFullCacheUpdated = false;
        private static Vector3Int boundsFullCached = new Vector3Int (2, 2, 2);

        private static bool setupPerformed = false;
        private static string[] areaKeyArray = new string[1];

        private static GameObject rcCursorObject;
        private static GameObject rcSelectionObject;
        private static GameObject rcGlowObject;

        private static MaterialPropertyBlock rcMpb = null;

        private static Material rcCursorMaterial;
        private static Material rcSelectionMaterial;
        private static Material rcGlowMaterial;

        private static Vector3 rcCursorTargetPosition;
        private static Vector3 rcCursorSmoothPosition;
        private static Vector3 rcCursorVelocityPosition;

        private static Vector3 rcGlowTargetPosition;
        private static Vector3 rcGlowSmoothPosition;
        private static Vector3 rcGlowVelocityPosition;

        private static Quaternion rcCursorTargetRotation;
        private static Quaternion rcCursorSmoothRotation;
        private static Quaternion rcCursorVelocityRotation;

        private static Vector3 rcSelectionTargetPosition;
        private static Vector3 rcSelectionSmoothPosition;
        private static Vector3 rcSelectionVelocityPosition;

        private static float updateTimeLast = 0f;
        private static float updateTimeDelta = 0f;
        private static bool checkPropConfiguration = true;
        private static bool spawnPropsWithAutorotation = false;
        private static bool spawnPropsWithClipboardColor = false;

        private static float navLinkSeparation = 0.15f;

        public static EditingTarget editingTarget = EditingTarget.Spot;
        public static AreaTileset editingTilesetSelected;
        private static string clipboardNameToLoad = "name_to_load";

        public static bool volumeBrushDepthGoesUp = true;
        public static Vector2Int volumeBrushDepthRange = new Vector2Int (0, 100);

        public static int volumeBrushDepth = 1;
        public static bool displayDestructibility = false;
        public static bool propagateDestructibilityDown = false;
        public static bool allowIndestructibleDestruction = false;
        public static bool swapTilesetOnVolumeEdits;
        public static bool swapColorOnVolumeEdits;
		public static bool applyOverlaysOnColorApply = false;
        public static bool applyMainOnColorApply = true;

        public static TilesetMultiBlockDefinition editingBrushReference;
        public static TilesetMultiBlockDefinition editingMultiblockReference;
        public static bool editingMultiblockUseBrush = true;
        public static int editingMultiblockRotation = 0;

        //private static byte lastSpotConfiguration = 0;
        //private static int lastBlockRotation = 0;
        //private static bool lastBlockFlipped = false;

        private static int[] reusedIntArray_OnVolumeChanged;
        private static Vector3 spotRaycastHitOffset = new Vector3 (-1.5f, 1.5f, -1.5f);

        private static GameObject propCursorInstance;
        private static AreaProp propPreviewInstance;
        private static GameObject propHolder;

        private static AreaPlacementProp propPlacementHandled;
        private static int propPlacementListIndex;
        private static int propSpotIndexVisualized;
        private static int propSelectionID;
        private static int propIndex;
        private static int propIndexVisualized = -1;

        private static byte propRotationInternal = (byte)0;

        private static byte propRotation
        {
            get => propRotationInternal;
            set
            {
                // Debug.Log ($"New prop rotation: {value}");
                propRotationInternal = value;
            }
        }

        private static int propRotationVisualized;
        private static bool propFlipped;
        private static bool propFlippedVisualized;
        private static float propOffsetX;
        private static float propOffsetZ;
        private static float propOffsetXVisualized;
        private static float propOffsetZVisualized;
        private static Vector4 propHSBPrimary = Constants.defaultHSBOffset;
        private static Vector4 propHSBSecondary = Constants.defaultHSBOffset;
        private static Vector4 propHSBPrimaryVisualized = Constants.defaultHSBOffset;
        private static Vector4 propHSBSecondaryVisualized = Constants.defaultHSBOffset;
        private static float propOffsetXClipboard = 0f;
        private static float propOffsetZClipboard = 0f;

        private static string pathToBackgroundsFull = "Assets/Resources/Content/AreaBackgrounds";
        private static List<AreaConfigurationData> configurationDataForBlocks;

        private static int interactionDistance = 1100;
        private static float interactionDistanceNav = 50f;

        public static List<byte> clipboardConfigurations = new List<byte> ();
        public static int clipboardTileset = 0;
        public static byte clipboardGroup = 0;
        public static byte clipboardSubtype = 0;
        public static byte clipboardRotation = 0;
        public static bool clipboardFlipping = false;
        public static TilesetVertexProperties clipboardColor = TilesetVertexProperties.defaults;
        public static bool clipboardMustOverwriteSubtype = true;
		public static bool clipboardOverwriteColor = false;

        public enum PropEditingMode
        {
            Place,
            Color
        }

        private static PropEditingMode propEditingMode = PropEditingMode.Place;

        private static Vector4 clipboardPropHSBPrimary;
        private static Vector4 clipboardPropHSBSecondary;

		private static List<int> propGenerationIds = new List<int>();
		private static int newPropId = 0;
		private static float propGenChance = 0.1f;
        private static bool propGenRequireFlatSurroundings = true;
        private static float propGenOffsetRange = 0f;

		private static bool absoluteColorMode = true;
		private static int selectedTilesetId = 0;
		private static HSBColor selectedPrimaryColor;
		private static HSBColor selectedSecondaryColor;
		private static float overrideValue = 0f;

        MaterialPropertyBlock _propBlockAcc;
        private static readonly int ID_InstancePropsOverride = Shader.PropertyToID ("_InstancePropsOverride");
        private static readonly int ID_HsbOffsetsPrimary = Shader.PropertyToID ("_HSBOffsetsPrimary");
        private static readonly int ID_HsbOffsetsSecondary = Shader.PropertyToID ("_HSBOffsetsSecondary");
        private static readonly int ID_PackedPropData = Shader.PropertyToID ("_PackedPropData");
        private static readonly Vector4 packedPropDataDefault = new Vector4 (1.0f, 0.0f, 0.0f, 1.0f);

		private class PaletteEntry
		{
			public int tilesetId;
            public String tilesetDescription;
			public HSBColor primaryColor;
			public HSBColor secondaryColor;

			public PaletteEntry()
			{
				tilesetId = 0;
                tilesetDescription = "";
				primaryColor = HSBColor.FromColor(Color.gray);
				secondaryColor = HSBColor.FromColor(Color.gray);
			}

			public PaletteEntry(PaletteEntry other)
			{
				tilesetId = other.tilesetId;
                tilesetDescription = other.tilesetDescription;
				primaryColor = other.primaryColor;
				secondaryColor = other.secondaryColor;
			}
		}

		private static PaletteEntry newPaletteEntry = new PaletteEntry();
		private static List<PaletteEntry> colorPalette = new List<PaletteEntry>();

        private static List<string> volumeSnippetKeys = new List<string>();

		private static string propFilter = "";

		private void CheckSetup ()
        {
	        EditorApplication.update -= UpdateInEditorApplication;
	        EditorApplication.update += UpdateInEditorApplication;

            if (setupPerformed && editingTilesetSelected != null)
                return;

            setupPerformed = true;

            editingTarget = EditingTarget.Spot;
            // AreaNavUtility.GetNavigationNodes (ref PhantomNavGraph.areaNodes, target as AreaManager);

            AreaTilesetHelper.CheckResources ();
            AreaAssetHelper.CheckResources ();

            if (AreaAssetHelper.propsPrototypesList.Count > 0)
                propSelectionID = AreaAssetHelper.propsPrototypesList[0].id;

            if (editingTilesetSelected == null)
                editingTilesetSelected = AreaTilesetHelper.database.tilesetFallback;

            // RefreshStuffForLevelLoad ();

			var defaultConfig = TilesetVertexProperties.defaults;
            selectedPrimaryColor = new HSBColor(defaultConfig.huePrimary, defaultConfig.saturationPrimary, defaultConfig.brightnessPrimary);
            selectedSecondaryColor = new HSBColor(defaultConfig.hueSecondary, defaultConfig.saturationSecondary, defaultConfig.brightnessSecondary);

            LoadColorPalette();
        }

        private void UpdateInEditorApplication ()
        {
            if (rcCursorObject != null && rcCursorObject.activeSelf)
            {
                updateTimeDelta = Time.realtimeSinceStartup - updateTimeLast;
                updateTimeLast = Time.realtimeSinceStartup;

                rcCursorSmoothPosition = Vector3.SmoothDamp (rcCursorSmoothPosition, rcCursorTargetPosition, ref rcCursorVelocityPosition, 0.05f, 100000f, Time.unscaledDeltaTime);
                rcCursorSmoothRotation = UtilityQuaternion.SmoothDamp (rcCursorSmoothRotation, rcCursorTargetRotation, ref rcCursorVelocityRotation, 0.05f, 100000f, updateTimeDelta);

                rcCursorObject.transform.position = rcCursorSmoothPosition;
                rcCursorObject.transform.rotation = rcCursorSmoothRotation;
            }

            if (rcSelectionObject != null && rcSelectionObject.activeSelf)
            {
                rcSelectionSmoothPosition = Vector3.SmoothDamp (rcSelectionSmoothPosition, rcSelectionTargetPosition, ref rcSelectionVelocityPosition, 0.05f, 100000f, updateTimeDelta);
                rcSelectionObject.transform.position = rcSelectionSmoothPosition;
            }

            if (rcGlowObject != null && rcSelectionObject.activeSelf)
            {
                rcGlowSmoothPosition = Vector3.SmoothDamp (rcGlowSmoothPosition, rcGlowTargetPosition, ref rcGlowVelocityPosition, 0.05f, 100000f, updateTimeDelta);
                rcGlowObject.transform.position = rcGlowSmoothPosition;
            }

            // Debug.Log ($"EAU | RC Selection: {rcSelectionObject.ToStringNullCheck ()} | Active: {rcSelectionObject != null && rcSelectionObject.activeSelf}");
        }

        private static byte[] bytesToTest = new byte[] { 1, 2, 4, 8, 51, 102, 153, 204, 48, 96, 144, 192, 15, 240 };
        private static string[] textToTest = new string[] { "roof edge", "roof edge", "roof edge", "roof edge", "wall", "wall", "wall", "wall", "ceiling edge", "ceiling edge", "ceiling edge", "ceiling edge", "floor", "ceiling" };

        private static bool IsTopEmpty (byte b)
        {
            // return (b & (16 | 32 | 64 | 128)) == 0;
            return (b & 240) == 0;
        }

        private void LoadVolumeSnippets()
        {
            var path = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), "Configs/AreaSnippets");
            var list = UtilitiesYAML.GetFileList (path);

            volumeSnippetKeys.Clear ();

            if (list != null)
                volumeSnippetKeys.AddRange (list);
        }

        private void SaveColorPalette()
		{
			UtilitiesYAML.SaveDataToFile ("Configs/LevelEditor", "colorPalette.yaml", colorPalette);
		}

		private void LoadColorPalette()
		{
			var list = UtilitiesYAML.LoadDataFromFile<List<PaletteEntry>>("Configs/LevelEditor/colorPalette.yaml");

			if(list != null)
			{
				colorPalette.Clear();
				colorPalette.AddRange(list);
			}
			else
			{
				Debug.LogWarning($"Could not load color palette");
			}
		}

        private void SaveColorPaletteProps()
		{
			UtilitiesYAML.SaveDataToFile ("Configs/LevelEditor", "colorPaletteProps.yaml", colorPalette);
		}

		private void LoadColorPaletteProps()
		{
			var list = UtilitiesYAML.LoadDataFromFile<List<PaletteEntry>>("Configs/LevelEditor/colorPaletteProps.yaml");

			if(list != null)
			{
				colorPalette.Clear();
				colorPalette.AddRange(list);
			}
			else
			{
				Debug.LogWarning($"Could not load color palette for props");
			}
		}

        private string DrawStringDropdown (string keySelected, string label, string[] keys)
        {
            if (keys == null || keys.Length == 0)
                return keySelected;

            int index = -1;
            for (int i = 0, count = keys.Length; i < count; ++i)
            {
                var keyCandidate = keys[i];
                if (string.Equals (keyCandidate, keySelected))
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                index = 0;
                keySelected = keys[0];
            }

            GUILayout.BeginHorizontal ();
            GUILayout.Label (label, EditorStyles.miniLabel, GUILayout.MaxWidth (80f));

            int indexNew = EditorGUILayout.Popup (index, keys);
            if (indexNew != index)
                keySelected = keys[indexNew];

            GUILayout.EndHorizontal ();

            return keySelected;
        }

        public override void OnInspectorGUI ()
        {
            if (am == null)
                am = target as AreaManager;

            if (am == null)
                am = CombatSceneHelper.ins.areaManager;

            if (am == null)
            {
                EditorGUILayout.HelpBox ("No inspector instance", MessageType.Warning);
                return;
            }

            CheckSetup ();

            EditorGUIUtility.labelWidth = 180f;

            float columnWidth = (Screen.width / 2f - 26f);

            var dataSelected = DataMultiLinkerCombatArea.selectedArea;
            bool dataPresent = dataSelected?.content?.core != null && am.points != null && am.points.Count > 0;

            // Property options start

            var data = DataMultiLinkerCombatArea.data;
            if (data != null && data.Count != 0)
            {
                var obj = DataMultiLinkerUtility.FindLinkerAsInterface (typeof (DataContainerCombatArea));
                var linker = obj != null ? obj as DataMultiLinkerCombatArea : null;
                if (linker != null && linker.dataFilteredIsolated != null && !string.IsNullOrEmpty (linker.dataFilteredIsolated.key))
                {
                    var keys = DataMultiLinkerCombatArea.GetKeys ();
                    if (keys.Count () != areaKeyArray.Length)
                        areaKeyArray = keys.ToArray ();

                    EditorGUI.BeginChangeCheck ();
                    var keyChanged = DrawStringDropdown (linker.dataFilteredIsolated.key, "Area config", areaKeyArray);
                    if (EditorGUI.EndChangeCheck () && !string.Equals (keyChanged, linker.dataFilteredIsolated.key))
                    {
                        if (dataPresent)
                            dataSelected.UnloadFromScene ();

                        linker.SetFilter (true, keyChanged, true);
                        data[keyChanged].SelectAndApplyToScene ();
                    }
                }
            }

            GUILayout.BeginVertical ("Box");

            bool boundsApplied = false; // Save bounds edit request for later
            if (dataPresent)
            {
                GUILayout.Label
                (
                    $"Volume: {am.points.Count} ({am.destructiblePointCount} destructible)\nProps: {(am.placementsProps == null ? "null" : am.placementsProps.Count.ToString ())}",
                    EditorStyles.miniLabel
                );

                GUILayout.BeginHorizontal ();
                GUILayout.Label ("Bounds", EditorStyles.miniLabel, GUILayout.MaxWidth (80f));

                boundsFullCached = dataSelected.content.core.bounds;
                int boundsX = EditorGUILayout.IntField (boundsFullCached.x);
                int boundsY = EditorGUILayout.IntField (boundsFullCached.y);
                int boundsZ = EditorGUILayout.IntField (boundsFullCached.z);
                boundsFullCached = new Vector3Int (boundsX, boundsY, boundsZ);

                bool boundsDifferent = boundsFullCached != am.boundsFull;
                GUI.enabled = boundsDifferent;
                boundsApplied = GUILayout.Button ("Apply", EditorStyles.miniButton, GUILayout.Width (80f));
                GUI.enabled = true;

                GUILayout.EndHorizontal ();
            }

            GUILayout.EndVertical ();

            // Property options end

            if (dataPresent)
            {
                GUILayout.BeginHorizontal ();

                var msg = $"Editing area: {dataSelected.key}\n\nThis is a secondary level data editor. It can not select levels independently. Open the Area database inspector to switch to a different levels, save or load.";
                EditorGUILayout.HelpBox (msg, MessageType.None);

                GUILayout.BeginVertical (GUILayout.Width (100f));
                if (GUILayout.Button ("Open parent", EditorStyles.miniButton))
                {
                    var obj = DataMultiLinkerUtility.FindLinkerAsInterface (typeof (DataContainerCombatArea));
                    if (obj != null)
                        obj.SelectObject ();
                }

                if (GUILayout.Button ("Unload", EditorStyles.miniButton))
                {
                    if (EditorUtility.DisplayDialog ("Unload?", "Are you sure you'd like to unload this level?", "Continue", "Cancel"))
                    {
                        dataSelected.UnloadFromScene ();
                    }
                }

                if (GUILayout.Button ("Reload", EditorStyles.miniButton))
                {
                    if (EditorUtility.DisplayDialog ("Reload?", "Are you sure you'd like to reload this level?", "Continue", "Cancel"))
                    {
                        var entryReloaded = DataMultiLinkerCombatArea.LoadDataIsolated (dataSelected.key);
                        if (entryReloaded != null)
                            entryReloaded.SelectAndApplyToScene ();
                    }
                }
                GUILayout.EndVertical ();

                // Another BeginVertical is needed here to avoid the button being slightly offset downwards
                GUILayout.BeginVertical (GUILayout.Width (100f));
                if (GUILayout.Button ("Save", GUI.skin.button, GUILayout.MaxHeight (58f), GUILayout.MinHeight (58f), GUILayout.MinWidth (100f), GUILayout.MaxWidth (100f)))
                {
                    dataSelected.LoadLevelContentFromScene ();
                    DataMultiLinkerCombatArea.SaveDataIsolated (dataSelected.key);
                }
                GUILayout.EndVertical ();

                GUILayout.EndHorizontal ();
            }
            else
            {
                GUILayout.BeginHorizontal ();

                var obj = DataMultiLinkerUtility.FindLinkerAsInterface (typeof (DataContainerCombatArea).FullName);
                var linker = obj != null ? obj as DataMultiLinkerCombatArea : null;

                var areaData = DataMultiLinkerCombatArea.data;
                var areaKvp = linker != null ? linker.dataFilteredIsolated : null;
                bool areaIsolatedPresent = areaKvp != null && !string.IsNullOrEmpty (areaKvp.key) && areaKvp.value != null;
                var areaIsolatedKey = areaIsolatedPresent ? areaKvp.key : "-";

                var msg = $"No loaded area.\nDB selection: {areaIsolatedKey}\n\nThis is a secondary level data editor. It can not select levels independently. Open the Area database inspector and select one of the configs to proceed.";
                EditorGUILayout.HelpBox (msg, MessageType.None);

                GUILayout.BeginVertical ();
                if (GUILayout.Button ("Open Area DB", EditorStyles.miniButton))
                {
                    if (obj != null)
                        obj.SelectObject ();
                }
                if (areaIsolatedPresent)
                {
                    if (GUILayout.Button ("Load DB Selection", EditorStyles.miniButton))
                    {
                        if (linker != null)
                            areaKvp.value.TrySelection (true);
                    }
                }
                GUILayout.EndVertical ();
                GUILayout.EndHorizontal ();

                return;
            }

            // Update options start

            // VPHelperEditor.BeginWidthClampedVerticalBox (0);
            EditorGUILayout.BeginVertical ("Box");

            EditorGUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            if (GUILayout.Button ("Updates " + (foldoutUpdates ? "■" : "▼"), EditorStyles.miniLabel))
                foldoutUpdates = !foldoutUpdates;
            EditorGUILayout.EndHorizontal ();


            if (foldoutUpdates)
            {
                GUILayout.BeginHorizontal ();
                if (DrawToolbarButtonAtSize ("Rebuild everything (1-7)\n►►►", am, true, false, 150f, columnWidth))
                {
                    am.rebuildCount = 0;
                    am.RebuildEverything ();
                }
                GUILayout.BeginVertical ();
                if (DrawToolbarButtonAtSize ("Update volume (1)", am, true, false, 22f, columnWidth))
                    am.UpdateVolume (false);
                if (DrawToolbarButtonAtSize ("Update spots (2)", am, true, false, 22f, columnWidth))
                    am.UpdateAllSpots (false);
                if (DrawToolbarButtonAtSize ("Update damage (3)", am, true, false, 22f, columnWidth))
                    am.RebuildAllBlockDamage ();
                if (DrawToolbarButtonAtSize ("Apply vertex colors (4)", am, true, false, 22f, columnWidth))
                    am.ApplyShaderPropertiesEverywhere ();
                if (DrawToolbarButtonAtSize ("Update collisions (5)", am, true, false, 22f, columnWidth))
                    am.RebuildCollisions ();
                if (DrawToolbarButtonAtSize ("Update globals (6)", am, true, false, 22f, columnWidth))
                    am.UpdateShaderGlobals ();
                GUILayout.EndVertical ();
                GUILayout.EndHorizontal ();
            }

            EditorGUILayout.EndVertical ();

            // Update options end
            // Editing toolbar start

            GUILayout.BeginVertical ("Box");
            GUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            if (GUILayout.Button ("Editing " + (foldoutEditing ? "■" : "▼"), EditorStyles.miniLabel))
                foldoutEditing = !foldoutEditing;
            GUILayout.EndHorizontal ();

            if (foldoutEditing)
            {
                GUILayout.BeginHorizontal ();
                DrawToolbarEditing (am, false);
                GUILayout.EndHorizontal ();
            }
            GUILayout.EndVertical ();

            // Editing toolbar end
            // Tileset toolbar start

            GUILayout.BeginVertical ("Box");

            foldoutTilesets = UtilityCustomInspector.DrawFoldout ("Tilesets", foldoutTilesets);
            if (foldoutTilesets)
            {
                if (GUILayout.Button ("Reload DB"))
                    AreaTilesetHelper.LoadDatabase ();

                if (AreaTilesetHelper.database != null && AreaTilesetHelper.database.tilesets != null)
                {
                    UtilityCustomInspector.DrawDictionary ("Main tilesets", AreaTilesetHelper.database.tilesets, DrawTilesetActive, null, false, false);
                    UtilityCustomInspector.DrawDictionary ("Interior tilesets", AreaTilesetHelper.database.tilesets, DrawTilesetInterior, null, false, false);
                }
                else
                    EditorGUILayout.HelpBox ("Tileset DB is not loaded", MessageType.Warning);
            }

            GUILayout.EndVertical ();

            // MB toolbar end
            // Prop toolbar start

            GUILayout.BeginVertical ("Box");

            foldoutProps = UtilityCustomInspector.DrawFoldout ("Props", foldoutProps);
            if (foldoutProps)
            {
                if (GUILayout.Button ("Reload DB"))
                    AreaAssetHelper.LoadResources ();

                GUILayout.Space(5f);

                EditorGUILayout.BeginHorizontal ();
                GUILayout.Label ("Props");
                GUILayout.FlexibleSpace ();
                GUILayout.Label ("Entries: " + AreaAssetHelper.propsPrototypesList.Count);
                EditorGUILayout.EndHorizontal ();

                GUILayout.Space(5f);

                EditorGUILayout.BeginHorizontal ();
                GUILayout.Label ("Filter", GUILayout.MaxWidth(32f));
                GUILayout.Space(10f);
                propFilter = EditorGUILayout.TextField(propFilter);
                EditorGUILayout.EndHorizontal ();

                GUILayout.Space(5f);

				bool filterById = false;
				filterById = int.TryParse(propFilter, out var propId);

                EditorGUILayout.BeginVertical ();
				foreach(var prop in AreaAssetHelper.propsPrototypesList)
				{
					if (propFilter.Length > 0 && !filterById && !prop.name.Contains(propFilter) || filterById && prop.id != propId)
						continue;

					EditorGUILayout.BeginVertical ();
					DrawProp(prop);
					EditorGUILayout.EndVertical ();
				}

                if (GUILayout.Button ("Paste color on filtered props", EditorStyles.miniButton))
                {
                    if (am.placementsProps != null)
                    {
                        foreach (var placement in am.placementsProps)
                        {
                            if (propFilter.Length > 0 && !filterById && !placement.prototype.name.Contains (propFilter) || filterById && placement.id != propId)
                                continue;

                            placement.hsbPrimary = clipboardPropHSBPrimary;
                            placement.hsbSecondary = clipboardPropHSBSecondary;
                            placement.UpdateMaterial_HSBOffsets (placement.hsbPrimary, placement.hsbSecondary);
                        }
                    }
                }

				EditorGUILayout.EndVertical();

               // UtilityCustomInspector.DrawList ("Props", AreaAssetHelper.propsPrototypesList, DrawProp, null, false, false);
            }

            GUILayout.EndVertical ();

            // Prop toolbar end
			// Prop Generation start

			GUILayout.BeginVertical ("Box");

			foldoutGenProps = UtilityCustomInspector.DrawFoldout ("Prop Generation", foldoutGenProps);
			if (foldoutGenProps)
			{
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Density", EditorStyles.miniLabel, GUILayout.MaxWidth (200f));
				propGenChance = EditorGUILayout.Slider (propGenChance, 0f, 1f);
				GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();
                GUILayout.Label ("Offset range", EditorStyles.miniLabel, GUILayout.MaxWidth (200f));
                propGenOffsetRange = EditorGUILayout.Slider (propGenOffsetRange, 0f, 1f);
                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();
                GUILayout.Label ("Require flat surroundings", EditorStyles.miniLabel, GUILayout.MaxWidth (200f));
                propGenRequireFlatSurroundings = EditorGUILayout.Toggle (propGenRequireFlatSurroundings);
                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();
                if (DrawToolbarButtonTwoLine ("Generate\nprops", am, false, false))
					GenerateProps (propGenChance, propGenerationIds, propGenRequireFlatSurroundings, propGenOffsetRange);

                if (DrawToolbarButtonTwoLine ("Remove\nall props", am, false, false))
					am.RemoveAllProps ();

                if (DrawToolbarButtonTwoLine ("Remove\nlisted props", am, false, false))
					am.RemovePropsWithIds (new HashSet<int>(propGenerationIds));
                GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				if (DrawToolbarButtonTwoLine ("Load\ngrass props", am, false, false))
				{
					propGenerationIds.Clear ();
					propGenerationIds.AddRange (AreaAssetHelper.grassIDs);
				}

				if (DrawToolbarButtonTwoLine ("Load\nbush props", am, false, false))
				{
					propGenerationIds.Clear ();
					propGenerationIds.AddRange (AreaAssetHelper.bushIDs);
				}

                if (DrawToolbarButtonTwoLine ("Load\ntree props", am, false, false))
                {
                    propGenerationIds.Clear ();
                    propGenerationIds.AddRange (AreaAssetHelper.treeIDs);
                }
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				if (DrawToolbarButtonTwoLine ("Load\nprop file", am, false, false))
				{
					var filename = EditorUtility.OpenFilePanel("Load Prop List", DataPathHelper.GetApplicationFolder (), "yaml");

					if(filename.Length > 0)
					{
						var list = UtilitiesYAML.LoadDataFromFile<List<int>>(filename);

						if(list != null)
						{
							propGenerationIds.Clear();
							propGenerationIds.AddRange(list);
						}
						else
						{
							Debug.LogError($"Could not load '{filename}'");
						}
					}
				}

				if (DrawToolbarButtonTwoLine ("Save\nprop file", am, false, false))
				{
					var filename = EditorUtility.SaveFilePanel("Save prop List", DataPathHelper.GetApplicationFolder (), "proplist", "yaml");

					if(filename.Length > 0)
						UtilitiesYAML.SaveDataToFile(filename, propGenerationIds, false);
				}
				GUILayout.EndHorizontal ();

				GUILayout.Space(8f);
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Prop ID", EditorStyles.miniLabel, GUILayout.MaxWidth (80f));
				newPropId = EditorGUILayout.IntField(newPropId);
				if (GUILayout.Button ("Add ID to list", EditorStyles.miniButton))
				{
					if(AreaAssetHelper.GetPropPrototype(newPropId) != null)
						propGenerationIds.Add(newPropId);
				}
				GUILayout.EndHorizontal ();

                GUILayout.BeginVertical ("Box");
				UtilityCustomInspector.DrawList ("Props", propGenerationIds, DrawPropById, null, false, false);
                GUILayout.EndVertical ();
			}

			GUILayout.EndVertical ();

			// Prop generation end
	        // Color Palette start

			GUILayout.BeginVertical ("Box");

			foldoutColorPalette = UtilityCustomInspector.DrawFoldout ("Color Palette", foldoutColorPalette);
			if (foldoutColorPalette)
			{
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Save"))
					SaveColorPalette();
				if (GUILayout.Button ("Load"))
					LoadColorPalette();
				GUILayout.EndHorizontal ();

				var tilesetArray = AreaTilesetHelper.database.tilesets.Select(kv => kv.Value).ToArray();

				GUILayout.BeginHorizontal ();

				var selectedIndex = EditorGUILayout.Popup(Array.FindIndex(tilesetArray, t => t.id == newPaletteEntry.tilesetId), tilesetArray.Select(t => t.name).ToArray(), GUILayout.Width(110f));
				if(selectedIndex >= 0 && selectedIndex < tilesetArray.Length)
					newPaletteEntry.tilesetId = tilesetArray[selectedIndex].id;

                newPaletteEntry.tilesetDescription = EditorGUILayout.TextField(newPaletteEntry.tilesetDescription);
                GUILayout.Space(5f);
				newPaletteEntry.primaryColor = HSBColor.FromColor(EditorGUILayout.ColorField(newPaletteEntry.primaryColor.ToColor(), GUILayout.MaxWidth(45f)));
				newPaletteEntry.secondaryColor = HSBColor.FromColor(EditorGUILayout.ColorField(newPaletteEntry.secondaryColor.ToColor(), GUILayout.MaxWidth(45f)));
                GUILayout.Space(5f);
				if (GUILayout.Button (new GUIContent("Add", "Add Swatch"), GUILayout.MaxWidth(45f)))
				{
					colorPalette.Add(new PaletteEntry(newPaletteEntry));

					//colorPalette.Sort((a,b) => a.tilesetId.CompareTo(b.tilesetId));
				}

                if (GUILayout.Button (new GUIContent("Copy", "Copy From Color Tool"), GUILayout.MaxWidth(45f)))
				{
					newPaletteEntry.tilesetId = selectedTilesetId;
					newPaletteEntry.primaryColor = selectedPrimaryColor;
					newPaletteEntry.secondaryColor = selectedSecondaryColor;
				}

                GUILayout.EndHorizontal ();

				UtilityCustomInspector.DrawList ("Swatches", colorPalette, DrawPaletteEntry, null, false, false, null, true);
			}

			GUILayout.EndVertical ();

			// Color Palette end
            // Volume snippets start

			GUILayout.BeginVertical ("Box");

			foldoutVolumeSnippets = UtilityCustomInspector.DrawFoldout ("Volume Snippets", foldoutVolumeSnippets);
			if (foldoutVolumeSnippets)
			{
				GUILayout.BeginHorizontal ();
                if (GUILayout.Button ("Load"))
                    LoadVolumeSnippets();
				GUILayout.EndHorizontal ();

                UtilityCustomInspector.DrawList ("Snippets", volumeSnippetKeys, DrawVolumeSnippet, null, false, false, null, true);
			}

			GUILayout.EndVertical ();

			// Volume snippets end
            // Utility toolbar start

            GUILayout.BeginVertical ("Box");
            GUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            GUILayout.Label ("Cleanup ■", EditorStyles.miniLabel);
            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal ();
            if (DrawToolbarButtonTwoLine (am.displayOnlyVolume ? "Show\neverything" : "Show\nonly volume", am, false, false))
            {
                am.SetVolumeDisplayMode (!am.displayOnlyVolume);
                EditorWindow view = EditorWindow.GetWindow<SceneView>();
                if (view != null)
                    view.Repaint();
            }
            if (DrawToolbarButtonTwoLine (am.displayProps ? "Hide\nprops" : "Show\nprops", am, false, false))
            {
                am.displayProps = !am.displayProps;
                am.RebuildEverything ();

                EditorWindow view = EditorWindow.GetWindow<SceneView>();
                if (view != null)
                    view.Repaint();
            }
            if (DrawToolbarButtonTwoLine ("Reset prop\noffsets", am, false, false))
                am.ResetPropOffsets ();
            if (DrawToolbarButtonTwoLine ("Reset\nsubtyping", am, false, false))
                am.ResetSubtyping ();
            if (DrawToolbarButtonTwoLine ("Reset\ndamage", am, false, false))
                am.ResetVolumeDamage ();
            if (DrawToolbarButtonTwoLine ("Erase & reset\neverything", am, false, false))
                am.EraseAndResetScene ();
            GUILayout.EndHorizontal ();
            GUILayout.EndVertical ();

            // Utility toolbar end
            // Tests toolbar start

            GUILayout.BeginVertical ("Box");
            GUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            GUILayout.Label ("Other ■", EditorStyles.miniLabel);
            GUILayout.EndHorizontal ();

            DrawSliceControls (am);

            GUILayout.BeginHorizontal ();
            if (DrawToolbarButtonTwoLine ("Regenerate\nnav graph", am, false, false))
                #if !PB_MODSDK
                AreaNavUtility.GetNavigationNodes (ref PhantomNavGraph.areaNodes, am);
                #else
                AreaNavUtility.GetNavigationNodes (ref AreaNavUtility.graph, am);
                #endif
            if (DrawToolbarButtonTwoLine ("Regenerate\nnav overrides", am, false, false))
                am.GenerateNavOverrides ();
            if (DrawToolbarButtonTwoLine ("Clear conf.\nnav overrides", am, false, false))
                am.ClearConflictingNavOverrides ();

            if (DrawToolbarButtonTwoLine ("Fill all\nvolume", am, false, false))
            {
                for (int i = 0; i < am.points.Count; ++i)
                {
                    var point = am.points[i];
                    if (point.pointPositionIndex.y > 0)
                        point.pointState = AreaVolumePointState.Full;
                }

                am.RebuildEverything ();
            }
            if (DrawToolbarButtonTwoLine ("Fill\nbottom", am, false, false))
            {
                int threshold = am.boundsFull.y - 1;
                for (int i = 0; i < am.points.Count; ++i)
                {
                    var point = am.points[i];
                    if (point.pointPositionIndex.y >= threshold)
                        point.pointState = AreaVolumePointState.Full;
                }

                am.RebuildEverything ();
            }
            if (DrawToolbarButtonTwoLine ("Mark roads\nindestructible", am, false, false))
            {
                int pointsSwitched = 0;
                for (int i = 0; i < am.points.Count; ++i)
                {
                    var point = am.points[i];
                    if (point.pointState == AreaVolumePointState.Empty)
                        continue;

                    var pointAbove = point.pointsWithSurroundingSpots[3];
                    if (pointAbove == null || pointAbove.pointState != AreaVolumePointState.Empty)
                        continue;

                    if (AreaManager.IsPointIndestructible (point, true, true, true, true, true))
                        continue;

                    bool match = AreaManager.IsPointInvolvingTileset (point, AreaTilesetHelper.idOfRoad);
                    if (!match)
                        continue;

                    point.destructible = false;
                    pointsSwitched += 1;
                }

                Debug.LogWarning ($"Points now indestructibe: {pointsSwitched}");

                am.RebuildEverything ();
            }

            if (DrawToolbarButtonTwoLine ("Mark only\ncargo tracked", am, false, false))
            {
                int pointsSwitched = 0;
                for (int i = 0; i < am.points.Count; ++i)
                {
                    var point = am.points[i];
                    if (point.pointState == AreaVolumePointState.Empty)
                        continue;

                    bool match = AreaManager.IsPointInvolvingTileset (point, AreaTilesetHelper.idOfCargo, 0);
                    point.destructionUntracked = !match;
                    if (match)
                    {
                        pointsSwitched += 1;
                        DebugExtensions.DrawCube (point.pointPositionLocal, Vector3.one * 1.35f, Color.green, 5f);
                    }
                }

                Debug.Log ($"Points involving cargo: {pointsSwitched}");
            }

            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal ();
            if (DrawToolbarButtonTwoLine ("Reset prop\nHSB offsets", am, false, false))
            {
                for (int i = 0; i < am.placementsProps.Count; ++i)
                {
                    am.placementsProps[i].hsbPrimary = Constants.defaultHSBOffset;
                    am.placementsProps[i].hsbSecondary = Constants.defaultHSBOffset;
                }
                am.ExecuteAllPropPlacements ();
            }
            if (DrawToolbarButtonTwoLine ("Replace\ncity grass", am, false, false))
                SwapRoadGrass ();
            if (DrawToolbarButtonTwoLine ("Remove\ncity crossings", am, false, false))
                RemoveCityCrossings ();
            if (DrawToolbarButtonTwoLine ("Remove all\nprops", am, false, false))
                am.RemoveAllProps ();
            if (DrawToolbarButtonTwoLine ("Remove float.\nprops", am, false, false))
                am.RemoveAllFloatingProps ();
            if (DrawToolbarButtonTwoLine ("Remove all\nfoliage", am, false, false))
                am.RemoveAllProps ("vegetation_");
            if (DrawToolbarButtonTwoLine ("Remove all\nemission", am, false, false))
                am.ResetTextureOverrides ();

            GUILayout.EndHorizontal ();

            GUILayout.EndVertical ();

            GUILayout.BeginVertical ("Box");
            GUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            GUILayout.Label ("Ramps ■", EditorStyles.miniLabel);
            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal ();

            if (DrawToolbarButtonTwoLine ("Remove\nall ramps", am, false, false))
                am.RemoveRampsEverywhere ();
            if (DrawToolbarButtonTwoLine ("Generate ramps\n(everywhere)", am, false, false))
                am.SetRampsEverywhere (AreaManager.SlopeProximityCheck.None);
            if (DrawToolbarButtonTwoLine ("Generate ramps\n(straight)", am, false, false))
                am.SetRampsEverywhere (AreaManager.SlopeProximityCheck.LateralSingle);
            if (DrawToolbarButtonTwoLine ("Generate ramps\n(wide straight)", am, false, false))
                am.SetRampsEverywhere (AreaManager.SlopeProximityCheck.LateralDouble);

            GUILayout.EndHorizontal ();

            am.rampImportOnGeneration = EditorGUILayout.ToggleLeft ("Import ramps from texture on generation", am.rampImportOnGeneration);

            GUILayout.EndVertical ();

            GUILayout.BeginVertical ("Box");
            GUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            GUILayout.Label ("Textures ■", EditorStyles.miniLabel);
            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal ();
            if (DrawToolbarButtonTwoLine ("Export\ndepth/ramps/roads", am, false, false))
                am.ExportToHeightmap ();
            if (DrawToolbarButtonTwoLine ("Import\ndepth", am, false, false))
                am.ImportHeightFromTexture ();
            if (DrawToolbarButtonTwoLine ("Import\nramps", am, false, false))
                am.ImportRampsFromTexture ();
            if (DrawToolbarButtonTwoLine ("Import\nroads", am, false, false))
                am.ImportRoadsFromTexture ();
            if (DrawToolbarButtonTwoLine ("Import\nprops", am, false, false))
                am.ImportPropsFromTexture ();
            GUILayout.EndHorizontal ();

            var biomeData = DataLinkerCombatBiomes.data;
            if (biomeData?.propGroups != null && biomeData.propGroups.Count > 0)
            {
                var keys = biomeData.propGroups.Keys.ToArray ();
                am.propImportOverrides = EditorGUILayout.ToggleLeft ("Override imported props", am.propImportOverrides);
                if (am.propImportOverrides)
                {
                    GUILayout.BeginVertical ("Box");
                    am.propImportOverrideRed = DrawStringDropdown (am.propImportOverrideRed, "R (tall)", keys);
                    am.propImportOverrideYellow = DrawStringDropdown (am.propImportOverrideYellow, "Y (mid)", keys);
                    am.propImportOverrideGreen = DrawStringDropdown (am.propImportOverrideGreen, "G (low)", keys);
                    GUILayout.EndVertical ();
                }
            }

            if (am.heightfieldPalette != null)
            {
                GUILayout.BeginVertical ("Box");
                GUILayout.Label ("Exported color to depth", EditorStyles.miniLabel);
                GUILayout.BeginHorizontal ();
                foreach (var colorLink in am.heightfieldPalette)
                {
                    GUILayout.BeginVertical ();
                    EditorGUILayout.ColorField (colorLink.color);
                    EditorGUILayout.IntField (colorLink.depthScaled);
                    EditorGUILayout.IntField (colorLink.depth);
                    GUILayout.EndVertical ();
                }
                GUILayout.EndHorizontal ();
                GUILayout.EndVertical ();
            }

            GUILayout.EndVertical ();

            if (editingTilesetSelected != null)
            {
                GUILayout.BeginVertical ("Box");
                GUILayout.Label ($"Override tileset: {editingTilesetSelected?.name}", EditorStyles.miniLabel);
                GUILayout.BeginHorizontal ();

                overrideIndexFrom = EditorGUILayout.IntField (overrideIndexFrom, GUILayout.Width (100f));
                overrideIndexTo = EditorGUILayout.IntField (overrideIndexTo, GUILayout.Width (100f));

                if (DrawToolbarButtonTwoLine ("Change mat.\noverrides", am, false, false))
                {
                    if (overrideIndexFrom != overrideIndexTo)
                    {
                        int count = 0;
                        for (int i = 0; i < am.points.Count; ++i)
                        {
                            var point = am.points[i];
                            if (point == null || !point.spotPresent)
                                continue;

                            if (point.blockTileset != editingTilesetSelected.id || !point.customization.overrideIndex.RoughlyEqual (overrideIndexFrom))
                                continue;

                            point.customization.overrideIndex = overrideIndexTo;
                            ++count;
                        }

                        Debug.Log ($"Adjusted material overrides from {overrideIndexFrom} to {overrideIndexTo} on {count} spots");
                    }

                    am.RebuildEverything ();
                }

                GUILayout.EndHorizontal ();
                GUILayout.EndVertical ();
            }

            // Tests toolbar end
            // Navigation visuals legend start

            GUILayout.BeginVertical ("Box");
            GUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            GUILayout.Label ("Navigation visualization legend ■", EditorStyles.miniLabel);
            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal ();
            GUILayout.Label ("■ White\n/ Red\n/ Green\n/ Blue\n/ Yellow\n/ Lime", EditorStyles.miniLabel);
            GUILayout.Label ("Navigation node\nLink (direct horizontal)\nLink (jump upward)\nLink (jump downward)\nLink (jump forward)\nLink (jump over)", EditorStyles.miniLabel);
            GUILayout.EndHorizontal ();
            GUILayout.EndVertical ();

            // Navigation visuals legend end

            float huePrimary = am.vertexPropertiesSelected.huePrimary;
            float saturationPrimary = am.vertexPropertiesSelected.saturationPrimary;
            float brightnessPrimary = am.vertexPropertiesSelected.brightnessPrimary;
            float hueSecondary = am.vertexPropertiesSelected.hueSecondary;
            float saturationSecondary = am.vertexPropertiesSelected.saturationSecondary;
            float brightnessSecondary = am.vertexPropertiesSelected.brightnessSecondary;
            float emissionIntensity = am.vertexPropertiesSelected.overrideIndex;
            float damageIntensity = am.vertexPropertiesSelected.damageIntensity;

            foldoutVC = EditorGUILayout.Foldout (foldoutVC, "VC options");
            if (foldoutVC)
            {
                EditorGUIUtility.labelWidth = 70f;
                GUILayout.BeginVertical ("Box");

                GUILayout.BeginHorizontal ();
                if (GUILayout.Button ("Set default"))
                {
                    am.vertexPropertiesSelected = TilesetVertexProperties.defaults;
                    huePrimary = am.vertexPropertiesSelected.huePrimary;
                    saturationPrimary = am.vertexPropertiesSelected.saturationPrimary;
                    brightnessPrimary = am.vertexPropertiesSelected.brightnessPrimary;
                    hueSecondary = am.vertexPropertiesSelected.hueSecondary;
                    saturationSecondary = am.vertexPropertiesSelected.saturationSecondary;
                    brightnessSecondary = am.vertexPropertiesSelected.brightnessSecondary;
                    emissionIntensity = am.vertexPropertiesSelected.overrideIndex;
                    damageIntensity = am.vertexPropertiesSelected.damageIntensity;
                }
                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();

                GUILayout.BeginVertical ("Box");
                GUILayout.Label ("Primary", EditorStyles.miniLabel);
                EditorGUILayout.ColorField (new HSBColor (huePrimary, saturationPrimary, brightnessPrimary).ToColor ());
                huePrimary = EditorGUILayout.Slider ("H", am.vertexPropertiesSelected.huePrimary, 0f, 1f);
                saturationPrimary = EditorGUILayout.Slider ("S", am.vertexPropertiesSelected.saturationPrimary, 0f, 1f);
                brightnessPrimary = EditorGUILayout.Slider ("B", am.vertexPropertiesSelected.brightnessPrimary, 0f, 1f);
                GUILayout.EndVertical ();

                GUILayout.BeginVertical ("Box");
                GUILayout.Label ("Secondary", EditorStyles.miniLabel);
                EditorGUILayout.ColorField (new HSBColor (hueSecondary, saturationSecondary, brightnessSecondary).ToColor ());
                hueSecondary = EditorGUILayout.Slider ("H", am.vertexPropertiesSelected.hueSecondary, 0f, 1f);
                saturationSecondary = EditorGUILayout.Slider ("S", am.vertexPropertiesSelected.saturationSecondary, 0f, 1f);
                brightnessSecondary = EditorGUILayout.Slider ("B", am.vertexPropertiesSelected.brightnessSecondary, 0f, 1f);
                GUILayout.EndVertical ();

                GUILayout.BeginVertical ("Box");
                GUILayout.Label ("Intensities", EditorStyles.miniLabel);
                EditorGUILayout.ColorField (new Color (emissionIntensity, emissionIntensity, emissionIntensity, damageIntensity));
                emissionIntensity = EditorGUILayout.Slider ("Emission", am.vertexPropertiesSelected.overrideIndex, 0f, 1f);
                damageIntensity = EditorGUILayout.Slider ("Damage", am.vertexPropertiesSelected.damageIntensity, 0f, 1f);
                GUILayout.EndVertical ();

                GUILayout.EndHorizontal ();

                GUILayout.EndVertical ();
                EditorGUIUtility.labelWidth = 150f;
            }

            if (GUI.changed)
            {
                Undo.RecordObject (am, "Tileset placement manager properties changed");
                if (boundsApplied && am.boundsFull != boundsFullCached)
                {
                    // am.boundsFull = boundsFullCached;
                    // am.RebuildEverything ();
                    am.RemapLevel (boundsFullCached);
                }

                am.vertexPropertiesSelected = new TilesetVertexProperties (huePrimary, saturationPrimary, brightnessPrimary, hueSecondary, saturationSecondary, brightnessSecondary, emissionIntensity, damageIntensity);

                GUI.changed = false;
            }


            if (GUILayout.Button ("Load configuration data"))
                configurationDataForBlocks = new List<AreaConfigurationData> (AreaTilesetHelper.database.configurationDataForBlocks);

            if (configurationDataForBlocks != null)
                UtilityCustomInspector.DrawList ("Configuration data", configurationDataForBlocks, DrawConfigurationData, null, true, false);

            foldoutDefault = EditorGUILayout.Foldout (foldoutDefault, "Other options");
            if (foldoutDefault)
            {
                DrawDefaultInspector ();
            }

            // Editing toolbar start

            GUILayout.BeginVertical ("Box");
            GUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            if (GUILayout.Button ("Debug " + (foldoutDebug ? "■" : "▼"), EditorStyles.miniLabel))
                foldoutDebug = !foldoutDebug;
            GUILayout.EndHorizontal ();

            if (foldoutDebug)
            {
                GUILayout.BeginHorizontal ();
                if (DrawToolbarButtonTwoLine ("Validate\nflags", am, true, false))
                    ValidateFlags (am);

                if (DrawToolbarButtonTwoLine ("Fix\nbrightness", am, true, false))
                    am.FixBrightnessValues ();

                if (DrawToolbarButtonTwoLine ("Reapply\ndefaults", am, true, false))
                    am.SetAllBlocksToDefault ();

                GUILayout.EndHorizontal ();
            }
            GUILayout.EndVertical ();

            // Editing toolbar end
        }

        private void DrawSliceControls (AreaManager am)
        {
            if (CombatSceneHelper.ins == null || CombatSceneHelper.ins.materialHelper == null)
                return;

            bool refreshMaterial = false;
            GUILayout.BeginVertical ("Box");

            EditorGUI.BeginChangeCheck ();
            GUILayout.BeginHorizontal ();
            GUILayout.Label ("Slicing", EditorStyles.miniLabel, GUILayout.MaxWidth (100f));
            var sliceEnabled = EditorGUILayout.Toggle (am.sliceEnabled);
            GUILayout.EndHorizontal ();

            if (EditorGUI.EndChangeCheck ())
            {
                am.sliceEnabled = sliceEnabled;
                refreshMaterial = true;
            }

            EditorGUI.BeginChangeCheck ();

            if (sliceEnabled)
            {
                EditorGUI.BeginChangeCheck ();
                GUILayout.BeginHorizontal ();
                GUILayout.Label ("Depth", EditorStyles.miniLabel, GUILayout.MaxWidth (100f));
                int sliceDepth = EditorGUILayout.IntSlider (am.sliceDepth, 0, Mathf.Max (0, am.boundsFull.y - 2));
                GUILayout.EndHorizontal ();

                if (EditorGUI.EndChangeCheck ())
                {
                    am.sliceDepth = sliceDepth;
                    refreshMaterial = true;
                }

                if (CombatSceneHelper.ins != null && CombatSceneHelper.ins.materialHelper != null)
                {
                    var m = CombatSceneHelper.ins.materialHelper;

                    GUILayout.BeginHorizontal ();
                    GUILayout.Label ("Highlight", EditorStyles.miniLabel, GUILayout.MaxWidth (100f));
                    var sliceAlpha = EditorGUILayout.Slider (m.sliceColor.a, 0f, 1f);
                    GUILayout.EndHorizontal ();

                    if (EditorGUI.EndChangeCheck ())
                    {
                        m.sliceColor = new Color (m.sliceColor.r, m.sliceColor.g, m.sliceColor.b, sliceAlpha);
                        refreshMaterial = true;
                    }
                }
            }

            GUILayout.EndVertical ();

            if (refreshMaterial)
            {
                am.UpdateSlicing ();
                EditorWindow view = EditorWindow.GetWindow<SceneView>();
                if (view != null)
                    view.Repaint();
            }
        }

        private void DrawConfigurationData (AreaConfigurationData data)
        {
            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label (data.configurationAsString, EditorStyles.miniLabel);
            GUILayout.FlexibleSpace ();
            GUILayout.Label
            (
                "RR: " + data.requiredRotation +
                " | RF: " + (data.requiredFlippingZ ? "Y" : "N") +
                " | CR: " + (data.customRotationPossible ? "Y" : "N") +
                " | CFM: " + data.customFlippingMode,
                EditorStyles.miniLabel
            );
            EditorGUILayout.EndHorizontal ();
        }

        private void DrawTilesetActive (int key, AreaTileset value)
        {
            if (value.usedAsInterior)
                return;

            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label (value.name);
            // GUILayout.Label (value.id.ToString (), EditorStyles.miniLabel);

            if (editingTilesetSelected != null)
            {
                GUILayout.FlexibleSpace ();
                if (value != editingTilesetSelected)
                {
                    if (GUILayout.Button ("Select"))
                        editingTilesetSelected = value;
                }
                else
                {
                    GUILayout.Label ("Selected", EditorStyles.miniLabel);
                }
                if (GUILayout.Button ("Fill"))
                {
                    editingTilesetSelected = value;
                    for (int i = 0; i < am.points.Count; ++i)
                        am.points[i].blockTileset = value.id;
                    am.RebuildEverything ();
                }
            }

            EditorGUILayout.EndHorizontal ();
        }

        private void DrawTilesetInterior (int key, AreaTileset value)
        {
            if (!value.usedAsInterior)
                return;

            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label (value.name);
            // GUILayout.Label (value.id.ToString (), EditorStyles.miniLabel);
            GUILayout.FlexibleSpace ();
            EditorGUILayout.EndHorizontal ();
        }

        private void DrawProp (AreaPropPrototypeData prototype)
        {
            var propLabelStyle = EditorStyles.label;
            if (propSelectionID == prototype.id)
            {
                EditorGUILayout.BeginVertical("Box");
                propLabelStyle = EditorStyles.boldLabel;
            }
            {
                EditorGUILayout.BeginHorizontal ();
                GUILayout.Label (prototype.prefab.id.ToString (), propLabelStyle, GUILayout.MinWidth (45f));
                GUILayout.Space(3f);
                GUILayout.Label (prototype.prefab.name, propLabelStyle);
                if (am != null)
                {
                    // GUILayout.Label (entry.idCached.ToString (), EditorStyles.miniLabel);
                    GUILayout.FlexibleSpace ();
                    if (propSelectionID != prototype.id)
                    {
                        if (GUILayout.Button ("Select"))
                        {
                            propSelectionID = prototype.id;
                            propIndex = AreaAssetHelper.propsPrototypesList.IndexOf (prototype);
                        }
                    }
                    else
                    {
                        GUILayout.Label ("Selected");
                    }
                }
                EditorGUILayout.EndHorizontal ();
            }
            if (propSelectionID == prototype.id)
                EditorGUILayout.EndVertical ();
        }

        private void DrawPropById (int propId)
        {
	        var prototype = AreaAssetHelper.GetPropPrototype(propId);
			if(prototype == null)
				return;

	        EditorGUILayout.BeginHorizontal ();
	        GUILayout.Label ($"{prototype.prefab.id} / {prototype.prefab.name}", EditorStyles.miniLabel);

	        GUILayout.FlexibleSpace ();
	        if (GUILayout.Button ("Remove", EditorStyles.miniButton))
	        {
				propGenerationIds.Remove(propId);
	        }

	        EditorGUILayout.EndHorizontal ();
        }

        private void DrawPaletteEntry (PaletteEntry entry)
        {
	        EditorGUILayout.BeginVertical ();
            EditorGUILayout.BeginHorizontal ();

	        GUILayout.Label (AreaTilesetHelper.GetTileset(entry.tilesetId)?.name??$"<tileset id={entry.tilesetId}>", EditorStyles.miniLabel, GUILayout.Width(90f));

            entry.tilesetDescription = EditorGUILayout.TextField(entry.tilesetDescription);
            GUILayout.Space(5f);
	        entry.primaryColor = HSBColor.FromColor(EditorGUILayout.ColorField(entry.primaryColor.ToColor(), GUILayout.MaxWidth(55f)));
	        entry.secondaryColor = HSBColor.FromColor(EditorGUILayout.ColorField(entry.secondaryColor.ToColor(), GUILayout.MaxWidth(55f)));
            GUILayout.Space(5f);

	        //GUILayout.FlexibleSpace ();
	        if (GUILayout.Button ("Apply", GUILayout.MaxWidth(45f)))
	        {
                if (editingTarget == EditingTarget.Color)
                {
                    selectedTilesetId = entry.tilesetId;
                    selectedPrimaryColor = entry.primaryColor;
                    selectedSecondaryColor = entry.secondaryColor;
                }
                else if (editingTarget == EditingTarget.Props)
                {
                    if (am.indexesOccupiedByProps.ContainsKey (propPlacementListIndex))
                    {
                        for (int i = 0; i < am.placementsProps.Count; ++i)
                        {
                            AreaPlacementProp placement = am.placementsProps[i];

                            if (placement != propPlacementHandled)
                                continue;

                            placement.hsbPrimary.x = entry.primaryColor.h;
                            placement.hsbPrimary.y = entry.primaryColor.s;
                            placement.hsbPrimary.z = entry.primaryColor.b;

                            placement.hsbSecondary.x = entry.secondaryColor.h;
                            placement.hsbSecondary.y = entry.secondaryColor.s;
                            placement.hsbSecondary.z = entry.secondaryColor.b;

                            placement.UpdateMaterial_HSBOffsets (placement.hsbPrimary, placement.hsbSecondary);
                        }
                    }
                    else
                    {
                        propHSBPrimary.x = entry.primaryColor.h;
                        propHSBPrimary.y = entry.primaryColor.s;
                        propHSBPrimary.z = entry.primaryColor.b;

                        propHSBSecondary.x = entry.secondaryColor.h;
                        propHSBSecondary.y = entry.secondaryColor.s;
                        propHSBSecondary.z = entry.secondaryColor.b;

                    }
                }
	        }
	        if (GUILayout.Button ("x", GUILayout.MaxWidth(25f)))
	        {
		        colorPalette.Remove(entry);
	        }

	        EditorGUILayout.EndHorizontal ();
            GUILayout.Space(3f);
            EditorGUILayout.EndVertical ();
        }

        private void DrawVolumeSnippet (string entry)
        {
            EditorGUILayout.BeginHorizontal ("Box");
            GUILayout.Label (entry, EditorStyles.miniLabel);
            GUILayout.FlexibleSpace ();

            if (GUILayout.Button ("◄", GUILayout.MaxWidth (25f)))
            {
                var path = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), "Configs/AreaSnippets");
                var filename = DataPathHelper.GetCombinedCleanPath (path, entry);

                if (filename.Length > 0)
                    am.clipboard?.LoadFromYAML(filename);
            }

            EditorGUILayout.EndHorizontal ();
        }

        private void DrawPropClaim (int indexOfSpot, AreaPlacementProp placement)
        {
            EditorGUILayout.BeginHorizontal ("Box");
            GUILayout.Label (indexOfSpot.ToString ());
            GUILayout.FlexibleSpace ();

            if (placement != null)
            {
                if (placement.prototype != null)
                    GUILayout.Label (placement.prototype.name, EditorStyles.miniLabel);
                else
                    GUILayout.Label ("prototype null", EditorStyles.miniLabel);
            }
            else
                GUILayout.Label ("placement null", EditorStyles.miniLabel);

            EditorGUILayout.EndHorizontal ();
        }

        private void DrawBrushSelector ()
        {
            UtilityCustomInspector.DrawField ("Brush", () => AreaManager.editingVolumeBrush = (AreaManager.EditingVolumeBrush)EditorGUILayout.EnumPopup (AreaManager.editingVolumeBrush));

            EditorGUILayout.BeginHorizontal ();
            if (GUILayout.Button ("None", EditorStyles.miniButtonLeft))
            {
                AreaManager.editingVolumeBrush = AreaManager.EditingVolumeBrush.Point;
            }
            if (GUILayout.Button ("2x2R", EditorStyles.miniButtonMid))
            {
                AreaManager.editingVolumeBrush = AreaManager.EditingVolumeBrush.Square2x2;
            }
            if (GUILayout.Button ("3x3R", EditorStyles.miniButtonMid))
            {
                AreaManager.editingVolumeBrush = AreaManager.EditingVolumeBrush.Square3x3;
            }
            if (GUILayout.Button ("3x3C", EditorStyles.miniButtonRight))
            {
                AreaManager.editingVolumeBrush = AreaManager.EditingVolumeBrush.Circle3x3;
            }
            EditorGUILayout.EndHorizontal ();
        }

        private void DrawSearchSelector ()
        {
            // UtilityCustomInspector.DrawField ("Flood fill", () => currentSearchFlags = Sirenix.OdinInspector.Editor.EnumSelector<SpotSearchFlags>.DrawEnumField(null, currentSearchFlags));

            var bkgColor = GUI.backgroundColor;
            var buttonColor = Color.HSVToRGB ((Mathf.Cos ((float)UnityEditor.EditorApplication.timeSinceStartup + 1f) * 0.5f + 0.325f) % 1f, 1f, 1f);
            bool floodFillActive = currentSearchFlags != SpotSearchFlags.None;
            GUI.backgroundColor = floodFillActive ? buttonColor : bkgColor;

            GUILayout.BeginVertical (GUILayout.Width (200f), GUILayout.MaxWidth (200f));

            GUILayout.BeginHorizontal ();
            UtilityCustomInspector.DrawField ("Flood fill", () => currentSearchFlags = (SpotSearchFlags)EditorGUILayout.EnumFlagsField (currentSearchFlags, GUILayout.Width (200f)));
            if (floodFillActive)
            {
                if (GUILayout.Button ("-", GUILayout.MaxWidth (25f)))
                {
                    currentSearchFlags = SpotSearchFlags.None;
                    floodFillActive = false;
                }
            }
            GUILayout.EndHorizontal ();

            if (GUI.changed)
            {
                if (currentSearchFlags == SpotSearchFlags.None)
                {
                    lastSearchOrigin = null;
                    lastSearchResults.Clear ();
                }
            }

            if (floodFillActive)
            {
                if (lastSearchResults != null && lastSearchResults.Count > 0 && lastSearchOrigin != null && lastSearchOrigin == lastSpotHovered)
                {
                    GUILayout.Label ("Warning! Ready to apply flood fill!", EditorStyles.boldLabel, GUILayout.Width (200f), GUILayout.MaxWidth (200f));
                }
                else
                {
                    // GUILayout.Label ("Flood fill ready to search", EditorStyles.boldLabel);
                    GUILayout.Label ("Ready to search for connected points...", EditorStyles.miniLabel, GUILayout.Width (200f), GUILayout.MaxWidth (200f));
                }
            }
            else
            {
                GUILayout.Label (" ", EditorStyles.miniLabel, GUILayout.Width (200f), GUILayout.MaxWidth (200f));
            }

            /*
            GUILayout.Space (8f);
            GUILayout.BeginHorizontal ();
            {
                GUI.backgroundColor = (currentSearchType == SpotSearchType.None) ? buttonColor : bkgColor;
                if (GUILayout.Button ("None", EditorStyles.miniButtonLeft))
                    currentSearchType = SpotSearchType.None;

                GUI.backgroundColor = (currentSearchType == SpotSearchType.SameFloor) ? buttonColor : bkgColor;
                if (GUILayout.Button ("Floor", EditorStyles.miniButtonMid))
                    currentSearchType = SpotSearchType.SameFloor;

                GUI.backgroundColor = (currentSearchType == SpotSearchType.SameFloorIsolated) ? buttonColor : bkgColor;
                if (GUILayout.Button ("Floor-Iso.", EditorStyles.miniButtonRight))
                    currentSearchType = SpotSearchType.SameFloorIsolated;
            }
            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal ();
            {
                GUI.backgroundColor = (currentSearchType == SpotSearchType.SameConfiguration) ? buttonColor : bkgColor;
                if (GUILayout.Button ("S. Cfg.", EditorStyles.miniButtonLeft))
                    currentSearchType = SpotSearchType.SameConfiguration;

                GUI.backgroundColor = (currentSearchType == SpotSearchType.SameTileset) ? buttonColor : bkgColor;
                if (GUILayout.Button ("S. Tls.", EditorStyles.miniButtonMid))
                    currentSearchType = SpotSearchType.SameTileset;

                GUI.backgroundColor = (currentSearchType == SpotSearchType.SameEverything) ? buttonColor : bkgColor;
                if (GUILayout.Button ("S. Evr.", EditorStyles.miniButtonMid))
                    currentSearchType = SpotSearchType.SameEverything;

                GUI.backgroundColor = (currentSearchType == SpotSearchType.SameColor) ? buttonColor : bkgColor;
                if (GUILayout.Button ("S. Clr.", EditorStyles.miniButtonRight))
                    currentSearchType = SpotSearchType.SameColor;
            }
            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal ();
            {
                GUI.backgroundColor = (currentSearchType == SpotSearchType.AllEmptyNodes) ? buttonColor : bkgColor;
                if (GUILayout.Button ("Empties", EditorStyles.miniButtonRight))
                    currentSearchType = SpotSearchType.AllEmptyNodes;
            }
            GUILayout.EndHorizontal ();
            */

            GUI.backgroundColor = bkgColor;
            GUILayout.EndVertical ();
        }

        private void DrawToolbarEditing (AreaManager t, bool restrictWidth)
        {
            DrawButtonEditingMode ("Volume\nshape", t, EditingTarget.Volume, restrictWidth);
            DrawButtonEditingMode ("Volume\ndamage", t, EditingTarget.Damage, restrictWidth);
            DrawButtonEditingMode ("Volume\ntransfer", t, EditingTarget.Transfer, restrictWidth);
            DrawButtonEditingMode ("Block\ntileset", t, EditingTarget.Tileset, restrictWidth);
            DrawButtonEditingMode ("Block type\n& transform", t, EditingTarget.Spot, restrictWidth);
            DrawButtonEditingMode ("Block\ncolor", t, EditingTarget.Color, restrictWidth);
            //DrawButtonEditingMode ("Multiblock\ntool", t, EditingTarget.MultiblocksV2, restrictWidth);
            DrawButtonEditingMode ("Prop\ntool", t, EditingTarget.Props, restrictWidth);
            DrawButtonEditingMode ("Nav\ntool", t, EditingTarget.Navigation, restrictWidth);
            DrawButtonEditingMode ("Road\ntool", t, EditingTarget.Roads, restrictWidth);
            DrawButtonEditingMode ("Road Curve\ntool", t, EditingTarget.RoadCurves, restrictWidth);
            DrawButtonEditingMode ("Terrain\nramp", t, EditingTarget.TerrainRamp, restrictWidth);
        }

        private void DrawButtonEditingMode (string label, AreaManager am, EditingTarget mode, bool restrictWidth)
        {
            if (DrawToolbarButtonTwoLine (label, am, restrictWidth, editingTarget == mode))
                SetEditingTarget (am, mode);
        }

        private bool DrawToolbarButtonTwoLine (string label, AreaManager t, bool restrictWidth, bool grayed)
        {
            return DrawToolbarButtonAtSize (label, t, restrictWidth, grayed, 45f, 85f);
        }

        private bool DrawToolbarButtonOneLine (string label, AreaManager t, bool restrictWidth, bool grayed, float width)
        {
            return DrawToolbarButtonAtSize (label, t, restrictWidth, grayed, 22f, 200f);
        }

        private bool DrawToolbarButtonAtSize (string label, AreaManager t, bool restrictWidth, bool grayed, float height, float width)
        {
            Color bg = GUI.backgroundColor;
            if (grayed)
                GUI.backgroundColor = Color.gray;
            bool result;

            if (restrictWidth)
                result = GUILayout.Button (string.Empty, GUI.skin.button, GUILayout.MaxHeight (height), GUILayout.MinHeight (height), GUILayout.MinWidth (width), GUILayout.MaxWidth (width));
            else
                result = GUILayout.Button (string.Empty, GUI.skin.button, GUILayout.MaxHeight (height), GUILayout.MinHeight (height));

            GUI.backgroundColor = bg;
            Rect last = GUILayoutUtility.GetLastRect ();
            last.Set (last.x + 3, last.y + 3, last.width - 3, last.height - 3);
            GUI.Label (last, label);

            return result;
        }

        private void OnDrawGizmosSelected ()
        {
            var am = target as AreaManager;
            if (am == null)
                return;
        }

        private void DrawHotkeyHintsPanel(string hintTextFirstLine, string hintTextSecondLine = "")
        {
            float panelWidth = 1100f;
            float panelHeight = 40f;
            float panelWidthAlt = 60f;

            // "Alt" side label
            GUILayout.BeginArea (new Rect (new Vector2 (Screen.width*0.5f - panelWidth*0.5f - panelWidthAlt - 10f, Screen.height - panelHeight*2f - 10f), new Vector2 (panelWidthAlt, panelHeight)));
            GUILayout.BeginVertical ("Box", GUILayout.Height(panelHeight));

            GUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            GUILayout.Label ("Alt +  ", EditorStyles.boldLabel, GUILayout.Height(panelHeight));
            GUILayout.EndHorizontal ();

            GUILayout.EndVertical ();
            GUILayout.EndArea ();

            // Main hotkey hints label
            GUILayout.BeginArea (new Rect (new Vector2 (Screen.width*0.5f - panelWidth*0.5f, Screen.height - panelHeight*2f - 10f), new Vector2 (panelWidth, panelHeight)));
            GUILayout.BeginVertical ("Box", GUILayout.Height(panelHeight));
            GUILayout.FlexibleSpace ();

            GUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            GUILayout.Label (hintTextFirstLine, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace ();
            GUILayout.EndHorizontal ();

            if (hintTextSecondLine.Length > 0)
            {
                GUILayout.BeginHorizontal ();
                GUILayout.FlexibleSpace ();
                GUILayout.Label (hintTextSecondLine, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace ();
                GUILayout.EndHorizontal ();
            }

            GUILayout.FlexibleSpace ();
            GUILayout.EndVertical ();
            GUILayout.EndArea ();
        }

        // private int hitCount = 0;
        // private Vector3 hitPointShifted = Vector3.one;
        // private float hitNormalOffsetMultiplier = 0.25f;

        /*
        private void OnDrawGizmos ()
        {
            AreaManager am = target as AreaManager;
            if (am == null)
                return;

            if (am.points != null && showStructuralAnalysis)
            {
                Color hc = Handles.color;
                Color gc = Gizmos.color;
                Color colorNonIsolated = new HSBColor (0f, 0f, 0.8f, 1f).ToColor ();

                for (int i = 0; i < am.points.Count; ++i)
                {
                    AreaVolumePoint p = am.points[i];
                    if (p != null && p.positionIndex.y < am.damageRestrictionDepth && p.structuralParent != null)
                    {
                        Gizmos.color = p.structuralGroup == 0 ? colorNonIsolated : new HSBColor (((float)p.structuralGroup / 7.8423f) % 1f, 1f, 1f, 1f).ToColor ();
                        Gizmos.DrawLine (p.positionLocal, p.structuralParent.positionLocal);
                        Gizmos.DrawCube (p.positionLocal, Vector3.one * 0.5f);

                        if (p.structuralGroup != 0)
                        {
                            Handles.color = Color.white;
                            Handles.Label (p.positionLocal, p.structuralGroup.ToString (), EditorStyles.whiteLabel);
                        }
                    }
                }

                Gizmos.color = gc;
                Handles.color = hc;
            }

            if (lastSearchResults != null && showLastSearchResults)
            {
                Color hc = Handles.color;
                Color gc = Gizmos.color;
                Color colorIsolated = new HSBColor (0f, 0f, 0.8f, 1f).ToColor ();
                Vector3 spotOffset = new Vector3 (1f, -1f, 1f) * (TilesetUtility.blockAssetSize / 2f);

                for (int i = 0; i < lastSearchResults.Count; ++i)
                {
                    AreaVolumePointSearchData psd = lastSearchResults[i];
                    if (psd != null)
                    {
                        bool parented = psd.parent != null;
                        Gizmos.color = parented ? new HSBColor (((float)psd.status / 734.8423f) % 1f, 1f, 1f, 1f).ToColor () : colorIsolated;
                        Gizmos.DrawCube (psd.point.positionLocal + spotOffset, Vector3.one * (parented ? 0.5f : 1f));
                        if (parented)
                            Gizmos.DrawLine (psd.point.positionLocal + spotOffset, psd.parent.point.positionLocal + spotOffset);
                    }
                }

                Gizmos.color = gc;
                Handles.color = hc;
            }
        }
        */

		private static Vector3[] pointArrayAB = new Vector3[2];
        private static Vector3[] pointArrayABC = new Vector3[3];
        private static Vector3[] pointArrayABCD = new Vector3[4];
        private static Vector3[] pointArrayABCDE = new Vector3[5];
        private static List<AreaNavLink> reusedLinks;

        private static List<Vector3> brushOffsetsCircle3x3 = new List<Vector3>
        {
            new Vector3 (3f, 0f, 0f),
            new Vector3 (0f, 0f, 3f),
            new Vector3 (-3f, 0f, 0f),
            new Vector3 (0f, 0f, -3f)
        };

        private static List<Vector3> brushOffsetsSquare3x3 = new List<Vector3>
        {
            new Vector3 (3f, 0f, 0f),
            new Vector3 (3f, 0f, 3f),
            new Vector3 (0f, 0f, 3f),
            new Vector3 (-3f, 0f, 3f),
            new Vector3 (-3f, 0f, 0f),
            new Vector3 (-3f, 0f, -3f),
            new Vector3 (0f, 0f, -3f)
        };

        private static List<Vector3> brushOffsetsSquare2x2 = new List<Vector3>
        {
            new Vector3 (-3f, 0f, 0f),
            new Vector3 (-3f, 0f, -3f),
            new Vector3 (0f, 0f, -3f)
        };

        private void OnSceneGUI ()
        {
            // Disable clicking on scene objects
            HandleUtility.AddDefaultControl (GUIUtility.GetControlID (FocusType.Passive));

            AreaManager am = target as AreaManager;
            if (am == null)
                return;

            am.UpdateShaderGlobals ();

            Event e = Event.current;

            // Killing some bad editor hotkeys
            if (Event.current.type == EventType.ExecuteCommand)
            {
                switch (Event.current.commandName)
                {
                    case "Copy":
                        Event.current.Use ();
                        break;
                    case "Cut":
                        Event.current.Use ();
                        break;
                    case "Paste":
                        Event.current.Use ();
                        break;
                    case "Delete":
                        Event.current.Use ();
                        break;
                    case "FrameSelected":
                        Event.current.Use ();
                        break;
                    case "Duplicate":
                        Event.current.Use ();
                        break;
                    case "SelectAll":
                        Event.current.Use ();
                        break;
                    default:
                        // Lets show any other commands that may come through
                        // Debug.Log (Event.current.commandName);
                        break;
                }
            }

            if (rcSelectionObject != null && rcSelectionObject.activeSelf)
            {
                if (editingTarget == EditingTarget.Volume || editingTarget == EditingTarget.Roads)
                {
                    Color hc = Handles.color;
                    Handles.color = Color.white.WithAlpha (0.5f);

                    var colorMainA = new HSBColor (0.5f, 1f, 1f, 1f).ToColor ();
                    var colorMainB = new HSBColor (0.1f, 1f, 0.8f, 1f).ToColor ();
                    var planeRotation = Quaternion.Euler (90f, 0f, 0f);

                    var brush = AreaManager.editingVolumeBrush;
                    if (volumeBrushDepth <= 1 && brush == AreaManager.EditingVolumeBrush.Point || lastPointHovered == null)
                        DrawZTestRect (rcSelectionTargetPosition, planeRotation, 1f, colorMainA, colorMainA.WithAlpha (0.5f));
                    else
                    {
                        var yStart = lastPointHovered.instancePosition.y;
                        var pointsInBrush = AreaManager.CollectPointsInBrush (lastPointHovered, AreaManager.editingVolumeBrush, volumeBrushDepth, volumeBrushDepthGoesUp, volumeBrushDepthRange);
                        for (int i = 0, iLimit = pointsInBrush.Count; i < iLimit; ++i)
                        {
                            var pointInBrush = pointsInBrush[i];
                            if (pointInBrush == null)
                                continue;

                            float interpolant = Mathf.Clamp01 (Mathf.Abs (yStart - pointInBrush.instancePosition.y) / 15f);
                            var color1 = Color.Lerp (colorMainA, colorMainB, interpolant);
                            var color2 = color1.WithAlpha (0.5f);
                            DrawZTestRect (pointInBrush.pointPositionLocal, planeRotation, 1f, color1, color2);
                        }
                    }

                    Handles.color = hc;
                }
            }

            bool volumeTarget =
                editingTarget == EditingTarget.Volume ||
                editingTarget == EditingTarget.Damage ||
                editingTarget == EditingTarget.Roads ||
                editingTarget == EditingTarget.Transfer ||
	            editingTarget == EditingTarget.TerrainRamp;

            if (e.alt)
            {
                if (!volumeTarget)
                {
                    if (lastSpotHovered != null)
                    {
                        DrawZTestWireCube (lastSpotHovered.instancePosition, Color.white.Opaque (), Color.cyan.WithAlpha (0.5f));

                        var neighborXPos = lastSpotHovered.pointsInSpot[1];
                        var neighborZPos = lastSpotHovered.pointsInSpot[2];
                        var neighborXNeg = lastSpotHovered.pointsWithSurroundingSpots[6];
                        var neighborZNeg = lastSpotHovered.pointsWithSurroundingSpots[5];

                        var offset = Vector3.up * 0.1f;
                        var origin = lastSpotHovered.pointPositionLocal + offset;

                        if (neighborXPos != null)
                        {
                            DrawZTestLine (origin, neighborXPos.pointPositionLocal + offset, colorAxisXPos);
                            DrawZTestLine (neighborXPos.pointPositionLocal + offset, neighborXPos.pointPositionLocal + offset * 4f, colorAxisXPos);
                        }

                        if (neighborZPos != null)
                        {
                            DrawZTestLine (origin, neighborZPos.pointPositionLocal + offset, colorAxisZPos);
                            DrawZTestLine (neighborZPos.pointPositionLocal + offset, neighborZPos.pointPositionLocal + offset * 4f, colorAxisZPos);
                        }

                        if (neighborXNeg != null)
                            DrawZTestLine (origin, neighborXNeg.pointPositionLocal + offset, colorAxisXNeg);

                        if (neighborZNeg != null)
                            DrawZTestLine (origin, neighborZNeg.pointPositionLocal + offset, colorAxisZNeg);

                        var direction = AreaAssetHelper.GetSurfaceDirection (lastSpotHovered.spotConfiguration);
                        var dirColor = direction == Vector3.up ? colorAxisYPos : colorAxisZPos;
                        DrawZTestLine (lastSpotHovered.instancePosition, lastSpotHovered.instancePosition + direction * 3f, dirColor);
                        DrawZTestWireCube (lastSpotHovered.instancePosition, Color.white.Opaque (), Color.cyan.WithAlpha (0.5f), 0.5f);
                    }
                }
                else
                {
                    if (lastPointHovered != null)
                    {
                        var halfExtentsCube = Vector3.one * 1.4f;
                        var halfExtentsBracket = Vector2.one * 1.3f;

                        Color GetColorFromMain (AreaVolumePoint point)
                        {
                            if (point == null)
                                return colorOffsetNeutral;

                            if (point.indestructibleIndirect)
                                return colorVolumeMainIndestructibleIndr;

                            if (AreaManager.IsPointIndestructible (point, false, true, true, false, false))
                                return colorVolumeMainIndestructibleHard;

                            if (!point.destructible)
                                return colorVolumeMainIndestructibleFlag;

                            if (point.destructionUntracked)
                                return colorVolumeMainDestructibleUntracked;

                            return colorVolumeMainDestructible;
                        }

                        DrawAAWireCube (lastPointHovered.pointPositionLocal, halfExtentsCube, GetColorFromMain (lastPointHovered));

                        var neighborXPos = lastPointHovered.pointsInSpot[1];
                        var neighborXNeg = lastPointHovered.pointsWithSurroundingSpots[6];

                        Color GetColorFromNeighbor (AreaVolumePoint point)
                        {
                            if (point == null)
                                return colorOffsetNeutral;

                            if (AreaManager.IsPointIndestructible (point, false, true, true, false, false))
                                return colorVolumeNeighborPrimaryIndestructibleHard;

                            if (!point.destructible)
                                return colorVolumeNeighborPrimaryIndestructibleFlag;

                            if (point.destructionUntracked)
                                return colorVolumeNeighborPrimaryDestructibleUntracked;

                            return colorVolumeNeighborPrimaryDestructible;
                        }

                        if (!e.shift)
                        {
                            if (neighborXPos != null && neighborXPos.pointState != AreaVolumePointState.Empty)
                                DrawAAWireCube (neighborXPos.pointPositionLocal, halfExtentsCube, GetColorFromNeighbor (neighborXPos));

                            if (neighborXNeg != null && neighborXNeg.pointState != AreaVolumePointState.Empty)
                                DrawAAWireCube (neighborXNeg.pointPositionLocal, halfExtentsCube, GetColorFromNeighbor (neighborXNeg));

                            var neighborZPos = lastPointHovered.pointsInSpot[2];
                            var neighborZNeg = lastPointHovered.pointsWithSurroundingSpots[5];

                            if (neighborZPos != null && neighborZPos.pointState != AreaVolumePointState.Empty)
                                DrawAAWireCube (neighborZPos.pointPositionLocal, halfExtentsCube, GetColorFromNeighbor (neighborZPos));

                            if (neighborZNeg != null && neighborZNeg.pointState != AreaVolumePointState.Empty)
                                DrawAAWireCube (neighborZNeg.pointPositionLocal, halfExtentsCube, GetColorFromNeighbor (neighborZNeg));

                            var neighborYPos = lastPointHovered.pointsInSpot[4];
                            var neighborYNeg = lastPointHovered.pointsWithSurroundingSpots[3];

                            if (neighborYPos != null && neighborYPos.pointState != AreaVolumePointState.Empty)
                                DrawAAWireCube (neighborYPos.pointPositionLocal, halfExtentsCube, GetColorFromNeighbor (neighborYPos));

                            if (neighborYNeg != null && neighborYNeg.pointState != AreaVolumePointState.Empty)
                                DrawAAWireCube (neighborYNeg.pointPositionLocal, halfExtentsCube, GetColorFromNeighbor (neighborYNeg));
                        }
                        else
                        {
                            var neighborYNeg = lastPointHovered.pointsWithSurroundingSpots[3];
                            if (neighborYNeg != null)
                            {
                                var surfacePos = lastPointHovered.pointPositionLocal + Vector3.up * 1.5f;
                                var offset = neighborYNeg.terrainOffset * TilesetUtility.blockAssetSize;

                                DrawAAWireSquare (surfacePos, halfExtentsBracket, colorOffsetNeutral);
                                if (Mathf.Abs (offset) > 0.05f)
                                {
                                    var color = offset > 0f ? colorOffsetPos : colorOffsetNeg;
                                    DrawAAWireSquare (surfacePos + Vector3.up * offset, halfExtentsBracket, color);
                                    DrawAALine (surfacePos, surfacePos + Vector3.up * offset, 3f, color);
                                }
                            }
                        }
                    }
                }
            }

            if ((editingTarget == EditingTarget.Damage || editingTarget == EditingTarget.Volume && displayDestructibility) && am.boundsFull.x > 2 && am.boundsFull.z > 2 && am.boundsFull.y > 2)
            {
                Vector3 cameraPositionCurrent = Vector3.zero;
                if (SceneView.currentDrawingSceneView.camera != null)
                    cameraPositionCurrent = SceneView.currentDrawingSceneView.camera.transform.position;

                // Start at bottom layer
                int startIndex = am.boundsFull.x * am.boundsFull.z * (am.boundsFull.y - 1);
                var points = am.points;
                var damageRestrictionDepth = am.damageRestrictionDepth;
                var damagePenetrationDepth = am.damagePenetrationDepth;

                var halfExtentsCube = Vector3.one * 1.4f;
                var halfExtentsBracket = Vector2.one * 1.3f;
                var planeRotation = Quaternion.Euler (90f, 0f, 0f);
                var offsetVertical = new Vector3 (0f, 1.51f, 0f);

                // Progress for size of one layer
                int offsetLimit = am.boundsFull.x * am.boundsFull.z;

                for (int i = 0; i < offsetLimit; ++i)
                {
                    int index = i + startIndex;
                    var point = points[index];

                    float sqrDistance = Vector3.SqrMagnitude (cameraPositionCurrent - point.pointPositionLocal);
                    if (sqrDistance > interactionDistance)
                        continue;

                    // Traverse up until you exit damage restriction depth
                    var pointAbove = point.pointsWithSurroundingSpots[3];
                    var pointLast = point;

                    while (pointAbove != null && pointAbove.pointPositionIndex.y >= damageRestrictionDepth)
                    {
                        pointLast = pointAbove;
                        pointAbove = pointAbove.pointsWithSurroundingSpots[3];
                    }

                    if (pointLast == null || pointLast.pointState == AreaVolumePointState.Empty)
                        continue;

                    var pos = offsetVertical + pointLast.pointPositionLocal;
                    DrawZTestRect (pos, planeRotation, 1.51f, colorVolumeMainIndestructibleHard, colorVolumeFadedIndestructibleHard);
                    DrawZTestLine (pos, pointLast.pointPositionLocal, colorVolumeMainIndestructibleHard, colorVolumeFadedIndestructibleHard);

                    while (pointAbove != null)
                    {
                        if (!pointAbove.destructible)
                        {
                            pos = offsetVertical + pointAbove.pointPositionLocal;
                            DrawZTestRect (pos, planeRotation, 1.51f, colorVolumeMainIndestructibleFlag, colorVolumeFadedIndestructibleFlag);
                            DrawZTestLine (pos, pointAbove.pointPositionLocal, colorVolumeMainIndestructibleFlag, colorVolumeFadedIndestructibleFlag);
                        }

                        pointAbove = pointAbove.pointsWithSurroundingSpots[3];
                    }
                }
            }

            if (editingTarget == EditingTarget.Navigation)
            {
                var hc = Handles.color;
                Handles.color = Color.white.WithAlpha (1f);
                var colorMain = new HSBColor (0.5f, 1f, 1f, 1f).ToColor ();
                var colorSecondary = new HSBColor (0.5f, 0f, 1f, 1f).ToColor ();
                var colorCulled = new HSBColor (0.55f, 0.5f, 1f, 0.5f).ToColor ();
                var planeRotation = Quaternion.Euler (90f, 0f, 0f);

                Vector3 cameraPositionCurrent = Vector3.zero;
                Vector3 flatHorizontalUnitVector = new Vector3 (1.0f, 0.0f, 1.0f);
                if (SceneView.currentDrawingSceneView.camera != null)
                    cameraPositionCurrent = SceneView.currentDrawingSceneView.camera.transform.position;

                float interactionDistanceNavSqr = interactionDistanceNav * interactionDistanceNav;

                foreach (var kvp in am.navOverrides)
                {
                    var index = kvp.Key;
                    if (!index.IsValidIndex (am.points))
                        continue;

                    var navOverride = kvp.Value;
                    var pointPos = am.points[index].instancePosition;

                    float sqrDistance = Vector3.SqrMagnitude (Vector3.Scale (cameraPositionCurrent - pointPos, flatHorizontalUnitVector));
                    if (sqrDistance > interactionDistanceNavSqr)
                        continue;

                    var center = pointPos + Vector3.up * navOverride.offsetY;
                    var saved = am.navOverridesSaved.ContainsKey (index);
                    var color = saved ? colorAxisYPos : colorMain;

                    DrawZTestCube (center, Quaternion.identity, 0.5f, color, colorCulled);
                    DrawZTestRect (center, planeRotation, 1f, color, colorCulled);
                    DrawZTestRect (pointPos, planeRotation, 0.25f, colorSecondary, colorCulled);
                }

                #if !PB_MODSDK
                var graph = PhantomNavGraph.areaNodes;
                #else
                var graph = AreaNavUtility.graph;
                #endif
                if (graph != null)
                {
                    for (int i = 0; i < graph.Count; ++i)
                    {
                        var node = graph[i];
                        reusedLinks = node.GetLinks ();
                        if (reusedLinks == null)
                            continue;

                        for (int n = 0; n < reusedLinks.Count; ++n)
                        {
                            var link = reusedLinks[n];
                            if (!link.destinationIndex.IsValidIndex (graph))
                                continue;

                            var nodeLinkDestination = graph[link.destinationIndex];
                            Vector3 positionStart = node.GetPosition ();
                            Vector3 positionEnd = graph[link.destinationIndex].GetPosition ();

                            float sqrDistance = Vector3.SqrMagnitude (Vector3.Scale (cameraPositionCurrent - positionStart, flatHorizontalUnitVector));
                            if (sqrDistance > interactionDistanceNavSqr)
                                continue;

                            if (link.type == AreaNavLinkType.Horizontal)
                            {
                                var dir = (positionEnd - positionStart).normalized;
                                var right = Vector3.Cross (dir, Vector3.up);
                                pointArrayABC[0] = positionStart;
                                pointArrayABC[1] = (positionStart + positionEnd) * 0.5f + right * navLinkSeparation;
                                pointArrayABC[2] = (positionEnd + pointArrayABC[1]) * 0.5f;

                                Handles.color = colorLinkHorizontal;
                                Handles.DrawAAPolyLine (3f, pointArrayABC);
                            }
                            else if (link.type == AreaNavLinkType.Diagonal)
                            {
                                var dir = (positionEnd - positionStart).normalized;
                                var right = Vector3.Cross (dir, Vector3.up);
                                pointArrayABC[0] = positionStart;
                                pointArrayABC[1] = (positionStart + positionEnd) * 0.5f + right * navLinkSeparation;
                                pointArrayABC[2] = (positionEnd + pointArrayABC[1]) * 0.5f;

                                Handles.color = colorLinkDiagonal;
                                Handles.DrawAAPolyLine (3f, pointArrayABC);
                            }
                            else if (link.type == AreaNavLinkType.JumpUp)
                            {
                                pointArrayABCDE[0] = positionStart;
                                pointArrayABCDE[1] = Vector3.Lerp (positionStart, positionEnd, 0.25f) + new Vector3 (0f, 1.5f, 0f);
                                pointArrayABCDE[2] = Vector3.Lerp (positionStart, positionEnd, 0.50f) + new Vector3 (0f, 2f, 0f);
                                pointArrayABCDE[3] = Vector3.Lerp (positionStart, positionEnd, 0.75f) + new Vector3 (0f, 1.25f, 0f);
                                pointArrayABCDE[4] = positionEnd;

                                Handles.color = colorLinkJumpUp;
                                Handles.DrawAAPolyLine (3f, pointArrayABCDE);
                            }
                            else if (link.type == AreaNavLinkType.JumpDown)
                            {
                                pointArrayABCD[0] = positionStart;
                                pointArrayABCD[1] = Vector3.Lerp (positionStart, positionEnd, 0.5f) + new Vector3 (0f, 1.5f, 0f);
                                pointArrayABCD[2] = Vector3.Lerp (positionStart, positionEnd, 0.75f) + Vector3.up;
                                pointArrayABCD[3] = positionEnd;

                                Handles.color = colorLinkJumpDown;
                                Handles.DrawAAPolyLine (3f, pointArrayABCD);
                            }
                            else if (link.type == AreaNavLinkType.JumpOverDrop)
                            {
                                pointArrayABC[0] = positionStart;
                                pointArrayABC[1] = (positionStart + positionEnd) * 0.5f + Vector3.up;
                                pointArrayABC[2] = positionEnd;

                                Handles.color = colorLinkJumpOverDrop;
                                Handles.DrawAAPolyLine (3f, pointArrayABC);
                            }
                            else if (link.type == AreaNavLinkType.JumpOverClimb)
                            {
                                pointArrayABCDE[0] = positionStart;
                                pointArrayABCDE[1] = Vector3.Lerp (positionStart, positionEnd, 0.25f) + new Vector3 (0f, 3f, 0f);
                                pointArrayABCDE[2] = Vector3.Lerp (positionStart, positionEnd, 0.50f) + new Vector3 (0f, 4f, 0f);
                                pointArrayABCDE[3] = Vector3.Lerp (positionStart, positionEnd, 0.75f) + new Vector3 (0f, 3f, 0f);
                                pointArrayABCDE[4] = positionEnd;

                                Handles.color = colorLinkJumpOverClimb;
                                Handles.DrawAAPolyLine (3f, pointArrayABCDE);
                            }
                        }
                    }

                    Handles.color = Color.white.WithAlpha (1f);
                    for (int i = 0; i < graph.Count; ++i)
                    {
                        var pos = graph[i].GetPosition ();
                        float sqrDistance = Vector3.SqrMagnitude (Vector3.Scale (cameraPositionCurrent - pos, flatHorizontalUnitVector));
                        if (sqrDistance > interactionDistanceNavSqr)
                            continue;

                        Handles.SphereHandleCap (0, pos, Quaternion.identity, 0.15f, EventType.Repaint);
                    }
                }

                Handles.color = hc;
            }

            if (editingTarget == EditingTarget.Props)
            {
                //Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                for (int i = 0; i < am.placementsProps.Count; ++i)
                {
                    AreaPlacementProp placement = am.placementsProps[i];

                    if (placement == null) // || !placement.prototype.prefab.allowPositionOffset)
                        continue;

                    Vector3 placementPositionCurrent = placement.state.cachedRootPosition;
                    if (SceneView.currentDrawingSceneView.camera != null)
                    {
                        float sqrDistance = Vector3.SqrMagnitude (SceneView.currentDrawingSceneView.camera.transform.position - placementPositionCurrent);
                        if (sqrDistance > interactionDistance)
                            continue;
                    }

                    // Important to make button size constant and distance-independent
                    // Otherwise there will be bugs with prop selection, off-screen props get selected if you click on an empty spot, etc.
                    float size = HandleUtility.GetHandleSize (placementPositionCurrent);

                    bool handleUsed = false;
                    Handles.color = Color.white.WithAlpha (0.35f);
                    handleUsed = Handles.Button (placementPositionCurrent, Quaternion.identity, size * handleSize, size * pickSize, Handles.DotHandleCap);

                    if (handleUsed)
                    {
                        propPlacementHandled = placement;
                        propPlacementListIndex = placement.pivotIndex;
                        Repaint ();
                    }

                    if (propPlacementHandled == placement && am.indexesOccupiedByProps.ContainsKey (propPlacementListIndex))
                    {
                        // Draw origin pivot box
                        AreaVolumePoint point = am.points[placement.pivotIndex];
                        Handles.color = Color.white;
                        Handles.DrawWireCube(point.instancePosition, handleSizeCube);

                        EditorGUI.BeginChangeCheck ();
                        Vector3 placementPositionModified = Handles.DoPositionHandle (placementPositionCurrent, Quaternion.Euler (0f, -90f * placement.rotation, 0f)); // Quaternion.LookRotation (Vector3.Normalize (point - ...));
                        if (EditorGUI.EndChangeCheck ())
                        {
                            // Update point variable again to prevent UI selection bugs
                            point = am.points[placement.pivotIndex];
                            Vector3 differenceCounterRotated = Quaternion.Euler (0f, 90f * placement.rotation, 0f) * (placementPositionModified - placementPositionCurrent);
                            Vector3 differenceInLocalSpace = (Quaternion)placement.state.cachedRootRotation * (placementPositionModified - placementPositionCurrent);

                            if (placement.prototype.prefab.compatibility == AreaProp.Compatibility.Floor)
                            {
                                placement.offsetX = Mathf.Clamp (placement.offsetX + differenceCounterRotated.x, -1.5f, 1.5f);
                                placement.offsetZ = Mathf.Clamp (placement.offsetZ + differenceCounterRotated.z, -1.5f, 1.5f);
                            }
                            else if (placement.prototype.prefab.compatibility == AreaProp.Compatibility.WallStraightMiddle)
                            {
                                //placement.offsetX = Mathf.Clamp (placement.offsetX + differenceInLocalSpace.x, -1.5f, 1.5f);
                                placement.offsetX = Mathf.Clamp (placement.offsetX + differenceCounterRotated.x, -1.5f, 1.5f);
                                placement.offsetZ = Mathf.Clamp (placement.offsetZ + differenceCounterRotated.y, -1.5f, 1.5f);
                            }
                            else if
                            (
                                placement.prototype.prefab.compatibility == AreaProp.Compatibility.WallStraightBottomToFloor ||
                                placement.prototype.prefab.compatibility == AreaProp.Compatibility.WallStraightTopToFloor
                            )
                            {
                                //placement.offsetX = Mathf.Clamp (placement.offsetX + differenceInLocalSpace.x, -1.5f, 1.5f);
                                placement.offsetZ = Mathf.Clamp (placement.offsetZ + differenceCounterRotated.z, -1.5f, 1.5f);
                            }

                            placement.offsetX = placement.offsetX.Truncate (2);
                            placement.offsetZ = placement.offsetZ.Truncate (2);

                            // Debug.LogWarning ("In-editor prop offset update not implemented yet");
                            // placement.instanceLegacy.transform.position = point.pointPositionLocal + new Vector3 (1f, -1f, 1f) * (TilesetUtility.blockAssetSize / 2f) + AreaUtility.GetPropOffsetAsVector (placement);

                            placement.UpdateOffsets (am);
                            UtilityECS.ScheduleUpdate ();


                        }
                    }
                }
            }

            /*
            if (am.points != null && showStructuralAnalysis)
            {
                Color hc = Handles.color;
                Color colorNonIsolated = new HSBColor (0f, 0f, 0.8f, 1f).ToColor ();

                for (int i = 0; i < am.points.Count; ++i)
                {
                    AreaVolumePoint p = am.points[i];
                    if (p != null && p.pointPositionIndex.y < am.damageRestrictionDepth && p.structuralParent != null)
                    {
                        Handles.color = p.structuralGroup == 0 ? colorNonIsolated : new HSBColor (((float)p.structuralGroup / 7.8423f) % 1f, 1f, 1f, 1f).ToColor ();
                        Handles.DrawAAPolyLine (4f, p.pointPositionLocal, p.structuralParent.pointPositionLocal);
                        Handles.CubeHandleCap (0, p.pointPositionLocal, Quaternion.identity, 0.5f, EventType.Repaint);

                        if (p.structuralGroup != 0)
                        {
                            Handles.color = Color.white;
                            Handles.Label (p.pointPositionLocal, p.structuralGroup.ToString (), EditorStyles.whiteLabel);
                        }
                    }
                }

                Handles.color = hc;
            }
            */

            if (lastSearchResults != null && showLastSearchResults)
            {
                Color hc = Handles.color;
                Color colorIsolated = new HSBColor (0f, 0f, 0.8f, 1f).ToColor ();
                Vector3 spotOffset = new Vector3 (1f, -1f, 1f) * (TilesetUtility.blockAssetSize / 2f);
                Quaternion rotation = Quaternion.Euler (90f, 0f, 0f);
                byte maskFloor = AreaNavUtility.configFloor;

                for (int i = 0; i < lastSearchResults.Count; ++i)
                {
                    AreaVolumePointSearchData psd = lastSearchResults[i];
                    if (psd != null && psd.status < 1000)
                    {
                        bool parented = psd.parent != null;
                        bool flat = psd.point.spotConfiguration == maskFloor;

                        Handles.color = parented ? new HSBColor (((float)psd.status / 734.8423f) % 1f, 1f, 1f, 1f).ToColor () : colorIsolated;

                        if (flat)
                            Handles.RectangleHandleCap (0, psd.point.pointPositionLocal + spotOffset, rotation, 1.5f, EventType.Repaint);

                        if (parented)
                        {
                            Handles.CubeHandleCap (0, psd.point.pointPositionLocal + spotOffset, Quaternion.identity, 0.5f, EventType.Repaint);
                            Handles.DrawAAPolyLine (4f, psd.point.pointPositionLocal + spotOffset, psd.parent.point.pointPositionLocal + spotOffset);
                        }
                        else
                            Handles.CubeHandleCap (0, psd.point.pointPositionLocal + spotOffset, Quaternion.identity, 1f, EventType.Repaint);
                    }
                }

                Handles.color = hc;
            }

            if (editingTarget == EditingTarget.Roads)
            {
                Color hc = Handles.color;
                Handles.color = new HSBColor (0.15f, 1f, 1f, 0.5f).ToColor ();

                Vector3 offset = new Vector3 (0, 1.5f, 0f);
                Quaternion rotation = Quaternion.Euler (90f, 45f, 0f);
                float scale = 1.5f * 0.7075f * 0.5f;

                for (int i = 0; i < am.points.Count; ++i)
                {
                    AreaVolumePoint point = am.points[i];
                    if (point == null || !point.road)
                        continue;

                    var pos = point.pointPositionLocal + offset;
                    Handles.RectangleHandleCap (0, pos, rotation, scale, EventType.Repaint);
                    // Handles.DrawLine (pos, point.pointPositionLocal);
                    // Handles.CubeHandleCap (0, pos, Quaternion.identity, 0.25f);
                }

                Handles.color = hc;
            }

            if (editingTarget == EditingTarget.Transfer)
            {
                var sourcePosA = AreaUtility.GetLocalPositionFromGridPosition (am.clipboardOrigin);
                var sourcePosB = AreaUtility.GetLocalPositionFromGridPosition (am.clipboardOrigin + am.clipboardBoundsRequested + Vector3Int.size1x1x1Neg);
                int sourceCornerAIndex = AreaUtility.GetIndexFromInternalPosition (am.clipboardOrigin, am.boundsFull);
                int sourceCornerBIndex = AreaUtility.GetIndexFromInternalPosition (am.clipboardOrigin + am.clipboardBoundsRequested + Vector3Int.size1x1x1Neg, am.boundsFull);

                var sourceColorMain = new HSBColor (0.0f, 1f, 1f, 1f).ToColor ();
                var sourceColorCulled = new HSBColor (0.0f, 0.65f, 0.5f, 1f).ToColor ();
                if (sourceCornerAIndex != -1 && sourceCornerBIndex != -1)
                {
                    sourceColorMain = new HSBColor (0.25f, 1f, 1f, 1f).ToColor ();
                    sourceColorCulled = new HSBColor (0.1f, 0.65f, 0.5f, 1f).ToColor ();
                }

                DrawZTestVolume (sourcePosA, sourcePosB, sourceColorMain, sourceColorCulled);
                DrawVolumeDirection(sourcePosA, sourcePosB, Vector3.right, sourceColorMain);

                var targetPosA = AreaUtility.GetLocalPositionFromGridPosition (am.targetOrigin);
                var targetPosB = AreaUtility.GetLocalPositionFromGridPosition (am.targetOrigin + am.clipboard.clipboardBoundsSaved + Vector3Int.size1x1x1Neg);
                int targetCornerAIndex = AreaUtility.GetIndexFromInternalPosition (am.targetOrigin, am.boundsFull);
                int targetCornerBIndex = AreaUtility.GetIndexFromInternalPosition (am.targetOrigin + am.clipboard.clipboardBoundsSaved + Vector3Int.size1x1x1Neg, am.boundsFull);

                var targetColorMain = new HSBColor (0.0f, 1f, 1f, 1f).ToColor ();
                var targetColorCulled = new HSBColor (0.0f, 0.65f, 0.5f, 1f).ToColor ();
                if (targetCornerAIndex != -1 && targetCornerBIndex != -1)
                {
                    targetColorMain = new HSBColor (0.55f, 1f, 1f, 1f).ToColor ();
                    targetColorCulled = new HSBColor (0.6f, 0.65f, 0.5f, 1f).ToColor ();
                }

                DrawZTestVolume (targetPosA, targetPosB, targetColorMain, targetColorCulled);
                DrawVolumeDirection(targetPosA, targetPosB, am.clipboard.clipboardDirection.ToVector3(), targetColorMain);
            }

            bool shift = (e.modifiers & EventModifiers.Shift) != 0;
            bool ctrl = (e.modifiers & EventModifiers.Control) != 0;
            if (e.type == EventType.KeyDown)
            {
                if (e.isKey)
                {
                    if (e.keyCode == KeyCode.Alpha2 || e.keyCode == KeyCode.F)
                        e.Use ();    // if you don't use the event, the default action will still take place.
                }
            }


            /*
            if (Contexts.sharedInstance.combat.hasPathfindingLink)
            {
                var nodes = Contexts.sharedInstance.combat.pathfindingLink.graph.nodes;
                if (nodes != null)
                {
                    for (int i = 0; i < nodes.Length; ++i)
                    {
                        AreaNavNode node = nodes[i];
                        if (node.links == null)
                            continue;

                        for (int n = 0; n < node.links.Count; ++n)
                        {
                            AreaNavLink link = node.links[n];
                            if (link.destinationIndex < 0 || link.destinationIndex >= nodes.Length)
                                continue;

                            AreaNavNode nodeLinkDestination = nodes[link.destinationIndex];

                            Vector3 positionStart = (Vector3)node.position;
                            Vector3 positionEnd = (Vector3)nodes[link.destinationIndex].position;
                            Color handleColorCached = Handles.color;

                            if (link.type == AreaNavLinkType.Horizontal)
                            {
                                Handles.color = colorLinkHorizontal;
                                Handles.DrawAAPolyLine (5f, new Vector3[] { positionStart, positionEnd });
                            }
                            else if (link.type == AreaNavLinkType.JumpUp)
                            {
                                Handles.color = colorLinkJumpUp;
                                Vector3 positionMidpointA = Vector3.Lerp (positionStart, positionEnd, 0.25f) + new Vector3 (0f, 1.5f, 0f);
                                Vector3 positionMidpointB = Vector3.Lerp (positionStart, positionEnd, 0.50f) + new Vector3 (0f, 2f, 0f);
                                Vector3 positionMidpointC = Vector3.Lerp (positionStart, positionEnd, 0.75f) + new Vector3 (0f, 1.25f, 0f);
                                Handles.DrawAAPolyLine (5f, new Vector3[] { positionStart, positionMidpointA, positionMidpointB, positionMidpointC, positionEnd });
                            }
                            else if (link.type == AreaNavLinkType.JumpDown)
                            {
                                Handles.color = colorLinkJumpDown;
                                Vector3 positionMidpointA = Vector3.Lerp (positionStart, positionEnd, 0.5f) + new Vector3 (0f, 1.5f, 0f);
                                Vector3 positionMidpointB = Vector3.Lerp (positionStart, positionEnd, 0.75f) + new Vector3 (0f, 1f, 0f);
                                Handles.DrawAAPolyLine (5f, new Vector3[] { positionStart, positionMidpointA, positionMidpointB, positionEnd });
                            }
                            else if (link.type == AreaNavLinkType.JumpOverDrop)
                            {
                                Handles.color = colorLinkJumpOverDrop;
                                Vector3 positionMidpoint = (positionStart + positionEnd) / 2f + new Vector3 (0f, 1f, 0f);
                                Handles.DrawAAPolyLine (5f, new Vector3[] { positionStart, positionMidpoint, positionEnd });
                            }
                            else if (link.type == AreaNavLinkType.JumpOverClimb)
                            {
                                Handles.color = colorLinkJumpOverClimb;
                                Vector3 positionMidpointA = Vector3.Lerp (positionStart, positionEnd, 0.25f) + new Vector3 (0f, 3f, 0f);
                                Vector3 positionMidpointB = Vector3.Lerp (positionStart, positionEnd, 0.50f) + new Vector3 (0f, 4f, 0f);
                                Vector3 positionMidpointC = Vector3.Lerp (positionStart, positionEnd, 0.75f) + new Vector3 (0f, 3f, 0f);
                                Handles.DrawAAPolyLine (5f, new Vector3[] { positionStart, positionMidpointA, positionMidpointB, positionMidpointC, positionEnd });
                            }

                            Handles.color = handleColorCached;
                        }
                    }
                    for (int i = 0; i < nodes.Length; ++i)
                    {
                        Handles.CubeHandleCap (0, am.transform.position + (Vector3)nodes[i].position, Quaternion.identity, 0.5f);
                    }
                }
            }
            */

            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Keypad0)
                    SetEditingTarget (am, EditingTarget.Volume);
                else if (e.keyCode == KeyCode.Keypad1)
                    SetEditingTarget (am, EditingTarget.Tileset);
                else if (e.keyCode == KeyCode.Keypad2)
                    SetEditingTarget (am, EditingTarget.Spot);
               // else if (e.keyCode == KeyCode.Keypad5)
               //     SetEditingTarget (am, EditingTarget.RoadCurves);
                else if (e.keyCode == KeyCode.Keypad6)
                    SetEditingTarget (am, EditingTarget.Props);
            }

            GUILayout.BeginArea (new Rect (new Vector2 (Screen.width - 95f, Screen.height - 565f), new Vector2 (95f, 565f)));
            DrawToolbarEditing (am, true);
            GUILayout.EndArea ();

            if (editingTarget == EditingTarget.Volume)
            {
                GUILayout.BeginVertical (EditorStyles.helpBox, GUILayout.Width (200f));
                GUILayout.Label ("Volume editing mode", EditorStyles.boldLabel);

                GUILayout.Space (8f);
                GUILayout.BeginVertical ("Box");

                GUILayout.BeginVertical ("Box");
                {
                    GUILayout.Label ("Current tileset:");
                    GUILayout.BeginVertical ("Box");
                    GUILayout.Label (editingTilesetSelected != null ? editingTilesetSelected.name : "null", EditorStyles.boldLabel);
                    GUILayout.EndVertical ();
                    swapTilesetOnVolumeEdits = EditorGUILayout.ToggleLeft (new GUIContent("Swap tileset", "Swap to 'Current tileset' when editing the volume"), swapTilesetOnVolumeEdits);
                    swapColorOnVolumeEdits = EditorGUILayout.ToggleLeft (new GUIContent("Swap color", "Swap to 'Current color' when editing the volume"), swapColorOnVolumeEdits);
                }
                GUILayout.EndVertical ();

                GUILayout.Space (8f);
                DrawBrushSelector ();

                GUILayout.Space (8f);
                GUILayout.BeginVertical ("Box");

                volumeBrushDepthRange.x = Mathf.Clamp (EditorGUILayout.IntField ("Edit depth min.", volumeBrushDepthRange.x), 0, boundsFullCached.y);
                volumeBrushDepthRange.y = Mathf.Clamp (EditorGUILayout.IntField ("Edit depth max.", volumeBrushDepthRange.y), 0, boundsFullCached.y);
                volumeBrushDepth = Mathf.Clamp (EditorGUILayout.IntField ("Brush depth", volumeBrushDepth), 1, boundsFullCached.y - 1);
                volumeBrushDepthGoesUp = EditorGUILayout.Toggle ("Brush depth goes up", volumeBrushDepthGoesUp);

                displayDestructibility = EditorGUILayout.ToggleLeft (new GUIContent("Display destructibility", "Show the 'floor' of damageable depth on the map"), displayDestructibility);
                GUILayout.EndVertical ();

                if (lastPointHovered != null)
                {
                    var lp = lastPointHovered;

                    GUILayout.Space (8f);
                    GUILayout.BeginVertical ("Box");
                    GUILayout.Label ($"Last hovered point {lp.pointPositionIndex.ToString ()}", EditorStyles.miniLabel);
                    GUILayout.Label ($"Index {lp.spotIndex.ToString ()}");

                    GUILayout.Space (6f);
                    lp.destructible = EditorGUILayout.ToggleLeft (new GUIContent("Destructible", "Mark last hovered cell as destructible or indestructible"), lp.destructible);
                    lp.destructionUntracked = EditorGUILayout.ToggleLeft (new GUIContent("Destruction untracked", "Choose not to count this cell towards total destruction (this affects atmosphere changes during combat)"), lp.destructionUntracked);
                    propagateDestructibilityDown = EditorGUILayout.ToggleLeft (new GUIContent("Propagate destructibility", "On toggling desturctibility, propagate the value up or down infinitely"), propagateDestructibilityDown);

                    GUILayout.Space (6f);
                    GUILayout.Label ($"  Local pos.: {lastPointHovered.pointPositionLocal}", EditorStyles.miniLabel);
                    GUILayout.Label ($"  Instance pos.: {lastPointHovered.instancePosition}", EditorStyles.miniLabel);

                    GUILayout.Space (4f);
                    GUILayout.Label ($"  Integrity (main): {lastPointHovered.integrity:0.###}", EditorStyles.miniLabel);
                    GUILayout.Label ($"  Integrity (anim.): {lastPointHovered.integrityForDestructionAnimation:0.###}", EditorStyles.miniLabel);
                    GUILayout.Label ($"  Interior (vis.): {lastPointHovered.instanceVisibilityInterior:0.###}", EditorStyles.miniLabel);

                    var entityMainPresent = AreaManager.InstancedModelExists (lastPointHovered.spotIndex);
                    var entityInteriorPresent = AreaManager.InstancedModelExists (lastPointHovered.spotIndex, interior: true);
                    if (entityMainPresent | entityInteriorPresent)
                    {
                        GUILayout.Space (4f);
                        GUILayout.Label ($"  Entity (main): {(entityMainPresent ? "present" : "-")}", EditorStyles.miniLabel);
                        GUILayout.Label ($"  Entity (interior): {(entityInteriorPresent ? "present" : "-")}", EditorStyles.miniLabel);

                        AreaManager.GetShaderDamage (lastPointHovered, out var damageTop, out var damageBottom, out var damageCritical);

                        GUILayout.Space (4f);
                        GUILayout.Label ($"  Sh. integrity (top): {damageTop}", EditorStyles.miniLabel);
                        GUILayout.Label ($"  Sh. integrity (low): {damageBottom}", EditorStyles.miniLabel);
                        GUILayout.Label ($"  Sh. damage (crit.): {damageCritical}", EditorStyles.miniLabel);
                    }

                    GUILayout.EndVertical ();
                }

                GUILayout.EndVertical ();
                GUILayout.EndVertical ();

                DrawHotkeyHintsPanel("[LMB] - Fill cell     [RMB] - Empty cell     [MMB] - Mark indestructible     [Ctrl + MMB] - Mark destructible",
                                        "[Q] - Toggle Destruction Untracked     [Shift + LMB/RMB] - Cell offset     [Shift + MW▲▼] - Change current tileset");
            }
            else if (editingTarget == EditingTarget.Transfer)
            {
                GUILayout.BeginVertical (EditorStyles.helpBox, GUILayout.Width (200f));
                GUILayout.Label ("Copy/pasting mode", EditorStyles.boldLabel);

                GUILayout.Space (8f);
                GUILayout.Label ("Source pivot and volume", EditorStyles.miniLabel);

                am.clipboardOrigin = UtilityCustomInspector.DrawVector3Int (string.Empty, am.clipboardOrigin);
                am.clipboardBoundsRequested = UtilityCustomInspector.DrawVector3Int (string.Empty, am.clipboardBoundsRequested);

                if (GUILayout.Button ("Shrinkwrap"))
                {
					var shrinkBounds = am.GetShrinkwrapBounds(am.clipboardOrigin, am.clipboardOrigin + am.clipboardBoundsRequested + Vector3Int.size1x1x1Neg);

					am.clipboardOrigin.y = shrinkBounds.topY;
					am.clipboardBoundsRequested.y = shrinkBounds.bottomY - shrinkBounds.topY + 1;
                }

                if (GUILayout.Button ("Copy"))
                    am.CopyVolume (am.clipboardOrigin, am.clipboardOrigin + am.clipboardBoundsRequested + Vector3Int.size1x1x1Neg);

				if (am.clipboard.IsValid)
                {
	                GUILayout.Space (8f);

                    GUILayout.BeginHorizontal ();
                    GUILayout.Label ("Points\nBounds\nTarget pivot", EditorStyles.miniLabel);
                    GUILayout.Label (string.Format ("{0}\n{1}", am.clipboard.clipboardPointsSaved.Count, am.clipboard.clipboardBoundsSaved), EditorStyles.miniLabel);
                    GUILayout.EndHorizontal ();

                    am.targetOrigin = UtilityCustomInspector.DrawVector3Int (string.Empty, am.targetOrigin);

                    if (GUILayout.Button ("Ground"))
                    {
	                    var shrinkBounds = am.GetShrinkwrapBounds(am.targetOrigin, am.targetOrigin + am.clipboard.clipboardBoundsSaved + Vector3Int.size1x1x1Neg);

	                    am.targetOrigin.y = shrinkBounds.bottomY - am.clipboard.clipboardBoundsSaved.y + 1;
                    }

					//Rotate clipboard controls
                    GUILayout.BeginHorizontal ();
                    GUILayout.Label ("Rotate: ", EditorStyles.miniLabel);
                    GUILayout.FlexibleSpace ();
                    if (GUILayout.Button ("←", GUILayout.Width (31f)))
                    {
	                    am.clipboard.Rotate(false);
                    }
                    if (GUILayout.Button ("↔", GUILayout.Width (31f)))
                    {
	                    am.clipboard.Rotate(false);
	                    am.clipboard.Rotate(false);
                    }
                    if (GUILayout.Button ("→", GUILayout.Width (31f)))
                    {
	                    am.clipboard.Rotate(true);
                    }
                    GUILayout.EndHorizontal ();

                    am.brushApplicationMode = (AreaManager.BrushApplicationMode) EditorGUILayout.EnumPopup (am.brushApplicationMode);
                    if (GUILayout.Button ("Paste (overwrite)"))
                        am.PasteVolume (am.targetOrigin, AreaManager.BrushApplicationMode.Overwrite);
                    if (GUILayout.Button ("Paste (additively)"))
                        am.PasteVolume (am.targetOrigin, AreaManager.BrushApplicationMode.Additive);
                    if (GUILayout.Button ("Paste (subtractive)"))
                        am.PasteVolume (am.targetOrigin, AreaManager.BrushApplicationMode.Subtractive);

                    GUILayout.Space (8f);
                    GUILayout.BeginVertical ("Box");
                    UtilityCustomInspector.DrawField ("Transfer volume", () => am.transferVolume = EditorGUILayout.Toggle (am.transferVolume));
                    UtilityCustomInspector.DrawField ("Transfer props", () => am.transferProps = EditorGUILayout.Toggle (am.transferProps));
                    GUILayout.EndVertical ();

                    GUILayout.BeginVertical ("Box");

                    am.clipboard.name = EditorGUILayout.TextField (am.clipboard.name);
                    if (GUILayout.Button ("Save to file"))
                    {
                        DataManagerLevelSnippet.SaveFromManager (am);

                        // var path = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), "Configs/AreaSnippets");
                        // var pathFromPanel = EditorUtility.SaveFilePanel("Save Clipboard", path, "snippet_name", "yaml");
                        // var filename = Path.GetFileNameWithoutExtension (pathFromPanel);
                        // Debug.Log ($"Saving snippet '{filename}' to: {path}");
                        // if (pathFromPanel.Length > 0)
                        //     am.clipboard?.SaveToYAML (pathFromPanel);
                    }

                    GUILayout.EndVertical ();
                }

                /*
                GUILayout.BeginVertical ("Box");

                var keysSnippets = DataManagerLevelSnippet.GetKeyList ();
                if (keysSnippets == null)
                {
                    GUILayout.Label ("No saved snippets", EditorStyles.miniLabel);
                    GUI.enabled = false;
                    if (GUILayout.Button ("Load", EditorStyles.miniButton)) { }
                    GUI.enabled = true;
                }
                else
                {
                    int index = -1;
                    var keySelected = DataManagerLevel.keySelected;

                    for (int i = 0, count = keysSnippets.Count; i < count; ++i)
                    {
                        var keyCandidate = keysSnippets[i];
                        if (string.Equals (keyCandidate, keySelected))
                        {
                            index = i;
                            break;
                        }
                    }

                    if (index == -1 && keysSnippets.Count > 0)
                    {
                        index = 0;
                        DataManagerLevel.keySelected = keysSnippets[0];
                    }

                    int indexNew = EditorGUILayout.Popup (index, keysSnippets.ToArray ());
                    if (indexNew != index)
                        keySelected = DataManagerLevel.keySelected = keysSnippets[indexNew];

                    GUI.enabled = index != -1;

                    if (GUILayout.Button ("Load", EditorStyles.miniButton))
                    {
                        DataManagerLevelSnippet.LoadDataFromKey (keySelected, true, out bool success);
                    }

                    GUI.enabled = true;
                }

                GUILayout.EndVertical ();
                */

                GUILayout.BeginVertical ("Box");
                if (GUILayout.Button ("Export mesh"))
                    am.ExportVolume (am.clipboardOrigin, am.clipboardOrigin + am.clipboardBoundsRequested + Vector3Int.size1x1x1Neg);
                UtilityCustomInspector.DrawField ("Combine", () => am.combineExports = EditorGUILayout.Toggle (am.combineExports));
                GUILayout.EndVertical ();

                GUILayout.EndVertical ();

                DrawHotkeyHintsPanel
                (
                    "[LMB] - Set copy origin     [RMB] - Set paste origin     [MMB] - Set bounds     [X] - Copy     [V] - Paste (overwrite)     [B] - Paste (additive)",
                    "[Alt + Shift] - Drag paste volume     [Alt + Shift + LMB] - Paste     [Alt + Shift + MW▲▼] - Adjust paste height"
                );
            }
            else if (editingTarget == EditingTarget.Damage)
            {
                GUILayout.BeginVertical (EditorStyles.helpBox, GUILayout.Width (200f));
                GUILayout.Label ("Damage editing mode", EditorStyles.boldLabel);

                GUILayout.Space (8f);
                GUILayout.BeginVertical ("Box");
                am.damageRestrictionDepth = EditorGUILayout.IntSlider ("Damageable depth", am.damageRestrictionDepth, 0, am.boundsFull.y);
                am.damagePenetrationDepth = EditorGUILayout.IntSlider ("Penetrateable depth", am.damagePenetrationDepth, 0, am.boundsFull.y);
                allowIndestructibleDestruction = EditorGUILayout.ToggleLeft (new GUIContent("Destroy indestructible", "Allow 'Volume damage' tool to destroy a cell even if it is marked as indestructible"), allowIndestructibleDestruction);
                GUILayout.EndVertical ();

                GUILayout.EndVertical ();

                DrawHotkeyHintsPanel("[LMB] - Restore     [RMB] - Destroy     [MW▲▼] - Adjust damage");
            }
            else if (editingTarget == EditingTarget.Color)
            {
                GUILayout.BeginVertical (EditorStyles.helpBox, GUILayout.Width (200f));
                GUILayout.Label ("Color editing mode", EditorStyles.boldLabel);

                GUILayout.Space (8f);
                GUILayout.BeginVertical ("Box");
                DrawSearchSelector ();
                //GUILayout.Space (8f);
                //UtilityCustomInspector.DrawField ("Absolute Colors", () => absoluteColorMode = EditorGUILayout.Toggle (absoluteColorMode));
                GUILayout.EndVertical ();

                GUILayout.BeginVertical ("Box");
                UtilityCustomInspector.DrawField ("Apply colors", () => applyMainOnColorApply = EditorGUILayout.Toggle (applyMainOnColorApply), true);
                GUILayout.Label ("Primary color (RGB)", EditorStyles.miniLabel);
                selectedPrimaryColor = HSBColor.FromColor(EditorGUILayout.ColorField (selectedPrimaryColor.ToColor ()));
                GUILayout.Space (3f);
                GUILayout.Label ("Primary color (HSV)", EditorStyles.miniLabel);
                selectedPrimaryColor.h = EditorGUILayout.Slider (selectedPrimaryColor.h, 0f, 1f);
                selectedPrimaryColor.s = EditorGUILayout.Slider (selectedPrimaryColor.s, 0f, 1f);
                selectedPrimaryColor.b = EditorGUILayout.Slider (selectedPrimaryColor.b, 0f, 1f);
                GUILayout.EndVertical ();

                GUILayout.BeginVertical ("Box");
                GUILayout.Label ("Secondary color (RGB)", EditorStyles.miniLabel);
                selectedSecondaryColor = HSBColor.FromColor(EditorGUILayout.ColorField (selectedSecondaryColor.ToColor ()));
                GUILayout.Space (3f);
                GUILayout.Label ("Secondary color (HSV)", EditorStyles.miniLabel);
                selectedSecondaryColor.h = EditorGUILayout.Slider (selectedSecondaryColor.h, 0f, 1f);
                selectedSecondaryColor.s = EditorGUILayout.Slider (selectedSecondaryColor.s, 0f, 1f);
                selectedSecondaryColor.b = EditorGUILayout.Slider (selectedSecondaryColor.b, 0f, 1f);
                selectedSecondaryColor.a = EditorGUILayout.Slider (selectedSecondaryColor.a, 0f, 1f);
                GUILayout.EndVertical ();

                GUILayout.BeginVertical ("Box");
                GUILayout.Space(5f);
                UtilityCustomInspector.DrawField ("Apply overlays", () => applyOverlaysOnColorApply = EditorGUILayout.Toggle (applyOverlaysOnColorApply), true);
                GUILayout.Space(5f);
                GUILayout.Label ("Overlays / emission", EditorStyles.miniLabel);

                var tileset = AreaTilesetHelper.GetTileset (lastSpotTileset);

                GUILayout.BeginHorizontal ();

                overrideValue = EditorGUILayout.FloatField (overrideValue);
                if (GUILayout.Button ("-1", EditorStyles.miniButton, GUILayout.Width (30f)))
                    overrideValue -= 1f;
                if (GUILayout.Button ("-0.1", EditorStyles.miniButton, GUILayout.Width (40f)))
                    overrideValue -= 0.1f;
                if (GUILayout.Button ("-", EditorStyles.miniButton, GUILayout.Width (30f)))
                    overrideValue = (Mathf.RoundToInt (overrideValue * 10f)) * 0.1f;
                if (GUILayout.Button ("+0.1", EditorStyles.miniButton, GUILayout.Width (40f)))
                    overrideValue += 0.1f;
                if (GUILayout.Button ("+1", EditorStyles.miniButton, GUILayout.Width (30f)))
                    overrideValue += 1f;

                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();
                if (GUILayout.Button ("Default", EditorStyles.miniButtonLeft))
                    overrideValue = 0f;
                if (GUILayout.Button ("Emissive", EditorStyles.miniButtonRight))
                    overrideValue = 1f;
                GUILayout.EndHorizontal ();
                overrideValue = Mathf.Clamp ((Mathf.RoundToInt (overrideValue * 10f)) * 0.1f, -10f, 1f);

                if (tileset != null && tileset.materialOverlays != null && tileset.materialOverlays.Count > 0)
                {
                    foreach (var kvp1 in tileset.materialOverlays)
                    {
                        var index = -kvp1.Key - 1f;
                        var label = $"{kvp1.Value.FirstLetterToUpperCase ()} ({index})";

                        if (GUILayout.Button (label, EditorStyles.miniButtonRight))
                            overrideValue = -kvp1.Key - 1f;
                    }
                }

                GUILayout.EndVertical ();

                GUILayout.EndVertical ();

                DrawHotkeyHintsPanel("[LMB] - Apply picked color     [RMB] - Apply default color     [MMB] - Pick color");
            }
            else if (editingTarget == EditingTarget.Tileset)
            {
                GUILayout.BeginVertical (EditorStyles.helpBox, GUILayout.Width (200f));
                GUILayout.Label ("Tileset editing mode", EditorStyles.boldLabel);

                GUILayout.BeginVertical ("Box");
                {
                    GUILayout.BeginVertical ("Box");
                    {
                        GUILayout.Label ("Current tileset:");
                        GUILayout.BeginVertical ("Box");
                        GUILayout.Label (editingTilesetSelected != null ? editingTilesetSelected.name : "null", EditorStyles.boldLabel);
                        GUILayout.EndVertical ();
                    }
                    GUILayout.EndVertical ();

                    GUILayout.Space (8f);
                    DrawSearchSelector ();
                    GUILayout.EndVertical ();
                }
                GUILayout.EndVertical ();

                DrawHotkeyHintsPanel("[LMB] - Set tileset     [MMB] - Pick tileset     [MW▲▼] - Select tileset");
            }
            else if (editingTarget == EditingTarget.Spot)
            {
                GUILayout.BeginVertical (EditorStyles.helpBox, GUILayout.Width (200f));
                {
                    GUILayout.Label ("Spot editing mode", EditorStyles.boldLabel);

                    GUILayout.Space (8f);
                    GUILayout.BeginVertical ("Box");
                    DrawSearchSelector ();
                    GUILayout.Space (4f);
                    {
                        UtilityCustomInspector.DrawField ("Subtype overwriting", () => clipboardMustOverwriteSubtype = EditorGUILayout.Toggle (clipboardMustOverwriteSubtype));
                        UtilityCustomInspector.DrawField ("Color overwriting", () => clipboardOverwriteColor = EditorGUILayout.Toggle (clipboardOverwriteColor));
                    }
                    GUILayout.EndVertical ();
                }
                GUILayout.EndVertical ();

                GUILayout.BeginVertical ("Box", GUILayout.MinWidth (200f));
                {
                    GUILayout.BeginVertical ("Box");
                    {
                        if (clipboardConfigurations != null && clipboardConfigurations.Count > 0 && AreaTilesetHelper.database.tilesets.ContainsKey (clipboardTileset))
                        {
                            EditorGUILayout.BeginHorizontal ("Box");
                            var tileset = AreaTilesetHelper.database.tilesets[clipboardTileset];
                            bool identifiersPresent = tileset.groupIdentifiers != null;

                            GUILayout.Label ("Configs\nTileset\nGroup/type\nOrientation");
                            GUILayout.Label (string.Format
                            (
                                "{0} ({1})\n{2} ({3})\n{4}/{5}\n{6} ({7})",
                                TilesetUtility.GetStringFromConfiguration (clipboardConfigurations[0]),
                                clipboardConfigurations[0],
                                tileset.name,
                                clipboardTileset,
                                identifiersPresent && tileset.groupIdentifiers.ContainsKey (clipboardGroup) ? $"{tileset.groupIdentifiers[clipboardGroup]} ({clipboardGroup})" : clipboardGroup.ToString (),
                                clipboardSubtype,
                                clipboardRotation,
                                clipboardFlipping ? "flipped" : "standard"
                            ));

                            EditorGUILayout.EndHorizontal ();
                        }

                        if (lastSpotInfoStyle == null)
                        {
                            lastSpotInfoStyle = EditorStyles.largeLabel;
                            lastSpotInfoStyle.richText = true;
                        }

                        GUILayout.Label (lastSpotInfoGroups, lastSpotInfoStyle);
                    }
                    GUILayout.EndVertical ();
                }
                GUILayout.EndVertical ();

                DrawHotkeyHintsPanel
                (
                "[LMB] - Rotate     [RMB] - Flip     [MMB] - Copy     [MW▲▼] - Change subtype     [Shift + MW▲▼] - Change group",
                "[V] - Paste subtype (using search)     [Shift + V] - Paste everything (only target)     [Q] - Randomize subtype"
                );
            }
            else if (editingTarget == EditingTarget.Navigation)
            {
                GUILayout.BeginVertical (EditorStyles.helpBox, GUILayout.Width (200f));
                GUILayout.Label ("Navigation editing mode", EditorStyles.boldLabel);
                GUILayout.Space (8f);

                GUILayout.Label ($"Saved nav overrides: {(am.navOverridesSaved != null ? am.navOverridesSaved.Count.ToString() : "-")}", EditorStyles.miniLabel);
                GUILayout.Label ($"All nav overrides: {(am.navOverrides != null ? am.navOverrides.Count.ToString() : "-")}", EditorStyles.miniLabel);

                GUILayout.BeginVertical ("Box");
                {
                    GUILayout.Label ("Link separation", EditorStyles.miniLabel);
                    navLinkSeparation = EditorGUILayout.Slider (navLinkSeparation, 0f, 0.5f);

                    GUILayout.Label ("Node view distance", EditorStyles.miniLabel);
                    interactionDistanceNav = EditorGUILayout.Slider (interactionDistanceNav, 50f, 600f);

                    if (lastSpotHovered != null)
                    {
                        if (am.navOverrides.ContainsKey (lastSpotHovered.spotIndex))
                            GUILayout.Label ("This override is autogenerated", EditorStyles.miniLabel);

                        if (am.navOverridesSaved.ContainsKey (lastSpotHovered.spotIndex))
                        {
                            GUILayout.Label ("Height", EditorStyles.miniLabel);
                            var navOverride = am.navOverridesSaved[lastSpotHovered.spotIndex];
                            navOverride.offsetY = EditorGUILayout.Slider (navOverride.offsetY, -1.5f, 1.5f);
                            if (GUILayout.Button ("Remove saved override", EditorStyles.miniButton))
                            {
                                am.navOverridesSaved.Remove (lastSpotHovered.spotIndex);
                                am.GenerateNavOverrides ();
                            }
                        }
                        else
                        {
                            if (GUILayout.Button ("Add saved override", EditorStyles.miniButton))
                            {
                                am.navOverridesSaved.Add (lastSpotHovered.spotIndex, new AreaDataNavOverride { pivotIndex = lastSpotHovered.spotIndex, offsetY = 0f });
                                am.GenerateNavOverrides ();
                            }
                        }
                    }
                }
                GUILayout.EndVertical ();

                // UtilityCustomInspector.DrawField ("Override", () => swapTilesetOnVolumeEdits = EditorGUILayout.Toggle (swapTilesetOnVolumeEdits));
                GUILayout.EndVertical ();

                DrawHotkeyHintsPanel("[LMB] - Add    [RMB] - Remove     [MW▲▼] - Adjust height");
            }
            else if (editingTarget == EditingTarget.Roads)
            {
                float lw = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 80f;
                GUILayout.BeginVertical (EditorStyles.helpBox, GUILayout.Width (200f));
                GUILayout.Label ("Road tool", EditorStyles.boldLabel);
                GUILayout.Space (8f);
                DrawBrushSelector ();
                GUILayout.Space (8f);
                AreaManager.roadSubtype = (AreaManager.RoadSubtype)EditorGUILayout.EnumPopup ("Type", AreaManager.roadSubtype);
                EditorGUILayout.BeginHorizontal ();
                if (GUILayout.Button ("G+D", EditorStyles.miniButtonLeft))
                    AreaManager.roadSubtype = AreaManager.RoadSubtype.GrassDirt;
                if (GUILayout.Button ("G+C", EditorStyles.miniButtonMid))
                    AreaManager.roadSubtype = AreaManager.RoadSubtype.GrassCurb;
                if (GUILayout.Button ("C+C", EditorStyles.miniButtonMid))
                    AreaManager.roadSubtype = AreaManager.RoadSubtype.ConcreteCurb;
                if (GUILayout.Button ("T+C", EditorStyles.miniButtonRight))
                    AreaManager.roadSubtype = AreaManager.RoadSubtype.TileCurb;

                EditorGUILayout.EndHorizontal ();
                GUILayout.Space (8f);
                GUILayout.EndVertical ();
                EditorGUIUtility.labelWidth = lw;

                DrawHotkeyHintsPanel("[LMB] - Add     [RMB] - Remove     [MW▲▼] - Swap type    [MMB] - Flood-fill road type");
            }
            else if (editingTarget == EditingTarget.Props)
            {
                GUILayout.BeginVertical (EditorStyles.helpBox, GUILayout.Width (300f));
                GUILayout.Label ("Prop tool", EditorStyles.boldLabel);
                GUILayout.Space (3f);

                GUILayout.BeginHorizontal ("Box");
                {
                    GUILayout.BeginVertical ();
                    {
                        GUILayout.Label ("Spot index: " + propPlacementListIndex + "  Spot position: " + AreaUtility.GetVolumePositionFromIndex (propPlacementListIndex, am.boundsFull), EditorStyles.miniLabel);
                        GUILayout.Space(2f);
                        propEditingMode = (PropEditingMode)EditorGUILayout.EnumPopup ("Mode", propEditingMode);
                        checkPropConfiguration = EditorGUILayout.Toggle ("Check spots", checkPropConfiguration);
                        spawnPropsWithAutorotation = EditorGUILayout.Toggle ("Autorotate on placement", spawnPropsWithAutorotation);
                        spawnPropsWithClipboardColor = EditorGUILayout.Toggle ("Use clipboard color", spawnPropsWithClipboardColor);
                    }
                    GUILayout.EndVertical ();

                    GUILayout.BeginHorizontal ();
                    GUILayout.Space(35f);
                    GUILayout.EndHorizontal ();
                }
                GUILayout.EndHorizontal ();

                DrawHotkeyHintsPanel
                    (
                    "[LMB] - Add prop to cell     [RMB] - Select cell     [RMB + Shift] - Copy prop     [MMB] - Delete prop     [MW▲▼] - Change prop",
                    "[Shift + Z] - Copy selected prop HSV     [Shift + X] - Paste selected prop HSV     [Shift + F] - Rotate selected prop     [Shift + G] - Flip selected prop"
                    );

				{
					var selectedPrototype = AreaAssetHelper.GetPropPrototype (propSelectionID);
                    GUILayout.Space (5f);
                    GUILayout.BeginVertical ("Box");
                    {
                        GUILayout.Label ("New Prop Spawn Setup", EditorStyles.boldLabel);
                        GUILayout.Space(3f);
                        GUILayout.BeginHorizontal ();
                        {
                            GUILayout.BeginVertical ("Box");
                            {
                                GUILayout.Label (propSelectionID + " - " + (AreaAssetHelper.GetPropPrototype (propSelectionID) != null ? AreaAssetHelper.GetPropPrototype (propSelectionID).name : "null"), EditorStyles.boldLabel);

                                GUILayout.BeginHorizontal ("Box");
                                {
                                    GUILayout.Label ("Orientation: " + propRotation + (propFlipped ? "-" : "+"), EditorStyles.miniLabel);
                                    GUILayout.FlexibleSpace ();
                                    if (GUILayout.Button ("←", GUILayout.Width (31f)))
                                    {
                                        propRotation = propRotation.OffsetAndWrap (false, 3);
                                    }
                                    if (GUILayout.Button ("↔", GUILayout.Width (31f)))
                                    {
                                        propFlipped = !propFlipped;
                                    }
                                    if (GUILayout.Button ("→", GUILayout.Width (31f)))
                                    {
                                        propRotation = propRotation.OffsetAndWrap (true, 3);
                                    }
                                    if (GUILayout.Button ("reset", EditorStyles.miniButton, GUILayout.MinWidth (45f)))
                                    {
                                        propRotation = 0;
                                        propFlipped = false;
                                    }
                                }
                                GUILayout.EndHorizontal ();

                                if (selectedPrototype != null && selectedPrototype.prefab.allowPositionOffset)
                                {
                                    GUILayout.BeginHorizontal ("Box");
                                    {
                                        float lw = EditorGUIUtility.labelWidth;
                                        float fw = EditorGUIUtility.fieldWidth;

                                        EditorGUIUtility.labelWidth = 40f;
                                        EditorGUIUtility.fieldWidth = 30f;

                                        EditorGUI.BeginChangeCheck ();

                                        EditorGUILayout.BeginVertical ();
                                        {
                                            GUILayout.Label ("Position offsets", EditorStyles.miniLabel);
                                            GUILayout.BeginHorizontal ();
                                            {
                                                propOffsetX = EditorGUILayout.FloatField ("X ↔", propOffsetX, GUILayout.MinWidth (80f));
                                                GUILayout.Space(5f);
                                                propOffsetZ = EditorGUILayout.FloatField ("Z ↔", propOffsetZ, GUILayout.MinWidth (80f));

                                                GUILayout.Space(30f);
                                                GUILayout.FlexibleSpace ();

                                                EditorGUILayout.BeginHorizontal ();
                                                {
                                                    if (GUILayout.Button ("copy", EditorStyles.miniButton, GUILayout.MinWidth (45f)))
                                                    {
                                                        propOffsetXClipboard = propOffsetX;
                                                        propOffsetZClipboard = propOffsetZ;
                                                    }
                                                    if (GUILayout.Button ("paste", EditorStyles.miniButton, GUILayout.MinWidth (45f)))
                                                    {
                                                        propOffsetX = propOffsetXClipboard;
                                                        propOffsetZ = propOffsetZClipboard;
                                                    }
                                                    if (GUILayout.Button ("reset", EditorStyles.miniButton, GUILayout.MinWidth (45f)))
                                                    {
                                                        propOffsetX = 0f;
                                                        propOffsetZ = 0f;
                                                    }
                                                }
                                                EditorGUILayout.EndHorizontal ();
                                            }
                                            EditorGUILayout.EndHorizontal ();
                                        }
                                        EditorGUILayout.EndVertical ();
                                    }
                                    EditorGUILayout.EndHorizontal ();
                                }

                                if (selectedPrototype != null && selectedPrototype.prefab.allowTinting)
                                {
                                    EditorGUILayout.BeginHorizontal ("Box");
                                    {
                                        EditorGUILayout.BeginVertical ();
                                        {
                                            GUILayout.Label ("HSV offsets", EditorStyles.miniLabel);
                                            EditorGUILayout.BeginHorizontal ();
                                            {
                                                propHSBPrimary.x = Mathf.Clamp01 (EditorGUILayout.FloatField ("H ↔", propHSBPrimary.x));
                                                propHSBPrimary.y = Mathf.Clamp01 (EditorGUILayout.FloatField ("S ↔", propHSBPrimary.y));
                                                propHSBPrimary.z = Mathf.Clamp01 (EditorGUILayout.FloatField ("V ↔", propHSBPrimary.z));
                                                var a = propHSBPrimary.w;

                                                EditorGUI.BeginChangeCheck ();
                                                var colPrimary = EditorGUILayout.ColorField (new HSBColor(propHSBPrimary.x, propHSBPrimary.y, propHSBPrimary.z, propHSBPrimary.w).ToColor());
                                                if (EditorGUI.EndChangeCheck ())
                                                {
                                                    var colPrimaryHSB = new HSBColor(colPrimary);
                                                    propHSBPrimary.x = colPrimaryHSB.h;
                                                    propHSBPrimary.y = colPrimaryHSB.s;
                                                    propHSBPrimary.z = colPrimaryHSB.b;
                                                    propHSBPrimary.w = a;
                                                }
                                            }
                                            EditorGUILayout.EndHorizontal ();

                                            EditorGUILayout.BeginHorizontal ();
                                            {
                                                propHSBSecondary.x = Mathf.Clamp01 (EditorGUILayout.FloatField ("H ↔", propHSBSecondary.x));
                                                propHSBSecondary.y = Mathf.Clamp01 (EditorGUILayout.FloatField ("S ↔", propHSBSecondary.y));
                                                propHSBSecondary.z = Mathf.Clamp01 (EditorGUILayout.FloatField ("V ↔", propHSBSecondary.z));
                                                var a = propHSBSecondary.w;

                                                EditorGUI.BeginChangeCheck ();
                                                var colSecondary = EditorGUILayout.ColorField(new HSBColor(propHSBSecondary.x, propHSBSecondary.y, propHSBSecondary.z, propHSBSecondary.w).ToColor());
                                                if (EditorGUI.EndChangeCheck ())
                                                {
                                                    var colSecondaryHSB = new HSBColor(colSecondary);
                                                    propHSBSecondary.x = colSecondaryHSB.h;
                                                    propHSBSecondary.y = colSecondaryHSB.s;
                                                    propHSBSecondary.z = colSecondaryHSB.b;
                                                    propHSBSecondary.w = a;
                                                }
                                            }
                                            EditorGUILayout.EndHorizontal ();
                                        }
                                        EditorGUILayout.EndVertical ();

                                        GUILayout.FlexibleSpace ();
                                        EditorGUILayout.BeginVertical ();
                                        {
                                            if (GUILayout.Button ("copy", EditorStyles.miniButton, GUILayout.Width (50f)))
                                            {
                                                clipboardPropHSBPrimary = propHSBPrimary;
                                                clipboardPropHSBSecondary = propHSBSecondary;
                                            }
                                            if (GUILayout.Button ("paste", EditorStyles.miniButton, GUILayout.Width (50f)))
                                            {
                                                propHSBPrimary = clipboardPropHSBPrimary;
                                                propHSBSecondary = clipboardPropHSBSecondary;
                                            }
                                            if (GUILayout.Button ("reset", EditorStyles.miniButton, GUILayout.Width (50f)))
                                            {
                                                propHSBPrimary = Constants.defaultHSBOffset;
                                                propHSBSecondary = Constants.defaultHSBOffset;
                                            }
                                        }
                                        EditorGUILayout.EndVertical ();
                                    }
                                    EditorGUILayout.EndHorizontal ();
                                }
                            }
                            GUILayout.EndVertical ();

                            GUILayout.BeginHorizontal ();
                            GUILayout.Space(35f);
                            GUILayout.EndHorizontal ();
                        }
                        GUILayout.EndHorizontal ();
                    }
                    EditorGUILayout.EndVertical ();
                }

                GUILayout.BeginVertical ("Box");
                GUILayout.Space (5f);
                GUILayout.Label ("Selected Props", EditorStyles.boldLabel);
                GUILayout.Space(3f);
                if (propPlacementListIndex == -1 || !am.indexesOccupiedByProps.ContainsKey (propPlacementListIndex))
                {
                    EditorGUILayout.HelpBox ("No prop list selected", MessageType.Info);
                }
                else
                {
                    int indexToRemove = -1;
                    GUILayout.BeginVertical ();

                    List<AreaPlacementProp> placements = am.indexesOccupiedByProps[propPlacementListIndex];
                    for (int i = 0; i < placements.Count; ++i)
                    {
                        AreaPlacementProp placement = placements[i];

                        Color colorPrevious = GUI.backgroundColor;
                        if (propPlacementHandled == placement)
                        {
                            // Set blue background color for selected prop's UI elements
                            GUI.backgroundColor = Color.Lerp (GUI.backgroundColor, Color.cyan, 0.35f);
                            GUILayout.BeginVertical ("Box");
                        }
                        else
                            GUILayout.BeginVertical ("Box");

                        var prototype = AreaAssetHelper.propsPrototypes[placement.id];
                        // Prop ID and name
                        var headerText = $"{placement.id} - {prototype.name}";
                        if (prototype.prefab == null)
                            headerText += " !NP";

                        GUILayout.Label (headerText, EditorStyles.boldLabel);
                        GUILayout.Space(5f);
                        GUILayout.BeginHorizontal ();

                        GUILayout.BeginVertical ();

                        GUILayout.BeginHorizontal ("Box");
                        GUILayout.Label ("Orientation: " + placement.rotation + (placement.flipped ? "-" : "+"), EditorStyles.miniLabel);
                        GUILayout.FlexibleSpace ();

                        EditorGUI.BeginChangeCheck ();

                        if (GUILayout.Button (new GUIContent("S", "Snap rotation to tile"), GUILayout.Width (31f)))
                        {
                            am.SnapPropRotation (placement);
                        }

                        bool commandPropRotLeft = GUILayout.Button ("←", GUILayout.Width (31f));
                        if (placement == propPlacementHandled && !e.alt && e.shift && !e.control && e.type == EventType.KeyUp && e.keyCode == KeyCode.F) commandPropRotLeft = true;
                        if (commandPropRotLeft)
                        {
                            placement.rotation = placement.rotation.OffsetAndWrap (false, 3);
                        }

                        bool commandPropFlip = GUILayout.Button ("↔", GUILayout.Width (31f));
                        if (placement == propPlacementHandled && !e.alt && e.shift && !e.control && e.type == EventType.KeyUp && e.keyCode == KeyCode.G) commandPropFlip = true;
                        if (commandPropFlip)
                        {
                            placement.flipped = !placement.flipped;
                        }

                        if (GUILayout.Button ("→", GUILayout.Width (31f)))
                        {
                            placement.rotation = placement.rotation.OffsetAndWrap (true, 3);
                        }

                        if (GUILayout.Button ("reset", EditorStyles.miniButton, GUILayout.MinWidth (45f)))
                        {
                            placement.rotation = 0;
                            placement.flipped = false;
                        }
                        GUILayout.EndHorizontal ();

                        if (EditorGUI.EndChangeCheck ())
                        {
                            am.ExecutePropPlacement (placement);
                            e.Use ();
                        }

                        if (placement.state != null && placement.prototype != null && placement.prototype.prefab.allowPositionOffset)
                        {
                            GUILayout.BeginHorizontal ("Box");

                            float lw = EditorGUIUtility.labelWidth;
                            float fw = EditorGUIUtility.fieldWidth;

                            EditorGUIUtility.labelWidth = 40f;
                            EditorGUIUtility.fieldWidth = 30f;

                            EditorGUI.BeginChangeCheck ();

                            EditorGUILayout.BeginVertical ();
                            GUILayout.Label ("Position offsets", EditorStyles.miniLabel);
                            GUILayout.BeginHorizontal ();
                            placement.offsetX = EditorGUILayout.FloatField ("X ↔", placement.offsetX, GUILayout.MinWidth (80f));
                            GUILayout.Space(5f);
                            placement.offsetZ = EditorGUILayout.FloatField ("Z ↔", placement.offsetZ, GUILayout.MinWidth (80f));

                            GUILayout.Space(30f);
                            GUILayout.FlexibleSpace ();

                            EditorGUILayout.BeginHorizontal ();
                            if (GUILayout.Button ("copy", EditorStyles.miniButton, GUILayout.MinWidth (45f)))
                            {
                                propOffsetXClipboard = placement.offsetX;
                                propOffsetZClipboard = placement.offsetZ;
                            }
                            if (GUILayout.Button ("paste", EditorStyles.miniButton, GUILayout.MinWidth (45f)))
                            {
                                placement.offsetX = propOffsetXClipboard;
                                placement.offsetZ = propOffsetZClipboard;
                                am.ExecutePropPlacement (placement);
                            }
                            if (GUILayout.Button ("reset", EditorStyles.miniButton, GUILayout.MinWidth (45f)))
                            {
                                placement.offsetX = 0f;
                                placement.offsetZ = 0f;
                                am.ExecutePropPlacement (placement);
                            }
                            EditorGUILayout.EndHorizontal ();
                            EditorGUILayout.EndHorizontal ();
                            EditorGUILayout.EndVertical ();

                            //EditorGUIUtility.labelWidth = lw;
                            //EditorGUIUtility.fieldWidth = fw;

                            if (EditorGUI.EndChangeCheck ())
                            {
                                placement.UpdateOffsets (am);
                                UtilityECS.ScheduleUpdate ();
                                e.Use ();
                            }

                            /*
                            if (GUI.changed)
                            {
                                Debug.LogWarning ("More than one update of entity positions might have unintended consequences");
                                placement.UpdateTransformations (am);
                            }
                            GUI.changed = false;
                            */

                            EditorGUILayout.EndHorizontal ();
                        }

                        if (placement.prototype != null && placement.prototype.prefab.allowTinting)
                        {
                            EditorGUILayout.BeginHorizontal ("Box");
                            {
                                float lw = EditorGUIUtility.labelWidth;
                                float fw = EditorGUIUtility.fieldWidth;

                                EditorGUIUtility.labelWidth = 40f;
                                EditorGUIUtility.fieldWidth = 30f;

                                EditorGUILayout.BeginVertical ();
                                {
                                    GUILayout.Label ("HSV offsets", EditorStyles.miniLabel);
                                    EditorGUILayout.BeginHorizontal ();
                                    {
                                        placement.hsbPrimary.x = Mathf.Clamp01 (EditorGUILayout.FloatField ("H ↔", placement.hsbPrimary.x));
                                        placement.hsbPrimary.y = Mathf.Clamp01 (EditorGUILayout.FloatField ("S ↔", placement.hsbPrimary.y));
                                        placement.hsbPrimary.z = Mathf.Clamp01 (EditorGUILayout.FloatField ("V ↔", placement.hsbPrimary.z));
                                        float a = placement.hsbPrimary.w;

                                        EditorGUI.BeginChangeCheck ();
                                        var colPrimary = EditorGUILayout.ColorField(new HSBColor(placement.hsbPrimary.x, placement.hsbPrimary.y, placement.hsbPrimary.z, placement.hsbPrimary.w).ToColor());
                                        if (EditorGUI.EndChangeCheck ())
                                        {
                                            var colPrimaryHSB = new HSBColor(colPrimary);
                                            placement.hsbPrimary.x = colPrimaryHSB.h;
                                            placement.hsbPrimary.y = colPrimaryHSB.s;
                                            placement.hsbPrimary.z = colPrimaryHSB.b;
                                            placement.hsbPrimary.w = a;
                                        }
                                    }
                                    EditorGUILayout.EndHorizontal ();

                                    EditorGUILayout.BeginHorizontal ();
                                    {
                                        placement.hsbSecondary.x = Mathf.Clamp01 (EditorGUILayout.FloatField ("H ↔", placement.hsbSecondary.x));
                                        placement.hsbSecondary.y = Mathf.Clamp01 (EditorGUILayout.FloatField ("S ↔", placement.hsbSecondary.y));
                                        placement.hsbSecondary.z = Mathf.Clamp01 (EditorGUILayout.FloatField ("V ↔", placement.hsbSecondary.z));
                                        float a = placement.hsbSecondary.w;

                                        EditorGUI.BeginChangeCheck ();
                                        var colSecondary = EditorGUILayout.ColorField(new HSBColor(placement.hsbSecondary.x, placement.hsbSecondary.y, placement.hsbSecondary.z, placement.hsbSecondary.w).ToColor());
                                        if (EditorGUI.EndChangeCheck ())
                                        {
                                            var colSecondaryHSB = new HSBColor(colSecondary);
                                            placement.hsbSecondary.x = colSecondaryHSB.h;
                                            placement.hsbSecondary.y = colSecondaryHSB.s;
                                            placement.hsbSecondary.z = colSecondaryHSB.b;
                                            placement.hsbSecondary.w = a;
                                        }
                                    }
                                    EditorGUILayout.EndHorizontal ();
                                }
                                EditorGUILayout.EndVertical ();

                                EditorGUIUtility.labelWidth = lw;
                                EditorGUIUtility.fieldWidth = fw;

                                if (GUI.changed)
                                    placement.UpdateMaterial_HSBOffsets (placement.hsbPrimary, placement.hsbSecondary);
                                GUI.changed = false;

                                GUILayout.FlexibleSpace ();
                                EditorGUILayout.BeginVertical ();
                                {
                                    bool commandPropHSVCopy = GUILayout.Button ("copy", EditorStyles.miniButton, GUILayout.Width (50f));
                                    if (placement == propPlacementHandled && !e.alt && e.shift && !e.control && e.type == EventType.KeyUp && e.keyCode == KeyCode.Z) commandPropHSVCopy = true;
                                    if (commandPropHSVCopy)
                                    {
                                        clipboardPropHSBPrimary = placement.hsbPrimary;
                                        clipboardPropHSBSecondary = placement.hsbSecondary;
                                        Debug.Log ("AM | Prop HSV copied");
                                        e.Use ();
                                    }
                                    bool commandPropHSVPaste = GUILayout.Button ("paste", EditorStyles.miniButton, GUILayout.Width (50f));
                                    if (placement == propPlacementHandled && !e.alt && e.shift && !e.control && e.type == EventType.KeyUp && e.keyCode == KeyCode.X) commandPropHSVPaste = true;
                                    if (commandPropHSVPaste)
                                    {
                                        placement.hsbPrimary = clipboardPropHSBPrimary;
                                        placement.hsbSecondary = clipboardPropHSBSecondary;
                                        placement.UpdateMaterial_HSBOffsets (placement.hsbPrimary, placement.hsbSecondary);
                                        Debug.Log ("AM | Prop HSV pasted");
                                        e.Use ();
                                    }
                                    if (GUILayout.Button ("reset", EditorStyles.miniButton, GUILayout.Width (50f)))
                                    {
                                        placement.hsbPrimary = Constants.defaultHSBOffset;
                                        placement.hsbSecondary = Constants.defaultHSBOffset;
                                        placement.UpdateMaterial_HSBOffsets (placement.hsbPrimary, placement.hsbSecondary);
                                    }
                                }
                                EditorGUILayout.EndVertical ();
                            }
                            EditorGUILayout.EndHorizontal ();
                        }

                        // Commented this out to remove list of subojbects from the UI and save on vertical space. Still useful for debug purposes.
                        /*if (placement.prototype != null)
                        {
                            for (int s = 0; s < placement.prototype.subObjects.Count; ++s)
                            {
                                var subObject = placement.prototype.subObjects[s];
                                var propRenderer = placement.prototype.prefab.renderers[subObject.contextIndex];
                                GUILayout.Label ($"{s}: {propRenderer?.renderer?.name}", EditorStyles.miniLabel);
                            }
                        }*/
                        GUILayout.EndVertical ();

                        GUILayout.BeginVertical ();
                        {
                            // UI - Select a prop
                            if (placement.prototype != null && placement.prototype.prefab.allowPositionOffset)
                            {
                                if (GUILayout.Button ("┌ ┐\n└ ┘", GUILayout.Width (35f), GUILayout.Height (35f)))
                                    propPlacementHandled = placement;
                            }
                            // UI - Delete a prop
                            if (GUILayout.Button ("×", GUILayout.Width (35f), GUILayout.Height (35f)))
                                indexToRemove = i;
                        }
                        GUILayout.EndVertical ();

                        EditorGUILayout.EndHorizontal ();
                        EditorGUILayout.EndVertical ();

                        // Reset background color for UI elements that don't belong to selected prop
                        GUI.backgroundColor = colorPrevious;

                        GUILayout.Space(10f);
                    }
                    GUILayout.EndVertical ();

                    if (indexToRemove != -1)
                    {
                        AreaPlacementProp placement = placements[indexToRemove];
                        am.RemovePropPlacement (placement);
                        indexToRemove = -1;
                    }
                }
                GUILayout.EndVertical ();
                GUILayout.EndVertical ();
            }
			else if (editingTarget == EditingTarget.TerrainRamp)
            {
	            GUILayout.BeginVertical (EditorStyles.helpBox, GUILayout.Width (200f));
	            GUILayout.Label ("Terrain Ramp mode", EditorStyles.boldLabel);
	            GUILayout.EndVertical ();

                DrawHotkeyHintsPanel
                (
                    "Click on the top point of a ramp",
                    "[LMB] - Rampify     [RMB] - Un-rampify     [Shift] - Strict eligibility check (block corner ramps)"
                );
            }
            else if (editingTarget == EditingTarget.RoadCurves)
            {
	            GUILayout.BeginVertical (EditorStyles.helpBox, GUILayout.Width (200f));
	            GUILayout.Label ("Road Curve mode", EditorStyles.boldLabel);
	            GUILayout.EndVertical ();

                DrawHotkeyHintsPanel
                (
                    "Click on a turn to smooth it",
                    "[LMB] - Smooth     [RMB] - Angled"
                );
            }


            if (e.alt)
            {
                KeyCode buttonPressed = KeyCode.None;
                if (e.type == EventType.KeyUp)
                {
                    buttonPressed = e.keyCode;
                    if (e.keyCode == KeyCode.LeftBracket || e.keyCode == KeyCode.RightBracket)
                    {
                        bool forward = e.keyCode == KeyCode.RightBracket;
                        // Hotkeys to rotate and flip a prop in preview mode (before placement)
                        if (editingTarget == EditingTarget.Props)
                        {
                            if (forward)
                                propRotation = propRotation.OffsetAndWrap (true, 3);
                            else
                                propFlipped = !propFlipped;
                        }
                    }
                    else
                    {
                        if (editingTarget == EditingTarget.Transfer)
                        {
                            if (buttonPressed == KeyCode.X)
                                am.CopyVolume (am.clipboardOrigin, am.clipboardOrigin + am.clipboardBoundsRequested + Vector3Int.size1x1x1Neg);
                            else if (buttonPressed == KeyCode.V)
                                am.PasteVolume (am.targetOrigin, am.brushApplicationMode);
                        }
                    }
                }

                else if (e.type == EventType.ScrollWheel)
                {
                    bool forward = e.delta.y > 0f;
                    buttonPressed = shift ? (forward ? KeyCode.LeftBracket : KeyCode.RightBracket) : (forward ? KeyCode.PageDown : KeyCode.PageUp);

                    if (editingTarget == EditingTarget.Props)
                    {
                        if (buttonPressed == KeyCode.PageDown || buttonPressed == KeyCode.PageUp)
                        {
                            AreaAssetHelper.CheckResources ();
                            propIndex = propIndex.OffsetAndWrap (buttonPressed == KeyCode.PageDown, 0, AreaAssetHelper.propsPrototypesList.Count - 1);
                            propSelectionID = AreaAssetHelper.propsPrototypesList[propIndex].id;
                        }
                        else if (buttonPressed == KeyCode.LeftBracket)
                            propFlipped = !propFlipped;
                        else if (buttonPressed == KeyCode.RightBracket)
                            propRotation = propRotation.OffsetAndWrap (true, 3);
                    }
                    else if (editingTarget == EditingTarget.Tileset)
                    {
                        int tilesetKeyNew = AreaTilesetHelper.OffsetBlockTileset (editingTilesetSelected.id, forward);
                        if (tilesetKeyNew != editingTilesetSelected.id)
                        {
                            editingTilesetSelected = AreaTilesetHelper.database.tilesets[tilesetKeyNew];
                        }
                    }
                    else if (editingTarget == EditingTarget.Volume)
                    {
                        if (shift)
                        {
                            int tilesetKeyNew = AreaTilesetHelper.OffsetBlockTileset (editingTilesetSelected.id, forward);
                            if (tilesetKeyNew != editingTilesetSelected.id)
                            {
                                editingTilesetSelected = AreaTilesetHelper.database.tilesets[tilesetKeyNew];
                            }
                        }
                        else
                        {
                            volumeBrushDepth += forward ? 1 : -1;
                            volumeBrushDepth = Mathf.Clamp (volumeBrushDepth, 1, boundsFullCached.y);
                        }
                    }
                }

                else if (e.type == EventType.MouseDown && buttonPressed == KeyCode.None)
                {
                    if (e.button == 0)
                        buttonPressed = KeyCode.Mouse0;
                    if (e.button == 1)
                        buttonPressed = KeyCode.Mouse1;
                    if (e.button == 2)
                        buttonPressed = KeyCode.Mouse2;
                }

                Ray worldRay = HandleUtility.GUIPointToWorldRay (e.mousePosition);
                RaycastHit hitInfo;

                if (Physics.Raycast (worldRay, out hitInfo, interactionDistance - 100, environmentLayerMask))
                {
                    // Normally, we'd want the hit to be shifted inward to move it closer to spot center, but not for additive operations
                    bool shiftOut = false;
					if (editingTarget == EditingTarget.Volume)
					{
						if (shift)
						{
							if (buttonPressed == KeyCode.Mouse0 || buttonPressed == KeyCode.Mouse1)
								shiftOut = true;
						}
						else
						{
							if (buttonPressed == KeyCode.Mouse0)
								shiftOut = true;
						}
					}
					else if (buttonPressed == KeyCode.Mouse0 && (editingTarget == EditingTarget.Damage || editingTarget == EditingTarget.Transfer))
						shiftOut = true;

					Vector3 hitPositionShifted = shiftOut ? hitInfo.point + hitInfo.normal * 0.5f : hitInfo.point - hitInfo.normal * 0.5f;

                    OnPositionHover (hitInfo.point, hitPositionShifted, hitInfo.normal);

                    bool eventPresent = (e.type == EventType.MouseDown || e.type == EventType.KeyUp || e.type == EventType.ScrollWheel) && buttonPressed != KeyCode.None;
                    if (eventPresent)
                        e.Use ();

                    if (eventPresent || shift && editingTarget != EditingTarget.Props)
                    {
                        OnPositionChanged (hitPositionShifted, buttonPressed, shift, ctrl);
                        // EditorUtility.SetDirty (am);
                        // if (!Application.isPlaying)
                        //     UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty ();
                    }

                    if (editingTarget == EditingTarget.Props && propEditingMode == PropEditingMode.Place)
                    {
                        VisualizeProp (hitPositionShifted);
                    }
                    else if (editingTarget == EditingTarget.Transfer)
                    {
                        VisualizeTransferPreview ();
                    }
                }
                else
                {
                    OnHoverEnd (am);

                    if (buttonPressed != KeyCode.None)
                        e.Use ();
                }
            }
            else
            {
                OnHoverEnd (am);
            }

            if (GUI.changed)
            {
                EditorWindow view = EditorWindow.GetWindow<SceneView>();
                view.Repaint();
                // SceneView.RepaintAll ();
            }
        }

        private const float handleSize = 0.1f;
        private readonly static Vector3 handleSizeCube = new Vector3 (0.25f, 0.25f, 0.25f);
        private const float pickSize = 0.4f;

        private static AreaVolumePoint currentPointHovered;
        private static AreaVolumePoint lastPointHovered;
        private static AreaVolumePoint currentSpotHovered;
        private static AreaVolumePoint lastSpotHovered;
        private static int lastSpotTileset;
        private static int lastSpotGroup;
        private static int lastSpotSubtype;

        private static GUIStyle lastSpotInfoStyle = null;
        private static string lastSpotInfoGroups = string.Empty;
        private static string lastSpotInfoSubtypes = string.Empty;
        private static StringBuilder lastSpotInfoBuilder = new StringBuilder ();



        public void OnPositionHover (Vector3 hitPosition, Vector3 hitPositionShifted, Vector3 hitDirection)
        {
            if (am == null)
            {
                Debug.LogError ("No area manager instance!");
                return;
            }

            bool volumeTarget =
                editingTarget == EditingTarget.Volume ||
                editingTarget == EditingTarget.Damage ||
                editingTarget == EditingTarget.Roads ||
                editingTarget == EditingTarget.Transfer ||
	            editingTarget == EditingTarget.TerrainRamp;

            if (rcCursorMaterial == null)
                rcCursorMaterial = Resources.Load<Material> ("Content/Debug/AreaCursor");

            if (rcSelectionMaterial == null)
                rcSelectionMaterial = Resources.Load<Material> ("Content/Debug/AreaSelection");

            if (rcGlowMaterial == null)
                rcGlowMaterial = Resources.Load<Material> ("Content/Debug/AreaGlow");

            if (rcMpb == null)
                rcMpb = new MaterialPropertyBlock ();

            if (rcCursorObject == null)
            {
                var rcCursorTransform = am.transform.Find ("selection_pointer");
                if (rcCursorTransform != null)
                    rcCursorObject = rcCursorTransform.gameObject;
                else
                {
                    var prefab = Resources.Load<GameObject> ("Content/Debug/selection_pointer");
                    if (prefab != null)
                    {
                        rcCursorObject = Instantiate (prefab);
                        rcCursorObject.transform.parent = am.transform;
                        rcCursorObject.name = prefab.name;
                        // rcCursorObject.hideFlags = HideFlags.HideInHierarchy;
                        MeshRenderer mr = rcCursorObject.GetComponentInChildren<MeshRenderer> ();
                        if (mr != null && rcCursorMaterial != null)
                            mr.sharedMaterial = rcCursorMaterial;

                        Light light = rcCursorObject.AddChild<Light> ();
                        light.gameObject.hideFlags = HideFlags.HideInHierarchy;
                        light.transform.localPosition = new Vector3 (0f, 0f, 1f);
                        light.color = Color.Lerp (Color.cyan, Color.blue, 0.25f);
                        light.intensity = 1f;
                        light.shadows = LightShadows.Soft;
                    }
                }
            }

            if (rcSelectionObject == null)
            {
                var rcSelectionTransform = am.transform.Find ("selection_cell");
                if (rcSelectionTransform != null)
                    rcSelectionObject = rcSelectionTransform.gameObject;
                else
                {
                    var prefab = Resources.Load<GameObject> ("Content/Debug/selection_cell");
                    if (prefab != null)
                    {
                        rcSelectionObject = Instantiate (prefab);
                        rcSelectionObject.transform.localScale = Vector3.one * 1.01f;
                        rcSelectionObject.transform.parent = am.transform;
                        rcSelectionObject.name = prefab.name;
                        // rcSelectionObject.hideFlags = HideFlags.HideInHierarchy;
                        MeshRenderer mr = rcSelectionObject.GetComponentInChildren<MeshRenderer> ();
                        if (mr != null && rcSelectionMaterial != null)
                            mr.sharedMaterial = rcSelectionMaterial;
                    }
                }
            }

            if (rcGlowObject == null)
            {
                var rcGlowTransform = am.transform.Find ("selection_glow");
                if (rcGlowTransform != null)
                    rcGlowObject = rcGlowTransform.gameObject;
                else
                {
                    rcGlowObject = PrimitiveHelper.CreatePrimitive (PrimitiveType.Quad, false);
                    rcGlowObject.transform.localScale = Vector3.one * 6f;
                    rcGlowObject.transform.parent = am.transform;
                    rcGlowObject.name = "selection_glow";
                    // rcGlowObject.hideFlags = HideFlags.HideInHierarchy;
                    MeshRenderer mr = rcGlowObject.GetComponent<MeshRenderer> ();
                    if (mr != null && rcGlowMaterial != null)
                        mr.sharedMaterial = rcGlowMaterial;

                    /*
                    Light light = rcGlowObject.AddComponent<Light> ();
                    light.color = rcGlowMaterial.color;
                    light.intensity = 1f;
                    */
                }
            }

            if (rcCursorObject != null)
            {
                if (!rcCursorObject.activeSelf)
                    rcCursorObject.SetActive (true);

                rcCursorTargetPosition = hitPosition;
                rcCursorTargetRotation = Quaternion.LookRotation (hitDirection);
            }

            if (rcSelectionObject != null && volumeTarget)
            {
                if (!rcSelectionObject.activeSelf)
                    rcSelectionObject.SetActive (true);

                Vector3 hitPositionShiftedDeeper = hitPosition - hitDirection * 0.5f;
                int index = AreaUtility.GetIndexFromWorldPosition (hitPositionShiftedDeeper, am.GetHolderColliders ().position, am.boundsFull);
                if (index != -1 && index.IsValidIndex (am.points))
                {
                    var point = am.points[index];
                    currentPointHovered = lastPointHovered = point;
                    rcSelectionTargetPosition = point.pointPositionLocal;
                }
            }

            if (rcGlowObject != null && volumeTarget)
            {
                if (!rcGlowObject.activeSelf)
                    rcGlowObject.SetActive (true);

                Vector3 hitPositionShiftedOutside = hitPosition + hitDirection * 0.5f;
                int index = AreaUtility.GetIndexFromWorldPosition (hitPositionShiftedOutside, am.GetHolderColliders ().position, am.boundsFull);
                if (index != -1 && index.IsValidIndex (am.points))
                {
                    var point = am.points[index];
                    rcGlowTargetPosition = point.pointPositionLocal;
                    // DrawZTestWireCube (point.pointPositionLocal, Color.white.WithAlpha (1f), Color.cyan.WithAlpha (0.5f));
                }
            }

            // Updating spot info



            int indexForSpot = AreaUtility.GetIndexFromWorldPosition (hitPositionShifted + spotRaycastHitOffset, Vector3.zero, am.boundsFull);
            if (indexForSpot < 0 || indexForSpot > am.points.Count - 1)
                return;

            var spotHovered = am.points[indexForSpot];
            if (spotHovered.spotConfiguration == AreaNavUtility.configEmpty || spotHovered.spotConfiguration == AreaNavUtility.configFull)
                return;

            // if (!volumeTarget)
            //     DrawCubeVolume (spotHovered.instancePosition, Color.white.WithAlpha (1f), Color.cyan.WithAlpha (0.5f));

            if
            (
                lastSpotHovered == spotHovered &&
                lastSpotTileset == spotHovered.blockTileset &&
                lastSpotGroup == spotHovered.blockGroup &&
                lastSpotSubtype == spotHovered.blockSubtype
            )
                return;

            lastSpotHovered = spotHovered;
            lastSpotTileset = spotHovered.blockTileset;
            lastSpotGroup = spotHovered.blockGroup;
            lastSpotSubtype = spotHovered.blockSubtype;

            AreaTileset tileset = AreaTilesetHelper.database.tilesets.ContainsKey (lastSpotHovered.blockTileset) ? AreaTilesetHelper.database.tilesets[lastSpotHovered.blockTileset] : null;
            if (tileset == null)
                return;

            AreaBlockDefinition definition = tileset.blocks[lastSpotHovered.spotConfiguration];
            if (definition == null)
                return;

            bool identifiersPresent = tileset.groupIdentifiers != null;

            if (lastSpotInfoBuilder == null)
                lastSpotInfoBuilder = new StringBuilder ();
            else
                lastSpotInfoBuilder.Clear ();

            lastSpotInfoBuilder.Append (lastSpotHovered.spotIndex);
            lastSpotInfoBuilder.Append ("\n");
            lastSpotInfoBuilder.Append (tileset.name);
            lastSpotInfoBuilder.Append ("\n");

            lastSpotInfoBuilder.Append ("█ ");
            lastSpotInfoBuilder.Append (lastSpotHovered.spotConfiguration);
            lastSpotInfoBuilder.Append (" (");
            lastSpotInfoBuilder.Append (TilesetUtility.GetStringFromConfiguration (lastSpotHovered.spotConfiguration));
            lastSpotInfoBuilder.Append (")\n");

            if (lastSpotHovered.spotConfigurationWithDamage != lastSpotHovered.spotConfiguration)
            {
                lastSpotInfoBuilder.Append ("░ ");
                lastSpotInfoBuilder.Append (lastSpotHovered.spotConfigurationWithDamage);
                lastSpotInfoBuilder.Append (" (");
                lastSpotInfoBuilder.Append (TilesetUtility.GetStringFromConfiguration (lastSpotHovered.spotConfigurationWithDamage));
                lastSpotInfoBuilder.Append (")\n");
            }

            lastSpotInfoBuilder.Append ("\n");

            foreach (var kvp1 in definition.subtypeGroups)
            {
                byte group = kvp1.Key;
                bool groupMatch = group == lastSpotHovered.blockGroup;

                if (groupMatch)
                    lastSpotInfoBuilder.Append ("<b>");
                bool identifierFound = identifiersPresent && tileset.groupIdentifiers.ContainsKey (group);
                lastSpotInfoBuilder.Append (group.ToString ());
                if (groupMatch)
                    lastSpotInfoBuilder.Append ("</b>");

                lastSpotInfoBuilder.Append (": ");
                bool first = true;
                foreach (var kvp2 in kvp1.Value)
                {
                    if (!first)
                        lastSpotInfoBuilder.Append (" - ");

                    byte subtype = kvp2.Key;
                    bool subtypeMatch = groupMatch && subtype == lastSpotHovered.blockSubtype;

                    if (subtypeMatch)
                    {
                        lastSpotInfoBuilder.Append ("<b>");
                        lastSpotInfoBuilder.Append (" <size=16>");
                    }
                    lastSpotInfoBuilder.Append (subtype);
                    if (subtypeMatch)
                    {
                        lastSpotInfoBuilder.Append ("</size>");
                        lastSpotInfoBuilder.Append ("</b>");
                    }

                    first = false;
                }

                if (identifierFound)
                {
                    if (groupMatch)
                        lastSpotInfoBuilder.Append ("<b>");
                    lastSpotInfoBuilder.Append (" <size=9>");
                    lastSpotInfoBuilder.Append (tileset.groupIdentifiers[group]);
                    lastSpotInfoBuilder.Append ("</size>");
                    if (groupMatch)
                        lastSpotInfoBuilder.Append ("</b>");
                }

                lastSpotInfoBuilder.Append ("\n");
            }

            lastSpotInfoGroups = lastSpotInfoBuilder.ToString ();
        }

        public void OnPositionChanged (Vector3 position, KeyCode button, bool shift, bool ctrl = false)
        {
            if (am == null)
            {
                Debug.LogError ("No area manager instance!");
                return;
            }

            // Debug.Log ("OPC | " + button);

            int indexForVolume = AreaUtility.GetIndexFromWorldPosition (position, am.GetHolderColliders ().position, am.boundsFull);
            int indexForSpot = AreaUtility.GetIndexFromWorldPosition (position + spotRaycastHitOffset, am.GetHolderColliders ().position, am.boundsFull);

            if
            (
                editingTarget == EditingTarget.Volume ||
                editingTarget == EditingTarget.Damage ||
                editingTarget == EditingTarget.Roads ||
                editingTarget == EditingTarget.Transfer ||
				editingTarget == EditingTarget.TerrainRamp
            )
            {
                if (indexForVolume != -1)
                    OnVolumeChanged (indexForVolume, indexForSpot, button, shift, ctrl);
            }
            else
            {

                if (indexForSpot != -1)
                    OnSpotChanged (indexForVolume, indexForSpot, button, shift, ctrl);
            }
        }

        public void OnVolumeChanged (int indexForVolume, int indexForSpot, KeyCode button, bool shift, bool ctrl = false)
        {
            // Debug.Log ("AM | OnVolumeChanged | Index " + index + " | MB: " + mouseButton);
            if (!indexForVolume.IsValidIndex (am.points))
            {
                Debug.LogWarning ("AM | OnVolumeChanged | Requested index " + indexForVolume + " is out of range");
                return;
            }

            AreaVolumePoint pointStart = am.points[indexForVolume];

            if (editingTarget == EditingTarget.Volume)
            {
                bool volumeAdded = button == KeyCode.Mouse0 || button == KeyCode.Mouse2; // TODO: If we remove volumeAdded and allow volume subtraction here, sometimes adding more cells won't work (example: bottom of cargo train cars)
                List<AreaVolumePoint> pointsToEdit = AreaManager.CollectPointsInBrush (pointStart, AreaManager.editingVolumeBrush, volumeBrushDepth, volumeAdded && volumeBrushDepthGoesUp, volumeBrushDepthRange);

                bool changePerformed = false;
                for (int i = 0; i < pointsToEdit.Count; ++i)
                {
                    AreaVolumePoint point = pointsToEdit[i];

                    // Can't remember why, but it's best to leave destroyed points alone
                    if (point.pointState == AreaVolumePointState.FullDestroyed)
                        continue;

                    if (!shift)
                    {
                        if (point.pointState == AreaVolumePointState.Empty && button == KeyCode.Mouse0)
                        {
                            point.pointState = AreaVolumePointState.Full;
                            changePerformed = true;
                        }
                        else if (point.pointState == AreaVolumePointState.Full)
                        {
                            if (button == KeyCode.Mouse1)
                            {
                                point.pointState = AreaVolumePointState.Empty;
                                changePerformed = true;
                            }
                            else if (button == KeyCode.Mouse2)
                            {
                                // Do not modify points with hard indestructibility from height, edges or tilesets to avoid wasting data
                                if (AreaManager.IsPointIndestructible (point, false, true, true, true, true))
                                {
                                    Debug.Log ($"Point {point.spotIndex} is already indirectly indestructible due to height, adjacency or tileset");
                                }
                                else
                                {
                                    bool destructibleState = ctrl; // Clear destructibility when CTRL is pressed
                                    point.destructible = destructibleState;

                                    if (propagateDestructibilityDown)
                                    {
                                        // Determine direction of propagation depending on UI toggle 'Brush depth goes up'
                                        AreaVolumePoint pointNeighbor = null;
                                        if (volumeBrushDepthGoesUp)
                                            pointNeighbor = point.pointsWithSurroundingSpots[3];
                                        else
                                            pointNeighbor = point.pointsInSpot[4];

                                        int pointsChanged = 0;
                                        while (pointNeighbor != null)
                                        {
                                            // Do not modify points with hard indestructibility from height, edges or tilesets to avoid wasting data
                                            if (AreaManager.IsPointIndestructible (pointNeighbor, false, true, true, true, true))
                                                break;

                                            pointNeighbor.destructible = point.destructible;

                                            // Determine direction of subsequent propagation depending on UI toggle 'Brush depth goes up'
                                            if (volumeBrushDepthGoesUp)
                                                pointNeighbor = pointNeighbor.pointsWithSurroundingSpots[3];
                                            else
                                                pointNeighbor = pointNeighbor.pointsInSpot[4];

                                            ++pointsChanged;
                                        }

                                        Debug.Log ($"Point {point.spotIndex} is now {(point.destructible ? "destructible" : "indestructible")} along with {pointsChanged} points under it");
                                    }
                                    else
                                    {
                                        Debug.Log ($"Point {point.spotIndex} is now {(point.destructible ? "destructible" : "indestructible")}");
                                    }
                                }
                            }
                            else if (button == KeyCode.Q)
                            {
                                point.destructionUntracked = !point.destructionUntracked;

                                if (propagateDestructibilityDown)
                                {
                                    var pointNeighborDown = point.pointsInSpot[4];
                                    int pointsChanged = 0;
                                    while (pointNeighborDown != null)
                                    {
                                        // Do not modify points with hard indestructibility from height, edges or tilesets to avoid wasting data
                                        if (AreaManager.IsPointIndestructible (pointNeighborDown, false, true, true, true, true))
                                            break;

                                        pointNeighborDown.destructionUntracked = point.destructionUntracked;
                                        pointNeighborDown = pointNeighborDown.pointsInSpot[4];
                                        ++pointsChanged;
                                    }

                                    Debug.Log ($"Point {point.spotIndex} destruction is now {(point.destructionUntracked ? "untracked" : "tracked")} along with {pointsChanged} points under it");
                                }
                                else
                                {
                                    Debug.Log ($"Point {point.spotIndex} is now {(point.destructible ? "destructible" : "indestructible")}");
                                }
                            }
                        }
                    }
                    else
                    {
                        if (point.pointState == AreaVolumePointState.Empty)
                        {
                            if (button == KeyCode.Mouse0)
                            {
                                point.terrainOffset = Mathf.RoundToInt (point.terrainOffset * 3f + 1f) / 3f;
                                Debug.LogWarning ($"Point {point.spotIndex} ({point.pointPositionIndex}, {point.pointState}) now has offset {point.terrainOffset}");
                                changePerformed = true;
                            }
                            else if (button == KeyCode.Mouse1)
                            {
                                point.terrainOffset = Mathf.RoundToInt (point.terrainOffset * 3f - 1f) / 3f;
                                Debug.LogWarning ($"Point {point.spotIndex} ({point.pointPositionIndex}, {point.pointState}) now has offset {point.terrainOffset}");
                                changePerformed = true;
                            }
                        }
                    }
                }

                if (changePerformed)
                {
                    bool terrainModified = false;
                    for (int i = 0; i < pointsToEdit.Count; ++i)
                    {
                        AreaVolumePoint point = pointsToEdit[i];

                        for (int s = 0; s < 8; ++s)
                        {
                            AreaVolumePoint pointWithNeighbourSpot = point.pointsWithSurroundingSpots[s];
                            if (pointWithNeighbourSpot == null)
                                continue;

                            pointWithNeighbourSpot.blockFlippedHorizontally = false;
                            pointWithNeighbourSpot.blockRotation = 0;
                            pointWithNeighbourSpot.blockGroup = 0;
                            pointWithNeighbourSpot.blockSubtype = 0;

                            if (swapTilesetOnVolumeEdits && pointWithNeighbourSpot.blockTileset != editingTilesetSelected.id)
                            {
                                if (!terrainModified)
                                    terrainModified =
                                        pointWithNeighbourSpot.blockTileset == AreaTilesetHelper.idOfTerrain ||
                                        editingTilesetSelected.id == AreaTilesetHelper.idOfTerrain;

                                pointWithNeighbourSpot.blockTileset = editingTilesetSelected.id;
                            }
                            else if (!terrainModified)
                                terrainModified = pointWithNeighbourSpot.blockTileset == AreaTilesetHelper.idOfTerrain;

                            if (swapColorOnVolumeEdits && pointWithNeighbourSpot.blockTileset != AreaTilesetHelper.idOfTerrain)
                            {
                                var properties = pointWithNeighbourSpot.customization;

                                properties.overrideIndex = overrideValue;
                                properties.huePrimary = selectedPrimaryColor.h;
                                properties.saturationPrimary = selectedPrimaryColor.s;
                                properties.brightnessPrimary = selectedPrimaryColor.b;
                                properties.hueSecondary = selectedSecondaryColor.h;
                                properties.saturationSecondary = selectedSecondaryColor.s;
                                properties.brightnessSecondary = selectedSecondaryColor.b;

                                pointWithNeighbourSpot.customization = properties;
                            }

                            am.UpdateSpotAtIndex (pointWithNeighbourSpot.spotIndex, false);
                            if (pointWithNeighbourSpot.blockTileset == 0)
                                pointWithNeighbourSpot.blockTileset = AreaTilesetHelper.idOfFallback;

                            am.RebuildBlock (pointWithNeighbourSpot);
                            am.RebuildCollisionForPoint (pointWithNeighbourSpot);
                        }

                        am.UpdateDamageAroundIndex (indexForVolume);
                    }

                    if (terrainModified)
                    {
                        var sceneHelper = CombatSceneHelper.ins;
                        sceneHelper.terrain.Rebuild (true);
                    }

                    if (am.sliceEnabled)
                        am.UpdateSlicing ();
                }
            }
            else if (editingTarget == EditingTarget.Transfer)
            {
                if (shift)
                {
                    am.targetOrigin = new Vector3Int (pointStart.pointPositionIndex.x, am.targetOrigin.y, pointStart.pointPositionIndex.z);
                    if (button == KeyCode.RightBracket)
                    {
                        am.targetOrigin.y -= 1;
                    }
                    else if (button == KeyCode.LeftBracket)
                    {
                        am.targetOrigin.y += 1;
                    }
                    if (button == KeyCode.Mouse0)
                    {
                        am.PasteVolume (am.targetOrigin, am.brushApplicationMode);

                        if (am.sliceEnabled)
                            am.UpdateSlicing ();
                    }
                }
                else
                {
                    if (button == KeyCode.Mouse0)
                    {
                        am.clipboardOrigin = new Vector3Int (pointStart.pointPositionIndex.x, 0, pointStart.pointPositionIndex.z);
                    }
                    else if (button == KeyCode.Mouse2)
                    {
                        var difference = new Vector3Int (pointStart.pointPositionIndex.x, 0, pointStart.pointPositionIndex.z) - am.clipboardOrigin;
                        var bounds = new Vector3Int (difference.x, am.boundsFull.y - 1, difference.z) + Vector3Int.size1x1x1;

                        if (Mathf.Abs (bounds.x) < 2 || Mathf.Abs (bounds.z) < 2)
                        {
                            Debug.LogWarning ("Selected area is too small!");
                            return;
                        }

                        if (bounds.x < 0)
                        {
                            bounds.x = am.clipboardOrigin.x - pointStart.pointPositionIndex.x;
                            am.clipboardOrigin.x = pointStart.pointPositionIndex.x;
                        }

                        if (bounds.z < 0)
                        {
                            bounds.z = am.clipboardOrigin.z - pointStart.pointPositionIndex.z;
                            am.clipboardOrigin.z = pointStart.pointPositionIndex.z;
                        }

                        am.clipboardBoundsRequested = bounds;
                    }
                    else if (button == KeyCode.Mouse1)
                    {
                        am.targetOrigin = new Vector3Int (pointStart.pointPositionIndex.x, am.targetOrigin.y, pointStart.pointPositionIndex.z);
                    }
                    else if (button == KeyCode.PageUp)
                    {
                        am.clipboardOrigin.y -= 1;
                    }
                    else if (button == KeyCode.PageDown)
                    {
                        am.clipboardOrigin.y += 1;
                    }
                }
            }

            else if (editingTarget == EditingTarget.Damage)
            {
                AreaVolumePoint point = am.points[indexForVolume];

                if (Application.isPlaying)
                {
                    #if !PB_MODSDK
                    if (button == KeyCode.Mouse1 && point.pointState == AreaVolumePointState.Full)
                    {
                        am.ApplyDamageToPoint (point, 1000);

                        var sceneHelper = CombatSceneHelper.ins;
                        var tileset = sceneHelper.areaManager.GetClosestTileset (point);

                        AssetPoolUtility.ActivateInstance (tileset.fxNameExplosion, point.pointPositionLocal, Vector3.forward);
                    }
                    #endif
                }

                else
                {
                    if
                    (
                        (button == KeyCode.Mouse0 && point.pointState == AreaVolumePointState.FullDestroyed) ||
                        (button == KeyCode.Mouse1 && point.pointState == AreaVolumePointState.Full)
                    )
                    {
                        if (button == KeyCode.Mouse0)
                        {
                            point.pointState = AreaVolumePointState.Full;
                            point.integrity = point.integrityForDestructionAnimation = 1f;
                        }
                        else
                        {
                            bool indestructible = AreaManager.IsPointIndestructible (point, true, true, true, true, true);
                            if (!indestructible || allowIndestructibleDestruction)
                            {
                                point.pointState = AreaVolumePointState.FullDestroyed;
                                point.integrity = point.integrityForDestructionAnimation = 0f;

                                #if !PB_MODSDK
                                if (!Application.isPlaying && DataShortcuts.sim.debugCombatStructureAnalysis)
                                {
                                    AreaUtility.ScanNeighborsDisconnectedOnDestruction (point, am.points);
                                }
                                #endif
                            }
                            else
                            {
                                Debug.LogWarning ($"Destruction blocked by point being indestructible: try enabling the option allowing removal of indestructible points");
                            }
                        }

                        am.UpdateSpotsAroundIndex (indexForVolume, false);
                        am.RebuildBlocksAroundIndex (indexForVolume);
                        am.UpdateDamageAroundIndex (indexForVolume);
                        am.RebuildCollisionsAroundIndex (indexForVolume);

                        if (am.sliceEnabled)
                            am.UpdateSlicing ();
                    }

                    else if (button == KeyCode.PageUp || button == KeyCode.PageDown)
                    {
                        point.integrity = Mathf.Clamp (point.integrity + (button == KeyCode.PageUp ? 0.1f : -0.1f), 0.1f, 1f);
                        for (int i = 0; i < point.pointsWithSurroundingSpots.Length; ++i)
                        {
                            AreaVolumePoint pointAround = point.pointsWithSurroundingSpots[i];
                            am.ApplyShaderPropertiesAtPoint (pointAround, AreaManager.ShaderOverwriteMode.None, true, true, true);
                        }
                    }
                }
            }
            else if (editingTarget == EditingTarget.Roads)
            {
                EditRoad (indexForVolume, button);
            }
			else if (editingTarget == EditingTarget.TerrainRamp)
            {
                if (button == KeyCode.Mouse0 || button == KeyCode.Mouse1)
                {
                    var proximityCheck = shift ? AreaManager.SlopeProximityCheck.LateralSingle : AreaManager.SlopeProximityCheck.None;
                    am.TrySettingSlope (pointStart, button == KeyCode.Mouse0, true, proximityCheck, true);
                }
                else if (button == KeyCode.Mouse2)
                {
                    if (indexForSpot != -1)
                    {
                        AreaVolumePoint pointForSpot = am.points[indexForSpot];
                        clipboardTileset = pointForSpot.blockTileset;
                    }
                }
            }
        }

        public void OnSpotChanged (int indexForVolume, int indexForSpot, KeyCode button, bool shift, bool ctrl = false)
        {
            // Debug.Log ("OSC | " + button);
            if (indexForSpot < 0 || indexForSpot > am.points.Count - 1)
            {
                Debug.LogWarning ("AM | OnSpotChanged | Requested index " + indexForSpot + " is out of range");
                return;
            }

            switch (editingTarget)
            {
                case EditingTarget.Tileset:
                    EditTileset (indexForSpot, button);
                    break;
                case EditingTarget.Spot:
                    EditBlock (indexForSpot, button, shift);
                    break;
				case EditingTarget.RoadCurves:
					am.EditRoadCurves(indexForSpot, button, shift);
					break;
                case EditingTarget.Color:
                    EditColor (indexForSpot, button);
                    break;
                case EditingTarget.Props:
                    EditProp (indexForSpot, button, shift);
                    break;
                case EditingTarget.Navigation:
                    EditNavigation (indexForSpot, button);
                    break;
                default:
                    break;
            }
        }

        public void EditTileset (int spotIndex, KeyCode mouseButton)
        {
            AreaVolumePoint startingPoint = am.points[spotIndex];
            if (mouseButton == KeyCode.Mouse0)
            {
                if (editingTilesetSelected == null)
                    return;

                bool terrainModified = false;
                List<AreaVolumePoint> pointsActedOn = new List<AreaVolumePoint> ();
                CheckSearchRequirements (startingPoint, ref pointsActedOn);
                if (pointsActedOn == null)
                    return;

                for (int i = 0; i < pointsActedOn.Count; ++i)
                {
                    AreaVolumePoint point = pointsActedOn[i];
                    if (point.blockTileset != editingTilesetSelected.id)
                    {
                        if (!terrainModified)
                        {
                            terrainModified =
                                point.blockTileset == AreaTilesetHelper.idOfTerrain ||
                                editingTilesetSelected.id == AreaTilesetHelper.idOfTerrain;
                        }

                        point.blockTileset = editingTilesetSelected.id;
                        am.RebuildBlock (point);
                    }
                }

                if (terrainModified)
                {
                    var sceneHelper = CombatSceneHelper.ins;
                    sceneHelper.terrain.Rebuild (true);
                }
            }

            else if (mouseButton == KeyCode.Mouse2)
            {
                editingTilesetSelected = AreaTilesetHelper.database.tilesets[startingPoint.blockTileset];
            }
        }



        /*
        public enum SpotSearchType
        {
            None = 0,
            SameFloor = 1,
            SameFloorIsolated = 2,
            SameConfiguration = 3,
            SameTileset = 4,
            SameEverything = 5,
            AllEmptyNodes = 6,
			SameColor = 7
        }
        */

        [Flags]
        public enum SpotSearchFlags
        {
            None = 0,
            SameFloor = 1,
            SameStructure = 2,
            SameTileset = 4,
            SameConfiguration = 8,
            SameColor = 16,
            SameFloorTileset = SameFloor | SameTileset,
            SameFloorColor = SameFloor | SameColor,
            SameFloorTilesetColor = SameFloor | SameTileset | SameColor,
            SameTilesetSameColor = SameTileset | SameColor
        }

        private static int pointSearchDataChangeTracker;
        private static List<AreaVolumePointSearchData> pointSearchData;
        private static List<AreaVolumePointSearchData> lastSearchResults = new List<AreaVolumePointSearchData> ();
        private static List<AreaVolumePoint> lastSearchResultsAsPoints = new List<AreaVolumePoint> ();
        private static AreaVolumePoint lastSearchOrigin;

        private static SpotSearchFlags currentSearchFlags = SpotSearchFlags.None;
        private static SpotSearchFlags lastSearchFlags = SpotSearchFlags.None;

        // private static SpotSearchType currentSearchType;
        // private static SpotSearchType lastSearchType;
        private static int defaultSearchStatus = -1;

        public delegate bool SearchValidationRoutine (AreaVolumePoint startingPoint, AreaVolumePointSearchData arg);
        private List<SearchValidationRoutine> searchValidationFunctions = new List<SearchValidationRoutine> ();

        private void CheckSearchRequirements (AreaVolumePoint startingPoint, ref List<AreaVolumePoint> pointsActedOn)
        {
            if (currentSearchFlags == SpotSearchFlags.None)
            {
                lastSearchOrigin = startingPoint;
                lastSearchFlags = SpotSearchFlags.None;

                lastSearchResultsAsPoints.Clear ();
                lastSearchResultsAsPoints.Add (startingPoint);
            }

            if (lastSearchOrigin == null || lastSearchOrigin != startingPoint || lastSearchFlags != currentSearchFlags)
                SearchForSpots (startingPoint, currentSearchFlags);
            else
            {
                pointsActedOn = lastSearchResultsAsPoints;
                // Debug.LogWarning ($"Acting on {pointsActedOn.Count} points");

                // lastSearchResultsAsPoints.Clear ();
                lastSearchResults.Clear ();
                lastSearchOrigin = null;
            }
        }

        private bool SearchValidationSameConfiguration (AreaVolumePoint startingPoint, AreaVolumePointSearchData arg)
        {
            bool result =
                arg.status == defaultSearchStatus &&
                arg.point.spotPresent &&
                arg.point.spotConfiguration == startingPoint.spotConfiguration;
            return result;
        }

        private bool SearchValidationSameFloor (AreaVolumePoint startingPoint, AreaVolumePointSearchData arg)
        {
            bool pairIsNotSeparated =
                arg.directionFromParentCandidate == PointNeighbourDirection.YPos ||
                arg.directionFromParentCandidate == PointNeighbourDirection.YNeg ||
                !TilesetUtility.IsConfigurationPairSeparated
                (
                    arg.parentCandidate.point.spotConfiguration,
                    arg.point.spotConfiguration,
                    arg.directionFromParentCandidate
                );

            bool result =
                arg.status == defaultSearchStatus &&
                arg.point.spotPresent &&
                arg.point.spotConfiguration != AreaNavUtility.configEmpty &&
                arg.point.pointPositionIndex.y == startingPoint.pointPositionIndex.y &&
                pairIsNotSeparated;

            return result;
        }

        private bool SearchValidationSameFloorIsolated (AreaVolumePoint startingPoint, AreaVolumePointSearchData arg)
        {
            bool pairIsNotSeparated =
                arg.directionFromParentCandidate == PointNeighbourDirection.YPos ||
                arg.directionFromParentCandidate == PointNeighbourDirection.YNeg ||
                !TilesetUtility.IsConfigurationPairSeparated
                (
                    arg.parentCandidate.point.spotConfiguration,
                    arg.point.spotConfiguration,
                    arg.directionFromParentCandidate
                );

            bool result =
                arg.status == defaultSearchStatus &&
                arg.point.spotPresent &&
                arg.point.spotConfiguration != AreaNavUtility.configEmpty &&
                arg.point.pointPositionIndex.y == startingPoint.pointPositionIndex.y &&
                pairIsNotSeparated &&
                arg.point.spotConfiguration != AreaNavUtility.configFloor;

            return result;
        }

        private bool SearchValidationSameColor (AreaVolumePoint startingPoint, AreaVolumePointSearchData arg)
        {
	        bool result =
		        arg.status == defaultSearchStatus &&
		        arg.point.spotPresent &&
		        arg.point.spotConfiguration != AreaNavUtility.configEmpty &&
		        arg.point.spotConfiguration != AreaNavUtility.configFull &&
		        Mathf.Approximately(arg.point.customization.huePrimary, startingPoint.customization.huePrimary) &&
		        Mathf.Approximately(arg.point.customization.saturationPrimary, startingPoint.customization.saturationPrimary) &&
		        Mathf.Approximately(arg.point.customization.brightnessPrimary, startingPoint.customization.brightnessPrimary) &&
		        Mathf.Approximately(arg.point.customization.hueSecondary, startingPoint.customization.hueSecondary) &&
		        Mathf.Approximately(arg.point.customization.saturationSecondary, startingPoint.customization.saturationSecondary) &&
		        Mathf.Approximately(arg.point.customization.brightnessSecondary, startingPoint.customization.brightnessSecondary);
	        return result;
        }

        private bool SearchValidationSameTileset (AreaVolumePoint startingPoint, AreaVolumePointSearchData arg)
        {
            bool result =
                arg.status == defaultSearchStatus &&
                arg.point.spotPresent &&
                arg.point.spotConfiguration != AreaNavUtility.configEmpty &&
                arg.point.spotConfiguration != AreaNavUtility.configFull &&
                arg.point.blockTileset == startingPoint.blockTileset;
            return result;
        }

        private bool SearchValidationSameEverything (AreaVolumePoint startingPoint, AreaVolumePointSearchData arg)
        {
            bool result =
                arg.status == defaultSearchStatus &&
                arg.point.spotPresent &&
                arg.point.spotConfiguration == startingPoint.spotConfiguration &&
                arg.point.blockTileset == startingPoint.blockTileset &&
                arg.point.blockGroup == startingPoint.blockGroup &&
                arg.point.blockSubtype == startingPoint.blockSubtype;
            return result;
        }

        private bool SearchValidationAllEmptyNodes (AreaVolumePoint startingPoint, AreaVolumePointSearchData arg)
        {
            bool result =
                arg.status == defaultSearchStatus &&
                arg.point.spotPresent &&
                (arg.point.spotConfiguration == AreaNavUtility.configEmpty || arg.point.spotConfiguration == AreaNavUtility.configFull);
            return result;
        }

        private void SearchForSpots (AreaVolumePoint startingPoint, SpotSearchFlags searchFlags)
        {
            if (startingPoint == null || !startingPoint.spotPresent || startingPoint.spotConfiguration == 0)
            {
                Debug.Log ("Bailing out of search for spots due to starting point being null, having no spot, or being empty");
                return;
            }

            lastSearchOrigin = startingPoint;
            lastSearchFlags = searchFlags;

            lastSearchResults.Clear ();
            lastSearchResultsAsPoints.Clear ();

            if (searchFlags == SpotSearchFlags.None)
            {
                lastSearchResultsAsPoints.Add (startingPoint);
            }
            else
            {
                // Debug.Log ("Beginning search with mode " + searchType);
                searchValidationFunctions.Clear ();

                if (searchFlags.HasFlag (SpotSearchFlags.SameConfiguration))
                    searchValidationFunctions.Add (SearchValidationSameConfiguration);

                if (searchFlags.HasFlag (SpotSearchFlags.SameFloor))
                    searchValidationFunctions.Add (SearchValidationSameFloor);

                if (searchFlags.HasFlag (SpotSearchFlags.SameStructure))
                    searchValidationFunctions.Add (SearchValidationSameFloorIsolated);

                if (searchFlags.HasFlag (SpotSearchFlags.SameTileset))
                    searchValidationFunctions.Add (SearchValidationSameTileset);

                if (searchFlags.HasFlag (SpotSearchFlags.SameColor))
                    searchValidationFunctions.Add (SearchValidationSameColor);

                AreaVolumePointSearchData psd;
                if (pointSearchData == null || pointSearchData.Count != am.points.Count || pointSearchDataChangeTracker != am.ChangeTracker)
                {
                    pointSearchData = new List<AreaVolumePointSearchData> (am.points.Count);
                    pointSearchDataChangeTracker = am.ChangeTracker;

                    for (int i = 0; i < am.points.Count; ++i)
                    {
                        psd = new AreaVolumePointSearchData ();
                        psd.point = am.points[i];
                        pointSearchData.Add (psd);
                    }

                    for (int i = 0; i < pointSearchData.Count; ++i)
                    {
                        psd = pointSearchData[i];

                        // spotpoints: 1 (X+) 2 (Z+) 4 (Y+)
                        // spotsAroundThisPoint: 3 (Y-), 5 (Z-), 6 (X-)

                        AreaVolumePoint pointYPos = psd.point.pointsInSpot[4];
                        if (pointYPos != null)
                            psd.neighbourYPos = pointSearchData[pointYPos.spotIndex];

                        AreaVolumePoint pointXPos = psd.point.pointsInSpot[1];
                        if (pointXPos != null)
                            psd.neighbourXPos = pointSearchData[pointXPos.spotIndex];

                        AreaVolumePoint pointXNeg = psd.point.pointsWithSurroundingSpots[6];
                        if (pointXNeg != null)
                            psd.neighbourXNeg = pointSearchData[pointXNeg.spotIndex];

                        AreaVolumePoint pointZPos = psd.point.pointsInSpot[2];
                        if (pointZPos != null)
                            psd.neighbourZPos = pointSearchData[pointZPos.spotIndex];

                        AreaVolumePoint pointZNeg = psd.point.pointsWithSurroundingSpots[5];
                        if (pointZNeg != null)
                            psd.neighbourZNeg = pointSearchData[pointZNeg.spotIndex];

                        AreaVolumePoint pointYNeg = psd.point.pointsWithSurroundingSpots[3];
                        if (pointYNeg != null)
                            psd.neighbourYNeg = pointSearchData[pointYNeg.spotIndex];
                    }
                }

                foreach (var psd1 in pointSearchData)
                {
                    psd1.status = defaultSearchStatus;
                }

                GetPointsConnected (startingPoint, searchValidationFunctions);

                for (int i = 0; i < pointSearchData.Count; ++i)
                {
                    psd = pointSearchData[i];
                    if (psd.status != defaultSearchStatus)
                    {
                        lastSearchResults.Add (psd);
                        lastSearchResultsAsPoints.Add (psd.point);
                    }
                }

                // Debug.Log ("Search results: " + lastSearchResults.Count);
            }
        }

        private void GetPointsConnected (AreaVolumePoint startingPoint, List<SearchValidationRoutine> validationFunctions)
        {
            if (startingPoint == null || !startingPoint.spotPresent || startingPoint.spotConfiguration == 0 || validationFunctions == null || validationFunctions.Count == 0)
                return;

            AreaVolumePointSearchData psd = null;
            Queue<AreaVolumePointSearchData> q = new Queue<AreaVolumePointSearchData> (am.points.Count);

            int iterations = 0;
            int limit = am.points.Count + 1000;

            for (int i = 0; i < pointSearchData.Count; ++i)
            {
                psd = pointSearchData[i];
                psd.status = defaultSearchStatus;
                psd.parent = null;
                psd.parentCandidate = null;
            }

            q.Enqueue (pointSearchData[startingPoint.spotIndex]);
            while (q.Count > 0)
            {
                psd = q.Dequeue ();

                if (q.Count > limit)
                    throw new System.Exception ("The algorithm is probably looping. Queue size: " + q.Count);

                if (psd.status != defaultSearchStatus)
                    continue;

                psd.status = iterations;
                psd.parent = psd.parentCandidate;
                psd.directionFromParent = psd.directionFromParentCandidate;

                if (CheckSearchStepValidity (startingPoint, psd.neighbourYPos, psd, PointNeighbourDirection.YPos, validationFunctions))
                    q.Enqueue (psd.neighbourYPos);

                if (CheckSearchStepValidity (startingPoint, psd.neighbourXPos, psd, PointNeighbourDirection.XPos, validationFunctions))
                    q.Enqueue (psd.neighbourXPos);

                if (CheckSearchStepValidity (startingPoint, psd.neighbourXNeg, psd, PointNeighbourDirection.XNeg, validationFunctions))
                    q.Enqueue (psd.neighbourXNeg);

                if (CheckSearchStepValidity (startingPoint, psd.neighbourZPos, psd, PointNeighbourDirection.ZPos, validationFunctions))
                    q.Enqueue (psd.neighbourZPos);

                if (CheckSearchStepValidity (startingPoint, psd.neighbourZNeg, psd, PointNeighbourDirection.ZNeg, validationFunctions))
                    q.Enqueue (psd.neighbourZNeg);

                if (CheckSearchStepValidity (startingPoint, psd.neighbourYNeg, psd, PointNeighbourDirection.YNeg, validationFunctions))
                    q.Enqueue (psd.neighbourYNeg);

                iterations++;
            }
        }

        private bool CheckSearchStepValidity
        (
            AreaVolumePoint startingPoint,
            AreaVolumePointSearchData psd,
            AreaVolumePointSearchData parentCandidate,
            PointNeighbourDirection direction,
            List<SearchValidationRoutine> validationFunctions
        )
        {
            if (psd == null || validationFunctions == null)
            {
                // Debug.Log ("Bailing out of search step due to data being null or validation function being null");
                return false;
            }
            else
            {
                psd.directionFromParentCandidate = direction;
                psd.parentCandidate = parentCandidate;

                foreach (var function in validationFunctions)
                {
                    if (function (startingPoint, psd) == false)
                        return false;
                }

                return true;
            }
        }




        public void EditBlock (int spotIndex, KeyCode button, bool shift)
        {
            AreaVolumePoint startingPoint = am.points[spotIndex];
            if (startingPoint.spotConfiguration == 0 || startingPoint.spotConfiguration == 255)
            {
                Debug.Log ("AM | EditBlockAtIndex | Index " + spotIndex + " has configuration " + startingPoint.spotConfiguration + " and shouldn't be possible to select");
                return;
            }

            // LMB                      - rotate
            // RMB                      - flip
            // MMB                      - copy block

            // ^ (scroll up)            - subtype next
            // v (scroll down)          - subtype prev
            // [ (bracket left)         - group prev
            // ] (bracket right)        - group next
            // < (arrow left / comma)   - paste group, subtype
            // > (arrow right / period) - paste group, subtype, rotation, flipping

            AreaBlockDefinition definition = AreaTilesetHelper.database.tilesets[startingPoint.blockTileset].blocks[startingPoint.spotConfiguration];
            AreaConfigurationData data = AreaTilesetHelper.database.configurationDataForBlocks[startingPoint.spotConfiguration];

            if (button == KeyCode.Mouse0)
            {
                if (data.customRotationPossible)
                {
                    startingPoint.blockRotation = startingPoint.blockRotation.OffsetAndWrap (true, 3);
                    am.RebuildBlock (startingPoint);
                }
            }

            else if (button == KeyCode.Mouse1)
            {
                if (data.customFlippingMode != -1)
                {
                    startingPoint.blockFlippedHorizontally = !startingPoint.blockFlippedHorizontally;
                    am.RebuildBlock (startingPoint);
                }
            }

            else if (button == KeyCode.Mouse2)
            {
                clipboardConfigurations = TilesetUtility.GetConfigurationTransformations (startingPoint.spotConfiguration);
                clipboardTileset = startingPoint.blockTileset;
                clipboardGroup = startingPoint.blockGroup;
                clipboardSubtype = startingPoint.blockSubtype;
                clipboardRotation = startingPoint.blockRotation;
                clipboardFlipping = startingPoint.blockFlippedHorizontally;
                clipboardColor = startingPoint.customization;

                lastSearchResults.Clear ();
                lastSearchOrigin = null;

                //AreaManager.pointTestIndex = startingPoint.spotIndex;
            }

            else if (button == KeyCode.LeftBracket || button == KeyCode.RightBracket)
            {
                bool forward = button == KeyCode.LeftBracket;
                byte groupKeyNew = AreaTilesetHelper.OffsetBlockGroup (definition, startingPoint.blockGroup, forward);
                if (groupKeyNew != startingPoint.blockGroup)
                {
                    startingPoint.blockGroup = groupKeyNew;
                    am.RebuildBlock (startingPoint);
                }
            }

            else if (button == KeyCode.PageDown || button == KeyCode.PageUp)
            {
                bool forward = button == KeyCode.PageUp;
                byte subtypeKeyNew = AreaTilesetHelper.OffsetBlockSubtype (definition, startingPoint.blockGroup, startingPoint.blockSubtype, forward);
                if (subtypeKeyNew != startingPoint.blockSubtype)
                {
                    startingPoint.blockSubtype = subtypeKeyNew;
                    am.RebuildBlock (startingPoint);
                }
            }

            else if (button == KeyCode.V)
            {
                if (shift)
                {
                    if (clipboardConfigurations.Contains (startingPoint.spotConfiguration))
                    {
                        startingPoint.blockTileset = clipboardTileset;
                        startingPoint.blockGroup = clipboardGroup;
                        startingPoint.blockSubtype = clipboardSubtype;
                        startingPoint.blockRotation = clipboardRotation;
                        startingPoint.blockFlippedHorizontally = clipboardFlipping;
                        am.RebuildBlock (startingPoint);
                    }
                }
                else
                {
                    List<AreaVolumePoint> pointsActedOn = new List<AreaVolumePoint> ();
                    CheckSearchRequirements (startingPoint, ref pointsActedOn);
                    if (pointsActedOn != null)
                    {
                        Debug.Log ("AM (I) | EditBlock | Replacing targeted spots | Points acted on: " + pointsActedOn.Count);
                        for (int i = 0; i < pointsActedOn.Count; ++i)
                        {
							bool doRebuild = false;
                            AreaVolumePoint point = pointsActedOn[i];
                            if (clipboardMustOverwriteSubtype)
                            {
                                if (clipboardConfigurations.Contains (point.spotConfiguration))
                                {
                                    point.blockTileset = clipboardTileset;
                                    point.blockGroup = clipboardGroup;
                                    point.blockSubtype = clipboardSubtype;
                                    doRebuild = true;
                                }
                            }
                            else
                            {
                                if
                                (
                                    AreaTilesetHelper.database.tilesets.ContainsKey (point.blockTileset) &&
                                    AreaTilesetHelper.database.tilesets[point.blockTileset].blocks[point.spotConfiguration] != null &&
                                    AreaTilesetHelper.database.tilesets[point.blockTileset].blocks[point.spotConfiguration].subtypeGroups.ContainsKey (clipboardGroup)
                                )
                                {
                                    point.blockTileset = clipboardTileset;
                                    point.blockGroup = clipboardGroup;
                                    doRebuild = true;
                                }
                            }

							if(clipboardOverwriteColor)
							{
								point.customization = clipboardColor;
								doRebuild = true;
							}

							if(doRebuild)
								am.RebuildBlock (point);
                        }
                    }
                }
            }

            else if (button == KeyCode.Q)
            {
                List<AreaVolumePoint> pointsActedOn = new List<AreaVolumePoint> ();
                CheckSearchRequirements (startingPoint, ref pointsActedOn);

                if (pointsActedOn != null)
                {
                    Debug.Log ("AM (I) | EditBlock | Randomizing targeted spots | Points acted on: " + pointsActedOn.Count);
                    for (int i = 0; i < pointsActedOn.Count; ++i)
                    {
                        AreaVolumePoint point = pointsActedOn[i];
                        AreaConfigurationData configurationData = AreaTilesetHelper.database.configurationDataForBlocks[point.spotConfiguration];

                        if
                        (
                            AreaTilesetHelper.database.tilesets.ContainsKey (point.blockTileset) &&
                            AreaTilesetHelper.database.tilesets[point.blockTileset].blocks[point.spotConfiguration] != null &&
                            AreaTilesetHelper.database.tilesets[point.blockTileset].blocks[point.spotConfiguration].subtypeGroups.ContainsKey (point.blockGroup)
                        )
                        {
                            SortedDictionary<byte, GameObject> subtypes = AreaTilesetHelper.database.tilesets[point.blockTileset].blocks[point.spotConfiguration].subtypeGroups[point.blockGroup];
                            point.blockSubtype = subtypes.GetRandomKey ();

                            if (configurationData.customRotationPossible)
                                point.blockRotation = (byte)Random.Range (0, 4);

                            am.RebuildBlock (point);
                        }
                    }
                }
            }
        }

		private void DrawVolumeDirection(Vector3 posA, Vector3 posB, Vector3 dir, Color colorMain)
		{
			var colorMainTransparent = colorMain.WithAlpha (0.15f);

			var hc = Handles.color;

			var centerPt = new Vector3((posA.x + posB.x) * 0.5f, posB.y, (posA.z + posB.z) * 0.5f);
			var halfSize = new Vector3((posB.x - posA.x) * 0.5f, 0f, (posB.z - posA.z) * 0.5f);

			var fwdStep = Vector3.Scale(dir, halfSize);
			var sideStep = new Vector3(fwdStep.z,  0f, -fwdStep.x);
			var edgePt = centerPt + fwdStep;

			Handles.color = colorMainTransparent;
			Handles.DrawAAConvexPolygon(edgePt + sideStep.normalized, edgePt + fwdStep.normalized, edgePt - sideStep.normalized);

			Handles.color = colorMain;
			Handles.DrawPolyLine(edgePt + sideStep.normalized, edgePt + fwdStep.normalized, edgePt - sideStep.normalized);

			Handles.color = hc;
		}

        private void DrawZTestVolume (Vector3 posA, Vector3 posB, Color colorMain, Color colorCulled)
        {
            Vector3 difference = posB - posA;
            difference.y = -difference.y;
            Vector3 center = (posA + posB) / 2f;

            var hc = Handles.color;
            var zt = Handles.zTest;

            var colorMainTransparent = colorMain.WithAlpha (0.15f);
            var colorCulledTransparent = colorCulled.WithAlpha (0.15f);

            Handles.zTest = CompareFunction.LessEqual;
            Handles.color = colorMain;
            Handles.CubeHandleCap (0, posA, Quaternion.identity, 0.5f, EventType.Repaint);
            Handles.CubeHandleCap (1, posB, Quaternion.identity, 0.5f, EventType.Repaint);

            Handles.zTest = CompareFunction.Greater;
            Handles.color = colorCulled;
            Handles.CubeHandleCap (0, posA, Quaternion.identity, 0.5f, EventType.Repaint);
            Handles.CubeHandleCap (1, posB, Quaternion.identity, 0.5f, EventType.Repaint);

            Handles.color = Color.white.WithAlpha (1f);
            transferPreviewVerts[0] = new Vector3 (posA.x, posB.y, posA.z);
            transferPreviewVerts[1] = new Vector3 (posA.x, posA.y, posA.z);
            transferPreviewVerts[2] = new Vector3 (posB.x, posA.y, posA.z);
            transferPreviewVerts[3] = new Vector3 (posB.x, posB.y, posA.z);
            Handles.zTest = CompareFunction.LessEqual;
            Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorMainTransparent, colorMain);
            Handles.zTest = CompareFunction.Greater;
            Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorCulledTransparent, colorCulled);

            transferPreviewVerts[0] = new Vector3 (posB.x, posB.y, posA.z);
            transferPreviewVerts[1] = new Vector3 (posB.x, posA.y, posA.z);
            transferPreviewVerts[2] = new Vector3 (posB.x, posA.y, posB.z);
            transferPreviewVerts[3] = new Vector3 (posB.x, posB.y, posB.z);
            Handles.zTest = CompareFunction.LessEqual;
            Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorMainTransparent, colorMain);
            Handles.zTest = CompareFunction.Greater;
            Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorCulledTransparent, colorCulled);

            transferPreviewVerts[0] = new Vector3 (posB.x, posB.y, posB.z);
            transferPreviewVerts[1] = new Vector3 (posB.x, posA.y, posB.z);
            transferPreviewVerts[2] = new Vector3 (posA.x, posA.y, posB.z);
            transferPreviewVerts[3] = new Vector3 (posA.x, posB.y, posB.z);
            Handles.zTest = CompareFunction.LessEqual;
            Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorMainTransparent, colorMain);
            Handles.zTest = CompareFunction.Greater;
            Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorCulledTransparent, colorCulled);

            transferPreviewVerts[0] = new Vector3 (posA.x, posB.y, posB.z);
            transferPreviewVerts[1] = new Vector3 (posA.x, posA.y, posB.z);
            transferPreviewVerts[2] = new Vector3 (posA.x, posA.y, posA.z);
            transferPreviewVerts[3] = new Vector3 (posA.x, posB.y, posA.z);
            Handles.zTest = CompareFunction.LessEqual;
            Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorMainTransparent, colorMain);
            Handles.zTest = CompareFunction.Greater;
            Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorCulledTransparent, colorCulled);

            Handles.color = hc;
            Handles.zTest = zt;
        }

        private Vector3[] polylinePointsTemp = new Vector3[2];

        private void DrawAAWireCube (Vector3 center, Vector3 halfExtents, Color color, float width = 3f)
        {
            var hc = Handles.color;
            Handles.color = color;

            var t1 = center + new Vector3 (-halfExtents.x, halfExtents.y, -halfExtents.z);
            var t2 = center + new Vector3 (halfExtents.x, halfExtents.y, -halfExtents.z);
            var t3 = center + new Vector3 (halfExtents.x, halfExtents.y, halfExtents.z);
            var t4 = center + new Vector3 (-halfExtents.x, halfExtents.y, halfExtents.z);

            var b1 = center + new Vector3 (-halfExtents.x, -halfExtents.y, -halfExtents.z);
            var b2 = center + new Vector3 (halfExtents.x, -halfExtents.y, -halfExtents.z);
            var b3 = center + new Vector3 (halfExtents.x, -halfExtents.y, halfExtents.z);
            var b4 = center + new Vector3 (-halfExtents.x, -halfExtents.y, halfExtents.z);

            DrawAALine (b1, t1, width);
            DrawAALine (b2, t2, width);
            DrawAALine (b3, t3, width);
            DrawAALine (b4, t4, width);

            DrawAALine (t1, t2, width);
            DrawAALine (t2, t3, width);
            DrawAALine (t3, t4, width);
            DrawAALine (t4, t1, width);

            DrawAALine (b1, b2, width);
            DrawAALine (b2, b3, width);
            DrawAALine (b3, b4, width);
            DrawAALine (b4, b1, width);

            Handles.color = hc;
        }

        private void DrawAAWireSquare (Vector3 center, Vector2 halfExtents, Color color, float width = 3f)
        {
            var hc = Handles.color;
            Handles.color = color;

            var t1 = center + new Vector3 (-halfExtents.x, 0, -halfExtents.y);
            var t2 = center + new Vector3 (halfExtents.x, 0, -halfExtents.y);
            var t3 = center + new Vector3 (halfExtents.x, 0, halfExtents.y);
            var t4 = center + new Vector3 (-halfExtents.x, 0, halfExtents.y);

            DrawAALine (t1, t2, width);
            DrawAALine (t2, t3, width);
            DrawAALine (t3, t4, width);
            DrawAALine (t4, t1, width);

            Handles.color = hc;
        }

        private void DrawAALine (Vector3 a, Vector3 b, float width)
        {
            polylinePointsTemp[0] = a;
            polylinePointsTemp[1] = b;
            Handles.DrawAAPolyLine (width, polylinePointsTemp);
        }

        private void DrawAALine (Vector3 a, Vector3 b, float width, Color color)
        {
            var hc = Handles.color;
            Handles.color = color;

            polylinePointsTemp[0] = a;
            polylinePointsTemp[1] = b;
            Handles.DrawAAPolyLine (width, polylinePointsTemp);

            Handles.color = hc;
        }

        private void DrawZTestWireCube (Vector3 center, Color colorMain, Color colorCulled, float size = 3f)
        {

            var hc = Handles.color;
            var zt = Handles.zTest;

            Handles.zTest = CompareFunction.LessEqual;
            Handles.color = colorMain;
            Handles.DrawWireCube (center, Vector3.one * size);

            Handles.zTest = CompareFunction.Greater;
            Handles.color = colorCulled;
            Handles.DrawWireCube (center, Vector3.one * size);

            Handles.color = hc;
            Handles.zTest = zt;
        }

        private void DrawZTestLine (Vector3 a, Vector3 b, Color colorMain) => DrawZTestLine (a, b, colorMain, colorMain.WithAlpha (0.5f));
        private void DrawZTestLine (Vector3 a, Vector3 b, Color colorMain, Color colorCulled)
        {

            var hc = Handles.color;
            var zt = Handles.zTest;

            Handles.zTest = CompareFunction.LessEqual;
            Handles.color = colorMain;
            Handles.DrawLine (a, b);

            Handles.zTest = CompareFunction.Greater;
            Handles.color = colorCulled;
            Handles.DrawLine (a, b);

            Handles.color = hc;
            Handles.zTest = zt;
        }

        private void DrawZTestCube (Vector3 center, Quaternion rotation, float size, Color colorMain, Color colorCulled)
        {
            var hc = Handles.color;
            var zt = Handles.zTest;

            Handles.zTest = CompareFunction.LessEqual;
            Handles.color = colorMain;
            Handles.CubeHandleCap (0, center, rotation, size, EventType.Repaint);

            Handles.zTest = CompareFunction.Greater;
            Handles.color = colorCulled;
            Handles.CubeHandleCap(0, center, rotation, size, EventType.Repaint);

            Handles.color = hc;
            Handles.zTest = zt;
        }

        private void DrawZTestRect (Vector3 center, Quaternion rotation, float size, Color colorMain, Color colorCulled)
        {
            var hc = Handles.color;
            var zt = Handles.zTest;

            Handles.zTest = CompareFunction.LessEqual;
            Handles.color = colorMain;
            Handles.RectangleHandleCap(0, center, rotation, size, EventType.Repaint);

            Handles.zTest = CompareFunction.Greater;
            Handles.color = colorCulled;
            Handles.RectangleHandleCap (0, center, rotation, size, EventType.Repaint);

            Handles.color = hc;
            Handles.zTest = zt;
        }



        public void EditRoad (int indexStart, KeyCode mouseButton)
        {
            var pointStart = am.points[indexStart];
            var operation = AreaManager.RoadEditingOperation.None;
            if (mouseButton == KeyCode.Mouse0)
                operation = AreaManager.RoadEditingOperation.Add;
            else if (mouseButton == KeyCode.Mouse1)
                operation = AreaManager.RoadEditingOperation.Remove;
            else if (mouseButton == KeyCode.PageUp)
                operation = AreaManager.RoadEditingOperation.SubtypeNext;
            else if (mouseButton == KeyCode.PageDown)
                operation = AreaManager.RoadEditingOperation.SubtypePrev;

            am.EditRoad (indexStart, operation, AreaManager.editingVolumeBrush);
        }



		HSBColor CalculateRequiredColorShift(HSBColor from, HSBColor to)
		{
			HSBColor shift = new HSBColor();

			var deltaH = to.h - from.h + 1f;
			var deltaS = to.s - from.s;
			var deltaB = to.b - from.b;

			shift.h = deltaH - Mathf.Floor(deltaH);
			shift.s = deltaS * 0.5f + 0.5f;
			shift.b = deltaB * 0.5f + 0.5f;

			return shift;
		}

		HSBColor ApplyColorShift(HSBColor from, HSBColor shift)
		{
			HSBColor result = from;

			result.h += shift.h;
			result.s += (shift.s - 0.5f) * 2f;
			result.b += (shift.b - 0.5f) * 2f;

			result.h = result.h - Mathf.Floor(result.h);
			result.s = Mathf.Clamp01(result.s);
			result.b = Mathf.Clamp01(result.b);

			return result;
		}

        public void EditColor (int spotIndex, KeyCode mouseButton)
        {
            if (!spotIndex.IsValidIndex (am.points))
                return;

            AreaVolumePoint startingPoint = am.points[spotIndex];
            if (mouseButton == KeyCode.Mouse0)
            {
                List<AreaVolumePoint> pointsActedOn = new List<AreaVolumePoint> ();
                CheckSearchRequirements (startingPoint, ref pointsActedOn);
                if (pointsActedOn == null)
                    return;

                if (pointsActedOn.Count > 1)
                    Debug.Log ("AM (I) | EditColor | Points acted on: " + pointsActedOn.Count);

                for (int i = 0; i < pointsActedOn.Count; ++i)
                {
	                var properties = pointsActedOn[i].customization;

					if (applyOverlaysOnColorApply)
						properties.overrideIndex = overrideValue;

                    if (applyMainOnColorApply)
                    {
                        properties.huePrimary = selectedPrimaryColor.h;
                        properties.saturationPrimary = selectedPrimaryColor.s;
                        properties.brightnessPrimary = selectedPrimaryColor.b;
                        properties.hueSecondary = selectedSecondaryColor.h;
                        properties.saturationSecondary = selectedSecondaryColor.s;
                        properties.brightnessSecondary = selectedSecondaryColor.b;
                    }

					am.ApplyShaderPropertiesAtPoint (pointsActedOn[i], properties, true, false, false);
                }
            }
            else if (mouseButton == KeyCode.Mouse2)
            {
				selectedPrimaryColor = new HSBColor(startingPoint.customization.huePrimary, startingPoint.customization.saturationPrimary, startingPoint.customization.brightnessPrimary);
				selectedSecondaryColor = new HSBColor(startingPoint.customization.hueSecondary, startingPoint.customization.saturationSecondary, startingPoint.customization.brightnessSecondary);

				selectedTilesetId = startingPoint.blockTileset;
				overrideValue = startingPoint.customization.overrideIndex;

                lastSearchResults.Clear ();
                lastSearchOrigin = null;
            }
        }

        private void GetPointsInColumnRecursive (List<AreaVolumePoint> results, AreaVolumePoint currentPoint, bool up)
        {
            if (results == null || currentPoint == null)
                return;

            AreaVolumePoint nextPoint = null;
            if (!results.Contains (currentPoint))
                results.Add (currentPoint);

            if (up)
            {
                AreaVolumePoint pointAbove = currentPoint.pointsWithSurroundingSpots[3];
                if (pointAbove != null && pointAbove.spotConfiguration != 0)
                    nextPoint = pointAbove;
            }
            else
            {
                AreaVolumePoint pointBelow = currentPoint.pointsInSpot[4];
                if (pointBelow != null && pointBelow.spotConfiguration != 0)
                    nextPoint = pointBelow;
            }

            if (nextPoint != null)
                GetPointsInColumnRecursive (results, nextPoint, up);
        }

        private void GetPointsAtHeight (List<AreaVolumePoint> results, AreaVolumePoint startingPoint)
        {
            if (results == null || startingPoint == null)
                return;

            int height = startingPoint.pointPositionIndex.y;
            for (int i = 0; i < am.points.Count; ++i)
            {
                AreaVolumePoint point = am.points[i];
                if (point.pointPositionIndex.y == height)
                    results.Add (point);
            }
        }

        // Fetch 3 surrounding points (exclude direction you came from)
        // For each result:
        // - check if configuration isn't 0
        // - check if it's not present in the results array yet
        // - add it to results array

        // AreaVolumePoint neighbour reference arrays:

        //    X ---> XZ
        //   /|     /|
        //  0 ---> Z |
        //  | |    | |
        //  | YX --|YXZ
        //  |/     |/
        //  Y ---> YZ

        //        0    1   2    3   4    5    6    7
        // [8]: this, +X, +Z, +XZ, +Y, +YX, +YZ, +XYZ
        // AreaVolumePoint[] spotPoints;

        //   YZ <--- Y
        //   /|     /|
        // YXZ <-- YX|
        //  | |    | |
        //  | Z <--| 0
        //  |/     |/
        // XZ <--- X

        //        0    1    2    3    4   5   6   7
        // [8]: -XYZ, -YZ, -XY, -Y, -XZ, -Z, -X, this
        // AreaVolumePoint[] spotsAroundThisPoint;

        private enum Direction
        {
            XPos,
            XNeg,
            YPos,
            YNeg,
            ZPos,
            ZNeg,
        }

        private static readonly byte[] maskForFloorTermination_XPos = new byte[] { 1, 9, 8, 17, 153, 136 };
        private static readonly byte[] maskForFloorTermination_ZPos = new byte[] { 8, 12, 4, 136, 204, 68 };
        private static readonly byte[] maskForFloorTermination_XNeg = new byte[] { 4, 6, 2, 68, 102, 34 };
        private static readonly byte[] maskForFloorTermination_ZNeg = new byte[] { 2, 3, 1, 34, 51, 17 };

        private void GetConnectedPointsAtHeight (List<AreaVolumePoint> results, AreaVolumePoint startingPoint)
        {
            if (results == null || startingPoint == null)
                return;

            bool allowFloor = startingPoint.spotConfiguration == (byte)15;

            results.Add (startingPoint);
            GetPointsInFloorRecursively (results, startingPoint, Direction.XNeg, allowFloor);
            GetPointsInFloorRecursively (results, startingPoint, Direction.ZNeg, allowFloor);
            GetPointsInFloorRecursively (results, startingPoint, Direction.XPos, allowFloor);
            GetPointsInFloorRecursively (results, startingPoint, Direction.ZPos, allowFloor);
        }

        private void TestPointFetch (AreaVolumePoint currentPoint)
        {
            if (currentPoint == null)
                return;

            Debug.Log ("Starting point | Index: " + currentPoint.spotIndex + " | Configuration: " + currentPoint.spotConfiguration + " / " + TilesetUtility.GetStringFromConfiguration (currentPoint.spotConfiguration));

            AreaVolumePoint pointXPos = currentPoint.pointsInSpot[1];
            if (pointXPos != null)
                Debug.Log ("X pos. (X+) | Index: " + pointXPos.spotIndex + " | Configuration: " + pointXPos.spotConfiguration + " / " + TilesetUtility.GetStringFromConfiguration (pointXPos.spotConfiguration));

            AreaVolumePoint pointZPos = currentPoint.pointsInSpot[2];
            if (pointZPos != null)
                Debug.Log ("Z pos. (Z+) | Index: " + pointZPos.spotIndex + " | Configuration: " + pointZPos.spotConfiguration + " / " + TilesetUtility.GetStringFromConfiguration (pointZPos.spotConfiguration));

            AreaVolumePoint pointXNeg = currentPoint.pointsWithSurroundingSpots[6];
            if (pointXNeg != null)
                Debug.Log ("X neg. (X-) | Index: " + pointXNeg.spotIndex + " | Configuration: " + pointXNeg.spotConfiguration + " / " + TilesetUtility.GetStringFromConfiguration (pointXNeg.spotConfiguration));

            AreaVolumePoint pointZNeg = currentPoint.pointsWithSurroundingSpots[5];
            if (pointZNeg != null)
                Debug.Log ("Z neg. (Z-) | Index: " + pointZNeg.spotIndex + " | Configuration: " + pointZNeg.spotConfiguration + " / " + TilesetUtility.GetStringFromConfiguration (pointZNeg.spotConfiguration));
        }

        private void GetPointsInFloorRecursively (List<AreaVolumePoint> results, AreaVolumePoint currentPoint, Direction directionOfPreviousPoint, bool allowFloor)
        {
            if (results == null || currentPoint == null)
                return;

            AreaVolumePoint pointXPos = null;
            bool allowFloorFurtherOnXPos = false;
            if (directionOfPreviousPoint != Direction.XPos)
                pointXPos = FillFromCandidate (results, currentPoint.pointsInSpot[1], Direction.XPos, allowFloor, out allowFloorFurtherOnXPos);

            AreaVolumePoint pointZPos = null;
            bool allowFloorFurtherOnZPos = false;
            if (directionOfPreviousPoint != Direction.ZPos)
                pointZPos = FillFromCandidate (results, currentPoint.pointsInSpot[2], Direction.ZPos, allowFloor, out allowFloorFurtherOnZPos);

            AreaVolumePoint pointXNeg = null;
            bool allowFloorFurtherOnXNeg = false;
            if (directionOfPreviousPoint != Direction.XNeg)
                pointXNeg = FillFromCandidate (results, currentPoint.pointsWithSurroundingSpots[6], Direction.XNeg, allowFloor, out allowFloorFurtherOnXNeg);

            AreaVolumePoint pointZNeg = null;
            bool allowFloorFurtherOnZNeg = false;
            if (directionOfPreviousPoint != Direction.ZNeg)
                pointZNeg = FillFromCandidate (results, currentPoint.pointsWithSurroundingSpots[5], Direction.ZNeg, allowFloor, out allowFloorFurtherOnZNeg);

            if (pointXPos != null)
                GetPointsInFloorRecursively (results, pointXPos, Direction.XNeg, allowFloorFurtherOnXPos);

            if (pointZPos != null)
                GetPointsInFloorRecursively (results, pointZPos, Direction.ZNeg, allowFloorFurtherOnZPos);

            if (pointXNeg != null)
                GetPointsInFloorRecursively (results, pointXNeg, Direction.XPos, allowFloorFurtherOnXNeg);

            if (pointZNeg != null)
                GetPointsInFloorRecursively (results, pointZNeg, Direction.ZPos, allowFloorFurtherOnZNeg);
        }

        private AreaVolumePoint FillFromCandidate (List<AreaVolumePoint> results, AreaVolumePoint candidate, Direction direction, bool allowFloor, out bool allowFloorFurther)
        {
            if (candidate == null)
            {
                allowFloorFurther = false;
                return null;
            }

            if (candidate.spotConfiguration != 15 && allowFloor)
                allowFloorFurther = false;
            else
                allowFloorFurther = allowFloor;

            if (candidate.spotConfiguration == 15 && !allowFloor)
                return null;

            if (candidate.spotConfiguration != 0 && !results.Contains (candidate))
            {
                results.Add (candidate);

                // We will null the candidate, preventing further recursive traversal
                // from that point on, if a point is an outer wall

                if (direction == Direction.XPos)
                {
                    for (int i = 0; i < maskForFloorTermination_XPos.Length; ++i)
                    {
                        if (candidate.spotConfiguration == maskForFloorTermination_XPos[i])
                            return null;
                    }
                }
                else if (direction == Direction.ZPos)
                {
                    for (int i = 0; i < maskForFloorTermination_ZPos.Length; ++i)
                    {
                        if (candidate.spotConfiguration == maskForFloorTermination_ZPos[i])
                            return null;
                    }
                }
                else if (direction == Direction.XNeg)
                {
                    for (int i = 0; i < maskForFloorTermination_XNeg.Length; ++i)
                    {
                        if (candidate.spotConfiguration == maskForFloorTermination_XNeg[i])
                            return null;
                    }
                }
                else if (direction == Direction.ZNeg)
                {
                    for (int i = 0; i < maskForFloorTermination_ZNeg.Length; ++i)
                    {
                        if (candidate.spotConfiguration == maskForFloorTermination_ZNeg[i])
                            return null;
                    }
                }

                return candidate;
            }
            else
                return null;
        }

        /*
        private void GetPointsInFloor (List<AreaVolumePoint> results, AreaVolumePoint currentPoint)
        {
            List<AreaVolumePoint> pointsOnZ = new List<AreaVolumePoint> ();
            GetPointsInRowZRecursively (pointsOnZ, currentPoint, true);
            GetPointsInRowZRecursively (pointsOnZ, currentPoint, false);

            for (int i = 0; i < pointsOnZ.Count; ++i)
            {
                AreaVolumePoint pointOnZ = pointsOnZ[i];
                GetPointsInRowXRecursively (results, pointOnZ, true);
                GetPointsInRowXRecursively (results, pointOnZ, false);
            }
        }
        */

        private static int[] reusedIntArray_PropPlacement;

        public void EditProp (int indexUnderCursor, KeyCode mouseButton, bool shift)
        {
            if (!indexUnderCursor.IsValidIndex (am.points))
            {
                Debug.Log ("AM (I) | EditProp | Early exit due to invalid index: " + indexUnderCursor);
                return;
            }

            if (AreaAssetHelper.propsPrototypesList == null || AreaAssetHelper.propsPrototypesList.Count == 0)
            {
                Debug.Log ("AM (I) | EditProp | Early exit due to missing prop library");
                return;
            }

            propPlacementListIndex = indexUnderCursor;
            List<AreaPlacementProp> placements = am.indexesOccupiedByProps.ContainsKey (indexUnderCursor) ? am.indexesOccupiedByProps[indexUnderCursor] : null;

            if (propEditingMode == PropEditingMode.Place)
            {
                // Add prop to cell
                if (mouseButton == KeyCode.Mouse0)
                {
                    Debug.Log
                    (
                        "AM (I) | EditProp | " + mouseButton +
                        " | Attempting to place new prop at index " + indexUnderCursor +
                        ", placements there: " + (placements == null ? "none" : placements.Count.ToString ())
                    );

                    AreaVolumePoint pointTargeted = am.points[indexUnderCursor];
                    AreaPlacementProp placement = new AreaPlacementProp ();
                    AreaPropPrototypeData prototype = AreaAssetHelper.GetPropPrototype (propSelectionID);

                    if (prototype == null)
                    {
                        prototype = AreaAssetHelper.propsPrototypesList[0];
                        propSelectionID = prototype.id;
                    }

                    placement.id = propSelectionID;
                    placement.pivotIndex = indexUnderCursor;
                    placement.rotation = propRotation;
                    placement.flipped = propFlipped;
                    placement.offsetX = propOffsetX;
                    placement.offsetZ = propOffsetZ;
                    placement.hsbPrimary = spawnPropsWithClipboardColor ? clipboardPropHSBPrimary : propHSBPrimary;
                    placement.hsbSecondary = spawnPropsWithClipboardColor ? clipboardPropHSBSecondary : propHSBSecondary;

                    int index = am.placementsProps.Count;

                    if (am.IsPropPlacementValid (placement, pointTargeted, prototype, checkPropConfiguration))
                    {
                        if (!am.indexesOccupiedByProps.ContainsKey (indexUnderCursor))
                        {
                            am.indexesOccupiedByProps.Add (indexUnderCursor, new List<AreaPlacementProp> ());
                            placements = am.indexesOccupiedByProps[indexUnderCursor];
                        }

                        am.indexesOccupiedByProps[indexUnderCursor].Add (placement);
                        am.placementsProps.Add (placement);

                        am.ExecutePropPlacement (placement);

                        if (spawnPropsWithAutorotation)
                            am.SnapPropRotation (placement);
                    }

                    if (placement.prototype != null && placement.prototype.prefab != null)
                    {
                        var lights = placement.prototype.prefab.activeLights;
                        bool lightPresent = lights != null && lights.Count > 0;
                        if (lightPresent && CombatSceneHelper.ins != null)
                        {
                            var lightHelper = CombatSceneHelper.ins.ambientLight;
                            lightHelper.OnLevelLoad ();
                        }
                    }
                }
                // Copy prop
				if (mouseButton == KeyCode.Mouse1 && shift)
				{
                    if (placements != null && placements.Count > 0)
					{
						// By default choose the first prop in the cell
                        var propToCopy = placements[0];
                        // If user has a prop selected, check if it's on the same cell and then copy that prop
                        for (int i = 0; i < placements.Count; ++i)
                        {
                            if (placements[i] == propPlacementHandled)
                            {
                                propToCopy = placements[i];
                                break;
                            }
                        }

						propSelectionID = propToCopy.prototype.id;
						propIndex = AreaAssetHelper.propsPrototypesList.IndexOf (propToCopy.prototype);

						propRotation = propToCopy.rotation;
						propFlipped = propToCopy.flipped;

						propOffsetX = propToCopy.offsetX;
						propOffsetZ = propToCopy.offsetZ;
                        propHSBPrimary = propToCopy.hsbPrimary;
                        propHSBSecondary = propToCopy.hsbSecondary;
					}
				}
                // Remove prop
                if (mouseButton == KeyCode.Mouse2)
                {
                    if (placements != null && placements.Count > 0)
                    {
						// By default choose the first prop in the cell
                        var propToDelete = placements[0];
                        // If user has a prop selected, check if it's on the same cell and then delete that prop
                        if (propPlacementHandled != null)
                        {
                            for (int i = 0; i < placements.Count; ++i)
                            {
                                if (placements[i] == propPlacementHandled)
                                {
                                    propToDelete = placements[i];
                                    break;
                                }
                            }
                        }

                        am.RemovePropPlacement (propToDelete);

                        var lights = propToDelete.prototype.prefab.activeLights;
                        bool lightPresent = lights != null && lights.Count > 0;
                        if (lightPresent && CombatSceneHelper.ins != null)
                        {
                            var lightHelper = CombatSceneHelper.ins.ambientLight;
                            lightHelper.OnLevelLoad ();
                        }
                    }
                }
            }
            else if (propEditingMode == PropEditingMode.Color)
            {
                if (placements == null || placements.Count == 0)
                    return;

                AreaPlacementProp placementActedOn = null;
                bool warn = false;
                for (int i = 0; i < placements.Count; ++i)
                {
                    if (placements[i] != null && placements[i].prototype != null && placements[i].prototype.prefab.allowTinting)
                    {
                        if (placementActedOn == null)
                            placementActedOn = placements[i];
                        else
                            warn = true;
                    }
                }

                if (warn)
                    Debug.LogWarning ("AM (I) | EditProp | More than one colorable prop occupies this cell, selecting the first one: " + placementActedOn.prototype.name);

                if (placementActedOn == null)
                {
                    Debug.LogWarning ("AM (I) | EditProp | No placements were found on the current point, aborting");
                    return;
                }

                if (mouseButton == KeyCode.Mouse0)
                {
                    placementActedOn.UpdateMaterial_HSBOffsets (clipboardPropHSBPrimary, clipboardPropHSBSecondary);
                }
                else if (mouseButton == KeyCode.Mouse1)
                {
                    placementActedOn.UpdateMaterial_HSBOffsets (Constants.defaultHSBOffset, Constants.defaultHSBOffset);
                }
                else if (mouseButton == KeyCode.Mouse2)
                {
                    clipboardPropHSBPrimary = placementActedOn.hsbPrimary;
                    clipboardPropHSBSecondary = placementActedOn.hsbSecondary;
                }
            }
        }

        public void EditNavigation (int spotIndex, KeyCode mouseButton)
        {
            AreaVolumePoint point = am.points[spotIndex];

            if (mouseButton == KeyCode.Mouse0)
            {
                if (!am.navOverridesSaved.ContainsKey (spotIndex))
                {
                    am.navOverridesSaved.Add (spotIndex, new AreaDataNavOverride { pivotIndex = spotIndex, offsetY = 0f });
                    am.GenerateNavOverrides ();
                }
            }

            else if (mouseButton == KeyCode.Mouse1)
            {
                if (am.navOverridesSaved.ContainsKey (spotIndex))
                {
                    am.navOverridesSaved.Remove (spotIndex);
                    am.GenerateNavOverrides ();
                }
            }

            else if (mouseButton == KeyCode.PageDown || mouseButton == KeyCode.PageUp)
            {
                if (am.navOverridesSaved.ContainsKey (spotIndex))
                {
                    var forward = mouseButton == KeyCode.PageUp;
                    var offsetYModified = Mathf.Clamp (am.navOverridesSaved[spotIndex].offsetY + (forward ? 1f : -1f) * 0.1f, -1.5f, 1.5f);
                    am.navOverridesSaved[spotIndex] = new AreaDataNavOverride { pivotIndex = spotIndex, offsetY = offsetYModified };
                }
            }
        }

        public void SetEditingTarget (AreaManager am, EditingTarget value)
        {
            editingTarget = value;
        }





        private void OnHoverEnd (AreaManager am)
        {
            //if (brushVisualHolder != null)
            //   DestroyImmediate (brushVisualHolder);

            //if (brushVisualPrefab != null)
            //    DestroyImmediate (brushVisualPrefab);

            if (propHolder != null)
                DestroyImmediate (propHolder);

            if (propCursorInstance != null)
                DestroyImmediate (propCursorInstance);

            if (transferPreviewInstance != null)
                DestroyImmediate (transferPreviewInstance);

            if (rcCursorObject != null)
                rcCursorObject.SetActive (false);

            if (rcSelectionObject != null)
                rcSelectionObject.SetActive (false);

            if (rcGlowObject != null)
                rcGlowObject.SetActive (false);

            //transferPreviewVersion += 1;
        }

        private static Material GetMaterial (ref Material material, Color color, int mode)
        {
            if (material == null)
            {
                material = new Material (AssetDatabase.GetBuiltinExtraResource<Material> ("Default-Material.mat"));
                material.SetFloat ("_Mode", (float)mode);
                if (mode == 2)
                {
                    material.SetInt ("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt ("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt ("_ZWrite", 0);
                    material.DisableKeyword ("_ALPHATEST_ON");
                    material.DisableKeyword ("_ALPHABLEND_ON");
                    material.EnableKeyword ("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 3000;
                }
                material.SetColor ("_Color", color);
                return material;
            }
            return material;
        }



        //private GameObject brushVisualHolder;
        //private GameObject brushVisualPrefab;

        //private Material brushMaterialVolumeEmpty = null;
        //private Material brushMaterialVolumePivot = null;
        //private Material brushMaterialVolumeFull = null;
        //private Material brushMaterialPrefab = null;

        //private int brushDrawRotation = 0;
        //private bool brushRedrawRequired = true;
        //private bool brushDrawPrefab = true;

        private void OnDestroy ()
        {
            EditorApplication.update -= UpdateInEditorApplication;

            //if (brushVisualHolder != null)
            //    DestroyImmediate (brushVisualHolder);

            if (propHolder != null)
                DestroyImmediate (propHolder);

            if (propCursorInstance != null)
                DestroyImmediate (propCursorInstance);

            if (transferPreviewInstance != null)
                DestroyImmediate (transferPreviewInstance);

            if (holderSpawns != null)
                DestroyImmediate (holderSpawns.gameObject);

            if (rcCursorObject != null)
                DestroyImmediate (rcCursorObject);

            if (rcSelectionObject != null)
                DestroyImmediate (rcSelectionObject);

            if (rcGlowObject != null)
                DestroyImmediate (rcGlowObject);
        }

        private void ValidateFlags (AreaManager am)
        {
            int counter = 0;
            foreach (KeyValuePair<int, AreaTileset> kvp in AreaTilesetHelper.database.tilesets)
            {
                for (int i = 0; i < kvp.Value.blocks.Length; ++i)
                {
                    AreaBlockDefinition blockDefinition = kvp.Value.blocks[i];
                    foreach (KeyValuePair<byte, SortedDictionary<byte, GameObject>> kvpGroup in blockDefinition.subtypeGroups)
                    {
                        SortedDictionary<byte, GameObject> group = kvpGroup.Value;
                        foreach (KeyValuePair<byte, GameObject> kvpSubtype in group)
                        {
                            GameObject subtype = kvpSubtype.Value;
                            if (subtype.hideFlags != HideFlags.None)
                            {
                                counter += 1;
                                Debug.Log ("Found a bad flag " + counter);
                                subtype.hideFlags = HideFlags.None;
                            }
                        }
                    }
                }
            }
        }



        private static GameObject transferPreviewInstance = null;
        //private int transferPreviewVersion = 0;
        //private int transferPreviewVersionVisualized = -1;
        private static List<AreaVolumePoint> transferPointsAffected;
        private static Vector3[] transferPreviewVerts = new Vector3[4];

        private void HideTransferPreview ()
        {
            if (transferPreviewInstance != null)
                transferPreviewInstance.gameObject.SetActive (false);
        }

        private void VisualizeTransferPreview ()
        {
            return;
            /*
            if
            (
                am.clipboardPointsSaved == null ||
                am.clipboardPointsSaved.Count == 0 ||
                am.targetOrigin == Vector3Int.size0x0x0
            )
            {
                // Debug.Log ("B1");
                HideTransferPreview ();
                return;
            }

            if
            (
                transferPreviewInstance != null &&
                transferPreviewVersionVisualized == transferPreviewVersion
            )
            {
                // Debug.Log ("B3");
                return;
            }

            transferPreviewVersionVisualized = transferPreviewVersion;
            if (transferPreviewInstance == null)
                transferPreviewInstance = new GameObject ("TransferPreviewInstance");
            else
                UtilityGameObjects.ClearChildren (transferPreviewInstance);

            // Bounds are not positions, they are limits, like array sizes - so we need to subtract 1 from bounds axes to get second corner
            Vector3Int cornerA = am.targetOrigin;
            Vector3Int cornerB = cornerA + am.clipboardBoundsSaved + Vector3Int.size1x1x1Neg;

            int indexA = AreaUtility.GetIndexFromInternalPosition (cornerA, am.boundsFull);
            int indexB = AreaUtility.GetIndexFromInternalPosition (cornerB, am.boundsFull);

            if (indexA == -1 || indexB == -1)
            {
                Debug.LogWarning (string.Format
                (
                    "PasteVolume | Failed to paste due to specified corner {0} or calculated corner {1} falling outside of target level bounds {2}",
                    cornerA,
                    cornerB,
                    am.boundsFull
                ));
                return;
            }

            // Since we are directly modifying boundary points, some spots outside of captured area would be affected - time to collect all points
            int affectedOriginX = Mathf.Max (0, cornerA.x - 1);
            int affectedOriginZ = Mathf.Max (0, cornerA.z - 1);
            var cornerAShifted = new Vector3Int (affectedOriginX, 0, affectedOriginZ);

            // Bounds are limits, like array sizes - so they will be bigger by 1 than just pure position difference
            // For example, a set of points [(0,0);(1,0);(0,1);(1;1)] has bounds of 2x2, not 1x1!
            int affectedBoundsX = cornerB.x - cornerAShifted.x + 1;
            int affectedBoundsZ = cornerB.z - cornerAShifted.z + 1;
            int affectedVolumeLength = affectedBoundsX * am.clipboardBoundsSaved.y * affectedBoundsZ;
            var affectedBounds = new Vector3Int (affectedBoundsX, am.clipboardBoundsSaved.y, affectedBoundsZ);
            transferPointsAffected = new List<AreaVolumePoint> (affectedVolumeLength);

            for (int i = 0; i < affectedVolumeLength; ++i)
            {
                Vector3Int affectedPointPosition = AreaUtility.GetVolumePositionFromIndex (i, affectedBounds);
                Vector3Int sourcePointPosition = affectedPointPosition + cornerAShifted;
                int sourcePointIndex = AreaUtility.GetIndexFromInternalPosition (sourcePointPosition, am.boundsFull);
                var sourcePoint = am.points[sourcePointIndex];
                transferPointsAffected.Add (sourcePoint);
                // Debug.DrawLine (sourcePoint.pointPositionLocal, sourcePoint.instancePosition, Color.white, 10f);
            }

            for (int i = 0; i < am.clipboardPointsSaved.Count; ++i)
            {
                var clipboardPoint = am.clipboardPointsSaved[i];
                var targetPointPosition = clipboardPoint.pointPositionIndex + cornerA;
                int targetPointIndex = AreaUtility.GetIndexFromInternalPosition (targetPointPosition, am.boundsFull);
                var targetPoint = am.points[targetPointIndex];
                if (clipboardPoint.pointState == AreaVolumePointState.Empty)
                    continue;

                var go = PrimitiveHelper.CreatePrimitive (PrimitiveType.Cube, false);
                go.transform.localScale = Vector3.one * 3.05f;
                go.transform.localPosition = targetPoint.pointPositionLocal;
                go.transform.parent = transferPreviewInstance.transform;
            }

            Material material = PrimitiveHelper.GetDefaultMaterial ();
            UtilityMaterial.SetMaterialsOnRenderers (transferPreviewInstance.gameObject, material);
            */
        }

        private void SwapRoadGrass ()
        {
            if (am == null)
                return;

            for (int i = 0; i < am.points.Count; ++i)
            {
                var point = am.points[i];
                if (!point.spotPresent || point.spotConfiguration != AreaNavUtility.configFloor)
                    continue;

                if (point.blockTileset != AreaTilesetHelper.idOfRoad)
                    continue;

                if (point.blockGroup != 1 || point.blockSubtype != 0)
                    continue;

                point.blockTileset = AreaTilesetHelper.idOfForest;
                point.blockGroup = 0;
                point.blockSubtype = 0;
                am.RebuildBlock (point);
            }
        }

        public void RemoveCityCrossings ()
        {
            for (int i = am.placementsProps.Count - 1; i >= 0; --i)
            {
                var placement = am.placementsProps[i];
                if (placement.id == 6703 || placement.id == 6702)
                    am.RemovePropPlacement (placement);
            }

            am.RebuildEverything ();
        }

        private void GenerateProps (float genChance, List<int> propIdList, bool requireFlatSurroundings, float offsetLimit)
        {
            if (am == null || propIdList.Count <= 0)
                return;

            bool offsetUsed = offsetLimit > 0f;

            for (int i = 0; i < am.points.Count; ++i)
            {
                var point = am.points[i];
                if (!point.spotPresent || point.spotConfiguration != AreaNavUtility.configFloor)
                    continue;

                if (am.indexesOccupiedByProps.ContainsKey (point.spotIndex))
                    continue;

                if (point.blockTileset != AreaTilesetHelper.idOfForest && point.blockTileset != AreaTilesetHelper.idOfTerrain)
                    continue;

                if (requireFlatSurroundings)
                {
                    AreaVolumePoint pointNeighbourXPos = point.pointsInSpot[1];
                    AreaVolumePoint pointNeighbourZPos = point.pointsInSpot[2];
                    AreaVolumePoint pointNeighbourXNeg = point.pointsWithSurroundingSpots[6];
                    AreaVolumePoint pointNeighbourZNeg = point.pointsWithSurroundingSpots[5];

                    bool surroundedByFloor =
                    (
                        (pointNeighbourXPos == null || (pointNeighbourXPos.spotPresent && pointNeighbourXPos.spotConfiguration == AreaNavUtility.configFloor)) &&
                        (pointNeighbourZPos == null || (pointNeighbourZPos.spotPresent && pointNeighbourZPos.spotConfiguration == AreaNavUtility.configFloor)) &&
                        (pointNeighbourXNeg == null || (pointNeighbourXNeg.spotPresent && pointNeighbourXNeg.spotConfiguration == AreaNavUtility.configFloor)) &&
                        (pointNeighbourZNeg == null || (pointNeighbourZNeg.spotPresent && pointNeighbourZNeg.spotConfiguration == AreaNavUtility.configFloor))
                    );

                    if (!surroundedByFloor)
                        continue;
                }

				var propId = propIdList[Random.Range (0, propIdList.Count)];
				var prototype = AreaAssetHelper.GetPropPrototype (propId);
                if (prototype == null)
                    continue;

                float chance = Random.Range (0f, 1f);
                if (chance > genChance)
                    continue;

                AreaPlacementProp placement = new AreaPlacementProp ();
                placement.id = prototype.id;
                placement.pivotIndex = point.spotIndex;
                placement.offsetX = placement.offsetZ = 0f;
                placement.rotation = propRotation;
                placement.flipped = propFlipped;
                placement.hsbPrimary = Constants.defaultHSBOffset;
                placement.hsbSecondary = Constants.defaultHSBOffset;

                if (offsetUsed)
                {
                    placement.offsetX = Random.Range (-offsetLimit, offsetLimit);
                    placement.offsetZ = Random.Range (-offsetLimit, offsetLimit);
                }

                if (!am.indexesOccupiedByProps.ContainsKey (point.spotIndex))
                    am.indexesOccupiedByProps.Add (point.spotIndex, new List<AreaPlacementProp> ());

                am.indexesOccupiedByProps[point.spotIndex].Add (placement);
                am.placementsProps.Add (placement);
                am.ExecutePropPlacement (placement);
            }
        }

        private void VisualizeProp (Vector3 position)
        {
            if (am == null)
            {
                Debug.Log ("VisualizeProp - AM is null");
                return;
            }

            if (propHolder == null)
                propHolder = GameObject.Find ("AreaManager_PropPreviewHolder");

            if (propHolder == null)
            {
                propHolder = new GameObject ("AreaManager_PropPreviewHolder");
                propHolder.hideFlags = HideFlags.HideAndDontSave;
            }

            if (propCursorInstance == null && am.debugPropVisualCursor != null)
            {
                propCursorInstance = Instantiate (am.debugPropVisualCursor);
                propCursorInstance.hideFlags = HideFlags.HideAndDontSave;
            }

            if (propCursorInstance != null)
                propCursorInstance.transform.position = position;

            if
            (
                AreaAssetHelper.propsPrototypesList == null ||
                AreaAssetHelper.propsPrototypesList.Count == 0
            )
            {
                HideProp ();
                return;
            }

            int spotIndex = AreaUtility.GetIndexFromWorldPosition (position + spotRaycastHitOffset, am.GetHolderColliders ().position, am.boundsFull);
            if (spotIndex == -1)
            {
                HideProp ();
                Debug.Log("VisualizeProp - Point index not found");
                return;
            }

            AreaVolumePoint point = am.points[spotIndex];
            if
            (
                !point.spotPresent ||
                point.spotConfiguration == (byte)0 ||
                point.spotConfiguration == (byte)255
            )
            {
                HideProp ();
                Debug.Log("VisualizeProp - Point not present");
                return;
            }

            // Only re-instantiate prop preview when prop index changes
            if (propIndexVisualized != propIndex)
            {
                if (!propIndex.IsValidIndex (AreaAssetHelper.propsPrototypesList))
                {
                    propIndex = 0;
                    HideProp ();
                    Debug.Log("VisualizeProp - Prop index invalid, bailing on instantiation");
                    return;
                }

                AreaPropPrototypeData prototype = AreaAssetHelper.propsPrototypesList[propIndex];
                if (prototype == null)
                {
                    HideProp ();
                    Debug.Log("VisualizeProp - Prop prototype data is null, bailing on instantiation");
                    return;
                }

                UtilityGameObjects.ClearChildren (propHolder);
                propPreviewInstance = Instantiate (prototype.prefab.gameObject).GetComponent<AreaProp> ();
                propPreviewInstance.transform.name = prototype.prefab.transform.name;

                // Snap previewed prop rotation to tile's configuration
                if (prototype.prefab.linkRotationToConfiguration)
                {
                    var configurationMask = AreaAssetHelper.GetPropMask (prototype.prefab.compatibility);
                    int spotConfigurationIndexInMask = configurationMask.IndexOf (point.spotConfiguration);
                    bool spotConfigurationPresentInMask = spotConfigurationIndexInMask >= 0;
                    if (spotConfigurationPresentInMask)
                    {
                        propRotation = (byte)configurationMask.IndexOf (point.spotConfiguration);
                        if (propFlipped)
                            propRotation = (byte)((propRotation + 2) % 4);
                        // Debug.Log ($"Comp: {prototype.prefab.compatibility} | Prop mask: {configurationMask.ToStringFormatted ()} | Spot config: {point.spotConfiguration} | New rotation: {configurationMask.IndexOf (point.spotConfiguration)}");
                    }
                }

                propPreviewInstance.transform.rotation = Quaternion.Euler (new Vector3 (0f, -90f * propRotation, 0f));
                propPreviewInstance.transform.position = point.pointPositionLocal + new Vector3 (1f, -1f, 1f) * (TilesetUtility.blockAssetSize / 2f) + AreaUtility.GetPropOffsetAsVector(prototype, propOffsetX, propOffsetZ, propRotation, propPreviewInstance.transform.localRotation);
                propPreviewInstance.transform.localScale = prototype.prefab.mirrorOnZAxis ? new Vector3 (1f, 1f, propFlipped ? -1f : 1f) : new Vector3 (propFlipped ? -1f : 1f, 1f, 1f);
                propPreviewInstance.transform.localScale = prototype.prefab.scaleRandomly ? Vector3.Lerp (prototype.prefab.scaleMin, prototype.prefab.scaleMax, 0.5f) : propPreviewInstance.transform.localScale;
                propPreviewInstance.transform.parent = propHolder.transform;
                propPreviewInstance.gameObject.SetFlags (HideFlags.HideAndDontSave);

                if (propPreviewInstance.colliderMain != null)
                    propPreviewInstance.colliderMain.enabled = false;

                if (propPreviewInstance.collidersSecondary != null)
                {
                    foreach (var c in propPreviewInstance.collidersSecondary)
                    {
                        if (c != null)
                            c.enabled = false;
                    }
                }

                UpdatePropHSBOffsets (propPreviewInstance);

                propIndexVisualized = propIndex;
            }
            else
            {
                if (propPreviewInstance != null)
                {

                    if (propPreviewInstance.gameObject.activeSelf == false)
                        propPreviewInstance.gameObject.SetActive (true);

                    if
                    (
                        propSpotIndexVisualized != spotIndex ||
                        propRotationVisualized != propRotation ||
                        propFlippedVisualized != propFlipped ||
                        propOffsetXVisualized != propOffsetX ||
                        propOffsetZVisualized != propOffsetZ
                    )
                    {
                        AreaPropPrototypeData prototype = AreaAssetHelper.propsPrototypesList[propIndex];
                        if (prototype == null)
                        {
                            HideProp ();
                            return;
                        }

                        // Snap previewed prop rotation to tile's configuration
                        if (propPreviewInstance.linkRotationToConfiguration)
                        {
                            var configurationMask = AreaAssetHelper.GetPropMask (propPreviewInstance.compatibility);
                            int spotConfigurationIndexInMask = configurationMask.IndexOf (point.spotConfiguration);
                            bool spotConfigurationPresentInMask = spotConfigurationIndexInMask >= 0;
                            if (spotConfigurationPresentInMask)
                            {
                                propRotation = (byte)configurationMask.IndexOf (point.spotConfiguration);
                                if (propFlipped)
                                    propRotation = (byte)((propRotation + 2) % 4);
                                // Debug.Log ($"Comp: {prototype.prefab.compatibility} | Prop mask: {configurationMask.ToStringFormatted ()} | Spot config: {point.spotConfiguration} | New rotation: {configurationMask.IndexOf (point.spotConfiguration)}");
                            }
                        }

                        propPreviewInstance.transform.rotation = Quaternion.Euler (new Vector3 (0f, -90f * propRotation, 0f));
                        propPreviewInstance.transform.position = point.pointPositionLocal + new Vector3 (1f, -1f, 1f) * (TilesetUtility.blockAssetSize / 2f) + AreaUtility.GetPropOffsetAsVector(prototype, propOffsetX, propOffsetZ, propRotation, propPreviewInstance.transform.localRotation);
                        propPreviewInstance.transform.localScale = propPreviewInstance.mirrorOnZAxis ? new Vector3 (1f, 1f, propFlipped ? -1f : 1f) : new Vector3 (propFlipped ? -1f : 1f, 1f, 1f);
                        propPreviewInstance.transform.localScale = propPreviewInstance.scaleRandomly ? Vector3.Lerp (propPreviewInstance.scaleMin, propPreviewInstance.scaleMax, 0.5f) : propPreviewInstance.transform.localScale;
                    }

                    if
                    (
                        propHSBPrimaryVisualized != propHSBPrimary ||
                        propHSBSecondaryVisualized != propHSBSecondary
                    )
                    {
                        UpdatePropHSBOffsets (propPreviewInstance);
                    }
                }
                else
                {
                    propIndexVisualized = -1;
                }
            }

            propSpotIndexVisualized = spotIndex;
            propRotationVisualized = propRotation;
            propFlippedVisualized = propFlipped;
            propOffsetXVisualized = propOffsetX;
            propOffsetZVisualized = propOffsetZ;
        }

        private void HideProp ()
        {
            if (propPreviewInstance != null)
                propPreviewInstance.gameObject.SetActive (false);
        }





        private void UpdatePropHSBOffsets (AreaProp propPreviewInstance)
        {
            if (propPreviewInstance != null)
            {
                // Make sure preview prop has the correct tinting applied
                if (propPreviewInstance.allowTinting)
                {
                    if (_propBlockAcc == null)
                        _propBlockAcc = new MaterialPropertyBlock ();
                    var visPropRenderersNew = propPreviewInstance.renderers;
                    for (int i = 0; i < visPropRenderersNew.Count; ++i)
                    {
                        if (visPropRenderersNew[i].mode != AreaProp.RendererMode.ActiveWhenDestroyed)
                        {
                            _propBlockAcc.Clear ();
                            visPropRenderersNew[i].renderer.GetPropertyBlock (_propBlockAcc);
                            // Toggle HSV override to ignore instance array data
                            _propBlockAcc.SetFloat (ID_InstancePropsOverride, 1.0f);
                            // Set HSV offsets to preview prop colors
                            _propBlockAcc.SetVector (ID_HsbOffsetsPrimary, propHSBPrimary);
                            _propBlockAcc.SetVector (ID_HsbOffsetsSecondary, propHSBSecondary);
                            // Set packed prop data to defaults just in case
                            _propBlockAcc.SetVector (ID_PackedPropData, packedPropDataDefault);
                            visPropRenderersNew[i].renderer.SetPropertyBlock (_propBlockAcc);
                        }
                        else
                        {
                            // Additionally hide meshes showing destroyed state, since we are iterating on renderers
                            visPropRenderersNew[i].renderer.gameObject.SetActive (false);
                        }
                    }
                }
            }
        }







        private static Transform holderSpawns;

        private static string holderSpawnsName = "SceneHolder_Spawns";

        public Transform GetHolderSpawns () { return UtilityGameObjects.GetTransformSafely (ref holderSpawns, holderSpawnsName, HideFlags.DontSave, Vector3.zero, "SceneHolder"); }



        private void FillPropReplacementMap
        (
            Transform parent, Dictionary<AreaPropPrototypeData, List<Transform>> propToTransformMap, List<AreaPropPrototypeData> prototypes
        )
        {
            bool added = false;
            for (int i = 0; i < prototypes.Count; ++i)
            {
                AreaPropPrototypeData prop = prototypes[i];
                if (parent.name.Contains (prop.name))
                {
                    if (!propToTransformMap.ContainsKey (prop))
                        propToTransformMap.Add (prop, new List<Transform> ());

                    propToTransformMap[prop].Add (parent);
                    added = true;
                }
            }

            if (!added)
            {
                for (int i = 0; i < parent.childCount; ++i)
                    FillPropReplacementMap (parent.GetChild (i), propToTransformMap, prototypes);
            }
        }
    }
}
