using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEditor;
 
[InitializeOnLoad]
[HarmonyPatch]
public class MaterialEditorPatch
{
    private static Type _type;
    private static bool _mouseUp;
 
    static MaterialEditorPatch()
    {
        Patch();
    }
 
    [MenuItem("PB Mod SDK/Other/Patch ExtractCustomEditorType")]
    public static void Patch()
    {
        var target = TargetMethod();
        var prefix = typeof(MaterialEditorPatch).GetMethod("Prefix", BindingFlags.NonPublic | BindingFlags.Static);
     
        var harmony = new Harmony ("ExtractCustomEditorTypePatch");
        harmony.Patch(target, new HarmonyMethod(prefix));
    }
 
 
    static MethodInfo TargetMethod()
    {
        _type = AccessTools.TypeByName("UnityEditor.ShaderGUIUtility");
        return _type.GetMethod("ExtractCustomEditorType", AccessTools.all); 
    }
    static Dictionary<string, Type> _cache = new Dictionary<string, Type>();
 
    static bool Prefix(ref Type __result, ref string customEditorName)
    {
        if (_cache.TryGetValue(customEditorName, out __result))
            return false;
        if (string.IsNullOrEmpty(customEditorName))
            return false;
     
        var str = "UnityEditor." + customEditorName;
        Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
     
        for (var i = loadedAssemblies.Length - 1; i >= 0; i--)
        {
            foreach (var type2 in loadedAssemblies[i].GetTypes())
            {
                if (!type2.FullName.Equals(customEditorName, StringComparison.Ordinal) &&
                    !type2.FullName.Equals(str, StringComparison.Ordinal))
                    continue;
                if (typeof(ShaderGUI).IsAssignableFrom(type2))
                {
                    __result = type2;
                    _cache.Add(customEditorName, type2);
                    return false;
                }
                break;
            }
        }
        _cache.Add(customEditorName, null);
     
        return false;
    }
}