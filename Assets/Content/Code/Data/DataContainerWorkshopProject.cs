using System.Collections.Generic;
using PhantomBrigade.Functions;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public static class ResourceKeys
    {
        public const string supplies = "supplies";
        public const string battery = "battery";
        public const string repairJuice = "repair_juice";
        public const string componentsR2 = "components_r2";
        public const string componentsR3 = "components_r3";
        
        public const string itemDroneDecoy = "item_drone_decoy";
        public const string itemBlockCombat = "item_block_combat";
        public const string itemBlockEscalation = "item_block_escalation";
        public const string itemBlockReinforcements = "item_block_reinforcements";
        public const string itemRainSensor = "item_rain_sensor";
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockResourceCost
    {
        [HorizontalGroup, HideLabel]
        [ValueDropdown ("@DataMultiLinkerResource.data.Keys")]
        public string key = "supplies";
        
        [HorizontalGroup, HideLabel, Min (0)]
        public int amount;
    }

    [HideReferenceObjectPicker]
    public class DataBlockWorkshopPart
    {
        [PropertyOrder (-2), Min (1)]
        public int count = 1;

        [InlineButtonClear]
        [ValueDropdown ("@DataMultiLinkerPartPreset.data.Keys")]
        public string key;

        [InlineButtonClear]
        [DictionaryKeyDropdown ("@DataMultiLinkerPartPreset.tags")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> tags;

        #region Editor
        #if UNITY_EDITOR

        /*
        [HorizontalGroup (20f), PropertyOrder (-1)]
        [Button (" ")]
        private void SwitchMode ()
        {
            tagsUsed = !tagsUsed;
            if (tagsUsed && tags == null)
                tags = new SortedDictionary<string, bool> { { string.Empty, true } };
        }
        */

        #endif
        #endregion
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockWorkshopSubsystem
    {
        [PropertyOrder (-2), Min (1)]
        public int count = 1;
        
        [HideInInspector]
        public bool tagsUsed = false;
        
        [HorizontalGroup, HideLabel]
        [HideIf ("tagsUsed")]
        [ValueDropdown ("@DataMultiLinkerSubsystem.data.Keys")]
        public string key;

        [ShowIf ("tagsUsed")]
        [HorizontalGroup, DictionaryKeyDropdown ("@DataMultiLinkerSubsystem.tags")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> tags;
        
        #region Editor
        #if UNITY_EDITOR
        
        [HorizontalGroup (20f), PropertyOrder (-1)]
        [Button (" ")]
        private void SwitchMode ()
        {
            tagsUsed = !tagsUsed;
            if (tagsUsed && tags == null)
                tags = new SortedDictionary<string, bool> { { string.Empty, true } };
        }
        
        #endif
        #endregion
    }

    [HideReferenceObjectPicker]
    public class DataBlockWorkshopUnit
    {
        [PropertyOrder (-2)]
        [InlineButton ("@qualityTable = string.Empty", "-")]
        [ValueDropdown ("@DataMultiLinkerQualityTable.data.Keys")]
        public string qualityTable = string.Empty;
        
        [PropertyOrder (-2)]
        [InlineButton ("@factionBranch = string.Empty", "-")]
        [ValueDropdown ("@DataMultiLinkerOverworldFactionBranch.data.Keys")]
        public string factionBranch = string.Empty;

        [HideInInspector]
        public bool tagsUsed = false;

        [HorizontalGroup, HideLabel]
        [HideIf ("tagsUsed")]
        [ValueDropdown ("@DataMultiLinkerUnitPreset.data.Keys")]
        public string key;
        
        [ShowIf ("tagsUsed")]
        [HorizontalGroup, DictionaryKeyDropdown ("@DataMultiLinkerUnitPreset.tags")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> tags;

        #region Editor
        #if UNITY_EDITOR
        
        [HorizontalGroup (20f), PropertyOrder (-1)]
        [Button (" ")]
        private void SwitchMode ()
        {
            tagsUsed = !tagsUsed;
            if (tagsUsed && tags == null)
                tags = new SortedDictionary<string, bool> { { string.Empty, true } };
        }
        
        #endif
        #endregion
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockWorkshopOutputResource
    {
        [HorizontalGroup, HideLabel]
        [ValueDropdown ("@DataMultiLinkerResource.data.Keys")]
        public string key = "supplies";
        
        [HorizontalGroup, HideLabel, Min (0)]
        public int amount;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockBoolCheck
    {
        [LabelText ("Required")]
        public bool v;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockFloat01
    {
        [LabelText ("Value")]
        [Range (0f, 1f)]
        public float f;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockFloatNormalizable
    {
        public bool normalized;
        
        [LabelText ("Value")]
        public float f;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockFloat
    {
        [LabelText ("Value")]
        public float f;
    }

    [HideReferenceObjectPicker]
    public class DataBlockInt
    {
        [LabelText ("Value")]
        public int i;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockVector2
    {
        [LabelText ("Value")]
        public Vector2 v;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockVector3
    {
        [LabelText ("Value")]
        public Vector3 v;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockRangeInt
    {
        public int min;
        public int max;
    }
    
    [HideReferenceObjectPicker, HideDuplicateReferenceBox]
    public class DataBlockComment
    {
        [HideIf ("editMode"), GUIColor ("commentColor"), YamlIgnore, ShowInInspector]
        [HideLabel, DisplayAsString (TextAlignment.Left, true, Overflow = false)]
        [InlineButton ("@editMode = true", SdfIconType.PencilSquare, " ")]
        public string commentReadonly { get { return comment; } set { } }
        
        [ShowIf ("editMode"), GUIColor ("commentColor")]
        [HideLabel, TextArea (0, 10)]
        [InlineButton ("@editMode = false", SdfIconType.PencilSquare, " ")]
        public string comment;

        [HideInInspector, YamlIgnore]
        private bool editMode = false;
        private Color commentColor = Color.HSVToRGB (0.3f, 0f, 1f).WithAlpha (0.66f);
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockColorInterpolated
    {
        public Color colorFrom;
        public Color colorTo;
    }
    
    public class DataBlockWorkshopVariantLink
    {
        [ValueDropdown ("@DataMultiLinkerWorkshopVariants.data.Keys")]
        public string key = "part_body_sockets";
    }

    public class DataBlockWorkshopProjectProcessed
    {
        public string projectKey;
        public string variantPrimaryKey;
        public string variantSecondaryKey;

        public int rating;
        public float duration;
        public int inputCharges;

        public Dictionary<string, int> inputResources = new Dictionary<string, int> ();
        
        public SortedDictionary<string, int> basePartRequirements = new SortedDictionary<string, int> ();
        
        public SortedDictionary<string, DataBlockOverworldEventSubcheckFloat> baseStatRequirements = new SortedDictionary<string, DataBlockOverworldEventSubcheckFloat> ();
        
        public List<DataBlockWorkshopPart> outputParts = new List<DataBlockWorkshopPart> ();
        
        public List<DataBlockWorkshopSubsystem> outputSubsystems = new List<DataBlockWorkshopSubsystem> ();
        
        public List<DataBlockWorkshopUnit> outputUnits = new List<DataBlockWorkshopUnit> ();
        
        public List<DataBlockWorkshopOutputResource> outputResources = new List<DataBlockWorkshopOutputResource> ();
    }
    
    public class DataBlockWorkshopProjectVariant
    {
        [LabelText ("Name / Desc.")]
        [YamlIgnore, ShowIf (DataEditor.textAttrArg)]
        public string textName;
        
        [HideLabel]
        [YamlIgnore, ShowIf (DataEditor.textAttrArg), TextArea (1, 10)]
        public string textDesc;
        
        [DataEditor.SpriteNameAttribute (true, 32f)]
        public string icon;
        
        [DropdownReference (true)]
        public DataBlockInt ratingOverride;
        
        [DropdownReference (true)]
        public DataBlockFloat durationMultiplier;
        
        [DropdownReference, DictionaryKeyDropdown ("@DataMultiLinkerBasePart.data.Keys")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, int> basePartRequirements;
        
        [DropdownReference, DictionaryKeyDropdown ("@DataMultiLinkerBaseStat.data.Keys")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, DataBlockOverworldEventSubcheckFloat> baseStatRequirements;
        
        [DropdownReference (true)]
        public DataBlockFloat inputChargeMultiplier;

        [DropdownReference, DictionaryKeyDropdown ("@DataMultiLinkerResource.data.Keys")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, float> inputResourceMultipliers;
        
        [DropdownReference, DictionaryKeyDropdown ("@DataMultiLinkerPartPreset.tags")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> outputPartTags;
        
        [DropdownReference, DictionaryKeyDropdown ("@DataMultiLinkerSubsystem.tags")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> outputSubsystemTags;
        
        [DropdownReference, DictionaryKeyDropdown ("@DataMultiLinkerUnitPreset.tags")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> outputUnitTags;

        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockWorkshopProjectVariant () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    public enum WorkshopTextSource
    {
        Shared,
        Group,
        Part,
        Subsystem,
        Hardpoint
    }

    public class DataBlockWorkshopTextSource
    {
        public WorkshopTextSource source = WorkshopTextSource.Group;
        
        [ValueDropdown ("GetKeys")]
        public string key;

        private static List<string> fallback = new List<string> ();
        private IEnumerable<string> GetKeys ()
        {
            if (source == WorkshopTextSource.Shared)
                return GetSharedKeys ();
            
            if (source == WorkshopTextSource.Group)
                return DataMultiLinkerEquipmentGroup.data.Keys;
            
            if (source == WorkshopTextSource.Part)
                return DataMultiLinkerPartPreset.data.Keys;
            
            if (source == WorkshopTextSource.Subsystem)
                return DataMultiLinkerSubsystem.data.Keys;
            
            if (source == WorkshopTextSource.Hardpoint)
                return DataMultiLinkerSubsystemHardpoint.data.Keys;

            return fallback;
        }

        protected virtual IEnumerable<string> GetSharedKeys () => null;
        protected virtual bool IsShortTextPreferred () => true;

        public string GetText ()
        {
            string result = string.Empty;
            
            if (source == WorkshopTextSource.Shared)
                result = DataManagerText.GetText (TextLibs.workshopShared, key, true);
            
            if (source == WorkshopTextSource.Group)
            {
                var group = DataMultiLinkerEquipmentGroup.GetEntry (key, false);
                if (group != null)
                {
                    if (!string.IsNullOrEmpty (group.textName))
                        result = IsShortTextPreferred () ? group.textName : group.textDesc;
                }
            }
            
            if (source == WorkshopTextSource.Part)
            {
                var partPreset = DataMultiLinkerPartPreset.GetEntry (key, false);
                if (partPreset != null)
                    result = IsShortTextPreferred () ? partPreset.GetPartModelName () : partPreset.GetPartModelDesc (appendGroupMain: true);
            }
            
            if (source == WorkshopTextSource.Subsystem)
            {
                var subsystem = DataMultiLinkerSubsystem.GetEntry (key, false);
                if (subsystem != null)
                    result = IsShortTextPreferred () ? subsystem.textNameProcessed?.s : subsystem.textDescProcessed?.s;
            }
            
            if (source == WorkshopTextSource.Hardpoint)
            {
                var hardpoint = DataMultiLinkerSubsystemHardpoint.GetEntry (key, false);
                if (hardpoint != null)
                    result = IsShortTextPreferred () ? hardpoint.textName : hardpoint.textDesc;
            }

            return result;
        }
    }
    
    public class DataBlockWorkshopTextSourceName : DataBlockWorkshopTextSource
    {
        protected override IEnumerable<string> GetSharedKeys () => DataMultiLinkerWorkshopProject.textKeysSharedHeaders;
    }
    
    public class DataBlockWorkshopTextSourceSubtitle : DataBlockWorkshopTextSource
    {
        protected override IEnumerable<string> GetSharedKeys () => DataMultiLinkerWorkshopProject.textKeysSharedSubtitles;
    }
    
    public class DataBlockWorkshopTextSourceDesc : DataBlockWorkshopTextSource
    {
        protected override IEnumerable<string> GetSharedKeys () => DataMultiLinkerWorkshopProject.textKeysSharedDescriptions;
        protected override bool IsShortTextPreferred () => false;
    }
    
    [LabelWidth (180f)]
    public class DataContainerWorkshopProject : DataContainerWithText, IDataContainerTagged
    {
        [ShowIf ("IsCoreVisible")]
        [HorizontalGroup ("Header", 180f), PropertyOrder (-5), ToggleLeft]
        public bool hidden;
        
        public bool rewarded = true;

        [ShowIf ("IsTextPreviewVisible"), ReadOnly, BoxGroup ("fgTextBox", false)]
        [ShowInInspector, PropertyOrder (-4)]
        private string textNamePreview => WorkshopUtility.GetProjectName (this);
        
        [ShowIf ("IsTextPreviewVisible"), ReadOnly, BoxGroup ("fgTextBox")]
        [ShowInInspector, PropertyOrder (-4)]
        private string textSubtitlePreview => WorkshopUtility.GetProjectSubtitle (this);
        
        [ShowIf ("IsTextPreviewVisible"), ReadOnly, BoxGroup ("fgTextBox")]
        [MultiLineProperty (5)]
        [ShowInInspector, PropertyOrder (-4)]
        private string textDescriptionPreview => WorkshopUtility.GetProjectDescription (this);

        [DropdownReference (true)]
        public DataBlockWorkshopTextSourceName textSourceName;
        
        [DropdownReference (true)]
        public DataBlockWorkshopTextSourceSubtitle textSourceSubtitle;
        
        [DropdownReference (true)]
        public DataBlockWorkshopTextSourceDesc textSourceDesc;
        
        [YamlIgnore]
        [LabelText ("Header / Desc.")]
        [DisableIf ("@textSourceName != null")]
        public string textName;
        
        [YamlIgnore]
        [HideLabel, TextArea (1, 10)]
        [DisableIf ("@textSourceDesc != null")]
        public string textDesc;

        [ShowIf ("IsCoreVisible")]
        [DataEditor.SpriteNameAttribute (false, 180f)]
        public string icon;

        [ShowIf ("AreTagsVisible")]
        [ValueDropdown ("@DataMultiLinkerWorkshopProject.tags")]
        public HashSet<string> tags = new HashSet<string> ();
        
        [ShowIf ("AreInputsOutputsVisible")]
        [DropdownReference (true)]
        public DataBlockFloat duration;
        
        [DropdownReference, DictionaryKeyDropdown ("@DataMultiLinkerBasePart.data.Keys")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, int> basePartRequirements;
        
        [DropdownReference, DictionaryKeyDropdown ("@DataMultiLinkerBaseStat.data.Keys")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, DataBlockOverworldEventSubcheckFloat> baseStatRequirements;
        
        [ShowIf ("AreInputsOutputsVisible")]
        [DropdownReference (true)]
        public DataBlockInt inputCharges;
        
        [ShowIf ("AreInputsOutputsVisible")]
        [DropdownReference, ListDrawerSettings (CustomAddFunction = "@new DataBlockResourceCost ()")]
        public List<DataBlockResourceCost> inputResources;

        [ShowIf ("AreInputsOutputsVisible")]
        [DropdownReference, ListDrawerSettings (CustomAddFunction = "@new DataBlockWorkshopPart ()")]
        public List<DataBlockWorkshopPart> outputParts;
        
        [ShowIf ("AreInputsOutputsVisible")]
        [DropdownReference, ListDrawerSettings (CustomAddFunction = "@new DataBlockWorkshopSubsystem ()")]
        public List<DataBlockWorkshopSubsystem> outputSubsystems;
        
        [ShowIf ("AreInputsOutputsVisible")]
        [DropdownReference, ListDrawerSettings (CustomAddFunction = "@new DataBlockWorkshopUnit ()")]
        public List<DataBlockWorkshopUnit> outputUnits;
        
        [ShowIf ("AreInputsOutputsVisible")]
        [DropdownReference, ListDrawerSettings (CustomAddFunction = "@new DataBlockWorkshopOutputResource ()")]
        public List<DataBlockWorkshopOutputResource> outputResources;
        
        [ShowIf ("AreInputsOutputsVisible")]
        [DropdownReference]
        public List<IOverworldFunction> outputFunctions;

        [ShowIf ("AreInputsOutputsVisible")]
        [DropdownReference (true)]
        public DataBlockWorkshopVariantLink variantLinkPrimary;
        
        [ShowIf ("AreInputsOutputsVisible")]
        [DropdownReference (true)]
        public DataBlockWorkshopVariantLink variantLinkSecondary;

        public override void OnBeforeSerialization ()
        {
            base.OnBeforeSerialization ();
        }

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
        }

        public override void ResolveText ()
        {
            // textSubtitle = !string.IsNullOrEmpty (textSubtitleKey) ? 
            //     DataManagerText.GetText (TextLibs.workshopShared, textSubtitleKey, true) : string.Empty;

            if (textSourceName == null)
                textName = DataManagerText.GetText (TextLibs.workshopEmbedded, $"{key}_header", true);
            
            if (textSourceDesc == null)
                textDesc = DataManagerText.GetText (TextLibs.workshopEmbedded, $"{key}_text", true);
        }
        
        public HashSet<string> GetTags (bool processed) => 
            tags;
        
        public bool IsHidden () => hidden;

        public SortedDictionary<string, DataBlockWorkshopProjectVariant> GetVariantsPrimary ()
        {
            if (variantLinkPrimary == null)
                return null;
            
            var sharedData = DataMultiLinkerWorkshopVariants.GetEntry (variantLinkPrimary.key);
            return sharedData?.variants;
        }

        public SortedDictionary<string, DataBlockWorkshopProjectVariant> GetVariantsSecondary ()
        {
            if (variantLinkSecondary == null)
                return null;
            
            var sharedData = DataMultiLinkerWorkshopVariants.GetEntry (variantLinkSecondary.key);
            return sharedData?.variants;
        }

        public DataBlockWorkshopProjectVariant GetVariantPrimary (string variantKey)
        {
            var variants = GetVariantsPrimary ();
            if (variants != null && !string.IsNullOrEmpty (variantKey) && variants.ContainsKey (variantKey))
                return variants[variantKey];
            else 
                return null;
        }
        
        public DataBlockWorkshopProjectVariant GetVariantSecondary (string variantKey)
        {
            var variants = GetVariantsSecondary ();
            if (variants != null && !string.IsNullOrEmpty (variantKey) && variants.ContainsKey (variantKey))
                return variants[variantKey];
            else 
                return null;
        }



        #region Editor
        #if UNITY_EDITOR
           
        [ShowInInspector]
        [ShowIf ("AreInputsOutputsVisible")]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerWorkshopProject () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        public override void SaveText ()
        {
            if (textSourceName == null)
            {
                DataManagerText.TryAddingTextToLibrary (TextLibs.workshopEmbedded, $"{key}_header", textName);
            }

            if (textSourceDesc == null)
            {
                DataManagerText.TryAddingTextToLibrary (TextLibs.workshopEmbedded, $"{key}_text", textDesc);
            }
        }

        private bool IsCoreVisible => DataMultiLinkerWorkshopProject.Presentation.showCore;
        private bool IsTextPreviewVisible => DataMultiLinkerWorkshopProject.Presentation.showTextPreview;
        private bool AreTagsVisible => DataMultiLinkerWorkshopProject.Presentation.showTags;
        private bool AreInputsOutputsVisible => DataMultiLinkerWorkshopProject.Presentation.showInputsOutputs;
        
        [PropertyOrder (10)]
        [FoldoutGroup ("Testing", false)]
        [YamlIgnore, ShowInInspector]
        [LabelText ("Variant (Primary)"), InlineButtonClear]
        [ValueDropdown ("GetPrimaryVariantKeys")]
        public string variantPrimaryKey;
        
        [PropertyOrder (10)]
        [FoldoutGroup ("Testing", false)]
        [YamlIgnore, ShowInInspector]
        [LabelText ("Variant (Secondary)"), InlineButtonClear]
        [ValueDropdown ("GetSecondaryVariantKeys")]
        public string variantSecondaryKey;
        
        [PropertyOrder (10)]
        [FoldoutGroup ("Testing", false)]
        [Button ("Test output", ButtonSizes.Large)]
        public void Process ()
        {
            if (output == null)
                output = new DataBlockWorkshopProjectProcessed ();
            
            output.ProcessProject (this, variantPrimaryKey, variantSecondaryKey);
        }

        [PropertyOrder (11), PropertySpace (8f)]
        [FoldoutGroup ("Testing", false)]
        [YamlIgnore, ShowInInspector]
        public DataBlockWorkshopProjectProcessed output;

        private IEnumerable<string> GetPrimaryVariantKeys () => GetVariantsPrimary ()?.Keys;
        private IEnumerable<string> GetSecondaryVariantKeys () => GetVariantsSecondary ()?.Keys;

        #endif

        #endregion
    }
}

