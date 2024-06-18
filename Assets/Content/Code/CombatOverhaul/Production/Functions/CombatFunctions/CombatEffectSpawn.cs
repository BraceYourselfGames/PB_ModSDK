using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatEffectSpawn : ICombatFunction
    {
        public TargetFromSource target = new TargetFromSource ();
        public DataBlockAsset asset = new DataBlockAsset ();
        
        [PropertyRange (0f, 5f)]
        public float delay = 0f;

        public void Run ()
        {
            #if !PB_MODSDK
            
            if (asset == null || asset.scale == Vector3.zero)
                return;
            
            if (target == null)
                return;

            var targetPositionFound = ScenarioUtility.GetTarget (null, target, out var targetPosition, out var targetDirection, out var targetUnitCombat);
            if (!targetPositionFound)
            {
                Debug.LogWarning ($"Failed to find a position for VFX {asset.key} using source {target.type} and name {target.name}");
                return;
            }

            AssetPoolUtility.ActivateInstance (asset.key, targetPosition, targetDirection, asset.scale, delay: delay);
            // Debug.Log ($"Spawning effect {asset.key} at target {target.type}/{target.name} ({targetPosition})");
            
            #endif
        }
    }
    
    [Serializable]
    public class CombatEffectSpawnSpatial : ICombatFunctionSpatial
    {
        [PropertyRange (0f, 5f)]
        public float delay = 0f;
        public DataBlockAsset asset = new DataBlockAsset ();
        public bool directionFlatten = false;
        public bool positionGround = false;

        public void Run (Vector3 position, Vector3 direction)
        {
            #if !PB_MODSDK
            
            if (asset == null || asset.scale == Vector3.zero)
                return;

            if (positionGround)
            {
                var ray = new Ray (position, Vector3.down);
                if (Physics.Raycast (ray, out var hit, 400f, LayerMasks.environmentMask))
                    position = hit.point;
            }

            if (directionFlatten)
            {
                direction = direction.FlattenAndNormalize ();
                if (direction == Vector3.zero)
                    direction = Vector3.forward;
            }

            AssetPoolUtility.ActivateInstance (asset.key, position, direction, asset.scale, delay: delay);
            // Debug.Log ($"Spawning effect {asset.key} at {position}");
            
            #endif
        }
    }
}