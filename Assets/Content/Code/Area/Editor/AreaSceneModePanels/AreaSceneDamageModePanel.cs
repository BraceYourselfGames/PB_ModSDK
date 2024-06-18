using Sirenix.OdinInspector;
using Sirenix.Utilities;

using UnityEngine;

namespace Area
{
    sealed class AreaSceneDamageModePanel : AreaSceneModePanel
    {
        public static AreaSceneModePanel Create (AreaSceneBlackboard bb) => new AreaSceneDamageModePanel (bb);

        public GUILayoutOptions.GUILayoutOptionsInstance Width => AreaScenePanelDrawer.ModePanelWidth;
        public string Title => "Damage editing mode";

        public void OnDisable ()
        {
            pointDisplay.OnDisable ();
            controls.OnDisable ();
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

            GUILayout.Space (4f);
            controls.Draw ();
        }

        public AreaSceneModeHints Hints => hints;

        AreaSceneDamageModePanel (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            controls = new DamageModeGeneralControls (bb);
        }

        readonly AreaSceneBlackboard bb;
        readonly DamageModePanelPointDisplay pointDisplay = new DamageModePanelPointDisplay ();
        readonly DamageModeGeneralControls controls;
        AreaVolumePoint pointCached;
        bool cachedHover;

        static readonly AreaSceneModeHints hints = new AreaSceneModeHints ()
        {
            HintText = "[LMB] - Restore     [RMB] - Destroy      [MMB] - Toggle destructibility     [MW▲▼] - Adjust damage",
        };
    }

    sealed class DamageModePanelPointDisplay : SelfDrawnGUI
    {
        [ShowInInspector]
        [GUIColor (nameof(color))]
        [HideLabel, DisplayAsString, EnableGUI]
        public string pointDisplay => point == null
            ? "Point: —"
            : (hover ? "Point: " : "Last point: ") + point.spotIndex;

        [ShowInInspector]
        [ShowIf (nameof(hasPoint))]
        [ToggleLeft]
        public bool destructible
        {
            get => hasPoint && point.destructible;
            set
            {
                if (point == null)
                {
                    return;
                }
                point.destructible = value;
            }
        }

        [ShowInInspector]
        [ShowIf (nameof(hasPoint))]
        [ToggleLeft]
        public bool destructionUntracked
        {
            get => hasPoint && point.destructionUntracked;
            set
            {
                if (point == null)
                {
                    return;
                }
                point.destructionUntracked = value;
            }
        }

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

        AreaVolumePoint point;
        bool hover;
    }

    sealed class DamageModeGeneralControls : SelfDrawnGUI
    {
        [ShowInInspector]
        [ToggleLeft]
        public bool displayDestructibility
        {
            get => bb.displayDestructibility;
            set => bb.displayDestructibility = value;
        }

        [ShowInInspector]
        [ToggleLeft]
        public bool propagateDestructibilityDown
        {
            get => bb.propagateDestructibilityDown;
            set => bb.propagateDestructibilityDown = value;
        }

        [ShowInInspector]
        [ToggleLeft]
        public bool allowIndestructibleDestruction
        {
            get => bb.allowIndestructibleDestruction;
            set => bb.allowIndestructibleDestruction = value;
        }

        [ShowInInspector]
        public DamageDepths damageDepths;

        public override void Draw ()
        {
            GUILayout.BeginVertical ("Box");
            base.Draw ();
            GUILayout.EndVertical ();
        }

        public DamageModeGeneralControls (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            damageDepths = new DamageDepths (bb);
        }

        readonly AreaSceneBlackboard bb;
    }
}
