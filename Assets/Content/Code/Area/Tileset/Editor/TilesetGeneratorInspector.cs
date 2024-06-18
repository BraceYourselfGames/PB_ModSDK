using UnityEngine;
using UnityEditor;

namespace Area
{
    [CustomEditor (typeof (TilesetGenerator))]
    public class TilesetGeneratorInspector : Editor
    {
        bool foldoutConfigurations = false;
        //bool foldoutVC = false;
        bool foldoutExport = true;

        public override void OnInspectorGUI ()
        {
            EditorGUILayout.HelpBox ("Do not use this generator for anything but reference - use Processor component below", MessageType.Warning);
            
            TilesetGenerator t = target as TilesetGenerator;

            if (t.configurationsByte == null || t.configurationsByte.Count == 0)
                t.GenerateConfigurations ();

            string folderTilesetImport = EditorGUILayout.TextField ("Tileset import folder", t.folderTilesetImport);
            string folderTilesetExport = EditorGUILayout.TextField ("Tileset export folder", t.folderTilesetExport);

            GUILayout.BeginHorizontal ();
            bool exportBlocks = t.exportBlocks;
            bool exportDamage = t.exportDamage;
            bool exportMultiblocks = t.exportMultiblocks;
            bool logMaterialReplacement = t.logMaterialReplacement;
            GUILayout.EndHorizontal ();

            foldoutExport = EditorGUILayout.Foldout (foldoutExport, "Export options");
            if (foldoutExport)
            {
                // EditorGUIUtility.labelWidth = 70f;
                GUILayout.BeginVertical ("Box");

                exportBlocks = EditorGUILayout.Toggle ("Blocks", t.exportBlocks);
                exportDamage = EditorGUILayout.Toggle ("Damage", t.exportDamage);
                exportMultiblocks = EditorGUILayout.Toggle ("Multiblocks", t.exportMultiblocks);
                logMaterialReplacement = EditorGUILayout.Toggle ("Log material replacement", t.logMaterialReplacement);

                GUILayout.EndVertical ();
                // EditorGUIUtility.labelWidth = 150f;
            }

            if (GUI.changed)
            {
                Undo.RecordObject (t, "TPG changed");
                t.folderTilesetImport = folderTilesetImport;
                t.folderTilesetExport = folderTilesetExport;
                // t.vertexPropertiesDefault = new TilesetVertexProperties (huePrimary, saturationPrimary, brightnessPrimary, hueSecondary, saturationSecondary, brightnessSecondary, emissionIntensity, damageIntensity);
                t.exportBlocks = exportBlocks;
                t.exportDamage = exportDamage;
                t.exportMultiblocks = exportMultiblocks;
                t.logMaterialReplacement = logMaterialReplacement;
                GUI.changed = false;
            }

            if (GUILayout.Button ("Load assets"))
                t.LoadAssets ();
            UtilityCustomInspector.DrawList ("Blocks", t.assetsBlocks, DrawPrefabPreferences, null, false, false);
            UtilityCustomInspector.DrawList ("Damage", t.assetsDamage, DrawPrefabPreferences, null, false, false);
            UtilityCustomInspector.DrawList ("Multiblocks", t.assetsMultiblocks, DrawPrefabPreferences, null, false, false);

            GUILayout.BeginHorizontal ("Box");
            if (GUILayout.Button ("Rebuild everything"))
                t.RebuildEverything ();
            GUILayout.BeginVertical ();
            if (GUILayout.Button ("Instantiate and check"))
                t.InstantiateAndCheckModels ();
            if (GUILayout.Button ("Replace materials"))
                t.ReplaceMaterials ();
            if (GUILayout.Button ("Merge blocks"))
                t.MergeEverything ();
            if (GUILayout.Button ("Place lights"))
                t.GenerateLights ();
            if (GUILayout.Button ("Save blocks"))
                t.SaveEverything ();
            if (GUILayout.Button ("Place everything"))
                t.PlaceEverything ();
            GUILayout.EndVertical ();
            GUILayout.EndHorizontal ();

            if (GUILayout.Button ("Create light definition"))
                t.AddLightSourceDefinition ();

            GUILayout.BeginHorizontal ("Box");
            GUILayout.Label ("Export progress");
            GUILayout.FlexibleSpace ();
            if (t.exportProgress != 0f)
                GUILayout.Label ((t.exportProgress * 100f).ToString ("00") + "%");
            GUILayout.EndHorizontal ();

            foldoutConfigurations = EditorGUILayout.Foldout (foldoutConfigurations, "Configurations (" + t.configurationsByte.Count + ")");
            if (foldoutConfigurations)
            {
                for (int i = 0; i < t.configurationsByte.Count; ++i)
                {
                    GUILayout.BeginHorizontal ();
                    GUILayout.Label (i.ToString ("Index: 000") + " | Byte: " + t.configurationsByte[i].ToString ("000") + " | Int: " + TilesetUtility.GetIntFromConfiguration (TilesetUtility.GetConfigurationFromByte (t.configurationsByte[i])).ToString ("00000000"), EditorStyles.miniLabel);
                    GUILayout.EndHorizontal ();
                }
            }

            DrawDefaultInspector ();
        }

        private void DrawPrefabPreferences (TilesetGenerator.PrefabPreferences p)
        {
            if (p != null)
            {
                EditorGUILayout.BeginHorizontal ();
                p.load = EditorGUILayout.Toggle (p.load);
                GUILayout.FlexibleSpace ();
                if (p.prefab != null)
                    GUILayout.Label (p.prefab.name);
                else
                    GUILayout.Label ("Prefab is null");
                EditorGUILayout.EndHorizontal ();
            }
            else
                EditorGUILayout.HelpBox ("Object is null", MessageType.Warning);
        }
    }
}