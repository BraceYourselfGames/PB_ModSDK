using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Text;
using Content.Code.Utility;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public static class PilotStatKeys
    {
        public const string hp = "hp";
        public const string hpRecovery = "hp_recovery";
        public const string fatigue = "fatigue";
        public const string trauma = "trauma";
        public const string score = "score";
        
        public const string supportSalvageBudget = "support_salvage_budget";
        public const string supportScrambleRange = "support_scramble_range";
        
        public const string combatHealthCached = "combat_hp_cached";
        public const string combatHealthHits = "combat_hp_hits";
        
        public const string selfOffsetExperience = "self_offset_exp";

        public const string critChanceOffset = "crit_chance_offset";
        public const string critMultiplierOffset = "crit_multiplier_offset";
            
        public const string unitOffsetDamage = "unit_offset_damage";
        public const string unitOffsetSpeed = "unit_offset_speed";
        public const string unitOffsetOverheatDamage = "unit_offset_overheat_damage";
        
        public const string unitShieldResistBuildup = "unit_shield_resist_buildup";
        public const string unitShieldResistDamage = "unit_shield_resist_damage";
    }

    public enum PilotStatReset
    {
        None,
        AnyChange,
        RangeChange
    }
    
    public class DataFilterPartPreset : DataBlockFilterLinked<DataContainerPartPreset>
    {
        public override IEnumerable<string> GetTags () => DataMultiLinkerPartPreset.GetTags ();
        public override SortedDictionary<string, DataContainerPartPreset> GetData () => DataMultiLinkerPartPreset.data;
        protected override string GetTooltip () => "Part preset filter.";
    }
    
    public class DataFilterUnitPreset : DataBlockFilterLinked<DataContainerUnitPreset>
    {
        public override IEnumerable<string> GetTags () => DataMultiLinkerUnitPreset.GetTags ();
        public override SortedDictionary<string, DataContainerUnitPreset> GetData () => DataMultiLinkerUnitPreset.data;
        protected override string GetTooltip () => "Unit preset filter.";
    }
    
    public class DataFilterUnitGroup : DataBlockFilterLinked<DataContainerCombatUnitGroup>
    {
        public override IEnumerable<string> GetTags () => DataMultiLinkerCombatUnitGroup.GetTags ();
        public override SortedDictionary<string, DataContainerCombatUnitGroup> GetData () => DataMultiLinkerCombatUnitGroup.data;
        protected override string GetTooltip () => "Unit group filter.";
    }

    public enum PilotUnitStatEffect
    {
        OffsetAbsolute,
        OffsetMultiplier
    }
    
    public class DataBlockPilotStatUnit
    {
        [ValueDropdown("@DataMultiLinkerUnitStats.data.Keys")] 
        public string statKey;
        
        public PilotUnitStatEffect effect = PilotUnitStatEffect.OffsetAbsolute;
        
        public float multiplier = 1;
    }

    public class DataBlockPilotStatUnitPart
    {
        [ValueDropdown("@DataMultiLinkerUnitStats.data.Keys")] 
        public string statKey;

        public PilotUnitStatEffect effect = PilotUnitStatEffect.OffsetAbsolute;
        public float multiplier = 1;
        
        [ValueDropdown ("@DataMultiLinkerPartSocket.data.Keys")]
        public HashSet<string> sockets = new HashSet<string> ();

        [DropdownReference (true)]
        public DataFilterPartPreset filterPartPreset;
        
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockPilotStatUnitPart () =>
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
    }

    public class DataBlockStatValueEffect
    {
        [TextArea (1, 10), YamlIgnore]
        public string textHint;
    }
    
    public class DataBlockCombatActionLink
    {
        [ValueDropdown ("@DataMultiLinkerAction.data.Keys")]
        public string key;
    }

    public class DataContainerPilotStat : DataContainerWithText
    {
        [ToggleLeft]
        public bool hidden;
        
        [HideIf (nameof(hidden))]
        [HorizontalGroup ("Color")]
        [OnValueChanged (nameof (OnColorChange), true), ColorUsage (false)]
        public Color color = new Color (0.75f, 0.3f, 0.3f, 1f);
        
        [HideIf (nameof(hidden))]
        [HorizontalGroup ("Color", 40f)]
        [YamlIgnore, ReadOnly, HideLabel, ColorUsage (false)]
        public Color colorHighlightDark;
        
        [HideIf (nameof(hidden))]
        [HorizontalGroup ("Color", 40f)]
        [YamlIgnore, ReadOnly, HideLabel, ColorUsage (false)]
        public Color colorHighlight;
        
        [HideIf (nameof(hidden))]
        [HorizontalGroup ("Color", 40f)]
        [YamlIgnore, ReadOnly, HideLabel, ColorUsage (false)]
        public Color colorHighlightMax;
        
        [HideIf (nameof(hidden))]
        [HorizontalGroup ("Color", 56f)]
        [YamlIgnore, ReadOnly, HideLabel]
        public string colorTagNormal;
        
        [HideIf (nameof(hidden))]
        [HorizontalGroup ("Color", 56f)]
        [YamlIgnore, ReadOnly, HideLabel]
        public string colorTagHighlight;
        
        [HideIf (nameof(hidden))]
        [YamlIgnore, HideInInspector]
        public float colorHue;
        
        [HideIf (nameof(hidden))]
        public int priority = 0;
        
        public bool unitEffect;
        public bool mutable;
        
        [ShowIf (nameof(mutable))]
        public bool resetOnDeployment;
        
        [ShowIf (nameof(mutable))]
        public bool refillOnDeployment;
        
        [Space (4f)]
        [HideIf (nameof(hidden))]
        public bool displayRounded;
        
        [HideIf (nameof(hidden))]
        public bool displayAtDefault;
        
        [HideIf (nameof(hidden))]
        public bool displayInList = true;
        
        [HideIf (nameof(hidden))]
        public bool displayInDebriefing = false;
        
        [HideIf (nameof(hidden))]
        public bool displayChangePositive = true;
        
        [HideIf (nameof(hidden))]
        public string displayFormat;
        
        [HideIf (nameof(hidden))]
        [DataEditor.SpriteNameAttribute (true, 32f)]
        public string icon;
        
        [Space (4f)]
        [HideIf (nameof(hidden))]
        [LabelText ("Name / Hint / Desc"), YamlIgnore]
        public string textName;
        
        [HideIf (nameof(hidden))]
        [HideLabel, TextArea (1, 2), YamlIgnore]
        public string textHint;
        
        [HideIf (nameof(hidden))]
        [HideLabel, TextArea (1, 10), YamlIgnore]
        public string textDesc;

        [DropdownReference]
        public DataBlockStringNonSerialized textNameMin;
        
        [DropdownReference]
        public DataBlockStringNonSerialized textNameMax;

        [HideIf (nameof(hidden))]
        [InfoBox ("$GetTextUnitStat")]
        [DropdownReference (true)]
        [ValueDropdown("@DataMultiLinkerUnitStats.data.Keys")] 
        public string textSourceUnitStat;
        
        [DropdownReference (true)]
        public DataBlockCombatActionLink actionLink;
        
        [Space (4f)]
        [ShowIf ("@rangeDefault != null")]
        public bool valueResetToMax = false;

        [ShowIf ("@valueDefault != null && rangeDefault != null")]
        public bool valueDefaultInterpolant = false;
        
        [InfoBox ("$GetValueDefaultText")]
        [DropdownReference (true)]
        public DataBlockFloat valueDefault;
        
        [DropdownReference (true)]
        public DataBlockVector2 rangeDefault;
        
        [DropdownReference (true)]
        public DataBlockFloat rangeMaxFloor;
        
        [DropdownReference (true)]
        public DataBlockFloat rangeMaxCeiling;
        
        [DropdownReference (true)]
        public DataBlockPilotStatUnit targetUnit;

        [DropdownReference (true)]
        public DataBlockPilotStatUnitPart targetUnitPart;
        
        [DropdownReference (true)]
        public DataBlockStatValueEffect valueMaxEffect;

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
            OnColorChange ();
        }

        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.pilotStats, $"{key}__header", true);
            textHint = DataManagerText.GetText (TextLibs.pilotStats, $"{key}__hint", true);
            textDesc = DataManagerText.GetText (TextLibs.pilotStats, $"{key}__text", true);
            
            if (textNameMin != null)
                textNameMin.s = DataManagerText.GetText (TextLibs.pilotStats, $"{key}__header_min", true);
            
            if (textNameMax != null)
                textNameMax.s = DataManagerText.GetText (TextLibs.pilotStats, $"{key}__header_max", true);
            
            if (valueMaxEffect != null)
                valueMaxEffect.textHint = DataManagerText.GetText (TextLibs.pilotStats, $"{key}__max_effect", true);
        }
        
        private void OnColorChange ()
        {
            var colorHSB = new HSBColor (color);
            colorHue = colorHSB.h;

            colorHighlight = Color.HSVToRGB (colorHSB.h, 0.32f * colorHSB.s, 1f).WithAlpha (1f);
            colorHighlightMax = Color.Lerp (colorHighlight, Color.white, 0.5f).WithAlpha (1f);
            colorHighlightDark = Color.Lerp (colorHighlight, Color.black, 0.5f).WithAlpha (1f);
            
            colorTagNormal = UtilityColor.ToHexRGB (color);
            colorTagHighlight = UtilityColor.ToHexRGB (colorHighlight);
        }

        private static StringBuilder sb = new StringBuilder ();

        public string GetTextName ()
        {
            if (!string.IsNullOrEmpty (textSourceUnitStat))
            {
                var unitStat = DataMultiLinkerUnitStats.GetEntry (textSourceUnitStat, false);
                var unitStatName = !string.IsNullOrEmpty (unitStat?.textName) ? unitStat.textName : textSourceUnitStat;

                if (string.IsNullOrEmpty (textName))
                    return unitStatName;

                sb.Clear ();
                sb.Append (unitStatName);
                sb.Append (" (");
                sb.Append (textName);
                sb.Append (")");
                return sb.ToString ();
            }

            if (actionLink != null)
            {
                var actionData = DataMultiLinkerAction.GetEntry (actionLink.key, false);
                if (actionData?.dataUI != null)
                {
                    sb.Clear ();
                    sb.Append (Txt.Get (TextLibs.uiBase, "pilot_ability_prefix"));
                    sb.Append (": ");
                    sb.Append (!string.IsNullOrEmpty (actionData.dataUI.textName) ? actionData.dataUI.textName : "?");

                    return sb.ToString ();
                }
            }

            return textName;
        }
        
        public string GetTextNameMin ()
        {
            if (textNameMin != null && !string.IsNullOrEmpty (textNameMin.s))
                return textNameMin.s;

            var tn = GetTextName ();
            tn += " / " + Txt.Get (TextLibs.uiBase, "pilot_stat_detail_min");
            return tn;
        }

        public string GetTextNameMax ()
        {
            if (textNameMax != null && !string.IsNullOrEmpty (textNameMax.s))
                return textNameMax.s;

            var tn = GetTextName ();
            tn += " / " + Txt.Get (TextLibs.uiBase, "pilot_stat_detail_max");
            return tn;
        }
        
        public string GetTextDesc ()
        {
            if (!string.IsNullOrEmpty (textSourceUnitStat))
            {
                var unitStat = DataMultiLinkerUnitStats.GetEntry (textSourceUnitStat, false);
                var unitStatDesc = !string.IsNullOrEmpty (unitStat?.textDesc) ? unitStat.textDesc : textSourceUnitStat;

                if (string.IsNullOrEmpty (textDesc))
                    return unitStatDesc;
            
                sb.Clear ();
                sb.Append (textDesc);
                sb.Append ("\n\n[cc]");
                sb.Append (unitStatDesc);
                sb.Append ("[ff]");
                return sb.ToString ();
            }

            if (actionLink != null)
            {
                var actionData = DataMultiLinkerAction.GetEntry (actionLink.key, false);
                if (actionData?.dataUI != null)
                {
                    sb.Clear ();
                    sb.Append (Txt.Get (TextLibs.uiBase, "pilot_ability_hint"));
                    sb.Append ("\n\n");
                    sb.Append (!string.IsNullOrEmpty (actionData.dataUI?.textDesc) ? actionData.dataUI.textDesc : "?");
                    return sb.ToString ();
                }
            }

            return textDesc;
        }

        #if UNITY_EDITOR
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            if (!string.IsNullOrEmpty (textName))
                DataManagerText.TryAddingTextToLibrary (TextLibs.pilotStats, $"{key}__header", textName);
            
            if (!string.IsNullOrEmpty (textHint))
                DataManagerText.TryAddingTextToLibrary (TextLibs.pilotStats, $"{key}__hint", textHint);
            
            if (!string.IsNullOrEmpty (textDesc))
                DataManagerText.TryAddingTextToLibrary (TextLibs.pilotStats, $"{key}__text", textDesc);
            
            if (textNameMin != null && !string.IsNullOrEmpty (textNameMin.s))
            {
                DataManagerText.TryAddingTextToLibrary (TextLibs.pilotStats, $"{key}__header_min", textNameMin.s);
            }
            
            if (textNameMax != null && !string.IsNullOrEmpty (textNameMax.s))
            {
                DataManagerText.TryAddingTextToLibrary (TextLibs.pilotStats, $"{key}__header_max", textNameMax.s);
            }
            
            if (valueMaxEffect != null && !string.IsNullOrEmpty (valueMaxEffect.textHint))
            {
                DataManagerText.TryAddingTextToLibrary (TextLibs.pilotStats, $"{key}__max_effect", valueMaxEffect.textHint);
            }
            
            
        }
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataContainerPilotStat () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        private string GetValueDefaultText ()
        {
            if (rangeDefault != null)
            {
                return "Default value must be in 0-1 range and is defined in normalized space relative to the range. For example, 1.0 equals 100% (range maximum).";
            }
            else
            {
                return "Default value is absolute as this stat does not define a range. For example, it could be equal to 0.5, 3, 75 etc.";
            }
        }

        private string GetTextUnitStat ()
        {
            if (string.IsNullOrEmpty (textSourceUnitStat))
                return string.Empty;
            
            var unitStat = DataMultiLinkerUnitStats.GetEntry (textSourceUnitStat, false);
            if (unitStat == null)
                return string.Empty;

            sb.Clear ();
            sb.Append (unitStat.textName);
            sb.Append ("\n");
            sb.Append (unitStat.textDesc);
            return sb.ToString ();
        }

        #if !PB_MODSDK
        private PersistentEntity GetTargetPilot ()
        {
            if (!Application.isPlaying)
                return null;
            
            if (!IDUtility.IsGameState (GameStates.combat))
                return null;

            var combat = Contexts.sharedInstance.combat;
            if (!combat.hasUnitSelected)
                return null;
            
            var unitCombat = IDUtility.GetCombatEntity (combat.unitSelected.id);
            if (unitCombat == null)
                return null;

            var unitPersistent = IDUtility.GetLinkedPersistentEntity (unitCombat);
            if (unitPersistent == null)
                return null;
            
            var pilot = IDUtility.GetLinkedPilot (unitPersistent);
            return pilot;
        }

        private static int testLimit = 8;
        private bool ArePilotCommandsAvailable () => GetTargetPilot () != null;
        
        [ButtonGroup, Button, PropertyOrder (-1), ShowIf (nameof(ArePilotCommandsAvailable))]
        private void SetTestLimitAndFill ()
        {
            var pilot = GetTargetPilot ();
            if (pilot == null)
                return;

            pilot.SetPilotStatRangeMax (key, testLimit);
            pilot.SetPilotStat (key, testLimit);
            CIControllerCombat.OnPilotUpdate (pilot, true);
        }

        [ButtonGroup, Button, PropertyOrder (-1), ShowIf (nameof(ArePilotCommandsAvailable))]
        private void RefillToLimit ()
        {
            var pilot = GetTargetPilot ();
            if (pilot == null)
                return;

            var ranged = PilotUtility.IsPilotStatRanged (pilot, key, out var range);
            if (ranged)
            {
                pilot.SetPilotStat (key, range.y);
                CIControllerCombat.OnPilotUpdate (pilot, true);
            }
        }
        
        [Button, PropertyOrder (-1), ShowIf (nameof(ArePilotCommandsAvailable))]
        private void SetTo (float value)
        {
            var pilot = GetTargetPilot ();
            if (pilot == null)
                return;

            pilot.SetPilotStat (key, value);
            CIControllerCombat.OnPilotUpdate (pilot, true);
        }
        
        [Button, PropertyOrder (-1), ShowIf (nameof(ArePilotCommandsAvailable))]
        private void SetLimit (float value)
        {
            var pilot = GetTargetPilot ();
            if (pilot == null)
                return;

            pilot.SetPilotStatRangeMax (key, value);
            CIControllerCombat.OnPilotUpdate (pilot, true);
        }
        #endif

        #endif
    }
}

