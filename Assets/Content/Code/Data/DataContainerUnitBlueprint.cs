using System;
using System.Collections.Generic;
using PhantomBrigade.Functions;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [Serializable]
    public class DataBlockUnitBlueprintSocket
    {
        [ValueDropdown ("@DataHelperUnitEquipment.GetPartPresetsForSocket (socket, true)")]
        public string presetDefault;
        public bool presetFiltered = true;
        
        public bool critical;
        public bool stabilizing;
        
        [HideInInspector][YamlIgnore]
        public string socket;
        
        [HideInInspector][YamlIgnore]
        public DataContainerUnitBlueprint parent;

        [HideLabel, BoxGroup ("UI")]
        public DataBlockUnitSocketUI ui;
    }
    
    [Serializable]
    public class DataBlockUnitSocketUI
    {
        [DataEditor.SpriteNameAttribute (true, 32f)]
        public string blueprintSpriteName;

        public bool textOverride;

        [InfoBox ("Text override off. This text is fetched from socket DB text sector. It can not be modified.", VisibleIf = "IsTextWarningVisible")]
        [GUIColor ("GetTextColor")]
        [LabelText ("Name / Desc.")][YamlIgnore]
        public string textName;

        [GUIColor ("GetTextColor")]
        [HideLabel, TextArea (1, 10)][YamlIgnore]
        public string textDesc;
        
        #if UNITY_EDITOR

        private bool IsTextWarningVisible => !textOverride;
        private Color GetTextColor => !textOverride ? Color.HSVToRGB (0.4f, 0.5f, 1f) : new Color (1f, 1f, 1f, 1f);

        #endif
    }
    
    [Serializable]
    public class DataBlockUnitBlueprintTrainingSocket
    {
        [ValueDropdown ("@DataMultiLinkerSubsystemHardpoint.data.Keys")]
        public HashSet<string> hardpointBlocklist = new HashSet<string> ();
        
        [DictionaryKeyDropdown ("@DataMultiLinkerSubsystemHardpoint.data.Keys")]
        [DictionaryValueDropdown ("@DataMultiLinkerSubsystem.data.Keys")]
        public Dictionary<string, string> subsystemOverrides = new Dictionary<string, string> ();
    }

    [Serializable]
    public class DataBlockUnitDestructionFragments
    {
        [ValueDropdown("GetKeys")]
        public string key = "fx_projectile_debris";

        [PropertyRange (1, 30)]
        public int count = 10;
        
        [PropertyRange (0.05f, 5f)]
        public float lifetime = 2f;
        
        [PropertyRange (0f, 100f)]
        public float speed = 30f;
        
        [PropertyRange (-2f, 2f)]
        public float gravity = 0.66f;
        
        [PropertyRange (0f, 1f)]
        public float damage = 0.1f;
        
        [PropertyRange (0f, 100f)]
        public float impact = 10f;
        
        [PropertyRange (0f, 1f)]
        public float scatterVertical = 0.2f;
        
        #if UNITY_EDITOR
        private IEnumerable<string> GetKeys () => DataMultiLinkerSubsystem.data.Keys;
        #endif
    }

    [Serializable][LabelWidth (180f)]
    public class DataContainerUnitBlueprint : DataContainerWithText
    {
        [YamlIgnore]
        [LabelText ("Name / Desc.")]
        public string textName;
        
        [YamlIgnore]
        [TextArea (1, 10), HideLabel]
        public string textDesc;
        
        [ValueDropdown ("GetClassTags")]
        public string classTag;
        public string classIcon;
        
        public string asset;
        public string assetPreview;

        public string audioEventOnMovementStart;
        public string audioEventOnMovementStop;
        
        public string audioSyncOnMovementElevation;
        public string audioSyncOnMovementTilt;

        //Speed and movement
        public float movementSpeedBase;
        public float movementSpeedMin;
        public float movementSpeedMax;
        
        public float dashDistanceBase;
        public float dashDistanceMin;
        public float dashDistanceMax;

        public float rotationSpeed;
        public float secondaryRotationSpeedLimit;
        public float secondaryRotationSpeedK;
        
        public string hitTableKey;
        public bool movementRestricted;
        public bool crashSpin = true;
        public bool crashPossible = true;

        public Vector3 centerPoint;

        public bool bodyTagPinningExempted = true;
        public bool salvageExempted = false;
        public int salvageBudgetBonus = 0;
		public float heatMax = 100.0f;
        
        [InlineButton ("SetDefaultParts", "Fill parts")]
        [InlineButton ("SetDefaultDependencies", "Fill dependencies")]
		public float heatOverloadConstant = 2.9f;
        
        [DropdownReference]
        [DictionaryKeyDropdown ("@DataMultiLinkerPartPreset.tags")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> partPresetFilter = new SortedDictionary<string, bool> ();

        [DictionaryKeyDropdown (DictionaryKeyDropdownType.Socket)]
        public SortedDictionary<string, DataBlockUnitBlueprintSocket> sockets = new SortedDictionary<string, DataBlockUnitBlueprintSocket> ();
        
        [DropdownReference]
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.Socket)]
        [DictionaryValueDropdown (DictionaryValueDropdownType.Socket)]
        public SortedDictionary<string, string> dependencies = new SortedDictionary<string, string> ();
        
        [DropdownReference (true)]
        public DataBlockUnitDestructionFragments destructionFragments;

        [DropdownReference]
        public List<ICombatFunctionTargeted> destructionFunctions;
        
        [DropdownReference (true)]
        public DataBlockScenarioUnitUncrewed uncrewed;

        [Tooltip("Used by the AI system to find and load a behavior tree for this unit type (if controlled by AI)")]
		public string aiBehavior;
        [Tooltip("How large will the AI system consider the unit, when planning to avoid collision?")]
        public float aiCollisionRadius = 4.283f; //The default is set to match the size of a tank collider
        [Tooltip("How tall is the cylinder describing the ai unity volume?")]
        public float aiCollisionHeight = 10f;

        public override void OnAfterDeserialization (string key)
        {
            foreach (var kvp in sockets)
            {
                var socketData = kvp.Value;
                socketData.parent = this;
                socketData.socket = kvp.Key;
            }
            
            base.OnAfterDeserialization (key);
        }

        public override void OnKeyReplacement (string keyOld, string keyNew)
        {
            base.OnKeyReplacement (keyOld, keyNew);

            var unitPresets = DataMultiLinkerUnitPreset.data;
            foreach (var kvp in unitPresets)
            {
                var unitPreset = kvp.Value;
                if (unitPreset.blueprintProcessed != keyOld)
                    continue;

                unitPreset.blueprintProcessed = keyNew;
                Debug.Log ($"Updated unit preset {kvp.Key} with new unit blueprint key: {keyOld} -> {keyNew}");
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
                        if (presetEmbedded.preset == null || presetEmbedded.preset.blueprintProcessed != keyOld)
                            continue;
                        
                        presetEmbedded.preset.blueprintProcessed = keyNew;
                        Debug.Log ($"Updated unit preset {kvp2.Key} in scenario {scenario.key} with new unit blueprint key: {keyOld} -> {keyNew}");
                    }
                }
            }
            
            var checks = DataMultiLinkerUnitCheck.data;
            foreach (var kvp in checks)
            {
                var check = kvp.Value;
                if (check.check.blueprints == null)
                    continue;

                foreach (var checkBlueprint in check.check.blueprints)
                {
                    if (checkBlueprint.blueprint != keyOld)
                        continue;

                    checkBlueprint.blueprint = keyNew;
                    Debug.Log ($"Updated unit check {check.key} with new blueprint key: {keyOld} -> {keyNew}");
                }
            }
            
            var actions = DataMultiLinkerAction.data;
            foreach (var kvp in actions)
            {
                var action = kvp.Value;
                if (action.dataCore == null || action.dataCore.check == null || action.dataCore.check.blueprints == null)
                    continue;

                foreach (var checkBlueprint in action.dataCore.check.blueprints)
                {
                    if (checkBlueprint.blueprint != keyOld)
                        continue;

                    checkBlueprint.blueprint = keyNew;
                    Debug.Log ($"Updated combat action {kvp.Key} unit check with new blueprint key: {keyOld} -> {keyNew}");
                }
            }
        }

        public DataBlockUnitSocketUI GetPartSocketInfo (string socket)
        {
            if (string.IsNullOrEmpty (socket) || sockets == null || !sockets.ContainsKey (socket))
                return null;

            var socketData = sockets[socket];
            return socketData.ui;
        }

        #if UNITY_EDITOR

        private void SetDefaultParts ()
        {
            sockets = new SortedDictionary<string, DataBlockUnitBlueprintSocket> ();
            foreach (var kvp in DataMultiLinkerPartSocket.data)
                sockets.Add (kvp.Key, new DataBlockUnitBlueprintSocket ());
        }

        private void SetDefaultDependencies ()
        {
            dependencies = new SortedDictionary<string, string> ();
            dependencies.Add (LoadoutSockets.leftOptionalPart, LoadoutSockets.leftEquipment);
            dependencies.Add (LoadoutSockets.rightOptionalPart, LoadoutSockets.rightEquipment);
        }

        private static List<string> GetClassTags () { return DataHelperUnitEquipment.GetClassTags (); }
        private static IEnumerable<string> GetSockets () { return DataHelperUnitEquipment.GetSockets (); }

        #endif
        
        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.unitBlueprints, $"{key}__header");
            textDesc = DataManagerText.GetText (TextLibs.unitBlueprints, $"{key}__text");
            
            foreach (var kvp in sockets)
            {
                var socketKey = kvp.Key;
                var socketData = kvp.Value;
                if (socketData.ui != null)
                {
                    if (socketData.ui.textOverride)
                    {
                        socketData.ui.textName = DataManagerText.GetText (TextLibs.unitBlueprints, $"{key}__vs_{socketKey}_header");
                        socketData.ui.textDesc = DataManagerText.GetText (TextLibs.unitBlueprints, $"{key}__vs_{socketKey}_text");
                    }
                    else
                    {
                        socketData.ui.textName = DataManagerText.GetText (TextLibs.equipmentSockets, $"{socketKey}__name");
                        socketData.ui.textDesc = DataManagerText.GetText (TextLibs.equipmentSockets, $"{socketKey}__text");
                    }
                }
            }
        }

        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerUnitBlueprint () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.unitBlueprints, $"{key}__header", textName);
            DataManagerText.TryAddingTextToLibrary (TextLibs.unitBlueprints, $"{key}__text", textDesc);
            
            foreach (var kvp in sockets)
            {
                var socketKey = kvp.Key;
                var socketData = kvp.Value;
                if (socketData.ui != null && socketData.ui.textOverride)
                {
                    DataManagerText.TryAddingTextToLibrary (TextLibs.unitBlueprints, $"{key}__vs_{socketKey}_header", socketData.ui.textName);
                    DataManagerText.TryAddingTextToLibrary (TextLibs.unitBlueprints, $"{key}__vs_{socketKey}_text", socketData.ui.textDesc);
                }
            }
        }
        
        #endif
        #endregion
    }
}

