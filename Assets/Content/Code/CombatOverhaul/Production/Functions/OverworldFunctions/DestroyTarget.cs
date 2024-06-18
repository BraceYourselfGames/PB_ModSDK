using System;
using PhantomBrigade.Data;
using PhantomBrigade.Linking;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class DestroyTarget : IOverworldEventFunction, IOverworldActionFunction 
    {
        public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
        {
            #if !PB_MODSDK

            Run (target);
            
            #endif
        }
        
        public void Run (OverworldActionEntity source)
        {
            #if !PB_MODSDK

            var target = IDUtility.GetOverworldActionOverworldTarget (source);
            Run (target);
            
            #endif
        }
        
        private void Run (OverworldEntity target)
        {
            #if !PB_MODSDK

            if (target == null)
            {
                Debug.LogWarning ($"DestroyTarget | Event function failed due to missing target: is null)");
                return;
            }
            
            var targetPersistent = IDUtility.GetLinkedPersistentEntity (target);
            if (targetPersistent == null || targetPersistent == IDUtility.playerBasePersistent)
            {
                Debug.LogWarning ($"DestroyTarget | Event function failed due to missing persistent counterpart or due to match with player base");
                return;
            }
            
            target.isDestroyed = true;
            targetPersistent.isDestroyed = true;

            // Destruction link seems to be firing one frame late when destruction flag is set in this system and I have absolutely no idea why
            if (target.hasOverworldView)
            {
                Debug.LogWarning ($"Manually triggering on-destruction link of overworld parent entity {target.ToLog ()} as proper link system never triggers for some bizarre reason");
                IOnDestroyLink destroyLink = target.overworldView.view;
                destroyLink?.OnDestroyLink ();
            }
            
            #endif
        }
    }
}