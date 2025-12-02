using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    public class OverworldGroupTargetFiltered : IOverworldFunction
    {
        public List<IOverworldEntityValidationFunction> checks = new List<IOverworldEntityValidationFunction>();
        public List<IOverworldTargetedFunction> functions = new List<IOverworldTargetedFunction> ();
        
        public void Run ()
        {
            #if !PB_MODSDK

            if (checks == null || checks.Count == 0 || functions == null || functions.Count == 0)
                return;

            var points = OverworldPointUtility.GetActivePoints (false, false, checks: checks);
            if (points == null || points.Count == 0)
            {
                Debug.LogWarning ($"Failed to find a point with a filter with {checks.Count} checks");
                return;
            }

            var point = points.GetRandomEntry ();
            Debug.Log ($"Found {points.Count} points with a filter with {checks.Count} checks, selected {point.ToLog ()}, executing {functions.Count} targeted functions");

            foreach (var function in functions)
            {
                if (function != null)
                    function.Run (point);
            }

            #endif
        }
    }
    
    public class OverworldGroupTargetNamed : IOverworldFunction
    {
        public string nameInternal;
        public List<IOverworldTargetedFunction> functions = new List<IOverworldTargetedFunction> ();
        
        public void Run ()
        {
            #if !PB_MODSDK

            var point = IDUtility.GetOverworldEntity (nameInternal);
            if (point == null)
            {
                Debug.LogWarning ($"Failed to find a point with an internal name {nameInternal}");
                return;
            }
            
            Debug.Log ($"Found point {point.ToLog ()} by internal name, executing {functions.Count} targeted functions");
            foreach (var function in functions)
            {
                if (function != null)
                    function.Run (point);
            }

            #endif
        }
    }
}