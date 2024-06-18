using UnityEngine;


//Used to recursively clean names, for bone matching. I shake my fist at whoever named things boneName 1, bonename 2, etc..
//Not meant for runtime, used to process assets in the editor
public class NameCleaner : MonoBehaviour
{
    public Transform target;
    public string[] splitStrings;
    private Transform current;
    private string cleanedName;

    [ContextMenu("Clean Names")]
    public void Clean()
    {
        if (target == null)
            target = transform;

        RecursivelyCleanName(target);
    }

    public void RecursivelyCleanName(Transform parent)
    {
        for(int i = 0; i < parent.childCount; ++i)
        {
            current = parent.GetChild(i);

            cleanedName = (current.name.Split(splitStrings, System.StringSplitOptions.RemoveEmptyEntries))[0];
            current.name = cleanedName;                        
            RecursivelyCleanName(current);
        }
    }
}
