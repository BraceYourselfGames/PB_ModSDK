using System;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatAudioSync : ICombatFunction
    {
        public string audioSyncKey = string.Empty;
        public float value = 0f;

        public void Run ()
        {
            #if !PB_MODSDK
            
            if (string.IsNullOrEmpty (audioSyncKey))
                return;
            
            AudioUtility.CreateAudioSyncUpdate (audioSyncKey, value);
            
            #endif
        }
    }
}