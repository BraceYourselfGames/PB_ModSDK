using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [Serializable]
    public class DataContainerTextLibraryCoreOld : DataContainerUnique
    {
        public string name = "English";
        public string flag = "icon_flag_english";
        public string prefix;
        public string subtitleBuffer = string.Empty;
        public float subtitleFadeoutTime = 2f;
        public bool insertNewLines;
        public int widthSubtitle = 360;
        public int sizeSubtitle = 16;
        public int sizeHint = 16;
        public int sizeTooltipHeader = 16;
        public int sizeTooltipContent = 12;
        public int spacingTooltipHeader = 2;
    }
    
    [Serializable]
    public class DataContainerTextSpecializedOld : DataContainerUnique
    {
        public Dictionary<string, DataBlockTextGroupPilotOld> pilotGroups;
        public Dictionary<string, DataBlockTextGroupUnitOld> unitGroups;
        public Dictionary<string, DataBlockTextGroupOverworldEntityOld> overworldEntityGroups;
    }
    
    [Serializable]
    public class DataBlockTextGroupPilotOld
    {
        [ValueDropdown ("@DataHelperUnitEquipment.GetFactions ()")]
        public HashSet<string> factions;
        public List<string> givenNames;
        public List<string> familyNames;
        public List<string> callsigns;
        public List<string> callsignSuffixes;
    }
    
    [Serializable]
    public class DataBlockTextGroupUnitOld
    {
        public List<string> callsigns;
        public List<string> callsignSuffixes;
    }

    [Serializable]
    public class DataBlockTextGroupOverworldEntityOld
    {
        public bool prefixFromID = false;
        public bool suffixFromID = false;
        public List<string> names;
    }

    [Serializable][HideReferenceObjectPicker]
    public class DataBlockTextEntryOld
    {
        [GUIColor ("GetTempBasedColor")]
        [ToggleLeft, LabelText ("Placeholder")]
        public bool temp = true;
        
        [TextArea (1, 10)][HideLabel][HideIf ("@DataManagerText.showProcessedText")]
        public string text = "";

        private static Color colorFinal = new Color (1f, 1f, 1f, 1f);
        private static Color colorTemp = new Color (1f, 0.6f, 0.6f, 1f);
        private Color GetTempBasedColor => temp ? colorTemp : colorFinal;
    }
    
    public enum TextSectorExportModeOld
    {
        None,
        LinePerKeySimple,
        LinePerKeySplit,
        LinePerGroup
    }
    
    [Serializable, HideReferenceObjectPicker]
    public class DataBlockTextSectorSplitInfoOld
    {
        public string displayName;
        public string prefix;
    }

    [Serializable]
    public class DataContainerTextSectorOld : DataContainer
    {
        [LabelText ("Name / Desc.")]
        public string name;
                
        [HideLabel, TextArea]
        public string description;
        
        public TextSectorExportModeOld exportMode = TextSectorExportModeOld.None;

        public List<DataBlockTextSectorSplitInfoOld> splits = new List<DataBlockTextSectorSplitInfoOld> ();

        public SortedDictionary<string, DataBlockTextEntryOld> entries = new SortedDictionary<string, DataBlockTextEntryOld> ();
    }
    
    [Serializable]
    public class DataContainerTextLibraryOld : DataContainerUnique
    {
        public DataContainerTextLibraryCoreOld core;
        public DataContainerTextSpecializedOld specialized;
        
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        [HideReferenceObjectPicker]
        public SortedDictionary<string, DataContainerTextSectorOld> sectors = new SortedDictionary<string, DataContainerTextSectorOld> ();
    }
}

