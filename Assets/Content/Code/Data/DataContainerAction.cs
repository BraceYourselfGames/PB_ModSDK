using System;
using System.Collections.Generic;
using System.Linq;
using PhantomBrigade.Functions;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public static class ActionCustomFlagKeys
    {
        public const string RangeFromEquipment = "range_from_equipment";
        public const string MeleeDamageDispersed = "melee_damage_dispersed";
        public const string MeleeDamageSplash = "melee_damage_splash";
        public const string MeleeImpactCrash = "melee_impact_crash";

        private static List<string> keys;
        public static List<string> GetKeys ()
        {
            if (keys == null) keys = FieldReflectionUtility.GetConstantStringFieldValues (typeof (ActionCustomFlagKeys));
            return keys;
        }
    }

    public static class ActionCustomFloatKeys
    {
        public const string MeleeRangeMin = "melee_range_min";
        public const string MeleeRangeMax = "melee_range_max";
        
        public const string MeleeDurationStrike = "melee_duration_strike";
        public const string MeleeDurationDashOut = "melee_duration_dash_out";
        public const string MeleeDurationDashAlign = "melee_duration_dash_align";
        public const string MeleeDurationDashFull = "melee_duration_dash_full";
        
        public const string MeleeTimeImpact = "melee_time_impact";
        public const string MeleeDistanceIn = "melee_distance_in";
        public const string MeleeDistanceOut = "melee_distance_out";
        public const string MeleeDistanceOffset = "melee_distance_offset";
        
        public const string RangeMin = "range_min";
        public const string RangeMax = "range_max";
        public const string DistanceCancelThreshold = "distance_cancel_threshold";
        public const string AngleThreshold = "angle_threshold";
        
        private static List<string> keys;
        public static List<string> GetKeys ()
        {
            if (keys == null) keys = FieldReflectionUtility.GetConstantStringFieldValues (typeof (ActionCustomFloatKeys));
            return keys;
        }
    }
    
    public static class ActionCustomVectorKeys
    {
        public const string MeleeCollisionSize = "melee_hit_size";
        public const string MeleeCollisionOffset = "melee_hit_offset";

        private static List<string> keys;
        public static List<string> GetKeys ()
        {
            if (keys == null) keys = FieldReflectionUtility.GetConstantStringFieldValues (typeof (ActionCustomVectorKeys));
            return keys;
        }
    }

    public static class ActionCustomIntKeys
    {
        public const string MeleeAnimationVariant = "melee_animation_variant";
        
        private static List<string> keys;
        public static List<string> GetKeys ()
        {
            if (keys == null) keys = FieldReflectionUtility.GetConstantStringFieldValues (typeof (ActionCustomIntKeys));
            return keys;
        }
    }

    public static class ActionCustomStringKeys
    {
        private static List<string> keys;
        public static List<string> GetKeys ()
        {
            if (keys == null) keys = FieldReflectionUtility.GetConstantStringFieldValues (typeof (ActionCustomStringKeys));
            return keys;
        }
    }
    
    

    [Serializable]
    public class DataBlockActionCore
    {
        public bool locking;
        public TrackType trackType;
        public PaintingType paintingType;
        public HeatType heatType;
        public DurationType durationType;
        [ShowIf ("@durationType == DurationType.Data")]
        public float duration;
        
        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerUnitStats.data.Keys")]
        public string durationUnitStat;
        public float heatChange;
        public bool secondaryDirection;
        
        [DropdownReference]
        [ListDrawerSettings (ShowPaging = false)] 
        public List<string> eventsOnValidation;
        
        [DropdownReference]
        [ListDrawerSettings (ShowPaging = false)] 
        public List<string> eventsOnCreation;
        
        [DropdownReference]
        [ListDrawerSettings (ShowPaging = false)] 
        public List<string> eventsOnModification;
        
        [DropdownReference]
        [ListDrawerSettings (ShowPaging = false)] 
        public List<string> eventsOnStart;
        
        [DropdownReference]
        [ListDrawerSettings (ShowPaging = false)] 
        public List<string> eventsOnEnd;
        
        [DropdownReference]
        [ListDrawerSettings (ShowPaging = false)] 
        public List<string> eventsOnDispose;
        
        [DropdownReference]
        [ListDrawerSettings (ShowPaging = false)] 
        public List<ICombatActionValidationFunction> functionsOnValidation;
        
        [DropdownReference]
        [ListDrawerSettings (ShowPaging = false)] 
        public List<ICombatActionExecutionFunction> functionsOnCreation;
        
        [DropdownReference]
        [ListDrawerSettings (ShowPaging = false)] 
        public List<ICombatActionExecutionFunction> functionsOnModification;
        
        [DropdownReference]
        [ListDrawerSettings (ShowPaging = false)] 
        public List<ICombatActionExecutionFunction> functionsOnStart;
        
        [DropdownReference]
        [ListDrawerSettings (ShowPaging = false)] 
        public List<ICombatActionExecutionFunction> functionsOnUpdate;
        
        [DropdownReference]
        [ListDrawerSettings (ShowPaging = false)] 
        public List<ICombatActionExecutionFunction> functionsOnEnd;
        
        [DropdownReference]
        [ListDrawerSettings (ShowPaging = false)] 
        public List<ICombatActionExecutionFunction> functionsOnDispose;

        // [ListDrawerSettings (ShowPaging = false)] 
        // public Dictionary<string, DataBlockActionEquipmentRequirement> partRequirements;

        [DropdownReference (true)]
        public DataBlockUnitCheck check;
        
        [DropdownReference (true)]
        public DataBlockScenarioSubcheckUnit unitCheck;
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockActionCore () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
    
    

    [Serializable]
    public class DataBlockActionUI
    {
        [YamlIgnore]
        [ShowIf (DataEditor.textAttrArg)]
        public string textName;
        
        [YamlIgnore]
        [ShowIf (DataEditor.textAttrArg)]
        [Multiline (3)]
        public string textDesc;
        
        [DataEditor.SpriteNameAttribute (true, 32f)]
        public string icon;
        
        public Color color;
        public Color colorOverride;
        public int sortingPriority;
        public bool hidden;
        public bool irreversible;
        public bool heating;
        public bool offensive;
        
        public Dictionary<CombatUIModes, DataBlockActionUIMode> modes;
    }
    
    [Serializable]
    public class DataBlockActionUIMode
    {
        [YamlIgnore]
        [ShowIf (DataEditor.textAttrArg)]
        public string textHeader;
        
        [YamlIgnore]
        [ShowIf (DataEditor.textAttrArg)]
        [Multiline (3)]
        public string textTooltip;
    }
    
    [Serializable]
    public class DataBlockActionDependency
    {
        [ValueDropdown ("@DataMultiLinkerAction.data.Keys")]
        public string key;
    }


    [Serializable]
    public class DataBlockActionMovement
    {
        public float movementSpeedScalar = 1f;
    }
    

    [Serializable]
    public class DataBlockActionEquipment
    {
        public bool partUsed = true;
        
        [LabelText ("Socket")]
        [ShowIf ("partUsed")]
        [ValueDropdown ("@DataMultiLinkerPartSocket.data.Keys")]
        [InlineButton ("ClearSocket", "-")]
        public string partSocket;
        
        /*
        [LabelText ("Hardpoint")]
        [ShowIf ("partUsed")]
        [ValueDropdown ("@DataMultiLinkerSubsystemHardpoint.data.Keys")]
        [InlineButton ("ClearHardpoint", "-")]
        public string subsystemHardpoint;
        */
        
        #if UNITY_EDITOR
        
        private void ClearSocket (string value) => partSocket = null;
        // private void ClearHardpoint (string value) => subsystemHardpoint = null;
        
        #endif
    }

    [Serializable]
    public class DataBlockActionFX
    {
        [LabelText ("Socket")]
        [ShowIf ("partUsed")]
        [ValueDropdown ("@DataMultiLinkerPartSocket.data.Keys")]
        [InlineButton ("ClearSocket", "-")]
        public string socket;
        
        [LabelText ("Hardpoint")]
        [ShowIf ("partUsed")]
        [ValueDropdown ("@DataMultiLinkerSubsystemHardpoint.data.Keys")]
        [InlineButton ("ClearHardpoint", "-")]
        public string hardpoint;
        
        // TODO: Extend to allow spawning effects directly from here instead of relying on equipment
        
        #if UNITY_EDITOR
        
        private void ClearSocket (string value) => socket = null;
        private void ClearHardpoint (string value) => hardpoint = null;
        
        #endif
    }
    
    [Serializable]
    public class DataBlockActionAI
    {
		[Serializable]
		public struct PossibleActionType
		{
			//For which AI purpose is this action fit?
	        public AIActionType aiActionType;

            //How is the weight calculated?
	        public AIWeightType weightType;

            //If weight type is DataConstant, this is just the final weight of the action
            //If weight type is Equipment, then it scales the stat value we get from the specified part
		    [ShowIf ("@weightType == AIWeightType.DataConstant || weightType == AIWeightType.Equipment")]
		    public float weightConstant;

            //If weight type is Equipment, which part socket do we check to calculate the weight?
            [ShowIf ("@weightType == AIWeightType.Equipment")]
            [ValueDropdown("@DataHelperUnitEquipment.GetSockets ()")]
            public string partSocket;

            //If weight type is Equipment, which stat do we check to calculate the weight?
            [ShowIf ("@weightType == AIWeightType.Equipment")] 
            [ValueDropdown("@DataMultiLinkerUnitStats.data.Keys")]
            public string statUsed;
		}

		//Should the AI stop thinking after this action?  (example: ejection)
		public bool actionEndsPlanning;

        [ListDrawerSettings (ShowPaging = false)] 
        public List<PossibleActionType> possibleActionTypes = new List<PossibleActionType> ();
    }
    
    [Serializable]
    public class DataBlockActionVisualsOnStart
    {
        public bool reactionLightsUsed = true;
        
        [ValueDropdown("@DataMultiLinkerAssetPools.data.Keys")]
        [InlineButton ("@fxKey = string.Empty", "-")]
        public string fxKey = string.Empty;
    }
    
    

    [Serializable] 
    public class DataBlockActionCustom
    {
        [ValueDropdown ("@ActionCustomFlagKeys.GetKeys ()")]
        [OnValueChanged ("ValidateFlags")]
        public List<string> flags;
        
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.ActionCustomInt)]
        public SortedDictionary<string, int> ints;
        
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.ActionCustomFloat)]
        public SortedDictionary<string, float> floats;
        
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.ActionCustomVector)]
        public SortedDictionary<string, Vector3> vectors;
        
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.ActionCustomString)]
        public SortedDictionary<string, string> strings;

        
        
        
