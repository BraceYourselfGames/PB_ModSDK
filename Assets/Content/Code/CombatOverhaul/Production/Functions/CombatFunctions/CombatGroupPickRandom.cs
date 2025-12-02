using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
    public class DataBlockRandomBranch
    {
        [PropertyOrder (-2)]
        [PropertyRange (0.01f, 1f)]
        public float weight = 1f;

        public List<ICombatFunction> functions = new List<ICombatFunction> ();
    }

    public class CombatGroupPickRandom : ICombatFunction
    {
        public List<DataBlockRandomBranch> branches = new List<DataBlockRandomBranch> ();
        private List<DataBlockRandomBranch> branchesEvaluated = new List<DataBlockRandomBranch> ();

        public void Run ()
        {
            #if !PB_MODSDK

            if (branches == null || branches.Count == 0)
                return;
            
            float total = 0f;
            
            branchesEvaluated.Clear ();
            foreach (var branch in branches)
            {
                if (branch == null)
                    continue;
                
                total += branch.weight;
                branchesEvaluated.Add (branch);
            }

            float r = Random.Range (0f, 1f) * total;
            DataBlockRandomBranch branchChosen = null;
            
            foreach (var branch in branchesEvaluated)
            {
                if (r < branch.weight)
                {
                    branchChosen = branch;
                    break;
                }
                
                r -= branch.weight;
            }
            
            if (branchChosen == null)
                branchChosen = branches[branches.Count - 1];

            if (branchChosen == null)
                return;

            if (branchChosen.functions != null)
            {
                foreach (var function in branchChosen.functions)
                {
                    if (function != null)
                        function.Run ();
                }
            }

            #endif
        }
    }
}