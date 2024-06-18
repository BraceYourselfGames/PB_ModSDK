using System;
using System.Collections.Generic;
using PhantomBrigade.Functions;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using YamlDotNet.Serialization;

#if UNITY_EDITOR
#endif

namespace PhantomBrigade.Data
{
    [Serializable]
    public class DataBlockOverworldEntityCore
    {
        public bool inventory = true;
        public bool selectable = true;
        public bool capturable = false;
        public bool recapturable = true;
        public bool interactable = true;
        
        [ShowIf ("interactable")]
        public float interactionRange = 5f;

        public bool destroyOnDefeat = false;

        public bool intelLocked = false;
        public bool detectable = true;
        public bool recognizable = true;
        public bool observable = true;
        public bool permanent = false;
        public bool generated = true;
        
        public bool refreshEventsOnExit = true;
        public bool refreshEventsOnEntry = true;

        [LabelText ("Faction from province")]
        public bool factionChangesWithProvince = false;
        
        [ValueDropdown ("@DataHelperUnitEquipment.GetFactions ()")]
        public string faction = Factions.enemy;
    }

    [Serializable]
    public class DataBlockOverworldEntitySpawning
    {
        public int patrolLimit;
        //What is the re-enforcement delay for this site?
        public float reenforcementDelay;
    }

    [Serializable]
    public class DataBlockOverworldEntityReinforcements
    {
        public bool destroyOnDefeat = true;
        
        [PropertyRange (0f, 1f)]
        public float threatMultiplier = 0.5f;

        [ValueDropdown("@DataMultiLinkerCombatReinforcement.data.Keys")]
        public HashSet<string> reinforcementKeys = new HashSet<string> ();
    }

    [Serializable]
    public class DataBlockOverworldEntityRewards
    {
        [ListDrawerSettings (DefaultExpandedState = false, ShowPaging = false, AlwaysAddDefaultValue = true)]
        public List<DataBlockOverworldEntityRewardsTrigger> triggersAfterCombat;
        
        [DictionaryKeyDropdown ("@DataShortcuts.sim.scenarioRewardKeys")]
        public SortedDictionary<string, List<DataBlockOverworldEntityRewardBlock>> blocks;
        
        private IEnumerable<string> GetLocalKeys => blocks != null ? blocks.Keys : null;
    }
    
    [Serializable]
    public class DataBlockOverworldInteractionEffects
    {
        public HashSet<string> effectsOnEntry;
        public HashSet<string> effectsOverTime;
    }

    [Serializable]
    public class DataBlockOverworldEntityEscalation
    {
        public float escalationGain = 0f;
        public float escalationGainWarMultiplier = 1f;
        public float warScoreDealt = 0f;
        public float warScoreRestored = 0f;
        public bool warObjectiveCandidate = true;
    }

    [Serializable]
    public class DataBlockOverworldEntityThreatLevel
    {
        public float baseThreatLevel;
    }

    [Serializable]
    public class DataBlockOverworldEntityBattleSite
    {
        public float timeLimit;
        
        public float warScoreLostMin;
        
        [FormerlySerializedAs("warScoreLost")]
        public float warScoreLostMax;
    }

    [Serializable]
    public class DataBlockOverworldEntityRewardsTrigger
    {
        [ToggleLeft]
        public bool outcomeVictory;
        
        [ToggleLeft]
        public bool outcomeDefeat;
        
        [ToggleLeft]
        public bool caseEarly;
        
        [ToggleLeft]
        public bool caseTotal;
        
