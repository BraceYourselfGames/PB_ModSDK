using System;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatCameraLock : ICombatFunction
    {
        public bool enabled;

        public void Run ()
        {
            #if !PB_MODSDK
            
            UnityEngine.Debug.Log ($"Setting camera input permission to {enabled}");
            GameCameraSystem.SetTutorialInputPermission (enabled);
            
            #endif
        }
    }
    
    [Serializable]
    public class CombatCameraCutsceneState : ICombatFunction
    {
        public bool enabled;

        public void Run ()
        {
            #if !PB_MODSDK
            
            UnityEngine.Debug.Log ($"Entering simplified cutscene state {enabled}");
            
            GameCameraSystem.SetTutorialInputPermission (!enabled);
            GameCursorSystem.SetVisibility (!enabled);
            CIViewLoader.SetGameplayUIVisible (!enabled);
            
            #endif
        }
    }
}