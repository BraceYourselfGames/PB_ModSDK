using Sirenix.Utilities;
using Sirenix.Utilities.Editor;

using UnityEditor;
using UnityEngine;

using PhantomBrigade.Data;
#if PB_MODSDK
using PhantomBrigade.SDK.ModTools;
#endif

namespace Area
{
    class AreaScenePanelDrawer
    {
        public void Draw (AreaSceneModePanel modePanel)
        {
            DrawTitlePanel ();
            DrawModePanel (modePanel);
        }

        public void DrawUIOutline ()
        {
            if (bb.showLevelInfo)
            {
                AreaSceneHelper.DrawUIOutline (bb.sceneTitlePanelScreenRect);
            }
            AreaSceneHelper.DrawUIOutline (bb.sceneModePanelScreenRect);
            AreaSceneHelper.DrawUIOutline (bb.sceneModePanelHintsScreenRect);
        }

        void DrawTitlePanel ()
        {
            if (!bb.showLevelInfo)
            {
                return;
            }
            if (DataMultiLinkerCombatArea.selectedArea == null)
            {
                return;
            }

            const string notEditableText = "SDK level is not editable";
            const string saveLabel = "Save";
            const string buttonLabel = "Edit Combat Area";
            var levelKey = DataMultiLinkerCombatArea.selectedArea.key;
            var titleStyle = SirenixGUIStyles.WhiteLabelCentered;
            var titleSize = titleStyle.CalcSize (GUIHelper.TempContent (levelKey));
            var boundsStr = AreaSceneHelper.FormatBounds (bb.am.boundsFull);
            var boundsStyle = SirenixGUIStyles.MiniLabelCentered;
            var boundsSize = boundsStyle.CalcSize (GUIHelper.TempContent (boundsStr));
            var width = Mathf.Max (titleSize.x, boundsSize.x);
            var height = titleSize.y + boundsSize.y + 8f;
            #if PB_MODSDK
            var modID = "";
            var hasModID = DataContainerModData.hasSelectedConfigs;
            if (hasModID)
            {
                modID = DataContainerModData.selectedMod.id;
                var modIDSize = titleStyle.CalcSize (GUIHelper.TempContent (modID));
                width = Mathf.Max (width, modIDSize.x);
                height += modIDSize.y + 20f;
                var saveSize = SirenixGUIStyles.Button.CalcSize (GUIHelper.TempContent (saveLabel));
                width = Mathf.Max (width, saveSize.x + 8f);
                height += saveSize.y + 4f;
            }
            else
            {
                var warningSize = titleStyle.CalcSize (GUIHelper.TempContent (notEditableText));
                width = Mathf.Max (width, warningSize.x);
                height += warningSize.y + 2f;
            }
            #endif
            var buttonSize = SirenixGUIStyles.Button.CalcSize (GUIHelper.TempContent (buttonLabel));
            width = Mathf.Max (width, buttonSize.x + 8f);
            height += buttonSize.y + 4f;
            var size = new Vector2 (width, height);
            var rect = new Rect (new Vector2 ((Screen.width - titleSize.x - 8f) / 2f, 4f), size);
            var boxRect = rect.Expand (4f, 0f);
            bb.sceneTitlePanelScreenRect = boxRect.Expand (10f, 10f, 4f, 10f);

            GUILayout.BeginArea (boxRect);
            GUILayout.BeginVertical (EditorStyles.helpBox, GUILayoutOptions.Width (rect.width));
            #if PB_MODSDK
            var c = GUI.contentColor;
            if (hasModID)
            {
                GUI.contentColor = DataContainerModData.colorSelected;
                GUILayout.Label (modID, titleStyle);
            }
            else
            {
                GUI.contentColor = SirenixGUIStyles.YellowWarningColor;
                GUILayout.Label (notEditableText, titleStyle);
            }
            GUI.contentColor = c;
            #endif
            GUILayout.Label (levelKey, titleStyle);
            GUILayout.Label (boundsStr, boundsStyle);
            var editMetadata = GUILayout.Button (buttonLabel, SirenixGUIStyles.Button);
            #if PB_MODSDK
            if (hasModID)
            {
                if (GUILayout.Button (saveLabel, SirenixGUIStyles.Button))
                {
                    AreaSceneHelper.SaveSelectedLevel ();
                }
            }
            #endif
            GUILayout.EndVertical ();
            GUILayout.EndArea ();

            if (editMetadata)
            {
                AreaSceneHelper.ReturnToAreaDB ();
            }
        }

        void DrawModePanel (AreaSceneModePanel panel)
        {
            GUILayout.BeginArea (new Rect (new Vector2 (sceneModePanelPadding, sceneModePanelPadding), new Vector2 (Screen.width - 2f * sceneModePanelPadding, Screen.height - 2f * sceneModePanelPadding)));
            var etype = Event.current.type;
            if (etype == EventType.Repaint)
            {
                GUIHelper.BeginLayoutMeasuring ();
            }

            GUILayout.BeginVertical (EditorStyles.helpBox, panel.Width);
            GUILayout.Label (panel.Title, EditorStyles.boldLabel);
            panel.Draw ();
            GUILayout.EndVertical ();

            if (etype == EventType.Repaint)
            {
                var rect = GUIHelper.EndLayoutMeasuring ();
                if (rect != bb.sceneModePanelUIRect)
                {
                    bb.sceneModePanelUIRect = rect;
                    bb.sceneModePanelScreenRect = rect.Expand (0f, 25f, 0f, 25f);
                }
            }
            GUILayout.EndArea ();

            if (etype == EventType.Layout)
            {
                cachedHints = panel.Hints;
            }
            if (cachedHints != null)
            {
                var rect = AreaSceneModePanelHelper.DrawModeHints (cachedHints);
                bb.sceneModePanelHintsScreenRect = rect.Expand (25f, 25f, 25f, 0f);
            }
        }

        AreaSceneModeHints cachedHints;

        public static GUILayoutOptions.GUILayoutOptionsInstance ModePanelWidth => GUILayoutOptions.Width (standardModePanelWidth);

        public AreaScenePanelDrawer (AreaSceneBlackboard bb)
        {
            this.bb = bb;
        }

        readonly AreaSceneBlackboard bb;

        const float sceneModePanelPadding = 10f;
        const float standardModePanelWidth = 200f;
    }
}
