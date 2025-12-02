using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PhantomBrigade.Data.UI;
using PhantomBrigade.Mods;
using QFSW.QC;
using UnityEngine;

namespace PhantomBrigade.Data
{
    public class TextureGroupAtlas
    {
        public bool uiAtlasMerge = false;
        public int padding = 2;
        public bool forceSquare = true;
        public Texture2D textureCombined;
        public Texture2D[] texturesPacked;
        public Rect[] rectsPacked;
    }

    #if !PB_MODSDK
    public class UIAtlasSpriteLink
    {
        public Texture2D texture;
        public UISpriteData spriteData;
    }
    #endif

    public class TextureGroup
    {
        public TextureGroupAtlas atlas;
        public string folderName = string.Empty;
        public Vector2Int requiredDimensions = new Vector2Int (0, 0);
        public int requiredFilesize = 0;
        public bool mipLevels = false;
        public bool opacityChecked = false;
        
        public bool blurUsed = false;
        public float blurDownscale = 1f;
        public int blurSteps = 8;

        public HashSet<string> textureKeysOpaque;
        public List<string> textureKeysInternal;
        public List<string> textureKeysExposed = new List<string> ();
        public SortedDictionary<string, Texture2D> textures = new SortedDictionary<string, Texture2D> ();
    }

    public static class TextureGroupKeys
    {
        public const string Codex = "codex";
        public const string CombatComms = "combatComms";
        public const string CombatAreas = "combatAreas";
        public const string PilotPortraits = "pilotPortraits";
        public const string OverworldLandscapes = "overworldLandscapes";
        public const string OverworldEvents = "overworldEvents";
        public const string OverworldEventsBlur2X = "overworldEventsBlur2X";
        public const string OverworldEventsBlur4X = "overworldEventsBlur4X";
        public const string OverworldEntities = "overworldEntities";
        public const string Sprites = "sprites";
    }

    [CommandPrefix ("tex.")]
    public static class TextureManager
    {
        private static bool loaded = false;
        private static bool loadedAnyContent = false;
        private static bool loadedDuringPlay = false;
        private static bool log = false;
        private static int blurStepLimit = 16;
        
        #if !PB_MODSDK
        public  static bool uiAtlasModified;
        private static Texture2D uiAtlasTextureOriginal;
        private static SortedDictionary<string, UIAtlasSpriteLink> uiAtlasSpriteLinksOriginal;
        private static SortedDictionary<string, UIAtlasSpriteLink> uiAtlasSpriteLinks;
        private static List<UISpriteData> spriteListCopy = new List<UISpriteData> ();
        #endif
        
        private static List<string> textureKeysBuffer = new List<string> ();
        
        public static SortedDictionary<string, string> groupKeysFromFolders = new SortedDictionary<string, string> ();

        public static SortedDictionary<string, TextureGroup> groups = new SortedDictionary<string, TextureGroup>
        {
            {
                TextureGroupKeys.CombatComms,
                new TextureGroup
                {
                    folderName = "UI/CombatComms",
                    opacityChecked = false,
                    requiredDimensions = new Vector2Int (128, 128),
                }
            },
            {
                TextureGroupKeys.CombatAreas,
                new TextureGroup
                {
                    folderName = "UI/CombatAreas",
                    opacityChecked = false,
                    requiredDimensions = new Vector2Int (512, 128)
                }
            },
            {
                TextureGroupKeys.PilotPortraits,
                new TextureGroup
                {
                    folderName = "UI/PilotPortraits",
                    opacityChecked = true,
                    requiredDimensions = new Vector2Int (256, 256),
                    textureKeysInternal = new List<string>
                    {
                        "pilot_missing",
                        "pilot_deceased",
                        "pilot_concussed",
                        "pilot_int_fallback_friendly",
                        "pilot_int_fallback_enemy",
                        "pilot_int_tutorial_lt",
                        "pilot_int_tutorial_cadet",
                        "pilot_int_tutorial_commander",
                        "pilot_int_enemy_commander",
                        "pilot_int_enemy_elite"
                    }
                }
            },
            {
                TextureGroupKeys.Codex,
                new TextureGroup
                {
                    folderName = "UI/Codex"
                }
            },
            {
                TextureGroupKeys.OverworldLandscapes,
                new TextureGroup
                {
                    folderName = "UI/OverworldLandscapes",
                    requiredDimensions = new Vector2Int (512, 256)
                }
            },
            {
                TextureGroupKeys.OverworldEvents,
                new TextureGroup
                {
                    folderName = "UI/OverworldEvents",
                    requiredDimensions = new Vector2Int (1024, 512)
                }
            },
            {
                TextureGroupKeys.OverworldEventsBlur2X,
                new TextureGroup
                {
                    folderName = "UI/OverworldEvents",
                    requiredDimensions = new Vector2Int (1024, 512),
                    blurUsed = true,
                    blurDownscale = 0.25f,
                    blurSteps = 2
                }
            },
            {
                TextureGroupKeys.OverworldEventsBlur4X,
                new TextureGroup
                {
                    folderName = "UI/OverworldEvents",
                    requiredDimensions = new Vector2Int (1024, 512),
                    blurUsed = true,
                    blurDownscale = 0.125f,
                    blurSteps = 4
                }
            },
            {
                TextureGroupKeys.OverworldEntities,
                new TextureGroup
                {
                    folderName = "UI/OverworldEntities",
                    requiredDimensions = new Vector2Int (512, 256)
                }
            },
            {
                TextureGroupKeys.Sprites,
                new TextureGroup
                {
                    folderName = "UI/Sprites",
                    atlas = new TextureGroupAtlas
                    {
                        uiAtlasMerge = true
                    }
                }
            }
        };

