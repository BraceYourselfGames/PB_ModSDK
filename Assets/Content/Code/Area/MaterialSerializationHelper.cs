using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu (fileName = "MaterialSerializationHelper", menuName = "Other/MaterialSerializationHelper", order = 1)]
public class MaterialSerializationHelper : ScriptableObject
{
    [Serializable]
    public class VectorArrayContainer
    {
        public string sourceNameX = "_SmoothnessMin";
        public string sourceNameY = "_SmoothnessMed";
        public string sourceNameZ = "_SmoothnessMax";
        public string sourceNameW = "_WorldSpaceUVOverride";
        public string targetName = "";
        
        [LabelText ("Smoothness (Min/Med/Max), World UV Override")]
        public List<Vector4> value;
    }

    [InfoBox ("This container is supposed to collect 4 float properties per material from a set of source materials and put that into a vector array. Arrays are not serialized on materials, and this class fills that gap")]

    public List<VectorArrayContainer> vectorArrays;
    public List<Material> sourceMaterials;
    public Material targetMaterial;
    
    private static readonly int propertyID_MainTex = Shader.PropertyToID ("_MainTex");
    private static readonly int propertyID_MSEO = Shader.PropertyToID ("_MSEO");


    #if UNITY_EDITOR
    
    [Button ("Rebuild texture array", ButtonSizes.Medium)]
    public void RebuildTextureArray ()
    {
        if (targetMaterial == null)
            return;

        string materialAssetName = targetMaterial.name;
        string materialAssetPath = UnityEditor.AssetDatabase.GetAssetPath (targetMaterial);
        string materialFolderPath = materialAssetPath.Substring (0, materialAssetPath.Length - materialAssetName.Length - 4);
        Debug.LogWarning ($"{materialAssetPath}, {materialAssetName}, {materialFolderPath}");
        
        if (sourceMaterials.Count <= 1)
        {
            Debug.LogWarning ($"Cancelling array collapse operation scheduled for this holder due to number of materials being too low");
            return;
        }

        Debug.Log ($"Proceeding to generate material  | Materials found suitable for array collapse: {sourceMaterials.Count} | Folder path: {materialFolderPath}");

        // We delay instantitation of these objects until we encounter a texture - so that we can copy its format
        Texture2DArray textureArrayAH = null;
        Texture2DArray textureArrayMSEO = null;

        for (int m = 0; m < sourceMaterials.Count; ++m)
        {
            var material = sourceMaterials[m];
            var textureAH = material.GetTexture (propertyID_MainTex) as Texture2D;
            var textureMSEO = material.GetTexture (propertyID_MSEO) as Texture2D;

            if (textureArrayAH == null)
            {
                Debug.Log ($"Creating AH texture array with size of {sourceMaterials.Count}, format {textureAH.format} and dimensions {textureAH.width}x{textureAH.height}");
                textureArrayAH = new Texture2DArray (textureAH.width, textureAH.height, sourceMaterials.Count, TextureFormat.BC7, true);
            }

            if (textureArrayMSEO == null)
            {
                Debug.Log ($"Creating MSEO texture array with size of {sourceMaterials.Count}, format {textureMSEO.format} and dimensions {textureMSEO.width}x{textureMSEO.height}");
                textureArrayMSEO = new Texture2DArray (textureMSEO.width, textureMSEO.height, sourceMaterials.Count, TextureFormat.BC7, true);
            }

            if (textureAH != null)
            {
                if (textureAH.width == textureArrayAH.width && textureAH.height == textureArrayAH.height)
                {
                    Debug.LogWarning ($"Copying AH texture {m} {textureAH.name}: {textureAH.width}x{textureAH.height}");
                    Graphics.CopyTexture (textureAH, 0, textureArrayAH, m);
                }
                else
                    Debug.LogWarning ($"Skipping array write for AH texture {m} {textureAH.name} due to mismatched dimensions: {textureAH.width}x{textureAH.height}");
            }
            else
                Debug.LogWarning ($"Skipping array write for missing AH texture {m}");

            if (textureMSEO != null)
            {
                if (textureMSEO.width == textureArrayMSEO.width && textureMSEO.height == textureArrayMSEO.height)
                {
                    Debug.LogWarning ($"Copying MSEO texture {m} {textureMSEO.name}: {textureMSEO.width}x{textureMSEO.height}");
                    Graphics.CopyTexture (textureMSEO, 0, textureArrayMSEO, m);
                }
                else
                    Debug.LogWarning ($"Skipping array write for MSEO texture {m} {textureMSEO.name} due to mismatched dimensions: {textureMSEO.width}x{textureMSEO.height}");
            }
            else
                Debug.LogWarning ($"Skipping array write for missing MSEO texture {m}");
        }

        Texture2DArray textureArrayAHFromAsset = null;
        if (textureArrayAH != null)
        {
            textureArrayAH.Apply (false, false);
            textureArrayAHFromAsset = UtilityAssetDatabase.CreateAssetSafely (textureArrayAH, $"{materialFolderPath}materialTexArrayAH.asset");
            textureArrayAHFromAsset.Apply (false, true);
        }

        Texture2DArray textureArrayMSEOFromAsset = null;
        if (textureArrayMSEO != null)
        {
            textureArrayMSEO.Apply (false, false);
            textureArrayMSEOFromAsset = UtilityAssetDatabase.CreateAssetSafely (textureArrayMSEO, $"{materialFolderPath}materialTexArrayMSEO.asset");
            textureArrayMSEOFromAsset.Apply (false, true);
        }
        
        if (textureArrayAHFromAsset != null)
            targetMaterial.SetTexture (Shader.PropertyToID ("_TexArrayAH"), textureArrayAHFromAsset);
        if (textureArrayMSEOFromAsset != null)
            targetMaterial.SetTexture (Shader.PropertyToID ("_TexArrayMSEO"), textureArrayMSEOFromAsset);
        
        UnityEditor.AssetDatabase.SaveAssets ();
        UnityEditor.AssetDatabase.Refresh (UnityEditor.ImportAssetOptions.ForceSynchronousImport);
    }
    
