using System;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Character Base Cloth Textures Data", menuName = "Pilots/Character Base Cloth Textures Data", order = 1)]
public class CharacterBaseClothTexturesData : ScriptableObject
{
    [BoxGroup]
    public bool SupportTinting = true;

    // Character head textures - head UVs are shared
    [BoxGroup]
    public Texture HeadBaseClothTextureAlbedo;
    
    [BoxGroup]
    public Texture HeadBaseClothTextureMSEO;
    
    [BoxGroup]
    public Texture HeadBaseClothTextureNormal;
    
    // Character body textures    
    [BoxGroup ("PBMale"), HideLabel]
    public CharacterBaseClothVariant variantM;
    
    [BoxGroup ("PBFemale"), HideLabel]
    public CharacterBaseClothVariant variantF;
    
    [Serializable]
    public class CharacterBaseClothVariant
    {
        public Texture BodyBaseClothTextureAlbedo;
        public Texture BodyBaseClothTextureMSEO;
        public Texture BodyBaseClothTextureNormal;
    }
}
