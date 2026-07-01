using System;
using System.Collections.Generic;
using PhantomBrigade.Combat;
using PhantomBrigade.Combat.Components;
using PhantomBrigade.Data;
#if !PB_MODSDK
using RootMotion.Dynamics;
#endif
using Sirenix.OdinInspector;
using UnityEngine;
using Random = System.Random;

namespace PhantomBrigade.Functions
{
    public class CombatUnitIntegrityChange : ICombatFunctionTargeted
    {
        public float value;
        public bool offset;

        [ValueDropdown ("@DataMultiLinkerPartSocket.data.Keys")]
        public HashSet<string> sockets;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            UnitUtilities.ModifyCombatIntegrityNormalized (unitPersistent, value, offset, sockets);

            #endif
        }
    }
    
    public class CombatUnitIntegrityChangeRandom : ICombatFunctionTargeted
    {
        public Vector2 range;
        public bool offset;
        public bool randomPerPart;

        [ValueDropdown ("@DataMultiLinkerPartSocket.data.Keys")]
        public HashSet<string> sockets;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            UnitUtilities.ModifyCombatIntegrityNormalized (unitPersistent, range, offset, sockets, randomPerPart);

            #endif
        }
    }
    
    public class CombatUnitRevive : ICombatFunctionTargeted
    {
        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null)
                return;
            
            if (!unitPersistent.isWrecked)
            {
                Debug.LogWarning ($"Failed to revive unit {unitPersistent.ToLog ()}: not wrecked");
                return;
            }
            
            Debug.Log ($"Reviving unit {unitPersistent.ToLog ()}");
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            var uvm = unitCombat.combatView.view.visualManager;
            var parts = EquipmentUtility.GetPartsInUnit (unitPersistent);
            
            foreach (var part in parts)
            {
                if (part == null || part.isDestroyed)
                    continue;
                
                part.isWrecked = false;
                part.isFunctional = true;
                
                if (part.hasDestructionTime)
                    part.RemoveDestructionTime ();
                
                if (part.hasDestructionProgress)
                    part.RemoveDestructionProgress ();
                    
                part.ReplaceIntegrityNormalized (1f);
                    
                float barrierMax = DataHelperStats.GetCachedStatForPart (UnitStats.barrier, part);
                if (barrierMax > 0f)
                    part.ReplaceBarrierNormalized (1f);

                var socket = part.partParentUnit.socket;
                uvm.OnIntegrityChange (socket, 1f);
                uvm.OnSocketDestructionChange (socket, 0f);
            }
            
            UnitUtilities.ModifyCombatIntegrityNormalized (unitPersistent, 1f, false, null);

            unitPersistent.isWrecked = false;
            unitPersistent.isFunctional = true;
            
            uvm.OnUnitRevival ();
            
            if (unitCombat.hasCrumpleTime)
                unitCombat.RemoveCrumpleTime ();

            if (unitCombat.hasExecutionHistory)
            {
                var history = unitCombat.executionHistory;
                history.flags.Clear ();
                history.values.Clear ();
                unitCombat.RefreshExecutionHistory (true);
            }
            
            CIHelperWorldMarkers.OnUnitChange (unitCombat);
            CIHelperOverlays.OnUnitEligibilityChange (unitPersistent);
            CIHelperOverlays.OnHistoryRefresh (unitCombat);
            
            CombatUtilities.AddScenarioStateRefreshContext (ScenarioStateRefreshContext.OnUnitDisabled);
            UnitStatusUtility.RemoveAllStatus (unitCombat);

            var combat = Contexts.sharedInstance.combat;
            int selectionID = combat.hasUnitSelected ? combat.unitSelected.id : IDUtility.invalidID;
            if (unitCombat.id.id == selectionID)
                CIViewCombatTimeline.ins.OnSelectedUnitChange (unitPersistent.id.id);
            
            CIViewCombatPopups.AddPopup (CombatPopupType.hmCrash, true, unitCombat);
            CIViewCombatNav.AddMessageEventUnit (unitCombat, Txt.Get (TextLibs.uiPause, "difficulty_screen_reset"));
            
            UnitUtilities.OnUnitGetUp (unitCombat);

            #endif
        }
    }
}