using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;
using Sirenix.Utilities;

using UnityEditor;
using UnityEngine;

namespace Area
{
    using Scene;

    sealed class AreaScenePropModePanel : AreaSceneModePanel
    {
        public static AreaSceneModePanel Create (AreaSceneBlackboard bb) => new AreaScenePropModePanel (bb);

        public GUILayoutOptions.GUILayoutOptionsInstance Width => GUILayoutOptions.Width (panelStartingWidth);
        public string Title => "Prop tool";

        public void OnDisable ()
        {
            propList.OnDisable ();
            panelOptions.OnDisable ();
            controls.OnDisable ();
        }

        public void Draw ()
        {
            DrawSpotInfo ();
            DrawEditOptions ();
            DrawPropList ();
            controls.Draw ();
        }

        public AreaSceneModeHints Hints => bb.propEditingMode == PropEditingMode.Color ? colorHints : hints;

        void DrawSpotInfo ()
        {
            if (Event.current.type == EventType.Layout)
            {
                if (bb.hoverActive)
                {
                    spotCached = bb.lastSpotHovered;
                }
                else if (bb.propEditInfo.PlacementListIndex.IsValidIndex(bb.am.points))
                {
                    spotCached = bb.am.points[bb.propEditInfo.PlacementListIndex];
                }
                else
                {
                    spotCached = bb.lastSpotHovered;
                }
            }

            if (spotCached == null)
            {
                AreaSceneModePanelHelper.DrawSpotUnknown ();
                return;
            }

            GUILayout.BeginHorizontal ("Box");
            GUILayout.Label ("Spot: " + spotCached.spotIndex, EditorStyles.miniLabel, GUILayoutOptions.MinWidth(35f).MaxWidth(70f));
            var position = spotCached.pointPositionIndex;
            GUILayout.Label (string.Format ("Position: ({0}, {1}, {2})", position.x, position.z, position.y), EditorStyles.miniLabel);
            GUILayout.Space (35f);
            GUILayout.EndHorizontal ();
        }

        void DrawEditOptions ()
        {
            GUILayout.Space (4f);
            GUILayout.BeginVertical ("Box");
            panelOptions.Draw ();
            GUILayout.EndVertical ();
        }

        void DrawPropList ()
        {
            if (!bb.showPropListInPanel)
            {
                return;
            }

            GUILayout.Space (5f);
            GUILayout.BeginVertical ("Box");
            if (bb.hoverActive)
            {
                UtilityCustomInspector.DrawFoldout (propListSectionName, false, GUILayoutOptions.MinWidth (panelStartingWidth).ExpandWidth ());
                GUILayout.EndVertical ();
                return;
            }
            expandPropList = UtilityCustomInspector.DrawFoldout (propListSectionName, expandPropList, GUILayoutOptions.MinWidth (panelStartingWidth).ExpandWidth ());
            if (expandPropList)
            {
                propList.Draw ();
            }
            GUILayout.EndVertical ();
        }

        AreaScenePropModePanel (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            propList = new PropModePanelSelector (bb);
            panelOptions = new PropModePanelOptions (bb);
            controls = new PropModePanelPropControls (bb);
        }

        readonly AreaSceneBlackboard bb;
        readonly PropModePanelSelector propList;
        readonly PropModePanelOptions panelOptions;
        readonly PropModePanelPropControls controls;
        bool expandPropList;
        bool expandNewPropSection;
        AreaVolumePoint spotCached;

        const float panelStartingWidth = 350f;
        const string propListSectionName = "Props";

