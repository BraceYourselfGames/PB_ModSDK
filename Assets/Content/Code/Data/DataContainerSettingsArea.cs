using System;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [Serializable] 
    public class DataContainerSettingsArea : DataContainerUnique
    {
        public float blockMass = 1f;
        public float blockDrag = 0.01f;
        public float blockAngularDrag = 0.1f;
        public float crashDamageRadius = 3f;
        public float crashDamageAtEpicenter = 50f;
        public float crashDamageAtEdge = 10f;
        public float crashBounceForceMultiplier = 1f;
        public float crashPushForceMultiplier = 1f;
        public float damageScalar = 0.015f;

        [Space (8f)]
        public float horizontalCost = 1500f;
        public float diagonalCost = 1500f;
        public float jumpUpCost = 3000f;
        public float jumpDownCost = 2000f;
        public float jumpOverClimbCost = 3000f;
        public float jumpOverDropCost = 3000f;
        
        
    }
}

