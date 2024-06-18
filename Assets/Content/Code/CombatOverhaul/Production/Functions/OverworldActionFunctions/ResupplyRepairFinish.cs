using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Game;
using PhantomBrigade.Overworld;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class ResupplyRepairFinish : IOverworldActionFunction
    {
        // Simple hardcoded costs based on prt_item_assault_01 and prt_body_elbrus_m
        private static Dictionary<string, int> costPerSocket = new Dictionary<string, int>
        {
            { LoadoutSockets.corePart, 130 },
            { LoadoutSockets.secondaryPart, 90 },
            { LoadoutSockets.leftOptionalPart, 70 },
            { LoadoutSockets.rightOptionalPart, 70 },
            { LoadoutSockets.leftEquipment, 0 },
            { LoadoutSockets.rightEquipment, 250 }
        };

        private static Dictionary<string, string> partsToAdd = new Dictionary<string, string> ();

        public void Run (OverworldActionEntity source)
        {
            #if !PB_MODSDK

            var playerBasePersistent = IDUtility.playerBasePersistent;
            if (playerBasePersistent == null)
                return;

            DataHelperLoading.UpdateTimeStamp (TimeStampKeys.ResupplyEnd);

            // Maximize battery
            // var statEnergyLimit = BasePartUtility.GetBaseStat (BaseStatKeys.EnergyLimit);
            // EquipmentUtility.SetResourceInInventory (playerBasePersistent, ResourceKeys.battery, statEnergyLimit, false);

            // Maximize repair resource
            // var statRepairResourceLimit = BasePartUtility.GetBaseStat (BaseStatKeys.UnitRepairResourceLimit);
            // EquipmentUtility.SetResourceInInventory (playerBasePersistent, ResourceKeys.repairJuice, statRepairResourceLimit, false);

            foreach (var kvp in DataMultiLinkerResource.data)
            {
                var resourceKey = kvp.Key;
                var resourceData = kvp.Value;

                if (resourceData.maxOnResupply)
                {
                    var limit = EquipmentUtility.GetResourceLimit (resourceData);
                    Debug.Log ($"Resupply: Trying to set resource {resourceKey} to limit {limit}");
                    EquipmentUtility.SetResourceInInventory (playerBasePersistent, resourceKey, limit);
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
                    EquipmentUtility.ModifyResourceInInventory (playerBasePersistent, resourceKey, offset);
                }
            }

            // Restore pilot state
            var persistent = Contexts.sharedInstance.persistent;
            var pilotGroup = persistent.GetGroup (PersistentMatcher.AllOf (PersistentMatcher.PilotTag, PersistentMatcher.PilotHealth));
            var pilotEntities = pilotGroup.GetEntities ();

            foreach (var pilot in pilotEntities)
            {
                var healthLimit = pilot.pilotStats.healthLimit;
                pilot.ReplacePilotHealth (healthLimit);
                UnitUtilities.UpdatePilotConcussionInputs (pilot);
            }

            // Repair all units
            var basePersistent = IDUtility.playerBasePersistent.id.id;
            var entitiesPersistent = persistent.GetEntitiesWithEntityLinkPersistentParent (basePersistent);

            int suppliesSpent = 0;
            int unitLevel = persistent.workshopLevel.level;

            foreach (var unit in entitiesPersistent)
            {
                if (unit == null || !unit.isUnitTag || unit.isDestroyed || unit.isWrecked || !unit.hasDataLinkUnitBlueprint)
                    continue;

                var unitBlueprint = unit.dataLinkUnitBlueprint.data;
                var sockets = unitBlueprint.sockets;
                var parts = EquipmentUtility.GetPartsInUnit (unit);
                partsToAdd.Clear ();

                foreach (var kvp in sockets)
                {
                    var socketKey = kvp.Key;
                    var socketData = kvp.Value;
                    var part = EquipmentUtility.GetPartInUnit (unit, socketKey, false, parts);

                    if (part != null)
                    {
                        // If part is present and damageable, ensure it's fully repaired
                        if (part.IsPartTaggedAs (EquipmentTags.damageable))
                        {
                            float integrityMax = DataHelperStats.GetCachedStatForPart (UnitStats.hp, part);
                            float barrierMax = DataHelperStats.GetCachedStatForPart (UnitStats.barrier, part);

                            bool integrityDamaged = integrityMax > 0f && part.integrityNormalized.f < 1f;
                            if (integrityDamaged)
                                part.ReplaceIntegrityNormalized (1f);

                            bool barrierDamaged = barrierMax > 0f && part.barrierNormalized.f < 1f;
                            if (barrierDamaged)
                                part.ReplaceBarrierNormalized (1f);
                        }
                    }
                    else
                    {
                        bool partShouldBePresent = !string.IsNullOrEmpty (socketData.presetDefault);
                        if (partShouldBePresent)
                        {
                            int cost = costPerSocket.ContainsKey (socketKey) ? costPerSocket[socketKey] : 0;
                            suppliesSpent += cost;

                            Debug.Log ($"Unit {unit.ToLog ()} has no part in socket {socketKey}, restoring it | Cost: +{cost} | Total cost: {suppliesSpent}");
                            partsToAdd.Add (socketKey, socketData.presetDefault);
                        }
                    }
                }

                // Iterate on added parts separately to avoid invalidation of iterator
                foreach (var kvp in partsToAdd)
                {
                    var socketKey = kvp.Key;
                    var presetKey = kvp.Value;

                    var partPreset = DataMultiLinkerPartPreset.GetEntry (presetKey);
                    if (partPreset == null)
                        continue;

                    var part = UnitUtilities.CreatePartEntityFromPreset (partPreset, level: unitLevel, rating: 1);
                    EquipmentUtility.AttachPartToUnit (part, unit, socketKey, true, refreshUnitStatCache: false);
                }

                if (unit.hasRepairStatus)
                    unit.RemoveRepairStatus ();
            }

            // Replenish roster if it's below minimum
            // This method includes refresh of all the warnings
            OverworldUtility.RefreshPlayerCombatStats ();
            var s = persistent.playerCombatStats;

            int pilotsToAdd = Mathf.Max (0, 2 - s.pilotCountReady);
            if (pilotsToAdd > 0)
            {
                for (int i = 0; i < pilotsToAdd; ++i)
                {
                    var nameInternalSafe = UnitUtilities.GetSafePersistentInternalName ("pilot");
                    var pilot = UnitUtilities.CreatePilotEntity
                    (
                        false,
                        Factions.player,
                        nameInternalSafe,
                        null,
                        render: true
                    );

                    pilot.ReplaceEntityLinkPersistentParent (playerBasePersistent.id.id);
                }
            }

            int unitsToAdd = Mathf.Max (0, 2 - s.unitCountReady);
            if (unitsToAdd > 0)
            {
                for (int i = 0; i < unitsToAdd; ++i)
                {
                    var nameInternalSafe = UnitUtilities.GetSafePersistentInternalName ("unit_new");
                    var unitPersistent = UnitUtilities.CreatePersistentUnit
                    (
                        nameInternalSafe,
                        "unit_mech",
                        playerBasePersistent.nameInternal.s,
                        Factions.player,
                        installDefaultParts: true,
                        level: unitLevel
                    );

                    DataHelperStats.RefreshStatCacheForUnit (unitPersistent);
                }
            }

            if (suppliesSpent > 0)
            {
                int suppliesCurrent = EquipmentUtility.GetResourceInInventoryRounded (playerBasePersistent, ResourceKeys.supplies);
                int suppliesFinal = Mathf.Max (100, suppliesCurrent - suppliesSpent);
                EquipmentUtility.SetResourceInInventory (playerBasePersistent, ResourceKeys.supplies, suppliesFinal);
                Debug.Log ($"Subtracted {suppliesSpent} from the current amount (up to a limit): {suppliesCurrent} - {suppliesSpent} > {suppliesFinal}");
            }
            
            if (unitsToAdd > 0 || pilotsToAdd > 0)
                Debug.LogWarning ($"Created {unitsToAdd} units and {pilotsToAdd} pilots to allow the player to continue");

            CIViewOverworldRoster.ins.RefreshOptionAvailability ();
            persistent.isPlayerCombatReadinessChecked = true;
            AchievementHelper.UnlockAchievement(Achievement.FirstResupply);
            
            GameEventUtility.OnEventGeneral (GameEventGeneral.OverworldResupplyEnd);
            
            #endif
        }
    }
}