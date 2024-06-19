using System;
using System.Collections.Generic;
using PhantomBrigade.Functions;
using PhantomBrigade.Functions.Equipment;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

#if PB_MODSDK
using PhantomBrigade.SDK.ModTools;
#endif

namespace PhantomBrigade.Data
{
    public static class PartEventsGeneral
    {
        public const string OnPartActivation = "on_part_activation";
        public const string OnPartDamage = "on_part_damage";
        public const string OnPartDestruction = "on_part_destruction";
        public const string OnTagChange = "on_tag_change";
        public const string OnOverheat = "on_overheat";
        public const string OnCollision = "on_collision";
        public const string OnCrash = "on_crash";
    }

    public static class PartEventsTargeted
    {
        public const string OnWeaponFired = "on_wpn_fired";
        public const string OnWeaponHit = "on_wpn_hit";
        public const string OnWeaponPartDestruction = "on_wpn_part_destruction";
    }

    public static class PartEventsAction
    {
        public const string OnActionStart = "on_action_start";
        public const string OnActionEnd = "on_action_end";
    }

    public class DataBlockSubsystemFunctions
    {
        [DropdownReference]
        [ListDrawerSettings (CustomAddFunction = "@new DataBlockSubsystemFunctionsGeneral ()")]
        public List<DataBlockSubsystemFunctionsGeneral> general;

        [DropdownReference]
        [ListDrawerSettings (CustomAddFunction = "@new DataBlockSubsystemFunctionsTargeted ()")]
        public List<DataBlockSubsystemFunctionsTargeted> targeted;

        [DropdownReference]
        [ListDrawerSettings (CustomAddFunction = "@new DataBlockSubsystemFunctionsAction ()")]
        public List<DataBlockSubsystemFunctionsAction> action;

        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockSubsystemFunctions () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    public class DataBlockSubsystemFunction
    {
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockSubsystemFunction () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        private Color GetElementColor (int index, Color defaultColor) => DataEditor.GetColorFromElementIndex (index);

        #endif
        #endregion
    }

    public class DataBlockSubsystemFunctionsGeneral : DataBlockSubsystemFunction
    {
        [ValueDropdown ("@FieldReflectionUtility.GetConstantStringFieldValues (typeof (PartEventsGeneral), false)")]
        public string context = string.Empty;
        
        [DropdownReference]
        [ListDrawerSettings (DefaultExpandedState = true, ElementColor = "GetElementColor")]
        public List<ICombatUnitValidationFunction> checks;
        
        [ListDrawerSettings (DefaultExpandedState = true, ElementColor = "GetElementColor")]
        public List<ISubsystemFunctionGeneral> functions = new List<ISubsystemFunctionGeneral> ();
    }
    
    public class DataBlockSubsystemFunctionsTargeted : DataBlockSubsystemFunction
    {
        [ValueDropdown ("@FieldReflectionUtility.GetConstantStringFieldValues (typeof (PartEventsTargeted), false)")]
        public string context = string.Empty;
        
        [DropdownReference]
        [ListDrawerSettings (DefaultExpandedState = true, ElementColor = "GetElementColor")]
        public List<ICombatUnitValidationFunction> checks;
        
        [ListDrawerSettings (DefaultExpandedState = true, ElementColor = "GetElementColor")]
        public List<ISubsystemFunctionTargeted> functions = new List<ISubsystemFunctionTargeted> ();
    }
    
    public class DataBlockSubsystemFunctionsAction : DataBlockSubsystemFunction
    {
        [ValueDropdown ("@FieldReflectionUtility.GetConstantStringFieldValues (typeof (PartEventsAction), false)")]
        public string context = string.Empty;
        
        [ValueDropdown ("@DataMultiLinkerAction.data.Keys")]
        public List<string> actionKeys = new List<string> ();
        
        [DropdownReference]
        [ListDrawerSettings (DefaultExpandedState = true, ElementColor = "GetElementColor")]
        public List<ICombatUnitValidationFunction> checks;

        [ListDrawerSettings (DefaultExpandedState = true, ElementColor = "GetElementColor")]
        public List<ISubsystemFunctionAction> functions = new List<ISubsystemFunctionAction> ();
    }

    public class DataBlockSubsystemAttachment
    {
        [ValueDropdown ("GetVisualKeys")]
        public string key;
        public bool centered = false;
        public bool activated = true;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale = new Vector3 (1f, 1f, 1f);

        public DataBlockSubsystemAttachment () { }
        public DataBlockSubsystemAttachment (DataBlockSubsystemAttachment source)
        {
            key = source.key;
            position = source.position;
            rotation = source.rotation;
            scale = source.scale;
        }

        private static IEnumerable<string> GetVisualKeys ()
        {
            var visuals = ItemHelper.GetAllVisuals ();
            return visuals != null && visuals.Count > 0 ? visuals.Keys : null;
        }

