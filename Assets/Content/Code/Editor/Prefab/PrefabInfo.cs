using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using System.Text;

public class PrefabInfo
{
    private const string longformInfoTemplate =
@"<i>PrefabInfo</i> | <b>Outermost prefab instance</b> selection is part of comes from the following asset:
{0} (...)

<b>Nearest prefab instance</b> root the selection is part of comes from the following asset:
{1}

<b>Innermost prefab instance</b> the selection is part of comes from the following asset:
{2}

Complete nesting chain from outermost to original:
";

    private const string shortformInfoTemplate =
@"<i>PrefabInfo</i> | <b>Outermost/nearest/innermost prefab instance</b> selection is part of comes from the following asset:
{0} (...)

Complete nesting chain from outermost to original:
";

    [MenuItem ("GameObject/Get info on prefab", false, -200)]
    [MenuItem ("Prefabs/Query/Get info on prefab")]
    static public void PrintPrefabInfo ()
    {
        var go = Selection.activeGameObject;
        if (go == null)
        {
            Debug.Log ("Please select a GameObject");
            return;
        }

        PrintPrefabInfo (go);
    }


    static public void PrintPrefabInfo (GameObject go)
    {
        if (PrefabInfoHelper.prefabAssetLast != null)
            PrefabInfoHelper.prefabAssetLast = null;

        StringBuilder stringBuilder = new StringBuilder ();
        PrefabStage prefabStage = PrefabStageUtility.GetPrefabStage (go);
        if (prefabStage != null)
        {
            GameObject openPrefabThatContentsIsPartOf = AssetDatabase.LoadAssetAtPath<GameObject> (prefabStage.assetPath);
            stringBuilder.AppendFormat
            (
                "<i>PrefabInfo</i> | The selected GameObject {0} is part of the Prefab contents of the Prefab Asset:\n{1}\n\n",
                go.name,
                PrefabInfoHelper.GetPrefabInfoString (openPrefabThatContentsIsPartOf)
            );
        }

        if (!PrefabUtility.IsPartOfPrefabInstance (go))
        {
            stringBuilder.Append ("<i>PrefabInfo</i> | The selected GameObject is a plain GameObject (not part of a Prefab instance).\n");
        }
        else
        {
            // This is the Prefab Asset that can be applied to via the Overrides dropdown.
            GameObject outermostPrefabAssetObject = PrefabUtility.GetCorrespondingObjectFromSource (go);
            // This is the Prefab Asset that determines the icon that is shown in the Hierarchy for the nearest root.
            GameObject nearestRootPrefabAssetObject = AssetDatabase.LoadAssetAtPath<GameObject> (PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot (go));
            // This is the Prefab Asset where the original version of the object comes from.
            GameObject originalPrefabAssetObject = PrefabUtility.GetCorrespondingObjectFromOriginalSource (go);

            if (outermostPrefabAssetObject == nearestRootPrefabAssetObject && outermostPrefabAssetObject == originalPrefabAssetObject)
                stringBuilder.AppendFormat (shortformInfoTemplate, PrefabInfoHelper.GetPrefabInfoString (outermostPrefabAssetObject));
            else
            {
                stringBuilder.AppendFormat
                (
                    longformInfoTemplate,
                    PrefabInfoHelper.GetPrefabInfoString (outermostPrefabAssetObject, true),
                    PrefabInfoHelper.GetPrefabInfoString (nearestRootPrefabAssetObject, true),
                    PrefabInfoHelper.GetPrefabInfoString (originalPrefabAssetObject, true)
                );
            }

            GameObject current = outermostPrefabAssetObject;
            while (current != null)
            {
                stringBuilder.AppendLine (PrefabInfoHelper.GetPrefabInfoString (current));
                current = PrefabUtility.GetCorrespondingObjectFromSource (current);
            }
        }

        stringBuilder.AppendLine ("");
        Debug.Log (stringBuilder.ToString (), go);
    }

    
}

public static class PrefabInfoHelper
{
    public static GameObject prefabAssetLast;

    public static string GetPrefabInfoString (GameObject prefabAsset, bool compareToLast = false)
    {
        if (prefabAsset == null)
            return "<b>null</b>";

        if (compareToLast && prefabAssetLast != null && prefabAsset == prefabAssetLast)
            return "<b>—</b> <i>(same as previous)</i>";
        else
        {
            if (compareToLast)
                prefabAssetLast = prefabAsset;

            string name = prefabAsset.transform.root.gameObject.name;
            string assetPath = AssetDatabase.GetAssetPath (prefabAsset);
            PrefabAssetType type = PrefabUtility.GetPrefabAssetType (prefabAsset);
            return string.Format ("<b>{0}</b> (type: {1}) at '{2}'", name, type, assetPath);
        }
    }
}
