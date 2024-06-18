using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [LabelWidth (120f)][HideReferenceObjectPicker]
    public class EquipmentLiveryPreview
    {
        public const float width = 88f;
        
        [HideInInspector]
        public bool showSelectButton = true;
        
        [HideInInspector]
        public string key;
        
        [HideInInspector]
        public DataContainerEquipmentLivery data;
            
        [HorizontalGroup ("color")]
        [ShowInInspector, HideLabel, CustomValueDrawer ("ColorDrawer")]
        public Color colorPrimary { get => data != null ? data.colorPrimary : Color.black; }
        
        [HorizontalGroup ("color")]
        [ShowInInspector, HideLabel, CustomValueDrawer ("ColorDrawer")]
        public Color colorSecondary { get => data != null ? data.colorSecondary : Color.black; }
        
        [HorizontalGroup ("color")]
        [ShowInInspector, HideLabel, CustomValueDrawer ("ColorDrawer")]
        public Color colorTertiary { get => data != null ? data.colorTertiary : Color.black; }

        public void Refresh (string key)
        {
            if (this.key != key)
            {
                this.key = key;
                data = DataMultiLinkerEquipmentLivery.GetEntry (key);
            }
        }

        #if UNITY_EDITOR
        
        [ShowIf ("showSelectButton")]
        [HorizontalGroup ("color", 32f)]
        [Button ("→")]
        private void Select ()
        {
            if (string.IsNullOrEmpty (key) || !DataMultiLinkerEquipmentLivery.data.ContainsKey (key))
                return;
            
            var linker = GameObject.FindObjectOfType<DataMultiLinkerEquipmentLivery> ();
            if (linker == null)
                return;

            UnityEditor.Selection.activeGameObject = linker.gameObject;
            linker.filter = key;
            linker.filterUsed = true;
            linker.ApplyFilter ();
        }
        
        private Color ColorDrawer (Color value, GUIContent label)
        {
            Sirenix.Utilities.Editor.SirenixEditorGUI.DrawSolidRect (16f, 16f, value.WithAlpha (1f), false);
            return value;
        }
        
        #endif
    }
    
    
    
    
    
    
        
    [HideReferenceObjectPicker]
    public class DataBlockUnitLiveryPresetNode
    {
        [ValueDropdown ("@DataMultiLinkerEquipmentLivery.data.Keys")]
        [HideLabel, HorizontalGroup, OnValueChanged ("RefreshPreview")]
        public string livery;
        
        [ShowIf ("@preview != null")]
        [NonSerialized, YamlIgnore, ShowInInspector, HideLabel, HorizontalGroup (EquipmentLiveryPreview.width)]
        public EquipmentLiveryPreview preview;
        
        public void RefreshPreview ()
        {
            if (preview == null) preview = new EquipmentLiveryPreview ();
            preview.Refresh (livery);
        }
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockUnitLiveryPresetSocket
    {
        [ShowIf ("IsNodePresent"), HideLabel]
        [InlineButton ("@node = null", "-")]
        public DataBlockUnitLiveryPresetNode node;
        
        [ShowIf ("AreHardpointsPresent"), HideLabel]
        [InlineButton ("@hardpoints = null", "-")]
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.Hardpoint)]
        [DictionaryDrawerSettings (KeyLabel = "Hardpoint", ValueLabel = "Override")]
        public Dictionary<string, DataBlockUnitLiveryPresetNode> hardpoints;

        private bool IsNodePresent => node != null;
        private bool AreHardpointsPresent => hardpoints != null;

        [PropertyOrder (-8)]
        [HideIf ("IsNodePresent")]
        [Button ("Add socket livery")]
        private void AddNode ()
        {
            if (node == null)
                node = new DataBlockUnitLiveryPresetNode ();
        }
        
        [PropertyOrder (-6)]
        [HideIf ("AreHardpointsPresent")]
        [Button ("Add hardpoints")]
        private void AddSockets ()
        {
            if (hardpoints == null)
                hardpoints = new Dictionary<string, DataBlockUnitLiveryPresetNode> ();
        }
    }
    
    
    
    

    
    [HideReferenceObjectPicker]
    public class DataBlockUnitLiveryHardpoint
    {
        [ValueDropdown ("@DataMultiLinkerEquipmentLivery.data.Keys")]
        [HideLabel, HorizontalGroup, OnValueChanged ("RefreshPreview")]
        public string livery;
        
        [ShowIf ("@preview != null")]
        [NonSerialized, YamlIgnore, ShowInInspector, HideLabel, HorizontalGroup (EquipmentLiveryPreview.width)]
        public EquipmentLiveryPreview preview;
        
        public void RefreshPreview ()
        {
            if (preview == null) preview = new EquipmentLiveryPreview ();
            preview.Refresh (livery);
        }
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockUnitLiverySocket
    {
        [ValueDropdown ("@DataMultiLinkerEquipmentLivery.data.Keys")]
        [HideLabel, HorizontalGroup, OnValueChanged ("RefreshPreview")]
        public string livery;
        
        [ShowIf ("@preview != null")]
        [NonSerialized, YamlIgnore, ShowInInspector, HideLabel, HorizontalGroup (EquipmentLiveryPreview.width)]
        public EquipmentLiveryPreview preview;

        [HideLabel, DictionaryDrawerSettings (KeyLabel = "Hardpoint", ValueLabel = "Override")]
        [ShowIf ("@overridesPerHardpoint != null")]
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.Hardpoint)]
        public Dictionary<string, DataBlockUnitLiveryHardpoint> overridesPerHardpoint;
        
        public void RefreshPreview ()
        {
            if (preview == null) preview = new EquipmentLiveryPreview ();
            preview.Refresh (livery);
        }

        private void AddOverrides ()
        {
            if (overridesPerHardpoint == null)
                overridesPerHardpoint = new Dictionary<string, DataBlockUnitLiveryHardpoint> ();
        }
    }
    
    
    
    
    
    [HideReferenceObjectPicker, LabelWidth (120f)]
    public class DataContainerUnitLiveryPreset : DataContainer
    {
        [PropertyOrder (-10)]
        [LabelText ("Name")]
        public string name;
        
        [PropertyOrder (-9)]
        [ValueDropdown ("@DataMultiLinkerUnitLiveryPreset.data.Keys")]
        [SuffixLabel ("@parentHierarchy")]
        [InlineButton ("@parent = string.Empty", "-", ShowIf = "@!string.IsNullOrEmpty (parent)")]
        [InlineButton ("TrySelectParent", "→", ShowIf = "@!string.IsNullOrEmpty (parent)")]
        public string parent = string.Empty;

        [YamlIgnore, ReadOnly, HideInInspector]
        public string parentHierarchy;
        
        [PropertyOrder (-8)]
        [ShowIf ("@IsNodePresent && !IsProcessingVisible"), HideLabel]
        [InlineButton ("@node = null", "-")]
        public DataBlockUnitLiveryPresetNode node;
        
        [PropertyOrder (-7)]
        [YamlIgnore, ReadOnly]
        [ShowIf ("@IsProcessingVisible && nodeProcessed != null"), HideLabel]
        public DataBlockUnitLiveryPresetNode nodeProcessed;
        
        [PropertyOrder (-6)]
        [ShowIf ("@AreSocketsPresent && !IsProcessingVisible"), HideLabel]
        [InlineButton ("@sockets = null", "-")]
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.Socket)]
        [DictionaryDrawerSettings (KeyLabel = "Socket", ValueLabel = "Override")]
        public Dictionary<string, DataBlockUnitLiveryPresetSocket> sockets;
        
        [PropertyOrder (-5)]
        [YamlIgnore, ReadOnly]
        [ShowIf ("@IsProcessingVisible && socketsProcessed != null"), HideLabel]
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.Socket)]
        [DictionaryDrawerSettings (KeyLabel = "Socket", ValueLabel = "Override")]
        public Dictionary<string, DataBlockUnitLiveryPresetSocket> socketsProcessed;
        
        


        
        
        [FoldoutGroup ("Legacy", false), HorizontalGroup ("Legacy/A")]
        [ValueDropdown ("@DataMultiLinkerEquipmentLivery.data.Keys")]
        [HideLabel, OnValueChanged ("RefreshPreview")]
        public string livery;
        
        [FoldoutGroup ("Legacy"), HorizontalGroup ("Legacy/A", EquipmentLiveryPreview.width)]
        [ShowIf ("@preview != null")]
        [NonSerialized, YamlIgnore, ShowInInspector, HideLabel]
        public EquipmentLiveryPreview preview;

        [FoldoutGroup ("Legacy")]
        [HideLabel, DictionaryDrawerSettings (KeyLabel = "Socket", ValueLabel = "Override")]
        [ShowIf ("@overridesPerSocket != null")]
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.Socket)]
        public Dictionary<string, DataBlockUnitLiverySocket> overridesPerSocket;

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
            RefreshPreview ();
            if (overridesPerSocket != null)
            {
                foreach (var kvp1 in overridesPerSocket)
                {
                    var overridePerSocket = kvp1.Value;
                    if (overridePerSocket == null)
                        continue;
                    
                    overridePerSocket.RefreshPreview ();
                    if (overridePerSocket.overridesPerHardpoint != null)
                    {
                        foreach (var kvp2 in overridePerSocket.overridesPerHardpoint)
                        {
                            var overridePerHardpoint = kvp2.Value;
                            if (overridePerHardpoint != null)
                                overridePerHardpoint.RefreshPreview ();
                        }
                    }
                }
            }
            
            if (node != null)
                node.RefreshPreview ();

            if (sockets != null)
            {
                foreach (var kvp1 in sockets)
                {
                    var socketInfo = kvp1.Value;
                    if (socketInfo == null)
                        continue;
                    
                    if (socketInfo.node != null)
                        socketInfo.node.RefreshPreview ();

                    if (socketInfo.hardpoints != null)
                    {
                        foreach (var kvp2 in socketInfo.hardpoints)
                        {
                            var harpdointInfo = kvp2.Value;
                            if (harpdointInfo != null)
                                harpdointInfo.RefreshPreview ();
                        }
                    }
                }
            }
        }

        public void RefreshPreview ()
        {
            if (preview == null) preview = new EquipmentLiveryPreview ();
            preview.Refresh (livery);
        }

        #if UNITY_EDITOR

        private void TrySelectParent ()
        {
            if (string.IsNullOrEmpty (parent) || !DataMultiLinkerUnitLiveryPreset.data.ContainsKey (parent))
                return;
            
            var linker = GameObject.FindObjectOfType<DataMultiLinkerUnitLiveryPreset> ();
            if (linker == null)
                return;

            UnityEditor.Selection.activeGameObject = linker.gameObject;
            linker.filter = parent;
            linker.filterUsed = true;
            linker.ApplyFilter ();
        }

        private bool IsProcessingVisible => DataMultiLinkerUnitLiveryPreset.Presentation.showProcessing;
        private bool IsNodePresent => node != null;
        private bool AreSocketsPresent => sockets != null;

        [PropertyOrder (-8)]
        [HideIf ("@IsNodePresent || IsProcessingVisible")]
        [Button ("Add root livery")]
        private void AddNode ()
        {
            if (node == null)
                node = new DataBlockUnitLiveryPresetNode ();
        }
        
        [PropertyOrder (-6)]
        [HideIf ("@AreSocketsPresent || IsProcessingVisible")]
        [Button ("Add sockets")]
        private void AddSockets ()
        {
            if (sockets == null)
                sockets = new Dictionary<string, DataBlockUnitLiveryPresetSocket> ();
        }
        
        #endif
    }
}

