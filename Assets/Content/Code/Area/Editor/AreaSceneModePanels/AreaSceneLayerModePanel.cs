using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

using UnityEngine;

#if PB_MODSDK
using PhantomBrigade.SDK.ModTools;
#endif

namespace Area
{
    using Scene;

    sealed class AreaSceneLayerModePanel : AreaSceneModePanel
    {
        public static AreaSceneModePanel Create (AreaSceneBlackboard bb) => new AreaSceneLayerModePanel (bb);

        public GUILayoutOptions.GUILayoutOptionsInstance Width => GUILayoutOptions.Width (300f);
        public string Title => "Layer editing mode";

        public void OnDisable ()
        {
            layerInfo.OnDisable ();
            operations.OnDisable ();
            diagnostics.OnDisable ();
        }

        public void Draw ()
        {
            if (Event.current.type == EventType.Layout)
            {
                cachedCell = bb.lastCellHovered ?? bb.lastCellInspected;
            }

            GUILayout.BeginVertical ("Box");
            layerInfo.Draw (cachedCell);
            GUILayout.EndVertical ();

            GUILayout.Space (4f);
            GUILayout.BeginVertical ("Box");
            operations.Draw ();
            GUILayout.EndVertical ();

            GUILayout.Space (4f);
            GUILayout.BeginVertical ("Box");
            diagnostics.Draw ();
            GUILayout.EndVertical ();
        }

        public AreaSceneModeHints Hints
        {
            get
            {
                hints.HintText = bb.enableLayerDiagnostics
                    ? (bb.cellEmpty
                        ? emptyDiagHint
                        : nonEmptyDiagHint)
                    + diagKeyboardHint
                    : bb.cellEmpty
                        ? emptyHint
                        : nonEmptyHint;
                return hints;
            }
        }

        AreaSceneLayerModePanel (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            layerInfo = new LayerInfo (bb);
            operations = new CellOperations (bb);
            diagnostics = new Diagnostics (bb);
        }

        readonly AreaSceneBlackboard bb;
        readonly LayerInfo layerInfo;
        readonly CellOperations operations;
        readonly Diagnostics diagnostics;

        AreaVolumePoint cachedCell;

        const string emptyHint = "<color=#66666699>[LMB] - Select/unselect cell     [Shift + LMB] - Extend selection</color>     [MW▲▼] - Move up/down";
        const string nonEmptyHint = "[LMB] - Select/unselect cell     [Shift + LMB] - Extend selection     [MW▲▼] - Move up/down";
        const string emptyDiagHint = "<color=#66666699>[LMB] - Select/unselect cell     [Shift + LMB] - Extend selection     [Control + LMB] - Add/remove mark</color>     [MW▲▼] - Move up/down";
        const string nonEmptyDiagHint = "[LMB] - Select/unselect cell     [Shift + LMB] - Extend selection     [Control + LMB] - Add/remove mark     [MW▲▼] - Move up/down";
        const string diagKeyboardHint = "\n[Z] - Zoom to inspect";

        static readonly AreaSceneModeHints hints = new AreaSceneModeHints ()
        {
            NonAltHintText = "[-] - Move up a layer     [=] - Move down a layer",
        };
    }

    sealed class LayerInfo : SelfDrawnGUI
    {
        [ShowInInspector]
        [BoxWithBackgroundGroup (OdinGroup.Name.Layer)]
        [LabelText ("Layer:"), LabelWidth (37f), DisplayAsString, EnableGUI]
        public int layer => bb.layer;

        [ShowInInspector]
        [BoxWithBackgroundGroup (OdinGroup.Name.Layer)]
        [EnableIf (nameof(enableShowProps))]
        [PropertyTooltip ("Show props on current layer. By default props are hidden on the current level.\nProps are always hidden on layers above the current one.")]
        [ToggleLeft]
        public bool showPropsOnLayer
        {
            get => bb.showPropsOnLayer;
            set => bb.showPropsOnLayer = value;
        }

        [ShowInInspector]
        [BoxWithBackgroundGroup (OdinGroup.Name.Cell, VisibleIf = nameof(hasCell))]
        [HorizontalGroup (OdinGroup.Name.CellPosition, Width = 100f)]
        [HideLabel, DisplayAsString, EnableGUI]
        public string cellIndex => "Cell: " + (cell != null ? cell.spotIndex.ToString () : "—");

        [ShowInInspector]
        [BoxWithBackgroundGroup (OdinGroup.Name.Cell)]
        [HorizontalGroup (OdinGroup.Name.CellPosition)]
        [HideLabel, DisplayAsString, EnableGUI]
        public string positionIndex => cell != null
            ? string.Format ("Position: ({0}, {1}, {2})", cell.pointPositionIndex.x, cell.pointPositionIndex.z, cell.pointPositionIndex.y)
            : "";

        [BoxWithBackgroundGroup (OdinGroup.Name.Cell)]
        [MiniLabel]
        public string location () => string.Format ("Center: ({0:F1}, {1:F1}, {2:F1})", cell.instancePosition.x, cell.instancePosition.z, cell.instancePosition.y);

        [BoxWithBackgroundGroup (OdinGroup.Name.Cell)]
        [MiniLabel]
        public string hasSpot () => "Spot: " + cell.spotPresent;

        [BoxWithBackgroundGroup (OdinGroup.Name.Cell)]
        [MiniLabel]
        public string cellState () => "State: " + cell.pointState;

        [BoxWithBackgroundGroup (OdinGroup.Name.Cell)]
        [MiniLabel]
        public string cellConfiguration () => string.Format ("█ {0} ({1}){2}", configuration, AreaSceneHelper.GetPointConfigurationDisplayString (configuration), cell.spotHasDamagedPoints ? " damaged" : "");

        [BoxWithBackgroundGroup (OdinGroup.Name.Cell)]
        [MiniLabel]
        public string cellTileset () => string.Format ("█ {0} ({1})", cell.blockTileset, AreaSceneHelper.GetTilesetDisplayName (cell.blockTileset));

        [BoxWithBackgroundGroup (OdinGroup.Name.Cell)]
        [MiniLabel]
        public string cellTilesetDefinition () => string.Format ("█ {0}/{1}/{2}/{3}", cell.blockGroup, cell.blockSubtype, cell.blockRotation, cell.blockFlippedHorizontally ? "F" : "-");

        [BoxWithBackgroundGroup (OdinGroup.Name.Cell)]
        [MiniLabel]
        public string cellIntegrity () => "Integrity: " + (cell.indestructibleIndirect
            ? "indestructible (indirect)"
            : cell.destructionUntracked
                ? "untracked"
                : cell.destructible
                    ? string.Format ("{0:F2}", cell.integrity)
                    : "indestructible");

        [BoxWithBackgroundGroup (OdinGroup.Name.Cell)]
        [PropertySpace (0f, 4f)]
        [MiniLabel (VisibleIf = nameof(hasTerrainOffset))]
        public string cellTerrainOffset () => string.Format ("Terrain offset: {0:F2}", cell.terrainOffset);

