using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitAnimateCompositeTransformToBlueprint : ICombatFunctionTargeted
    {
        public float startTime;
        public float duration;
        public bool targetSecondary;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return;

            if (!unitCombat.hasUnitCompositeChild)
            {
                Debug.LogWarning ($"Unit {unitPersistent.ToLog ()} is not a composite child, can't animate it");
                return;
            }
            
            if (!unitCombat.hasUnitCompositeLink)
            {
                Debug.LogWarning ($"Unit {unitPersistent.ToLog ()} is not a composite link, can't animate it");
                return;
            }

            var link = unitCombat.unitCompositeLink;
            var bp = DataMultiLinkerUnitComposite.GetEntry (link.blueprintKey);
            if (bp == null)
                return;
            
            if (bp.layoutProcessed == null || bp.layoutProcessed.units == null || !bp.layoutProcessed.units.TryGetValue (link.unitKey, out var unitInfo) || unitInfo.linkTransform == null)
            {
                Debug.LogWarning ($"Unit {unitPersistent.ToLog ()} can't find transform info for key {link.unitKey} under composite blueprint {bp.key}, can't animate it");
                return;
            }

            var lt = unitInfo.linkTransform;
            var positionTarget = Vector3.zero;
            var rotationTarget = Quaternion.identity;

            if (targetSecondary)
            {
                if (lt.secondary == null)
                {
                    Debug.LogWarning ($"Unit {unitPersistent.ToLog ()} can't find secondary transform info for key {link.unitKey} under composite blueprint {bp.key}, can't animate it");
                    return;
                }

                positionTarget = lt.secondary.position;
                rotationTarget = Quaternion.Euler (lt.secondary.rotation);
            }
            else
            {
                positionTarget = lt.position;
                rotationTarget = Quaternion.Euler (lt.rotation);
            }
            
            if (this.duration <= 0f)
            {
                var position = positionTarget;
                var rotation = rotationTarget;
                var id = unitCombat.unitCompositeChild.combatID;
                unitCombat.ReplaceUnitCompositeChild (id, position, rotation);
            }
            else
            {
                var time = Contexts.sharedInstance.combat.simulationTime.f;
                var startTime = time + Mathf.Max (0f, this.startTime);
                var duration = Mathf.Max (0f, this.duration);
                var position = positionTarget;
                var rotation = rotationTarget;
                unitCombat.ReplaceUnitCompositeChildAnimation (startTime, duration, position, rotation);
                // Debug.Log ($"T{time:0.##}s | Unit {unitCombat.ToLog ()} receives child transform animation | Start: {startTime:0.##}s | Duration: {duration:0.##} | Position: {position}");
            }
            
            #endif
        }
    }

    [Serializable]
    public class CombatUnitAnimateCompositeTransform : ICombatFunctionTargeted
    {
        public float startTime;
        public float duration;
        
        public Vector3 position;
        public Vector3 rotation;

        [LabelText ("Inverse Immediately")]
        public bool applyInverseImmediately;
    
        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return;

            if (!unitCombat.hasUnitCompositeChild)
            {
                Debug.LogWarning ($"Unit {unitPersistent.ToLog ()} is not a composite child, can't animate it");
                return;
            }

            if (this.duration <= 0f)
            {
                var position = unitCombat.unitCompositeChild.position + this.position;
                var rotation = unitCombat.unitCompositeChild.rotation * Quaternion.Euler (this.rotation);
                var id = unitCombat.unitCompositeChild.combatID;
                unitCombat.ReplaceUnitCompositeChild (id, position, rotation);
            }
            else
            {
                if (applyInverseImmediately)
                {
                    var position1 = unitCombat.unitCompositeChild.position - this.position;
                    var rotation1 = unitCombat.unitCompositeChild.rotation * Quaternion.Euler (-this.rotation);
                    var id = unitCombat.unitCompositeChild.combatID;
                    unitCombat.ReplaceUnitCompositeChild (id, position1, rotation1);
                }
                
                var time = Contexts.sharedInstance.combat.simulationTime.f;
                var startTime = time + Mathf.Max (0f, this.startTime);
                var duration = Mathf.Max (0f, this.duration);
                var position = unitCombat.unitCompositeChild.position + this.position;
                var rotation = unitCombat.unitCompositeChild.rotation * Quaternion.Euler (this.rotation);
                unitCombat.ReplaceUnitCompositeChildAnimation (startTime, duration, position, rotation);
                // Debug.Log ($"T{time:0.##}s | Unit {unitCombat.ToLog ()} receives child transform animation | Start: {startTime:0.##}s | Duration: {duration:0.##} | Position: {position}");
            }
            
            #endif
        }
    }
}