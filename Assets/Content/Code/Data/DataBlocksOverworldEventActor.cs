using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PhantomBrigade.Functions;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;
using static PhantomBrigade.Data.DataBlockOverworldEventActorWorldCheck;

namespace PhantomBrigade.Data
{
    public enum ActorSource
    {
        Self,
        Target
    }
    
    public enum ActorPick
    {
        Random,
        First,
        Last
    }
    
    public enum ActorUnitSort
    {
        ID,
        Name,
        Level
    }
    
    public enum ActorPilotSort
    {
        ID,
        Name,
        Health
    }

    public static class DebugHelper
    {
        public static bool logToConsole = false;
    
        public static void Log (string message, bool logToConsoleEnd = false)
        {
            Debug.Log (message);
        }
        
        public static void LogWarning (string message, bool logToConsoleEnd = false)
        {
            Debug.LogWarning (message);
        }
    }

    public static class OverworldEventUtility
    {
        private static StringBuilder sbDev = new StringBuilder ();

        public static string PrintStepDescription (DataBlockOverworldEventStep step, bool expanded = true)
        {
            if (step == null)
                return string.Empty;
            
            sbDev.Clear ();
            
            if (expanded)
            {
                sbDev.Append ("[aa]Steps can apply a variety of effects when entered. Additionally, steps define the options available to the player.[ff]");
                sbDev.Append ("\n\n[aa]Mood: [ff]");
                sbDev.Append (step.eventMood);
            }

            if (step.hopeChange != null)
            {
                if (sbDev.Length > 0)
                    sbDev.Append ("\n\n");
                sbDev.Append ("[aa]Hope change: [ff]\n- Offset by ");
                sbDev.Append (step.hopeChange.offset.ToString ("0.##"));
            }
            
            if (step.warScoreChange != null)
            {
                if (sbDev.Length > 0)
                    sbDev.Append ("\n\n");
                sbDev.Append ("[aa]War score change: [ff]\n- ");
                sbDev.Append (step.warScoreChange.faction);
                sbDev.Append (" offset by ");
                sbDev.Append (step.warScoreChange.offset.ToString ("0.##"));
            }

            if (step.actorRefresh != null && (step.actorRefresh.refreshWorld || step.actorRefresh.refreshUnits || step.actorRefresh.refreshPilots))
            {
                if (sbDev.Length > 0)
                    sbDev.Append ("\n\n");
                sbDev.Append ("[aa]Actor refresh: [ff]");
                
                if (step.actorRefresh.refreshWorld)
                    sbDev.Append ("\n- World actors refreshed");
                
                if (step.actorRefresh.refreshUnits)
                    sbDev.Append ("\n- Unit actor refreshed");
                
                if (step.actorRefresh.refreshPilots)
                    sbDev.Append ("\n- Pilot actors refreshed");
            }
            
            if (step.memoryChanges != null && step.memoryChanges.Count > 0)
            {
                if (sbDev.Length > 0)
                    sbDev.Append ("\n\n");
                sbDev.Append ("[aa]Memory changes: [ff]");

                foreach (var group in step.memoryChanges)
                {
                    if (group == null || group.changes == null || group.changes.Count == 0)
                        continue;
                
                    sbDev.Append ("\n- ");
                    sbDev.Append (logLookupMemoryChangeContexts[group.context]);

                    if (group.context == MemoryChangeContextEvent.ActorWorld || group.context == MemoryChangeContextEvent.ActorUnit || group.context == MemoryChangeContextEvent.ActorPilot)
                    {
                        sbDev.Append (" (");
                        sbDev.Append (group.actorKey);
                        sbDev.Append (")");
                    }
                    else if (group.context == MemoryChangeContextEvent.SpecificProvince)
                    {
                        sbDev.Append (" (");
                        sbDev.Append (group.provinceKey);
                        sbDev.Append (")");
                    }

                    sbDev.Append (": ");
                    foreach (var change in group.changes)
                    {
                        sbDev.Append ("\n  - ");
                        sbDev.Append (change.ToString ());
                    }
                }
            }

            if (step.functions != null && step.functions.Count > 0)
            {
                if (sbDev.Length > 0)
                    sbDev.Append ("\n\n");
                sbDev.Append ("[aa]Function calls: [ff]");

                foreach (var function in step.functions)
                {
                    if (function == null)
                        continue;

                    sbDev.Append ("\n- ");
                    sbDev.Append (function.GetType ().Name);

                    if (function is IOverworldFunctionLog functionLogged)
                    {
                        sbDev.Append (": ");
                        sbDev.Append (functionLogged.ToLog ());
                    }
                }
            }

            if (step.options != null)
            {
                if (sbDev.Length > 0)
                {
                    if (expanded)
                        sbDev.Append ("\n[aa]___[ff]");
                    sbDev.Append ("\n\n");
                }
                sbDev.Append ("[aa]Options in this step: [ff]");
                foreach (var link in step.options)
                {
                    sbDev.Append ("\n- ");
                    sbDev.Append (link.key);
                    sbDev.Append (link.shared ? " [aa](shared)[ff]" : " [aa](embedded)[ff]");
                }
            }
            
            // ...

            var output = sbDev.ToString ();
            if (!expanded)
            {
                output = output.Replace ("[aa]", string.Empty);
                output = output.Replace ("[ff]", string.Empty);
            }

            return output;
        }
        
