using System;
using System.Collections.Generic;
using System.Linq;
using PhantomBrigade.Combat.Components;
using PhantomBrigade.Functions;
using PhantomBrigade.Game;
using PhantomBrigade.Input.Components;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public class DataBlockScenarioSlotSorting
    {
        [SuffixLabel("$GetSuffixLabel")]
        public bool invert = false;
        
        #if UNITY_EDITOR
        private string GetSuffixLabel => invert ? "Lowest distance preferred" : "Highest distance preferred";
        #endif
    }
    
    public class DataBlockScenarioSlotRegistration
    {
        public bool skipRegistration = true;
    }
    
    public class DataBlockScenarioSlotSortingDistancePlayer : DataBlockScenarioSlotSorting
    {
        
    }
    
    public class DataBlockScenarioSlotSortingDistanceEnemy : DataBlockScenarioSlotSorting
    {
        
    }
    
    public class DataBlockScenarioSlotSortingDistanceState : DataBlockScenarioSlotSorting
    {
        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerScenario\", \"GetStateKeys\")")]
        public string key;
    }
    
    public class DataBlockScenarioSlotSortingDistanceRetreat : DataBlockScenarioSlotSorting
    {
        public string key;
    }
    
    public class DataBlockScenarioSlotSortingDistanceLocation : DataBlockScenarioSlotSorting
    {
        public string key;
    }
    
    public class DataBlockScenarioSlotSortingDistanceSpawn : DataBlockScenarioSlotSorting
    {
        public string key;
    }
    
    public class DataBlockScenarioSlot
    {
        [YamlIgnore, HideInInspector, NonSerialized] 
        public DataContainerScenario parentScenario;
        
        [InfoBox ("@GetReport ()", InfoMessageType.None, "IsInfoVisible")]
        [BoxGroup ("Core", false)]
        public bool spawnTagsUsed = true;
        
        [BoxGroup ("Core")]
        [ShowIf ("spawnTagsUsed")]
        [DictionaryKeyDropdown ("@DataShortcuts.sim.combatSpawnTags")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> spawnTags = new SortedDictionary<string, bool> { { "perimeter_outer", true } };
        
        [BoxGroup ("Core")]
        [HideIf ("spawnTagsUsed")]
        public string spawnGroupKey;
        
        [ShowIf ("spawnTagsUsed")]
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckFloat filterDistancePlayer;

        [ShowIf ("spawnTagsUsed")]
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckFloat filterDistanceEnemy;
        
        [ShowIf ("spawnTagsUsed")]
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckFloatKeyGeneric filterDistanceSpawn;
        
        [ShowIf ("spawnTagsUsed")]
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckFloatKeyGeneric filterDistanceLocation;
        
        [ShowIf ("spawnTagsUsed")]
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckFloatKeyGeneric filterDistanceVolume;

        [ShowIf ("spawnTagsUsed")]
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckFloatForStates filterDistanceState;

        [ShowIf ("spawnTagsUsed")]
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckFloatForRetreats filterDistanceRetreat;

        [ShowIf ("spawnTagsUsed")]
        [DropdownReference (true)]
        public DataBlockScenarioSlotSorting sorting;
        
        [ShowIf ("spawnTagsUsed")]
        [DropdownReference (true)]
        public DataBlockScenarioSlotRegistration spawnRegistration;
        
        [YamlIgnore, HideInInspector] 
        public string origin;

        public virtual string GetDescription ()
        {
            return "?";
        }

        public virtual string GetReport ()
        {
            return $"Parent scenario: {(parentScenario != null ? parentScenario.key : "-")} | Origin: {origin}";
        }
        
        #region Editor
        #if UNITY_EDITOR

        private bool IsInfoVisible => DataMultiLinkerScenario.Presentation.showUtilityData;
        private IEnumerable<string> GetStepKeys => 
            parentScenario != null && parentScenario.stepsProc != null ? parentScenario.stepsProc.Keys : null;

        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioSlot () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }



    public class DataBlockScenarioSlotCheck
    {
        [YamlIgnore, HideInInspector, NonSerialized] 
        public DataBlockScenarioUnitGroup parentSlot;
        
        public bool log;
        
        [DropdownReference]
        [DictionaryKeyDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerScenario\", \"GetStateKeys\")")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> stateRequirements;

        [DropdownReference]
        public List<IOverworldValidationFunction> checksBase;
        
        [DropdownReference]
        public List<IOverworldValidationFunction> checksTarget;
        
        [DropdownReference]
        public List<IOverworldValidationFunction> checksProvince;

        public bool IsPassed ()
        {
            #if !PB_MODSDK

            if (stateRequirements != null && stateRequirements.Count > 0)
            {
                bool stateRequirementsMet = ScenarioUtility.AreStateRequirementsPassed (stateRequirements);
                if (!stateRequirementsMet)
                {
                    if (log)
                        Debug.LogWarning ($"Step {parentSlot?.parentStep?.key} unit group slot {parentSlot?.index} skipped: state requirement check not passed");
                    return false;
                }
            }

            if (checksBase != null)
            {
                var basePersistent = IDUtility.playerBasePersistent;
                
                if (basePersistent == null)
                    return false;

                foreach (var function in checksBase)
                {
                    if (function != null && !function.IsValid (basePersistent))
                    {
                        if (log)
                            Debug.LogWarning ($"Step {parentSlot?.parentStep?.key} unit group slot {parentSlot?.index} skipped: player base didn't pass the condition {function.GetType ().Name}");
                        return false;
                    }
                }
            }
            
            if (checksTarget != null)
            {
                var persistent = Contexts.sharedInstance.persistent;
                var targetPersistent = IDUtility.GetPersistentEntity (persistent.combatSiteLink.persistentID);
                
                if (targetPersistent == null)
                    return false;

                foreach (var function in checksTarget)
                {
                    if (function != null && !function.IsValid (targetPersistent))
                    {
                        if (log)
                            Debug.LogWarning ($"Step {parentSlot?.parentStep?.key} unit group slot {parentSlot?.index} skipped: target {targetPersistent.ToLog ()} didn't pass the condition {function.GetType ().Name}");
                        return false;
                    }
                }
            }
            
            if (checksProvince != null)
            {
                var persistent = Contexts.sharedInstance.persistent;
                var targetPersistent = IDUtility.GetPersistentEntity (persistent.combatSiteLink.persistentID);
                var targetOverworld = IDUtility.GetLinkedOverworldEntity (targetPersistent);
                var provinceKey = targetOverworld != null ? DataHelperProvince.GetProvinceKeyAtEntity (targetOverworld) : null;
                var provincePersistent = IDUtility.GetPersistentEntity (provinceKey);
                
                if (provincePersistent == null)
                    return false;

                foreach (var function in checksProvince)
                {
                    if (function != null && !function.IsValid (provincePersistent))
                    {
                        if (log)
                            Debug.LogWarning ($"Step {parentSlot?.parentStep?.key} unit group slot {parentSlot?.index} skipped: province {provincePersistent.ToLog ()}  didn't pass the condition {function.GetType ().Name}");
                        return false;
                    }
                }
            }

            if (log)
                Debug.LogWarning ($"Step {parentSlot?.parentStep?.key} unit group slot {parentSlot?.index} will be activated, all checks passed");
            return true;

            #else
            return false;
            #endif
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioSlotCheck () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
    
    
    
    [Serializable]
    public class DataBlockScenarioUnitGroup : DataBlockScenarioSlot
    {
        [YamlIgnore, HideInInspector, NonSerialized] 
        public DataBlockScenarioStep parentStep;
        
        [YamlIgnore, HideInInspector]
        public int index;
        
        [DropdownReference (true)]
        public DataBlockScenarioSlotCheck check;

        [DropdownReference (true)]
        [ValueDropdown ("@DataShortcuts.sim.scenarioUnitTags")]
        public HashSet<string> combatTags;

        [BoxGroup ("Optional", false)]
        [LabelText ("Group Occupied")]
        // [InfoBox("At the moment, all spawn groups are always marked as fully occupied and partial fits are not supported")]
        public bool spawnGroupOccupied = true;
        
        [BoxGroup ("Optional")]
        [LabelText ("Avoid Partial Fits")]
        [HideInInspector]
        public bool spawnCountStrict = true;
        
        [BoxGroup ("Optional")]
        [LabelText ("Random Order")]
        public bool spawnOrderRandom = true;
        
        [BoxGroup ("Optional"), HorizontalGroup ("Optional/Faction")]
        [LabelText ("Faction Branch Override")]
        [PropertyTooltip ("Use something other than faction branch of the site when generating units in this unit group. Used when unit group you're targeting specifies it takes unit preset tags from overworld.")]
        public bool factionBranchOverride = false;

        [BoxGroup ("Optional"), HorizontalGroup ("Optional/Faction")]
        [HideLabel, ShowIf ("factionBranchOverride")]
        [ValueDropdown ("@DataMultiLinkerOverworldFactionBranch.data.Keys")]
        public string factionBranchKey;

        [BoxGroup ("Optional")]
        [PropertyRange (0, 2)]
        public int baseGrade = 0;
        
        [BoxGroup ("Optional")]
        [PropertyRange (0, 2)]
        public int maxGrade = 2;
        
        [BoxGroup ("Optional")]
        public bool cloneable = true;
        
        [DropdownReference]
        [LabelText ("Functions On Units")]
        public List<ICombatFunctionTargeted> functions;

        public virtual void OnAfterDeserialization ()
        {
            
        }
        
        public override string GetReport ()
        {
            var reportBase = base.GetReport ();
            return $"{reportBase} | Parent step: {(parentStep != null ? parentStep.key : "-")} | Index: {index}";
        }
    }
    
    [Serializable]
    public class DataBlockScenarioUnitGroupLink : DataBlockScenarioUnitGroup
    {
        [ValueDropdown ("@DataMultiLinkerCombatUnitGroup.data.Keys")]
        public string key;
        
        public override string GetDescription ()
        {
            return $"link {key}";
        }
    }
    
    [Serializable]
    public class DataBlockScenarioUnitGroupFilter : DataBlockScenarioUnitGroup
    {
        [HorizontalGroup]
        [LabelText ("Filter From Branch")]
        public bool tagsFromFactionBranch = true;

        [ShowIf ("tagsFromFactionBranch"), ShowInInspector]
        [HideLabel, HorizontalGroup (200f), ValueDropdown ("@DataMultiLinkerOverworldFactionBranch.data.Keys")]
        private string branchKey = "branch_army";
        
        [OnValueChanged ("OnAfterDeserialization", true)]
        [DictionaryKeyDropdown ("@DataMultiLinkerCombatUnitGroup.tags")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> tags = new SortedDictionary<string, bool> ();

        [YamlIgnore]
        [LabelText ("Groups Preview")]
        [ListDrawerSettings (DefaultExpandedState = false, ShowPaging = false, IsReadOnly = true)]
        public List<DataContainerLink<DataContainerCombatUnitGroup>> groupsFiltered;

        private static SortedDictionary<string, bool> tagsCombined = new SortedDictionary<string, bool> ();
        
        public override void OnAfterDeserialization ()
        {
            #if UNITY_EDITOR

            UtilityCollections.ClearOrCreate (ref groupsFiltered);
            if (tags == null || tags.Count == 0)
                return;

            SortedDictionary<string, bool> tagFilter = null;
            if (tagsFromFactionBranch && !string.IsNullOrEmpty (branchKey))
            {
                tagsCombined.Clear ();
                tagsCombined.Add (branchKey, true);
                foreach (var kvp in tags)
                    tagsCombined[kvp.Key] = kvp.Value;
                tagFilter = tagsCombined;
            }
            else
                tagFilter = tags;
            
            var groupsWithTags = DataTagUtility.GetContainersWithTags (DataMultiLinkerCombatUnitGroup.data, tagFilter);
            foreach (var objectWithTags in groupsWithTags)
            {
                var unitGroup = objectWithTags as DataContainerCombatUnitGroup;
                if (unitGroup == null)
                    continue;
                    
                var entry = new DataContainerLink<DataContainerCombatUnitGroup> (unitGroup);
                groupsFiltered.Add (entry);
            }

            #endif
        }
        
        public override string GetDescription ()
        {
            return $"filter {tags.ToStringFormattedKeyValuePairs ()}";
        }
    }
    
    [Serializable]
    public class DataBlockScenarioUnitGroupEmbedded : DataBlockScenarioUnitGroup
    {
        [ListDrawerSettings (AlwaysAddDefaultValue = true)]
        public List<DataBlockScenarioUnitResolver> units = new List<DataBlockScenarioUnitResolver> ();

        public override string GetDescription ()
        {
            if (units != null && units.Count > 0)
                return $"embedded, {units.Count} units";
            else
                return "embedded, no units";
        }
    }

    [Serializable, HideReferenceObjectPicker]
    public class DataBlockScenarioCheck
    {
        [DropdownReference]
        public DataBlockOverworldEventSubcheckInt turn;
        
        [DropdownReference]
        public DataBlockOverworldEventSubcheckInt countEnemy;
        
        [DropdownReference]
        public DataBlockOverworldEventSubcheckInt countFriendly;

        public bool used => turn != null || countEnemy != null || countFriendly != null;
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioCheck () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
    
    public class DataBlockScenarioExit
    {
        public bool lootUsed = false;
        public bool debriefingUsed = false;
        public bool ejectionsUsed = false;
    }

    [Flags]
    public enum CombatOutcomesMask
    {
        None = 0,
        VictoryEarly = 1,
        VictoryTotal = 2,
        DefeatEarly = 4,
        DefeatTotal = 8
    }
    
    public class DataBlockScenarioExitFunctions
    {
        public CombatOutcomesMask mask = CombatOutcomesMask.None;
        public List<IOverworldFunction> functions;
    }
    
    public class DataBlockScenarioExitBehaviour
    {
        public DataBlockScenarioExit victory;
        public DataBlockScenarioExit defeat;
        
        public List<DataBlockScenarioExitFunctions> functions;
        
        #if !PB_MODSDK
        [ValueDropdown ("GetCallKeys")]
        #endif
        public List<string> calls;
        
        #region Editor
        #if UNITY_EDITOR

        #if !PB_MODSDK
        private IEnumerable<string> GetCallKeys => 
            FieldReflectionUtility.GetConstantStringFieldValues (typeof (ScenarioFunctionKeys));
        #endif

        #endif
        #endregion
    }

    
    /*
    public class DataBlockScenarioOutcomeReactions
    {
        public List<DataBlockScenarioOutcomeReaction> reactions = new List<DataBlockScenarioOutcomeReaction>
        {
            new DataBlockScenarioOutcomeReaction ()
        };
    }
    
    public class DataBlockScenarioOutcomeReaction
    {
        [ToggleLeft, BoxGroup]
        public bool outcomeVictory;
        
        [ToggleLeft, BoxGroup]
        public bool outcomeDefeat;
        
        [ToggleLeft, BoxGroup]
        public bool caseEarly;
        
        [ToggleLeft, BoxGroup]
        public bool caseTotal;
        
        [HideLabel, HideReferenceObjectPicker]
        public DataBlockScenarioStateReaction reaction = new DataBlockScenarioStateReaction ();
    }
    */

    [HideReferenceObjectPicker]
    public class DataBlockScenarioRetreat
    {
        [InfoBox ("$GetReport")]
        public string stateKey = "retreat_default";
        public bool stateValueModified;
        [ShowIf ("stateValueModified")]
        public bool stateValue;

        // public string key;
        // public CombatOutcome outcome;

        public string GetReport ()
        {
            return $"Retreat is managed by state {stateKey}. Retreat outcome: {(stateValueModified ? stateValue ? "victory" : "defeat" : "unmodified, managed by state value")}";
        }
    }
    
    
    
    [HideReferenceObjectPicker]
    public class DataBlockScenarioActionRestrictions
    {
        public bool inverted = false;
        
        [ValueDropdown ("@DataMultiLinkerAction.data.Keys")]
        [LabelText("@inverted ? \"Blocked actions\" : \"Allowed actions\"")]
        public HashSet<string> keys = new HashSet<string> ();
        
        [ValueDropdown ("@DataMultiLinkerAction.data.Keys")]
        [LabelText("Disabled actions")]
        public HashSet<string> keysDisabled = new HashSet<string> ();
    }

    [HideReferenceObjectPicker]
    public class DataBlockScenarioCutscene
    {
        [ValueDropdown ("@DataMultiLinkerCutscene.data.Keys")]
        public string key;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockScenarioCutsceneVideo
    {
        [ValueDropdown ("@DataMultiLinkerCutsceneVideo.data.Keys")]
        public string key;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockScenarioAtmosphere
    {
        [PropertyRange (0f, 1f)]
        public float fogTarget = 0.66f;
        
        [PropertyRange (0.1f, 1f)]
        public float fogSpeed = 0.25f;
    }

    [HideReferenceObjectPicker]
    public class DataBlockScenarioCamera
    {
        public bool positionOverride = false;
        
        [ShowIf ("positionOverride")]
        public Vector3 position = Vector3.zero;
        
        [ShowIf ("positionOverride")]
        [LabelText ("Position From Entity")]
        public string positionFromEntityName;
        
        public bool rotationXOverride = false;
        
        [ShowIf ("rotationXOverride")]
        public float rotationX = 45f;
        
        public bool rotationYOverride = false;
        
        [ShowIf ("rotationYOverride")]
        public float rotationY = 180f;
        
        public bool zoomOverride = false;
        
        [ShowIf ("zoomOverride")]
        public float zoom = 0.5f;

        [Button ("Apply to camera"), PropertyOrder (-1), HideInEditorMode]
        public void ApplyToCamera ()
        {
            #if !PB_MODSDK

            var cameraPos = GameCameraSystem.GetPositionTarget ();
            var cameraRotationX = GameCameraSystem.GetRotationX ();
            var cameraRotationY = GameCameraSystem.GetRotationY ();
            var cameraZoom = GameCameraSystem.GetZoomTarget ();

            if (positionOverride)
            {
                if (!string.IsNullOrEmpty (positionFromEntityName))
                {
                    var unitPersistent = Contexts.sharedInstance.persistent.GetEntityWithNameInternal (positionFromEntityName);
                    var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
                    var combatView = unitCombat != null && unitCombat.hasCombatView ? unitCombat.combatView.view : null;
                    if (combatView != null)
                        cameraPos = combatView.transform.position;
                }
                else
                    cameraPos = position;
            }

            if (rotationXOverride)
                cameraRotationX = rotationX;
                
            if (rotationYOverride)
                cameraRotationY = rotationY;
                
            if (zoomOverride)
                cameraZoom = zoom;
                
            GameCameraSystem.OverrideInputTargets (cameraPos, cameraRotationY, cameraRotationX, cameraZoom);

            #endif
        }
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockScenarioUnitSelection
    {
        public string nameInternal;
    }
    
    
    

    [HideReferenceObjectPicker]
    public class DataBlockScenarioSubcheckConstraintUnit
    {
        public string nameInternal;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockScenarioSubcheckConstraintFaction
    {
        [ValueDropdown ("@DataHelperUnitEquipment.GetFactions ()")]
        public string key = Factions.player;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockScenarioSubcheckConstraintPosition
    {
        public Vector3 position;
        public float radius;
    }
    
    // New action constraint
    [HideReferenceObjectPicker]
    public class DataBlockScenarioSubcheckUnitState
    {
        [PropertyTooltip ("A check demanding a unit to be active or not. A unit is considered active if it is not wrecked, has a pilot, and that pilot is not dead or knocked out.")]
        [DropdownReference (true), InlineProperty]
        public DataBlockOverworldEventSubcheckBool unitActive;
        
        [PropertyTooltip ("A check demanding a unit to be mobile or not. A unit is considered mobile if active and capable of moving.")]
        [DropdownReference (true), InlineProperty]
        public DataBlockOverworldEventSubcheckBool unitMobile;
        
        [PropertyTooltip ("A check demanding a unit to be hidden or not.")]
        [DropdownReference (true), InlineProperty]
        public DataBlockOverworldEventSubcheckBool unitHidden;
        
        [PropertyTooltip ("A check demanding a unit to be destroyed or not.")]
        [DropdownReference (true), InlineProperty]
        public DataBlockOverworldEventSubcheckBool unitWrecked;
        
        [PropertyTooltip ("A check demanding a unit to be non-functional or not. Warning: crashing units are non-functional, do not use this to check for unit destruction.")]
        [DropdownReference (true), InlineProperty]
        public DataBlockOverworldEventSubcheckBool unitDisabled;
        
        [PropertyTooltip ("A check demanding a pilot to be ejected or not.")]
        [DropdownReference (true), InlineProperty]
        [LabelText ("Pilot Ejected")]
        public DataBlockOverworldEventSubcheckBool pilotMissing;
        
        [PropertyTooltip ("A check demanding a pilot to be dead or not.")]
        [DropdownReference (true), InlineProperty]
        public DataBlockOverworldEventSubcheckBool pilotDead;

        [PropertyTooltip ("A check demanding a pilot to be knocked out or not.")]
        [DropdownReference (true), InlineProperty]
        public DataBlockOverworldEventSubcheckBool pilotConcussed;
        
        [PropertyTooltip ("A check demanding a pilot to be active or not. A pilot is considered active when it is present, not dead, and not knocked out. A subset of unit active check.")]
        [DropdownReference (true), InlineProperty]
        public DataBlockOverworldEventSubcheckBool pilotActive;
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioSubcheckUnitState () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
    
    // New action constraint
    [HideReferenceObjectPicker]
    public class DataBlockScenarioSubcheckUnitAction
    {
        [ValueDropdown ("@DataMultiLinkerAction.data.Keys")]
        public string key;
        
        [DropdownReference (true)]
        public DataBlockInt actionCount;

        [DropdownReference (true)]
        public DataBlockScenarioSubcheckConstraintUnit targetUnit;
        
        [DropdownReference (true)]
        public DataBlockScenarioSubcheckConstraintPosition targetPosition;
        
        [DropdownReference]
        public HashSet<CombatUIModes> targetVisibleModes;

        [DropdownReference]
        public SortedDictionary<string, bool> targetVisibleStateValues;
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioSubcheckUnitAction () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    // Old action constraint
    [HideReferenceObjectPicker]
    public class DataBlockScenarioSubcheckActionPlanned
    {
        [ValueDropdown ("@DataMultiLinkerAction.data.Keys")]
        public string key;

        [DropdownReference]
        public DataBlockScenarioSubcheckConstraintFaction faction;
        
        [DropdownReference]
        public DataBlockScenarioSubcheckConstraintUnit owner;
        
        [DropdownReference]
        public DataBlockScenarioSubcheckConstraintUnit targetUnit;
        
        [DropdownReference]
        public DataBlockScenarioSubcheckConstraintPosition targetPosition;
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioSubcheckActionPlanned () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockScenarioSubcheckConstraintUnitTags
    {
        [DictionaryKeyDropdown ("@DataShortcuts.sim.scenarioUnitTags")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> filter;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockScenarioLocationVisual
    {
        [ValueDropdown ("@DataMultiLinkerAssetPools.data.Keys")]
        public string key = "obj_prop_loot_01";
        public bool reactionAnimation = true;
    }

    [HideReferenceObjectPicker]
    public class DataBlockScenarioStateLocationRetreat
    {
        public bool commsUsed = false;
    }
    
    // New location constraint
    [HideReferenceObjectPicker]
    public class DataBlockScenarioStateLocation
    {
        public bool visibleInWorld = true;
        
        [ShowInInspector, BoxGroup]
        public IDataBlockAreaLocationProvider locationProvider;

        [DropdownReference (true)]
        public DataBlockScenarioLocationVisual visual;

        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioStateLocation () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    // Old location constraint
    [HideReferenceObjectPicker]
    public class DataBlockScenarioSubcheckLocationOccupied
    {
        [HorizontalGroup]
        public string locationSourceName;

        [HorizontalGroup (0.3f), HideLabel]
        public CombatLocationSource locationSource = CombatLocationSource.State;

        [DropdownReference]
        public DataBlockOverworldEventSubcheckBool occupied;
        
        [DropdownReference]
        public DataBlockOverworldEventSubcheckFloat distance;
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioSubcheckLocationOccupied () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }


    [HideReferenceObjectPicker]
    public class DataBlockScenarioVolume
    {
        public bool visibleInWorld = true;
        
        [ShowInInspector]
        public IDataBlockAreaVolumeProvider volumeProvider;

        [DropdownReference]
        public DataBlockOverworldEventSubcheckFloat integrity;
        
        [DropdownReference]
        public DataBlockOverworldEventSubcheckInt destructions;

        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockScenarioVolume () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    [HideReferenceObjectPicker]
    public class DataBlockScenarioOutcome
    {
        public CombatOutcome type;
        public bool early = true;
        public bool instant = false;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockScenarioFunctionsPerUnit
    {
        [DropdownReference]
        public List<ICombatFunction> functions;
        
        [DropdownReference]
        public List<ICombatFunctionTargeted> functionsTargeted;

        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioFunctionsPerUnit () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
    
    
    [HideReferenceObjectPicker]
    public class DataBlockScenarioMusicMood
    {
        [ValueDropdown ("GetKeys")]
        public string key;
        
        public bool executionAllowed = true;
        
        #if UNITY_EDITOR
        private static List<string> GetKeys => new List<string> { "optimism", "uncertainty", "fear" };
        #endif
    }

    [HideReferenceObjectPicker]
    public class DataBlockScenarioReactiveMusic
    {
	    [PropertyTooltip("Is the change to mood relative to the current mood?")]
        public MusicReactiveModifier.Mode mode;
        [PropertyTooltip("Either the magnitude of the change or the absolute value")]
		public int value;
		[PropertyTooltip("Number of turns this effect lasts for")]
        public int numberOfTurns;
        [PropertyTooltip("Should the reactive music system pause updates while this is active?")]
        public bool pauseEvaluation;
    }

    [HideReferenceObjectPicker]
    public class DataBlockScenarioAudio
    {
        public float time = 0f;
        
        [ShowIf ("@time > 0f")]
        public bool realtime = true;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockScenarioAudioEvent : DataBlockScenarioAudio
    {
        public string key;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockScenarioAudioState : DataBlockScenarioAudio
    {
        public string key;
        public string state;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockScenarioAudioSync : DataBlockScenarioAudio
    {
        public string key;
        public float value;
    }
    
        
    [HideReferenceObjectPicker]
    public class DataBlockScenarioCommLink
    {
        [DisableIf ("hidden")]
        [LabelText ("Time / Hidden"), LabelWidth (100f)]
        [HorizontalGroup]
        public float time = 0f;
        
        [HorizontalGroup (100f)]
        [HideLabel]
        public bool hidden = false;

        [DisableIf ("hidden")]
        [ValueDropdown ("@DataMultiLinkerCombatComms.data.Keys")]
        [HideLabel]
        public string key;
        
        #if UNITY_EDITOR

        [ShowInInspector]
        [MultiLineProperty (3), GUIColor (1f, 1f, 1f, 0.5f)]
        [HideLabel]
        public string content
        {
            get
            {
                return GetContent ();
            }
            set
            {
                //
            }
        }

        private string GetContent ()
        {
            if (string.IsNullOrEmpty (key))
                return string.Empty;
            
            var data = DataMultiLinkerCombatComms.GetEntry (key);
            if (data == null)
                return string.Empty;

            if (data.textContent == null || data.textContent.Count == 0)
                return string.Empty;

            if (data.textContent.Count == 1)
                return data.textContent[0];

            var textContentCombined = data.textContent.ToStringFormatted (true, multilinePrefix: "- ");
            return textContentCombined;
        }

        [Button, HideInEditorMode, PropertyOrder (-1)]
        private void Test ()
        {
            #if !PB_MODSDK

            if (!Application.isPlaying)
                return;
            
            CIViewCombatComms.ScheduleMessage (this);

            #endif
        }
        
        private string GetSuffix => hidden ? "hidden" : "active";

        #endif
    }

    [HideReferenceObjectPicker]
    public class DataBlockScenarioUnitChangeFlags
    {
        public bool visible = true;
        public bool playerControllable = false;
        public bool aiControllable = false;
        public bool aiControllableNextTurn = false;
    }

    [HideReferenceObjectPicker]
    public class DataBlockScenarioUnitChangeDestruction
    {
        public float delay;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockScenarioUnitChangeTransform
    {
        public Vector3 position = Vector3.zero;
        public float rotation = 0f;
    }

    public enum CombatTargetSource
    {
        None,
        Self,
        SelfRelative,
        Unit,
        UnitRelative,
        UnitBlackboard,
        UnitBlackboardRelative,
        State,
        Location,
        Volume
    }
    
    public enum CombatLocationSource
    {
        State,
        Location
    }

    public class DataBlockActionFunctionTimed
    {
        [PropertyRange (0f, 1f)]
        public float timeNormalized;
        public DataBlockActionFunctionRepeat repeat;
        public List<ICombatFunctionTargeted> functions;
    }

    public class DataBlockActionFunctionRepeat
    {
        [PropertyRange (0f, 1f)]
        public float timeNormalizedEnd = 0.05f;
        public int count = 1;
    }

    public class DataBlockScenarioUnitTargetFiltered
    {
        public bool average = false;
        public bool directional = false;
        
        [HideIf ("average")]
        public bool relative = false;
        
        public DataBlockScenarioSubcheckUnit filter = new DataBlockScenarioSubcheckUnit ();
    }

    public class TargetFromSourceLocal
    {
        public bool grounded;
        public float groundOffset;
    }

    public enum MovementActionCustomMode
    {
        Linear,
        LinearRaycastDown,
        LinearRaycastForward
    }
    
    public class MovementActionCustomization
    {
        public bool align = true;
        public bool raycast = true;
        
        [ShowIf ("raycast")]
        public bool raycastDrop = false;
        
        [ShowIf ("raycast")]
        public float raycastOffset = 0f;

        [ShowIf ("raycast")]
        [PropertyRange (0f, 90f)]
        public float raycastPitch = 90f;
    }

    
    [HideReferenceObjectPicker]
    public class DataBlockScenarioUnitChangeAction
    {
        [ValueDropdown ("@DataMultiLinkerAction.data.Keys")]
        public string key;

        [PropertyTooltip ("Only used on secondary timeline actions: primary timeline actions are automatically placed. Set to below 0 to make secondary timeline actions")]
        public float startTime;

        [LabelText ("Local Time")]
        public bool startTimeIsLocal = true;
        public bool locked = false;
        
        [DropdownReference (true)]
        public DataBlockFloat durationVariable;
        
        [DropdownReference (true)]
        public TargetFromSourceLocal targetLocal;
        
        [DropdownReference (true)]
        [GUIColor ("primaryColor")]
        public TargetFromSource target;
        
        [DropdownReference (true)]
        [GUIColor ("secondaryColor")]
        public TargetFromSourceInterpolated targetSecondary;
        
        [DropdownReference (true)]
        public DataBlockScenarioUnitTargetFiltered targetUnitFiltered;
        
        [DropdownReference (true)]
        public TargetUnitLocalOffsets targetUnitLocalOffsets;
        
        [DropdownReference (true)]
        public MovementActionCustomization movementCustom;

        [DropdownReference]
        public List<DataBlockActionFunctionTimed> functionsTimed;
        
        [DropdownReference]
        public List<ICombatActionExecutionFunction> functionsOnAction;

        public void SortFunctions ()
        {
            if (functionsTimed != null)
                functionsTimed.Sort ((x, y) => x.timeNormalized.CompareTo (y.timeNormalized));
        }

        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioUnitChangeAction () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        private Color primaryColor = Color.HSVToRGB (0.3f, 0.1f, 1f);
        private Color secondaryColor = Color.HSVToRGB (0.15f, 0.1f, 1f);
        
        #endif
        #endregion
    }

    [HideReferenceObjectPicker]
    public class DataBlockScenarioUnitChange
    {
        [DropdownReference (true)]
        public DataBlockScenarioUnitChangeFlags flags;
        
        [DropdownReference (true)]
        public DataBlockScenarioUnitChangeDestruction destruction;
        
        [DropdownReference (true)]
        public DataBlockScenarioUnitChangeTransform transform;
        
        [DropdownReference]
        public List<DataBlockScenarioUnitChangeAction> actions;
        
        [DropdownReference]
        public List<ICombatFunctionTargeted> functions;

        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioUnitChange () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockScenarioUnitChangeChecked : DataBlockScenarioUnitChange
    {
        [PropertyOrder (-1)]
        [DropdownReference (true)]
        public string nameInternal;
        
        [PropertyOrder (-1)]
        [DropdownReference (true)]
        public DataBlockScenarioSubcheckUnit check;
    }
    
    
    
    // Transition data block defines a connection from one step to another
    // Since there can be more than one transition, they are ranked by priority and evaluated top to bottom
    // Transitions can be gated by certain facts ("states") being true or false, similar to tag filtering

    public class DataBlockScenarioTransition
    {
        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerScenario\", \"GetStepKeys\")")]
        [HideLabel]
        public string stepKey;
    }
    
    public class DataBlockScenarioTransitionStateBased : DataBlockScenarioTransition
    {
        [PropertyRange (0, 5)]
        public int priority;
        
        [LabelText ("Scope Cleanup")]
        public bool stateCleanupOnUse = false;
        
        [DictionaryKeyDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerScenario\", \"GetStateKeys\")")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> stateRequirements;
    }
    
    public class DataBlockScenarioTransitionTimeBased : DataBlockScenarioTransition
    {
        [PropertyRange (0, 60)]
        public float time;
    }

    public class DataBlockScenarioSubcheckTurn : DataBlockOverworldEventSubcheckInt
    {
        public bool relative;
    }
    
    public class DataBlockScenarioSubcheckTurnModulus : DataBlockOverworldEventSubcheckInt
    {
        public bool relative;
        public int factor;
    }

    public class DataBlockUnitFilterBlackboardExport
    {
        public string key = "tgt";
        public bool indexed;
    }
    
    public class DataBlockUnitFilterMemoryExport
    {
        [ValueDropdown ("@DataMultiLinkerOverworldMemory.data.Keys")]
        public string key = "combat_sc_targets_present";
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockScenarioSubcheckUnitFilter : DataBlockScenarioSubcheckUnit
    {
        [PropertyOrder (-2)]
        [HideIf ("@name != null")]
        [SuffixLabel ("@GetSortSuffix ()")]
        public TargetSortMode sort = TargetSortMode.None;
        
        [LabelText ("Max Selections")]
        [PropertyOrder (-2)]
        public int unitLimit = -1;
        
        [ShowIf ("@unitLimit > 1")]
        [LabelText ("Repeated Selection")]
        [PropertyOrder (-2)]
        public bool unitRepeats = false;

        [PropertyOrder (50)]
        [DropdownReference (true)]
        [LabelText ("Export To Blackboard (Limited)")]
        public DataBlockUnitFilterBlackboardExport exportEntitiesToBlackboardLimited;
        
        [PropertyOrder (50)]
        [DropdownReference (true)]
        [LabelText ("Export To Memory (Limited)")]
        public DataBlockUnitFilterMemoryExport exportCountToMemoryLimited;


        #if !PB_MODSDK

        public List<CombatEntity> GetFilteredUnitsUsingSettings
        (
            Vector3 originPosition = default,
            Vector3 originDirection = default,
            int desiredFactionOverride = -1
        )
        {
            var result = GetFilteredUnitsWithLimit (sort, unitLimit, unitRepeats, originPosition, originDirection, desiredFactionOverride);
            
            if (exportCountToMemoryLimited != null && !string.IsNullOrEmpty (exportCountToMemoryLimited.key))
            {
                int count = result != null ? result.Count : 0;
                var basePersistent = IDUtility.playerBasePersistent;
                basePersistent.SetMemoryFloat (exportCountToMemoryLimited.key, count);
            }
            
            if (result != null && exportEntitiesToBlackboardLimited != null && !string.IsNullOrEmpty (exportEntitiesToBlackboardLimited.key))
            {
                var blackboardKey = exportEntitiesToBlackboardLimited.key;
                if (exportEntitiesToBlackboardLimited.indexed)
                    ScenarioUtility.SaveEntitiesToGlobalBlackboard (blackboardKey, result);
                else
                    ScenarioUtility.SaveEntityToGlobalBlackboard (blackboardKey, result.FirstOrDefault ());
            }
            
            return result;
        }

        #endif
        
        #if UNITY_EDITOR

        private string GetSortSuffix ()
        {
            if (sort == TargetSortMode.None)
                return "Unordered units";
            if (sort == TargetSortMode.Dot)
                return "Closest units to forward dir. first";
            if (sort == TargetSortMode.DotInv)
                return "Farthest units from forward dir. first";
            if (sort == TargetSortMode.Distance) 
                return "Closest units first";
            if (sort == TargetSortMode.DistanceInv)
                return "Farthest units first";
            return "?";
        }
        
        #endif
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockScenarioSubcheckUnitCompositeConnected : DataBlockScenarioSubcheckUnit
    {
        [PropertyOrder (-2)]
        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerUnitComposite\", \"GetUnitKeys\")")]
        public HashSet<string> unitKeys = new HashSet<string> ();
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockScenarioSubcheckUnitCounted : DataBlockScenarioSubcheckUnit
    {
        [DropdownReference (true)] 
        public DataBlockOverworldEventSubcheckInt count;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockScenarioSubcheckUnitRelativeTransform
    {
        [DropdownReference (true)]
        public TargetFromSource origin;
            
        [ShowIf ("@directionDot != null || directionAngle != null")]
        public Vector3 directionOriginRotation;
        
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckFloat directionDot;
        
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckFloat directionDotFlat;
        
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckFloat directionAngle;
        
        [DropdownReference (true)]
        public SubsystemRetargetFilterRange distance;
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (10)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioSubcheckUnitRelativeTransform () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    public enum TargetSortMode
    {
        None,
        Distance,
        DistanceInv,
        Dot,
        DotInv
    }

    [HideReferenceObjectPicker]
    public class DataBlockScenarioSubcheckUnit
    {
        [PropertyOrder (9)]
        [DropdownReference (true)]
        public DataBlockUnitFilterMemoryExport exportCountToMemory;
        
        [PropertyOrder (9)]
        [DropdownReference (true)]
        public DataBlockUnitFilterBlackboardExport exportEntitiesToBlackboard;
        
        [DropdownReference]
        [DictionaryKeyDropdown ("@DataShortcuts.sim.scenarioUnitTags")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> tags;

        [DropdownReference (true)]
        public DataBlockScenarioSubcheckConstraintUnit name;
        
        [DropdownReference (true)]
        public DataBlockScenarioSubcheckConstraintFaction faction;

        [DropdownReference (true)]
        public DataBlockScenarioSubcheckUnitState state;

        [DropdownReference (true)]
        public DataBlockScenarioSubcheckUnitAction actionPlanned;

        [DropdownReference (true)] 
        public DataBlockOverworldEventSubcheckBool locationOccupied;
        
        [DropdownReference (true)] 
        public DataBlockScenarioSubcheckLocationOccupied locationOccupiedUnlinked;
        
        // [DropdownReference (true)] 
        // public DataBlockOverworldEventSubcheckInt partsLost;
        
        [DropdownReference (true)]
        public DataBlockScenarioSubcheckUnitRelativeTransform relativeTransform;
        
        [DropdownReference]
        [DictionaryKeyDropdown ("@DataMultiLinkerUnitBlueprint.data.Keys")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> blueprints;
        
        [DropdownReference]
        [DictionaryKeyDropdown ("@DataHelperUnitEquipment.GetClassTags ()")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> classes;
        
        [DropdownReference]
        [DictionaryKeyDropdown ("@DataMultiLinkerUnitStats.data.Keys")]
        // [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, DataBlockOverworldEventSubcheckFloat> stats;
        
        [DropdownReference]
        [DictionaryKeyDropdown ("@DataHelperStats.GetNormalizedCombatStatKeys")]
        // [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, DataBlockOverworldEventSubcheckFloat> statsNormalized;
        
        [DropdownReference]
        [DictionaryKeyDropdown ("@DataMultiLinkerAction.data.Keys")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> actionsInProgress;

        [DropdownReference]
        public List<ICombatUnitValidationFunction> functions;

        #if !PB_MODSDK

        private static List<CombatEntity> unitTargetsFiltered = new List<CombatEntity> ();
        private static List<CombatEntity> unitTargetsSelected = new List<CombatEntity> ();
        private static List<int> unitTargetsSelectedIndexes = new List<int> ();

        public List<CombatEntity> FilterExternalUnits
        (
            List<CombatEntity> unitTargetsExternal,
            TargetSortMode sort,
            int limit, 
            bool repeats = true,
            Vector3 originPosition = default,
            Vector3 originDirection = default
        )
        {
            if (originDirection.sqrMagnitude.RoughlyEqual (0f))
                originDirection = Vector3.forward;

            originPositionCached = originPosition;
            originDirectionCached = originDirection;
            
            unitTargetsFiltered.Clear ();
            unitTargetsFiltered.AddRange (unitTargetsExternal);
            unitTargetsSelected.Clear ();

            // Return empty collection if the filter is null or empty
            int filteredCount = unitTargetsExternal.Count;
            if (filteredCount == 0)
                return unitTargetsSelected;

            // Return all filtered units if there is no limit on returned number is under it
            if (limit <= 0 || limit >= filteredCount)
            {
                unitTargetsSelected.AddRange (unitTargetsFiltered);
                return unitTargetsSelected;
            }

            // Sort units
            if (sort == TargetSortMode.None)
            {
                unitTargetsSelectedIndexes.Clear ();
                for (int i = 0; i < unitTargetsFiltered.Count; ++i)
                    unitTargetsSelectedIndexes.Add (i);
                
                for (int u = 0; u < limit; ++u)
                {
                    var indexRandom = unitTargetsSelectedIndexes.GetRandomEntry ();
                    unitTargetsSelected.Add (unitTargetsFiltered[indexRandom]);

                    if (!repeats)
                    {
                        unitTargetsSelectedIndexes.RemoveAt (indexRandom);
                        if (unitTargetsSelectedIndexes.Count == 0)
                            break;
                    }
                }
            }
            else
            {
                if (sort == TargetSortMode.Distance)
                    unitTargetsFiltered.Sort (CompareUnitsByDistance);
                else if (sort == TargetSortMode.DistanceInv)
                    unitTargetsFiltered.Sort (CompareUnitsByDistanceInv);
                else if (sort == TargetSortMode.Dot)
                    unitTargetsFiltered.Sort (CompareUnitsByDot);
                else if (sort == TargetSortMode.DotInv)
                    unitTargetsFiltered.Sort (CompareUnitsByDotInv);

                // var report = unitTargetsFiltered.ToStringFormatted (true, toStringOverride: (x) => $"- {x.ToLog ()}: {Vector3.Magnitude (x.position.v - originPositionCached)}");
                // Debug.Log ($"Sorted targets ({unitTargetsFiltered.Count}) by {sort}:\n{report}");
                
                for (int u = 0; u < limit; ++u)
                {
                    int index = repeats ? 0 : u;
                    unitTargetsSelected.Add (unitTargetsFiltered[index]);
                }
            }

            return unitTargetsSelected;
        }
        
        public List<CombatEntity> GetFilteredUnitsWithLimit
        (
            TargetSortMode sort,
            int limit, 
            bool repeats = true,
            Vector3 originPosition = default,
            Vector3 originDirection = default,
            int desiredFactionOverride = -1
        )
        {
            // Debug.Log ($"Attempting to filter units | Limit: {limit} | Repeats allowed: {repeats}");
            GetFilteredUnits (originPosition, originDirection, desiredFactionOverride);
            
            unitTargetsSelected.Clear ();

            // Return empty collection if the filter is null or empty
            int filteredCount = unitTargetsFiltered.Count;
            if (filteredCount == 0)
                return unitTargetsSelected;

            // Return all filtered units if there is no limit on returned number is under it
            if (limit <= 0 || limit >= filteredCount)
                return unitTargetsFiltered;

            // Sort units
            if (sort == TargetSortMode.None)
            {
                unitTargetsSelectedIndexes.Clear ();
                for (int i = 0; i < unitTargetsFiltered.Count; ++i)
                    unitTargetsSelectedIndexes.Add (i);
                
                for (int u = 0; u < limit; ++u)
                {
                    int indexOfIndex = UnityEngine.Random.Range (0, unitTargetsSelectedIndexes.Count);
                    var indexOfUnit = unitTargetsSelectedIndexes[indexOfIndex];
                    unitTargetsSelected.Add (unitTargetsFiltered[indexOfUnit]);

                    if (!repeats)
                    {
                        unitTargetsSelectedIndexes.RemoveAt (indexOfIndex);
                        if (unitTargetsSelectedIndexes.Count == 0)
                            break;
                    }
                }
            }
            else
            {
                if (sort == TargetSortMode.Distance)
                    unitTargetsFiltered.Sort (CompareUnitsByDistance);
                else if (sort == TargetSortMode.DistanceInv)
                    unitTargetsFiltered.Sort (CompareUnitsByDistanceInv);
                else if (sort == TargetSortMode.Dot)
                    unitTargetsFiltered.Sort (CompareUnitsByDot);
                else if (sort == TargetSortMode.DotInv)
                    unitTargetsFiltered.Sort (CompareUnitsByDotInv);

                // var report = unitTargetsFiltered.ToStringFormatted (true, toStringOverride: (x) => $"- {x.ToLog ()}: {Vector3.Magnitude (x.position.v - originPositionCached)}");
                // Debug.Log ($"Sorted targets ({unitTargetsFiltered.Count}) by {sort}:\n{report}");
                
                for (int u = 0; u < limit; ++u)
                {
                    int index = repeats ? 0 : u;
                    unitTargetsSelected.Add (unitTargetsFiltered[index]);
                }
            }

            return unitTargetsSelected;
        }

        private static Vector3 originPositionCached = default;
        private static Vector3 originDirectionCached = default;
        
        private int CompareUnitsByDistanceInv (CombatEntity a, CombatEntity b)
        {
            int comp = CompareUnitsByDistance (a, b);
            return -comp;
        }

        private int CompareUnitsByDistance (CombatEntity a, CombatEntity b)
        {
            float aDistanceSqr = 0f;
            float bDistanceSqr = 0f;

            if (a != null && a.hasPosition)
                aDistanceSqr = Vector3.SqrMagnitude (a.position.v - originPositionCached);
            
            if (b != null && b.hasPosition)
                bDistanceSqr = Vector3.SqrMagnitude (b.position.v - originPositionCached);
            
            return aDistanceSqr.CompareTo (bDistanceSqr);
        }
        
        private int CompareUnitsByDotInv (CombatEntity a, CombatEntity b)
        {
            int comp = CompareUnitsByDot (a, b);
            return -comp;
        }
        
        private int CompareUnitsByDot (CombatEntity a, CombatEntity b)
        {
            float aDot = -1f;
            float bDot = -1f;

            if (a != null && a.hasPosition)
                aDot = Vector3.Dot ( (a.position.v - originPositionCached).normalized, originDirectionCached);

            if (b != null && b.hasPosition)
                bDot = Vector3.Dot ( (b.position.v - originPositionCached).normalized, originDirectionCached);
            
            return bDot.CompareTo (aDot);
        }
        
        public CombatEntity GetFilteredUnitRandom 
        (
            Vector3 originPosition = default,
            Vector3 originDirection = default,
            int desiredFactionOverride = -1
        )
        {
            GetFilteredUnits (originPosition, originDirection, desiredFactionOverride);

            if (unitTargetsFiltered == null || unitTargetsFiltered.Count == 0)
                return null;
            
            var unitTarget = unitTargetsFiltered.GetRandomEntry ();
            return unitTarget;
        }

        public List<CombatEntity> GetFilteredUnits 
        (
            Vector3 originPosition = default,
            Vector3 originDirection = default,
            int desiredFactionOverride = -1
        )
        {
            originPositionCached = originPosition;
            originDirectionCached = originDirection;
            
            unitTargetsFiltered.Clear ();
            if (IDUtility.IsGameState (GameStates.combat))
            {
                var unitParticipants = ScenarioUtility.GetCombatParticipantUnits ();
                // Debug.Log ($"Starting unit filtering | Participants: {unitParticipants.Count}");

                bool desiredFactionOverrideUsed = desiredFactionOverride >= 0;
                bool desiredFactionAllied = desiredFactionOverride == 1;

                foreach (var unitPersistent in unitParticipants)
                {
                    if (unitPersistent == null || unitPersistent.isHidden || !unitPersistent.isUnitDeployed || unitPersistent.isDestroyed || unitPersistent.isWrecked)
                        continue;

                    var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
                    if (unitCombat == null || !unitCombat.isUnitTag || unitCombat.isHidden || !unitCombat.hasPosition)
                        continue;

                    if (!ScenarioUtility.IsUnitMatchingCheck (unitPersistent, unitCombat, this, true, true, originPosition: originPosition, originDirection: originDirection))
                    {
                        // Debug.Log ($"- Skipping unit {unitPersistent.ToLog ()} mismatch with the check");
                        continue;
                    }

                    if (desiredFactionOverrideUsed)
                    {
                        bool allied = CombatUIUtility.IsUnitFriendly (unitCombat);
                        if (allied != desiredFactionAllied)
                            continue;
                    }

                    // Debug.Log ($"- Adding unit {unitPersistent.ToLog ()} to filtered list");
                    unitTargetsFiltered.Add (unitCombat);
                }
            }

            if (exportCountToMemory != null && !string.IsNullOrEmpty (exportCountToMemory.key))
            {
                var basePersistent = IDUtility.playerBasePersistent;
                basePersistent.SetMemoryFloat (exportCountToMemory.key, unitTargetsFiltered.Count);
            }
            
            if (exportEntitiesToBlackboard != null && !string.IsNullOrEmpty (exportEntitiesToBlackboard.key))
            {
                var blackboardKey = exportEntitiesToBlackboard.key;
                if (exportEntitiesToBlackboard.indexed)
                    ScenarioUtility.SaveEntitiesToGlobalBlackboard (blackboardKey, unitTargetsFiltered);
                else
                    ScenarioUtility.SaveEntityToGlobalBlackboard (blackboardKey, unitTargetsFiltered.FirstOrDefault ());
            }
            
            // Debug.Log ($"Filtered {unitTargetsFiltered.Count} units using a list of {unitParticipants.Count} candidates");
            return unitTargetsFiltered;
        }

        #endif

        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioSubcheckUnit () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        [ShowIf ("@state == null")]
        [YamlIgnore, ShowInInspector, HideLabel, DisplayAsString (TextAlignment.Left, Overflow = true, FontSize = 10), BoxGroup ("Info", false)]
        public const string stateLabel = "Implicit state check: true will be returned if the checked unit is active";

        [ShowIf ("@tags != null"), Button ("Insert tag filter"), PropertyOrder (-1)]
        private void InsertTagCheck ([HideLabel]string tag)
        {
            if (tags == null || string.IsNullOrEmpty (tag) || tags.ContainsKey (tag))
                return;
            
            tags.Add (tag, true);
        }

        #endif
        #endregion
    }

    public class DataBlockScenarioStateUI
    {
        public bool briefingHidden;
        public bool checkboxUsed;
        public bool progressInverted;
        public bool progressLimitHidden;
        public bool progressHiddenAtZero;
        
        [DropdownReference (true)]
        public DataBlockFloat01 moodIntensityNormal;
        
        [DropdownReference (true)]
        public DataBlockFloat01 moodIntensityProgress;
        
        [DropdownReference (true)]
        public DataBlockInt progressLimitOverride;
        
        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerOverworldMemory.data.Keys")]
        public string progressLimitMemory;
        
        [DropdownReference (true)]
        [DataEditor.SpriteNameAttribute (true, 32f)]
        public string progressSuffixSprite;
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioStateUI () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }

    public class DataBlockScenarioStateText
    {
        [InlineButtonClear]
        [ValueDropdown("@DataMultiLinkerScenario.textKeysStateHeader")]
        [LabelText ("Header Key")]
        public string textNameKey;
        
        [InlineButtonClear]
        [ValueDropdown("@DataMultiLinkerScenario.textKeysStateDescription")]
        [LabelText ("Description Key")]
        public string textDescKey;
    }
    
    // States are akin to popular concept of mission objectives, but not restricted to positive goals a player must accomplish
    // An "objective" is a bit loaded, implying that whether it's fulfilled is up to you and potentially having 3 states: incomplete, complete, failed
    // Rather than thinking in terms of objectives, we can think in terms of facts (or "states") - simple judgements about state of things
    // More things can fall under that umbrella that might not fall under the umbrella of "objective".

    // With a concept of "facts" or "states", we don't deal with ambiguities and define a simple question: "Is [...] true?"
    // Something like "enemy X is destroyed" can be a "completed" "objective", but things like "you failed to protect building Y" or
    // "Z time passed" or "action W was created on timeline" don't fit that term neatly. Framing all these as "facts" fits everything.

    [GUIColor ("@$value != null ? $value.GetStateColor () : Color.white")]
    public class DataBlockScenarioState
    {
        public Color GetStateColor ()
        {
            return Color.HSVToRGB (Mathf.Abs (0.6f - (float)priority / 12f) % 1f, 0.1f, 1f);
        }
        
        [DropdownReference (true), HideLabel] 
        public DataBlockComment comment;
        
        [ShowIf("visible")]
        [YamlIgnore]
        [LabelText("Header / Desc.")]
        [DisableIf("@!string.IsNullOrEmpty (textNameKey)")]
        public string textName;

        [ShowIf("visible")]
        [YamlIgnore]
        [HideLabel, TextArea(1, 10)]
        [DisableIf("@!string.IsNullOrEmpty (textDescKey)")]
        public string textDesc;

        [ShowIf("visible")]
        [InlineButton("ResetTextHeaderKey", "-", ShowIf = "@!string.IsNullOrEmpty (textNameKey)")]
        [ValueDropdown("@DataMultiLinkerScenario.textKeysStateHeader")]
        [OnValueChanged("ResolveText")]
        [LabelText("Header Key")]
        public string textNameKey = "generic_unknown_header";

        [ShowIf ("visible")]
        [InlineButton("ResetTextDescKey", "-", ShowIf = "@!string.IsNullOrEmpty (textDescKey)")]
        [ValueDropdown("@DataMultiLinkerScenario.textKeysStateDescription")]
        [OnValueChanged("ResolveText")]
        [LabelText("Description Key")]
        public string textDescKey = "generic_unknown_text";

        [DropdownReference (true)]
        public DataBlockScenarioStateText textOnCompletion;

        // Is this state actively checked, or does it only exist as a flag that's set manually?
        public bool evaluated = true;

        // Is this state visible in scenario UI
        public bool visible = true;

        // Does this state start in scope without requiring starting step to bring it in
        public bool startInScope = false;
        
        [FoldoutGroup ("Priorities")]
        [LabelText ("Gen. Priority")]
        public int priorityGeneration = 0;

        [FoldoutGroup ("Priorities")]
        [LabelText ("Eval. Priority")]
        public int priority = 0;
        
        [FoldoutGroup ("Priorities")]
        [ShowIf ("visible")]
        [LabelText ("Display Priority")]
        public int priorityDisplay = 0;

        [PropertyRange (-2, 1)]
        [ShowIf("visible")]
        public int mood = 0;
        
        [DropdownReference (true)]
        [LabelText ("UI")]
        public DataBlockScenarioStateUI ui;

        [ShowIf("IsEvaluatedFieldVisible")]
        public ScenarioStateRefreshContext evaluationContext = ScenarioStateRefreshContext.OnExecutionEnd;

        [InfoBox("@GetEvaluationOnOutcomeText ()", "@evaluationOnOutcome != null")]
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckBool evaluationOnOutcome;

        [ShowIf ("IsEvaluatedFieldVisible")]
        [DropdownReference (true)]
        public DataBlockScenarioSubcheckTurn turn;
        
        [ShowIf ("IsEvaluatedFieldVisible")]
        [DropdownReference (true)]
        public DataBlockScenarioSubcheckTurnModulus turnModulus;

        [ShowIf ("IsEvaluatedFieldVisible")]
        [DropdownReference]
        [ListDrawerSettings (DefaultExpandedState = true, AlwaysAddDefaultValue = true)]
        public List<DataBlockScenarioSubcheckUnitCounted> unitChecks;

        [ShowIf ("IsEvaluatedFieldVisible")]
        [DropdownReference (true)]
        public DataBlockScenarioSubcheckUnit unitCheckLinked;
        
        [DropdownReference (true)]
        public DataBlockScenarioStateLocation location;
        
        [GUIColor (1f, 0.945f, 0.929f)]
        [DropdownReference (true)]
        public DataBlockScenarioStateLocationRetreat locationRetreat;

        [ShowIf("IsEvaluatedFieldVisible")]
        [DropdownReference(true)]
        public DataBlockScenarioVolume volume;
        
        [ShowIf ("IsEvaluatedFieldVisible")]
        [DropdownReference (false)]
        [DictionaryKeyDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerScenario\", \"GetStateKeys\")")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> stateValues;

        [ShowIf ("IsEvaluatedFieldVisible")]
        [DropdownReference (true)]
        public DataBlockOverworldMemoryCheckGroup memoryBase;
        
        [ShowIf ("IsEvaluatedFieldVisible")]
        [DropdownReference]
        public List<ICombatStateValidationFunction> functions;
        
        // [DropdownReference (false)]
        // [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerScenario\", \"GetStateKeys\")")]
        // public HashSet<string> stateScopeConflicts;

        [DropdownReference (true)]
        public DataBlockScenarioStateReactions reactions;  

        [YamlIgnore, HideInInspector, NonSerialized] 
        public DataContainerScenario parentScenario;
        
        [HideInInspector, YamlIgnore] 
        public string key;
        
        public void ResolveText ()
        {
            if (!visible)
                return;
            
            if (parentScenario == null || string.IsNullOrEmpty (key))
            {
                Debug.LogWarning ($"Failed to resolve text for scenario state | Scenario: {(parentScenario != null ? parentScenario.key : "null")} | Key: {key}");
                return;
            }

            var scenarioKey = parentScenario.key;
            var headerShared = !string.IsNullOrEmpty (textNameKey);
            var headerSector = headerShared ? TextLibs.scenarioStatesShared : TextLibs.scenarioEmbedded;
            var headerKey = headerShared ? textNameKey : $"{scenarioKey}__state_{key}_0c_header";
                    
            var descShared = !string.IsNullOrEmpty (textDescKey);
            var descSector = descShared ? TextLibs.scenarioStatesShared : TextLibs.scenarioEmbedded;
            var descKey = descShared ? textDescKey : $"{scenarioKey}__state_{key}_0c_text";

            textName = DataManagerText.GetText (headerSector, headerKey, true);
            textDesc = DataManagerText.GetText (descSector, descKey, true);
        }

        #region Editor
        #if UNITY_EDITOR

        [ShowIf ("evaluated")]
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioState () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        private bool IsEvaluatedFieldVisible => evaluated;
        
        private void ResetTextHeaderKey ()
        {
            textNameKey = string.Empty;
            ResolveText ();
        }
        
        private void ResetTextDescKey ()
        {
            textDescKey = string.Empty;
            ResolveText ();
        }

        private string GetEvaluationOnOutcomeText ()
        {
            if (evaluationOnOutcome == null)
                return null;

            if (evaluationOnOutcome.present)
                return "This state will only be evaluated on end of combat. It will not be evaluated during standard updates, no matter the context.";
            else
                return "This state will not be evaluated once combat outcome is decided. It will only be evaluated during standard updates.";
        }

        #endif

        #endregion
    }

    [LabelWidth (220f)]
    public class DataBlockScenarioStateReactions
    {
        [PropertyTooltip ("What value should the state have to trigger effects below")]
        public bool expectedValue = true;
        
        [PropertyTooltip ("Whether the state should be removed from scope once number of triggers reaches the limit below")]
        public bool scopeRemovalOnLimit = true;

        [Min (1)]
        [PropertyTooltip ("How many times the state can trigger effects upon reaching expected value")]
        public int triggerLimit = 1;

        [PropertyTooltip ("Whether triggering this state increments the trigger counter (disabling this allows re-run of reactions)")]
        public bool triggerIncrement = true;
        
        [HideIf ("SimpleDisplayPossible")]
        [InfoBox ("$effectsWarningString", InfoMessageType.Warning, "IsEffectsWarningVisible")]
        [PropertyTooltip ("Optional effects triggered when state gets expected value. Keyed by number of times state reached that value.")]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public Dictionary<int, DataBlockScenarioStateReaction> effectsPerIncrement = new Dictionary<int, DataBlockScenarioStateReaction>
        {
            { 1, new DataBlockScenarioStateReaction () }
        };
        
        #region Editor
        #if UNITY_EDITOR

        [HideDuplicateReferenceBox]
        [HideLabel]
        [ShowIf ("SimpleDisplayPossible")]
        [ShowInInspector, BoxGroup ("EffectSimple", false, HideWhenChildrenAreInvisible = true)]
        private DataBlockScenarioStateReaction effectPerIncrementFirst
        {
            get
            {
                int index = triggerIncrement ? 1 : 0;
                if (effectsPerIncrement != null && effectsPerIncrement.TryGetValue (index, out var entry))
                    return entry;
                return null;
            }
            set
            { 
                int index = triggerIncrement ? 1 : 0;
                effectsPerIncrement[index] = value;
            }
        }

        private string effectsWarningString = string.Empty;

        private bool IsEffectsWarningVisible ()
        {
            if (effectsPerIncrement == null || effectsPerIncrement.Count == 0)
                return false;

            if (triggerIncrement)
            {
                foreach (var kvp in effectsPerIncrement)
                {
                    if (kvp.Key < 1)
                    {
                        effectsWarningString = "The key in this collection should be \"1\" or higher - it is the number of times state reached expected value, and if that's 0, there was never an opportunity to trigger anything";
                        return true;
                    }

                    if (kvp.Key > triggerLimit)
                    {
                        effectsWarningString = "The key in this collection should be equal or lower to \"trigger limit\" - once the limit is reached, no effects are triggered further";
                        return true;
                    }
                }
            }
            else
            {
                if (effectsPerIncrement.Count != 1 || !effectsPerIncrement.ContainsKey (0))
                {
                    effectsWarningString = "The key in this collection should be \"0\": when trigger count incrementing is off, the only reachable reaction would be one that's attached to zero";
                    return true;
                }
            }

            return false;
        }

        private bool SimpleDisplayPossible ()
        {
            if (effectsPerIncrement == null || effectsPerIncrement.Count != 1)
                return false;

            if (triggerIncrement)
            {
                if (triggerLimit != 1 || !effectsPerIncrement.ContainsKey (1))
                    return false;
            }
            else
            {
                if (!effectsPerIncrement.ContainsKey (0))
                    return false;
            }

            return true;
        }

        [ShowIf ("@SimpleDisplayPossible () && triggerIncrement"), BoxGroup ("EffectSimple", false, HideWhenChildrenAreInvisible = true)]
        [Button ("Effects Per Trigger")]
        private void ExpandPerIncrement ()
        {
            int index = 1;
            if (effectsPerIncrement == null)
                effectsPerIncrement = new Dictionary<int, DataBlockScenarioStateReaction> ();
            while (effectsPerIncrement.ContainsKey (index))
                index += 1;
            effectsPerIncrement[index] = new DataBlockScenarioStateReaction ();
            triggerLimit = index;
        }

        #endif

        #endregion
    }

    public class DataBlockScenarioStateOutcomeMask
    {
        public bool outcomeVictory;
        public bool outcomeDefeat;
        public bool caseEarly;
        public bool caseTotal;
    }
    
    public class DataBlockScenarioTransitionFromState
    {
        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerScenario\", \"GetStepKeys\")")]
        [HideLabel]
        public string stepKey;
    }

    [HideReferenceObjectPicker]
    public class DataBlockScenarioStateReaction
    {
        [DropdownReference]
        [ValueDropdown ("@FieldReflectionUtility.GetConstantStringFieldValues (typeof (ScenarioBlockTags), false)")]
        public HashSet<string> tags;
        
        [DropdownReference (true)]
        public DataBlockScenarioStateOutcomeMask executionOnOutcome;
        
        [DropdownReference (true)]
        public DataBlockScenarioTransitionFromState stepTransition;
        
        [DropdownReference]
        public List<DataBlockScenarioCommLink> commsOnStart;
        
        [DropdownReference]
        public List<DataBlockMemoryChangeGroupScenario> memoryChanges;
        
        [DropdownReference]
        [DictionaryKeyDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerScenario\", \"GetStateKeys\")")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> stateScopeChanges;

        [DropdownReference]
        [DictionaryKeyDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerScenario\", \"GetStateKeys\")")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> stateValueChanges;
        
        [DropdownReference]
        [DictionaryKeyDropdown ("@DataMultiLinkerOverworldMemory.data.Keys")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> memoryDisplayChanges;
        
        [DropdownReference]
        [DictionaryKeyDropdown ("@DataShortcuts.sim.scenarioUnitTags")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> unitTagDisplayChanges;

        [DropdownReference]
        [DictionaryKeyDropdown ("@DataShortcuts.sim.scenarioRewardKeys")]
        public Dictionary<string, int> rewards;
        
        [DropdownReference]
        #if !PB_MODSDK
        [ValueDropdown ("GetCallKeys")]
        #endif
        public List<string> callsImmediate;
        
        [DropdownReference]
        #if !PB_MODSDK
        [ValueDropdown ("GetCallKeys")]
        #endif
        public List<string> callsDelayed;
        
        [DropdownReference]
        public List<ICombatFunction> functions;
        
        [DropdownReference]
        public DataBlockScenarioFunctionsPerUnit functionsPerUnit;
        
        [DropdownReference]
        [OnValueChanged ("UpdateUnitGroups", true)]
        public List<DataBlockScenarioUnitGroup> unitGroups;
        
        [DropdownReference (true)]
        public DataBlockScenarioOutcome outcome;

        [ShowIf ("AreCallsPresent")]
        public bool callsDelayedOutcomeCheck = false;
        
        [ShowIf ("@AreCallsPresent && callsDelayedOutcomeCheck")]
        public CombatOutcome callsDelayedOutcomeRequired = CombatOutcome.Victory;
        
        public void UpdateUnitGroups ()
        {
            if (unitGroups != null)
            {
                for (int i = 0; i < unitGroups.Count; ++i)
                {
                    var g = unitGroups[i];
                    g.origin = $"reaction group {i}";
                    g.OnAfterDeserialization ();
                }
            }
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioStateReaction () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #if !PB_MODSDK
        private IEnumerable<string> GetCallKeys => 
            FieldReflectionUtility.GetConstantStringFieldValues (typeof (ScenarioStateFunctionKeys));
        #endif

        private bool AreCallsPresent => callsDelayed != null && callsDelayed.Count > 0;
        
        [GUIColor (1f, 0.75f, 0.5f), ShowIf ("IsCommsUpdateAvailable")]
        [Button ("Update comms", ButtonSizes.Medium), PropertyOrder (-1)]
        private void UpdateUnlocalizedComms (string key)
        {
            DataMultiLinkerUnitComposite.UpdateUnlocalizedCommsInFunctions (functions, key);
        }

        private bool IsCommsUpdateAvailable ()
        {
            if (functions != null)
            {
                foreach (var fn in functions)
                {
                    if (fn != null && fn is CombatLogMessageUnlocalized fnMessage)
                        return true;
                }
            }
            return false;
        }

        #endif

        #endregion
    }

    public enum ScenarioTransitionEvaluation
    {
        OnStateChange,
        OnExecutionStart,
        OnExecutionEnd,
        OnRealTime
    }

    public class DataBlockScenarioStepCore
    {
        [HideLabel, ColorUsage (false)]
        public Color color = Color.white;

        [YamlIgnore]
        [LabelText ("Header / Desc.")]
        [DisableIf ("@textFromScenarioGroup || !string.IsNullOrEmpty (textCurrentPrimaryKey)")]
        public string textHeader;
        
        [YamlIgnore]
        [HideLabel, TextArea (1, 10)]
        [DisableIf ("@textFromScenarioGroup || !string.IsNullOrEmpty (textCurrentSecondaryKey)")]
        public string textDesc;

        [HideIf ("textFromScenarioGroup")]
        [InlineButton ("ResetTextHeaderKey", "-", ShowIf = "@!string.IsNullOrEmpty (textCurrentPrimaryKey)")]
        [ValueDropdown ("@DataMultiLinkerScenario.textKeysStepHeader")]
        [OnValueChanged ("ResolveText")]
        [LabelText ("Header Key")]
        public string textCurrentPrimaryKey = "generic_unknown_header";

        [HideIf ("textFromScenarioGroup")]
        [InlineButton ("ResetTextDescKey", "-", ShowIf = "@!string.IsNullOrEmpty (textCurrentSecondaryKey)")]
        [ValueDropdown ("@DataMultiLinkerScenario.textKeysStepDescription")]
        [OnValueChanged ("ResolveText")]
        [LabelText ("Description Key")]
        public string textCurrentSecondaryKey = "generic_unknown_text";

        [OnValueChanged ("ResolveText")]
        public bool textFromScenarioGroup = false;
        
        [LabelText ("Threat Rating %")]
        public float threatRatingPercentage = 1;
        public bool allowOutcomeVictory = true;
        public bool allowOutcomeDefeat = true;
        public bool unitsRevealed = false;
        public bool executionAllowed = false;
        public bool visible = true;
        
        [YamlIgnore, HideInInspector, NonSerialized] 
        public DataBlockScenarioStep parentStep;
        
        private void ResetTextHeaderKey ()
        {
            textCurrentPrimaryKey = string.Empty;
            ResolveText ();
        }
        
        private void ResetTextDescKey ()
        {
            textCurrentSecondaryKey = string.Empty;
            ResolveText ();
        }
        
        public void ResolveText ()
        {
            if (parentStep == null || parentStep.parentScenario == null || string.IsNullOrEmpty (parentStep.key))
            {
                Debug.LogWarning ($"Failed to resolve text for scenario step | Parent step: {parentStep.ToStringNullCheck ()} | Parent scenario: {parentStep?.parentScenario.ToStringNullCheck ()} | Key: {parentStep?.key}");
                return;
            }
            
            if (textFromScenarioGroup)
            {
                textHeader = string.Empty;
                textDesc = string.Empty;
                
                var groupKeys = parentStep?.parentScenario?.groupKeys;
                if (groupKeys != null && groupKeys.Count > 0)
                {
                    var groupKeyMain = groupKeys[0];
                    var group = DataMultiLinkerScenarioGroup.GetEntry (groupKeyMain, false);
                    if (group != null && !group.hidden)
                    {
                        textHeader = group.textName;
                        textDesc = group.textDesc;
                    }
                }
            }
            else
            {
                var stepKey = parentStep.key;
                var scenarioKey = parentStep.parentScenario.key;
                var headerShared = !string.IsNullOrEmpty (textCurrentPrimaryKey);
                var headerSector = headerShared ? TextLibs.scenarioStepsShared : TextLibs.scenarioEmbedded;
                var headerKey = headerShared ? textCurrentPrimaryKey : $"{scenarioKey}__step_{stepKey}_0c_header";
                    
                var descShared = !string.IsNullOrEmpty (textCurrentSecondaryKey);
                var descSector = descShared ? TextLibs.scenarioStepsShared : TextLibs.scenarioEmbedded;
                var descKey = descShared ? textCurrentSecondaryKey : $"{scenarioKey}__step_{stepKey}_0c_text";

                textHeader = DataManagerText.GetText (headerSector, headerKey, true);
                textDesc = DataManagerText.GetText (descSector, descKey, true);
            }
        }
    }
    
    public class DataBlockScenarioStepTransitions
    {
        [PropertySpace (4f)]
        public ScenarioTransitionEvaluation transitionMode = ScenarioTransitionEvaluation.OnExecutionEnd;
        
        [ShowIf ("AreStandardTransitionsVisible")]
        public bool transitionBasedScope = true;
        
        [ShowIf ("AreStandardTransitionsVisible")]
        public bool transitionLocksExecution = false;
        
        [ShowIf ("AreStandardTransitionsVisible")]
        [LabelText ("Transitions")]
        public List<DataBlockScenarioTransitionStateBased> transitionsStateBased;
        
        [HideIf ("AreStandardTransitionsVisible")]
        [LabelText ("Transition")]
        public DataBlockScenarioTransitionTimeBased transitionTimeBased;
        
        private bool AreStandardTransitionsVisible => transitionMode != ScenarioTransitionEvaluation.OnRealTime;
    }
    
    [LabelWidth (220f)]
    public class DataBlockScenarioStep
    {
        [DropdownReference (true), HideLabel] 
        public DataBlockComment comment;

        [DropdownReference (true)]
        public DataBlockScenarioStepCore core;
        
        [DropdownReference]
        [ValueDropdown ("@FieldReflectionUtility.GetConstantStringFieldValues (typeof (ScenarioBlockTags), false)")]
        public HashSet<string> tags;

        [ShowIf ("AreStepBlocksVisible")]
        [DropdownReference]
        [ListDrawerSettings (AlwaysAddDefaultValue = true, DefaultExpandedState = false)]
        [OnValueChanged ("UpdateUnitGroups", true)]
        public List<DataBlockScenarioUnitGroup> unitGroups;
        
        [ShowIf ("AreStepBlocksVisible")]
        [DropdownReference]
        [DictionaryKeyDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerScenario\", \"GetStateKeys\")")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> stateScopeChanges;
        
        [ShowIf ("AreStepBlocksVisible")]
        [DropdownReference]
        [DictionaryKeyDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerScenario\", \"GetStateKeys\")")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> stateValueChanges;
        
        [DropdownReference]
        [DictionaryKeyDropdown ("@DataMultiLinkerOverworldMemory.data.Keys")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> memoryDisplayChanges;
        
        [DropdownReference]
        [DictionaryKeyDropdown ("@DataShortcuts.sim.scenarioUnitTags")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> unitTagDisplayChanges;
        
        [ShowIf ("AreStepBlocksVisible")]
        [DropdownReference]
        public List<DataBlockScenarioUnitChangeChecked> unitChanges;
        
        [ShowIf ("AreStepBlocksVisible")]
        [DropdownReference (true)]
        public DataBlockScenarioRetreat retreat;
        
        [ShowIf ("AreStepBlocksVisible")]
        [DropdownReference (true)]
        public DataBlockScenarioActionRestrictions actionRestrictions;

        [ShowIf ("AreStepBlocksVisible")]
        [PropertySpace (4f)]
        [DropdownReference (true)]
        public DataBlockScenarioCutsceneVideo cutsceneVideoOnStart;
        
        [ShowIf ("AreStepBlocksVisible")]
        [DropdownReference (true)]
        public DataBlockScenarioAtmosphere atmosphereOnStart;

        [ShowIf ("AreStepBlocksVisible")]
        [DropdownReference]
        public List<DataBlockScenarioHintConditional> hintsConditional;

        [ShowIf ("AreStepBlocksVisible")]
        [DropdownReference (true)]
        public DataBlockScenarioUnitSelection unitSelection;
        
        [ShowIf ("AreStepBlocksVisible")]
        [DropdownReference (true)]
        public DataBlockScenarioCamera camera;

        [ShowIf ("AreStepBlocksVisible")]
        [DropdownReference (true)]
        public DataBlockScenarioMusicMood musicMood;
        
        [ShowIf ("AreStepBlocksVisible")]
        [DropdownReference (true)]
        public DataBlockSavedFloat musicIntensity;

        [ShowIf ("AreStepBlocksVisible")]
        [DropdownReference (true)]
        public DataBlockScenarioReactiveMusic musicReactive;

        [ShowIf ("AreStepBlocksVisible")]
        [PropertySpace (4f)]
        [DropdownReference]
        public List<DataBlockScenarioCommLink> commsOnStart;
        
        [ShowIf ("AreStepBlocksVisible")]
        [DropdownReference]
        public List<DataBlockScenarioAudioEvent> audioEventsOnStart;
        
        [ShowIf ("AreStepBlocksVisible")]
        [DropdownReference]
        public List<DataBlockScenarioAudioState> audioStatesOnStart;
        
        [ShowIf ("AreStepBlocksVisible")]
        [DropdownReference]
        public List<DataBlockScenarioAudioSync> audioSyncsOnStart;

        [ShowIf ("AreStepBlocksVisible")]
        [DropdownReference]
        public List<ICombatFunction> functions;
        
        [ShowIf ("AreStepBlocksVisible")]
        [DropdownReference (true)]
        public DataBlockScenarioOutcome outcome;

        [DropdownReference (true)]
        public DataBlockScenarioStepTransitions transitions;


        [YamlIgnore, HideInInspector, NonSerialized] 
        public DataContainerScenario parentScenario;

        [HideInInspector, YamlIgnore] 
        public string key;
        
        private const string boxGroupStats = "stats";
        private const string boxGroupCore = "core";
        private const string boxGroupTransition = "transition";
        
        public void UpdateUnitGroups ()
        {
            if (unitGroups != null)
            {
                for (int i = 0; i < unitGroups.Count; ++i)
                {
                    var g = unitGroups[i];
                    if (g == null)
                        continue;
                    
                    g.origin = $"step {key} group {i}";
                    g.OnAfterDeserialization ();
                }
            }
        }

        #region Editor
        #if UNITY_EDITOR

        // [FoldoutReferenceButton ("check")]
        // private void ToggleCheck () => DataEditor.ToggleReferenceField (ref check);
        
        private bool AreStepBlocksVisible  => DataMultiLinkerScenario.Presentation.showStepBlocks;
        private bool AreStatsVisible => DataMultiLinkerScenario.Presentation.showStats;

        [ShowInInspector, ShowIf ("AreStepBlocksVisible")]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioStep () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    [Serializable, HideReferenceObjectPicker, LabelWidth (200f)]
    public class DataBlockScenarioUnitCustomization
    {
        [DropdownReference (true)]
        [LabelText ("Unit Name (Internal)")]
        public DataBlockScenarioUnitCustomName name;
    
        [DropdownReference (true)]
        [LabelText ("Unit Name (Visible)")]
        public DataBlockScenarioUnitCustomID id;
        
        [DropdownReference (true)]
        [LabelText ("Unit Name (Visible, Reused)")]
        public DataBlockLocString idReused;
        
        [DropdownReference (true)]
        [LabelText ("Pilot Name (Visible)")]
        public DataBlockScenarioPilotCustomID idPilot;

        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerUnitRole.data.Keys")]
        public string role;

        [DropdownReference (true)]
        public DataBlockScenarioUnitCustomSpawn spawn;
        
        [DropdownReference (true)]
        public DataBlockScenarioUnitCustomLanding landing;
        
        [DropdownReference (true)]
        public DataBlockScenarioUnitCustomFlags flags;
        
        [DropdownReference (true)]
        public DataBlockScenarioUnitCustomFaction faction;
        
        [DropdownReference (true)]
        public DataBlockScenarioUnitCustomSpeed speed; 
        
        [DropdownReference (true)]
        public DataBlockScenarioUnitUncrewed uncrewed;
        
        [DropdownReference (true)]
        public DataBlockScenarioPilotAppearance pilotAppearance;
        
        [DropdownReference (true)]
        public DataBlockScenarioPilotStats pilotStats;
        
        [DropdownReference (true)]
        public DataBlockUnitPredictionLimit predictionLimit;

        [DropdownReference (true)]
        public DataBlockUnitAnimationOverrides animationOverrides;

        [DropdownReference]
        [DictionaryKeyDropdown ("@DataMultiLinkerUnitStats.data.Keys")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, float> statMultipliers;
        
        [DropdownReference]
        public SortedDictionary<string, float> hitDirectionModifiers;

        [DropdownReference]
        public List<ICombatFunctionTargeted> functions;

        [DropdownReference]
        [ValueDropdown ("@DataShortcuts.sim.scenarioUnitTags")]
        public HashSet<string> combatTags;

        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioUnitCustomization () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
    
    [Serializable]
    public class DataBlockUnitPredictionLimit
    {
        [PropertyRange (0f, 5f)]
        public float limit;
    }
    
    [Serializable]
    public class DataBlockScenarioUnitCustomName
    {
        public string name;
    }
    
    [Serializable, HideReferenceObjectPicker, LabelWidth (180f)]
    public class DataBlockScenarioUnitResolver
    {
        [YamlIgnore, HideInInspector]
        public DataContainerScenario parentScenario;
        
        [YamlIgnore, HideInInspector]
        public DataContainerCombatUnitGroup parentContainer;
        
        [HorizontalGroup ("KeyGroup", 16f)]
        [ToggleLeft, HideLabel, PropertyTooltip ("Skip the unit preset resolvers on this config and use this key to resolve a unit preset directly from the global unit presets database.")]
        public bool keyExternal = false;
        
        [GUIColor ("GetColor")]
        [HorizontalGroup ("KeyGroup")]
        [ValueDropdown ("GetUnitKeys")]
        [SuffixLabel ("$GetKeyHint")]
        [HideLabel]
        public string key;
        
        [HorizontalGroup ("A", 0.6f)]
        [LabelText ("Count")]
        [OnValueChanged ("ValidateCountMin")]
        public int countMin = 1;
        
        [HorizontalGroup ("A")]
        [ShowIf ("countRandom")]
        [HideLabel]
        [HideInInspector]
        [OnValueChanged ("ValidateCountMax")]
        public int countMax = 1;
        
        [HorizontalGroup ("A", 18f)]
        [HideLabel]
        [HideInInspector]
        public bool countRandom = false;
        
        [HorizontalGroup ("B", 0.6f)]
        [LabelText ("Level Offset")]
        public int levelOffsetMin = 0;
        
        [HorizontalGroup ("B")]
        [ShowIf ("levelOffsetRandom")]
        [HideLabel]
        [HideInInspector]
        public int levelOffsetMax = 0;
        
        [HorizontalGroup ("B", 18f)]
        [HideLabel]
        [HideInInspector]
        public bool levelOffsetRandom = false;

        [DropdownReference]
        [GUIColor ("GetQualityTableColor")]
        [ValueDropdown ("GetQualityTableKeys")]
        public string qualityTableKey;
        
        [DropdownReference]
        [LabelText ("AI Behavior")]
        [ValueDropdown ("GetAIBehaviorKeys")]
        public string aiBehavior = string.Empty;
        
        [DropdownReference]
        [LabelText ("AI Targeting")]
        [ValueDropdown ("GetAITargetingProfileKeys")]
        public string aiTargeting = string.Empty;
        
        [DropdownReference]
        [ValueDropdown ("@DataMultiLinkerUnitLiveryPreset.data.Keys")]
        // [HorizontalGroup ("C"), OnValueChanged ("RefreshLiveryPresetPreview")]
        public string liveryPreset;
        
        /*
        [ShowIf ("@liveryPresetPreview != null")]
        [NonSerialized, YamlIgnore, ShowInInspector, HideLabel, HorizontalGroup ("C", EquipmentLiveryPreview.width)]
        [InlineButton ("RemoveLiveryPreset", "-")]
        public EquipmentLiveryPreview liveryPresetPreview;
        */
        
        [DropdownReference (true)]
        public DataBlockScenarioUnitCustomization custom;

        [ShowInInspector, FoldoutGroup ("Preview"), ShowIf ("keyExternal")]
        [HideLabel, HideReferenceObjectPicker, HideDuplicateReferenceBox]
        public DataContainerUnitPreset presetPreview
        {
            get
            {
                var preset = DataMultiLinkerUnitPreset.GetEntry (key, false);
                return preset;
            }
        }
        
        public void OnAfterDeserialization ()
        {
            RefreshLiveryPresetPreview ();
        }
        
        public void OnBeforeSerialization ()
        {

        }
        
        public void RefreshLiveryPresetPreview ()
        {
            /*
            bool used = false;
            if (!string.IsNullOrEmpty (liveryPreset))
            {
                var lp = DataMultiLinkerUnitLiveryPreset.GetEntry (liveryPreset, false);
                if (lp != null)
                {
                    var liveryRoot = DataMultiLinkerEquipmentLivery.GetEntry (lp.livery, false);
                    if (liveryRoot != null)
                    {
                        used = true;
                        if (liveryPresetPreview == null) 
                            liveryPresetPreview = new EquipmentLiveryPreview ();
                        liveryPresetPreview.Refresh (lp.livery);
                        liveryPresetPreview.showSelectButton = false;
                    }
                }
            }

            if (!used && liveryPresetPreview != null)
                liveryPresetPreview = null;
            */
        }

        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioUnitResolver () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        private IEnumerable<string> GetQualityTableKeys => DataMultiLinkerQualityTable.data.Keys;
        private Color GetQualityTableColor => !string.IsNullOrEmpty(qualityTableKey) && DataMultiLinkerQualityTable.data.TryGetValue(qualityTableKey, out var v) ? v.uiColor : Color.white;

        private void ValidateCountMin () => 
            countMin = Mathf.Clamp (countMin, 0, countRandom ? countMax : 9);
        
        private void ValidateCountMax () => 
            countMax = Mathf.Clamp (countMax, countMin, 9);

        private IEnumerable<string> GetAIBehaviorKeys () => 
            DataShortcuts.ai.unitBehaviors;
        
        private IEnumerable<string> GetAITargetingProfileKeys () => 
            DataMultiLinkerAITargetingProfile.data.Keys;

        private IEnumerable<string> GetUnitKeys ()
        {
            if (keyExternal)
                return DataMultiLinkerUnitPreset.GetKeys ();
            if (parentScenario != null && parentScenario.unitPresetsProc != null)
                return parentScenario.unitPresetsProc.Keys;
            if (parentContainer != null && parentContainer.unitPresets != null)
                return parentContainer.unitPresets.Keys;
            return null;
        }
        
        private bool IsKeyValid ()
        {
            bool stringValid = !string.IsNullOrEmpty (key);
            if (keyExternal)
            {
                var unitPreset = DataMultiLinkerUnitPreset.GetEntry (key, false);
                return unitPreset != null;
            }
            if (parentScenario != null)
                return stringValid && parentScenario.unitPresetsProc != null && parentScenario.unitPresetsProc.ContainsKey (key);
            if (parentContainer != null)
                return stringValid && parentContainer.unitPresets != null && parentContainer.unitPresets.ContainsKey (key);
            return false;
        }

        private string GetKeyHint => keyExternal ? "External preset" : "Internal resolver";

        private Color GetColor => 
            Color.HSVToRGB (0f, IsKeyValid () ? 0f : 0.5f, 1f);

        #endif
        #endregion
    }
    

    
    [Serializable]
    public class DataBlockScenarioUnitCustomSpawn
    {
        public int index = -1;
    }
    
    [Serializable]
    public class DataBlockScenarioUnitCustomLanding
    {
        public bool landingUsed = true;
        public DataBlockScenarioUnitCustomLandingData landingData;
    }
    
    [Serializable]
    public class DataBlockScenarioUnitCustomLandingData
    {
        public float duration = 3f;
        public Vector3 positionStart = new Vector3 (0f, 0f, 0f);
        public Vector3 rotationStart = new Vector3 (0f, 0f, 0f);
        public bool easing = true;
        public bool convertToAction = false;
    }
    
    [Serializable]
    public class DataBlockScenarioUnitCustomFlags
    {
        public bool controlByPlayer;
        public bool controlledByAI;
        public bool uncrewed;
    }
    
    [Serializable]
    public class DataBlockScenarioUnitCustomFaction
    {
        [ValueDropdown ("@DataHelperUnitEquipment.GetFactions ()")]
        public string faction;
    }

    [Serializable]
    public class DataBlockScenarioUnitCustomSpeed
    {
        public bool libraryUsed = true;
        
        [HideIf ("libraryUsed")]
        public float speed;
    }
    
    [Serializable]
    public class DataBlockScenarioUnitCustomID
    {
        [YamlIgnore]
        public string nameOverride;
    }
    
    [Serializable]
    public class DataBlockLocString
    {
        [PropertyOrder (-1)]
        [YamlIgnore, ShowInInspector, DisplayAsString (true), ReadOnly, BoxGroup, HideLabel]
        public string text => DataManagerText.GetText (textSector, textKey, true);
        
        [ValueDropdown ("@DataManagerText.GetLibrarySectorKeys ()")]
        public string textSector = "ui_combat";
        
        [ValueDropdown ("@DataManagerText.GetLibraryTextKeys (textSector)")]
        public string textKey;
    }
    
    [Serializable]
    public class DataBlockScenarioPilotAppearance
    {
        public string presetKey;
    }
    
    [Serializable]
    public class DataBlockScenarioUnitUncrewed
    {
        public string portraitKey;
    }
    
    [Serializable]
    public class DataBlockScenarioPilotStats
    {
        public int healthCurrent = 100;
        public int healthLimit = 100;
        public int concussionOffset = 20;
    }
    
    [Serializable]
    public class DataBlockScenarioPilotCustomID
    {
        [YamlIgnore]
        public string nameOverride;
        
        [YamlIgnore]
        public string callsignOverride;
    }
    
    
    [Serializable][LabelWidth (180f)]
    public class DataBlockScenarioUnit
    {
        // [ShowInInspector, LabelText ("Grade"), LabelWidth (120f)]
        // [ValueDropdown ("GetQualityKeys"), OnValueChanged ("RefreshQualityInteger"), HorizontalGroup ("A")]
        // public string qualityEditable;

        // [ReadOnly, HideLabel, HorizontalGroup ("A", 60f)]
        // public int quality;
        
        /*
        public void OnAfterDeserialization ()
        {
            RefreshQualityString ();
        }
        
        public void OnBeforeSerialization ()
        {
            RefreshQualityInteger ();
        }
        
        private void RefreshQualityInteger ()
        {
            for (int i = 0; i < UnitEquipmentQuality.text.Length; ++i)
            {
                var text = UnitEquipmentQuality.text[i];
                if (text == qualityEditable)
                {
                    quality = i;
                    return;
                }
            }

            quality = 0;
        }
        
        private void RefreshQualityString ()
        {
            var index = quality.IsValidIndex (UnitEquipmentQuality.text) ? quality : 0;
            qualityEditable = UnitEquipmentQuality.text[index];
        }
        
        #if UNITY_EDITOR
        
        private IEnumerable<string> GetQualityKeys => UnitEquipmentQuality.text;

        #endif
        */
    }
    
    [Serializable]
    public class DataBlockScenarioUnitFilter : DataBlockScenarioUnit
    {
        public bool tagsFromOverworld = true;
        
        [OnValueChanged ("RefreshWithoutBranch", true)]
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.UnitPresetTag)]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> tags = new SortedDictionary<string, bool> ();
        
        #if UNITY_EDITOR
        
        [GUIColor ("GetFilteredColor")]
        [LabelText ("Filtered Unit Presets")]
        [YamlIgnore, ListDrawerSettings (DefaultExpandedState = false, ShowPaging = false, DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        public List<DataContainerLink<DataContainerUnitPreset>> presetsFiltered;

        public void RefreshWithoutBranch () => Refresh (null);

        private static Color colorWarning = Color.HSVToRGB (0.1f, 0.4f, 1f);
        private static Color colorError = Color.HSVToRGB (0.0f, 0.6f, 1f);
        private static Color colorNeutral = Color.HSVToRGB (0.55f, 0.2f, 1f);

        private Color GetFilteredColor ()
        {
            if (presetsFiltered == null || presetsFiltered.Count == 0)
                return colorError;

            if (presetsFiltered.Count > 8)
                return colorWarning;

            return colorNeutral;
        }
        
        private static SortedDictionary<string, bool> unitGroupTagFilterCombined = new SortedDictionary<string, bool> ();
        private static List<DataContainerCombatUnitGroup> unitGroupsSelected = new List<DataContainerCombatUnitGroup> ();
        private static List<string> unitGroupBranchTagsRemoved = new List<string> ();
        private static string unitGroupBranchTagPrefix = "branch_";
        
        public void Refresh (string factionBranchDefault)
        {
            if (presetsFiltered == null)
                presetsFiltered = new List<DataContainerLink<DataContainerUnitPreset>> ();
            else
                presetsFiltered.Clear ();
                    
            if (tags == null || tags.Count == 0)
                return;

            bool tagsModifed = false;
            if (tagsFromOverworld && !string.IsNullOrEmpty (factionBranchDefault))
            {
                var fb = DataMultiLinkerOverworldFactionBranch.GetEntry (factionBranchDefault, false);
                if (fb != null && fb.unitPresetTags != null && fb.unitPresetTags.Count > 0)
                {
                    tagsModifed = true;
                    if (unitGroupTagFilterCombined == null)
                        unitGroupTagFilterCombined = new SortedDictionary<string, bool> ();
                    else
                        unitGroupTagFilterCombined.Clear ();
                    
                    foreach (var kvp2 in fb.unitPresetTags)
                        unitGroupTagFilterCombined.Add (kvp2.Key, kvp2.Value);
                    
                    foreach (var kvp2 in tags)
                    {
                        // Making sure there are no conflicting branch tags
                        if (kvp2.Key.StartsWith (unitGroupBranchTagPrefix))
                        {
                            unitGroupBranchTagsRemoved.Clear ();
                            foreach (var kvp3 in unitGroupTagFilterCombined)
                            {
                                if (kvp3.Key.StartsWith (unitGroupBranchTagPrefix))
                                    unitGroupBranchTagsRemoved.Add (kvp3.Key);
                            }

                            foreach (var branchTag in unitGroupBranchTagsRemoved)
                                unitGroupTagFilterCombined.Remove (branchTag);
                        }
                        
                        unitGroupTagFilterCombined[kvp2.Key] = kvp2.Value;
                    }
                }
            }

            var tagRequirementsUsed = tagsModifed ? unitGroupTagFilterCombined : tags;
            var unitPresetsWithTags = DataTagUtility.GetContainersWithTags (DataMultiLinkerUnitPreset.data, tagRequirementsUsed);
            foreach (var objectWithTags in unitPresetsWithTags)
            {
                var unitPreset = objectWithTags as DataContainerUnitPreset;
                if (unitPreset == null || unitPreset.hidden)
                    continue;
                
                var entry = new DataContainerLink<DataContainerUnitPreset> (unitPreset);
                presetsFiltered.Add (entry);
            }
        }

        #endif
    }
    
    [Serializable]
    public class DataBlockScenarioUnitPresetLink : DataBlockScenarioUnit
    {
        [ValueDropdown ("@DataMultiLinkerUnitPreset.data.Keys")]
        public string preset;
    }
    
    [Serializable]
    public class DataBlockScenarioUnitPresetEmbedded : DataBlockScenarioUnit
    {
        [HideLabel]
        public DataContainerUnitPreset preset = new DataContainerUnitPreset ();
    }

    [Serializable]
    public class DataBlockScenarioBriefingGroup
    {
        [ValueDropdown ("@DataMultiLinkerScenarioGroup.data.Keys")]
        public string key; 
        
        [PropertyRange (0, 50)]
        public int counter = 0;
    }
    
    [Serializable]
    public class DataBlockScenarioBriefingUnitCounts
    {
        public int groupsImmediate = 0;
        public int groupsDelayed = 0;
    }

    public class DataBlockScenarioCore
    {
        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerScenario\", \"GetStepKeys\")")]
        public string stepOnStart = string.Empty;
        
        public bool timeLocked = false;
        
        [ShowIf ("timeLocked")]
        public float time;

        public bool scalingUsed = true;
        public bool loadImmediately = false;
        
        public bool externalBranchUsed = true;
        public bool externalLevelUsed = true;
        
        public bool reinforcementsUsed = true;
        public bool replayUsed = true;
        public bool unitLossUsed = true;
        public bool lootingUsed = true;
        public bool musicDynamic = false;
        public bool introUsed = true;
        
        [DropdownReference (true)]
        public DataBlockScenarioMusicCustom musicCustom;
        
        [ShowIf ("introUsed")]
        [DropdownReference (true)]
        public DataBlockAreaIntro introOverride;
        
        [DropdownReference]
        public List<DataBlockScenarioSlot> briefingSpawnHighlights;
        
        [DropdownReference]
        public List<DataBlockScenarioBriefingGroup> briefingGroupsInjected;
        
        [DropdownReference]
        public DataBlockScenarioBriefingUnitCounts briefingUnitsInjected;
        
        [DropdownReference]
        [ValueDropdown ("@DataMultiLinkerAction.data.Keys")]
        public HashSet<string> actionBlocklist;
        
        [DropdownReference]
        public DataBlockScenarioExitBehaviour customExitBehaviour;
        
        [DropdownReference]
        public DataBlockFloat01 weatherOverridePrecipitation;
        
        [DropdownReference]
        public DataBlockFloat weatherOverrideTemperature;
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioCore () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    public class DataBlockScenarioMusicCustom
    {
        [ValueDropdown ("@FieldReflectionUtility.GetConstantStringFieldValues (typeof (AudioCombatMusicState), false)")]
        public string linearStateKey = string.Empty;
    }
    
    public class DataBlockScenarioGenerationInjection
    {
        [ToggleLeft]
        public bool enabled = true;
        public string group = "default";
        
        [DropdownReference]
        [DictionaryKeyDropdown ("@DataMultiLinkerScenario.tags")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> tagFilter = new SortedDictionary<string, bool> ();
        
        [DropdownReference]
        public List<IOverworldValidationFunction> functionsBase;
        
        [DropdownReference]
        public List<IOverworldValidationFunction> functionsSite;
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioGenerationInjection () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
    
    public class DataBlockScenarioAreaRequirements
    {
        [OnValueChanged ("RefreshPrediction")]
        [ShowIf ("tagFilterUsed")]
        public bool tagFilterFromSite = true;
        
        public bool tagFilterUsed = true;

        [OnValueChanged ("RefreshPrediction")]
        [ShowIf ("tagFilterUsed")]
        [DictionaryKeyDropdown ("@DataMultiLinkerCombatArea.tags")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> tagFilter = new SortedDictionary<string, bool> ();
        
        [HideIf ("tagFilterUsed")]
        [ValueDropdown ("@DataMultiLinkerCombatArea.data.Keys")]
        public List<string> keys = new List<string> ();
        
        #if UNITY_EDITOR

        [ShowInInspector, YamlIgnore, ReadOnly, ListDrawerSettings (DefaultExpandedState = true)]
        private List<string> listPreview = new List<string> ();

        public void RefreshPrediction ()
        {
            if (!tagFilterUsed || tagFilter == null || tagFilter.Count == 0)
                return;
            
            var list = ScenarioUtility.GetAreaKeysFromFilter (tagFilter);
            listPreview.Clear ();
            foreach (var key in list)
                listPreview.Add (key);
        }
        
        #endif
    }

    public class DataBlockScenarioEntry
    {
        public bool squadUsed = true;
        
        [ShowIf ("squadUsed")]
        public int squadSize = 4;
        
        public DataBlockScenarioSlot squadSlotCustom;
    }
    
    [HideReferenceObjectPicker]
    public class DataContainerScenarioParent
    {
        [GUIColor ("GetKeyColor")]
        [ValueDropdown ("@DataMultiLinkerScenario.data.Keys")]
        [SuffixLabel ("@hierarchyProperty"), HideLabel]
        public string key;

        [YamlIgnore, ReadOnly, HideInInspector]
        private string hierarchyProperty => DataMultiLinkerScenario.Presentation.showHierarchy ? hierarchy : string.Empty;
        
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

            bool present = DataMultiLinkerScenario.data.ContainsKey (key);
            return present ? colorNormal : colorError;
        }
        
        #endif
        #endregion
    }
    
    
    [Serializable][LabelWidth (180f)]
    public class DataContainerScenario : DataContainerWithText, IDataContainerTagged
    {
        [YamlIgnore, ReadOnly]
        [PropertyOrder (-5), ShowIf ("@IsTabCore && groupKeys != null && groupKeys.Count > 0")]
        [ListDrawerSettings (DefaultExpandedState = false)]
        [LabelText ("Groups (Filtering)")]
        public List<string> groupKeys;

        [Space (8f)]
        [PropertyOrder (-5), ShowIf ("IsTabCore")]
        public bool hidden = true;

        [PropertyOrder (-5), ShowIf ("IsTabCore")]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, CustomAddFunction = "@new DataContainerScenarioParent ()")]
        [DropdownReference]
        public List<DataContainerScenarioParent> parents = new List<DataContainerScenarioParent> ();
        
        [YamlIgnore, LabelText ("Children"), ReadOnly]
        [PropertyOrder (-5), ShowIf ("@IsTabCore && children != null && children.Count > 0")]
        [ListDrawerSettings (DefaultExpandedState = false)]
        public List<string> children = new List<string> ();

        [HideDuplicateReferenceBox]
        [PropertyOrder (-4), ShowIf ("IsTabCore")]
        [DropdownReference (true)]
        public DataBlockScenarioCore core;
        
        [HideDuplicateReferenceBox]
        [PropertyOrder (-4), ShowIf ("@IsTabCore && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockScenarioCore coreProc;
        
        
        [HideDuplicateReferenceBox]
        [PropertyOrder (-3), ShowIf ("IsTabCore")]
        [DropdownReference (true)]
        public DataBlockScenarioAreaRequirements areas;
        
        [HideDuplicateReferenceBox]
        [PropertyOrder (-3), ShowIf ("@IsTabCore && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockScenarioAreaRequirements areasProc;
        
        
        [HideDuplicateReferenceBox]
        [PropertyOrder (-3), ShowIf ("IsTabCore")]
        [DropdownReference (true)]
        public DataBlockScenarioEntry entry;
        
        [HideDuplicateReferenceBox]
        [PropertyOrder (-3), ShowIf ("@IsTabCore && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockScenarioEntry entryProc;
        
        [HideDuplicateReferenceBox]
        [PropertyOrder (-3), ShowIf ("IsTabCore")]
        [DropdownReference (true)]
        public DataBlockScenarioGenerationInjection generationInjection;
        
        
        [PropertyOrder (-2), BoxGroup ("Tags")]
        [ShowIf ("@IsTabCore && tags != null"), Button]
        private void InsertTag (string tag)
        {
            if (!string.IsNullOrEmpty (tag) && !tags.Contains (tag))
                tags.Add (tag);
            
            DataMultiLinkerScenario.ProcessRelated (this);
            UpdateGroups ();
        }
        
        [PropertyOrder (-2), BoxGroup ("Tags")]
        [ShowIf ("@IsTabCore && tags != null"), Button]
        private void InsertTagFromGroup ([ValueDropdown ("@DataMultiLinkerScenarioGroup.data.Keys")] string key)
        {
            var group = DataMultiLinkerScenarioGroup.GetEntry (key);
            if (group == null || group.tags == null)
                return;

            foreach (var kvp in group.tags)
            {
                var tag = kvp.Key;
                bool present = kvp.Value;
                
                if (present)
                {
                    if (!tags.Contains (tag))
                        tags.Add (tag);
                }
                else
                {
                    if (tags.Contains (tag))
                        tags.Remove (tag);
                }
            }
            
            DataMultiLinkerScenario.ProcessRelated (this);
            UpdateGroups ();
        }
        
        [PropertyOrder (-1), BoxGroup ("Tags")]
        [ShowIf ("IsTabCore")]
        [ValueDropdown ("@DataMultiLinkerScenario.tags")]
        [HideLabel]
        public HashSet<string> tags;
        
        [PropertyOrder (-1), BoxGroup ("Tags")]
        [ShowIf ("@IsTabCore && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        [HideLabel]
        public HashSet<string> tagsProc;

        
        [ShowIf ("@IsTabStates && states != null && stateKeysSortedGeneration != null && stateKeysSortedGeneration.Count > 0")]
        [FoldoutGroup ("State Key Sorting")]
        [YamlIgnore, ShowInInspector, ReadOnly]
        [ListDrawerSettings (DefaultExpandedState = false)]
        public List<string> stateKeysSortedGeneration = new List<string> ();
        
        [ShowIf ("@IsTabStates && states != null && stateKeysSortedEvaluation != null && stateKeysSortedEvaluation.Count > 0")]
        [FoldoutGroup ("State Key Sorting")]
        [YamlIgnore, ShowInInspector, ReadOnly]
        [ListDrawerSettings (DefaultExpandedState = false)]
        public List<string> stateKeysSortedEvaluation = new List<string> ();
        
        [ShowIf ("IsTabStates")]
        [FoldoutGroup ("State Key Sorting")]
        [YamlIgnore, ShowInInspector, ReadOnly]
        [ListDrawerSettings (DefaultExpandedState = false)]
        public List<string> stateKeysSortedDisplay = new List<string> ();
        
        [ShowIf ("IsTabStates")]
        [OnValueChanged ("RefreshParentsInStates", true)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataBlockScenarioState> states;
        
        [ShowIf ("@IsTabStates && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataBlockScenarioState> statesProc;
        
        
        [ShowIf ("IsTabSteps")]
        [OnValueChanged ("RefreshParentsInSteps", true)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataBlockScenarioStep> steps;
        
        [ShowIf ("@IsTabSteps && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataBlockScenarioStep> stepsProc;
        
        [ShowIf ("IsTabOther")]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataBlockScenarioUnit> unitPresets;
        
        [ShowIf ("@IsTabOther && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataBlockScenarioUnit> unitPresetsProc;

        public HashSet<string> GetTags (bool processed) => 
            processed ? tagsProc : tags;
        
        
        public bool IsHidden () => hidden;

        private void UpdateUnitGroups () => UpdateUnitGroupsSelective (false);
        
        private void UpdateUnitGroupsSelective (bool processed)
        {
            var stepsSelected = processed ? stepsProc : steps;
            if (stepsSelected != null)
            {
                foreach (var kvp in stepsSelected)
                {
                    var step = kvp.Value;
                    if (step == null)
                        continue;
                    
                    step.UpdateUnitGroups ();
                }
            }
            
            var statesSelected = processed ? statesProc : states;
            if (statesSelected != null)
            {
                foreach (var kvp in statesSelected)
                {
                    var state = kvp.Value;
                    if (state == null)
                        continue;
                    
                    if (state.reactions != null && state.reactions.effectsPerIncrement != null)
                    {
                        foreach (var kvp2 in state.reactions.effectsPerIncrement)
                        {
                            var reaction = kvp2.Value;
                            if (reaction == null)
                                continue;
                            
                            reaction.UpdateUnitGroups ();
                        }
                    }
                }
            }
        }
        
        public override void OnAfterDeserialization (string key)
        {
            RefreshParentsInSteps ();
            RefreshParentsInStates ();
            
            base.OnAfterDeserialization (key);

            UpdateGroups ();
            UpdateUnitGroups ();

            if (unitPresets != null)
            {
                int presetEmbeddedIndex = 0;
                foreach (var kvp in unitPresets)
                {
                    var value = kvp.Value;
                    if (value == null)
                        continue;
                    
                    if (value is DataBlockScenarioUnitPresetEmbedded presetEmbedded)
                    {
                        if (presetEmbedded.preset != null)
                        {
                            presetEmbedded.preset.OnAfterDeserialization ($"scenario_{key}_custom_{presetEmbeddedIndex}");
                            presetEmbedded.preset.OnAfterDeserializationEmbedded ();
                            presetEmbeddedIndex += 1;
                        }
                    }
                }
            }
        }

        public void Clear ()
        {
            parents = null;
            core = null;
            areas = null;
            entry = null;
            tags = null;
            states = null;
            steps = null;
            unitPresets = null;
        }

        public void RefreshNonSerializedDataProcessed ()
        {
            RefreshParentsInStepsSelective (true);
            RefreshParentsInStatesSelective (true);
            UpdateGroups ();
            UpdateUnitGroupsSelective (true);
            
            #if UNITY_EDITOR
            if (areasProc != null)
                areasProc.RefreshPrediction ();
            #endif
        }

        private static List<DataContainerScenarioGroup> groupsFound = new List<DataContainerScenarioGroup> ();

        public void UpdateGroups ()
        {
            if (groupKeys != null)
                groupKeys.Clear ();
            else
                groupKeys = new List<string> ();
            
            groupsFound.Clear ();
            if (tagsProc == null)
                return;

            foreach (var kvp in DataMultiLinkerScenarioGroup.data)
            {
                var group = kvp.Value;
                if (group.hidden)
                    continue;
                
                if (group.tags != null && group.tags.Count > 0)
                {
                    bool match = true;
                    foreach (var kvp2 in group.tags)
                    {
                        var tag = kvp2.Key;
                        var required = kvp2.Value;
                        var present = tagsProc.Contains (tag);

                        if (required != present)
                        {
                            match = false;
                            break;
                        }
                    }

                    if (!match)
                        continue;
                }

                groupsFound.Add (group);
            }

            int groupsCount = groupsFound.Count;
            if (groupsCount == 0)
                return;
            
            if (groupsCount > 1)
                groupsFound.Sort ((x, y) => x.priority.CompareTo (y.priority));

            foreach (var group in groupsFound)
                groupKeys.Add (group.key);

            if (stepsProc != null && groupKeys != null && groupKeys.Count > 0)
            {
                var groupKeyMain = groupKeys[0];
                var groupMain = DataMultiLinkerScenarioGroup.GetEntry (groupKeyMain, false);
                
                foreach (var kvp in stepsProc)
                {
                    var step = kvp.Value;
                    if (step != null && step.core != null && step.core.textFromScenarioGroup)
                    {
                        step.core.textHeader = string.Empty;
                        step.core.textDesc = string.Empty;

                        if (groupMain != null && !groupMain.hidden)
                        {
                            step.core.textHeader = groupMain.textName;
                            step.core.textDesc = groupMain.textDesc;
                        }
                    }
                }
            }
        }

        private void RefreshParentsInStates () => RefreshParentsInStatesSelective (false);

        private void RefreshParentsInStatesSelective (bool processed)
        {
            var statesSelected = processed ? statesProc : states;
            if (statesSelected == null)
                return;
            
            if (stateKeysSortedGeneration == null)
                stateKeysSortedGeneration = new List<string> (statesSelected.Count);
            else
                stateKeysSortedGeneration.Clear ();

            if (stateKeysSortedEvaluation == null)
                stateKeysSortedEvaluation = new List<string> (statesSelected.Count);
            else
                stateKeysSortedEvaluation.Clear ();

            if (stateKeysSortedDisplay == null)
                stateKeysSortedDisplay = new List<string> (statesSelected.Count);
            else
                stateKeysSortedDisplay.Clear ();

            foreach (var kvp in statesSelected)
            {
                var stateKey = kvp.Key;
                var state = kvp.Value;
                if (state == null)
                    continue;

                state.key = stateKey;
                state.parentScenario = this;
                
                stateKeysSortedGeneration.Add (stateKey);
                stateKeysSortedEvaluation.Add (stateKey);
                if (state.visible)
                    stateKeysSortedDisplay.Add (stateKey);
            }
            
            stateKeysSortedGeneration.Sort ((x, y) => statesSelected[x].priorityGeneration.CompareTo (statesSelected[y].priorityGeneration));
            stateKeysSortedEvaluation.Sort ((x, y) => statesSelected[x].priority.CompareTo (statesSelected[y].priority));
            stateKeysSortedDisplay.Sort ((x, y) => statesSelected[x].priorityDisplay.CompareTo (statesSelected[y].priorityDisplay));
        }

        private void RefreshParentsInSteps () => RefreshParentsInStepsSelective (false);
        
        private void RefreshParentsInStepsSelective (bool processed)
        {
            var entrySelected = processed ? entryProc : entry;
            if (entrySelected != null && entrySelected.squadSlotCustom != null)
                entrySelected.squadSlotCustom.parentScenario = this;

            var stepsSelected = processed ? stepsProc : steps;
            if (stepsSelected == null)
                return;

            foreach (var kvp in stepsSelected)
            {
                var stepKey = kvp.Key;
                var step = kvp.Value;
                if (step == null)
                    continue;

                step.key = stepKey;
                step.parentScenario = this;

                if (step.core != null)
                    step.core.parentStep = step;

                if (step.unitGroups != null)
                {
                    for (int i = 0; i < step.unitGroups.Count; ++i)
                    {
                        var unitGroupSlot = step.unitGroups[i];
                        if (unitGroupSlot == null)
                            continue;
                        
                        unitGroupSlot.index = i;
                        unitGroupSlot.parentStep = step;
                        unitGroupSlot.parentScenario = this;

                        if (unitGroupSlot.check != null)
                            unitGroupSlot.check.parentSlot = unitGroupSlot;

                        if (unitGroupSlot is DataBlockScenarioUnitGroupEmbedded unitGroupSlotEmbedded)
                        {
                            if (unitGroupSlotEmbedded.units != null)
                            {
                                foreach (var unit in unitGroupSlotEmbedded.units)
                                {
                                    if (unit == null)
                                        continue;
                                
                                    unit.parentScenario = this;
                                    unit.OnAfterDeserialization ();
                                }
                            }
                        }
                    }
                }
            }
        }
        
        public override void OnBeforeSerialization ()
        {
            if (steps == null)
                return;

            foreach (var kvp in steps)
            {
                var step = kvp.Value;
                if (step == null)
                    continue;

                if (step.unitGroups != null)
                {
                    for (int i = 0; i < step.unitGroups.Count; ++i)
                    {
                        var unitGroup = step.unitGroups[i];
                        if (unitGroup == null)
                            continue;
                        
                        if (unitGroup is DataBlockScenarioUnitGroupEmbedded unitGroupEmbedded)
                        {
                            if (unitGroupEmbedded.units != null)
                            {
                                foreach (var unit in unitGroupEmbedded.units)
                                {
                                    if (unit == null)
                                        continue;

                                    unit.OnBeforeSerialization ();
                                }
                            }
                        }
                    }
                }
            }
        }
        
        public override void ResolveText ()
        {
            // textName = DataManagerText.GetText (TextLibs.scenarioEmbedded, $"{key}__core_header", true);
            // textDesc = DataManagerText.GetText (TextLibs.scenarioEmbedded, $"{key}__core_text", true);
            
            if (states != null)
            {
                foreach (var kvp in states)
                {
                    var stateKey = kvp.Key;
                    var state = kvp.Value;
                    if (state == null)
                        continue;

                    state.ResolveText ();
                }
            }
            
            if (steps != null)
            {
                foreach (var kvp in steps)
                {
                    var stepKey = kvp.Key;
                    var step = kvp.Value;
                    if (step == null)
                        continue;

                    if (step.core != null)
                        step.core.ResolveText ();

                    if (step.hintsConditional != null && step.hintsConditional.Count > 0)
                    {
                        for (int i = 0; i < step.hintsConditional.Count; ++i)
                        {
                            var hint = step.hintsConditional[i];
                            if (hint == null)
                                continue;
                            
                            var textKeyHintConditional = $"{key}__step_{stepKey}_1hc_{i}_text";
                            // hint.text = DataManagerText.GetText (TextLibs.scenarioEmbedded, textKeyHintConditional, true);
                            
                            if (hint.data != null)
                                hint.data.text = DataManagerText.GetText (TextLibs.scenarioEmbedded, textKeyHintConditional, true);
                        }
                    }

                    if (step.unitGroups != null)
                    {
                        // Start at -1 to allow += 1 on first step of the loop body, simplifying bailouts
                        int groupIndex = -1;
                        foreach (var group in step.unitGroups)
                        {
                            groupIndex += 1;
                            if (group == null)
                                continue;

                            if (group is DataBlockScenarioUnitGroupEmbedded groupEmbedded)
                            {
                                if (groupEmbedded.units != null)
                                {
                                    // Start at -1 to allow += 1 on first step of the loop body, simplifying bailouts
                                    int unitIndex = -1;
                                    foreach (var resolver in groupEmbedded.units)
                                    {
                                        unitIndex += 1;
                                        if (resolver.custom == null || (resolver.custom.id == null && resolver.custom.idPilot == null))
                                            continue;
                                        
                                        // Construct prefix once, since it's quite long, easy to make typos
                                        var textKeyPrefixID = $"{key}__step_{stepKey}_g{groupIndex}_u{unitIndex}";
                                        
                                        if (resolver.custom.id != null)
                                        {
                                            var textKey = $"{textKeyPrefixID}_unit";
                                            resolver.custom.id.nameOverride = DataManagerText.GetText (TextLibs.scenarioEmbedded, textKey, true);
                                        }

                                        if (resolver.custom.idPilot != null)
                                        {
                                            var textKeyCallsign = $"{textKeyPrefixID}_pilot_callsign";
                                            resolver.custom.idPilot.callsignOverride = DataManagerText.GetText (TextLibs.scenarioEmbedded, textKeyCallsign, true);
                                            
                                            var textKeyName = $"{textKeyPrefixID}_pilot_name";
                                            resolver.custom.idPilot.nameOverride = DataManagerText.GetText (TextLibs.scenarioEmbedded, textKeyName, true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private const string horGroupStepsHeader = "StepsHeader";
        private const string btGroupCore = "CoreButtons";

        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerScenario () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        private IEnumerable<string> GetStateKeys => statesProc != null ? statesProc.Keys : null;
        private IEnumerable<string> GetStepKeys => stepsProc != null ? stepsProc.Keys : null;

        private bool AreTabsVisible => DataMultiLinkerScenario.Presentation.tabsVisiblePerConfig;
        private Color GetTabColor (bool open) => Color.white.WithAlpha (open ? 0.5f : 1f);
        private Color GetElementColor (int index) => DataEditor.GetColorFromElementIndex (index);
        
        private bool IsInheritanceVisible => DataMultiLinkerScenario.Presentation.showInheritance;
        public bool IsTabCore => DataMultiLinkerScenario.Presentation.tabCore;
        public bool IsTabSteps => DataMultiLinkerScenario.Presentation.tabSteps;
        public bool IsTabStates => DataMultiLinkerScenario.Presentation.tabStates;
        public bool IsTabOther => DataMultiLinkerScenario.Presentation.tabOther;
        
        [PropertyOrder (-10), GUIColor("@GetTabColor (IsTabCore)"), ShowIf ("AreTabsVisible")]
        [Button ("Core", ButtonSizes.Large), ButtonGroup]
        public void SetTabCore () => UtilityBool.Invert (ref DataMultiLinkerScenario.Presentation.tabCore);

        [PropertyOrder (-10), GUIColor("@GetTabColor (IsTabSteps)"), ShowIf ("AreTabsVisible")]
        [Button ("Steps", ButtonSizes.Large), ButtonGroup]
        public void SetTabSteps () => UtilityBool.Invert (ref DataMultiLinkerScenario.Presentation.tabSteps);

        [PropertyOrder (-10), GUIColor("@GetTabColor (IsTabStates)"), ShowIf ("AreTabsVisible")]
        [Button ("States", ButtonSizes.Large), ButtonGroup]
        public void SetTabStates () => UtilityBool.Invert (ref DataMultiLinkerScenario.Presentation.tabStates);

        [PropertyOrder (-10), GUIColor("@GetTabColor (IsTabOther)"), ShowIf ("AreTabsVisible")]
        [Button ("Other", ButtonSizes.Large), ButtonGroup]
        public void SetTabOther () => UtilityBool.Invert (ref DataMultiLinkerScenario.Presentation.tabOther);

        public override void SaveText ()
        {
            if (states != null)
            {
                foreach (var kvp in states)
                {
                    var statesKey = kvp.Key;
                    var state = kvp.Value;
                    if (state == null || !state.visible)
                        continue;
                    
                    if (string.IsNullOrEmpty (state.textNameKey))
                    {
                        var textKeyPrimary = $"{key}__state_{statesKey}_0c_header";
                        DataManagerText.TryAddingTextToLibrary (TextLibs.scenarioEmbedded, textKeyPrimary, state.textName);
                    }
  
                    if (string.IsNullOrEmpty (state.textDescKey))
                    {
                        var textKeySecondary = $"{key}__state_{statesKey}_0c_text";
                        DataManagerText.TryAddingTextToLibrary (TextLibs.scenarioEmbedded, textKeySecondary, state.textDesc);
                    }
                }
            }

            if (steps != null)
            {
                foreach (var kvp in steps)
                {
                    var stepKey = kvp.Key;
                    var step = kvp.Value;
                    if (step == null)
                        continue;
                    
                    if (step.core != null && !step.core.textFromScenarioGroup)
                    {
                        if (string.IsNullOrEmpty (step.core.textCurrentPrimaryKey))
                        {
                            var textKeyPrimary = $"{key}__step_{stepKey}_0c_header";
                            DataManagerText.TryAddingTextToLibrary (TextLibs.scenarioEmbedded, textKeyPrimary, step.core.textHeader);
                        }

                        if (string.IsNullOrEmpty (step.core.textCurrentSecondaryKey))
                        {
                            var textKeySecondary = $"{key}__step_{stepKey}_0c_text";
                            DataManagerText.TryAddingTextToLibrary (TextLibs.scenarioEmbedded, textKeySecondary, step.core.textDesc);
                        }
                    }

                    if (step.hintsConditional != null && step.hintsConditional.Count > 0)
                    {
                        for (int i = 0; i < step.hintsConditional.Count; ++i)
                        {
                            var hint = step.hintsConditional[i];
                            if (hint == null)
                                continue;
                            
                            var textKeyHintConditional = $"{key}__step_{stepKey}_1hc_{i}_text";
                            
                            // DataManagerText.TryAddingTextToLibrary (TextLibs.scenarioEmbedded, textKeyHintConditional, hint.text);
                            if (hint.data != null)
                                DataManagerText.TryAddingTextToLibrary (TextLibs.scenarioEmbedded, textKeyHintConditional, hint.data.text);
                        }
                    }
                    
                    if (step.unitGroups != null)
                    {
                        // Start at -1 to allow += 1 on first step of the loop body, simplifying bailouts
                        int groupIndex = -1;
                        foreach (var group in step.unitGroups)
                        {
                            groupIndex += 1;
                            if (group == null)
                                continue;

                            if (group is DataBlockScenarioUnitGroupEmbedded groupEmbedded)
                            {
                                if (groupEmbedded.units != null)
                                {
                                    // Start at -1 to allow += 1 on first step of the loop body, simplifying bailouts
                                    int unitIndex = -1;
                                    foreach (var resolver in groupEmbedded.units)
                                    {
                                        unitIndex += 1;
                                        if (resolver.custom == null || (resolver.custom.id == null && resolver.custom.idPilot == null))
                                            continue;
                                        
                                        // Construct prefix once, since it's quite long, easy to make typos
                                        var textKeyPrefixID = $"{key}__step_{stepKey}_g{groupIndex}_u{unitIndex}";
                                        
                                        if (resolver.custom.id != null)
                                        {
                                            var textKey = $"{textKeyPrefixID}_unit";
                                            DataManagerText.TryAddingTextToLibrary (TextLibs.scenarioEmbedded, textKey, resolver.custom.id.nameOverride);
                                        }

                                        if (resolver.custom.idPilot != null)
                                        {
                                            var textKeyCallsign = $"{textKeyPrefixID}_pilot_callsign";
                                            DataManagerText.TryAddingTextToLibrary (TextLibs.scenarioEmbedded, textKeyCallsign, resolver.custom.idPilot.callsignOverride);
                                            
                                            var textKeyName = $"{textKeyPrefixID}_pilot_name";
                                            DataManagerText.TryAddingTextToLibrary (TextLibs.scenarioEmbedded, textKeyName, resolver.custom.idPilot.nameOverride);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [ShowIf ("IsTabCore")]
        [Button ("Select"), PropertyOrder (-2), ButtonGroup (btGroupCore)]
        public void SelectForEditing () => DataMultiLinkerScenario.selectedScenario = this;
        
        [ShowIf ("IsTabCore")]
        [Button ("Load area"), PropertyOrder (-1), ButtonGroup (btGroupCore)]
        public void Load ()
        {
            if (CombatSceneHelper.ins == null)
                return;
            
            string areaKey = null;
            if (areasProc.tagFilterUsed)
            {
                var scenariosMatchingTag = DataTagUtility.GetKeysWithTags (DataMultiLinkerCombatArea.data, areasProc.tagFilter);
                if (scenariosMatchingTag.Count == 0)
                {
                    Debug.LogWarning ($"Failed to find any combat areas matching tag requirements");
                    return;
                }

                int randomIndex = UnityEngine.Random.Range (0, scenariosMatchingTag.Count);
                areaKey = scenariosMatchingTag[randomIndex];
                Debug.LogWarning ($"Found combat area config matching tag requirements: {areaKey} | Warning: different map might be loaded every time");
            }
            else
                areaKey = areasProc.keys?.GetRandomEntry ();
            
            if (!string.IsNullOrEmpty (areaKey))
            {
                var areaData = DataMultiLinkerCombatArea.GetEntry (areaKey);
                if (areaData == null)
                    return;

                areaData.SelectAndApplyToScene ();
            }
        }

        #endif
        #endregion
    }

    [Serializable]
    public class DataBlockUnitSetup
    {
        [ValueDropdown ("GetFactions")][GUIColor ("GetFactionColor")]
        public string faction = Factions.enemy;

        [FoldoutGroup ("Pilot"), LabelText ("Custom Internal Name")]
        public string pilotNameInternalCustom;

        [FoldoutGroup ("Unit"), LabelText ("Custom Internal Name")]
        public string unitNameInternalCustom;
        
        [FoldoutGroup ("Unit"), LabelText ("Preset Used")]
        public bool unitFromLibrary = true;
        
        [FoldoutGroup ("Unit"), LabelText ("Preset Name")]
        [ShowIf ("unitFromLibrary"), ValueDropdown ("GetPresetNames")]
        public string unitPresetName;
        
        [FoldoutGroup ("Unit"), LabelText ("Level")]
        public int level = 1;
        
        [FoldoutGroup ("Unit")]
        public bool speedOverrideUsed = false;
        
        [FoldoutGroup ("Unit")]
        [ShowIf ("speedOverrideUsed")]
        public bool speedFromBlueprint = true;
        
        [FoldoutGroup ("Unit")]
        [ShowIf ("@speedOverrideUsed && !speedFromBlueprint")]
        public float speedOverride;
        
        

        [FoldoutGroup ("Unit"), LabelText ("Custom")]
        [HideIf ("unitFromLibrary")]
        public DataContainerUnitPreset unit;
        
        [FoldoutGroup ("Unit"), LabelText ("Combat Data")]
        public DataBlockUnitSetupCombat combatInfo;
        
        #if UNITY_EDITOR
        
        private static IEnumerable<string> GetFactions () { return DataHelperUnitEquipment.GetFactions (); }
        private static IEnumerable<string> GetPresetNames () { return DataMultiLinkerUnitPreset.data.Keys; }

        private Color GetFactionColor () => 
            Color.HSVToRGB (CombatUIUtility.IsFactionFriendly (faction) ? 0.55f : 0f, 0.5f, 1f);

        #endif
    }
    
    [Serializable]
    public class DataBlockUnitSetupCombat
    {
        public bool isVisible;
        public bool isPlayerControlled;
        public bool isAIControlled;
        public string aiPlanningKey;

        public bool preferredSpawnUsed = false;

        // [InlineButton ("CopyLocationFromSpawn", "Copy from spawn")]
        [InlineButton ("DiscretizeLocation", "Discretize")]
        // [ValueDropdown ("GetSpawnGroupNames")]
        [ShowIf ("preferredSpawnUsed")]
        public string preferredSpawnGroup;
        
        // [PropertyRange (0, "GetSpawnIndexLimit")]
        [ShowIf ("preferredSpawnUsed")]
        public int preferredSpawnIndex;
        
        public Vector3 position;
        public Vector3 rotation;

        #if UNITY_EDITOR

        /*
        [NonSerialized]
        private DataContainerScenario parent;
        private static readonly IEnumerable<string> spawnNamesFallback = new List<string> ();

        public void SetInspectorProperties (DataContainerScenario parent)
        {
            this.parent = parent;
        }

        private IEnumerable<string> GetSpawnGroupNames ()
        {
            return parent != null && parent.spawnGroups != null ? parent.spawnGroups.Keys : spawnNamesFallback;
        }
        
        private int GetSpawnIndexLimit ()
        {
            if (parent == null || parent.spawnGroups == null || string.IsNullOrEmpty (preferredSpawnGroup) || parent.spawnGroups.ContainsKey (preferredSpawnGroup))
                return 0;

            var parentSpawnGroup = parent.spawnGroups[preferredSpawnGroup];
            if (parentSpawnGroup == null || parentSpawnGroup.points == null)
                return 0;

            return parentSpawnGroup.points.Count;
        }

        public void CopyLocationFromSpawn ()
        {
            if (parent == null || parent.spawnGroups == null || string.IsNullOrEmpty (preferredSpawnGroup) || parent.spawnGroups.ContainsKey (preferredSpawnGroup))
                return;

            var parentSpawnGroup = parent.spawnGroups[preferredSpawnGroup];
            if (parentSpawnGroup == null || parentSpawnGroup.points == null || !preferredSpawnIndex.IsValidIndex (parentSpawnGroup.points))
                return;

            var point = parentSpawnGroup.points[preferredSpawnIndex];
            position = point.point;
            rotation = point.rotation;
        }
        */
        
        public void DiscretizeLocation ()
        {
            var gridStep = 3f;
            var gridOffset = 1.5f;
            
            var clampedPoint = new Vector3
            (
                Mathf.Round ((position.x - gridOffset) / gridStep) * gridStep + gridOffset,
                Mathf.Round ((position.y + gridOffset) / gridStep) * gridStep - gridOffset,
                Mathf.Round ((position.z - gridOffset) / gridStep) * gridStep + gridOffset
            );

            position = clampedPoint;
            UnityEditor.SceneView.RepaintAll ();
        }
        
        #endif
    }
}