        public static void LoadGroup (string groupKey)
        {
            if (string.IsNullOrEmpty (groupKey) || !groups.ContainsKey (groupKey))
                return;

            var group = groups[groupKey];

            if (group.textures != null)
            {
                var keys = group.textures.Keys.ToList ();
                foreach (var key in keys)
                {
                    var tex = group.textures[key];
                    if (tex != null)
                    {
                        if (Application.isPlaying)
                            GameObject.Destroy (tex);
                        else
                            GameObject.DestroyImmediate (tex);
                    }
                }
            }

            // Build lookup
            if (!groupKeysFromFolders.ContainsKey (group.folderName))
                groupKeysFromFolders.Add (group.folderName, groupKey);
            else
                Debug.LogWarning ($"Group {groupKey} is using path {group.folderName} already used by another group {groupKeysFromFolders[group.folderName]}");

            // Clear collections
            if (group.textures == null)
                group.textures = new SortedDictionary<string, Texture2D> ();
            else
                group.textures.Clear ();

            if (group.textureKeysExposed == null)
                group.textureKeysExposed = new List<string> ();
            else
                group.textureKeysExposed.Clear ();

            // Load special internal textures so that we don't have to implement special support for them in UI code
            if (group.textureKeysInternal != null)
            {
                foreach (var key in group.textureKeysInternal)
                {
                    if (group.textures.ContainsKey (key))
                        continue;

                    var texture = DataLinkerUI.GetTexture (key);
                    if (texture == null)
                        continue;

                    // Do not register these textures to exposed keys
                    group.textures.Add (key, texture);
                }
            }

            // Load standard set
            var folderPath = $"{Application.streamingAssetsPath}/{group.folderName}/";
            LoadTexturesFrom (group, folderPath);
            
            /*
            // Allow mods to inject additional textures (if group with same path exists)
            if (Application.isPlaying && ModManager.AreModsActive ())
            {
                foreach (var modLoadedData in ModManager.loadedMods)
                {
                    if (modLoadedData.textureFolders == null || !modLoadedData.metadata.includesTextures)
                        continue;

                    foreach (var textureFolder in modLoadedData.textureFolders)
                    {
                        var folderName = textureFolder.pathTrimmed;
                        if (!groupKeysFromFolders.ContainsKey (folderName))
                        {
                            Debug.LogWarning ($"Mod {modLoadedData.metadata.id} is using unrecognized texture folder {folderName}, skipping it");
                            continue;
                        }

                        var groupKeyMatched = groupKeysFromFolders[folderName];
                        var groupMatched = groups[groupKeyMatched];
                        var folderPathMatched = textureFolder.pathFull;
                        LoadTexturesFrom (group, folderPathMatched);
                    }
                }
            }
            */
        }
        
        public static void CheckDatabase ()
        {
            if (!loaded)
                Load ();
        }
        
        public static bool AreAssetsPresent ()
        {
            CheckDatabase ();
            return loadedAnyContent;
        }

        [Command ("load", "For internal use when debugging the texture management system. Force reload of the texture manager system. The game will process files from StreamingAssets folder.")]
        public static void Load ()
        {
            loaded = true;
            loadedDuringPlay = Application.isPlaying;
            loadedAnyContent = false;

            // Clear lookup
            groupKeysFromFolders.Clear ();
            
            bool folderExists = Directory.Exists (Application.streamingAssetsPath);
            if (!folderExists)
                return;

            // Iterate over groups
            foreach (var kvp in groups)
            {
                var groupKey = kvp.Key;
                var group = kvp.Value;

                // Build lookup
                if (!groupKeysFromFolders.ContainsKey (group.folderName))
                    groupKeysFromFolders.Add (group.folderName, groupKey);
                else
                    Debug.LogWarning ($"Group {groupKey} is using path {group.folderName} already used by another group {groupKeysFromFolders[group.folderName]}");

                // Clear collections
                if (group.textures == null)
                    group.textures = new SortedDictionary<string, Texture2D> ();
                else
                    group.textures.Clear ();

                if (group.textureKeysExposed == null)
                    group.textureKeysExposed = new List<string> ();
                else
                    group.textureKeysExposed.Clear ();

                // Load special internal textures so that we don't have to implement special support for them in UI code
                if (group.textureKeysInternal != null)
                {
                    foreach (var key in group.textureKeysInternal)
                    {
                        if (group.textures.ContainsKey (key))
                            continue;

                        var texture = DataLinkerUI.GetTexture (key);
                        if (texture == null)
                            continue;

                        // Do not register these textures to exposed keys
                        group.textures.Add (key, texture);
                    }
                }

                // Load standard set
                var folderPath = $"{Application.streamingAssetsPath}/{group.folderName}/";
                LoadTexturesFrom (group, folderPath);
                
                if (!loadedAnyContent && group.textures.Count > 0)
                    loadedAnyContent = true;
            }

            // Allow mods to inject additional textures (if group with same path exists)
            if (Application.isPlaying && ModManager.AreModsActive ())
            {
                foreach (var modLoadedData in ModManager.loadedMods)
                {
                    if (modLoadedData.textureFolders == null || !modLoadedData.metadata.includesTextures)
                        continue;

                    foreach (var textureFolder in modLoadedData.textureFolders)
                    {
                        var folderName = textureFolder.pathTrimmed;
                        if (!groupKeysFromFolders.ContainsKey (folderName))
                        {
                            Debug.LogWarning ($"Mod {modLoadedData.metadata.id} is using unrecognized texture folder {folderName}, skipping it");
                            continue;
                        }

                        var groupKey = groupKeysFromFolders[folderName];
                        var group = groups[groupKey];

                        var folderPath = textureFolder.pathFull;
                        LoadTexturesFrom (group, folderPath);
                    }
                }
            }

            // Sort exposed keys
            foreach (var kvp in groups)
            {
                var group = kvp.Value;
                group.textureKeysExposed.Sort ();
            }

            foreach (var kvp in groups)
            {
                var group = kvp.Value;
                if (group.atlas != null)
                    PackTexturesToAtlas (group);
            }
        }

