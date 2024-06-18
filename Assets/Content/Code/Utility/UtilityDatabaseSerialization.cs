using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using PhantomBrigade.Data;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
using Unity.EditorCoroutines.Editor;
#endif

#if PB_MODSDK
using PhantomBrigade.SDK.ModTools;
#endif

[ExecuteInEditMode]
public class UtilityDatabaseSerialization : MonoBehaviour
{
    #if UNITY_EDITOR
    #if PB_MODSDK
    
    public static UtilityDatabaseSerialization ins;
    private static bool initialized = false;

    bool hasSelectedMod => DataContainerModData.hasSelectedConfigs;

    [ShowInInspector, PropertyOrder (-100), HideReferenceObjectPicker, HideLabel, HideDuplicateReferenceBox]
    private static ModConfigStatus status = new ModConfigStatus ();

    public static string configsPath => DataContainerModData.selectedMod.GetModPathConfigs ();

    bool enableSaveButtons => hasSelectedMod && IsUtilityOperationAvailable;
    #endif

    bool IsUtilityOperationAvailable => utilityCoroutine == null;
    EditorCoroutine utilityCoroutine;

    void OnUtilityCoroutineEnd ()
    {
        utilityCoroutine = null;
        EditorUtility.ClearProgressBar ();
    }

    static bool IsSubclassOfOpenGenericType (Type typeOpenGeneric, Type typeChecked)
    {
        while (typeChecked != null && typeChecked != typeof(object)) {
            var cur = typeChecked.IsGenericType ? typeChecked.GetGenericTypeDefinition() : typeChecked;
            if (typeOpenGeneric == cur) {
                return true;
            }
            typeChecked = typeChecked.BaseType;
        }
        return false;
    }

    public static List<Component> GetChildrenInheritingOpenGenericType (GameObject parent, Type typeOpenGeneric)
    {
        var componentsInChildren = parent.GetComponentsInChildren<Component> ();
        var componentsFiltered = new List<Component> ();

        foreach (var component in componentsInChildren)
        {
            if (component == null)
                continue;

            var componentType = component.GetType ();
            if (IsSubclassOfOpenGenericType (typeOpenGeneric, componentType))
                componentsFiltered.Add (component);
        }

        return componentsFiltered;
    }

    [PropertyOrder (OdinGroup.Order.UtilityCoroutine)]
    [HideIf (nameof(IsUtilityOperationAvailable))]
    [PropertySpace (0f, 8f)]
    [GUIColor ("lightred")]
    [Button ("Stop coroutine", ButtonSizes.Large)]
    public void CancelCoroutine ()
    {
        EditorCoroutineUtility.StopCoroutine (utilityCoroutine);
        OnUtilityCoroutineEnd ();
    }

    [VerticalGroup (OdinGroup.Name.Standard, Order = OdinGroup.Order.Standard)]
    [Button ("Log all", ButtonSizes.Large)]
    public void LogAll ()
    {
        var linkerType = typeof (DataLinker<>);
        var linkerComponents = GetChildrenInheritingOpenGenericType (gameObject, linkerType);
        var linkerText = linkerComponents.ToStringFormatted (multiline: true, toStringOverride: (x) => x.GetType ().Name);
        Debug.Log ($"Linkers ({linkerComponents.Count}):\n{linkerText}");

        var multilinkerType = typeof (DataMultiLinker<>);
        var multilinkerComponents = GetChildrenInheritingOpenGenericType (gameObject, multilinkerType);
        var multilinkerText = multilinkerComponents.ToStringFormatted (multiline: true, toStringOverride: (x) => x.GetType ().Name);
        Debug.Log ($"Multilinkers ({multilinkerComponents.Count}):\n{multilinkerText}");
    }
    
