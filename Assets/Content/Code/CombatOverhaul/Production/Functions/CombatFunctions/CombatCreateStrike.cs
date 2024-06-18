using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatCreateStrike : ICombatFunction
    {
        [PropertyRange (0f, 5f)]
        public float delay = 0f;

        [ValueDropdown ("@DataMultiLinkerCombatStrike.data.Keys")]
        public string blueprintKey;

        public TargetFromSource target;

        public void Run ()
        {
            #if !PB_MODSDK
            
            var blueprint = DataMultiLinkerCombatStrike.GetEntry (blueprintKey);
            if (blueprint == null)
                return;
            
            var combat = Contexts.sharedInstance.combat;
            var delaySafe = Mathf.Clamp (delay, 0f, 5f);
            var startTime = combat.simulationTime.f + delaySafe;

            var position = Vector3.zero;
            var direction = Vector3.forward;
            
            bool targetFound = ScenarioUtility.GetTarget
            (
                null, 
                target, 
                out var targetPosition, 
                out var targetDirection, 
                out var targetUnitCombat
            );

            if (targetFound)
            {
                position = targetPosition;
                direction = targetDirection.FlattenAndNormalize ();
            }

            CombatStrikeHelper.AddStrike (blueprint, position, direction, startTime);
            
            #endif
        }

        [Button, PropertyOrder (-1), HideInEditorMode]
        private void Test ()
        {
            #if !PB_MODSDK
            
            if (!IDUtility.IsGameState (GameStates.combat))
                return;
            
            Run ();
            
            #endif
        }
    }
}