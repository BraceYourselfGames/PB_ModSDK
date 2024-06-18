using UnityEngine;

[ExecuteInEditMode]
public class AssignMaterialsOnStart : MonoBehaviour
{
    public Material[] sharedMaterialsOverride;

    private void OnEnable ()
    {
        if (sharedMaterialsOverride == null || sharedMaterialsOverride.Length == 0)
            return;

        var r = gameObject.GetComponent<Renderer> ();
        if (r == null)
            return;
        
        r.sharedMaterials = sharedMaterialsOverride;
    }
}
