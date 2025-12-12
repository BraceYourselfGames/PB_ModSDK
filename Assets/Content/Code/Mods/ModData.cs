using System;
using System.Collections.Generic;
using System.Reflection;
using PhantomBrigade.Data;
using UnityEngine;
using Sirenix.OdinInspector;
using YamlDotNet.Serialization;

#if PB_MODSDK
using PhantomBrigade.SDK.ModTools;
#endif

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Internal;
#endif

namespace PhantomBrigade.Mods
{
    public enum ModVisibility
    {
        Public = 0,
        FriendsOnly = 1,
        Private = 2,
        Unlisted = 3
    }

    public class ModWorkshopData
    {
        [LabelText ("Published page ID")]
        [HorizontalGroup]
        [ReadOnly, LabelWidth (144f)]
        public string publishedID;

        public ModVisibility visibility = ModVisibility.Private;

        [LabelText ("Copy version text")]
        public bool textFromMetadataVersion = true;

        [LabelText ("Copy main text")]
        public bool textFromMetadataMain = true;

        [HideIf (nameof(textFromMetadataMain))]
        [LabelText ("Name / Desc.")]
        public string title;

        [HideIf (nameof(textFromMetadataMain))]
        [HideLabel, TextArea (1, 10)]
        public string description;

        [HideInInspector]
        // [FoldoutGroup ("Internal data", false)]
        // [HideLabel, TextArea (1, 10)]
        public string internalData;

        [TextArea (1, 10)]
        public string changes;

        #if PB_MODSDK && UNITY_EDITOR
        List<string> workshopTags => SteamWorkshopHelper.tags;
        [ValueDropdown ("@" + nameof(workshopTags))]
        #endif

        public HashSet<string> tags = new HashSet<string> ();

        [LabelText ("Upload preview")]
        public bool texPreviewUpload = true;

        // Optional texture preview. Automatically generated texture based on a PNG file
        // placed next to the mod folder under ModFiles/. Not hosted within mod folders
        // because this image should not be included into Steam Workshop mod content uploads
        [YamlIgnore, ShowIf (nameof(texPreviewUpload))]
        [OnInspectorGUI (Append = "@DrawTexturePreview (128)")] // Useful attr. for GUI injection
        [LabelText ("Preview")]
        public Texture2D texPreview;

        // This field tracks whether texture loading was already attempted before, to ensure
        // each redraw doesn't retrigger creation of a new texture
        [YamlIgnore]
        private bool texLoadAttempt;

        [YamlIgnore]
        public const string texFilename = "workshop_preview.png";

        #if UNITY_EDITOR

        [YamlIgnore, HideInInspector]
        public DataContainerModData parent;

        private bool IsPublished => !string.IsNullOrEmpty (publishedID) && publishedID.Length > 5;

        [HorizontalGroup (100f)]
        [Button ("Open URL", ButtonHeight = 21), ShowIf ("IsPublished")]
        private void OpenPublishedPage ()
        {
            if (!IsPublished)
                return;

            var url = "https://steamcommunity.com/sharedfiles/filedetails/?id=" + publishedID;
            Application.OpenURL (url);
        }

