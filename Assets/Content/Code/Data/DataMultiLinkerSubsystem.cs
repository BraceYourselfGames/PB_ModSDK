using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerSubsystem : DataMultiLinker<DataContainerSubsystem>
    {
        public DataMultiLinkerSubsystem ()
        {
            textSectorKeys = new List<string> { TextLibs.equipmentSubsystems };
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            DataMultiLinkerUtility.RegisterOnTextExport 
            (
                dataType, 
                () => TextLibraryHelper.OnBeforeTextExport (dataType, TextLibs.equipmentSubsystems),
                () => TextLibraryHelper.OnAfterTextExport (dataType, TextLibs.equipmentSubsystems)
            );
        }
        
        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool showCore = true;
            
            [ShowInInspector]
            public static bool showUI = true;
            
            [ShowInInspector]
            public static bool showTags = true;
            
            [ShowInInspector]
            public static bool showHardpoints = true;
            
            [ShowInInspector]
            public static bool showVisuals = true;

            [ShowInInspector]
            public static bool showStats = true;
            
            [ShowInInspector]
            public static bool showBlocks = true;
            
            [ShowInInspector]
            [ShowIf ("showInheritance")]
            [InfoBox ("Warning: this mode triggers inheritance processing every time inheritable fields are modified. This is useful for instantly previewing changes to things like stat or inherited text, but it can cause data loss if you are not careful. Only currently modified config is protected from being reloaded, save if you switch to another config.", VisibleIf = "autoUpdateInheritance")]
            public static bool autoUpdateInheritance = false;

            [ShowInInspector]
            public static bool showInheritance = false;

            [ShowInInspector]
            public static bool showTagCollections = false;
            
            [ShowInInspector]
            public static bool showHardpointCollection = false;

            [ShowInInspector]
            public static bool logUpdates = false;
            
            [ShowInInspector]
            public static bool showPartText = true;

            [ShowInInspector]
            public static int debugGenerationLevel
            {
                get => EquipmentUtility.debugGenerationLevel;
                set => EquipmentUtility.debugGenerationLevel = value;
            }
            
            [ShowInInspector]
            public static int debugGenerationRating
            {
                get => EquipmentUtility.debugGenerationRating;
                set => EquipmentUtility.debugGenerationRating = value;
            }
        }

        [ShowInInspector, HideLabel, FoldoutGroup ("View options")]
        public Presentation presentation = new Presentation ();
        
        [ShowIf ("@DataMultiLinkerSubsystem.Presentation.showTagCollections")]
        [ShowInInspector]
        public static HashSet<string> tags = new HashSet<string> ();
        
        [ShowIf ("@DataMultiLinkerSubsystem.Presentation.showTagCollections")]
        [ShowInInspector, ReadOnly]
        public static Dictionary<string, HashSet<string>> tagsMap = new Dictionary<string, HashSet<string>> ();
        
        [ShowIf ("@DataMultiLinkerSubsystem.Presentation.showHardpointCollection")]
        [ShowInInspector, ReadOnly]
        public static Dictionary<string, List<string>> hardpointsKeyMap = new Dictionary<string, List<string>> ();
        
        [HideInInspector, ReadOnly]
        public static Dictionary<string, List<DataContainerSubsystem>> hardpointsDataMap = new Dictionary<string, List<DataContainerSubsystem>> ();


        
        public static void OnAfterDeserialization ()
        {
            // Process every subsystem recursively first
            foreach (var kvp in data)
                ProcessRecursiveStart (kvp.Value);
            
            DataTagUtility.RegisterTags (data, ref tags, ref tagsMap);
            
            hardpointsKeyMap.Clear ();
            hardpointsDataMap.Clear ();

            // Fill parents after recursive processing is done on all subsystems, ensuring lack of cyclical refs etc
            foreach (var kvp1 in data)
            {
                var subsystem = kvp1.Value;
                if (subsystem == null)
                    continue;
                
                var key = kvp1.Key;
                subsystem.children.Clear ();
                
                foreach (var kvp2 in data)
                {
                    var blueprint = kvp2.Value;
                    if (blueprint.parent == key)
                        subsystem.children.Add (blueprint.key);
                }

                if (!subsystem.hidden)
                {
                    foreach (var hardpoint in subsystem.hardpointsProcessed)
                    {
                        if (!hardpointsKeyMap.ContainsKey (hardpoint))
                            hardpointsKeyMap.Add (hardpoint, new List<string> (10));
                        hardpointsKeyMap[hardpoint].Add (subsystem.key);
                        
                        if (!hardpointsDataMap.ContainsKey (hardpoint))
                            hardpointsDataMap.Add (hardpoint, new List<DataContainerSubsystem> (10));
                        hardpointsDataMap[hardpoint].Add (subsystem);
                    }
                }
            }
            
            foreach (var kvp1 in data)
                Postprocess (kvp1.Value);
        }


        private static List<DataContainerSubsystem> subsystemsUpdated = new List<DataContainerSubsystem> ();

        public static void ProcessRelated (DataContainerSubsystem origin)
        {
            if (origin == null)
                return;

            subsystemsUpdated.Clear ();
            subsystemsUpdated.Add (origin);
            
            if (origin.children != null)
            {
                foreach (var childKey in origin.children)
                {
                    var subsystem = GetEntry (childKey);
                    if (subsystem != null)
                        subsystemsUpdated.Add (subsystem);
                }
            }
            
            foreach (var subsystem in subsystemsUpdated)
            {
                // Avoid localization refresh and other losses on origin
                if (subsystem != origin)
                    subsystem.OnAfterDeserialization (subsystem.key);
            }

            foreach (var subsystem in subsystemsUpdated)
                ProcessRecursiveStart (subsystem);

            foreach (var subsystem in subsystemsUpdated)
                Postprocess (subsystem);

            if (Presentation.logUpdates)
                Debug.Log ($"Updated {subsystemsUpdated.Count} subsystems: {subsystemsUpdated.ToStringFormatted (toStringOverride: (a) => a.key)}");
        }

        private static void ProcessRecursiveStart (DataContainerSubsystem origin)
        {
            if (origin == null)
                return;
            
            if (origin.textNameProcessed != null)
                origin.textNameProcessed.s = string.Empty;

            if (origin.textDescProcessed != null)
                origin.textDescProcessed.s = string.Empty;
            
            origin.parentHierarchy = string.Empty;
            origin.hardpointsProcessed.Clear ();
            origin.statsProcessed.Clear ();
            origin.tagsProcessed.Clear ();
                
            origin.visualsProcessed = null;
            origin.attachmentsProcessed = null;
            origin.activationProcessed = null;
            origin.projectileProcessed = null;
            origin.beamProcessed = null;
            origin.customProcessed = null;
            origin.functionsProcessed = null;
                
            ProcessRecursive (origin, origin, 0);
        }

        private static void Postprocess (DataContainerSubsystem subsystem)
        {
            if (subsystem.textNameFromHardpoint != null)
            {
                var hardpointKey =
                    !string.IsNullOrEmpty (subsystem.textNameFromHardpoint.hardpointOverride) ? 
                    subsystem.textNameFromHardpoint.hardpointOverride : 
                    subsystem.hardpointsProcessed?.FirstOrDefault ();
                
                var hardpointInfo = DataMultiLinkerSubsystemHardpoint.GetEntry (hardpointKey, false);
                if (hardpointInfo == null || string.IsNullOrEmpty (hardpointInfo.textSuffix))
                    Debug.LogWarning ($"Subsystem {subsystem.key} requests name from hardpoint {hardpointKey}, but that hardpoint lacks a suffix text or no such hardpoint exists");
                else
                {
                    if (subsystem.textNameFromHardpoint.suffix)
                    {
                        // In suffix more, proceeding only makes sense if there is something to append it to
                        if (subsystem.textNameProcessed != null && !string.IsNullOrEmpty (subsystem.textNameProcessed.s))
                            subsystem.textNameProcessed.s = $"{subsystem.textNameProcessed.s}-{hardpointInfo.textSuffix}";
                    }
                    else
                    {
                        // In this mode hardpoint provides a full name:
                        // Useful for cases where you want all subsystems under some hardpoint to display one name
                        if (subsystem.textNameProcessed == null)
                            subsystem.textNameProcessed = new DataBlockEquipmentTextName ();
                        AppendToNameString (ref subsystem.textNameProcessed.s, hardpointInfo.textSuffix);
                    }
                }
            }
            
            subsystem.UpdateGroups ();

            #if !PB_MODSDK

            if (Application.isPlaying && IDUtility.IsGameLoaded ())
            {
                var equipment = Contexts.sharedInstance.equipment;
                var entities = equipment.GetEntitiesWithDataKeySubsystem (subsystem.key);
                if (entities != null && entities.Count > 0)
                {
                    Debug.Log ($"Replacing reference to blueprint {subsystem.key} on {entities.Count} subsystems. It is recommended to restart the game to fully apply the changes.");
                    foreach (var entity in entities)
                        entity.ReplaceDataLinkSubsystem (subsystem);
                }
            }

            #endif
        }

        private static void ProcessRecursive (DataContainerSubsystem current, DataContainerSubsystem root, int depth)
        {
            if (current == null || root == null)
            {
                Debug.LogWarning ($"Received null step or root subsystem reference while validating subsystem hierarchy");
                return;
            }
            
            if (depth > 0 && current == root)
            {
                Debug.LogWarning ($"Encountered dependency loop at depth level {depth} when processing subsystem {root.key}");
                return;
            }
            
            if (current.textName != null && !string.IsNullOrEmpty (current.textName.s))
            {
                if (root.textNameProcessed == null)
                    root.textNameProcessed = new DataBlockEquipmentTextName ();
                AppendToNameString (ref root.textNameProcessed.s, current.textName.s);
            }

            if (!string.IsNullOrEmpty (current.textNameFromPreset))
            {
                var partPreset = DataMultiLinkerPartPreset.GetEntry (current.textNameFromPreset, false);
                if (partPreset == null || partPreset.textNameProcessed == null || string.IsNullOrEmpty (partPreset.textNameProcessed.s))
                    Debug.LogWarning ($"Subsystem {current.key} requests name from part preset {current.textNameFromPreset}, but that preset lacks a name or no such preset exists | Preset: {partPreset.ToStringNullCheck ()} | Origin: {root.key}");
                else
                {
                    if (root.textNameProcessed == null)
                        root.textNameProcessed = new DataBlockEquipmentTextName ();
                    AppendToNameString (ref root.textNameProcessed.s, partPreset.textNameProcessed.s);
                }
            }

            if (current.textDesc != null && !string.IsNullOrEmpty (current.textDesc.s))
            {
                if (root.textDescProcessed == null)
                    root.textDescProcessed = new DataBlockEquipmentTextDesc ();
                AppendToDescString (ref root.textDescProcessed.s, current.textDesc.s);
            }
            
            if (!string.IsNullOrEmpty (current.textDescFromPreset))
            {
                var partPreset = DataMultiLinkerPartPreset.GetEntry (current.textDescFromPreset, false);
                if (partPreset == null || partPreset.textDescProcessed == null || string.IsNullOrEmpty (partPreset.textDescProcessed.s))
                    Debug.LogWarning ($"Subsystem {current.key} requests description from part preset {current.textDescFromPreset}, but that preset lacks a name or no such preset exists | Origin: {root.key}");
                else
                {
                    if (root.textDescProcessed == null)
                        root.textDescProcessed = new DataBlockEquipmentTextDesc ();
                    AppendToNameString (ref root.textDescProcessed.s, partPreset.textDescProcessed.s);
                }
            }
            
            if (current.hardpoints != null)
            {
                foreach (var hardpoint in current.hardpoints)
                {
                    if (!string.IsNullOrEmpty (hardpoint) && !root.hardpointsProcessed.Contains (hardpoint))
                        root.hardpointsProcessed.Add (hardpoint);
                }
            }
            
            if (current.functions != null)
            {
                if (root.functionsProcessed == null)
                    root.functionsProcessed = new DataBlockSubsystemFunctions ();

                if (current.functions.general != null && root.functionsProcessed.general == null)
                    root.functionsProcessed.general = current.functions.general;
                
                if (current.functions.targeted != null && root.functionsProcessed.targeted == null)
                    root.functionsProcessed.targeted = current.functions.targeted;
                
                if (current.functions.action != null && root.functionsProcessed.action == null)
                    root.functionsProcessed.action = current.functions.action;
            }
            
            // Not critical, but a lot of our data uses this - inserting tags based on parent hierarchy
            if (!root.tagsProcessed.Contains (current.key))
                root.tagsProcessed.Add (current.key);
            
            if (current.tags != null)
            {
                foreach (var tag in current.tags)
                {
                    if (!string.IsNullOrEmpty (tag) && !root.tagsProcessed.Contains (tag))
                        root.tagsProcessed.Add (tag);
                }
            }
            
            if (current.stats != null)
            {
                foreach (var kvp in current.stats)
                {
                    var key = kvp.Key;
                    var blockCurrent = kvp.Value;

                    if (root.statsProcessed.ContainsKey (key))
                    {
                        var blockRoot = root.statsProcessed[key];
                        
                        blockRoot.value *= blockCurrent.value;
                        blockRoot.report += $" x {blockCurrent.value}";
                    }
                    else
                    {
                        var blockRoot = new DataBlockSubsystemStat ();
                        root.statsProcessed.Add (key, blockRoot);
                        
                        blockRoot.value = blockCurrent.value;
                        blockRoot.report = blockCurrent.value.ToString ();
                        blockRoot.targetMode = blockCurrent.targetMode;
                        blockRoot.targetSocket = blockCurrent.targetSocket;
                        blockRoot.targetHardpoint = blockCurrent.targetHardpoint;
                    }
                }
            }

            // The exact index in the visual collection is very important, as it maps to base frame renderer indexes
            // So instead of a simple merge where we add all non-empty keys, we maintain indexes
            
            if (current.visuals != null)
            {
                if (root.visualsProcessed == null)
                {
                    root.visualsProcessed = new List<string> (current.visuals);
                }
                else
                {
                    int countRoot = root.visualsProcessed.Count;
                    for (int i = 0, countCurrent = current.visuals.Count; i < countCurrent; ++i)
                    {
                        var visualKey = current.visuals[i];
                        if (i < countRoot)
                        {
                            // Only insert visuals if child left them empty
                            if (string.IsNullOrEmpty (root.visualsProcessed[i]))
                                root.visualsProcessed[i] = visualKey;
                        }
                        else
                            root.visualsProcessed.Add (visualKey);
                    }
                }
            }
            
            if (current.attachments != null)
            {
                if (root.attachmentsProcessed == null)
                    root.attachmentsProcessed = new Dictionary<string, DataBlockSubsystemAttachment> (current.attachments.Count);
                
                foreach (var kvp in current.attachments)
                {
                    if (!root.attachmentsProcessed.ContainsKey (kvp.Key))
                        root.attachmentsProcessed.Add (kvp.Key, kvp.Value);
                }
            }
            
            // Replace block whole
            // if (current.activation != null && root.activationProcessed == null)
            //     root.activationProcessed = current.activation;
            
            // Merge block
            if (current.activation != null)
            {
                if (root.activationProcessed == null)
                    root.activationProcessed = new DataBlockSubsystemActivation_V2 ();

                if (current.activation.visual != null && root.activationProcessed.visual == null)
                    root.activationProcessed.visual = current.activation.visual;
                
                if (current.activation.audio != null && root.activationProcessed.audio == null)
                    root.activationProcessed.audio = current.activation.audio;
                
                if (current.activation.light != null && root.activationProcessed.light == null)
                    root.activationProcessed.light = current.activation.light;
                
                if (current.activation.recoil != null && root.activationProcessed.recoil == null)
                    root.activationProcessed.recoil = current.activation.recoil;
                
                if (current.activation.timing != null && root.activationProcessed.timing == null)
                    root.activationProcessed.timing = current.activation.timing;
            }

            // Replace block whole
            // if (current.projectile != null && root.projectileProcessed == null)
            //     root.projectileProcessed = current.projectile;
            
            // Merge block
            if (current.projectile != null)
            {
                if (root.projectileProcessed == null)
                {
                    root.projectileProcessed = new DataBlockSubsystemProjectile_V2 ();
                    root.projectileProcessed.parentSubsystem = root;
                }

                if (current.projectile.visual != null && root.projectileProcessed.visual == null)
                    root.projectileProcessed.visual = current.projectile.visual;
                
                if (current.projectile.audio != null && root.projectileProcessed.audio == null)
                    root.projectileProcessed.audio = current.projectile.audio;
                
                if (current.projectile.damageDelay != null && root.projectileProcessed.damageDelay == null)
                    root.projectileProcessed.damageDelay = current.projectile.damageDelay;
                
                if (current.projectile.distribution != null && root.projectileProcessed.distribution == null)
                    root.projectileProcessed.distribution = current.projectile.distribution;
                
                if (current.projectile.range != null && root.projectileProcessed.range == null)
                    root.projectileProcessed.range = current.projectile.range;
                
                if (current.projectile.fragmentation != null && root.projectileProcessed.fragmentation == null)
                    root.projectileProcessed.fragmentation = current.projectile.fragmentation;
                
                if (current.projectile.fragmentationDelayed != null && root.projectileProcessed.fragmentationDelayed == null)
                    root.projectileProcessed.fragmentationDelayed = current.projectile.fragmentationDelayed;
                
                if (current.projectile.falloff != null && root.projectileProcessed.falloff == null)
                    root.projectileProcessed.falloff = current.projectile.falloff;
                
                if (current.projectile.falloffGlobal != null && root.projectileProcessed.falloffGlobal == null)
                    root.projectileProcessed.falloffGlobal = current.projectile.falloffGlobal;
                
                if (current.projectile.animationFade != null && root.projectileProcessed.animationFade == null)
                    root.projectileProcessed.animationFade = current.projectile.animationFade;
                
                if (current.projectile.ballistics != null && root.projectileProcessed.ballistics == null)
                    root.projectileProcessed.ballistics = current.projectile.ballistics;
                
                if (current.projectile.fuseProximity != null && root.projectileProcessed.fuseProximity == null)
                    root.projectileProcessed.fuseProximity = current.projectile.fuseProximity;
                
                if (current.projectile.hitResponse != null && root.projectileProcessed.hitResponse == null)
                    root.projectileProcessed.hitResponse = current.projectile.hitResponse;
                
                if (current.projectile.splashDamage != null && root.projectileProcessed.splashDamage == null)
                    root.projectileProcessed.splashDamage = current.projectile.splashDamage;
                
                if (current.projectile.splashImpact != null && root.projectileProcessed.splashImpact == null)
                    root.projectileProcessed.splashImpact = current.projectile.splashImpact;
                
                if (current.projectile.guidanceData != null && root.projectileProcessed.guidanceData == null)
                    root.projectileProcessed.guidanceData = current.projectile.guidanceData;
                
                if (current.projectile.guidanceAudio != null && root.projectileProcessed.guidanceAudio == null)
                    root.projectileProcessed.guidanceAudio = current.projectile.guidanceAudio;
                
                if (current.projectile.statusBuildup != null && root.projectileProcessed.statusBuildup == null)
                    root.projectileProcessed.statusBuildup = current.projectile.statusBuildup;
                
                if (current.projectile.uiTrajectory != null && root.projectileProcessed.uiTrajectory == null)
                    root.projectileProcessed.uiTrajectory = current.projectile.uiTrajectory;
                
                if (current.projectile.uiSpeedAverage != null && root.projectileProcessed.uiSpeedAverage == null)
                    root.projectileProcessed.uiSpeedAverage = current.projectile.uiSpeedAverage;
                
                if (current.projectile.uiCoverageWeight != null && root.projectileProcessed.uiCoverageWeight == null)
                    root.projectileProcessed.uiCoverageWeight = current.projectile.uiCoverageWeight;
                
                if (current.projectile.uiOptimumThreshold != null && root.projectileProcessed.uiOptimumThreshold == null)
                    root.projectileProcessed.uiOptimumThreshold = current.projectile.uiOptimumThreshold;
            }

            // Replace block whole
            if (current.beam != null && root.beamProcessed == null)
                root.beamProcessed = current.beam;
            
            // Replace custom block whole
            // if (current.custom != null && root.customProcessed == null)
            //     root.customProcessed = current.custom;

            // Merge custom block
            if (current.custom != null)
            {
                if (root.customProcessed == null)
                    root.customProcessed = new DataBlockPartCustom ();

                if (current.custom.flags != null && current.custom.flags.Count > 0)
                {
                    if (root.customProcessed.flags == null)
                        root.customProcessed.flags = new HashSet<string> ();

                    foreach (var flag in current.custom.flags)
                    {
                        if (!root.customProcessed.flags.Contains (flag))
                            root.customProcessed.flags.Add (flag);
                    }
                }

                if (current.custom.ints != null && current.custom.ints.Count > 0)
                {
                    if (root.customProcessed.ints == null)
                        root.customProcessed.ints = new SortedDictionary<string, int> ();

                    foreach (var kvp in current.custom.ints)
                    {
                        if (!root.customProcessed.ints.ContainsKey (kvp.Key))
                            root.customProcessed.ints.Add (kvp.Key, kvp.Value);
                        else
                            root.customProcessed.ints[kvp.Key] = kvp.Value;
                    }
                }
                
                if (current.custom.floats != null && current.custom.floats.Count > 0)
                {
                    if (root.customProcessed.floats == null)
                        root.customProcessed.floats = new SortedDictionary<string, float> ();

                    foreach (var kvp in current.custom.floats)
                    {
                        if (!root.customProcessed.floats.ContainsKey (kvp.Key))
                            root.customProcessed.floats.Add (kvp.Key, kvp.Value);
                        else
                            root.customProcessed.floats[kvp.Key] = kvp.Value;
                    }
                }
                
                if (current.custom.vectors != null && current.custom.vectors.Count > 0)
                {
                    if (root.customProcessed.vectors == null)
                        root.customProcessed.vectors = new SortedDictionary<string, Vector3> ();

                    foreach (var kvp in current.custom.vectors)
                    {
                        if (!root.customProcessed.vectors.ContainsKey (kvp.Key))
                            root.customProcessed.vectors.Add (kvp.Key, kvp.Value);
                        else
                            root.customProcessed.vectors[kvp.Key] = kvp.Value;
                    }
                }
                
                if (current.custom.strings != null && current.custom.strings.Count > 0)
                {
                    if (root.customProcessed.strings == null)
                        root.customProcessed.strings = new SortedDictionary<string, string> ();

                    foreach (var kvp in current.custom.strings)
                    {
                        if (!root.customProcessed.strings.ContainsKey (kvp.Key))
                            root.customProcessed.strings.Add (kvp.Key, kvp.Value);
                        else
                            root.customProcessed.strings[kvp.Key] = kvp.Value;
                    }
                }
            }
            
            // Just in case we somehow missed a cyclical dependency
            if (depth > 20)
            {
                Debug.LogWarning ($"Subsystem {current.key} fails to complete recursive processing in under 20 steps. Current hierarchy:\n{current.parentHierarchy}");
                return;
            }

            if (string.IsNullOrEmpty (current.parent))
                return;
            
            if (depth > 0)
                root.parentHierarchy += $"←  {current.parent}  ";

            if (current.key == current.parent)
            {
                Debug.LogWarning ($"Subsystem {current.key} references itself as a parent, which is invalid configuration - overriding");
                current.parent = null;
                return;
            }

            var archetypeParent = GetEntry (current.parent);
            if (archetypeParent == null)
            {
                Debug.LogWarning ($"Failed to find parent subsystem {current.parent} for subsystem {current.key}");
                return;
            }

            ProcessRecursive (archetypeParent, root, depth + 1);
        }

        private static void AppendToNameString (ref string target, string value)
        {
            if (string.IsNullOrEmpty (target))
                target = value;
            else
            {
                var s = target;
                if (s.EndsWith (")"))
                    s = s.Substring (0, s.Length - 1) + $", {value})";
                else
                    s = $"{s} ({value})";
                target = s;
            }
        }

        private static void AppendToDescString (ref string target, string value)
        {
            if (string.IsNullOrEmpty (target))
                target = value;
            else
                target = $"{target}\n\n{value}";
        }
        
        

        public static List<string> GetSubsystemKeysWithHardpoint (string hardpoint)
        {
            if (hardpointsKeyMap == null || hardpointsKeyMap.Count == 0 || string.IsNullOrEmpty (hardpoint))
                return null;
            if (!hardpointsKeyMap.TryGetValue (hardpoint, out var set))
                return null;
            return set;
        }
        
        public static List<DataContainerSubsystem> GetSubsystemsWithHardpoint (string hardpoint)
        {
            if (hardpointsDataMap == null || hardpointsDataMap.Count == 0 || string.IsNullOrEmpty (hardpoint))
                return null;
            if (!hardpointsDataMap.TryGetValue (hardpoint, out var set))
                return null;
            return set;
        }
        
        public void OnDestroy ()
        {
            #if UNITY_EDITOR
            DestroyVisualHolder ();
            #endif
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
        
        private static GameObject visualHolder;
        
        [FoldoutGroup ("Utilities", false), PropertyOrder (-20)]
        [Button (ButtonSizes.Large)]
        public static void DestroyVisualHolder ()
        {
            if (visualHolder == null)
                visualHolder = GameObject.Find ("DataPreviewHolder");
            
            if (visualHolder != null)
            {
                Debug.Log ("Cleaning up visual helper for DataMultiLinkerSubsystem");
                UtilityGameObjects.ClearChildren (visualHolder.transform);
                DestroyImmediate (visualHolder);
                visualHolder = null;
            }
        }
        
        public static GameObject VisualizeObject (DataContainerSubsystem subsystem, bool processed, bool clearHolder = true, bool focus = true)
        {
            if (!AssetPackageHelper.AreUnitAssetsInstalled ())
            {
                Debug.LogWarning (AssetPackageHelper.unitAssetWarning);
                return null;
            }
            
            if (subsystem == null)
                return null;
            
            if (processed)
            {
                if (subsystem.visualsProcessed == null && subsystem.attachmentsProcessed == null)
                {
                    Debug.LogWarning ($"Subsystem {subsystem.key}: No processed visuals or attachments to visualize");
                    return null;
                }
            }
            else
            {
                if (subsystem.visuals == null && subsystem.attachments == null)
                {
                    Debug.LogWarning ($"Subsystem {subsystem.key}: No embedded visuals or attachments to visualize");
                    return null;
                }
            }
            
            if (visualHolder == null)
                visualHolder = GameObject.Find ("DataPreviewHolder");
            if (visualHolder == null)
            {
                visualHolder = new GameObject ("DataPreviewHolder");
                // visualHolder.hideFlags = HideFlags.HideAndDontSave;
                visualHolder.transform.position = new Vector3 (0f, 50f, 0f);
            }

            if (clearHolder)
                UtilityGameObjects.ClearChildren (visualHolder.transform);

            var subholder = new GameObject (subsystem.key).transform;
            subholder.parent = visualHolder.transform;
            subholder.SetLocalTransformationToZero ();

            var visuals = processed ? subsystem.visualsProcessed : subsystem.visuals;
            if (visuals != null && visuals.Count > 0)
            {
                foreach (var key in visuals)
                {
                    if (string.IsNullOrEmpty (key))
                        continue;
                    
                    var visualPrefab = ItemHelper.GetVisual (key, false);
                    if (visualPrefab == null)
                    {
                        Debug.LogWarning ($"Subsystem {subsystem.key}: Failed to fetch visual prefab named {key}");
                        continue;
                    }

                    var visualInstance = Instantiate (visualPrefab.gameObject).GetComponent<ItemVisual> ();
                    var t = visualInstance.transform;
                    t.name = visualPrefab.name;
                    t.parent = subholder;
                    t.localPosition = (visualInstance.customTransform ? visualInstance.customPosition : Vector3.zero);
                    t.localRotation = (visualInstance.customTransform ? Quaternion.Euler (visualInstance.customRotation) : Quaternion.identity);
                    t.localScale = Vector3.one;
                }
            }
            
            var attachments = processed ? subsystem.attachmentsProcessed : subsystem.attachments;
            if (attachments != null && attachments.Count > 0)
            {
                foreach (var kvp in attachments)
                {
                    var block = kvp.Value;
                    if (block == null)
                        continue;
                
                    var visualPrefab = ItemHelper.GetVisual (block.key, false);
                    if (visualPrefab == null)
                    {
                        Debug.LogWarning ($"Subsystem {subsystem.key}: Failed to fetch visual prefab named {block.key}");
                        continue;
                    }

                    var visualInstance = Instantiate (visualPrefab.gameObject).GetComponent<ItemVisual> ();
                    var t = visualInstance.transform;
                    var rotationQt = Quaternion.Euler (block.rotation);

                    var position = block.position;
                    if (block.centered)
                    {
                        if (visualInstance.renderers != null && visualInstance.renderers.Count > 0)
                        {
                            var bounds = visualInstance.GetRendererBounds ();
                            var centerScaled = new Vector3 (bounds.center.x * block.scale.x, bounds.center.y * block.scale.y, bounds.center.z * block.scale.z);
                            position -= rotationQt * centerScaled;
                        }
                    }
                    
                    t.name = visualPrefab.name;
                    t.parent = subholder;
                    t.localPosition = (visualInstance.customTransform ? visualInstance.customPosition : Vector3.zero) + position;
                    t.localRotation = (visualInstance.customTransform ? Quaternion.Euler (visualInstance.customRotation) : Quaternion.identity) * Quaternion.Euler (block.rotation);
                    t.localScale = block.scale;
                }
            }

            if (focus && UnityEditor.SceneView.lastActiveSceneView != null)
            {
                Bounds bounds = new Bounds (visualHolder.transform.position, Vector3.zero);
                MeshRenderer[] renderers = visualHolder.GetComponentsInChildren<MeshRenderer> ();
                for (int i = 0; i < renderers.Length; ++i)
                {
                    var mr = renderers[i];
                    bounds.Encapsulate (mr.bounds);
                }
                
                if (renderers.Length != 0)
                    UnityEditor.SceneView.lastActiveSceneView.Frame (bounds);
                else
                    UnityEditor.SceneView.lastActiveSceneView.LookAt (visualHolder.transform.position);
            }

            return subholder.gameObject;
        }


        [FoldoutGroup ("Utilities", false), PropertyOrder (-20)]
        [Button]
        public void MultiplyStat (string statKey, float multiplier)
        {
            if (statKey == null)
                return;
            
            foreach (var kvp in dataFiltered)
            {
                var config = kvp.value;
                if (config == null || config.stats == null || !config.stats.ContainsKey (statKey))
                    continue;

                var block = config.stats[statKey];
                if (block.value.RoughlyEqual (1f))
                    continue;

                var valueOld = block.value;
                block.value *= multiplier;

                var count = config.stats.ContainsKey (UnitStats.activationCount) ? Mathf.RoundToInt (config.stats[UnitStats.activationCount].value) : 0;
                if (count <= 1)
                    Debug.Log ($"{statKey} on {config.key}: {valueOld:0.##} → {block.value:0.##}");
                else
                {
                    var totalOld = valueOld * count;
                    var totalNew = block.value * count;
                    Debug.Log ($"{statKey} on {config.key}: {valueOld:0.##} → {block.value:0.##} | Shots: {count} | Full volley: {totalOld:0.##} → {totalNew:0.##}");
                }
            }
        }

        [FoldoutGroup ("Utilities", false), PropertyOrder (-20)]
        [Button (ButtonSizes.Large)]
        public void LogProximityFuse ()
        {
            foreach (var kvp in data)
            {
                var c = kvp.Value;
                if (c.projectile?.guidanceData != null)
                {
                    var fuse = c.projectile.fuseProximity != null;
                    if (fuse)
                        Debug.Log ($"{kvp.Key} | Guided, using proximity fuse at {c.projectile.fuseProximity.distance:0.#}m");
                    else
                        Debug.LogWarning ($"{kvp.Key} | Guided, not using proximity fuse");
                }
            }
        }
        
        [FoldoutGroup ("Utilities", false), PropertyOrder (-20)]
        [Button (ButtonSizes.Large)]
        public void UpdateGuidanceFormat ()
        {
            foreach (var kvp in data)
            {
                var c = kvp.Value;
                if (c.projectile?.guidanceData != null)
                {
                    var gd = c.projectile.guidanceData;
                    gd.rigidbodyDriftDrag = gd.rigidbodyDrag;
                }
            }
        }
        
        [FoldoutGroup ("Utilities", false), PropertyOrder (-20)]
        [Button (ButtonSizes.Large)]
        public void UpdateMissileBodyAssets ()
        {
            foreach (var kvp in data)
            {
                var c = kvp.Value;
                if (c.projectile?.visual?.body != null)
                {
                    var b = c.projectile.visual.body;
                    var keyOld = b.key;
                    if (!keyOld.Contains ("fx_projectile_missile_01"))
                        continue;
                    
                    b.key = "fx_projectile_missile_01_regular";
                    b.keyEnemy = null;
                    
                    var scale = Vector3.one;
                    if (keyOld.Contains ("large"))
                        scale *= 1.6f;
                    else if (keyOld.Contains ("small"))
                        scale *= 0.7f;

                    b.scale = scale;
                }
            }
        }

        /*
        [FoldoutGroup ("Utilities", false), PropertyOrder (-20)]
        [Button (ButtonSizes.Large)]
        public void Migrate ()
        {
            data.Clear ();
            
            // Migrate lowest level subsystems
            foreach (var kvp in DataMultiLinkerSubsystemBlueprint.data)
            {
                var key = kvp.Key;
                if (data.ContainsKey (key))
                {
                    Debug.LogWarning ($"Can't migrate key {key} from blueprint DB");
                    continue;
                }
                
                var sb = kvp.Value;
                if (sb == null)
                    continue;

                var s = new DataContainerSubsystem ();
                s.key = sb.key;
                data.Add (s.key, s);

                if (sb.archetype != null)
                    s.parent = sb.archetype;

                if (sb.tags != null && sb.tags.Count > 0)
                    s.tags = new HashSet<string> (sb.tags);

                if (sb.hardpointsCompatible != null && sb.hardpointsCompatible.Count > 0)
                    s.hardpoints = new List<string> (sb.hardpointsCompatible);

                if (sb.visualPrefabs != null && sb.visualPrefabs.Count > 0)
                    s.visuals = new List<string> (sb.visualPrefabs);

                if (sb.stats != null && sb.stats.Count > 0)
                {
                    foreach (var kvp2 in sb.stats)
                    {
                        var statKey = kvp2.Key;
                        var statBlock = kvp2.Value;

                        s.stats.Add (statKey, new DataBlockSubsystemStat
                        {
                            value = statBlock.multiplier,
                            targetMode = statBlock.targeted ? 1 : 0,
                            targetSocket = statBlock.targeted && !string.IsNullOrEmpty (statBlock.targetSocket) ? statBlock.targetSocket : string.Empty,
                            targetHardpoint = statBlock.targeted && !string.IsNullOrEmpty (statBlock.targetHardpoint) ? statBlock.targetHardpoint : string.Empty,
                        });
                    }
                }
            }
            
            // Migrate archetypes
            foreach (var kvp in DataMultiLinkerSubsystemArchetype.data)
            {
                var key = kvp.Key;
                if (data.ContainsKey (key))
                {
                    Debug.LogWarning ($"Can't migrate key {key} from archetype DB");
                    continue;
                }
                
                var sa = kvp.Value;
                if (sa == null)
                    continue;

                var s = new DataContainerSubsystem ();
                s.key = sa.key;
                data.Add (s.key, s);

                if (sa.parent != null)
                    s.parent = sa.parent;

                if (sa.tags != null && sa.tags.Count > 0)
                    s.tags = new HashSet<string> (sa.tags);

                if (sa.stats != null && sa.stats.Count > 0)
                {
                    foreach (var kvp2 in sa.stats)
                    {
                        var statKey = kvp2.Key;
                        var statBlock = kvp2.Value;

                        s.stats.Add (statKey, new DataBlockSubsystemStat
                        {
                            value = statBlock.baseValue,
                            targetMode = 0,
                            targetSocket = string.Empty,
                            targetHardpoint = string.Empty,
                        });
                    }
                }
            }
        }
        */

        [FoldoutGroup ("Utilities", false), PropertyOrder (-20)]
        [Button (ButtonSizes.Large)]
        public void AssignDistributions ()
        {
            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var subsystem = kvp.Value;
                
                if (subsystem.parent == null || !subsystem.parent.Contains ("armor_"))
                    continue;

                Debug.Log ($"Subsystem {key} gets stat distribution for armor");
                
                subsystem.statDistribution = "armor";
                subsystem.OnAfterDeserialization (key);
            }
            
            OnAfterDeserialization ();
        }
        
        [FoldoutGroup ("Utilities", false), PropertyOrder (-20)]
        [Button (ButtonSizes.Large)]
        public void AddHardpointTextToArmor ()
        {
            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var subsystem = kvp.Value;
                
                if (subsystem.statDistribution == null || subsystem.statDistribution != "armor")
                    continue;

                var hardpointKey = subsystem.hardpoints.FirstOrDefault ();
                Debug.Log ($"Subsystem {key} gets text suffix from hardpoint {hardpointKey}");
                subsystem.textNameFromHardpoint = new DataBlockEquipmentTextFromHardpoint ();
                // subsystem.OnAfterDeserialization (key);
            }
            
            OnAfterDeserialization ();
        }
        
        /*
        [FoldoutGroup ("Utilities", false), PropertyOrder (-20)]
        [Button (ButtonSizes.Large)]
        public void MovePerkAndAuxText ()
        {
            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var subsystem = kvp.Value;
                
                if (!key.Contains ("aux") && !key.Contains ("perk"))
                    continue;

                var subsystemOld = DataMultiLinkerSubsystemBlueprint.GetEntry (key);
                if (subsystemOld == null)
                    continue;

                if (subsystem.textName == null && !string.IsNullOrEmpty (subsystemOld.textName))
                {
                    subsystem.textName = new DataBlockEquipmentTextName { s = subsystemOld.textName };
                    Debug.Log ($"Moved name in {key}: {subsystemOld.textName}");
                }
                
                if (subsystem.textDesc == null && !string.IsNullOrEmpty (subsystemOld.textDesc))
                {
                    subsystem.textDesc = new DataBlockEquipmentTextDesc { s = subsystemOld.textDesc };
                    Debug.Log ($"Moved description in {key}: {subsystemOld.textDesc}");
                }
            }
        }
        */
        
        [FoldoutGroup ("Utilities", false), PropertyOrder (-20)]
        [Button (ButtonSizes.Large)]
        public void AssignHiddenFlags ()
        {
            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var subsystem = kvp.Value;
                subsystem.hidden = subsystem.children != null && subsystem.children.Count > 0;
            }
        }
        
        [FoldoutGroup ("Utilities", false), PropertyOrder (-20)]
        [Button (ButtonSizes.Large)]
        public void RemoveHardpointRedirect ()
        {
            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var subsystem = kvp.Value;
                
                if (subsystem.activation == null)
                    continue;

                var a = subsystem.activation;
                if (a.visual == null)
                    continue;

                var v = a.visual;
                if (!string.IsNullOrEmpty (v.localHardpointOverride))
                {
                    // Skip tanks
                    if (v.localSocketOverride == "secondary")
                        continue;
                    
                    Debug.Log ($"Clearing socket/hardpoint overrides on subsystem {key}: [{v.localSocketOverride}]/[{v.localHardpointOverride}]");
                    v.localSocketOverride = string.Empty;
                    v.localHardpointOverride = string.Empty;
                }
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void AssignRatingIndexes ()
        {
            foreach (var kvp in data)
            {
                var subsystemKey = kvp.Key;
                var subsystem = kvp.Value;

                int ratingDetected = -1;
                if (subsystemKey.EndsWith ("_r1"))
                    ratingDetected = 1;
                else if (subsystemKey.EndsWith ("_r2"))
                    ratingDetected = 2;
                else if (subsystemKey.EndsWith ("_r3"))
                    ratingDetected = 3;

                if (ratingDetected != -1 && ratingDetected != subsystem.rating)
                {
                    Debug.Log ($"Changing subsystem rating for {subsystemKey}: {subsystem.rating} → {ratingDetected}");
                    subsystem.rating = ratingDetected;
                }
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void AssignDescriptionFromPreset ()
        {
            foreach (var kvp in data)
            {
                var subsystemKey = kvp.Key;
                var subsystem = kvp.Value;
                
                if (!subsystemKey.StartsWith ("armor") || string.IsNullOrEmpty (subsystem.textNameFromPreset))
                    continue;

                subsystem.textDescFromPreset = subsystem.textNameFromPreset;
                Debug.LogWarning ($"{subsystemKey} | Set description from preset {subsystem.textNameFromPreset}");
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void LogMissingDamage ()
        {
            foreach (var kvp in data)
            {
                var subsystemKey = kvp.Key;
                var subsystem = kvp.Value;

                if (subsystem.hidden || !subsystemKey.Contains ("wpn") || subsystemKey.Contains ("shield"))
                    continue;

                var block = subsystem.stats[UnitStats.weaponDamage];
                if (block.value < 1f)
                    Debug.LogWarning ($"{subsystemKey} - wpn_damage: {block.value}");
                else
                    Debug.Log ($"{subsystemKey} - wpn_damage: {block.value}");
            }
        }

        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void LogWeaponHardpoints ()
        {
            var hardpointMain = "internal_main_equipment";
            
            foreach (var kvp in data)
            {
                var subsystemKey = kvp.Key;
                var subsystem = kvp.Value;
                
                if (!subsystemKey.Contains ("wpn_"))
                    continue;

                bool hardpointMainFound = subsystem.hardpointsProcessed != null && subsystem.hardpointsProcessed.Contains (hardpointMain);
                if (!hardpointMainFound)
                {
                    Debug.Log ($"{subsystemKey}: no hardpoint {hardpointMain}");
                }
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void LogStatsForSplash ()
        {
            foreach (var kvp in data)
            {
                var subsystemKey = kvp.Key;
                var subsystem = kvp.Value;

                if (subsystem.hidden || !subsystemKey.Contains ("wpn") || subsystemKey.Contains ("shield"))
                    continue;

                string projectileText = subsystem.projectile != null ? "Projectile defined directly" : subsystem.projectileProcessed != null ? "Projectile defined in parent" : "No projectile data";
                bool splashDamage = subsystem.projectileProcessed != null && subsystem.projectileProcessed.splashDamage != null;
                bool splashImpact = subsystem.projectileProcessed != null && subsystem.projectileProcessed.splashImpact != null;
                
                if (subsystem.statsProcessed.ContainsKey (UnitStats.weaponDamageRadius))
                    Debug.LogWarning ($"{subsystemKey} | {UnitStats.weaponDamageRadius} present | Splash damage possible: {splashDamage} | {projectileText}");
                
                if (subsystem.statsProcessed.ContainsKey (UnitStats.weaponImpactRadius))
                    Debug.LogWarning ($"{subsystemKey} | {UnitStats.weaponImpactRadius} present | Splash impact possible: {splashImpact} | {projectileText}");
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void LogStatsForGuidance ()
        {
            foreach (var kvp in data)
            {
                var subsystemKey = kvp.Key;
                var subsystem = kvp.Value;

                if (subsystem.hidden || !subsystemKey.Contains ("wpn") || subsystemKey.Contains ("shield") || subsystem.projectileProcessed == null || subsystem.projectileProcessed.guidanceData == null)
                    continue;

                var lifetimeFound = subsystem.statsProcessed.TryGetValue (UnitStats.weaponProjectileLifetime, out var lifetime);
                var uiSpeedAverage = subsystem.projectileProcessed.uiSpeedAverage != null ? subsystem.projectileProcessed.uiSpeedAverage.f : 0f;
                var uiTimeAverage100 = uiSpeedAverage > 0f ? (100f / uiSpeedAverage).ToString ("0.##") : "?";
                Debug.Log ($"{subsystemKey} | Lifetime: {(lifetimeFound ? lifetime.value.ToString () : "?")} | Prediction speed: {uiSpeedAverage} | Prediction time to 100m: {uiTimeAverage100}");
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void LogFalloffGlobal ()
        {
            foreach (var kvp in data)
            {
                var subsystemKey = kvp.Key;
                var subsystem = kvp.Value;

                if (subsystem.hidden || !subsystemKey.Contains ("wpn") || subsystemKey.Contains ("shield") || subsystem.projectile == null || subsystem.projectile.falloff == null)
                    continue;
                
                if (subsystem.projectile.falloffGlobal != null)
                {
                    var f = subsystem.projectile.falloffGlobal;
                    Debug.Log ($"{subsystemKey} | Has global falloff | Start from stat: {f.rangeStartFromStat} | {f.rangeStart}+{f.gradientSize}");
                }
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void LogStatPresence (string statKey)
        {
            if (string.IsNullOrEmpty (statKey))
                return;
            
            foreach (var kvp in data)
            {
                var subsystemKey = kvp.Key;
                var subsystem = kvp.Value;

                if (subsystem.hidden || subsystem.statsProcessed == null)
                    continue;

                if (!subsystem.statsProcessed.TryGetValue (statKey, out var statData))
                    continue;

                if (statData.value <= 0f)
                    continue;
                
                Debug.Log ($"{subsystemKey} | {statKey}: {statData.value:0.###} ({statData.report})");
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void LogCustomFlags ()
        {
            foreach (var kvp in data)
            {
                var subsystemKey = kvp.Key;
                var subsystem = kvp.Value;

                if (subsystem.customProcessed == null || subsystem.customProcessed.flags == null || subsystem.customProcessed.flags.Count == 0)
                    continue;

                Debug.Log ($"{subsystemKey} | Custom flags: {subsystem.customProcessed.flags.ToStringFormatted ()}");
            }
        }

        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void FixDuplicatedHardpoints ()
        {
            foreach (var kvp in data)
            {
                var subsystemKey = kvp.Key;
                var subsystem = kvp.Value;

                if (!subsystem.hidden || subsystem.children == null || subsystem.children.Count == 0)
                    continue;
                
                if (subsystem.hardpoints != null && subsystem.hardpoints.Count > 0)
                    continue;

                string hp = null;
                bool hpMatch = true;
                
                DataContainerSubsystem child = null;
                foreach (var ck in subsystem.children)
                {
                    var c = DataMultiLinkerSubsystem.GetEntry (ck);
                    if (c.hardpoints == null)
                    {
                        hpMatch = false;
                        break;
                    }

                    var hpc = c.hardpoints.FirstOrDefault ();
                    if (hpc == null || (hp != null && hpc != hp))
                    {
                        hpMatch = false;
                        break;
                    }

                    hp = hpc;
                }

                if (hpMatch)
                {
                    Debug.Log ($"Detected unwanted hardpoint duplication under subsystem {subsystemKey} with hardpoint {hp}:\n{subsystem.children.ToStringFormatted ()}");
                    subsystem.hardpoints = new List<string> { hp };
                    foreach (var ck in subsystem.children)
                    {
                        var c = DataMultiLinkerSubsystem.GetEntry (ck);
                        c.hardpoints = null;
                    }
                }
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void FixDuplicatedText ()
        {
            foreach (var kvp in data)
            {
                var subsystemKey = kvp.Key;
                var subsystem = kvp.Value;

                if (!subsystem.hidden || subsystem.children == null || subsystem.children.Count == 0)
                    continue;
                
                if (subsystem.textName != null && !string.IsNullOrEmpty (subsystem.textName.s))
                    continue;

                string name = null;
                bool nameMatch = true;
                
                DataContainerSubsystem child = null;
                foreach (var ck in subsystem.children)
                {
                    var c = DataMultiLinkerSubsystem.GetEntry (ck);
                    if (c.textName == null)
                    {
                        nameMatch = false;
                        break;
                    }

                    var nameCandidate = c.textName.s;
                    if (nameCandidate == null || (name != null && nameCandidate != name))
                    {
                        nameMatch = false;
                        break;
                    }

                    name = nameCandidate;
                }

                if (nameMatch)
                {
                    Debug.Log ($"Detected unwanted name duplication under subsystem {subsystemKey} with name {name}:\n{subsystem.children.ToStringFormatted ()}");
                    subsystem.textName = new DataBlockEquipmentTextName { s = name };
                    foreach (var ck in subsystem.children)
                    {
                        var c = DataMultiLinkerSubsystem.GetEntry (ck);
                        c.textName = null;
                        c.textDesc = null;
                    }
                }
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void ConvertPrefabToAttachments (
            [ValueDropdown ("@DataMultiLinkerSubsystem.data.Keys")] string key,
            GameObject prefab
        )
        {
            if (prefab == null)
                return;
            
            var blueprint = GetEntry (key);
            if (blueprint == null)
                return;

            var visuals = prefab.GetComponentsInChildren<ItemVisual> ();
            if (visuals.Length == 0)
            {
                Debug.LogWarning ($"No visuals found in referenced prefab {prefab.name}");
                return;
            }

            var root = prefab.transform;
            Debug.Log ($"Found {visuals.Length} visuals in referenced prefab {prefab.name}");

            blueprint.attachments = new Dictionary<string, DataBlockSubsystemAttachment> ();
            int i = 0;
            
            foreach (var visual in visuals)
            {
                var t = visual.transform;
                var lookupKey = $"{i:00}_{visual.name}";
                var pos = t.localPosition;
                var rot = t.localRotation.eulerAngles; // Quaternion.LookRotation (root.InverseTransformDirection (t.forward)).eulerAngles;
                var scale = t.localScale;

                var visualKey = visual.name;
                var path = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot (visual);
                if (!string.IsNullOrEmpty (path) && path.EndsWith (".prefab"))
                {
                    path = path.Replace (".prefab", string.Empty);
                    var split = path.Split ('/');
                    if (split.Length > 1)
                        path = split[split.Length - 1];
                    visualKey = path;
                }
                
                blueprint.attachments[lookupKey] = new DataBlockSubsystemAttachment
                {
                    key = visualKey,
                    position = pos,
                    rotation = rot,
                    scale = scale
                };
                
                Debug.Log ($"{key} attachment {visualKey}: {visual.name}, pos. {pos}, rot. {rot}, scale {scale}\nPath: {path}");
                i += 1;
            }
        }
        
        /*
        [FoldoutGroup ("Utilities", false), PropertyOrder (-20)]
        [Button (ButtonSizes.Large)]
        public void MoveBlocksFromBlueprints ()
        {
            foreach (var kvp in DataMultiLinkerSubsystemBlueprint.data)
            {
                var key = kvp.Key;
                var subsystemBlueprint = kvp.Value;

                var subsystemNew = GetEntry (key);
                if (subsystemNew == null)
                    continue;

                if (subsystemBlueprint.dataActivation != null)
                    subsystemNew.activation = subsystemBlueprint.dataActivation;
                
                if (subsystemBlueprint.dataProjectile != null)
                    subsystemNew.projectile = subsystemBlueprint.dataProjectile;
                
                if (subsystemBlueprint.dataBeam != null)
                    subsystemNew.beam = subsystemBlueprint.dataBeam;
                
                if (subsystemBlueprint.dataCustom != null)
                    subsystemNew.custom = subsystemBlueprint.dataCustom;
            }
            
            OnAfterDeserialization ();
        }
        */
        
        /*
        [FoldoutGroup ("Utilities", false), PropertyOrder (-20)]
        [Button (ButtonSizes.Large)]
        public void MoveOldBlocks ()
        {
            foreach (var kvp in data)
            {
                var subsystem = kvp.Value;

                if (subsystem.activation != null)
                {
                    subsystem.activationOld = subsystem.activation;
                    subsystem.activation = null;
                }
                
                if (subsystem.projectile != null)
                {
                    subsystem.projectileOld = subsystem.projectile;
                    subsystem.projectile = null;
                }
                
                if (subsystem.beam != null)
                {
                    subsystem.beamOld = subsystem.beam;
                    subsystem.beam = null;
                }
            }
            
            OnAfterDeserialization ();
        }
        */

        /*
        [FoldoutGroup ("Utilities", false), PropertyOrder (-20)]
        [Button (ButtonSizes.Large)]
        public void ConvertOldBlocks ()
        {
            foreach (var kvp in data)
            {
                var subsystem = kvp.Value;

                if (subsystem.activationOld != null)
                {
                    var old = subsystem.activationOld;
                    var a = new DataBlockSubsystemActivation_V2 ();
                    subsystem.activation = a;

                    a.visual = new DataBlockSubsystemActivationVisual
                    {
                        local = new DataBlockAssetFactionBased
                        {
                            factionUsed = old.visualByFaction,
                            key = old.visualName,
                            keyEnemy = old.visualNameEnemy
                        },
                        localSocketOverride = old.visualSocketOverride,
                        localHardpointOverride = old.visualHardpointOverride
                    };

                    old.visualName = null;
                    old.visualNameEnemy = null;
                    old.visualSocketOverride = null;
                    old.visualHardpointOverride = null;
                    
                    if (!old.soundOnFirstActivation.IsNullOrEmpty () || !old.soundOnMidActivation.IsNullOrEmpty () || !old.soundOnLastActivation.IsNullOrEmpty () || !old.soundLoopOnActivationStart.IsNullOrEmpty () || !old.soundLoopOnActivationEnd.IsNullOrEmpty ())

                    a.audio = new DataBlockSubsystemActivationAudio
                    {
                        onActivationFirst = old.soundOnFirstActivation,
                        onActivationMid = old.soundOnMidActivation,
                        onActivationLast = old.soundOnLastActivation,
                        onActivationStart = old.soundLoopOnActivationStart,
                        onActivationEnd = old.soundLoopOnActivationEnd
                    };
                    
                    old.soundOnFirstActivation = null;
                    old.soundOnMidActivation = null;
                    old.soundOnLastActivation = null;
                    old.soundLoopOnActivationStart = null;
                    old.soundLoopOnActivationEnd = null;

                    a.light = new DataBlockSubsystemActivationLight
                    {
                        shared = old.lightProfileShared,
                        key = old.lightProfileKey,
                        custom = old.lightProfileCustom
                    };

                    old.lightProfileKey = null;
                    old.lightProfileCustom = null;

                    a.recoil = new DataBlockSubsystemActivationRecoil
                    {
                        key = old.recoil
                    };

                    old.recoil = null;

                    // Clear old data to prevent YAML linking
                    subsystem.activationOld = null;
                }

                if (subsystem.projectileOld != null)
                {
                    var old = subsystem.projectileOld;
                    var p = new DataBlockSubsystemProjectile_V2 ();
                    subsystem.projectile = p;

                    // Move dispersion bool to flags
                    if (old.damageDispersed)
                    {
                        if (subsystem.custom == null)
                            subsystem.custom = new DataBlockPartCustom ();

                        if (subsystem.custom.flags == null)
                            subsystem.custom.flags = new HashSet<string> ();

                        if (!subsystem.custom.flags.Contains (PartCustomFlagKeys.DamageDispersed))
                            subsystem.custom.flags.Add (PartCustomFlagKeys.DamageDispersed);
                    }

                    // Move some floats to stats
                    if (old.lifetime > 0f || old.ricochetChance > 0f)
                    {
                        if (subsystem.stats == null)
                            subsystem.stats = new SortedDictionary<string, DataBlockSubsystemStat> ();

                        if (old.lifetime > 0f)
                            subsystem.stats[UnitStats.weaponProjectileLifetime] = new DataBlockSubsystemStat { value = old.lifetime };

                        if (old.ricochetChance > 0f)
                            subsystem.stats[UnitStats.weaponProjectileRicochet] = new DataBlockSubsystemStat { value = old.ricochetChance };
                    }

                    p.visual = new DataBlockSubsystemProjectileVisual
                    {
                        body = new DataBlockAssetFactionBased
                        {
                            factionUsed = old.visualByFaction,
                            key = old.visualName,
                            keyEnemy = old.visualNameEnemy,
                            scale = old.visualSize
                        }
                    };
                    
                    old.visualName = null;
                    old.visualNameEnemy = null;

                    if (!string.IsNullOrEmpty (old.fxOnImpact))
                    {
                        p.visual.impact = new DataBlockAssetFactionBased { key = old.fxOnImpact };
                        old.fxOnImpact = null;
                    }

                    if (!string.IsNullOrEmpty (old.fxOnDeactivation))
                    {
                        p.visual.deactivation = new DataBlockAssetFactionBased { key = old.fxOnDeactivation };
                        old.fxOnDeactivation = null;
                    }

                    if (!string.IsNullOrEmpty (old.soundOnImpact))
                    {
                        p.audio = new DataBlockSubsystemProjectileAudio { onImpact = old.soundOnImpact };
                        old.soundOnImpact = null;
                    }

                    if (old.fragmentCount > 1)
                    {
                        p.fragmentation = new DataBlockSubsystemProjectileFragmentation
                        {
                            count = old.fragmentCount,
                            angleFromStat = true
                        };
                    }

                    if (old.falloff != null)
                    {
                        p.falloff = old.falloff;
                        old.falloff = null;
                    }

                    if (old.animationFade != null)
                    {
                        p.animationFade = old.animationFade;
                        old.animationFade = null;
                    }
                    
                    if (old.ballistics != null)
                    {
                        p.ballistics = old.ballistics;
                        old.ballistics = null;
                    }
                    
                    if (old.fuseProximity != null)
                    {
                        p.fuseProximity = old.fuseProximity;
                        old.fuseProximity = null;
                    }
                    
                    if (old.splashDamage != null)
                    {
                        p.splashDamage = old.splashDamage;
                        old.splashDamage = null;
                    }
                    
                    if (old.splashImpact != null)
                    {
                        p.splashImpact = old.splashImpact;
                        old.splashImpact = null;
                    }
                    
                    if (old.guidanceData != null)
                    {
                        p.guidanceData = old.guidanceData;
                        old.guidanceData = null;
                    }
                    
                    if (old.guidanceAudio != null)
                    {
                        p.guidanceAudio = old.guidanceAudio;
                        old.guidanceAudio = null;
                    }
                    
                    if (old.uiTrajectory != null)
                    {
                        p.uiTrajectory = old.uiTrajectory;
                        old.uiTrajectory = null;
                    }

                    // Clear old data to prevent YAML linking
                    subsystem.projectileOld = null;
                }

                if (subsystem.beamOld != null)
                {
                    var old = subsystem.beamOld;
                    var b = new DataBlockSubsystemBeam_V2 ();
                    subsystem.beam = b;

                    b.body = new DataBlockAssetFactionBased
                    {
                        factionUsed = old.visualByFaction,
                        key = old.visualName,
                        keyEnemy = old.visualNameEnemy
                    };
                    
                    if (subsystem.stats == null)
                        subsystem.stats = new SortedDictionary<string, DataBlockSubsystemStat> ();

                    if (old.maxDistance > 0f)
                        subsystem.stats[UnitStats.weaponRangeMax] = new DataBlockSubsystemStat { value = old.maxDistance };

                    if (old.damageRadius > 0f)
                        subsystem.stats[UnitStats.weaponDamageRadius] = new DataBlockSubsystemStat { value = old.damageRadius };

                    if (old.secondsToFullDamage > 0f)
                        subsystem.stats[UnitStats.weaponDamageBuildup] = new DataBlockSubsystemStat { value = old.secondsToFullDamage };

                    if (old.foldoutSpeed > 0f)
                        subsystem.stats[UnitStats.weaponProjectileSpeed] = new DataBlockSubsystemStat { value = old.foldoutSpeed };

                    subsystem.beamOld = null;
                }
            }
        }
        */
        
        #endif
    }
}


