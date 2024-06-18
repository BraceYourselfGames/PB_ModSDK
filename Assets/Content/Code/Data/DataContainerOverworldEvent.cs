using System;
using System.Collections.Generic;
using System.Text;
using PhantomBrigade.Functions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    public interface ICustomDataProvider
    {
        string GetKey ();
        bool IsFlagPresent (string key);
        bool TryGetInt (string key, out int result, int fallback = default);
        bool TryGetFloat (string key, out float result, float fallback = default);
        bool TryGetString (string key, out string result, string fallback = default);

        IEnumerable<string> GetFlagKeys ();
        IEnumerable<string> GetIntKeys ();
        IEnumerable<string> GetFloatKeys ();
        IEnumerable<string> GetStringKeys ();
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldEventChanceModifier
    {
        [PropertyRange (0f, 1f), InlineButton ("@entryOnBase = 0", "0"), InlineButton ("@entryOnBase = 1", "1")]
        public float entryOnBase = 1f;
        
        [PropertyRange (0f, 1f), InlineButton ("@entryOnTarget = 0", "0"), InlineButton ("@entryOnTarget = 1", "1")]
        public float entryOnTarget = 1f;
        
        [PropertyRange (0f, 1f), InlineButton ("@completionOnBase = 0", "0"), InlineButton ("@completionOnBase = 1", "1")]
        public float completionOnBase = 0f;
        
        [PropertyRange (0f, 1f), InlineButton ("@completionOnTarget = 0", "0"), InlineButton ("@completionOnTarget = 1", "1")]
        public float completionOnTarget = 0f;
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldEventActionLinks
    {
        [ValueDropdown ("@DataMultiLinkerOverworldAction.data.Keys")]
        public HashSet<string> resetCompletionOnOwner = new HashSet<string> ();
        
        [ValueDropdown ("@DataMultiLinkerOverworldAction.data.Keys")]
        public HashSet<string> resetCompletionOnTarget = new HashSet<string> ();
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldEventTagFilter
    {
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.ScenarioTag)]
        public SortedDictionary<string, bool> tags;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockSpawnFlags
    {
        public bool isWarObjective = false;
    }

    public enum OverworldEventSpawnLocationProvider
    {
        SourceProvince,
        TargetProvince,
        SpecificProvince
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldEventSpawnData
    {
        [PropertyTooltip ("This key is used to find spawned entities later for additional modifications. If you spawn multiple entities, give them unique keys.")]
        [LabelText ("Lookup Key")]
        public string spawnLookupKey = "spawn_01";
        
        public OverworldEventSpawnLocationProvider locationProvider = OverworldEventSpawnLocationProvider.SourceProvince;

        [ShowIf ("@locationProvider == OverworldEventSpawnLocationProvider.SpecificProvince")]
        [ValueDropdown ("@DataMultiLinkerOverworldProvinceBlueprints.data.Keys")]
        public string provinceKey;

        [LabelText ("Spawn Group")]
        [ValueDropdown ("GetSpawnGroupKeys")]
        public string spawnGroupKey = "general";

        [HorizontalGroup ("profile")]
        [LabelText ("Custom Profile")]
        public bool generationProfileCustom;
        
        [HideLabel, HorizontalGroup ("profile")]
        [HideIf ("generationProfileCustom")]
        [ValueDropdown ("@DataMultiLinkerOverworldSiteGenerationSettings.data.Keys")]
        public string generationProfileKey;
        
        [ShowIf ("generationProfileCustom")]
        [HideLabel, BoxGroup ("Profile", false)]
        public DataContainerOverworldSiteGenerationSettings generationProfile;
        
        [ValueDropdown ("@DataHelperUnitEquipment.GetFactions ()")]
        public string faction = Factions.enemy;
        
        [DropdownReference]
        public List<DataBlockMemoryChangeFloat> memoryChanges;

        [DropdownReference (true)]
        public DataBlockIntel intel;
        
        [DropdownReference (true)]
        public DataBlockSpawnFlags flags;

        #region Editor
        #if UNITY_EDITOR
        
        private IEnumerable<string> GetSpawnGroupKeys => 
            DataMultiLinkerOverworldProvinceBlueprints.spawnGroupKeys;
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockOverworldEventSpawnData () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    public enum ActionOwnerProvider
    {
        Base,
        Source,
        SourceProvince,
        Target,
        TargetProvince,
        Spawn,
        SpawnProvince,
        ActorSite,
        ActorSiteProvince,
    }
    
    public enum ActionTargetProvider
    {
        None,
        Base,
        BaseProvince,
        Source,
        SourceProvince,
        Target,
        TargetProvince,
        Spawn,
        SpawnProvince,
        ActorSite,
        ActorSiteProvince,
        ActorUnit,
        ActorPilot
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldActionInstanceData
    {
        [ValueDropdown ("@DataMultiLinkerOverworldAction.data.Keys")]
        public string key;

        public ActionOwnerProvider owner = ActionOwnerProvider.Base;

        [ShowIf ("IsOwnerKeyVisible")]
        public string ownerKey;

        public ActionTargetProvider target = ActionTargetProvider.None;

        [ShowIf ("IsTargetKeyVisible")]
        public string targetKey;
        
        public bool visible = true;

        [DropdownReference (true)]
        public DataBlockFloat durationOverride;

        [DropdownReference (true)]
        public DataBlockFloat durationMultiplier;

        private static StringBuilder sb = new StringBuilder ();

        public override string ToString ()
        {
            sb.Clear ();
            sb.Append ("- ");
            sb.Append (key);

            if (!string.IsNullOrEmpty (key))
            {
                var data = DataMultiLinkerOverworldAction.GetEntry (key, false);
                if (data == null)
                    sb.Append (" (?)");
                else
                {
                    sb.Append (" (");
                    sb.Append (data.ui != null ? data.ui.textName : "?");
                    sb.Append (")");
                }
            }
            
            sb.Append ("\n  - Owner: ");
            sb.Append (OverworldEventUtility.logLookupActionOwner[owner]);
            if 
            (
                owner == ActionOwnerProvider.ActorSite || 
                owner == ActionOwnerProvider.Spawn
            )
            {
                sb.Append (" | Key: ");
                sb.Append (ownerKey);
            }
            
            sb.Append ("\n  - Target: ");
            sb.Append (OverworldEventUtility.logLookupActionTarget[target]);
            if 
            (
                target == ActionTargetProvider.ActorSite || 
                target == ActionTargetProvider.ActorUnit || 
                target == ActionTargetProvider.ActorPilot || 
                target == ActionTargetProvider.Spawn
            )
            {
                sb.Append (" | Key: ");
                sb.Append (targetKey);
            }
            
            if (durationMultiplier != null)
            {
                sb.Append ("\n  - Duration multiplier: ");
                sb.Append (durationMultiplier.f);
            }

            if (durationOverride != null)
            {
                sb.Append ("\n  - Duration override: ");
                sb.Append (durationOverride.f);
            }

            return sb.ToString ();
        }

        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockOverworldActionInstanceData () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        private bool IsOwnerKeyVisible =>
            owner == ActionOwnerProvider.ActorSite || owner == ActionOwnerProvider.Spawn;

        private bool IsTargetKeyVisible =>
            target == ActionTargetProvider.ActorSite || 
            target == ActionTargetProvider.ActorUnit || 
            target == ActionTargetProvider.ActorPilot || 
            target == ActionTargetProvider.Spawn;
        
        /*
        private IEnumerable<string> GetOwnerKeys () 
        {
            if (context == MemoryChangeContextEvent.ActorUnit)
                return parentEvent != null && parentEvent.actorsUnits != null ? parentEvent.actorsUnits.Keys : null;
            else if (context == MemoryChangeContextEvent.ActorPilot)
                return parentEvent != null && parentEvent.actorsPilots != null ? parentEvent.actorsPilots.Keys : null;
            else if (context == MemoryChangeContextEvent.ActorWorld)
                return parentEvent != null && parentEvent.actorsSites != null ? parentEvent.actorsSites.Keys : null;
            else
                return null;
        }
        */

        #endif
        #endregion
    }

    public enum OverworldEventEvaluationInterval
    {
        Short,
        Medium,
        Long
    }

    public static class EventEvaluationGroups
	{
        public const string OnTime = "OnTime";
        public const string OnContact = "OnContact";
        public const string OnPostCombat = "OnPostCombat";
        public const string OnProvinceLiberated = "OnProvinceLiberated";
	}
    
    [Serializable][LabelWidth(180f)]
    public class DataContainerOverworldEvent : 
        DataContainerWithText, 
        IComparable<DataContainerOverworldEvent>, 
        UtilityCollections.IWeightedCollectionEntry,
        ICustomDataProvider
    {
        [ShowIf ("IsTabCore")]
        public bool hidden = true;
        
        [ShowIf ("IsTabCore")]
        public bool forced = false;
        
        [ShowIf ("IsTabCore")]
        public bool optional = false;

        [ShowIf ("IsTabCore")]
        public bool refreshEventsOnExit = true;

        [ShowIf ("IsTabCore")]
        public bool restartTimeOnExit = true;
        
        [ShowIf ("@IsTabCore || IsTabWriting"), OnValueChanged ("CheckGeneratedTextVariantsAndResolveText")]
        public bool textVariantsForPronouns = false;
        
        [ShowIf ("IsTabCore")]
        [ValueDropdown("GetStepKeys")]
        public string stepOnStart = "start";
        
        [ShowIf ("IsEvaluationVisible")]
        [FoldoutGroup (fdGroupCoreEval, false)]
        public int priority;

        [ShowIf ("IsEvaluationVisible")]
        [FoldoutGroup (fdGroupCoreEval)]
        public int group;
        
        [ShowIf ("IsEvaluationVisible")]
        [FoldoutGroup (fdGroupCoreEval)]
        public HashSet<int> groupConflicts;
        
        [ShowIf ("IsEvaluationVisible")]
        [FoldoutGroup (fdGroupCoreEval)]
        [PropertyRange (0f, 1f)]
        public float chance = 0.25f;

        [ShowIf ("IsEvaluationVisible")]
        [FoldoutGroup (fdGroupCoreEval)]
        public DataBlockOverworldEventChanceModifier chanceModifier;
        
        [ShowIf ("IsTabCore")]
        [FoldoutGroup (fdGroupCoreEval)]
        public bool trackCompletionOnSelf = false;
        
        [ShowIf ("IsTabCore")]
        [FoldoutGroup (fdGroupCoreEval)]
        public bool trackCompletionOnTarget = false;

        [ShowIf("IsEvaluationVisible")]
        [FoldoutGroup(fdGroupCoreEval)]
        [ValueDropdown("GetEvaluationGroupKeys")]
        public string evaluationGroup = EventEvaluationGroups.OnContact;

        [ShowIf ("@IsEvaluationVisible && IsEvaluationOverTime")]
        [FoldoutGroup (fdGroupCoreEval)]
        public OverworldEventEvaluationInterval evaluationInterval = OverworldEventEvaluationInterval.Short;


        [ShowIf ("@IsTabWriting || IsTabSteps")]
        [OnValueChanged ("RefreshParentsInStepsOnEdit", true)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        [ListDrawerSettings (NumberOfItemsPerPage = 1)]
        public SortedDictionary<string, DataBlockOverworldEventStep> steps = new SortedDictionary<string, DataBlockOverworldEventStep>
        {
            { "start", new DataBlockOverworldEventStep { key = "start" } }
        };

        
        [ShowIf ("@IsTabWriting || IsTabOptions")]
        [OnValueChanged ("RefreshParentsInOptionsOnEdit", true)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        [LabelText ("Custom Options")]
        public SortedDictionary<string, DataContainerOverworldEventOption> options = new SortedDictionary<string, DataContainerOverworldEventOption>(){
            {"intel", new DataContainerOverworldEventOption()},
            {"leave", new DataContainerOverworldEventOption()},
        };

        
        [ShowIf ("IsTabOther")]
        [LabelText ("Action Links")]
        [DropdownReference (true)]
        public DataBlockOverworldEventActionLinks actions;
        
        [ShowIf ("IsTabOther")]
        [OnValueChanged ("RefreshParentsInWorldActors", true)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        [DropdownReference]
        public Dictionary<string, DataBlockOverworldEventActorWorld> actorsSites;

        [ShowIf ("IsTabOther")]
        [OnValueChanged ("RefreshParentsInUnitActors", true)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        [DropdownReference]
        public Dictionary<string, DataBlockOverworldEventActorUnit> actorsUnits;
        
        [ShowIf ("IsTabOther")]
        [OnValueChanged ("RefreshParentsInPilotActors", true)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        [DropdownReference]
        public Dictionary<string, DataBlockOverworldEventActorPilot> actorsPilots;

        [ShowIf ("IsTabOther")]
        [DropdownReference]
        public SortedDictionary<string, bool> customFlags;
        
        [ShowIf ("IsTabOther")]
        [DropdownReference]
        public SortedDictionary<string, int> customInts;
        
        [ShowIf ("IsTabOther")]
        [DropdownReference]
        public SortedDictionary<string, float> customFloats;
        
        [ShowIf ("IsTabOther")]
        [DropdownReference]
        public SortedDictionary<string, string> customStrings;
        
        [ShowIf ("IsTabOther")]
        [DropdownReference]
        public SortedDictionary<string, DataBlockOverworldEventTagFilter> customTagFilters;




        public override void OnAfterDeserialization (string key)
        {
            // We need to generate text variants before text resolve happens
            CheckGeneratedTextVariants (key);
        
            // Text is resolved in the base version of OnAfterDeserialization
            base.OnAfterDeserialization (key);
            
            RefreshParentsInSteps (true);
            RefreshParentsInOptions (true);
            RefreshParentsInUnitActors ();
            RefreshParentsInPilotActors ();
            RefreshParentsInWorldActors ();
        }

        public override void OnKeyReplacement (string keyOld, string keyNew)
        {
            base.OnKeyReplacement (keyOld, keyNew);

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
                        OnKeyReplacementInEventCalls (step.functions, keyOld, keyNew, $"Event {kvp.Key}, step {kvp2.Key}");
                    }
                }

                if (eventData.options != null)
                {
                    foreach (var kvp2 in eventData.options)
                    {
                        var option = kvp2.Value;
                        OnKeyReplacementInEventCalls (option.functions, keyOld, keyNew, $"Event {kvp.Key}, embedded option {kvp2.Key}");
                    }
                }
            }

            var dataOptions = DataMultiLinkerOverworldEventOption.data;
            foreach (var kvp in dataOptions)
            {
                var option = kvp.Value;
                OnKeyReplacementInEventCalls (option.functions, keyOld, keyNew, $"Shared option {kvp.Key}");
            }

            var dataActions = DataMultiLinkerOverworldAction.data;
            foreach (var kvp in dataActions)
            {
                var action = kvp.Value;
                OnKeyReplacementInActionChange (action.changesOnStart, keyOld, keyNew, $"Action {kvp.Key}, on start");
                OnKeyReplacementInActionChange (action.changesOnCancellation, keyOld, keyNew, $"Action {kvp.Key}, on cancellation");
                OnKeyReplacementInActionChange (action.changesOnTermination, keyOld, keyNew, $"Action {kvp.Key}, on termination");
                OnKeyReplacementInActionChange (action.changesOnCompletion, keyOld, keyNew, $"Action {kvp.Key}, on completion");
            }
        }

        private void OnKeyReplacementInEventCalls (List<IOverworldEventFunction> calls, string keyOld, string keyNew, string context)
        {
            if (calls == null)
                return;

            foreach (var call in calls)
            {
                if (call is StartOverworldEvent startEvent)
                {
                    for (var i = 0; i < startEvent.eventKeys.Count; i++)
                    {
                        if (startEvent.eventKeys[i] == keyOld)
                        {
                            Debug.LogWarning ($"{context} | Call {i} (StartOverworldEvent) | Replacing event key: {keyOld} -> {keyNew})");
                            startEvent.eventKeys[i] = keyNew;
                        }
                    }
                }
            }
        }
        
        private void OnKeyReplacementInActionChange (DataBlockOverworldActionChange change, string keyOld, string keyNew, string context)
        {
            if (change == null || change.functions == null)
                return;
            
            for (int i = 0; i < change.functions.Count; ++i)
            {
                var function = change.functions[i];
                if (function is StartOverworldEvent functionStartEvent && functionStartEvent.eventKeys != null && functionStartEvent.eventKeys.Count > 0)
                {
                    for (int x = 0; x < functionStartEvent.eventKeys.Count; ++x)
                    {
                        var eventKey = functionStartEvent.eventKeys[x];
                        if (eventKey == keyOld)
                        {
                            Debug.LogWarning ($"{context} | Function {i} (StartOverworldEvent) argument | Replacing event key: {keyOld} -> {keyNew})");
                            functionStartEvent.eventKeys[x] = keyNew;
                        }  
                    }
                }
            }
        }

        private void CheckGeneratedTextVariants (string key)
        {
            // Just in case, warn the user if they're trying to use this flag on events that do not have the expected pilot actor
            if (textVariantsForPronouns && (actorsPilots == null || actorsPilots.Count == 0 || !actorsPilots.ContainsKey ("pilot_0")))
                Debug.LogWarning ($"Event {key} requires text variant generation for pilot pronouns, but has no pilot actor slot named pilot_0!");

            // A note on textVariantsGenerated.Clear ():
            // This is not strictly necessary if we only run this method on after deserialization, but
            // we might also toggle flags affecting generation in the inspector, which also triggers this.
            // So, the collection might not be empty, and it makes sense to clear it just in case.
            
            if (steps != null)
            {
                foreach (var kvp in steps)
                {
                    var step = kvp.Value;
                    if (step == null)
                        continue;
                    
                    if (step.textVariantsGenerated != null)
                        step.textVariantsGenerated.Clear ();

                    if (textVariantsForPronouns)
                        TextVariantHelper.GeneratePilotTextVariants (ref step.textVariantsGenerated);
                }
            }
            
            if (options != null)
            {
                foreach (var kvp in options)
                {
                    var option = kvp.Value;
                    if (option == null)
                        continue;
                    
                    if (option.textVariantsGenerated != null)
                        option.textVariantsGenerated.Clear ();

                    if (textVariantsForPronouns)
                        TextVariantHelper.GeneratePilotTextVariants (ref option.textVariantsGenerated);
                }
            }
        }
        
        private void CheckGeneratedTextVariantsAndResolveText ()
        {
            CheckGeneratedTextVariants (key);
            ResolveText ();
        }

        private void RefreshParentsInStepsOnEdit ()
        {
            RefreshParentsInSteps (false);
        }

        private void RefreshParentsInSteps (bool onAfterDeserialization)
        {
            if (steps == null)
                return;

            foreach (var kvp in steps)
            {
                var step = kvp.Value;
                if (step == null)
                    continue;
                
                step.key = kvp.Key;
                step.parent = this;
                
                if (step.check != null)
                    step.check.RefreshParents (parentStep: step);
                
                if (step.functions != null)
                    RefreshParentsInFunctions (step.functions, onAfterDeserialization);
                
                if (step.options != null)
                {
                    foreach (var optionLink in step.options)
                    {
                        if (optionLink == null)
                            continue;
                    
                        optionLink.parentStep = step;
                        optionLink.parentEvent = this;
                    }
                }
                
                if (step.memoryChanges != null)
                {
                    foreach (var changeGroup in step.memoryChanges)
                    {
                        if (changeGroup == null)
                            continue;
                        
                        changeGroup.parentEvent = this;
                    }
                }
            }
        }

        private void RefreshParentsInFunctions (List<IOverworldEventFunction> functions, bool onAfterDeserialization)
        {
            if (functions == null)
                return;

            foreach (var function in functions)
            {
                if (function is IOverworldEventParent functionParented)
                    functionParented.ParentEvent = this;
            }
        }

        private void RefreshParentsInOptionsOnEdit ()
        {
            RefreshParentsInOptions (false);
        }
        
        private void RefreshParentsInOptions (bool onAfterDeserialization)
        {
            if (options == null)
                return;
            
            foreach (var kvp in options)
            {
                var option = kvp.Value;
                if (option != null)
                    option.RefreshParents (kvp.Key, this, onAfterDeserialization);
            }
        }
        
        private void RefreshParentsInWorldActors ()
        {
	        if (actorsSites == null)
		        return;
            
	        foreach (var kvp in actorsSites)
	        {
		        if (kvp.Value != null)
                {
                    kvp.Value.parent = this;
                    kvp.Value.key = kvp.Key;
                }
            }
        }

        private void RefreshParentsInUnitActors ()
        {
            if (actorsUnits == null)
                return;
            
            foreach (var kvp in actorsUnits)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.parent = this;
                    kvp.Value.key = kvp.Key;
                }
            }
        }
        
        private void RefreshParentsInPilotActors ()
        {
            if (actorsPilots == null)
                return;
            
            foreach (var kvp in actorsPilots)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.parent = this;
                    kvp.Value.key = kvp.Key;
                }
            }
        }





        public int CompareTo (DataContainerOverworldEvent other)
        {
            if (ReferenceEquals (null, other)) 
                return 1;
            
            return priority == other.priority ? 0 : priority.CompareTo (other.priority);
        }

        public float GetWeight ()
        {
            return chance;
        }

        public string GetCoreDescription ()
        {
            sb.Clear ();

            if (hidden)
                sb.Append ("Hidden | ");
            
            sb.Append ("Group: ");
            sb.Append (evaluationGroup);

            return sb.ToString ();
        }
        
        public IEnumerable<string> GetFlagKeys () => customFlags?.Keys;
        public IEnumerable<string> GetIntKeys () => customInts?.Keys;
        public IEnumerable<string> GetFloatKeys () => customFloats?.Keys;
        public IEnumerable<string> GetStringKeys () => customStrings?.Keys;

        private static StringBuilder sb = new StringBuilder ();
        private static readonly List<string> actorKeyCache = new List<string> ();
        public IEnumerable<string> GetActorKeys ()
        {
            actorKeyCache.Clear ();

            if (this != null)
            {
                var x = actorsPilots == null;

                actorKeyCache.AddRange (actorsPilots?.Keys ?? (IEnumerable<string>)Array.Empty<string> ());
                actorKeyCache.AddRange (actorsSites?.Keys ?? (IEnumerable<string>)Array.Empty<string> ());
                actorKeyCache.AddRange (actorsUnits?.Keys ?? (IEnumerable<string>)Array.Empty<string> ());
            }
            
            return actorKeyCache;
        }

        public string GetKey () => 
            key;
        
        public bool IsFlagPresent (string key)
        {
            var found = 
                customFlags != null && 
                !string.IsNullOrEmpty (key) && 
                customFlags.ContainsKey (key);
        
            return found;
        }
        
        public bool TryGetInt (string key, out int result, int fallback = default)
        {
            var found = 
                customInts != null && 
                !string.IsNullOrEmpty (key) && 
                customInts.ContainsKey (key);
        
            result = found ? customInts[key] : fallback;
            return found;
        }
        
        public bool TryGetFloat (string key, out float result, float fallback = default)
        {
            var found = 
                customFloats != null && 
                !string.IsNullOrEmpty (key) && 
                customFloats.ContainsKey (key);
        
            result = found ? customFloats[key] : fallback;
            return found;
        }
        
        public bool TryGetString (string key, out string result, string fallback = default)
        {
            var found = 
                customStrings != null && 
                !string.IsNullOrEmpty (key) && 
                customStrings.ContainsKey (key);
        
            result = found ? customStrings[key] : fallback;
            return found;
        }
        
        public bool TryGetTagFilter (string key, out SortedDictionary<string, bool> result, SortedDictionary<string, bool> fallback = default)
        {
            var found = 
                customTagFilters != null && 
                !string.IsNullOrEmpty (key) && 
                customTagFilters.ContainsKey (key) && 
                customTagFilters[key] != null && 
                customTagFilters[key].tags != null;
        
            result = found ? customTagFilters[key].tags : fallback;
            return found;
        }

        public override void ResolveText ()
        {
            // Ensure double underscore separator follows the event name
            // textName = DataManagerText.GetText (TextLibs.overworldEvents, $"ev_{key}__0c_name");
            
            if (steps != null)
            {
                foreach (var kvp in steps)
                {
                    var stepKey = kvp.Key;
                    var step = kvp.Value;
                    if (step == null)
                        continue;

                    step.textName = DataManagerText.GetText (TextLibs.overworldEvents, $"ev_{key}__1s_{stepKey}_header");
                    step.textDesc = DataManagerText.GetText (TextLibs.overworldEvents, $"ev_{key}__1s_{stepKey}_text");
                    
                    if (step.textVariants != null)
                    {
                        foreach (var kvp2 in step.textVariants)
                        {
                            var variantKey = kvp2.Key;
                            var variant = kvp2.Value;
                            if (variant == null)
                                continue;
                            
                            variant.textHeader = DataManagerText.GetText (TextLibs.overworldEvents, $"ev_{key}__1s_{stepKey}_header_tv_{variantKey}");
                            variant.textContent = DataManagerText.GetText (TextLibs.overworldEvents, $"ev_{key}__1s_{stepKey}_text_tv_{variantKey}");
                        }
                    }
                    
                    if (step.textVariantsGenerated != null)
                    {
                        foreach (var kvp2 in step.textVariantsGenerated)
                        {
                            var variantKey = kvp2.Key;
                            var variant = kvp2.Value;
                            if (variant == null)
                                continue;
                            
                            variant.textHeader = DataManagerText.GetText (TextLibs.overworldEvents, $"ev_{key}__1s_{stepKey}_header_tvg_{variantKey}");
                            variant.textContent = DataManagerText.GetText (TextLibs.overworldEvents, $"ev_{key}__1s_{stepKey}_text_tvg_{variantKey}");
                        }
                    }
                }
            }

            if (options != null)
            {
                foreach (var kvp in options)
                {
                    var optionKey = kvp.Key;
                    var option = kvp.Value;
                    if (option == null)
                        continue;

                    option.textHeader = DataManagerText.GetText (TextLibs.overworldEvents, $"ev_{key}__2o_{optionKey}_header");
                    option.textContent = DataManagerText.GetText (TextLibs.overworldEvents, $"ev_{key}__2o_{optionKey}_text");
                    
                    if (option.textVariants != null)
                    {
                        foreach (var kvp2 in option.textVariants)
                        {
                            var variantKey = kvp2.Key;
                            var variant = kvp2.Value;
                            if (variant == null)
                                continue;
                            
                            variant.textHeader = DataManagerText.GetText (TextLibs.overworldEvents, $"ev_{key}__2o_{optionKey}_header_tv_{variantKey}");
                            variant.textContent = DataManagerText.GetText (TextLibs.overworldEvents, $"ev_{key}__2o_{optionKey}_text_tv_{variantKey}");
                        }
                    }
                    
                    if (option.textVariantsGenerated != null)
                    {
                        foreach (var kvp2 in option.textVariantsGenerated)
                        {
                            var variantKey = kvp2.Key;
                            var variant = kvp2.Value;
                            if (variant == null)
                                continue;
                            
                            variant.textHeader = DataManagerText.GetText (TextLibs.overworldEvents, $"ev_{key}__2o_{optionKey}_header_tvg_{variantKey}");
                            variant.textContent = DataManagerText.GetText (TextLibs.overworldEvents, $"ev_{key}__2o_{optionKey}_text_tvg_{variantKey}");
                        }
                    }
                }
            }
        }
        
        private const string tabGroupCore = "Core";
        private const string tabGroupSteps = "Steps";
        private const string tabGroupOptions = "Options";
        private const string tabGroupActors = "Actors";
        private const string tabGroupCustom = "Custom";
        private const string horGroupStepsHeader = "_DefaultTabGroup/Steps/Header";
        private const string btGroupCore = "_DefaultTabGroup/Core/Buttons";
        private const string fdGroupCoreEval = "Evaluation";
        private const string fdGroupCoreActions = "_DefaultTabGroup/Core/Action Links";

        #if UNITY_EDITOR
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;
            
            // Ensure double underscore separator follows the event name
            // DataManagerText.TryAddingTextToLibrary (TextLibs.overworldEvents, $"ev_{key}__0c_name", textName);
            
            if (steps != null)
            {
                foreach (var kvp in steps)
                {
                    var stepKey = kvp.Key;
                    var step = kvp.Value;
                    if (step == null)
                        continue;

                    DataManagerText.TryAddingTextToLibrary (TextLibs.overworldEvents, $"ev_{key}__1s_{stepKey}_header", step.textName);
                    DataManagerText.TryAddingTextToLibrary (TextLibs.overworldEvents, $"ev_{key}__1s_{stepKey}_text", step.textDesc);

                    if (step.textVariants != null)
                    {
                        foreach (var kvp2 in step.textVariants)
                        {
                            var variantKey = kvp2.Key;
                            var variant = kvp2.Value;
                            if (variant == null)
                                continue;
                            
                            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldEvents, $"ev_{key}__1s_{stepKey}_header_tv_{variantKey}", variant.textHeader);
                            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldEvents, $"ev_{key}__1s_{stepKey}_text_tv_{variantKey}", variant.textContent);
                        }
                    }
                    
                    if (step.textVariantsGenerated != null)
                    {
                        foreach (var kvp2 in step.textVariantsGenerated)
                        {
                            var variantKey = kvp2.Key;
                            var variant = kvp2.Value;
                            if (variant == null)
                                continue;
                            
                            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldEvents, $"ev_{key}__1s_{stepKey}_header_tvg_{variantKey}", variant.textHeader);
                            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldEvents, $"ev_{key}__1s_{stepKey}_text_tvg_{variantKey}", variant.textContent);
                        }
                    }
                }
            }

            if (options != null)
            {
                foreach (var kvp in options)
                {
                    var optionKey = kvp.Key;
                    var option = kvp.Value;
                    if (option == null)
                        continue;

                    DataManagerText.TryAddingTextToLibrary (TextLibs.overworldEvents, $"ev_{key}__2o_{optionKey}_header", option.textHeader);
                    DataManagerText.TryAddingTextToLibrary (TextLibs.overworldEvents, $"ev_{key}__2o_{optionKey}_text", option.textContent);
                    
                    if (option.textVariants != null)
                    {
                        foreach (var kvp2 in option.textVariants)
                        {
                            var variantKey = kvp2.Key;
                            var variant = kvp2.Value;
                            if (variant == null)
                                continue;
                            
                            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldEvents, $"ev_{key}__2o_{optionKey}_header_tv_{variantKey}", variant.textHeader);
                            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldEvents, $"ev_{key}__2o_{optionKey}_text_tv_{variantKey}", variant.textContent);
                        }
                    }
                    
                    if (option.textVariantsGenerated != null)
                    {
                        foreach (var kvp2 in option.textVariantsGenerated)
                        {
                            var variantKey = kvp2.Key;
                            var variant = kvp2.Value;
                            if (variant == null)
                                continue;
                            
                            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldEvents, $"ev_{key}__2o_{optionKey}_header_tvg_{variantKey}", variant.textHeader);
                            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldEvents, $"ev_{key}__2o_{optionKey}_text_tvg_{variantKey}", variant.textContent);
                        }
                    }
                }
            }
        }

        private bool AreTabsVisible => DataMultiLinkerOverworldEvent.Presentation.tabsVisiblePerConfig;
        private Color GetTabColor (bool open) => Color.white.WithAlpha (open ? 0.5f : 1f);
        
        public bool IsTabWriting => DataMultiLinkerOverworldEvent.Presentation.tabWriting;

        [PropertyOrder (-10), GUIColor("@GetTabColor (IsTabWriting)"), ShowIf ("AreTabsVisible")]
        [Button ("Writing", ButtonSizes.Large), ButtonGroup]
        public void SetTabWriting ()
        {
            UtilityBool.Invert (ref DataMultiLinkerOverworldEvent.Presentation.tabWriting);
            if (IsTabWriting)
            {
                DataMultiLinkerOverworldEvent.Presentation.tabCore = false;
                DataMultiLinkerOverworldEvent.Presentation.tabSteps = false;
                DataMultiLinkerOverworldEvent.Presentation.tabOptions = false;
                DataMultiLinkerOverworldEvent.Presentation.tabOther = false;
            }
        }
        
        public bool IsTabCore => DataMultiLinkerOverworldEvent.Presentation.tabCore;
        public bool IsEvaluationVisible => DataMultiLinkerOverworldEvent.Presentation.tabCore && !forced;

        public bool IsEvaluationOverTime => evaluationGroup == EventEvaluationGroups.OnTime;

        [PropertyOrder (-10), GUIColor("@GetTabColor (IsTabCore)"), ShowIf ("AreTabsVisible")]
        [Button ("Core", ButtonSizes.Large), ButtonGroup]
        public void SetTabCore () 
        {
            UtilityBool.Invert (ref DataMultiLinkerOverworldEvent.Presentation.tabCore);
            if (IsTabCore && IsTabWriting)
                SetTabWriting ();
        }
        
        public bool IsTabSteps => DataMultiLinkerOverworldEvent.Presentation.tabSteps;

        [PropertyOrder (-10), GUIColor("@GetTabColor (IsTabSteps)"), ShowIf ("AreTabsVisible")]
        [Button ("Steps", ButtonSizes.Large), ButtonGroup]
        public void SetTabSteps () 
        {
            UtilityBool.Invert (ref DataMultiLinkerOverworldEvent.Presentation.tabSteps);
            if (IsTabSteps && IsTabWriting)
                SetTabWriting ();
        }
        
        public bool IsTabOptions => DataMultiLinkerOverworldEvent.Presentation.tabOptions;

        [PropertyOrder (-10), GUIColor("@GetTabColor (IsTabOptions)"), ShowIf ("AreTabsVisible")]
        [Button ("Options", ButtonSizes.Large), ButtonGroup]
        public void SetTabOptions () 
        {
            UtilityBool.Invert (ref DataMultiLinkerOverworldEvent.Presentation.tabOptions);
            if (IsTabOptions && IsTabWriting)
                SetTabWriting ();
        }
        
        public bool IsTabOther => DataMultiLinkerOverworldEvent.Presentation.tabOther;
        
        [PropertyOrder (-10), GUIColor("@GetTabColor (IsTabOther)"), ShowIf ("AreTabsVisible")]
        [Button ("Other", ButtonSizes.Large), ButtonGroup]
        public void SetTabOther () 
        {
            UtilityBool.Invert (ref DataMultiLinkerOverworldEvent.Presentation.tabOther);
            if (IsTabOther && IsTabWriting)
                SetTabWriting ();
        }

        [ShowIf ("IsTabOther")]
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataContainerOverworldEvent () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        private static bool IsCoreVisible => DataMultiLinkerOverworldEvent.Presentation.showCore;
        private static bool IsCheckVisible => DataMultiLinkerOverworldEvent.Presentation.showCheck;
        private static bool AreStepsVisible => DataMultiLinkerOverworldEvent.Presentation.showSteps;
        private static bool AreOptionsVisible => DataMultiLinkerOverworldEvent.Presentation.showOptions;
        private static bool AreActionsVisible => DataMultiLinkerOverworldEvent.Presentation.showActions;
        private static bool AreActorsVisible => DataMultiLinkerOverworldEvent.Presentation.showActors;
        private static bool IsCustomDataVisible => DataMultiLinkerOverworldEvent.Presentation.showCustomData;

        private IEnumerable<string> GetStepKeys () => 
            steps != null ? steps.Keys : null;

        private IEnumerable<string> GetEvaluationGroupKeys() =>
            FieldReflectionUtility.GetConstantStringFieldValues(typeof(EventEvaluationGroups));
        
        #endif
    }

    public static class OverworldEventFlagKeys
    {
        public const string Spawn_Hostile = "Spawn/Hostile";
    }
    
    public static class OverworldEventIntKeys
    {
        public const string Spawn_LevelDelta = "Spawn/LevelDelta";
    }
    
    public static class OverworldEventFloatKeys
    {
        public const string Spawn_Lifetime = "Spawn/Lifetime";
    }
    
    public static class OverworldEventStringKeys
    {
        public const string Spawn_Faction = "Spawn/Faction";
    }
    
    public static class OverworldEventSpawnDataKeys
    {
        public const string Spawn_Primary = "Spawn/Primary";
    }
}