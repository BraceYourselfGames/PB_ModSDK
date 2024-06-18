using System;
using System.Collections.Generic;
using System.Reflection;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public sealed class ButtonIconAttributeDrawer : OdinAttributeDrawer<ButtonIconAttribute>
{
    protected override void DrawPropertyLayout (GUIContent label)
    {
        ButtonIconAttribute attribute = Attribute;
        CallNextDrawer (label);

        var rect = GUIHelper.GetCurrentLayoutRect ();
        var icon = GetIcon (attribute.Icon);
        icon.Draw (rect.Expand (-2), 16);
    }

    private EditorIcon GetIcon (string key)
    {
        var prop = typeof (EditorIcons).GetProperty (key);
        if (prop != null)
            return (EditorIcon) prop.GetValue (null, null);
        else
            return EditorIcons.Transparent;
    }
}

public sealed class ButtonIconSdfAttributeDrawer : OdinAttributeDrawer<ButtonIconSdfAttribute>
{
    protected override void DrawPropertyLayout (GUIContent label)
    {
        ButtonIconSdfAttribute attribute = Attribute;
        CallNextDrawer (label);

        var iconType = attribute.IconType;
        int drawSize = attribute.DrawSize;

        Rect rect = GUILayoutUtility.GetLastRect ();
        Rect rectAligned = rect.AlignCenter(drawSize, drawSize);

        // Rect rectText = rect;
        // rectText.x += drawSize;
        // rectText.width = 256;
        // GUI.Label (rectText, $"Original - S: {rect.size}, P: {rect.position}\nAligned - S: {rectAligned.size}, P: {rectAligned.position}", EditorStyles.miniLabel);

        SdfIcons.DrawIcon (rectAligned, iconType);
    }
}

public class ButtonWithIconAttributeProcessor<T> : OdinAttributeProcessor<T>
{
    public override void ProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        if (parentProperty == null || parentProperty.Parent == null)
            return;
        
        if (!attributes.HasAttribute<ButtonWithIconAttribute> ())
            return;
        
        var source = attributes.GetAttribute<ButtonWithIconAttribute> ();
        var fieldName = member.Name;

        attributes.Add (new ButtonAttribute (" ", source.DrawSizeButton));
        attributes.Add (new ButtonIconSdfAttribute (source.IconType, source.DrawSizeIcon));
        
        if (!string.IsNullOrEmpty (source.Tooltip))
            attributes.Add (new PropertyTooltipAttribute (source.Tooltip));
    }
}

public static class AttributeDrawerUtility
{
    public const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
}

public class CustomisableFoldoutGroupAttributeDrawer : OdinGroupDrawer<CustomisableFoldoutGroupAttribute>
{
    protected override void DrawPropertyLayout (GUIContent label)
    {
        SirenixEditorGUI.BeginBox ();
        SirenixEditorGUI.BeginBoxHeader ();
        Attribute.Expanded = EditorGUILayout.BeginFoldoutHeaderGroup (Attribute.Expanded, Attribute.GroupName, EditorStyles.foldoutHeader);

        if (Attribute.OnGroupDrawGUI != null)
            Property.SerializationRoot.ValueEntry.TypeOfValue.GetMethod (Attribute.OnGroupDrawGUI, AttributeDrawerUtility.flags)?.Invoke (Property.SerializationRoot.ValueEntry.WeakSmartValue, null);

        EditorGUILayout.EndFoldoutHeaderGroup ();
        SirenixEditorGUI.EndBoxHeader ();

        if (Attribute.Expanded)
        {
            for (int i = 0; i < Property.Children.Count; i++)
                Property.Children[i].Draw ();
        }

        SirenixEditorGUI.EndBox ();
    }
}