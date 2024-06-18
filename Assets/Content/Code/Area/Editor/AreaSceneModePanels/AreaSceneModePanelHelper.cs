using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;

using UnityEditor;
using UnityEngine;

namespace Area
{
    using Scene;

    interface AreaSceneModePanel
    {
        public GUILayoutOptions.GUILayoutOptionsInstance Width { get; }
        public string Title { get; }
        public void OnDisable ();
        public void Draw ();
        public AreaSceneModeHints Hints { get; }
    }

    class AreaSceneModeHints
    {
        public string LeaderText = "";
        public string HintText = "";
        public string NonAltHintText = "";
    }

    static class AreaSceneModePanelHelper
    {
        public const string SDKModPlaceHolder = "<SDK>";

        public static void DrawSearchSelector (AreaSceneBlackboard bb)
        {
            UtilityCustomInspector.DrawField ("Mode", () => bb.currentSearchType = (SpotSearchType)EditorGUILayout.EnumPopup (bb.currentSearchType));

            ref var currentSearchType = ref bb.currentSearchType;
            GUILayout.Space (4f);
            GUILayout.BeginHorizontal ();
            if (GUILayout.Button ("None", EditorStyles.miniButtonLeft))
            {
                currentSearchType = SpotSearchType.None;
            }
            if (GUILayout.Button ("Empties", EditorStyles.miniButtonRight))
            {
                currentSearchType = SpotSearchType.AllEmptyNodes;
            }
            if (GUILayout.Button ("Floor", EditorStyles.miniButtonLeft))
            {
                currentSearchType = SpotSearchType.SameFloor;
            }
            if (GUILayout.Button ("Floor-Iso.", EditorStyles.miniButtonRight))
            {
                currentSearchType = SpotSearchType.SameFloorIsolated;
            }
            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal ();
            if (GUILayout.Button ("S. Cfg.", EditorStyles.miniButtonLeft))
            {
                currentSearchType = SpotSearchType.SameConfiguration;
            }
            if (GUILayout.Button ("S. Tls.", EditorStyles.miniButtonMid))
            {
                currentSearchType = SpotSearchType.SameTileset;
            }
            if (GUILayout.Button ("S. Clr.", EditorStyles.miniButtonMid))
            {
                currentSearchType = SpotSearchType.SameColor;
            }
            if (GUILayout.Button ("S. Evr.", EditorStyles.miniButtonRight))
            {
                currentSearchType = SpotSearchType.SameEverything;
            }
            GUILayout.EndHorizontal ();
        }

        public static void DrawSpotCursorSelector (AreaSceneBlackboard bb)
        {
            GUILayout.BeginVertical ("Box", GUILayout.MinWidth (250f));
            UtilityCustomInspector.DrawField ("Cursor", () => bb.currentSpotCursorType = (SpotCursorType)EditorGUILayout.EnumPopup (bb.currentSpotCursorType));
            GUILayout.EndVertical ();
        }

