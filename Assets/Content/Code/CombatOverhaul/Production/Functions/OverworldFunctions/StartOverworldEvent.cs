using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class StartOverworldEvent : IOverworldEventFunction, IOverworldActionFunction, IOverworldFunction, IOverworldFunctionLog
    {
        [ValueDropdown ("@DataMultiLinkerOverworldEvent.data.Keys")]
        public List<string> eventKeys = new List<string> ();

        public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
        {
            #if !PB_MODSDK
            
            Run (target, $"event {eventData.key}");
            
            #endif
        }
        
        public void Run (OverworldActionEntity source)
        {
            #if !PB_MODSDK
            
            var blueprintKey = source.dataKeyOverworldAction.s;
            var target = IDUtility.GetOverworldActionOverworldTarget (source);
            Run (target, $"action {blueprintKey}");
            
            #endif
        }

        public void Run ()
        {
            #if !PB_MODSDK
            
            Run (null, $"standalone function");
            
            #endif
        }

        private void Run (OverworldEntity target, string context)
        {
            #if !PB_MODSDK
            
            var overworld = Contexts.sharedInstance.overworld;
            if (overworld.hasEventInProgress)
            {
                Debug.LogWarning ($"Failed to start overworld event from {context} due to in progress event {overworld.eventInProgress.key}");
                return;
            }
        
            var ownerOverworld = IDUtility.playerBaseOverworld;
            if (ownerOverworld == null)
            {
                Debug.LogWarning ($"Failed to start overworld event from {context} due to missing owner");
                return;
            }

            if (eventKeys == null || eventKeys.Count == 0)
            {
                Debug.LogWarning ($"Failed to start overworld event from {context} due to missing event keys argument");
                return;
            }

            var eventKeysPassed = new List<string> ();
            
            foreach (var eventKey in eventKeys)
            {
                bool eventPossible = OverworldEventUtility.IsEventEnterable (eventKey, target, false);
                if (!eventPossible)
                    continue;
                
                eventKeysPassed.Add (eventKey);
            }

            if (eventKeysPassed.Count == 0)
            {
                Debug.LogWarning ($"Failed to start overworld event from {context}, not a single event from list of {eventKeys.Count} passed the check");
                return;
            }

            int eventKeyIndex = UnityEngine.Random.Range (0, eventKeysPassed.Count);
            var eventKeySelected = eventKeysPassed[eventKeyIndex];
            OverworldEventUtility.TryEnterEvent (eventKeySelected, target);
            
            #endif
        }
        
        public string ToLog ()
        {
            if (eventKeys == null || eventKeys.Count == 0)
                return "empty";

            return eventKeys.ToStringFormatted ();
        }
    }
}