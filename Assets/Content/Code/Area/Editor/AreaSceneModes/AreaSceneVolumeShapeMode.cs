using UnityEngine;

namespace Area
{
    using Scene;

    sealed class AreaSceneVolumeShapeMode : AreaSceneMode
    {
        public EditingMode EditingMode => EditingMode.Volume;

        public AreaSceneModePanel Panel { get; }

        public void OnDisable ()
        {
            bb.onEditingModeChanged -= OnEditingModeChanged;
            Panel.OnDisable ();
        }

        public void OnDestroy () { }

        public int LayerMask => AreaSceneCamera.environmentLayerMask;

        public bool Hover (Event e, RaycastHit hitInfo)
        {
            if (!AreaSceneModeHelper.DisplayVolumeCursor (bb, hitInfo, cursorID, pointChecker))
            {
                return false;
            }
            bb.gizmos.DrawWireframesForVolume (e.shift);
            var status = pointChecker.denyAddBlockTop || pointChecker.denyRemoveBlock ? VolumeSelectionStatus.Limited : VolumeSelectionStatus.OK;
            AreaSceneModeHelper.DrawVolumeSelectionHandles (bb, status);

            var (eventType, button) = AreaSceneModeHelper.ResolveEvent (e);
            switch (eventType)
            {
                case EventType.ScrollWheel:
                    if (e.shift)
                    {
                        AreaSceneModeHelper.ChangeVolumeBrush (bb, e);
                        return true;
                    }
                    ChangeSelectedTileset (e);
                    return true;
                case EventType.MouseDown:
                    Edit (bb.lastPointHovered, hitInfo.normal, button);
                    return true;
            }
            return false;
        }

        public void OnHoverEnd () => bb.gizmos.cursor.HideCursor ();

        public bool HandleSceneUserInput (Event e) => false;

        public void DrawSceneMarkup (Event e, System.Action repaint)
        {
            AreaSceneModeHelper.TryRebuildTerrain (bb);
        }

        void ChangeSelectedTileset (Event e)
        {
            var forward = e.delta.y > 0f;
            ref var volumeTilesetSelected = ref bb.volumeTilesetSelected;
            var tilesetIndexCurrent = System.Array.IndexOf (AreaSceneHelper.volumeTilesetIDs, volumeTilesetSelected.id);
            var tilesetIndexOffset = tilesetIndexCurrent.OffsetAndWrap (forward, 0, AreaSceneHelper.volumeTilesetIDs.Length - 1);
            volumeTilesetSelected = AreaTilesetHelper.database.tilesets[AreaSceneHelper.volumeTilesetIDs[tilesetIndexOffset]];
        }

        void Edit (AreaVolumePoint pointStart, Vector3 direction, KeyCode button)
        {
            var (ok, op) = GetOperation (button);
            if (!ok)
            {
                return;
            }
            if (!op.TryGetTargetBlocks (pointStart, direction))
            {
                return;
            }
            var terrainModified = op.Apply ();
            if (!terrainModified)
            {
                return;
            }

            var sceneHelper = CombatSceneHelper.ins;
            if (sceneHelper != null)
            {
                sceneHelper.terrain.Rebuild (true);
            }
        }

        (bool, AreaSceneVolumeOperation) GetOperation (KeyCode button)
        {
            switch (button)
            {
                case KeyCode.Mouse0:
                    return (true, new VolumeAddOperation (bb, checks));
                case KeyCode.Mouse1:
                    return (true, new VolumeRemoveOperation(bb, checks));
                default:
                    return (false, null);
            }
        }

        void StartWarning ()
        {
            var cursor = (AreaSceneVolumeCursor)bb.gizmos.cursor.GetCursor (cursorID);
            if (cursor == null)
            {
                return;
            }

            cursor.showWarning = true;
            cursor.warningTimeStart = Time.realtimeSinceStartup;
            checks.onWarning ();
        }

        int GetCursorMaterialID ()
        {
            var cursor = (AreaSceneVolumeCursor)bb.gizmos.cursor.GetCursor (cursorID);
            if (cursor == null)
            {
                return -1;
            }

            if (cursor.showWarning && warningDuration > Time.realtimeSinceStartup - cursor.warningTimeStart)
            {
                return cursor.warningMaterialID;
            }
            cursor.showWarning = false;
            if (pointChecker.denyAddBlockTop || pointChecker.denyRemoveBlock)
            {
                return cursor.limitedMaterialID;
            }
            return cursor.standardMaterialID;
        }

        void OnEditingModeChanged ()
        {
            if (bb.editingMode != EditingMode.Volume && lastEditingMode != EditingMode.Volume)
            {
                return;
            }
            if (bb.editingMode == EditingMode.Volume && lastEditingMode == EditingMode.Volume)
            {
                return;
            }
            AreaSceneModeHelper.ResetPointer (bb);
            lastEditingMode = bb.editingMode;
        }

        public static AreaSceneMode CreateInstance (AreaSceneBlackboard bb) => new AreaSceneVolumeShapeMode (bb);

        AreaSceneVolumeShapeMode (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            var cursor = new AreaSceneVolumeCursor (bb.gizmos.cursor);
            cursorID = cursor.ID;
            pointChecker = new TopBottomVolumePointChecker (bb, GetCursorMaterialID);
            checks = new Checks (pointChecker);
            checks.onActionDenied += StartWarning;
            Panel = new AreaSceneVolumeModePanel (bb, checks);
            bb.onEditingModeChanged += OnEditingModeChanged;
            lastEditingMode = bb.editingMode;
        }

        readonly AreaSceneBlackboard bb;
        readonly int cursorID;
        readonly TopBottomVolumePointChecker pointChecker;
        readonly Checks checks;
        EditingMode lastEditingMode;

        const float warningDuration = 0.75f;

        sealed class Checks : AreaSceneVolumeModePanel.Checks, VolumeAddOperation.Checks, VolumeRemoveOperation.Checks
        {
            public bool denyAddBlockTop => pointChecker.denyAddBlockTop;
            public bool denyRemoveBlock => pointChecker.denyRemoveBlock;
            public System.Action onWarning { get; set; }
            public System.Action onActionDenied { get; set; }

            public Checks (TopBottomVolumePointChecker pointChecker)
            {
                this.pointChecker = pointChecker;
            }

            readonly TopBottomVolumePointChecker pointChecker;
        }
    }
}
