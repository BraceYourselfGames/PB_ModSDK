using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PhantomBrigade.Data;
using PhantomBrigade.Mods;
using PhantomBrigade.SDK.ModTools;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PhantomBrigade.ModTools
{
    [Serializable, HideDuplicateReferenceBox]
    public class ModConfigEditSource
    {
        [PropertyOrder (12), BoxGroup ("dataLinker", false), LabelText ("Global DB edits")]
        [OnValueChanged ("UpdateLinkers", true)]
        [ListDrawerSettings (HideAddButton = true, DraggableItems = false)]
        [HideIf ("@dataLinkers == null")]
        public List<ModConfigEditLinker> dataLinkers;
        
        [PropertyOrder (22), BoxGroup ("dataMultiLinker", false), LabelText ("Collection DB edits")]
        [OnValueChanged ("UpdateMultiLinkers", true)]
        [ListDrawerSettings (HideAddButton = true, DraggableItems = false)]
        [HideIf ("@dataMultiLinkers == null")]
        public List<ModConfigEditMultiLinker> dataMultiLinkers;

        public void OnAfterDeserialization ()
        {
            #if UNITY_EDITOR
            UpdateLinkers ();
            UpdateMultiLinkers ();
            #endif
        }
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        [PropertyOrder (10), BoxGroup ("dataLinker"), HorizontalGroup ("dataLinker/Add"), HideLabel]
        [ValueDropdown ("GetLinkerKeysUnused")] 
        private string dataLinkerTypeAdded = string.Empty;

        [PropertyOrder (11), BoxGroup ("dataLinker"), HorizontalGroup ("dataLinker/Add", 200f)]
        [Button ("Add global DB edit", 21), EnableIf ("IsLinkerTypeSelectionValid")]
        private void AddLinkerEdit ()
        {
            var type = dataLinkerTypeAdded;
            dataLinkerTypeAdded = string.Empty;
            
            if (string.IsNullOrEmpty (type) || !GetLinkerKeys.Contains (type))
            {
                Debug.LogWarning ($"Can't add linker edit: type [{type}] is not valid");
                return;
            }
            
            if (dataLinkers != null)
            {
                foreach (var entry in dataLinkers)
                {
                    if (entry != null && string.Equals (entry.type, type))
                    {
                        Debug.LogWarning ($"Can't add linker edit: type [{type}] already in use");
                        return;
                    }
                }
            }
            
            if (dataLinkers == null)
                dataLinkers = new List<ModConfigEditLinker> ();
            
            dataLinkers.Add (new ModConfigEditLinker
            {
                type = type,
                file = new ModConfigEditSourceFile ()
            });
            
            UpdateLinkers ();
        }
        
        [ShowInInspector]
        [PropertyOrder (10), BoxGroup ("dataMultiLinker"), HorizontalGroup ("dataMultiLinker/Add"), HideLabel]
        [ValueDropdown ("GetMultiLinkerKeysUnused")] 
        private string dataMultiLinkerTypeAdded = string.Empty;

        [PropertyOrder (21), BoxGroup ("dataMultiLinker"), HorizontalGroup ("dataMultiLinker/Add", 200f)]
        [Button ("Add collection DB edit", 21), EnableIf ("IsMultiLinkerTypeSelectionValid")]
        private void AddMultiLinkerEdit ()
        {
            var type = dataMultiLinkerTypeAdded;
            dataMultiLinkerTypeAdded = string.Empty;
            AddMultiLinkerEdit (type);
        }

        public ModConfigEditMultiLinker AddMultiLinkerEdit (string type)
        {
            if (string.IsNullOrEmpty (type) || !GetMultiLinkerKeys.Contains (type))
            {
                Debug.LogWarning ($"Can't add multi-linker edit: type [{type}] is not valid");
                return null;
            } 
            
            if (dataMultiLinkers != null)
            {
                foreach (var entry in dataMultiLinkers)
                {
                    if (entry != null && string.Equals (entry.type, type))
                    {
                        Debug.LogWarning ($"Can't multi-linker edit: type [{type}] already in use");
                        return entry;
                    }
                }
            }

            if (dataMultiLinkers == null)
                dataMultiLinkers = new List<ModConfigEditMultiLinker> ();

            var mce = new ModConfigEditMultiLinker
            {
                type = type,
                edits = new List<ModConfigEditSourceFileMultiLinker> (),
            };
            dataMultiLinkers.Add (mce);
            
            UpdateMultiLinkers ();

            return mce;
        }
	    
        private Color GetElementColor (int index) => DataEditor.GetColorFromElementIndex (index);
        private List<string> typeNamesTemp = new List<string> ();

        public IEnumerable<string> GetLinkerKeys => UtilityDatabaseSerialization.GetLinkerTypeNames ();
       
        public IEnumerable<string> GetMultiLinkerKeys => UtilityDatabaseSerialization.GetMultiLinkerTypeNames ();
        
        public IEnumerable<string> GetLinkerKeysUnused => 
            GetTypeNamesUnused (UtilityDatabaseSerialization.GetLinkerTypeNames (), linkerTypeCoverage);
        public IEnumerable<string> GetMultiLinkerKeysUnused => 
            GetTypeNamesUnused (UtilityDatabaseSerialization.GetMultiLinkerTypeNames (), multiLinkerTypeCoverage);

        private bool IsLinkerTypeSelectionValid ()
        {
            var type = dataLinkerTypeAdded;
            if (string.IsNullOrEmpty (type) || !GetLinkerKeys.Contains (type))
                return false;

            var keysUnused = GetLinkerKeysUnused;
            return keysUnused != null && keysUnused.Contains (type);
        }
            
        
        private bool IsMultiLinkerTypeSelectionValid ()
        {
            var type = dataMultiLinkerTypeAdded;
            if (string.IsNullOrEmpty (type) || !GetMultiLinkerKeys.Contains (type))
                return false;

            var keysUnused = GetMultiLinkerKeysUnused;
            return keysUnused != null && keysUnused.Contains (type);
        }
        
        private IEnumerable<string> GetTypeNamesUnused (IEnumerable<string> typeNamesAll, Dictionary<string, int> typeUsageMap)
        {
            typeNamesTemp.Clear ();
            if (typeNamesAll != null)
            {
                foreach (var typeNameCandidate in typeNamesAll)
                {
                    if (typeUsageMap == null || !typeUsageMap.ContainsKey (typeNameCandidate))
                        typeNamesTemp.Add (typeNameCandidate);
                }
            }

            return typeNamesTemp;
        }

        private void UpdateLinkers () 
        {
            linkerTypeCoverage.Clear ();
            if (dataLinkers == null)
                return;
            
            for (int i = 0; i < dataLinkers.Count; ++i)
            {
                var entry = dataLinkers[i];
                if (entry == null)
                    continue;
                
                entry.parent = this;
                if (string.IsNullOrEmpty (entry.type))
                    entry.component = null;
                else
                {
                    entry.component = UtilityDatabaseSerialization.GetComponentForLinker (entry.type);
                    if (linkerTypeCoverage.ContainsKey (entry.type))
                        linkerTypeCoverage[entry.type] += 1;
                    else
                        linkerTypeCoverage.Add (entry.type, 1);
                }
            }
        }
        
        private void UpdateMultiLinkers ()
        {
            multiLinkerTypeCoverage.Clear ();
            if (dataMultiLinkers == null)
                return;
            
            for (int i = 0; i < dataMultiLinkers.Count; ++i)
            {
                var entry = dataMultiLinkers[i];
                if (entry == null)
                    continue;

                entry.parent = this;
                
                if (string.IsNullOrEmpty (entry.type))
                    entry.component = null;
                else
                {
                    entry.component = UtilityDatabaseSerialization.GetInterfaceForMultiLinker (entry.type);
                    if (multiLinkerTypeCoverage.ContainsKey (entry.type))
                        multiLinkerTypeCoverage[entry.type] += 1;
                    else
                        multiLinkerTypeCoverage.Add (entry.type, 1);
                }
                
                entry.OnEditsChanged ();
            }
        }
        
        private Dictionary<string, int> linkerTypeCoverage = new Dictionary<string, int> ();
        private Dictionary<string, int> multiLinkerTypeCoverage = new Dictionary<string, int> ();

        public bool IsLinkerTypeDuplicate (string type)
        {
            if (!string.IsNullOrEmpty (type) && linkerTypeCoverage != null && linkerTypeCoverage.TryGetValue (type, out var count))
                return count >= 2;
            return false;
        }
        
        public bool IsMultiLinkerTypeDuplicate (string type)
        {
            if (!string.IsNullOrEmpty (type) && multiLinkerTypeCoverage != null && multiLinkerTypeCoverage.TryGetValue (type, out var count))
                return count >= 2;
            return false;
        }

        public void SaveToMod (DataContainerModData modData)
        {
            if (modData == null)
            {
                Debug.LogWarning ("Can't save config edits, parent mod not provided");
                return;
            }

            var rootPath = modData.GetModPathProject ();
            if (!Directory.Exists (rootPath))
            {
                Debug.LogWarning ($"Can't save config edits, mod {modData.id} directory doesn't exist: {rootPath}");
                return;
            }

            var editPath = DataPathHelper.GetCombinedCleanPath (rootPath, "ConfigEdits");
            UtilitiesYAML.PrepareClearDirectory (editPath, false, false);
            Debug.Log ($"Exporting config edits for mod {modData.id} to {editPath}");
            
            if (dataLinkers != null)
            {
                int e = -1;
                foreach (var entry in dataLinkers)
                {
                    e += 1;
                    var editsSource = entry?.file?.edits;
                    if (editsSource == null || editsSource.Count == 0)
                        continue;
                    
                    if (entry.component == null)
                    {
                        Debug.LogWarning ($"Linker edit {e} ({entry.type}) skipped, no component found for type nam {entry.type}");
                        continue;
                    }

                    var linkerType = entry.component.GetType ();
                    var containerType = linkerType?.BaseType?.GenericTypeArguments.FirstOrDefault ();
                    var containerPath = DataPathUtility.GetPath (containerType);
                    
                    if (string.IsNullOrEmpty (containerPath))
                    {
                        Debug.LogWarning ($"Linker edit {e} ({entry.type}) skipped, no path found for type nam {entry.type}");
                        continue;
                    }

                    containerPath = containerPath.Replace ("Configs", "ConfigEdits");
                    containerPath = DataPathHelper.GetCombinedCleanPath (rootPath, containerPath) + ".yaml";

                    var editsSaved = new List<string> (editsSource.Count);
                    var config = new ModConfigEditSerialized { edits = editsSaved };
                    
                    for (int i = 0; i < editsSource.Count; ++i)
                    {
                        var editSource = editsSource[i];
                        var editSaved = GetEditSavedString (editSource);
                        if (!string.IsNullOrEmpty (editSaved))
                            editsSaved.Add (editSaved);
                    }
                    
                    UtilitiesYAML.SaveToFile (containerPath, config);
                    Debug.Log ($"Linker edit {e} ({entry.type}): \n- {containerPath}");
                }
            }
            
            if (dataMultiLinkers != null)
            {
                int e = -1;
                foreach (var entry in dataMultiLinkers)
                {
                    e += 1;
                    if (entry == null || entry.edits == null || entry.edits.Count == 0)
                        continue;
                    
                    if (entry.component == null)
                    {
                        Debug.LogWarning ($"Multi-linker edit {e} ({entry.type}) skipped, no component found for type name {entry.type}");
                        continue;
                    }

                    var linkerType = entry.component.GetType ();
                    var containerType = linkerType?.BaseType?.GenericTypeArguments.FirstOrDefault ();
                    var dirPath = DataPathUtility.GetPath (containerType);
                    
                    if (string.IsNullOrEmpty (dirPath))
                    {
                        Debug.LogWarning ($"Multi-linker edit {e} ({entry.type}) skipped, no path found for type name {entry.type}");
                        continue;
                    }
                    
                    dirPath = dirPath.Replace ("Configs", "ConfigEdits");
                    dirPath = DataPathHelper.GetCombinedCleanPath (rootPath, dirPath);

                    foreach (var configEdit in entry.edits)
                    {
                        if (configEdit == null || string.IsNullOrEmpty (configEdit.key))
                        {
                            continue;
                        }
                        var editsSource = configEdit.edits;
                        if (editsSource == null || (editsSource.Count == 0 && !configEdit.removed))
                        {
                            continue;
                        }

                        var key = configEdit.key;
                        var config = new ModConfigEditSerialized ()
                        {
                            removed = configEdit.removed,
                        };
                        var editsSaved = new List<string> ();
                        config.edits = editsSaved;

                        if (!configEdit.removed)
                        {
                            for (int i = 0; i < editsSource.Count; ++i)
                            {
                                var editSource = editsSource[i];
                                var editSaved = GetEditSavedString (editSource);
                                if (!string.IsNullOrEmpty (editSaved))
                                    editsSaved.Add (editSaved);
                            }
                        }

                        var containerPath = DataPathHelper.GetCombinedCleanPath (dirPath, key) + ".yaml";
                        UtilitiesYAML.SaveToFile (containerPath, config);
                        Debug.Log ($"Multi-linker edit {e} ({entry.type}) {key}: \n- {containerPath}");
                    }
                }
            }
        }
        
        private ModConfigEditStep GetEditSaved (ModConfigEditSourceLine editSource)
        {
            if (editSource == null)
            {
                return null;
            }

            var valueFinal = editSource.value;
            if (operationMap.TryGetValue ((int)editSource.operation, out var op))
            {
                switch (editSource.operation)
                {
                    case ModUtilities.EditOperation.NullValue:
                    case ModUtilities.EditOperation.DefaultValue:
                    case ModUtilities.EditOperation.Remove:
                        valueFinal = op;
                        break;
                    case ModUtilities.EditOperation.Add:
                        valueFinal += op;
                        break;
                }
            }
                        
            return new ModConfigEditStep
            {
                path = editSource.path,
                value = valueFinal,
            };
        }
        
        private string GetEditSavedString (ModConfigEditSourceLine editSource)
        {
            if (editSource == null)
            {
                return null;
            }

            var valueFinal = editSource.value;
            if (operationMap.TryGetValue ((int)editSource.operation, out var op))
            {
                switch (editSource.operation)
                {
                    case ModUtilities.EditOperation.NullValue:
                    case ModUtilities.EditOperation.DefaultValue:
                    case ModUtilities.EditOperation.Remove:
                        valueFinal = op;
                        break;
                    case ModUtilities.EditOperation.Add:
                        valueFinal += " " + op;
                        break;
                }
            }

            return editSource.path + ": " + valueFinal;
        }

        static readonly Dictionary<int, string> operationMap = new Dictionary<int, string> ()
        {
            [(int)ModUtilities.EditOperation.NullValue] = ModUtilities.Constants.Operator.NullValue,
            [(int)ModUtilities.EditOperation.DefaultValue] = ModUtilities.Constants.Operator.DefaultValue,
            [(int)ModUtilities.EditOperation.Add] = ModUtilities.Constants.Operator.Insert,
            [(int)ModUtilities.EditOperation.Remove] = ModUtilities.Constants.Operator.Remove,
        };
        #endif
        #endregion
    }
    
    [HideReferenceObjectPicker, HideDuplicateReferenceBox]
    public class ModConfigEditLinker
    {
        [HorizontalGroup]
        [GUIColor ("GetTypeColor")]
        [HideLabel, ValueDropdown ("GetLinkerTypes"), ReadOnly]
        public string type;
        
        [HideInInspector, YamlIgnore]
        public Component component;

        [HideLabel]
        public ModConfigEditSourceFile file = new ModConfigEditSourceFile ();
        
        #region Editor
        #if UNITY_EDITOR

        [HideInInspector, YamlIgnore]
        public ModConfigEditSource parent = null;
        
        private IEnumerable<string> GetLinkerTypes => parent?.GetLinkerKeysUnused;
        private bool IsTypeDuplicate => parent != null && parent.IsLinkerTypeDuplicate (type);
        private Color GetTypeColor => IsTypeDuplicate || !IsComponentPresent ? ModToolsColors.HighlightError : Color.white;
        private bool IsComponentPresent => component != null;

        [HorizontalGroup (80f)]
        [Button ("Open"), ShowIf (nameof(IsComponentPresent))]
        private void SelectComponent ()
        {
            if (IsComponentPresent)
                Selection.activeGameObject = component.gameObject;
        }

        #endif

        #endregion
    }

    [HideReferenceObjectPicker, HideDuplicateReferenceBox]
    public class ModConfigEditMultiLinker
    {
        [HorizontalGroup]
        [GUIColor ("GetTypeColor")]
        [HideLabel, ValueDropdown ("GetMultiLinkerTypes"), ReadOnly]
        public string type;
        
        [HideInInspector, YamlIgnore]
        public IDataMultiLinker component;

        [PropertyOrder (11)]
        [OnValueChanged ("OnEditsChanged", true)]
        [ListDrawerSettings (HideAddButton = true, DraggableItems = false)]
        public List<ModConfigEditSourceFileMultiLinker> edits = new List<ModConfigEditSourceFileMultiLinker> ();
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        [PropertyOrder (10), HorizontalGroup ("Add"), HideLabel]
        [ValueDropdown ("GetConfigKeysUnused")] 
        private string configKeyAdded = string.Empty;

        [PropertyOrder (11), HorizontalGroup ("Add")]
        [Button ("Add config edit", 21), EnableIf ("IsConfigKeyValid")]
        private void AddFileEdit ()
        {
            if (parent == null)
            {
                return;
            }
            var keyNew = configKeyAdded;
            configKeyAdded = string.Empty;
            AddFileEdit (keyNew);
        }

        public void AddFileEdit(string keyNew, bool removed = false)
        {
            var keys = GetConfigKeysAll ();
            if (keys == null)
            {
                return;
            }
            if (string.IsNullOrEmpty (type) || !keys.Contains (keyNew))
            {
                Debug.LogWarning ($"Can't add config edit: key [{keyNew}] is not valid");
                return;
            }

            if (edits != null)
            {
                foreach (var editExisting in edits)
                {
                    if (editExisting != null && string.Equals (editExisting.key, keyNew))
                    {
                        if (editExisting.removed == removed)
                        {
                            Debug.LogWarning ($"Can't add config edit: key [{keyNew}] already in use");
                        }
                        else if (removed)
                        {
                            editExisting.removed = true;
                        }
                        return;
                    }
                }
            }

            edits ??= new List<ModConfigEditSourceFileMultiLinker> ();
            edits.Add (new ModConfigEditSourceFileMultiLinker
            {
                key = keyNew,
                removed = removed,
                edits = new List<ModConfigEditSourceLine> ()
            });

            OnEditsChanged ();
        }

        public void OnEditsChanged ()
        {
            if (edits == null)
                return;
            
            edits.Sort ((x, y) => string.Compare (x.key, y.key, StringComparison.Ordinal));
            foreach (var edit in edits)
                edit.parent = this;
        }
        
        [HideInInspector, YamlIgnore]
        public ModConfigEditSource parent = null;
        
        private IEnumerable<string> GetMultiLinkerTypes => parent?.GetMultiLinkerKeysUnused;
        private bool IsTypeDuplicate => parent != null && parent.IsMultiLinkerTypeDuplicate (type);
        private Color GetTypeColor => IsTypeDuplicate || !IsComponentPresent ? ModToolsColors.HighlightError : Color.white;
        private bool IsComponentPresent => component != null;

        [HorizontalGroup (80f)]
        [Button ("Open"), ShowIf (nameof(IsComponentPresent))]
        private void SelectComponent ()
        {
            if (IsComponentPresent)
                Selection.activeGameObject = component.GetObject ();
        }

        private IEnumerable<string> GetConfigKeysAll ()
        {
            if (component == null)
                return null;
            return component.GetKeysLocal ();
        }
        
        private List<string> configKeysUsed = new List<string> ();
        private List<string> GetConfigKeysUsed ()
        {
            configKeysUsed.Clear ();
            if (edits != null)
            {
                foreach (var edit in edits)
                    configKeysUsed.Add (edit.key);
            }
            return configKeysUsed;
        }
        
        private List<string> configKeysUnused = new List<string> ();
        private List<string> GetConfigKeysUnused ()
        {
            var keysUsed = GetConfigKeysUsed ();
            var keysAll = GetConfigKeysAll ();
            
            configKeysUnused.Clear ();
            if (keysAll != null)
            {
                foreach (var key in keysAll)
                {
                    if (keysUsed == null || !keysUsed.Contains (key))
                        configKeysUnused.Add (key);
                }
            }
            return configKeysUnused;
        }

        private bool IsConfigKeyValid ()
        {
            var keysUsed = GetConfigKeysUsed ();
            var keysAll = GetConfigKeysAll ();
            return !string.IsNullOrEmpty (configKeyAdded) && !keysUsed.Contains (configKeyAdded) && keysAll.Contains (configKeyAdded);
        }
        
        #endif
        #endregion
    }
    
    [HideDuplicateReferenceBox, HideReferenceObjectPicker]
    public class ModConfigEditSourceFile
    {
        [ListDrawerSettings (CustomAddFunction = "@new ModConfigEditSourceLine ()")]
        public List<ModConfigEditSourceLine> edits = new List<ModConfigEditSourceLine> ();
    }
    
    public class ModConfigEditSourceFileMultiLinker : ModConfigEditSourceFile
    {
        [HorizontalGroup]
        [PropertyOrder (-2), ReadOnly, HideLabel]
        public string key;
        
        [PropertyOrder (-1)]
        public bool removed;
        
        [HideInInspector, YamlIgnore]
        public ModConfigEditMultiLinker parent = null;
        
        #if UNITY_EDITOR
        
        private bool IsComponentPresent => parent != null && parent.component != null;

        [HorizontalGroup (80f)]
        [Button ("Open"), ShowIf (nameof(IsComponentPresent))]
        private void SelectConfig ()
        {
            if (parent == null || parent.component == null)
                return;

            var multiLinker = parent.component;
            multiLinker.SelectObject ();
            multiLinker.SetFilter (true, key, true);
        }
        
        #endif
    }
    
    [LabelWidth (80f)]
    [HideDuplicateReferenceBox, HideReferenceObjectPicker]
    public class ModConfigEditSourceLine
    {
        [HideLabel, SuffixLabel ("Path", true)]
        public string path;
        
        [PropertyOrder (2)]
        [HorizontalGroup, HideLabel, SuffixLabel ("Value", true)]
        [ShowIf ("IsValueVisible")]
        public string value;
        
        [PropertyOrder (1)]
        [HorizontalGroup (120f), HideLabel]
        public ModUtilities.EditOperation operation;
        
        #region Editor
        #if UNITY_EDITOR

        private bool IsValueVisible => operation == ModUtilities.EditOperation.Add || operation == ModUtilities.EditOperation.Override;
        
        #endif
        #endregion
    }
}