        public static string PrintOptionDescription (DataContainerOverworldEventOption option, bool expanded = true)
        {
            if (option == null)
                return string.Empty;
            
            sbDev.Clear ();

            if (expanded)
            {
                sbDev.Append ("[aa]Options can apply a variety of side effects when selected. They can also lead to another event step.[ff]");

                sbDev.Append ("\n\n[aa]Priority: [ff]");
                sbDev.Append (option.priority);

                sbDev.Append ("\n[aa]Mood: [ff]");
                sbDev.Append (option.optionMood);

                if (option.check != null)
                {
                    sbDev.Append ("\n[aa]Hidden if check failed: [ff]");
                    sbDev.Append (option.checkPreventsUnlock ? "Yes" : "No");
                }

                if (option.injection != null)
                {
                    sbDev.Append ("\n\n[aa]Injected option | Checks: [ff]");

                    if (option.injection.optionCombatPresent != null)
                    {
                        sbDev.Append ("\n- Combat entry option: ");
                        sbDev.Append (option.injection.optionCombatPresent.v ? "required" : "unwanted");
                    }

                    if (option.injection.optionExitPresent != null)
                    {
                        sbDev.Append ("\n- Exit option: ");
                        sbDev.Append (option.injection.optionExitPresent.v ? "required" : "unwanted");
                    }

                    if (option.injection.optionKeysFilter != null && option.injection.optionKeysFilter.Count > 0)
                    {
                        sbDev.Append ("\n- Specific existing options: ");
                        foreach (var kvp in option.injection.optionKeysFilter)
                        {
                            sbDev.Append ("\n  - ");
                            sbDev.Append (kvp.Key);
                            sbDev.Append (": ");
                            sbDev.Append (kvp.Value ? "required" : "unwanted");
                        }
                    }

                    if (option.injection.eventKeysCompatible != null && option.injection.eventKeysCompatible.Count > 0)
                    {
                        sbDev.Append ("\n- Compatible events: ");
                        foreach (var eventKey in option.injection.eventKeysCompatible)
                        {
                            sbDev.Append ("\n  - ");
                            sbDev.Append (eventKey);
                        }
                    }

                    if (option.injection.eventKeysBlocked != null && option.injection.eventKeysBlocked.Count > 0)
                    {
                        sbDev.Append ("\n- Incompatible events: ");
                        foreach (var eventKey in option.injection.eventKeysBlocked)
                        {
                            sbDev.Append ("\n  - ");
                            sbDev.Append (eventKey);
                        }
                    }
                }
            }

            if (option.hopeChange != null)
            {
                if (sbDev.Length > 0)
                    sbDev.Append ("\n\n");
                
                sbDev.Append ("[aa]Hope change: [ff]\n- Offset by ");
                sbDev.Append (option.hopeChange.offset.ToString ("0.##"));
            }
            
            if (option.warScoreChange != null)
            {
                if (sbDev.Length > 0)
                    sbDev.Append ("\n\n");
                
                sbDev.Append ("[aa]War score change: [ff]\n- ");
                sbDev.Append (option.warScoreChange.faction);
                sbDev.Append (" offset by ");
                sbDev.Append (option.warScoreChange.offset.ToString ("0.##"));
            }

            if (option.memoryChanges != null && option.memoryChanges.Count > 0)
            {
                if (sbDev.Length > 0)
                    sbDev.Append ("\n\n");
                
                sbDev.Append ("[aa]Memory changes: [ff]");

                foreach (var group in option.memoryChanges)
                {
                    if (group == null || group.changes == null || group.changes.Count == 0)
                        continue;
                
                    sbDev.Append ("\n- ");
                    sbDev.Append (logLookupMemoryChangeContexts[group.context]);

                    if (group.context == MemoryChangeContextEvent.ActorWorld || group.context == MemoryChangeContextEvent.ActorUnit || group.context == MemoryChangeContextEvent.ActorPilot)
                    {
                        sbDev.Append (" (");
                        sbDev.Append (group.actorKey);
                        sbDev.Append (")");
                    }
                    else if (group.context == MemoryChangeContextEvent.SpecificProvince)
                    {
                        sbDev.Append (" (");
                        sbDev.Append (group.provinceKey);
                        sbDev.Append (")");
                    }

                    sbDev.Append (": ");
                    foreach (var change in group.changes)
                    {
                        sbDev.Append ("\n  - ");
                        sbDev.Append (change.ToString ());
                    }
                }
            }

            if (option.functions != null && option.functions.Count > 0)
            {
                if (sbDev.Length > 0)
                    sbDev.Append ("\n\n");
                
                sbDev.Append ("[aa]Function calls: [ff]");

                foreach (var function in option.functions)
                {
                    if (function == null)
                        continue;

                    sbDev.Append ("\n- ");
                    sbDev.Append (function.GetType ().Name);

                    if (function is IOverworldFunctionLog functionLogged)
                    {
                        sbDev.Append (": ");
                        sbDev.Append (functionLogged.ToLog ());
                    }
                }
            }
            
            if (option.combat != null)
            {
                if (sbDev.Length > 0)
                    sbDev.Append ("\n\n");
                
                sbDev.Append ("[aa]Starting combat: [ff]");
                sbDev.Append (option.combat.optional ? "\n- Optional" : "\n- Non-optional");
                
                if (option.combat.scenarioTagsFromTarget)
                    sbDev.Append ("\n- Scenario tags taken from target");
                else
                {
                    sbDev.Append ("\n- Custom scenario tags");
                    if (option.combat.scenarioTags != null)
                    {
                        foreach (var kvp in option.combat.scenarioTags)
                        {
                            bool required = kvp.Value;
                            sbDev.Append ($"\n  - {kvp.Key}: {(required ? "required" : "prohibited")}");
                        }
                    }
                }
            }
            
            if (option.steps != null && option.steps.Count > 0)
            {
                if (sbDev.Length > 0)
                    sbDev.Append ("\n\n");
                
                sbDev.Append ("[aa]Leads to step: [ff]");
                foreach (var key in option.steps)
                {
                    sbDev.Append ("\n- ");
                    sbDev.Append (key);
                }
            }
            else
            {
                if (sbDev.Length > 0)
                    sbDev.Append ("\n\n");
                
                sbDev.Append ("Ends the event.");
                if (expanded)
                    sbDev.Append (" Registers event as complete. [aa]This can reduce the chance it'll be selected again.");
            }
   
            var output = sbDev.ToString ();
            if (!expanded)
            {
                output = output.Replace ("[aa]", string.Empty);
                output = output.Replace ("[ff]", string.Empty);
            }

            return output.ToString ();
        }

