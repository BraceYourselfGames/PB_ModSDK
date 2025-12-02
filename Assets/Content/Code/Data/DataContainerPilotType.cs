using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Text;
using Content.Code.Utility;
using PhantomBrigade.Game;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public class DataBlockPilotTraitSlotBase : DataBlockFilterLinked<DataContainerPilotTrait>
    {
        public override IEnumerable<string> GetTags () => DataMultiLinkerPilotTrait.GetTags ();
        public override SortedDictionary<string, DataContainerPilotTrait> GetData () => DataMultiLinkerPilotTrait.data;
        protected override string GetTooltip () => "Pilot trait filter.";
    }
    
    public class DataBlockPilotTraitSlot : DataBlockPilotTraitSlotBase
    {
        [YamlIgnore, HideInInspector]
        public string key;

        [PropertyOrder (-1)]
        public bool hidden = false;

        [PropertyOrder (-1)]
        public int level;
        
        [PropertyOrder (-1)]
        public PilotTraitSlotType type = PilotTraitSlotType.Unlockable;

        [PropertyOrder (-1)]
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckInt prestigeRank;

        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockPilotTraitSlot () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    public class DataBlockPilotTypeTutorialInfo
    {
        public int priority = 0;
        
        [DropdownReference (true)]
        public PilotIdentificationLocOnly identification;
        
        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerPilotAppearancePreset.data.Keys")]
        public string appearancePreset;
        
        [InfoBox ("This pilot type will appear during the recruitment tutorial with the following starter trait")]
        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerPilotTrait.data.Keys")]
        public string traitKeyStarter;
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockPilotTypeTutorialInfo () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
    
    public class DataContainerPilotType : DataContainerWithText, IDataContainerTagged
    {
        public bool hidden;
        
        // [HideInInspector]
        // [InfoBox ("$GetExpLabel"), LabelText ("XP")]
        // public int xp = 0;

        [LabelText ("Name / Desc"), YamlIgnore]
        public string textName;
        
        [HideLabel, TextArea (1, 10), YamlIgnore]
        public string textDesc;
        
        [BoxGroup ("Icon, frame, pattern")]
        [DataEditor.SpriteNameAttribute (false, 48f)]
        public string spriteIcon;

        [BoxGroup ("Icon, frame, pattern")]
        [DataEditor.SpriteNameAttribute (false, 48f)]
        public string spriteFrame;
        
        [BoxGroup ("Icon, frame, pattern")]
        [DataEditor.SpriteNameAttribute (false, 48f)]
        public string spritePattern;

        public bool equipmentCritCapable = true;
        public int equipmentRangePreferred = -1;
        
        [PropertySpace (4f)]
        public HashSet<string> tags;

        [ValueDropdown ("GetEquipmentTags")]
        public HashSet<string> equipmentTagsPreferred = new HashSet<string> ();
        
        [PropertySpace (4f)]
        [YamlIgnore, HideLabel, ShowIf ("IsDisplayIsolated"), ShowInInspector]
        private DataViewIsolatedDictionary<DataBlockPilotTraitSlot> traitIsolated; 

        [PropertySpace (4f)]
        [HideIf ("IsDisplayIsolated")]
        [OnValueChanged("RefreshTraits", true)]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, CustomAddFunction = "@new DataBlockPilotTraitGroup ()")]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.Foldout)]
        public SortedDictionary<string, DataBlockPilotTraitSlot> traits = new SortedDictionary<string, DataBlockPilotTraitSlot> (); 

        [DropdownReference]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.Foldout)]
        public SortedDictionary<string, float> statValues = new SortedDictionary<string, float> (); 
        
        [DropdownReference]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.Foldout)]
        public SortedDictionary<string, Vector2> statRanges = new SortedDictionary<string, Vector2> ();

        [DropdownReference (true)]
        public DataBlockPilotTypeTutorialInfo tutorialInfo;
        
        public HashSet<string> GetTags (bool processed)
        {
            return tags;
        }
        
        public bool IsHidden () => hidden;
        
        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);

            RefreshTraits ();
            
            #if UNITY_EDITOR
            
            traitIsolated = new DataViewIsolatedDictionary<DataBlockPilotTraitSlot> 
            (
                "Trait slots", 
                () => traits, 
                () => GetSlotKeys,
                RefreshTraits
            );
            
            #endif
        }

        private void RefreshTraits ()
        {
            if (traits != null)
            {
                foreach (var kvp in traits)
                {
                    var tg = kvp.Value;
                    if (tg != null)
                    {
                        tg.key = kvp.Key;
                        tg.Refresh ();
                    }
                }
            }
        }

        private HashSet<string> GetEquipmentTags ()
        {
            var data = DataMultiLinkerSubsystem.data;
            return DataMultiLinkerSubsystem.tags;
        }
        
        private IEnumerable<string> GetSlotKeys => traits != null ? traits.Keys : null;

        public DataBlockPilotTraitSlot GetSlotInfo (string slotKey)
        {
            return !string.IsNullOrEmpty (slotKey) && traits.TryGetValue (slotKey, out var s) && s != null ? s : null;
        }

        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.pilotTypes, $"{key}__header");
            textDesc = DataManagerText.GetText (TextLibs.pilotTypes, $"{key}__text");
        }

        #if UNITY_EDITOR
        
        private bool IsDisplayIsolated => DataMultiLinkerPilotType.Presentation.showIsolatedEntries;
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataContainerPilotType () =>
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.pilotTypes, $"{key}__header", textName);
            DataManagerText.TryAddingTextToLibrary (TextLibs.pilotTypes, $"{key}__text", textDesc);
        }

        /*
        private static StringBuilder sb = new StringBuilder ();

        private string GetExpLabel ()
        {
            sb.Clear ();
            sb.Append ("Starting XP: ");
            sb.Append (xp);

            int level = PilotUtility.GetLevelFromExperience (xp);
            sb.Append ("\nLevel: ");
            sb.Append (level);
        
            int xpToCurrent = PilotUtility.GetExperienceTotalForLevel (level);
            sb.Append ("\n");
            sb.Append ("XP for current level: ");
            sb.Append (xpToCurrent);

            int xpLocal = xp - xpToCurrent;
            int xpToNext = PilotUtility.GetExperienceForNextLevel (level);
            sb.Append ("\n");
            sb.Append ("XP to next level: ");
            sb.Append (xpLocal);
            sb.Append (" / ");
            sb.Append (xpToNext);

            return sb.ToString ();
        }
        */
    
        #endif
    }
}

