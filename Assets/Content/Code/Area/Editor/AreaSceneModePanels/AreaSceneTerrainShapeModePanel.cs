using Sirenix.OdinInspector;
using Sirenix.Utilities;

using UnityEngine;

namespace Area
{
    using Scene;

    sealed class AreaSceneTerrainShapeModePanel : AreaSceneModePanel
    {
        public GUILayoutOptions.GUILayoutOptionsInstance Width => AreaScenePanelDrawer.ModePanelWidth;
        public string Title => "Terrain shape mode";

        public void OnDisable ()
        {
            pointDisplay.OnDisable ();
            brushSelector.OnDisable ();
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
        }

        public AreaSceneModeHints Hints => hints;

        public AreaSceneTerrainShapeModePanel (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            brushSelector = new BrushSelector (bb);
        }

        readonly AreaSceneBlackboard bb;
        readonly TerrainShapeModePanelPointDisplay pointDisplay = new TerrainShapeModePanelPointDisplay ();
        readonly BrushSelector brushSelector;
        AreaVolumePoint pointCached;
        bool cachedHover;

        static readonly AreaSceneModeHints hints = new AreaSceneModeHints ()
        {
            HintText = "[LMB] - Add block      [RMB] - Remove block      [MW▲▼] - Change brush"
                + "\n[Shift + LMB] - Increase terrain offset      [Shift + RMB] - Decrease terrain offset",
        };
    }

    sealed class TerrainShapeModePanelPointDisplay : SelfDrawnGUI
    {
        [ShowInInspector]
        [GUIColor (nameof(color))]
        [HideLabel, DisplayAsString, EnableGUI]
        public string pointDisplay => hasPoint
            ? (hover ? "Point: " : "Last point: ") + point.spotIndex
            : "Point: —";

        [ShowInInspector]
        [ShowIf (nameof(hasOffset))]
        [HideLabel, DisplayAsString, EnableGUI]
        public string terrainOffset => "Terrain offset: " + (hasOffset
            ? point.pointsWithSurroundingSpots[WorldSpace.SpotNeighbor.Up].terrainOffset.ToString ("+0.##;-0.##")
            : "<none>");

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
        bool hasOffset => hasPoint && point.pointsWithSurroundingSpots[WorldSpace.SpotNeighbor.Up] != null && point.pointsWithSurroundingSpots[WorldSpace.SpotNeighbor.Up].terrainOffset != 0f;

        AreaVolumePoint point;
        bool hover;
    }
}
