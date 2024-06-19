using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ActionResolvers;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;

using UnityEditor;
using UnityEngine;

namespace Area
{
    using Scene;

    sealed class AreaSceneModeToolbarButtonAttributeDrawer : OdinAttributeDrawer<AreaSceneModeToolbarButtonAttribute>
    {
        protected override void Initialize ()
        {
            modeResolver = ValueResolver.Get (Property, Attribute.CurrentMode, EditingMode.Spot);
            setModeResolver = ActionResolver.Get (Property, Attribute.SetMode);
            enableIfResolver = ValueResolver.Get (Property, Attribute.EnableIf, true);
        }

        protected override bool CanDrawAttributeProperty (InspectorProperty property)
        {
            return property.Info.PropertyType == PropertyType.Value
                && property.Info.TypeOfValue == typeof(EditingMode);
        }

        protected override void DrawPropertyLayout (GUIContent label)
        {
            ValueResolver.DrawErrors (modeResolver, enableIfResolver);
            ActionResolver.DrawErrors (setModeResolver);

            var text = Attribute.LabelText;
            var height = Attribute.ButtonHeight;
            var width = Attribute.ButtonWidth;
            var mode = (EditingMode)Property.ValueEntry.WeakSmartValue;
            var currentMode = mode == modeResolver.GetValue ();

            bool pressed;
            var enabled = enableIfResolver.GetValue () || currentMode;
            using (new EditorGUI.DisabledScope (!enabled))
            {
                var bg = GUI.backgroundColor;
                if (currentMode)
                {
                    GUI.backgroundColor = Color.gray;
                }
                pressed = GUILayout.Button (GUIContent.none, GUI.skin.button, GUILayoutOptions.MaxHeight (height).MinHeight (height).MinWidth (width).MaxWidth (width));
                GUI.backgroundColor = bg;

                var rect = GUILayoutUtility.GetLastRect ();
                rect.Set (rect.x + 3, rect.y + 2, rect.width - 10, rect.height - 3);
                label = new GUIContent (text);
                GUI.Label (rect, label, currentMode ? SirenixGUIStyles.WhiteLabelCentered : SirenixGUIStyles.WhiteLabel);
            }

            if (currentMode || !pressed)
            {
                return;
            }

            Debug.Log ("Editing mode: " + mode);
            setModeResolver.DoAction ();
            if (setModeResolver.HasError)
            {
                Debug.Log ("ToolbarButton DoAction error: " + setModeResolver.ErrorMessage);
            }
        }

        ValueResolver<EditingMode> modeResolver;
        ActionResolver setModeResolver;
        ValueResolver<bool> enableIfResolver;
    }

    sealed class MiniLabelAttributeDrawer : OdinAttributeDrawer<MiniLabelAttribute>
    {
        protected override void Initialize ()
        {
            visibleIfResolver = ValueResolver.Get (Property, Attribute.VisibleIf, true);
            if (Property.Info.PropertyType == PropertyType.Method)
            {
                labelTextResolver = ValueResolver.GetForString (Property, "@" + Property.Name + "()");
                return;
            }
            labelTextResolver = ValueResolver.GetForString (Property, Attribute.LabelText);
        }

        protected override bool CanDrawAttributeProperty (InspectorProperty property)
        {
            if (property.Info.PropertyType == PropertyType.Value)
            {
                return true;
            }
            if (property.ParentType == null)
            {
                return false;
            }

            var mi = property.ParentType.GetMethod (property.Name);
            if (mi == null)
            {
                return false;
            }
            return mi.ReturnType == typeof(string);
        }

        protected override void DrawPropertyLayout (GUIContent label)
        {
            ValueResolver.DrawErrors (labelTextResolver, visibleIfResolver);

            if (!visibleIfResolver.GetValue ())
            {
                return;
            }
            style ??= new GUIStyle (EditorStyles.miniLabel)
            {
                wordWrap = Attribute.Multiline,
            };
            GUILayout.Label (labelTextResolver.GetValue (), style);
        }

        ValueResolver<string> labelTextResolver;
        ValueResolver<bool> visibleIfResolver;
        GUIStyle style;
    }

