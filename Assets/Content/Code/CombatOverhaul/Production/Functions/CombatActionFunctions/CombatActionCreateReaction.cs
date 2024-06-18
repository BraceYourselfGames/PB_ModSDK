using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

#if !PB_MODSDK
using PhantomBrigade.Action.Components;
#endif

namespace PhantomBrigade.Functions
{
    public class DataBlockReactionCheck
    {
        public bool losCheck = false;
        public float angleThreshold = 0f;
        public float rotationSpeed = 0f;

    }
    
    [Serializable]
    public class CombatActionCreateReaction : ICombatActionExecutionFunction
    {
        [ValueDropdown ("@DataMultiLinkerAction.data.Keys")]
        public string dependencyActionKey;

        [DropdownReference (true)]
        public DataBlockFloat delay;
        
        [DropdownReference (true)]
        public DataBlockVector2 range;
        
        [DropdownReference (true)]
        public DataBlockVector2 directionAnimation;
        
        [DropdownReference (true)]
        public DataBlockFloat duration;
        
        [DropdownReference (true)]
        public DataBlockReactionCheck overrideCheck;
        
        [DropdownReference]
        public List<DataBlockActionFunctionTimed> functionsTimed;

        
        public void Run (CombatEntity unitCombat, ActionEntity action)
        {
            #if !PB_MODSDK

            if (action == null)
                return;
            
            var unitPersistent = IDUtility.GetLinkedPersistentEntity (unitCombat);
            if (unitCombat == null || unitPersistent == null)
                return;

            // If there is no dependent action to create, we can't really proceed
            var actionData = action.dataLinkAction.data;

            var dependencyData = DataMultiLinkerAction.GetEntry (dependencyActionKey, false);
            if (dependencyData == null)
            {
                Debug.LogWarning ($"Failed to initialize reaction {action.dataKeyAction.s} due to missing dependency action {dependencyActionKey}");
                return;
            }
            
            // Likewise, if valid duration can't be found, we can't safely place anything on the timeline later
            var dependencyDuration = DataHelperAction.GetPaintedActionDuration (dependencyData.key, unitPersistent);
            if (duration != null)
                dependencyDuration = duration.f;
            
            if (dependencyDuration <= 0f)
            {
                Debug.LogWarning ($"Failed to initialize reaction {action.dataKeyAction.s} due to invalid duration {dependencyDuration}");
                return;
            }

            var dependencyDelay = 0f;
            if (delay != null)
                dependencyDelay = Mathf.Max (0f, delay.f);

            var rangeChecked = DataHelperAction.GetReactionRange (actionData, dependencyData, unitPersistent);
            if (range != null)
                rangeChecked = range.v;

            action.isReaction = true;
            action.ReplaceReactionDependencyKey (dependencyData.key);
            action.ReplaceReactionDependencyData (dependencyData);
            action.ReplaceReactionDependencyTiming (dependencyDelay, dependencyDuration);
            action.ReplaceReactionDependencyRange (rangeChecked);
            
            if (actionData.dataCustom != null)
            {
                bool losCheck = false;
                var angleThreshold = 0f;
                var rotationSpeed = 0f;

                if (dependencyData.dataEquipment != null && dependencyData.dataEquipment.partUsed)
                {
                    var part = EquipmentUtility.GetPartInUnit (unitPersistent, dependencyData.dataEquipment.partSocket);
                    if (part != null)
                    {
                        var subsystemActivated = part.hasPrimaryActivationSubsystem ? IDUtility.GetEquipmentEntity (part.primaryActivationSubsystem.equipmentID) : null;
                        var subsystemBlueprint = subsystemActivated?.dataLinkSubsystem.data;
                        if (subsystemBlueprint != null)
                        {
                            subsystemBlueprint.TryGetFloat (PartCustomFloatKeys.ReactionAngleThreshold, out angleThreshold);
                            subsystemBlueprint.TryGetFloat (PartCustomFloatKeys.ReactionRotationSpeed, out rotationSpeed);
                            losCheck = subsystemBlueprint.IsFlagPresent (PartCustomFlagKeys.ReactionLosCheck);
                        }
                    }
                }

	            action.ReplaceReactionChecks (losCheck, angleThreshold, rotationSpeed);
            }
            
            if (overrideCheck != null)
                action.ReplaceReactionChecks (overrideCheck.losCheck, overrideCheck.angleThreshold, overrideCheck.rotationSpeed);

            if (directionAnimation != null)
                action.ReplaceReactionRotationTargets (directionAnimation.v.x, directionAnimation.v.y);
            
            if (functionsTimed != null)
            {
                var list = action.hasFunctionsTimed ? action.functionsTimed.list : new List<FunctionTimed> (functionsTimed.Count);
                action.ReplaceReactionDependencyFunctions (list);

                foreach (var functionTimed in functionsTimed)
                {
                    float timeStartNormalized = Mathf.Clamp01 (functionTimed.timeNormalized);
                    if (functionTimed.repeat == null)
                    {
                        list.Add (new FunctionTimed
                        {
                            timeNormalized = timeStartNormalized,
                            completed = false,
                            functions = UtilitiesYAML.CloneThroughYaml (functionTimed.functions)
                        });
                    }
                    else
                    {
                        int count = Mathf.Clamp (functionTimed.repeat.count, 1, 100);
                        float timeEndNormalized = Mathf.Clamp (functionTimed.repeat.timeNormalizedEnd, Mathf.Max (timeStartNormalized, 0.01f), 1f);
                        float interval = timeEndNormalized - timeStartNormalized;
                        
                        for (int r = 0; r < count; ++r)
                        {
                            var timeNormalizedSlice = timeStartNormalized + ((float)r / (float)(count - 1)) * interval;
                            list.Add (new FunctionTimed
                            {
                                timeNormalized = timeNormalizedSlice,
                                completed = false,
                                functions = UtilitiesYAML.CloneThroughYaml (functionTimed.functions)
                            });
                        }
                    }
                }
            }
            
            #endif
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public CombatActionCreateReaction () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
}