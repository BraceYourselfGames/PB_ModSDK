using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using PhantomBrigade.Overworld.Systems;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldEntityMemoryToQuest : OverworldFunctionQuest, IOverworldTargetedFunction
    {
        [ValueDropdown ("@DataMultiLinkerOverworldMemory.data.Keys")]
        public string memoryKey;
        public bool offset = false;
        public bool consume = false;
        
        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK
            
            var questState = GetQuestState ();
            if (questState == null)
                return;

            if (string.IsNullOrEmpty (memoryKey))
                return;
            
            var entityPersistent = IDUtility.GetLinkedPersistentEntity (entityOverworld);
            if (entityPersistent == null)
                return;

            var memoryPresent = entityPersistent.TryGetMemoryRounded (memoryKey, out int memoryValue);
            if (!memoryPresent)
                return;

            if (offset)
                OverworldQuestUtility.TryOffsetQuestMemory (questState.key, memoryKey, memoryValue);
            else
                OverworldQuestUtility.TrySetQuestMemory (questState.key, memoryKey, memoryValue);
            
            if (consume)
                entityPersistent.RemoveMemoryFloat (memoryKey);

            #endif
        }
    }
    
    [Serializable]
    public class OverworldEntityMemoryFromQuest : OverworldFunctionQuest, IOverworldTargetedFunction
    {
        [ValueDropdown ("@DataMultiLinkerOverworldMemory.data.Keys")]
        public string memoryKey;
        public bool offset = false;
        public bool consume = false;
        
        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK
            
            var questState = GetQuestState ();
            if (questState == null)
                return;

            if (string.IsNullOrEmpty (memoryKey))
                return;
            
            var entityPersistent = IDUtility.GetLinkedPersistentEntity (entityOverworld);
            if (entityPersistent == null)
                return;

            var memoryPresent = OverworldQuestUtility.TryGetQuestMemory (questState.key, memoryKey, out int memoryValue);
            if (!memoryPresent)
                return;

            if (offset)
                entityPersistent.OffsetMemoryFloat (memoryKey, memoryValue);
            else
                entityPersistent.SetMemoryFloat (memoryKey, memoryValue);
            
            if (consume)
                OverworldQuestUtility.TryRemoveQuestMemory (questState.key, memoryKey);

            #endif
        }
    }
}