using System;
using PhantomBrigade.Data;
using PhantomBrigade.Input.Components;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatForceExecution : ICombatFunction
    {
        [PropertyRange (1, 2)]
        public int turnsAdvanced = 1;
        public float timeScaleForced = -1f;

        [Button ("Test"), HideInEditorMode]
        public void Run ()
        {
            #if !PB_MODSDK

            CombatUtilities.ConfirmExecution (turnsAdvanced, timeScaleForced);
            
            #endif
        }
    }
}