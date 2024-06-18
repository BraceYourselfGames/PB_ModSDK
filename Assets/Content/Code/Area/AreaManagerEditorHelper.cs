#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Area
{
    public static class AreaManagerEditorHelper
    {
        public static void Flush ()
        {
            materialEditorHelperBase = null;
            materialVolumeEmpty = null;
            materialVolumeEmptyHintedA = null;
            materialVolumeEmptyHintedB = null;
            materialVolumeFull = null;
            materialVolumeFullHintedA = null;
            materialVolumeFullHintedB = null;
            materialSpot = null;
            meshCube = null;
        }

        private static Material materialEditorHelperBase;
        private static Material materialVolumeEmpty;
        private static Material materialVolumeEmptyHintedA;
        private static Material materialVolumeEmptyHintedB;
        private static Material materialVolumeFull;
        private static Material materialVolumeFullHintedA;
        private static Material materialVolumeFullHintedB;
        private static Material materialMultiblockValid;
        private static Material materialMultiblockInvalid;
        private static Material materialSpot;
        private static Material materialSpawnFriendly;
        private static Material materialSpawnEnemy;

        private static Color colorVolumeEmpty = new Color32 (146, 146, 146, 255);
        private static Color colorVolumeEmptyHintedA = new Color32 (130, 130, 130, 255);
        private static Color colorVolumeEmptyHintedB = new Color32 (211, 74, 33, 255);
        private static Color colorVolumeFull = new Color32 (182, 208, 83, 255);
        private static Color colorVolumeFullHintedA = new Color32 (123, 208, 83, 255);
        private static Color colorVolumeFullHintedB = new Color32 (211, 74, 33, 255);
        private static Color colorSpot = new Color32 (208, 83, 83, 255);

        private static Color colorMultiblockValid = new Color32 (123, 208, 83, 255);
        private static Color colorMultiblockInvalid = new Color32 (208, 83, 83, 255);

        private static Color colorSpawnFriendly = new Color32 (83, 182, 208, 255);
        private static Color colorSpawnEnemy = new Color32 (208, 83, 83, 255);

        public static Material GetMaterialVolumeEmpty ()
        { return GetMaterialStandard (ref materialVolumeEmpty, colorVolumeEmpty, 0); }

        public static Material GetMaterialVolumeEmptyHintedA ()
        { return GetMaterialStandard (ref materialVolumeEmptyHintedA, colorVolumeEmptyHintedA, 0); }

        public static Material GetMaterialVolumeEmptyHintedB ()
        { return GetMaterialStandard (ref materialVolumeEmptyHintedB, colorVolumeEmptyHintedB, 0); }

        public static Material GetMaterialVolumeFull ()
        { return GetMaterialStandard (ref materialVolumeFull, colorVolumeFull, 0); }

        public static Material GetMaterialVolumeFullHintedA ()
        { return GetMaterialStandard (ref materialVolumeFullHintedA, colorVolumeFullHintedA, 0); }

        public static Material GetMaterialVolumeFullHintedB ()
        { return GetMaterialStandard (ref materialVolumeFullHintedB, colorVolumeFullHintedB, 0); }

        public static Material GetMaterialSpot ()
        { return GetMaterialStandard (ref materialSpot, colorSpot, 0); }

        public static Material GetMaterialMultiblockValid ()
        { return GetMaterialStandard (ref materialMultiblockValid, colorMultiblockValid, 0); }

        public static Material GetMaterialMultiblockInvalid ()
        { return GetMaterialStandard (ref materialMultiblockInvalid, colorMultiblockInvalid, 0); }

        public static Material GetMaterialSpawnFriendly ()
        { return GetMaterialStandard (ref materialSpawnFriendly, colorSpawnFriendly.WithAlpha (0.8f), 2); }

        public static Material GetMaterialSpawnEnemy ()
        { return GetMaterialStandard (ref materialSpawnEnemy, colorSpawnEnemy.WithAlpha (0.8f), 2); }

        private static Material GetMaterialStandard (ref Material material, Color color, int mode)
        {
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
                    material.EnableKeyword ("PROCEDURAL_INSTANCING_ON");
                    material.renderQueue = 3000;
                }
                material.SetColor ("_Color", color);
                return material;
            }
            return material;
        }

        private static Material GetMaterialEditorHelper (ref Material material, Color color, float opacity, float push, float emission)
        {
            if (material == null)
            {
                material = new Material (GetMaterialEditorHelperBase ());
                material.SetColor ("_Color", color);
                material.SetFloat ("_Opacity", opacity);
                material.SetFloat ("_Push", push);
                material.SetFloat ("_Emission", emission);
                return material;
            }
            return material;
        }

        private static Material GetMaterialEditorHelperBase ()
        {
            if (materialEditorHelperBase == null)
                materialEditorHelperBase = new Material (Shader.Find ("Editor/HelperDeferred"));
            return materialEditorHelperBase;
        }

        private static Mesh meshCube;

        public static Mesh GetMeshCube ()
        {
            if (meshCube == null)
                meshCube = AssetDatabase.LoadAssetAtPath ("Assets/Content/Objects/EditorAssets/cube.fbx", typeof (Mesh)) as Mesh;
            return meshCube;
        }
    }
}

#endif
