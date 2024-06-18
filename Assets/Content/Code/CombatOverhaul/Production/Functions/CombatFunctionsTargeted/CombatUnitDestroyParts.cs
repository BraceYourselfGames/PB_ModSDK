using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitDestroyParts : ICombatFunctionTargeted
    {
        [ValueDropdown ("@DataMultiLinkerPartSocket.data.Keys")]
        public HashSet<string> sockets = new HashSet<string> ();
    
        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return;

            var parts = EquipmentUtility.GetPartsInUnit (unitPersistent);
            foreach (var socket in sockets)
            {
                var part = EquipmentUtility.GetPartInUnit (unitPersistent, socket, parts: parts);
                if (part == null)
                    continue;
        
                part.ReplaceBarrierNormalized (0);
                part.ReplaceIntegrityNormalized (0);

                var damageToBarrier = DataHelperStats.GetCachedStatForPart (UnitStats.barrier, part);
                var damageToIntegrity = DataHelperStats.GetCachedStatForPart (UnitStats.hp, part);

                unitCombat.executionHistory.values.lostBarrier = damageToBarrier;
                unitCombat.executionHistory.values.lostIntegrity = damageToIntegrity;

                if (unitCombat.hasCombatView)
                {
                    var viewGO = unitCombat.combatView.view.gameObject;
                    var uvm = viewGO.GetComponentInChildren<IUnitVisualManager> ();
                    uvm.OnIntegrityChange (socket, 0);
                }

                EquipmentUtility.OnPartDestruction (unitCombat, null, unitPersistent, part, HitDirections.front);
            }
            
            #endif
        }
    }
}