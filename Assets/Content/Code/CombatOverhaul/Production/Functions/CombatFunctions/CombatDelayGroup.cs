using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    public class CombatToOverworldGroup : ICombatFunction
    {
        public List<IOverworldFunction> functions = new List<IOverworldFunction> ();
        
        public void Run ()
        {
            #if !PB_MODSDK

            if (functions != null)
            {
                foreach (var function in functions)
                {
                    if (function != null)
                        function.Run ();
                }
            }
            
            #endif
        }

    }
    
    [PropertyTooltip ("Allows delaying a group of functions")]
    public class CombatDelayGroup : ICombatFunction
    {
        public bool unscaled;
        public float delayMin;
        public float delayMax;
        public int repeats = 1;
        public List<ICombatFunction> functions;

        public void Run ()
        {
            #if !PB_MODSDK

            int repeatsFinal = Mathf.Clamp (repeats, 1, 10);
            for (int i = 0; i < repeatsFinal; ++i)
            {
                var delay = Random.Range (delayMin, delayMax);
                delay = Mathf.Clamp (delay, 0f, 60f);

                if (delay <= 0f)
                    RunDelayed ();
                else if (unscaled)
                    Co.Delay (delay, RunDelayed);
                else
                    Co.DelayScaled (delay, RunDelayed);
            }
            
            #endif
        }
        
        private void RunDelayed ()
        {
            #if !PB_MODSDK
            
            if (!IDUtility.IsGameLoaded () || !IDUtility.IsGameState (GameStates.combat))
                return;
            
            /*
            var game = Contexts.sharedInstance.game;
            var combat = Contexts.sharedInstance.combat;
            if (game.isCutsceneInProgress || combat.isScenarioIntroInProgress)
                return;
            */
            
            if (functions != null)
            {
                foreach (var function in functions)
                {
                    if (function != null)
                        function.Run ();
                }
            }
            
            #endif
        }
    }
}