    [DrawerPriority (DrawerPriorityLevel.ValuePriority)]
    sealed class DisplayAsMultilineStringAttributeDrawer : OdinAttributeDrawer<DisplayAsMultilineStringAttribute>
    {
        protected override bool CanDrawAttributeProperty (InspectorProperty property) => property.Info.PropertyType == PropertyType.Value && property.Info.TypeOfValue == typeof(string);

        protected override void DrawPropertyLayout (GUIContent label)
        {
            var text = Property.ValueEntry.WeakSmartValue as string;
            if (string.IsNullOrEmpty (text))
            {
                CallNextDrawer (label);
                return;
            }

            var lines = text.Split ('\n');
            foreach (var line in lines)
            {
                GUILayout.Label (line, EditorStyles.label);
            }
        }
    }

    [DrawerPriority (DrawerPriorityLevel.ValuePriority)]
    sealed class EditingVolumeBrushButtonsDrawer : OdinValueDrawer<AreaManager.EditingVolumeBrush>
    {
        protected override bool CanDrawValueProperty (InspectorProperty property) => property.GetAttribute<EnumButtonsAttribute> () != null;

        protected override void DrawPropertyLayout (GUIContent label)
        {
            GUILayout.BeginHorizontal ();
            var value = (int)ValueEntry.SmartValue;
            foreach (var k in labelMapKeys)
            {
                var style = k == firstButton
                    ? SirenixGUIStyles.MiniButtonLeft
                    : k == lastButton
                        ? SirenixGUIStyles.MiniButtonRight
                        : SirenixGUIStyles.MiniButtonMid;
                var bg = GUI.backgroundColor;
                if (k == value)
                {
                    GUI.backgroundColor = Color.gray;
                }
                if (GUILayout.Button (labelMap[k], style))
                {
                    ValueEntry.SmartValue = (AreaManager.EditingVolumeBrush)k;
                }
                GUI.backgroundColor = bg;
            }
            GUILayout.EndHorizontal ();
        }

        static readonly Dictionary<int, string> labelMap = new Dictionary<int, string> ()
        {
            [(int)AreaManager.EditingVolumeBrush.Point] = " 1x1 ",
            [(int)AreaManager.EditingVolumeBrush.Square2x2] = "2x2R",
            [(int)AreaManager.EditingVolumeBrush.Square3x3] = "3x3R",
            [(int)AreaManager.EditingVolumeBrush.Circle] = "3x3C",
        };
        static readonly List<int> labelMapKeys = labelMap.Keys.OrderBy (k => k).ToList ();
        static readonly int firstButton = labelMapKeys.First ();
        static readonly int lastButton = labelMapKeys.Last ();
    }

    [DrawerPriority (DrawerPriorityLevel.ValuePriority)]
    sealed class RoadSubtypeButtonsDrawer : OdinValueDrawer<AreaManager.RoadSubtype>
    {
        protected override bool CanDrawValueProperty (InspectorProperty property) => property.GetAttribute<EnumButtonsAttribute> () != null;

        protected override void DrawPropertyLayout (GUIContent label)
        {
            GUILayout.BeginHorizontal ();
            var value = (int)ValueEntry.SmartValue;
            foreach (var k in labelMapKeys)
            {
                var style = k == firstButton
                    ? SirenixGUIStyles.MiniButtonLeft
                    : k == lastButton
                        ? SirenixGUIStyles.MiniButtonRight
                        : SirenixGUIStyles.MiniButtonMid;
                var bg = GUI.backgroundColor;
                if (k == value)
                {
                    GUI.backgroundColor = Color.gray;
                }
                if (GUILayout.Button (labelMap[k], style))
                {
                    ValueEntry.SmartValue = (AreaManager.RoadSubtype)k;
                }
                GUI.backgroundColor = bg;
            }
            GUILayout.EndHorizontal ();
        }

        static readonly Dictionary<int, string> labelMap = new Dictionary<int, string> ()
        {
            [(int)AreaManager.RoadSubtype.GrassDirt] = "G+D",
            [(int)AreaManager.RoadSubtype.GrassCurb] = "G+C",
            [(int)AreaManager.RoadSubtype.ConcreteCurb] = "C+C",
            [(int)AreaManager.RoadSubtype.TileCurb] = "T+C",
        };
        static readonly List<int> labelMapKeys = labelMap.Keys.OrderBy (k => k).ToList ();
        static readonly int firstButton = labelMapKeys.First ();
        static readonly int lastButton = labelMapKeys.Last ();
    }

