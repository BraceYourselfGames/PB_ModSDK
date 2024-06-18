using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Functions.Equipment;
using UnityEngine;
using Sirenix.OdinInspector;

public class UtilityUnitSystemBuilder : MonoBehaviour
{
    [ShowInInspector]
    [ValueDropdown ("@DataMultiLinkerUnitPreset.data.Keys")]
    public static string unitPresetBase = "vhc_system_wpn_turret_01";
    
    [ShowInInspector]
    [ValueDropdown ("@DataMultiLinkerPartSocket.data.Keys")]
    public static string partPresetSocket = "core";
    
    [ShowInInspector]
    [ValueDropdown ("@DataMultiLinkerPartPreset.data.Keys")]
    public static string partPresetBase = "vhc_system_wpn_turret_01";
    
    [ShowInInspector]
    [ValueDropdown ("@DataMultiLinkerSubsystemHardpoint.data.Keys")]
    public static string subsystemHardpoint = "vhc_general";
    
    [ShowInInspector]
    [ValueDropdown ("@DataMultiLinkerSubsystem.data.Keys")]
    public static string subsystemBase = "vhc_system_wpn_turret_01";
    
    [ShowInInspector]
    [ValueDropdown ("@ItemHelper.GetAllVisuals ().Keys")]
    public static string subsystemAttachment = "comp_boss_turret_01";

    [ShowInInspector]
    public static string rootPrefix = "vhc_system_";
    
    [ShowInInspector]
    public static string rootName = "wpn_turret_01";

    [ShowInInspector, PropertyOrder (11), FoldoutGroup ("Unit Preset", false), Button ("Save")]
    public void SaveUnitPreset () => DataMultiLinkerUnitPreset.SaveData ();
    
    [ShowInInspector, PropertyOrder (12), HideLabel, FoldoutGroup ("Unit Preset", false)]
    public static DataContainerUnitPreset unitPreset = null;
    
    [ShowInInspector, PropertyOrder (21), FoldoutGroup ("Part Preset", false), Button ("Save")]
    public void SavePartPreset () => DataMultiLinkerPartPreset.SaveData ();
    
    [ShowInInspector, PropertyOrder (22), HideLabel, FoldoutGroup ("Part Preset", false)]
    public static DataContainerPartPreset partPreset = null;
    
    [ShowInInspector, PropertyOrder (31), FoldoutGroup ("Subsystem", false), Button ("Save")]
    public void SaveSubsystem () => DataMultiLinkerSubsystem.SaveData ();
    
    [ShowInInspector, PropertyOrder (32), HideLabel, FoldoutGroup ("Subsystem", false)]
    public static DataContainerSubsystem subsystem = null;
    

    [Button (ButtonSizes.Large), PropertyOrder (-1)]
    public void CreateSystemUnit ()
    {
        unitPreset = null;
        partPreset = null;
        subsystem = null;
        
        var unitPresetOrigin = DataMultiLinkerUnitPreset.GetEntry (unitPresetBase);
        var partPresetOrigin = DataMultiLinkerPartPreset.GetEntry (partPresetBase);
        var subsystemOrigin = DataMultiLinkerSubsystem.GetEntry (subsystemBase);
        if (unitPresetOrigin == null || partPresetOrigin == null || subsystemOrigin == null)
            return;

        var unitPresetKey = $"{rootPrefix}{rootName}";
        if (DataMultiLinkerUnitPreset.data.ContainsKey (unitPresetKey))
        {
            Debug.LogWarning ($"Unit preset key {unitPresetKey} already exists");
            return;
        }
        
        var partPresetKey = $"{rootPrefix}{rootName}";
        if (DataMultiLinkerPartPreset.data.ContainsKey (partPresetKey))
        {
            Debug.LogWarning ($"Part preset key {partPresetKey} already exists");
            return;
        }
        
        var subsystemKey = $"{rootPrefix}{rootName}";
        if (DataMultiLinkerSubsystem.data.ContainsKey (subsystemKey))
        {
            Debug.LogWarning ($"Subsystem key {subsystemKey} already exists");
            return;
        }
        
        unitPreset = UtilitiesYAML.CloneThroughYaml (unitPresetOrigin);
        unitPreset.parts = new SortedDictionary<string, DataBlockUnitPartOverride>
        {
            {
                partPresetSocket, 
                new DataBlockUnitPartOverride
                {
                    preset = new DataBlockPartSlotResolverKeys
                    {
                        keys = new List<string> { partPresetKey }
                    }
                }
            }
        };
        
        DataMultiLinkerUnitPreset.data.Add (unitPresetKey, unitPreset);
        unitPreset.OnAfterDeserialization (unitPresetKey);
        DataMultiLinkerUnitPreset.SaveData ();
        
        partPreset = UtilitiesYAML.CloneThroughYaml (partPresetOrigin);
        partPreset.genSteps = new List<IPartGenStep>
        {
            new AddHardpoints
            {
                hardpointsTargeted = new List<string> { subsystemHardpoint },
                subsystemsInitial = new List<string> { subsystemKey }
            }
        };
        
        DataMultiLinkerPartPreset.data.Add (partPresetKey, partPreset);
        partPreset.OnAfterDeserialization (partPresetKey);
        DataMultiLinkerPartPreset.SaveData ();
        
        subsystem = UtilitiesYAML.CloneThroughYaml (subsystemOrigin);
        subsystem.attachments = new Dictionary<string, DataBlockSubsystemAttachment>
        {
            {
                "main",
                new DataBlockSubsystemAttachment
                {
                    key = subsystemAttachment,
                    scale = Vector3.one
                }
            }
        };
        
        DataMultiLinkerSubsystem.data.Add (subsystemKey, subsystem);
        subsystem.OnAfterDeserialization (subsystemKey);
        DataMultiLinkerSubsystem.SaveData ();
    }

    [Button (ButtonSizes.Large), PropertyOrder (-1)]
    private void SaveAll ()
    {
        DataMultiLinkerUnitPreset.SaveData ();
        DataMultiLinkerPartPreset.SaveData ();
        DataMultiLinkerSubsystem.SaveData ();
    }
}
