using UnityEngine;

[ExecuteInEditMode]
public class VegetationDeadzoneOverride : MonoBehaviour
{
    private static MaterialPropertyBlock block;
    private static int id;

    private void Awake ()
    {
        Setup ();
    }

    private void OnEnable ()
    {
        Setup ();
    }

    private void Setup ()
    {
        if (Utilities.isPlaymodeChanging)
            return;

        if (block == null)
        {
            id = Shader.PropertyToID ("_BendingDeadzoneBottomHeight");
            block = new MaterialPropertyBlock ();
        }

        block.SetFloat (id, transform.position.y);

        MeshRenderer mr = gameObject.GetComponent<MeshRenderer> ();
        if (mr != null)
            mr.SetPropertyBlock (block);

        MeshRenderer[] mrs = gameObject.GetComponentsInChildren<MeshRenderer> ();
        if (mrs != null)
        {
            for (int i = 0; i < mrs.Length; ++i)
            {
                if (mrs[i] != null && mrs[i] != mr)
                    mrs[i].SetPropertyBlock (block);
            }
        }
    }
}