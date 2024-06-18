using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CompleteAction : IOverworldEventFunction, IOverworldActionFunction, IOverworldFunctionLog
    {
        [ValueDropdown ("@DataMultiLinkerOverworldAction.data.Keys")]
        public string actionKey;

        public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
        {
            Run ();
        }

        public void Run (OverworldActionEntity source)
        {
            Run ();
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
                    Debug.Log ($"Completing action {actionCandidate.ToLog ()} with key {key} from event-driven function");
                    actionCandidate.isCompleted = true;
                }
            }
            
            #endif
        }
        
        public string ToLog ()
        {
            if (actionKey == null)
                return "null";
            
            return actionKey;
        }
    }
}