using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldValidateResupplyPossible : IOverworldGlobalValidationFunction
    {
        public bool IsValid ()
        {
            #if !PB_MODSDK

            var basePersistent = IDUtility.playerBasePersistent;
            if (basePersistent == null)
                return false;

            if (OverworldUtility.IsCurrentProvinceFriendly ())
                return false;
            
            bool alreadyRested = basePersistent != null && basePersistent.TryGetMemoryRounded (EventMemoryIntAutofilled.World_Auto_RestCompleted, out int v) && v > 0;
            if (alreadyRested)
                return false;

            int campSupplies = EquipmentUtility.GetResourceInInventoryRounded (basePersistent, ResourceKeys.campSupplies);
            int campSuppliesLimit = Mathf.RoundToInt (EquipmentUtility.GetResourceLimit (ResourceKeys.campSupplies));
            if (campSupplies < campSuppliesLimit)
                return true;
            
            var pilots = PilotUtility.GetPilotsAtBase ();
            foreach (var pilot in pilots)
            {
                var hpMax = pilot.GetPilotStatMax (PilotStatKeys.hp);
                var hp = pilot.GetPilotStat (PilotStatKeys.hp);
                if (hp < hpMax)
                    return true;
                    
                var fatigue = pilot.GetPilotStat (PilotStatKeys.fatigue);
                if (fatigue > 0)
                    return true;
            }
            
            var units = UnitUtilities.GetUnitsAtBase ();
            foreach (var unit in units)
            {
                var hp = unit.hasUnitFrameIntegrity ? unit.unitFrameIntegrity.f : 1f;
                if (hp < 1f)
                    return true;
            }

            return false;

            #else
            return false;
            #endif
        }
    }
}