        public static Dictionary<MemoryChangeContextEvent, string> logLookupMemoryChangeContexts = new Dictionary<MemoryChangeContextEvent, string>
        {
            { MemoryChangeContextEvent.Source, "Base" },
            { MemoryChangeContextEvent.SourceProvince, "Province of the base" },
            { MemoryChangeContextEvent.Target, "Target" },
            { MemoryChangeContextEvent.TargetProvince, "Province of the target" },
            { MemoryChangeContextEvent.SpecificProvince, "Province" },
            { MemoryChangeContextEvent.ActorUnit, "Unit actor" },
            { MemoryChangeContextEvent.ActorPilot, "Pilot actor" },
            { MemoryChangeContextEvent.ActorWorld, "Site actor" }
        };
        
        public static Dictionary<ActionOwnerProvider, string> logLookupActionOwner = new Dictionary<ActionOwnerProvider, string>
        {
            { ActionOwnerProvider.Base, "Base" },
            { ActionOwnerProvider.Source, "Base" },
            { ActionOwnerProvider.SourceProvince, "Province of the base" },
            { ActionOwnerProvider.Target, "Target" },
            { ActionOwnerProvider.TargetProvince, "Province of the target" },
            { ActionOwnerProvider.Spawn, "Spawned entity" },
            { ActionOwnerProvider.SpawnProvince, "Province of the spawned entity" },
            { ActionOwnerProvider.ActorSite, "Site actor" },
            { ActionOwnerProvider.ActorSiteProvince, "Province of the site actor" }
        };
        