        public void DrawTexturePreview (int previewHeight)
        {
            if (texPreview == null)
            {
                if (!texLoadAttempt)
                    RefreshTexturePreview ();
            }

            bool texIsFallback = false;
            var texPreviewDisplayed = texPreview;
            if (texPreviewDisplayed == null)
            {
                texPreviewDisplayed = SteamWorkshopHelper.GetPreviewTexShared ();
                texIsFallback = true;
            }
            
            if (texPreviewDisplayed == null)
            {
                GUILayout.BeginHorizontal ();
                GUILayout.FlexibleSpace ();
                if (GUILayout.Button ("Reload", UnityEditor.EditorStyles.miniButton, GUILayout.Width (128f)))
                    RefreshTexturePreview ();

                GUILayout.EndHorizontal ();

                return;
            }

            previewHeight = Mathf.Clamp (previewHeight, 16, 512);
            var width = Mathf.Min (GUIHelper.ContextWidth - 88f - 15f, previewHeight);
            var shrink = width / (float)texPreviewDisplayed.width;
            var height = texPreviewDisplayed.height * shrink;
            
            var gc = GUI.color;
            var gcSecondary = GUI.color.WithAlpha (0.8f);
            
            using (var horizontalScope = new GUILayout.HorizontalScope ())
            {
                using (var verticalScope = new GUILayout.VerticalScope ("Box"))
                {
                    GUILayout.Space (2f);
                    
                    GUILayout.BeginHorizontal ();
                    
                    // Start of 1st column
                    GUILayout.BeginVertical ();
                    
                    GUILayout.BeginHorizontal ();
                    GUILayout.FlexibleSpace ();
                    if (GUILayout.Button ("Open folder", EditorStyles.miniButton, GUILayout.Width (width)))
                    {
                        if (parent != null)
                        {
                            var pathMod = parent.GetModPathProject ();
                            Application.OpenURL ("file://" + pathMod);
                        }
                    }
                    GUILayout.EndHorizontal ();

                    if (!texIsFallback)
                    {
                        GUI.color = gcSecondary;
                        hueOverride = EditorGUILayout.Slider ("", hueOverride, 0f, 1f);
                        GUI.color = gc;
                    }
                    
                    // End of 2nd column
                    GUILayout.EndVertical ();
                    
                    // Start of 2nd column
                    GUILayout.BeginVertical ();
                    
                    if (texIsFallback)
                    {
                        if (GUILayout.Button ("Create custom", EditorStyles.miniButton, GUILayout.Width (width)))
                            MakeUniquePreview ();
                    }
                    else
                    {
                        if (GUILayout.Button ("Save", EditorStyles.miniButton, GUILayout.Width (width)))
                            SavePreviewToFile ();
                        
                        GUI.color = gcSecondary;
                        if (GUILayout.Button ("Set hue", EditorStyles.miniButton, GUILayout.Width (width)))
                            OverrideHue ();
                        GUI.color = gc;
                        
                        GUILayout.Label ("Overriding the hue can\nbe a quick way to make\na custom thumbnail", EditorStyles.centeredGreyMiniLabel, GUILayout.Width (width));
                    }
                    
                    // End of 2nd column
                    GUILayout.EndVertical ();
                    
                    // Start of 3rd column
                    GUILayout.BeginVertical ();
                    
                    if (GUILayout.Button ("Reload", EditorStyles.miniButton))
                        RefreshTexturePreview ();
                    
                    // The label drawing is the easiest way to display textures, but it's distorted by Editor GUI and won't show true color
                    GUILayout.Label (texPreviewDisplayed, GUILayout.Width (width), GUILayout.Height (height));
                    
                    // We can steal the rect from that easy method and draw a raw texture instead
                    var rect = GUILayoutUtility.GetLastRect ();
                    EditorGUI.DrawPreviewTexture(rect, texPreviewDisplayed);
                    
                    if (texIsFallback)
                        GUILayout.Label ("Default image", EditorStyles.centeredGreyMiniLabel);
                    else
                        GUILayout.Label (texFilename, EditorStyles.centeredGreyMiniLabel);
                    
                    // End of 3rd column
                    GUILayout.EndVertical ();
                    
                    GUILayout.EndHorizontal ();
                    GUILayout.Space (2f);
                }
            }
        }
        
        private void RefreshTexturePreview ()
        {
            texLoadAttempt = true;
            texPreview = null;

            if (parent == null)
                return;

            var projectPath = parent.GetModPathProject ();
            if (string.IsNullOrEmpty (projectPath))
                return;

            var filePath = DataPathHelper.GetCombinedCleanPath (projectPath, texFilename);
            var fileExists = File.Exists (filePath);
            if (!fileExists)
                return;

            var pngBytes = File.ReadAllBytes (filePath);
            texPreview = new Texture2D (4, 4, TextureFormat.RGBA32, true, false)
            {
                name = texFilename,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                anisoLevel = 2,
            };

            texPreview.LoadImage (pngBytes);
        }

