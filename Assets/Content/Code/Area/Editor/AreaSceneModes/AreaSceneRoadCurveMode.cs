using UnityEngine;

namespace Area
{
    using Scene;

    sealed class AreaSceneRoadCurveMode : AreaSceneMode
    {
        public EditingMode EditingMode => EditingMode.RoadCurves;

        public AreaSceneModePanel Panel { get; }

        public void OnDisable () => Panel.OnDisable ();
        public void OnDestroy () { }

        public int LayerMask => AreaSceneCamera.environmentLayerMask;

        public bool Hover (Event e, RaycastHit hitInfo)
        {
            if (!AreaSceneModeHelper.DisplaySpotCursor (bb, hitInfo))
            {
                return false;
            }
            var (eventType, button) = AreaSceneModeHelper.ResolveEvent (e);
            switch (eventType)
            {
                case EventType.MouseDown:
                    bb.am.EditRoadCurves (bb.lastSpotHovered.spotIndex, button, e.shift);
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

        public static AreaSceneMode CreateInstance (AreaSceneBlackboard bb) => new AreaSceneRoadCurveMode (bb);

        AreaSceneRoadCurveMode (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            Panel = new AreaSceneRoadCurveModePanel ();
        }

        readonly AreaSceneBlackboard bb;
    }
}
