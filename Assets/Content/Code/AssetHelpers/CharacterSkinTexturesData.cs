using System;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Character Skin Textures Data", menuName = "Pilots/Character Skin Textures Data", order = 1)]
public class CharacterSkinTexturesData : ScriptableObject
{
    [Range (0.0f, 3.0f)]
    public float SkinSmoothnessIntensity = 1.6f;

    [BoxGroup ("PBMale"), HideLabel]
    public CharacterSkinVariant variantM;
    
    [BoxGroup ("PBFemale"), HideLabel]
    public CharacterSkinVariant variantF;
    
    [Serializable]
    public class CharacterSkinVariant
    {
        public Texture HeadSkinTextureAlbedo;
        public Texture HeadSkinTextureRDSO;
        public Texture HeadSkinTextureNormal;
        public Texture BodySkinTextureAlbedo;
        public Texture BodySkinTextureRDSO;
        public Texture BodySkinTextureNormal;
    }
}
