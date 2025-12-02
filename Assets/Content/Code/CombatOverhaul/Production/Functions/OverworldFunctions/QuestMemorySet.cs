using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    public class QuestMemorySet : OverworldFunctionQuest, IOverworldFunction
    {
        [ValueDropdown ("@DataMultiLinkerOverworldMemory.data.Keys")]
        public string memoryKey;

        public int value;
        
        public void Run ()
        {
            #if !PB_MODSDK
            
            var questState = GetQuestState ();
            if (questState == null)
                return;

            OverworldQuestUtility.TrySetQuestMemory (questState.key, memoryKey, value);

            #endif
        }
    }
    
    public class QuestMemoryOffset : OverworldFunctionQuest, IOverworldFunction
    {
        [ValueDropdown ("@DataMultiLinkerOverworldMemory.data.Keys")]
        public string memoryKey;

        public int value;

        public DataBlockInt min;
        public DataBlockInt max;
        
        public void Run ()
        {
            #if !PB_MODSDK
            
            var questState = GetQuestState ();
            if (questState == null)
                return;

            if (min == null && max == null)
                OverworldQuestUtility.TryOffsetQuestMemory (questState.key, memoryKey, value);
            else
            {
                var valueModified = OverworldQuestUtility.TryGetQuestMemory (questState.key, memoryKey, out var v) ? v : 0;
                valueModified += value;

                if (min != null)
                    valueModified = Mathf.Max (min.i, valueModified);

                if (max != null)
                    valueModified = Mathf.Min (max.i, valueModified);

                OverworldQuestUtility.TrySetQuestMemory (questState.key, memoryKey, valueModified);
            }

            #endif
        }
    }
    
    public class QuestMemoryRemove : OverworldFunctionQuest, IOverworldFunction
    {
        [ValueDropdown ("@DataMultiLinkerOverworldMemory.data.Keys")]
        public string memoryKey;

        public void Run ()
        {
            #if !PB_MODSDK
            
            var questState = GetQuestState ();
            if (questState == null)
                return;

            OverworldQuestUtility.TryRemoveQuestMemory (questState.key, memoryKey);

            #endif
        }
    }
}