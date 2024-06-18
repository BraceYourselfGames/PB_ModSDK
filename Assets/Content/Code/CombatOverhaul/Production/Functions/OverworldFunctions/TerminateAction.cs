using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class TerminateAction : IOverworldEventFunction
    {
        [ValueDropdown ("@DataMultiLinkerOverworldAction.data.Keys")]
        public string actionKey;

        public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
        {
            #if !PB_MODSDK

            Run ();
            
            #endif
        }

        public void Run (OverworldActionEntity source)
        {
            #if !PB_MODSDK

            Run ();
            
            #endif
        }

        private void Run ()
        {
            #if !PB_MODSDK

            if (string.IsNullOrEmpty (actionKey))
                return;
            
            var baseOverworld = IDUtility.playerBaseOverworld;
            var overworldAction = Contexts.sharedInstance.overworldAction;
            var actions = overworldAction.GetEntitiesWithOverworldOwner (baseOverworld.id.id);

            foreach (var actionCandidate in actions)
            {
                if (actionCandidate.isCompleted || actionCandidate.isCancelled || actionCandidate.isDestroyed)
                    continue;

                var key = actionCandidate.dataKeyOverworldAction.s;
                if (key == actionKey)
                {
                    Debug.Log ($"Terminating action {actionCandidate.ToLog ()} with key {key} from event-driven function");
                    actionCandidate.isDestroyed = true;
                }
            }
            
            #endif
        }
    }
}