        static readonly AreaSceneModeHints hints = new AreaSceneModeHints ()
        {
            HintText = "[LMB] - Add prop     [Shift + RMB] - Copy prop     [MMB] - Remove prop     [MW▲▼] - Change preview prop"
                + "\n[Right Square Bracket] - Rotate preview prop     [Left Square Bracket] - Flip preview prop",
            NonAltHintText = "[Shift + F] - Rotate selected prop     [Shift + G] - Flip selected prop"
                + "\n[Shift + Z] - Copy selected prop HSV     [Shift + X] - Paste selected prop HSV"
                + "\n[Delete] - Remove selected prop",
        };
        static readonly AreaSceneModeHints colorHints = new AreaSceneModeHints ()
        {
            LeaderText = "<color=#ffea04bb>Applying prop color</color>\nUse the Mode dropdown in the Prop tool panel to switch back to placing props",
            HintText = "[LMB] - Apply picked color     [RMB] - Apply default color     [MMB] - Pick color",
        };
    }

    [HideLabel, HideReferenceObjectPicker]
    sealed class PropModePanelSelector : SelfDrawnGUI
    {
        [ShowInInspector]
        [PropertyOrder (0f)]
        [LabelWidth (72f)]
        public string propFilter
        {
            get => bb.propFilter;
            set => bb.propFilter = value;
        }

        [ShowInInspector]
        [PropertyOrder (1f)]
        [TableList (AlwaysExpanded = true, IsReadOnly = true, NumberOfItemsPerPage = itemsPerPage, ShowPaging = true)]
        public readonly List<PropTableEntry> props;

        public override void OnDisable ()
        {
            bb.onPropListChanged -= Populate;
            base.OnDisable ();
        }

        void Populate ()
        {
            AreaSceneModeHelper.PopulatePropList (bb, altList: props);
            // Stuff dummies to avoid the table list stealing the focus from the
            // filter text field when it changes or hides its toolbar, scrollbar
            // or hides itself when there are no rows.
            var dummies = props.Count == 0 ? itemsPerPage : props.Count % itemsPerPage;
            while (0 != dummies--)
            {
                props.Add (new PropTableEntry ());
            }
        }

        public PropModePanelSelector (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            props = new List<PropTableEntry> ();
            props.AddRange (bb.props);
            bb.onPropListChanged += Populate;
        }

        readonly AreaSceneBlackboard bb;

        const int itemsPerPage = 10;
    }

    [HideLabel, HideReferenceObjectPicker]
    sealed class PropModePanelOptions : SelfDrawnGUI
    {
        [ShowInInspector]
        public PropEditingMode mode
        {
            get => bb.propEditingMode;
            set => bb.propEditingMode = value;
        }

        [ShowInInspector]
        [LabelText ("Check spot compatibility")]
        public bool checkCompatibility
        {
            get => bb.checkPropConfiguration;
            set => bb.checkPropConfiguration = value;
        }

        [ShowInInspector]
        public bool showOccludedHandles
        {
            get => bb.showOccludedPropHandles;
            set => bb.showOccludedPropHandles = value;
        }

        [ShowInInspector]
        public bool showPropListInPanel
        {
            get => bb.showPropListInPanel;
            set => bb.showPropListInPanel = value;
        }

        public PropModePanelOptions (AreaSceneBlackboard bb)
        {
            this.bb = bb;
        }

        readonly AreaSceneBlackboard bb;
    }

    [HideLabel, HideReferenceObjectPicker]
    sealed class PropModePanelPropControls : SelfDrawnGUI
    {
        [ShowInInspector]
        [TitleGroup ("Preview Prop", GroupID = "PreviewProp", HorizontalLine = false, VisibleIf = "@!" + nameof(showSelected))]
        public readonly PropModePanelEntry previewProp;

        [ShowInInspector]
        [RightFoldoutGroup ("PreviewPropHideAway", "Preview Prop", VisibleIf = nameof(showSelected))]
        public readonly PropModePanelEntry previewPropHideAway;

        [ShowInInspector]
        [TitleGroup ("Selected Props", HorizontalLine = false, VisibleIf = nameof(showSelected))]
        public readonly PropModeSelectedProps selectedProps = new PropModeSelectedProps ();

        public override void Draw ()
        {
            var propEditInfo = bb.propEditInfo;
            previewProp.prototype = AreaAssetHelper.GetPropPrototype (propEditInfo.SelectionID);
            previewPropHideAway.prototype = previewProp.prototype;
            PopulateSelected ();
            base.Draw ();
        }

