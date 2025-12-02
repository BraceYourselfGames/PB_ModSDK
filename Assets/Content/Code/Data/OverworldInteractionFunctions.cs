using System;
using System.Collections.Generic;
using Content.Code.Utility;
using PhantomBrigade.Functions;
using PhantomBrigade.Overworld.Components;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [TypeHinted]
    public interface IInteractionOptionFunction
    {
        public void Run ();
    }

    public class InteractionOptionExit : IInteractionOptionFunction
    {
        public void Run ()
        {
            #if !PB_MODSDK

            OverworldInteractionUtility.TryExitFromCurrentInteraction ();

            #endif
        }
    }
    
    public class InteractionOptionEnterStep : IInteractionOptionFunction
    {
        [LabelText (" → ")]
        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerOverworldInteraction\", \"GetStepKeys\")")]
        public string step;

        public void Run ()
        {
            #if !PB_MODSDK

            var overworld = Contexts.sharedInstance.overworld;
            if (!overworld.hasInteractionState)
                return;

            var state = overworld.interactionState;
            overworld.ReplaceInteractionState (state.targetOverworldID, state.key, step);

            #endif
        }
    }
    
    public class InteractionOptionEnterStepRandom : IInteractionOptionFunction
    {
        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerOverworldInteraction\", \"GetStepKeys\")")]
        public List<string> steps = new List<string> ();
        
        public void Run ()
        {
            #if !PB_MODSDK

            if (steps == null || steps.Count == 0)
                return;
            
            var overworld = Contexts.sharedInstance.overworld;
            if (!overworld.hasInteractionState)
                return;
            
            var state = overworld.interactionState;
            var interactionKey = state.key;
            var stepKeyCurrent = state.step;
            
            if (string.IsNullOrEmpty (interactionKey) || string.IsNullOrEmpty (stepKeyCurrent))
                return;

            var randomStepCache = OverworldInteractionUtility.randomStepCache;
            var cacheKey = $"{interactionKey}_{stepKeyCurrent}";
            var stepKeysAvailableFromCache = randomStepCache.TryGetValue (cacheKey, out var v) ? v : null;
            if (stepKeysAvailableFromCache == null)
            {
                Debug.Log ($"EnterStepRandom | First time a random transition occurs from interaction {interactionKey} step {stepKeyCurrent}");
                stepKeysAvailableFromCache = new HashSet<string> ();
                foreach (var sk in steps)
                    stepKeysAvailableFromCache.Add (sk);
                randomStepCache[cacheKey] = stepKeysAvailableFromCache;
            }

            string stepKeySelected = null;
            if (stepKeysAvailableFromCache.Count == 0 || stepKeysAvailableFromCache.Count >= steps.Count)
            {
                stepKeySelected = steps.GetRandomEntry ();
                if (stepKeysAvailableFromCache.Count > 0 && stepKeysAvailableFromCache.Contains (stepKeySelected))
                    stepKeysAvailableFromCache.Remove (stepKeySelected);
                Debug.Log ($"EnterStepRandom | Moving to step {stepKeySelected} randomly chosen from {steps.Count} keys: {steps.ToStringFormatted ()}");
            }
            else
            {
                stepKeySelected = stepKeysAvailableFromCache.GetRandomEntry ();
                Debug.Log ($"EnterStepRandom | Moving to step {stepKeySelected} randomly chosen from {stepKeysAvailableFromCache.Count} filtered keys: {stepKeysAvailableFromCache.ToStringFormatted ()} | Full set: {steps.ToStringFormatted ()}");
                stepKeysAvailableFromCache.Remove (stepKeySelected);
            }

            if (string.IsNullOrEmpty (stepKeySelected))
            {
                Debug.Log ($"EnterStepRandom | Failed to select any target step");
                return;
            }

            overworld.ReplaceInteractionState (state.targetOverworldID, interactionKey, stepKeySelected);

            #endif
        }
    }
    
    public class InteractionOptionEnterRecruitment : IInteractionOptionFunction
    {
        public void Run ()
        {
            #if !PB_MODSDK

            CIViewOverworldPilotRecruitment.ins.TryEntry ();

            #endif
        }
    }
    
    public class InteractionOptionEnterTravel : IInteractionOptionFunction
    {
        public void Run ()
        {
            #if !PB_MODSDK

            if (!CIViewOverworldCampaign.ins.IsEntered ())
                CIViewOverworldCampaign.ins.TryEntry ();

            CIViewOverworldCampaign.ins.TryEntry ();

            #endif
        }
    }
}

