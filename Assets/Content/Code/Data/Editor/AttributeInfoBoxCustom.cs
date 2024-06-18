using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public sealed class InfoBoxBottomAttributeDrawer : OdinAttributeDrawer<InfoBoxBottomAttribute>
{
    private bool drawMessageBox;
    private ValueResolver<bool> visibleIfResolver;
    private ValueResolver<string> messageResolver;
    private ValueResolver<Color> iconColorResolver;
    private ValueResolver<Color> overlayColorResolver;
    private MessageType messageType;
    
    protected override void Initialize()
    {
        this.visibleIfResolver = ValueResolver.Get(this.Property, this.Attribute.VisibleIf, true);
        this.messageResolver = ValueResolver.GetForString(this.Property, this.Attribute.Message);
        this.iconColorResolver = ValueResolver.Get(this.Property, this.Attribute.IconColor, EditorStyles.label.normal.textColor);
        this.overlayColorResolver = ValueResolver.Get(this.Property, this.Attribute.OverlayColor, Color.white);
        this.drawMessageBox = this.visibleIfResolver.GetValue();

        switch (this.Attribute.InfoMessageType)
        {
            default:
            case InfoMessageType.None:
                this.messageType = MessageType.None;
                break;
            case InfoMessageType.Info:
                this.messageType = MessageType.Info;
                break;
            case InfoMessageType.Warning:
                this.messageType = MessageType.Warning;
                break;
            case InfoMessageType.Error:
                this.messageType = MessageType.Error;
                break;
        }
    }

    protected override void DrawPropertyLayout(GUIContent label)
    {
        // And the message boxe below:
        bool valid = true;

        if (this.visibleIfResolver.HasError)
        {
            SirenixEditorGUI.ErrorMessageBox(this.visibleIfResolver.ErrorMessage);
            valid = false;
        }

        if (this.messageResolver.HasError)
        {
            SirenixEditorGUI.ErrorMessageBox(this.messageResolver.ErrorMessage);
            valid = false;
        }

        if (this.iconColorResolver.HasError)
        {
            SirenixEditorGUI.ErrorMessageBox(this.iconColorResolver.ErrorMessage);
            valid = false;
        }
        
        if (this.overlayColorResolver.HasError)
        {
            SirenixEditorGUI.ErrorMessageBox(this.overlayColorResolver.ErrorMessage);
            valid = false;
        }
        
        if (valid && Event.current.type == EventType.Layout)
        {
            this.drawMessageBox = this.visibleIfResolver.GetValue();
        }

        var c = GUI.color;
        if (this.drawMessageBox)
            GUI.color = this.overlayColorResolver.GetValue ();
        
        // Draw the value above
        this.CallNextDrawer(label);

        GUI.color = c;

        if (!valid)
        {
            return;
        }

        if (this.Attribute.GUIAlwaysEnabled)
        {
            GUIHelper.PushGUIEnabled(true);
        }

        if (this.drawMessageBox)
        {
            var message = this.messageResolver.GetValue();

            if (this.Attribute.HasDefinedIcon)
            {
                SirenixEditorGUI.IconMessageBox(message, this.Attribute.Icon, this.iconColorResolver.GetValue());
            }
            else
            {
                SirenixEditorGUI.MessageBox(message, this.messageType);
            }
        }

        if (this.Attribute.GUIAlwaysEnabled)
        {
            GUIHelper.PopGUIEnabled();
        }
    }
}