        void PopulateSelected ()
        {
            selectedProps.props.Clear ();
            if (bb.propEditInfo.PlacementListIndex == -1)
            {
                return;
            }
            if (!bb.am.indexesOccupiedByProps.TryGetValue (bb.propEditInfo.PlacementListIndex, out var placements))
            {
                return;
            }
            foreach (var placement in placements)
            {
                var spf = new SelectedPropFunctions (bb, placement);
                var ep = new PropModePanelEntry (bb, spf, isSelected: bb.propEditInfo.PlacementHandled == placement)
                {
                    prototype = placement.prototype,
                };
                selectedProps.props.Add (ep);
            }
        }

        bool showSelected => !bb.hoverActive && selectedProps.props.Count != 0;

        public PropModePanelPropControls (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            var npf = new NewPropFunctions (bb);
            previewProp = new PropModePanelEntry (bb, npf, isPreview: true);
            previewPropHideAway = new PropModePanelEntry (bb, npf, isPreview: true);
        }

        readonly AreaSceneBlackboard bb;
    }

    [HideLabel, HideReferenceObjectPicker]
    sealed class PropModeSelectedProps
    {
        [ShowInInspector]
        [HideLabel]
        public readonly List<PropModePanelEntry> props = new List<PropModePanelEntry> ();
    }

    [HideLabel, HideReferenceObjectPicker]
    sealed class PropModePanelEntry
    {
        [ShowInInspector]
        [BoxWithBackgroundGroup (OdinGroup.Name.Entry)]
        [VerticalGroup (OdinGroup.Name.Header, Order = OdinGroup.Order.Header)]
        [HideLabel, DisplayAsString, EnableGUI]
        public string header => hasPrototype ? prototype.id + " - " + prototype.name : "null";

        [HorizontalGroup (OdinGroup.Name.Controls, Order = OdinGroup.Order.Controls)]
        [VerticalGroup (OdinGroup.Name.Values, Order = OdinGroup.Order.Values)]
        [BoxWithBackgroundGroup (OdinGroup.Name.OrientationBG, Order = OdinGroup.Order.Orientation)]
        [HorizontalGroup (OdinGroup.Name.Orientation)]
        [MiniLabel, EnableGUI]
        public string orientation
        {
            get
            {
                var (rotation, flipped) = pf.GetOrientation ();
                return "Orientation: " + rotation + (flipped ? "-" : "+");
            }
        }

        [HorizontalGroup (OdinGroup.Name.Orientation, Width = 25f)]
        [ShowIf (nameof(showSnap))]
        [PropertyTooltip ("Snaps rotation to tile")]
        [Button ("S")]
        public void SnapRotation ()
        {
            bb.propEditFunctions = pf;
            bb.propEditCommand = PropEditCommand.Snap;
        }

        [HorizontalGroup (OdinGroup.Name.Orientation, Width = 25f)]
        [PropertyTooltip ("Rotates clockwise")]
        [Button ("←")]
        public void RotateLeft ()
        {
            bb.propEditFunctions = pf;
            bb.propEditCommand = PropEditCommand.RotateLeft;
        }

        [HorizontalGroup (OdinGroup.Name.Orientation, Width = 25f)]
        [PropertyTooltip ("Flips prop")]
        [Button ("↔")]
        public void Flip ()
        {
            bb.propEditFunctions = pf;
            bb.propEditCommand = PropEditCommand.Flip;
        }

        [HorizontalGroup (OdinGroup.Name.Orientation, Width = 25f)]
        [PropertyTooltip ("Rotates prop anticlockwise")]
        [Button ("→")]
        public void RotateRight ()
        {
            bb.propEditFunctions = pf;
            bb.propEditCommand = PropEditCommand.RotateRight;
        }

