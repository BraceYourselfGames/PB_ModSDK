using Sirenix.OdinInspector;

using UnityEngine;

namespace Area
{
    using Scene;

    [HideLabel, HideReferenceObjectPicker]
    sealed class AreaSceneDebugToggles
    {
        [ShowInInspector]
        [BoxGroup (OdinGroup.Name.DisplayInfo, false, Order = OdinGroup.Order.DisplayInfo)]
        [ToggleLeft, LabelText ("Display In-Scene Level Info")]
        public bool showLevelInfo
        {
            get => bb.showLevelInfo;
            set
            {
                bb.showLevelInfo = value;
                bb.repaintScene = true;
            }
        }

        [ShowInInspector]
        [BoxGroup (OdinGroup.Name.DisplayInfo)]
        [PropertyTooltip ("Display detailed debug info in volume shape mode panel")]
        [ToggleLeft]
        public bool displayVolumeDebugInfo
        {
            get => bb.displayVolumeDebugInfo;
            set => bb.displayVolumeDebugInfo = value;
        }

        [ShowInInspector]
        [BoxGroup (OdinGroup.Name.Logging, false, Order = OdinGroup.Order.Logging)]
        [ToggleLeft]
        public bool enableCursorLogging
        {
            get => bb.enableCursorLogging;
            set => bb.enableCursorLogging = value;
        }

        [ShowInInspector]
        [BoxGroup (OdinGroup.Name.Logging)]
        [ToggleLeft]
        public bool enableDamageLogging
        {
            get => bb.enableDamageLogging;
            set => bb.enableDamageLogging = value;
        }

        [ShowInInspector]
        [BoxGroup (OdinGroup.Name.Logging)]
        [ToggleLeft]
        public bool enableLayerCapsuleScanLogToFile
        {
            get => bb.enableCapsuleScanLogToFile;
            set => bb.enableCapsuleScanLogToFile = value;
        }

        [ShowInInspector]
        [BoxGroup (OdinGroup.Name.Logging)]
        [ShowIf (nameof(enableLayerCapsuleScanLogToFile))]
        [FilePath (AbsolutePath = true, Extensions = "txt, log")]
        [LabelText ("Log Path"), LabelWidth (68f)]
        public string layerCapsuleScanLogFilePath
        {
            get => bb.capsuleScanLogFilePath;
            set => bb.capsuleScanLogFilePath = value;
        }

        [ShowInInspector]
        [BoxGroup (OdinGroup.Name.Logging)]
        [ToggleLeft]
        public bool enableLayerCapsuleScanTrace
        {
            get => bb.capsuleScanTrace;
            set => bb.capsuleScanTrace = value;
        }

        [ShowInInspector]
        [BoxGroup (OdinGroup.Name.Logging)]
        [ShowIf (nameof(enableLayerCapsuleScanTrace))]
        [FilePath (AbsolutePath = true, Extensions = "txt, log")]
        [LabelText ("Trace Path"), LabelWidth (68f)]
        public string layerCapsuleScanTraceFilePath
        {
            get => bb.capsuleScanTraceFilePath;
            set => bb.capsuleScanTraceFilePath = value;
        }

        [ShowInInspector]
        [BoxGroup (OdinGroup.Name.Logging)]
        [ToggleLeft]
        public bool enableLayerMeshLogging
        {
            get => bb.enableLayerMeshLogging;
            set => bb.enableLayerMeshLogging = value;
        }

        [ShowInInspector]
        [BoxGroup (OdinGroup.Name.Logging)]
        [ToggleLeft]
        public bool enableTerrainShapeLogging
        {
            get => bb.enableTerrainShapeLogging;
            set => bb.enableTerrainShapeLogging = value;
        }

        [ShowInInspector]
        [BoxGroup (OdinGroup.Name.Logging)]
        [ToggleLeft]
        public bool enableVolumeTransferLogging
        {
            get => bb.enableTransferLogging;
            set => bb.enableTransferLogging = value;
        }

        [ShowInInspector]
        [BoxGroup (OdinGroup.Name.Logging)]
        [ToggleLeft]
        public bool enableVolumeShapeLogging
        {
            get => bb.enableVolumeLogging;
            set => bb.enableVolumeLogging = value;
        }

