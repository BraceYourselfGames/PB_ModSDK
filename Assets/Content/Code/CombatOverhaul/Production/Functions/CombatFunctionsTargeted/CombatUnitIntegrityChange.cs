using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitIntegrityChange : ICombatFunctionTargeted
    {
        public float value;
        public bool offset;

        [ValueDropdown ("@DataMultiLinkerPartSocket.data.Keys")]
        public HashSet<string> sockets;

        private static List<EquipmentEntity> partsModified = new List<EquipmentEntity> ();

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return;
            
            var parts = EquipmentUtility.GetPartsInUnit (unitPersistent);
            if (parts == null || parts.Count == 0)
                return;
            
            partsModified.Clear ();
            partsModified.AddRange (parts);

            foreach (var part in partsModified)
            {
                if (part == null || !part.IsPartTaggedAs (EquipmentTags.damageable) || part.isDestroyed)
                    continue;
                
                if (sockets != null && !sockets.Contains (part.partParentUnit.socket))
                    continue;
                
                float integrityMax = DataHelperStats.GetCachedStatForPart (UnitStats.hp, part);
                float barrierMax = DataHelperStats.GetCachedStatForPart (UnitStats.barrier, part);
                float partBarrierProportion = integrityMax <= 0f ? 1f : Mathf.Clamp01 (barrierMax / integrityMax);
                float partIntegrityProportion = 1f - partBarrierProportion;

                float integrityNormalized = part.integrityNormalized.f;
                float barrierNormalized = part.barrierNormalized.f;
                float ehpNormalized = (integrityNormalized * integrityMax + barrierNormalized * barrierMax) / (integrityMax + barrierMax);

                if (offset)
                    ehpNormalized = Mathf.Clamp01 (ehpNormalized + value);
                else
                    ehpNormalized = Mathf.Clamp01 (value);
                
                integrityNormalized = ehpNormalized * partIntegrityProportion;
                barrierNormalized = ehpNormalized * partBarrierProportion;
                
                part.ReplaceIntegrityNormalized (integrityNormalized);
                part.ReplaceBarrierNormalized (barrierNormalized);
            }
            
            #endif
        }
    }
}