        [HorizontalGroup (OdinGroup.Name.Orientation, Width = 44f)]
        [PropertyTooltip ("Resets orientation")]
        [Button ("reset")]
        public void ResetOrientation ()
        {
            bb.propEditFunctions = pf;
            bb.propEditCommand = PropEditCommand.ResetRotation;
        }

        [BoxWithBackgroundGroup (OdinGroup.Name.PositionBG, Order = OdinGroup.Order.Position)]
        [VerticalGroup (OdinGroup.Name.Position, VisibleIf = nameof(showPositionControls))]
        [MiniLabel, EnableGUI]
        public string positionOffsets => "Position offsets";

        [ShowInInspector]
        [HorizontalGroup (OdinGroup.Name.PositionControls, Width = 82f)]
        [MinValue (nameof(minOffset)), MaxValue (nameof(maxOffset))]
        [LabelText ("X ↔"), LabelWidth (33f)]
        public float offsetX
        {
            get => pf.GetOffset ().x;
            set
            {
                var offset = pf.GetOffset ();
                bb.propOffset = (value, offset.z);
                bb.propEditFunctions = pf;
                bb.propEditCommand = PropEditCommand.Offset;
            }
        }

        [ShowInInspector]
        [HorizontalGroup (OdinGroup.Name.PositionControls, Width = 82f)]
        [MinValue (nameof(minOffset)), MaxValue (nameof(maxOffset))]
        [LabelText ("Z ↔"), LabelWidth (33f)]
        public float offsetZ
        {
            get => pf.GetOffset ().z;
            set
            {
                var offset = pf.GetOffset ();
                bb.propOffset = (offset.x, value);
                bb.propEditFunctions = pf;
                bb.propEditCommand = PropEditCommand.Offset;
            }
        }

        [ShowInInspector]
        [HorizontalGroup (OdinGroup.Name.PositionControls)]
        [HideLabel, DisplayAsString]
        // This is a hack to get the buttons to right align.
        string positionGap => "";

        [HorizontalGroup (OdinGroup.Name.PositionControls, Width = 44f)]
        [PropertySpace (2f)]
        [PropertyTooltip ("Copies position offsets")]
        [Button ("copy")]
        public void CopyOffsets ()
        {
            bb.propEditFunctions = pf;
            bb.propEditCommand = PropEditCommand.CopyPosition;
        }

        [HorizontalGroup (OdinGroup.Name.PositionControls, Width = 44f)]
        [PropertySpace (2f)]
        [PropertyTooltip ("Pastes position offsets")]
        [Button ("paste")]
        public void PasteOffsets ()
        {
            bb.propEditFunctions = pf;
            bb.propEditCommand = PropEditCommand.PastePosition;
        }

        [HorizontalGroup (OdinGroup.Name.PositionControls, Width = 44f)]
        [PropertySpace (2f)]
        [PropertyTooltip ("Resets position offsets")]
        [Button ("reset")]
        public void ResetOffsets ()
        {
            bb.propEditFunctions = pf;
            bb.propEditCommand = PropEditCommand.ResetPosition;
        }

        [BoxWithBackgroundGroup (OdinGroup.Name.ColorBG, Order = OdinGroup.Order.Color)]
        [HorizontalGroup (OdinGroup.Name.Color, VisibleIf = nameof(showColorControls))]
        [VerticalGroup (OdinGroup.Name.ColorControls)]
        [PropertyOrder (OdinGroup.Order.ColorHeader)]
        [MiniLabel, EnableGUI]
        public string colorOffsets => "Color offsets";

        [ShowInInspector]
        [VerticalGroup (OdinGroup.Name.ColorControls)]
        [PropertyOrder (OdinGroup.Order.ColorPrimary)]
        public readonly PropColorControls colorPrimary;

        [ShowInInspector]
        [VerticalGroup (OdinGroup.Name.ColorControls)]
        [PropertyOrder (OdinGroup.Order.ColorSecondary)]
        public readonly PropColorControls colorSecondary;

