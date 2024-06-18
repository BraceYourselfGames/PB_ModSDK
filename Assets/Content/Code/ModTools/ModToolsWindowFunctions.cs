using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Area;
using Content.Code.Utility;
using PhantomBrigade.Data;
using PhantomBrigade.Mods;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace PhantomBrigade.ModTools
{
    [TypeHinted]
    public interface IModToolsCheck
    {
        public bool IsTrue ();
    }

    [TypeHinted]
    public class ModToolsCheckAssetPackage : IModToolsCheck
    {
        public bool required;

        public bool IsTrue ()
        {
            bool assetsPresent = AssetPackageHelper.AreLevelAssetsInstalled () && AssetPackageHelper.AreUnitAssetsInstalled ();
            return required == assetsPresent;
        }
    }

    [TypeHinted]
    public class ModToolsCheckSceneName : IModToolsCheck
    {
        public bool expected = true;
        public string name;

        public bool IsTrue ()
        {
            var sceneActive = SceneManager.GetActiveScene ();
            bool match = sceneActive.name.Contains (name);
            return match == expected;
        }
    }

    [TypeHinted]
    public interface IModToolsFunction
    {
        public void Run ();
    }

    public class ModToolsFunctionOpenLink : IModToolsFunction
    {
        [LabelText ("URL")]
        public string url = "";

        public void Run ()
        {
            Application.OpenURL (url);
        }
    }
    
    public class ModToolsFunctionOpenLinkAssets : IModToolsFunction
    {
        public void Run ()
        {
            var url = AssetPackageHelper.levelAssetURL;
            Application.OpenURL (url);
        }
    }

    public class ModToolsFunctionOpenFolder : IModToolsFunction
    {
        public PathParentType parent = PathParentType.User;
        public string path = "";

        public void Run ()
        {
            string pathDir = DataPathHelper.GetCombinedPath (parent, path);
            if (!Directory.Exists (pathDir))
            {
                Debug.Log ($"Directory doesn't exist: {pathDir}");
                return;
            }

            Application.OpenURL ($"file://{pathDir}");
        }
    }

    public class ModToolsFunctionCreateFolder : IModToolsFunction
    {
        public PathParentType parent = PathParentType.User;
        public string path = "";

        public void Run ()
        {
            string pathDir = DataPathHelper.GetCombinedPath (parent, path);
            if (Directory.Exists (pathDir))
            {
                Debug.Log ($"Directory already exists: {pathDir}");
                return;
            }

            try
            {
                Directory.CreateDirectory (pathDir);
                Debug.Log ($"Directory created: {pathDir}");
            }
            catch (Exception e)
            {
                Debug.LogException (e);
            }
        }
    }

    public class ModToolsFunctionCreateConfig : IModToolsFunction
    {
        public PathParentType parent = PathParentType.User;
        public string path = "";
        public string filename = "";

        public void Run ()
        {
            #if UNITY_EDITOR
            
            string pathDir = DataPathHelper.GetCombinedPath (parent, path);

            if (!Directory.Exists (pathDir))
            {
                Debug.Log ($"Directory doesn't exist: {pathDir}");
                return;
            }

            if (string.IsNullOrEmpty (filename))
            {
                Debug.LogWarning ($"Can't save a config: invalid name");
            }

            var payload = GetPayload ();
            if (payload == null)
            {
                Debug.LogWarning ($"Can't save a config: no data to save");
                return;
            }

            UtilitiesYAML.SaveToFile (pathDir, $"{filename}.yaml", payload);
            
            #endif
        }

        protected virtual object GetPayload ()
        {
            return null;
        }
    }

    public class ModToolsFunctionCreateConfigMetadata : ModToolsFunctionCreateConfig
    {
        public ModMetadata payload = new ModMetadata ();

        protected override object GetPayload () => payload;
    }



    public class ModToolsFunctionCallMethodStatic : IModToolsFunction
    {
        [Tooltip ("Type of the view class to target.")]
        public string typeName = "";

        [Tooltip ("Method name. Method must be public and static to be discovered.")]
        public string methodName = "";

        [Tooltip ("Method name. Method must be public and static to be discovered.")]
        public string argument = "";

        public void Run ()
        {
            #if UNITY_EDITOR
            
            if (string.IsNullOrEmpty (typeName))
            {
                Debug.LogWarning ($"Failed to call a static method: no type name provided");
                return;
            }

            var type = Type.GetType (typeName);
            if (type == null)
            {
                Debug.LogWarning ($"Failed to call a static method: failed to find type {typeName}");
                return;
            }

            if (string.IsNullOrEmpty (methodName))
            {
                Debug.LogWarning ($"Failed to call a static method: no method name provided for type {typeName}");
                return;
            }

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.FlattenHierarchy;
            var methodInfo = type.GetMethod (methodName, flags);
            if (methodInfo == null)
            {
                Debug.LogWarning ($"Failed to call a static method: failed to find method {methodName} on type {typeName}");
                return;
            }

            if (!string.IsNullOrEmpty (argument))
            {
                Debug.LogWarning ($"Invoking method {methodName} on type {typeName} with an argument: {argument}");
                methodInfo.Invoke (null, new object[] { argument });
            }
            else
            {
                Debug.LogWarning ($"Invoking method {methodName} on type {typeName} without an argument");
                methodInfo.Invoke (null, null);
            }
            
            #endif
        }
    }

    public class ModToolsFunctionDatabaseSelect : IModToolsFunction
    {
        [Tooltip ("Type of the data multi linker DB to target")]
        public string typeName = "";

        public void Run ()
        {
            #if UNITY_EDITOR
            
            var linker = DataMultiLinkerUtility.FindLinkerAsInterface (typeName);
            if (linker != null)
                linker.SelectObject ();
            
            #endif
        }
    }

    public class ModToolsFunctionDatabaseFilter : IModToolsFunction
    {
        [Tooltip ("Type of the data multi linker DB to target")]
        public string typeName = "";

        public bool filterUsed = true;
        
        [ShowIf ("filterUsed")]
        public bool filterExact = true;

        [ShowIf ("filterUsed")]
        public string filter = "";

        public void Run ()
        {
            #if UNITY_EDITOR
            
            var linker = DataMultiLinkerUtility.FindLinkerAsInterface (typeName);
            if (linker != null)
                linker.SetFilter (filterUsed, filter, filterExact);
            
            #endif
        }
    }

    public class ModToolsFunctionDatabaseSave : IModToolsFunction
    {
        [Tooltip ("Type of the data multi linker DB to target")]
        public string typeName = "";

        public void Run ()
        {
            #if UNITY_EDITOR
            
            var linker = DataMultiLinkerUtility.FindLinkerAsInterface (typeName);
            if (linker != null)
                linker.SaveDataLocal ();
            
            #endif
        }
    }

    public class ModToolsFunctionDatabaseCreateEntry : IModToolsFunction
    {
        [Tooltip ("Type of the data multi linker DB to target")]
        public string typeName = "";
        public string entryKey = "";

        public void Run ()
        {
            #if UNITY_EDITOR
            
            Debug.LogFormat ("Create entry in multi linker DB | type: {0} | key: {1}", typeName, entryKey);
            if (string.IsNullOrEmpty (entryKey))
            {
                return;
            }

            var linker = DataMultiLinkerUtility.FindLinkerAsInterface (typeName);
            if (linker == null)
            {
                Debug.LogWarning ("Unable to find linker interface for : " + typeName);
                return;
            }

            linker.SelectObject ();
            linker.CreateEntry (entryKey);
            linker.SetFilter (true, entryKey, true);
            Editor inspector = null;
            Editor.CreateCachedEditor (Selection.activeGameObject, null, ref inspector);
            if (inspector != null)
            {
                // This ensures that the newly created entry is immediately shown in the inspector.
                // Otherwise you have to move the mouse over the inspector to get it to update and
                // show the new entry. If you don't, it looks like the button didn't do anything.
                inspector.Repaint ();
            }
            
            #endif
        }
    }

    public class ModToolsFunctionDatabaseLoad : IModToolsFunction
    {
        [Tooltip ("Type of the data multi linker DB to target")]
        public string typeName = "";

        public void Run ()
        {
            #if UNITY_EDITOR
            
            var linker = DataMultiLinkerUtility.FindLinkerAsInterface (typeName);
            if (linker != null)
                linker.LoadDataLocal ();
            
            #endif
        }
    }

    public class ModToolsFunctionSelectComponent : IModToolsFunction
    {
        [Tooltip ("Type of the view class to target.")]
        public string typeName = "";

        public void Run ()
        {
            #if UNITY_EDITOR
            
            if (string.IsNullOrEmpty (typeName))
            {
                Debug.LogWarning ($"Failed to select component: no type name provided");
                return;
            }

            var type = Type.GetType (typeName);
            if (type == null)
            {
                Debug.LogWarning ($"Failed to select component: failed to find type {typeName}");
                return;
            }

            var obj = GameObject.FindObjectOfType (type);
            if (obj == null)
            {
                Debug.LogWarning ($"Failed to select component: failed to find a GameObject with type {typeName}");
                return;
            }

            Selection.activeObject = obj;
            
            #endif
        }
    }

    public class ModToolsFunctionSelectProjectAsset : IModToolsFunction
    {
        [Tooltip ("Path to a project asset to be selected")]
        public string path = "";

        public void Run ()
        {
            #if UNITY_EDITOR
            
            if (string.IsNullOrEmpty (path))
            {
                Debug.LogWarning ($"Can't select an asset: no path provided");
                return;
            }
            
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object> (path);
            if (asset == null)
            {
                Debug.LogWarning ($"Can't select an asset: nothing found at path {path}");
                return;
            }

            Selection.activeObject = asset;
            
            #endif
        }
    }
    
    public class ModToolsFunctionOpenMainScene : IModToolsFunction
    {
        public void Run ()
        {
            #if UNITY_EDITOR

            var pathExtended = "Assets/ContentOptional/Scenes/game_extended_sdk.unity";
            var assetExtended = AssetDatabase.LoadAssetAtPath<SceneAsset> (pathExtended);
            
            var pathMain = "Assets/Content/Scenes/game_main_sdk.unity";
            var assetMain = AssetDatabase.LoadAssetAtPath<SceneAsset> (pathMain);

            if (assetExtended != null)
            {
                Debug.Log ("Extended scene found, opening...");
                EditorSceneManager.OpenScene (pathExtended);
            }
            else if (assetMain != null)
            {
                Debug.Log ("Main scene found, opening...");
                EditorSceneManager.OpenScene (pathMain);
            }
            else
            {
                Debug.LogWarning ("Failed to find both main and extended scene. Verify integrity of this install.");
            }
            
            #endif
        }
    }
    
    public class ModToolsFunctionOpenScene : IModToolsFunction
    {
        public string path = "Assets/Content/Scenes/game_main_sdk";
        
        public void Run ()
        {
            #if UNITY_EDITOR

            var pathFinal = path;
            if (!pathFinal.EndsWith (".unity"))
                pathFinal += ".unity";
            
            var asset = AssetDatabase.LoadAssetAtPath<SceneAsset> (pathFinal);
            if (asset != null)
            {
                Debug.Log ($"Opening scene at path: {pathFinal}");
                EditorSceneManager.OpenScene (pathFinal);
            }
            else
            {
                Debug.LogWarning ($"Failed to find the scene at path {path}");
            }
            
            #endif
        }
    }

    public class ModToolsFunctionRefreshAssetManagers : IModToolsFunction
    {
        public void Run ()
        {
            #if UNITY_EDITOR
            
            TextureManager.Load ();
            AreaTilesetHelper.LoadDatabase ();
            AreaAssetHelper.LoadResources ();
            ItemHelper.LoadVisuals ();
            
            #endif
        }
    }
}
