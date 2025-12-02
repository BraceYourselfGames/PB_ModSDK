using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldAudioEvent : IOverworldFunction
    {
        public string eventKey = null;

        public void Run ()
        {
            #if !PB_MODSDK
            
            if (string.IsNullOrEmpty (eventKey))
                return;
            
            AudioUtility.CreateAudioEvent (eventKey);
            
            #endif
        }
    }
}