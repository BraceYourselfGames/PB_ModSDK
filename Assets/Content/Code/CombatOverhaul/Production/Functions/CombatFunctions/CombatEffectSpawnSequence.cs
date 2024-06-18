using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatEffectSpawnSequence : ICombatFunction
    {
        public class EffectSequenceEntry
        {
            public float delay;
            public Vector3 position;
            public Vector3 rotation;
        }
        
        public TargetFromSource target = new TargetFromSource ();
        public DataBlockAsset asset = new DataBlockAsset ();
        public List<EffectSequenceEntry> sequence = new List<EffectSequenceEntry> ();
        
        #if UNITY_EDITOR
        #if !PB_MODSDK
        
        [Button, PropertyOrder (-1)]
        private void GrabFromScene (GameObject holder, string prefabName)
        {
            if (holder == null || asset == null)
                return;

            var pool = DataMultiLinkerAssetPools.GetEntry (asset.key);
            if (pool == null || pool.prefab == null)
                return;

            if (string.IsNullOrEmpty (prefabName))
                prefabName = pool.prefab.name;
            
            if (string.IsNullOrEmpty (prefabName))
                return;

            sequence = new List<EffectSequenceEntry> ();

            var t = holder.transform;
            for (int i = 0; i < t.childCount; ++i)
            {
                var child = t.GetChild (i);
                if (!child.name.Contains (prefabName))
                    continue;
                
                sequence.Add (new EffectSequenceEntry
                {
                    delay = 0f,
                    position = child.localPosition,
                    rotation = child.localRotation.eulerAngles
                });
            }
        }
        
        #endif
        
        [Button, PropertyOrder (-1)]
        private void SetDelay (Vector2 range)
        {
            if (sequence == null || sequence.Count == 0)
                return;

            for (int i = 0; i < sequence.Count; ++i)
            {
                var interpolant = (float)i / (float)(sequence.Count - 1);
                sequence[i].delay = Mathf.Lerp (range.x, range.y, interpolant);
            }
        }
        
        #endif

        public void Run ()
        {
            #if !PB_MODSDK
            
            if (asset == null || asset.scale == Vector3.zero)
                return;
            
            if (target == null || sequence == null || sequence.Count == 0)
                return;

            var targetPositionFound = ScenarioUtility.GetTarget (null, target, out var targetPosition, out var targetDirection, out var targetUnitCombat);
            if (!targetPositionFound)
            {
                Debug.LogWarning ($"Failed to find a position for VFX {asset.key} using source {target.type} and name {target.name}");
                return;
            }

            var targetRotation = Quaternion.LookRotation (targetDirection);
            foreach (var entry in sequence)
            {
                var posFinal = targetPosition + targetRotation * entry.position;
                var rotFinal = targetRotation * Quaternion.Euler (entry.rotation);
                var delay = Mathf.Clamp (entry.delay, 0f, 5f);
                
                AssetPoolUtility.ActivateInstance (asset.key, posFinal, rotFinal * Vector3.forward, asset.scale, delay: delay);
                // Debug.Log ($"Spawning effect {asset.key} at target {target.type}/{target.name} ({targetPosition})");
            }
            
            #endif
        }
    }
}