        [HorizontalGroup (OdinGroup.Name.Color, Width = 44f)]
        [VerticalGroup (OdinGroup.Name.ColorCPR)]
        [PropertySpace (2f)]
        [PropertyTooltip ("Copies primary and secondary color offsets")]
        [Button ("copy")]
        public void CopyColor ()
        {
            bb.propEditFunctions = pf;
            bb.propEditCommand = PropEditCommand.CopyColor;
        }

        [VerticalGroup (OdinGroup.Name.ColorCPR)]
        [PropertySpace (2f)]
        [PropertyTooltip ("Pastes primary and secondary color offsets")]
        [Button ("paste")]
        public void PasteColor ()
        {
            bb.propEditFunctions = pf;
            bb.propEditCommand = PropEditCommand.PasteColor;
        }

        [VerticalGroup (OdinGroup.Name.ColorCPR)]
        [PropertySpace (2f)]
        [PropertyTooltip ("Resets primary and secondary color offsets")]
        [Button ("reset")]
        public void ResetColor ()
        {
            bb.propEditFunctions = pf;
            bb.propEditCommand = PropEditCommand.ResetColor;
        }

        [HorizontalGroup (OdinGroup.Name.Controls, Width = 32f)]
        [VerticalGroup (OdinGroup.Name.ChangeRemove, VisibleIf = "@!" + nameof(isPreview), Order = OdinGroup.Order.ChangeRemove)]
        [HideIf (nameof(isSelected))]
        [PropertySpace (3f)]
        [PropertyTooltip ("Displays the scene gizmos on this prop so it can be moved in scene.")]
        [Button (SdfIconType.HandIndex, ButtonHeight = 32)]
        [HideLabel]
        public void SelectProp ()
        {
            bb.propEditFunctions = pf;
            bb.propEditCommand = PropEditCommand.ChangeSelected;
        }

        [VerticalGroup (OdinGroup.Name.ChangeRemove)]
        [ShowIf (nameof(isSelected))]
        [PropertySpace (3f)]
        [PropertyTooltip ("Hides scene gizmos on this prop. If the prop overlaps other props, the entire group will be deselected.")]
        [Button (SdfIconType.EyeSlash, ButtonHeight = 32)]
        [HideLabel]
        public void DeselectProp ()
        {
            bb.propEditFunctions = pf;
            bb.propEditCommand = PropEditCommand.ChangeSelected;
        }

        [VerticalGroup (OdinGroup.Name.ChangeRemove)]
        [EnableIf (nameof(isSelected))]
        [PropertySpace (2f)]
        [PropertyTooltip ("Removes selected prop from the scene.")]
        [Button (SdfIconType.BackspaceReverse, ButtonHeight = 32)]
        [HideLabel]
        public void RemoveProp ()
        {
            bb.propEditFunctions = pf;
            bb.propEditCommand = PropEditCommand.DeleteSelected;
        }

        [HideInInspector]
        public AreaPropPrototypeData prototype;

        void ChangePrimaryColor (Vector4 color)
        {
            bb.propColor = (color, pf.GetSecondaryColor ());
            bb.propEditFunctions = pf;
            bb.propEditCommand = PropEditCommand.ChangeColor;
        }

        void ChangeSecondaryColor (Vector4 color)
        {
            bb.propColor = (pf.GetPrimaryColor (), color);
            bb.propEditFunctions = pf;
            bb.propEditCommand = PropEditCommand.ChangeColor;
        }

        bool hasPrototype => prototype != null;
        bool hasPrefab => hasPrototype && prototype.prefab != null;
        bool showPositionControls => hasPrefab && prototype.prefab.allowPositionOffset;
        bool showColorControls => hasPrefab && prototype.prefab.allowTinting;

        float minOffset => -WorldSpace.HalfBlockSize;
        float maxOffset => WorldSpace.HalfBlockSize;

        [HideInInspector]
        public readonly bool isSelected;

