using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PhantomBrigade.Data.UI;
using PhantomBrigade.Mods;
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

    public class UIAtlasSpriteLink
    {
        public Texture2D texture;
    }

    public class TextureGroup
    {
        public TextureGroupAtlas atlas;
        public string folderName = string.Empty;
        public Vector2Int requiredDimensions = new Vector2Int (0, 0);
        public int requiredFilesize = 0;
        public bool opacityChecked = false;

        public HashSet<string> textureKeysOpaque;
        public List<string> textureKeysInternal;
        public List<string> textureKeysExposed = new List<string> ();
        public SortedDictionary<string, Texture2D> textures = new SortedDictionary<string, Texture2D> ();
    }

    public static class TextureGroupKeys
    {
        public const string CombatComms = "combatComms";
        public const string CombatAreas = "combatAreas";
        public const string PilotPortraits = "pilotPortraits";
        public const string OverworldEvents = "overworldEvents";
        public const string OverworldEntities = "overworldEntities";
        public const string Sprites = "sprites";
    }
    
    public static class TextureManager
    {
        private static bool loaded = false;
        private static bool loadedAnyContent = false;
        private static bool loadedDuringPlay = false;
        private static bool log = false;

        public  static bool uiAtlasModified;
        private static Texture2D uiAtlasTextureOriginal;
        private static SortedDictionary<string, UIAtlasSpriteLink> uiAtlasSpriteLinksOriginal;
        private static SortedDictionary<string, UIAtlasSpriteLink> uiAtlasSpriteLinks;
        public static SortedDictionary<string, string> groupKeysFromFolders = new SortedDictionary<string, string> ();

        public static SortedDictionary<string, TextureGroup> groups = new SortedDictionary<string, TextureGroup>
        {
            {
                TextureGroupKeys.CombatComms,
                new TextureGroup
                {
                    folderName = "UI/CombatComms",
                    opacityChecked = false,
                    requiredDimensions = new Vector2Int (128, 128)
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
                TextureGroupKeys.OverworldEvents,
                new TextureGroup
                {
                    folderName = "UI/OverworldEvents",
                    requiredDimensions = new Vector2Int (1024, 512)
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
                        var tex = new Texture2D (4, 4, TextureFormat.BC7, true, false);
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

        public static Texture2D GetTexture (string groupKey, string textureKey)
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

            var group = !string.IsNullOrEmpty (groupKey) && groups.ContainsKey (groupKey) ? groups[groupKey] : null;
            if (group == null || group.textures == null)
            {
                Debug.LogWarning ($"Texture manager failed to find group {groupKey} or it had no textures to return");
                return null;
            }

            var texture = !string.IsNullOrEmpty (textureKey) && group.textures.ContainsKey (textureKey) ? group.textures[textureKey] : null;
            if (texture == null)
                Debug.LogWarning ($"Texture manager failed to find texture {textureKey} in group {groupKey}");

            return texture;
        }
        
        public static Texture2D GetTextureAndOpacity (string groupKey, string textureKey, out bool opaque)
        {
            opaque = false;
            if (!loaded)
                Load ();
            
            // No point logging missing textures if not a single texture is in memory - it's not an issue with a particular group/key then
            if (!loadedAnyContent)
                return null;

            var group = !string.IsNullOrEmpty (groupKey) && groups.ContainsKey (groupKey) ? groups[groupKey] : null;
            if (group == null || group.textures == null)
            {
                Debug.LogWarning ($"Texture manager failed to find group {groupKey} or it had no textures to return");
                return null;
            }

            var texture = !string.IsNullOrEmpty (textureKey) && group.textures.ContainsKey (textureKey) ? group.textures[textureKey] : null;
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
                // Debug.Log ("UI atlases not supported in mod SDK");
                // RebuildUIAtlas (group);
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
    }
}