using System;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public class DataBlockUnitStatFlags
    {
        public bool isDamage;
    }

    public class DataBlockUnitStatStatusRelationship
    {
        [ValueDropdown ("@DataMultiLinkerUnitStatus.data.Keys")]
        public string key;
    }
    
    [Serializable][LabelWidth (180f)]
    public class DataContainerUnitStat : DataContainerWithText
    {
        [YamlIgnore]
        [ShowIf ("IsUIVisible")]
        [LabelText ("Name / Desc.")]
        public string textName;
        
        [YamlIgnore]
        [TextArea][ShowIf ("IsUIVisible")]
        [HideLabel]
        public string textDesc;
        
        [ShowIf ("IsUIVisible")]
        [LabelText ("Priority")]
        public int uiPriority;
        
        [ShowIf ("IsUIVisible")]
        [LabelText ("Color")]
        public Color uiColor;
        
        [ShowIf ("@IsUIVisible && showAsOutput")]
        [LabelText ("Color (Output UI)")]
        public Color uiColorOutput;
        
        [ShowIf ("IsUIVisible")]
        [LabelText ("Icon")]
        [DataEditor.SpriteNameAttribute (true, 32f)]
        public string uiIcon;
        
        [ShowIf ("@IsUIVisible && showAsOutput")]
        [LabelText ("Icon (Output UI)")]
        [DataEditor.SpriteNameAttribute (true, 32f)]
        public string uiIconOutput;

        [ShowIf ("IsUIVisible")]
        [LabelText ("Limit")]
        public int uiLimit = 100;
        
        [ShowIf ("IsUIVisible")]
        [LabelText ("UI Multiplier")]
        public float uiMultiplier = 1;

        [ShowIf ("IsUIVisible")]
        [LabelText ("Invert Comparison")]
        public bool uiComparisonInverted = false;
        
        [ShowIf ("IsUIVisible")]
        [LabelText ("Display As Percentage")]
        public bool uiPercentage = false;
        
        [ShowIf ("IsUIVisible")]
        [LabelText ("Display Separator")]
        public bool uiSeparator = false;

        [ShowIf ("IsUIVisible")]
        [LabelText ("Rounded in UI")]
        public bool rounded = false;
        
        public bool derived = false;

        public DataBlockUnitStatFlags flags;

        [Space (8f)]
        [ShowIf ("IsExposureVisible")]
        [LabelText ("Show Per Unit")]
        public bool exposedPerUnit = true;
        
        [ShowIf ("IsExposureVisible")]
        [LabelText ("Show Per Part")]
        public bool exposedPerPart = true;
        
        [ShowIf ("IsExposureVisible")]
        [LabelText ("Show Per System")]
        public bool exposedPerSubsystem = true;
        
        [ShowIf ("IsExposureVisible")]
        [LabelText ("Show As Output")]
        public bool showAsOutput = false;
        
        [ShowIf ("IsExposureVisible")]
        [LabelText ("Show In Updated UI")]
        public bool showInList = true;
        
        [ShowIf ("IsExposureVisible")]
        [LabelText ("Show If Another Non-Zero (Key)")]
        [ValueDropdown("@DataMultiLinkerUnitStats.data.Keys")]
        [InlineButton ("@displayIfNonZero = string.Empty", "-", ShowIf = "@!string.IsNullOrEmpty (displayIfNonZero)")]
        public string displayIfNonZero = string.Empty;
        
        [ShowIf ("IsExposureVisible")]
        [LabelText ("Show As Range (Key)")]
        [ValueDropdown("@DataMultiLinkerUnitStats.data.Keys")]
        [InlineButton ("@displayAsRangeUsing = string.Empty", "-", ShowIf = "@!string.IsNullOrEmpty (displayAsRangeUsing)")]
        public string displayAsRangeUsing = string.Empty;

        [Space (8f)]
        [ShowIf ("AreValuesVisible")]
        [LabelText ("Avg. Subsystem Mult.")]
        public bool averageSubsystemMultipliers = false;

        [DropdownReference (true)]
        public DataBlockFloat minInPart;
        
        [DropdownReference (true)]
        public DataBlockFloat minInUnit;
        
        [DropdownReference (true)]
        public string minFromStat;
        
        [DropdownReference (true)]
        public DataBlockFloat maxInPart;
        
        [DropdownReference (true)]
        public DataBlockFloat maxInUnit;
        
        [DropdownReference (true)]
        public string maxFromStat;
        
        [DropdownReference (true)]
        public DataBlockFloat increasePerLevel;
        
        [DropdownReference (true)]
        public DataBlockFloat scale;
        
        [DropdownReference (true)]
        public DataBlockUnitStatStatusRelationship statusLink;
        
        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.unitStats, $"{key}__name");
            textDesc = DataManagerText.GetText (TextLibs.unitStats, $"{key}__desc");
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataContainerUnitStat () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.unitStats, $"{key}__name", textName);
            DataManagerText.TryAddingTextToLibrary (TextLibs.unitStats, $"{key}__desc", textDesc);
        }
        
        private static bool IsUIVisible => DataMultiLinkerUnitStats.Presentation.showUI;
        private static bool IsExposureVisible => DataMultiLinkerUnitStats.Presentation.showExposure;
        private static bool AreValuesVisible => DataMultiLinkerUnitStats.Presentation.showValues;

        private void UpdateStats ()
        {
            #if !PB_MODSDK

            if (Application.isPlaying)
                DataHelperStats.RefreshStatCacheForAllParts (key);

            #endif
        }
        
        #endif
        #endregion
    }
}

