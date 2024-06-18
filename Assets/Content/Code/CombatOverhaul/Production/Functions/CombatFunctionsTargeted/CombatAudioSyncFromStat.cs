using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatAudioSyncFromStat : ICombatFunctionTargeted
    {
        [ValueDropdown ("@DataHelperStats.GetNormalizedCombatStatKeys")]
        public string statKey = string.Empty;
        public string audioSyncKey = string.Empty;
        
        public Vector2 inputRange = new Vector2 (0f, 1f);
        public Vector2 outputRange = new Vector2 (0f, 1f);

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            if (string.IsNullOrEmpty (statKey) || string.IsNullOrEmpty (audioSyncKey) || unitPersistent == null)
                return;

            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            var valueNormalized = DataHelperStats.GetNormalizedCombatStat (unitCombat, statKey);
            var valueRemap = Mathf.Clamp01 (valueNormalized.RemapToRange (inputRange.x, inputRange.y, 0f, 1f));
            var valueSync = Mathf.Lerp (outputRange.x, outputRange.y, valueRemap);
            
            // Debug.Log ($"Updating audio sync {audioSyncKey}: {valueSync:0.##} | Stat {statKey}: {valueNormalized:0.##} -> {valueRemap:0.##}");
            
            AudioUtility.CreateAudioSyncUpdate (audioSyncKey, valueSync);
            
            #endif
        }
    }
}