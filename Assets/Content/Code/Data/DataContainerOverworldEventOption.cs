using System.Collections.Generic;
using PhantomBrigade.Functions;
using PhantomBrigade.Data.UI;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public enum OutcomeFallbackMode
    {
        None,
        Exit,
        CurrentStep,
        SpecificStep
    }
    
    public class DataContainerOverworldEventOption : DataContainerWithText
    {
        // Deprecated, remove as soon as events can be overwritten, not removed for now to avoid deserialization warnings
        [HideInInspector]
        public int priority;
        
        [HideInInspector]
        public bool colorFromStep = false;
        
        [HideInInspector]
        public bool colorCustom = false;

        [ValueDropdown ("GetColorKeys")]
        [ShowIf ("IsTabWriting")]
        public string colorKey = DataKeysEventColor.Neutral;
        
        [HideInInspector]
        [ColorUsage (showAlpha: false)]
        public Color color = Color.white;

        [ShowIf ("IsTabWriting")]
        //This determines what stinger will play when the player chooses an option
        public EventMusicMoods optionMood = EventMusicMoods.Neutral;
        
        [LabelText ("Header / Content")]
        [YamlIgnore]
        public string textHeader ="PLACEHOLDER";
        
        [HideLabel]
        [YamlIgnore]
        public string textContent ="PLACEHOLDER";
        
        [DropdownReference]
        [ShowIf ("AreTextVariantsVisible")]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataBlockEventTextVariant> textVariants;
        
        [YamlIgnore]
        [ShowIf ("AreTextVariantsGeneratedVisible")]
        [DictionaryDrawerSettings (IsReadOnly = true, DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataBlockEventTextVariant> textVariantsGenerated;

        [HideIf ("IsTabWriting")]
        public bool completing;
        
        [ShowIf ("@!IsTabWriting && check != null")]
        [LabelText ("Hide If Check Fails")]
        public bool checkPreventsUnlock = true;
        
        [ShowIf ("@!IsTabWriting")]
        [LabelText ("Hide If Resources Low")]
        public bool resourceChangePreventsUnlock = true;
        
        [ShowIf ("@!IsTabWriting && AreStepsVisible")][HideInInspector]
        public bool stepByPriority;
        
        [ShowIf ("@!IsTabWriting && AreStepsVisible")][HideInInspector]
        public OutcomeFallbackMode stepFallbackMode = OutcomeFallbackMode.None;

        [ShowIf ("@!IsTabWriting && AreStepsVisible && stepFallbackMode == OutcomeFallbackMode.SpecificStep")]
        [ValueDropdown("GetStepKeys")]
        public string stepFallbackKey;
        
        [HideIf ("IsTabWriting")]
        [DropdownReference (true)]
        public DataBlockOverworldEventOptionInjection injection;

        [HideIf ("IsTabWriting")]
        [DropdownReference (true)]
        public DataBlockOverworldEventCheck check = new DataBlockOverworldEventCheck();
        
        [HideIf ("IsTabWriting")]
        [DropdownReference (true)]
        public DataBlockOverworldEventHopeChange hopeChange;
        
        [HideIf ("IsTabWriting")]
        [DropdownReference (true)]
        public DataBlockOverworldEventWarScoreChange warScoreChange;
        
        [HideIf ("IsTabWriting")]
        [DropdownReference (true)]
        public DataBlockOverworldEventCombat combat;

        /*
        [HideIf ("IsTabWriting")]
        [DropdownReference]
        [ListDrawerSettings (CustomAddFunction = "@new DataBlockResourceChange ()")]
        public List<DataBlockResourceChange> resourceChanges;
        */
        
        [HideIf ("IsTabWriting")]
        [DropdownReference]
        [ListDrawerSettings (CustomAddFunction = "@new DataBlockMemoryChangeGroupEvent ()")]
        public List<DataBlockMemoryChangeGroupEvent> memoryChanges = new List<DataBlockMemoryChangeGroupEvent> { new DataBlockMemoryChangeGroupEvent () };
        /*
        [HideIf ("IsTabWriting")]
        [DropdownReference]
        [ListDrawerSettings (CustomAddFunction = "@new DataBlockOverworldActionInstanceData ()")]
        public List<DataBlockOverworldActionInstanceData> actionsCreated;
        */
        
        // [HideIf ("IsTabWriting")]
        // [DropdownReference]
        // [ListDrawerSettings (CustomAddFunction = "@new DataBlockOverworldEventCall ()")]
        // public List<DataBlockOverworldEventCall> calls;
        
        [HideIf ("IsTabWriting")]
        [DropdownReference]
        public List<IOverworldEventFunction> functions;

        [HideIf ("IsTabWriting")]
        [DropdownReference]
        [ValueDropdown("GetStepKeys")]
        public HashSet<string> steps;
        
        [HideInInspector, YamlIgnore]
        public DataContainerOverworldEvent parent;
        
        

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
            
            if (functions != null)
                RefreshParentsInFunctions (functions, parent, true);
        }

        public void RefreshParents (string key, DataContainerOverworldEvent parentEvent, bool onAfterDeserialization)
        {
            parent = parentEvent;
            
            if (onAfterDeserialization)
                base.OnAfterDeserialization (key);

            check?.RefreshParents (parentOption: this);

            if (functions != null)
                RefreshParentsInFunctions (functions, parent, onAfterDeserialization);
                    
            if (memoryChanges != null)
            {
                foreach (var changeGroup in memoryChanges)
                {
                    changeGroup.parentEvent = parentEvent;
                }
            }
        }
        
        private void RefreshParentsInFunctions (List<IOverworldEventFunction> functionsRefreshed, DataContainerOverworldEvent parentEvent, bool onAfterDeserialization)
        {
            if (functionsRefreshed == null)
                return;

            foreach (var function in functionsRefreshed)
            {
                if (function is IOverworldEventParent functionParented)
                    functionParented.ParentEvent = parentEvent;
            }
        }
        
        public override void ResolveText ()
        {
            if (parent != null)
                return;
            
            // Ensure double underscore separator follows the event name
            textHeader = DataManagerText.GetText (TextLibs.overworldEvents, $"os_{key}__header");
            textContent = DataManagerText.GetText (TextLibs.overworldEvents, $"os_{key}__text");
        }

        #region Editor
        #if UNITY_EDITOR
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            // Ensure double underscore separator follows the event name
            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldEvents, $"os_{key}__header", textHeader);
            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldEvents, $"os_{key}__text", textContent);
        }
        
        private bool IsTabWriting => parent != null && parent.IsTabWriting;
        private bool AreTextVariantsVisible => IsTabWriting && DataMultiLinkerOverworldEvent.Presentation.showTextVariants;
        private bool AreTextVariantsGeneratedVisible => IsTabWriting && DataMultiLinkerOverworldEvent.Presentation.showTextVariantsGenerated && textVariantsGenerated != null && textVariantsGenerated.Count > 0;
        
        [ShowInInspector]
        [HideIf ("IsTabWriting")]
        private DataEditor.DropdownReferenceHelper helper;

        public DataContainerOverworldEventOption () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        private bool AreStepsVisible => 
            parent != null;
        
        private IEnumerable<string> GetStepKeys () => 
            parent != null && parent.steps != null ? parent.steps.Keys : null;

        private IEnumerable<string> GetColorKeys () => 
            DataMultiLinkerUIColor.data.Keys;
        
        #endif
        #endregion
    }
}