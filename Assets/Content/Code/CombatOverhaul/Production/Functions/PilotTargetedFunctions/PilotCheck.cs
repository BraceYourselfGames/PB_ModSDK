using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class PilotCheck : IPilotTargetedFunction
    {
        public List<IPilotValidationFunction> checks = new List<IPilotValidationFunction> ();
        
        [LabelText ("Then")]
        [DropdownReference]
        public List<IPilotTargetedFunction> functions;
        
        [LabelText ("Else")]
        [DropdownReference]
        public List<IPilotTargetedFunction> functionsElse;
        
        public void Run (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK
            
            if (checks == null)
                return;

            bool passed = true;
            if (checks != null)
            {
                foreach (var check in checks)
                {
                    if (check != null && !check.IsValid (pilot, entityPersistentLinked))
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
                        function.Run (pilot, entityPersistentLinked);
                }
            }
            
            #endif
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public PilotCheck () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
}