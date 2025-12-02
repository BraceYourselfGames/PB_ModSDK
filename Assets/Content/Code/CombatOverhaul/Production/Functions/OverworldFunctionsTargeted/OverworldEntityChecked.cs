using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using PhantomBrigade.Overworld.Systems;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldEntityChecked : IOverworldTargetedFunction
    {
        [LabelText ("Conditions")]
        public List<IOverworldEntityValidationFunction> conditions;
        
        [LabelText ("Then")]
        [DropdownReference]
        public List<IOverworldTargetedFunction> functions;
        
        [LabelText ("Else")]
        [DropdownReference]
        public List<IOverworldTargetedFunction> functionsElse;
        
        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK
            
            if (functions == null)
                return;
            
            var entityPersistent = IDUtility.GetLinkedPersistentEntity (entityOverworld);
            if (entityPersistent == null)
                return;

            bool passed = true;
            if (conditions != null)
            {
                foreach (var condition in conditions)
                {
                    if (!condition.IsValid (entityPersistent))
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
                        function.Run (entityOverworld);
                }
            }
            
            #endif
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public OverworldEntityChecked () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
}