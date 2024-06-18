using System;

using Sirenix.OdinInspector;
using Sirenix.Utilities;

using UnityEngine;

namespace Area
{
    using Scene;

    sealed class AreaSceneModeToolbar : SelfDrawnGUI
    {
        [AreaSceneModeToolbar]
        public ModeButtons modeButtons;

        public override void Draw ()
        {
            if (!bb.showModeToolbar)
            {
                return;
            }

            if (bb.lastScreenSize.x != Screen.width || bb.lastScreenSize.y != Screen.height)
            {
                bb.lastScreenSize = new Vector2Int (Screen.width, Screen.height);
                var rect = new Rect (new Vector2 (Screen.width - sceneToolbarWidth, Screen.height - sceneToolbarHeight), toolbarSize);
                bb.toolbarUIRect = rect;
                bb.toolbarScreenRect = rect.Expand (25f, 0f, 25f, 0f);
            }

            GUILayout.BeginArea (bb.toolbarUIRect);
            base.Draw ();
            GUILayout.EndArea ();
        }

        public void DrawUIOutline ()
        {
            if (!bb.showModeToolbar)
            {
                return;
            }
            AreaSceneHelper.DrawUIOutline (bb.toolbarScreenRect);
        }

        internal AreaSceneModeToolbar (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            modeButtons = new ModeButtons (bb);
        }

        readonly AreaSceneBlackboard bb;

        internal const float buttonHeight = 45f;
        internal const float buttonWidth = 85f;

        const float sceneToolbarWidth = 95f;
        const float sceneToolbarHeight = (buttonHeight + 6f) * (int)EditingMode.Count + 4f;
        static readonly Vector2 toolbarSize = new Vector2 (sceneToolbarWidth, sceneToolbarHeight);
    }

    [AttributeUsage (AttributeTargets.Field | AttributeTargets.Property)]
    sealed class AreaSceneModeToolbarAttribute : ShowInInspectorAttribute { }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false)]
    sealed class AreaSceneModeToolbarButtonAttribute : ShowInInspectorAttribute
    {
        public readonly float ButtonHeight = AreaSceneModeToolbar.buttonHeight;
        public readonly float ButtonWidth = AreaSceneModeToolbar.buttonWidth;
        public readonly string LabelText;
        public readonly string CurrentMode;
        public readonly string SetMode;
        public string EnableIf;

        public AreaSceneModeToolbarButtonAttribute (string labelText, string modeProperty)
        {
            LabelText = labelText;
            CurrentMode = modeProperty;
            SetMode = "@" + modeProperty + " = $value";
        }
    }
}
