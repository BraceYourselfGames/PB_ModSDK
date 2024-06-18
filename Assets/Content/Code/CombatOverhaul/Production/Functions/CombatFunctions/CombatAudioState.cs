using System;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatAudioState : ICombatFunction
    {
        public string audioStateKey = string.Empty;
        public string value = string.Empty;

        public void Run ()
        {
            #if !PB_MODSDK
            
            if (string.IsNullOrEmpty (audioStateKey) || string.IsNullOrEmpty (value))
                return;
            
            AudioUtility.CreateAudioStateEvent (audioStateKey, value);
            
            #endif
        }
    }
}