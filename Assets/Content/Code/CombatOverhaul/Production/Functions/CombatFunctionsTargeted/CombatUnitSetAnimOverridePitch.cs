using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitSetAnimOverridePitch : ICombatFunctionTargeted
    {
        [PropertyRange (-90f, 90f)]
        public float angle = 0f;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            unitCombat.ReplaceUnitAnimationOverrideFixedPitch (angle);

            #endif
        }
    }
}