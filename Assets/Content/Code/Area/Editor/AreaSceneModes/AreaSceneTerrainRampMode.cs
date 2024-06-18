using UnityEngine;

namespace Area
{
    using Scene;

    sealed class AreaSceneTerrainRampMode : AreaSceneMode
    {
        public EditingMode EditingMode => EditingMode.TerrainRamp;

        public AreaSceneModePanel Panel { get; }

        public void OnDisable () => Panel.OnDisable ();
        public void OnDestroy () { }

        public int LayerMask => AreaSceneCamera.environmentLayerMask;

        public bool Hover (Event e, RaycastHit hitInfo)
        {
            if (!AreaSceneModeHelper.DisplayVolumeCursor (bb, hitInfo, cursorID))
            {
                return false;
            }
            bb.gizmos.DrawWireframesForVolume (e.shift);
            var (eventType, button) = AreaSceneModeHelper.ResolveEvent (e);
            switch (eventType)
            {
                case EventType.MouseDown:
                    Edit (button, e.shift);
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

        void Edit (KeyCode button, bool shift)
        {
            var pointStart = bb.lastPointHovered;
            var spotIndex = pointStart.spotIndex;
            switch (button)
            {
                case KeyCode.Mouse0:
                case KeyCode.Mouse1:
                {
                    var proximityCheck = shift ? AreaManager.SlopeProximityCheck.LateralSingle : AreaManager.SlopeProximityCheck.None;
                    bb.am.TrySettingSlope(pointStart, button == KeyCode.Mouse0, true, proximityCheck, true);
                    break;
                }
                case KeyCode.Mouse2 when spotIndex.IsValidIndex(bb.am.points):
                {
                    var pointForSpot = bb.am.points[spotIndex];
                    bb.clipboardTilesetInfo.Tileset = pointForSpot.blockTileset;
                    break;
                }
            }
        }

        public static AreaSceneMode CreateInstance (AreaSceneBlackboard bb) => new AreaSceneTerrainRampMode (bb);

        AreaSceneTerrainRampMode (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            var cursor = new AreaSceneVolumeCursor (bb.gizmos.cursor);
            cursorID = cursor.ID;
            Panel = new AreaSceneTerrainRampModePanel ();
        }

        readonly AreaSceneBlackboard bb;
        readonly int cursorID;
    }
}