        public static void LoadTexturesFrom (TextureGroup group, string folderPath)
        {
            try
            {
                var filePaths = System.IO.Directory.GetFiles (folderPath, "*.png");
                for (int i = 0; i < filePaths.Length; ++i)
                {
                    var filePath = filePaths[i];
                    try
                    {
                        var fileInfo = new FileInfo (filePath);
                        var key = System.IO.Path.GetFileNameWithoutExtension (filePath);
                        var size = fileInfo.Length;

                        if (group.requiredFilesize > 0 && size > group.requiredFilesize)
                        {
                            Debug.LogWarning ($"Skipping loading of texture {key} due to file size {size} exceeding limit of {group.requiredFilesize}");
                            continue;
                        }

                        byte[] pngBytes = System.IO.File.ReadAllBytes (filePath);
                        var tex = new Texture2D (4, 4, TextureFormat.BC7, group.mipLevels, false);
                        tex.ignoreMipmapLimit = true;
                        tex.name = Path.GetFileNameWithoutExtension (fileInfo.Name);
                        tex.wrapMode = TextureWrapMode.Clamp;
                        tex.filterMode = FilterMode.Bilinear;
                        tex.anisoLevel = 2;
                        tex.LoadImage (pngBytes);

                        bool dimensionsChecked = group.requiredDimensions.x != 0 && group.requiredDimensions.y != 0;
                        if (dimensionsChecked)
                        {
                            if (group.requiredDimensions.x > 0 && tex.width != group.requiredDimensions.x)
                            {
                                Debug.LogWarning ($"Skipping loading of texture {key} due to width {tex.width} not matching required width {group.requiredDimensions.x}");
                                continue;
                            }

                            if (group.requiredDimensions.y > 0 && tex.height != group.requiredDimensions.y)
                            {
                                Debug.LogWarning ($"Skipping loading of texture {key} due to height {tex.height} not matching required width {group.requiredDimensions.y}");
                                continue;
                            }
                        }

                        if (!group.textures.ContainsKey (key))
                        {
                            group.textures.Add (key, tex);
                            group.textureKeysExposed.Add (key);
                            if (log)
                                Debug.Log ($"Loaded new texture from path: {filePath}");

                            if (group.opacityChecked)
                            {
                                var pixels = tex.GetPixels32 ();
                                bool alphaPresent = false;
                                var alphaFull = (byte)255;
                                int length = tex.width * tex.height;
                                for (int p = 0; p < length; ++p)
                                {
                                    var pixelColor = pixels[p];
                                    if (pixelColor.a != alphaFull)
                                    {
                                        alphaPresent = true;
                                        break;
                                    }
                                }

                                if (!alphaPresent)
                                {
                                    if (group.textureKeysOpaque == null)
                                        group.textureKeysOpaque = new HashSet<string> { key };
                                    else if (!group.textureKeysOpaque.Contains (key))
                                        group.textureKeysOpaque.Add (key);
                                    
                                    if (log)
                                        Debug.Log ($"Texture is opaque: {filePath}");
                                }
                            }
                        }
                        else
                        {
                            group.textures[key] = tex;
                            if (log)
                                Debug.Log ($"Overriding texture {key} using path: {filePath}");
                        }
                    }
                    catch (Exception e2)
                    {
                        Debug.LogWarning ($"Failed to read a file at path: {filePath} | Exception: {e2}");
                    }
                }

                if (group.blurUsed)
                {
                    textureKeysBuffer.Clear ();
                    textureKeysBuffer.AddRange (group.textures.Keys);
                    
                    foreach (var texKey in textureKeysBuffer)
                    {
                        var texSource = group.textures[texKey];
                        #if !PB_MODSDK
                        var width = Mathf.CeilToInt (Mathf.Clamp (texSource.width * group.blurDownscale, 4, texSource.width));
                        var height = Mathf.CeilToInt (Mathf.Clamp (texSource.height * group.blurDownscale, 4, texSource.height));
                        var texBlur = UtilityTextureOperations.CreateCopyAtSize (texSource, width, height, group.mipLevels);
                        UtilityTextureOperations.ApplyGaussianBlur (ref texBlur, Mathf.Clamp (group.blurSteps, 1, blurStepLimit));
                        group.textures[texKey] = texBlur;
                        #else
                        group.textures[texKey] = texSource;
                        #endif
                    }
                }
            }
            catch (Exception e1)
            {
                Debug.LogWarning ($"Failed to fetch a list of files at path: {folderPath} | Exception: {e1}");
            }

            if (log)
                Debug.Log ($"Loaded textures from path: {folderPath} | Discovered {group.textures.Count} files");
        }