    [DrawerPriority (DrawerPriorityLevel.ValuePriority)]
    sealed class EditingVolumeBrushListDrawer : OdinValueDrawer<AreaManager.EditingVolumeBrush>
    {
        protected override bool CanDrawValueProperty (InspectorProperty property) => property.GetAttribute<EnumDropdownAttribute> () != null;

        protected override void DrawPropertyLayout (GUIContent label)
        {
            var rect = EditorGUILayout.GetControlRect ();
            var rectDropdown = rect.MinHeight (EditorGUIUtility.singleLineHeight);
            var rectButtonRight = rectDropdown.TakeFromRight (22f);
            var rectButtonLeft = rectDropdown.TakeFromRight (22f);
            ValueEntry.SmartValue = EnumSelector<AreaManager.EditingVolumeBrush>.DrawEnumField (rectDropdown, label, ValueEntry.SmartValue);
            if (GUI.Button (rectButtonLeft, EditorGUIUtility.IconContent ("d_back"), SirenixGUIStyles.Button))
            {
                Roll (false);
            }
            if (GUI.Button (rectButtonRight, EditorGUIUtility.IconContent ("d_forward"), SirenixGUIStyles.Button))
            {
                Roll (true);
            }
        }

        void Roll (bool forward)
        {
            var v = (int)ValueEntry.SmartValue;
            ValueEntry.SmartValue = (AreaManager.EditingVolumeBrush)v.OffsetAndWrap (forward, 0, (int)AreaManager.EditingVolumeBrush.Circle);
        }
    }

    [DrawerPriority (DrawerPriorityLevel.ValuePriority)]
    sealed class RoadSubtypeListDrawer : OdinValueDrawer<AreaManager.RoadSubtype>
    {
        protected override bool CanDrawValueProperty (InspectorProperty property) => property.GetAttribute<EnumDropdownAttribute> () != null;

        protected override void DrawPropertyLayout (GUIContent label)
        {
            var rect = EditorGUILayout.GetControlRect ();
            var rectDropdown = rect.MinHeight (EditorGUIUtility.singleLineHeight);
            var rectButtonRight = rectDropdown.TakeFromRight (22f);
            var rectButtonLeft = rectDropdown.TakeFromRight (22f);
            ValueEntry.SmartValue = EnumSelector<AreaManager.RoadSubtype>.DrawEnumField (rectDropdown, label, ValueEntry.SmartValue);
            if (GUI.Button (rectButtonLeft, EditorGUIUtility.IconContent ("d_back"), SirenixGUIStyles.Button))
            {
                Roll (false);
            }
            if (GUI.Button (rectButtonRight, EditorGUIUtility.IconContent ("d_forward"), SirenixGUIStyles.Button))
            {
                Roll (true);
            }
        }

        void Roll (bool forward)
        {
            var v = (int)ValueEntry.SmartValue;
            var delta = forward ? 10 : -10;
            ValueEntry.SmartValue = (AreaManager.RoadSubtype)v.OffsetAndWrap (delta, (int)AreaManager.RoadSubtype.TileCurb);
        }
    }

    [DrawerPriority (DrawerPriorityLevel.AttributePriority)]
    sealed class InspectorSurrogateGroupButtonDrawer : OdinValueDrawer<EditingMode>
    {
        protected override void Initialize ()
        {
            var attr = Property.GetAttribute<InspectorSurrogateGroupButtonAttribute> ();
            modeResolver = ValueResolver.Get (Property, attr.CurrentMode, EditingMode.Spot);
            setModeResolver = ActionResolver.Get (Property, attr.SetMode);
            enableIfResolver = ValueResolver.Get (Property, attr.EnableIf, true);
        }

        protected override bool CanDrawValueProperty (InspectorProperty property)
        {
            return property.Info.PropertyType == PropertyType.Value
                && property.Info.TypeOfValue == typeof(EditingMode)
                && property.Attributes.HasAttribute<InspectorSurrogateGroupButtonAttribute> ();
        }

