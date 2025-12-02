using System.Collections.Generic;
using System.Text;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerPilotPersistentProfile : DataMultiLinker<DataContainerPilotPersistentProfile>
    {
        public DataMultiLinkerPilotPersistentProfile ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.pilotProfiles);
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
        }

        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool showTagCollections;

            [ShowInInspector]
            public static bool showInheritance = false;
        }

        [ShowInInspector] [HideLabel]
        public Presentation presentation = new Presentation ();
        
        private static StringBuilder sb = new StringBuilder ();

        public static void OnAfterDeserialization ()
        {
            // Process every subsystem recursively first
            foreach (var kvp in data)
                ProcessRecursiveStart (kvp.Value);

            // Fill parents after recursive processing is done on all presets, ensuring lack of cyclical refs etc
            foreach (var kvp1 in data)
            {
                var presetA = kvp1.Value;
                if (presetA == null)
                    continue;

                var key = kvp1.Key;

                if (presetA.children != null)
                    presetA.children.Clear ();

                foreach (var kvp2 in data)
                {
                    var presetB = kvp2.Value;
                    if (presetB.parents == null || presetB.parents.Count == 0)
                        continue;

                    foreach (var link in presetB.parents)
                    {
                        if (link.key == key)
                        {
                            if (presetA.children == null)
                                presetA.children = new List<string> ();
                            presetA.children.Add (presetB.key);
                        }
                    }
                }
            }

            foreach (var kvp1 in data)
                Postprocess (kvp1.Value);
        }

        private static List<DataContainerPilotPersistentProfile> entriesUpdated = new List<DataContainerPilotPersistentProfile> ();

        public static void ProcessRelated (DataContainerPilotPersistentProfile origin)
        {
            if (origin == null)
                return;

            entriesUpdated.Clear ();
            entriesUpdated.Add (origin);

            if (origin.children != null)
            {
                foreach (var childKey in origin.children)
                {
                    var entry = GetEntry (childKey);
                    if (entry != null)
                        entriesUpdated.Add (entry);
                }
            }

            foreach (var entry in entriesUpdated)
            {
                // Avoid localization refresh and other losses on origin
                if (entry != origin)
                    entry.OnAfterDeserialization (entry.key);
            }

            foreach (var entry in entriesUpdated)
                ProcessRecursiveStart (entry);

            foreach (var entry in entriesUpdated)
                Postprocess (entry);
        }
        
        public static void Postprocess (DataContainerPilotPersistentProfile target)
        {
            // Add any code that requires fully filled processed fields here, e.g. procgen text based on merged data
        }
        
        public static void ProcessRecursiveStart (DataContainerPilotPersistentProfile origin)
        {
            if (origin == null)
                return;

            origin.coreProc = null;
            origin.textNameProc = null;
            origin.textCallsignProc = null;
            origin.textUnitProc = null;
            origin.textDescProc = null;
            origin.textSectionsProc = null;
            
            origin.roleKeyProc = null;
            origin.appearanceKeyProc = null;
            origin.liveryPresetProc = null;
            
            origin.traitSlotsInjectedProc = null;
            origin.unitPresetFilterProc = null;

            origin.injectionCheckProc = null;
            origin.deploymentTurnCheckProc = null;
            origin.deploymentConditionsProc = null;
            
            origin.effectsOnSpawnProc = null;
            origin.effectsOnLandingStartProc = null;
            origin.effectsOnArrivalProc = null;
            origin.effectsOnTraumaCombatProc = null;
            origin.effectsOnTraumaDebriefingProc = null;
            origin.effectsOnDeathCombatProc = null;
            origin.effectsOnDeathDebriefingProc = null;

            origin.directorProc = null;
            
            ProcessRecursive (origin, origin, 0);
        }

        private static void ProcessRecursive (DataContainerPilotPersistentProfile current, DataContainerPilotPersistentProfile root, int depth)
        {
            if (current == null || root == null)
            {
                Debug.LogWarning ($"Received null step or root interaction reference while validating pilot profile hierarchy");
                return;
            }

            if (depth > 0 && current == root)
            {
                Debug.LogWarning ($"Encountered dependency loop at depth level {depth} when processing pilot profile {root.key}");
                return;
            }
            
            if (current.core != null && root.coreProc == null)
                root.coreProc = current.core;

            if (current.textName != null && root.textNameProc == null)
                root.textNameProc = current.textName;
            
            if (current.textCallsign != null && root.textCallsignProc == null)
                root.textCallsignProc = current.textCallsign;
            
            if (current.textUnit != null && root.textUnitProc == null)
                root.textUnitProc = current.textUnit;

            if (current.textDesc != null && root.textDescProc == null)
                root.textDescProc = current.textDesc;
            
            if (current.textSections != null && root.textSectionsProc == null)
                root.textSectionsProc = current.textSections;
            

            if (current.roleKey != null && root.roleKeyProc == null)
                root.roleKeyProc = current.roleKey;
            
            if (current.appearanceKey != null && root.appearanceKeyProc == null)
                root.appearanceKeyProc = current.appearanceKey;
            
            if (current.liveryPreset != null && root.liveryPresetProc == null)
                root.liveryPresetProc = current.liveryPreset;
            
            
            if (current.traitSlotsInjected != null && root.traitSlotsInjectedProc == null)
                root.traitSlotsInjectedProc = current.traitSlotsInjected;
            
            if (current.unitPresetFilter != null && root.unitPresetFilterProc == null)
                root.unitPresetFilterProc = current.unitPresetFilter;
            
            
            if (current.injectionCheck != null && root.injectionCheckProc == null)
                root.injectionCheckProc = current.injectionCheck;
            
            if (current.deploymentTurnCheck != null && root.deploymentTurnCheckProc == null)
                root.deploymentTurnCheckProc = current.deploymentTurnCheck;
            
            if (current.deploymentConditions != null && root.deploymentConditionsProc == null)
                root.deploymentConditionsProc = current.deploymentConditions;
            
            
            if (current.effectsOnSpawn != null && root.effectsOnSpawnProc == null)
                root.effectsOnSpawnProc = current.effectsOnSpawn;
            
            if (current.effectsOnLandingStart != null && root.effectsOnLandingStartProc == null)
                root.effectsOnLandingStartProc = current.effectsOnLandingStart;
            
            if (current.effectsOnArrival != null && root.effectsOnArrivalProc == null)
                root.effectsOnArrivalProc = current.effectsOnArrival;
            
            if (current.effectsOnTraumaCombat != null && root.effectsOnTraumaCombatProc == null)
                root.effectsOnTraumaCombatProc = current.effectsOnTraumaCombat;
            
            if (current.effectsOnTraumaDebriefing != null && root.effectsOnTraumaDebriefingProc == null)
                root.effectsOnTraumaDebriefingProc = current.effectsOnTraumaDebriefing;
            
            if (current.effectsOnDeathCombat != null && root.effectsOnDeathCombatProc == null)
                root.effectsOnDeathCombatProc = current.effectsOnDeathCombat;
            
            if (current.effectsOnDeathDebriefing != null && root.effectsOnDeathDebriefingProc == null)
                root.effectsOnDeathDebriefingProc = current.effectsOnDeathDebriefing;
            
            
            if (current.director != null && root.directorProc == null)
                root.directorProc = current.director;
            
            // Just in case we somehow missed a cyclical dependency
            if (depth > 20)
            {
                Debug.LogWarning ($"Pilot profile {current.key} fails to complete recursive processing in under 20 steps.");
                return;
            }

            // No parents further up
            if (current.parents == null || current.parents.Count == 0)
                return;

            for (int i = 0, count = current.parents.Count; i < count; ++i)
            {
                var link = current.parents[i];
                if (link == null || string.IsNullOrEmpty (link.key))
                {
                    Debug.LogWarning ($"Pilot profile {current.key} has null or empty parent link at index {i}!");
                    continue;
                }

                if (link.key == current.key)
                {
                    Debug.LogWarning ($"Pilot profile {current.key} has invalid parent key matching itself at index {i}");
                    continue;
                }

                var parent = GetEntry (link.key, false);
                if (parent == null)
                {
                    Debug.LogWarning ($"Pilot profile {current.key} has invalid parent key {link.key} at index {i} that can't be found in the database");
                    continue;
                }
                
                // Append next hierarchy level for easier preview
                if (parent.parents != null && parent.parents.Count > 0)
                {
                    sb.Clear ();
                    for (int i2 = 0, count2 = parent.parents.Count; i2 < count2; ++i2)
                    {
                        if (i2 > 0)
                            sb.Append (", ");

                        var parentOfParent = parent.parents[i2];
                        if (parentOfParent == null || string.IsNullOrEmpty (parentOfParent.key))
                            sb.Append ("—");
                        else
                            sb.Append (parentOfParent.key);
                    }

                    link.hierarchy = sb.ToString ();
                }
                else
                    link.hierarchy = "—";
                
                ProcessRecursive (parent, root, depth + 1);
            }
        }
    }
}