        private static List<string> listFallback = new List<string> ();

        public static List<string> GetExposedTextureKeys (string groupKey)
        {
            if (!loaded)
                Load ();
            
            // No point logging missing textures if not a single texture is in memory - it's not an issue with a particular group/key then
            if (!loadedAnyContent)
                return null;

            var group = !string.IsNullOrEmpty (groupKey) && groups.ContainsKey (groupKey) ? groups[groupKey] : null;
            if (group == null || group.textureKeysExposed == null)
            {
                Debug.LogWarning ($"Texture manager failed to find group {groupKey} or it had no exposed keys to return");
                return listFallback;
            }

            return group.textureKeysExposed;
        }

        public static Texture2D GetTexture (string groupKey, string textureKey, bool log = true)
        {
            if (!loaded)
                Load ();
            #if UNITY_EDITOR
            else if (loadedDuringPlay != Application.isPlaying)
                Load ();
            #endif
            
            // No point logging missing textures if not a single texture is in memory - it's not an issue with a particular group/key then
            if (!loadedAnyContent)
                return null;

            var group = !string.IsNullOrEmpty (groupKey) && groups.TryGetValue (groupKey, out var group1) ? group1 : null;
            if (group == null || group.textures == null)
            {
                if (log)
                    Debug.LogWarning ($"Texture manager failed to find group {groupKey} or it had no textures to return");
                return null;
            }
            
            var texture = !string.IsNullOrEmpty (textureKey) && group.textures.TryGetValue (textureKey, out var groupTexture) ? groupTexture : null;
            if (texture == null)
            {
                if (log)
                    Debug.LogWarning ($"Texture manager failed to find texture {textureKey} in group {groupKey}");
            }

            return texture;
        }
        
        public static Texture2D GetTextureAndOpacity (string groupKey, string textureKey, out bool opaque)
        {
            opaque = false;
            if (!loaded)
                Load ();

            var group = !string.IsNullOrEmpty (groupKey) && groups.TryGetValue (groupKey, out var group1) ? group1 : null;
            if (group == null || group.textures == null)
            {
                Debug.LogWarning ($"Texture manager failed to find group {groupKey} or it had no textures to return");
                return null;
            }

            var texture = !string.IsNullOrEmpty (textureKey) && group.textures.TryGetValue (textureKey, out var groupTexture) ? groupTexture : null;
            if (texture == null)
                Debug.LogWarning ($"Texture manager failed to find texture {textureKey} in group {groupKey}");

            opaque = group.textureKeysOpaque != null && group.textureKeysOpaque.Contains (textureKey);
            return texture;
        }

        private static void PackTexturesToAtlas (TextureGroup group)
        {
            // var atlas = VPHelper.atlasMain;
            // if (atlas == null)
            //     return;

            if (group.atlas == null)
                return;

            if (group.textures == null || group.textures.Count == 0)
                return;

            if (group.atlas.textureCombined == null)
                group.atlas.textureCombined = new Texture2D (1, 1, TextureFormat.ARGB32, false);

            bool uiAtlasMerge = group.atlas.uiAtlasMerge;
            if (uiAtlasMerge)
            {
                #if !PB_MODSDK
                RebuildUIAtlas (group);
                #endif
            }
            else
            {
                var texturesPackedList = new List<Texture2D> ();

                // Directly drop loaded textures to packed list
                foreach (var kvp in group.textures)
                {
                    var textureLoaded = kvp.Value;
                    texturesPackedList.Add (textureLoaded);
                }

                group.atlas.texturesPacked = texturesPackedList.ToArray ();

                int maxSize = SystemInfo.maxTextureSize;
                group.atlas.rectsPacked = UITexturePacker.PackTextures
                (
                    group.atlas.textureCombined,
                    group.atlas.texturesPacked,
                    4,
                    4,
                    group.atlas.padding,
                    maxSize,
                    group.atlas.forceSquare
                );
            }
        }

        #if !PB_MODSDK
        
