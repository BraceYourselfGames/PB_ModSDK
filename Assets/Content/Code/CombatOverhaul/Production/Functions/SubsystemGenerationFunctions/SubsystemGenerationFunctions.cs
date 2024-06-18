using System;
using System.Collections.Generic;
using System.Text;
using Content.Code.Utility;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions.Equipment
{
    
    // Example rules
    // Addition sequence
        
    // Add hardpoint
    // - If rating condition is satisfied
    // - If tag is present on part preset
        
    // Determine hardpoint content (start with all subsystems)
    // - Trim systems not matching hardpoint (start with this and make it mandatory for better perf / smaller starting set)
    // - Trim systems not matching tag filter
    // - Trim systems not matching key set
    // - Trim systems not matching stat set (directly defined)
    // - Trim systems not matching stat set (from another hardpoint, assuming it's already added and trimmed to 1 system)
    // - Trim systems not matching rating (directly defined)
    // - Trim systems not matching rating (from part preset)
        
    // Filter hardpoint state
    // - Fuse by rating
    
    [TypeHinted]
    public interface IPartGenStep
    {
        public void Run (DataContainerPartPreset preset, Dictionary<string, GeneratedHardpoint> layout, int rating, bool log);
        public int GetPriority ();
    }

    [TypeHinted]
    public interface IPartGenCheck
    {
        public bool IsPassed (DataContainerPartPreset preset, Dictionary<string, GeneratedHardpoint> layout, int rating);
    }

    public class CheckPartRating : IPartGenCheck
    {
        [LabelText ("Min/Max Rating"), HorizontalGroup]
        public int ratingMin = 0;

        [HideLabel, HorizontalGroup (0.4f)]
        public int ratingMax = 0;

        public bool IsPassed (DataContainerPartPreset preset, Dictionary<string, GeneratedHardpoint> layout, int rating)
        {
            if (rating >= ratingMin && rating <= ratingMax)
                return true;

            return false;
        }
    }

    public class CheckPartTag : IPartGenCheck
    {
        public bool requireAll = false;
        public SortedDictionary<string, bool> filter = new SortedDictionary<string, bool> { { string.Empty, true } };

        public bool IsPassed (DataContainerPartPreset preset, Dictionary<string, GeneratedHardpoint> layout, int rating)
        {
            if (preset == null)
                return false;

            if (filter == null || filter.Count == 0)
                return false;

            var tags = preset.tagsProcessed;
            int matches = 0;
            
            foreach (var kvp in filter)
            {
                var tag = kvp.Key;
                bool required = kvp.Value;
                bool present = tags.Contains (tag);

                if (required == present)
                    ++matches;
            }

            if (requireAll)
                return matches == filter.Count;
            else
                return matches > 0;
        }
    }

    public class PartGenStepBase
    {
        [GUIColor ("GetColorFooter")]
        [PropertyOrder (100), HorizontalGroup ("Footer", 100f)]
        [SuffixLabel ("Priority"), HideLabel]
        public int priority = 0;
        
        [ShowIf ("AreCommentsVisible")]
        [DropdownReference, HideLabel] 
        public DataBlockComment comment;
        
        [DropdownReference]
        public List<IPartGenCheck> checks;

        public bool AreChecksPassed (DataContainerPartPreset preset, Dictionary<string, GeneratedHardpoint> layout, int rating, bool log)
        {
            if (preset == null || layout == null)
                return false;
            
            bool checksPassed = true;
            if (checks != null)
            {
                for (int i = 0, checkCount = checks.Count; i < checkCount; ++i)
                {
                    var check = checks[i];
                    if (check == null)
                        continue;

                    bool checkPassed = check.IsPassed (preset, layout, rating);
                    if (!checkPassed)
                    {
                        if (log)
                            Debug.LogWarning ($"Part preset gen: {preset.key} | P{priority} | R{rating} | Skipping generation step {this.GetType ().Name} | Check {i} ({check.GetType ().Name}) failed");
                        checksPassed = false;
                        break;
                    }
                }
            }

            return checksPassed;
        }

        public int GetPriority ()
        {
            return priority;
        }
        
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100), HorizontalGroup ("Footer")]
        private DataEditor.DropdownReferenceHelper helper;
        
        public PartGenStepBase () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        private static Color GetColorFooter = new Color (1f, 1f, 1f, 0.6f); // Color.HSVToRGB ((float)priority * 0.01f, 0.5f, 1f).WithAlpha (0.5f);
        private bool AreCommentsVisible => DataMultiLinkerPartPreset.Presentation.showComments;

        #endif
    }

    public class PartGenStepTargeted : PartGenStepBase
    {
        [DropdownReference]
        // [ValueDropdown ("@DropdownUtils.ParentTypeMethod ($property, \"DataContainerPartPreset\", \"GetGeneratedHardpointKeys\")")]
        [ValueDropdown ("@DataMultiLinkerSubsystemHardpoint.data.Keys")]
        public List<string> hardpointsTargeted = new List<string> ();
    }

    public class AddHardpoints : PartGenStepTargeted, IPartGenStep
    {
        [ValueDropdown ("@DataMultiLinkerSubsystem.data.Keys")]
        [DropdownReference]
        public List<string> subsystemsInitial;

        [ShowIf ("@hardpointsTargeted != null && hardpointsTargeted.Count > 1")]
        [ToggleLeft]
        public bool pickRandom = false;

        public void Run (DataContainerPartPreset preset, Dictionary<string, GeneratedHardpoint> layout, int rating, bool log)
        {
            if (hardpointsTargeted == null || hardpointsTargeted.Count == 0)
                return;
            
            bool passed = AreChecksPassed (preset, layout, rating, log);
            if (!passed)
                return;
            
            if (pickRandom)
            {
                var hardpointKey = hardpointsTargeted.GetRandomEntry ();
                Run (preset, layout, rating, hardpointKey, log);
            }
            else
            {
                foreach (var hardpoint in hardpointsTargeted)
                    Run (preset, layout, rating, hardpoint, log);
            }
        }
        
        private void Run (DataContainerPartPreset preset, Dictionary<string, GeneratedHardpoint> layout, int rating, string hardpointKey, bool log)
        {
            if (string.IsNullOrEmpty (hardpointKey))
                return;

            var hardpointData = DataMultiLinkerSubsystemHardpoint.GetEntry (hardpointKey, false);
            if (hardpointData == null)
            {
                if (log)
                    Debug.LogWarning ($"Part preset gen: {preset.key} | P{priority} | R{rating} | Skipping hardpoint {hardpointKey} | Hardpoint couldn't be found");
                return;
            }
            
            if (layout.ContainsKey (hardpointKey))
            {
                if (log)
                    Debug.LogWarning ($"Part preset gen: {preset.key} | P{priority} | R{rating} | Skipping hardpoint {hardpointKey} | Layout already contains this hardpoint: {layout.ToStringFormattedKeys ()}");
                return;
            }

            // Start with a set of subsystems that are eligible for a given hardpoint.
            // That immediately cuts off systems that would never qualify, giving us a small precomputed set
            var subsystemsForHardpoint = DataMultiLinkerSubsystem.GetSubsystemsWithHardpoint (hardpointKey);
            if (subsystemsForHardpoint == null || subsystemsForHardpoint.Count == 0)
            {
                if (log)
                    Debug.LogWarning ($"Part preset gen: {preset.key} | P{priority} | R{rating} | Skipping hardpoint {hardpointKey} | Not a single subsystem defined for this hardpoint");
                return;
            }
            
            // Grab a hardpoint description container from a pool - we need to manipulate an independent collection to avoid corrupting the precomputed set
            // This is non-allocating for as long as pool already had a free list, which it normally would.
            // If something goes terribly wrong and the pool has too many lists checked out, it might return null, so we check for null.
            var hardpointGenerated = EquipmentGenUtility.hardpointPool.Get ();
            if (hardpointGenerated != null)
            {
                // Decide on how to fill the collection in the layout for this hardpoint
                if (subsystemsInitial != null)
                {
                    // If the step defines an initial set of systems, great - let's seed the entry for this hardpoint in the layout with them
                    // Note that it's a perfectly valid case to define this collection but leave it empty: this means a hardpoint is added but must start empty
                    if (subsystemsInitial.Count > 0)
                    {
                        // Before we proceed to adding, we need to validate whether each listed key points to an actual blueprint and that each blueprint fits the hardpoint
                        foreach (var subsystemKey in subsystemsInitial)
                        {
                            var subsystem = DataMultiLinkerSubsystem.GetEntry (subsystemKey, false);
                            if (subsystem == null)
                            {
                                if (log)
                                    Debug.LogWarning ($"Part preset gen: {preset.key} | P{priority} | R{rating} | Skipping initial subsystem {subsystemKey} in hardpoint {hardpointKey}: failed to find blueprint");
                                continue;
                            }

                            // if (subsystem.hidden)
                            //     continue;

                            if (subsystem.hardpointsProcessed == null || !subsystem.hardpointsProcessed.Contains (hardpointKey))
                            {
                                if (log)
                                    Debug.LogWarning ($"Part preset gen: {preset.key} | P{priority} | R{rating} | Skipping initial subsystem {subsystemKey} in hardpoint {hardpointKey}: blueprint doesn't list this hardpoint");
                                continue;
                            }

                            hardpointGenerated.subsystemCandidates.Add (subsystem);
                        }
                        
                        if (log)
                            Debug.Log ($"Part preset gen: {preset.key} | P{priority} | R{rating} | Added hardpoint {hardpointKey} | Predefined subsystems after filtering ({hardpointGenerated.subsystemCandidates.Count}): {hardpointGenerated.subsystemCandidates.ToStringFormatted (toStringOverride: (x) => x.key)}");
                    }
                    else
                    {
                        if (log)
                            Debug.Log ($"Part preset gen: {preset.key} | P{priority} | R{rating} | Added hardpoint {hardpointKey} | Generator specifies this hardpoint should always be empty");
                    }
                }
                else
                {
                    // Add the precomputed set appropriate for a given hardpoint if nothing was seeded
                    hardpointGenerated.subsystemCandidates.AddRange (subsystemsForHardpoint);
                    
                    if (log)
                        Debug.Log ($"Part preset gen: {preset.key} | P{priority} | R{rating} | Added hardpoint {hardpointKey} | Auto-filled {subsystemsForHardpoint.Count} subsystems based on hardpoint");
                }
                
                // Finally, we can register the filtered set to the layout
                layout.Add (hardpointKey, hardpointGenerated);
            }
        }
    }

    public class TrimSystemsByTagFilter : PartGenStepTargeted, IPartGenStep
    {
        [ToggleLeft, PropertyOrder (1)]
        public bool requireAll = false;
        
        [DictionaryKeyDropdown ("@DataMultiLinkerSubsystem.tags")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> filter = new SortedDictionary<string, bool> { { string.Empty, true } };

        public void Run (DataContainerPartPreset preset, Dictionary<string, GeneratedHardpoint> layout, int rating, bool log)
        {
            if (EquipmentGenUtility.IsStepPossible (this, preset, layout, rating, log))
                EquipmentGenUtility.RunPerHardpoint (this, Run, preset, layout, rating, log);
        }
        
        private void Run (DataContainerPartPreset preset, Dictionary<string, GeneratedHardpoint> layout, int rating, string hardpointKey, bool log)
        {
            if (string.IsNullOrEmpty (hardpointKey) || !layout.ContainsKey (hardpointKey))
                return;

            if (filter == null || filter.Count == 0)
                return;

            int filterSize = filter.Count;

            var hardpointGenerated = layout[hardpointKey];
            var subsystemCandidates = hardpointGenerated.subsystemCandidates;
            
            for (int i = subsystemCandidates.Count - 1; i >= 0; --i)
            {
                var subsystem = subsystemCandidates[i];
                var tags = subsystem.tagsProcessed;
                int matches = 0;
            
                foreach (var kvp in filter)
                {
                    var tag = kvp.Key;
                    bool required = kvp.Value;
                    bool present = tags.Contains (tag);

                    if (required == present)
                        ++matches;
                }

                bool passed = requireAll ? matches == filterSize : matches > 0;
                if (passed)
                    continue;
                
                if (log)
                    Debug.LogWarning ($"Part preset gen: {preset.key} | P{priority} | R{rating} | Hardpoint {hardpointKey} | Trimming subsystem {subsystem.key} based on tags | {subsystemCandidates.Count} left\n- {filter.ToStringFormattedKeyValuePairs ()}");
                subsystemCandidates.RemoveAt (i);
            }
        }
    }

    public class TrimSystemsByKeys : PartGenStepTargeted, IPartGenStep
    {
        [ValueDropdown ("@DataMultiLinkerSubsystem.data.Keys")]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false)]
        public List<string> keys = new List<string> ();

        public void Run (DataContainerPartPreset preset, Dictionary<string, GeneratedHardpoint> layout, int rating, bool log)
        {
            if (EquipmentGenUtility.IsStepPossible (this, preset, layout, rating, log))
                EquipmentGenUtility.RunPerHardpoint (this, Run, preset, layout, rating, log);
        }

        private void Run (DataContainerPartPreset preset, Dictionary<string, GeneratedHardpoint> layout, int rating, string hardpointKey, bool log)
        {
            if (string.IsNullOrEmpty (hardpointKey) || !layout.ContainsKey (hardpointKey))
                return;

            if (keys == null || keys.Count == 0)
                return;

            var hardpointGenerated = layout[hardpointKey];
            var subsystemCandidates = hardpointGenerated.subsystemCandidates;
            
            for (int i = subsystemCandidates.Count - 1; i >= 0; --i)
            {
                var subsystem = subsystemCandidates[i];
                if (keys.Contains (subsystem.key))
                    continue;

                if (log)
                    Debug.LogWarning ($"Part preset gen: {preset.key} | P{priority} | R{rating} | Hardpoint {hardpointKey} | Trimming subsystem {subsystem.key} based on keys | {subsystemCandidates.Count} left\n- Keys: {keys.ToStringFormatted ()}");
                subsystemCandidates.RemoveAt (i);
            }
        }
    }

    public class TrimSystemsByStats : PartGenStepTargeted, IPartGenStep
    {
        public bool statsDesired = true;
        
        [ValueDropdown ("@DataMultiLinkerUnitStats.data.Keys")]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false)]
        public HashSet<string> statKeys = new HashSet<string> ();

        public void Run (DataContainerPartPreset preset, Dictionary<string, GeneratedHardpoint> layout, int rating, bool log)
        {
            if (EquipmentGenUtility.IsStepPossible (this, preset, layout, rating, log))
                EquipmentGenUtility.RunPerHardpoint (this, Run, preset, layout, rating, log);
        }

        private void Run (DataContainerPartPreset preset, Dictionary<string, GeneratedHardpoint> layout, int rating, string hardpointKey, bool log)
        {
            if (preset == null || layout == null)
                return;

            if (string.IsNullOrEmpty (hardpointKey) || !layout.ContainsKey (hardpointKey))
                return;

            if (statKeys == null || statKeys.Count == 0)
                return;

            var hardpointGenerated = layout[hardpointKey];
            var subsystemCandidates = hardpointGenerated.subsystemCandidates;
            
            for (int i = subsystemCandidates.Count - 1; i >= 0; --i)
            {
                var subsystem = subsystemCandidates[i];

                // For each listed stat
                foreach (var statKeyListed in statKeys)
                {
                    // If subsystem has it
                    bool statInSubsystem = subsystem.statsProcessed.TryGetValue (statKeyListed, out var block) && block.value > 0f;

                    if (statsDesired)
                    {
                        if (!statInSubsystem)
                        {
                            if (log)
                                Debug.LogWarning ($"Part preset gen: {preset.key} | P{priority} | R{rating} | Hardpoint {hardpointKey} | Trimming subsystem {subsystem.key} based on absence of desired stat {statKeyListed} | {subsystemCandidates.Count} left\n- Stats: {statKeys.ToStringFormatted ()}");
                            subsystemCandidates.RemoveAt (i);
                            break;
                        }
                    }
                    else
                    {
                        if (statInSubsystem)
                        {
                            if (log)
                                Debug.LogWarning ($"Part preset gen: {preset.key} | P{priority} | R{rating} | Hardpoint {hardpointKey} | Trimming subsystem {subsystem.key} based on presence of undesired stat {statKeyListed} | {subsystemCandidates.Count} left\n- Stats: {statKeys.ToStringFormatted ()}");
                            subsystemCandidates.RemoveAt (i);
                            break;
                        }
                    }
                }
            }
        }
    }

    public class TrimSystemsByStatsInHardpoint : PartGenStepTargeted, IPartGenStep
    {
        [LabelText ("Hardpoint Referenced")]
        [ValueDropdown ("@DropdownUtils.ParentTypeMethod ($property, \"DataContainerPartPreset\", \"GetGeneratedHardpointKeys\")")]
        public string hardpointKeySource;
        
        private static Dictionary<string, DataBlockSubsystemStat> statsCollected = new Dictionary<string, DataBlockSubsystemStat> ();

        private void FillStatCollection (Dictionary<string, GeneratedHardpoint> layout)
        {
            statsCollected.Clear ();

            if (layout == null)
                return;
            
            if (string.IsNullOrEmpty (hardpointKeySource) || !layout.ContainsKey (hardpointKeySource))
                return;

            var hardpointGeneratedSource = layout[hardpointKeySource];
            var subsystemCandidatesSource = hardpointGeneratedSource.subsystemCandidates;

            if (subsystemCandidatesSource == null || subsystemCandidatesSource.Count == 0)
                return;
            
            foreach (var subsystem in subsystemCandidatesSource)
            {
                if (subsystem == null || subsystem.statsProcessed == null)
                    continue;

                bool damageRadiusIncompatible = subsystem.beamProcessed == null && subsystem.projectileProcessed?.splashDamage == null;
                bool impactRadiusIncompatible = subsystem.projectileProcessed?.splashImpact == null;
                bool ballisticsIncompatible = subsystem.projectileProcessed?.guidanceData != null;
                
                foreach (var kvp in subsystem.statsProcessed)
                {
                    var statKey = kvp.Key;
                    var statValue = kvp.Value;

                    if (statValue.value <= 0f || statsCollected.ContainsKey (statKey))
                        continue;
                    
                    // Do not collect damage radius stat if weapon isn't using it
                    if (damageRadiusIncompatible && statKey == UnitStats.weaponDamageRadius)
                        continue;
                    
                    // Do not collect impact radius stat if weapon isn't using it
                    if (impactRadiusIncompatible && statKey == UnitStats.weaponImpactRadius)
                        continue;

                    // Do not collect scatter stat if weapon isn't using it
                    if (ballisticsIncompatible)
                    {
                        if 
                        (
                            statKey == UnitStats.weaponScatterAngle || 
                            statKey == UnitStats.weaponScatterAngleMoving || 
                            statKey == UnitStats.weaponProjectileSpeed
                        )
                            continue;
                    }
                    
                    statsCollected.Add (statKey, statValue);
                }
            }
        }
        
        public void Run (DataContainerPartPreset preset, Dictionary<string, GeneratedHardpoint> layout, int rating, bool log)
        {
            FillStatCollection (layout);
            
            if (EquipmentGenUtility.IsStepPossible (this, preset, layout, rating, log))
                EquipmentGenUtility.RunPerHardpoint (this, Run, preset, layout, rating, log);
        }

        private void Run (DataContainerPartPreset preset, Dictionary<string, GeneratedHardpoint> layout, int rating, string hardpointKey, bool log)
        {
            if (preset == null || layout == null)
                return;

            if (string.IsNullOrEmpty (hardpointKey) || !layout.ContainsKey (hardpointKey))
                return;

            if (string.IsNullOrEmpty (hardpointKeySource) || !layout.ContainsKey (hardpointKeySource))
                return;

            if (string.Equals (hardpointKeySource, hardpointKey))
                return;

            var hardpointGeneratedSource = layout[hardpointKeySource];
            var subsystemCandidatesSource = hardpointGeneratedSource.subsystemCandidates;

            if (subsystemCandidatesSource == null || subsystemCandidatesSource.Count == 0)
                return;

            var hardpointGenerated = layout[hardpointKey];
            var subsystemCandidates = hardpointGenerated.subsystemCandidates;
            for (int i = subsystemCandidates.Count - 1; i >= 0; --i)
            {
                var subsystem = subsystemCandidates[i];
                foreach (var kvp in subsystem.statsProcessed)
                {
                    var statKeyInSubsystem = kvp.Key;
                    var statBlock = kvp.Value;
                    
                    if (statBlock.targetMode == 0)
                    {
                        if (statsCollected.TryGetValue (statKeyInSubsystem, out var statBlockSource))
                        {
                            // Permit subsystem to remain if it adds value to stat that is present in the list AND isn't 0
                            if (statBlockSource.value > 0f)
                                continue;
                            
                            if (log)
                                Debug.LogWarning ($"Part preset gen: {preset.key} | P{priority} | R{rating} | Hardpoint {hardpointKey} | Trimming subsystem {subsystem.key} based on presence of non-targeted stat {statKeyInSubsystem}, not present in stat list or equal to 0 at source | {subsystemCandidates.Count} left\n- Stats: {statsCollected.ToStringFormattedKeys ()}");
                            subsystemCandidates.RemoveAt (i);
                            break;
                        }
                    }
                    else
                    {
                        // What's left is targeted stats, such as +10% damage.
                        // These should be filtered out if stats collected in referenced hardpoint don't contain targeted stat.
                        if (statsCollected.ContainsKey (statKeyInSubsystem))
                            continue;
                        
                        if (log)
                            Debug.LogWarning ($"Part preset gen: {preset.key} | P{priority} | R{rating} | Hardpoint {hardpointKey} | Trimming subsystem {subsystem.key} based on presence of targeted stat {statKeyInSubsystem}, not present in stat list | {subsystemCandidates.Count} left\n- Stats: {statsCollected.ToStringFormattedKeys ()}");
                        subsystemCandidates.RemoveAt (i);
                        break;
                    }
                }
            }
        }
    }

    public class TrimSystemsByChance : PartGenStepTargeted, IPartGenStep
    {
        public bool difficultyUsed = false;
        
        [ShowIf ("difficultyUsed")]
        public string difficultyKey = string.Empty;
        
        [HideIf ("difficultyUsed")]
        [PropertyRange (0f, 1f)]
        public float chance = 0.5f;

        public bool individual = false;

        public void Run (DataContainerPartPreset preset, Dictionary<string, GeneratedHardpoint> layout, int rating, bool log)
        {
            if (EquipmentGenUtility.IsStepPossible (this, preset, layout, rating, log))
                EquipmentGenUtility.RunPerHardpoint (this, Run, preset, layout, rating, log);
        }

        private void Run (DataContainerPartPreset preset, Dictionary<string, GeneratedHardpoint> layout, int rating, string hardpointKey, bool log)
        {
            if (preset == null || layout == null)
                return;

            if (string.IsNullOrEmpty (hardpointKey) || !layout.ContainsKey (hardpointKey))
                return;

            var chanceFinal = Mathf.Clamp01 (chance);
            if (difficultyUsed)
            {
                #if !PB_MODSDK
                chanceFinal = Mathf.Clamp01 (DifficultyUtility.GetMultiplier (difficultyKey));
                #else
                chanceFinal = 0.5f;
                #endif
            }

            var hardpointGenerated = layout[hardpointKey];
            var subsystemCandidates = hardpointGenerated.subsystemCandidates;

            if (individual)
            {
                for (int i = subsystemCandidates.Count - 1; i >= 0; --i)
                {
                    var subsystem = subsystemCandidates[i];
                    var random = UnityEngine.Random.Range (0f, 1f);
                    if (random <= chanceFinal)
                        continue;

                    if (log)
                        Debug.LogWarning ($"Part preset gen: {preset.key} | P{priority} | R{rating} | Hardpoint {hardpointKey} | Trimming subsystem {subsystem.key} based on per-system random chance {random:F1} not passing threshold {chance:F1} | {subsystemCandidates.Count} left");
                    subsystemCandidates.RemoveAt (i);
                }
            }
            else
            {
                var random = UnityEngine.Random.Range (0f, 1f);
                if (random <= chanceFinal)
                    return;

                if (log)
                    Debug.LogWarning ($"Part preset gen: {preset.key} | P{priority} | R{rating} | Hardpoint {hardpointKey} | Trimming all {subsystemCandidates.Count} subsystems based on per-hardpoint random chance {random:F1} not passing threshold {chance:F1}");
                subsystemCandidates.Clear ();
            }
        }
    }

    public class TrimSystemsByRating : PartGenStepTargeted, IPartGenStep
    {
        [LabelText ("@GetLabel"), HorizontalGroup]
        public int ratingMin = 0;
        
        [HideLabel, HorizontalGroup (0.4f)]
        public int ratingMax = 1;

        public bool relative = false;
        
        public void Run (DataContainerPartPreset preset, Dictionary<string, GeneratedHardpoint> layout, int rating, bool log)
        {
            if (EquipmentGenUtility.IsStepPossible (this, preset, layout, rating, log))
                EquipmentGenUtility.RunPerHardpoint (this, Run, preset, layout, rating, log);
        }
        
        private void Run (DataContainerPartPreset preset, Dictionary<string, GeneratedHardpoint> layout, int rating, string hardpointKey, bool log)
        {
            if (preset == null || layout == null)
                return;

            if (string.IsNullOrEmpty (hardpointKey) || !layout.ContainsKey (hardpointKey))
                return;

            var hardpointGenerated = layout[hardpointKey];
            var subsystemCandidates = hardpointGenerated.subsystemCandidates;
            
            for (int i = subsystemCandidates.Count - 1; i >= 0; --i)
            {
                var subsystem = subsystemCandidates[i];

                if (relative)
                {
                    int difference = subsystem.rating - rating;
                    if (difference >= ratingMin && difference <= ratingMax)
                        continue;
                }
                else
                {
                    if (subsystem.rating >= ratingMin && subsystem.rating <= ratingMax)
                        continue;
                }

                if (log)
                    Debug.LogWarning ($"Part preset gen: {preset.key} | P{priority} | R{rating} | Hardpoint {hardpointKey} | Trimming subsystem {subsystem.key} based on rating {subsystem.rating} not matching generated one | {subsystemCandidates.Count} left\n- Generated rating: {rating} | Required subsystem rating: {ratingMin}-{ratingMax}");
                subsystemCandidates.RemoveAt (i);
            }
        }
        
        #if UNITY_EDITOR

        private string GetLabel => relative ? $"Min/Max Delta: {ratingMin}-{ratingMax}" : $"Min/Max Rating: {ratingMin}-{ratingMax}";

        #endif
    }
    
    public class SetHardpointState : PartGenStepTargeted, IPartGenStep
    {
        public bool fused = true;
        
        public void Run (DataContainerPartPreset preset, Dictionary<string, GeneratedHardpoint> layout, int rating, bool log)
        {
            if (EquipmentGenUtility.IsStepPossible (this, preset, layout, rating, log))
                EquipmentGenUtility.RunPerHardpoint (this, Run, preset, layout, rating, log);
        }
        
        private void Run (DataContainerPartPreset preset, Dictionary<string, GeneratedHardpoint> layout, int rating, string hardpointKey, bool log)
        {
            if (preset == null || layout == null)
                return;

            if (string.IsNullOrEmpty (hardpointKey) || !layout.ContainsKey (hardpointKey))
                return;

            var hardpointGenerated = layout[hardpointKey];
            hardpointGenerated.fused = fused;
            
            if (log)
                Debug.Log ($"Part preset gen: {preset.key} | P{priority} | R{rating} | Hardpoint {hardpointKey} state updated | Fused: {fused}");
        }
    }

    public class GeneratedHardpoint : IObjectPoolType
    {
        public bool fused = true;
        public List<DataContainerSubsystem> subsystemCandidates;

        public void OnRetrieval ()
        {
            fused = true;
            
            if (subsystemCandidates != null)
                subsystemCandidates.Clear ();
            else
                subsystemCandidates = new List<DataContainerSubsystem> (8);
        }

        public void OnReturn ()
        {
            if (subsystemCandidates != null)
                subsystemCandidates.Clear ();
        }
    }

    public static class EquipmentGenUtility
    {
        public delegate void RunPerHardpointDelegate (DataContainerPartPreset preset, Dictionary<string, GeneratedHardpoint> layout, int rating, string hardpointKey, bool log);

        public static ObjectPoolProvider<GeneratedHardpoint> hardpointPool = new ObjectPoolProvider<GeneratedHardpoint> ();

        public static void ReturnTempGenerationData (Dictionary<string, GeneratedHardpoint> layout)
        {
            if (layout == null)
                return;

            foreach (var kvp in layout)
                hardpointPool.Return (kvp.Value);
            
            layout.Clear ();
        }

        public static bool IsStepPossible 
        (
            IPartGenStep step, 
            DataContainerPartPreset preset,
            Dictionary<string, GeneratedHardpoint> layout, 
            int rating, 
            bool log
        )
        {
            if (step == null || preset == null || layout == null)
                return false;
            
            if (step is PartGenStepBase stepBase)
            {
                bool passed = stepBase.AreChecksPassed (preset, layout, rating, log);
                if (!passed)
                    return false;
            }

            return true;
        }
        
        // A lot of generation functions are useful at a part level (targeting all subsystems),
        // at targeted level (targeting specific set of hardpoints) and at hardpoint level (targeting one hardpoint).
        // Interface steps optionally inherit from a few base classes, allowing one interface and one per-hardpoint method
        // to work for all 3 cases. To avoid a lot of boilerplate per generator class, this delegate based method is useful.
        public static void RunPerHardpoint 
        (
            IPartGenStep step, 
            RunPerHardpointDelegate action,
            DataContainerPartPreset preset, 
            Dictionary<string, GeneratedHardpoint> layout, 
            int rating, 
            bool log
        )
        {
            if (step is PartGenStepTargeted stepTargeted)
            {
                if (stepTargeted.hardpointsTargeted != null && stepTargeted.hardpointsTargeted.Count > 0)
                {
                    foreach (var hardpoint in stepTargeted.hardpointsTargeted)
                        action.Invoke (preset, layout, rating, hardpoint, log);
                }
                else foreach (var kvp in layout)
                    action.Invoke (preset, layout, rating, kvp.Key, log);
            }
            else
            {
                foreach (var kvp in layout)
                    action.Invoke (preset, layout, rating, kvp.Key, log);
            }
        }
        
        public static bool IsRatingMatchingFilter (SubsystemSlotRating filter, int rating, bool strict, bool invert)
        {
            bool hasCommon = (filter & SubsystemSlotRating.Common) != 0;
            bool hasUncommon = (filter & SubsystemSlotRating.Uncommon) != 0;
            bool hasRare = (filter & SubsystemSlotRating.Rare) != 0;
            bool result = false;

            if (strict)
            {
                if (rating == 3)
                    result = filter == SubsystemSlotRating.Rare;
                else if (rating == 2)
                    result = filter == SubsystemSlotRating.Uncommon;
                else if (rating == 1)
                    result = filter == SubsystemSlotRating.Common;
                else
                    result = filter == SubsystemSlotRating.None;
            }
            else
            {
                if (rating == 3)
                    result = hasRare;
                else if (rating == 2)
                    result = hasUncommon;
                else if (rating == 1)
                    result = hasCommon;
                else
                    result = true;
            }

            if (invert)
                result = !result;

            return result;
        }

        public static Color GetRatingFilterColor (SubsystemSlotRating filter)
        {
            bool hasCommon = (filter & SubsystemSlotRating.Common) != 0;
            bool hasUncommon = (filter & SubsystemSlotRating.Uncommon) != 0;
            bool hasRare = (filter & SubsystemSlotRating.Rare) != 0;

            if (hasRare)
            {
                if (hasUncommon || hasCommon)
                    return Color.HSVToRGB (0.5f, 0.5f, 1f);
                else
                    return Color.HSVToRGB (0.55f, 0.7f, 1f);
            }
            
            if (hasUncommon)
            {
                if (hasCommon)
                    return Color.HSVToRGB (0.25f, 0.2f, 1f);
                else
                    return Color.HSVToRGB (0.3f, 0.4f, 1f);
            }
            
            return Color.HSVToRGB (0f, 0f, 1f);
        }

        private static StringBuilder sb = new StringBuilder ();
        
        public static string GetLayoutDescription (Dictionary<string, GeneratedHardpoint> layout, bool printNames = true)
        {
            sb.Clear ();
            
            if (layout != null)
            {
                foreach (var kvp in layout)
                {
                    var hardpointKey = kvp.Key;
                    var hardpointGenerated = kvp.Value;
                    var hardpointData = DataMultiLinkerSubsystemHardpoint.GetEntry (hardpointKey, false);
                    bool visible = hardpointData != null && hardpointData.exposed;
                    bool editable = visible && hardpointData.editable && hardpointGenerated.fused;
                    
                    sb.Append ("\n- Hardpoint: ");
                    sb.Append (hardpointKey);

                    if (printNames)
                    {
                        if (visible && !string.IsNullOrEmpty (hardpointData.textName))
                        {
                            sb.Append (" (");
                            sb.Append (hardpointData.textName);
                            sb.Append (")");
                        }
                    }
                    
                    if (!visible)
                        sb.Append (" | Hidden");
                    
                    if (editable)
                        sb.Append (" | Editable");

                    var subsystemCandidates = hardpointGenerated.subsystemCandidates;
                    if (subsystemCandidates != null && subsystemCandidates.Count > 0)
                    {
                        foreach (var subsystem in subsystemCandidates)
                        {
                            sb.Append ("\n   - ");
                            sb.Append (subsystem.key);
                            
                            if (printNames)
                            {
                                var subsystemData = DataMultiLinkerSubsystem.GetEntry (subsystem.key, false);
                                if (subsystemData != null && subsystemData.textNameProcessed != null && !string.IsNullOrEmpty (subsystemData.textNameProcessed.s))
                                {
                                    sb.Append (" (");
                                    sb.Append (subsystemData.textNameProcessed.s);
                                    sb.Append (")");
                                }
                            }
                        }
                    }
                    else
                    {
                        sb.Append ("\n   - empty");
                    }
                }
            }

            return sb.ToString ();
        }
    }

    public class ListPoolProvider<T>
    {
        public Stack<List<T>> listPool = new Stack<List<T>> ();
        public int listsCreated = 0;
        public int listsReturned = 0;
        public int listsReused = 0;
        public Type type = typeof (T);
        
        public List<T> GetList ()
        {
            int listsInPool = listPool.Count;
            if (listsInPool == 0)
            {
                listsCreated += 1;

                int delta = listsCreated - listsReturned;
                if (delta >= 100)
                {
                    Debug.LogWarning ($"List pool ({type.Name}) reached the limit of 100 lists checked out, returning null | Created: {listsCreated} | Returned: {listsReturned}");
                    return null;
                }
                
                // Debug.Log ($"List pool ({type.Name}) empty, returning new list. Created so far: {segmentListsCreated}");
                return new List<T> (8);
            }
            else
            {
                listsReused += 1;
                // Debug.Log ($"List pool ({type.Name}) has {listsInPool} lists, reusing one. Reused so far: {segmentListsReused}");
                var list = listPool.Pop ();
                list.Clear ();
                return list;
            }
        }

        public void ReturnList (List<T> list)
        {
            listsReturned += 1;
            listPool.Push (list);
            // Debug.Log ($"List pool ({type.Name}) gets a returned list, new size is {segmentListPool.Count}. Returned so far: {segmentListsReturned}");
        }
    }

    public interface IObjectPoolType
    {
        public void OnRetrieval ();
        public void OnReturn ();
    }
    
    public class ObjectPoolProvider<T> where T : class, IObjectPoolType, new ()
    {
        public Stack<T> pool = new Stack<T> ();
        public int created = 0;
        public int returned = 0;
        public int reused = 0;
        public Type type = typeof (T);
        public int limit = 100;
        
        public T Get ()
        {
            int listsInPool = pool.Count;
            if (listsInPool == 0)
            {
                created += 1;

                int delta = created - returned;
                if (limit > 0 && delta >= limit)
                {
                    Debug.LogWarning ($"Pool ({type.Name}) reached the limit of 100 lists checked out, returning null | Created: {created} | Returned: {returned}");
                    return null;
                }
                
                // Debug.Log ($"Pool ({type.Name}) empty, returning new list. Created so far: {segmentListsCreated}");
                var entry = new T ();
                entry.OnRetrieval ();
                return entry;
            }
            else
            {
                reused += 1;
                // Debug.Log ($"Pool ({type.Name}) has {listsInPool} lists, reusing one. Reused so far: {segmentListsReused}");
                var entry = pool.Pop ();
                entry.OnRetrieval ();
                return entry;
            }
        }

        public void Return (T entry)
        {
            if (entry == null)
            {
                Debug.LogWarning ($"Pool ({type.Name}) received a null returned entry");
                return;
            }
            
            returned += 1;
            entry.OnReturn ();
            pool.Push (entry);
            // Debug.Log ($"Pool ({type.Name}) gets a returned list, new size is {segmentListPool.Count}. Returned so far: {segmentListsReturned}");
        }
    }
}