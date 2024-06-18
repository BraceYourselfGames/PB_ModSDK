using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitFunctionsChecked : ICombatFunctionTargeted
    {
        [LabelText ("Unit condition"), BoxGroup]
        public DataBlockScenarioSubcheckUnit unitCheck = new DataBlockScenarioSubcheckUnit ();
        
        [LabelText ("Then")]
        [DropdownReference]
        public List<ICombatFunctionTargeted> functions;
        
        [LabelText ("Else")]
        [DropdownReference]
        public List<ICombatFunctionTargeted> functionsElse;
        
        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            if (functions == null)
                return;
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return;

            bool passed = true;
            if (unitCheck != null)
                passed = ScenarioUtility.IsUnitMatchingCheck (unitPersistent, unitCombat, unitCheck, true, true, false);
            
            var functionsUsed = passed ? functions : functionsElse;
            if (functionsUsed != null)
            {
                foreach (var function in functionsUsed)
                {
                    if (function != null)
                        function.Run (unitPersistent);
                }
            }
            
            #endif
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public CombatUnitFunctionsChecked () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
}