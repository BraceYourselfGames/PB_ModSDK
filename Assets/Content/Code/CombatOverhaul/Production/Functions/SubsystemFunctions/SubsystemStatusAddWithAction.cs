using System;
using PhantomBrigade.Combat.Components;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class SubsystemStatusAddWithAction : ISubsystemFunctionAction
    {
        [ValueDropdown ("@DataMultiLinkerUnitStatus.data.Keys")]
        public string key = string.Empty;
        
        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerUnitStats.data.Keys")]
        [LabelText ("Duration Override Stat")]
        public string durationFullOverrideStat;
        
        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerUnitStats.data.Keys")]
        [LabelText ("Update Count Stat")]
        public string updateCountOverrideStat;

        public void OnPartEventAction (EquipmentEntity part, DataContainerSubsystem subsystemBlueprint, string context, ActionEntity action)
        {
            #if !PB_MODSDK

            if (action == null || !action.hasDuration || !action.hasStartTime)
                return;
            
            var unitCombat = action.hasActionOwner ? IDUtility.GetCombatEntity (action.actionOwner.combatID) : null;
            if (unitCombat == null)
                return;
            
            var unitPersistent = IDUtility.GetLinkedPersistentEntity (unitCombat);
            if (unitPersistent == null)
                return;

            var definition = DataMultiLinkerUnitStatus.GetEntry (key);
            if (definition == null)
                return;

            var combat = Contexts.sharedInstance.combat;
            var timeSimulation = combat.hasSimulationTime ? combat.simulationTime.f : 0f;
            var timeLocal = timeSimulation - action.startTime.f;
            var durationFullOverride = action.duration.f - timeLocal;

            if (!string.IsNullOrEmpty (durationFullOverrideStat))
                durationFullOverride = DataHelperStats.GetCachedStatForUnit (durationFullOverrideStat, unitPersistent);
            
            if (durationFullOverride <= 0f)
                return;

            float durationUpdateOverride = -1f;
            if (!string.IsNullOrEmpty (updateCountOverrideStat))
            {
                int updateCount = Mathf.RoundToInt (DataHelperStats.GetCachedStatForUnit (updateCountOverrideStat, unitPersistent));
                if (updateCount > 0)
                    durationUpdateOverride = 1f / updateCount;
            }
            
            UnitStatusUtility.AddStatus (unitCombat, key, UnitStatusSource.Equipment, durationFullOverride, durationUpdateOverride);
            
            #endif
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public SubsystemStatusAddWithAction () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
}