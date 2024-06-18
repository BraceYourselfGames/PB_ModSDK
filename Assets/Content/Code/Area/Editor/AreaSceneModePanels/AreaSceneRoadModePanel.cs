using Sirenix.OdinInspector;
using Sirenix.Utilities;

using UnityEngine;

namespace Area
{
    sealed class AreaSceneRoadModePanel : AreaSceneModePanel
    {
        public GUILayoutOptions.GUILayoutOptionsInstance Width => AreaScenePanelDrawer.ModePanelWidth;
        public string Title => "Road tool";

        public void OnDisable ()
        {
            pointDisplay.OnDisable ();
            brushSelector.OnDisable ();
            subtypeSelector.OnDisable ();
        }

        public void Draw ()
        {
            if (Event.current.type == EventType.Layout)
            {
                pointCached = bb.lastPointHovered;
                cachedHover = bb.hoverActive;
            }

            GUILayout.Space (4f);
            pointDisplay.Draw (pointCached, cachedHover);

            if (pointCached == null)
            {
                return;
            }

            GUILayout.Space (4f);
            GUILayout.BeginVertical ("Box");
            brushSelector.Draw ();
            GUILayout.EndVertical ();

            GUILayout.Space (8f);
            GUILayout.BeginVertical ("Box");
            subtypeSelector.Draw ();
            GUILayout.EndVertical ();
        }

        public AreaSceneModeHints Hints => hints;

        public AreaSceneRoadModePanel (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            brushSelector = new BrushSelector (bb);
        }

        readonly AreaSceneBlackboard bb;
        readonly RoadModePanelPointDisplay pointDisplay = new RoadModePanelPointDisplay ();
        readonly RoadSubtypeSelector subtypeSelector = new RoadSubtypeSelector ();
        readonly BrushSelector brushSelector;
        AreaVolumePoint pointCached;
        bool cachedHover;

        static readonly AreaSceneModeHints hints = new AreaSceneModeHints ()
        {
            HintText = "[LMB] - Add     [RMB] - Remove    [Shift + MW▲▼] - Change brush\n[MMB] - Flood-fill road type     [MW▲▼] - Swap type",
        };
    }

    sealed class RoadModePanelPointDisplay : SelfDrawnGUI
    {
        [ShowInInspector]
        [GUIColor (nameof(color))]
        [HideLabel, DisplayAsString, EnableGUI]
        public string pointDisplay => hasPoint
            ? (hover ? "Point: " : "Last point: ") + point.spotIndex + (isRoad ? " (road)" : "")
            : "Point: —";

        public void Draw (AreaVolumePoint pointHovered, bool hoverActive)
        {
            point = pointHovered;
            hover = hoverActive;

            GUILayout.BeginVertical ("Box");
            base.Draw ();
            GUILayout.EndVertical ();
        }

        Color color => point == null ? Color.gray : hover ? Color.white : Color.yellow;
        bool hasPoint => point != null;
        bool isRoad => hasPoint && point.road;

        AreaVolumePoint point;
        bool hover;
    }
}
