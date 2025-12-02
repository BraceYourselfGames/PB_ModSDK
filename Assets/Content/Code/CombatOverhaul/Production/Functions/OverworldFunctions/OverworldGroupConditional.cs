using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    public class OverworldGroupConditional : IOverworldFunction
    {
        [DropdownReference]
        public List<IOverworldGlobalValidationFunction> checksGlobal;
        
        [DropdownReference]
        public List<IOverworldEntityValidationFunction> checksBase;
        
        [DropdownReference]
        public List<DataBlockOverworldResolvedIntCheck> checksBaseValues;

        [LabelText ("Then")]
        [DropdownReference]
        public List<IOverworldFunction> functions;
        
        [LabelText ("Else")]
        [DropdownReference]
        public List<IOverworldFunction> functionsElse;
        
        public void Run ()
        {
            #if !PB_MODSDK

            bool passed = true;
            if (checksGlobal != null)
            {
                foreach (var check in checksGlobal)
                {
                    if (check != null && !check.IsValid ())
                    {
                        passed = false;
                        break;
                    }
                }
            }
            
            if (passed && checksBase != null)
            {
                var basePersistent = IDUtility.playerBasePersistent;
                foreach (var check in checksBase)
                {
                    if (check != null && !check.IsValid (basePersistent))
                    {
                        passed = false;
                        break;
                    }
                }
            }
            
            if (passed && checksBaseValues != null)
            {
                var baseOverworld = IDUtility.playerBaseOverworld;
                foreach (var check in checksBaseValues)
                {
                    if (check != null && !check.IsPassedWithEntity (baseOverworld))
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
                        function.Run ();
                }
            }

            #endif
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public OverworldGroupConditional () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
}