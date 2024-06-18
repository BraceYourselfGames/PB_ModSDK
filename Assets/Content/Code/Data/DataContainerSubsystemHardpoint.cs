using System;
using System.Collections.Generic;
using PhantomBrigade.Functions.Equipment;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [Serializable][LabelWidth (180f)]
    public class DataContainerSubsystemHardpoint : DataContainerWithText, IDataContainerTagged, IDataContainerKeyReplacementWarning
    {
        [ShowIf ("exposed")]
        [LabelText ("Name")]
        [YamlIgnore, ShowIf (DataEditor.textAttrArg)]
        public string textName;
        
        [ShowIf ("@exposed && textSuffixUsed")]
        [LabelText ("Suffix / Desc.")]
        [YamlIgnore, ShowIf (DataEditor.textAttrArg)]
        public string textSuffix;

        [ShowIf ("@exposed && textDescUsed")]
        [HideLabel]
        [YamlIgnore, ShowIf (DataEditor.textAttrArg), TextArea (1, 10)]
        public string textDesc;

        [ShowIf ("exposed")]
        public bool textSuffixUsed = true;
        
        [ShowIf ("exposed")]
        public bool textDescUsed = true;

        [PropertyTooltip ("Whether player can edit content of this hardpoint (if subsystem doesn't say it's fused)")]
        public bool editable = true;
        
        [PropertyTooltip ("Whether player can see this hardpoint and its content (for instance, perk hardpoints are not editable, but players need to see their content)")]
        public bool exposed = false;
        
        [PropertyTooltip ("Blocks anything mounted to this subsystem from participating in the stat system (usually set on purely hardpoints receiving purely visual systems)")]
        public bool visual = false;
        
        [PropertyTooltip ("Marks content of this hardpoint as irrelevant for unit display. A subsystem might still include visuals, but they won't be used by combat unit visual managers (e.g. x-ray model of a reactor won't be visible in combat)")]
        public bool isInternal = false;

        [PropertyTooltip ("Whether subsystems in this hardpoint affect level of a part")]
        public bool leveled = false;
        
        [PropertyTooltip ("Whether subsystems in this hardpoint affect level of a part")]
        public bool group = false;
        
        [PropertyTooltip ("Whether the hardpoint is hidden in workshop")]
        public bool workshopObscured = false;
        
        public bool forceVisualRoot = false;
        
        [ShowIf ("exposed")]
        public string icon = "s_icon_m_link_squad";
        
        [ShowIf ("exposed")]
        public int sortPriority = 0;
        public int duplication = 1;

        [PropertyRange (0f, 1f)]
        public float damageWeight = 1f;
        
        [PropertyRange (0f, 1f)]
        public float damageToAlpha = 1f;
        
        [ValueDropdown ("@DataMultiLinkerSubsystemHardpoint.groups")]
        public string visualGroup;

        [ValueDropdown ("@DataMultiLinkerSubsystemHardpoint.tags")]
        public HashSet<string> tags = new HashSet<string> ();




        public HashSet<string> GetTags (bool processed)
        {
            return tags;
        }
        
        public bool IsHidden () => false;

        public void KeyReplacementWarning ()
        {
            #if UNITY_EDITOR
                        
            DataMultiLinkerSubsystem.PrintChangeWarning ();
            DataMultiLinkerPartPreset.PrintChangeWarning ();
            DataMultiLinkerUnitPreset.PrintChangeWarning ();
            DataMultiLinkerScenario.PrintChangeWarning ();
            DataMultiLinkerUnitCheck.PrintChangeWarning ();
            DataMultiLinkerAction.PrintChangeWarning ();
            Debug.LogWarning ($"Save changed!");
                        
            #endif
        }

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
        }

        public override void OnKeyReplacement (string keyOld, string keyNew)
        {
            base.OnKeyReplacement (keyOld, keyNew);

            var subsystemBlueprints = DataMultiLinkerSubsystem.data;
            foreach (var kvp in subsystemBlueprints)
            {
                var subsystemBlueprint = kvp.Value;
                if (subsystemBlueprint.hardpoints == null || !subsystemBlueprint.hardpoints.Contains (keyOld))
                    continue;

                subsystemBlueprint.hardpoints.Remove (keyOld);
                subsystemBlueprint.hardpoints.Add (keyNew);
                Debug.Log ($"Updated subsystem blueprint {subsystemBlueprint.key} with new subsystem hardpoint key: {keyOld} -> {keyNew}");
            }
            
            var partPresets = DataMultiLinkerPartPreset.data;
            foreach (var kvp in partPresets)
            {
                var partPreset = kvp.Value;
                if (partPreset.genSteps == null)
                    continue;

                foreach (var step in partPreset.genSteps)
                {
                    if (step is PartGenStepTargeted stepTargeted && stepTargeted.hardpointsTargeted != null)
                    {
                        if (stepTargeted.hardpointsTargeted.Contains (keyOld))
                        {
                            stepTargeted.hardpointsTargeted.Remove (keyOld);
                            stepTargeted.hardpointsTargeted.Add (keyNew);
                            Debug.Log ($"Updated part preset {partPreset.key} with new subsystem hardpoint key: {keyOld} -> {keyNew}");
                        }
                    }
                }
            }

            var unitPresets = DataMultiLinkerUnitPreset.data;
            foreach (var kvp in unitPresets)
            {
                var unitPreset = kvp.Value;
                if (unitPreset.parts == null)
                    continue;

                foreach (var kvp2 in unitPreset.parts)
                {
                    var partOverride = kvp2.Value;
                    if (partOverride.systems == null || !partOverride.systems.ContainsKey (keyOld))
                        continue;

                    var value = partOverride.systems[keyOld];
                    partOverride.systems.Remove (keyOld);
                    partOverride.systems.Add (keyNew, value);
                    Debug.Log ($"Updated unit preset {unitPreset.key} with new subsystem hardpoint key: {keyOld} -> {keyNew}");
                }
            }

            var scenarios = DataMultiLinkerScenario.data;
            foreach (var kvp in scenarios)
            {
                var scenario = kvp.Value;
                if (scenario.unitPresetsProc == null)
                    continue;
                
                foreach (var kvp2 in scenario.unitPresetsProc)
                {
                    if (kvp2.Value is DataBlockScenarioUnitPresetEmbedded presetEmbedded)
                    {
                        if (presetEmbedded.preset == null)
                            continue;

                        var parts = presetEmbedded.preset.parts;
                        if (parts == null)
                            continue;

                        foreach (var kvp3 in parts)
                        {
                            var partOverride = kvp3.Value;
                            if (partOverride.systems == null || !partOverride.systems.ContainsKey (keyOld))
                                continue;

                            var value = partOverride.systems[keyOld];
                            partOverride.systems.Remove (keyOld);
                            partOverride.systems.Add (keyNew, value);
                            Debug.Log ($"Updated unit preset {kvp2.Key} in scenario {scenario.key} with new subsystem hardpoint key: {keyOld} -> {keyNew}");
                        }
                    }
                }
            }
            
            var unitGroups = DataMultiLinkerCombatUnitGroup.data;
            foreach (var kvp in unitGroups)
            {
                var unitGroup = kvp.Value;
                if (unitGroup.unitPresets == null)
                    continue;
                
                foreach (var kvp2 in unitGroup.unitPresets)
                {
                    if (kvp2.Value is DataBlockScenarioUnitPresetEmbedded presetEmbedded)
                    {
                        if (presetEmbedded.preset == null)
                            continue;

                        var parts = presetEmbedded.preset.parts;
                        if (parts == null)
                            continue;

                        foreach (var kvp3 in parts)
                        {
                            var partOverride = kvp3.Value;
                            if (partOverride.systems == null || !partOverride.systems.ContainsKey (keyOld))
                                continue;

                            var value = partOverride.systems[keyOld];
                            partOverride.systems.Remove (keyOld);
                            partOverride.systems.Add (keyNew, value);
                            Debug.Log ($"Updated unit preset {kvp2.Key} in unit group {unitGroup.key} with new subsystem hardpoint key {keyOld} -> {keyNew}");
                        }
                    }
                }
            }
            
            var checks = DataMultiLinkerUnitCheck.data;
            foreach (var kvp in checks)
            {
                var check = kvp.Value;
                if (check.check.subsystems == null)
                    continue;

                foreach (var checkSubsystem in check.check.subsystems)
                {
                    if (checkSubsystem.hardpoint != keyOld)
                        continue;

                    checkSubsystem.hardpoint = keyNew;
                    Debug.Log ($"Updated unit check {check.key} with new subsystem hardpoint key: {keyOld} -> {keyNew}");
                }
            }
            
            var actions = DataMultiLinkerAction.data;
            foreach (var kvp in actions)
            {
                var action = kvp.Value;
                if (action.dataCore?.check?.subsystems == null)
                    continue;

                foreach (var checkSubsystem in action.dataCore.check.subsystems)
                {
                    if (checkSubsystem.hardpoint != keyOld)
                        continue;

                    checkSubsystem.hardpoint = keyNew;
                    Debug.Log ($"Updated combat action {action.key} check with new subsystem hardpoint key: {keyOld} -> {keyNew}");
                }
            }

            /*
            var save = DataManagerSave.data;
            if (save != null && save.units != null)
            {
                foreach (var kvp in save.units)
                {
                    var unit = kvp.Value;
                    if (unit.parts == null) 
                        continue;

                    foreach (var kvp2 in unit.parts)
                    {
                        var part = kvp2.Value;
                        if (part.systems == null || !part.systems.ContainsKey (keyOld))
                            continue;

                        var value = part.systems[keyOld];
                        part.systems.Remove (keyOld);
                        part.systems.Add (keyNew, value);
                        Debug.Log ($"Updated saved unit {unit.key} with new subsystem hardpoint key: {keyOld} -> {keyNew}");
                    }
                }
            }
            */
        }

        public override void ResolveText ()
        {
            if (!exposed)
                return;
            
            textName = DataManagerText.GetText (TextLibs.equipmentHardpoints, $"{key}_name", true);
            
            if (textDescUsed)
                textDesc = DataManagerText.GetText (TextLibs.equipmentHardpoints, $"{key}_text", true);
            
            if (textSuffixUsed)
                textSuffix = DataManagerText.GetText (TextLibs.equipmentHardpoints, $"{key}_suffix", true);
        }

        #if UNITY_EDITOR

        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            if (exposed)
            {
                DataManagerText.TryAddingTextToLibrary (TextLibs.equipmentHardpoints, $"{key}_name", textName);
                
                if (textDescUsed)
                    DataManagerText.TryAddingTextToLibrary (TextLibs.equipmentHardpoints, $"{key}_text", textDesc);
                
                if (textSuffixUsed)
                    DataManagerText.TryAddingTextToLibrary (TextLibs.equipmentHardpoints, $"{key}_suffix", textSuffix);
            }
        }
        
        #endif
    }
}

