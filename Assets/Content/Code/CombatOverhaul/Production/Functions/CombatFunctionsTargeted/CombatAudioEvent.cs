using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatAudioEvent : ICombatFunctionTargeted, ICombatFunction
    {
        [BoxGroup ("Offset", false)]
        public Vector3 offset;

        [ValueDropdown ("@AudioEvents.GetKeys ()")]
        public string audio = null;

        public float delayScaled = 0f;
        
        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null || !unitCombat.isUnitTag || !unitCombat.hasPosition)
                return;

            var position = unitCombat.position.v;
            if (unitCombat.hasLocalCenterPoint)
                position += unitCombat.GetCenterOffset ();
            
            if (unitCombat.hasRotation)
                position += unitCombat.rotation.q * offset;

            Run (position);
            
            #endif
        }

        public void Run ()
        {
            #if !PB_MODSDK
            
            Run (offset);
            
            #endif
        }
        
        public void Run (Vector3 position)
        {
            #if !PB_MODSDK
            
            if (string.IsNullOrEmpty (audio))
                return;
            
            var delaySafe = Mathf.Clamp (delayScaled, 0f, 5f);
            if (delaySafe <= 0f)
                AudioUtility.CreateAudioEvent (audio, position);
            else
                Co.DelayScaled (delaySafe, () => AudioUtility.CreateAudioEvent (audio, position));
            
            #endif
        }
    }
}