        [Command ("print-groups")]
        public static void PrintGroups (string filter = null)
        {
            var sb = new StringBuilder ();
            sb.Append ("Texture groups:");

            foreach (var kvp in groups)
            {
                var groupKey = kvp.Key;

                if (!string.IsNullOrEmpty (filter) && !groupKey.Contains (filter))
                    continue;

                var group = kvp.Value;

                sb.Append ("\n");
                sb.Append (groupKey);

                sb.Append ("\n- Folder name: ");
                sb.Append (group.folderName);

                sb.Append ("\n- Expected resolution: ");
                sb.Append (group.requiredDimensions == Vector2Int.zero ? "no restrictions" : group.requiredDimensions.ToString ());

                sb.Append ("\n- Maximum filesize: ");
                sb.Append (group.requiredFilesize <= 0f ? "no restrictions" : $"{group.requiredFilesize} bytes");

                sb.Append ("\n- Loaded textures ");
                if (group.textures != null && group.textures.Count > 0)
                {
                    sb.Append (" (");
                    sb.Append (group.textures.Count);
                    sb.Append ("): ");

                    foreach (var kvp2 in group.textures)
                    {
                        var key = kvp2.Key;
                        var tex = kvp2.Value;

                        sb.Append ("\n  - ");
                        sb.Append (key);
                        sb.Append (" (");
                        sb.Append (tex.format);
                        sb.Append (", ");
                        sb.Append (tex.width);
                        sb.Append ("x");
                        sb.Append (tex.height);
                        sb.Append (")");
                    }
                }
                else
                    sb.Append (": -");

                if (group.atlas != null)
                {
                    var a = group.atlas;
                    sb.Append ("\n- Texture atlas used: ");

                    sb.Append ("\n  - ");
                    sb.Append (a.uiAtlasMerge ? "Merged to UI" : "Standalone texture");
                    sb.Append ("\n  - ");
                    sb.Append (a.forceSquare ? "Forced square" : "No aspect restrictions");
                    sb.Append ("\n  - Padding: ");
                    sb.Append (a.padding);

                    sb.Append ("\n  - Final texture: ");
                    if (a.textureCombined != null)
                    {
                        var tex = a.textureCombined;

                        sb.Append (tex.format);
                        sb.Append (", ");
                        sb.Append (tex.width);
                        sb.Append ("x");
                        sb.Append (tex.height);
                    }
                    else
                        sb.Append ("-");

                    sb.Append ("\n  - Lookup: ");
                    if (a.texturesPacked != null && a.texturesPacked.Length > 0)
                        sb.Append (a.texturesPacked.Length);
                    else
                        sb.Append ("-");
                }

                sb.Append ("\n");
            }

            SubmitBuilderToConsole (sb);
        }

        [Command ("print-atlas-lookup", "For internal use when debugging the texture management system. Prints the lookup dictionary of packed atlas texture in a given texture group. Used for debugging atlas texture packing issues when using UI texture mods.")]
        private static void PrintAtlasLookup (string groupKey, string filter = null)
        {
            if (string.IsNullOrEmpty (groupKey) || !groups.TryGetValue (groupKey, out var group))
            {
                QuantumConsole.Instance.LogToConsole ($"Failed to find texture group {groupKey}");
                return;
            }

            if (group?.atlas?.texturesPacked == null)
            {
                QuantumConsole.Instance.LogToConsole ($"Failed to find atlas lookup on texture group {groupKey}");
                return;
            }

            var sb = new StringBuilder ();
            bool filterUsed = !string.IsNullOrEmpty (filter);

            var a = group.atlas;
            if (a.texturesPacked != null && a.texturesPacked.Length > 0)
            {
                sb.Append ("Packed textures (");
                sb.Append (a.texturesPacked.Length);
                sb.Append ("): ");

                for (int i = 0, count = a.texturesPacked.Length; i < count; ++i)
                {
                    var tex = a.texturesPacked[i];
                    var rect = a.rectsPacked[i];

                    if (filterUsed && !tex.name.Contains (filter))
                        continue;

                    sb.Append ($"\n");
                    sb.Append (i);
                    sb.Append (": ");
                    sb.Append (tex.name);
                    sb.Append (" (");
                    sb.Append (tex.width);
                    sb.Append ("x");
                    sb.Append (tex.height);
                    sb.Append (") - ");

                    sb.Append (rect.x.ToString ("0.###"));
                    sb.Append (", ");
                    sb.Append (rect.y.ToString ("0.###"));
                    sb.Append (" (");
                    sb.Append (rect.width.ToString ("0.###"));
                    sb.Append ("x");
                    sb.Append (rect.height.ToString ("0.###"));
                    sb.Append (")");
                }
            }
            else
                sb.Append ("-");

            sb.Append ("\n");

            SubmitBuilderToConsole (sb);
        }

        [Command ("print-atlas-sprite-links-modified", "For internal use when debugging the texture management system. Print sprites contained in the atlas, inclusive of changes applied by mods. Optional argument allows filtering specific subset of sprites.")]
        private static void PrintAtlasSpriteLinksModified (string filter = null)
        {
            PrintAtlasSpriteLinks (uiAtlasSpriteLinks, filter);
        }

        [Command ("print-atlas-sprite-links-original", "For internal use when debugging the texture management system. Print sprites contained in the atlas, without changes applied by mods. Optional argument allows filtering specific subset of sprites.")]
        private static void PrintAtlasSpriteLinksOriginal (string filter = null)
        {
            PrintAtlasSpriteLinks (uiAtlasSpriteLinksOriginal, filter);
        }

