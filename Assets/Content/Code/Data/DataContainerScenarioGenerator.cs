using System;
using System.Collections.Generic;
using PhantomBrigade.Functions;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public class DataBlockScenarioGeneratorFilter : DataBlockFilterLinked<DataContainerScenarioGenerator>
    {
        public override IEnumerable<string> GetTags () => DataMultiLinkerScenarioGenerator.GetTags ();
        public override SortedDictionary<string, DataContainerScenarioGenerator> GetData () => DataMultiLinkerScenarioGenerator.data;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockScenarioGeneratorRewards
    {
        [DropdownReference, HideLabel] 
        public DataBlockComment comment;
        
        public List<DataBlockOverworldPointReward> rewards = new List<DataBlockOverworldPointReward> ();
        
        public void Refresh ()
        {
            if (rewards != null)
            {
                foreach (var reward in rewards)
                {
                    if (reward != null)
                        reward.Refresh ();
                }
            }
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioGeneratorRewards () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockScenarioGeneratorSquad
    {
        [DropdownReference, HideLabel] 
        public DataBlockComment comment;
        
        [ValueDropdown ("@DataMultiLinkerScenario.GetTags ()")]
        [DropdownReference, LabelText ("Sc. tags required")]
        public HashSet<string> scenarioTagsRequired;
        
        [ValueDropdown ("@DataMultiLinkerScenario.GetTags ()")]
        [DropdownReference, LabelText ("Sc. tags blocked")]
        public HashSet<string> scenarioTagsBlocked;
        
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, CustomAddFunction = "@new DataBlockScenarioGeneratorSquadSlot ()")]
        public List<DataBlockScenarioGeneratorSquadSlot> slots = new List<DataBlockScenarioGeneratorSquadSlot> ();
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioGeneratorSquad () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockScenarioGeneratorSquadSlot
    {
        [GUIColor ("GetQualityTableColor"), LabelText ("Quality")]
        [ValueDropdown ("GetQualityTableKeys")]
        public string qualityTable;

        [ValueDropdown ("@DataMultiLinkerUnitLiveryPreset.data.Keys")]
        public string livery;

        public int levelOffsetMin = 0;
        public int levelOffsetMax = 0;
        
        [InlineProperty, HideLabel]
        public DataFilterUnitPreset presetFilter = new DataFilterUnitPreset ();
        
        #region Editor
        #if UNITY_EDITOR
        
        private IEnumerable<string> GetQualityTableKeys => DataMultiLinkerQualityTable.data.Keys;
        private Color GetQualityTableColor => !string.IsNullOrEmpty (qualityTable) && DataMultiLinkerQualityTable.data.TryGetValue (qualityTable, out var v) ? v.uiColor : Color.white;

        #endif
        #endregion
    }

    [HideReferenceObjectPicker]
    public class DataBlockScenarioGeneratorCore
    {
        public int priority = 0;
        
        [ValueDropdown ("@FieldReflectionUtility.GetConstantStringFieldValues (typeof (StandaloneScenarioType), false)")]
        public string type = StandaloneScenarioType.remote;

        [ValueDropdown ("@DataMultiLinkerScenarioStandaloneGroup.GetKeys ()")]
        public string group;
    }
    
    public class DataBlockScenarioGeneratorParent : DataContainerParent
    {
        protected override IEnumerable<string> GetKeys () => DataMultiLinkerScenarioGenerator.data.Keys;
    }
    
    public class DataContainerScenarioGenerator : DataContainerWithText, IDataContainerTagged
    {
        [ToggleLeft]
        public bool hidden;
        
        [ShowIf ("IsCoreVisible")]
        [DropdownReference]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, CustomAddFunction = "@new DataBlockScenarioGeneratorParent ()")]
        public List<DataBlockScenarioGeneratorParent> parents;
        
        [ShowIf ("IsCoreVisible")]
        [YamlIgnore, ReadOnly]
        [ShowIf ("@children != null && children.Count > 0")]
        [ListDrawerSettings (DefaultExpandedState = false)]
        public List<string> children;
        
        
        [DropdownReference (true)]
        public DataBlockScenarioGeneratorCore core;
        
        [ShowIf ("@IsCoreVisible && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockScenarioGeneratorCore coreProc;
        
        
        [DropdownReference]
        public HashSet<string> tags;
        
        [ShowIf ("@IsCoreVisible && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public HashSet<string> tagsProc;
        

        [DropdownReference (true)]
        public DataBlockTextTrimodal textList;
        
        [ShowIf ("$IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockTextTrimodal textListProc;
        
        
        [DropdownReference (true)]
        public DataBlockInt escalation;
        
        [ShowIf ("$IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockInt escalationProc;
        
        
        [DropdownReference (true)]
        public DataBlockInt levelOffset;
        
        [ShowIf ("$IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockInt levelOffsetProc;
        
        
        [DropdownReference]
        public List<IOverworldGlobalValidationFunction> checksGlobal;
        
        [ShowIf ("$IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public List<IOverworldGlobalValidationFunction> checksGlobalProc;
        

        [DropdownReference]
        public List<IOverworldEntityValidationFunction> checksBase;
        
        [ShowIf ("$IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public List<IOverworldEntityValidationFunction> checksBaseProc;
        

        [DropdownReference (true)]
        public DataBlockOverworldEntityScenarios scenarioFilter;
        
        [ShowIf ("$IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldEntityScenarios scenarioFilterProc;
        
        
        [DropdownReference (true)]
        public List<DataBlockProvinceScenarioChange> scenarioChanges;
        
        [ShowIf ("$IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public List<DataBlockProvinceScenarioChange> scenarioChangesProc;
        
        
        [DropdownReference (true)]
        public List<DataBlockScenarioGeneratorSquad> squads;
        
        [ShowIf ("$IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public List<DataBlockScenarioGeneratorSquad> squadsProc;
        
        
        [DropdownReference (true)]
        public DataBlockScenarioGeneratorFilter squadFilter;
        
        [ShowIf ("$IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockScenarioGeneratorFilter squadFilterProc;
        
        
        [DropdownReference (true)]
        public List<DataBlockScenarioGeneratorRewards> rewardPool;
        
        [ShowIf ("$IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public List<DataBlockScenarioGeneratorRewards> rewardPoolProc;
        
        
        [DropdownReference (true)]
        public List<DataBlockCampaignEffect> effectsOnCompletion;
        
        [ShowIf ("$IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public List<DataBlockCampaignEffect> effectsOnCompletionProc;
        
        public HashSet<string> GetTags (bool processed) =>
            processed ? tagsProc : tags;
        
        public bool IsHidden () => hidden;
        
        public override void ResolveText ()
        {
            if (textList != null)
                textList.ResolveText (TextLibs.scenarioGenerators, $"{key}_list");
        }

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);

            if (rewardPool != null)
            {
                foreach (var reward in rewardPool)
                {
                    if (reward != null)
                        reward.Refresh ();
                }
            }
        }

        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerScenarioGenerator () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        private bool IsCoreVisible => DataMultiLinkerScenarioGenerator.Presentation.showCore;
        private bool IsInheritanceVisible => DataMultiLinkerScenarioGenerator.Presentation.showInheritance;
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            if (textList != null)
                textList.SaveText (TextLibs.scenarioGenerators, $"{key}_list");
        }

        #endif
        #endregion
    }
}

