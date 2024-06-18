using System;
using PhantomBrigade.Game;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatMusicLinearOverride : ICombatFunction
    {
        public string value;
        
        public void Run ()
        {
            #if !PB_MODSDK
            
            var combat = Contexts.sharedInstance.combat;
            if (combat.hasMusicReactiveStatus)
            {
                Debug.LogWarning ($"Removing reactive music status component from combat to allow forcing linear music state");
                combat.RemoveMusicReactiveStatus ();
            }
            
            AudioUtility.CreateAudioStateEvent(AudioStateMusic.CombatMusicLinearState, value);
            
            #endif
        }
    }
}