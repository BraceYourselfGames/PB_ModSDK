using System;
using System.Collections.Generic;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitSetMemory : ICombatFunctionTargeted
    {
        public enum ResolverMode
        {
            Sum,
            Multiply,
            Random,
            Min,
            Max
        }
        
        [ValueDropdown ("@DataMultiLinkerOverworldMemory.data.Keys")]
        public string key;
        
        public UnitMemoryContext context = UnitMemoryContext.MobileBase;
        public ValueOperation operation = ValueOperation.Set;

        public ResolverMode resolverMode = ResolverMode.Random;
        public List<ICombatUnitValueResolver> resolvers = new List<ICombatUnitValueResolver> ();

        private List<float> valuesResolved = new List<float> ();

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            if (unitPersistent == null || string.IsNullOrEmpty (key) || resolvers == null || resolvers.Count == 0)
                return;

            // Get host taking the memory value
            PersistentEntity hostPersistent = null;
            if (context == UnitMemoryContext.Unit)
            {
                hostPersistent = unitPersistent;
            }
            else if (context == UnitMemoryContext.Pilot)
            {
                var pilotPersistent = IDUtility.GetLinkedPilot (unitPersistent);
                hostPersistent = pilotPersistent;
            }
            else if (context == UnitMemoryContext.MobileBase)
            {
                var basePersistent = IDUtility.playerBasePersistent;
                hostPersistent = basePersistent;
            }
            else if (context == UnitMemoryContext.BattleSite)
            {
                var sitePersistent = ScenarioUtility.GetCombatSite ();
                hostPersistent = sitePersistent;
            }
            
            if (hostPersistent == null)
                return;
            
            // Resolve all values
            valuesResolved.Clear ();
            foreach (var resolver in resolvers)
            {
                var valueResolved = 0f;
                if (resolver != null)
                    valueResolved = resolver.Resolve (unitPersistent);
                valuesResolved.Add (valueResolved);
            }

            // Calculate final value based on resolved values
            var valueArgument = 0f;
            
            if (resolverMode == ResolverMode.Random)
                valueArgument = valuesResolved.GetRandomEntry ();
            else if (resolverMode == ResolverMode.Sum)
            {
                foreach (var valueResolved in valuesResolved)
                    valueArgument += valueResolved;
            }
            else if (resolverMode == ResolverMode.Multiply)
            {
                bool first = true;
                foreach (var valueResolved in valuesResolved)
                {
                    if (first)
                    {
                        first = false;
                        valueArgument = valueResolved;
                    }
                    else
                        valueArgument *= valueResolved;
                }
            }
            else if (resolverMode == ResolverMode.Min)
            {
                bool first = true;
                foreach (var valueResolved in valuesResolved)
                {
                    if (first)
                    {
                        first = false;
                        valueArgument = valueResolved;
                    }
                    else
                        valueArgument = Mathf.Min (valueArgument, valueResolved);
                }
            }
            else if (resolverMode == ResolverMode.Max)
            {
                bool first = true;
                foreach (var valueResolved in valuesResolved)
                {
                    if (first)
                    {
                        first = false;
                        valueArgument = valueResolved;
                    }
                    else
                        valueArgument = Mathf.Max (valueArgument, valueResolved);
                }
            }
            
            var valueCurrent = 0f;
            hostPersistent.TryGetMemoryFloat (key, out valueCurrent);
            var valueWritten = valueCurrent.ApplyOperation (operation, valueArgument);
            
            // Modify memory
            hostPersistent.SetMemoryFloat (key, valueWritten);
            
            #endif
        }
    }
}