        public void Draw (AreaVolumePoint cellHovered)
        {
            cell = cellHovered;
            configuration = cellHovered != null
                ? cellHovered.spotHasDamagedPoints ? cellHovered.spotConfigurationWithDamage : cellHovered.spotConfiguration
                : (byte)0;
            base.Draw ();
        }

        public LayerInfo (AreaSceneBlackboard bb)
        {
            this.bb = bb;
        }

        bool hasCell => cell != null;
        bool hasTerrainOffset => hasCell && cell.terrainOffset != 0f;
        bool enableShowProps => AreaManager.displayProps;

        readonly AreaSceneBlackboard bb;
        AreaVolumePoint cell;
        byte configuration;

        static class OdinGroup
        {
            public static class Name
            {
                public const string Cell = nameof(Cell);
                public const string CellPosition = Cell + "/Position";
                public const string Layer = nameof(Layer);
            }
        }
    }

    sealed class CellOperations : SelfDrawnGUI
    {
        [TitleGroup ("Cell operations", horizontalLine: false, GroupID = OdinGroup.Name.CellOperations)]
        [BoxGroup (OdinGroup.Name.Warning, false, VisibleIf = nameof(hasSelectionOtherLayer), Order = OdinGroup.Order.Operations.Warning)]
        [InfoBox ("$" + nameof(selectionOtherLayerWarningText), InfoMessageType.Warning, VisibleIf = nameof(hasSelectionOtherLayer))]
        [Button]
        public void GoToLayer () => bb.layer = bb.selectionLayer;

        [VerticalGroup (OdinGroup.Name.SelectedCells, VisibleIf = "@!" + nameof(hasSelectionOtherLayer), Order = OdinGroup.Order.Operations.SelectedCells)]
        [MiniLabel, EnableGUI]
        public string selectedCells => bb.selectedSpots.Count == 0
            ? "No selected cells"
            : bb.layer == bb.selectionLayer
                ? string.Format ("Selected cells: {0} ({1})", bb.selectedSpots.Count, ElidedCellList ())
                : string.Format ("Selected cells: {0} cell{1} on layer {2}", bb.selectedSpots.Count, bb.selectedSpots.Count == 1 ? "" : "s", bb.selectionLayer);

        [BoxGroup (OdinGroup.Name.Selectors, false, Order = OdinGroup.Order.Operations.Selectors)]
        [HorizontalGroup (OdinGroup.Name.SelectorsTileset, Order = OdinGroup.Order.Operations.SubOrder.TilesetSelector)]
        [BoxGroup (OdinGroup.Name.Selectors)]
        [ValueDropdown (nameof(availableTilesets))]
        [LabelText ("Tileset"), LabelWidth (50f)]
        public int selectorTileset = -1;

        [HorizontalGroup (OdinGroup.Name.SelectorsTileset, Width = 60f)]
        [EnableIf (nameof(enableSelectorButton))]
        [PropertySpace (2f)]
        [Button ("Select")]
        public void SelectTilesetOnLayer ()
        {
            var am = bb.am;
            var points = am.points;
            var layerSize = am.boundsFull.x * am.boundsFull.z;
            var accumulated = new List<int> ();
            var startIndex = bb.layer * layerSize;
            var endIndex = startIndex + layerSize;
            for (var i = startIndex; i < endIndex; i += 1)
            {
                var spot = points[i];
                if (!spot.spotPresent)
                {
                    continue;
                }
                if (selectorTileset != spot.blockTileset)
                {
                    continue;
                }
                if (selectorTileset == 0 && AreaManager.IsSpotInterior (spot))
                {
                    continue;
                }
                if (selectorTileset == 0 && AreaSceneHelper.IsFreeSpace (spot))
                {
                    continue;
                }
                accumulated.Add (spot.spotIndex);
            }
            bb.selectedSpots.Clear ();
            bb.selectedSpots.AddRange (accumulated);
            bb.selectionLayer = bb.layer;
        }

        [BoxGroup (OdinGroup.Name.Selectors)]
        [EnableIf (nameof(hasSelected))]
        [PropertyOrder (OdinGroup.Order.Operations.SubOrder.Deselect)]
        [Button]
        public void DeselectAll () => bb.selectedSpots.Clear ();

        [BoxGroup (OdinGroup.Name.CellProperties, false, VisibleIf = nameof(hasSelectedThisLayer), Order = OdinGroup.Order.Operations.CellProperties)]
        [HorizontalGroup (OdinGroup.Name.Update, Order = OdinGroup.Order.Operations.SubOrder.StateTileset)]
        [VerticalGroup (OdinGroup.Name.CellStateTileset)]
        [ValueDropdown (nameof(stateChanges))]
        [LabelWidth (50f)]
        public StateChange state;

        [VerticalGroup (OdinGroup.Name.CellStateTileset)]
        [ValueDropdown (nameof(availableTilesets))]
        [LabelText ("Tileset"), LabelWidth (50f)]
        public int currentTileset = -1;

        [HorizontalGroup (OdinGroup.Name.Update, Width = 60f)]
        [EnableIf (nameof(enableUpdateButton))]
        [PropertySpace (2f)]
        [Button ("Apply", ButtonHeight = 38)]
        public void UpdateSelectedCells ()
        {
            if (state != StateChange.Unchanged)
            {
                bb.spotChangeState = state == StateChange.Empty ? AreaVolumePointState.Empty : AreaVolumePointState.Full;
                bb.spotChange = SpotChange.State;
            }
            if (currentTileset != -1)
            {
                bb.spotChangeTileset = currentTileset;
                bb.spotChange = bb.spotChange == SpotChange.State ? SpotChange.StateAndTileset : SpotChange.Tileset;
            }
        }

        [ButtonGroup (OdinGroup.Name.ButtonsEmptyFull, Order = OdinGroup.Order.Operations.SubOrder.EmptyFull)]
        [PropertyTooltip ("Clear state and tileset information for selected cells.\nThis is a dangerous operation because it will turn the cells into thin air and you won't be able to select them again.")]
        [GUIColor ("lightorange")]
        [Button ("Set Empty", Icon = SdfIconType.ExclamationTriangle, IconAlignment = IconAlignment.LeftOfText)]
        public void ResetSelectedEmpty () => bb.spotChange = SpotChange.Empty;

        [ButtonGroup (OdinGroup.Name.ButtonsEmptyFull)]
        [PropertyTooltip ("Convert selected cells to interior tiles.\nClears all tileset information.")]
        [Button ("Set Interior", Icon = SdfIconType.Box, IconAlignment = IconAlignment.LeftOfText)]
        public void ResetSelectedFull () => bb.spotChange = SpotChange.Interior;

        bool hasSelected => bb.selectedSpots.Count != 0;
        bool hasSelectedThisLayer => hasSelected && bb.layer == bb.selectionLayer;
        bool hasSelectionOtherLayer => hasSelected && bb.layer != bb.selectionLayer;
        bool enableUpdateButton => hasSelectedThisLayer && (state != StateChange.Unchanged || currentTileset != -1);
        bool enableSelectorButton => selectorTileset != -1;

