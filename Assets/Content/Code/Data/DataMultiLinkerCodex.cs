using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerCodex : DataMultiLinker<DataContainerCodex>
    {
        public DataMultiLinkerCodex ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.codex); 
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
        }

        public class NavLookupGroup
        {
            public string groupKey;
            public List<string> entryKeys;
        }
        
        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool redrawOnChanges = false;
        }

        [ShowInInspector] [HideLabel]
        public Presentation presentation = new Presentation ();
        
        private static List<NavLookupGroup> navLookup = new List<NavLookupGroup> ();
        private static List<DataContainerCodex> navLookupSortedGroups = new List<DataContainerCodex> ();
        private static Dictionary<string, List<DataContainerCodex>> navLookupSortedChildren = new Dictionary<string, List<DataContainerCodex>> ();

        public static List<NavLookupGroup> GetNavLookup ()
        {
            LoadDataChecked ();
            return navLookup;
        }

        public static void OnAfterDeserialization ()
        {
            navLookup.Clear ();
            navLookupSortedGroups.Clear ();
            navLookupSortedChildren.Clear ();

            var s = data;
            foreach (var kvp in s)
            {
                var entry = kvp.Value;
                if (entry == null || string.IsNullOrEmpty (entry.key))
                    continue;

                if (!entry.listed)
                    continue;
                
                bool parentUsed = !string.IsNullOrEmpty (entry.parent) && s.ContainsKey (entry.parent);
                if (parentUsed)
                {
                    if (!navLookupSortedChildren.ContainsKey (entry.parent))
                        navLookupSortedChildren.Add (entry.parent, new List<DataContainerCodex> { entry } );
                    else
                        navLookupSortedChildren[entry.parent].Add (entry);
                }
                else
                {
                    navLookupSortedGroups.Add (entry);
                    if (!navLookupSortedChildren.ContainsKey (entry.key))
                        navLookupSortedChildren.Add (entry.key, new List<DataContainerCodex> ());
                }
            }

            navLookupSortedGroups.Sort ((x, y) => x.priority.CompareTo (y.priority));
            foreach (var kvp in navLookupSortedChildren)
            {
                var children = kvp.Value;
                children.Sort ((x, y) => x.priority.CompareTo (y.priority));
            }

            foreach (var group in navLookupSortedGroups)
            {
                var entryKeys = new List<string> ();
                var entries = navLookupSortedChildren.TryGetValue (group.key, out var v) ? v : null;
                if (entries != null)
                {
                    foreach (var entry in entries)
                        entryKeys.Add (entry.key);
                }
                
                var groupListing = new NavLookupGroup
                {
                    groupKey = group.key,
                    entryKeys = entryKeys
                };
                
                navLookup.Add (groupListing);
            }
        }

        [Button, FoldoutGroup ("Utilities", false)]
        private void GenerateCatalog ()
        {
            var partPresets = DataMultiLinkerPartPreset.data;
            foreach (var kvp in partPresets)
            {
                var preset = kvp.Value;
                if (preset.hidden)
                    continue;
                
                if (preset.tagsProcessed == null || preset.tagsProcessed.Contains (EquipmentTags.incompatible))
                    continue;
                
                var key = $"catalog_part_{kvp.Key}";
                DataContainerCodex entry = null;
                
                if (data.ContainsKey (key))
                    entry = data[key];
                else
                {
                    entry = new DataContainerCodex ();
                    data.Add (key, entry);
                }
                
                entry.key = key;
                entry.parent = "catalog_part";
                entry.textTitle = preset.GetPartModelName ();
                entry.sections = new List<ICodexSection>
                {
                    new CodexSectionTextSimple
                    {
                        text = preset.GetPartModelDesc ()
                    }
                };
            }
        }
    }
}


