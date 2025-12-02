using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldValidateWeather : IOverworldEntityValidationFunction
    {
        [HideReferenceObjectPicker, HideLabel]
        public DataBlockOverworldEventSubcheckFloatRange check = new DataBlockOverworldEventSubcheckFloatRange ();
        
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK
            
            if (entityPersistent == null)
                return false;

            if (check == null)
                return false;
            
            var overworld = Contexts.sharedInstance.overworld;
            var weatherColor = overworld.hasPlayerWeather ? overworld.playerWeather.c : Color.black;
            bool weatherValid = check.IsPassed (weatherColor.b);
            return weatherValid;
            
            #else
            return false;
            #endif
        }
    }
}