public void ValidateFlags ()
{
    if (flags != null)
    {
        flags = new HashSet<string> (flags).ToList ();
        flags.Sort ();
    }
}

        public bool IsFlagPresent (string key)
        {
            var found = !string.IsNullOrEmpty (key) && flags != null && flags.Contains (key);
            return found;
        }
        
        public bool TryGetInt (string key, out int result)
        {
            var found = !string.IsNullOrEmpty (key) && ints != null && ints.ContainsKey (key);
            result = found ? ints[key] : default;
            return found;
        }
        
        public bool TryGetFloat (string key, out float result)
        {
            var found = !string.IsNullOrEmpty (key) && floats != null && floats.ContainsKey (key);
            result = found ? floats[key] : default;
            return found;
        }
        
        public bool TryGetVector3 (string key, out Vector3 result)
        {
            var found = !string.IsNullOrEmpty (key) && vectors != null && vectors.ContainsKey (key);
            result = found ? vectors[key] : default;
            return found;
        }
        
        public bool TryGetString (string key, out string result)
        {
            var found = !string.IsNullOrEmpty (key) && strings != null && strings.ContainsKey (key);
            result = found ? strings[key] : default;
            return found;
        }
    }




    public enum TrackType
    {
        Primary,
        Secondary,
        Double
    }
    
    public enum PaintingType
    {
        Wait,		// Draw a line / facing with time
        Path,		// Draw a path to define order / time inferred
        Melee,      // Target a unit then define direction
        Dash,       // Target a point without pathfinding (just directional collision check)
        Targeting,	// Targets a point or unit / time placement
        TargetingDirectional, // Targets a point, then adds a secondary direction from that point
        Timing,		// Only needs to be placed at a specific time
		DualTiming	// Like timing, but also blocks the positional track
    }
 
    public enum DurationType
    {
        Variable,
        Data,
        Equipment,
        UnitStat
    }
    
    public static class CinematicType
    {
        public static string Idling = "Idling";
        public static string Threatened = "Threatened";
        public static string Crashing = "Crashing";
    };

    public enum HeatType
    {
		DataConstant,
		DataPercentOfMax,
		DataConstantRate
    }

	public enum AIWeightType
    {
		DataConstant,   //weight comes from a fixed constant
        Equipment       //weight comes from an equipment stat
    }
    
    public enum AIActionType
    {
        None,			//Used to block out plans - do not turn plans with this type into actions!
        AttackMain,
        AttackSecondary,
        ReactiveAttackMain,
        ReactiveAttackSecondary,
        Eject,
        Move,
        Wait,
        VentHeat,
        Guard,
        Dash,
        ReactiveGuard
    }

    [Serializable]
    public class DataContainerAction : DataContainerWithText
    {
        [HideReferenceObjectPicker, DisableContextMenu]
        [HideLabel, BoxGroup ("A", false)]
        [ShowIf ("IsUIVisible")]
        public DataBlockActionUI dataUI = new DataBlockActionUI ();
        
        [HideReferenceObjectPicker, DisableContextMenu]
        [HideLabel, BoxGroup ("B", false)]
        [ShowIf ("IsCoreVisible")]
        public DataBlockActionCore dataCore = new DataBlockActionCore ();

        [DropdownReference (true)]
        public DataContainerUnitFactionCheck dataFactionCheck;

        [DropdownReference (true)]
        public DataBlockActionMovement dataMovement;
        
        [DropdownReference (true)]
        public DataBlockActionEquipment dataEquipment;

        [DropdownReference (true)]
        public DataBlockActionAI dataAI;

        [DropdownReference (true)]
        public DataBlockActionVisualsOnStart dataVisualsOnStart;
        
        [DropdownReference (true)]
        public DataBlockActionCustom dataCustom;

        [DropdownReference]
        public List<DataBlockActionFunctionTimed> functionsTimed;



        public bool IsFlagPresent (string key)
        {
            var found = dataCustom != null && dataCustom.IsFlagPresent (key);
            return found;
        }
        
        public bool TryGetInt (string key, out int result, int fallback = default)
        {
            result = fallback;
            var found = dataCustom != null && dataCustom.TryGetInt (key, out result);
            if (!found)
                result = fallback;
            return found;
        }
        
        public bool TryGetFloat (string key, out float result, float fallback = default)
        {
            result = fallback;
            var found = dataCustom != null && dataCustom.TryGetFloat (key, out result);
            if (!found)
                result = fallback;
            return found;
        }
        
        public bool TryGetVector3 (string key, out Vector3 result, Vector3 fallback = default)
        {
            result = fallback;
            var found = dataCustom != null && dataCustom.TryGetVector3 (key, out result);
            if (!found)
                result = fallback;
            return found;
        }
        
        public bool TryGetString (string key, out string result, string fallback = default)
        {
            result = fallback;
            var found = dataCustom != null && dataCustom.TryGetString (key, out result);
            if (!found)
                result = fallback;
            return found;
        }
        
        
        
        
        public override void ResolveText ()
        {
            if (dataUI == null)
                return;
        
            dataUI.textName = DataManagerText.GetText (TextLibs.combatActions, $"{key}_name");
            dataUI.textDesc = DataManagerText.GetText (TextLibs.combatActions, $"{key}_text");

            if (dataUI.modes != null)
            {
                foreach (var kvp in dataUI.modes)
                {
                    var modeKey = kvp.Key.ToString ().ToLowerInvariant ();
                    kvp.Value.textHeader = DataManagerText.GetText (TextLibs.combatActions, $"{key}_mode_{modeKey}_header");
                    kvp.Value.textTooltip = DataManagerText.GetText (TextLibs.combatActions, $"{key}_mode_{modeKey}_tooltip");
                }
            }
        }

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
            
            if (dataCore.check != null)
                dataCore.check.key = key;
            
            if (dataCustom != null)
                dataCustom.ValidateFlags ();
        }

        #if UNITY_EDITOR

        public override void SaveText ()
        {
            if (!IsTextSavingPossible () || dataUI == null)
                return;

            if (!string.IsNullOrEmpty (dataUI.textName))
                DataManagerText.TryAddingTextToLibrary (TextLibs.combatActions, $"{key}_name", dataUI.textName);
            
            if (!string.IsNullOrEmpty (dataUI.textDesc))
                DataManagerText.TryAddingTextToLibrary (TextLibs.combatActions, $"{key}_text", dataUI.textDesc);
            
            if (dataUI.modes != null)
            {
                foreach (var kvp in dataUI.modes)
                {
                    var modeKey = kvp.Key.ToString ().ToLowerInvariant ();
                    DataManagerText.TryAddingTextToLibrary (TextLibs.combatActions, $"{key}_mode_{modeKey}_header", kvp.Value.textHeader);
                    DataManagerText.TryAddingTextToLibrary (TextLibs.combatActions, $"{key}_mode_{modeKey}_tooltip", kvp.Value.textTooltip);
                }
            }
        }
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerAction () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        private bool IsUIVisible () => DataMultiLinkerAction.showUI;
        private bool IsCoreVisible () => DataMultiLinkerAction.showCore;
        private bool IsOtherVisible () => DataMultiLinkerAction.showOther;

        #endif
    }
}

