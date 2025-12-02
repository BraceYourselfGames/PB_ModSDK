using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldAudioSync : IOverworldFunction
    {
        public string syncKey = null;
        public float value = 0f;

        public void Run ()
        {
            #if !PB_MODSDK
            
            if (string.IsNullOrEmpty (syncKey))
                return;
            
            AudioUtility.CreateAudioSyncUpdate (syncKey, value);
            
            #endif
        }
    }
}