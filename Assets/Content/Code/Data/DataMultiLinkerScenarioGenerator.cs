using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerScenarioGenerator : DataMultiLinker<DataContainerScenarioGenerator>
    {
        public DataMultiLinkerScenarioGenerator ()
        {
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.scenarioGenerators); 
        }
        
        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool showCore = true;
            
            [ShowInInspector]
            public static bool showInheritance = false;
            
            [ShowInInspector]
            public static bool showTagCollections = false;
        }

        [ShowInInspector] [HideLabel]
        public Presentation presentation = new Presentation ();
        
        [ShowIf ("@DataMultiLinkerScenarioGenerator.Presentation.showTagCollections")]
        [ShowInInspector]
        public static HashSet<string> tags = new HashSet<string> ();

        [ShowIf ("@DataMultiLinkerScenarioGenerator.Presentation.showTagCollections")]
        [ShowInInspector] [ReadOnly]
        public static Dictionary<string, HashSet<string>> tagsMap = new Dictionary<string, HashSet<string>> ();
        
        public static HashSet<string> GetTags ()
        {
            LoadDataChecked ();
            return tags;
        }
        
        private static StringBuilder sb = new StringBuilder ();
        
        public static void OnAfterDeserialization ()
        {
            // Process every subsystem recursively first
            foreach (var kvp in data)
                ProcessRecursiveStart (kvp.Value);

            // Fill parents after recursive processing is done on all presets, ensuring lack of cyclical refs etc
            foreach (var kvp1 in data)
            {
                var presetA = kvp1.Value;
                if (presetA == null)
                    continue;

                var key = kvp1.Key;
                
                if (presetA.children != null)
                    presetA.children.Clear ();

                foreach (var kvp2 in data)
                {
                    var presetB = kvp2.Value;
                    if (presetB.parents == null || presetB.parents.Count == 0)
                        continue;

                    foreach (var link in presetB.parents)
                    {
                        if (link.key == key)
                        {
                            if (presetA.children == null)
                                presetA.children = new List<string> ();
                            presetA.children.Add (presetB.key);
                        }
                    }
                }
            }
            
            DataTagUtility.RegisterTags (data, ref tags, ref tagsMap);

            foreach (var kvp1 in data)
                Postprocess (kvp1.Value);
        }
        
        private static List<DataContainerScenarioGenerator> childrenUpdated = new List<DataContainerScenarioGenerator> ();
        
        public static void ProcessRelated (DataContainerScenarioGenerator origin)
        {
            if (origin == null)
                return;

            childrenUpdated.Clear ();
            childrenUpdated.Add (origin);
            
            if (origin.children != null)
            {
                foreach (var childKey in origin.children)
                {
                    var child = GetEntry (childKey);
                    if (child != null)
                        childrenUpdated.Add (child);
                }
            }
            
            foreach (var child in childrenUpdated)
            {
                // Avoid localization refresh and other losses on origin
                if (child != origin)
                    child.OnAfterDeserialization (child.key);
            }

            foreach (var child in childrenUpdated)
                ProcessRecursiveStart (child);

            foreach (var child in childrenUpdated)
                Postprocess (child);
        }
        
        public static void ProcessRecursiveStart (DataContainerScenarioGenerator origin)
        {
            if (origin == null)
                return;

            origin.coreProc = null;
            origin.tagsProc = null;
            origin.textListProc = null;
            origin.checksGlobalProc = null;
            origin.checksBaseProc = null;
            origin.scenarioFilterProc = null;
            origin.scenarioChangesProc = null;
            origin.squadsProc = null;
            origin.rewardPoolProc = null;
            origin.effectsOnCompletionProc = null;

            ProcessRecursive (origin, origin, 0);
        }
        
        public static void Postprocess (DataContainerScenarioGenerator target)
        {
            // Add any code that requires fully filled processed fields here, e.g. procgen text based on merged data
        }

        private static void ProcessRecursive (DataContainerScenarioGenerator current, DataContainerScenarioGenerator root, int depth)
        {
            if (current == null || root == null)
            {
                Debug.LogWarning ($"Received null step or root generator reference while validating generator hierarchy");
                return;
            }

            if (depth > 0 && current == root)
            {
                Debug.LogWarning ($"Encountered dependency loop at depth level {depth} when processing generator {root.key}");
                return;
            }

            if (current.core != null && root.coreProc == null)
                root.coreProc = current.core;
            
            if (current.tags != null && current.tags.Count > 0)
            {
                if (root.tagsProc == null)
                    root.tagsProc = new HashSet<string> ();

                foreach (var tag in current.tags)
                {
                    if (!string.IsNullOrEmpty (tag) && !root.tagsProc.Contains (tag))
                        root.tagsProc.Add (tag);
                }
            }
            
            if (current.textList != null && root.textListProc == null)
                root.textListProc = current.textList;
            
            if (current.escalation != null && root.escalationProc == null)
                root.escalationProc = current.escalation;
            
            if (current.levelOffset != null && root.levelOffsetProc == null)
                root.levelOffsetProc = current.levelOffset;
            
            if (current.checksGlobal != null && root.checksGlobalProc == null)
                root.checksGlobalProc = current.checksGlobal;
            
            if (current.checksBase != null && root.checksBaseProc == null)
                root.checksBaseProc = current.checksBase;
            
            if (current.scenarioFilter != null && root.scenarioFilterProc == null)
                root.scenarioFilterProc = current.scenarioFilter;
            
            if (current.scenarioChanges != null && root.scenarioChangesProc == null)
                root.scenarioChangesProc = current.scenarioChanges;
            
            // if (current.squads != null && root.squadsProc == null)
            //     root.squadsProc = current.squads;

            if (current.squads != null && current.squads.Count > 0)
            {
                if (root.squadsProc == null)
                    root.squadsProc = new List<DataBlockScenarioGeneratorSquad> ();

                foreach (var squad in current.squads)
                {
                    if (squad == null || squad.slots == null || squad.slots.Count == 0)
                        continue;

                    root.squadsProc.Add (squad);
                }
            }
            
            if (current.squadFilter != null && root.squadFilterProc == null)
                root.squadFilterProc = current.squadFilter;
            
            // if (current.rewardPool != null && root.rewardPoolProc == null)
            //     root.rewardPoolProc = current.rewardPool;
            
            if (current.rewardPool != null && current.rewardPool.Count > 0)
            {
                if (root.rewardPoolProc == null)
                    root.rewardPoolProc = new List<DataBlockScenarioGeneratorRewards> ();

                foreach (var rewardPool in current.rewardPool)
                {
                    if (rewardPool == null || rewardPool.rewards == null || rewardPool.rewards.Count == 0)
                        continue;

                    root.rewardPoolProc.Add (rewardPool);
                }
            }
            
            // if (current.effectsOnCompletion != null && root.effectsOnCompletionProc == null)
            //     root.effectsOnCompletionProc = current.effectsOnCompletion;
            
            if (current.effectsOnCompletion != null && current.effectsOnCompletion.Count > 0)
            {
                if (root.effectsOnCompletionProc == null)
                    root.effectsOnCompletionProc = new List<DataBlockCampaignEffect> ();

                foreach (var effect in current.effectsOnCompletion)
                {
                    if (effect == null)
                        continue;

                    root.effectsOnCompletionProc.Add (effect);
                }
            }
            
            // Just in case we somehow missed a cyclical dependency
            if (depth > 20)
            {
                Debug.LogWarning ($"Generator {current.key} fails to complete recursive processing in under 20 steps.");
                return;
            }

            // No parents further up
            if (current.parents == null || current.parents.Count == 0)
                return;

            for (int i = 0, count = current.parents.Count; i < count; ++i)
            {
                var link = current.parents[i];
                if (link == null || string.IsNullOrEmpty (link.key))
                {
                    Debug.LogWarning ($"Generator {current.key} has null or empty parent link at index {i}!");
                    continue;
                }

                if (link.key == current.key)
                {
                    Debug.LogWarning ($"Generator {current.key} has invalid parent key matching itself at index {i}");
                    continue;
                }

                var parent = GetEntry (link.key, false);
                if (parent == null)
                {
                    Debug.LogWarning ($"Generator {current.key} has invalid parent key {link.key} at index {i} that can't be found in the database");
                    continue;
                }
                
                // Append next hierarchy level for easier preview
                if (parent.parents != null && parent.parents.Count > 0)
                {
                    sb.Clear ();
                    for (int i2 = 0, count2 = parent.parents.Count; i2 < count2; ++i2)
                    {
                        if (i2 > 0)
                            sb.Append (", ");

                        var parentOfParent = parent.parents[i2];
                        if (parentOfParent == null || string.IsNullOrEmpty (parentOfParent.key))
                            sb.Append ("—");
                        else
                            sb.Append (parentOfParent.key);
                    }

                    link.hierarchy = sb.ToString ();
                }
                else
                    link.hierarchy = "—";
                
                ProcessRecursive (parent, root, depth + 1);
            }
        }
    }
}


