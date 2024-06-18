using System;
using PhantomBrigade.Linking;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class DestroySelf : IOverworldActionFunction 
    {
        public void Run (OverworldActionEntity source)
        {
            #if !PB_MODSDK

            var self = IDUtility.GetOverworldActionOwner (source);
            if (self == null)
            {
                Debug.LogError ($"DestroySelf | Event function failed due to missing action owner");
                return;
            }
            
            var targetPersistent = IDUtility.GetLinkedPersistentEntity (self);
            if (targetPersistent == null || targetPersistent == IDUtility.playerBasePersistent)
            {
                Debug.LogError ($"DestroySelf | Event function failed due to missing persistent counterpart or due to match with player base ({self.ToStringNullCheck ()})");
                return;
            }
            
            self.isDestroyed = true;
            targetPersistent.isDestroyed = true;

            // Destruction link seems to be firing one frame late when destruction flag is set in this system and I have absolutely no idea why
            if (self.hasOverworldView)
            {
                Debug.LogWarning ($"Manually triggering on-destruction link of overworld parent entity {self.ToLog ()} as proper link system never triggers for some bizarre reason");
                IOnDestroyLink destroyLink = self.overworldView.view;
                destroyLink?.OnDestroyLink ();
            }
            
            #endif
        }
    }
}