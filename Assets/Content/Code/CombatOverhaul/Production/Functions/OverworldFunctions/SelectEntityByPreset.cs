using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    public class OverworldFunctionWithDelay
    {
        [PropertyRange (0f, 60f)]
        public float delay = 0f;

        public void Run ()
        {
            #if !PB_MODSDK
            
            var delaySafe = Mathf.Clamp (delay, 0f, 60f);
            if (delaySafe <= 0f)
                RunDelayed ();
            else
            {
                // Debug.Log ($"Starting countdown to function: {delaySafe:0.##}");
                Co.Delay (delaySafe, RunDelayed);
            }
            
            #endif
        }

        protected virtual bool IsRunningPossible ()
        {
            #if !PB_MODSDK
            
            if (!IDUtility.IsGameLoaded () || !IDUtility.IsGameState (GameStates.overworld))
            {
                Debug.LogWarning ($"Can't run a delayed function due to wrong context");
                return false;
            }
            
            #endif
            
            return true;
        }

        private void RunDelayed ()
        {
            // Since invocation of this method might have been delayed, we need to run some checks and bail if they fail
            if (!IsRunningPossible ())
                return;
            
            RunPayload ();
        }

        protected virtual void RunPayload ()
        {
            
        }
    }
    
    [Serializable]
    public class SelectEntityByPreset : OverworldFunctionWithDelay, IOverworldFunction
    {
        [ValueDropdown("@DataMultiLinkerOverworldPointPreset.GetKeys ()")] 
        public string key;
        
        private static List<int> entityCandidateIDs = new List<int> ();

        protected override void RunPayload ()
        {
            #if !PB_MODSDK

            var preset = DataMultiLinkerOverworldPointPreset.GetEntry (key);
            if (preset == null)
                return;

            var overworld = Contexts.sharedInstance.overworld;
            var entities = overworld.GetEntitiesWithDataKeyPointPreset (key);
            entityCandidateIDs.Clear ();
			
            foreach (var entityCandidate in entities)
            {
                if (entityCandidate.isDestroyed || entityCandidate.isHidden || !entityCandidate.hasPosition)
                    continue;

                entityCandidateIDs.Add (entityCandidate.id.id);
            }

            if (entityCandidateIDs.Count == 0)
            {
                Debug.Log ($"Can't find any overworld entities using point preset {key}. Preset exists, but nothing is currently placed on the map using it.");
                return;
            }

            int siteOverworldID = entityCandidateIDs.GetRandomEntry ();
            var siteOverworld = IDUtility.GetOverworldEntity (siteOverworldID);
            if (siteOverworld == null || !siteOverworld.hasPosition || siteOverworld.isDestroyed || !siteOverworld.hasId)
            {
                Debug.Log ($"Selected site couldn't be used for selection");
                return;
            }

            overworld.ReplaceSelectedEntity (siteOverworldID);
            GameCameraSystem.SetTargetOverworldEntity (siteOverworldID);
            
            #endif
        }
    }
}