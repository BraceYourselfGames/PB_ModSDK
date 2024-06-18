using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    public class UnlockAchievement : IOverworldEventFunction, IOverworldActionFunction, IOverworldFunction, ICombatFunction
    {
        [ValueDropdown("@AchievementHelper.GetAchievement()")]
        public string key;

        public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
        {
            #if !PB_MODSDK
            
            AchievementHelper.UnlockAchievement (key);
            
            #endif
        }
		
        public void Run (OverworldActionEntity source)
        {
            #if !PB_MODSDK
            
            AchievementHelper.UnlockAchievement (key);
            
            #endif
        }

        public void Run ()
        {
            #if !PB_MODSDK
            
            AchievementHelper.UnlockAchievement (key);
            
            #endif
        }
    }
}