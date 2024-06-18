using System;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class SelectBase : IOverworldFunction
    {
        public void Run ()
        {
            #if !PB_MODSDK
            
            if (!IDUtility.IsGameState (GameStates.overworld))
                return;

            var baseOverworldID = IDUtility.playerBaseOverworld.id.id;
            var overworld = Contexts.sharedInstance.overworld;
            
            GameCameraSystem.SetTargetOverworldEntity (baseOverworldID);
            overworld.ReplaceSelectedEntity (baseOverworldID);
            
            #endif
        }
    }
}