        private bool IsTexPreviewAvailable ()
        {
            if (texPreview == null)
            {
                Debug.LogWarning ("Preview texture not loaded");
                return false;
            }
            
            if (!texPreview.isReadable)
            {
                Debug.LogWarning ("Preview texture not readable");
                return false;
            }

            return true;
        }

        private void MakeUniquePreview ()
        {
            texPreview = SteamWorkshopHelper.GetPreviewTexUnique ();
            if (texPreview == null)
            {
                Debug.LogWarning ("Couldn't create a unique preview");
                return;
            }
            
            SavePreviewToFile ();
        }

        private void SavePreviewToFile ()
        {
            if (!IsTexPreviewAvailable ())
                return;

            var modPath = parent != null ? parent.GetModPathProject () : null;
            if (string.IsNullOrEmpty (modPath) || !Directory.Exists (modPath))
            {
                Debug.LogWarning ($"Failed to find mod project directory {modPath}, can't save the preview");
                return;
            }
            
            var texPath = DataPathHelper.GetCombinedCleanPath (modPath, ModWorkshopData.texFilename);
            var png = texPreview.EncodeToPNG ();
            File.WriteAllBytes (texPath, png);
            Debug.Log ($"Saved preview image to file: \n{texPath}");
        }

        private float hueOverride = 0f;
        
        private void OverrideHue ()
        {
            if (!IsTexPreviewAvailable ())
                return;

            hueOverride = Mathf.Clamp01 (hueOverride);
            
            // Crude path, sets everything to one hue
            var pixels = texPreview.GetPixels ();
            for (int i = 0; i < pixels.Length; i++)
            {
                var color = pixels[i];
                var colorHsb = new HSBColor (color);
                colorHsb.h = hueOverride;
                pixels[i] = colorHsb.ToColor ();
            }

            texPreview.SetPixels (pixels);
            texPreview.Apply (true, false);
        }

        #endif
    }

    [HideReferenceObjectPicker]
    #if PB_MODSDK
    [LabelWidth (156f)]
    #else
    [LabelWidth (200f)]
    #endif
    public class ModMetadata
    {
        [YamlIgnore, ReadOnly, HideInInspector]
        public string directory;

        [YamlIgnore, ReadOnly, HideInInspector]
        public string path;

        [PropertyOrder (-2)]
        public int priority;

        [ShowInInspector, HorizontalGroup]
        [PropertyRange (0f, 1f)]
        public float colorHue = 0.5f;

        [ShowInInspector, HorizontalGroup (50f), HideLabel]
        public Color color => Color.HSVToRGB (colorHue, 1f, 1f);

        #if PB_MODSDK
        [HideInInspector]
        #endif
        [LabelText ("ID"), ReadOnly]
        public string id;

        [LabelText ("Version")]
        public string ver;

        [LabelText ("Web page URL")]
        public string url;

        #if PB_MODSDK && UNITY_EDITOR
        [EnableIf (nameof(isConfigEnabled))]
        #endif
        public bool includesConfigOverrides;

        [ReadOnly, PropertyTooltip ("Add config edits via the bottom right component menu to enable this flag")]
        public bool includesConfigEdits;

        [HideInInspector] // Currently unused
        public bool includesConfigTrees;

        [ReadOnly, PropertyTooltip ("Add library DLLs via the bottom right component menu to enable this flag")]
        public bool includesLibraries;

        [ReadOnly, PropertyTooltip ("Add textures into the local Textures folder under the mod project folder to enable this flag")]
        public bool includesTextures;
        
        [ReadOnly, PropertyTooltip ("Add text edits via the bottom right component menu to enable this flag")]
        public bool includesLocalizationEdits;
        
        public bool includesLocalizations;

        [ReadOnly, PropertyTooltip ("Add asset bundles via the bottom right component menu to enable this flag")]
        public bool includesAssetBundles;

