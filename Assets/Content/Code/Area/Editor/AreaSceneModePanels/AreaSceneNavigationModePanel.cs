using System.Collections.Generic;

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;

using UnityEditor;
using UnityEngine;

namespace Area
{
    using Scene;

    sealed class AreaSceneNavigationModePanel : AreaSceneModePanel
    {
        public GUILayoutOptions.GUILayoutOptionsInstance Width => AreaScenePanelDrawer.ModePanelWidth;
        public string Title => "Navigation editing tool";

        public void OnDisable ()
        {
            legend.OnDisable ();
            controls.OnDisable ();
        }

        public void Draw ()
        {
            if (Event.current.OnLayout ())
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
            expandLegend = UtilityCustomInspector.DrawFoldout ("Legend", expandLegend, Width.ExpandWidth ());
            if (expandLegend)
            {
                legend.Draw ();
            }
            GUILayout.EndVertical ();

            GUILayout.Space (4f);
            GUILayout.BeginVertical ("Box");
            controls.Draw (spotCached);
            GUILayout.EndVertical ();
        }

        public AreaSceneModeHints Hints => hints;

        public AreaSceneNavigationModePanel (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            controls = new NavigationModePanelControls (bb);
        }

        readonly AreaSceneBlackboard bb;
        readonly NavigationModePanelLegend legend = new NavigationModePanelLegend ();
        readonly NavigationModePanelControls controls;
        AreaVolumePoint spotCached;
        bool expandLegend = true;

        static readonly AreaSceneModeHints hints = new AreaSceneModeHints ()
        {
            HintText = "[LMB] - Add override    [RMB] - Remove override     [MW▲▼] - Adjust override height",
        };
    }

    sealed class NavigationModePanelControls : SelfDrawnGUI
    {
        [ShowInInspector]
        [MiniSlider (0f, 0.5f)]
        public float linkSeparation
        {
            get => bb.navLinkSeparation;
            set => bb.navLinkSeparation = value;
        }

        [ShowIf (nameof(showAddOverride))]
        [PropertySpace (4f)]
        [Button]
        public void AddOverride () =>
            bb.am.navOverrides.Add (spotCached.spotIndex, new AreaDataNavOverride
            {
                pivotIndex = spotCached.spotIndex,
                offsetY = 0f,
            });

        [VerticalGroup (OdinGroup.Name.Override, VisibleIf = nameof(showOverrideControls))]
        [ShowInInspector]
        [PropertySpace (2f)]
        [MiniSlider (nameof(minHeight), nameof(maxHeight))]
        public float height
        {
            get => bb.am.navOverrides.TryGetValue(spotCached.spotIndex, out var navOverride) ? navOverride.offsetY : 0f;
            set
            {
                var navOverride = bb.am.navOverrides[spotCached.spotIndex];
                navOverride.offsetY = value;
                bb.am.navOverrides[spotCached.spotIndex] = navOverride;
            }
        }

        [VerticalGroup (OdinGroup.Name.Override)]
        [PropertySpace (4f)]
        [Button]
        public void RemoveOverride () => bb.am.navOverrides.Remove (spotCached.spotIndex);

        public void Draw (AreaVolumePoint hoveredSpot)
        {
            spotCached = hoveredSpot;
            Draw ();
        }

        bool hasSpot => spotCached != null;
        bool showAddOverride => hasSpot && !showOverrideControls;
        bool showOverrideControls => hasSpot && bb.am.navOverrides.ContainsKey(spotCached.spotIndex);

        float minHeight => -WorldSpace.HalfBlockSize;
        float maxHeight => WorldSpace.HalfBlockSize;

        string NiceName (InspectorProperty property) => property.NiceName;

        public NavigationModePanelControls (AreaSceneBlackboard bb)
        {
            this.bb = bb;
        }

        readonly AreaSceneBlackboard bb;
        AreaVolumePoint spotCached;

        static class OdinGroup
        {
            public static class Name
            {
                public const string Override = nameof(Override);
            }
        }
    }
    sealed class NavigationModePanelLegend : SelfDrawnGUI
    {
        [TableList (AlwaysExpanded = true, IsReadOnly = true, HideToolbar = true, DefaultMinColumnWidth = 55)]
        public List<NavigationLegendEntry> navigationLegend = new List<NavigationLegendEntry> ()
        {
            new NavigationLegendEntry () { color = "■ White", feature = "Navigation node", sceneColor = Color.white, },
            new NavigationLegendEntry () { color = "/ Red", feature = "Link (direct horizontal)", sceneColor = Colors.Link.Horizontal, },
            new NavigationLegendEntry () { color = "/ Orange", feature = "Link (direct diagonal)", sceneColor = Colors.Link.Diagonal, },
            new NavigationLegendEntry () { color = "/ Green", feature = "Link (jump upward)", sceneColor = Colors.Link.JumpUp, },
            new NavigationLegendEntry () { color = "/ Blue", feature = "Link (jump downward)", sceneColor = Colors.Link.JumpDown, },
            new NavigationLegendEntry () { color = "/ Yellow", feature = "Link (jump forward)", sceneColor = Colors.Link.JumpOverDrop, },
            new NavigationLegendEntry () { color = "/ Lime", feature = "Link (jump over)", sceneColor = Colors.Link.JumpOverClimb, },
        };
    }
}
