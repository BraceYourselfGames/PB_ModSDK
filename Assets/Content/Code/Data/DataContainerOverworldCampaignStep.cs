using System;
using System.Collections.Generic;
using PhantomBrigade.Functions;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public class CampaignStepBranchPrefs
    {
        public bool modifiersUnique = true;
    }
    
    public class CampaignStepBranch
    {
        [DropdownReference (true)]
        public DataBlockComment comment;

        public CampaignStepBranchPrefs preferences;

        [DropdownReference]
        public List<IOverworldGlobalValidationFunction> checksGlobal;

        [DropdownReference]
        public List<IOverworldEntityValidationFunction> checksBase;

        [InfoBox ("A campaign step must have an outbound link!", InfoMessageType.Error, VisibleIf = "@links == null || links.Count == 0")]
        [ListDrawerSettings (DefaultExpandedState = true, CustomAddFunction = "@new CampaignStepLink ()")]
        public List<CampaignStepLink> links = new List<CampaignStepLink> ();

        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public CampaignStepBranch () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
    
    public class CampaignStepLink
    {
        [DropdownReference (true)]
        public DataBlockComment comment;
        
        [DropdownReference]
        public List<IOverworldGlobalValidationFunction> checksGlobal;

        [DropdownReference]
        public List<IOverworldEntityValidationFunction> checksBase;
        
        [GUIColor ("$GetStepKeyColor")]
        [ValueDropdown("@DataMultiLinkerOverworldCampaignStep.data.Keys")]
        public string stepKey;
        
        [PropertyRange (0, 360)]
        public int direction = 0;
        
        [DropdownReference (true)]
        public DataBlockInt levelOffset;
        
        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerOverworldFactionBranch.data.Keys")]
        public string branchKey;
        
        [DropdownReference (true)]
        public DataBlockOverworldProvinceFilter filter;
        
        [DropdownReference (true), ListDrawerSettings (DefaultExpandedState = true)]
        public List<DataBlockOverworldModifierFilter> modifiers;
        
        // [DropdownReference (true)]
        // public List<DataBlockOverworldProvinceFilter> children;
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public CampaignStepLink () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        private Color GetStepKeyColor ()
        {
            if (string.IsNullOrEmpty (stepKey))
                return Color.HSVToRGB (0f, 0.35f, 1f);
            
            if (DataMultiLinkerOverworldCampaignStep.GetEntry (stepKey, false) == null)
                return Color.HSVToRGB (0f, 0.25f, 1f);

            return Color.HSVToRGB (0.56f, 0.25f, 1f);
        }
        
        #endif
        #endregion
    }
    
    public class DataBlockCampaignEffectTargeted
    {
        [DropdownReference (true)]
        public List<IOverworldEntityValidationFunction> checks;
        
        [DropdownReference]
        public List<IOverworldTargetedFunction> functionsTargeted;
        
        #if !PB_MODSDK
        public bool TryExecution (OverworldEntity targetOverworld)
        {
            if (!Application.isPlaying)
                return false;

            var targetPersistent = IDUtility.GetLinkedPersistentEntity (targetOverworld);
            if (targetOverworld == null || targetOverworld.isDestroyed || targetPersistent == null || targetPersistent.isDestroyed)
                return false;

            if (checks != null)
            {
                bool valid = true;
                foreach (var check in checks)
                {
                    if (check != null && !check.IsValid (targetPersistent))
                    {
                        valid = false;
                        break;
                    }
                }
                
                if (!valid)
                    return false;
            }
            
            if (functionsTargeted != null)
            {
                foreach (var function in functionsTargeted)
                {
                    if (function != null)
                        function.Run (targetOverworld);
                }
            }

            return true;
        }
        #endif
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockCampaignEffectTargeted () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    public class DataBlockCampaignEffect
    {
        [DropdownReference]
        public List<IOverworldFunction> functionsGlobal;

        [DropdownReference]
        public List<IOverworldTargetedFunction> functionsBase;

        public void Run ()
        {
            #if !PB_MODSDK
            if (!Application.isPlaying)
                return;
            
            if (functionsGlobal != null)
            {
                foreach (var function in functionsGlobal)
                {
                    if (function != null)
                        function.Run ();
                }
            }
			        
            if (functionsBase != null)
            {
                var baseOverworld = IDUtility.playerBaseOverworld;
                foreach (var function in functionsBase)
                {
                    if (function != null)
                        function.Run (baseOverworld);
                }
            }
            #endif
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockCampaignEffect () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
    
    public class DataBlockCampaignProgressDisplay
    {
        [ValueDropdown ("@DataMultiLinkerOverworldMemory.data.Keys")]
        public string memoryKey;

        public int target;
    }
    
    public class DataBlockCampaignStepUI
    {       
        [DropdownReference (true)]
        public DataBlockStringNonSerialized textName;
        
        [DropdownReference (true)]
        public DataBlockStringNonSerializedLong textDesc;
        
        [DropdownReference (true)]
        public DataBlockStringNonSerialized textNavHint;
        
        [DropdownReference (true)]
        public DataBlockStringNonSerialized textNavHeader;
        
        [DropdownReference (true)]
        public DataBlockStringNonSerializedLong textNavDesc;
        
        [DropdownReference (true)]
        [DataEditor.SpriteNameAttribute (true, 32f)]
        public string iconNav;
        
        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerOverworldQuest.GetKeys ()")]
        public string textFromQuest;
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockCampaignStepUI () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
    
    [Serializable, HideReferenceObjectPicker, LabelWidth (160f)]
    public class DataContainerOverworldCampaignStep : DataContainerWithText
    {
        [DropdownReference (true)]
        public DataBlockComment comment;

        [DropdownReference (true)] 
        public DataBlockCampaignStepUI ui;

        [DropdownReference (true)]
        public DataBlockOverworldInteractionFilter travelInteraction;
        
        [DropdownReference (true)] 
        public DataBlockCampaignEffect entry = new DataBlockCampaignEffect ();
        
        [DropdownReference (true)] 
        public DataBlockCampaignEffect liberation = new DataBlockCampaignEffect ();
        
        [DropdownReference (true)] 
        public DataBlockCampaignProgressDisplay progressDisplay;

        [DropdownReference]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.Foldout)]
        public SortedDictionary<string, DataBlockCampaignEffect> effects;

        [InfoBox ("A campaign step must have an outbound branch!", InfoMessageType.Error, VisibleIf = "@branches == null || branches.Count == 0")]
        [ListDrawerSettings (DefaultExpandedState = true, CustomAddFunction = "@new CampaignStepBranch ()")]
        public List<CampaignStepBranch> branches = new List<CampaignStepBranch> ();

        [DropdownReference (true)] 
        public HashSet<string> tags;
        
        public override void ResolveText ()
        {
            if (ui != null)
            {
                if (ui.textName != null)
                    ui.textName.s = DataManagerText.GetText (TextLibs.overworldCampaign, $"{key}__header");
                
                if (ui.textDesc != null)
                    ui.textDesc.s = DataManagerText.GetText (TextLibs.overworldCampaign, $"{key}__text");
                
                if (ui.textNavHeader != null)
                    ui.textNavHeader.s = DataManagerText.GetText (TextLibs.overworldCampaign, $"{key}__nav_header");
                
                if (ui.textNavDesc != null)
                    ui.textNavDesc.s = DataManagerText.GetText (TextLibs.overworldCampaign, $"{key}__nav_text");
            }
        }

        public bool TryExecutingEffect (string effectKey)
        {
            if (effects == null || string.IsNullOrEmpty (effectKey) || !effects.TryGetValue (effectKey, out var effect) || effect == null)
            {
                Debug.LogWarning ($"Campaign step {key} has no effect key {effectKey} defined, skipping execution...");
                return false;
            }
            
            Debug.LogWarning ($"Campaign step {key} has effect key {effectKey} defined, executing...");
            effect.Run ();
            return true;
        }

        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerOverworldCampaignStep () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            if (ui != null)
            {
                if (ui.textName != null)
                    DataManagerText.TryAddingTextToLibrary (TextLibs.overworldCampaign, $"{key}__header", ui.textName.s);
                
                if (ui.textDesc != null)
                    DataManagerText.TryAddingTextToLibrary (TextLibs.overworldCampaign, $"{key}__text", ui.textDesc.s);
                
                if (ui.textNavHeader != null)
                    DataManagerText.TryAddingTextToLibrary (TextLibs.overworldCampaign, $"{key}__nav_header", ui.textNavHeader.s);
                
                if (ui.textNavDesc != null)
                    DataManagerText.TryAddingTextToLibrary (TextLibs.overworldCampaign, $"{key}__nav_text", ui.textNavDesc.s);
            }
        }

        #endif
        #endregion
    }
}