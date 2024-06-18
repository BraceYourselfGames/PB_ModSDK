using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitSetStatMultiplier : ICombatFunctionTargeted
    {
        public enum Mode
        {
            Set,
            Offset,
            Multiply
        }

        [ValueDropdown ("@DataMultiLinkerUnitStats.data.Keys")]
        public string key;
        public Mode mode = Mode.Set;
        public float value = 5f;
        public bool refreshFullStats = false;
        
        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return;

            var multiplierCurrent = 1f;
            if (unitPersistent.hasStatMultipliers && unitPersistent.statMultipliers.multipliers.TryGetValue (key, out var multiplier))
                multiplierCurrent = multiplier;

            var multiplierModified = 1f;
            if (mode == Mode.Set)
                multiplierModified = value;
            else if (mode == Mode.Offset)
                multiplierModified = multiplierCurrent + value;
            else if (mode == Mode.Multiply)
                multiplierModified = multiplierCurrent * value;

            if (multiplierCurrent.RoughlyEqual (multiplierModified, 0.0001f))
                return;
            
            if (unitPersistent.hasStatMultipliers)
            {
                Debug.Log ($"Unit {unitPersistent.ToLog ()} stat {key} multiplier changed: {multiplierCurrent} -> {multiplierModified}");
                var multipliers = unitPersistent.statMultipliers.multipliers;
                multipliers[key] = multiplierModified;
                unitPersistent.ReplaceStatMultipliers (multipliers);
            }
            else
            {
                Debug.Log ($"Unit {unitPersistent.ToLog ()} stat {key} multiplier added: {multiplierCurrent} -> {multiplierModified}");
                var multipliers = new Dictionary<string, float> { { key, value } };
                unitPersistent.AddStatMultipliers (multipliers);
            }
            
            DataHelperStats.RefreshStatCacheForUnit (unitPersistent, false, true, refreshFullStats);
            
            #endif
        }
    }
}