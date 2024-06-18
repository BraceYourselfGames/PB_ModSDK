using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class GameObjectType
{
    [MenuItem ("GameObject/Get info on type", false, -200)]
    [MenuItem ("Prefabs/Query/Get info on GameObject")]
    static public void PrintGameObjectInfo ()
    {
        var go = Selection.activeGameObject;
        if (go == null)
        {
            Debug.Log ("Please select a GameObject");
            return;
        }

        PrintGameObjectInfo (go);
    }

    static public void PrintGameObjectInfo (GameObject go)
    {
        var mainStage = StageUtility.GetMainStageHandle ();

        // Lets determine which stage we are in first because getting Prefab info depends on it
        var currentStage = StageUtility.GetStageHandle (go);
        if (currentStage == mainStage)
        {
            if (PrefabUtility.IsPartOfPrefabInstance (go))
            {
                var type = PrefabUtility.GetPrefabAssetType (go);
                var asset = PrefabUtility.GetCorrespondingObjectFromSource (go);
                string info = PrefabInfoHelper.GetPrefabInfoString (asset);
                Debug.Log (string.Format ("<i>ObjectInfo</i> | Selection ({0}) is part of a <b>prefab instance</b> in the MainStage. Prefab asset: \n{1}", go.name, info));
            }
            else
                Debug.Log (string.Format ("<i>ObjectInfo</i> | Selection ({0}) is a plain GameObject in the MainStage", go.name));
        }
        else
        {
            var prefabStage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage (go);
            if (prefabStage != null)
            {
                if (PrefabUtility.IsPartOfPrefabInstance (go))
                {
                    var asset = PrefabUtility.GetCorrespondingObjectFromSource (go);
                    string info = PrefabInfoHelper.GetPrefabInfoString (asset);
                    Debug.Log (string.Format ("<i>ObjectInfo</i> | Selection ({0}) is in the PrefabStage. It is part of a nested prefab instance. Prefab asset: \n{1}", go.name, info));
                }
                else
                {
                    var asset = AssetDatabase.LoadAssetAtPath<GameObject> (prefabStage.assetPath);
                    string info = PrefabInfoHelper.GetPrefabInfoString (asset);
                    Debug.Log (string.Format ("<i>ObjectInfo</i> | Selection ({0}) is in the PrefabStage and comes from the currently opened prefab. Prefab asset: \n{1}", go.name, info));
                }
            }
            else if (EditorSceneManager.IsPreviewSceneObject (go))
            {
                Debug.LogWarning (string.Format ("<i>ObjectInfo</i> | Selection ({0})  is not in the MainStage, nor in the PrefabStage. It occupies a PreviewScene (used for preview rendering or other utilities)", go.name));
            }
            else
            {
                Debug.LogError (string.Format ("<i>ObjectInfo</i> | Selection ({0}) is an unknown object type", go.name));
            }
        }
    }
}
