using Sirenix.Utilities;

using UnityEditor;
using UnityEngine;

namespace Area
{
    using Scene;

    sealed class AreaSceneTilesetModePanel : AreaSceneModePanel
    {
        public static AreaSceneModePanel Create (AreaSceneBlackboard bb) => new AreaSceneTilesetModePanel (bb);

        public GUILayoutOptions.GUILayoutOptionsInstance Width => AreaScenePanelDrawer.ModePanelWidth;
        public string Title => "Tileset editing mode";

        public void OnDisable ()
        {
            tilesetSelection.OnDisable ();
        }

        public void Draw ()
        {
            if (Event.current.type == EventType.Layout)
            {
                spotCached = bb.lastSpotHovered;
            }

            if (spotCached == null)
            {
                AreaSceneModePanelHelper.DrawSpotUnknown ();
                return;
            }

            GUILayout.BeginVertical ("Box");
            GUILayout.Label ("Spot: " + spotCached.spotIndex, EditorStyles.miniLabel);
            GUILayout.EndVertical ();

            GUILayout.Space (4f);
            GUILayout.BeginVertical ("Box");
            AreaSceneModePanelHelper.DrawSearchSelector (bb);
            GUILayout.EndVertical ();

            GUILayout.Space (4f);
            GUILayout.BeginVertical ("Box");
            tilesetSelection.Draw ();
            GUILayout.EndVertical ();

            GUILayout.Space (4f);
            GUILayout.BeginVertical ("Box");
            GUILayout.Label ("Spot tileset");
            var configuration = spotCached.spotHasDamagedPoints ? spotCached.spotConfigurationWithDamage : spotCached.spotConfiguration;
            GUILayout.Label (string.Format ("█ {0} ({1})", configuration, AreaSceneHelper.GetPointConfigurationDisplayString (configuration)));
            GUILayout.Label ("█ " + AreaSceneHelper.GetTilesetDisplayName (spotCached.blockTileset));
            GUILayout.EndVertical ();
        }

        public AreaSceneModeHints Hints => hints;

        AreaSceneTilesetModePanel (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            tilesetSelection = new TilesetSelection (bb, EditingMode.Tileset);
        }

        readonly AreaSceneBlackboard bb;
        readonly TilesetSelection tilesetSelection;
        AreaVolumePoint spotCached;

        static readonly AreaSceneModeHints hints = new AreaSceneModeHints ()
        {
            HintText = "[LMB] - Set tileset     [MMB] - Pick tileset     [MW\u25b2\u25bc] - Select tileset",
        };
    }
}
