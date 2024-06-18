using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public class DataBlockBaseActionTargeting
    {
        public bool navigable = false;
        public float rangeMin = 0f;
        public float rangeMax = 100f;
    }
    
    [LabelWidth (120f)]
    public class DataContainerBaseAction : DataContainerWithText
    {
        [DataEditor.SpriteNameAttribute (true, 32f)]
        public string icon;
        
        [HorizontalGroup ("A")]
        [LabelText ("Priority / Color")]
        public int priority;

        [HorizontalGroup ("A")]
        [HideLabel]
        public Color color;

        public string inputKey;
        
        [HideIf ("instant")]
        [Tooltip ("Abilities belonging to same group can't be activated together")]
        public string group;

        [HideIf ("@string.IsNullOrEmpty (group) || instant")]
        [LabelText ("Conflict Block")]
        [Tooltip ("If true, this ability can't be activated if another ability from same group is active. If false, it can be activated but will end conflicting ability")]
        public bool groupActivationBlock;

        [Tooltip ("Whether this ability represents an ongoing effect and should be tracked by the game, preventing repeated activation")]
        public bool instant;

        [Tooltip ("Whether this ability requires additional confirmation to start")]
        public bool dialog;

        [YamlIgnore]
        [LabelText ("Name / Desc.")]
        public string textName;

        [YamlIgnore]
        [HideLabel, TextArea (1, 10)]
        public string textDesc;

        [ShowIf ("dialog")]
        [LabelText ("Header / Content.")]
        public string textDialogHeader;

        [ShowIf ("dialog")]
        [HideLabel, TextArea (1, 10)]
        public string textDialogContent;
        
        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerResource.data.Keys")]
        public string resourceKeyLinked;
        
        [DropdownReference (true)]
        public DataBlockBaseActionTargeting targeting;

        [DropdownReference]
        [ListDrawerSettings (DefaultExpandedState = true, AlwaysAddDefaultValue = true, ShowPaging = false)]
        public List<string> callsForUnlock;
        
        [DropdownReference]
        [ListDrawerSettings (DefaultExpandedState = true, AlwaysAddDefaultValue = true, ShowPaging = false)]
        public List<string> callsForAvailability;
        
        [DropdownReference]
        [ListDrawerSettings (DefaultExpandedState = true, AlwaysAddDefaultValue = true, ShowPaging = false)]
        public List<string> callsOnStart;
        
        [DropdownReference]
        [ListDrawerSettings (DefaultExpandedState = true, AlwaysAddDefaultValue = true, ShowPaging = false)]
        public List<string> callsOnEnd;


        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
        }

        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.baseActions, $"{key}__core_header");
            textDesc = DataManagerText.GetText (TextLibs.baseActions, $"{key}__core_text");

            if (dialog)
            {
                textDialogHeader = DataManagerText.GetText (TextLibs.baseActions, $"{key}__dialog_header");
                textDialogContent = DataManagerText.GetText (TextLibs.baseActions, $"{key}__dialog_text");
            }
        }

        #region Editor
        #if UNITY_EDITOR

        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.baseActions, $"{key}__core_header", textName);
            DataManagerText.TryAddingTextToLibrary (TextLibs.baseActions, $"{key}__core_text", textDesc);
            
            if (dialog)
            {
                DataManagerText.TryAddingTextToLibrary (TextLibs.baseActions, $"{key}__dialog_header", textDialogHeader);
                DataManagerText.TryAddingTextToLibrary (TextLibs.baseActions, $"{key}__dialog_text", textDialogContent);
            }
        }
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerBaseAction () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif

        #endregion
    }
}

