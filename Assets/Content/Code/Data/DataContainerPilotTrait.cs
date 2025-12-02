using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Text;
using Content.Code.Utility;
using PhantomBrigade.Functions;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public class DataBlockFloatChange
    {
        public enum Mode
        {
            Set,
            Offset,
            Multiply,
            SubtractFrom,
            Min,
            Max,
            Round
        }
        
        public Mode mode = Mode.Offset;
        public float value = 0f;

        public float ApplyToValue (float valueCurrent)
        {
            var valueModified = valueCurrent;
            
            if (mode == Mode.Set)
                valueModified = value;
            else if (mode == Mode.Offset)
                valueModified = valueCurrent + value;
            else if (mode == Mode.Multiply)
                valueModified = valueCurrent * value;
            else if (mode == Mode.SubtractFrom)
                valueModified = value - valueCurrent;
            else if (mode == Mode.Min)
                valueModified = Mathf.Min (valueCurrent, value);
            else if (mode == Mode.Max)
                valueModified = Mathf.Max (valueCurrent, value);
            else if (mode == Mode.Round)
            {
                int roundingIncrement = Mathf.RoundToInt (Mathf.Max (1f, value));
                valueModified = Mathf.RoundToInt (valueCurrent / roundingIncrement) * roundingIncrement;
            }
            
            return valueModified;
        }

        public void AppendToStringBuilder (StringBuilder sb, string format)
        {
            if (sb == null)
                return;
            
            if (mode == Mode.Set)
                sb.Append (value.ToString (format));
            else if (mode == Mode.Offset)
            {
                var valueAbs = Mathf.Abs (value);
                sb.Append (value > 0f ? "+" : "-");
                sb.Append (valueAbs.ToString (format));
            }
            else if (mode == Mode.Multiply)
            {
                var valueOffset = Mathf.Max (0f, value) - 1f;
                var valueAbsPercent = Mathf.RoundToInt (Mathf.Abs (valueOffset) * 100f);
                sb.Append (valueOffset > 0f ? "+" : "-");
                sb.Append (valueAbsPercent);
                sb.Append ("%");
            }
            else
            {
                sb.Append ("?");
            }
        }
    }
    
    public class DataBlockFilterEventPilot
    {
        [InfoBox ("This event does not record a target entity besides the pilot and can't be used. Consider using the type filter instead.", InfoMessageType.Error, VisibleIf = "$IsHintVisible")]
        [LabelText ("On Pilot/Entity Event")]
        public PilotEventType eventType;

        private bool IsHintVisible ()
        {
            return !PilotUtility.pilotEventTypesWithEntities.Contains (eventType);
        }
    }
    
    public class DataBlockPilotTraitEffectConditional : DataBlockPilotTraitEffect
    {
        [InlineButtonClear, ShowIf (nameof(filterEventTypes))]
        [PropertyOrder (-1)]
        [LabelText ("$GetFilterLabel"), LabelWidth (140f)]
        [ValueDropdown("@PilotUtility.pilotEventTypesList")]
        public HashSet<PilotEventType> filterEventTypes;

        [PropertyOrder (-1)]
        [InlineButtonClear, ShowIf (nameof(filterEventEntity))]
        [HideReferenceObjectPicker, HideLabel]
        public DataBlockFilterEventPilot filterEventEntity;
        
        [PropertyOrder (-2), GUIColor ("red")]
        [HideIf ("@filterEventEntity != null || filterEventTypes != null")]
        [Button ("Trigger on event(s)"), ButtonGroup ("G")]
        private void AddFilterEvent ()
        {
            filterEventTypes = new HashSet<PilotEventType> { PilotEventType.Unknown };
        }

        [PropertyOrder (-2), GUIColor ("red")]
        [HideIf ("@filterEventEntity != null || filterEventTypes != null")]
        [Button ("Trigger on entity event"), ButtonGroup ("G")]
        private void AddFilterEntity ()
        {
            filterEventEntity = new DataBlockFilterEventPilot { eventType = PilotEventType.CombatUnitTakedownEnemy };
        }

        private string GetFilterLabel ()
        {
            return filterEventTypes != null && filterEventTypes.Count > 1 ? "On Pilot Events" : "On Pilot Event";
        }
        
        private bool IsHintVisible ()
        {
            return filterEventTypes == null && filterEventEntity == null;
        }
        
        #if UNITY_EDITOR

        private void OnInspectorGUI ()
        {
            GUILayout.Label ("Test");
        }
        
        #endif
    }
    
    public class DataBlockPilotTraitEffect
    {
        [LabelText ("Active on disabled pilots")]
        public bool activeOnDisabledPilots = false;
        
        [DropdownReference]
        public List<IOverworldGlobalValidationFunction> checksGlobal;
        
        [DropdownReference]
        public List<IOverworldEntityValidationFunction> checksBase;
        
        [DropdownReference]
        public List<IPilotValidationFunction> checksSelf;
        
        [DropdownReference]
        public List<ICombatUnitValidationFunction> checksUnit;

        [DropdownReference]
        public List<IOverworldFunction> functionsGlobal;
        
        [DropdownReference]
        public List<ICombatFunction> functionsCombat;
        
        [DropdownReference]
        public List<IOverworldTargetedFunction> functionsBase;

        [DropdownReference]
        public List<ICombatFunctionTargeted> functionsUnit;
        
        [DropdownReference]
        public List<IPilotTargetedFunction> functionsSelf;
        
        [DropdownReference]
        public List<ICombatFunctionTargeted> functionsEventUnit;
        
        [DropdownReference (true)]
        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerPilotTrait\", \"GetEffectKeys\")")]
        public HashSet<string> linkedEffectKeys;

        [YamlIgnore, HideInInspector]
        public string key;
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockPilotTraitEffect () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    public enum DataDevState
    {
        None = 0,
        Test,
        Blocked,
        InProgress,
        ReadyForReview,
        Finalized
    }
    
    [LabelWidth (180f)]
    public class DataContainerPilotTrait : DataContainerWithText, IDataContainerTagged
    {
        [DropdownReference (true), HideLabel]
        public DataBlockComment comment;
        
        [InfoBox ("$GetStateHint", InfoMessageType.None, VisibleIf = "@state != DataDevState.None"), GUIColor ("$GetStateColor")]
        public DataDevState state = DataDevState.None; 
        
        public bool hidden;
        public int priority;
        public int level;
        
        [DataEditor.SpriteNameAttribute (false, 40f)]
        public string icon;
        
        [LabelText ("Name / Desc"), YamlIgnore]
        public string textName;
        
        [HideLabel, TextArea (1, 10), YamlIgnore]
        public string textDesc;

        [ValueDropdown ("@DataMultiLinkerPilotTrait.tags")]
        public HashSet<string> tags;
        
        [InfoBox ("If a trait has a declared group, only one trait with that group can be selected across entire pilot level tree.", InfoMessageType.None)]
        [DropdownReference (true)]
        public string group;
        
        [DropdownReference (true)]
        [DictionaryKeyDropdown ("@DataMultiLinkerPilotTrait.GetTags ()")]
        public SortedDictionary<string, bool> existingTraitTagsFilter;
        
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckInt prestigeRank;
        
        [DropdownReference (true)]
        public DataBlockCombatActionLink actionLink;
        
        [DropdownReference]
        [ValueDropdown ("@DataMultiLinkerPilotType.data.Keys")]
        public HashSet<string> pilotTypesExclusive;
        
        [DropdownReference]
        [ValueDropdown ("@DataMultiLinkerPilotType.data.Keys")]
        public HashSet<string> pilotTypesIncompatible;
        
        [DropdownReference]
        [ValueDropdown ("@DataMultiLinkerPilotTrait.tags")]
        public HashSet<string> activationRecordTags;

        [DropdownReference]
        [DictionaryKeyDropdown ("@DataMultiLinkerPilotStat.data.Keys")]
        public SortedDictionary<string, DataBlockFloatChange> statValues;
        
        [DropdownReference]
        [DictionaryKeyDropdown ("@DataMultiLinkerPilotStat.data.Keys")]
        public SortedDictionary<string, DataBlockFloatChange> statRangesMin;
        
        [DropdownReference]
        [DictionaryKeyDropdown ("@DataMultiLinkerPilotStat.data.Keys")]
        public SortedDictionary<string, DataBlockFloatChange> statRangesMax;
        
        [DropdownReference]
        [ListDrawerSettings (CustomAddFunction = "@new DataBlockPilotTraitEffectConditional ()")]
        public List<DataBlockPilotTraitEffectConditional> effectsOnEvents;
        
        [DropdownReference]
        public SortedDictionary<string, DataBlockPilotTraitEffect> effectsShared;
        
        public HashSet<string> GetTags (bool processed) => 
            tags;

        public bool IsHidden () => hidden;
        
        private IEnumerable<string> GetEffectKeys => effectsShared != null ? effectsShared.Keys : null;
        private static StringBuilder sb = new StringBuilder ();

        public string GetIcon ()
        {
            if (actionLink != null)
            {
                // Allow text name to serve as an override if it is defined
                if (!string.IsNullOrEmpty (icon))
                    return icon;

                var actionData = DataMultiLinkerAction.GetEntry (actionLink.key, false);
                if (actionData?.dataUI != null)
                    return actionData.dataUI.icon;
            }

            return icon;
        }

        public string GetTextName ()
        {
            if (actionLink != null)
            {
                var actionData = DataMultiLinkerAction.GetEntry (actionLink.key, false);
                if (actionData != null && actionData.dataUI != null)
                {
                    // Allow text name to serve as an override if it is defined
                    if (!string.IsNullOrEmpty (textName))
                        return textName;

                    sb.Clear ();
                    sb.Append (Txt.Get (TextLibs.uiBase, "pilot_ability_prefix"));
                    sb.Append (": ");

                    sb.Append (!string.IsNullOrEmpty (actionData.dataUI.textName) ? actionData.dataUI.textName : "?");
                    return sb.ToString ();
                }
            }

            return textName;
        }

        public string GetTextDesc ()
        {
            if (actionLink != null)
            {
                var actionData = DataMultiLinkerAction.GetEntry (actionLink.key, false);
                if (actionData != null && actionData.dataUI != null)
                {
                    sb.Clear ();
                    if (!string.IsNullOrEmpty (textDesc))
                        sb.Append (textDesc);

                    if (sb.Length > 0)
                        sb.Append ("\n\n");
                    sb.Append (Txt.Get (TextLibs.uiBase, "pilot_ability_hint"));

                    if (sb.Length > 0)
                        sb.Append ("\n\n");

                    sb.Append (Txt.Get (TextLibs.uiBase, "pilot_ability_status_effect"));
                    sb.Append (": [b]");
                    sb.Append (actionData.dataUI.textName);
                    sb.Append ("[/b]\n[cc][i]");
                    sb.Append (actionData.dataUI.textDesc);
                    sb.Append ("[ff][/i]");

                    return sb.ToString ();
                }
            }

            return textDesc;
        }
        
        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);

            if (effectsOnEvents != null)
            {
                for (int i = 0, iLimit = effectsOnEvents.Count; i < iLimit; ++i)
                {
                    var effect = effectsOnEvents[i];
                    if (effect != null)
                        effect.key = "cond_" + i;
                }
            }
            
            if (effectsShared != null)
            {
                foreach (var kvp in effectsShared)
                {
                    if (kvp.Value != null)
                        kvp.Value.key = "sh_" + kvp.Key;
                }
            }
        }

        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.pilotTraits, $"{key}__c_header", true);
            textDesc = DataManagerText.GetText (TextLibs.pilotTraits, $"{key}__c_text", true);
        }

        #if UNITY_EDITOR
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            if (!string.IsNullOrEmpty (textName))
                DataManagerText.TryAddingTextToLibrary (TextLibs.pilotTraits, $"{key}__c_header", textName);
            
            if (!string.IsNullOrEmpty (textDesc))
                DataManagerText.TryAddingTextToLibrary (TextLibs.pilotTraits, $"{key}__c_text", textDesc);
        }
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataContainerPilotTrait () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        private string GetStateHint ()
        {
            switch (state)
            {
                case DataDevState.Test:
                    return "We don't need to worry about those, just got to remember to mark them hidden to disable them from appearing in random generation and to delete them before the next playtest. Useful if there is no completed content whatsoever in a given category.";
                case DataDevState.Blocked:
                    return "We can't progress further, there's a missing stat, function, event or something else blocking implementation. We'll review and add missing functionality or reconsider the trait if that can't be done.";
                case DataDevState.InProgress:
                    return "The trait is actively being iterated on and should not be reviewed or modified externally.";
                case DataDevState.ReadyForReview:
                    return "Artyom can review implementation, adjust if needed and mark it Final if everything looks good. If there are issues that can't be solved, the state would be set to In Progress instead.";
                case DataDevState.Finalized:
                    return "We won't touch it again except for balance changes unless we discover a bug later.";
                default:
                    return string.Empty;
            }
        }
        
        private Color GetStateColor ()
        {
            switch (state)
            {
                case DataDevState.Test:
                    return Color.HSVToRGB (0f, 0f, 0.8f);
                case DataDevState.Blocked:
                    return Color.HSVToRGB (0.01f, 0.5f, 1f);
                case DataDevState.InProgress:
                    return Color.HSVToRGB (0.2f, 0.3f, 1f);
                case DataDevState.ReadyForReview:
                    return Color.HSVToRGB (0.3f, 0.5f, 1f);
                case DataDevState.Finalized:
                    return Color.HSVToRGB (0.56f, 0.5f, 1f);
                case DataDevState.None:
                    return Color.HSVToRGB (0.95f, 0.5f, 1f);
                default:
                    return Color.white;
            }
            
            return Color.white;
        }

        #endif
    }
}