        protected override void DrawPropertyLayout (GUIContent label)
        {
            ValueResolver.DrawErrors (modeResolver);
            ActionResolver.DrawErrors (setModeResolver);

            var attr = Property.GetAttribute<InspectorSurrogateGroupButtonAttribute> ();
            var text = attr.LabelText;
            var mode = ValueEntry.SmartValue;
            var currentMode = mode == modeResolver.GetValue ();

            bool pressed;
            var enabled = enableIfResolver.GetValue () || currentMode;
            using (new EditorGUI.DisabledScope (!enabled))
            {
                var bg = GUI.backgroundColor;
                if (currentMode)
                {
                    GUI.backgroundColor = Color.gray;
                }
                pressed = GUILayout.Button (GUIHelper.TempContent (text), GUI.skin.button);
                GUI.backgroundColor = bg;
            }

            if (currentMode || !pressed)
            {
                return;
            }

            Debug.Log ("Editing mode: " + mode);
            setModeResolver.DoAction ();
            if (setModeResolver.HasError)
            {
                Debug.Log ("Inspector surrogate mode button DoAction error: " + setModeResolver.ErrorMessage);
            }
        }

        ValueResolver<EditingMode> modeResolver;
        ActionResolver setModeResolver;
        ValueResolver<bool> enableIfResolver;
    }

    sealed class Vector3IntDrawer : OdinValueDrawer<Vector3Int>
    {
        protected override void Initialize ()
        {
            var attr = Property.GetAttribute<BoundsCheckAttribute> ();
            if (attr != null)
            {
                errorFieldResolver = ValueResolver.Get (Property, attr.ErrorField, -1);
            }

            var attrEnableIf = Property.GetAttribute<EnableFieldIfAttribute> ();
            if (attrEnableIf != null)
            {
                enableFieldIfXResolver = ValueResolver.Get (Property, attrEnableIf.X, true);
                enableFieldIfYResolver = ValueResolver.Get (Property, attrEnableIf.Y, true);
                enableFieldIfZResolver = ValueResolver.Get (Property, attrEnableIf.Z, true);
            }

            var attrWidth = Property.GetAttribute<LabelWidthAttribute> ();
            if (attrWidth != null)
            {
                labelWidth = attrWidth.Width;
            }
        }

        protected override void DrawPropertyLayout (GUIContent label)
        {
            var enabledX = true;
            var enabledY = true;
            var enabledZ = true;

            if (errorFieldResolver != null)
            {
                ValueResolver.DrawErrors (errorFieldResolver);
                lastErrorField = errorFieldResolver.GetValue ();
            }
            if (enableFieldIfXResolver != null)
            {
                ValueResolver.DrawErrors (enableFieldIfXResolver, enableFieldIfYResolver, enableFieldIfZResolver);
                enabledX = enableFieldIfXResolver.GetValue ();
                enabledY = enableFieldIfYResolver.GetValue ();
                enabledZ = enableFieldIfZResolver.GetValue ();
            }

            var rectControl = EditorGUILayout.GetControlRect ();
            rectControl = rectControl.MinHeight (EditorGUIUtility.singleLineHeight);

            if (labelWidth == 0f)
            {
                var size = SirenixGUIStyles.Label.CalcSize (label);
                labelWidth = size.x;
            }
            var rectLabel = rectControl.TakeFromLeft (labelWidth);
            GUI.Label (rectLabel, label, SirenixGUIStyles.Label);

            var v = ValueEntry.SmartValue;
            var rects = CalcRects (rectControl);
            var x = DrawIntField (rects[0], rects[1], fieldLabels[0], v.x, enabledX);
            var z = DrawIntField (rects[2], rects[3], fieldLabels[1], v.z, enabledZ);
            var y = DrawIntField (rects[4], rects[5], fieldLabels[2], v.y, enabledY);
            if (x == v.x && z == v.z && y == v.y)
            {
                return;
            }
            ValueEntry.SmartValue = new Vector3Int (x, y, z);
        }

        Rect[] CalcRects (Rect rectControl)
        {
            var rects = new Rect[fieldLabels.Length * 2];
            for (var i = fieldLabels.Length - 1; i >= 0; i -= 1)
            {
                rects[i * 2 + 1] = rectControl.TakeFromRight (35f);
                var style = SirenixGUIStyles.LeftAlignedGreyMiniLabel;
                var content = GUIHelper.TempContent (fieldLabels[i]);
                var width = style.CalcSize (content).x;
                rects[i * 2] = rectControl.TakeFromRight (width);
                rectControl.TakeFromRight (2f);
            }
            return rects;
        }

