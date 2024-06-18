using System.Collections.Generic;
using UnityEngine;

public class TilesetArrayTest : MonoBehaviour
{
    public Texture2DArray array;
    public List<Texture2D> textures;
    public string path = "Assets/TextureArray";

    [ContextMenu ("Create array")]
    public void TestArrayGeneration ()
    {
        #if UNITY_EDITOR
        if (textures.Count == 0 || textures[0] == null)
            return;

        Texture2D texturePrimary = textures[0];
        int width = texturePrimary.width;
        int height = texturePrimary.height;
        int depth = textures.Count;
        TextureFormat format = texturePrimary.format;

        array = new Texture2DArray (width, height, depth, format, true);
        for (int i = 0; i < textures.Count; i++)
            array.SetPixels (textures[i].GetPixels (), i);
        array.Apply ();

        UnityEditor.AssetDatabase.CreateAsset (array, path + ".asset");
        UnityEditor.AssetDatabase.SaveAssets ();
        UnityEditor.AssetDatabase.Refresh ();
        #endif
    }
}
