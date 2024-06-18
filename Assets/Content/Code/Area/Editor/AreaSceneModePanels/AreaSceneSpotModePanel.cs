using Sirenix.Utilities;

using UnityEditor;
using UnityEngine;

namespace Area
{
    sealed class AreaSceneSpotModePanel : AreaSceneModePanel
    {
        public GUILayoutOptions.GUILayoutOptionsInstance Width => GUILayoutOptions.MinWidth (250f);
        public string Title => "Spot editing mode";

        public void OnDisable () { }

        public void Draw ()
        {
            if (Event.current.type == EventType.Layout)
            {
                spotCached = bb.lastSpotHovered;
                cachedGroupInfo = bb.lastSpotInfoGroups;
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
            GUILayout.BeginVertical ("Box", GUILayout.MinWidth (250f));
            AreaSceneModePanelHelper.DrawSearchSelector (bb);
            GUILayout.EndVertical ();

            var clipboardTilesetInfo = bb.clipboardTilesetInfo;
            GUILayout.Space (4f);
            GUILayout.BeginVertical ("Box", GUILayout.MinWidth (250f));
            UtilityCustomInspector.DrawField ("Subtype overwriting", () => clipboardTilesetInfo.MustOverwriteSubtype = EditorGUILayout.Toggle (clipboardTilesetInfo.MustOverwriteSubtype));
            UtilityCustomInspector.DrawField ("Color overwriting", () => clipboardTilesetInfo.OverwriteColor = EditorGUILayout.Toggle (clipboardTilesetInfo.OverwriteColor));
            GUILayout.EndVertical ();
            GUILayout.BeginVertical ("Box", GUILayout.MinWidth (250f));
            if (clipboardTilesetInfo.Configurations != null && clipboardTilesetInfo.Configurations.Count > 0 && AreaTilesetHelper.database.tilesets.ContainsKey (clipboardTilesetInfo.Tileset))
            {
                EditorGUILayout.BeginHorizontal ("Box");
                var tileset = AreaTilesetHelper.database.tilesets[clipboardTilesetInfo.Tileset];
                var identifiersPresent = tileset.groupIdentifiers != null;

                GUILayout.Label ("Configs\nTileset\nGroup/type\nOrientation");
                GUILayout.Label (string.Format
                (
                    "{0} ({1})\n{2} ({3})\n{4}/{5}\n{6} ({7})",
                    AreaSceneHelper.GetPointConfigurationDisplayString (clipboardTilesetInfo.Configurations[0]),
                    clipboardTilesetInfo.Configurations[0],
                    tileset.name,
                    clipboardTilesetInfo.Tileset,
                    identifiersPresent && tileset.groupIdentifiers.ContainsKey (clipboardTilesetInfo.Group)
                        ? $"{tileset.groupIdentifiers[clipboardTilesetInfo.Group]} ({clipboardTilesetInfo.Group})"
                        : clipboardTilesetInfo.Group.ToString (),
                    clipboardTilesetInfo.Subtype,
                    clipboardTilesetInfo.Rotation,
                    clipboardTilesetInfo.Flipping ? "flipped" : "standard"
                ));

                EditorGUILayout.EndHorizontal ();
            }
            AreaSceneModePanelHelper.DrawGroupInfo (cachedGroupInfo);
        }

        public AreaSceneModeHints Hints => hints;

        public AreaSceneSpotModePanel (AreaSceneBlackboard bb)
        {
            this.bb = bb;
        }

        readonly AreaSceneBlackboard bb;
        AreaVolumePoint spotCached;
        string cachedGroupInfo;

        static readonly AreaSceneModeHints hints = new AreaSceneModeHints ()
        {
            HintText = "[LMB] - Rotate     [RMB] - Flip     [MMB] - Copy     [MW▲▼] - Change subtype     [Shift + MW▲▼] - Change group"
                + "\n[V] - Paste subtype (using search)     [Shift + V] - Paste everything (only target)     [Q] - Randomize subtype",
        };
    }
}
