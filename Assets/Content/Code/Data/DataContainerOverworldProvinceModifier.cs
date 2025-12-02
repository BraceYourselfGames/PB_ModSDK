using System;
using System.Collections.Generic;
using PhantomBrigade.Functions;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Data
{
    public static class ProvinceModifierKeys
    {
        public const string CampCostIncrease = "gen_camp_cost";
        public const string Escalation = "gen_escalation";
        public const string LevelUp = "gen_level_up";
        public const string CombatHighAlert = "gen_combat_high_alert";
        
        public const string ProgressBreakthrough = "gen_progress_breakthrough";
        public const string ProgressCapital = "gen_progress_capital";
        
        public const string PilotLimitRookie = "gen_pilot_limit_rookie";
        public const string PilotLimitExpert = "gen_pilot_limit_expert";

        public const string SpecialIntro = "sp_tutorial_intro";
        public const string SpecialWeakened = "sp_weakened";
    }

    public class DataBlockUnitCombatEffect
    {
        [DropdownReference]
        public List<IOverworldGlobalValidationFunction> checksGlobal;
        
        [DropdownReference]
        public List<IOverworldEntityValidationFunction> checksBase;
        
        [DropdownReference]
        public List<IPilotValidationFunction> checksPilot;
        
        [DropdownReference]
        public List<ICombatUnitValidationFunction> checksUnit;
        
        [DropdownReference]
        public List<IOverworldFunction> functionsGlobal;
        
        [DropdownReference]
        public List<IOverworldTargetedFunction> functionsBase;
        
        [DropdownReference]
        public List<ICombatFunction> functionsCombat;

        [DropdownReference]
        public List<ICombatFunctionTargeted> functionsUnit;
        
        [DropdownReference]
        public List<IPilotTargetedFunction> functionsPilot;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            if (unitPersistent == null || unitPersistent.isDestroyed)
                return;

            var pilot = IDUtility.GetLinkedPilot (unitPersistent);
            
            if (checksGlobal != null)
            {
                foreach (var check in checksGlobal)
                {
                    bool valid = check.IsValid ();
                    if (!valid)
                        return;
                }
            }
                
            if (checksBase != null)
            {
                var basePersistent = IDUtility.playerBasePersistent;
                foreach (var check in checksBase)
                {
                    bool valid = check.IsValid (basePersistent);
                    if (!valid)
                        return;
                }
            }
            
            if (checksPilot != null)
            {
                if (pilot == null)
                    return;
                
                foreach (var check in checksPilot)
                {
                    bool valid = check.IsValid (pilot, null);
                    if (!valid)
                        return;
                }
            }
            
            if (checksUnit != null)
            {
                foreach (var check in checksUnit)
                {
                    bool valid = check.IsValid (unitPersistent);
                    if (!valid)
                        return;
                }
            }
            
            if (functionsGlobal != null)
            {
                foreach (var function in functionsGlobal)
                {
                    if (function != null)
                        function.Run ();
                }
            }
            
            if (functionsCombat != null)
            {
                foreach (var function in functionsCombat)
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
            
            if (functionsPilot != null && pilot != null)
            {
                foreach (var function in functionsPilot)
                {
                    if (function != null)
                        function.Run (pilot, null);
                }
            }

            if (functionsUnit != null)
            {
                foreach (var function in functionsUnit)
                {
                    if (function != null)
                        function.Run (unitPersistent);
                }
            }
            #endif
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockUnitCombatEffect () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
    
    public class DataBlockUnitGenerationModifier
    {
        [DropdownReference]
        public List<IOverworldGlobalValidationFunction> checksGlobal;
        
        [DropdownReference]
        public List<IOverworldEntityValidationFunction> checksBase;

        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerUnitBlueprint.data.Keys")]
        public string blueprintKey;
        
        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerUnitPreset.data.Keys")]
        public string presetKey;
        
        [DropdownReference (true)]
        public DataFilterUnitPreset presetFilter;

        [DropdownReference]
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.Socket)]
        [DictionaryDrawerSettings (KeyLabel = "Socket")]
        public SortedDictionary<string, DataBlockUnitPartOverride> partOverrides;
        
        public bool IsApplicable (DataContainerUnitPreset preset)
        {
            #if !PB_MODSDK
            if (preset == null)
                return false;
            
            if (checksGlobal != null)
            {
                foreach (var check in checksGlobal)
                {
                    bool valid = check.IsValid ();
                    if (!valid)
                        return false;
                }
            }
                
            if (checksBase != null)
            {
                var basePersistent = IDUtility.playerBasePersistent;
                foreach (var check in checksBase)
                {
                    bool valid = check.IsValid (basePersistent);
                    if (!valid)
                        return false;
                }
            }

            if (!string.IsNullOrEmpty (presetKey) && !string.Equals (presetKey, preset.key))
                return false;
            
            if (!string.IsNullOrEmpty (blueprintKey) && !string.Equals (blueprintKey, preset.blueprintProcessed))
                return false;

            if (presetFilter != null)
            {
                if (!presetFilter.IsCandidateValid (preset))
                    return false;
            }
            #endif
            
            return true;
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockUnitGenerationModifier () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
    
    public class DataBlockPilotOverworldEffect
    {
        [DropdownReference]
        public List<IOverworldGlobalValidationFunction> checksGlobal;
        
        [DropdownReference]
        public List<IOverworldEntityValidationFunction> checksBase;
        
        [DropdownReference]
        public List<IPilotValidationFunction> checksPilot;

        [DropdownReference]
        public List<IOverworldFunction> functionsGlobal;
        
        [DropdownReference]
        public List<IOverworldTargetedFunction> functionsBase;

        [DropdownReference]
        public List<IPilotTargetedFunction> functionsPilot;

        public void Run (PersistentEntity pilot)
        {
            #if !PB_MODSDK
            if (pilot == null || pilot.isDestroyed)
                return;

            if (checksGlobal != null)
            {
                foreach (var check in checksGlobal)
                {
                    bool valid = check.IsValid ();
                    if (!valid)
                        return;
                }
            }
                
            if (checksBase != null)
            {
                var basePersistent = IDUtility.playerBasePersistent;
                foreach (var check in checksBase)
                {
                    bool valid = check.IsValid (basePersistent);
                    if (!valid)
                        return;
                }
            }
            
            if (checksPilot != null)
            {
                foreach (var check in checksPilot)
                {
                    bool valid = check.IsValid (pilot, null);
                    if (!valid)
                        return;
                }
            }

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
            
            if (functionsPilot != null && pilot != null)
            {
                foreach (var function in functionsPilot)
                {
                    if (function != null)
                        function.Run (pilot, null);
                }
            }
            #endif
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockPilotOverworldEffect () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }


    [Serializable][HideReferenceObjectPicker][LabelWidth (200f)]
    public class DataContainerOverworldProvinceModifier : DataContainerWithText, IDataContainerTagged
    {
        public bool hidden = false;
        public bool positive = false;

        public bool trackVisitUniqueness = true;
        public bool trackBranchUniqueness = true;
        
        public int priority = 0;
        public bool tinted = false;
        
        [PropertyRange (0, 360), HorizontalGroup]
        public int hue = 0;

        [YamlIgnore, ShowInInspector, HorizontalGroup (32f), HideLabel]
        private Color color => Color.HSVToRGB (Mathf.Clamp01 (hue / 360f), tinted ? 0.75f : 0.25f, tinted ? 1f : 0.5f);

        [DataEditor.SpriteNameAttribute (false, 180f)]
        public string icon;
        
        public HashSet<string> tags = new HashSet<string> ();
        
        [LabelText ("Name / Desc.")]
        [YamlIgnore]
        public string textName;
        
        [TextArea][HideLabel]
        [YamlIgnore]
        public string textDesc;
        
        [DropdownReference (true)]
        public DataBlockInt levelOffset;

        [DropdownReference (true)]
        public DataBlockFloat pilotExperienceMultiplier;
        
        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerPilotPersistentProfile.data.Keys")]
        public string pilotPersistentInjected;

        [DropdownReference (true)]
        public SortedDictionary<string, QuestLiberationSpawnVariants> liberationSpawnVariants;
        
        [DropdownReference (true)]
        public List<DataBlockProvinceScenarioChange> scenarioChanges;
        
        [DropdownReference (true)]
        public DataBlockCampaignEffect effectsOnEntry;
        
        [DropdownReference (true)]
        public DataBlockCampaignEffect effectsOnCompletion;
        
        [DropdownReference]
        public List<DataBlockCampaignEffectTargeted> effectsOnSpawn;
        
        [DropdownReference]
        public List<DataBlockUnitCombatEffect> effectsOnUnitSpawn;
        
        [DropdownReference]
        public List<DataBlockUnitCombatEffect> effectsOnUnitArrival;
        
        [DropdownReference]
        public List<DataBlockUnitCombatEffect> effectsOnUnitDefeat;
        
        [DropdownReference]
        public List<DataBlockUnitGenerationModifier> unitGenModifiers;
        
        // [DropdownReference, PropertyOrder (5)]
        // [LabelText ("Effects on events")]
        // public List<DataBlockOverworldQuestEffectGroupContextual> effectsOnEvents;

        public bool IsHidden () => hidden;
        
        public HashSet<string> GetTags (bool processed) => 
            tags;
        
        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.overworldModifiers, $"{key}_name");
            textDesc = DataManagerText.GetText (TextLibs.overworldModifiers, $"{key}_desc");
        }
        
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;

        public DataContainerOverworldProvinceModifier () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldModifiers, $"{key}_name", textName);
            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldModifiers, $"{key}_desc", textDesc);
        }
        
        private Color GetSectionColor (int section)
        {
            return Color.HSVToRGB ((float)section / 10f + 0.1f, 0.15f, 1f);
        }

        #endif
    }
}