        public static Dictionary<ActionTargetProvider, string> logLookupActionTarget = new Dictionary<ActionTargetProvider, string>
        {
            { ActionTargetProvider.None, "None" },
            { ActionTargetProvider.Base, "Base" },
            { ActionTargetProvider.BaseProvince, "Province of the base" },
            { ActionTargetProvider.Source, "Base" },
            { ActionTargetProvider.SourceProvince, "Province of the base" },
            { ActionTargetProvider.Target, "Target" },
            { ActionTargetProvider.TargetProvince, "Province of the target" },
            { ActionTargetProvider.Spawn, "Spawned entity" },
            { ActionTargetProvider.SpawnProvince, "Province of the spawned entity" },
            { ActionTargetProvider.ActorSite, "Site actor" },
            { ActionTargetProvider.ActorSiteProvince, "Province of the site actor" },
            { ActionTargetProvider.ActorUnit, "Unit actor" },
            { ActionTargetProvider.ActorPilot, "Pilot actor" }
        };

        public static string PrintCheckDescription (DataBlockOverworldEventCheck check, bool extended = true, bool listTaggedKeys = false)
        {
            if (check == null)
                return string.Empty;
            
            sbDev.Clear ();
            
            if (extended)
                sbDev.Append ("[aa]Checks are groups of conditions used by overworld event steps and options. A step can be entered and an option can be used only if all conditions are satisfied.[ff]");

            if (check.self != null)
            {
                sbDev.Append ("\n\n[aa]Check (base): [ff]");
                var c = check.self;
                
                if (c.tags != null && c.tags.Count > 0)
                {
                    sbDev.Append ($"\n- Tags ({(c.tagsMethod == EntityCheckMethod.RequireAll ? "all must match" : "any of the following")}):");
                    foreach (var requirement in c.tags)
                    {
                        bool required = !requirement.not;
                        sbDev.Append ($"\n  - {requirement.tag}: {(required ? "required" : "prohibited")}");
                    }
                }

                if (c.eventMemory != null && c.eventMemory.checks != null && c.eventMemory.checks.Count > 0)
                {
                    sbDev.Append ($"\n- Memory ({(c.eventMemory.method == EntityCheckMethod.RequireAll ? "all must match" : "any of the following")}):");
                    foreach (var subcheck in c.eventMemory.checks)
                    {
                        sbDev.Append ("\n  - ");
                        sbDev.Append (subcheck.ToString ());
                    }
                }
                
                if (c.resources != null && c.resources.Count > 0)
                {
                    sbDev.Append ("\n- Resources:");
                    foreach (var kvp in c.resources)
                    {
                        sbDev.Append ("\n  - ");
                        sbDev.Append (kvp.Key);
                        sbDev.Append (": ");
                        sbDev.Append (kvp.Value);
                    }
                }
                
                if (c.baseParts != null && c.baseParts.Count > 0)
                {
                    sbDev.Append ("\n- Base upgrades:");
                    foreach (var kvp in c.baseParts)
                    {
                        bool required = kvp.Value > 0;
                        sbDev.Append ($"\n  - {kvp.Key}: {(required ? "required" : "prohibited")}");
                    }
                }

                if (c.combatReady != null)
                {
                    sbDev.Append ($"\n- Combat readiness: must be {(c.combatReady.ready ? "ready" : "in danger")}");
                }
                
                if (c.movement != null)
                {
                    sbDev.Append ($"\n- Movement: must be {(c.movement.present ? "moving" : "stopped")}");
                }
                
                if (c.movementMode != null)
                {
                    sbDev.Append ($"\n- Movement mode: must {(c.movementMode.not ? "not be" : "be")}: ");
                    sbDev.Append (c.movementMode.modes.ToStringFormatted ());
                }
                
                if (c.deployment != null)
                {
                    sbDev.Append ($"\n- Deployment: must be {(c.deployment.present ? "deployed" : "free")}");
                }

                if (c.pilotsAvailable != null)
                {
                    sbDev.Append ("\n- Pilots available: ");
                    sbDev.Append (c.pilotsAvailable.check.ToString ());
                }
                
                if (c.unitsAvailable != null)
                {
                    sbDev.Append ("\n- Units available: ");
                    sbDev.Append (c.unitsAvailable.check.ToString ());
                }
                
                if (c.pilots != null)
                {
                    sbDev.Append ("\n- Pilots: ");
                    sbDev.Append (c.pilots.present ? "must be present" : "must be absent");
                    
                    if (c.pilots.factionChecked)
                    {
                        sbDev.Append (c.pilots.factionInverted ? ", must not have faction " : ", must have faction ");
                        sbDev.Append (c.pilots.faction);
                    }
                }
                
                if (c.units != null)
                {
                    sbDev.Append ("\n- Units: ");
                    sbDev.Append (c.units.present ? "must be present" : "must be absent");
                }
                
                if (c.levelDeltaTarget != null)
                {
                    sbDev.Append ("\n- Level delta to target: ");
                    sbDev.Append (c.levelDeltaTarget.ToString ());
                }
                
                if (c.levelDeltaProvince != null)
                {
                    sbDev.Append ("\n- Level delta to province: ");
                    sbDev.Append (c.levelDeltaProvince.ToString ());
                }
                
                if (c.weather != null)
                {
                    sbDev.Append ("\n- Weather: must ");
                    sbDev.Append (c.weather.not ? "not be" : "be");
                    sbDev.Append (" in range ");
                    sbDev.Append (c.weather.range);
                }
            }

            if (check.target != null)
            {
                sbDev.Append ("\n\n[aa]Check (target): [ff]");
                var c = check.target;
                
                if (c.tags != null && c.tags.Count > 0)
                {
                    sbDev.Append ($"\n- Tags ({(c.tagsMethod == EntityCheckMethod.RequireAll ? "all must match" : "any of the following")}):");
                    foreach (var requirement in c.tags)
                    {
                        bool required = !requirement.not;
                        sbDev.Append ($"\n  - {requirement.tag}: {(required ? "required" : "prohibited")}");
                    }
                    
                    if (listTaggedKeys)
                    {
                        var keysByTag = DataTagUtility.GetKeysWithTags (DataMultiLinkerOverworldEntityBlueprint.data, c.tags);
                        if (keysByTag.Count > 0)
                        {
                            sbDev.Append ($"\n- Blueprints (from tags above):");
                            int i = 0;
                            foreach (var key in keysByTag)
                            {
                                var bp = DataMultiLinkerOverworldEntityBlueprint.GetEntry (key, false);
                                if (bp != null && !bp.hidden)
                                {
                                    var bpName = bp.textNameProcessed != null ? bp.textNameProcessed.s : null;
                                    if (string.IsNullOrEmpty (bpName))
                                        bpName = "?";
                                    sbDev.Append ($"\n  - {key} ({bpName})");
                                }

                                i += 1;
                                if (i >= 10)
                                {
                                    sbDev.Append ($"\n  - ... ({keysByTag.Count - i} more)");
                                    break;
                                }
                            }
                        }
                    }
                }

                if (c.eventMemory != null && c.eventMemory.checks != null && c.eventMemory.checks.Count > 0)
                {
                    sbDev.Append ($"\n- Memory ({(c.eventMemory.method == EntityCheckMethod.RequireAll ? "all must match" : "any of the following")}):");
                    foreach (var subcheck in c.eventMemory.checks)
                    {
                        sbDev.Append ("\n  - ");
                        sbDev.Append (subcheck.ToString ());
                    }
                }
                
                if (c.resources != null && c.resources.Count > 0)
                {
                    sbDev.Append ("\n- Resources:");
                    foreach (var kvp in c.resources)
                    {
                        sbDev.Append ("\n  - ");
                        sbDev.Append (kvp.Key);
                        sbDev.Append (": ");
                        sbDev.Append (kvp.Value);
                    }
                }
                
                if (c.ai != null)
                {
                    sbDev.Append ("\n- AI: ");
                    sbDev.Append ("\n  - Detection: ");
                    sbDev.Append (c.ai.detection.ToStringFormattedKeyValuePairs ());
                    sbDev.Append ("\n  - State: ");
                    sbDev.Append (c.ai.states.ToStringFormattedKeyValuePairs ());
                }

                if (c.pilots != null)
                {
                    sbDev.Append ("\n- Pilots: ");
                    sbDev.Append (c.pilots.present ? "must be present" : "must be absent");
                    
                    if (c.pilots.factionChecked)
                    {
                        sbDev.Append (c.pilots.factionInverted ? ", must not have faction " : ", must have faction ");
                        sbDev.Append (c.pilots.faction);
                    }
                }
                
                if (c.units != null)
                {
                    sbDev.Append ("\n- Garrison: ");
                    sbDev.Append (c.units.present ? "must be present" : "must be absent");
                }
                
                if (c.faction != null)
                {
                    sbDev.Append ($"\n- Faction: must be ");
                    if (c.faction.hostileCheck)
                        sbDev.Append (c.faction.hostile ? "hostile" : "friendly");
                    else 
                        sbDev.Append (c.faction.factions.ToStringFormatted ());
                }
            }

            if (check.province != null)
            {
                sbDev.Append ("\n\n[aa]Check (province): [ff]");
                var c = check.province;

                sbDev.Append ("\n- ");
                sbDev.Append (c.positionProvider == ProvincePositionProvider.Self ? "At base location" : "At target location");
                
                if (c.eventMemory != null && c.eventMemory.checks != null && c.eventMemory.checks.Count > 0)
                {
                    sbDev.Append ($"\n- Memory ({(c.eventMemory.method == EntityCheckMethod.RequireAll ? "all must match" : "any of the following")}):");
                    foreach (var subcheck in c.eventMemory.checks)
                    {
                        sbDev.Append ($"\n  - ");
                        sbDev.Append (subcheck.ToString ());
                    }
                }
                
                if (c.faction != null)
                {
                    sbDev.Append ($"\n- Faction: must be ");
                    if (c.faction.hostileCheck)
                        sbDev.Append (c.faction.hostile ? "hostile" : "friendly");
                    else 
                        sbDev.Append (c.faction.factions.ToStringFormatted ());
                }

                if (c.access != null)
                {
                    sbDev.Append ($"\n- Access: must be ");
                    sbDev.Append (c.access.accessible ? "possible to contest" : "impossible to contest");
                }
            }

            if (check.action != null && check.action.actions != null && check.action.actions.Count > 0)
            {
                sbDev.Append ("\n\n[aa]Check (actions): [ff]");
                var c = check.action;
                
                sbDev.Append (c.actionMethod == EntityCheckMethod.RequireAll ? "\n- All conditions mandatory" : "\n-One condition sufficient");

                foreach (var subcheck in c.actions)
                {
                    sbDev.Append ("\n- ");
                    if (subcheck.tagsUsed)
                    {
                        sbDev.Append ("Action with tags: ");
                        sbDev.Append (subcheck.tags.ToStringFormattedKeyValuePairs ());
                    }
                    else
                    {
                        sbDev.Append ("Action ");
                        sbDev.Append (subcheck.key);
                    }

                    sbDev.Append ("\n  - ");
                    sbDev.Append (subcheck.owner == OverworldEntitySource.EventSelf ? "Owned by the base" : "Owned by the contact");
                    
                    if (subcheck.actionTargetChecked)
                    {
                        sbDev.Append ("\n  - ");
                        sbDev.Append (subcheck.actionTargetComparison == OverworldEntitySource.EventSelf ? "Targeting the base" : "Targeting the contact");
                    }
                    
                    sbDev.Append ("\n  - ");
                    sbDev.Append (subcheck.actionDesired ? "Should exist" : "Should not exist");
                }
            }

            if (check.actors != null)
            {
                sbDev.Append ("\n\n[aa]Check (actors): [ff]");
                var c = check.actors;
                
                if (c.actorsWorldPresent != null && c.actorsWorldPresent.Count > 0)
                {
                    sbDev.Append ("\n- Sites:");
                    foreach (var kvp in c.actorsWorldPresent)
                    {
                        sbDev.Append ("\n  - ");
                        sbDev.Append (kvp.Key);
                        sbDev.Append (": ");
                        sbDev.Append (kvp.Value ? "must be present" : "must be absent");
                    }
                }

                if (c.actorsUnitsPresent != null && c.actorsUnitsPresent.Count > 0)
                {
                    sbDev.Append ("\n- Units:");
                    foreach (var kvp in c.actorsUnitsPresent)
                    {
                        sbDev.Append ("\n  - ");
                        sbDev.Append (kvp.Key);
                        sbDev.Append (": ");
                        sbDev.Append (kvp.Value ? "must be present" : "must be absent");
                    }
                }

                if (c.actorsPilotsPresent != null && c.actorsPilotsPresent.Count > 0)
                {
                    sbDev.Append ("\n- Pilots:");
                    foreach (var kvp in c.actorsPilotsPresent)
                    {
                        sbDev.Append ("\n  - ");
                        sbDev.Append (kvp.Key);
                        sbDev.Append (": ");
                        sbDev.Append (kvp.Value ? "must be present" : "must be absent");
                    }
                }
            }

            return sbDev.ToString ();
        }
    }

