using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public enum InputActionType
    {
        ButtonPositive,
        ButtonNegative,
        Axis
    }

    public static class InputActionGroup
    {
        public const string Internal = "Internal";
        public const string Global = "Global";
        public const string Overworld = "Overworld";
        public const string Base = "Base";
        public const string Combat = "Combat";
    }
    
    public class DataContainerInputAction : DataContainerWithText
    {
        [YamlIgnore, ShowIf (DataEditor.textAttrArg), HideLabel]
        public string textName;
        
        [YamlIgnore, ShowIf (DataEditor.textAttrArg), HideLabel, TextArea]
        public string textDesc;

        [LabelText ("Action ID")]
        public int actionID;

        public InputActionType type = InputActionType.ButtonPositive;

        public int defaultValueKeyboard = -1;
        public int defaultValueMouse = -1;
        public int defaultValueGamepad = -1;
        public bool buttonAsAxisGamepad = false;
        
        [ValueDropdown ("@FieldReflectionUtility.GetConstantStringFieldValues (typeof (InputActionGroup), false)")]
        [OnValueChanged ("UpdateSortedLists")]
        [HorizontalGroup]
        public string group = InputActionGroup.Internal;

        [HorizontalGroup (60f), HideLabel]
        public int subGroup = 0;

        [OnValueChanged ("UpdateSortedLists")]
        public int priority = 0;

        public bool visible = true;
        public bool locked = false;
        public bool visibleForMKB = false;
        public bool visibleForGamepad = false;
        
        public bool removableValueKeyboard = true;
        public bool removableValueMouse = true;
        public bool removableValueGamepad = true;
    
        public override void OnBeforeSerialization ()
        {
            
            base.OnBeforeSerialization ();
        }

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
        }
        
        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.uiSettingInputAction, $"{key}_name");
        }
        
        #if UNITY_EDITOR
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.uiSettingInputAction, $"{key}_name", textName);
        }

        private void UpdateSortedLists ()
        {
            DataMultiLinkerInputAction.UpdateSortedLists ();
        }
        
        #endif
    }
}