        private static void PrintAtlasSpriteLinks (SortedDictionary<string, UIAtlasSpriteLink> links, string filter = null)
        {
            if (links == null || links.Count == 0)
            {
                QuantumConsole.Instance.LogToConsole ($"UI atlas sprite links empty or null");
                return;
            }

            var sb = new StringBuilder ();

            sb.Append ("Atlas sprite links (");
            sb.Append (links.Count);
            sb.Append ("): ");

            bool filterUsed = !string.IsNullOrEmpty (filter);
            int i = 0;
            foreach (var kvp in links)
            {
                var link = kvp.Value;
                var tex = link.texture;
                var sd = link.spriteData;
                bool sdPresent = sd != null;
                var name = sdPresent ? sd.name : tex.name;

                if (filterUsed && !name.Contains (filter))
                    continue;

                bool sizeMismatch = sdPresent && (sd.width != tex.width || sd.height != tex.height);

                sb.Append ($"\n");
                sb.Append (i);
                sb.Append (": ");
                sb.Append (sdPresent ? sd.name : tex.name);
                sb.Append (" (");

                if (sizeMismatch)
                {
                    sb.Append ("SD ");
                    sb.Append (sd.width);
                    sb.Append ("x");
                    sb.Append (sd.height);

                    sb.Append (", 2D ");
                    sb.Append (tex.width);
                    sb.Append ("x");
                    sb.Append (tex.height);
                }
                else if (sdPresent)
                {
                    sb.Append ("SD ");
                    sb.Append (sd.width);
                    sb.Append ("x");
                    sb.Append (sd.height);
                }
                else
                {
                    sb.Append ("2D ");
                    sb.Append (tex.width);
                    sb.Append ("x");
                    sb.Append (tex.height);
                }

                sb.Append (") - ");

                if (sdPresent)
                {
                    sb.Append (sd.x.ToString ("0.###"));
                    sb.Append (", ");
                    sb.Append (sd.y.ToString ("0.###"));
                }
                else
                {
                    sb.Append ("no SD");
                }

                i += 1;
            }


            sb.Append ("\n");

            SubmitBuilderToConsole (sb);
        }

        [Command ("print-atlas-sprite-link-comparison", "For internal use when debugging the texture management system. Print sprites contained in the atlas, comparing original state to state after applying mods. Optional argument allows filtering specific subset of sprites.")]
        private static void PrintAtlasSpriteLinkComparison (string filter = null)
        {
            if (uiAtlasSpriteLinks == null || uiAtlasSpriteLinks.Count == 0 || uiAtlasSpriteLinksOriginal == null || uiAtlasSpriteLinksOriginal.Count == 0)
            {
                QuantumConsole.Instance.LogToConsole ($"UI atlas sprite links empty or null");
                return;
            }

            var sb = new StringBuilder ();

            sb.Append ("Atlas sprite links (");
            sb.Append (uiAtlasSpriteLinks.Count);
            sb.Append (" current, ");
            sb.Append (uiAtlasSpriteLinksOriginal.Count);
            sb.Append (" original): ");

            bool filterUsed = !string.IsNullOrEmpty (filter);
            int i = 0;
            foreach (var kvp in uiAtlasSpriteLinks)
            {
                var key = kvp.Key;
                var link = kvp.Value;
                var sd = link.spriteData;

                if (sd != null)
                {
                    var linkOriginalPresent = uiAtlasSpriteLinksOriginal.TryGetValue (key, out var linkOriginal);
                    if (linkOriginalPresent)
                    {
                        var sdOriginal = linkOriginal.spriteData;
                        bool sdOriginalPresent = sdOriginal != null;
                        if (sdOriginalPresent && (sdOriginal.width != sd.width || sdOriginal.height != sd.height))
                        {
                            sb.Append ("\n");
                            sb.Append (i);
                            sb.Append (": ");

                            sb.Append (sdOriginal.width);
                            sb.Append ("x");
                            sb.Append (sdOriginal.height);
                            sb.Append (" → ");

                            sb.Append (sd.width);
                            sb.Append ("x");
                            sb.Append (sd.height);
                        }
                    }
                }

                i += 1;
            }


            sb.Append ("\n");

            SubmitBuilderToConsole (sb);
        }

        private static void SubmitBuilderToConsole (StringBuilder sb)
        {
            if (sb == null)
                return;

            var text = sb.ToString ();
            if (text.Length > 8000)
            {
                int diff = text.Length - 8000;
                text = text.Substring (0, 8000);
                text += $" [... +{diff}]\n";
            }

            QuantumConsole.Instance.LogToConsole (text);
        }
        
