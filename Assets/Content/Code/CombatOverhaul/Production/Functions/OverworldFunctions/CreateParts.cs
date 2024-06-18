using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using UnityEngine;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CreateParts : IOverworldEventFunction, IOverworldActionFunction, IOverworldFunction, ICombatFunction
    {
        public bool log = false;
        
        [ListDrawerSettings (DefaultExpandedState = true, CustomAddFunction = "@new DataBlockVirtualParts ()")]
        public List<DataBlockVirtualParts> parts = new List<DataBlockVirtualParts> ();
        
        private static List<string> keysFiltered = new List<string> ();

        public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
        {
            Run ();
        }
        
        public void Run (OverworldActionEntity source)
        {
            Run ();
        }
        
        public void Run ()
        {
            #if !PB_MODSDK

            var basePersistent = IDUtility.playerBasePersistent;
            if (basePersistent == null || parts == null)
                return;

            var persistent = Contexts.sharedInstance.persistent;
            int levelBase = persistent.hasWorkshopLevel ? persistent.workshopLevel.level : 1;

            foreach (var description in parts)
            {
                DataContainerQualityTable qualityTable = null;
                if (!string.IsNullOrEmpty (description.qualityTableKey))
                    DataMultiLinkerQualityTable.data.TryGetValue (description.qualityTableKey, out qualityTable);

                int count = description.countRandom ? UnityEngine.Random.Range (description.countMin, description.countMax + 1) : description.countMin;
                if (description.tagsUsed)
                {
                    var partPresetsMatchingTags = DataTagUtility.GetKeysWithTags (DataMultiLinkerPartPreset.data, description.tags, returnAllOnEmptyFilter: true);
                    keysFiltered.Clear ();

                    foreach (var partPresetCandidateKey in partPresetsMatchingTags)
                    {
                        var partPresetCandidate = DataMultiLinkerPartPreset.GetEntry (partPresetCandidateKey);
                        if (partPresetCandidate == null || partPresetCandidate.hidden)
                            continue;
                                
                        if (partPresetCandidate.tagsProcessed != null && partPresetCandidate.tagsProcessed.Contains (EquipmentTags.incompatible))
                            continue;

                        keysFiltered.Add (partPresetCandidateKey);
                    }

                    if (keysFiltered.Count > 0)
                    {
                        for (int i = 0; i < count; ++i) //part count loop
                        {
                            int rating = 1;
                            if (qualityTable != null)
                                rating = qualityTable.RollRandomQuality ();

                            var presetName = keysFiltered.GetRandomEntry ();
                            int level = description.levelRandom ? UnityEngine.Random.Range (description.levelMin, description.levelMax + 1) : description.levelMin;
                            level += levelBase;

                            var part = UnitUtilities.CreatePartEntityFromPreset (presetName, level: level, rating: rating);
                            if (part != null)
                            {
                                part.ReplaceSalvageSelection (false);
                                EquipmentUtility.AttachPartToInventory (part, basePersistent, true, log);
                            }
                        }
                    }
                    else
                        Debug.LogWarning ($"Failed to resolve virtual part group with tags {description.tags.ToStringFormattedKeyValuePairs ()}");
                }
                else
                {
                    var partPreset = DataMultiLinkerPartPreset.GetEntry (description.preset);
                    if (partPreset == null)
                        continue;

                    for (int i = 0; i < count; ++i)
                    {
                        int rating = 1;
                        if (qualityTable != null)
                            rating = qualityTable.RollRandomQuality ();
                        
                        var presetName = description.preset;
                        int level = description.levelRandom ? UnityEngine.Random.Range (description.levelMin, description.levelMax + 1) : description.levelMin;
                        level += levelBase;

                        var part = UnitUtilities.CreatePartEntityFromPreset (presetName, level: level, rating: rating);
                        if (part != null)
                        {
                            part.ReplaceSalvageSelection (false);
                            EquipmentUtility.AttachPartToInventory (part, basePersistent, true, log);
                        }
                    }
                }
            }
            
            #endif
        }
    }
}