    [VerticalGroup (OdinGroup.Name.Standard)]
    [Button ("Load all linkers", ButtonSizes.Large)]
    public void LoadAllLinkers ()
    {
        var linkerType = typeof (DataLinker<>);
        var linkerComponents = GetChildrenInheritingOpenGenericType (gameObject, linkerType);
        foreach (var linkerComponent in linkerComponents)
        {
            var componentType = linkerComponent.GetType ();
            var methodInfoLoad = componentType.GetMethod ("LoadData", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (methodInfoLoad != null)
                methodInfoLoad.Invoke (null, null);
        }
    }

    [VerticalGroup (OdinGroup.Name.Standard)]
    [Button ("Load all multilinkers", ButtonSizes.Large)]
    public void LoadAllMultiLinkers ()
    {
        var multilinkerType = typeof (DataMultiLinker<>);
        var multilinkerComponents = GetChildrenInheritingOpenGenericType (gameObject, multilinkerType);
        foreach (var multilinkerComponent in multilinkerComponents)
        {
            var multilinkerInterface = multilinkerComponent as IDataMultiLinker;
            if (multilinkerInterface != null)
                multilinkerInterface.LoadDataLocal ();
        }
    }

    [VerticalGroup (OdinGroup.Name.Standard)]
    #if PB_MODSDK
    [EnableIf (nameof(enableSaveButtons))]
    #else
    [EnableIf (nameof(IsUtilityOperationAvailable))]
    #endif
    [Button ("Save all linkers", ButtonSizes.Large)]
    public void SaveAllLinkers ()
    {
        if (Application.isPlaying)
            return;

        if (EditorUtility.DisplayDialog ("Save all", "Are you sure you'd like to save every single linker DB?", "Confirm", "Cancel"))
        {
            var linkerType = typeof (DataLinker<>);
            utilityCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless (SaveAllChildrenInheritingOpenGenericTypeIE (linkerType));
        }
    }

    [VerticalGroup (OdinGroup.Name.Standard)]
    #if PB_MODSDK
    [EnableIf (nameof(enableSaveButtons))]
    #else
    [EnableIf (nameof(IsUtilityOperationAvailable))]
    #endif
    [Button ("Save all multilinkers", ButtonSizes.Large)]
    public void SaveAllMultilinkers ()
    {
        if (Application.isPlaying)
            return;

        if (EditorUtility.DisplayDialog ("Save all", "Are you sure you'd like to save every single multilinker DB?", "Confirm", "Cancel"))
        {
            var multilinkerType = typeof (DataMultiLinker<>);
            utilityCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless (SaveAllChildrenInheritingOpenGenericTypeIE (multilinkerType));
        }
    }

    public IEnumerator SaveAllChildrenInheritingOpenGenericTypeIE (Type typeOpenGeneric, bool forceLoad = true)
    {
        yield return null;

        var components = GetChildrenInheritingOpenGenericType (gameObject, typeOpenGeneric);
        var linkerText = components.ToStringFormatted (multiline: true, toStringOverride: (x) => x.GetType ().Name);
        Debug.Log ($"Children of type {typeOpenGeneric.Name} ({components.Count}):\n{linkerText}");

        for (int i = 0, count = components.Count; i < count; ++i)
        {
            var component = components[i];
            var componentType = component.GetType ();

            var methodInfoLoad = componentType.GetMethod ("LoadData", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            var methodInfoSave = componentType.GetMethod ("SaveData", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            var ok = forceLoad
                ? methodInfoLoad != null && methodInfoSave != null
                : methodInfoSave != null;

            if (ok)
            {
                var progress = (float)i / count;
                var percent = Mathf.RoundToInt (progress * 100f);
                var textHeader = $"{i + 1}/{count} - {percent}%";

                if (forceLoad)
                {
                    EditorUtility.DisplayProgressBar (textHeader, $"Loading {componentType.Name}", progress);
                    methodInfoLoad.Invoke (null, null);
                    yield return null;
                }

                EditorUtility.DisplayProgressBar (textHeader, $"Saving {componentType.Name}", progress);
                methodInfoSave.Invoke (null, null);
                yield return null;
            }
        }

        OnUtilityCoroutineEnd ();
    }

    #if PB_MODSDK
    public static IEnumerable<string> GetLinkerTypeNames ()
    {
        if (initialized && linkerComponentLookup != null)
            return linkerComponentLookup.Keys;
        return null;
    }

    public static IEnumerable<string> GetMultiLinkerTypeNames ()
    {
        if (initialized && multiLinkerComponentLookup != null)
            return multiLinkerComponentLookup.Keys;
        return null;
    }

    public static Component GetComponentForLinker (string typeName)
    {
        if (!initialized || string.IsNullOrEmpty (typeName))
            return null;

        if (linkerComponentLookup.TryGetValue (typeName, out var component))
            return component;

        return null;
    }

    public static Component GetComponentForMultiLinker (string typeName)
    {
        if (!initialized || string.IsNullOrEmpty (typeName))
            return null;

        if (multiLinkerComponentLookup.TryGetValue (typeName, out var component))
            return component;

        return null;
    }

    public static IDataMultiLinker GetInterfaceForMultiLinker (string typeName)
    {
        if (!initialized || string.IsNullOrEmpty (typeName))
            return null;

        if (multiLinkerInterfaceLookup.TryGetValue (typeName, out var component))
            return component;

        return null;
    }

    public static IDataMultiLinker GetMultiLinkerForContainer (Type containerType) =>
        initialized
            ? containerLookup.TryGetValue (containerType, out var dml)
                ? dml
                : null
            : null;

    public Dictionary<Type, IDataMultiLinker> FindAllMultiLinkers () => containerLookup;

    public static bool checksumsLoaded;
    public static ConfigChecksums.Checksum checksumSDKConfigsRoot;

    public static bool AnySDKConfigsChecksumChanges ()
    {
        if (!checksumsLoaded)
        {
            Debug.Log ("Checksums not loaded");
            return false;
        }

        var checksumPath = Path.Combine (DataPathHelper.GetApplicationFolder (), checksumSDKConfigsRootFileName);
        try
        {
            var checksumString = File.ReadAllText (checksumPath);
            if (checksumString.Length != 32)
            {
                Debug.Log ("Checksum length <> 32 : " + checksumString);
                return false;
            }
            var cksum = new ConfigChecksums.Checksum ();
            if (!long.TryParse (checksumString.Substring(0, 16), System.Globalization.NumberStyles.AllowHexSpecifier, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out cksum.HalfSum2))
            {
                Debug.Log ("Parse error HalfSum1");
                return false;
            }
            if (!long.TryParse (checksumString.Substring(16), System.Globalization.NumberStyles.AllowHexSpecifier, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out cksum.HalfSum1))
            {
                Debug.Log ("Parse error HalfSum2");
                return false;
            }
            return !ConfigChecksums.ChecksumEqual (cksum, checksumSDKConfigsRoot);
        }
        catch (IOException ioe)
        {
            Debug.LogError ("Failed to read SDK configs root checksum: " + checksumPath);
            Debug.LogException (ioe);
            return false;
        }
    }

    [VerticalGroup (OdinGroup.Name.Mod, Order = OdinGroup.Order.Mod)]
    [PropertySpace (8f)]
    [Button ("Reset loadedOnce for all\nloaded linkers & multilinkers", ButtonHeight = 40)]
    public void ResetLoadedOnce ()
    {
        foreach (var dml in multiLinkerInterfaceLookup.Values)
        {
            dml.ResetLoadedOnce ();
        }
        foreach (var linkerComponent in linkerComponentLookup.Values)
        {
            var mi = linkerComponent.GetType().GetMethod ("ResetLoadedOnce");
            if (mi == null)
            {
                continue;
            }
            mi.Invoke (linkerComponent, emptyParams);
        }
    }

    [VerticalGroup (OdinGroup.Name.Mod)]
    [EnableIf (nameof(IsUtilityOperationAvailable))]
    [PropertyTooltip ("Creates checksum file for SDK config DBs. This file is used to detect config changes in mods. The restore from SDK functionality expects this file to exist.")]
    [Button ("Create checksums for SDK config DBs", ButtonSizes.Large)]
    public void ChecksumSDKConfigs ()
    {
        utilityCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless (ChecksumSDKConfigsIE ());
    }

    [VerticalGroup (OdinGroup.Name.Mod)]
    [Button (ButtonSizes.Large)]
    public static void LoadSDKChecksums () => (checksumsLoaded, checksumSDKConfigsRoot) = ModToolsHelper.LoadSDKChecksums (force: true);

    [VerticalGroup (OdinGroup.Name.Mod)]
    [EnableIf (nameof(enableSaveChecksum))]
    [Button ("Save root checksum for SDK configs", ButtonSizes.Large)]
    public void SaveRootChecksum ()
    {
        if (!checksumsLoaded)
        {
            return;
        }
        // In text format, the checksum goes from MSB to LSB.
        var checksum = string.Format ("{0:x16}{1:x16}", checksumSDKConfigsRoot.HalfSum2, checksumSDKConfigsRoot.HalfSum1);
        var checksumPath = Path.Combine (DataPathHelper.GetApplicationFolder (), checksumSDKConfigsRootFileName);
        try
        {
            File.WriteAllText (checksumPath, checksum);
        }
        catch (IOException ioe)
        {
            Debug.LogError ("Failed to save SDK root checksum to file: " + checksumPath);
            Debug.LogException (ioe);
        }
    }

    [VerticalGroup (OdinGroup.Name.Mod)]
    [EnableIf (nameof(enableConfigUpdates))]
    [PropertyTooltip ("Update config DBs in all mods. New or change config entries from the SDK will be copied to the mod. This may take a while.")]
    [Button (ButtonSizes.Large)]
    public void UpdateAllFromSDK ()
    {
        if (!EditorUtility.DisplayDialog
        (
            "Update Mods from SDK",
            "Are you sure you'd like to update the config DBs in all your mods with the latest values from the SDK? This will take a long time. Proceed?",
            "Proceed",
            "Cancel"
        ))
        {
            return;
        }
        utilityCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless (UpdateAllModConfigsIE ());
    }

    [VerticalGroup (OdinGroup.Name.Mod)]
    [ShowIf (nameof(hasSelectedMod))]
    [EnableIf (nameof(enableRestore))]
    [GUIColor ("lightorange")]
    [PropertyTooltip ("Replaces all config DBs in mod with the SDK config DBs. This will also delete any ConfigOverrides you may have created.")]
    [Button ("Restore all configs from SDK", ButtonSizes.Large, Icon = SdfIconType.ExclamationTriangle, IconAlignment = IconAlignment.LeftOfText)]
    public void RestoreAllFromSDK ()
    {
        var modData = DataContainerModData.selectedMod;
        if (modData == null)
            return;
        
        var projectPath = modData.GetModPathProject ();
        if (!EditorUtility.DisplayDialog ("Restore Configs from SDK", $"Are you sure you'd like to restore all the config DBs in this project (ID {modData.id}) from the SDK? This will delete any ConfigOverrides you have created.\n\nProject folder: \n{projectPath}", "Confirm", "Cancel"))
        {
            return;
        }
        utilityCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless (RestoreAllIE ());
    }
    
    void OnEnable ()
    {
        if (!initialized)
        {
            initialized = true; 
            Initialize ();
        }
    }

    void Update ()
    {
        if (!initialized)
        {
            initialized = true;
            Initialize ();
        }
    }

    void Initialize ()
    {
        initialized = true;
        ins = this;

        linkerComponentLookup.Clear ();
        multiLinkerComponentLookup.Clear ();
        containerLookup.Clear ();

        var linkerType = typeof (DataLinker<>);
        var linkerComponents = GetChildrenInheritingOpenGenericType (gameObject, linkerType);
        foreach (var component in linkerComponents)
        {
            var type = component.GetType ();
            linkerComponentLookup[type.Name] = component;
        }

        var multilinkerType = typeof (DataMultiLinker<>);
        var multilinkerComponents = GetChildrenInheritingOpenGenericType (gameObject, multilinkerType);
        foreach (var component in multilinkerComponents)
        {
            var type = component.GetType ();
            multiLinkerComponentLookup[type.Name] = component;
            var dml = component as IDataMultiLinker;
            multiLinkerInterfaceLookup[type.Name] = dml;
            if (dml == null)
            {
                continue;
            }
            var containerType = ResolveContainerType (dml);
            containerLookup[containerType] = dml;
        }
    }

    IEnumerator UpdateAllModConfigsIE ()
    {
        yield return ModToolsHelper.UpdateAllModConfigsIE (checksumSDKConfigsRoot);
        OnUtilityCoroutineEnd ();
    }

    IEnumerator RestoreAllIE ()
    {
        yield return ModToolsHelper.DeleteModConfigsIE (configsPath);
        yield return CopyConfigsIE ();
        OnUtilityCoroutineEnd ();
    }

    static IEnumerator CopyConfigsIE ()
    {
        Directory.CreateDirectory (configsPath);
        yield return null;
        var root = new DirectoryInfo (DataPathHelper.GetApplicationFolder ());
        yield return ModToolsHelper.CopyConfigsIE (root, configsPath);
    }

    static Type ResolveContainerType (IDataMultiLinker dml)
    {
        var t = dml.GetType ();
        if (!t.IsConstructedGenericType)
        {
            t = t.BaseType;
        }
        return t.GetGenericArguments ()[0];
    }

    IEnumerator ChecksumSDKConfigsIE ()
    {
        var root = new DirectoryInfo (DataPathHelper.GetApplicationFolder ());
        yield return ModToolsHelper.ChecksumSDKConfigsIE (root);
        LoadSDKChecksums ();
        OnUtilityCoroutineEnd ();
    }

    bool enableRestore => IsUtilityOperationAvailable && hasSelectedMod && ModToolsHelper.HasChanges (DataContainerModData.selectedMod);
    bool enableSaveChecksum
    {
        get
        {
            var sdkRootDirectory = new DirectoryInfo (DataPathHelper.GetApplicationFolder ());
            return ConfigChecksums.ChecksumsExist (sdkRootDirectory);
        }
    }
    bool enableConfigUpdates => IsUtilityOperationAvailable && checksumsLoaded;

    static readonly Dictionary<string, Component> linkerComponentLookup = new Dictionary<string, Component> ();
    static readonly Dictionary<string, Component> multiLinkerComponentLookup = new Dictionary<string, Component> ();
    static readonly Dictionary<string, IDataMultiLinker> multiLinkerInterfaceLookup = new Dictionary<string, IDataMultiLinker> ();
    static readonly Dictionary<Type, IDataMultiLinker> containerLookup = new Dictionary<Type, IDataMultiLinker> ();

    static readonly object[] emptyParams = {};

    const string checksumSDKConfigsRootFileName = "checksumConfigsRoot.txt";
    #endif

    static class OdinGroup
    {
        public static class Name
        {
            public const string Mod = "Mod Buttons";
            public const string Standard = "Standard Buttons";
        }

        public static class Order
        {
            public const float UtilityCoroutine = -1f;
            public const float Standard = 0f;
            public const float Mod = 1f;
        }
    }
    #endif
}
