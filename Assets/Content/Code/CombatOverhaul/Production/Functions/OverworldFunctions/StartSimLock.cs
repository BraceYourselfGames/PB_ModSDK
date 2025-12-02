using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    public class StartSimLock : IOverworldFunction
    {
        public float duration = 12f;
        public string type = "resupply";

        public void Run ()
        {
            #if !PB_MODSDK

            var durationFinal = Mathf.Clamp (duration, 1f, 60f);
            var overworld = Contexts.sharedInstance.overworld;
            var time = overworld.simulationTime.f;
            overworld.ReplaceSimulationLockCountdown (time, durationFinal, type);
            
            #endif
        }
    }
    
    public class ApplySimLockEffect : IOverworldFunction
    {
        public string type = "resupply";
        public bool displayDialog = true;
        
        public void Run ()
        {
            #if !PB_MODSDK

            OverworldPointUtility.ApplySimLockEffects (type, displayDialog);
            
            #endif
        }
    }

    public class ImplementSimLockEffectCamp : IOverworldFunction
    {
        public void Run ()
        {
            #if !PB_MODSDK
            
            // Register rest, to lock ability to rest again till next combat
            // var basePersistent = IDUtility.playerBasePersistent;
            // basePersistent.SetMemoryFloat (EventMemoryIntAutofilled.World_Auto_RestCompleted, 1);

            // GameEventUtility.OnEvent (GameEventType.OverworldTimeSkipEndCamp);

            // This method includes refresh of all the warnings
            // OverworldUtility.RefreshPlayerCombatStats ();
            
            AchievementHelper.UnlockAchievement (AchievementKeys.FirstResupply);
            
            #endif
        }
    }
    
    public class ImplementSimLockEffectTravel : IOverworldFunction
    {
        public void Run ()
        {
            #if !PB_MODSDK
            
            // Reset rest availability
            // var basePersistent = IDUtility.playerBasePersistent;
            // basePersistent.SetMemoryFloat (EventMemoryIntAutofilled.World_Auto_RestCompleted, 0);
                
            // Resupply/retreat logic runs on end of travel, but no longer resets fatigue to 0: let's do it here
            var pilotEntities = PilotUtility.GetPilotsAtBase ();
            foreach (var pilot in pilotEntities)
            {
                var hpMax = pilot.GetPilotStatMax (PilotStatKeys.hp);
                var hp = pilot.GetPilotStat (PilotStatKeys.hp);
                    
                if (hp < hpMax)
                    pilot.SetPilotStat (PilotStatKeys.hp, hpMax);
                    
                int fatigue = pilot.GetPilotStatRounded (PilotStatKeys.fatigue);
                if (fatigue > 0)
                    pilot.SetPilotStat (PilotStatKeys.fatigue, 0);
                    
                pilot.SetPilotStat (PilotStatKeys.score, 0);
            }
                
            var units = UnitUtilities.GetUnitsAtBase ();
            foreach (var unit in units)
            {
                if (unit != null && !unit.isDestroyed && unit.hasUnitFrameIntegrity)
                {
                    float frameIntegrity = unit.unitFrameIntegrity.f;
                    if (frameIntegrity < 1f)
                        unit.ReplaceUnitFrameIntegrity (1f);
                }
            }

            var basePersistent = IDUtility.playerBasePersistent;
            
            int campSuppliesMinimum = DataHelperProvince.GetCampCost ();
            int campSupplies = Mathf.RoundToInt (EquipmentUtility.GetResourceInInventory (basePersistent, ResourceKeys.campSupplies));
            if (campSupplies < campSuppliesMinimum)
            {
                Debug.Log ($"Resupply: Changing camp supplies from {campSupplies} to {campSuppliesMinimum}");
                EquipmentUtility.SetResourceInInventory (basePersistent, ResourceKeys.campSupplies, campSuppliesMinimum);
            }
            
            // Replenish roster if it's below minimum
            // This method includes refresh of all the warnings
            // OverworldUtility.RefreshPlayerCombatStats ();
            
            #endif
        }
    }
    
    public class ImplementSimLockEffectResupply : IOverworldFunction
    {
        public void Run ()
        {
            #if !PB_MODSDK
            
            var basePersistent = IDUtility.playerBasePersistent;
            // basePersistent.SetMemoryFloat (EventMemoryIntAutofilled.World_Auto_RestCompleted, 1);
            
            foreach (var kvp in DataMultiLinkerResource.data)
            {
                var resourceKey = kvp.Key;
                var resourceData = kvp.Value;

                if (resourceData.maxOnResupply)
                {
                    var limit = EquipmentUtility.GetResourceLimit (resourceData);
                    Debug.Log ($"Resupply: Trying to set resource {resourceKey} to limit {limit}");
                    EquipmentUtility.SetResourceInInventory (basePersistent, resourceKey, limit);
                }
                else if (resourceData.offsetOnResupply != null)
                {
                    var offset = resourceData.offsetOnResupply.f;

                    if (!string.IsNullOrEmpty (resourceData.basePartForResupply))
                    {
                        bool installed = BasePartUtility.IsPartInstalled (resourceData.basePartForResupply);
                        if (!installed)
                            continue;
                    }

                    Debug.Log ($"Resupply: Trying to offset resource {resourceKey} by {offset}");
                    EquipmentUtility.ModifyResourceInInventory (basePersistent, resourceKey, offset);
                }
            }

            bool resourceChanges = true;
            if (resourceChanges)
            {
                // var campSupplies = DifficultyUtility.GetMultiplier (DifficultyKeys.OverworldCampSupplies);
                int campCost = DataHelperProvince.GetCampCost ();
                int campSuppliesMinimum = campCost;
                int campSupplyDifficultyInput = Mathf.RoundToInt (DifficultyUtility.GetMultiplier (DifficultyKeys.OverworldCampSupplies));
                campSuppliesMinimum = Mathf.Max (0, campSuppliesMinimum + campSupplyDifficultyInput - 1);
                
                int campSupplies = Mathf.RoundToInt (EquipmentUtility.GetResourceInInventory (basePersistent, ResourceKeys.campSupplies));
                if (campSupplies < campSuppliesMinimum)
                {
                    // sb.Append ("\n");
                    // sb.Append ("- Restocked camp supplies");
                    // sb.Append (": ");
                    // sb.Append (campSuppliesMinimum - campSupplies);
                        
                    Debug.Log ($"Resupply: Changing camp supplies from {campSupplies} to {campSuppliesMinimum} | Base cost: {campCost} | Difficulty input: {campSupplyDifficultyInput}");
                    EquipmentUtility.SetResourceInInventory (basePersistent, ResourceKeys.campSupplies, campSuppliesMinimum);
                }
            }
            
            // Replenish roster if it's below minimum
            // This method includes refresh of all the warnings
            OverworldUtility.RefreshPlayerCombatStats ();
            
            var persistent = Contexts.sharedInstance.persistent;
            var s = persistent.playerCombatStats;

            var pilotsAtBase = PilotUtility.GetPilotsAtBase ();
            int pilotsPresent = 0;
            foreach (var pilot in pilotsAtBase)
            {
                if (pilot == null || pilot.isDestroyed || pilot.isWrecked)
                    continue;

                pilotsPresent += 1;
            }
            
            int pilotsToAdd = Mathf.Max (0, 2 - pilotsPresent);
            if (pilotsToAdd > 0)
            {
                for (int i = 0; i < pilotsToAdd; ++i)
                {
                    var typeKeyOverride = "rookie";
                    var nameInternalSafe = IDUtility.GetSafePersistentInternalName ("pilot_rec");
                    var pilot = PilotUtility.CreatePilotEntity
                    (
                        false,
                        Factions.player,
                        nameInternalSafe,
                        null,
                        true,
                        typeKeyOverride
                    );

                    if (pilot != null)
                    {
                        pilot.ReplaceEntityLinkPersistentParent (basePersistent.id.id);
                        Debug.Log ($"Resupply: Created new pilot {pilot.ToLog ()}");
                    }
                }
            }
            
            int workshopLevel = WorkshopUtility.GetWorkshopOutputLevel ();
            var unitsAtBase = UnitUtilities.GetUnitsAtBase ();
            int unitsPresent = 0;
            foreach (var unitPersistent in unitsAtBase)
            {
                if (unitPersistent == null || unitPersistent.isDestroyed || unitPersistent.isWrecked)
                    continue;

                unitsPresent += 1;
            }
            
            int unitsToAdd = Mathf.Max (0, 2 - unitsPresent);
            if (unitsToAdd > 0)
            {
                for (int i = 0; i < unitsToAdd; ++i)
                {
                    var nameInternalSafe = IDUtility.GetSafePersistentInternalName ("unit_new");
                    var unitPersistent = UnitUtilities.CreatePersistentUnit
                    (
                        nameInternalSafe,
                        "unit_mech",
                        basePersistent.nameInternal.s,
                        Factions.player,
                        installDefaultParts: true,
                        level: workshopLevel
                    );

                    DataHelperStats.RefreshStatCacheForUnit (unitPersistent);
                    Debug.Log ($"Resupply: Created new unit {unitPersistent.ToLog ()}");
                }
            }
            
            // This method includes refresh of all the warnings
            // OverworldUtility.RefreshPlayerCombatStats ();
                
            // var persistent = Contexts.sharedInstance.persistent;
            // persistent.isPlayerCombatReadinessChecked = true;
            // AchievementHelper.UnlockAchievement(Achievement.FirstResupply);

            // OverworldQuestUtility.AddRefreshContext (QuestStateRefreshContextGlobal.OnCampComplete);
            // GameEventUtility.OnEvent (GameEventType.OverworldTimeSkipEndResupply);
            
            #endif
        }
    }
    
    public class CompleteWorkshopQueue : IOverworldFunction
    {
        public void Run ()
        {
            #if !PB_MODSDK
            
            var overworldAction = Contexts.sharedInstance.overworldAction;
            var workshopActions = overworldAction.GetEntitiesWithDataKeyOverworldAction ("workshop_build");
            if (workshopActions != null && workshopActions.Count > 0)
            {
                int i = 0;
                foreach (var action in workshopActions)
                {
                    if (action.isCompleted || action.isCancelled || action.isDestroyed)
                        continue;

                    action.isCompleted = true;
                    i += 1;
                }
            }
            
            #endif
        }
    }
    
    public class OffsetTime : IOverworldFunction
    {
        [PropertyRange (0f, 48f)]
        public float time; 

        public bool holdCountdown = true;
        
        public void Run ()
        {
            #if !PB_MODSDK

            var overworld = Contexts.sharedInstance.overworld;
            float currentTime = overworld.simulationTime.f;
            float newTime = currentTime + Mathf.Max (time, 0f);
            overworld.ReplaceSimulationTime (newTime);
            
            PostprocessingHelper.SetBlackoutTarget (1f, true);
            PostprocessingHelper.SetBlackoutTarget (0f, false);

            if (holdCountdown && overworld.hasOverworldResetCountdown)
            {
                var c = overworld.overworldResetCountdown;
                float startTimeNew = Mathf.Min (c.startTime + Mathf.Max (0f, time), newTime);
                overworld.ReplaceOverworldResetCountdown (startTimeNew, c.duration);
            }

            #endif
        }
    }
}