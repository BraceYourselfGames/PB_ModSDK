using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatCreateDamage : ICombatFunction
    {
        [PropertyRange (0f, 10f)]
        public float delay = 0f;
        public Vector3 target;
        public int damage;

        public void Run ()
        {
            #if !PB_MODSDK
            
            var delaySafe = Mathf.Clamp (delay, 0f, 60f);
            if (delaySafe <= 0f)
                RunDelayed ();
            else
                Co.DelayScaled (delaySafe, RunDelayed);
            
            #endif
        }
        
        private void RunDelayed ()
        {
            #if !PB_MODSDK
            
            // Since this might have been delayed, we need some safety checks
            if (!IDUtility.IsGameLoaded () || !IDUtility.IsGameState (GameStates.combat))
                return;
            
            var game = Contexts.sharedInstance.game;
            if (game.isCutsceneInProgress)
                return;
            
            var combat = Contexts.sharedInstance.combat;
            if (combat.isScenarioIntroInProgress)
                return;
            
            CombatSceneHelper.ins.areaManager.ApplyDamageToPosition (target, damage);
            
            #endif
        }
    }
}