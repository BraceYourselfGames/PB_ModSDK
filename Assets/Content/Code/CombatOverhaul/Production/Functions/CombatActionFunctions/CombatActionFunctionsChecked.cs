using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatActionFunctionsChecked : ICombatActionExecutionFunction
    {
        [LabelText ("Unit condition"), BoxGroup]
        public DataBlockScenarioSubcheckUnit unitCheck = new DataBlockScenarioSubcheckUnit ();
        
        [LabelText ("Then")]
        [DropdownReference]
        public List<ICombatActionExecutionFunction> functions;
        
        [LabelText ("Else")]
        [DropdownReference]
        public List<ICombatActionExecutionFunction> functionsElse;

        public void Run (CombatEntity unitCombat, ActionEntity action)
        {
            #if !PB_MODSDK

            if (functions == null)
                return;
            
            var unitPersistent = IDUtility.GetLinkedPersistentEntity (unitCombat);
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
                        function.Run (unitCombat, action);
                }
            }
            
            #endif
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public CombatActionFunctionsChecked () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
}