        int DrawIntField (Rect rectLabel, Rect rectValue, string label, int value, bool enabled)
        {
            var content = GUIHelper.TempContent (label);
            GUI.Label (rectLabel, content, SirenixGUIStyles.LeftAlignedGreyMiniLabel);
            var hasError = lastErrorField != -1 && fieldLabels[lastErrorField] == label;
            if (hasError)
            {
                GUIHelper.PushColor (Color.red);
            }
            using (new EditorGUI.DisabledScope (!enabled))
            {
                value = SirenixEditorFields.IntField (rectValue, value);
            }
            if (hasError)
            {
                GUIHelper.PopColor ();
            }
            return value;
        }

        ValueResolver<int> errorFieldResolver;
        ValueResolver<bool> enableFieldIfXResolver;
        ValueResolver<bool> enableFieldIfYResolver;
        ValueResolver<bool> enableFieldIfZResolver;
        float labelWidth;
        int lastErrorField = -1;

        static readonly string[] fieldLabels = { "x", "z", "y", };
    }

    sealed class TransferModeRotationDrawer : OdinValueDrawer<TransferModePanelControls.Rotation>
    {
        protected override void Initialize ()
        {
            var attr = Property.GetAttribute<OnCommandAttribute> ();
            if (!string.IsNullOrEmpty (attr.Command))
            {
                commandResolver = ActionResolver.Get (Property, attr.Command);
            }

            var attrWidth = Property.GetAttribute<LabelWidthAttribute> ();
            if (attrWidth != null)
            {
                labelWidth = attrWidth.Width;
            }
        }

        protected override bool CanDrawValueProperty (InspectorProperty property) => property.Attributes.HasAttribute<OnCommandAttribute> ();

        protected override void DrawPropertyLayout (GUIContent label)
        {
            if (commandResolver != null)
            {
                ActionResolver.DrawErrors (commandResolver);
            }

            var rect = EditorGUILayout.GetControlRect ();
            var rectControl = rect.MinHeight (EditorGUIUtility.singleLineHeight);
            if (labelWidth == 0f)
            {
                labelWidth = SirenixGUIStyles.Label.CalcSize (label).x;
            }
            var rectLabel = rectControl.TakeFromLeft (labelWidth);
            GUI.Label (rectLabel, label, SirenixGUIStyles.Label);
            rectControl.TakeFromLeft (4f);
            rectControl = DrawButton (rectControl, TransferModePanelControls.Rotation.Anticlockwise, "←", SirenixGUIStyles.ButtonLeft);
            rectControl = DrawButton (rectControl, TransferModePanelControls.Rotation.Flip, "↔", SirenixGUIStyles.ButtonMid);
            DrawButton (rectControl, TransferModePanelControls.Rotation.Clockwise, "→", SirenixGUIStyles.ButtonRight);
        }

        Rect DrawButton (Rect rectControl, TransferModePanelControls.Rotation rotation, string label, GUIStyle style)
        {
            var content = GUIHelper.TempContent (label);
            var rectButton = rectControl.TakeFromLeft (31f);
            if (GUI.Button (rectButton, content, style))
            {
                ValueEntry.SmartValue = rotation;
                ValueEntry.ApplyChanges ();
                if (commandResolver == null)
                {
                    return rectControl;
                }
                commandResolver.DoAction ();
                if (commandResolver.HasError)
                {
                    Debug.LogFormat ("Volume rotation {0} button DoAction error: {1}", rotation, commandResolver.ErrorMessage);
                }
            }
            return rectControl;
        }

        ActionResolver commandResolver;
        float labelWidth;
    }

    sealed class BoxTitleGroupAttributeDrawer : OdinGroupDrawer<BoxTitleGroupAttribute>
    {
        protected override void Initialize ()
        {
            titleResolver = ValueResolver.GetForString (Property, Attribute.Title);
        }

