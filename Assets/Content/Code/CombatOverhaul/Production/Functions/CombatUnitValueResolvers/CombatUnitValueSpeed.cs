using System;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitValueSpeed : ICombatUnitValueResolver
    {
        public enum Mode
        {
            Limit,
            Current
        }

        public Mode mode = Mode.Limit;
        
        public float Resolve (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return 0f;

            if (mode == Mode.Limit)
            {
                if (unitCombat.hasMovementSpeedCurrent)
                    return unitCombat.movementSpeedCurrent.f;
            }
            else if (mode == Mode.Current)
            {
                if (unitCombat.hasVelocity)
                    return unitCombat.velocity.v.magnitude;
            }

            return 0f;

            #else
            return 0f;
            #endif
        }
    }
}