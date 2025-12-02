using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public class DataBlockOverworldRewardPart : DataBlockFilterLinkedCounted<DataContainerPartPreset>
    {
        [PropertyOrder (-2), LabelWidth (130f), OnValueChanged (nameof(Refresh))]
        public bool factionFilter = false;
        
        #if UNITY_EDITOR
        [YamlIgnore]
        [PropertyOrder (-2), LabelWidth (130f), LabelText ("Faction (Preview)")]
        [ShowIf (nameof (factionFilter)), OnValueChanged (nameof(Refresh))]
        [ValueDropdown ("@DataMultiLinkerOverworldFactionBranch.data.Keys")]
        public string factionBranchPreview = "branch_army";
        #endif
        
        [DropdownReference (true)]
        [PropertyOrder (-2), LabelWidth (120f)]
        [GUIColor (nameof(GetQualityTableColor)), LabelText ("Quality")]
        [ValueDropdown (nameof(GetQualityTableKeys))]
        public string qualityTableKey;
        
        /*[PropertyOrder (-3)]
        [HorizontalGroup]
        [LabelText ("Level"), SuffixLabel ("$GetLevelLabelText", true)]
        public int levelMin = 1;
        
        [PropertyOrder (-3)]
        [HorizontalGroup (0.2f)]
        [HideLabel, ShowIf (nameof(countRandom)), SuffixLabel ("max", true)]
        public int levelMax = 1;
        
        [HideInInspector]
        public bool levelRandom = false;
        */

        [DropdownReference (true)]
        public DataBlockRangeInt levelRange;
        
        /*
        [Button ("$GetLevelButtonText", ButtonHeight = 21), HorizontalGroup (120f)]
        private void ToggleLevelRandom ()
        {
            levelRandom = !levelRandom;
            if (!levelRandom)
                levelMax = levelMin;
        }

        private string GetLevelLabelText () => levelRandom ? "min" : string.Empty;

        private string GetLevelButtonText () => levelRandom ? "Switch to fixed" : "Switch to random";
        */
        
        public override bool IsCandidateValid (DataContainerPartPreset candidate)
        {
            bool valid = base.IsCandidateValid (candidate);
            if (!valid)
                return false;

            #if UNITY_EDITOR
            if (factionFilter && !Application.isPlaying && !string.IsNullOrEmpty (factionBranchPreview))
            {
                if (candidate.tagsProcessed == null || !candidate.tagsProcessed.Contains (factionBranchPreview))
                    return false;
            }
            #endif
            
            return true;
        }

        public override IEnumerable<string> GetTags () => DataMultiLinkerPartPreset.GetTags ();
        public override SortedDictionary<string, DataContainerPartPreset> GetData () => DataMultiLinkerPartPreset.data;
        
        private IEnumerable<string> GetQualityTableKeys => DataMultiLinkerQualityTable.data.Keys;
        private Color GetQualityTableColor => !string.IsNullOrEmpty(qualityTableKey) && DataMultiLinkerQualityTable.data.TryGetValue(qualityTableKey, out var v) ? v.uiColor : Color.white;

        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockOverworldRewardPart () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
    
    public class DataBlockOverworldRewardSubsystem : DataBlockFilterLinkedCounted<DataContainerSubsystem>
    {
        [DropdownReference (true)]
        public DataBlockRangeInt ratingRange;
        
        public override IEnumerable<string> GetTags () => DataMultiLinkerSubsystem.GetTags ();
        public override SortedDictionary<string, DataContainerSubsystem> GetData () => DataMultiLinkerSubsystem.data;
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockOverworldRewardSubsystem () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
    
    public class DataBlockOverworldRewardWorkshop : DataBlockFilterLinkedCounted<DataContainerWorkshopProject>
    {
        [PropertyOrder (-2), LabelWidth (130f), OnValueChanged (nameof(Refresh))]
        public bool factionFilter = false;
        
        public override IEnumerable<string> GetTags () => DataMultiLinkerWorkshopProject.GetTags ();
        public override SortedDictionary<string, DataContainerWorkshopProject> GetData () => DataMultiLinkerWorkshopProject.data;
    }
    
    [LabelWidth (120f)]
    public class DataContainerOverworldReward : DataContainerWithText, IDataContainerTagged
    {
        [ColorUsage (false, false), HideLabel, HorizontalGroup]
        public Color color = Color.white;

        [YamlIgnore, HideLabel, ReadOnly, HorizontalGroup (64f)]
        public string colorTag;
        
        public bool hidden;
        
        [DataEditor.SpriteNameAttribute (true, 32f)]
        public string icon;
        
        [LabelText ("Name")][YamlIgnore]
        public string textName;
        
        [ValueDropdown("@DataMultiLinkerOverworldReward.GetTags ()")]
        public HashSet<string> tags = new HashSet<string> ();
        
        [DropdownReference, DictionaryKeyDropdown ("@DataMultiLinkerResource.data.Keys")]
        public SortedDictionary<string, DataBlockVirtualResource> resources;
        
        [DropdownReference, ListDrawerSettings (DefaultExpandedState = true, CustomAddFunction = "@new DataBlockOverworldRewardWorkshop ()")]
        public List<DataBlockOverworldRewardWorkshop> projects;
        
        [DropdownReference, ListDrawerSettings (DefaultExpandedState = true, CustomAddFunction = "@new DataBlockOverworldRewardPart ()")]
        public List<DataBlockOverworldRewardPart> parts;
        
        [DropdownReference, ListDrawerSettings (DefaultExpandedState = true, CustomAddFunction = "@new DataBlockOverworldRewardSubsystem ()")]
        public List<DataBlockOverworldRewardSubsystem> subsystems;

        public HashSet<string> GetTags (bool processed)
        {
            return tags;
        }
        
        public bool IsHidden () => hidden;

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);

            colorTag = UtilityColor.ToHexTagRGB (color);

            if (projects != null)
            {
                foreach (var project in projects)
                {
                    if (project != null)
                        project.Refresh ();
                }
            }

            if (parts != null)
            {
                foreach (var part in parts)
                {
                    if (part != null)
                        part.Refresh ();
                }
            }
            
            if (subsystems != null)
            {
                foreach (var subsystem in subsystems)
                {
                    if (subsystem != null)
                        subsystem.Refresh ();
                }
            }
        }

        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.overworldRewards, $"{key}__name");
        }
        
        private static StringBuilder sbTemp = new StringBuilder ();
    
        public static void AppendRewardGroupsCollapsedToList (IDictionary<string, int> rewardsCollapsed, List<string> list, bool colorTags = true, string prefix = null)
        {
            if (rewardsCollapsed == null || rewardsCollapsed.Count == 0 || list == null)
                return;

            foreach (var kvp in rewardsCollapsed)
            {
                var rewardKey = kvp.Key;
                int rewardCount = kvp.Value;
            
                var rewardData = DataMultiLinkerOverworldReward.GetEntry (rewardKey, false);
                if (rewardData == null || rewardData.hidden)
                    continue;

                var textName = rewardData.textName;
                if (string.IsNullOrEmpty (textName))
                    textName = Txt.Get (TextLibs.uiOverworld, "reward_unknown_header");
            
                sbTemp.Clear ();

                if (!string.IsNullOrEmpty (prefix))
                {
                    sbTemp.Append ("[cc]");
                    sbTemp.Append (prefix);
                    sbTemp.Append (": [ff]");
                }

                bool tagUsed = colorTags && !string.IsNullOrEmpty (rewardData.colorTag);
                if (tagUsed)
                    sbTemp.Append (rewardData.colorTag);
            
                sbTemp.Append (textName);

                if (rewardCount > 1)
                {
                    sbTemp.Append (" × ");
                    sbTemp.Append (kvp.Value);
                }

                if (tagUsed)
                    sbTemp.Append ("[-]");
            
                var textFinal = sbTemp.ToString ();
                if (!string.IsNullOrEmpty (textFinal))
                    list.Add (textFinal);
            }
        }

        #region Editor
        #if UNITY_EDITOR
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldRewards, $"{key}__name", textName);
        }
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerOverworldReward () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif

        #endregion
    }
    
}

