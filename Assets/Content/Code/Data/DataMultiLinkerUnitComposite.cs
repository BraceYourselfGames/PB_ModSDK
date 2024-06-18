using System.Collections.Generic;
using System.Linq;
using System.Text;
using PhantomBrigade.Functions;
using UnityEngine;
using Sirenix.OdinInspector;


namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerUnitComposite : DataMultiLinker<DataContainerUnitComposite>
    {
        public DataMultiLinkerUnitComposite ()
        {
            textSectorKeys = new List<string> { TextLibs.unitComposites };
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            DataMultiLinkerUtility.RegisterOnTextExport 
            (
                dataType, 
                () => TextLibraryHelper.OnBeforeTextExport (dataType, TextLibs.unitComposites),
                () => TextLibraryHelper.OnAfterTextExport (dataType, TextLibs.unitComposites)
            );
        }
        
        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool showInheritance = false;
            
            [ShowInInspector]
            public static bool showTagCollections = false;
        }

        [ShowInInspector, HideLabel, FoldoutGroup ("View options")]
        public Presentation presentation = new Presentation ();
        
        [ShowIf ("@DataMultiLinkerUnitComposite.Presentation.showTagCollections")]
        [ShowInInspector] //, ReadOnly]
        public static HashSet<string> tags = new HashSet<string> ();
        
        [ShowIf ("@DataMultiLinkerUnitComposite.Presentation.showTagCollections")]
        [ShowInInspector, ReadOnly]
        public static Dictionary<string, HashSet<string>> tagsMap = new Dictionary<string, HashSet<string>> ();
        
        public static DataBlockUnitCompositeDirector selectedDirector;
        public static DataBlockUnitDirectorNavigationOption selectedNavOption;
        public static DataBlockAreaPoint selectedNavPoint;
        
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
                presetA.children.Clear ();
                
                foreach (var kvp2 in data)
                {
                    var presetB = kvp2.Value;
                    if (presetB.parents == null || presetB.parents.Count == 0)
                        continue;

                    foreach (var link in presetB.parents)
                    {
                        if (link.key == key)
                            presetA.children.Add (presetB.key);
                    }
                }
            }
            
            DataTagUtility.RegisterTags (data, ref tags, ref tagsMap);
            
            foreach (var kvp1 in data)
                Postprocess (kvp1.Value);
        }


        private static List<DataContainerUnitComposite> compositesUpdated = new List<DataContainerUnitComposite> ();

        public static void ProcessRelated (DataContainerUnitComposite origin)
        {
            if (origin == null)
                return;

            compositesUpdated.Clear ();
            compositesUpdated.Add (origin);
            
            if (origin.children != null)
            {
                foreach (var childKey in origin.children)
                {
                    var composite = GetEntry (childKey);
                    if (composite != null)
                        compositesUpdated.Add (composite);
                }
            }
            
            foreach (var composite in compositesUpdated)
            {
                // Avoid localization refresh and other losses on origin
                if (composite != origin)
                    composite.OnAfterDeserialization (composite.key);
            }

            foreach (var composite in compositesUpdated)
                ProcessRecursiveStart (composite);

            foreach (var composite in compositesUpdated)
                Postprocess (composite);

            // if (Presentation.logUpdates)
            //     Debug.Log ($"Updated {compositesUpdated.Count} unit composites: {compositesUpdated.ToStringFormatted (toStringOverride: (a) => a.key)}");
        }

        private static void ProcessRecursiveStart (DataContainerUnitComposite origin)
        {
            if (origin == null)
                return;
            
            origin.coreProcessed = null;
            origin.uiProcessed = null;
            origin.tagsProcessed = null;
            origin.layoutProcessed = null;
            origin.directorProcessed = null;
            origin.eventsProcessed = null;
            
            if (origin.parents != null)
            {
                foreach (var parent in origin.parents)
                {
                    if (parent != null)
                        parent.hierarchy = string.Empty;
                }
            }
                
            ProcessRecursive (origin, origin, 0);
        }

        private static void Postprocess (DataContainerUnitComposite composite)
        {
            // Add any code that requires fully filled processed fields here, e.g. procgen text based on merged data
            composite.SortDirectorNodes ();

            if (composite.directorProcessed != null && composite.directorProcessed.nodes != null)
            {
                var nodes = composite.directorProcessed.nodes;
                for (int i = nodes.Count - 1; i >= 0; --i)
                {
                    var nodeRoot = nodes[i];
                    
                    if (nodeRoot.removedInProcessing)
                        nodes.RemoveAt (i);
                }
            }
            
            if (composite.layoutProcessed != null && composite.layoutProcessed.units != null)
            {
                var units = composite.layoutProcessed.units;
                var keys = units.Keys.ToList ();
                for (int i = keys.Count - 1; i >= 0; --i)
                {
                    var unitKey = keys[i];
                    var unit = units[unitKey];

                    if (unit.removedInProcessing)
                        units.Remove (unitKey);
                }
            }
        }

        private static void ProcessRecursive (DataContainerUnitComposite current, DataContainerUnitComposite root, int depth)
        {
            if (current == null || root == null)
            {
                Debug.LogWarning ($"Received null step or root composite reference while validating unit composite hierarchy");
                return;
            }
            
            if (depth > 0 && current == root)
            {
                Debug.LogWarning ($"Encountered dependency loop at depth level {depth} when processing unit composite {root.key}");
                return;
            }

            // Replace block whole
            if (current.core != null && root.coreProcessed == null)
                root.coreProcessed = current.core;
            
            // Replace block whole
            if (current.ui != null)
            {
                if (root.uiProcessed == null)
                    root.uiProcessed = new DataBlockUnitCompositeUI ();

                if (current.ui.textName != null)
                    root.uiProcessed.textName = current.ui.textName;

                if (current.ui.textType != null)
                    root.uiProcessed.textType = current.ui.textType;
            }
            
            if (current.tags != null)
            {
                if (root.tagsProcessed == null)
                    root.tagsProcessed = new HashSet<string> ();
                
                foreach (var tag in current.tags)
                {
                    if (!string.IsNullOrEmpty (tag) && !root.tagsProcessed.Contains (tag))
                        root.tagsProcessed.Add (tag);
                }
            }
            
            // Merge block
            if (current.layout != null && current.layout.units != null)
            {
                if (root.layoutProcessed == null)
                    root.layoutProcessed = new DataBlockUnitCompositeLayout { units = new SortedDictionary<string, DataBlockUnitComposite> () };
                
                foreach (var kvp in current.layout.units)
                {
                    var unitKey = kvp.Key;
                    var unit = kvp.Value;
                    if (unit == null)
                        continue;

                    if (!root.layoutProcessed.units.TryGetValue (unitKey, out var unitProcessed))
                    {
                        unitProcessed = new DataBlockUnitComposite ();
                        root.layoutProcessed.units.Add (unitKey, unitProcessed);
                    }

                    if (!string.IsNullOrEmpty (unit.preset) && string.IsNullOrEmpty (unitProcessed.preset))
                        unitProcessed.preset = unit.preset;
                    
                    if (unit.textName != null && unitProcessed.textName == null)
                        unitProcessed.textName = unit.textName;
                    
                    if (unit.textDesc != null && unitProcessed.textDesc == null)
                        unitProcessed.textDesc = unit.textDesc;

                    if (unit.comment != null && unitProcessed.comment == null)
                        unitProcessed.comment = unit.comment;

                    if (unit.spawnInfo != null && unitProcessed.spawnInfo == null)
                        unitProcessed.spawnInfo = unit.spawnInfo;
                        
                    if (unit.spawnCustomization != null && unitProcessed.spawnCustomization == null)
                        unitProcessed.spawnCustomization = unit.spawnCustomization;
                        
                    if (unit.spawnFunctions != null && unitProcessed.spawnFunctions == null)
                        unitProcessed.spawnFunctions = unit.spawnFunctions;
                    
                    if (unit.assignableEventsDestruction != null && unitProcessed.assignableEventsDestruction == null)
                        unitProcessed.assignableEventsDestruction = unit.assignableEventsDestruction;
                        
                    if (unit.linkDamageRedirect != null && unitProcessed.linkDamageRedirect == null)
                        unitProcessed.linkDamageRedirect = unit.linkDamageRedirect;
                        
                    if (unit.linkTransform != null && unitProcessed.linkTransform == null)
                        unitProcessed.linkTransform = unit.linkTransform;
                    
                    if (unit.linksConditional != null && unitProcessed.linksConditional == null)
                        unitProcessed.linksConditional = unit.linksConditional;
                    
                    if (unit.legStepBlocklist != null && unitProcessed.legStepBlocklist == null)
                        unitProcessed.legStepBlocklist = unit.legStepBlocklist;
                }
            }
            
            // Merge the director data block
            if (current.director != null)
            {
                // If processed director instance is not yet created, make one
                if (root.directorProcessed == null)
                    root.directorProcessed = new DataBlockUnitCompositeDirector { nodes = new List<DataBlockUnitDirectorNodeRoot> () };
                
                if (current.director.booting != null)
                {
                    var bootingCurrent = current.director.booting;
                    var bootingProcessed = root.directorProcessed.booting;
                    
                    // If processed booting config is not yet created, make one
                    if (bootingProcessed == null)
                    {
                        // Fill the field with a new instance
                        bootingProcessed = new DataBlockUnitDirectorBooting ();
                        root.directorProcessed.booting = bootingProcessed;

                        // Assign value fields, as in other cases of with processed instances
                        bootingProcessed.evaluateFacing = bootingCurrent.evaluateFacing;
                        bootingProcessed.evaluateNavigation = bootingCurrent.evaluateNavigation;
                        bootingProcessed.evaluateNodes = bootingCurrent.evaluateNodes;
                    }

                    // If a current booting config has functions, insert them into processed instance
                    if (bootingCurrent.functions != null)
                    {
                        if (bootingProcessed.functions == null)
                            bootingProcessed.functions = new List<ICombatFunction> ();

                        foreach (var function in bootingCurrent.functions)
                        {
                            if (function != null)
                                bootingProcessed.functions.Add (function);
                        }
                    }
                    
                    // If a current booting config has functions per child, insert them into processed instance
                    if (bootingCurrent.functionsPerChild != null)
                    {
                        if (bootingProcessed.functionsPerChild == null)
                            bootingProcessed.functionsPerChild = new List<DataBlockUnitChildFunctions> ();

                        foreach (var function in bootingCurrent.functionsPerChild)
                        {
                            if (function != null)
                                bootingProcessed.functionsPerChild.Add (function);
                        }
                    }
                }
                
                if (current.director.facing != null && root.directorProcessed.facing == null)
                    root.directorProcessed.facing = current.director.facing;
                
                if (current.director.navigation != null && root.directorProcessed.navigation == null)
                    root.directorProcessed.navigation = current.director.navigation;

                if (current.director.nodes != null)
                {
                    var nodesRootProcessed = root.directorProcessed.nodes;
                    foreach (var node in current.director.nodes)
                    {
                        if (node == null)
                            continue;
                        
                        DataBlockUnitDirectorNodeRoot nodeRoot = null;
                        if (!string.IsNullOrEmpty (node.name))
                        {
                            for (int i = 0; i < nodesRootProcessed.Count; ++i)
                            {
                                var nodeRootCandidate = nodesRootProcessed[i];
                                if (nodeRootCandidate == null || string.IsNullOrEmpty (nodeRootCandidate.name))
                                    continue;

                                if (nodeRootCandidate.name == node.name)
                                {
                                    nodeRoot = nodeRootCandidate;
                                    break;
                                }
                            }
                        }

                        // Just add a node that wasn't yet encountered to the list
                        if (nodeRoot == null)
                            nodesRootProcessed.Add (node);
                        
                        // Alternative implementation, assemble root processed nodes piece by piece
                        // Of limited use since it only allows composition of root nodes when nodes are deeply nested
                        // It's more likely to cause issues with live editing than to be of help in real use cases, so we skip this for now
                        /*
                        if (nodeRoot == null)
                        {
                            nodeRoot = new DataBlockUnitDirectorNodeRoot
                            {
                                name = node.name, 
                                childMode = node.childMode, 
                                enabled = node.enabled
                            };
                            
                            nodesRootProcessed.Add (nodeRoot);
                        }
                        
                        if (node.comment != null && nodeRoot.comment == null)
                            nodeRoot.comment = node.comment;
                        
                        if (node.color != null && nodeRoot.color == null)
                            nodeRoot.color = node.color;

                        if (node.selfChange != null && nodeRoot.selfChange == null)
                            nodeRoot.selfChange = node.selfChange;
                        
                        if (node.functionGroups != null && nodeRoot.functionGroups == null)
                            nodeRoot.functionGroups = node.functionGroups;
                        
                        if (node.children != null && nodeRoot.children == null)
                            nodeRoot.children = node.children;
                        
                        if (node.turn != null && nodeRoot.turn == null)
                            nodeRoot.turn = node.turn;
                        
                        if (node.turnModulus != null && nodeRoot.turnModulus == null)
                            nodeRoot.turnModulus = node.turnModulus;
                        
                        if (node.unitSelfCheck != null && nodeRoot.unitSelfCheck == null)
                            nodeRoot.unitSelfCheck = node.unitSelfCheck;
                        
                        if (node.unitConnectedCheck != null && nodeRoot.unitConnectedCheck == null)
                            nodeRoot.unitConnectedCheck = node.unitConnectedCheck;
                        
                        if (node.unitFilterCheck != null && nodeRoot.unitFilterCheck == null)
                            nodeRoot.unitFilterCheck = node.unitFilterCheck;
                        
                        if (node.unitFilterCount != null && nodeRoot.unitFilterCount == null)
                            nodeRoot.unitFilterCount = node.unitFilterCount;
                        
                        if (node.memoryBase != null && nodeRoot.memoryBase == null)
                            nodeRoot.memoryBase = node.memoryBase;
                        */
                    }
                }
            }

            if (current.events != null)
            {
                if (root.eventsProcessed == null)
                    root.eventsProcessed = new DataBlockUnitCompositeEvents ();

                if (current.events.eventsAssignable != null)
                {
                    if (root.eventsProcessed.eventsAssignable == null)
                        root.eventsProcessed.eventsAssignable = new SortedDictionary<string, DataBlockUnitCompositeAssignedEvent> ();

                    foreach (var kvp in current.events.eventsAssignable)
                    {
                        var eventKey = kvp.Key;
                        var eventData = kvp.Value;
                        
                        if (eventData == null)
                            continue;

                        if (!root.eventsProcessed.eventsAssignable.TryGetValue (eventKey, out var eventDataProcessed))
                        {
                            eventDataProcessed = new DataBlockUnitCompositeAssignedEvent ();
                            root.eventsProcessed.eventsAssignable.Add (eventKey, eventDataProcessed);
                        }

                        if (eventData.unitCount != null && eventDataProcessed.unitCount == null)
                            eventDataProcessed.unitCount = eventData.unitCount;
                        
                        if (eventData.ui != null && eventDataProcessed.ui == null)
                            eventDataProcessed.ui = eventData.ui;
                        
                        if (eventData.functions != null && eventDataProcessed.functions == null)
                            eventDataProcessed.functions = eventData.functions;
                        
                        if (eventData.functionsTargeted != null && eventDataProcessed.functionsTargeted == null)
                            eventDataProcessed.functionsTargeted = eventData.functionsTargeted;
                    }
                }
                
                if (current.events.eventsSpatial != null)
                {
                    if (root.eventsProcessed.eventsSpatial == null)
                        root.eventsProcessed.eventsSpatial = new SortedDictionary<string, List<DataBlockUnitCompositeSpatialEffect>> ();

                    foreach (var kvp in current.events.eventsSpatial)
                    {
                        var eventKey = kvp.Key;
                        var eventData = kvp.Value;
                        
                        if (!root.eventsProcessed.eventsSpatial.ContainsKey (eventKey))
                            root.eventsProcessed.eventsSpatial.Add (eventKey, eventData);
                    }
                }
            }

            if (current.events != null && root.eventsProcessed == null)
                root.eventsProcessed = current.events;

            // Just in case we somehow missed a cyclical dependency
            if (depth > 20)
            {
                Debug.LogWarning ($"Unit composite {current.key} fails to complete recursive processing in under 20 steps.");
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
                    Debug.LogWarning ($"Unit composite {current.key} has null or empty parent link at index {i}!");
                    continue;
                }

                if (link.key == current.key)
                {
                    Debug.LogWarning ($"Unit composite {current.key} has invalid parent key matching itself at index {i}");
                    continue;
                }

                var parent = GetEntry (link.key, false);
                if (parent == null)
                {
                    Debug.LogWarning ($"Unit composite {current.key} has invalid parent key {link.key} at index {i} that can't be found in the database");
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
        
        #if UNITY_EDITOR
        
        [BoxGroup ("Assets", false), GUIColor (1f, 0.9f, 0.8f)]
        [HideIf ("@AssetPackageHelper.AreUnitAssetsInstalled ()")]
        [InfoBox ("@AssetPackageHelper.unitAssetWarning", InfoMessageType.Warning)]
        [Button (SdfIconType.Download, "Download package"), PropertyOrder (-3)]
        public void DownloadAssets ()
        {
            Application.OpenURL (AssetPackageHelper.levelAssetURL);
        }
        
        public static void UpdateUnlocalizedCommsInFunctions (List<ICombatFunction> functions, string messageKey)
        {
            if (functions == null || string.IsNullOrEmpty (messageKey))
                return;

            bool found = false;
            
            for (int i = functions.Count - 1; i >= 0; --i)
            {
                var fn = functions[i];
                if (fn is CombatLogMessageUnlocalized fnMessage)
                {
                    functions.RemoveAt (i);
                    functions.Add (new CombatCreateCommsMessage
                    {
                        key = messageKey
                    });

                    var dataFetched = DataMultiLinkerCombatComms.data;
                    if (dataFetched.TryGetValue (messageKey, out var comm))
                    {
                        Debug.LogWarning ($"Function {i} replaced | Used existing message {messageKey}");
                        break;
                    }
                    else
                    {
                        Debug.LogWarning ($"Function {i} replaced | Created new message {messageKey} with text: {fnMessage.text}");
                        DataMultiLinkerCombatComms.data.Add (messageKey, new DataContainerCombatComms
                        {
                            source = "enemy_boss_intercepted",
                            key = messageKey,
                            duration = 3f,
                            priority = false,
                            textContent = new List<string> { fnMessage.text }
                        });
                    
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
                return;
            
            DataMultiLinkerCombatComms.SaveData ();
            
            var linker = GameObject.FindObjectOfType<DataMultiLinkerCombatComms> ();
            if (linker == null)
                return;

            UnityEditor.Selection.activeGameObject = linker.gameObject;
            linker.filterUsed = true;
            linker.ApplyFilter ();
            linker.SaveText ();
        }

        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-10)]
        public void SetLegsIndestructible ()
        {
            foreach (var kvp in data)
            {
                var bp = kvp.Value;
                if (bp.layout != null && bp.layout.units != null)
                {
                    foreach (var kvp2 in bp.layout.units)
                    {
                        var unitKey = kvp2.Key;
                        var unit = kvp2.Value;
                        if (unit == null || !unitKey.Contains ("leg"))
                            continue;

                        if (unit.spawnCustomization == null)
                            unit.spawnCustomization = new DataBlockScenarioUnitCustomization ();

                        if (unit.spawnCustomization.combatTags == null)
                            unit.spawnCustomization.combatTags = new HashSet<string> ();

                        if (!unit.spawnCustomization.combatTags.Contains (ScenarioUnitTags.ReducedDamageIn))
                        {
                            unit.spawnCustomization.combatTags.Add (ScenarioUnitTags.ReducedDamageIn);
                            Debug.Log ($"{bp.key} / {unitKey}: marked indestructible");
                        }

                    }
                }
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-10)]
        public void LogBootingConfigs ()
        {
            foreach (var kvp in data)
            {
                var bp = kvp.Value;
                if (bp.director != null && bp.director.booting != null)
                {
                    Debug.Log ($"{bp.key}: has boot config");
                }
            }
        }
        
        #endif
    }
}
