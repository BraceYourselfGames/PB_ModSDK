using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class SelectEntity : IOverworldFunction
    {
        public string nameInternal;
        
        public float delay = 0f;
        
        [HideIf ("@string.IsNullOrEmpty (nameInternal)")]
        public bool select = true;
        
        [HideIf ("@string.IsNullOrEmpty (nameInternal)")]
        public bool focus = true;
        
        public void Run ()
        {
            #if !PB_MODSDK
            
            var delaySafe = Mathf.Clamp (delay, 0f, 60f);
            if (delaySafe <= 0f)
                RunDelayed ();
            else
                Co.Delay (delaySafe, RunDelayed);
            
            #endif
        }
        
        private void RunDelayed ()
        {
            #if !PB_MODSDK
            
            // Since this might have been delayed, we need some safety checks
            if (!IDUtility.IsGameLoaded () || !IDUtility.IsGameState (GameStates.overworld))
                return;
            
            var sitePersistent = IDUtility.GetPersistentEntity (nameInternal);
            var siteOverworld = IDUtility.GetLinkedOverworldEntity (sitePersistent);
            if (siteOverworld == null)
            {
                if (Contexts.sharedInstance.combat.hasUnitSelected)
                {
                    Contexts.sharedInstance.combat.RemoveUnitSelected ();
                    GameCameraSystem.ClearTarget ();
                }
                
                return;
            }

            int siteOverworldID = siteOverworld.id.id;

            if (select)
                Contexts.sharedInstance.overworld.ReplaceSelectedEntity (siteOverworldID);
            
            if (focus)
                GameCameraSystem.SetTargetOverworldEntity (siteOverworldID);
            
            #endif
        }
    }
}