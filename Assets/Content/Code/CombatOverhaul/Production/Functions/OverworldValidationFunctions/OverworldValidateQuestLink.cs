using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldValidateQuestLink : OverworldFunctionQuest, IOverworldEntityValidationFunction
    {
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            var questState = GetQuestState ();
            if (questState == null)
                return false;
            
            var entityOverworld = IDUtility.GetLinkedOverworldEntity (entityPersistent);
            if (entityPersistent == null || entityOverworld == null)
                return false;

            bool linkMatch = entityOverworld.hasQuestLink && string.Equals (entityOverworld.questLink.key, questState.key, StringComparison.Ordinal);
            return linkMatch;

            #else
            return false;
            #endif
        }
    }

    public class OverworldValidateInteractionEncounter
    {
        
    }
}