        public PropModePanelEntry (AreaSceneBlackboard bb, PropEditFunctions pf, bool isPreview = false, bool isSelected = false)
        {
            this.bb = bb;
            this.pf = pf;
            this.isPreview = isPreview;
            this.isSelected = isSelected;
            showSnap = !isPreview;
            colorPrimary = new PropColorControls (pf.GetPrimaryColor, ChangePrimaryColor);
            colorSecondary = new PropColorControls (pf.GetSecondaryColor, ChangeSecondaryColor);
        }

        readonly AreaSceneBlackboard bb;
        readonly PropEditFunctions pf;
        readonly bool isPreview;
        readonly bool showSnap;

        static class OdinGroup
        {
            public static class Name
            {
                public const string ChangeRemove = Controls + "/" + nameof(ChangeRemove);
                public const string Color = ColorBG + "/" + nameof(Color);
                public const string ColorBG = Values + "/" + nameof(ColorBG);
                public const string ColorControls = Color + "/Controls";
                public const string ColorCPR = Color + "/CPR";
                public const string Controls = Entry + "/" + nameof(Controls);
                public const string Entry = nameof(Entry);
                public const string Header = Entry + "/" + nameof(Header);
                public const string Orientation = OrientationBG + "/" + nameof(Orientation);
                public const string OrientationBG = Values + "/" + nameof(OrientationBG);
                public const string Position = PositionBG + "/" + nameof(Position);
                public const string PositionBG = Values + "/" + nameof(PositionBG);
                public const string PositionControls = Position + "/Controls";
                public const string Values = Controls + "/" + nameof(Values);
            }

            public static class Order
            {
                public const float Header = 0f;
                public const float Controls = 1f;

                public const float Values = 0f;
                public const float ChangeRemove = 1f;

                public const float Orientation = 0f;
                public const float Position = 1f;
                public const float Color = 2f;

                public const float ColorHeader = 0f;
                public const float ColorPrimary = 1f;
                public const float ColorSecondary = 2f;
            }
        }
    }

    [HideLabel, HideReferenceObjectPicker]
    sealed class PropColorControls
    {
        [ShowInInspector]
        [HorizontalGroup (groupName, Width = 78f)]
        [PropertyTooltip ("Set hue offset")]
        [MinValue (0f), MaxValue (1f)]
        [LabelText ("H ↔"), LabelWidth (33f)]
        public float colorH
        {
            get => getPropColor().x;
            set
            {
                var c = getPropColor ();
                c.x = value;
                changePropColor (c);
            }
        }

        [ShowInInspector]
        [HorizontalGroup (groupName, Width = 78f)]
        [PropertyTooltip ("Set saturation offset")]
        [MinValue (0f), MaxValue (1f)]
        [LabelText ("S ↔"), LabelWidth (33f)]
        public float colorS
        {
            get => getPropColor().y;
            set
            {
                var c = getPropColor ();
                c.y = value;
                changePropColor (c);
            }
        }

        [ShowInInspector]
        [HorizontalGroup (groupName, Width = 78f)]
        [PropertyTooltip ("Set brightness offset")]
        [MinValue (0f), MaxValue (1f)]
        [LabelText ("V ↔"), LabelWidth (33f)]
        public float colorV
        {
            get => getPropColor().z;
            set
            {
                var c = getPropColor ();
                c.z = value;
                changePropColor (c);
            }
        }

        [ShowInInspector]
        [HorizontalGroup (groupName, Width = 36f)]
        [HideLabel]
        public Color colorSwatch
        {
            get
            {
                var c = getPropColor ();
                return new HSBColor (c.x, c.y, c.z).ToColor ();
            }
            set
            {
                var hsb = new HSBColor (value);
                var c = new Vector4 (hsb.h, hsb.s, hsb.b);
                changePropColor (c);
            }
        }

        public PropColorControls (Func<Vector4> getPropColor, Action<Vector4> changePropColor)
        {
            this.getPropColor = getPropColor;
            this.changePropColor = changePropColor;
        }

        readonly Func<Vector4> getPropColor;
        readonly Action<Vector4> changePropColor;

        const string groupName = "Color";
    }
}