        protected override void DrawPropertyLayout (GUIContent label)
        {
            ValueResolver.DrawErrors (titleResolver);

            if (Attribute.SpaceBefore > 0f)
            {
                GUILayout.Space (Attribute.SpaceBefore);
            }
            var title = titleResolver.GetValue ();
            SirenixEditorGUI.BeginBox ();
            SirenixEditorGUI.Title (title, "", TextAlignment.Center, true);
            for (var i = 0; i < Property.Children.Count; i += 1)
            {
                Property.Children[i].Draw ();
            }
            SirenixEditorGUI.EndBox ();
            if (Attribute.SpaceAfter > 0f)
            {
                GUILayout.Space (Attribute.SpaceAfter);
            }
        }

        ValueResolver<string> titleResolver;
    }

    sealed class ConditionalInfoAttributeDrawer : OdinAttributeDrawer<ConditionalInfoBoxAttribute>
    {
        protected override void Initialize ()
        {
            messageResolver = ValueResolver.GetForString (Property, Attribute.Message);
            messageTypeResolver = ValueResolver.Get (Property, Attribute.MessageType, MessageType.Info);
            visibleIfResolver = ValueResolver.Get (Property, Attribute.VisibleIf, true);
        }

        protected override void DrawPropertyLayout (GUIContent label)
        {
            ValueResolver.DrawErrors (messageResolver, messageTypeResolver, visibleIfResolver);
            if (!visibleIfResolver.GetValue ())
            {
                CallNextDrawer (label);
                return;
            }
            var msg = messageResolver.GetValue ();
            var msgType = messageTypeResolver.GetValue ();
            EditorGUILayout.HelpBox (msg, msgType);
            CallNextDrawer (label);
        }

        ValueResolver<string> messageResolver;
        ValueResolver<MessageType> messageTypeResolver;
        ValueResolver<bool> visibleIfResolver;
    }

    sealed class MiniSliderAttributeDrawer : OdinAttributeDrawer<MiniSliderAttribute>
    {
        protected override void Initialize ()
        {
            minValueResolver = ValueResolver.Get (Property, Attribute.MinGetter, Attribute.MinValue);
            maxValueResolver = ValueResolver.Get (Property, Attribute.MaxGetter, Attribute.MaxValue);
        }

        protected override void DrawPropertyLayout (GUIContent label)
        {
            ValueResolver.DrawErrors (minValueResolver, maxValueResolver);

            var labelHeight = SirenixGUIStyles.LeftAlignedWhiteMiniLabel.CalcSize (label).y;
            var rectControl = EditorGUILayout.GetControlRect (false, labelHeight + verticalSpacing + EditorGUIUtility.singleLineHeight);
            var rectLabel = rectControl.TakeFromTop (labelHeight);
            GUI.Label (rectLabel, label, SirenixGUIStyles.LeftAlignedWhiteMiniLabel);
            rectControl.TakeFromTop (verticalSpacing);
            Property.ValueEntry.WeakSmartValue = SirenixEditorFields.RangeFloatField (rectControl, (float)Property.ValueEntry.WeakSmartValue, minValueResolver.GetValue (), maxValueResolver.GetValue ());
        }

        ValueResolver<float> minValueResolver;
        ValueResolver<float> maxValueResolver;

        const float verticalSpacing = 3f;
    }

    sealed class BoxWithBackgroundGroupAttributeDrawer : OdinGroupDrawer<BoxWithBackgroundGroupAttribute>
    {
        protected override void Initialize ()
        {
            visibleIfResolver = ValueResolver.Get (Property, Attribute.VisibleIf, true);
        }

        protected override void DrawPropertyLayout (GUIContent label)
        {
            ValueResolver.DrawErrors (visibleIfResolver);

            if (!visibleIfResolver.GetValue ())
            {
                return;
            }

            GUILayout.BeginVertical ("Box");
            for (var i = 0; i < Property.Children.Count; i += 1)
            {
                Property.Children[i].Draw();
            }
            GUILayout.EndVertical ();
        }

        ValueResolver<bool> visibleIfResolver;
    }

    sealed class MarkedCellsDrawer : OdinValueDrawer<MarkedCells>
    {
        protected override void DrawPropertyLayout (GUIContent label)
        {
            var child = Property.Children[0];
            for (var i = 0; i < child.Children.Count; i += 1)
            {
                if (child.Children[i].ValueEntry.WeakSmartValue == null)
                {
                    continue;
                }
                child.Children[i].Draw ();
            }
        }
    }