        string selectionOtherLayerWarningText => "One or more cells are selected on layer " + bb.selectionLayer;

        string ElidedCellList ()
        {
            switch (bb.selectedSpots.Count)
            {
                case 1:
                    return bb.selectedSpots[0].ToString ();
                case 2:
                case 3:
                    return string.Join (", ", bb.selectedSpots);
                default:
                    return string.Join (", ", bb.selectedSpots.Take (3)) + " ...";
            }
        }

        public CellOperations (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            availableTilesets.Add ("Unchanged", -1);
            availableTilesets.Add (AreaSceneHelper.unresolvedTilesetName + " (0)", 0);
            foreach (var value in AreaTilesetHelper.database.tilesets.Values)
            {
                availableTilesets.Add (AreaSceneHelper.GetTilesetDisplayName (value) + " (" + value.id + ")", value.id);
            }
        }

        readonly AreaSceneBlackboard bb;
        readonly ValueDropdownList<int> availableTilesets = new ValueDropdownList<int> ();

        static readonly IEnumerable<StateChange> stateChanges = Enumerable
            .Range ((int)StateChange.Unchanged, (int)StateChange.EnumCount)
            .Cast<StateChange> ()
            .ToList ();

        public enum StateChange
        {
            Unchanged = 0,
            Empty,
            Full,
            // Add other enum values before this entry.
            EnumCount,
        }

        static class OdinGroup
        {
            public static class Name
            {
                public const string ButtonsEmptyFull = CellProperties + "/Empty+Full";
                public const string CellOperations = nameof(CellOperations);
                public const string CellProperties = CellOperations + "/" + nameof(CellProperties);
                public const string CellStateTileset = Update + "/StateTileset";
                public const string SelectedCells = CellOperations + "/" + nameof(SelectedCells);
                public const string Selectors = CellOperations + "/" + nameof(Selectors);
                public const string SelectorsTileset = Selectors + "/Tileset";
                public const string Update = CellProperties + "/" + nameof(Update);
                public const string Warning = CellOperations + "/" + nameof(Warning);
            }

            public static class Order
            {
                public static class Operations
                {
                    public const float Warning = 0f;
                    public const float SelectedCells = 1f;
                    public const float Selectors = 2f;
                    public const float CellProperties = 4f;

                    public static class SubOrder
                    {
                        public const float StateTileset = 0f;
                        public const float EmptyFull = 1f;

                        public const float TilesetSelector = 0f;
                        public const float Deselect = 1f;
                    }
                }
            }
        }
    }

    sealed class Diagnostics : SelfDrawnGUI
    {
        [ShowInInspector]
        [VerticalGroup (OdinGroup.Name.Enable, VisibleIf = "@!" + nameof(enableDiagnostics))]
        [ToggleLeft]
        public bool enableDiagnostics
        {
            get => bb.enableLayerDiagnostics;
            set => bb.enableLayerDiagnostics = value;
        }

        [TitleGroup ("Diagnostics", horizontalLine: false, VisibleIf = nameof(enableDiagnostics), GroupID = OdinGroup.Name.Diagnostics)]
        [HorizontalGroup (OdinGroup.Name.Controls)]
        [ToggleLeft, LabelText ("Log scans")]
        public bool log;

        [HorizontalGroup (OdinGroup.Name.Controls, Width = 125f)]
        [MinValue (0f), MaxValue (nameof(maxTarget))]
        [OnValueChanged (nameof(GoToTarget))]
        [DelayedProperty, LabelText ("Inspect"), LabelWidth (45f)]
        public int targetIndex;

        [VerticalGroup (OdinGroup.Name.Buttons)]
        [ButtonGroup (OdinGroup.Name.TopLevelButtons)]
        [Button]
        public void RebuildTerrain () => bb.rebuildTerrain = true;

        [VerticalGroup (OdinGroup.Name.Buttons)]
        [ButtonGroup (OdinGroup.Name.TopLevelButtons)]
        [Button]
        public void FixEdgePoints ()
        {
            var am = bb.am;
            var bounds = am.boundsFull;
            var lastX = bounds.x - 1;
            var layerSize = bounds.x * bounds.z;
            var points = am.points;
            var update = new HashSet<int> ();
            for (var y = 1; y < bounds.y; y += 1)
            {
                var layerStart = y * layerSize;
                for (var z = 0; z < bounds.z; z += 1)
                {
                    var index = lastX + z * bounds.x + layerStart;
                    var point = points[index];
                    if (point.pointState != AreaVolumePointState.Empty)
                    {
                        continue;
                    }
                    var pointAbove = points[index - layerSize];
                    if (pointAbove.pointState != AreaVolumePointState.Full)
                    {
                        continue;
                    }
                    point.pointState = AreaVolumePointState.Full;
                    update.Add (index);
                    if (log)
                    {
                        Debug.Log ("Set edge point to full @ " + point.spotIndex);
                    }
                }
                var lastZ = bounds.x * (bounds.z - 1);
                for (var x = 0; x < bounds.x; x += 1)
                {
                    var index = x + lastZ + layerStart;
                    var point = points[index];
                    if (point.pointState != AreaVolumePointState.Empty)
                    {
                        continue;
                    }
                    var pointAbove = points[index - layerSize];
                    if (pointAbove.pointState != AreaVolumePointState.Full)
                    {
                        continue;
                    }
                    point.pointState = AreaVolumePointState.Full;
                    update.Add (index);
                    if (log)
                    {
                        Debug.Log ("Set edge point to full @ " + point.spotIndex);
                    }
                }
            }
            if (update.Count == 0)
            {
                return;
            }
            foreach (var index in update.OrderBy (i => i))
            {
                AreaSceneModeHelper.UpdateNeighborState(am, index);
            }
            am.RebuildAllBlocks ();
            bb.rebuildTerrain = true;
        }

        [FoldoutGroup (OdinGroup.Name.Scanners, true, Order = OdinGroup.Order.Scanners)]
        [ButtonGroup (OdinGroup.Name.ScanButtonsRow1)]
        [Button ("Empty Bottom")]
        public void ScanForBottomLayerEmpty ()
        {
            var am = bb.am;
            var bounds = am.boundsFull;
            var points = am.points;

            unresolvedSpotsByLayer.Clear ();
            for (var i = bounds.x * bounds.z * (bounds.y - 1); i < points.Count; i += 1)
            {
                var spot = points[i];
                if (spot.pointState != AreaVolumePointState.Empty)
                {
                    continue;
                }
                var layer = spot.pointPositionIndex.y;
                if (!unresolvedSpotsByLayer.TryGetValue (layer, out var spots))
                {
                    spots = new HashSet<int> ();
                    unresolvedSpotsByLayer[layer] = spots;
                }
                spots.Add (i);
                if (log)
                {
                    Debug.LogFormat ("Empty bottom spot @ {0}/{1}", spot.spotIndex, spot.pointPositionIndex);
                }
            }

            bb.manualSpotMarking = false;
            bb.markedSpotsByLayer.Clear ();
            foreach (var kvp in unresolvedSpotsByLayer)
            {
                bb.markedSpotsByLayer[kvp.Key] = kvp.Value;
            }
            lastScan = LayerScan.BottomEmpty;
        }

