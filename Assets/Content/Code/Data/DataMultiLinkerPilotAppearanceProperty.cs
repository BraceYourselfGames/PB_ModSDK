using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerPilotAppearanceProperty : DataMultiLinker<DataContainerPilotAppearanceProperty>
    {
        public DataMultiLinkerPilotAppearanceProperty ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.pilotAppearance); 
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
        }

        public static HashSet<string> groups = new HashSet<string> ();
        public static List<DataContainerPilotAppearanceProperty> dataSorted = new List<DataContainerPilotAppearanceProperty> ();
        
        public static void OnAfterDeserialization ()
        {
            UpdateSorting ();
        }

        public static void UpdateSorting ()
        {
            dataSorted.Clear ();
            foreach (var kvp in data)
                dataSorted.Add (kvp.Value);
            dataSorted.Sort (CompareForSorting);
            
            groups.Clear ();
            foreach (var config in dataSorted)
            {
                var group = config.group;
                if (!string.IsNullOrEmpty (group) && !groups.Contains (group))
                    groups.Add (group);
            }
        }

        protected class GroupChanges
        {
            [BoxGroup]
            public DataBlockPilotPropHeader header = new DataBlockPilotPropHeader
            {
                text = "Header",
                icon = "s_icon_l32_character1"
            };
            
            public string group;
            public int priority;

            [ValueDropdown ("@DataMultiLinkerPilotAppearanceProperty.data.Keys")]
            [ListDrawerSettings (DefaultExpandedState = true)]
            public List<string> entries = new List<string> ();

            [Button]
            private void AddFromGroup ([ValueDropdown ("@DataMultiLinkerPilotAppearanceProperty.groups")] string groupAdded)
            {
                if (string.IsNullOrEmpty (groupAdded))
                    return;

                var data = DataMultiLinkerPilotAppearanceProperty.data;
                foreach (var kvp in data)
                {
                    var config = kvp.Value;
                    if (config != null && string.Equals (config.group, groupAdded))
                        entries.Add (kvp.Key);
                        
                }
            }
        }
        
        [FoldoutGroup ("Utilities", false), ShowInInspector]
        [ListDrawerSettings (CustomAddFunction = "@new GroupChanges ()")]
        private static List<GroupChanges> groupChanges = new List<GroupChanges> ();
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        private static void ApplyGroupChanges ()
        {
            foreach (var groupChange in groupChanges)
            {
                if (groupChange == null || groupChange.entries == null || groupChange.entries.Count == 0)
                    continue;
                
                for (int i = 0; i < groupChange.entries.Count; i++)
                {
                    var key = groupChange.entries[i];
                    var config = GetEntry (key, false);
                    if (config == null)
                        continue;

                    if (groupChange.header != null)
                    {
                        if (i == 0)
                        {
                            config.header = new DataBlockPilotPropHeader
                            {
                                text = groupChange.header.text,
                                icon = groupChange.header.icon
                            };
                        }
                        else
                            config.header = null;
                    }

                    config.priority = groupChange.priority + i;
                    config.group = groupChange.group;
                }
            }
            
            UpdateSorting ();

            #if !PB_MODSDK
            if (Application.isPlaying)
            {
                if (CIViewBaseEditor.ins.IsEntered ())
                    CIViewBaseEditor.ins.RefreshView (true);
            }
            #endif
        }

        private static int CompareForSorting (DataContainerPilotAppearanceProperty c1, DataContainerPilotAppearanceProperty c2)
        {
            if (c1 == null && c2 == null) { return 0; }
            if (c1 == null) { return 1; }
            if (c2 == null) { return -1; }
            
            // int groupPriority = string.Compare (c1.group, c2.group, StringComparison.Ordinal);
            // if (groupPriority != 0)
            //     return groupPriority;

            int compPriority = c1.priority.CompareTo (c2.priority);
            if (compPriority != 0)
                return compPriority;

            return string.Compare (c1.key, c2.key, StringComparison.Ordinal);
        }

        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void GenerateSliderProperties ()
        {
            var dataFetched = data;
            if (dataFetched == null)
                return;
            
            var pilotData = DataShortcuts.pilots;
            if (pilotData.blendshapesForCustomization == null)
                return;

            foreach (var kvp in pilotData.blendshapesForCustomization)
            {
                var blendShapeKey = kvp.Key;
                var propConfigKey = $"bs_{kvp.Key}";
                DataContainerPilotAppearanceProperty config = null;
                if (dataFetched.TryGetValue (propConfigKey, out var configExisting) && configExisting != null)
                    config = configExisting;
                else
                {
                    config = new DataContainerPilotAppearanceProperty ();
                    config.key = propConfigKey;
                    data.Add (propConfigKey, config);
                }

                config.priority = 100;
                config.group = "blendshapes";
                config.implementation = new PilotPropBlendShape
                {
                    blendShapeKey = blendShapeKey
                };
            }
        }
    }
}