    //TODO
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldEventActorWorld
    {
	    [YamlIgnore, HideInInspector]
	    public string key;
        
	    [YamlIgnore, HideInInspector, NonSerialized]
	    public DataContainerOverworldEvent parent;
        
        [InfoBox ("Critical actor checked by the starting step", InfoMessageType.None, "IsCritical")]
        [InlineButton ("MakeCritical", "Make critical", ShowIf = "IsNotCritical")]
        public bool hidden;

        public DataBlockOverworldEventActorWorldCheck check;
        
        #if UNITY_EDITOR

        private bool IsNotCritical () => !IsCritical ();
        private bool IsCritical ()
        {
            if (parent == null || parent.steps == null || string.IsNullOrEmpty (parent.stepOnStart) || !parent.steps.ContainsKey (parent.stepOnStart) || string.IsNullOrEmpty (key))
                return false;
            
            var step = parent.steps[parent.stepOnStart];
            if (step.check == null || step.check.actors == null || step.check.actors.actorsWorldPresent == null || !step.check.actors.actorsWorldPresent.ContainsKey (key))
                return false;

            return step.check.actors.actorsWorldPresent[key];
        }
        
        public void MakeCritical ()
        {
            if (parent == null || parent.steps == null || string.IsNullOrEmpty (parent.stepOnStart) || !parent.steps.ContainsKey (parent.stepOnStart) || string.IsNullOrEmpty (key))
            {
                Debug.LogWarning ($"Failed to insert check making this actor mandatory on entry - no parent or no starting step");
                return;
            }

            var step = parent.steps[parent.stepOnStart];
            if (step.check == null)
                step.check = new DataBlockOverworldEventCheck ();

            if (step.check.actors == null)
                step.check.actors = new DataBlockOverworldEventCheckActors ();

            if (step.check.actors.actorsWorldPresent == null)
                step.check.actors.actorsWorldPresent = new Dictionary<string, bool> { { key, true } };
            else
                step.check.actors.actorsWorldPresent[key] = true;
            
            Debug.LogWarning ($"World actor {key} is now critical (must be present for entry) on event {parent.key}");
        }
        
        #endif
    }

    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldEventActorUnit
    {
        [YamlIgnore, HideInInspector]
        public string key;
        
        [YamlIgnore, HideInInspector, NonSerialized]
        public DataContainerOverworldEvent parent;
        
        [InfoBox ("Critical actor checked by the starting step", InfoMessageType.None, "IsCritical")]
        [InlineButton ("MakeCritical", "Make critical", ShowIf = "IsNotCritical")]
        public bool hidden;
        
        public ActorSource source = ActorSource.Self;
        
        public ActorUnitSort sort = ActorUnitSort.Name;

        public ActorPick pick = ActorPick.Random;

        public DataBlockOverworldEventActorUnitCheck check;

        #if UNITY_EDITOR
        
        private IEnumerable<string> GetStepKeys () => 
            parent != null && parent.steps != null ? parent.steps.Keys : null;
        
        #endif
        
        private static StringBuilder sb = new StringBuilder ();

        public override string ToString ()
        {
            sb.Clear ();
            sb.Append ("Unit actor slot:");

            sb.Append ("\nSource: ");
            sb.Append (source);
            
            sb.Append ("\nSort: ");
            sb.Append (sort);
            
            sb.Append ("\nPick: ");
            sb.Append (pick);

            if (check != null)
            {
                sb.Append ("\nCheck:");
                sb.Append (check);
            }

            return sb.ToString ();
        }
        
        #if UNITY_EDITOR

        private bool IsNotCritical () => !IsCritical ();
        private bool IsCritical ()
        {
            if (parent == null || parent.steps == null || string.IsNullOrEmpty (parent.stepOnStart) || !parent.steps.ContainsKey (parent.stepOnStart) || string.IsNullOrEmpty (key))
                return false;
            
            var step = parent.steps[parent.stepOnStart];
            if (step.check == null || step.check.actors == null || step.check.actors.actorsUnitsPresent == null || !step.check.actors.actorsUnitsPresent.ContainsKey (key))
                return false;

            return step.check.actors.actorsUnitsPresent[key];
        }
        
        public void MakeCritical ()
        {
            if (parent == null || parent.steps == null || string.IsNullOrEmpty (parent.stepOnStart) || !parent.steps.ContainsKey (parent.stepOnStart) || string.IsNullOrEmpty (key))
            {
                Debug.LogWarning ($"Failed to insert check making this actor mandatory on entry - no parent or no starting step");
                return;
            }

            var step = parent.steps[parent.stepOnStart];
            if (step.check == null)
                step.check = new DataBlockOverworldEventCheck ();

            if (step.check.actors == null)
                step.check.actors = new DataBlockOverworldEventCheckActors ();

            if (step.check.actors.actorsUnitsPresent == null)
                step.check.actors.actorsUnitsPresent = new Dictionary<string, bool> { { key, true } };
            else
                step.check.actors.actorsUnitsPresent[key] = true;
            
            Debug.LogWarning ($"Unit actor {key} is now critical (must be present for entry) on event {parent.key}");
        }
        
        #endif
    }
    
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldEventActorPilot
    {
        [YamlIgnore, HideInInspector]
        public string key;
        
        [YamlIgnore, HideInInspector]
        public DataContainerOverworldEvent parent;
        
        [InfoBox ("Critical actor checked by the starting step", InfoMessageType.None, "IsCritical")]
        [InlineButton ("MakeCritical", "Make critical", ShowIf = "IsNotCritical")]
        public bool hidden;

        public ActorSource source = ActorSource.Self;
        
        public ActorPilotSort sort = ActorPilotSort.Name;

        public ActorPick pick = ActorPick.Random;

        public DataBlockOverworldEventActorPilotCheck check;
        
        #if UNITY_EDITOR
        
        private IEnumerable<string> GetStepKeys () => 
            parent != null && parent.steps != null ? parent.steps.Keys : null;
        
        #endif
        
        private static StringBuilder sb = new StringBuilder ();
        
        public override string ToString ()
        {
            sb.Clear ();
            sb.Append ("Pilot actor slot:");

            sb.Append ("\nSource: ");
            sb.Append (source);
            
            sb.Append ("\nSort: ");
            sb.Append (sort);
            
            sb.Append ("\nPick: ");
            sb.Append (pick);

            if (check != null)
            {
                sb.Append ("\nCheck:");
                sb.Append (check);
            }

            return sb.ToString ();
        }
        
        #if UNITY_EDITOR

        private bool IsNotCritical () => !IsCritical ();
        private bool IsCritical ()
        {
            if (parent == null || parent.steps == null || string.IsNullOrEmpty (parent.stepOnStart) || !parent.steps.ContainsKey (parent.stepOnStart) || string.IsNullOrEmpty (key))
                return false;
            
            var step = parent.steps[parent.stepOnStart];
            if (step.check == null || step.check.actors == null || step.check.actors.actorsPilotsPresent == null || !step.check.actors.actorsPilotsPresent.ContainsKey (key))
                return false;

            return step.check.actors.actorsPilotsPresent[key];
        }
        
        public void MakeCritical ()
        {
            if (parent == null || parent.steps == null || string.IsNullOrEmpty (parent.stepOnStart) || !parent.steps.ContainsKey (parent.stepOnStart) || string.IsNullOrEmpty (key))
            {
                Debug.LogWarning ($"Failed to insert check making this actor mandatory on entry - no parent or no starting step");
                return;
            }

            var step = parent.steps[parent.stepOnStart];
            if (step.check == null)
                step.check = new DataBlockOverworldEventCheck ();

            if (step.check.actors == null)
                step.check.actors = new DataBlockOverworldEventCheckActors ();

            if (step.check.actors.actorsPilotsPresent == null)
                step.check.actors.actorsPilotsPresent = new Dictionary<string, bool> { { key, true } };
            else
                step.check.actors.actorsPilotsPresent[key] = true;
            
            Debug.LogWarning ($"Pilot actor {key} is now critical (must be present for entry) on event {parent.key}");
        }
        
        #endif
    }
}