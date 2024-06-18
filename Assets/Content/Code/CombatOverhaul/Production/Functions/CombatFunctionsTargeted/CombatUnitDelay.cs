using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [PropertyTooltip ("Allows delaying a function")]
    public class CombatUnitDelay : ICombatFunctionTargeted
    {
        public bool unscaled;
        public float delayMin;
        public float delayMax;
        public int repeats = 1;
        
        [HideLabel, BoxGroup]
        public ICombatFunctionTargeted function;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            if (!IDUtility.IsGameState (GameStates.combat))
                return;

            var unitPersistentID = unitPersistent.id.id;
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            bool coroutinePossible = unitCombat != null && unitCombat.hasCombatView && unitCombat.combatView.view != null;
            
            int repeatsFinal = Mathf.Clamp (repeats, 1, 10);
            for (int i = 0; i < repeatsFinal; ++i)
            {
                var delay = UnityEngine.Random.Range (delayMin, delayMax);
                delay = Mathf.Clamp (delay, 0f, 60f);

                if (delay <= 0f)
                    RunDelayed (unitPersistentID);
                else if (coroutinePossible)
                {
                    var view = unitCombat.combatView.view;
                    view.StartCoroutine (RunDelayedIE (unitPersistentID, delay, unscaled));
                }
            }
            
            #endif
        }

        private IEnumerator RunDelayedIE (int unitPersistentID, float delay, bool unscaled)
        {
            yield return null;
            
            #if !PB_MODSDK

            if (delay > 0f)
            {
                if (unscaled)
                {
                    var wait = new WaitForSecondsRealtime (delay);
                    yield return wait;
                }
                else
                {
                    var wait = new WaitForSeconds (delay);
                    yield return wait;
                }
            }

            RunDelayed (unitPersistentID);

            #endif
        }

        private void RunDelayed (int unitPersistentID)
        {
            #if !PB_MODSDK
            
            if (!IDUtility.IsGameLoaded () || !IDUtility.IsGameState (GameStates.combat))
                return;
            
            var game = Contexts.sharedInstance.game;
            var combat = Contexts.sharedInstance.combat;
            if (game.isCutsceneInProgress || combat.isScenarioIntroInProgress)
                return;

            var unitPersistent = IDUtility.GetPersistentEntity (unitPersistentID);
            if (unitPersistent == null)
                return;
            
            if (function != null)
                function.Run (unitPersistent);
            
            #endif
        }
    }
}