    sealed class MarkedCellListDrawer : OdinValueDrawer<MarkedCellList>
    {
        protected override void DrawPropertyLayout (GUIContent label)
        {
            var marked = ValueEntry.SmartValue.marked;
            var cellCount = marked.Count;
            if (cellCount == 0)
            {
                return;
            }

            GUILayout.BeginHorizontal ("Box");
            var rectControl = EditorGUILayout.GetControlRect ();
            var expanded = ValueEntry.SmartValue.expanded;
            var buttonText = expanded ? "▲" : "▼";
            var content = GUIHelper.TempContent (buttonText);
            var buttonStyle = SirenixGUIStyles.LeftAlignedWhiteMiniLabel;
            var width = buttonStyle.CalcSize (content).x;
            var rectDropdownButton = rectControl.TakeFromLeft (width);
            rectControl.TakeFromLeft (2f);
            var layer = ValueEntry.SmartValue.layer;
            var labelLayer = "Layer " + layer;
            content = GUIHelper.TempContent (labelLayer);
            width = SirenixGUIStyles.Label.CalcSize (content).x;
            var rectLabel = rectControl.TakeFromLeft (width);
            var rectGoButton = rectControl.TakeFromRight (30f);
            content = GUIHelper.TempContent (buttonText);
            if (GUI.Button (rectDropdownButton, content, buttonStyle))
            {
                ValueEntry.SmartValue.expanded = !expanded;
            }
            content = GUIHelper.TempContent (labelLayer);
            GUI.Label (rectLabel, content, SirenixGUIStyles.Label);
            content = GUIHelper.TempContent (cellCount + " cell" + (cellCount == 1 ? "" : "s"));
            GUI.Label (rectControl, content, SirenixGUIStyles.CenteredWhiteMiniLabel);
            var bb = ValueEntry.SmartValue.bb;
            if (layer != bb.layer && GUI.Button (rectGoButton, "Go"))
            {
                bb.layer = layer;
            }
            GUILayout.EndHorizontal ();

            if (!expanded)
            {
                return;
            }

            GUILayout.BeginVertical ("Box");
            for (var i = 0; i < Property.Children.Count; i += 1)
            {
                Property.Children[i].Draw ();
            }
            GUILayout.EndVertical ();
        }
    }

    sealed class PropModeSelectedPropsDrawer : OdinValueDrawer<PropModeSelectedProps>
    {
        protected override void DrawPropertyLayout (GUIContent label)
        {
            var child = Property.Children[0];
            for (var i = 0; i < child.Children.Count; i += 1)
            {
                var grandchild = child.Children[i];
                if (grandchild.ValueEntry.WeakSmartValue == null)
                {
                    continue;
                }
                var entry = (PropModePanelEntry)grandchild.ValueEntry.WeakSmartValue;
                if (!entry.isSelected)
                {
                    grandchild.Draw ();
                    continue;
                }
                var colorPrevious = GUI.backgroundColor;
                GUI.backgroundColor = Color.Lerp (GUI.backgroundColor, Color.cyan, 0.35f);
                grandchild.Draw ();
                GUI.backgroundColor = colorPrevious;
            }
        }
    }

    sealed class RightFoldoutGroupAttributeDrawer : OdinGroupDrawer<RightFoldoutGroupAttribute>
    {
        protected override void DrawPropertyLayout (GUIContent label)
        {
            GUILayout.BeginVertical ("Box");
            var width = label != null ? SirenixGUIStyles.RightAlignedWhiteMiniLabel.CalcSize (label).x + 20f : 100f;
            Property.State.Expanded = UtilityCustomInspector.DrawFoldout (Attribute.Title, Property.State.Expanded, GUILayoutOptions.MinWidth (width).ExpandWidth ());
            if (SirenixEditorGUI.BeginFadeGroup (this, Property.State.Expanded))
            {
                for (var i = 0; i < Property.Children.Count; i += 1)
                {
                    Property.Children[i].Draw ();
                }
            }
            SirenixEditorGUI.EndFadeGroup ();
            GUILayout.EndVertical ();
        }
    }
}
