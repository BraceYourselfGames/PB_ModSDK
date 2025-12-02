using System;
using System.Collections.Generic;
using PhantomBrigade.Combat;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    public class PilotFunctionGroup : IPilotTargetedFunction
    {
        [LabelText ("Conditions")]
        public List<IPilotValidationFunction> conditions;
        
        [LabelText ("Then")]
        [DropdownReference]
        public List<IPilotTargetedFunction> functions;
        
        [LabelText ("Else")]
        [DropdownReference]
        public List<IPilotTargetedFunction> functionsElse;
        
        public void Run (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK
            
            if (functions == null || pilot == null || !pilot.isPilotTag)
                return;

            bool passed = true;
            if (conditions != null)
            {
                foreach (var condition in conditions)
                {
                    if (!condition.IsValid (pilot, null))
                    {
                        passed = false;
                        break;
                    }
                }
            }
            
            var functionsUsed = passed ? functions : functionsElse;
            if (functionsUsed != null)
            {
                foreach (var function in functionsUsed)
                {
                    if (function != null)
                        function.Run (pilot, null);
                }
            }
            
            #endif
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public PilotFunctionGroup () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
}