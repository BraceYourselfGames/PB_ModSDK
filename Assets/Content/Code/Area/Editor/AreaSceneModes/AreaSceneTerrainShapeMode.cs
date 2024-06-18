using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Area
{
    using Scene;

    sealed class AreaSceneTerrainShapeMode : AreaSceneMode
    {
        public EditingMode EditingMode => EditingMode.TerrainShape;

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
            var (eventType, button) = AreaSceneModeHelper.ResolveEvent (e);
            isOffsetOperation = e.shift;
            if (e.type == EventType.MouseDown && !isOffsetOperation)
            {
                pointChecker.operation = button == KeyCode.Mouse0 ? TerrainPointChecker.Operation.Add : TerrainPointChecker.Operation.Remove;
            }
            else
            {
                pointChecker.operation = TerrainPointChecker.Operation.Hover;
            }
            if (!AreaSceneModeHelper.DisplayVolumeCursor (bb, hitInfo, cursorID, isOffsetOperation ? changeTerrainOffsetChecks.pointChecker : (VolumePointChecker)pointChecker))
            {
                return false;
            }

            var status = pointChecker.denyTerrainEdit ? VolumeSelectionStatus.Warning : VolumeSelectionStatus.OK;
            AreaSceneModeHelper.DrawVolumeSelectionHandles (bb, status);

            switch (eventType)
            {
                case EventType.ScrollWheel:
                    AreaSceneModeHelper.ChangeVolumeBrush (bb, e);
                    return true;
                case EventType.MouseDown:
                    Edit (hitInfo.normal, button);
                    return true;
                case EventType.MouseDrag:
                    if (isOffsetOperation)
                    {
                        return false;
                    }
                    if (pointChecker.denyTerrainEdit)
                    {
                        return true;
                    }
                    if (lastOpIndex == bb.lastPointHovered.spotIndex)
                    {
                        return true;
                    }
                    if (lastLayer != bb.lastPointHovered.pointPositionIndex.y)
                    {
                        return true;
                    }
                    Edit (hitInfo.normal, button);
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

        void Edit (Vector3 direction, KeyCode button)
        {
            lastOpIndex = lastLayer = -1;

            var (ok, op) = GetOperation (button, isOffsetOperation);
            if (!ok)
            {
                return;
            }
            if (!op.TryGetTargetBlocks (bb.lastPointHovered, direction))
            {
                return;
            }
            if (!op.Apply ())
            {
                return;
            }

            var sceneHelper = CombatSceneHelper.ins;
            if (sceneHelper != null)
            {
                sceneHelper.terrain.Rebuild (true);
            }
            lastOpIndex = bb.lastPointHovered.spotIndex;
            lastLayer = bb.lastPointHovered.pointPositionIndex.y;
        }

        (bool, AreaSceneVolumeOperation) GetOperation (KeyCode button, bool shift)
        {
            switch (button)
            {
                case KeyCode.Mouse0:
                    return (true, shift
                        ? (AreaSceneVolumeOperation)new ChangeTerrainOffsetOperation (bb, changeTerrainOffsetChecks, ChangeTerrainOffsetOperation.OffsetDirection.Up)
                        : new TerrainAddOperation (bb, addChecks));
                case KeyCode.Mouse1:
                    return (true, shift
                        ? (AreaSceneVolumeOperation)new ChangeTerrainOffsetOperation (bb, changeTerrainOffsetChecks, ChangeTerrainOffsetOperation.OffsetDirection.Down)
                        : new TerrainRemoveOperation (bb, removeChecks));
                default:
                    return (false, null);
            }
        }

        public static bool CheckBrushPoints (AreaVolumePoint pointStart, List<AreaVolumePoint> pointsToEdit, bool upSpots, AreaSceneHelper.FreeSpacePolicy freeSpacePolicy, bool log)
        {
            var pointsBrush = AreaManager.CollectPointsInBrush (pointStart, AreaManager.editingVolumeBrush);
            if (log)
            {
                Debug.Log ("Edit terrain | brush points (" + pointsBrush.Count + "): " + pointsBrush.Select (pt => pt.spotIndex).ToStringFormatted ());
            }
            var pointsValid = new List<AreaVolumePoint> ();
            foreach (var point in pointsBrush)
            {
                var result = AreaSceneHelper.VolumePointAllTerrain (point, upSpots, freeSpacePolicy);
                if (result == AreaSceneHelper.TerrainResult.Error)
                {
                    if (log)
                    {
                        Debug.Log ("Edit terrain -- brush point not terrain | index: " + point.spotIndex);
                    }
                    return false;
                }
                if (result == AreaSceneHelper.TerrainResult.FreeSpace)
                {
                    if (freeSpacePolicy != AreaSceneHelper.FreeSpacePolicy.LookDownPass)
                    {
                        continue;
                    }
                    var neighborDown = point.pointsInSpot[WorldSpace.PointNeighbor.Down];
                    pointsValid.Add (neighborDown);
                    if (log)
                    {
                        Debug.Log ("Edit terrain -- look down policy added down neighbor | index: " + neighborDown.spotIndex);
                    }
                }
                pointsValid.Add (point);
            }
            pointsToEdit.Clear ();
            pointsToEdit.AddRange (pointsValid);
            return true;
        }

        int GetCursorMaterialID ()
        {
            var cursor = (AreaSceneVolumeCursor)bb.gizmos.cursor.GetCursor (cursorID);
            if (cursor == null)
            {
                return -1;
            }
            return cursor.showWarning ? cursor.warningMaterialID : cursor.standardMaterialID;
        }

        void OnPointCheckTrigger ()
        {
            var cursor = (AreaSceneVolumeCursor)bb.gizmos.cursor.GetCursor (cursorID);
            if (cursor == null)
            {
                return;
            }
            cursor.showWarning = isOffsetOperation ? changeTerrainOffsetChecks.denyTerrainEdit : pointChecker.denyTerrainEdit;
        }

        void OnEditingModeChanged ()
        {
            if (bb.editingMode != EditingMode.TerrainShape && lastEditingMode != EditingMode.TerrainShape)
            {
                return;
            }
            if (bb.editingMode == EditingMode.TerrainShape && lastEditingMode == EditingMode.TerrainShape)
            {
                return;
            }
            AreaSceneModeHelper.ResetPointer (bb);
            lastEditingMode = bb.editingMode;
        }

        public static AreaSceneMode CreateInstance (AreaSceneBlackboard bb) => new AreaSceneTerrainShapeMode (bb);

        AreaSceneTerrainShapeMode (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            var cursor = new AreaSceneVolumeCursor (bb.gizmos.cursor);
            cursorID = cursor.ID;
            pointChecker = new TerrainPointChecker (bb, GetCursorMaterialID, OnPointCheckTrigger);
            addChecks = new AddChecks (pointChecker);
            removeChecks = new RemoveChecks (pointChecker);
            changeTerrainOffsetChecks = new ChangeTerrainOffsetChecks (bb, GetCursorMaterialID, OnPointCheckTrigger);
            Panel = new AreaSceneTerrainShapeModePanel (bb);
            bb.onEditingModeChanged += OnEditingModeChanged;
            lastEditingMode = bb.editingMode;
        }

        readonly AreaSceneBlackboard bb;
        readonly int cursorID;
        readonly TerrainPointChecker pointChecker;
        readonly AddChecks addChecks;
        readonly RemoveChecks removeChecks;
        readonly ChangeTerrainOffsetChecks changeTerrainOffsetChecks;
        EditingMode lastEditingMode;
        int lastOpIndex = -1;
        int lastLayer = -1;
        bool isOffsetOperation;

        sealed class AddChecks : TerrainAddOperation.Checks
        {
            public bool denyAddBlockTop => pointChecker.denyAddBlock;
            public bool denyTerrainEdit => pointChecker.denyTerrainEdit;
            public System.Action onActionDenied { get; set; }

            public AddChecks (TerrainPointChecker pointChecker)
            {
                this.pointChecker = pointChecker;
            }

            readonly TerrainPointChecker pointChecker;
        }

        sealed class RemoveChecks : TerrainRemoveOperation.Checks
        {
            public bool denyRemoveBlock => pointChecker.denyRemoveBlock;
            public bool denyTerrainEdit => pointChecker.denyTerrainEdit;
            public System.Action onActionDenied { get; set; }

            public RemoveChecks (TerrainPointChecker pointChecker)
            {
                this.pointChecker = pointChecker;
            }

            readonly TerrainPointChecker pointChecker;
        }

        sealed class ChangeTerrainOffsetChecks : ChangeTerrainOffsetOperation.Checks
        {
            public bool denyTerrainEdit => pointChecker.denyTerrainEdit;
            public System.Action onActionDenied { get; set; }

            public ChangeTerrainOffsetChecks (AreaSceneBlackboard bb, TerrainOffsetChecker.GetMaterialID getMaterialID, VolumePointChecker.OnTrigger onTrigger)
            {
                pointChecker = new TerrainOffsetChecker(bb, getMaterialID, onTrigger);
            }

            public readonly TerrainOffsetChecker pointChecker;
        }
    }
}