    #endif

    [Button ("Fill vector array", ButtonSizes.Medium)]
    public void FillFromSources ()
    {
        if (sourceMaterials == null)
            return;

        for (int i = 0; i < vectorArrays.Count; ++i)
        {
            var container = vectorArrays[i];

            if (container.value != null && container.value.Count > 0 && container.value.Count != sourceMaterials.Count)
                Debug.LogWarning ($"Warning: array in container {i} was generated from a different number of materials - {container.value.Count}, not {sourceMaterials.Count}");

            container.value = new List<Vector4> ();

            for (int m = 0; m < sourceMaterials.Count; ++m)
            {
                var material = sourceMaterials[m];
                var value = Vector4.zero;

                if (material.HasProperty (container.sourceNameX))
                    value.x = material.GetFloat (Shader.PropertyToID (container.sourceNameX));

                if (material.HasProperty (container.sourceNameY))
                    value.y = material.GetFloat (Shader.PropertyToID (container.sourceNameY));

                if (material.HasProperty (container.sourceNameZ))
                    value.z = material.GetFloat (Shader.PropertyToID (container.sourceNameZ));

                if (material.HasProperty (container.sourceNameW))
                    value.w = material.GetFloat (Shader.PropertyToID (container.sourceNameW));

                container.value.Add (value);
            }
        }
    }

    [Button ("Apply to target", ButtonSizes.Medium)]
    public void ApplyToTarget ()
    {
        // Debug.Log ($"Applying shader properties from data {this.name}", this);
        if (targetMaterial == null)
        {
            Debug.LogWarning ("Target material absent", this);
            return;
        }

        for (int i = 0; i < vectorArrays.Count; ++i)
        {
            var container = vectorArrays[i];

            // No point checking for HasProperty - these can't be serialized
            targetMaterial.SetVectorArray (container.targetName, container.value.ToArray ());
        }
    }
}