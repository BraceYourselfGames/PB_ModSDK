using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerOverworldEvent : DataMultiLinker<DataContainerOverworldEvent>
    {
        public DataMultiLinkerOverworldEvent ()
        {
            textSectorKeys = new List<string> {TextLibs.overworldEvents};
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            DataMultiLinkerUtility.RegisterOnTextExport
            (
                dataType,
                () => TextLibraryHelper.OnBeforeTextExport (dataType, TextLibs.overworldEvents, "ev_"),
                () => TextLibraryHelper.OnAfterTextExport (dataType, TextLibs.overworldEvents)
            );
        }

        [HideReferenceObjectPicker]
        public class Presentation
        {
            public static bool tabWriting = false;
            public static bool tabCore = false;
            public static bool tabSteps = true;
            public static bool tabOptions = true;
            public static bool tabOther = false;

            private const string foldoutGroup = "Advanced options";

            private Color GetTabColor (bool open) => Color.white.WithAlpha (open ? 0.5f : 1f);

            [PropertyOrder (-10), GUIColor ("@GetTabColor (tabWriting)")]
            [Button ("Writing", ButtonSizes.Small), ButtonGroup]
            public static void SetTabWriting ()
            {
                tabWriting = !tabWriting;
                if (tabWriting)
                {
                    tabCore = false;
                    tabSteps = false;
                    tabOptions = false;
                    tabOther = false;
                }
            }

            [PropertyOrder (-10), GUIColor ("@GetTabColor (tabCore)")]
            [Button ("Core", ButtonSizes.Small), ButtonGroup]
            public void SetTabCore ()
            {
                tabCore = !tabCore;
                if (tabCore)
                    tabWriting = false;
            }

            [PropertyOrder (-10), GUIColor ("@GetTabColor (tabSteps)")]
            [Button ("Steps", ButtonSizes.Small), ButtonGroup]
            public void SetTabSteps ()
            {
                tabSteps = !tabSteps;
                if (tabSteps)
                    tabWriting = false;
            }

            [PropertyOrder (-10), GUIColor ("@GetTabColor (tabOptions)")]
            [Button ("Options", ButtonSizes.Small), ButtonGroup]
            public void SetTabOptions ()
            {
                tabOptions = !tabOptions;
                if (tabOptions)
                    tabWriting = false;
            }

            [PropertyOrder (-10), GUIColor ("@GetTabColor (tabOther)")]
            [Button ("Other", ButtonSizes.Small), ButtonGroup]
            public void SetTabOther ()
            {
                tabOther = !tabOther;
                if (tabOther)
                    tabWriting = false;
            }

            [FoldoutGroup (foldoutGroup, false)]
            [ShowInInspector, HideIf ("tabWriting")]
            public static bool tabsVisiblePerConfig = false;

            [FoldoutGroup (foldoutGroup, false)]
            [ShowInInspector, HideIf ("tabWriting")]
            public static bool showCore = true;

            [FoldoutGroup (foldoutGroup, false)]
            [ShowInInspector, HideIf ("tabWriting")]
            public static bool showTextVariants = true;

            [FoldoutGroup (foldoutGroup, false)]
            [ShowInInspector, HideIf ("tabWriting")]
            public static bool showTextVariantsGenerated = true;

            [FoldoutGroup (foldoutGroup, false)]
            [ShowInInspector, HideIf ("tabWriting")]
            public static bool showCheck = true;

            [FoldoutGroup (foldoutGroup, false)]
            [ShowInInspector, HideIf ("tabWriting")]
            public static bool showSteps = true;

            [FoldoutGroup (foldoutGroup, false)]
            [ShowInInspector, HideIf ("tabWriting")]
            public static bool showStepCore = true;
            
            [FoldoutGroup (foldoutGroup, false)]
            [ShowInInspector, HideIf ("tabWriting")]
            public static bool showStepImages = true;

            [FoldoutGroup (foldoutGroup, false)]
            [ShowInInspector, HideIf ("tabWriting")]
            public static bool showStepCheck = true;

            [FoldoutGroup (foldoutGroup, false)]
            [ShowInInspector, HideIf ("tabWriting")]
            public static bool showStepOptions = true;
            
            [FoldoutGroup (foldoutGroup, false)]
            [ShowInInspector, HideIf ("tabWriting")]
            public static bool showStepOptionsText = true;

            [FoldoutGroup (foldoutGroup, false)]
            [ShowInInspector, HideIf ("tabWriting")]
            public static bool showStepEffects = true;

            [FoldoutGroup (foldoutGroup, false)]
            [ShowInInspector, HideIf ("tabWriting")]
            public static bool showOptions = true;

            [FoldoutGroup (foldoutGroup, false)]
            [ShowInInspector, HideIf ("tabWriting")]
            public static bool showActors = true;

            [FoldoutGroup (foldoutGroup, false)]
            [ShowInInspector, HideIf ("tabWriting")]
            public static bool showActions = true;

            [FoldoutGroup (foldoutGroup, false)]
            [ShowInInspector, HideIf ("tabWriting")]
            public static bool showCustomData = true;

            [FoldoutGroup (foldoutGroup, false)]
            [ShowInInspector, HideIf ("tabWriting")]
            public static bool showLookups = false;
        }

        [FoldoutGroup ("View options", true), ShowInInspector, HideLabel]
        public Presentation presentation = new Presentation ();

        [ShowIf ("@!Presentation.tabWriting && Presentation.showLookups")]
        [ShowInInspector, ReadOnly]
        private static List<string> keysEvaluatedAtContact;

        [ShowIf ("@!DataMultiLinkerOverworldEvent.Presentation.tabWriting && DataMultiLinkerOverworldEvent.Presentation.showLookups")]
        [ShowInInspector, ReadOnly]
        private static List<string> keysEvaluatedAtInterval;

        [ShowIf ("@!DataMultiLinkerOverworldEvent.Presentation.tabWriting && DataMultiLinkerOverworldEvent.Presentation.showLookups")]
        [ShowInInspector, ReadOnly]
        private static List<string> keysEvaluatedAtIntervalShort;

        [ShowIf ("@!DataMultiLinkerOverworldEvent.Presentation.tabWriting && DataMultiLinkerOverworldEvent.Presentation.showLookups")]
        [ShowInInspector, ReadOnly]
        private static List<string> keysEvaluatedAtIntervalMedium;

        [ShowIf ("@!DataMultiLinkerOverworldEvent.Presentation.tabWriting && DataMultiLinkerOverworldEvent.Presentation.showLookups")]
        [ShowInInspector, ReadOnly]
        private static List<string> keysEvaluatedAtIntervalLong;

        [ShowIf("@!DataMultiLinkerOverworldEvent.Presentation.tabWriting && DataMultiLinkerOverworldEvent.Presentation.showLookups")]
        [ShowInInspector, ReadOnly]
        private static List<string> keysEvaluatedPostCombat;

        [ShowIf("@!DataMultiLinkerOverworldEvent.Presentation.tabWriting && DataMultiLinkerOverworldEvent.Presentation.showLookups")]
        [ShowInInspector, ReadOnly]
        private static List<string> keysEvaluatedOnProvinceLiberation;

        public static List<DataContainerOverworldEvent> eventsHidden = new List<DataContainerOverworldEvent> ();
        public static List<DataContainerOverworldEvent> eventsEvaluatedAtContact = new List<DataContainerOverworldEvent> ();
        public static List<DataContainerOverworldEvent> eventsEvaluatedAtInterval = new List<DataContainerOverworldEvent> ();
        public static List<DataContainerOverworldEvent> eventsEvaluatedAtIntervalShort = new List<DataContainerOverworldEvent> ();
        public static List<DataContainerOverworldEvent> eventsEvaluatedAtIntervalMedium = new List<DataContainerOverworldEvent> ();
        public static List<DataContainerOverworldEvent> eventsEvaluatedAtIntervalLong = new List<DataContainerOverworldEvent> ();
        public static List<DataContainerOverworldEvent> eventsEvaluatedPostCombat = new List<DataContainerOverworldEvent>();
        public static List<DataContainerOverworldEvent> eventsEvaluatedOnLiberation = new List<DataContainerOverworldEvent>();


        public static void OnAfterDeserialization ()
        {
            eventsHidden.Clear ();
            eventsEvaluatedAtContact.Clear ();
            eventsEvaluatedAtInterval.Clear ();
            eventsEvaluatedAtIntervalShort.Clear ();
            eventsEvaluatedAtIntervalMedium.Clear ();
            eventsEvaluatedAtIntervalLong.Clear ();
            eventsEvaluatedPostCombat.Clear();
            eventsEvaluatedOnLiberation.Clear();

            // Debug.LogWarning ($"Overworld event database loaded, setting up lookups");

            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var container = kvp.Value;

                if (container.hidden || container.forced)
                    eventsHidden.Add (container);
                else
				{
                    if (container.evaluationGroup == EventEvaluationGroups.OnContact)
                        eventsEvaluatedAtContact.Add(container);
                    else if (container.evaluationGroup == EventEvaluationGroups.OnPostCombat)
                        eventsEvaluatedPostCombat.Add(container);
                    else if (container.evaluationGroup == EventEvaluationGroups.OnProvinceLiberated)
                        eventsEvaluatedOnLiberation.Add(container);
					else
					{
                        eventsEvaluatedAtInterval.Add (container);
                        switch (container.evaluationInterval)
                        {
                            case OverworldEventEvaluationInterval.Short:
                                eventsEvaluatedAtIntervalShort.Add (container);
                                break;
                            case OverworldEventEvaluationInterval.Medium:
                                eventsEvaluatedAtIntervalMedium.Add (container);
                                break;
                            case OverworldEventEvaluationInterval.Long:
                                eventsEvaluatedAtIntervalLong.Add (container);
                                break;
                            default:
                                eventsEvaluatedAtIntervalShort.Add (container);
                                break;
                        }
                    }
                }
            }

            keysEvaluatedAtContact = eventsEvaluatedAtContact.Select (f => f.key).ToList ();
            keysEvaluatedAtInterval = eventsEvaluatedAtInterval.Select (f => f.key).ToList ();
            keysEvaluatedAtIntervalShort = eventsEvaluatedAtIntervalShort.Select (f => f.key).ToList ();
            keysEvaluatedAtIntervalMedium = eventsEvaluatedAtIntervalMedium.Select (f => f.key).ToList ();
            keysEvaluatedAtIntervalLong = eventsEvaluatedAtIntervalLong.Select (f => f.key).ToList ();
            keysEvaluatedPostCombat = eventsEvaluatedPostCombat.Select(f => f.key).ToList();
            keysEvaluatedOnProvinceLiberation = eventsEvaluatedOnLiberation.Select(f => f.key).ToList();
        }
        
        [Button]
        public void DebugOutputAllEventsWithInterval(OverworldEventEvaluationInterval intervalKey)
        {
            var s = string.Empty;
            foreach (var d in data)
            {
                if (d.Value.evaluationGroup == EventEvaluationGroups.OnTime && d.Value.evaluationInterval == intervalKey&& !d.Value.hidden)
                {
                    s += (d.Value.chance + " \t"+ d.Key + "\n");
                  
                }
            }
            Debug.Log(s);
        }
        
        [Button]
        public void MigrateEvents(OverworldEventEvaluationInterval intervalKey)
        {
            var s = string.Empty;
            foreach (var d in data)
            {
                if (d.Value.evaluationGroup == EventEvaluationGroups.OnTime && d.Value.evaluationInterval == intervalKey&& !d.Value.hidden)
                {
                   d.Value.chance += 0.75f;
                   d.Value.chance = Mathf.Clamp01(d.Value.chance);
                   d.Value.chance = (float)Math.Round(d.Value.chance, 2);
                   s += (d.Value.chance + " \t"+ d.Key + "\n");
                }
            }
            Debug.Log(s);
        }

        [Button]
        public void CheckPriority()
        {
            var s = string.Empty;
            foreach (var d in data)
            {
                if (d.Key.Contains("notification")  )
                {
                    s += (d.Value.priority +  " \t" +d.Key + "\n");
                  
                  

                }
            }
            Debug.Log(s);
        }

        [GUIColor ("@GetSaveButtonColor (UnsavedChangesPossibleInRelatedDatabases)"), PropertyOrder (-10)]
        private void SaveAllDatabases ()
        {
            SaveData ();
            DataMultiLinkerOverworldEventOption.SaveData ();
        }

        [HideInInspector]
        private string SaveAllButtonText =>
            UnsavedChangesPossibleInRelatedDatabases ? "Save data (all related databases)*" : "Save data (all related databases)";

        private bool UnsavedChangesPossibleInRelatedDatabases =>
            unsavedChangesPossible ||
            DataMultiLinkerOverworldEventOption.unsavedChangesPossible;

        /*
        [FoldoutGroup ("Utilities", false)]
        [Button ("Upgrade actor calls", ButtonSizes.Large), PropertyOrder (-10)]
        public void UpgradeActorCalls ()
        {
            foreach (var kvp in data)
            {
                var eventData = kvp.Value;

                if (eventData.steps != null)
                {
                    foreach (var kvp2 in eventData.steps)
                    {
                        var stepKey = kvp2.Key;
                        var stepData = kvp2.Value;
                        var context = $"Event {eventData.key} | Step: {stepKey}";

                        UpgradeActorCalls (eventData, stepData.calls, context);
                    }
                }

                if (eventData.options != null)
                {
                    foreach (var kvp2 in eventData.options)
                    {
                        var optionKey = kvp2.Key;
                        var optionData = kvp2.Value;
                        var context = $"Event {eventData.key} | Option: {optionKey}";

                        UpgradeActorCalls (eventData, optionData.calls, context);
                    }
                }
            }
        }
        */


        #if UNITY_EDITOR

        [FoldoutGroup ("Utilities", false)]
        [Button (), PropertyOrder (-10)]
        public void LogGeneratedTextVariants ()
        {
            foreach (var kvp in data)
            {
                var eventData = kvp.Value;
                foreach (var kvs in eventData.steps)
                {
                    if (kvs.Value.textVariantsGenerated != null)
                    {
                        Debug.Log ($"{kvp.Key}: generated text in step {kvs.Key}");
                    }
                }
            }
        }

        [FoldoutGroup ("Utilities", false)]
        [Button (), PropertyOrder (-10)]
        public void LogNonStandardPilotKeys ()
        {
            var s = string.Empty;
            foreach (var kvp in data)
            {
                s += $"Event: {kvp.Key}\n";

                if (kvp.Value.actorsPilots != null)
                    foreach (var kvs in kvp.Value.actorsPilots)
                    {
                        if (kvs.Key != "pilot_0")
                        {
                            s += $"- Pilot Key: {kvs.Key}\n";
                        }
                    }
            }

            Debug.Log (s);
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button (), PropertyOrder (-10)]
        public void LogEventRefreshOnExit ()
        {
            var listRefreshed = new List<string> ();
            var listSilent = new List<string> ();
            
            foreach (var kvp in data)
            {
                var ev = kvp.Value;
                if (ev.refreshEventsOnExit)
                    listRefreshed.Add (ev.key);
                else
                {
                    listSilent.Add (ev.key);
                    ev.refreshEventsOnExit = true;
                }
            }

            Debug.Log ($"Refresh on exit ({listRefreshed.Count}):\n{listRefreshed.ToStringFormatted (true, multilinePrefix: "- ")}");
            Debug.LogWarning ($"No refresh on exit ({listSilent.Count}):\n{listSilent.ToStringFormatted (true, multilinePrefix: "- ")}");
        }

        [FoldoutGroup ("Utilities", false)]
        [Button ("Generate text comments", ButtonSizes.Large), PropertyOrder (-10)]
        public void GenerateDevComments ()
        {
            var textSector = DataManagerText.GetLibrarySector (TextLibs.overworldEvents);
            if (textSector == null)
                return;

            foreach (var kvp in data)
            {
                var eventKey = kvp.Key;
                var eventData = kvp.Value;

                if (eventData.steps != null)
                {
                    foreach (var kvp2 in eventData.steps)
                    {
                        var stepKey = kvp2.Key;
                        var textEntry = DataManagerText.GetTextEntryFromLibrary (TextLibs.overworldEvents, $"ev_{eventKey}__1s_{stepKey}_text");
                        if (textEntry == null)
                            continue;

                        var step = kvp2.Value;
                        var desc = OverworldEventUtility.PrintStepDescription (step, false);
                        textEntry.noteDev = new DataBlockTextNote { text = desc };
                    }
                }

                if (eventData.options != null)
                {
                    foreach (var kvp2 in eventData.options)
                    {
                        var optionKey = kvp2.Key;
                        var textEntry = DataManagerText.GetTextEntryFromLibrary (TextLibs.overworldEvents, $"ev_{eventKey}__2o_{optionKey}_header");
                        if (textEntry == null)
                            continue;

                        var option = kvp2.Value;
                        var desc = OverworldEventUtility.PrintOptionDescription (option, false);
                        textEntry.noteDev = new DataBlockTextNote { text = desc };
                    }
                }
            }

            foreach (var kvp in DataMultiLinkerOverworldEventOption.data)
            {
                var optionKey = kvp.Key;
                var textEntry = DataManagerText.GetTextEntryFromLibrary (TextLibs.overworldEvents, $"os_{optionKey}__header");
                if (textEntry == null)
                    continue;

                var option = kvp.Value;
                var desc = OverworldEventUtility.PrintOptionDescription (option, false);
                textEntry.noteDev = new DataBlockTextNote { text = desc };
            }

            DataManagerText.SaveLibrary ();
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-10)]
        public void LogBlockedOptions ()
        {
            foreach (var kvp in data)
            {
                var eventKey = kvp.Key;
                var eventData = kvp.Value;

                if (eventData.steps != null)
                {
                    foreach (var kvp2 in eventData.steps)
                    {
                        var stepKey = kvp2.Key;
                        var step = kvp2.Value;
                        
                        if (step.options == null)
                            continue;

                        foreach (var optionLink in step.options)
                        {
                            if (optionLink == null)
                                continue;

                            DataContainerOverworldEventOption optionData = null;
                            if (optionLink.shared)
                                optionData = DataMultiLinkerOverworldEventOption.GetEntry (optionLink.key);
                            else if (eventData.options != null)
                            {
                                if (string.IsNullOrEmpty (optionLink.key) || !eventData.options.ContainsKey (optionLink.key))
                                {
                                    Debug.LogWarning ($"Failed to find custom event option using key \"{optionLink.key}\" referenced by step \"{stepKey}\" in event \"{eventData.key}\"");
                                    continue;
                                }

                                optionData = eventData.options[optionLink.key];
                            }

                            if (optionData == null)
                            {
                                Debug.LogWarning ($"Failed to find event option data \"{optionLink.key}\" referenced by step \"{stepKey}\" in event \"{eventData.key}\"");
                                continue;
                            }

                            if (!optionData.checkPreventsUnlock && optionData.check != null)
                            {
                                var desc = OverworldEventUtility.PrintCheckDescription (optionData.check);
                                desc = desc.Replace ("[aa]", string.Empty).Replace ("[ff]", string.Empty);
                                Debug.LogWarning ($"Event {eventKey} step {stepKey} option {optionLink.key} is displayed even when the check fails. Verify this is appropriate based on the check:\n\n{optionData.textHeader}\n{optionData.textContent}\n\n{desc}");
                            }
                        }
                    }
                }
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-10)]
        public void LogCustomCombat ()
        {
            var sb = new StringBuilder ();
            int combatOptionsTotal = 0;
            var combatOptionsCustom = new List<string> ();
            var combatOptionsGeneric = new List<string> ();
            var scenariosList = new List<string> ();
            var areasListFromSite = new List<string> ();
            var areasListFromScenario = new List<string> ();

            var areasListMain = new List<string> ();
            var areasListMainTemp = DataTagUtility.GetKeysWithTags (DataMultiLinkerCombatArea.data, new Dictionary<string, bool> { { "type_main", true } });
            areasListMain.AddRange (areasListMainTemp);

            foreach (var kvp in data)
            {
                var eventKey = kvp.Key;
                var eventData = kvp.Value;
                if (eventData.hidden)
                    continue;

                if (eventData.steps != null)
                {
                    foreach (var kvp2 in eventData.steps)
                    {
                        var stepKey = kvp2.Key;
                        var step = kvp2.Value;
                        
                        if (step.options == null)
                            continue;

                        foreach (var optionLink in step.options)
                        {
                            if (optionLink == null)
                                continue;

                            DataContainerOverworldEventOption optionData = null;
                            if (optionLink.shared)
                                optionData = DataMultiLinkerOverworldEventOption.GetEntry (optionLink.key);
                            else if (eventData.options != null)
                            {
                                if (string.IsNullOrEmpty (optionLink.key) || !eventData.options.ContainsKey (optionLink.key))
                                {
                                    Debug.LogWarning ($"Failed to find custom event option using key \"{optionLink.key}\" referenced by step \"{stepKey}\" in event \"{eventData.key}\"");
                                    continue;
                                }

                                optionData = eventData.options[optionLink.key];
                            }

                            if (optionData == null)
                            {
                                Debug.LogWarning ($"Failed to find event option data \"{optionLink.key}\" referenced by step \"{stepKey}\" in event \"{eventData.key}\"");
                                continue;
                            }

                            if (optionData.combat == null)
                                continue;

                            var co = optionData.combat;
                            bool custom = !co.scenarioTagsFromTarget;
                            int index = custom ? combatOptionsCustom.Count : combatOptionsGeneric.Count;
                            index += 1;

                            var textHeader = $"\n\n{index:00} ____________\nEvent {eventKey}\n- Type: {eventData.evaluationGroup}\n- Step: \"{step.textName}\" (key \"{stepKey}\", {(stepKey == eventData.stepOnStart ? "starting" : "nested")})\n- Option: \"{optionLink.GetOption ()?.textHeader}\" (key \"{optionLink.key}\")";
                            var textOptional = co.optional ? "Player can leave" : "Player can't leave";
                            var textCustomTags = co.scenarioTags != null && co.scenarioTags.Count > 0 ? $"Scenario filter:\n{co.scenarioTags.ToStringFormattedKeyValuePairs (true, multilinePrefix: "  - ")}" : "No scenario filter";
                            var text = $"{textHeader}\n- {textOptional}\n- {textCustomTags}";
                            
                            var stepOnStartFound = eventData.steps.TryGetValue (eventData.stepOnStart, out var stepOnStart);
                            if (stepOnStartFound)
                            {
                                if (stepOnStart.check != null)
                                {
                                    var textCheck = OverworldEventUtility.PrintCheckDescription (stepOnStart.check, false, true);
                                    textCheck = textCheck.Replace ("[aa]", string.Empty);
                                    textCheck = textCheck.Replace ("[ff]", string.Empty);
                                    text += textCheck;
                                }
                                
                                scenariosList.Clear ();
                                areasListFromSite.Clear ();
                                areasListFromScenario.Clear ();

                                if (stepOnStart.check?.target?.tags != null)
                                {
                                    var blueprintKeysByTag = DataTagUtility.GetKeysWithTags (DataMultiLinkerOverworldEntityBlueprint.data, stepOnStart.check.target.tags);
                                    if (blueprintKeysByTag.Count > 0)
                                    {
                                        foreach (var blueprintKey in blueprintKeysByTag)
                                        {
                                            var bp = DataMultiLinkerOverworldEntityBlueprint.GetEntry (blueprintKey, false);
                                            if (bp != null)
                                            {
                                                if (bp.scenariosProcessed != null)
                                                {
                                                    var s = bp.scenariosProcessed.tags;
                                                    var scenarios = DataTagUtility.GetContainersWithTags (DataMultiLinkerScenario.data, s);

                                                    foreach (var kvp3 in scenarios)
                                                    {
                                                        var scenario = kvp3 as DataContainerScenario;
                                                        var scenarioKey = scenario.key;
                                                        if (!scenariosList.Contains (scenarioKey))
                                                            scenariosList.Add (scenarioKey);
                                                    }
                                                }

                                                if (bp.areasProcessed != null)
                                                {
                                                    var s = bp.areasProcessed.tags;
                                                    var areas = DataTagUtility.GetContainersWithTags (DataMultiLinkerCombatArea.data, s);

                                                    foreach (var kvp3 in areas)
                                                    {
                                                        var area = kvp3 as DataContainerCombatArea;
                                                        var areaKey = area.key;
                                                        if (!areasListFromSite.Contains (areaKey))
                                                            areasListFromSite.Add (areaKey);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                bool areasFromSitesUsed = true;
                                var scenarioContext = scenariosList.Count > 0 ? "from sites filtered by event" : "no context";
                                    
                                if (co.scenarioTags != null && co.scenarioTags.Count > 0)
                                {
                                    scenarioContext = "from option";
                                    scenariosList.Clear ();
                                    var scenarios = DataTagUtility.GetKeysWithTags (DataMultiLinkerScenario.data, co.scenarioTags);
                                        scenariosList.AddRange (scenarios);
                                }

                                if (scenariosList.Count > 0)
                                {
                                    scenariosList.Sort ();
                                    var textScenarios = $"\n\nScenarios {scenarioContext}:\n{scenariosList.ToStringFormatted (true, multilinePrefix: "- ")}";
                                    text += textScenarios;

                                    foreach (var scenarioKey in scenariosList)
                                    {
                                        var scenario = DataMultiLinkerScenario.GetEntry (scenarioKey, false);
                                        if (scenario != null)
                                        {
                                            if (scenario.areasProc.tagFilterUsed)
                                            {
                                                var areaKeysByTag = DataTagUtility.GetKeysWithTags (DataMultiLinkerCombatArea.data, scenario.areasProc.tagFilter);
                                                foreach (var areaKey in areaKeysByTag)
                                                {
                                                    if (!areasListFromScenario.Contains (areaKey))
                                                        areasListFromScenario.Add (areaKey);
                                                }
                                            }
                                            else
                                            {
                                                areasFromSitesUsed = false;
                                                var areaKey = scenario.areasProc.keys?.GetRandomEntry ();
                                                if (!string.IsNullOrEmpty (areaKey) && !areasListFromScenario.Contains (areaKey))
                                                    areasListFromScenario.Add (areaKey);
                                            }
                                        }
                                    }
                                    
                                    if (areasListFromScenario.Count > 0)
                                    {
                                        areasListFromScenario.Sort ();

                                        if (areasFromSitesUsed && areasListFromSite.Count > 0)
                                        {
                                            for (int i = areasListFromScenario.Count - 1; i >= 0; --i)
                                            {
                                                var areaKey = areasListFromScenario[i];
                                                if (!areasListFromSite.Contains (areaKey))
                                                    areasListFromScenario.RemoveAt (i);
                                            }
                                        }

                                        if (areasListFromScenario.SequenceEqual (areasListMain))
                                        {
                                            var textAreas = $"\n\nAreas (from scenarios):\n- main_* (all general purpose levels)";
                                            text += textAreas;
                                        }
                                        else
                                        {
                                            var textAreas = $"\n\nAreas (from scenarios):\n{areasListFromScenario.ToStringFormatted (true, multilinePrefix: "- ")}";
                                            text += textAreas;

                                            if (areasListFromSite.SequenceEqual (areasListFromScenario))
                                                areasFromSitesUsed = false;
                                        }
                                    }
                                }
                                
                                if (areasFromSitesUsed && areasListFromSite.Count > 0)
                                {
                                    areasListFromSite.Sort ();
                                    
                                    if (areasListFromScenario.SequenceEqual (areasListMain))
                                    {
                                        var textAreas = $"\n\nAreas (from sites filtered by event):\n- main_* (all general purpose levels)";
                                        text += textAreas;
                                    }
                                    else
                                    {
                                        var textAreas = $"\n\nAreas (from sites filtered by event):\n{areasListFromSite.ToStringFormatted (true, multilinePrefix: "- ")}";
                                        text += textAreas;
                                    }
                                }
                            }

                            combatOptionsTotal += 1;
                            if (custom)
                                combatOptionsCustom.Add (text);
                            else
                                combatOptionsGeneric.Add (text);
                        }
                    }
                }
            }

            sb.Clear ();
            sb.Append ($"Generic combat options (scenario influenced by site) - {combatOptionsGeneric.Count}:\n");
            foreach (var co in combatOptionsGeneric)
            {
                sb.Append (co);
            }
            
            var report = sb.ToString ();
            Debug.Log (report);
            
            sb.Clear ();
            sb.Append ($"Override combat options (site ignored) - {combatOptionsCustom.Count}:\n");
            foreach (var co in combatOptionsCustom)
            {
                sb.Append (co);
            }

            report = sb.ToString ();
            Debug.Log (report);
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-10)]
        public void LogStepImages ()
        {
            var lookup = new SortedDictionary<string, int> ();
        
            foreach (var kvp in data)
            {
                var eventKey = kvp.Key;
                var eventData = kvp.Value;

                if (eventData.steps != null)
                {
                    foreach (var kvp2 in eventData.steps)
                    {
                        var stepKey = kvp2.Key;
                        var step = kvp2.Value;
                        
                        if (string.IsNullOrEmpty (step.image))
                        {
                            Debug.LogWarning ($"Event {eventKey} step {stepKey} has no image!");
                            continue;
                        }

                        if (!lookup.ContainsKey (step.image))
                            lookup.Add (step.image, 1);
                        else
                            lookup[step.image] += 1;
                    }
                }
            }
            
            Debug.Log ($"Event illustration use:\n{lookup.ToStringFormattedKeyValuePairs (true)}");
        }
        
        [FoldoutGroup("Utilities", false)]
        [Button("Log event chances", ButtonSizes.Large), PropertyOrder(-10)]
        public void LogEventChance ()
		{
            foreach (var kvp in data)
            {
                var eventKey = kvp.Key;
                if (eventKey.Contains ("ftue"))
                    continue;
                
                var eventData = kvp.Value;
                if (eventData.hidden || eventData.evaluationGroup != EventEvaluationGroups.OnTime || eventData.chanceModifier == null)
                    continue;

                var ch = eventData.chanceModifier;
                bool chanceRemovedOnCompletionOnly = ch.completionOnBase.RoughlyEqual (0f) && !ch.entryOnBase.RoughlyEqual (0f);
                if (!chanceRemovedOnCompletionOnly)
                    continue;

                if (eventData.steps != null && eventData.steps.TryGetValue (eventData.stepOnStart, out var stepOnStart))
                {
                    int optionsCount = stepOnStart.options.Count;
                    int optionsCountCompleting = 0;
                    foreach (var optionLink in stepOnStart.options)
                    {
                        DataContainerOverworldEventOption optionData = null;
                        if (optionLink.shared)
                            optionData = DataMultiLinkerOverworldEventOption.GetEntry (optionLink.key);
                        else if (eventData.options != null)
                        {
                            if (string.IsNullOrEmpty (optionLink.key) || !eventData.options.ContainsKey (optionLink.key))
                            {
                                Debug.LogWarning ($"Failed to find custom event option using key \"{optionLink.key}\" referenced by step \"{kvp.Key}\" in event \"{eventData.key}\"");
                                continue;
                            }

                            optionData = eventData.options[optionLink.key];
                        }
                        
                        if (optionData == null)
                            continue;

                        if (optionData.completing)
                            optionsCountCompleting += 1;
                    }
                    
                    if (optionsCountCompleting < optionsCount)
                        Debug.LogWarning ($"Event {kvp.Key} is blocked from reoccurring on completion but {optionsCountCompleting}/{optionsCount} options are completing");
                }
                else
                    Debug.LogWarning ($"Event {kvp.Key} is blocked from reoccurring on completion, no options checked");
            }
		}


        /*
        [FoldoutGroup ("Utilities", false)]
        [Button (ButtonSizes.Large), PropertyOrder (-10)]
        public static void UpgradeResourceChanges ()
        {
            foreach (var kvp in data)
            {
                var evt = kvp.Value;

                if (evt.options != null)
                {
                    foreach (var kvp2 in evt.options)
                    {
                        var option = kvp2.Value;
                        var context = $"event {kvp.Key} option {kvp2.Key}";
                        UpgradeResourceChanges (ref option.resourceChanges, ref option.functions, option.resourceChangePreventsUnlock, context);
                    }
                }
            }
            
            foreach (var kvp2 in DataMultiLinkerOverworldEventOption.data)
            {
                var option = kvp2.Value;
                var context = $"shared option {kvp2.Key}";
                UpgradeResourceChanges (ref option.resourceChanges, ref option.functions, option.resourceChangePreventsUnlock, context);
            }
        }

        private static void UpgradeResourceChanges (ref List<DataBlockResourceChange> resourceChanges, ref List<IOverworldEventFunction> functions, bool strict, string context)
        {
            if (resourceChanges == null || resourceChanges.Count == 0)
                return;

            if (functions == null)
                functions = new List<IOverworldEventFunction> ();

            foreach (var change in resourceChanges)
                change.checkStrict = strict;

            Debug.Log ($"Upgrading resource changes in {context}:\n{resourceChanges.ToStringFormatted (true, multilinePrefix: "- ")}");
            functions.Add (new ModifyResources { resourceChanges = resourceChanges });

            resourceChanges = null;
        }
        */

#endif

        /*
        [FoldoutGroup ("Utilities", false)]
        [Button (ButtonSizes.Large), PropertyOrder (-10)]
        public static void LogFunctionCoverage ()
        {
            var classesAvailable = new Dictionary<string, Type> ();
            foreach (var type in Assembly.GetExecutingAssembly ().DefinedTypes)
            {
                if (typeof (IOverworldEventFunction).IsAssignableFrom (type))
                    classesAvailable[type.Name] = type;
            }

            var functionKeysOld = FieldReflectionUtility.GetConstantStringFieldNames (typeof (OverworldEventFunctionKeys));
            var functionKeysMissing = new List<string> ();
            int functionKeysCovered = 0;

            foreach (var functionKey in functionKeysOld)
            {
                if (!classesAvailable.ContainsKey (functionKey))
                    functionKeysMissing.Add (functionKey);
                else
                    functionKeysCovered += 1;
            }
            
            if (functionKeysMissing.Count == 0)
                Debug.Log ($"All {functionKeysOld.Count} overworld event functions covered");
            else
                Debug.LogWarning ($"{functionKeysMissing.Count}/{functionKeysOld.Count} overworld event functions missing:\n{functionKeysMissing.ToStringFormatted (true, multilinePrefix: "- ")}");
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button (ButtonSizes.Large), PropertyOrder (-10)]
        public static void UpgradeSpawnFunctions ()
        {
            foreach (var kvp in data)
            {
                var blueprint = kvp.Value;
                
                if (blueprint.steps != null)
                {
                    foreach (var kvp2 in blueprint.steps)
                    {
                        var step = kvp2.Value;
                        var context = $"event {kvp.Key} step {kvp2.Key}";
                        UpgradeSpawnFunctionsInEvent (step.functions, blueprint, context);
                    }
                }
                
                if (blueprint.options != null)
                {
                    foreach (var kvp2 in blueprint.options)
                    {
                        var option = kvp2.Value;
                        var context = $"event {kvp.Key} option {kvp2.Key}";
                        UpgradeSpawnFunctionsInEvent (option.functions, blueprint, context);
                    }
                }

                blueprint.customSpawnData = null;
            }

            foreach (var kvp in DataMultiLinkerOverworldEventOption.data)
            {
                var option = kvp.Value;
                var context = $"standalone option {kvp.Key}";
                UpgradeSpawnFunctionsInEvent (option.functions, null, context);
            }
        }

        private static void UpgradeSpawnFunctionsInEvent (List<IOverworldEventFunction> functions, DataContainerOverworldEvent blueprint, string context)
        {
            if (functions == null)
                return;
            
            foreach (var function in functions)
            {
                if (function == null)
                    continue;
                
                var functionTyped = function as CreateOverworldEntity;
                if (functionTyped == null)
                    continue;
                
                UpgradeSpawnFunctionInEvent (functionTyped, blueprint, context);
            }
        }

        private static void UpgradeSpawnFunctionInEvent (CreateOverworldEntity function, DataContainerOverworldEvent blueprint, string context)
        {
            if (string.IsNullOrEmpty (function.spawnDataKey))
            {
                Debug.LogWarning ($"Failed to update spawn function in {context}: string is null or empty");
                return;
            }

            if (blueprint == null || blueprint.customSpawnData == null || !blueprint.customSpawnData.TryGetValue (function.spawnDataKey, out var spawnData))
            {
                Debug.LogWarning ($"Failed to update spawn function in {context}: no data found for spawn key {function.spawnDataKey}");
                return;
            }

            Debug.Log ($"Updated spawn function in {context}, spawn data now embedded");
            function.spawnData = UtilitiesYAML.CloneThroughYaml (spawnData);
            function.spawnData.spawnLookupKey = function.spawnDataKey;
            function.spawnDataKey = null;
        }
        */

        /*

        [FoldoutGroup ("Utilities", false)]
        [Button ("Log calls", ButtonSizes.Large), PropertyOrder (-10)]
        public static void DumpFunctions ()
        {
            var rows = new List<string> ();
            foreach (var evt in data)
            {
                if (evt.Value.options != null)
                {
                    foreach (var option in evt.Value.options)
                    {
                        if ((option.Value.calls == null) != (option.Value.functions == null))
                        {
                            Debug.LogError ($"{evt.Key} opt {option.Key} has calls: {option.Value.calls != null} != has functions: {option.Value.functions != null}");
                            continue;
                        }
                        
                        if (option.Value.calls != null)
                        {
                            if (option.Value.calls.Count != option.Value.functions.Count)
                            {
                                Debug.LogError ($"{evt.Key} opt {option.Key} calls: {option.Value.calls.Count} != functions: {option.Value.functions.Count}");
                                continue;
                            }

                            for (var i = 0; i < option.Value.calls.Count; i++)
                            {
                                var call = option.Value.calls[i];
                                var function = option.Value.functions[i];

                                rows.Add ($"{evt.Key}\topt\t{option.Key}\t{call.key}\t{call.early}\t" +
                                          $"{DumpArgType (call.arg1)}\t{DumpArg (call.arg1)}\t" +
                                          $"{DumpArgType (call.arg2)}\t{DumpArg (call.arg2)}\t" +
                                          $"{function.GetType ().Name}: {JsonUtility.ToJson (function)}");
                            }
                        }
                    }
                }

                if (evt.Value.steps != null)
                {
                    foreach (var step in evt.Value.steps)
                    {
                        if ((step.Value.calls == null) != (step.Value.functions == null))
                        {
                            Debug.LogError ($"{evt.Key} opt {step.Key} has calls: {step.Value.calls != null} != has functions: {step.Value.functions != null}");
                            continue;
                        }
                        
                        if (step.Value.calls != null)
                        {
                            if (step.Value.calls.Count != step.Value.functions.Count)
                            {
                                Debug.LogError ($"{evt.Key} opt {step.Key} calls: {step.Value.calls.Count} != functions: {step.Value.functions.Count}");
                                continue;
                            }

                            for (var i = 0; i < step.Value.calls.Count; i++)
                            {
                                var call = step.Value.calls[i];
                                var function = step.Value.functions[i];
                                
                                rows.Add ($"{evt.Key}\tstep\t{step.Key}\t{call.key}\t{call.early}\t" +
                                          $"{DumpArgType (call.arg1)}\t{DumpArg (call.arg1)}\t" +
                                          $"{DumpArgType (call.arg2)}\t{DumpArg (call.arg2)}\t" +
                                          $"{function.GetType ().Name}: {JsonUtility.ToJson (function)}");
                            }
                        }
                    }
                }
            }

            GUIUtility.systemCopyBuffer = string.Join ("\n", rows);
        }
        */

        /*
        static string DumpArgType (EventCallArg arg)
        {
            return arg?.GetType ().Name;
        }

        static string DumpArg (EventCallArg arg)
        {
            return arg switch
            {
                EventCallArgFloat f => f.value.ToString (CultureInfo.InvariantCulture),
                EventCallArgInt i => i.value.ToString (),
                EventCallArgString s => s.value.ToString (),
                EventCallArgStringList l => string.Join (", ", l.value),
                _ => ""
            };
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button ("Convert calls to functions", ButtonSizes.Large), PropertyOrder (-10)]
        public static void TransferFunctions ()
        {
            var types = new Dictionary<string, Type> ();

            foreach (var type in Assembly.GetExecutingAssembly ().DefinedTypes)
            {
                if (typeof (OverworldEventFunction).IsAssignableFrom (type))
                {
                    types[type.Name] = type;
                }
            }

            foreach (var kvp in DataMultiLinkerOverworldEventOption.data)
            {
                var option = kvp.Value;
                if (option.calls != null)
                {
                    option.functions = new List<OverworldEventFunction> ();
                    foreach (var call in option.calls)
                    {
                        Debug.Log ($"Shared option {kvp.Key} has call {call.key}");
                        if (call.key != null && types.TryGetValue (call.key, out var type))
                        {
                            var function = Activator.CreateInstance (type) as OverworldEventFunction;

                            try
                            {
                                function.Convert (call.arg1, call.arg2);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError ($"error converting: {call.key}({DumpArg (call.arg1)}, {DumpArg (call.arg2)})");
                                Debug.LogException (e);
                            }

                            option.functions.Add (function);
                            Debug.Log ($"- Converted to function object {function.GetType ().Name}");
                        }
                        else
                        {
                            Debug.LogWarning ("Couldn't find OverworldEventFunction called " + call.key);
                        }
                    }

                    option.calls = null;
                }
            }

            foreach (var evt in data)
            {
                if (evt.Value.options != null)
                {
                    foreach (var option in evt.Value.options)
                    {
                        if (option.Value.calls != null)
                        {
                            option.Value.functions = new List<OverworldEventFunction> ();
                            foreach (var call in option.Value.calls)
                            {
                                if (call.key != null && types.TryGetValue (call.key, out var type))
                                {
                                    var function = Activator.CreateInstance (type) as OverworldEventFunction;
                                    
                                    try
                                    {
                                        function.Convert (call.arg1, call.arg2);
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.LogError ($"error converting: {call.key}({DumpArg (call.arg1)}, {DumpArg (call.arg2)})");
                                        Debug.LogException (e);
                                    }

                                    option.Value.functions.Add (function);
                                }
                                else
                                {
                                    Debug.LogWarning ("Couldn't find OverworldEventFunction called " + call.key);
                                }
                            }

                            option.Value.calls = null;
                        }
                    }
                }

                if (evt.Value.steps != null)
                {
                    foreach (var step in evt.Value.steps)
                    {
                        if (step.Value.calls != null)
                        {
                            step.Value.functions = new List<OverworldEventFunction> ();
                            foreach (var call in step.Value.calls)
                            {
                                if (call.key != null && types.TryGetValue (call.key, out var type))
                                {
                                    var function = Activator.CreateInstance (type) as OverworldEventFunction;
                                    
                                    try
                                    {
                                        function.Convert (call.arg1, call.arg2);
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.LogError ($"error converting: {call.key}({DumpArg (call.arg1)}, {DumpArg (call.arg2)})");
                                        Debug.LogException (e);
                                    }
                                    
                                    step.Value.functions.Add (function);
                                }
                                else
                                {
                                    Debug.LogWarning ("Couldn't find OverworldEventFunction called " + call.key);
                                }
                            }
                            
                            step.Value.calls = null;
                        }
                    }
                }
            }
        }
        
        */

        /*
        [FoldoutGroup ("Utilities", false)]
        [Button ("Upgrade actors", ButtonSizes.Large), PropertyOrder (-10)]
        public void UpgradeActors ()
        {
            var actorKeyLookupWorld = new Dictionary<int, string> ();
            var actorKeyLookupUnits = new Dictionary<int, string> ();
            var actorKeyLookupPilots = new Dictionary<int, string> ();

            int actorsSites = 0;
            int actorsUnits = 0;
            int actorsPilots = 0;
            
            int memoryChangesSites = 0;
            int memoryChangesUnits = 0;
            int memoryChangesPilots = 0;
            
            int actionOwnerSites = 0;
            int actionTargetSites = 0;
            int actionTargetUnits = 0;
            int actionTargetPilots = 0;
        
            foreach (var kvp in data)
            {
                var eventData = kvp.Value;
                
                actorKeyLookupWorld.Clear ();
                actorKeyLookupUnits.Clear ();
                actorKeyLookupPilots.Clear ();
                
                if (eventData.actorsWorld != null)
                {
                    eventData.actorsSites = new Dictionary<string, DataBlockOverworldEventActorWorld> ();
                    
                    for (int i = 0; i < eventData.actorsWorld.Count; ++i)
                    {
                        var actorSlot = eventData.actorsWorld[i];
                        var actorKey = $"world_{i}";
                        
                        eventData.actorsSites.Add (actorKey, actorSlot);
                        actorKeyLookupWorld.Add (i, actorKey);
                        actorsSites += 1;
                    }
                    
                    eventData.actorsWorld = null;
                }
                
                if (eventData.actorUnits != null)
                {
                    eventData.actorsUnits = new Dictionary<string, DataBlockOverworldEventActorUnit> ();
                    
                    for (int i = 0; i < eventData.actorUnits.Count; ++i)
                    {
                        var actorSlot = eventData.actorUnits[i];
                        var actorKey = $"unit_{i}";
                        
                        eventData.actorsUnits.Add (actorKey, actorSlot);
                        actorKeyLookupUnits.Add (i, actorKey);
                        actorsUnits += 1;
                    }
                    
                    eventData.actorUnits = null;
                }
                
                if (eventData.actorPilots != null)
                {
                    eventData.actorsPilots = new Dictionary<string, DataBlockOverworldEventActorPilot> ();
                    
                    for (int i = 0; i < eventData.actorPilots.Count; ++i)
                    {
                        var actorSlot = eventData.actorPilots[i];
                        var actorKey = $"pilot_{i}";
                        
                        eventData.actorsPilots.Add (actorKey, actorSlot);
                        actorKeyLookupPilots.Add (i, actorKey);
                        actorsPilots += 1;
                    }
                    
                    eventData.actorPilots = null;
                }

                if (eventData.steps != null)
                {
                    foreach (var kvp2 in eventData.steps)
                    {
                        var stepKey = kvp2.Key;
                        var stepData = kvp2.Value;
                        var context = $"Event {eventData.key} | Step: {stepKey}";
                        
                        if (stepData.memoryChanges != null)
                        {
                            UpgradeActorsInMemoryChanges
                            (
                                stepData.memoryChanges,
                                actorKeyLookupWorld, actorKeyLookupUnits, actorKeyLookupPilots,
                                ref memoryChangesSites, ref memoryChangesUnits, ref memoryChangesPilots,
                                context
                            );
                        }
                        
                        if (stepData.actionsCreated != null)
                        {
                            UpgradeActorsInActionsCreated
                            (
                                stepData.actionsCreated,
                                actorKeyLookupWorld, actorKeyLookupUnits, actorKeyLookupPilots,
                                ref actionOwnerSites, ref actionTargetSites, ref actionTargetUnits, ref actionTargetPilots,
                                context
                            );
                        }
                    }
                }

                if (eventData.options != null)
                {
                    foreach (var kvp2 in eventData.options)
                    {
                        var optionKey = kvp2.Key;
                        var optionData = kvp2.Value;
                        var context = $"Event {eventData.key} | Option: {optionKey}";
                        
                        if (optionData.memoryChanges != null)
                        {
                            UpgradeActorsInMemoryChanges
                            (
                                optionData.memoryChanges,
                                actorKeyLookupWorld, actorKeyLookupUnits, actorKeyLookupPilots,
                                ref memoryChangesSites, ref memoryChangesUnits, ref memoryChangesPilots,
                                context
                            );
                        }

                        if (optionData.actionsCreated != null)
                        {
                            UpgradeActorsInActionsCreated
                            (
                                optionData.actionsCreated,
                                actorKeyLookupWorld, actorKeyLookupUnits, actorKeyLookupPilots,
                                ref actionOwnerSites, ref actionTargetSites, ref actionTargetUnits, ref actionTargetPilots,
                                context
                            );
                        }
                    }
                }
            }

            Debug.Log ($"Total actors:\n- Sites: {actorsSites}\n- Units: {actorsUnits}\n- Pilots: {actorsPilots}");
            Debug.Log ($"Total memory changes to actors:\n- Sites: {memoryChangesSites}\n- Units: {memoryChangesUnits}\n- Pilots: {memoryChangesPilots}");
            Debug.Log ($"Total actions spawned with actors:\n- Sites (owners): {actionOwnerSites}\n- Sites (targets): {actionTargetSites}\n- Units (targets): {actionTargetUnits}\n- Pilots (targets): {actionTargetPilots}");
        }

        private void UpgradeActorsInMemoryChanges
        (
            List<DataBlockMemoryChangeGroupEvent> memoryChanges,
            Dictionary<int, string> actorKeyLookupWorld, Dictionary<int, string> actorKeyLookupUnits, Dictionary<int, string> actorKeyLookupPilots,
            ref int memoryChangesSites, ref int memoryChangesUnits, ref int memoryChangesPilots,
            string context
        )
        {
            if (memoryChanges == null)
                return;

            foreach (var group in memoryChanges)
            {
                if (group.context == MemoryChangeContextEvent.ActorWorld)
                {
                    memoryChangesSites += 1;
                    var actorKeyFound = actorKeyLookupWorld.TryGetValue (group.actorIndex, out var actorKey);
                    if (actorKeyFound)
                    {
                        group.actorKey = actorKey;
                        Debug.Log ($"{context} | Memory change to site actor ({group.context}) index: {group.actorIndex} | Actor key: {group.actorKey}");
                    }
                    else
                    {
                        Debug.LogWarning ($"{context} | Memory change to site actor ({group.context}) index: {group.actorIndex} | Failed to find actor key, available lookup: {actorKeyLookupWorld.ToStringFormattedKeyValuePairs ()}");
                    }
                }
                
                else if (group.context == MemoryChangeContextEvent.ActorUnit)
                {
                    memoryChangesUnits += 1;
                    var actorKeyFound = actorKeyLookupUnits.TryGetValue (group.actorIndex, out var actorKey);
                    if (actorKeyFound)
                    {
                        group.actorKey = actorKey;
                        Debug.Log ($"{context} | Memory change to unit actor ({group.context}) index: {group.actorIndex} | Actor key: {group.actorKey}");
                    }
                    else
                    {
                        Debug.LogWarning ($"{context} | Memory change to unit actor ({group.context}) index: {group.actorIndex} | Failed to find actor key, available lookup: {actorKeyLookupWorld.ToStringFormattedKeyValuePairs ()}");
                    }
                }
                
                else if (group.context == MemoryChangeContextEvent.ActorPilot)
                {
                    memoryChangesPilots += 1;
                    var actorKeyFound = actorKeyLookupPilots.TryGetValue (group.actorIndex, out var actorKey);
                    if (actorKeyFound)
                    {
                        group.actorKey = actorKey;
                        Debug.Log ($"{context} | Memory change to pilot actor ({group.context}) index: {group.actorIndex} | Actor key: {group.actorKey}");
                    }
                    else
                    {
                        Debug.LogWarning ($"{context} | Memory change to pilot actor ({group.context}) index: {group.actorIndex} | Failed to find actor key, available lookup: {actorKeyLookupWorld.ToStringFormattedKeyValuePairs ()}");
                    }
                }
            }
        }

        private void UpgradeActorsInActionsCreated 
        (
            List<DataBlockOverworldActionInstanceData> actionsCreated, 
            Dictionary<int, string> actorKeyLookupWorld, Dictionary<int, string> actorKeyLookupUnits, Dictionary<int, string> actorKeyLookupPilots, 
            ref int actionOwnerSites, ref int actionTargetSites, ref int actionTargetUnits, ref int actionTargetPilots, 
            string context
        )
        {
            if (actionsCreated == null)
                return;
            
            foreach (var actionData in actionsCreated)
            {
                if (actionData.owner == ActionOwnerProvider.ActorSite || actionData.owner == ActionOwnerProvider.ActorSiteProvince)
                {
                    actionOwnerSites += 1;
                    var actorKeyFound = actorKeyLookupWorld.TryGetValue (actionData.ownerIndex, out var actorKey);
                    if (actorKeyFound)
                    {
                        actionData.ownerKey = actorKey;
                        Debug.Log ($"{context} | Action owner resolved through site actor ({actionData.owner}) index: {actionData.ownerIndex} | Actor key: {actionData.ownerKey}");
                    }
                    else
                    {
                        Debug.LogWarning ($"{context} | Action owner resolved through site actor ({actionData.owner}) index: {actionData.ownerIndex} | Failed to find actor key, available lookup: {actorKeyLookupWorld.ToStringFormattedKeyValuePairs ()}");
                    }
                }
                
                if (actionData.target == ActionTargetProvider.ActorSite || actionData.target == ActionTargetProvider.ActorSiteProvince)
                {
                    actionTargetSites += 1;
                    var actorKeyFound = actorKeyLookupWorld.TryGetValue (actionData.targetIndex, out var actorKey);
                    if (actorKeyFound)
                    {
                        actionData.targetKey = actorKey;
                        Debug.Log ($"{context} | Action target resolved through site actor ({actionData.owner}) index: {actionData.ownerIndex} | Actor key: {actionData.targetKey}");
                    }
                    else
                    {
                        Debug.LogWarning ($"{context} | Action target resolved through site actor ({actionData.owner}) index: {actionData.ownerIndex} | Failed to find actor key, available lookup: {actorKeyLookupWorld.ToStringFormattedKeyValuePairs ()}");
                    }
                }
                
                else if (actionData.target == ActionTargetProvider.ActorUnit)
                {
                    actionTargetUnits += 1;
                    var actorKeyFound = actorKeyLookupUnits.TryGetValue (actionData.targetIndex, out var actorKey);
                    if (actorKeyFound)
                    {
                        actionData.targetKey = actorKey;
                        Debug.Log ($"{context} | Action target resolved through unit actor ({actionData.owner}) index: {actionData.ownerIndex} | Actor key: {actionData.targetKey}");
                    }
                    else
                    {
                        Debug.LogWarning ($"{context} | Action target resolved through unit actor ({actionData.owner}) index: {actionData.ownerIndex} | Failed to find actor key, available lookup: {actorKeyLookupWorld.ToStringFormattedKeyValuePairs ()}");
                    }
                }
                
                else if (actionData.target == ActionTargetProvider.ActorPilot)
                {
                    actionTargetPilots += 1;
                    var actorKeyFound = actorKeyLookupPilots.TryGetValue (actionData.targetIndex, out var actorKey);
                    if (actorKeyFound)
                    {
                        actionData.targetKey = actorKey;
                        Debug.Log ($"{context} | Action target resolved through pilot actor ({actionData.owner}) index: {actionData.ownerIndex} | Actor key: {actionData.targetKey}");
                    }
                    else
                    {
                        Debug.LogWarning ($"{context} | Action target resolved through pilot actor ({actionData.owner}) index: {actionData.ownerIndex} | Failed to find actor key, available lookup: {actorKeyLookupWorld.ToStringFormattedKeyValuePairs ()}");
                    }
                }
            }
        }
        */

        /*
        private void UpgradeOptionCombat (DataContainerOverworldEventOption option, DataContainerOverworldEvent parentEvent)
        {
            if (option == null || option.calls == null || option.calls.Count == 0)
                return;

            var optionKey = option.key;
            var context = parentEvent != null ? $"event {parentEvent.key}, embedded option {optionKey}" : $"shared option {optionKey}";
            
            for (int i = option.calls.Count - 1; i >= 0; --i)
            {
                var call = option.calls[i];
                if (call == null)
                    continue;

                bool startFromTagsOnEvent = call.key == OverworldEventFunctionKeys.StartCombatFromTagsOnEvent;
                bool startFromTagsOnEntity = call.key == OverworldEventFunctionKeys.StartCombatFromTagsOnEntity;
                bool startFromTagsCombined = call.key == OverworldEventFunctionKeys.StartCombatFromTagsCombined;
                bool start = startFromTagsOnEvent || startFromTagsOnEntity || startFromTagsCombined;
                if (!start)
                    continue;
                
                option.calls.RemoveAt (i);
                bool scenarioTagsFromTarget = startFromTagsOnEntity || startFromTagsCombined;
                bool scenarioTagsFromEvent = startFromTagsOnEvent || startFromTagsCombined;
                
                option.combat = new DataBlockOverworldEventCombat ();
                option.combat.scenarioTagsFromTarget = scenarioTagsFromTarget;

                if (!scenarioTagsFromEvent)
                {
                    Debug.Log ($"Converted call {call.key} from {context} to combat block | Tags from target: {true} | Tags from event: false");
                    continue;
                }

                if (parentEvent == null)
                {
                    Debug.LogWarning ($"Failed to extract additional tags for call {call.key} from {context}: no parent scenario");
                    continue;
                }

                if (call.arg1 == null || call.arg1 is EventCallArgString == false)
                {
                    Debug.LogWarning ($"Failed to extract additional tags for call {call.key} from {context}: wrong type of first argument");
                    continue;
                }

                var tagFilterKey = ((EventCallArgString)call.arg1).value;
                var tagFilterFound = parentEvent.TryGetTagFilter (tagFilterKey, out var tagFilterFromEvent);
                
                if (!tagFilterFound)
                {
                    Debug.LogWarning ($"Failed to extract additional tags for call {call.key} from {context}: no tag collection found using key {tagFilterKey}");
                    continue;
                }

                option.combat.scenarioTags = new SortedDictionary<string, bool> (tagFilterFromEvent);
                parentEvent.customTagFilters.Remove (tagFilterKey);
                if (parentEvent.customTagFilters.Count == 0)
                    parentEvent.customTagFilters = null;
                
                Debug.Log ($"Converted call {call.key} from {context} to combat block | Tags from target: {scenarioTagsFromTarget} | Tags from event:\n{option.combat.scenarioTags.ToStringFormattedKeyValuePairs (true)}");
            }
        }
        */

        /*
        
        [Button ("Upgrade checks", ButtonSizes.Large), PropertyOrder (-10)]
        public void UpgradeChecks ()
        {
            foreach (var kvp in DataMultiLinkerOverworldEvent.data)
            {
                var container = kvp.Value;
                if (container.steps != null)
                {
                    foreach (var kvp2 in container.steps)
                    {
                        var stepKey = kvp2.Key;
                        var step = kvp2.Value;
                        UpgradeCheck (step.check);
                    }
                }

                if (container.options != null)
                {
                    foreach (var kvp2 in container.options)
                    {
                        var optionKey = kvp2.Key;
                        var option = kvp2.Value;
                        UpgradeCheck (option.check);
                    }
                }
            }

            foreach (var kvp in DataMultiLinkerOverworldEventOption.data)
            {
                var optionKey = kvp.Key;
                var option = kvp.Value;
                UpgradeCheck (option.check);
            }
        }

        private void UpgradeCheck (DataBlockOverworldEventCheck c)
        {
            if (c == null)
                return;
            
            if (c.self != null && c.self.supplies != null)
            {
                var s = c.self.supplies;
                c.self.supplies = null;
                c.self.resources = new Dictionary<string, DataBlockOverworldEventSubcheckInt>
                {
                    {
                        ResourceKeys.supplies,
                        new DataBlockOverworldEventSubcheckInt
                        {
                            check = s.check,
                            value = s.value
                        }
                    }
                };
            }
            
            if (c.target != null && c.target.supplies != null)
            {
                var s = c.target.supplies;
                c.target.supplies = null;
                c.target.resources = new Dictionary<string, DataBlockOverworldEventSubcheckInt>
                {
                    {
                        ResourceKeys.supplies,
                        new DataBlockOverworldEventSubcheckInt
                        {
                            check = s.check,
                            value = s.value
                        }
                    }
                };
            }
        }
        
        */
    }
}