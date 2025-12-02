using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    public class OverworldValidateSimLock : DataBlockSubcheckBool, IOverworldGlobalValidationFunction
    {
        protected override string GetLabel () => present ? "Sim. lock should be present" : "Sim. lock should be absent";
        
        public bool IsValid ()
        {
            #if !PB_MODSDK

            var overworld = Contexts.sharedInstance.overworld;
            bool lockActive = overworld.hasSimulationLockCountdown;
            return lockActive == present;

            #else
            return false;
            #endif
        }
    }
    
    public class OverworldValidatePilotRecruitment : IOverworldGlobalValidationFunction
    {
        public bool IsValid ()
        {
            #if !PB_MODSDK

            var overworld = Contexts.sharedInstance.overworld;
            int pilotLimit = BasePartUtility.GetBaseStatAsInt (BaseStatKeys.PilotSlots);
            var pilots = PilotUtility.GetPilotsAtBase ();
            
            if (pilots.Count >= pilotLimit)
                return false;
            
            return true;

            #else
            return false;
            #endif
        }
    }
    
    public class OverworldValidateCampSelectionPilot : DataBlockSubcheckBool, IOverworldGlobalValidationFunction
    {
        protected override string GetLabel () => present ? "Pilot should be present" : "Pilot should be absent";

        public List<IPilotValidationFunction> checks; 
        
        public bool IsValid ()
        {
            #if !PB_MODSDK

            bool pilotPresent = true;
            var campEntitiesSelected = OverworldPointUtility.campEntitiesSelected;
            foreach (var entityPersistentID in campEntitiesSelected)
            {
                var entityPersistent = IDUtility.GetPersistentEntity (entityPersistentID);
                if (entityPersistent == null || entityPersistent.isDestroyed)
                    continue;
                
                if (!entityPersistent.isPilotTag)
                    continue;

                if (checks != null)
                {
                    bool valid = true;
                    foreach (var check in checks)
                    {
                        if (check != null && !check.IsValid (entityPersistent, null))
                        {
                            valid = false;
                            break;
                        }
                    }

                    if (!valid)
                        continue;
                }

                pilotPresent = true;
                break;
            }
            
            return pilotPresent == present;

            #else
            return false;
            #endif
        }
    }
    
    public class OverworldValidateCampSelectionUnit : DataBlockSubcheckBool, IOverworldGlobalValidationFunction
    {
        protected override string GetLabel () => present ? "Pilot should be present" : "Pilot should be absent";

        public List<IOverworldUnitValidationFunction> checks; 
        
        public bool IsValid ()
        {
            #if !PB_MODSDK

            bool unitPresent = true;
            var campEntitiesSelected = OverworldPointUtility.campEntitiesSelected;
            foreach (var entityPersistentID in campEntitiesSelected)
            {
                var entityPersistent = IDUtility.GetPersistentEntity (entityPersistentID);
                if (entityPersistent == null || entityPersistent.isDestroyed)
                    continue;
                
                if (!entityPersistent.isUnitTag)
                    continue;

                if (checks != null)
                {
                    bool valid = true;
                    foreach (var check in checks)
                    {
                        if (check != null && !check.IsValid (entityPersistent))
                        {
                            valid = false;
                            break;
                        }
                    }

                    if (!valid)
                        continue;
                }

                unitPresent = true;
                break;
            }
            
            return unitPresent == present;

            #else
            return false;
            #endif
        }
    }
}