        public static Rect DrawModeHints (AreaSceneModeHints hints)
        {
            const float leaderHeightOffset = -19f;
            const float hintHeightOffset = -14;
            const float lineHeight = 20f;

            var hintLines = hints.HintText.Split (lineSplitter, StringSplitOptions.RemoveEmptyEntries);
            var nonAltHintLines = hints.NonAltHintText.Split (lineSplitter, StringSplitOptions.RemoveEmptyEntries);
            var totalHintLineCount = hintLines.Length + nonAltHintLines.Length;
            var leader = hints.LeaderText.Split (lineSplitter, StringSplitOptions.RemoveEmptyEntries);
            var panelWidth = 1100f;
            var panelWidthAlt = 60f;
            var panelHeightAlt = hintLines.Length * lineHeight;
            var panelHeightNonAlt = nonAltHintLines.Length * lineHeight + (hintLines.Length == 1 && nonAltHintLines.Length == 1 ? 5f : 0f);
            var panelHeightLeader = leader.Length * lineHeight;
            var panelHeight = Mathf.Max (2 * lineHeight, panelHeightAlt + panelHeightNonAlt + panelHeightLeader);
            var panelYOffset = Mathf.Max (2 * lineHeight, panelHeight + (totalHintLineCount > 1 ? 2f : 1f) * lineHeight);
            var panelLeaderYOffset = panelYOffset + leader.Length * lineHeight;

            var maxWidth = 0f;
            var lines = new List<string> (hintLines);
            lines.AddRange (leader);
            lines.AddRange (nonAltHintLines);
            foreach (var line in lines)
            {
                hintStyle.CalcMinMaxWidth (GUIHelper.TempContent (line), out _, out var width);
                if (width > maxWidth)
                {
                    maxWidth = width;
                }
            }

            panelWidth = Mathf.Min (panelWidth, maxWidth + 40f);
            var hintAreaRect = new Rect (new Vector2 (Screen.width * 0.5f - panelWidth * 0.5f, Screen.height - panelYOffset + hintHeightOffset), new Vector2 (panelWidth, panelHeight));
            var areaRect = new Rect (hintAreaRect);

            if (leader.Length != 0)
            {
                var leaderAreaRect = new Rect (new Vector2 (Screen.width * 0.5f - panelWidth * 0.5f, Screen.height - panelLeaderYOffset + leaderHeightOffset), new Vector2 (panelWidth, panelHeightLeader));
                GUILayout.BeginArea (leaderAreaRect);
                GUILayout.BeginVertical ("Box", GUILayout.Height (panelHeightLeader));

                foreach (var lead in leader)
                {
                    GUILayout.BeginHorizontal ();
                    GUILayout.FlexibleSpace ();
                    GUILayout.Label (lead, hintStyle);
                    GUILayout.FlexibleSpace ();
                    GUILayout.EndHorizontal ();
                }

                GUILayout.EndVertical ();
                GUILayout.EndArea ();

                areaRect = areaRect.ExpandTo (leaderAreaRect.min);
            }

            if (hintLines.Length != 0)
            {
                // "Alt" side label
                var altAreaRect = new Rect (new Vector2 (Screen.width * 0.5f - panelWidth * 0.5f - panelWidthAlt - 10f, Screen.height - panelYOffset + hintHeightOffset), new Vector2 (panelWidthAlt, panelHeightAlt));
                GUILayout.BeginArea (altAreaRect);
                GUILayout.BeginVertical ("Box", GUILayout.Height (panelHeightAlt));

                GUILayout.BeginHorizontal ();
                GUILayout.FlexibleSpace ();
                GUILayout.Label ("Alt +  ", altStyle, GUILayout.ExpandHeight (true));
                GUILayout.FlexibleSpace ();
                GUILayout.EndHorizontal ();

                GUILayout.EndVertical ();
                GUILayout.EndArea ();

                areaRect = areaRect.ExpandTo (altAreaRect.min);
            }

            GUILayout.BeginArea (hintAreaRect);

            if (hintLines.Length != 0)
            {
                // Main hotkey hints label
                GUILayout.BeginVertical ("Box", GUILayout.Height (panelHeightAlt));
                foreach (var hint in hintLines)
                {
                    GUILayout.BeginHorizontal ();
                    GUILayout.FlexibleSpace ();
                    GUILayout.Label (hint, hintStyle);
                    GUILayout.FlexibleSpace ();
                    GUILayout.EndHorizontal ();
                }
                GUILayout.EndVertical ();
            }

            if (nonAltHintLines.Length != 0)
            {
                GUILayout.BeginVertical ("Box", GUILayout.Height (panelHeightNonAlt));
                foreach (var hint in nonAltHintLines)
                {
                    GUILayout.BeginHorizontal ();
                    GUILayout.FlexibleSpace ();
                    GUILayout.Label (hint, hintStyle);
                    GUILayout.FlexibleSpace ();
                    GUILayout.EndHorizontal ();
                }
                GUILayout.EndVertical ();
            }

            GUILayout.EndArea ();

            return areaRect;
        }

        public static void DrawSpotUnknown ()
        {
            GUILayout.BeginVertical ("Box");
            GUILayout.Label ("Spot: —", SirenixGUIStyles.LeftAlignedGreyMiniLabel, GUILayoutOptions.MinWidth (90f).ExpandWidth ());
            GUILayout.EndVertical ();
        }

        public static void DrawGroupInfo (string groupInfo) => GUILayout.Label (groupInfo, spotInfoStyle);

        public static void InitializeStyles ()
        {
            altStyle = new GUIStyle (EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                richText = true,
            };
            hintStyle = new GUIStyle (EditorStyles.boldLabel)
            {
                richText = true,
            };
            spotInfoStyle = new GUIStyle (EditorStyles.largeLabel)
            {
                richText = true,
            };
        }

        static GUIStyle altStyle;
        static GUIStyle hintStyle;
        static GUIStyle spotInfoStyle;

        static readonly char[] lineSplitter = { '\n' };
    }

    [HideLabel, HideReferenceObjectPicker]
    sealed class TilesetSelection : SelfDrawnGUI
    {
        [ShowInInspector]
        [ValueDropdown (nameof(availableTilesets))]
        [HideLabel, Title ("@$property.NiceName", horizontalLine: false, bold: false)]
        public int currentTileset
        {
            get => editingMode == EditingMode.Volume ? bb.volumeTilesetSelected.id : bb.spotTilesetSelected.id;
            set
            {
                if (editingMode == EditingMode.Volume)
                {
                    bb.volumeTilesetSelected = AreaTilesetHelper.database.tilesets[value];
                    return;
                }
                bb.spotTilesetSelected = AreaTilesetHelper.database.tilesets[value];
            }
        }

        public TilesetSelection (AreaSceneBlackboard bb, EditingMode editingMode)
        {
            this.bb = bb;
            this.editingMode = editingMode;

            if (AreaTilesetHelper.database?.tilesets == null)
            {
                return;
            }

            var ddl = (ValueDropdownList<int>)availableTilesets;
            foreach (var value in AreaTilesetHelper.database.tilesets.Values)
            {
                if (value.usedAsInterior)
                {
                    continue;
                }
                if (editingMode == EditingMode.Volume && !AreaSceneHelper.volumeTilesetIDs.Contains (value.id))
                {
                    continue;
                }
                ddl.Add (AreaSceneHelper.GetTilesetDisplayName (value), value.id);
            }
        }

