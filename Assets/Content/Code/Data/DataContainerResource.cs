using System.Collections.Generic;
using PhantomBrigade.Functions;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [HideReferenceObjectPicker]
    public class DataBlockResourceStatSource
    {
        public float multiplier = 1f;
    }

    [HideReferenceObjectPicker]
    public class DataContainerResource : DataContainerWithText
    {
        public bool hidden = false;
        
        [YamlIgnore]
        [LabelText ("Name / Desc.")]
        public string textName;
        
        [YamlIgnore]
        [HideLabel, TextArea (1, 10)]
        public string textDesc;

        public string format = "0.##";
        public int debugAmount = 100;
        public int priority = 0;

        public bool colorOverrideUsed = false;
        public Color colorOverride = Color.white.WithAlpha (1f);
        
        public bool showCounter = true;
        public bool showCounterNegative = false;
        public bool maxOnResupply = false;

        [DataEditor.SpriteNameAttribute (false, 32f)]
        public string icon;

        [ValueDropdown ("@DataMultiLinkerBaseStat.data.Keys")]
        [DropdownReference (true)]
        public string baseStatLimit;
        
        [ValueDropdown ("@DataMultiLinkerBasePart.data.Keys")]
        [DropdownReference (true)]
        public string basePartForResupply;
        
        [DropdownReference (true)]
        public DataBlockFloat constantLimit;
        
        [DropdownReference (true)]
        public DataBlockFloat offsetOnResupply;
        
        [DropdownReference]
        public Dictionary<string, DataBlockResourceStatSource> statLinks;

        public override void OnKeyReplacement (string keyOld, string keyNew)
        {
            base.OnKeyReplacement (keyOld, keyNew);

            FunctionUtility.ReplaceInFunction (typeof (ModifyResources), keyOld, keyNew, (function, context) =>
            {
                var functionTyped = (ModifyResources)function;
                if (functionTyped.resourceChanges != null)
                {
                    foreach (var change in functionTyped.resourceChanges)
                    {
                        if (change != null)
                            FunctionUtility.TryReplaceInString (ref change.key, keyOld, keyNew, context);
                    }
                }
            });

            FunctionUtility.ReplaceInFunction (typeof (ModifyResourcesTarget), keyOld, keyNew, (function, context) =>
            {
                var functionTyped = (ModifyResourcesTarget)function;
                if (functionTyped.resourceChanges != null)
                {
                    foreach (var change in functionTyped.resourceChanges)
                    {
                        if (change != null)
                            FunctionUtility.TryReplaceInString (ref change.key, keyOld, keyNew, context);
                    }
                }
            });
        }

        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.overworldResources, $"{key}__name");
            textDesc = DataManagerText.GetText (TextLibs.overworldResources, $"{key}__desc");
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldResources, $"{key}__name", textName);
            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldResources, $"{key}__desc", textDesc);
        }
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerResource () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
}

