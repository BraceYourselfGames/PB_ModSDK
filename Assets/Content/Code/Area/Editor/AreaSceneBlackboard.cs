using System.Collections.Generic;

using UnityEngine;

namespace Area
{
    using Scene;

    delegate void OnLevelLoaded();
    delegate void OnEditingModeChanged ();
    delegate void OnPropListChanged ();

    class AreaSceneBlackboard
    {
        public static void ClearPersistentClipboardInfo ()
        {
            clipboardSource = ClipboardSource.Scene;
            clipboardAreaKey = "";
            clipboardSnippetKey = "";
            #if PB_MODSDK
            clipboardModID = "";
            #endif
        }

        public OnLevelLoaded onLevelLoaded;
        public OnEditingModeChanged onEditingModeChanged;
        public OnPropListChanged onPropListChanged;

        public AreaManager am;
        public AreaSceneGizmos gizmos;
        public AreaSceneSpotInfo spotInfo;

        public EditingMode editingMode
        {
            get => editingModeInternal;
            set
            {
                editingModeInternal = value;
                onEditingModeChanged?.Invoke ();
            }
        }
        EditingMode editingModeInternal = EditingMode.Spot;

        public bool repaintScene;
        public bool brushChanged;

        public AreaTileset volumeTilesetSelected;
        public AreaTileset spotTilesetSelected;
        public readonly TilesetColorInfo tilesetColor = new TilesetColorInfo ();
        public readonly ClipboardTilesetInfo clipboardTilesetInfo = new ClipboardTilesetInfo();
        public int lastSpotTilesetID = -1;
        public string lastSpotInfoGroups = string.Empty;

        public readonly List<PropTableEntry> props = new List<PropTableEntry> ();
        public string propFilter
        {
            get => propFilterInternal;
            set
            {
                propFilterInternal = value;
                onPropListChanged?.Invoke ();
            }
        }
        string propFilterInternal;
        public readonly PropEditInfo propEditInfo = new PropEditInfo ();
        public PropEditingMode propEditingMode = PropEditingMode.Place;
        public readonly ClipboardPropColor clipboardPropColor = new ClipboardPropColor();
        public (float X, float Z) propOffset;
        public (Vector4 Primary, Vector4 Secondary) propColor;
        public PropEditCommand propEditCommand;
        public PropEditFunctions propEditFunctions;

        public bool hoverActive;
        public AreaVolumePoint lastPointHovered;
        public AreaVolumePoint lastSpotHovered;
        public SpotHoverType lastSpotType;
        public Vector3 pointerDirection;

        public SpotCursorType currentSpotCursorType;
        public bool enableCursorLogging;
        public bool showVolumeWireFrames;
        public bool enableVolumeLogging;
        public bool displayVolumeDebugInfo;
        public float volumeInteractionDistance = 100f;

        public bool enableTransferLogging;
        public bool groundSourceOriginOnClick = true;
        public bool targetOriginYCursorLock = true;
        public bool punchDownTerrainOnPaste;
        public bool checkPropCompatibilityOnPaste = true;
        // This information has to survive the AreaManager inspector being unloaded and reloaded
        // so that snippets can be copied across mods.
        public static ClipboardSource clipboardSource;
        public static string clipboardAreaKey;
        public static string clipboardSnippetKey;
        #if PB_MODSDK
        public static string clipboardModID;
        #endif

        public bool swapTilesetOnVolumeEdits;
        public bool overrideTerrainAndRoadTilesetsOnVolumeEdits = true;

        public bool propagateDestructibilityDown;
        public bool allowIndestructibleDestruction;
        public bool displayDestructibility = true;
        public bool enableDamageLogging;

        public bool checkPropConfiguration = true;
        public bool showOccludedPropHandles = true;
        public bool showPropListInPanel;

        // XXX not sure what toggles this boolean.
        public bool showLastSearchResults = true;
        public SpotSearchType currentSearchType;

        public bool vertexColorChanged;

        public float navLinkSeparation = 0.15f;
        public float navigationInteractionDistance = AreaSceneCamera.interactionDistanceNavigation;

        public int layer;
        public int selectionLayer;
        public AreaVolumePoint lastCellHovered;
        public AreaVolumePoint lastCellInspected;
        public bool cellEmpty = true;
        public bool showPropsOnLayer;
        public bool enableLayerDiagnostics;
        public bool enableLayerMeshLogging;
        public bool enableCapsuleScanLogToFile;
        public string capsuleScanLogFilePath;
        public bool capsuleScanTrace;
        public string capsuleScanTraceFilePath;
        public SpotChange spotChange;
        public AreaVolumePointState spotChangeState;
        public int spotChangeTileset;
        public bool rebuildTerrain;
        public bool manualSpotMarking;
        public readonly Dictionary<int, HashSet<int>> markedSpotsByLayer = new Dictionary<int, HashSet<int>> ();
        public readonly List<int> selectedSpots = new List<int> ();

        public bool enableTerrainShapeLogging;

        public Vector2Int lastScreenSize;
        public Rect sceneTitlePanelScreenRect;
        public Rect sceneModePanelUIRect;
        public Rect sceneModePanelScreenRect;
        public Rect sceneModePanelHintsScreenRect;
        public Rect toolbarUIRect;
        public Rect toolbarScreenRect;

        // XXX not sure what toggles this boolean.
        public bool showStructuralAnalysis = false;

        public bool showAreaBorder;
        public bool showAreaSegments = true;
        public bool showFields = true;
        public bool showLevelInfo = true;
        public bool showModeToolbar = true;
        public bool showSceneUIDebugOutlines;
    }
}
