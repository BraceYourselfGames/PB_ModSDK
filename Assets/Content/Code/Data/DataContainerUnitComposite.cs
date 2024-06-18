using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PhantomBrigade.Functions;
using PhantomBrigade.Functions.Equipment;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public class DataBlockUnitLinkTransform
    {
        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerUnitComposite\", \"GetUnitKeys\")")]
        public string unitKey;

        public Vector3 position;
        public Vector3 rotation;

        [InlineButtonClear]
        public DataBlockUnitLinkTransformSecondary secondary;
    }

    public class DataBlockUnitLinkTransformSecondary
    {
        public enum SecondaryStartMode
        {
            Ignore,
            Animate,
            Apply
        }

        [BoxGroup]
        public SecondaryStartMode startMode = SecondaryStartMode.Ignore;

        [ShowIf ("@startMode == SecondaryStartMode.Animate")]
        public Vector2 startAnimTimings = new Vector2 (0f, 1f);

        public Vector3 position;
        public Vector3 rotation;
    }

    public class DataBlockUnitLinkDamageRedirect
    {
        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerUnitComposite\", \"GetUnitKeys\")")]
        public string unitKey;
    }

    public class DataBlockUnitLinkConditional
    {
        [DropdownReference (true), HideLabel]
        public DataBlockComment comment;

        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerUnitComposite\", \"GetUnitKeys\")")]
        public HashSet<string> unitKeys = new HashSet<string> ();

        [BoxGroup ("Check", false)]
        public DataBlockScenarioSubcheckUnit check = new DataBlockScenarioSubcheckUnit ();

        public int triggerLimit = 1;

        [DropdownReference]
        public List<ICombatFunction> functions;

        [DropdownReference]
        public List<ICombatFunctionTargeted> functionsTargetedSelf;

        [DropdownReference]
        public List<ICombatFunctionTargeted> functionsTargetedPerLink;

        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockUnitLinkConditional () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        [GUIColor (1f, 0.75f, 0.5f), ShowIf ("IsCommsUpdateAvailable")]
        [Button ("Update comms", ButtonSizes.Medium), PropertyOrder (-1)]
        private void UpdateUnlocalizedComms (string key)
        {
            DataMultiLinkerUnitComposite.UpdateUnlocalizedCommsInFunctions (functions, key);
        }

        private bool IsCommsUpdateAvailable ()
        {
            if (functions != null)
            {
                foreach (var fn in functions)
                {
                    if (fn != null && fn is CombatLogMessageUnlocalized fnMessage)
                        return true;
                }
            }
            return false;
        }

        #endif
        #endregion
    }

    public class DataBlockUnitCompositeSpawn
    {
        [DropdownReference (true)]
        public DataBlockInt levelOffset;

        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerUnitLiveryPreset.data.Keys")]
        public string liveryPreset;

        [DropdownReference (true)]
        [LabelText ("Default AI Behavior")]
        [ValueDropdown ("@DataShortcuts.ai.unitBehaviors")]
        public string aiBehavior;

        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerAITargetingProfile.data.Keys")]
        public string aiTargeting;

        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockUnitCompositeSpawn () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    public class DataBlockUnitComposite
    {
        [DropdownReference (true), HideLabel]
        public DataBlockComment comment;

        [ValueDropdown ("@DataMultiLinkerUnitPreset.data.Keys")]
        public string preset;

        [DropdownReference (true)]
        public DataBlockStringNonSerialized textName;

        [DropdownReference (true)]
        public DataBlockStringNonSerializedLong textDesc;

        [DropdownReference (true)]
        public DataBlockUnitCompositeSpawn spawnInfo;

        [DropdownReference (true)]
        public DataBlockScenarioUnitCustomization spawnCustomization;

        [DropdownReference]
        public List<ICombatFunctionTargeted> spawnFunctions;

        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerUnitComposite\", \"GetAssignableEventKeys\")")]
        [DropdownReference (true)]
        public HashSet<string> assignableEventsDestruction;

        [DropdownReference (true)]
        public DataBlockUnitLinkDamageRedirect linkDamageRedirect;

        [DropdownReference (true)]
        public DataBlockUnitLinkTransform linkTransform;

        [DropdownReference]
        public List<DataBlockUnitLinkConditional> linksConditional;

        [DropdownReference]
        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerUnitComposite\", \"GetUnitKeys\")")]
        public HashSet<string> legStepBlocklist;

        [HideInInspector, NonSerialized, YamlIgnore]
        public bool removedInProcessing = false;

        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockUnitComposite () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    public class DataBlockUnitCompositeCore
    {
        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerUnitComposite\", \"GetUnitKeys\")")]
        public string unitRoot = "a_core";

        [PropertyTooltip ("Primary movement speed in meters per second")]
        public float speedTranslation = 3f;

        [PropertyTooltip ("Primary rotation speed in radians per second")]
        public float speedRotationPrimary = 0.5f;

        [PropertyTooltip ("Secondary rotation speed in degrees per second")]
        public float speedRotationSecondary = 30f;

        #region Editor
        #if UNITY_EDITOR

        #endif
        #endregion


    }

    public class DataBlockUnitCompositeLayout
    {
        [YamlIgnore]
        [GUIColor ("GetUnitColor"), HideLabel]
        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerUnitComposite\", \"GetUnitKeys\")")]
        [InlineButton ("ClearFilter", "-")]
        [OnValueChanged ("ApplyFilter")]
        [SuffixLabel ("Isolated selection")]
        public string unitKey = string.Empty;

        [ShowIf ("IsUnitSelected")]
        [YamlIgnore, BoxGroup ("A", false), HideLabel]
        public DataBlockUnitComposite unitFiltered;

        [HideIf ("IsUnitSelected")]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        public SortedDictionary<string, DataBlockUnitComposite> units = new SortedDictionary<string, DataBlockUnitComposite> ();

        [YamlIgnore, HideInInspector]
        public DataContainerUnitComposite parent;

        #region Editor
        #if UNITY_EDITOR

        private bool IsUnitSelected ()
        {
            return unitFiltered != null;
        }

        private void ClearFilter ()
        {
            unitFiltered = null;
            unitKey = string.Empty;
        }

        private void ApplyFilter ()
        {
            unitFiltered = null;

            if (units == null || string.IsNullOrEmpty (unitKey))
                return;

            foreach (var kvp in units)
            {
                if (kvp.Key.Contains (unitKey))
                {
                    unitFiltered = kvp.Value;
                    break;
                }
            }
        }

        private Color colorSelected = Color.HSVToRGB (0.3f, 0.2f, 1f);
        private Color GetUnitColor => unitFiltered != null ? colorSelected : Color.white;

        [Button, PropertyOrder (-1)]
        private void FillFromReference (GameObject holder, bool clear)
        {
            if (holder == null)
                return;

            string unitKeyRoot = null;
            if (parent != null && parent.coreProcessed != null)
                unitKeyRoot = parent.coreProcessed.unitRoot;
            if (string.IsNullOrEmpty (unitKeyRoot))
                unitKeyRoot = "a_core";

            if (clear && units != null)
            {
                var keys = units.Keys.ToList ();
                foreach (var key in keys)
                {
                    if (key != unitKeyRoot)
                        units.Remove (key);
                }
            }

            var subsystems = DataMultiLinkerSubsystem.data;
            var presets = DataMultiLinkerPartPreset.data;

            var t = holder.transform;
            for (int i = 0; i < t.childCount; ++i)
            {
                var child = t.GetChild (i);
                var childObject = child.gameObject;
                var childName = child.name;

                if (!childObject.activeSelf)
                    continue;

                // Determine it's a valid visual prefab that'll be found by the game
                var itemVisual = childObject.GetComponent<ItemVisual> ();
                if (itemVisual == null)
                    continue;

                // Determine it's a prefab instance, which is important for getting its true name
                var prefabInstance = UnityEditor.PrefabUtility.IsPartOfPrefabInstance (childObject);
                if (!prefabInstance)
                    continue;

                // Determine whether the object we're dealing with is the root of an instance and not part of a higher level instance
                var prefabRoot = UnityEditor.PrefabUtility.GetNearestPrefabInstanceRoot (childObject);
                if (prefabRoot == null || prefabRoot != childObject)
                    continue;

                var prefabAsset = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource (prefabRoot);
                var prefabName = prefabAsset.name;

                Debug.Log ($"C{i} ({childName}) | Prefab asset: {prefabName} ({prefabAsset.GetInstanceID ()})");

                bool subsystemFound = false;
                string subsystemKeySelected = null;

                foreach (var kvp in subsystems)
                {
                    var subsystem = kvp.Value;
                    if (subsystem == null || subsystem.hidden || subsystem.attachments == null || subsystem.attachments.Count == 0)
                        continue;

                    var subsystemKey = kvp.Key;
                    foreach (var kvp2 in subsystem.attachments)
                    {
                        var attachment = kvp2.Value;
                        if (attachment == null || prefabName != attachment.key)
                            continue;

                        var attachmentKey = kvp2.Key;
                        subsystemKeySelected = subsystemKey;
                        subsystemFound = true;

                        Debug.Log ($"C{i} ({childName}) | Found a matching subsystem attachment: {subsystemKey}/{attachmentKey}");
                        break;
                    }

                    if (subsystemFound)
                        break;
                }

                if (string.IsNullOrEmpty (subsystemKeySelected))
                    continue;

                var unitPresetKey = subsystemKeySelected;
                var unitPreset = DataMultiLinkerUnitPreset.GetEntry (subsystemKeySelected, false);
                if (unitPreset == null)
                {
                    Debug.LogWarning ($"C{i} ({childName}) | Failed to find unit preset {subsystemKeySelected}, skipping...");
                    continue;
                }

                string unitKeyFinal = null;
                var unitPresetKeyShort = unitPresetKey.Replace ("vhc_system_", string.Empty);

                if (units == null)
                    units = new SortedDictionary<string, DataBlockUnitComposite> ();

                DataBlockUnitComposite block = null;
                if (units.TryGetValue (childName, out var value))
                    block = value;
                if (block == null)
                {
                    block = new DataBlockUnitComposite ();
                    units[childName] = block;
                    Debug.LogWarning ($"C{i} ({childName}) | Added unit to dictionary");
                }

                block.preset = unitPresetKey;

                // Don't update transform on root
                if (childName == unitKeyRoot)
                    continue;

                if (block.linkTransform == null)
                    block.linkTransform = new DataBlockUnitLinkTransform ();

                var lt = block.linkTransform;
                lt.position = child.localPosition;
                lt.rotation = child.localRotation.eulerAngles;
                lt.unitKey = unitKeyRoot;

                Debug.LogWarning ($"C{i} ({childName}) | Updated unit transform | Position: {lt.position} | Rotation: {lt.rotation}");
            }
        }

        #endif
        #endregion
    }

    public class DataBlockUnitChildFunctions
    {
        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerUnitComposite\", \"GetUnitKeys\")")]
        public string unitKey = string.Empty;
        public List<ICombatFunctionTargeted> functions = new List<ICombatFunctionTargeted> ();
    }

    public class DataBlockUnitDirectorBooting
    {
        public bool evaluateFacing = false;
        public bool evaluateNavigation = false;
        public bool evaluateNodes = false;

        public List<ICombatFunction> functions = new List<ICombatFunction> ();

        public List<DataBlockUnitChildFunctions> functionsPerChild = new List<DataBlockUnitChildFunctions> ();
    }

    public class DataBlockUnitCompositeDirector
    {
        // [TabGroup ("Routines")]
        [DropdownReference]
        public DataBlockUnitDirectorBooting booting;

        // [TabGroup ("Routines")]
        [DropdownReference]
        public DataBlockUnitDirectorFacing facing;

        // [TabGroup ("Routines")]
        [DropdownReference]
        public DataBlockUnitDirectorNavigation navigation;

        // [TabGroup ("Nodes")]
        [YamlIgnore]
        [GUIColor ("GetNodeKeyColor"), HideLabel]
        [ValueDropdown ("GetNodeKeys")]
        [InlineButton ("ClearFilter", "-")]
        [OnValueChanged ("ApplyFilter")]
        [SuffixLabel ("Isolated selection")]
        public string nodeKey = string.Empty;

        // [TabGroup ("Nodes")]
        [ShowIf ("IsNodeSelected")]
        [YamlIgnore, HideLabel]
        public DataBlockUnitDirectorNodeRoot nodeFiltered;

        // [TabGroup ("Nodes")]
        [HideIf ("IsNodeSelected")]
        [ListDrawerSettings (CustomAddFunction = "@new DataBlockUnitDirectorNodeRoot ()", ElementColor = "GetElementColor", OnBeginListElementGUI = "DrawEntryGUI")]
        [LabelText ("Behavior Nodes")]
        [OnValueChanged ("UpdateNodeData", true, InvokeOnInitialize = true)]
        public List<DataBlockUnitDirectorNodeRoot> nodes = new List<DataBlockUnitDirectorNodeRoot> ();

        public void UpdateNodeData ()
        {
            if (nodes == null)
                return;

            for (int i = 0, iLimit = nodes.Count; i < iLimit; ++i)
            {
                var node = nodes[i];
                if (node == null)
                    continue;

                node.treeParent = null;
                node.treeDepth = 1;
                node.treeIndex = i + 1;
                node.hierarchyTextCached = GetHierarchyText (node);

                if (node.children != null)
                {
                    for (int c = 0, cLimit = node.children.Count; c < cLimit; ++c)
                    {
                        var nodeChild = node.children[c];
                        if (nodeChild != null)
                            UpdateChildNodeData (nodeChild, node, c);
                    }
                }
            }
        }

        private static string nameFallback = "...";
        private static StringBuilder sb = new StringBuilder ();
        private static List<DataBlockUnitDirectorNode> nodesPrinted = new List<DataBlockUnitDirectorNode> ();

        private string GetHierarchyText (DataBlockUnitDirectorNode node)
        {
            if (node == null)
                return string.Empty;

            sb.Clear ();

            var root = node;
            while (root.treeParent != null)
            {
                root = root.treeParent;
                if (root != null)
                {
                    sb.Append ("〈 ");
                    if (string.IsNullOrEmpty (root.name))
                        sb.Append (nameFallback);
                    else
                        sb.Append (root.name);
                }
            }

            sb.Append ("   ");
            sb.Append (node.treeIndex);
            sb.Append (' ');

            for (int i = 0; i < node.treeDepth; ++i)
                sb.Append ('|');

            return sb.ToString ();
        }

        private string GetIndexText (int treeDepth, int treeIndex)
        {
            sb.Clear ();
            for (int i = 0; i < treeDepth; ++i)
                sb.Append ('|');
            sb.Append (' ');
            sb.Append (treeIndex);

            return sb.ToString ();
        }

        private void UpdateChildNodeData (DataBlockUnitDirectorNode node, DataBlockUnitDirectorNode nodeParent, int index)
        {
            if (node == null || nodeParent == null)
                return;

            node.treeParent = nodeParent;
            node.treeDepth = nodeParent.treeDepth + 1;
            node.treeIndex = index + 1;
            node.hierarchyTextCached = GetHierarchyText (node);
            node.OnDataChanges ();

            if (node.children != null)
            {
                for (int c = 0, cLimit = node.children.Count; c < cLimit; ++c)
                {
                    var nodeChild = node.children[c];
                    if (nodeChild != null)
                        UpdateChildNodeData (nodeChild, node, c);
                }
            }
        }

        #region Editor
        #if UNITY_EDITOR

        private string GetSelectLabel => DataMultiLinkerUnitComposite.selectedDirector == this ? "Deselect" : "Select";

        // [TabGroup ("Routines")]
        [Button ("@GetSelectLabel"), PropertyOrder (-10)]
        private void Select ()
        {
            if (DataMultiLinkerUnitComposite.selectedDirector != this)
                DataMultiLinkerUnitComposite.selectedDirector = this;
            else
                DataMultiLinkerUnitComposite.selectedDirector = null;
        }

        private Color GetElementColor (int index, Color defaultColor)
        {
            var value = nodes != null && index >= 0 && index < nodes.Count ? nodes[index] : null;
            if (value != null)
            {
                if (!value.enabled)
                    return Color.gray.WithAlpha (0.2f);
                if (value.color != null)
                    return value.color.v.WithAlpha (0.2f);
            }
            return DataEditor.GetColorFromElementIndex (index);
        }

        private void DrawEntryGUI (int index)
        {
            if (nodes == null || !index.IsValidIndex (nodes))
                return;

            var node = nodes[index];
            if (node == null)
                return;

            // GUI.backgroundColor = bgColor;

            sb.Clear ();
            sb.Append (!string.IsNullOrEmpty (node.name) ? node.name : "Unnamed");

            sb.Append (" | P");
            sb.Append (node.priority);

            if (node.selfChange != null)
            {
                sb.Append (" | Unit: ");
                sb.Append (node.selfChange.unitKey);
            }

            if (node.children != null)
            {
                sb.Append (" | ");
                sb.Append (node.children.Count);
                sb.Append (" nodes");
            }

            GUILayout.BeginHorizontal ();
            GUILayout.Label (sb.ToString (), UnityEditor.EditorStyles.miniLabel);
            GUILayout.EndHorizontal ();

            // GUI.backgroundColor = bgColorOld;
        }

        private bool IsNodeSelected ()
        {
            return nodeFiltered != null;
        }

        private void ClearFilter ()
        {
            nodeFiltered = null;
            nodeKey = string.Empty;
        }

        private void ApplyFilter ()
        {
            nodeFiltered = null;

            if (nodes == null || string.IsNullOrEmpty (nodeKey))
                return;

            foreach (var node in nodes)
            {
                if (node != null && !string.IsNullOrEmpty (node.name) && node.name.Contains (nodeKey))
                {
                    nodeFiltered = node;
                    break;
                }
            }
        }

        private Color colorSelected = Color.HSVToRGB (0.3f, 0.2f, 1f);
        private Color GetNodeKeyColor => nodeFiltered != null ? colorSelected : Color.white;

        private List<string> nodeKeys = new List<string> ();

        private IEnumerable<string> GetNodeKeys ()
        {
            nodeKeys.Clear ();

            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    if (node != null && !string.IsNullOrEmpty (node.name))
                        nodeKeys.Add (node.name);
                }
            }

            return nodeKeys;
        }

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockUnitCompositeDirector () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    public class DataBlockUnitCompositeEvents
    {
        [DropdownReference]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.ExpandedFoldout, KeyLabel = " ")]
        // [DictionaryKeyDropdown ("@FieldReflectionUtility.GetConstantStringFieldValues (typeof (UnitCompositeEventTypes), false)")]
        public SortedDictionary<string, List<DataBlockUnitCompositeSpatialEffect>> eventsSpatial;

        [DropdownReference]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.ExpandedFoldout, KeyLabel = " ")]
        // [DictionaryKeyDropdown ("@FieldReflectionUtility.GetConstantStringFieldValues (typeof (UnitCompositeEventTypes), false)")]
        public SortedDictionary<string, DataBlockUnitCompositeAssignedEvent> eventsAssignable;

        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockUnitCompositeEvents () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    [HideReferenceObjectPicker]
    public class DataBlockUnitCompositeParent
    {
        [GUIColor ("GetKeyColor")]
        [ValueDropdown ("@DataMultiLinkerUnitComposite.data.Keys")]
        [SuffixLabel ("@hierarchyProperty"), HideLabel]
        public string key;

        [YamlIgnore, ReadOnly, HideInInspector]
        private string hierarchyProperty => DataMultiLinkerPartPreset.Presentation.showHierarchy ? hierarchy : string.Empty;

        [YamlIgnore, ReadOnly, HideInInspector]
        public string hierarchy;

        #region Editor
        #if UNITY_EDITOR

        private static Color colorError = Color.Lerp (Color.red, Color.white, 0.5f);
        private static Color colorNormal = Color.white;

        private Color GetKeyColor ()
        {
            if (string.IsNullOrEmpty (key))
                return colorError;

            bool present = DataMultiLinkerUnitComposite.data.ContainsKey (key);
            return present ? colorNormal : colorError;
        }

        #endif
        #endregion
    }

    public class DataContainerUnitComposite : DataContainerWithText, IDataContainerTagged
    {
        [TabGroup ("Core")]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, CustomAddFunction = "@new DataBlockUnitCompositeParent ()")]
        [DropdownReference]
        public List<DataBlockUnitCompositeParent> parents = new List<DataBlockUnitCompositeParent> ();

        [TabGroup ("Core")]
        [YamlIgnore, LabelText ("Children"), ReadOnly]
        [ShowIf ("@children != null && children.Count > 0")]
        [ListDrawerSettings (DefaultExpandedState = false)]
        public List<string> children = new List<string> ();


        [TabGroup ("Core")]
        public bool hidden = false;


        [TabGroup ("Core")]
        public DataBlockUnitCompositeCore core = new DataBlockUnitCompositeCore ();

        [TabGroup ("Core")]
        [ShowIf ("@IsInheritanceVisible && coreProcessed != null")]
        [YamlIgnore, ReadOnly]
        public DataBlockUnitCompositeCore coreProcessed;


        [LabelText ("UI")]
        [TabGroup ("Core")]
        public DataBlockUnitCompositeUI ui = new DataBlockUnitCompositeUI ();

        [TabGroup ("Core")]
        [LabelText ("UI Processed")]
        [ShowIf ("@IsInheritanceVisible && uiProcessed != null")]
        [YamlIgnore, ReadOnly]
        public DataBlockUnitCompositeUI uiProcessed;


        [TabGroup ("Core")]
        public HashSet<string> tags;

        [TabGroup ("Core")]
        [ShowIf ("@IsInheritanceVisible && tagsProcessed != null")]
        [YamlIgnore, ReadOnly]
        public HashSet<string> tagsProcessed;


        [TabGroup ("Layout")]
        [HideReferenceObjectPicker, HideLabel]
        public DataBlockUnitCompositeLayout layout = new DataBlockUnitCompositeLayout ();

        [TabGroup ("Layout")]
        [HideReferenceObjectPicker, HideLabel]
        [ShowIf ("@IsInheritanceVisible && layoutProcessed != null")]
        [YamlIgnore, ReadOnly]
        public DataBlockUnitCompositeLayout layoutProcessed;


        [TabGroup ("Director")]
        [HideReferenceObjectPicker, HideLabel]
        public DataBlockUnitCompositeDirector director = new DataBlockUnitCompositeDirector ();

        [TabGroup ("Director")]
        [HideReferenceObjectPicker, HideLabel]
        [ShowIf ("@IsInheritanceVisible && directorProcessed != null")]
        [YamlIgnore, ReadOnly]
        public DataBlockUnitCompositeDirector directorProcessed;


        [TabGroup ("Events")]
        [HideReferenceObjectPicker, HideLabel]
        public DataBlockUnitCompositeEvents events = new DataBlockUnitCompositeEvents ();

        [TabGroup ("Events")]
        [HideReferenceObjectPicker, HideLabel]
        [ShowIf ("@IsInheritanceVisible && eventsProcessed != null")]
        [YamlIgnore, ReadOnly]
        public DataBlockUnitCompositeEvents eventsProcessed;

        public bool IsHidden () => hidden;

        public HashSet<string> GetTags (bool processed) =>
            processed ? tagsProcessed : tags;

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
            if (director != null)
                director.UpdateNodeData ();
            if (layout != null)
                layout.parent = this;
        }

        public override void ResolveText ()
        {
            if (ui != null)
            {
                if (ui.textName != null)
                    ui.textName.s = DataManagerText.GetText (TextLibs.unitComposites, $"{key}__core_name");
                if (ui.textType != null)
                    ui.textType.s = DataManagerText.GetText (TextLibs.unitComposites, $"{key}__core_type");
            }

            if (layout != null && layout.units != null)
            {
                foreach (var kvp in layout.units)
                {
                    var unitKey = kvp.Key;
                    var unitInfo = kvp.Value;

                    if (unitInfo.textName != null)
                        unitInfo.textName.s = DataManagerText.GetText (TextLibs.unitComposites, $"{key}__unit_{unitKey}_name");

                    if (unitInfo.textDesc != null)
                        unitInfo.textDesc.s = DataManagerText.GetText (TextLibs.unitComposites, $"{key}__unit_{unitKey}_text");
                }
            }

            if (events != null)
            {
                if (events.eventsAssignable != null)
                {
                    foreach (var kvp in events.eventsAssignable)
                    {
                        var eventKey = kvp.Key;
                        var eventData = kvp.Value;

                        if (string.IsNullOrEmpty(eventKey) || eventData == null || eventData.ui == null)
                            continue;

                        eventData.ui.textName = DataManagerText.GetText (TextLibs.unitComposites, $"{key}__ev_{eventKey}_name");
                        eventData.ui.textDesc = DataManagerText.GetText (TextLibs.unitComposites, $"{key}__ev_{eventKey}_text");
                    }
                }
            }
        }

        public void SortDirectorNodes ()
        {
            if (directorProcessed == null || directorProcessed.nodes == null || directorProcessed.nodes.Count <= 1)
                return;

            var nodes = directorProcessed.nodes;
            nodes.Sort (CompareDirectorNodesForSorting);

            for (int i = nodes.Count - 1; i >= 0; --i)
            {
                var step = nodes[i];
                if (step == null)
                    nodes.RemoveAt (i);
            }
        }

        private int CompareDirectorNodesForSorting (DataBlockUnitDirectorNodeRoot node1, DataBlockUnitDirectorNodeRoot node2)
        {
            if (node1 == null)
            {
                if (node2 == null)
                    return 0;
                else
                    return -1;
            }
            else
            {
                if (node2 == null)
                    return 1;
                else
                    return node1.priority.CompareTo (node2.priority);
            }
        }

        #region Editor
        #if UNITY_EDITOR

        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            if (ui != null)
            {
                if (ui.textName != null)
                    DataManagerText.TryAddingTextToLibrary (TextLibs.unitComposites, $"{key}__core_name", ui.textName.s);
                if (ui.textType != null)
                    DataManagerText.TryAddingTextToLibrary (TextLibs.unitComposites, $"{key}__core_type", ui.textType.s);
            }

            if (layout != null && layout.units != null)
            {
                foreach (var kvp in layout.units)
                {
                    var unitKey = kvp.Key;
                    var unitInfo = kvp.Value;

                    if (unitInfo.textName != null)
                        DataManagerText.TryAddingTextToLibrary (TextLibs.unitComposites, $"{key}__unit_{unitKey}_name", unitInfo.textName.s);

                    if (unitInfo.textDesc != null)
                        DataManagerText.TryAddingTextToLibrary (TextLibs.unitComposites, $"{key}__unit_{unitKey}_text", unitInfo.textDesc.s);
                }
            }

            if (events != null)
            {
                if (events.eventsAssignable != null)
                {
                    foreach (var kvp in events.eventsAssignable)
                    {
                        var eventKey = kvp.Key;
                        var eventData = kvp.Value;

                        if (string.IsNullOrEmpty(eventKey) || eventData == null || eventData.ui == null)
                            continue;

                        DataManagerText.TryAddingTextToLibrary (TextLibs.unitComposites, $"{key}__ev_{eventKey}_name", eventData.ui.textName);
                        DataManagerText.TryAddingTextToLibrary (TextLibs.unitComposites, $"{key}__ev_{eventKey}_text", eventData.ui.textDesc);
                    }
                }
            }
        }

        private IEnumerable<string> GetUnitKeys => layoutProcessed?.units?.Keys;
        private IEnumerable<string> GetAssignableEventKeys => eventsProcessed?.eventsAssignable?.Keys;

        private bool IsInheritanceVisible => DataMultiLinkerUnitComposite.Presentation.showInheritance;

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataContainerUnitComposite () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        private static GameObject visualHolder;

        [Button, PropertyOrder (-1), ButtonGroup ("Header")]
        public void DestroyVisualHolder () => DataMultiLinkerSubsystem.DestroyVisualHolder ();

        [EnableIf ("@AssetPackageHelper.AreUnitAssetsInstalled ()")]
        [Button, PropertyOrder (-1), ButtonGroup ("Header")]
        public void Visualize ()
        {
            if (!AssetPackageHelper.AreUnitAssetsInstalled ())
            {
                Debug.LogWarning (AssetPackageHelper.unitAssetWarning);
                return;
            }

            if (layoutProcessed == null || layoutProcessed.units == null || layoutProcessed.units.Count == 0)
                return;

            bool first = true;
            foreach (var kvp in layoutProcessed.units)
            {
                var unitKey = kvp.Key;
                var unit = kvp.Value;

                if (unit == null)
                {
                    Debug.LogWarning ($"{unitKey}: null data");
                    continue;
                }

                var unitPreset = DataMultiLinkerUnitPreset.GetEntry (unit.preset, false);
                if (unitPreset == null)
                {
                    Debug.LogWarning ($"{unitKey}: unit preset {unit.preset} not found");
                    continue;
                }

                if (unitPreset.partsProcessed == null || !unitPreset.partsProcessed.TryGetValue (LoadoutSockets.corePart, out var link))
                {
                    Debug.LogWarning ($"{unitKey}: unit preset {unit.preset} contains no processed part for core socket");
                    continue;
                }

                if (link.preset != null && link.preset is DataBlockPartSlotResolverKeys resolver)
                {
                    if (resolver.keys == null || resolver.keys.Count == 0)
                    {
                        Debug.LogWarning ($"{unitKey}: unit preset {unit.preset} part resolver has no keys");
                        continue;
                    }

                    var partPresetKey = resolver.keys[0];
                    var partPreset = DataMultiLinkerPartPreset.GetEntry (partPresetKey);
                    if (partPreset == null)
                    {
                        Debug.LogWarning ($"{unitKey}: part preset {partPresetKey} not found (from unit preset {unitPreset.key})");
                        continue;
                    }

                    if (partPreset.genStepsProcessed == null || partPreset.genStepsProcessed.Count == 0)
                    {
                        Debug.LogWarning ($"{unitKey}: part preset {partPreset.key} contains no processed gen steps");
                        continue;
                    }

                    bool found = false;
                    foreach (var step in partPreset.genStepsProcessed)
                    {
                        if (step != null && step is AddHardpoints stepHardpoint)
                        {
                            found = true;

                            if (stepHardpoint.subsystemsInitial == null || stepHardpoint.subsystemsInitial.Count == 0)
                            {
                                Debug.LogWarning ($"{unitKey}: part preset {partPreset.key} hardpoint step has no subsystem keys");
                                continue;
                            }

                            var subsystemKey = stepHardpoint.subsystemsInitial[0];
                            var subsystem = DataMultiLinkerSubsystem.GetEntry (subsystemKey);
                            if (subsystem == null)
                            {
                                Debug.LogWarning ($"{unitKey}: subsystem {subsystemKey} not found (from part preset {partPreset.key})");
                                continue;
                            }

                            var subholder = DataMultiLinkerSubsystem.VisualizeObject (subsystem, true, first, false);
                            first = false;

                            if (subholder == null)
                                continue;

                            if (unit.linkTransform != null)
                            {
                                subholder.transform.localPosition = unit.linkTransform.position;
                                subholder.transform.localRotation = Quaternion.Euler (unit.linkTransform.rotation);
                            }
                        }
                    }
                }
            }
        }

        #endif
        #endregion
    }

    public class DataBlockUnitCompositeSpatialEffect : DataBlockUnitDirectorChecked
    {
        [DropdownReference (true)]
        [LabelText ("Functions (Spatial)")]
        public List<ICombatFunctionSpatial> functionsSpatial;

        [DropdownReference (true)]
        [LabelText ("Functions (Self-targeted)")]
        public List<ICombatFunctionTargeted> functionsTargetedSelf;

        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockUnitCompositeSpatialEffect () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    public class DataBlockUnitCompositeAssignedEventUI
    {
        [DataEditor.SpriteNameAttribute (true, 32f)]
        public string icon;

        public bool iconEmbedded;

        [YamlIgnore]
        [LabelText ("Name / Desc.")]
        public string textName;

        [YamlIgnore]
        [HideLabel, TextArea (1, 10)]
        public string textDesc;
    }

    public class DataBlockUnitCompositeAssignedEvent
    {
        [DropdownReference (true)]
        public DataBlockInt unitCount;

        [LabelText ("UI")]
        [DropdownReference (true)]
        public DataBlockUnitCompositeAssignedEventUI ui;

        [DropdownReference (true)]
        [LabelText ("Functions (Global)")]
        public List<ICombatFunction> functions;

        [DropdownReference (true)]
        [LabelText ("Functions (Self-targeted)")]
        public List<ICombatFunctionTargeted> functionsTargeted;

        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockUnitCompositeAssignedEvent () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    public class DataBlockUnitCompositeUI
    {
        public DataBlockStringNonSerialized textName;
        public DataBlockStringNonSerialized textType;
    }

    public static class UnitCompositeEventTypes
    {
        public const string legLiftoff = "leg_liftoff";
        public const string legContact = "leg_contact";
    }
}
