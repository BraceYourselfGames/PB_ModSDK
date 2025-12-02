using System;
using System.Collections.Generic;
using PhantomBrigade.Combat;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    public class PilotStatSet : IPilotTargetedFunction, IFunctionLocalizedText
    {
        [InfoBox ("@GetLocalizedText ()", InfoMessageType.None)]
        [ValueDropdown ("@DataMultiLinkerPilotStat.data.Keys")]
        public string key;
        public bool normalized;
        public float value;
        
        public void Run (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK
            
            if (pilot != null)
                pilot.SetPilotStat (key, value, normalized);
            
            #endif
        }
        
        public string GetLocalizedText ()
        {
            #if !PB_MODSDK

            var statInfo = DataMultiLinkerPilotStat.GetEntry (key, false);
            if (statInfo == null)
                return string.Empty;

            var format = !string.IsNullOrEmpty (statInfo.displayFormat) ? statInfo.displayFormat : "0.##";
            var text = normalized ? TextUtility.GetPercentageFromNormalizedValue (value) : value.ToString (format);
            return $"[cc]{statInfo.textName}:[ff] {text}";

            #else
            return null;
            #endif
        }
    }
    
    public class PilotStatClamp : IPilotTargetedFunction
    {
        [ValueDropdown ("@DataMultiLinkerPilotStat.data.Keys")]
        public string key;
        public float min;
        public float max;
        
        public void Run (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK
            
            if (pilot != null)
            {
                var valueCurrent = pilot.GetPilotStat (key);
                var valueClamped = Mathf.Clamp (valueCurrent, min, Mathf.Max (min, max));
                pilot.SetPilotStat (key, valueClamped);
            }
            
            #endif
        }
    }
}