        public void OffsetPosition (Vector3 positionChange, bool local)
        {
            if (local)
                positionChange = Quaternion.Euler (rotation) * positionChange;

            SetPosition (position + positionChange, false);
        }

        public void SetPosition (Vector3 positionNew, bool local)
        {
            if (local)
                positionNew = Quaternion.Euler (rotation) * positionNew;

            position = positionNew;
            position.x = Mathf.Clamp (position.x, -10f, 10f);
            position.y = Mathf.Clamp (position.y, -10f, 10f);
            position.z = Mathf.Clamp (position.z, -10f, 10f);
        }

        public void OffsetRotation (Vector3 rotationChange)
        {
            SetRotation (rotation + rotationChange);
        }

        public void SetRotation (Vector3 rotationChange)
        {
            rotation = rotationChange;
            rotation.x = WrapAngle (rotation.x);
            rotation.y = WrapAngle (rotation.y);
            rotation.z = WrapAngle (rotation.z);
        }

        private float WrapAngle (float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }

        public void OffsetScale (Vector3 scaleChange)
        {
            SetScale (scale + scaleChange);
        }

        public void SetScale (Vector3 scaleChange)
        {
            scale = scaleChange;
            var limit = 0.01f;

            if (Mathf.Abs (scale.x) < limit)
                scale.x = scale.x < 0f ? -limit : limit;

            if (Mathf.Abs (scale.y) < limit)
                scale.y = scale.y < 0f ? -limit : limit;

            if (Mathf.Abs (scale.z) < limit)
                scale.z = scale.z < 0f ? -limit : limit;
        }

        public void SetVectorAxis (int mode, bool offset, float input, string axis)
        {
            var vector = offset ? Vector3.zero : GetVector (mode);
            if (string.Equals (axis, "x", StringComparison.InvariantCultureIgnoreCase))
                vector.x = input;
            if (string.Equals (axis, "y", StringComparison.InvariantCultureIgnoreCase))
                vector.y = input;
            if (string.Equals (axis, "z", StringComparison.InvariantCultureIgnoreCase))
                vector.z = input;

            SetVector (mode, offset, vector);
        }

        public void Mirror (string axis, bool positionUpdated, bool rotationUpdated, bool scaleUpdated)
        {
            var rotationQt = Quaternion.Euler (rotation);
            var directionForward = rotationQt * Vector3.forward;
            var directionUp = rotationQt * Vector3.up;

            if (string.Equals (axis, "x", StringComparison.InvariantCultureIgnoreCase))
            {
                directionForward = Vector3.Reflect (directionForward, Vector3.right);
                directionUp = Vector3.Reflect (directionUp, Vector3.right);
                if (rotationUpdated)
                    rotation = Quaternion.LookRotation (directionForward, directionUp).eulerAngles;
                if (positionUpdated)
                    position.x = -position.x;
                if (scaleUpdated)
                    scale.x = -scale.x;
            }
            if (string.Equals (axis, "y", StringComparison.InvariantCultureIgnoreCase))
            {
                directionForward = Vector3.Reflect (directionForward, Vector3.down);
                directionUp = Vector3.Reflect (directionUp, Vector3.down);
                if (rotationUpdated)
                    rotation = Quaternion.LookRotation (directionForward, directionUp).eulerAngles;
                if (positionUpdated)
                    position.y = -position.y;
                if (scaleUpdated)
                    scale.y = -scale.y;
            }
            if (string.Equals (axis, "z", StringComparison.InvariantCultureIgnoreCase))
            {
                directionForward = Vector3.Reflect (directionForward, Vector3.forward);
                directionUp = Vector3.Reflect (directionUp, Vector3.forward);
                if (rotationUpdated)
                    rotation = Quaternion.LookRotation (directionForward, directionUp).eulerAngles;
                if (positionUpdated)
                    position.z = -position.z;
                if (scaleUpdated)
                    scale.z = -scale.z;
            }
        }

        public Vector3 GetVector (int mode)
        {
            var vector = Vector3.zero;
            if (mode == 0)
                vector = position;
            else if (mode == 1)
                vector = rotation;
            else if (mode == 2)
                vector = scale;
            return vector;
        }

        public void SetVector (int mode, bool offset, Vector3 input)
        {
            if (mode == -1)
            {
                if (offset)
                    OffsetPosition (input, true);
                else
                    SetPosition (input, true);
            }
            else if (mode == 0)
            {
                if (offset)
                    OffsetPosition (input, false);
                else
                    SetPosition (input, false);
            }
            else if (mode == 1)
            {
                if (offset)
                    OffsetRotation (input);
                else
                    SetRotation (input);
            }
            else if (mode == 2)
            {
                if (offset)
                    OffsetScale (input);
                else
                    SetScale (input);
            }
        }
    }



