using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitEffectSpawnSequence : ICombatFunctionTargeted
    {
        public class EffectSequenceEntry
        {
            public float delay;
            public Vector3 position;
            public Vector3 rotation;
        }

        public Vector3 position;
        public Vector3 rotation;
        public DataBlockAsset asset = new DataBlockAsset ();
        public List<EffectSequenceEntry> sequence = new List<EffectSequenceEntry> ();
        
        #if UNITY_EDITOR

        [Button, PropertyOrder (-1)]
        private void GrabFromScene (GameObject holder, string prefabName)
        {
            #if !PB_MODSDK
            
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
            
            #endif
        }
        
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

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            if (asset == null || asset.scale == Vector3.zero)
                return;
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null || !unitCombat.hasPosition || !unitCombat.hasRotation)
                return;

            var targetPosition = unitCombat.GetCenterPoint () + (unitCombat.rotation.q * position);
            var targetDirection = (unitCombat.rotation.q * Quaternion.Euler (rotation)) * Vector3.forward;

            var targetRotation = Quaternion.LookRotation (targetDirection);
            foreach (var entry in sequence)
            {
                var posFinal = targetPosition + targetRotation * entry.position;
                var rotFinal = targetRotation * Quaternion.Euler (entry.rotation);
                var delay = Mathf.Clamp (entry.delay, 0f, 5f);
                
                AssetPoolUtility.ActivateInstance (asset.key, posFinal, rotFinal * Vector3.forward, asset.scale, delay: delay);
                // Debug.Log ($"Spawning effect {asset.key} at unit {unitPersistent.ToLog ()} ({targetPosition})");
            }
            
            #endif
        }
    }
}