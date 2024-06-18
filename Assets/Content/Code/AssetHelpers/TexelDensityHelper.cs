using UnityEngine;
using System.Collections.Generic;

public class TexelDensityHelper : MonoBehaviour
{
    public Texture globalDetailTexture;
    public float globalTriplanarScale = 2f;
    public float detailTilingBase = 1f;

    [Range (0f, 0.75f)]
    public float damageIntensity = 0.25f;

    [Range (0f, 0.75f)]
    public float paintIntensity = 0.25f;

    public Color colorPrimary;
    public Color colorSecondary;
    public Color colorTertiary;
    public Texture paintTexture;
    public List<GameObject> prefabs;
    public bool adjustForTexelDensity = false;

	public void Evaluate ()
    {
        Shader.SetGlobalFloat ("_GlobalTriplanarScale", globalTriplanarScale);
        if (globalDetailTexture != null)
            Shader.SetGlobalTexture ("_GlobalDetailTex", globalDetailTexture);

        if (prefabs == null || prefabs.Count == 0)
            return;

        for (int i = 0; i < prefabs.Count; ++i)
        {
            materials = new Dictionary<int, Material> ();
            GetMaterialsRecursive (prefabs[i].transform);

            if (materials.Count == 0)
                continue;

            foreach (KeyValuePair<int, Material> kvp in materials)
            {
                Material m = kvp.Value;
                m.SetFloat ("_Damage", damageIntensity);
                m.SetColor ("_ColorPrimary", colorPrimary);
                m.SetColor ("_ColorSecondary", colorSecondary);
                m.SetColor ("_ColorTertiary", colorTertiary);
                m.SetFloat ("_PaintIntensity", paintIntensity);
                if (paintTexture != null)
                    m.SetTexture ("_PaintTex", paintTexture);
            }
        }
    }

    private Dictionary<int, Material> materials;
    private MeshRenderer target;

    private void GetMaterialsRecursive (Transform parent)
    {
        MeshRenderer mr = parent.GetComponent<MeshRenderer> ();
        if (mr != null && mr.sharedMaterial != null && !materials.ContainsKey (mr.sharedMaterial.GetInstanceID ()))
        {
            materials.Add (mr.sharedMaterial.GetInstanceID (), mr.sharedMaterial);

            if (adjustForTexelDensity && parent.name.Contains ("surface"))
            {
                MeshFilter mf = parent.GetComponent<MeshFilter> ();
                Mesh mesh = mf.sharedMesh;

                if (mesh != null)
                {
                    int index0 = mesh.triangles[0];
                    int index1 = mesh.triangles[1];

                    Vector3 pos0 = mesh.vertices[index0];
                    Vector3 pos1 = mesh.vertices[index1];
                    float posDifference = Vector3.Distance (pos0, pos1);

                    Vector2 uv0 = mesh.uv[index0];
                    Vector2 uv1 = mesh.uv[index1];
                    float uvDifference = Vector2.Distance (uv0, uv1);

                    float multiplier = 1 / uvDifference;
                    float detailTilingAdjusted = posDifference * multiplier * detailTilingBase;

                    mr.sharedMaterial.SetFloat ("_DetailTiling", detailTilingAdjusted);
                }
            }
        }

        for (int i = 0; i < parent.childCount; ++i)
            GetMaterialsRecursive (parent.GetChild (i));
    }
}
