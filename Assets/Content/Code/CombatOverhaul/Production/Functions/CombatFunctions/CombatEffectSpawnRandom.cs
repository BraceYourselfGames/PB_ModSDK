using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatEffectSpawnRandom : ICombatFunction
    {
        public List<TargetFromSource> targets = new List<TargetFromSource> ();
        public DataBlockAsset asset = new DataBlockAsset ();
        
        [PropertyRange (0f, 5f)]
        public float delay = 0f;

        public void Run ()
        {
            #if !PB_MODSDK
            
            if (asset == null || asset.scale == Vector3.zero)
                return;
            
            if (targets == null || targets.Count == 0)
                return;

            var target = targets.GetRandomEntry ();
            if (target == null)
                return;
            
            var targetPositionFound = ScenarioUtility.GetTarget (null, target, out var targetPosition, out var targetDirection, out var targetUnitCombat);
            if (!targetPositionFound)
            {
                Debug.LogWarning ($"Failed to find a position for VFX {asset.key} using source {target.type} and name {target.name}");
                return;
            }

            AssetPoolUtility.ActivateInstance (asset.key, targetPosition, targetDirection, asset.scale, delay: delay);
            // Debug.Log ($"Spawning effect {asset.key} at randomized target {target.type}/{target.name} ({targetPosition})");
            
            #endif
        }
    }
}