        [ButtonGroup (OdinGroup.Name.ScanButtonsRow1)]
        [Button ("Full/Empty")]
        public void ScanForFullOverEmpty ()
        {
            var am = bb.am;
            var layerSize = bb.am.boundsFull.x * bb.am.boundsFull.z;
            var points = am.points;
            stackedTerrainSpotsByLayer.Clear ();
            for (var i = 0; i < points.Count; i += 1)
            {
                var spot = am.points[i];
                if (!spot.spotPresent)
                {
                    continue;
                }
                if (AreaManager.IsSpotInterior (spot))
                {
                    continue;
                }
                if (AreaSceneHelper.IsFreeSpace (spot))
                {
                    continue;
                }
                var isTerrain = AreaManager.IsPointTerrain (spot);
                if (!isTerrain && spot.blockTileset != 0)
                {
                    continue;
                }
                var fullTopOverEmptyBottom = (spot.spotConfiguration & TilesetUtility.configurationBitTopSelf) == TilesetUtility.configurationBitTopSelf
                    && (spot.spotConfiguration & TilesetUtility.configurationBitBottomSelf) == 0;
                if (!fullTopOverEmptyBottom)
                {
                    continue;
                }
                var layer = spot.pointPositionIndex.y + 1;
                if (!stackedTerrainSpotsByLayer.TryGetValue (layer, out var spots))
                {
                    spots = new List<int> ();
                    stackedTerrainSpotsByLayer[layer] = spots;
                }
                spots.Add (i + layerSize);
                if (log)
                {
                    Debug.LogFormat
                    (
                        "Full over empty @ {0}/{1} | {2} | {3}",
                        i,
                        spot.pointPositionIndex,
                        layer,
                        AreaSceneHelper.GetPointConfigurationDisplayString (spot.spotConfiguration)
                    );
                }
            }

            bb.manualSpotMarking = false;
            bb.markedSpotsByLayer.Clear ();
            foreach (var kvp in stackedTerrainSpotsByLayer)
            {
                bb.markedSpotsByLayer[kvp.Key] = new HashSet<int> (kvp.Value);
            }
            lastScan = LayerScan.FullOverEmpty;
        }

        [ButtonGroup (OdinGroup.Name.ScanButtonsRow2)]
        [Button ("Stacked Terrain")]
        public void ScanForStackedTerrain ()
        {
            var am = bb.am;
            var bounds = am.boundsFull;
            var layerSize = bounds.x * bounds.z;
            var points = am.points;

            var flatMap = new BitVector32[bounds.x * bounds.z];
            for (var z = 0; z < bounds.z; z += 1)
            {
                for (var x = 0; x < bounds.x; x += 1)
                {
                    var fmi = x + z * bounds.x;
                    var bv = flatMap[fmi];
                    bv[cellLayer] = bounds.y;
                    flatMap[fmi] = bv;
                }
            }

            stackedTerrainSpotsByLayer.Clear ();
            for (var i = 0; i < points.Count; i += 1)
            {
                var spot = am.points[i];
                var isTerrain = AreaManager.IsPointTerrain (spot);
                var isInterior = AreaManager.IsSpotInterior (spot);
                if (!isTerrain && !isInterior)
                {
                    continue;
                }

                var y = spot.pointPositionIndex.y;
                var fmi = i - y * layerSize;
                var bv = flatMap[fmi];
                var layer = bv[cellLayer];
                if (y < layer)
                {
                    bv[cellLayer] = y;
                    bv[cellConfiguration] = spot.spotConfiguration;
                    flatMap[fmi] = bv;
                    continue;
                }
                if (isInterior)
                {
                    continue;
                }
                if (y - 1 == layer)
                {
                    var cfgAbove = bv[cellConfiguration] & TilesetUtility.configurationFloor;
                    var cfg = (spot.spotConfiguration & 0xF0) >> 4;
                    if (cfg == cfgAbove && cfgAbove != TilesetUtility.configurationFloor)
                    {
                        bv[cellLayer] = y;
                        bv[cellConfiguration] = spot.spotConfiguration;
                        flatMap[fmi] = bv;
                        continue;
                    }
                }
                if (!stackedTerrainSpotsByLayer.TryGetValue (y, out var spots))
                {
                    spots = new List<int> ();
                    stackedTerrainSpotsByLayer[y] = spots;
                }
                spots.Add (i);
                if (log)
                {
                    Debug.LogFormat
                    (
                        "Stacked terrain @ {0}/{1} | {2}/{3} | {4}",
                        i,
                        spot.pointPositionIndex,
                        layer,
                        AreaSceneHelper.GetPointConfigurationDisplayString ((byte)bv[cellConfiguration]),
                        AreaSceneHelper.GetPointConfigurationDisplayString (spot.spotConfiguration)
                    );
                }
            }

            bb.manualSpotMarking = false;
            bb.markedSpotsByLayer.Clear ();
            foreach (var kvp in stackedTerrainSpotsByLayer)
            {
                bb.markedSpotsByLayer[kvp.Key] = new HashSet<int> (kvp.Value);
            }
            lastScan = LayerScan.StackedTerrain;
        }

        [ButtonGroup (OdinGroup.Name.ScanButtonsRow2)]
        [Button ("Capsules")]
        public void ScanForEncapsulatedSpace ()
        {
            EncapsulatedSpaceScanner.Scan (bb, log);
            lastScan = LayerScan.Capsules;
        }

        [ButtonGroup (OdinGroup.Name.ScanButtonsRow3)]
        [Button ("Full Tile")]
        public void ScanForFullSpotWithTile ()
        {
            var am = bb.am;
            var points = am.points;

            unresolvedSpotsByLayer.Clear ();
            for (var i = 0; i < points.Count; i += 1)
            {
                var spot = points[i];
                if (!spot.spotPresent)
                {
                    continue;
                }
                if (spot.blockTileset == 0)
                {
                    continue;
                }
                if (spot.pointState != AreaVolumePointState.Full)
                {
                    continue;
                }
                if (spot.spotConfiguration != TilesetUtility.configurationFull)
                {
                    continue;
                }
                var layer = spot.pointPositionIndex.y;
                if (!unresolvedSpotsByLayer.TryGetValue (layer, out var spots))
                {
                    spots = new HashSet<int> ();
                    unresolvedSpotsByLayer[layer] = spots;
                }
                spots.Add (i);
                if (log)
                {
                    Debug.LogFormat
                    (
                        "Full (interior) spot with tile @ {0}/{1} | tileset: {2} ({3})",
                        spot.spotIndex,
                        spot.pointPositionIndex,
                        AreaSceneHelper.GetTilesetDisplayName (spot.blockTileset),
                        spot.blockTileset
                    );
                }
            }

            bb.manualSpotMarking = false;
            bb.markedSpotsByLayer.Clear ();
            foreach (var kvp in unresolvedSpotsByLayer)
            {
                bb.markedSpotsByLayer[kvp.Key] = kvp.Value;
            }
            lastScan = LayerScan.FullWithTile;
        }

