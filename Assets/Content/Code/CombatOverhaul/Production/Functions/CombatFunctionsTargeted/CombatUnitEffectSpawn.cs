using System;
using PhantomBrigade.Data;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitEffectSpawn : ICombatFunctionTargeted
    {
        public Vector3 position;
        public Vector3 rotation;
        public DataBlockAsset asset = new DataBlockAsset ();

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            if (asset == null || asset.scale == Vector3.zero)
                return;
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null || !unitCombat.hasPosition || !unitCombat.hasRotation)
                return;

            var positionFinal = unitCombat.GetCenterPoint () + (unitCombat.rotation.q * position);
            var directionFinal = (unitCombat.rotation.q * Quaternion.Euler (rotation)) * Vector3.forward;
            
            AssetPoolUtility.ActivateInstance (asset.key, positionFinal, directionFinal, asset.scale);
            // Debug.Log ($"Spawning effect {asset.key} at unit {unitPersistent.ToLog ()} / {unitCombat.ToLog ()}");
            
            #endif
        }
    }
}