using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class HierarchyIntegrityChecker : MonoBehaviour
{
    #if UNITY_EDITOR
    
    [ShowInInspector, ReadOnly]
    private static EventSystem eventSystemCurrent
    {
        get
        {
            return EventSystem.current;
        }
    }
    
    private int goCount = 0, componentsCount = 0, missingCount = 0;

    private void OnEnable ()
    {
        FindInSelected ();
    }

    private void FindInSelected ()
    {
        goCount = 0;
        componentsCount = 0;
        missingCount = 0;
        RecursiveSearch (gameObject);
    }

    private void RecursiveSearch (GameObject g)
    {
        goCount++;

        var components = g.GetComponents<Component> ();

        // Create a serialized object so that we can edit the component list
        var serializedObject = new SerializedObject (g);
        var prop = serializedObject.FindProperty ("m_Component");

        for (int j = 0; j < components.Length; j++)
        {
            componentsCount++;
            if (components[j] == null)
            {
                missingCount++;
                string s = g.name;
                Transform t = g.transform;
                while (t.parent != null)
                {
                    s = t.parent.name + "/" + s;
                    t = t.parent;
                }

                Debug.LogWarning (s + " has an empty/broken script attached in position: " + j, g);
            }
        }

        foreach (Transform child in g.transform)
            RecursiveSearch (child.gameObject);
    }
    #endif
}