    [Serializable][HideReferenceObjectPicker]
    public class DataBlockSubsystemStat
    {
        [HorizontalGroup ("A")]
        [HideLabel]
        public float value;

        [HorizontalGroup ("A")]
        [YamlIgnore, ShowIf ("IsReportVisible")]
        [HideLabel]
        public string report;

        [HideInInspector]
        public int targetMode = 0;

        [HorizontalGroup ("B")]
        [ShowIf ("IsTargetVisible")]
        [HideLabel]
        [ValueDropdown ("@DataMultiLinkerPartSocket.data.Keys")]
        [InlineButton ("@targetSocket = string.Empty", "-", ShowIf = "@!string.IsNullOrEmpty (targetSocket)")]
        public string targetSocket = string.Empty;

        [HorizontalGroup ("B")]
        [ShowIf ("IsTargetVisible")]
        [HideLabel]
        [ValueDropdown ("@DataMultiLinkerSubsystemHardpoint.data.Keys")]
        [InlineButton ("@targetHardpoint = string.Empty", "-", ShowIf = "@!string.IsNullOrEmpty (targetHardpoint)")]
        public string targetHardpoint = string.Empty;

        #region Editor
        #if UNITY_EDITOR

        private bool IsTargetVisible => targetMode > 0;
        private bool IsReportVisible => report != null;

        private string GetTargetModeText ()
        {
            if (targetMode == 0)
                return "+"; // value is added to sum in part
            else if (targetMode == 1)
                return "+% (TGT)"; // value is multiplied with target sum, the result is added to target sum
            else if (targetMode == 2)
                return "×% (TGT)"; // value multiplies the target sum
            else
                return "== (TGT)"; // value is forced to provided one
        }

        [PropertyOrder (-1)]
        [HorizontalGroup ("A", 80f)]
        [Button ("@GetTargetModeText ()")]
        private void SwitchTargetMode ()
        {
            targetMode += 1;
            if (targetMode > 3)
                targetMode = 0;

            if (targetMode != 0)
                targetSocket = targetHardpoint = string.Empty;
        }

        #endif
        #endregion

    }




    [Serializable][LabelWidth (180f)]// [HideReferenceObjectPicker]
    public class DataContainerSubsystem : DataContainerWithText, IDataContainerTagged
    {
        [YamlIgnore, ReadOnly]
        [ShowIf ("@IsUIVisible && !string.IsNullOrEmpty (groupMainKey)")]
        [ListDrawerSettings (DefaultExpandedState = false)]
        [LabelText ("Group (Main)")]
        public string groupMainKey;

        [YamlIgnore, ReadOnly]
        [ShowIf ("@IsUIVisible && groupFilterKeys != null && groupFilterKeys.Count > 0")]
        [ListDrawerSettings (DefaultExpandedState = false)]
        [LabelText ("Groups (Filtering)")]
        public List<string> groupFilterKeys;

        [ShowIf ("IsCoreVisible")]
        public bool hidden = false;

        [GUIColor ("GetParentColor")]
        [ShowIf ("IsCoreVisible")]
        [ValueDropdown ("@DataMultiLinkerSubsystem.data.Keys")]
        [SuffixLabel ("@parentHierarchy")]
        [OnValueChanged ("OnFullRefreshRequired")]
        public string parent = string.Empty;

        [ShowIf ("IsCoreVisible")]
        [YamlIgnore, ReadOnly, HideInInspector]
        public string parentHierarchy = string.Empty;

        [ShowIf ("IsCoreVisible")]
        [YamlIgnore, LabelText ("Children"), ReadOnly]
        [ListDrawerSettings (DefaultExpandedState = false)]
        public List<string> children = new List<string> ();

        [PropertyRange (0, 4)]
        [ShowIf ("IsCoreVisible")]
        public int rating = 1;

        [YamlIgnore, PropertyOrder (-1)]
        [ShowIf ("IsPartTextVisible")]
        [ShowInInspector, DisplayAsString (true)]
        [LabelText ("Part preset usage")]
        private string partPresetUsage => DataMultiLinkerPartPreset.TryGetFixedUsageDescription (this);

        [ShowIf ("@IsUIVisible")]
        [DropdownReference (true)]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        [ValueDropdown ("@DataMultiLinkerPartPreset.data.Keys")]
        public string textNameFromPreset;

        [ShowIf ("@IsUIVisible")]
        [DropdownReference (true)]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        public DataBlockEquipmentTextFromHardpoint textNameFromHardpoint;

        [ShowIf ("@IsUIVisible")]
        [DropdownReference]
        public DataBlockEquipmentTextName textName;