        [Command ("print-atlas-validation")]
        private static void ValidateUIAtlas (string filter = null)
        {
            var atlas = VPHelper.atlasMain;
            if (atlas == null)
            {
                Debug.Log ($"Failed to update UI atlas as no reference was found");
                return;
            }

            bool filterUsed = !string.IsNullOrEmpty (filter);

            // Collect all existing sprites, loading source texture and all sprite atlas data together
            var spriteList = atlas.spriteList;
            for (int i = 0, count = spriteList.Count; i < count; ++i)
            {
                var sd = spriteList[i];
                if (sd == null || string.IsNullOrEmpty (sd.name))
                    continue;

                if (filterUsed && !sd.name.Contains (filter))
                    continue;

                var texture = Resources.Load<Texture2D> ($"Content/UI/Sprites/{sd.name}");
                if (texture == null)
                {
                    Debug.LogWarning ($"Failed to find texture for UI atlas sprite {i} ({sd.name})");
                    continue;
                }

                if (texture.width != sd.width || texture.height != sd.height)
                {
                    Debug.LogWarning ($"Texture for UI atlas sprite {i} ({sd.name}) doesn't match sprite data: SD {sd.width}x{sd.height} / 2D {texture.width}x{texture.height}", texture);
                }
                else if (filterUsed)
                {
                    Debug.Log ($"{i} ({sd.name}): SD {sd.width}x{sd.height} / 2D {texture.width}x{texture.height}");
                }
            }
        }

        public static void ResetUIAtlas ()
        {
            if (!uiAtlasModified)
            {
                Debug.LogWarning ($"Skipping UI atlas reset as it was not modified last");
                return;
            }
            
            // RebuildUIAtlas (null);
            Debug.LogWarning ($"Resetting UI atlas");

            uiAtlasModified = false;
            
            var atlas = VPHelper.atlasMain;
            if (atlas == null || atlas.texture == null)
            {
                Debug.Log ($"Failed to reset UI atlas as no reference to asset was found");
                return;
            }

            if (uiAtlasSpriteLinksOriginal == null || uiAtlasSpriteLinksOriginal.Count == 0)
            {
                Debug.Log ($"Failed to reset UI atlas as no original link data was found");
                return;
            }
            
            if (uiAtlasTextureOriginal == null)
            {
                Debug.Log ($"Failed to reset UI atlas as no original texture data was found");
                return;
            }

            var spriteList = new List<UISpriteData> ();
            foreach (var kvp in uiAtlasSpriteLinksOriginal)
            {
                var link = kvp.Value;
                if (link.spriteData != null)
                {
                    var sd = new UISpriteData ();
                    sd.CopyFrom (link.spriteData);
                    spriteList.Add (sd);
                }
            }
            
            atlas.spriteMaterial.mainTexture = uiAtlasTextureOriginal;
            atlas.spriteList = spriteList;
            atlas.SortAlphabetically ();
            atlas.MarkAsChanged ();

            UIFontHelper.RebuildFontsGlobally (true);
            Debug.LogWarning ($"Resetting UI atlas to original state with {uiAtlasSpriteLinksOriginal.Count} entries");
        }

