using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    public class SubsystemRetargetFilterRange
    {
        public string socketStatSource;
        public Vector2 range;
    }
    
    public class SubsystemRetargetFilterCone
    {
        public float dotMin;
    }
    
    [Serializable]
    public class SubsystemRetargetProjectiles : ISubsystemFunctionTargeted
    {
        public DataBlockScenarioSubcheckUnit unitCheck;
        public SubsystemRetargetFilterRange range;
        public SubsystemRetargetFilterCone cone;
        
        // public DataBlockInt targetProjectileCount;
        // public DataBlockInt targetLimit;

        private static List<CombatEntity> unitTargetsAll = new List<CombatEntity> ();
        // private static List<CombatEntity> unitTargetsTrimmed = new List<CombatEntity> ();
        // private static Dictionary<int, int> unitTargetUsage = new Dictionary<int, int> ();

        public void OnPartEventTargeted (EquipmentEntity part, DataContainerSubsystem subsystemBlueprint, string context, Vector3 position, Vector3 direction, Vector3 targetPosition, CombatEntity targetUnitCombat, CombatEntity projectile)
        {
            #if !PB_MODSDK

            if (projectile == null)
                return;
            
            // Remove target so that any bailouts don't leave original guidance in place
            if (projectile.hasProjectileTargetEntity)
            {
                var targetEntity = IDUtility.GetCombatEntity (projectile.projectileTargetEntity.combatID);
                if (targetEntity != null)
                {
                    var targetPos = targetEntity.GetCenterPoint ();
                    
                    projectile.RemoveProjectileTargetEntity ();
                    projectile.ReplaceProjectileTargetPosition (targetPos);
                }
            }

            if (part == null)
            {
                Debug.LogWarning ($"SubsystemRetargetProjectiles | Subsystem {subsystemBlueprint} | Failed to find parent part");
                return;
            }
            
            var unitOwnerPersistent = part != null && part.hasPartParentUnit ? IDUtility.GetPersistentEntity (part.partParentUnit.persistentID) : null;
            var unitOwnerCombat = IDUtility.GetLinkedCombatEntity (unitOwnerPersistent);
            if (unitOwnerPersistent == null)
            {
                Debug.LogWarning ($"SubsystemRetargetProjectiles | Subsystem {subsystemBlueprint} | Part {part.ToLog ()} | Failed to find persistent unit owner");
                return;
            }

            bool unitOwnerFriendly = CombatUIUtility.IsUnitFriendly (unitOwnerPersistent);
            bool unitTargetFriendly = !unitOwnerFriendly;
            
            if (unitOwnerCombat == null || !unitOwnerCombat.hasPosition || !unitOwnerCombat.hasDirectionTargetPrimary)
            {
                Debug.LogWarning ($"SubsystemRetargetProjectiles | Subsystem {subsystemBlueprint} | Part {part.ToLog ()} | Unit: {unitOwnerPersistent.ToLog ()} | Failed to find combat unit owner");
                return;
            }
            
            var unitOwnerPos = unitOwnerCombat.position.v;
            var unitOwnerDir = unitOwnerCombat.directionTargetPrimary.v;

            bool rangeChecked = range != null;
            Vector2 rangeMinMax = new Vector2 (1f, 1000f);
            if (rangeChecked)
            {
                if (!string.IsNullOrEmpty (range.socketStatSource))
                {
                    var partInSocket = EquipmentUtility.GetPartInUnit (unitOwnerPersistent, range.socketStatSource);
                    if (partInSocket != null)
                    {
                        float rangeMin = DataHelperStats.GetCachedStatForPart (UnitStats.weaponRangeMin, partInSocket);
                        float rangeMax = DataHelperStats.GetCachedStatForPart (UnitStats.weaponRangeMax, partInSocket);
                        rangeMinMax = new Vector2 (range.range.x + rangeMin, range.range.y + rangeMax);
                    }
                    else 
                        rangeMinMax = new Vector2 (range.range.x, range.range.y);
                }
                else
                    rangeMinMax = new Vector2 (range.range.x, range.range.y);
            }

            bool dotChecked = cone != null;
            float dotMin = -1f;
            if (dotChecked)
                dotMin = cone.dotMin;

            unitTargetsAll.Clear ();

            var unitParticipants = ScenarioUtility.GetParticipantUnitsPersistent ();
            foreach (var unitPersistent in unitParticipants)
            {
                if (unitPersistent.isHidden || !unitPersistent.isUnitDeployed || unitPersistent.isDestroyed || unitPersistent.isWrecked)
                    continue;

                var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
                if (!ScenarioUtility.IsUnitActive (unitPersistent, unitCombat, false))
                    continue;
            
                bool friendly = CombatUIUtility.IsFactionFriendly (unitPersistent.faction.s);
                if (friendly != unitTargetFriendly)
                    continue;

                if (!ScenarioUtility.IsUnitMatchingCheck (unitPersistent, unitCombat, unitCheck, true, true))
                    continue;
                
                if (unitCombat.hasCurrentDashAction)
                    continue;

                if (rangeChecked)
                {
                    var distance = Vector3.Distance (unitCombat.position.v, unitOwnerPos);
                    if (distance < rangeMinMax.x || distance > rangeMinMax.y)
                        continue;
                }

                if (dotChecked)
                {
                    var directionCandidate = Vector3.Normalize (unitCombat.position.v - unitOwnerPos);
                    var dot = Vector3.Dot (unitOwnerDir, directionCandidate);
                    if (dot < dotMin)
                        continue;
                }
                
                unitTargetsAll.Add (unitCombat);
            }

            if (unitTargetsAll.Count == 0)
            {
                Debug.LogWarning ($"SubsystemRetargetProjectiles | Subsystem {subsystemBlueprint} | Part {part.ToLog ()} | Unit: {unitOwnerPersistent.ToLog ()} | Failed to find valid targets using a list of {unitParticipants.Count} candidates");
                return;
            }

            var unitTargetCombat = unitTargetsAll.GetRandomEntry ();
            var unitTargetCombatID = unitTargetCombat.id.id;
                
            projectile.ReplaceProjectileTargetEntity (unitTargetCombatID);
            projectile.ReplaceProjectileTargetPosition (unitTargetCombat.position.v);
            
            Debug.LogWarning ($"SubsystemRetargetProjectiles | Subsystem {subsystemBlueprint} | Part {part.ToLog ()} | Unit: {unitOwnerPersistent.ToLog ()} | Projectile {projectile.ToLog ()} now targets unit {unitTargetCombat.ToLog ()} ({unitParticipants.Count} candidates)");
            
            #endif
        }

        /*
        public void OnPartEventAction (EquipmentEntity subsystem, string context, ActionEntity action)
        {
            if (action == null || !action.hasDataKeyAction || !action.hasActionOwner)
                return;
            
            var combat = Contexts.sharedInstance.combat;
            var projectiles = combat.GetEntitiesWithParentAction (action.id.id);
            if (projectiles == null || projectiles.Count == 0)
            {
                Debug.LogWarning ($"SubsystemRetargetProjectiles | Action {action.ToLog ()} | Failed to find any related projectiles");
                return;
            }
            
            var part = subsystem != null && subsystem.hasSubsystemParentPart ? IDUtility.GetEquipmentEntity (subsystem.subsystemParentPart.equipmentID) : null;
            if (part == null)
            {
                Debug.LogWarning ($"SubsystemRetargetProjectiles | Action {action.ToLog ()} | Subsystem {subsystem.ToLog ()} | Failed to find parent part");
                return;
            }
            
            var unitOwnerPersistent = part != null && part.hasPartParentUnit ? IDUtility.GetPersistentEntity (part.partParentUnit.persistentID) : null;
            var unitOwnerCombat = IDUtility.GetLinkedCombatEntity (unitOwnerPersistent);
            if (unitOwnerPersistent == null)
            {
                Debug.LogWarning ($"SubsystemRetargetProjectiles | Action {action.ToLog ()} | Subsystem {subsystem.ToLog ()} | Part {part.ToLog ()} | Failed to find persistent unit owner");
                return;
            }

            bool unitOwnerFriendly = CombatUIUtility.IsUnitFriendly (unitOwnerPersistent);
            bool unitTargetFriendly = !unitOwnerFriendly;
            
            if (unitOwnerCombat == null || !unitOwnerCombat.hasPosition || !unitOwnerCombat.hasDirectionTargetPrimary)
            {
                Debug.LogWarning ($"SubsystemRetargetProjectiles | Action {action.ToLog ()} | Subsystem {subsystem.ToLog ()} | Part {part.ToLog ()} | Unit: {unitOwnerPersistent.ToLog ()} | Failed to find combat unit owner");
                return;
            }
            var unitOwnerPos = unitOwnerCombat.position.v;
            var unitOwnerDir = unitOwnerCombat.directionTargetPrimary.v;

            bool rangeChecked = range != null;
            Vector2 rangeMinMax = new Vector2 (1f, 1000f);
            if (rangeChecked)
            {
                if (range.statsUsed)
                {
                    float rangeMin = DataHelperStats.GetCachedStatForPart (UnitStats.weaponRangeMin, part);
                    float rangeMax = DataHelperStats.GetCachedStatForPart (UnitStats.weaponRangeMax, part);
                    rangeMinMax = new Vector2 (range.range.x + rangeMin, range.range.y + rangeMax);
                }
                else
                    rangeMinMax = new Vector2 (range.range.x, range.range.y);
            }

            bool dotChecked = cone != null;
            float dotMin = -1f;
            if (dotChecked)
                dotMin = cone.dotMin;

            unitTargetsAll.Clear ();

            var unitParticipants = ScenarioUtility.GetCombatParticipantUnits ();
            foreach (var unitPersistent in unitParticipants)
            {
                if (unitPersistent.isHidden || !unitPersistent.isUnitDeployed || unitPersistent.isDestroyed || unitPersistent.isWrecked)
                    continue;

                var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
                if (!ScenarioUtility.IsUnitActive (unitPersistent, unitCombat, false))
                    continue;
            
                bool friendly = CombatUIUtility.IsFactionFriendly (unitPersistent.faction.s);
                if (friendly != unitTargetFriendly)
                    continue;

                if (!ScenarioUtility.IsUnitMatchingCheck (unitPersistent, unitCombat, unitCheck, true, true))
                    continue;
                
                if (unitCombat.hasCurrentDashAction)
                    continue;

                if (rangeChecked)
                {
                    var distance = Vector3.Distance (unitCombat.position.v, unitOwnerPos);
                    if (distance < rangeMinMax.x || distance > rangeMinMax.y)
                        continue;
                }

                if (dotChecked)
                {
                    var direction = Vector3.Normalize (unitCombat.position.v - unitOwnerPos);
                    var dot = Vector3.Dot (unitOwnerDir, direction);
                    if (dot < dotMin)
                        continue;
                }
                
                unitTargetsAll.Add (unitCombat);
            }

            if (unitTargetsAll.Count == 0)
            {
                Debug.LogWarning ($"SubsystemRetargetProjectiles | Action {action.ToLog ()} | Subsystem {subsystem.ToLog ()} | Part {part.ToLog ()} | Unit: {unitOwnerPersistent.ToLog ()} | Failed to find valid targets using a list of {unitParticipants.Count} candidates");
                return;
            }

            // If there are too many targets and there is a limit on how many projectiles get split, trim the list
            if (targetLimit != null && targetLimit.i > 0 && unitTargetsAll.Count > targetLimit.i)
            {
                while (unitTargetsAll.Count > targetLimit.i)
                    unitTargetsAll.RemoveRandomEntry ();
            }

            int projectilesAllowedPerUnit = 0;
            if (targetProjectileCount != null)
                projectilesAllowedPerUnit = targetProjectileCount.i;

            unitTargetUsage.Clear ();
            unitTargetsTrimmed.Clear ();
            unitTargetsTrimmed.AddRange (unitTargetsAll);

            foreach (var projectile in projectiles)
            {
                var unitTargetCombat = unitTargetsTrimmed.GetRandomEntry ();
                var unitTargetCombatID = unitTargetCombat.id.id;
                
                projectile.ReplaceProjectileTargetEntity (unitTargetCombatID);
                projectile.ReplaceProjectileTargetPosition (unitTargetCombat.position.v);

                if (!unitTargetUsage.ContainsKey (unitTargetCombatID))
                    unitTargetUsage.Add (unitTargetCombatID, 1);
                else
                    unitTargetUsage[unitTargetCombatID] += 1;

                // If limit is checked and usage of this target is now at limit (e.g. limit is 2 and this was the second time a projectile got targeted to this unit)
                if (projectilesAllowedPerUnit > 0 && unitTargetUsage[unitTargetCombatID] >= projectilesAllowedPerUnit)
                {
                    // Remove the given unit from list of possible targets so that no more projectiles get allocated to it
                    unitTargetsTrimmed.Remove (unitTargetCombat);
                    
                    // However, if we're down to no remaining units, the limit should be raised by 1 and the list reset, allowing equal distribution of overflowing projectiles
                    if (unitTargetsTrimmed.Count == 0)
                    {
                        unitTargetsTrimmed.AddRange (unitTargetsAll);
                        projectilesAllowedPerUnit += 1;
                    }
                }
            }
        }
        */
    }
}