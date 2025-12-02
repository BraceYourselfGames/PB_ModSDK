using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldIntValueQuestMemory : IOverworldIntValueFunction
    {
        [ValueDropdown ("@DataMultiLinkerOverworldQuest.GetKeys ()")]
        public string questKey;

        [ShowInInspector, ValueDropdown ("memoryPrefixes"), InlineButtonClear]
        public string memoryPrefix;
        
        [ValueDropdown ("@DataMultiLinkerOverworldMemory.data.Keys")]
        public string memoryKey;
        
        private static List<string> memoryPrefixes = new List<string>
        {
            OverworldPointUtility.memoryPrefixPointKey,
            OverworldPointUtility.memoryPrefixPointTag,
        };
        
        public int Resolve (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK
            
            var memoryKeyFinal = string.IsNullOrEmpty (memoryPrefix) ? memoryKey : memoryPrefix + memoryKey;
            bool found = OverworldQuestUtility.TryGetQuestMemory (questKey, memoryKeyFinal, out int value);
            if (!found)
                return 0;

            return value;

            #endif
            
            return 0;
        }
    }
}