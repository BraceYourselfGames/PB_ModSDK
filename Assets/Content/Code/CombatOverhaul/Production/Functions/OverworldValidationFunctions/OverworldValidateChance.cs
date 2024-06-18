using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldValidateChance : IOverworldValidationFunction
    {
        [PropertyRange (0f, 1f)]
        public float chance = 0.5f;

        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            var chanceFinal = Mathf.Clamp01 (chance);
            bool passed = Random.Range (0f, 1f) < chanceFinal;
            return passed;

            #else
            return false;
            #endif
        }
    }
}