        [ShowIf ("@IsUIVisible && IsInheritanceVisible && textNameProcessed != null")]
        [YamlIgnore, ReadOnly]
        public DataBlockEquipmentTextName textNameProcessed;

        [ShowIf ("@IsUIVisible")]
        [DropdownReference (true)]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        [ValueDropdown ("@DataMultiLinkerPartPreset.data.Keys")]
        public string textDescFromPreset;

        [ShowIf ("@IsUIVisible")]
        [DropdownReference]
        public DataBlockEquipmentTextDesc textDesc;

        [ShowIf ("@IsUIVisible && IsInheritanceVisible && textDescProcessed != null")]
        [YamlIgnore, ReadOnly]
        public DataBlockEquipmentTextDesc textDescProcessed;




        [BoxGroup ("Hardpoints", false)]
        [ShowIf ("AreHardpointsVisible")]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        [ValueDropdown ("@DataMultiLinkerSubsystemHardpoint.data.Keys")]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false)]
        [DropdownReference]
        public List<string> hardpoints = new List<string> ();

        [BoxGroup ("Hardpoints", false)]
        [ShowIf ("AreHardpointsProcessedVisible")]
        [ValueDropdown ("@DataMultiLinkerSubsystemHardpoint.data.Keys")]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false)]
        [YamlIgnore, ReadOnly]
        public List<string> hardpointsProcessed = new List<string> ();


        [BoxGroup ("Tags", false)]
        [ShowIf ("AreTagsVisible")]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        [ValueDropdown ("@DataMultiLinkerSubsystem.tags")]
        [DropdownReference]
        public HashSet<string> tags = new HashSet<string> ();

        [BoxGroup ("Tags", false)]
        [ShowIf ("AreTagsProcessedVisible")]
        [ValueDropdown ("@DataMultiLinkerSubsystem.tags")]
        [YamlIgnore, ReadOnly]
        public HashSet<string> tagsProcessed = new HashSet<string> ();


        [BoxGroup ("Stats", false)]
        [ShowIf ("@AreStatsVisible && stats != null")]
        [OnValueChanged ("OnFullRefreshRequired")]
        [InlineButton ("ClearStatDistribution", "-", ShowIf = "IsStatDistributionUsed")]
        [ValueDropdown ("@DataMultiLinkerSubsystemStatDistributions.data.Keys")]
        public string statDistribution = string.Empty;

        [Space (4f)]
        [BoxGroup ("Stats", false)]
        [ShowIf ("AreStatsVisible")]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        [DisableIf ("IsStatDistributionUsed")]
        [DictionaryKeyDropdown ("@DataMultiLinkerUnitStats.data.Keys")]
        [DictionaryDrawerSettings (KeyColumnWidth = 200f)]
        [DropdownReference]
        public SortedDictionary<string, DataBlockSubsystemStat> stats = new SortedDictionary<string, DataBlockSubsystemStat> ();

        [BoxGroup ("Stats", false)]
        [ShowIf ("AreStatsProcessedVisible")]
        [DictionaryKeyDropdown ("@DataMultiLinkerUnitStats.data.Keys")]
        [DictionaryDrawerSettings (KeyColumnWidth = 200f)]
        [ReadOnly, YamlIgnore]
        public SortedDictionary<string, DataBlockSubsystemStat> statsProcessed = new SortedDictionary<string, DataBlockSubsystemStat> ();


        [ShowIf ("AreVisualsVisible")]
        [DropdownReference]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false)]
        [ValueDropdown ("GetVisualKeys")]
        public List<string> visuals;

        [ShowIf ("AreVisualsProcessedVisible")]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false)]
        [YamlIgnore, ReadOnly]
        public List<string> visualsProcessed;


        [ShowIf ("AreAttachmentsVisible")]
        [DropdownReference]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false)]
        public Dictionary<string, DataBlockSubsystemAttachment> attachments;

        [ShowIf ("AreAttachmentsProcessedVisible")]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false)]
        [YamlIgnore, ReadOnly]
        public Dictionary<string, DataBlockSubsystemAttachment> attachmentsProcessed;


        [ShowIf ("AreBlocksVisible")]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        [DropdownReference (true)]
        public DataBlockSubsystemActivation_V2 activation;

        [ShowIf ("@AreBlocksProcessedVisible && activationProcessed != null")]
        [YamlIgnore, ReadOnly]
        public DataBlockSubsystemActivation_V2 activationProcessed;


        [ShowIf ("AreBlocksVisible")]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        [DropdownReference (true)]
        public DataBlockSubsystemProjectile_V2 projectile;

        [ShowIf ("@AreBlocksProcessedVisible && projectileProcessed != null")]
        [YamlIgnore, ReadOnly]
        public DataBlockSubsystemProjectile_V2 projectileProcessed;


        [ShowIf ("AreBlocksVisible")]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        [DropdownReference (true)]
        public DataBlockSubsystemBeam_V2 beam;

        [ShowIf ("@AreBlocksProcessedVisible && beamProcessed != null")]
        [YamlIgnore, ReadOnly]
        public DataBlockSubsystemBeam_V2 beamProcessed;


        [ShowIf ("AreBlocksVisible")]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        [DropdownReference (true)]
        public DataBlockPartCustom custom;

        [ShowIf ("@AreBlocksProcessedVisible && customProcessed != null")]
        [YamlIgnore, ReadOnly]
        public DataBlockPartCustom customProcessed;



        [ShowIf ("AreBlocksVisible")]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        [DropdownReference (true)]
        public DataBlockSubsystemFunctions functions;

        [ShowIf ("@AreBlocksProcessedVisible && functions != null")]
        [YamlIgnore, ReadOnly]
        public DataBlockSubsystemFunctions functionsProcessed;



        public HashSet<string> GetTags (bool processed)
        {
            return processed ? tagsProcessed : tags;
        }

        public bool IsHidden () => hidden;

        public override void OnBeforeSerialization ()
        {
            base.OnBeforeSerialization ();

            if (projectile != null)
                projectile.OnBeforeSerialization ();
        }

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);

            if (key == parent)
                parent = null;

            if (projectile != null)
                projectile.OnAfterDeserialization (this);

            UpdateStatDistribution ();
        }

        public bool IsFlagPresent (string key)
        {
            var found = customProcessed != null && customProcessed.IsFlagPresent (key);
            return found;
        }

        public bool TryGetInt (string key, out int result, int fallback = default)
        {
            result = fallback;
            var found = customProcessed != null && customProcessed.TryGetInt (key, out result);
            if (!found)
                result = fallback;

            return found;
        }

        public bool TryGetFloat (string key, out float result, float fallback = default)
        {
            result = fallback;
            var found = customProcessed != null && customProcessed.TryGetFloat (key, out result);
            if (!found)
                result = fallback;

            return found;
        }

        public bool TryGetVector (string key, out Vector3 result, Vector3 fallback = default)
        {
            result = fallback;
            var found = customProcessed != null && customProcessed.TryGetVector (key, out result);
            if (!found)
                result = fallback;

            return found;
        }

        public bool TryGetString (string key, out string result, string fallback = default)
        {
            result = default;
            var found = customProcessed != null && customProcessed.TryGetString (key, out result);
            if (!found)
                result = fallback;

            return found;
        }


        public override void OnKeyReplacement (string keyOld, string keyNew)
        {
            base.OnKeyReplacement (keyOld, keyNew);

            var partPresets = DataMultiLinkerPartPreset.data;
            foreach (var kvp in partPresets)
            {
                var partPreset = kvp.Value;
                if (partPreset.genSteps == null)
                    continue;

                foreach (var step in partPreset.genSteps)
                {
                    if (step is AddHardpoints stepHardpoint && stepHardpoint.subsystemsInitial != null)
                    {
                        if (stepHardpoint.subsystemsInitial.Contains (keyOld))
                        {
                            stepHardpoint.subsystemsInitial.Remove (keyOld);
                            stepHardpoint.subsystemsInitial.Add (keyNew);
                            Debug.Log ($"Updated part preset {partPreset.key} hardpoints {stepHardpoint.hardpointsTargeted?.ToStringFormatted ()} with new subsystem blueprint key:\n{keyOld} -> {keyNew}");
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
                    if (partOverride.systems == null)
                        continue;

                    foreach (var hardpoint in hardpointsProcessed)
                    {
                        if (!partOverride.systems.ContainsKey (hardpoint))
                            continue;

                        var system = partOverride.systems[hardpoint];
                        if (system.resolver == null)
                            continue;

                        if (system.resolver is DataBlockSubsystemSlotResolverKeys resolverKeys && resolverKeys.keys != null)
                        {
                            for (int i = 0; i < resolverKeys.keys.Count; ++i)
                            {
                                if (resolverKeys.keys[i] != keyOld)
                                    continue;

                                resolverKeys.keys[i] = keyNew;
                                Debug.Log ($"Updated unit preset {kvp.Key} socket {kvp2.Key} hardpoint {hardpoint} with new subsystem blueprint key:\n{keyOld} -> {keyNew}");
                            }
                        }
                    }
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
                            if (partOverride.systems == null)
                                continue;

                            foreach (var hardpoint in hardpointsProcessed)
                            {
                                if (!partOverride.systems.ContainsKey (hardpoint))
                                    continue;

                                var system = partOverride.systems[hardpoint];
                                if (system.resolver == null)
                                    continue;

                                if (system.resolver is DataBlockSubsystemSlotResolverKeys resolverKeys && resolverKeys.keys != null)
                                {
                                    for (int i = 0; i < resolverKeys.keys.Count; ++i)
                                    {
                                        if (resolverKeys.keys[i] != keyOld)
                                            continue;

                                        resolverKeys.keys[i] = keyNew;
                                        Debug.Log ($"Updated scenario {kvp.Key} unit preset {kvp2.Key} socket {kvp3.Key} hardpoint {hardpoint} with new subsystem blueprint key:\n{keyOld} -> {keyNew}");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var subsystems = DataMultiLinkerSubsystem.data;
            foreach (var kvp in subsystems)
            {
                var subsystem = kvp.Value;
                if (subsystem.parent == keyOld)
                {
                    subsystem.parent = keyNew;
                    Debug.Log ($"Updated subsystem {kvp.Key} with new parent key:\n{keyOld} -> {keyNew}");
                }
            }

            var overworldEntities = DataMultiLinkerOverworldEntityBlueprint.data;
            foreach (var kvp in overworldEntities)
            {
                var blueprint = kvp.Value;
                if (blueprint.rewards == null || blueprint.rewards.blocks == null)
                    continue;

                var rewards = blueprint.rewards.blocks;
                foreach (var kvp2 in rewards)
                {
                    var blocks = kvp2.Value;
                    if (blocks == null)
                        continue;

                    foreach (var block in blocks)
                    {
                        if (block == null || block.subsystems == null)
                            continue;

                        foreach (var subsystem in block.subsystems)
                        {
                            if (subsystem == null || subsystem.tagsUsed || subsystem.blueprint != keyOld)
                                continue;

                            subsystem.blueprint = keyNew;
                            Debug.Log ($"Updated reward block in overworld entity {blueprint.key} reward {kvp2.Key} with subsystem key {keyOld} -> {keyNew}");
                        }
                    }
                }
            }

            DataMultiLinkerSubsystem.OnAfterDeserialization ();
        }

        private static List<DataContainerEquipmentGroup> groupsFound = new List<DataContainerEquipmentGroup> ();

        public void UpdateGroups ()
        {
            if (hidden)
                return;

            bool groupsPresent = groupFilterKeys != null;
            if (groupsPresent)
                groupFilterKeys.Clear ();

            groupsFound.Clear ();

            foreach (var kvp in DataMultiLinkerEquipmentGroup.data)
            {
                var group = kvp.Value;
                if (!group.subsystems)
                    continue;

                if (group.tagsSubsystem != null && group.tagsSubsystem.Count > 0)
                {
                    bool match = true;
                    foreach (var kvp2 in group.tagsSubsystem)
                    {
                        var tag = kvp2.Key;
                        var required = kvp2.Value;
                        var present = tagsProcessed.Contains (tag);

                        if (required != present)
                        {
                            match = false;
                            break;
                        }
                    }

                    if (!match)
                        continue;
                }

                groupsFound.Add (group);
            }

            foreach (var kvp in DataMultiLinkerEquipmentGroup.dataGenerated)
            {

            }

            int groupsCount = groupsFound.Count;
            if (groupsCount == 0)
                return;

            if (groupsCount > 1)
                groupsFound.Sort ((x, y) => x.priority.CompareTo (y.priority));

            foreach (var group in groupsFound)
            {
                if (group.visibleInName && string.IsNullOrEmpty (groupMainKey))
                    groupMainKey = group.key;

                if (group.visibleInFilters || group.visibleAsPerk)
                {
                    if (!groupsPresent)
                    {
                        groupsPresent = true;
                        groupFilterKeys = new List<string> ();
                    }

                    groupFilterKeys.Add (group.key);
                }
            }
        }

        public void UpdateStatDistribution ()
        {
            // Override stats collection using the parent
            if (string.IsNullOrEmpty (statDistribution))
                return;

            var subsystemParent = DataMultiLinkerSubsystem.GetEntry (parent);
            if (subsystemParent == null || subsystemParent.stats == null || subsystemParent.stats.Count == 0)
            {
                Debug.LogWarning ($"Subsystem {key} | Failed to apply stat distribution {statDistribution}: parent subsystem {parent} doesn't exist or contains no stats");
                return;
            }

            var data = DataMultiLinkerSubsystemStatDistributions.GetEntry (statDistribution, false);
            if (data == null || data.hardpoints == null || data.hardpoints.Count == 0)
            {
                Debug.LogWarning ($"Subsystem {key} | Failed to apply stat distribution {statDistribution}: such a distribution doesn't exist or has no hardpoints");
                return;
            }

            if (hardpoints == null || hardpoints.Count == 0)
            {
                Debug.LogWarning ($"Subsystem {key} | Failed to apply stat distribution {statDistribution}: no hardpoints defined so it's not possible to determine which multiplier to use");
                return;
            }

            string hardpointUsed = null;
            float hardpointMultiplier = 0f;

            foreach (var hardpointCandidate in hardpoints)
            {
                if (data.hardpoints.ContainsKey (hardpointCandidate))
                {
                    hardpointUsed = hardpointCandidate;
                    hardpointMultiplier = data.hardpoints[hardpointCandidate].multiplier;
                    break;
                }
            }

            if (hardpointUsed == null)
            {
                Debug.LogWarning ($"Subsystem {key} | Failed to apply stat distribution {statDistribution}: no hardpoint compatible with distribution | Subsystem hardpoints: {hardpoints.ToStringFormatted ()} | Distribution hardpoints: {data.hardpoints.ToStringFormattedKeys ()}");
                return;
            }

            if (stats == null)
                stats = new SortedDictionary<string, DataBlockSubsystemStat> ();
            else
                stats.Clear ();

            foreach (var kvp in subsystemParent.stats)
            {
                var statKey = kvp.Key;
                var blockParent = kvp.Value;
                var blockCurrent = new DataBlockSubsystemStat ();
                stats.Add (statKey, blockCurrent);

                blockCurrent.value = hardpointMultiplier;
                blockCurrent.targetMode = blockParent.targetMode;
                blockCurrent.targetSocket = blockParent.targetSocket;
                blockCurrent.targetHardpoint = blockParent.targetHardpoint;
            }
        }

        public override void ResolveText ()
        {
            if (textName != null)
                textName.s = DataManagerText.GetText (TextLibs.equipmentSubsystems, $"{key}__name");

            if (textDesc != null)
                textDesc.s = DataManagerText.GetText (TextLibs.equipmentSubsystems, $"{key}__text");
        }

        #region Editor
        #if UNITY_EDITOR

        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            if (textName != null)
                DataManagerText.TryAddingTextToLibrary (TextLibs.equipmentSubsystems, $"{key}__name", textName.s);

            if (textDesc != null)
                DataManagerText.TryAddingTextToLibrary (TextLibs.equipmentSubsystems, $"{key}__text", textDesc.s);
        }

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataContainerSubsystem () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        private bool IsInheritanceVisible => DataMultiLinkerSubsystem.Presentation.showInheritance;
        private bool IsCoreVisible => DataMultiLinkerSubsystem.Presentation.showCore;
        private bool IsUIVisible => DataMultiLinkerSubsystem.Presentation.showUI;

        private bool AreTagsVisible => DataMultiLinkerSubsystem.Presentation.showTags;
        private bool AreTagsProcessedVisible => AreTagsVisible && IsInheritanceVisible;

        private bool AreHardpointsVisible => DataMultiLinkerSubsystem.Presentation.showHardpoints;
        private bool AreHardpointsProcessedVisible => AreHardpointsVisible && IsInheritanceVisible;

        private bool AreVisualsVisible => DataMultiLinkerSubsystem.Presentation.showVisuals;
        private bool AreVisualsProcessedVisible => AreVisualsVisible && IsInheritanceVisible && visualsProcessed != null;

        private bool AreAttachmentsVisible => DataMultiLinkerSubsystem.Presentation.showVisuals;
        private bool AreAttachmentsProcessedVisible => AreAttachmentsVisible && IsInheritanceVisible && attachmentsProcessed != null;

        private bool AreStatsVisible => DataMultiLinkerSubsystem.Presentation.showStats;
        private bool AreStatsProcessedVisible => AreStatsVisible && IsInheritanceVisible && statsProcessed != null;

        private bool AreBlocksVisible => DataMultiLinkerSubsystem.Presentation.showBlocks;
        private bool AreBlocksProcessedVisible => AreBlocksVisible && IsInheritanceVisible;

        private bool IsPartTextVisible => DataMultiLinkerSubsystem.Presentation.showPartText && !string.IsNullOrEmpty (DataMultiLinkerPartPreset.TryGetFixedUsageDescription (this));

        private Color GetParentColor =>
            !string.IsNullOrEmpty (parent) && DataMultiLinkerSubsystem.data.ContainsKey (parent) ? Color.white : Color.yellow;

        private bool IsStatDistributionUsed => !string.IsNullOrEmpty (statDistribution);
        private void ClearStatDistribution ()
        {
            statDistribution = string.Empty;
            DataMultiLinkerSubsystem.ProcessRelated (this);
        }

        private static IEnumerable<string> GetVisualKeys ()
        {
            var visuals = ItemHelper.GetAllVisuals ();
            return visuals != null && visuals.Count > 0 ? visuals.Keys : null;
        }

        private void OnFullRefreshRequired ()
        {
            // It's useful to re-run whole post-deserialization process to refresh child list, validate parent links etc.
            if (DataMultiLinkerSubsystem.Presentation.autoUpdateInheritance)
                DataMultiLinkerSubsystem.ProcessRelated (this);

            #if !PB_MODSDK

            if (Application.isPlaying)
                DataHelperStats.RefreshStatCacheForSubsystem (this);

            #endif
        }

        private static string keyVisualizedLast = null;

        [EnableIf ("@AssetPackageHelper.AreUnitAssetsInstalled ()")]
        [Button ("Visualize (processed)"), ButtonGroup, PropertyOrder (-3)]
        [ShowIf ("AreVisualsVisible")]
        public void Visualize ()
        {
            bool focus = false;
            if (!string.Equals (keyVisualizedLast, key))
            {
                keyVisualizedLast = key;
                focus = true;
            }
            
            DataMultiLinkerSubsystem.VisualizeObject (this, true, focus: focus);
        }

        [EnableIf ("@AssetPackageHelper.AreUnitAssetsInstalled ()")]
        [Button ("Visualize (isolated)"), ButtonGroup, PropertyOrder (-3)]
        [ShowIf ("AreVisualsVisible")]
        public void VisualizeIsolaved ()
        {
            bool focus = false;
            if (!string.Equals (keyVisualizedLast, key))
            {
                keyVisualizedLast = key;
                focus = true;
            }
            
            DataMultiLinkerSubsystem.VisualizeObject (this, false, focus: focus);
        }

        #if !PB_MODSDK

        [Button ("Spawn"), ButtonGroup, HideInEditorMode, PropertyOrder (-2)]
        private void IssueToPlayer ()
        {
            if (!Application.isPlaying || !Contexts.sharedInstance.persistent.hasDataKeySave || hidden)
                return;

            var playerBasePersistent = IDUtility.playerBasePersistent;
            if (playerBasePersistent == null)
                return;

            var level = EquipmentUtility.debugGenerationLevel;
            var subsystemEntity = UnitUtilities.CreateSubsystemEntity (key);
            if (subsystemEntity == null)
                return;

            EquipmentUtility.AttachSubsystemToInventory (subsystemEntity, playerBasePersistent, true, true);
        }

        #endif

        #if PB_MODSDK
        bool enableGenerate => DataContainerModData.hasSelectedConfigs;

        [EnableIf ("@" + nameof(enableGenerate))]
        #endif
        [HideInPlayMode]
        [Button ("Generate workshop project"), PropertyOrder (-2)]
        private void ToWorkshop ()
        {
            string hardpointKeySelected = null;
            if (hardpointsProcessed != null)
            {
                foreach (var hardpointKey in hardpointsProcessed)
                {
                    var hardpointInfo = DataMultiLinkerSubsystemHardpoint.GetEntry (hardpointKey);
                    if (hardpointInfo != null && hardpointInfo.editable)
                    {
                        hardpointKeySelected = hardpointKey;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty (hardpointKeySelected))
            {
                Debug.LogWarning ($"Can't generate a workshop project: no editable hardpoints found");
                return;
            }

            var data = DataMultiLinkerWorkshopProject.data;
            var keyWorkshop = key.Replace ("internal_", "sub_");

            if (data.ContainsKey (keyWorkshop))
            {
                Debug.LogWarning ($"Workshop project with key {keyWorkshop} already present: ");
                return;
            }

            var p = new DataContainerWorkshopProject ();
            p.hidden = false;
            p.textSourceName = new DataBlockWorkshopTextSourceName { key = key, source = WorkshopTextSource.Subsystem };
            p.textSourceSubtitle = new DataBlockWorkshopTextSourceSubtitle { key = hardpointKeySelected, source = WorkshopTextSource.Hardpoint };
            p.textSourceDesc = new DataBlockWorkshopTextSourceDesc { key = key, source = WorkshopTextSource.Hardpoint };

            p.tags = new HashSet<string> { "group_utility", "type_subsystem" };
            p.icon = DataMultiLinkerEquipmentGroup.GetEntry (groupMainKey)?.icon;
            p.duration = new DataBlockFloat { f = 1f };
            p.inputResources = new List<DataBlockResourceCost>
            {
                new DataBlockResourceCost { key = ResourceKeys.supplies, amount = 1 },
                new DataBlockResourceCost { key = ResourceKeys.componentsR2, amount = 1 },
                new DataBlockResourceCost { key = ResourceKeys.componentsR3, amount = 1 }
            };

            p.outputSubsystems = new List<DataBlockWorkshopSubsystem> { new DataBlockWorkshopSubsystem { count = 1, key = key, tags = null } };

            data.Add (keyWorkshop, p);

            var linker = GameObject.FindObjectOfType<DataMultiLinkerWorkshopProject> ();
            if (linker != null)
                linker.SetFilterAndSelect (keyWorkshop);
        }

        #endif
        #endregion
    }
}
