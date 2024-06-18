using System;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatChangeFogCustom : ICombatFunction
    {
        [PropertyRange (0f, 25f)]
        public float time;

        [PropertyRange (0f, 25f)]
        public float delay;
        
        [PropertyRange (0f, 1f)]
        public float densityGlobal;
        
        [PropertyRange (0f, 1f)]
        public float densityDistance;
        
        [PropertyRange (0f, 1f)]
        public float densityHeight;
        
        [PropertyRange (-100f, 100f)]
        public float heightOffset;

        public void Run ()
        {
            #if !PB_MODSDK
            
            PostprocessingHelper.SetFogCustomActive (true);
            PostprocessingHelper.SetFogCustomAnimation (time, delay, densityGlobal, densityDistance, densityHeight, heightOffset);
            
            #endif
        }

        [Button, PropertyOrder (-1), HideInEditorMode]
        private void Test ()
        {
            #if !PB_MODSDK
            
            if (!IDUtility.IsGameState (GameStates.combat))
                return;
            
            Run ();
            
            #endif
        }
    }
}