using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class UtilityMaterial
{
    public static Material GetMaterialInstance (ref Material material, Color color, int mode)
    {
        #if UNITY_EDITOR
        if (material == null)
        {
            material = new Material (AssetDatabase.GetBuiltinExtraResource<Material> ("Default-Material.mat"));
            material.SetFloat ("_Mode", (float)mode);
            if (mode == 2)
            {
                material.SetInt ("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt ("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt ("_ZWrite", 0);
                material.DisableKeyword ("_ALPHATEST_ON");
                material.DisableKeyword ("_ALPHABLEND_ON");
                material.EnableKeyword ("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
            }
            material.SetColor ("_Color", color);
            return material;
        }
        return material;
        #else
        return null;
        #endif
    }

    public static void SetMaterialsOnRenderers (GameObject target, Material material)
    {
        if (target == null || material == null)
            return;

        MeshRenderer[] mrs = target.GetComponentsInChildren<MeshRenderer> ();
        for (int i = 0; i < mrs.Length; ++i)
        {
            mrs[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Material[] sharedMaterialsCopy = mrs[i].sharedMaterials;
            for (int m = 0; m < sharedMaterialsCopy.Length; ++m)
                sharedMaterialsCopy[m] = material;
            mrs[i].sharedMaterials = sharedMaterialsCopy;
        }

        //MeshRenderer mr = target.GetComponent<MeshRenderer> ();
        //mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        //Material[] sharedMaterialsCopyB = mr.sharedMaterials;
        //for (int m = 0; m < sharedMaterialsCopyB.Length; ++m)
        //    sharedMaterialsCopyB[m] = material;
        //mr.sharedMaterials = sharedMaterialsCopyB;
    }

    private static System.Reflection.MethodInfo getBuiltinExtraResourcesMethod;

    #if UNITY_EDITOR
    public static Material GetDefaultMaterial ()
    {
        if (getBuiltinExtraResourcesMethod == null)
        {
            System.Reflection.BindingFlags bfs = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
            getBuiltinExtraResourcesMethod = typeof (EditorGUIUtility).GetMethod ("GetBuiltinExtraResource", bfs);
        }
        return (Material)getBuiltinExtraResourcesMethod.Invoke (null, new object[] { typeof (Material), "Default-Material.mat" });
    }
    #endif
}
