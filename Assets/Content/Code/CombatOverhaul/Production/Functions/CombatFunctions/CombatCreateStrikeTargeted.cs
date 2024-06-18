using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatCreateStrikeTargeted : ICombatFunction
    {
        [BoxGroup ("Filter", false)]
        public DataBlockScenarioSubcheckUnitFilter unitFilter = new DataBlockScenarioSubcheckUnitFilter ();

        [ValueDropdown ("@DataMultiLinkerCombatStrike.data.Keys")]
        public List<string> blueprintKeys = new List<string> ();

        public Vector2 delayRange;
        
        public List<ITargetModifierFunction> modifiers;

        public void Run ()
        {
            #if !PB_MODSDK
            
            if (blueprintKeys == null || blueprintKeys.Count == 0)
                return;

            var combat = Contexts.sharedInstance.combat;
            if (unitFilter == null)
                return;
            
            var unitTargets = unitFilter.GetFilteredUnitsUsingSettings ();
            if (unitTargets == null || unitTargets.Count == 0)
                return;

            Debug.Log ($"Received {unitTargets.Count} targets for strikes | Limit: {unitFilter.unitLimit} | Repeats: {unitFilter.unitRepeats}");
            foreach (var unitTargetCombat in unitTargets)
            {
                var blueprintKey = blueprintKeys.GetRandomEntry ();
                var blueprint = DataMultiLinkerCombatStrike.GetEntry (blueprintKey);
                if (blueprint == null)
                    continue;
                
                var delay = Random.Range (delayRange.x, delayRange.y);
                var delaySafe = Mathf.Clamp (delay, 0f, 5f);
                var startTime = combat.simulationTime.f + delaySafe;
                
                var unitTargetCombatID = unitTargetCombat.id.id;
                ScenarioUtility.GetTargetFromUnit (unitTargetCombat, modifiers, out var targetPosition, out var targetDirection);
                
                Debug.Log ($"Creating strike {blueprint.key} targeted at unit {unitTargetCombat.ToLog ()} at {targetPosition}");
                CombatStrikeHelper.AddStrike (blueprint, targetPosition, targetDirection, startTime, targetUnitCombatID: unitTargetCombatID);
            }
            
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