        [ButtonGroup (OdinGroup.Name.ScanButtonsRow3)]
        [EnableIf (nameof(isUntamperedLoad))]
        [Button ("Unresolved")]
        public void ScanForUnresolved ()
        {
            var am = bb.am;
            var points = am.points;

            unresolvedSpotsByLayer.Clear ();
            for (var i = 0; i < points.Count; i += 1)
            {
                var spot = points[i];
                if (!spot.spotPresent)
                {
                    continue;
                }
                if (AreaManager.IsSpotInterior (spot))
                {
                    continue;
                }
                if (AreaSceneHelper.IsFreeSpace (spot))
                {
                    continue;
                }
                if (spot.blockTileset != 0)
                {
                    continue;
                }
                var layer = spot.pointPositionIndex.y;
                if (!unresolvedSpotsByLayer.TryGetValue (layer, out var spots))
                {
                    spots = new HashSet<int> ();
                    unresolvedSpotsByLayer[layer] = spots;
                }
                spots.Add (spot.spotIndex);
                if (log)
                {
                    Debug.LogFormat
                    (
                        "Unresolved spot @ {0}/{1} | configuration: {2}",
                        spot.spotIndex,
                        spot.pointPositionIndex,
                        AreaSceneHelper.GetPointConfigurationDisplayString (spot.spotConfiguration)
                    );
                }
            }

            bb.manualSpotMarking = false;
            bb.markedSpotsByLayer.Clear ();
            foreach (var kvp in unresolvedSpotsByLayer)
            {
                bb.markedSpotsByLayer[kvp.Key] = kvp.Value;
            }
            lastScan = LayerScan.Unresolved;
        }

        [BoxGroup (OdinGroup.Name.Marked, false, VisibleIf = nameof(hasMarked), Order = OdinGroup.Order.Marked)]
        [VerticalGroup (OdinGroup.Name.MarkedButtons, Order = OdinGroup.Order.MarkedButtons)]
        [PropertySpace (0f, 2f)]
        [Button ("Umark all cells")]
        public void UnmarkAll () => bb.markedSpotsByLayer.Clear ();

        [VerticalGroup (OdinGroup.Name.MarkedButtons)]
        [ShowIf (nameof(showSetMarkedInterior))]
        [PropertyTooltip ("Convert marked cells to interior tiles. This applies to all layers.\n\nClears all tileset information in the marked cells.")]
        [Button ("Set all marked cells as interior", Icon = SdfIconType.Box, IconAlignment = IconAlignment.LeftOfText)]
        public void SetAllMarkedInterior () => bb.spotChange = SpotChange.InteriorMarkedAll;

        [VerticalGroup (OdinGroup.Name.MarkedButtons)]
        [ShowIf (nameof(showSetMarkedFull))]
        [PropertyTooltip ("Set top self for marked cells to full. This applies to all layers.")]
        [Button ("Set all marked cells as full", Icon = SdfIconType.Box, IconAlignment = IconAlignment.LeftOfText)]
        public void SetAllMarkedFull () => bb.spotChange = SpotChange.FullMarkedAll;

        [ShowInInspector]
        [TitleGroup ("$" + nameof(markedCellsTitle), null, TitleAlignments.Left, false, GroupID = OdinGroup.Name.MarkedCells, Order = OdinGroup.Order.MarkedCells)]
        public readonly MarkedCells markedCells = new MarkedCells ();

        string markedCellsTitle => "Marked cells (" + (bb.manualSpotMarking ? mixedMarkingLabel : scanLabels[(int)lastScan]) + ")";

        public override void Draw ()
        {
            PopulateMarkedCellLists ();
            base.Draw ();
        }

        void GoToTarget ()
        {
            var cell = bb.am.points[targetIndex];
            AreaSceneModeHelper.InspectCell (bb, cell);
            if (!bb.markedSpotsByLayer.TryGetValue (bb.layer, out var marked))
            {
                marked = new HashSet<int> ();
                bb.markedSpotsByLayer[bb.layer] = marked;
            }
            else if (marked.Contains (cell.spotIndex))
            {
                return;
            }
            marked.Add (cell.spotIndex);
            bb.manualSpotMarking = true;
        }

        void PopulateMarkedCellLists ()
        {
            if (!enableDiagnostics || !hasMarked)
            {
                return;
            }

            if (markedCells.marked.Count == 0)
            {
                for (var i = 0; i < bb.am.boundsFull.y; i += 1)
                {
                    markedCells.marked.Add (null);
                }
            }

            var points = bb.am.points;
            for (var i = 0; i < markedCells.marked.Count; i += 1)
            {
                var mcl = markedCells.marked[i];
                if (!bb.markedSpotsByLayer.TryGetValue (i, out var spots))
                {
                    mcl?.Reset ();
                    continue;
                }
                if (mcl == null)
                {
                    mcl = new MarkedCellList ()
                    {
                        bb = bb,
                        layer = i,
                    };
                    markedCells.marked[i] = mcl;
                }
                var collected = spots
                    .OrderBy (index => index)
                    .Select (index => new MarkedCell ()
                    {
                        bb = bb,
                        cell = points[index],
                    });
                mcl.marked.Clear ();
                mcl.marked.AddRange (collected);
                mcl.expanded = mcl.expanded && mcl.scan == lastScan;
                mcl.scan = lastScan;
            }
        }

