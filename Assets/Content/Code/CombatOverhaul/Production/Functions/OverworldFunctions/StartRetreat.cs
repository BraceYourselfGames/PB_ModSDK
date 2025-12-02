using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class StartRetreat :  IOverworldFunction
    {
        public void Run ()
        {
            #if !PB_MODSDK
            
            bool retreatSuccess = OverworldUtility.TryRetreatToResupplyBase ();
            
            #endif
        }
    }
}