        [InfoBox ("This mod might not load in PB 2.0 if you leave this field empty or do not enter \"2.0\" into it.", InfoMessageType.Warning, VisibleIf = "IsVersionWarningVisible")]
        public string gameVersionMin;

        [YamlIgnore, ReadOnly, HideInInspector]
        public Version gameVersionMinPacked;

        public string gameVersionMax;

        [YamlIgnore, ReadOnly, HideInInspector]
        public Version gameVersionMaxPacked;

        [LabelText ("Name / Desc.")]
        public string name;

        [HideLabel, TextArea (1, 10)]
        public string desc;

        #if PB_MODSDK
        [YamlIgnore, HideInInspector]
        public bool isConfigEnabled;
        #endif

        private bool IsVersionWarningVisible ()
        {
            return string.IsNullOrEmpty (gameVersionMin) || !gameVersionMin.StartsWith ("2.");
        }

        public void OnAfterDeserialization (string directory, string path)
        {
            this.directory = directory;
            this.path = path;

            if (!string.IsNullOrEmpty (gameVersionMin))
            {
                bool versionMinParsed = ModManager.TryParseVersionString (gameVersionMin, out int major, out int minor, out int patch, false);
                if (versionMinParsed)
                    gameVersionMinPacked = new Version (major, minor, patch);
                else
                    Debug.LogWarning ($"Mod {id} | Failed to parse part of metadata: min game version string {gameVersionMin}");
            }

            if (!string.IsNullOrEmpty (gameVersionMax))
            {
                bool versionMaxParsed = ModManager.TryParseVersionString (gameVersionMax, out int major, out int minor, out int patch, true);
                if (versionMaxParsed)
                    gameVersionMaxPacked = new Version (major, minor, patch);
                else
                    Debug.LogWarning ($"Mod {id} | Failed to parse part of metadata: max game version string {gameVersionMax}");
            }
        }
    }

    [HideReferenceObjectPicker]
    public class ModMetadataPreloaded
    {
        public string source;
        public string directoryName;
        public string pathFull;
        public ModMetadata metadata;
    }

    [HideReferenceObjectPicker]
    public class ModLoadedData
    {
        public ModMetadata metadata;

        [ShowIf (nameof(showConfigOverrides))]
        public List<ModConfigOverride> configOverrides;

        [ShowIf (nameof(showConfigEdits))]
        public List<ModConfigEditLoaded> configEdits;

        [ShowIf (nameof(showConfigTrees))]
        public List<ModConfigTreeLoaded> configTrees;

        [ShowIf (nameof(showLocalizationEdits))]
        public Dictionary<string, List<ModLocalizationEditLoaded>> localizationEdits;

        [ShowIf (nameof(showTextures))]
        public List<ModTextureFolder> textureFolders;

        [HideInInspector]
        public Dictionary<string, ModLevelLoaded> levels;

        [ShowIf (nameof(showAssetBundles))]
        public List<AssetBundle> assetBundles;

        [HideInInspector]
        public List<Assembly> assemblies;

        [ShowIf (nameof(showLibraries))]
        public List<string> assemblyNames;

        [HideInInspector]
        public List<MethodBase> patchedMethods;

        [ShowIf (nameof(showLibraries))]
        public List<string> patchedMethodNames;

        private bool showConfigOverrides => metadata != null && metadata.includesConfigOverrides;
        private bool showConfigEdits => metadata != null && metadata.includesConfigEdits;
        private bool showConfigTrees => metadata != null && metadata.includesConfigTrees;
        private bool showLocalizationEdits => metadata != null && metadata.includesLocalizationEdits;
        private bool showLibraries => metadata != null && metadata.includesLibraries;
        private bool showTextures => metadata != null && metadata.includesTextures;
        private bool showAssetBundles => metadata != null && metadata.includesAssetBundles;

