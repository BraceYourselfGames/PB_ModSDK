using System;
using System.Collections.Generic;
using PhantomBrigade.Combat;
using PhantomBrigade.Data;
using PhantomBrigade.Game;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    public class PilotTraitInject : IPilotTargetedFunction
    {
        [ValueDropdown ("@DataMultiLinkerPilotTrait.data.Keys")]
        public string traitKey;
        public string slotKey;
        
        public void Run (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK

            if (pilot == null)
                return;

            PilotUtility.TryInjectingTraitKey (pilot, traitKey, slotKey);

            #endif
        }
    }
    
    public class PilotLevelGain : IPilotTargetedFunction
    {
        public void Run (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK

            if (pilot == null)
                return;

            var xpCurrent = pilot.hasPilotExperience ? pilot.pilotExperience.current : 0;
            int levelCurrent = PilotUtility.GetLevelFromExperience (xpCurrent);
            int prestigeThreshold = PilotUtility.GetPrestigeThreshold (pilot);
            if (levelCurrent >= prestigeThreshold)
                return;
            
            int xpToNext = PilotUtility.GetExperienceForNextLevel (levelCurrent);
            PilotUtility.OffsetExperience (pilot, xpToNext);

            #endif
        }
    }
    
    public class PilotTypeReset : IPilotTargetedFunction
    {
        [ValueDropdown ("@DataMultiLinkerPilotType.data.Keys")]
        public string typeKey;
        
        [ValueDropdown ("@DataMultiLinkerPilotTrait.data.Keys")]
        public string traitKeyStarter;
        
        private static SortedDictionary<string, PilotTraitNode> nodesInjected = new SortedDictionary<string, PilotTraitNode> ();
        
        public void Run (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK

            PilotUtility.ResetPilotTypeDataWithStarter (pilot, typeKey, traitKeyStarter);

            #endif
        }
    }
    
    public class PilotStatOffset : IPilotTargetedFunction, IFunctionLocalizedText
    {
        [InfoBox ("@GetLocalizedText ()", InfoMessageType.None)]
        [ValueDropdown ("@DataMultiLinkerPilotStat.data.Keys")]
        [LabelText ("Stat")]
        public string key;
        public bool normalized;
        public float value;
        
        public void Run (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK
            
            if (pilot != null)
                pilot.OffsetPilotStat (key, value, normalized);
            
            #endif
        }
        
        public string GetLocalizedText ()
        {
            #if !PB_MODSDK

            var statInfo = DataMultiLinkerPilotStat.GetEntry (key, false);
            if (statInfo == null)
                return string.Empty;

            var format = !string.IsNullOrEmpty (statInfo.displayFormat) ? statInfo.displayFormat : "0.##";
            var text = normalized ? TextUtility.GetPercentageFromNormalizedOffset (value) : value > 0f ? "+" + value.ToString (format) : value.ToString (format);
            return $"[cc]{statInfo.textName}:[ff] {text}";

            #else
            return null;
            #endif
        }
    }
    
    public class PilotStatOffsetDifficulty : IPilotTargetedFunction, IFunctionLocalizedText
    {
        [InfoBox ("@GetLocalizedText ()", InfoMessageType.None)]
        [ValueDropdown ("@DataMultiLinkerDifficultySetting.data.Keys")]
        [LabelText ("Difficulty")]
        public string multiplier;
        
        [ValueDropdown ("@DataMultiLinkerPilotStat.data.Keys")]
        [LabelText ("Stat")]
        public string key;
        public bool normalized;
        public float value;
        
        public void Run (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK

            var valueFinal = value * DifficultyUtility.GetMultiplier (multiplier);
            if (pilot != null)
                pilot.OffsetPilotStat (key, valueFinal, normalized);
            
            #endif
        }
        
        public string GetLocalizedText ()
        {
            #if !PB_MODSDK

            var statInfo = DataMultiLinkerPilotStat.GetEntry (key, false);
            if (statInfo == null)
                return string.Empty;

            var format = !string.IsNullOrEmpty (statInfo.displayFormat) ? statInfo.displayFormat : "0.##";
            var valueFinal = value * DifficultyUtility.GetMultiplier (multiplier);
            var text = normalized ? TextUtility.GetPercentageFromNormalizedOffset (valueFinal) : valueFinal > 0f ? "+" + valueFinal.ToString (format) : valueFinal.ToString (format);
            return $"[cc]{statInfo.textName}:[ff] {text}";

            #else
            return null;
            #endif
        }
    }
    
    public class PilotStatOffsetClamped : IPilotTargetedFunction
    {
        [InfoBox ("@GetLocalizedText ()", InfoMessageType.None)]
        [ValueDropdown ("@DataMultiLinkerPilotStat.data.Keys")]
        [LabelText ("Stat")]
        public string key;
        
        public bool normalized;
        
        [LabelText ("Offset")]
        public float value;
        public float min;
        public float max;
        
        public bool avoidShiftIntoRange = true;
        
        public void Run (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK
            
            if (pilot != null)
            {
                if (normalized)
                {
                    var valueCurrentNorm = pilot.GetPilotStatNormalized (key);
                    if (avoidShiftIntoRange)
                    {
                        if (value > 0f && valueCurrentNorm > max)
                            return;
                    
                        if (value < 0f && valueCurrentNorm < min)
                            return;
                    }
                    
                    var valueClampedNorm = Mathf.Clamp (valueCurrentNorm + value, min, Mathf.Max (min, max));
                    pilot.SetPilotStat (key, valueClampedNorm, true);
                }
                else
                {
                    var valueCurrent = pilot.GetPilotStat (key);
                    if (avoidShiftIntoRange)
                    {
                        if (value > 0f && valueCurrent > max)
                            return;
                    
                        if (value < 0f && valueCurrent < min)
                            return;
                    }
                    
                    var valueClamped = Mathf.Clamp (valueCurrent + value, min, Mathf.Max (min, max));
                    pilot.SetPilotStat (key, valueClamped);
                }
            }
            
            #endif
        }
        
        public string GetLocalizedText ()
        {
            #if !PB_MODSDK

            var statInfo = DataMultiLinkerPilotStat.GetEntry (key, false);
            if (statInfo == null)
                return string.Empty;

            var format = !string.IsNullOrEmpty (statInfo.displayFormat) ? statInfo.displayFormat : "0.##";
            var text = normalized ? TextUtility.GetPercentageFromNormalizedOffset (value) : value > 0f ? "+" + value.ToString (format) : value.ToString (format);
            return $"[cc]{statInfo.textName}:[ff] {text}";

            #else
            return null;
            #endif
        }
    }
    
    public class PilotStatModifierReplace : IPilotTargetedFunction
    {
        [ValueDropdown ("@DataMultiLinkerPilotStat.data.Keys")]
        [LabelText ("Stat")]
        public string statKey;
        public string modifierKey;
        public float value;
        
        public void Run (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK
            
            if (pilot != null)
                PilotUtility.ReplaceStatModifier (pilot, statKey, modifierKey, value);
            
            #endif
        }
    }
    
    public class PilotStatModifierRemove : IPilotTargetedFunction
    {
        [ValueDropdown ("@DataMultiLinkerPilotStat.data.Keys")]
        [LabelText ("Stat")]
        public string statKey;
        public string modifierKey;
        
        public void Run (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK
            
            if (pilot != null)
                PilotUtility.RemoveStatModifier (pilot, statKey, modifierKey);
            
            #endif
        }
    }
    
    public class SetPilotProfileLink : IPilotTargetedFunction
    {
        [ValueDropdown ("@DataMultiLinkerPilotPersistentProfile.data.Keys")]
        [LabelText ("Profile")]
        public string profileKey;

        public void Run (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK

            if (pilot != null)
                PilotUtility.ReplacePilotProfileLink (pilot, profileKey);

            #endif
        }
    }
}