        bool hasMarked
        {
            get
            {
                foreach (var spots in bb.markedSpotsByLayer.Values)
                {
                    if (spots.Count != 0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        bool showSetMarkedFull => hasMarked && lastScan == LayerScan.FullOverEmpty;
        bool showSetMarkedInterior => hasMarked && lastScan != LayerScan.FullOverEmpty;
        bool isUntamperedLoad => DataMultiLinkerCombatArea.selectedArea != null && !DataMultiLinkerCombatArea.selectedArea.errorsCorrectedOnLoad;

        double maxTarget => bb.am.points.Count;

        public Diagnostics (AreaSceneBlackboard bb)
        {
            this.bb = bb;
        }

        readonly AreaSceneBlackboard bb;
        LayerScan lastScan;

        static readonly Dictionary<int, List<int>> stackedTerrainSpotsByLayer = new Dictionary<int, List<int>> ();
        static readonly Dictionary<int, HashSet<int>> unresolvedSpotsByLayer = new Dictionary<int, HashSet<int>> ();
        static readonly BitVector32.Section cellConfiguration = BitVector32.CreateSection (byte.MaxValue);
        static readonly BitVector32.Section cellLayer = BitVector32.CreateSection (16, cellConfiguration);

        static readonly Dictionary<int, string> scanLabels = new Dictionary<int, string> ()
        {
            [(int)LayerScan.None] = "—",
            [(int)LayerScan.StackedTerrain] = "stacked terrain",
            [(int)LayerScan.Unresolved] = "unresolved",
            [(int)LayerScan.FullOverEmpty] = "full over empty",
            [(int)LayerScan.FullWithTile] = "full with tile",
            [(int)LayerScan.BottomEmpty] = "bottom empty",
            [(int)LayerScan.Capsules] = "capsules",
        };

        const string mixedMarkingLabel = "mixed";

        static class OdinGroup
        {
            public static class Name
            {
                public const string Controls = Diagnostics + "/" + nameof(Controls);
                public const string Diagnostics = nameof(Diagnostics);
                public const string Enable = nameof(Enable);
                public const string Buttons = Diagnostics + "/" + nameof(Buttons);
                public const string Marked = Buttons + "/" + nameof(Marked);
                public const string MarkedButtons = Marked + "/Buttons";
                public const string MarkedCells = Marked + "/Cells";
                public const string ScanButtonsRow1 = Scanners + "/Row1";
                public const string ScanButtonsRow2 = Scanners + "/Row2";
                public const string ScanButtonsRow3 = Scanners + "/Row3";
                public const string Scanners = Buttons + "/" + nameof(Scanners);
                public const string TopLevelButtons = Buttons + "/TopLevel";
            }

            public static class Order
            {
                public const float Scanners = 0f;
                public const float Marked = 1f;

                public const float MarkedButtons = 0f;
                public const float MarkedCells = 1f;
            }
        }
    }

    enum LayerScan
    {
        None = 0,
        StackedTerrain,
        Unresolved,
        FullOverEmpty,
        FullWithTile,
        BottomEmpty,
        Capsules,
    }

    [HideLabel, HideReferenceObjectPicker]
    sealed class MarkedCells
    {
        public readonly List<MarkedCellList> marked = new List<MarkedCellList> ();
    }

    [HideLabel, HideReferenceObjectPicker]
    sealed class MarkedCellList
    {
        [ButtonGroup ("Buttons", Order = 0f)]
        [EnableIf (nameof(enableSetButton))]
        [PropertyTooltip ("$" + nameof(tooltipText))]
        [Button ("$" + nameof(buttonText), Icon = SdfIconType.Box, IconAlignment = IconAlignment.LeftOfText)]
        public void SetLayerMarked () => bb.spotChange = scan == LayerScan.FullOverEmpty ? SpotChange.FullMarkedLayer : SpotChange.InteriorMarkedLayer;

        [ButtonGroup ("Buttons")]
        [EnableIf (nameof(enableButtons))]
        [Button]
        public void Unmark ()
        {
            if (!bb.markedSpotsByLayer.TryGetValue (bb.layer, out var cells))
            {
                return;
            }
            cells.Clear ();
        }

        [ShowInInspector]
        [PropertyOrder (1f)]
        [PropertySpace (2f)]
        [TableList (AlwaysExpanded = true, IsReadOnly = true, NumberOfItemsPerPage = 10, ShowPaging = true)]
        public readonly List<MarkedCell> marked = new List<MarkedCell> ();

        [HideInInspector]
        public bool expanded;
        [HideInInspector]
        public int layer = -1;
        [HideInInspector]
        public LayerScan scan;
        [HideInInspector]
        public AreaSceneBlackboard bb;

        public void Reset ()
        {
            marked.Clear ();
            expanded = false;
            scan = LayerScan.None;
        }

        bool enableButtons => bb.layer == layer;
        bool enableSetButton => enableButtons && scan != LayerScan.Unresolved;
        string buttonText => scan == LayerScan.FullOverEmpty ? "Set full" : "Set interior";
        string tooltipText => scan == LayerScan.FullOverEmpty
            ? "Set top self for marked cells on the current layer to full."
            : "Convert marked cells on the current layer to interior tiles.\n\nClears all tileset information in the marked cells.";
    }

    [HideLabel, HideReferenceObjectPicker]
    sealed class MarkedCell
    {
        [ShowInInspector]
        [TableColumnWidth (50, Resizable = false)]
        [DisplayAsString, HideLabel, EnableGUI]
        public int index => cell.spotIndex;

        [ShowInInspector]
        [DisplayAsString, HideLabel, EnableGUI]
        public string coordinate => string.Format ("({0}, {1})", cell.pointPositionIndex.x, cell.pointPositionIndex.z);

        [TableColumnWidth (65, Resizable = false)]
        [Button ("$" + nameof(selectButtonText))]
        public void Select ()
        {
            if (isSelected)
            {
                bb.selectedSpots.Remove (index);
                return;
            }

            var layer = cell.pointPositionIndex.y;
            if (bb.selectionLayer != layer)
            {
                bb.selectedSpots.Clear ();
                bb.selectionLayer = layer;
            }
            bb.selectedSpots.Add (index);
        }

        [TableColumnWidth (65, Resizable = false)]
        [Button]
        public void Inspect () => AreaSceneModeHelper.InspectCell (bb, cell);

        bool isSelected => bb.selectionLayer == cell.pointPositionIndex.y && bb.selectedSpots.Contains (index);
        string selectButtonText => isSelected ? "Deselect" : "Select";

        [HideInInspector]
        public AreaSceneBlackboard bb;
        [HideInInspector]
        public AreaVolumePoint cell;
    }

    static class EncapsulatedSpaceScanner
    {
        public static void Scan (AreaSceneBlackboard bb, bool log)
        {
            trace = bb.capsuleScanTrace && !string.IsNullOrWhiteSpace (bb.capsuleScanTraceFilePath);
            if (!trace)
            {
                ScanInternal (bb, log);
                return;
            }
            using (traceout = new StreamWriter (File.Open (bb.capsuleScanTraceFilePath, FileMode.Create, FileAccess.Write)))
            {
                traceout.WriteLine ("Scan start: {0:u}", System.DateTime.UtcNow);
                #if PB_MODSDK
                if (DataContainerModData.hasSelectedConfigs)
                {
                    traceout.WriteLine ("Mod: " + DataContainerModData.selectedMod.id);
                }
                else
                {
                    traceout.WriteLine ("SDK");
                }
                #endif
                traceout.WriteLine ("Area: " + bb.am.areaName);
                traceout.WriteLine ();
                ScanInternal (bb, log);
            }
        }

        static void ScanInternal (AreaSceneBlackboard bb, bool log)
        {
            var am = bb.am;
            var layerSize = bb.am.boundsFull.x * bb.am.boundsFull.z;
            var points = am.points;

            encapsulated.Clear ();
            var bottomStop = points.Count - layerSize;
            for (var i = points.Count - 1; i >= bottomStop; i -= 1)
            {
                var spot = points[i];
                connected.Clear ();
                EncapsulateSpot (bb, spot, false);
                CoalesceSpaces ();
            }
            for (var i = points.Count - layerSize - 1; i >= 0; i -= 1)
            {
                var spot = points[i];
                connected.Clear ();
                var bottomResult = EncapsulateSpot (bb, spot, true);
                var topResult = EncapsulateSpot (bb, spot, false);
                if (bottomResult == EncapsulateResult.Unconnectable && topResult == EncapsulateResult.Unconnectable)
                {
                    continue;
                }
                if (trace)
                {
                    traceout.WriteLine ("encapsulate result | bottom: {0} | top: {1} | connected: {2}", bottomResult, topResult, connected.ToStringFormatted ());
                }
                CoalesceSpaces ();
                if (topResult != EncapsulateResult.Solo)
                {
                    if (trace)
                    {
                        traceout.WriteLine ();
                    }
                    continue;
                }
                if (spot.pointState != AreaVolumePointState.Empty)
                {
                    if (trace)
                    {
                        traceout.WriteLine ();
                    }
                    continue;
                }
                var eset = new HashSet<int> ()
                {
                    spot.spotIndex,
                };
                encapsulated.Add (eset);
                if (trace)
                {
                    traceout.WriteLine ("solo | space: {0} | spot: {1}", encapsulated.Count - 1, spot.spotIndex);
                    traceout.WriteLine ();
                }
            }

            bb.manualSpotMarking = false;
            bb.markedSpotsByLayer.Clear ();
            if (encapsulated.Count == 1)
            {
                // There's only a single contiguous space in the level.
                // That's what we want to see.
                StartCapsuleLog (bb, 0);
                return;
            }

            // Filter out the largest encapsulated space. I'm assuming this is
            // the level. The rest are discontiguous spaces that are probably
            // artifacts.
            var maxSize = 0;
            var level = -1;
            for (var i = 0; i < encapsulated.Count; i += 1)
            {
                if (maxSize < encapsulated[i].Count)
                {
                    maxSize = encapsulated[i].Count;
                    level = i;
                }
            }
            if (level != -1)
            {
                StartCapsuleLog (bb, level);
                encapsulated.RemoveAt (level);
            }

            foreach (var set in encapsulated)
            {
                foreach (var i in set)
                {
                    var index = Mathf.Abs (i);
                    var layer = points[index].pointPositionIndex.y;
                    if (!bb.markedSpotsByLayer.TryGetValue (layer, out var spots))
                    {
                        spots = new HashSet<int> ();
                        bb.markedSpotsByLayer[layer] = spots;
                    }
                    spots.Add (index);
                }
                if (log)
                {
                    Debug.Log ("Capsule (" + set.Count + ")\n" + set.OrderBy (i => i).Select (i => i + "/" + SpotPositionAsString (points[i])).ToStringFormatted (true));
                }
            }
            LogAllCapsules (bb);
        }

        static EncapsulateResult EncapsulateSpot (AreaSceneBlackboard bb, AreaVolumePoint spot, bool down) =>
            !spot.spotPresent
                ? EncapsulateResult.Unconnectable
                : AreaManager.IsSpotInterior (spot)
                    ? EncapsulateResult.Unconnectable
                    : AreaSceneHelper.IsFreeSpace (spot) && spot.blockTileset == 0
                        ? EncapsulateResult.Unconnectable
                        : ConnectToSpace (bb, spot, down);

        static void CoalesceSpaces ()
        {
            if (connected.Count < 2)
            {
                return;
            }

            var ordered = connected.OrderBy (i => i).ToList ();
            var contiguous = encapsulated[ordered.First ()];
            if (trace)
            {
                traceout.WriteLine ("coalesce | contiguous: {0} | spot count: {1}", ordered.First (), contiguous.Count);
            }
            foreach (var i in ordered.Skip (1).Reverse ())
            {
                var space = encapsulated[i];
                contiguous.UnionWith (space);
                if (trace)
                {
                    traceout.WriteLine ("coalesce | space: {0} | spot count: {1}", i, space.Count);
                }
                encapsulated.RemoveAt (i);
            }
        }

        static EncapsulateResult ConnectToSpace (AreaSceneBlackboard bb, AreaVolumePoint spot, bool down)
        {
            var added = false;
            var rowSize = bb.am.boundsFull.x;
            var layerSize = down ? rowSize * bb.am.boundsFull.z : 0;
            var points = bb.am.points;
            var outOfBounds = points.Count;
            if (spot.spotIndex >= outOfBounds)
            {
                return EncapsulateResult.Unconnectable;
            }

            var bit = down ? TilesetUtility.configurationBitBottomSelf : TilesetUtility.configurationBitTopSelf;
            var empty = spot.pointState == AreaVolumePointState.Empty;
            if (trace)
            {
                traceout.WriteLine ("connect | down: {0} | spot: {1} | empty: {2}", down, spot.spotIndex, empty);
            }
            added |= TryNeighborSpaces (spot, bit, (byte)(spot.spotConfiguration >> 4 & TilesetUtility.configurationFloor), layerSize, rowSize, outOfBounds);

            var neighborIndex = spot.spotIndex + layerSize;
            if (down)
            {
                var downEmpty = (spot.spotConfiguration & bit) == 0;
                var connectable = empty && downEmpty;
                if (connectable)
                {
                    if (trace)
                    {
                        traceout.WriteLine ("down connectable | configuration: {0} | neighbor: {1}", AreaSceneHelper.GetPointConfigurationDisplayString (spot.spotConfiguration), neighborIndex);
                    }
                    added |= TryAddToSpace (spot.spotIndex, neighborIndex);
                    return added ? EncapsulateResult.Connected : EncapsulateResult.Solo;
                }
                if (downEmpty)
                {
                    if (trace)
                    {
                        traceout.WriteLine ("down empty | configuration: {0} | neighbor: {1}", AreaSceneHelper.GetPointConfigurationDisplayString (spot.spotConfiguration), neighborIndex);
                    }
                    added |= TryAddToSpace (-spot.spotIndex, neighborIndex);
                }
                if (added || !IsVerticalConnectPossible(points, spot.spotConfiguration, neighborIndex))
                {
                    return added ? EncapsulateResult.Connected : EncapsulateResult.Solo;
                }
                added |= TryAddToSpace (spot.spotIndex, neighborIndex);
                return added ? EncapsulateResult.Connected : EncapsulateResult.Solo;
            }

            if ((spot.spotConfiguration & TilesetUtility.configurationFloor) == 0)
            {
                bit >>= 4;
                if (trace)
                {
                    traceout.WriteLine ("connect ceiling | spot: {0} | bit: {1} | configuration: {2}", spot.spotIndex, bit, AreaSceneHelper.GetPointConfigurationDisplayString (spot.spotConfiguration));
                }
                added |= TryNeighborSpaces (spot, bit, TilesetUtility.configurationFloor, layerSize, rowSize, outOfBounds);
                return added ? EncapsulateResult.Connected : EncapsulateResult.Solo;
            }

            if (neighborIndex < outOfBounds && empty && !added)
            {
                added |= TryAddToSpace (spot.spotIndex, neighborIndex);
            }
            // if (!empty && !added)
            // {
            //     added |= TryAddToSpace (-spot.spotIndex, -spot.spotIndex);
            // }
            return added ? EncapsulateResult.Connected : EncapsulateResult.Solo;
        }

        static bool TryNeighborSpaces (AreaVolumePoint spot, byte bit, byte connectable, int layerSize, int rowSize, int outOfBounds)
        {
            var added = false;
            if (trace)
            {
                traceout.WriteLine ("neighbor spaces | spot: {0} | bit: {1} | connectable: {2:X2} | configuration: {3}", spot.spotIndex, bit == TilesetUtility.configurationBitBottomSelf ? "self bottom" : "self top", connectable, AreaSceneHelper.GetPointConfigurationDisplayString (spot.spotConfiguration));
            }
            foreach (var offset in neighborOffsets)
            {
                bit >>= 1;
                if ((spot.spotConfiguration & bit) == bit)
                {
                    continue;
                }
                var neighborIndex = spot.spotIndex + offset.x + offset.z * rowSize + layerSize;
                if (neighborIndex >= outOfBounds)
                {
                    continue;
                }
                var spotIndex = spot.spotIndex * ((connectable & bit) == bit ? -1 : 1);
                added |= TryAddToSpace (spotIndex, neighborIndex);
            }
            return added;
        }

        static bool TryAddToSpace (int spotIndex, int neighborIndex)
        {
            var added = false;
            if (trace)
            {
                traceout.WriteLine ("try add | spot: {0} | neighbor: {1}", spotIndex, neighborIndex);
            }
            for (var i = 0; i < encapsulated.Count; i += 1)
            {
                var set = encapsulated[i];
                if (!set.Contains (neighborIndex))
                {
                    continue;
                }
                if (trace)
                {
                    traceout.WriteLine ("try add found | space: {0} | spots ({1}): {2}", i, set.Count, set.Count < 40 ? set.OrderBy (idx => idx).ToStringFormatted () : "...");
                }
                set.Add (spotIndex);
                added = true;
                if (spotIndex < 0)
                {
                    continue;
                }
                connected.Add (i);
                if (trace)
                {
                    traceout.WriteLine ("try add connect | space: " + i);
                }
            }
            return added;
        }

        static bool IsVerticalConnectPossible (List<AreaVolumePoint> points, byte spotConfiguration, int neighborIndex)
        {
            if (trace)
            {
                traceout.Write ("vertical | configuration: {0} | neighbor: {1}", AreaSceneHelper.GetPointConfigurationDisplayString (spotConfiguration), neighborIndex);
            }
            if (neighborIndex >= points.Count)
            {
                if (trace)
                {
                    traceout.WriteLine (" | no connect");
                }
                return false;
            }
            if ((spotConfiguration & TilesetUtility.configurationFloor) == TilesetUtility.configurationFloor)
            {
                if (trace)
                {
                    traceout.WriteLine (" | no connect");
                }
                return false;
            }

            var spot = points[neighborIndex];
            if (AreaManager.IsSpotInterior (spot))
            {
                if (trace)
                {
                    traceout.WriteLine (" | no connect");
                }
                return false;
            }
            if (AreaSceneHelper.IsFreeSpace (spot))
            {
                if (trace)
                {
                    traceout.WriteLine (" | no connect");
                }
                return false;
            }
            if ((spot.spotConfiguration & TilesetUtility.configurationTopMask) >> 4 != (spotConfiguration & TilesetUtility.configurationFloor))
            {
                if (trace)
                {
                    traceout.WriteLine (" | no connect");
                }
                return false;
            }
            if (trace)
            {
                traceout.WriteLine (" | neigbor configuration: " + AreaSceneHelper.GetPointConfigurationDisplayString (spot.spotConfiguration));
            }
            return true;
        }

        static void StartCapsuleLog (AreaSceneBlackboard bb, int level)
        {
            if (!bb.enableCapsuleScanLogToFile || string.IsNullOrWhiteSpace (bb.capsuleScanLogFilePath))
            {
                return;
            }

            var points = bb.am.points;
            try
            {
                using (var outp = new StreamWriter (File.Open (bb.capsuleScanLogFilePath, FileMode.Create, FileAccess.Write)))
                {
                    outp.WriteLine ("Scan start: {0:u}", System.DateTime.UtcNow);
                    outp.WriteLine ("Area: " + bb.am.areaName);
                    var space = encapsulated[level];
                    outp.WriteLine ("Main space: " + space.Count);
                    if (encapsulated.Count != 1)
                    {
                        var capsuleCounts = encapsulated
                            .Select ((set, i) => new
                            {
                                Index = i,
                                Set = set,
                            })
                            .Where (x => x.Index != level)
                            .Select (x => x.Set.Count);
                        outp.WriteLine ("Capsules ({0}): {1}", encapsulated.Count - 1, string.Join (",", capsuleCounts));
                    }
                    outp.WriteLine ("Main space");
                    foreach (var index in space.Select(Mathf.Abs).OrderBy (i => i))
                    {
                        var spot = points[index];
                        var pos = spot.pointPositionIndex;
                        outp.WriteLine ("  {0}/({1}, {2}, {3})", spot.spotIndex, pos.x, pos.z, pos.y);
                    }
                }
            }
            catch (IOException ioe)
            {
                Debug.LogError ("Error opening capsule scan log file for writing: " + bb.capsuleScanLogFilePath);
                Debug.LogException (ioe);
            }
        }

        static void LogAllCapsules (AreaSceneBlackboard bb)
        {
            if (!bb.enableCapsuleScanLogToFile || string.IsNullOrWhiteSpace (bb.capsuleScanLogFilePath))
            {
                return;
            }

            var points = bb.am.points;
            try
            {
                using (var outp = new StreamWriter (File.Open (bb.capsuleScanLogFilePath, FileMode.Append, FileAccess.Write)))
                {
                    foreach (var set in encapsulated)
                    {
                        outp.WriteLine ("Capsule ({0})", set.Count);
                        foreach (var index in set.Select(Mathf.Abs).OrderBy (i => i))
                        {
                            var spot = points[index];
                            var pos = spot.pointPositionIndex;
                            outp.WriteLine ("  {0}/({1}, {2}, {3})", spot.spotIndex, pos.x, pos.z, pos.y);
                        }
                    }
                }
            }
            catch (IOException ioe)
            {
                Debug.LogError ("Error opening capsule scan log file for writing: " + bb.capsuleScanLogFilePath);
                Debug.LogException (ioe);
            }
        }

        static string SpotPositionAsString (AreaVolumePoint spot)
        {
            var pos = spot.pointPositionIndex;
            return string.Format ("({0}, {1}, {2})", pos.x, pos.z, pos.y);
        }

        static bool trace;
        static StreamWriter traceout;

        static readonly List<HashSet<int>> encapsulated = new List<HashSet<int>> ();
        static readonly HashSet<int> connected = new HashSet<int> ();

        static readonly Vector3Int[] neighborOffsets =
        {
            new Vector3Int (1, 0, 0),
            new Vector3Int (1, 0, 1),
            new Vector3Int (0, 0, 1),
        };

        enum EncapsulateResult
        {
            Unconnectable = 0,
            Solo,
            Connected,
        }
    }
}
