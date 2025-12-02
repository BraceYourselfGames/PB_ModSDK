using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    public class QuestExit : OverworldFunctionQuest, IOverworldFunction
    {
        public bool effectOnExit = true;
        
        public void Run ()
        {
            #if !PB_MODSDK
            
            var questState = GetQuestState ();
            if (questState == null)
                return;

            OverworldQuestUtility.TryRemovingQuest (questState.key, effectOnExit);
            
            #endif
        }
    }
}