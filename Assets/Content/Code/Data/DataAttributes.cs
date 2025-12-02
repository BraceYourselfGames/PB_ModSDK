using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
    public enum DictionaryKeyDropdownType
    {
        Expression,
        Socket,
        SocketTag,
        Hardpoint,
        HardpointTag,
        Stat,
        ActionCustomInt,
        ActionCustomFloat,
        ActionCustomVector,
        ActionCustomString,
        PartCustomInt,
        PartCustomFloat,
        PartCustomVector,
        PartCustomString,
        UnitPresetTag,
        UnitGroupTag,
        ScenarioTag,
        AreaTag
    }

    public enum DictionaryValueDropdownType
    {
        Expression,
        Socket,
        Hardpoint,
        PartPreset,
        PartPresetForSocketKey,
        Subsystem,
        SubsystemForHardpointKey
    }

    [AttributeUsage (AttributeTargets.Field)]
    [Conditional ("UNITY_EDITOR")]
    public class DictionaryKeyDropdown : Attribute
    {
        public DictionaryKeyDropdownType type;
        public string expression = string.Empty;
        public bool append = false;
        public bool allowEmpty = false;

        public DictionaryKeyDropdown (string expression, bool append = false, bool allowEmpty = false)
        {
            this.type = DictionaryKeyDropdownType.Expression;
            this.expression = expression;
            this.append = append;
            this.allowEmpty = allowEmpty;
        }

        public DictionaryKeyDropdown (DictionaryKeyDropdownType type, bool append = false, bool allowEmpty = false)
        {
            this.type = type;
            this.expression = null;
            this.append = append;
            this.allowEmpty = allowEmpty;
        }
    }

    [AttributeUsage (AttributeTargets.Field)]
    [Conditional ("UNITY_EDITOR")]
    public class DictionaryValueDropdown : Attribute
    {
        public DictionaryValueDropdownType type;
        public string expression = string.Empty;
        public bool append = false;
        public bool allowEmpty = false;

        public DictionaryValueDropdown (string expression, bool append = false, bool allowEmpty = false)
        {
            this.type = DictionaryValueDropdownType.Expression;
            this.expression = expression;
            this.append = append;
            this.allowEmpty = allowEmpty;
        }

        public DictionaryValueDropdown (DictionaryValueDropdownType type, bool append = false, bool allowEmpty = false)
        {
            this.type = type;
            this.expression = null;
            this.append = append;
            this.allowEmpty = allowEmpty;
        }
    }

    [AttributeUsage (AttributeTargets.Field)]
    [Conditional ("UNITY_EDITOR")]
    public class DictionaryStringKeyTag : Attribute
    {
    }

    [AttributeUsage (AttributeTargets.Field)]
    [Conditional ("UNITY_EDITOR")]
    public class DictionaryStringValueTag : Attribute
    {
    }

    [AttributeUsage (AttributeTargets.Field)]
    public class OnInspectorGUIStart : Attribute
    {
        public string expression;

        public OnInspectorGUIStart (string expression)
        {
            this.expression = expression;
        }
    }

    public class EditorIconShared
    {
        public const string Plus = "Plus";
        public const string Minus = "Minus";
        public const string X = "X";
    }

    [AttributeUsage (AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    [Conditional ("UNITY_EDITOR")]
    public class ButtonIconAttribute : Attribute
    {
        public readonly string Icon;

        public ButtonIconAttribute (string icon = "Transparent")
        {
            Icon = icon;
        }
    }
    
    [AttributeUsage (AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    [Conditional ("UNITY_EDITOR")]
    public class ButtonWithIconAttribute : Attribute
    {
        public readonly SdfIconType IconType;
        public readonly int DrawSizeButton;
        public readonly int DrawSizeIcon;
        public readonly string Tooltip;

        public ButtonWithIconAttribute (SdfIconType iconType)
        { 
            IconType = iconType;
            DrawSizeButton = (int)ButtonSizes.Large;
            DrawSizeIcon = 24;
            Tooltip = null;
        }
        
        public ButtonWithIconAttribute (SdfIconType iconType, string tooltip)
        { 
            IconType = iconType;
            DrawSizeButton = (int)ButtonSizes.Large;
            DrawSizeIcon = 24;
            Tooltip = tooltip;
        }
        
        public ButtonWithIconAttribute (SdfIconType iconType, ButtonSizes buttonSize)
        { 
            IconType = iconType;
            DrawSizeButton = (int)buttonSize;
            DrawSizeIcon = 24;
            Tooltip = null;
        }
        
        public ButtonWithIconAttribute (SdfIconType iconType, ButtonSizes buttonSize, string tooltip)
        { 
            IconType = iconType;
            DrawSizeButton = (int)buttonSize;
            DrawSizeIcon = 24;
            Tooltip = tooltip;
        }

        public ButtonWithIconAttribute (SdfIconType iconType, ButtonSizes buttonSize, int iconSize, string tooltip)
        {
            IconType = iconType;
            DrawSizeButton = (int)buttonSize;
            DrawSizeIcon = iconSize;
            Tooltip = tooltip;
        }
    }
    
    [AttributeUsage (AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    [Conditional ("UNITY_EDITOR")]
    public class ButtonIconSdfAttribute : Attribute
    {
        public readonly SdfIconType IconType;
        public readonly int DrawSize;

        public ButtonIconSdfAttribute (SdfIconType iconType = SdfIconType.ArrowDown, int drawSize = 16)
        {
            IconType = iconType;
            DrawSize = drawSize;
        }
    }
    
    [AttributeUsage (AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    [Conditional ("UNITY_EDITOR")]
    public class InfoBoxBottomAttribute : Attribute
    {
        /// <summary>The message to display in the info box.</summary>
        public string Message;
        /// <summary>The type of the message box.</summary>
        public InfoMessageType InfoMessageType;
        /// <summary>
        /// Optional member field, property or function to show and hide the info box.
        /// </summary>
        public string VisibleIf;
        /// <summary>
        /// When <c>true</c> the InfoBox will ignore the GUI.enable flag and always draw as enabled.
        /// </summary>
        public bool GUIAlwaysEnabled;
        /// <summary>Supports a variety of color formats, including named colors (e.g. "red", "orange", "green", "blue"), hex codes (e.g. "#FF0000" and "#FF0000FF"), and RGBA (e.g. "RGBA(1,1,1,1)") or RGB (e.g. "RGB(1,1,1)"), including Odin attribute expressions (e.g "@this.MyColor"). Here are the available named colors: black, blue, clear, cyan, gray, green, grey, magenta, orange, purple, red, transparent, transparentBlack, transparentWhite, white, yellow, lightblue, lightcyan, lightgray, lightgreen, lightgrey, lightmagenta, lightorange, lightpurple, lightred, lightyellow, darkblue, darkcyan, darkgray, darkgreen, darkgrey, darkmagenta, darkorange, darkpurple, darkred, darkyellow.</summary>
        public string IconColor;
        /// <summary>Supports a variety of color formats, including named colors (e.g. "red", "orange", "green", "blue"), hex codes (e.g. "#FF0000" and "#FF0000FF"), and RGBA (e.g. "RGBA(1,1,1,1)") or RGB (e.g. "RGB(1,1,1)"), including Odin attribute expressions (e.g "@this.MyColor"). Here are the available named colors: black, blue, clear, cyan, gray, green, grey, magenta, orange, purple, red, transparent, transparentBlack, transparentWhite, white, yellow, lightblue, lightcyan, lightgray, lightgreen, lightgrey, lightmagenta, lightorange, lightpurple, lightred, lightyellow, darkblue, darkcyan, darkgray, darkgreen, darkgrey, darkmagenta, darkorange, darkpurple, darkred, darkyellow.</summary>
        public string OverlayColor;
        private SdfIconType icon;

        /// <summary>The icon to be displayed next to the message.</summary>
        public SdfIconType Icon
        {
            get => this.icon;
            set
            {
                this.icon = value;
                this.HasDefinedIcon = true;
            }
        }

        public bool HasDefinedIcon { get; private set; }

        public InfoBoxBottomAttribute (string message, InfoMessageType infoMessageType = InfoMessageType.Info, string visibleIfMemberName = null)
        {
            this.Message = message;
            this.InfoMessageType = infoMessageType;
            this.VisibleIf = visibleIfMemberName;
        }
        
        public InfoBoxBottomAttribute (string message, string visibleIfMemberName)
        {
            this.Message = message;
            this.InfoMessageType = InfoMessageType.Info;
            this.VisibleIf = visibleIfMemberName;
        }

        /// <summary>Displays an info box above the property.</summary>
        /// <param name="message">The message for the message box. Supports referencing a member string field, property or method by using $.</param>
        /// <param name="icon">The icon to be displayed next to the message.</param>
        /// <param name="visibleIfMemberName">Name of member bool to show or hide the message box.</param>
        public InfoBoxBottomAttribute (string message, SdfIconType icon, string visibleIfMemberName = null)
        {
            this.Message = message;
            this.Icon = icon;
            this.VisibleIf = visibleIfMemberName;
            this.InfoMessageType = InfoMessageType.None;
        }
    }

    [AttributeUsage (AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    [Conditional ("UNITY_EDITOR")]
    public class DropdownReference : Attribute
    {
        public readonly bool addBoxGroup;
        public readonly string boxGroupPrefix;

        public DropdownReference (bool addBoxGroup, string boxGroupPrefix)
        {
            this.addBoxGroup = addBoxGroup;
            this.boxGroupPrefix = boxGroupPrefix;
        }
        
        public DropdownReference (bool addBoxGroup)
        {
            this.addBoxGroup = addBoxGroup;
            this.boxGroupPrefix = null;
        }

        public DropdownReference ()
        {
            this.addBoxGroup = false;
            this.boxGroupPrefix = null;
        }
    }

    [AttributeUsage (AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    [Conditional ("UNITY_EDITOR")]
    public class FoldoutReference : Attribute
    {
        public readonly string groupName;

        public FoldoutReference (string groupName)
        {
            this.groupName = groupName;
        }

        public FoldoutReference ()
        {
            
        }
    }
    
    [AttributeUsage (AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    [Conditional ("UNITY_EDITOR")]
    public class InlineButtonClear : Attribute
    {
        public readonly bool clearToNull;
        
        public InlineButtonClear (bool clearToNull)
        {
            this.clearToNull = clearToNull;
        }

        public InlineButtonClear ()
        {
            clearToNull = true;
        }
    }

    [AttributeUsage (AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    [Conditional ("UNITY_EDITOR")]
    public class FoldoutReferenceButton : Attribute
    {
        public readonly string fieldName;
        public readonly string groupName;

        public FoldoutReferenceButton (string fieldName, string groupName)
        {
            this.fieldName = fieldName;
            this.groupName = groupName;
        }

        public FoldoutReferenceButton (string fieldName)
        {
            this.fieldName = fieldName;
        }
    }

    [AttributeUsage (AttributeTargets.Property | AttributeTargets.Field)]
    public class CustomisableFoldoutGroupAttribute : FoldoutGroupAttribute
    {
        public string OnGroupDrawGUI { get; set; }

        public CustomisableFoldoutGroupAttribute (string groupName, string methodName, int order = 0) : base (groupName, order)
        {
            OnGroupDrawGUI = methodName;
        }

        public CustomisableFoldoutGroupAttribute (string groupName, bool expanded, string methodName, int order = 0) : base (groupName, expanded, order)
        {
            OnGroupDrawGUI = methodName;
        }
    }
    
    #pragma warning disable
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [global::System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class ColoredBoxGroupAttribute : BoxGroupAttribute
    {
        public float R, G, B, A;
        public bool BoldLabel;


        public ColoredBoxGroupAttribute(
            string @group,
            float r, float g, float b, float a,
            bool showLabel = true,
            bool centerLabel = false,
            bool boldLabel = false,
            float order = 0) : base(@group, showLabel, centerLabel, order)
        {
            R = r;
            G = g;
            B = b;
            A = a;

            BoldLabel = boldLabel;
        }
    }
    
    [AttributeUsage (AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class InterfaceInfoAttribute : Attribute
    {
        public readonly string info;

        public InterfaceInfoAttribute (string info)
        {
            this.info = info;
        }
    }
}