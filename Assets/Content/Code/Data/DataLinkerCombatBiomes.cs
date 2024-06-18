using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataLinkerCombatBiomes : DataLinker<DataContainerCombatBiomes>
    {
        public static DataBlockCombatBiomeLayer GetLayerData (string key)
        {
            if (string.IsNullOrEmpty (key) || data == null || data.layers == null || !data.layers.ContainsKey (key))
                return null;

            return data.layers[key];
        }
        
        public static Texture2D GetTextureSlope (string key)
        {
            if (string.IsNullOrEmpty (key) || data == null || data.texturesSlopes == null || !data.texturesSlopes.ContainsKey (key))
                return null;

            return data.texturesSlopes[key].texture;
        }
        
        public static Texture2D GetTextureDistant (string key)
        {
            if (string.IsNullOrEmpty (key) || data == null || data.texturesDistant == null || !data.texturesDistant.ContainsKey (key))
                return null;

            return data.texturesDistant[key].texture;
        }
        
        public static Texture2D GetTextureSplat (string key)
        {
            if (string.IsNullOrEmpty (key) || data == null || data.texturesSplats == null || !data.texturesSplats.ContainsKey (key))
                return null;

            return data.texturesSplats[key].texture;
        }
    }
}