        [DictionaryKeyDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataBlockOverworldEntityRewards\", \"GetLocalKeys\")")]
        public Dictionary<string, int> rewards = new Dictionary<string, int> ();
    }

    [Serializable]
    public class DataBlockOverworldEntityRewardBlock
    {
        [DropdownReference, DictionaryKeyDropdown ("@DataMultiLinkerResource.data.Keys")]
        public SortedDictionary<string, DataBlockVirtualResource> resources;
        
        [DropdownReference, ListDrawerSettings (DefaultExpandedState = true, CustomAddFunction = "@new DataBlockVirtualWorkshopProject ()")]
        public List<DataBlockVirtualWorkshopProject> projects;
        
        [DropdownReference, ListDrawerSettings (DefaultExpandedState = true, CustomAddFunction = "@new DataBlockVirtualParts ()")]
        public List<DataBlockVirtualParts> parts;
        
        [DropdownReference, ListDrawerSettings (DefaultExpandedState = true, CustomAddFunction = "@new DataBlockVirtualSubsystems ()")]
        public List<DataBlockVirtualSubsystems> subsystems;
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockOverworldEntityRewardBlock () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    [Serializable]
    public class DataBlockOverworldEntityVisual
    {
        public string colorKeyOverrideVision = null;
        public List<DataBlockViewModelAssetPath> visualPrefabs;
    }

    [Serializable]
    public class DataBlockOverworldEntityMovement
    {
        public float speed;
        public bool rotateToFacing;
    }

    [Serializable]
    public class DataBlockOverworldEntityRanges
    {
	    public float vision;
        public DataBlockOverworldEntityObserver observerData;
    }
    
    [Serializable]
    public class DataBlockOverworldEntityObserver
    {
        public bool contactUsed = true;
        public float contactRange = 2.5f;

        public List<string> contactChecks = new List<string> ();

        public List<string> contactEffects = new List<string> ();
    }

    [Serializable]
    public class DataBlockOverworldEntityDetection
    {
        public float pingInterval;
        public float detectionIncrementMin;
        public float detectionIncrementMax;
        public float detectionDecayTime;
    }
    
    [Serializable]
    public class DataBlockOverworldEntityIntel
    {
        public bool detected;
        public bool recognized;
        public bool inVisibleRange;
    }
    
    [Serializable]
    public class DataBlockOverworldEntityAI
    {
        public bool isAggressive = true;
        public bool activeWhenUntracked = true;
    }
    
    [Serializable]
    public class DataBlockOverworldEntityBattery
    {
        public float maxCharge;
        public float rechargeRate;
        public float rechargeDelay;
    }
    
    [Serializable]
    public class DataBlockOverworldEntityRepairJuice
    {
        public float maxCharge;
        public float repairRate;
    }

    public class DataBlockScenarioChangeCompositeSpawn
    {
        public string instanceNameOverride;
        
        [ValueDropdown ("@DataMultiLinkerUnitComposite.data.Keys")]
        public string blueprintKey;
    }

    [Serializable]
    public class DataBlockOverworldEntityScenarioChanges
    {
        [DropdownReference]
        public DataBlockScenarioChangeCompositeSpawn compositeOnStart;
        
        [DropdownReference]
        public List<ICombatFunction> functionsOnStart;
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldEntityScenarioChanges () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    [Serializable]
    public class DataBlockOverworldEntityScenarios
    {
        [PropertyTooltip ("Optional scenario constraints. These usually shouldn't include mission type and should typically just specify location type etc.")]
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.ScenarioTag)]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> tags = new SortedDictionary<string, bool> ();
    }
    
    [Serializable]
    public class DataBlockOverworldEntityAreas
    {
        [PropertyTooltip ("Optional area constraints. These usually shouldn't include mission type and should typically just specify location type etc.")]
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.AreaTag)]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> tags = new SortedDictionary<string, bool> ();
    }

    [Serializable]
    public class DataBlockOverworldEntityProduction
    {
        public int limit;
        public float duration;

        [ValueDropdown ("@DataHelperUnitEquipment.GetFactions ()")]
        public HashSet<string> controllingFactions = new HashSet<string> ();
        public List<string> eventsOnCompletion;
    }
    
    [Serializable]
    public class DataBlockOverworldEntityFactionBranch
    {
        [ValueDropdown ("@DataMultiLinkerOverworldFactionBranch.data.Keys")]
        public string factionBranch;
    }
    
    [Serializable]
    public class DataBlockOverworldEntityUnits
    {

    }
    
    [HideReferenceObjectPicker]
    public class DataBlockStringNonSerialized
    {
        [YamlIgnore]
        [HideLabel]
        public string s;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockStringNonSerializedLong
    {
        [YamlIgnore]
        [HideLabel, TextArea (1, 10)]
        public string s;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockOverworldBlueprintParent
    {
        [GUIColor ("GetKeyColor")]
        [ValueDropdown ("@DataMultiLinkerOverworldEntityBlueprint.data.Keys")]
        [SuffixLabel ("@hierarchy"), HideLabel]
        public string key;
        
        [YamlIgnore, ReadOnly, HideInInspector]
        public string hierarchy;


        #region Editor
        #if UNITY_EDITOR

        private static Color colorError = Color.Lerp (Color.red, Color.white, 0.5f);
        private static Color colorNormal = Color.white;
        
        private Color GetKeyColor ()
        {
            if (string.IsNullOrEmpty (key))
                return colorError;

            bool present = DataMultiLinkerOverworldEntityBlueprint.data.ContainsKey (key);
            return present ? colorNormal : colorError;
        }
        
        #endif
        #endregion
    }

    public static class ScenarioBlockTags
    {
        public const string Start = "start";
        public const string Reinforcement1 = "reinforcement1";
        public const string Reinforcement2 = "reinforcement2";
        public const string Reinforcement3 = "reinforcement3";
    }

    public class DataBlockScenarioUnits
    {
        public bool step = true;
        
        [ValueDropdown ("@FieldReflectionUtility.GetConstantStringFieldValues (typeof (ScenarioBlockTags), false)")]
        public HashSet<string> tags = new HashSet<string> ();

        [ListDrawerSettings (AlwaysAddDefaultValue = true, DefaultExpandedState = false)]
        public List<DataBlockScenarioUnitGroup> unitGroups = new List<DataBlockScenarioUnitGroup> ();
    }
    
    [Serializable]
    public class DataContainerOverworldEntityBlueprint : DataContainerWithText, IDataContainerTagged
    {
        private const string refreshMethod = "OnFullRefreshRequired";
        
        [ToggleLeft]
        public bool hidden = false;
        
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false)]
        [OnValueChanged (refreshMethod, true)]
        public List<DataBlockOverworldBlueprintParent> parents = new List<DataBlockOverworldBlueprintParent> ();
        
        [YamlIgnore, LabelText ("Children"), ReadOnly]
        public List<string> children = new List<string> ();

        
        [ShowIf ("AreTagsVisible")]
        [ValueDropdown("@DataMultiLinkerOverworldEntityBlueprint.tags")]
        [OnValueChanged (refreshMethod, true)]
        public HashSet<string> tags = new HashSet<string> ();
        
        [ShowIf ("@AreTagsVisible && IsInheritanceVisible")]
        [ReadOnly, YamlIgnore]
        public HashSet<string> tagsProcessed = new HashSet<string> ();

        
        [Space (8f)]
        [DropdownReference (true)]
        [ShowIf ("IsUIVisible")]
        [LabelText ("Name")]
        [OnValueChanged (refreshMethod, true)]
        public DataBlockStringNonSerialized textName;

        [ShowIf ("@IsUIVisible && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        [HideLabel]
        public DataBlockStringNonSerialized textNameProcessed;
        
        
        [DropdownReference (true)]
        [ShowIf ("IsUIVisible")]
        [LabelText ("Desc.")]
        [OnValueChanged (refreshMethod, true)]
        public DataBlockStringNonSerializedLong textDesc;

        [ShowIf ("@IsUIVisible && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        [HideLabel]
        public DataBlockStringNonSerializedLong textDescProcessed;


        [DropdownReference (true)]
        [ShowIf ("IsUIVisible")]
        [OnValueChanged (refreshMethod, true)]
        public string textIdentifierGroup;
        
        [ShowIf ("@IsUIVisible && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public string textIdentifierGroupProcessed;
        
        
        [DropdownReference (true)]
        [ShowIf ("IsUIVisible")]
        [ValueDropdown ("@TextureManager.GetExposedTextureKeys (TextureGroupKeys.OverworldEntities)")]
        [OnInspectorGUI ("@DropdownUtils.DrawTexturePreview ($value, TextureGroupKeys.OverworldEntities, 128)", false)]
        [OnValueChanged (refreshMethod, true)]
        public string image;
        
        [ShowIf ("@IsUIVisible && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public string imageProcessed;
        
        
        [DropdownReference (true)]
        [ShowIf ("IsUIVisible")]
        [DataEditor.SpriteNameAttribute (true, 40f)]
        [OnValueChanged (refreshMethod, true)]
        public string icon;
        
        [ShowIf ("@IsUIVisible && IsInheritanceVisible")]
        [DataEditor.SpriteNameAttribute (true, 40f)]
        [YamlIgnore, ReadOnly]
        public string iconProcessed;
        
        
        [Space (8f)]
        [DropdownReference (true)]
        [LabelText ("Core", SdfIconType.Grid3x2Gap)]
        [ShowIf ("IsCoreVisible")]
        [OnValueChanged (refreshMethod, true)]
        public DataBlockOverworldEntityCore core;
        
        [ShowIf ("@IsCoreVisible && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldEntityCore coreProcessed;
        
        
        [Space (8f)]
        [DropdownReference (true)]
        [GUIColor ("@GetSectionColor (0)")]
        [LabelText ("Faction Branch", SdfIconType.Flag)]
        [ShowIf ("IsCoreVisible")]
        [OnValueChanged (refreshMethod, true)]
        public DataBlockOverworldEntityFactionBranch factionBranch;
        
        [ShowIf ("@IsCoreVisible && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldEntityFactionBranch factionBranchProcessed;
        
        
        [DropdownReference (true)]
        [GUIColor ("@GetSectionColor (1)")]
        [LabelText ("Visual", SdfIconType.GeoAlt)]
        [ShowIf ("AreVisualsVisible")]
        [OnValueChanged (refreshMethod, true)]
        public DataBlockOverworldEntityVisual visual;
        
        [ShowIf ("@AreVisualsVisible && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldEntityVisual visualProcessed;
        
        
        [DropdownReference (true)]
        [GUIColor ("@GetSectionColor (2)")]
        [LabelText ("Movement", SdfIconType.ArrowsMove)]
        [ShowIf ("IsMovementVisible")]
        [OnValueChanged (refreshMethod, true)]
        public DataBlockOverworldEntityMovement movement;
        
        [ShowIf ("@IsMovementVisible && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldEntityMovement movementProcessed;
        
        
        [DropdownReference (true)]
        [GUIColor ("@GetSectionColor (3)")]
        [LabelText ("Ranges", SdfIconType.RecordCircle)]
        [ShowIf ("AreRangesVisible")]
        [OnValueChanged (refreshMethod, true)]
        public DataBlockOverworldEntityRanges ranges;
        
        [ShowIf ("@AreRangesVisible && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldEntityRanges rangesProcessed;
        
        
        [DropdownReference (true)]
        [GUIColor ("@GetSectionColor (4)")]
        [LabelText ("Detection", SdfIconType.Eye)]
        [ShowIf ("AreRangesVisible")]
        [OnValueChanged (refreshMethod, true)]
        public DataBlockOverworldEntityDetection detection;
        
        [ShowIf ("@AreRangesVisible && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldEntityDetection detectionProcessed;       


        [DropdownReference (true)]
        [GUIColor ("@GetSectionColor (5)")]
        [LabelText ("Initial Intel", SdfIconType.ClipboardData)]
        [ShowIf ("IsIntelVisible")]
        [OnValueChanged (refreshMethod, true)]
        public DataBlockOverworldEntityIntel intel;
        
        [ShowIf ("@IsIntelVisible && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldEntityIntel intelProcessed;
        
        
        [DropdownReference (true)]
        [GUIColor ("@GetSectionColor (6)")]
        [LabelText ("Combat Capability", SdfIconType.People)]
        [ShowIf ("AreUnitsVisible")]
        [OnValueChanged (refreshMethod, true)]
        public DataBlockOverworldEntityUnits units;
        
        [ShowIf ("@AreUnitsVisible && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldEntityUnits unitsProcessed;
        
        
        [DropdownReference (true)]
        [GUIColor ("@GetSectionColor (7)")]
        [LabelText ("Scenario Filter", SdfIconType.Diagram2)]
        [ShowIf ("AreScenariosVisible")]
        [OnValueChanged (refreshMethod, true)]
        public DataBlockOverworldEntityScenarios scenarios;
        
        [ShowIf ("@AreScenariosVisible && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldEntityScenarios scenariosProcessed;
        
        
        [DropdownReference]
        [GUIColor ("@GetSectionColor (8)")]
        [LabelText ("Scenario Unit Injection", SdfIconType.PersonPlus)]
        [ShowIf ("AreScenariosVisible")]
        [OnValueChanged (refreshMethod, true)]
        public List<DataBlockScenarioUnits> scenarioUnits;
        
        [ShowIf ("@AreScenariosVisible && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public List<DataBlockScenarioUnits> scenarioUnitsProcessed;
        
        
        [DropdownReference]
        [GUIColor ("@GetSectionColor (8)")]
        [LabelText ("Scenario Changes", SdfIconType.ClipboardPlus)]
        [ShowIf ("AreScenariosVisible")]
        [OnValueChanged (refreshMethod, true)]
        public DataBlockOverworldEntityScenarioChanges scenarioChanges;
        
        [ShowIf ("@AreScenariosVisible && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldEntityScenarioChanges scenarioChangesProcessed;
        
        
        [DropdownReference (true)]
        [GUIColor ("@GetSectionColor (9)")]
        [LabelText ("Area Filter", SdfIconType.Map)]
        [ShowIf ("AreScenariosVisible")]
        [OnValueChanged (refreshMethod, true)]
        public DataBlockOverworldEntityAreas areas;
        
        [ShowIf ("@AreScenariosVisible && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldEntityAreas areasProcessed;
        
        #if UNITY_EDITOR
        
        [GUIColor ("@GetSectionColor (9)")]
        [FoldoutGroup ("Area Keys Filtered", false)]
        [ShowInInspector, ShowIf ("@areas != null")]
        [InlineButton ("UpdateAreaKeysProc", SdfIconType.ArrowRepeat, " Proc.")]
        [InlineButton ("UpdateAreaKeysIn", SdfIconType.ArrowRepeat, " Internal")]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, IsReadOnly = true)]
        [HideLabel]
        private List<string> areaKeysFiltered;

        private void UpdateAreaKeysProc () => UpdateAreaKeys (true);
        private void UpdateAreaKeysIn () => UpdateAreaKeys (false);
        
        public void UpdateAreaKeys (bool processed)
        {
            var filter = processed ? areasProcessed?.tags : areas.tags;
            var keys = ScenarioUtility.GetAreaKeysFromFilter (filter, false);

            if (areaKeysFiltered == null)
                areaKeysFiltered = new List<string> (keys.Count);
            else
                areaKeysFiltered.Clear ();
            areaKeysFiltered.AddRange (keys);
        }

        #endif

        [DropdownReference (true)]
        [GUIColor ("@GetSectionColor (10)")]
        [LabelText ("Production", SdfIconType.MinecartLoaded)]
        [ShowIf ("IsProductionVisible")]
        [OnValueChanged (refreshMethod, true)]
        public DataBlockOverworldEntityProduction production;
        
        [ShowIf ("@IsProductionVisible && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldEntityProduction productionProcessed;
        
        
        [DropdownReference (true)]
        [GUIColor ("@GetSectionColor (10)")]
        [LabelText ("AI", SdfIconType.EmojiExpressionless)]
        [ShowIf ("IsAIVisible")]
        [OnValueChanged (refreshMethod, true)]
        public DataBlockOverworldEntityAI ai;
        
        [ShowIf ("@IsAIVisible && IsInheritanceVisible")]
        [LabelText ("AI Processed")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldEntityAI aiProcessed;


        [DropdownReference (true)]
        [GUIColor ("@GetSectionColor (11)")]
        [LabelText ("Spawning", SdfIconType.PlusCircleDotted)]
        [ShowIf("IsSpawningVisible")]
        [OnValueChanged (refreshMethod, true)]
        public DataBlockOverworldEntitySpawning spawning;

        [ShowIf("@IsSpawningVisible && IsInheritanceVisible")]
        [LabelText ("Spawning Processed")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldEntitySpawning spawningProcessed;

        
        [DropdownReference (true)]
        [GUIColor ("@GetSectionColor (12)")]
        [LabelText ("Reinforcements", SdfIconType.SendPlus)]
        [ShowIf("AreReinforcementsVisible")]
        [OnValueChanged (refreshMethod, true)]
        public DataBlockOverworldEntityReinforcements reinforcements;

        [ShowIf("@AreReinforcementsVisible && IsInheritanceVisible")]
        [LabelText ("Reinforcements Processed")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldEntityReinforcements reinforcementsProcessed;
        

        [DropdownReference (true)]
        [GUIColor ("@GetSectionColor (13)")]
        [LabelText ("Rewards", SdfIconType.CartCheck)]
        [ShowIf("AreRewardsVisible")]
        [OnValueChanged (refreshMethod, true)]
        public DataBlockOverworldEntityRewards rewards;

        [ShowIf("@AreRewardsVisible && IsInheritanceVisible")]
        [LabelText ("Rewards Processed")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldEntityRewards rewardsProcessed;
        
        
        [DropdownReference (true)]
        [GUIColor ("@GetSectionColor (14)")]
        [LabelText ("Effects", SdfIconType.Sunrise)]
        public DataBlockOverworldInteractionEffects interactionEffects;

        [ShowIf("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        [LabelText ("Effects Processed")]
        public DataBlockOverworldInteractionEffects interactionEffectsProcessed;
        
        
        [DropdownReference (true)]
        [GUIColor ("@GetSectionColor (15)")]
        [LabelText ("Salvage Budget", SdfIconType.Wallet2)]
        public DataBlockFloat salvageBudget;
        
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockFloat salvageBudgetProcessed;
        

        [DropdownReference (true)]
        [GUIColor ("@GetSectionColor (16)")]
        [LabelText ("Escalation", SdfIconType.ExclamationDiamond)]
        [ShowIf("IsEscalationVisible")]
        public DataBlockOverworldEntityEscalation escalation;

        [ShowIf("@IsEscalationVisible && IsInheritanceVisible")]
        [LabelText ("Escalation Processed")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldEntityEscalation escalationProcessed;
        
        
        [DropdownReference (true)]
        [GUIColor ("@GetSectionColor (17)")]
        [LabelText ("Threat Level", SdfIconType.BarChart)]
        [ShowIf("IsThreatLevelVisible")]
        public DataBlockOverworldEntityThreatLevel threatLevel;

        [ShowIf("@IsThreatLevelVisible && IsInheritanceVisible")]
        [LabelText ("Threat Level Processed")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldEntityThreatLevel threatLevelProcessed;
        
        
        [DropdownReference (true)]
        [GUIColor ("@GetSectionColor (18)")]
        [LabelText ("Battle Site", SdfIconType.ChevronBarContract)]
        [ShowIf ("IsBattleSiteVisible")]
        public DataBlockOverworldEntityBattleSite battleSite;

        [ShowIf("@IsBattleSiteVisible && IsInheritanceVisible")]
        [LabelText ("Battle Site")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldEntityBattleSite battleSiteProcessed;


        
        public HashSet<string> GetTags (bool processed)
        {
            return processed ? tagsProcessed : tags;
        }
        
        public bool IsHidden () => hidden;

        public override void OnBeforeSerialization ()
        {
            base.OnBeforeSerialization ();
            
            if (visual != null && visual.visualPrefabs != null && visual.visualPrefabs.Count > 0)
            {
                foreach (var v in visual.visualPrefabs)
                {
                    if (v != null)
                        v.OnBeforeSerialization ();
                }
            }
        }

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);

            if (visual != null && visual.visualPrefabs != null && visual.visualPrefabs.Count > 0)
            {
                foreach (var v in visual.visualPrefabs)
                {
                    if (v != null)
                        v.OnAfterDeserialization (key);
                }
            }

            UpdateUnitGroups ();
        }

        private void UpdateUnitGroups ()
        {
            if (scenarioUnits != null)
            {
                for (int x = 0; x < scenarioUnits.Count; ++x)
                {
                    var su = scenarioUnits[x];
                    if (su.unitGroups != null)
                    {
                        for (int i = 0; i < su.unitGroups.Count; ++i)
                        {
                            var g = su.unitGroups[i];
                            g.origin = $"entity {key} group {x}-{i}";
                            g.OnAfterDeserialization ();
                        }
                    }
                }
            }
        }
        
        public override void ResolveText ()
        {
            if (textName != null)
                textName.s = DataManagerText.GetText (TextLibs.overworldEntities, $"{key}__name");
            
            if (textDesc != null)
            if (textDesc != null)
                textDesc.s = DataManagerText.GetText (TextLibs.overworldEntities, $"{key}__text");
        }

        public override void OnKeyReplacement (string keyOld, string keyNew)
        {
            base.OnKeyReplacement (keyOld, keyNew);

            var dataBlueprints = DataMultiLinkerOverworldEntityBlueprint.data;
            foreach (var kvp in dataBlueprints)
            {
                var blueprint = kvp.Value;
                if (blueprint.parents != null)
                {
                    for (int i = 0; i < blueprint.parents.Count; ++i)
                    {
                        var parent = blueprint.parents[i];
                        if (parent != null && parent.key == keyOld)
                        {
                            Debug.LogWarning ($"Entity blueprint {kvp.Key}, parent block {i} | Replacing entity key: {keyOld} -> {keyNew})");
                            parent.key = keyNew;
                        }
                    }
                }
            }
            
            var dataEvents = DataMultiLinkerOverworldEvent.data;
            foreach (var kvp in dataEvents)
            {
                var eventData = kvp.Value;
                if (eventData == null)
                    continue;
                
                if (eventData.steps != null)
                {
                    foreach (var kvp2 in eventData.steps)
                    {
                        var step = kvp2.Value;
                        OnKeyReplacementInTagCheck (step.check?.self?.tags, keyOld, keyNew, $"Event {kvp.Key}, step {kvp2.Key} | Self check");
                        OnKeyReplacementInTagCheck (step.check?.target?.tags, keyOld, keyNew, $"Event {kvp.Key}, step {kvp2.Key} | Target check");
                    }
                }

                if (eventData.options != null)
                {
                    foreach (var kvp2 in eventData.options)
                    {
                        var option = kvp2.Value;
                        OnKeyReplacementInTagCheck (option.check?.self?.tags, keyOld, keyNew, $"Event {kvp.Key}, embedded option {kvp2.Key} | Self check");
                        OnKeyReplacementInTagCheck (option.check?.target?.tags, keyOld, keyNew, $"SEvent {kvp.Key}, embedded option {kvp2.Key} | Target check");
                    }
                }
            }
            
            // Replace entity key in every possible use of CreateOverworldEntity function
            FunctionUtility.ReplaceInFunction (typeof (CreateOverworldEntity), keyOld, keyNew, (function, context) =>
            {
                var functionTyped = (CreateOverworldEntity)function;
                FunctionUtility.TryReplaceInDictionary (functionTyped.spawnData?.generationProfile?.siteTags, keyOld, keyNew, context);
            });

            var dataOptions = DataMultiLinkerOverworldEventOption.data;
            foreach (var kvp in dataOptions)
            {
                var option = kvp.Value;
                OnKeyReplacementInTagCheck (option.check?.self?.tags, keyOld, keyNew, $"Shared option {kvp.Key} | Self check");
                OnKeyReplacementInTagCheck (option.check?.target?.tags, keyOld, keyNew, $"Shared option {kvp.Key} | Target check");
            }

            var dataGenerationProfiles = DataMultiLinkerOverworldSiteGenerationSettings.data;
            foreach (var kvp in dataGenerationProfiles)
            {
                var profile = kvp.Value;
                if (profile.siteTags != null && profile.siteTags.ContainsKey (keyOld))
                {
                    Debug.LogWarning ($"Generation profile {kvp.Key}, site tags | Replacing entity key: {keyOld} -> {keyNew})");
                    var value = profile.siteTags[keyOld];
                    profile.siteTags.Remove (keyOld);
                    profile.siteTags.Add (keyNew, value);
                }
            }
        }

        private void OnKeyReplacementInTagCheck (List<DataBlockOverworldEventSubcheckTag> subchecks, string keyOld, string keyNew, string context)
        {
            if (subchecks == null)
                return;

            for (int i = 0; i < subchecks.Count; ++i)
            {
                var subcheck = subchecks[i];
                if (subcheck != null && subcheck.tag == keyOld)
                {
                    Debug.LogWarning ($"{context} | Tag check {i} | Replacing entity key: {keyOld} -> {keyNew})");
                    subcheck.tag = keyNew;
                }
            }
        }




        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerOverworldEntityBlueprint () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            /*
            if (!string.IsNullOrEmpty (textNameOld))
            {
                textName = new DataBlockStringNonSerialized { s = textNameOld };
                DataManagerText.TryAddingText (TextLibs.overworldEntities, $"{key}__name", textNameOld);
            }
            textNameOld = null;

            if (!string.IsNullOrEmpty (textDescOld))
            {
                textDesc = new DataBlockStringNonSerializedLong { s = textDescOld };
                DataManagerText.TryAddingText (TextLibs.overworldEntities, $"{key}__text", textDescOld);
            }
            textDescOld = null;
            */
            
            if (textName != null)
                DataManagerText.TryAddingTextToLibrary (TextLibs.overworldEntities, $"{key}__name", textName.s);
            
            if (textDesc != null)
                DataManagerText.TryAddingTextToLibrary (TextLibs.overworldEntities, $"{key}__text", textDesc.s);
        }

        private bool IsUIVisible => DataMultiLinkerOverworldEntityBlueprint.Presentation.showUI;
        private bool IsCoreVisible => DataMultiLinkerOverworldEntityBlueprint.Presentation.showCore;
        private bool AreVisualsVisible => DataMultiLinkerOverworldEntityBlueprint.Presentation.showVisuals;
        private bool IsMovementVisible => DataMultiLinkerOverworldEntityBlueprint.Presentation.showMovement;
        
        private bool AreRangesVisible => DataMultiLinkerOverworldEntityBlueprint.Presentation.showRanges;
        private bool IsIntelVisible => DataMultiLinkerOverworldEntityBlueprint.Presentation.showIntel;
        private bool IsBatteryVisible => DataMultiLinkerOverworldEntityBlueprint.Presentation.showBattery;
        private bool IsRepairJuiceVisible => DataMultiLinkerOverworldEntityBlueprint.Presentation.showBattery;
        private bool AreUnitsVisible => DataMultiLinkerOverworldEntityBlueprint.Presentation.showUnits;
        private bool AreScenariosVisible => DataMultiLinkerOverworldEntityBlueprint.Presentation.showScenarios;
        
        private bool IsProductionVisible => DataMultiLinkerOverworldEntityBlueprint.Presentation.showProduction;
        private bool IsAIVisible => DataMultiLinkerOverworldEntityBlueprint.Presentation.showAI;
        private bool IsLootVisible => DataMultiLinkerOverworldEntityBlueprint.Presentation.showLoot;
        private bool AreRewardsVisible => DataMultiLinkerOverworldEntityBlueprint.Presentation.showRewards;
        private bool AreTagsVisible => DataMultiLinkerOverworldEntityBlueprint.Presentation.showTags;
        
        private bool IsEscalationVisible => DataMultiLinkerOverworldEntityBlueprint.Presentation.showEscalation;
        private bool IsThreatLevelVisible => DataMultiLinkerOverworldEntityBlueprint.Presentation.showThreatLevel;
        private bool IsBattleSiteVisible => DataMultiLinkerOverworldEntityBlueprint.Presentation.showBattleSite;

        private bool IsSpawningVisible => DataMultiLinkerOverworldEntityBlueprint.Presentation.showSpawning;
        private bool AreReinforcementsVisible => DataMultiLinkerOverworldEntityBlueprint.Presentation.showReinforcements;
        private bool IsInheritanceVisible => DataMultiLinkerOverworldEntityBlueprint.Presentation.showInheritance;

        private void OnFullRefreshRequired ()
        {
            if (DataMultiLinkerOverworldEntityBlueprint.Presentation.autoUpdateInheritance)
                DataMultiLinkerOverworldEntityBlueprint.ProcessRelated (this);
            
            UpdateUnitGroups ();
        }

        private Color GetSectionColor (int index)
        {
            return Color.HSVToRGB ((float)index / 50f + 0.25f, 0.15f, 1f);
        }

        #endif
    }
}
