using System;
using System.Collections.Generic;
using PhantomBrigade.Functions;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [Serializable, HideReferenceObjectPicker, LabelWidth (160f)]
    public class DataContainerOverworldCampaignStart : DataContainerWithText
    {
        public bool hidden = false;
        public Color color = Color.white.WithAlpha (1f);
        
        [LabelText ("Name"), YamlIgnore]
        public string textName;
        
        [LabelText ("Sub / Desc."), YamlIgnore]
        public string textSub;

        [HideLabel, TextArea (1, 10), YamlIgnore]
        public string textDesc;

        [ValueDropdown("@DataMultiLinkerOverworldCampaignStep.data.Keys")]
        public string stepKey;
        
        [ValueDropdown ("@DataMultiLinkerOverworldProvinceBlueprints.data.Keys")]
        public string provinceKey;
        
        public Vector3 position;
        
        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerOverworldFactionBranch.data.Keys")]
        public string branchKey;

        [DropdownReference]
        public List<IOverworldFunction> functionsGlobal;
        
        [DropdownReference]
        public List<IOverworldTargetedFunction> functionsBase;
        
        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.overworldStart, $"{key}__header");
            textSub = DataManagerText.GetText (TextLibs.overworldStart, $"{key}__sub");
            textDesc = DataManagerText.GetText (TextLibs.overworldStart, $"{key}__text");
        }

        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerOverworldCampaignStart () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            if (!string.IsNullOrEmpty (textName))
                DataManagerText.TryAddingTextToLibrary (TextLibs.overworldStart, $"{key}__header", textName);
            
            if (!string.IsNullOrEmpty (textSub))
                DataManagerText.TryAddingTextToLibrary (TextLibs.overworldStart, $"{key}__sub", textSub);
            
            if (!string.IsNullOrEmpty (textDesc))
                DataManagerText.TryAddingTextToLibrary (TextLibs.overworldStart, $"{key}__text", textDesc);
        }

        #endif
        #endregion
    }
}