        public UnityEngine.Object GetAsset (string assetBundleName, string assetName, bool log = true)
        {
            string modID = metadata != null ? metadata.id : "?";

            if (assetBundles == null)
            {
                if (log)
                    DebugHelper.LogWarning ($"GetAsset failed: mod {modID} has no asset bundles");
                return null;
            }

            AssetBundle assetBundle = null;
            foreach (var assetBundleCandidate in assetBundles)
            {
                if (!string.Equals (assetBundleCandidate.name, assetBundleName))
                    continue;

                assetBundle = assetBundleCandidate;
                break;
            }

            if (assetBundle == null)
            {
                if (log)
                    DebugHelper.LogWarning ($"GetAsset failed: mod {modID} has no asset bundle named {assetBundleName}");
                return null;
            }

            var asset = assetBundle.LoadAsset (assetName);
            if (asset == null)
            {
                if (log)
                    DebugHelper.LogWarning ($"GetAsset failed: mod {modID} has no asset named {assetName} in asset bundle {assetBundleName} | Existing assets:\n{assetBundle.GetAllAssetNames ().ToStringFormatted (true, multilinePrefix: "- ")}");
                return null;
            }

            if (log)
                DebugHelper.Log ($"GetAsset success: mod {modID} has asset named {assetName} in asset bundle {assetBundleName} | Type: {asset.GetType ().Name}");
            return asset;
        }
    }

    [HideReferenceObjectPicker]
    public class ModConfig
    {
        // public List<ModConfigEntry> list = new List<ModConfigEntry> ();
        public List<ModConfigEntry> overrides;
    }

    [HideReferenceObjectPicker]
    public class ModConfigEntry
    {
        [ValueDropdown ("@ModManager.metadataLookup.Keys")]
        [HideLabel, HorizontalGroup]
        public string id;

        [HideLabel, HorizontalGroup, ToggleLeft]
        public bool enabled;
    }

    [HideReferenceObjectPicker]
    public class ModConfigEditLoaded
    {
        public string pathFull;
        public string pathTrimmed;
        public string filename;
        public string key;
        public string typeName;
        public Type type;
        public ModConfigEdit data;
    }

    [HideReferenceObjectPicker]
    public class ModLocalizationEditLoaded
    {
        public string pathFull;
        public string pathTrimmed;
        public string filename;
        public string key;
        public ModLocalizationEdit data;
    }

    [HideReferenceObjectPicker]
    public class  ModLevelLoaded
    {
        public string pathCombined;
    }

    [HideReferenceObjectPicker]
    public class  ModTextureFolder
    {
        public string pathFull;
        public string pathTrimmed;
    }

    [HideReferenceObjectPicker]
    public class ModConfigEditSerialized
    {
        public bool removed;

        [HideIf ("removed")]
        public List<string> edits;
    }

    [HideReferenceObjectPicker]
    public class ModConfigEdit
    {
        public bool removed;

        [HideIf ("removed")]
        [TableList]
        public List<ModConfigEditStep> edits;
    }

    [HideReferenceObjectPicker]
    public class ModConfigEditStep
    {
        [HideLabel]
        public string path;

        [HideLabel]
        public string value;
    }

    [HideReferenceObjectPicker]
    public class ModLocalizationEdit
    {
        [TableList]
        public SortedDictionary<string, string> edits;
    }

    public class ModConfigOverride
    {
        public string pathFull;
        public string pathTrimmed;
        public string filename;
        public string key;
        public string typeName;
        public Type type;

        [FoldoutGroup ("Data", false), HideLabel, HideReferenceObjectPicker]
        public object containerObject;
    }

    [HideReferenceObjectPicker]
    public class ModConfigTreeLoaded
    {
        public string pathFull;
        public string pathTrimmed;
        public string filename;
        public string key;
        public string typeName;
        public Type type;
        public ModConfigTree data;
    }

    [HideReferenceObjectPicker]
    public class ModConfigTree
    {
        public string path;
        public string action;
        public string value;
        public List<string> children;
    }

    [HideReferenceObjectPicker]
    public class ModConfigTreeSelector
    {
        public string key;
    }

    [HideReferenceObjectPicker]
    public class ModConfigTreeEdit
    {
        public string key;
    }
}
