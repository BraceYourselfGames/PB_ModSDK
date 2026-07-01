using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [Serializable] 
    public class DataContainerSettingsArea : DataContainerUnique
    {
        public float blockMass = 1f;
        public float blockDrag = 0.01f;
        public float blockAngularDrag = 0.1f;
        
        [ValueDropdown("@DataMultiLinkerAssetPools.data.Keys"), InlineButtonClear]
        public string crashHitEffect = "fx_aoe_hit_01";
        public float crashDamageRadius = 6f;
        public float crashDamageToUnits = 0.45f;
        public float crashImpulseToUnits = 6f;
        
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

