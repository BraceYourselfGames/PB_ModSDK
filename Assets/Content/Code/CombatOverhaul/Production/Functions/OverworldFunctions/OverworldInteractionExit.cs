using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldInteractionExit : IOverworldFunction
    {
        public void Run ()
        {
            #if !PB_MODSDK

            OverworldInteractionUtility.TryExitFromCurrentInteraction ();

            #endif
        }
    }
}