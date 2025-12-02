using System;
using System.Collections.Generic;
using Entitas;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldIntValueFixed : IOverworldIntValueFunction
    {
        public int value;

        public int Resolve (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            return value;

            #endif
            
            return 0;
        }
    }
    
    [Serializable]
    public class OverworldIntValueUnitsAtBase : IOverworldIntValueFunction
    {
        public List<ICombatUnitValidationFunction> checks;
        
        public int Resolve (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            var units = UnitUtilities.GetUnitsAtBase ();
            int unitsPassed = 0;
            
            foreach (var unitPersistent in units)
            {
                if (checks != null)
                {
                    bool passed = true;
                    foreach (var check in checks)
                    {
                        if (check != null && !check.IsValid (unitPersistent))
                        {
                            passed = false;
                            break;
                        }
                    }
                    
                    if (!passed)
                        continue;
                }

                unitsPassed += 1;
            }

            return unitsPassed;

            #endif
            
            return 0;
        }
    } 
    
    [Serializable]
    public class OverworldIntValueUnitsInCombat : IOverworldIntValueFunction
    {
        public List<ICombatUnitValidationFunction> checks;
        
        public int Resolve (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK
            
            if (!IDUtility.IsGameState (GameStates.combat))
                return 0;

            var units = ScenarioUtility.GetParticipantUnitsPersistent ();
            int unitsPassed = 0;
            
            foreach (var unitPersistent in units)
            {
                if (checks != null)
                {
                    bool passed = true;
                    foreach (var check in checks)
                    {
                        if (check != null && !check.IsValid (unitPersistent))
                        {
                            passed = false;
                            break;
                        }
                    }
                    
                    if (!passed)
                        continue;
                }

                unitsPassed += 1;
            }

            return unitsPassed;

            #endif
            
            return 0;
        }
    } 
    
    [Serializable]
    public class OverworldIntValuePilotsAtBase : IOverworldIntValueFunction
    {
        public List<IPilotValidationFunction> checks;
        
        public int Resolve (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            var pilots = PilotUtility.GetPilotsAtBase ();
            int pilotsPassed = 0;
            
            foreach (var pilot in pilots)
            {
                if (checks != null)
                {
                    bool passed = true;
                    foreach (var check in checks)
                    {
                        if (check != null && !check.IsValid (pilot, null))
                        {
                            passed = false;
                            break;
                        }
                    }
                    
                    if (!passed)
                        continue;
                }
                
                if (pilot.hasDeathStatus)
                    continue;

                pilotsPassed += 1;
            }

            return pilotsPassed;

            #endif
            
            return 0;
        }
    }
    
    [Serializable]
    public class OverworldIntValueCompletionPreset : IOverworldIntValueFunction
    {
        [ValueDropdown ("@DataMultiLinkerOverworldPointPreset.GetKeys ()")]
        public string key;

        public int Resolve (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            var overworld = Contexts.sharedInstance.overworld;
            var c = overworld.overworldPointCompletions.keys;
            return c != null && !string.IsNullOrEmpty (key) && c.TryGetValue (key, out var v) ? v : 0;

            #endif
            
            return 0;
        }
    }
    
    [Serializable]
    public class OverworldIntValueCompletionScenario : IOverworldIntValueFunction
    {
        [ValueDropdown ("@DataMultiLinkerScenario.GetKeys ()")]
        public string key;

        public int Resolve (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            var overworld = Contexts.sharedInstance.overworld;
            var c = overworld.overworldScenarioCompletions.keys;
            return c != null && !string.IsNullOrEmpty (key) && c.TryGetValue (key, out var v) ? v : 0;

            #endif
            
            return 0;
        }
    }
    
    [Serializable]
    public class OverworldIntValueCompletionArea : IOverworldIntValueFunction
    {
        [ValueDropdown ("@DataMultiLinkerCombatArea.GetKeys ()")]
        public string key;

        public int Resolve (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            var overworld = Contexts.sharedInstance.overworld;
            var c = overworld.overworldAreaCompletions.keys;
            return c != null && !string.IsNullOrEmpty (key) && c.TryGetValue (key, out var v) ? v : 0;

            #endif
            
            return 0;
        }
    }
    
    [Serializable]
    public class OverworldIntValueMemory : IOverworldIntValueFunction
    {
        [ValueDropdown ("@DataMultiLinkerOverworldMemory.GetKeys ()")]
        public string key;

        public int Resolve (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            var entityPersistent = IDUtility.GetLinkedPersistentEntity (entityOverworld);
            if (entityPersistent == null)
                return 0;

            bool found = entityPersistent.TryGetMemoryRounded (key, out var value);
            return found ? value : 0;

            #endif
            
            return 0;
        }
    }
    
    [Serializable]
    public class OverworldIntValueMemoryAdv : IOverworldIntValueFunction
    {
        [ValueDropdown ("@DataMultiLinkerOverworldMemory.GetKeys ()")]
        public string key;

        public int valueFallback;

        public List<IFloatOperation> operations;

        public int Resolve (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            var entityPersistent = IDUtility.GetLinkedPersistentEntity (entityOverworld);
            if (entityPersistent == null)
                return valueFallback;

            bool found = entityPersistent.TryGetMemoryFloat (key, out var v);
            var valueOutput = found ? v : (float)valueFallback;

            if (operations != null)
            {
                foreach (var op in operations)
                {
                    if (op == null)
                        continue;
                    
                    valueOutput = op.Apply (valueOutput);
                    
                    if (float.IsNaN (valueOutput) || float.IsInfinity (valueOutput))
                    {
                        Debug.LogWarning ($"Encountered an invalid float operation leading to NaN or infinity via function {op.GetType ()}");
                        valueOutput = 0;
                        break;
                    }
                }
            }

            return Mathf.RoundToInt (valueOutput);

            #endif
            
            return 0;
        }
    }
    
    [Serializable]
    public class OverworldIntValueCampaignProgress : IOverworldIntValueFunction
    {
        public int Resolve (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            var overworld = Contexts.sharedInstance.overworld;
            var campaignState = overworld.campaignState.s;
            return campaignState.progress;

            #endif
            
            return 0;
        }
    }
    
    [Serializable]
    public class OverworldIntValueCampaignLiberationCount : IOverworldIntValueFunction
    {
        public int Resolve (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            var overworld = Contexts.sharedInstance.overworld;
            var campaignState = overworld.campaignState.s;
            return campaignState.liberationCount;

            #endif
            
            return 0;
        }
    }
    
    [Serializable]
    public class OverworldIntValueWorkshopQueue : IOverworldIntValueFunction
    {
        public int Resolve (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            var actions = WorkshopUtility.GetOngoingActions ();
            return actions != null ? actions.Count : 0;

            #endif
            
            return 0;
        }
    }
    
    [Serializable]
    public class OverworldIntValueCombatTurn : IOverworldIntValueFunction
    {
        public int Resolve (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            var combat = Contexts.sharedInstance.combat;
            var c = combat.hasCurrentTurn ? combat.currentTurn.i : 0;
            return c;

            #endif
            
            return 0;
        }
    }
    
    [Serializable]
    public class OverworldIntValueCombatDestruction : IOverworldIntValueFunction
    {
        public int Resolve (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            var combat = Contexts.sharedInstance.combat;
            var c = combat.hasDestructionCount ? combat.destructionCount.i : 0;
            return c;

            #endif
            
            return 0;
        }
    }
}