        private static void RebuildUIAtlas (TextureGroup groupInserted)
        {
            var atlas = VPHelper.atlasMain;
            if (atlas == null || atlas.texture == null)
            {
                Debug.Log ($"Failed to update UI atlas as no reference to asset was found");
                return;
            }

            var atlasTexture = atlas.texture as Texture2D;
            if (atlasTexture == null)
            {
                Debug.Log ($"Failed to update UI atlas as no reference to atlas texture was found");
                return;
            }

            if (uiAtlasSpriteLinksOriginal == null)
            {
                // Only build links once to ensure clean atlas description is preserved from first load
                uiAtlasTextureOriginal = atlasTexture;
                uiAtlasSpriteLinksOriginal = new SortedDictionary<string, UIAtlasSpriteLink> ();
                uiAtlasSpriteLinks = new SortedDictionary<string, UIAtlasSpriteLink> ();

                var atlasTexturePixels = uiAtlasTextureOriginal.GetPixels32 ();
                var atlasTextureWidth = uiAtlasTextureOriginal.width;
                var atlasTextureHeight = uiAtlasTextureOriginal.height;

                // Collect all existing sprites, loading source texture and all sprite atlas data together
                foreach (var spriteData in atlas.spriteList)
                {
                    var spriteName = spriteData.name;
                    if (string.IsNullOrEmpty (spriteName))
                        continue;

                    var link = new UIAtlasSpriteLink ();
                    uiAtlasSpriteLinksOriginal.Add (spriteName, link);

                    link.spriteData = new UISpriteData ();
                    link.spriteData.CopyFrom (spriteData);

                    // This is simple, but requires the build to embed every source sprite as a resource
                    // To boot, some sprites were integrated into the atlas with settings that are no longer the same, e.g. alpha trim
                    // So directly loaded source files might be incompatible with sprite data, look off, not match slicing offsets etc.
                    // link.texture = Resources.Load<Texture2D> ($"Content/UI/Sprites/{spriteName}");

                    int width = spriteData.width;
                    int height = spriteData.height;
                    var texturePixelsNew = new Color32 [width * height];

                    for (int y = 0; y < height; ++y)
                    {
                        for (int x = 0; x < width; ++x)
                        {
                            int atlasIndex = (atlasTextureHeight - spriteData.y + y - height) * atlasTextureWidth + (spriteData.x + x);
                            int newIndex = y * width + x;
                            texturePixelsNew[newIndex] = atlasTexturePixels[atlasIndex];
                        }
                    }

                    var textureCropped = new Texture2D (width, height);
                    textureCropped.name = spriteData.name;
                    textureCropped.SetPixels32 (texturePixelsNew);
                    textureCropped.Apply ();

                    link.texture = textureCropped;

                    if (link.texture == null)
                        Debug.LogWarning ($"Failed to find texture for UI atlas sprite {spriteName}");
                }
            }

            bool groupUsed = groupInserted != null && groupInserted.atlas != null && groupInserted.textures != null && groupInserted.textures.Count > 0;
            if (groupUsed)
                Debug.Log ($"Preparing to merge {groupInserted.textures.Count} sprites with the UI atlas with {uiAtlasSpriteLinksOriginal.Count} existing sprites");
            else
                Debug.Log ($"Preparing to reset UI atlas back to original state with {uiAtlasSpriteLinksOriginal.Count} sprites");

            // Tag whether atlas is modified for later reference
            uiAtlasModified = true; // groupUsed

            // Insert original sprites first
            uiAtlasSpriteLinks.Clear ();
            foreach (var kvp in uiAtlasSpriteLinksOriginal)
            {
                var linkOriginal = kvp.Value;
                var sdOriginal = linkOriginal.spriteData;
                var sd = new UISpriteData ();
                sd.CopyFrom (sdOriginal);

                var link = new UIAtlasSpriteLink
                {
                    texture = linkOriginal.texture,
                    spriteData = sd
                };
                
                uiAtlasSpriteLinks.Add (kvp.Key, link);
            }

            // Iterate over loaded textures from streaming assets folder
            if (groupUsed)
            {
                foreach (var kvp in groupInserted.textures)
                {
                    var textureLoaded = kvp.Value;
                    var spriteName = kvp.Key;

                    if (uiAtlasSpriteLinks.TryGetValue (spriteName, out var link))
                    {
                        Debug.LogWarning ($"UI sprite replaced: {spriteName}");
                        link.texture = textureLoaded;
                    }
                    else
                    {
                        Debug.LogWarning ($"UI sprite added: {spriteName}");
                        uiAtlasSpriteLinks.Add (spriteName, new UIAtlasSpriteLink
                        {
                            texture = textureLoaded,
                            spriteData = null
                        });
                    }
                }
            }

            List<Texture2D> texturesPackedList = new List<Texture2D> ();
            Texture2D[] texturesPacked = null;
            Rect[] rectsPacked = null;
            Texture2D textureCombined = null;
            int padding = 2;
            bool forceSquare = true;

            if (groupUsed)
            {
                var a = groupInserted.atlas;
                textureCombined = a.textureCombined;
                padding = a.padding;
                forceSquare = a.forceSquare;
            }

            if (textureCombined == null)
                textureCombined = new Texture2D (1, 1, TextureFormat.ARGB32, false);

            // Now that full set is compiled, compile textures to list
            foreach (var kvp in uiAtlasSpriteLinks)
            {
                var link = kvp.Value;
                texturesPackedList.Add (link.texture);
            }

            Debug.Log ($"Merged UI atlas lookup: {uiAtlasSpriteLinks.Count} sprites");
            texturesPacked = texturesPackedList.ToArray ();

            int maxSize = SystemInfo.maxTextureSize;
            rectsPacked = UITexturePacker.PackTextures
            (
                textureCombined,
                texturesPacked,
                4,
                4,
                padding,
                maxSize,
                forceSquare
            );

            // If group is used, move values into it for later inspection
            if (groupUsed)
            {
                var a = groupInserted.atlas;
                a.textureCombined = textureCombined;
                a.texturesPacked = texturesPacked;
                a.rectsPacked = rectsPacked;
            }

            textureCombined.name = $"{atlas.name}_packed";

            var spriteList = atlas.spriteList;
            spriteList.Clear ();

            for (int i = 0, count = rectsPacked.Length; i < count; ++i)
            {
                var rectPacked = rectsPacked[i];
                var texturePacked = texturesPacked[i];
                var textureName = texturePacked.name;

                var rect = NGUIMath.ConvertToPixels (rectPacked, textureCombined.width, textureCombined.height, true);
                if (Mathf.RoundToInt (rect.width) != texturePacked.width)
                    continue;

                // Create new sprite data object
                var sd = new UISpriteData ();

                // If original atlas contained this sprite, copy everything about it
                if (uiAtlasSpriteLinksOriginal.TryGetValue (textureName, out var link) && link.spriteData != null)
                    sd.CopyFrom (link.spriteData);

                sd.name = textureName;
                sd.x = Mathf.RoundToInt (rect.x);
                sd.y = Mathf.RoundToInt (rect.y);
                sd.width = Mathf.RoundToInt (rect.width);
                sd.height = Mathf.RoundToInt (rect.height);

                uiAtlasSpriteLinks[textureName].spriteData = sd;

                spriteList.Add (sd);
            }

            atlas.spriteMaterial.mainTexture = textureCombined;
            atlas.spriteList = spriteList;
            atlas.SortAlphabetically ();
            atlas.MarkAsChanged ();

            UIFontHelper.RebuildFontsGlobally (true);
        }
        
        #endif
    }
}