using System.Collections.Generic;
using UnityEngine;

public static class PrimitiveHelper
{
    private static Dictionary<PrimitiveType, Mesh> primitiveMeshes = new Dictionary<PrimitiveType, Mesh>();
    private static Material defaultMaterial;

    public static GameObject CreatePrimitive(PrimitiveType type, bool withCollider)
    {
        if (withCollider) { return GameObject.CreatePrimitive(type); }

        GameObject gameObject = new GameObject(type.ToString());
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = GetPrimitiveMesh(type);
        gameObject.AddComponent<MeshRenderer>();

        return gameObject;
    }

    public static Mesh GetPrimitiveMesh(PrimitiveType type)
    {
        if (!primitiveMeshes.ContainsKey(type))
            CreatePrimitiveMesh(type);

        return primitiveMeshes[type];
    }

    private static Mesh CreatePrimitiveMesh(PrimitiveType type)
    {
        GameObject gameObject = GameObject.CreatePrimitive(type);
        Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;

        if (defaultMaterial == null)
            defaultMaterial = gameObject.GetComponent<MeshRenderer> ().sharedMaterial;

        GameObject.DestroyImmediate(gameObject);

        primitiveMeshes[type] = mesh;
        return mesh;
    }

    public static Material GetDefaultMaterial ()
    {
        return defaultMaterial;
    }
}