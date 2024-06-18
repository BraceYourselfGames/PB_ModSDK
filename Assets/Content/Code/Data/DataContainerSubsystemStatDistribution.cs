using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockStatDistributionSecondary
    {
        [HideLabel]
        [SuffixLabel ("@GetSuffixLabelText ()")]
        [PropertyRange (0f, 1f)]
        public float multiplier = 1f;

        #if UNITY_EDITOR

        [YamlIgnore][HideInInspector]
        public float duplication = 1;
        
        private string GetSuffixLabelText () =>
            duplication == 1 ? "—" : $"✕ {duplication} = {(duplication * multiplier):G4}";
        
        #endif
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockStatDistribution
    {
        [HideLabel]
        [ShowIf ("@parent != null && parent.uniform")]
        [SuffixLabel ("@GetSuffixLabelText ()")]
        [PropertyRange (0f, 1f)]
        public float multiplier = 1f;
        
        [HideLabel]
        [HideIf ("@parent != null && parent.uniform")]
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.Stat)]
        public SortedDictionary<string, DataBlockStatDistributionSecondary> multipliersPerStat;
        
        #if UNITY_EDITOR
        
        [YamlIgnore][HideInInspector]
        public DataContainerSubsystemStatDistribution parent;
        
        [YamlIgnore][HideInInspector]
        public float duplication = 1;
        
        private string GetSuffixLabelText () =>
            duplication == 1 ? "—" : $"✕ {duplication} = {(duplication * multiplier):G4}";
        
        #endif
    }
    
    [Serializable][LabelWidth (140f)][HideReferenceObjectPicker]
    public class DataContainerSubsystemStatDistribution : DataContainer
    {
        [PropertyOrder (0)]
        public bool uniform = true;
        
        [PropertyOrder (1)]
        [InlineButton ("Fill", "Auto-fill")]
        public string hardpointFilter = "mech_external";
        
        [PropertyOrder (2)]
        [OnValueChanged ("ValidateStats")]
        [ValueDropdown ("@DataMultiLinkerUnitStats.data.Keys")]
        public List<string> stats = new List<string> ();
        
        [PropertyOrder (10)]
        [InfoBox ("@GetWarningText ()", InfoMessageType.Warning, "IsDistributionNonNormalized")]
        [OnValueChanged ("UpdateInspectorData")]
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.Hardpoint)]
        public SortedDictionary<string, DataBlockStatDistribution> hardpoints = new SortedDictionary<string, DataBlockStatDistribution> ();

        private SortedDictionary<string, float> nonUniformSums = new SortedDictionary<string,float> ();

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
            
            ValidateStats ();
            foreach (var kvp in hardpoints)
            {
                var block = kvp.Value;
                if (uniform && block.multipliersPerStat != null)
                    block.multipliersPerStat = null;
                else if (!uniform && block.multipliersPerStat == null)
                    block.multipliersPerStat = new SortedDictionary<string, DataBlockStatDistributionSecondary> ();
            }
            
            #if UNITY_EDITOR
            UpdateInspectorData ();
            #endif
        }

        public override void OnBeforeSerialization ()
        {
            base.OnBeforeSerialization ();
            
            ValidateStats ();
            foreach (var kvp in hardpoints)
            {
                var block = kvp.Value;
                if (uniform && block.multipliersPerStat != null)
                    block.multipliersPerStat = null;
            }
        }

        private float GetUniformSum ()
        {
            if (hardpoints == null)
                return 0f;

            var sum = 0f;
            foreach (var kvp in hardpoints)
            {
                var hardpointInfo = DataMultiLinkerSubsystemHardpoint.GetEntry (kvp.Key, false);
                if (hardpointInfo == null)
                    continue;

                int duplication = hardpointInfo.duplication;
                sum += kvp.Value.multiplier * duplication;
            }

            return sum;
        }
        
        private SortedDictionary<string, float> GetNonUniformSums ()
        {
            nonUniformSums.Clear ();
            if (hardpoints == null)
                return nonUniformSums;

            foreach (var stat in stats)
            {
                var sum = 0f;
                foreach (var kvp in hardpoints)
                {
                    var block = kvp.Value;
                    if (block.multipliersPerStat == null || !block.multipliersPerStat.ContainsKey (stat))
                        continue;
                    
                    var hardpointInfo = DataMultiLinkerSubsystemHardpoint.GetEntry (kvp.Key);
                    if (hardpointInfo == null)
                        continue;

                    int duplication = hardpointInfo.duplication;
                    sum += block.multipliersPerStat[stat].multiplier * duplication;
                }
                
                nonUniformSums.Add (stat, sum);
            }

            return nonUniformSums;
        }





        private StringBuilder sb = new StringBuilder ();
        
        public void ValidateStats ()
        {
            if (stats != null)
            {
                stats = new HashSet<string> (stats).ToList ();
                stats.Sort ();
            }
        }

        private bool IsDistributionNonNormalized ()
        {
            if (hardpoints == null)
                return false;
            
            if (uniform)
            {
                var sum = GetUniformSum ();
                return !sum.RoughlyEqual (1f);
            }
            else
            {
                var sums = GetNonUniformSums ();
                foreach (var kvp in sums)
                {
                    if (!kvp.Value.RoughlyEqual (1f))
                        return true;
                }
                
                return false;
            }
        }

        private string GetWarningText ()
        {
            if (uniform)
            {
                var sum = GetUniformSum ();
                if (!sum.RoughlyEqual (1f))
                    return $"The distribution isn't normalized, the total is: {sum}";
                else
                    return "The distribution is normal";
            }
            else
            {
                sb.Clear ();
                sb.Append ("The distribution isn't normalized on at least one of the stats: ");
                
                var sums = GetNonUniformSums ();
                foreach (var kvp in sums)
                {
                    sb.Append ("\n");
                    sb.Append (kvp.Key);
                    sb.Append (": ");
                    sb.Append (kvp.Value);
                }

                return sb.ToString ();
            }
        }
        
        private void Fill ()
        {
            hardpoints = new SortedDictionary<string, DataBlockStatDistribution> ();
            var hardpointData = DataMultiLinkerSubsystemHardpoint.data;
            
            foreach (var kvp in hardpointData)
            {
                var hardpointKey = kvp.Key;
                var hardpointInfo = kvp.Value;
                
                if (hardpointInfo.visual || !hardpointKey.Contains (hardpointFilter))
                    continue;
                
                var block = new DataBlockStatDistribution ();
                hardpoints.Add (hardpointKey, block);
                
                block.multiplier = 1f;
                if (uniform)
                    block.multipliersPerStat = null;
                else
                {
                    block.multipliersPerStat = new SortedDictionary<string, DataBlockStatDistributionSecondary> ();
                    for (int i = 0; i < stats.Count; ++i)
                        block.multipliersPerStat.Add (stats[i], new DataBlockStatDistributionSecondary { multiplier = 1f });
                }
            }
#if UNITY_EDITOR
            UpdateInspectorData ();
#endif
        }

        [PropertyOrder (9)]
        [ShowIf ("IsDistributionNonNormalized")]
        [Button ("Normalize")]
        private void Normalize ()
        {
            if (hardpoints == null || hardpoints.Count == 0 || !IsDistributionNonNormalized ())
                return;

            if (uniform)
            {
                var sum = GetUniformSum ();
                
                if (sum.RoughlyEqual (0f))
                {
                    var multiplier = 1f / hardpoints.Count;
                    foreach (var kvp in hardpoints)
                    {
                        var block = kvp.Value;
                        block.multiplier = (float)Math.Round (multiplier, 4);
                    }
                }
                else
                {
                    var multiplier = 1f / sum;
                    foreach (var kvp in hardpoints)
                    {
                        var block = kvp.Value;
                        block.multiplier *= (float)Math.Round (block.multiplier * multiplier, 4);
                    }
                }
            }
            else
            {
                var sums = GetNonUniformSums ();
                foreach (var kvp1 in sums)
                {
                    var stat = kvp1.Key;
                    var sum = kvp1.Value;

                    if (sum.RoughlyEqual (0f))
                    {
                        var multiplier = 1f / hardpoints.Count;
                        foreach (var kvp in hardpoints)
                        {
                            var block = kvp.Value;
                            if (block.multipliersPerStat == null || !block.multipliersPerStat.ContainsKey (stat))
                                continue;
                    
                            var hardpointInfo = DataMultiLinkerSubsystemHardpoint.GetEntry (kvp.Key);
                            if (hardpointInfo == null || hardpointInfo.duplication == 0)
                                continue;
                            
                            var blockSecondary = block.multipliersPerStat[stat];
                            blockSecondary.multiplier = (float)Math.Round (multiplier / hardpointInfo.duplication, 4);
                        }
                    }
                    else
                    {
                        var multiplier = 1f / sum;
                        foreach (var kvp in hardpoints)
                        {
                            var block = kvp.Value;
                            if (block.multipliersPerStat == null || !block.multipliersPerStat.ContainsKey (stat))
                                continue;
                    
                            var hardpointInfo = DataMultiLinkerSubsystemHardpoint.GetEntry (kvp.Key);
                            if (hardpointInfo == null)
                                continue;

                            var blockSecondary = block.multipliersPerStat[stat];
                            blockSecondary.multiplier = (float)Math.Round (blockSecondary.multiplier * multiplier, 4);
                        }
                    }
                }
            }
        }
        
        #if UNITY_EDITOR
        
        private void UpdateInspectorData ()
        {
            if (hardpoints == null)
                return;

            foreach (var kvp1 in hardpoints)
            {
                var block = kvp1.Value;
                block.parent = this;
            
                var hardpointInfo = DataMultiLinkerSubsystemHardpoint.GetEntry (kvp1.Key);
                if (hardpointInfo == null)
                    continue;
                
                block.duplication = hardpointInfo.duplication;
                if (block.multipliersPerStat == null)
                    continue;

                foreach (var kvp2 in block.multipliersPerStat)
                {
                    var blockSecondary = kvp2.Value;
                    blockSecondary.duplication = hardpointInfo.duplication;
                }
            }
        }
        
        #endif
    }
}

