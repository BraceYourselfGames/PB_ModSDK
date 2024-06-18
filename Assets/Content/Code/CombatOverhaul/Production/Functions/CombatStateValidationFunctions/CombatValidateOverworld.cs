using System;
using System.Collections.Generic;
using PhantomBrigade.Data;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatValidateOverworldBase : ICombatStateValidationFunction
    {
        public List<IOverworldValidationFunction> functions = new List<IOverworldValidationFunction> ();
        
        public bool IsValid (string stateKey, DataBlockScenarioState stateDefinition)
        {
            #if !PB_MODSDK

            if (functions == null)
                return false;

            var basePersistent = IDUtility.playerBasePersistent;
            if (basePersistent == null)
                return false;
            
            foreach (var function in functions)
            {
                if (function != null)
                {
                    bool valid = function.IsValid (basePersistent);
                    if (!valid)
                        return false;
                }
            }
            
            return true;

            #else
            return false;
            #endif
        }
    }
    
    [Serializable]
    public class CombatValidateOverworldSite : ICombatStateValidationFunction
    {
        public List<IOverworldValidationFunction> functions = new List<IOverworldValidationFunction> ();
        
        public bool IsValid (string stateKey, DataBlockScenarioState stateDefinition)
        {
            #if !PB_MODSDK

            if (functions == null)
                return false;

            var targetPersistent = ScenarioUtility.GetCombatSite ();
            if (targetPersistent == null)
                return false;
            
            foreach (var function in functions)
            {
                if (function != null)
                {
                    bool valid = function.IsValid (targetPersistent);
                    if (!valid)
                        return false;
                }
            }
            
            return true;

            #else
            return false;
            #endif
        }
    }
}