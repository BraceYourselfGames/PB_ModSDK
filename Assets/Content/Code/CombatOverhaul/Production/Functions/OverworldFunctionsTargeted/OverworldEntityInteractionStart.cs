using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldEntityInteractionStart : IOverworldTargetedFunction
    {
        [ValueDropdown ("@DataMultiLinkerOverworldInteraction.GetKeys ()")]
        public string key;
        
        public bool validated = false;
        
        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            var interaction = DataMultiLinkerOverworldInteraction.GetEntry (key);
            if (interaction == null)
                return;
            
            OverworldInteractionUtility.StartInteraction (interaction, entityOverworld, validated);

            #endif
        }
    }
    
    [Serializable]
    public class OverworldInteractionStartFilterTargeted : OverworldInteractionStartFilter
    {
        [PropertyOrder (-2)]
        public string targetNameInternal;
        
        public override void Run ()
        {
            #if !PB_MODSDK

            var targetOverworld = IDUtility.GetOverworldEntity (targetNameInternal);
            if (targetOverworld == null)
            {
                Debug.LogWarning ($"Failed to find forced interaction target with internal name {targetNameInternal}");
                return;
            }
            
            var keysFiltered = GetFilteredKeys (DataMultiLinkerOverworldInteraction.data);
            OverworldInteractionUtility.StartInteractionFromKeys (targetOverworld, keysFiltered, overrideOngoing);

            #endif
        }
    }
    
    public class DataBlockOverworldInteractionFilter : DataBlockFilterLinked<DataContainerOverworldInteraction>
    {
        public override IEnumerable<string> GetTags () => DataMultiLinkerOverworldInteraction.GetTags ();
        public override SortedDictionary<string, DataContainerOverworldInteraction> GetData () => DataMultiLinkerOverworldInteraction.data;
    }
    
    [Serializable]
    public class OverworldInteractionStartFilter : DataBlockOverworldInteractionFilter, IOverworldFunction
    {
        [PropertyOrder (-1)]
        public bool overrideOngoing = true;

        public virtual void Run ()
        {
            #if !PB_MODSDK

            var baseOverworld = IDUtility.playerBaseOverworld;
            var keysFiltered = GetFilteredKeys (DataMultiLinkerOverworldInteraction.data);
            OverworldInteractionUtility.StartInteractionFromKeys (baseOverworld, keysFiltered, overrideOngoing);

            #endif
        }
    }
    
    [Serializable]
    public class OverworldInteractionStartFilterFallback : OverworldInteractionStartFilter
    {
        [PropertyOrder (2)]
        public List<IOverworldFunction> functionsFallback = new List<IOverworldFunction> ();

        public override void Run ()
        {
            #if !PB_MODSDK

            var baseOverworld = IDUtility.playerBaseOverworld;
            var keysFiltered = GetFilteredKeys (DataMultiLinkerOverworldInteraction.data);
            bool started = OverworldInteractionUtility.StartInteractionFromKeys (baseOverworld, keysFiltered, overrideOngoing);

            if (!started && functionsFallback != null)
            {
                foreach (var function in functionsFallback)
                {
                    if (function != null)
                        function.Run ();
                }
            }

            #endif
        }
    }
}