        readonly AreaSceneBlackboard bb;
        readonly EditingMode editingMode;
        readonly IEnumerable availableTilesets = new ValueDropdownList<int> ();
    }

    [HideLabel, HideReferenceObjectPicker]
    sealed class BrushSelector : SelfDrawnGUI
    {
        [EnumDropdown]
        [LabelWidth (35f)]
        public AreaManager.EditingVolumeBrush brush
        {
            get => AreaManager.editingVolumeBrush;
            set
            {
                AreaManager.editingVolumeBrush = value;
                bb.brushChanged = true;
            }
        }

        [PropertySpace (2f)]
        [EnumButtons]
        public AreaManager.EditingVolumeBrush brushButtons
        {
            get => brush;
            set => brush = value;
        }

        public BrushSelector (AreaSceneBlackboard bb)
        {
            this.bb = bb;
        }

        readonly AreaSceneBlackboard bb;
    }

    [HideLabel, HideReferenceObjectPicker]
    sealed class RoadSubtypeSelector : SelfDrawnGUI
    {
        [EnumDropdown]
        [LabelWidth (35f)]
        public AreaManager.RoadSubtype type
        {
            get => AreaManager.roadSubtype;
            set => AreaManager.roadSubtype = value;
        }

        [PropertySpace (2f)]
        [EnumButtons]
        public AreaManager.RoadSubtype subtypeButtons
        {
            get => type;
            set => type = value;
        }
    }

    [HideLabel, HideReferenceObjectPicker]
    sealed class DamageDepths : SelfDrawnGUI
    {
        [ShowInInspector]
        [PropertyRange (0, nameof(maxDepth))]
        public int damageableDepth
        {
            get => bb.am.damageRestrictionDepth;
            set => bb.am.damageRestrictionDepth = value;
        }

        [ShowInInspector]
        [PropertyRange (0, nameof(maxDepth))]
        public int penetrateableDepth
        {
            get => bb.am.damagePenetrationDepth;
            set => bb.am.damagePenetrationDepth = value;
        }

        int maxDepth => bb.am.boundsFull.y;

        public DamageDepths (AreaSceneBlackboard bb)
        {
            this.bb = bb;
        }

        readonly AreaSceneBlackboard bb;
    }

    [AttributeUsage (AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    sealed class MiniLabelAttribute : ShowInInspectorAttribute
    {
        public readonly string LabelText = "@$value";
        public string VisibleIf;
        public bool Multiline;
    }

    [AttributeUsage (AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class EnumButtonsAttribute : ShowInInspectorAttribute
    {
    }

    [AttributeUsage (AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class EnumDropdownAttribute : ShowInInspectorAttribute
    {
    }

    [AttributeUsage (AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class OnCommandAttribute : ShowInInspectorAttribute
    {
        public readonly string Command;

        public OnCommandAttribute (string command)
        {
            Command = command;
        }
    }

    [AttributeUsage (AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public sealed class BoxTitleGroupAttribute : PropertyGroupAttribute
    {
        public readonly string Title;
        public float SpaceBefore;
        public float SpaceAfter;

        public BoxTitleGroupAttribute (string path)
            : base (path)
        {
            Title = path.Split ('/').Last ();
        }

        public BoxTitleGroupAttribute (string path, string title)
            : base (path)
        {
            Title = title;
        }
    }

    [AttributeUsage (AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class BoundsCheckAttribute : Attribute
    {
        public readonly string ErrorField;

        public BoundsCheckAttribute (string errorField)
        {
            ErrorField = "$" + errorField;
        }
    }

    [AttributeUsage (AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public sealed class ConditionalInfoBoxAttribute : Attribute
    {
        public readonly string Message;
        public string MessageType;
        public string VisibleIf;

        public ConditionalInfoBoxAttribute (string message)
        {
            Message = message;
        }
    }

    [AttributeUsage (AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class EnableFieldIfAttribute : Attribute
    {
        public string X;
        public string Y;
        public string Z;
    }

    public sealed class MiniSliderAttribute : Attribute
    {
        public readonly string MinGetter;
        public readonly string MaxGetter;
        public readonly float MinValue;
        public readonly float MaxValue;

        public MiniSliderAttribute (float minValue, float maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public MiniSliderAttribute (string minGetter, string maxGetter)
        {
            MinGetter = minGetter;
            MaxGetter = maxGetter;
        }

        public MiniSliderAttribute (float minValue, string maxGetter)
        {
            MinValue = minValue;
            MaxGetter = maxGetter;
        }

        public MiniSliderAttribute (string minGetter, float maxValue)
        {
            MinGetter = minGetter;
            MaxValue = maxValue;
        }
    }

    [AttributeUsage (AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public sealed class BoxWithBackgroundGroupAttribute : PropertyGroupAttribute
    {
        public BoxWithBackgroundGroupAttribute(string path) : base(path) { }
    }
}
