using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitTargetCompositeConnected : ICombatFunctionTargeted
    {
        private Color GetUnitColor = Color.HSVToRGB (0.3f, 0.2f, 1f);
        
        [GUIColor ("GetUnitColor")]
        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerUnitComposite\", \"GetUnitKeys\")")]
        public string unitKey;
        public List<ICombatFunctionTargeted> functionsTargeted;
        
        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null || string.IsNullOrEmpty (unitKey) || functionsTargeted == null || functionsTargeted.Count == 0)
                return;

            if (!unitCombat.hasUnitCompositeLink)
            {
                Debug.LogWarning ($"Can't run targeted functions on composite child {unitKey} using unit {unitPersistent.ToLog ()} - no composite link found");
                return;
            }

            var link = unitCombat.unitCompositeLink;
            var unitsInComposite = UnitUtilities.GetUnitsInComposite (link.compositeInstanceKey);
            if (unitsInComposite == null || !unitsInComposite.TryGetValue (unitKey, out var unitCombatTarget))
            {
                Debug.LogWarning ($"Can't run targeted functions on composite child {unitKey} using unit {unitPersistent.ToLog ()} with instance key {link.compositeInstanceKey} - couldn't find a child unit with that key under composite {link.compositeInstanceKey}");
                return;
            }

            var unitPersistentTarget = IDUtility.GetLinkedPersistentEntity (unitCombatTarget);
            foreach (var function in functionsTargeted)
                function.Run (unitPersistentTarget);
            
            #endif
        }
    }
}