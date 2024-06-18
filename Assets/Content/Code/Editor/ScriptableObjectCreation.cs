#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class ScriptableObjectCreation
{
    public static T CreateAsset<T>(string folder, string fileName) where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();

        AssetDatabase.CreateAsset(asset, "Assets/Content/Objects/" + folder + "/" + fileName);

        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;

        return asset;
    }
}
#endif