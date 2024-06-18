using Sirenix.Utilities;
using Sirenix.Utilities.Editor;

using UnityEditor;
using UnityEngine;

namespace Area
{
    sealed class AreaSceneColorModePanel : AreaSceneModePanel
    {
        public GUILayoutOptions.GUILayoutOptionsInstance Width => AreaScenePanelDrawer.ModePanelWidth;
        public string Title => "Color editing mode";

        public void OnDisable () { }

        public void Draw ()
        {
            if (Event.current.OnEventType(EventType.Layout))
            {
                spotCached = bb.lastSpotHovered;
                cachedTilesetID = bb.lastSpotTilesetID;
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
            //GUILayout.Space (8f);
            //UtilityCustomInspector.DrawField ("Absolute Colors", () => absoluteColorMode = EditorGUILayout.Toggle (absoluteColorMode));
            GUILayout.EndVertical ();

            var tilesetColor = bb.tilesetColor;
            GUILayout.Space (4f);
            GUILayout.BeginVertical ("Box");
            UtilityCustomInspector.DrawField ("Apply colors", () => tilesetColor.ApplyMainOnColorApply = EditorGUILayout.Toggle (tilesetColor.ApplyMainOnColorApply), true);
            GUILayout.Label ("Primary color (RGB)", EditorStyles.miniLabel);
            tilesetColor.SelectedPrimaryColor = HSBColor.FromColor (EditorGUILayout.ColorField (tilesetColor.SelectedPrimaryColor.ToColor ()));
            GUILayout.Space (3f);
            GUILayout.Label ("Primary color (HSV)", EditorStyles.miniLabel);
            tilesetColor.SelectedPrimaryColor.h = EditorGUILayout.Slider (tilesetColor.SelectedPrimaryColor.h, 0f, 1f);
            tilesetColor.SelectedPrimaryColor.s = EditorGUILayout.Slider (tilesetColor.SelectedPrimaryColor.s, 0f, 1f);
            tilesetColor.SelectedPrimaryColor.b = EditorGUILayout.Slider (tilesetColor.SelectedPrimaryColor.b, 0f, 1f);
            GUILayout.EndVertical ();

            GUILayout.BeginVertical ("Box");
            GUILayout.Label ("Secondary color (RGB)", EditorStyles.miniLabel);
            tilesetColor.SelectedSecondaryColor = HSBColor.FromColor (EditorGUILayout.ColorField (tilesetColor.SelectedSecondaryColor.ToColor ()));
            GUILayout.Space (3f);
            GUILayout.Label ("Secondary color (HSV)", EditorStyles.miniLabel);
            tilesetColor.SelectedSecondaryColor.h = EditorGUILayout.Slider (tilesetColor.SelectedSecondaryColor.h, 0f, 1f);
            tilesetColor.SelectedSecondaryColor.s = EditorGUILayout.Slider (tilesetColor.SelectedSecondaryColor.s, 0f, 1f);
            tilesetColor.SelectedSecondaryColor.b = EditorGUILayout.Slider (tilesetColor.SelectedSecondaryColor.b, 0f, 1f);
            tilesetColor.SelectedSecondaryColor.a = EditorGUILayout.Slider (tilesetColor.SelectedSecondaryColor.a, 0f, 1f);
            GUILayout.EndVertical ();

            GUILayout.BeginVertical ("Box");
            GUILayout.Space (5f);
            UtilityCustomInspector.DrawField ("Apply overlays", () => tilesetColor.ApplyOverlaysOnColorApply = EditorGUILayout.Toggle (tilesetColor.ApplyOverlaysOnColorApply), true);
            GUILayout.Space (5f);
            GUILayout.Label ("Overlays / emission", EditorStyles.miniLabel);

            GUILayout.BeginHorizontal ();

            var overrideValue = EditorGUILayout.FloatField (tilesetColor.OverrideValue);
            if (GUILayout.Button ("-1", EditorStyles.miniButton, GUILayout.Width (30f)))
            {
                overrideValue -= 1f;
            }
            if (GUILayout.Button ("-0.1", EditorStyles.miniButton, GUILayout.Width (40f)))
            {
                overrideValue -= 0.1f;
            }
            if (GUILayout.Button ("-", EditorStyles.miniButton, GUILayout.Width (30f)))
            {
                overrideValue = Mathf.RoundToInt (overrideValue * 10f) * 0.1f;
            }
            if (GUILayout.Button ("+0.1", EditorStyles.miniButton, GUILayout.Width (40f)))
            {
                overrideValue += 0.1f;
            }
            if (GUILayout.Button ("+1", EditorStyles.miniButton, GUILayout.Width (30f)))
            {
                overrideValue += 1f;
            }
            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal ();
            if (GUILayout.Button ("Default", EditorStyles.miniButtonLeft))
            {
                overrideValue = 0f;
            }
            if (GUILayout.Button ("Emissive", EditorStyles.miniButtonRight))
            {
                overrideValue = 1f;
            }
            GUILayout.EndHorizontal ();

            overrideValue = Mathf.Clamp (Mathf.RoundToInt (overrideValue * 10f) * 0.1f, -10f, 1f);

            var tileset = AreaTilesetHelper.GetTileset (cachedTilesetID);
            if (tileset != null && tileset.materialOverlays != null && tileset.materialOverlays.Count > 0)
            {
                foreach (var kvp1 in tileset.materialOverlays)
                {
                    var index = -kvp1.Key - 1f;
                    var label = string.Format ("{0} ({1})", kvp1.Value.FirstLetterToUpperCase (), index);

                    GUILayout.Space (2f);
                    if (GUILayout.Button (label, EditorStyles.miniButtonRight))
                    {
                        overrideValue = -kvp1.Key - 1f;
                    }
                }
            }
            GUILayout.EndVertical ();

            tilesetColor.OverrideValue = overrideValue;
        }

        public AreaSceneModeHints Hints => hints;

        public AreaSceneColorModePanel (AreaSceneBlackboard bb)
        {
            this.bb = bb;
        }

        readonly AreaSceneBlackboard bb;
        AreaVolumePoint spotCached;
        int cachedTilesetID;

        static readonly AreaSceneModeHints hints = new AreaSceneModeHints ()
        {
            HintText = "[LMB] - Apply picked color     [RMB] - Apply default color     [MMB] - Pick color",
        };
    }
}
