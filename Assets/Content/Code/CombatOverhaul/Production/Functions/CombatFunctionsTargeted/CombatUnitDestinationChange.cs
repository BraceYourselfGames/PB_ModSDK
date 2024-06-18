using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class TargetUnitLocalOffsets
    {
        public bool eased = false;
        public bool linear = true;
        public Vector3 from;
        public Vector3 to;
    }
    
    public enum TargetedPointInterpolationMode
    {
        Linear,
        ArcAroundOrigin,
        ArcAroundMidpoint
    }
    
    [Serializable]
    public class TargetFromSourceInterpolated : TargetFromSource
    {
        [PropertyOrder (1)]
        public bool eased = false;
        
        [PropertyOrder (1)]
        public TargetedPointInterpolationMode mode = TargetedPointInterpolationMode.Linear;

        [PropertyOrder (1), ShowIf ("@mode == TargetedPointInterpolationMode.ArcAroundMidpoint")]
        public Vector3 midpointOffsetLocal;
    }
    
    [Serializable]
    public class TargetFromSource
    {
        public CombatTargetSource type = CombatTargetSource.None;
        
        [GUIColor ("GetNameColor")]
        [InlineButtonClear, ShowIf ("IsTargetUsed")]
        public string name;
        
        [ShowIf ("IsCenterUsed")]
        public bool center = true;
        
        [ShowIf ("@modifiers != null")]
        [PropertyTooltip ("If false, local modifiers are applied in the rotation frame of a target. If true, local modifiers are applied in a rotation frame from origin to target. The latter is useful for cases like expressing a target position that's always to the left of a targeted unit in the eyes of a firing unit.")]
        public bool modifiersDirectional = false;

        [PropertyOrder (2)]
        public List<ITargetModifierFunction> modifiers;
        
        public bool IsTargetUsed => type != CombatTargetSource.None && type != CombatTargetSource.Self && type != CombatTargetSource.SelfRelative;
        
        #region Editor
        #if UNITY_EDITOR
        
        public bool IsLocalUsed => type == CombatTargetSource.None;
        public bool IsCenterUsed => type == CombatTargetSource.State || type == CombatTargetSource.Location || type == CombatTargetSource.Volume;
        private Color GetNameColor => IsTargetUsed && string.IsNullOrEmpty (name) ? Color.HSVToRGB (0f, 0.5f, 1f) : Color.white;

        #endif
        #endregion
    }

    [Serializable]
    public class CombatUnitDestinationChange : ICombatFunctionTargeted
    {
        public TargetFromSource target = new TargetFromSource ();

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            // AI agent binding can be delayed by a frame, which will prevent this from working on just-spawned units
            Co.DelayFrames (1, () => RunDelayed (unitPersistent));
            
            #endif
        }
        
        private void RunDelayed (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            if (target == null)
                return;
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            var aiAgent = IDUtility.GetLinkedAIAgent (unitCombat);

            if (aiAgent == null)
            {
                Debug.LogWarning ($"Failed to set destination on unit {unitPersistent.ToLog ()}: no combat unit or AI entity");
                return;
            }

            var targetPositionFound = ScenarioUtility.GetTargetPosition (unitCombat, target, out var targetPosition);
            if (!targetPositionFound)
            {
                Debug.LogWarning ($"Failed to find a destination for unit {unitPersistent.ToLog ()} using source {target.type} and name {target.name}");
                return;
            }
            
            // Debug.Log ($"Modified AI destination of unit {unitPersistent.ToLog ()} to {targetPosition} | Source: {targetSource}, {targetName}");
            aiAgent.ReplaceAgentBehaviorDestination (targetPosition);
            
            #endif
        }
    }
}