using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    public class UnlockAchievement : IOverworldActionFunction, IOverworldFunction, ICombatFunction
    {
        [ValueDropdown("@AchievementKeys.GetKeys ()")]
        public string key;

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