        [ShowInInspector]
        [BoxGroup (OdinGroup.Name.Area, false, Order = OdinGroup.Order.Area)]
        [ToggleLeft]
        public bool showAreaBorder
        {
            get => bb.showAreaBorder;
            set
            {
                bb.showAreaBorder = value;
                bb.repaintScene = true;
            }
        }

        [ShowInInspector]
        [BoxGroup (OdinGroup.Name.Area)]
        [ToggleLeft]
        public bool showAreaSegments
        {
            get => bb.showAreaSegments;
            set
            {
                bb.showAreaSegments = value;
                AreaSceneHelper.ToggleSegmentVisiblity (value);
            }
        }

        [ShowInInspector]
        [BoxGroup (OdinGroup.Name.Area)]
        [ToggleLeft]
        public bool showFields
        {
            get => bb.showFields;
            set
            {
                bb.showFields = value;
                AreaSceneHelper.ToggleFieldsVisibility (value);
            }
        }

        [ShowInInspector]
        [BoxGroup (OdinGroup.Name.Terrain, false, Order = OdinGroup.Order.Terrain)]
        [ToggleLeft]
        public bool firstTileCollectPolicy
        {
            get => ProceduralMeshTerrainV2.firstTileCollectPolicy;
            set
            {
                if (ProceduralMeshTerrainV2.firstTileCollectPolicy != value)
                {
                    bb.rebuildTerrain = true;
                }
                ProceduralMeshTerrainV2.firstTileCollectPolicy = value;
            }
        }

        [ShowInInspector]
        [BoxGroup (OdinGroup.Name.Terrain)]
        [ToggleLeft]
        public bool useNewTerrainMeshAlgorithm
        {
            get => bb.am.useNewTerrainMeshAlgorithm;
            set
            {
                if (bb.am.useNewTerrainMeshAlgorithm != value)
                {
                    bb.rebuildTerrain = true;
                }
                bb.am.useNewTerrainMeshAlgorithm = value;
            }
        }

        [ShowInInspector]
        [BoxGroup (OdinGroup.Name.Cursor, false, Order = OdinGroup.Order.Cursor)]
        [PropertyTooltip ("Use a square wireframe in place of the standard cube wireframe for the spot cursor in tileset/spot/color editing mode")]
        [ToggleLeft]
        public bool useSquareForSpotCursor
        {
            get => bb.currentSpotCursorType == SpotCursorType.Square;
            set => bb.currentSpotCursorType = value ? SpotCursorType.Square : SpotCursorType.Cube;
        }

        [ShowInInspector]
        [BoxGroup (OdinGroup.Name.Cursor)]
        [PropertyTooltip ("Show cube wireframes around cursor when in volume editing mode")]
        [ToggleLeft]
        public bool showVolumeWireFrames
        {
            get => bb.showVolumeWireFrames;
            set => bb.showVolumeWireFrames = value;
        }

        [ShowInInspector]
        [BoxGroup (OdinGroup.Name.Other, false, Order = OdinGroup.Order.Other)]
        [ToggleLeft]
        public bool drawVolumePasteHighlights
        {
            get => bb.am.debugPasteDrawHighlights;
            set => bb.am.debugPasteDrawHighlights = value;
        }

        [ShowInInspector]
        [BoxGroup (OdinGroup.Name.Other)]
        [ToggleLeft]
        public bool showSceneUIDebugOutlines
        {
            get => bb.showSceneUIDebugOutlines;
            set
            {
                bb.showSceneUIDebugOutlines = value;
                bb.repaintScene = true;
            }
        }

        public AreaSceneDebugToggles (AreaSceneBlackboard bb)
        {
            this.bb = bb;
        }

        readonly AreaSceneBlackboard bb;

        static class OdinGroup
        {
            public static class Name
            {
                public const string Area = nameof(Area);
                public const string Cursor = nameof(Cursor);
                public const string DisplayInfo = nameof(DisplayInfo);
                public const string Logging = nameof(Logging);
                public const string Other = nameof(Other);
                public const string Props = nameof(Props);
                public const string Terrain = nameof(Terrain);
            }

            public static class Order
            {
                public const float DisplayInfo = 0f;
                public const float Logging = 1f;
                public const float Area = 2f;
                public const float Terrain = 3f;
                public const float Cursor = 4f;
                public const float Other = 5f;
            }
        }
    }
}
