using System;
using System.Runtime.InteropServices;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    public class DataBlockDestructionRadius
    {
        public float radius = 0.0f;
        public float exponent = 2.0f;
    }

    [Serializable]
    public class CombatCreateDamage : ICombatFunction
    {
        [PropertyRange (0f, 10f)]
        public float delay = 0f;
        public Vector3 target;
        public int damage;

        [PropertyTooltip ("Force cells to be destructible that were set indestructible by designer")]
        public bool forceDestructible = false;

        [PropertyTooltip ("Optional radius to destroy multiple cells, set above zero to enable")]
        public DataBlockDestructionRadius radius;
        
        [Button, HideInEditorMode]
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

            if (radius != null && radius.radius > 0.0f)
            {
                CombatSceneHelper.ins.areaManager.ApplyDamageToRadius (target, damage, radius.radius, radius.exponent, forceDestructible);
            }
            else
            {
                CombatSceneHelper.ins.areaManager.ApplyDamageToPosition (target, damage, false, forceDestructible);
            }
            
            #endif
        }
    }
}