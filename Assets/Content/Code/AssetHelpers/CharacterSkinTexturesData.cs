using System;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Character Skin Textures Data", menuName = "Pilots/Character Skin Textures Data", order = 1)]
public class CharacterSkinTexturesData : ScriptableObject
{
    // Character skin textures
    [BoxGroup]
    public Texture HeadSkinTextureAlbedo;
    
    [BoxGroup]
    public Texture HeadSkinTextureRDSO;
    
    [BoxGroup]
    public Texture HeadSkinTextureNormal;
    
    [BoxGroup]
    public Texture BodySkinTextureAlbedo;
    
    [BoxGroup]
    public Texture BodySkinTextureRDSO;
    
    [BoxGroup]
    public Texture BodySkinTextureNormal;

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
