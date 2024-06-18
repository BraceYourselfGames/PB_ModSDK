using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Area
{
    [CustomEditor (typeof (TilesetEdgeBlockDefinition))]
    public class TilesetEdgeBlockDefinitionInspector : Editor
    {
        private GameObject[] visualPoints;
        private GameObject visualHolder;
        private GameObject visualPrefab;

        private Material materialVolumeEmpty = null;
        private Material materialVolumeFullDestroyed = null;
        private Material materialVolumeFull = null;
        private Material materialVolumeEmptyOrDestroyed = null;
        private Material materialVolumeIrrelevant = null;
        private Material materialPrefab = null;

        private bool redrawRequired = true;
        private bool drawPrefab = true;
        private Vector3 drawPositionShift = new Vector3 (0f, 100f, 0f);

        private int rotation = 0;
        private int plane = 0;
        private int configurationSelected = 0;

        /*
        void OnEnable ()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        void OnDisable ()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }
        */

        private void OnDestroy ()
        {
            if (visualHolder != null)
                DestroyImmediate (visualHolder);
        }

        public override void OnInspectorGUI ()
        {
            TilesetEdgeBlockDefinition t = target as TilesetEdgeBlockDefinition;

            if (t.configurations == null)
                t.configurations = new List<AreaVolumePointConfiguration> ();

            bool drawPrefabOld = drawPrefab;
            int planeOld = plane;
            int rotationOld = rotation;
            int instanceIDOld = t.prefab.GetInstanceID ();
            int configurationSelectionOld = configurationSelected;

            GUILayout.BeginVertical ("Box");
            if (GUILayout.Button ("Focus on visualization"))
            {
                if (visualHolder != null && SceneView.lastActiveSceneView != null)
                    SceneView.lastActiveSceneView.LookAt (visualHolder.transform.position);
            }

            drawPrefab = EditorGUILayout.Toggle ("Preview prefab", drawPrefab);
            plane = EditorGUILayout.IntSlider ("Plane", plane, 0, 5);
            rotation = EditorGUILayout.IntSlider ("Rotation", rotation, 0, 3);
            GUILayout.EndVertical ();

            GUILayout.BeginVertical ("Box");

            if (configurationSelected >= t.configurations.Count)
                configurationSelected = t.configurations.Count - 1;

            if (t.configurations.Count > 0)
            {
                configurationSelected = Mathf.Clamp (configurationSelected, 0, t.configurations.Count);

                GUILayout.BeginVertical ("Box");
                GUILayout.Label ("Selected configuration: " + configurationSelected);
                GUILayout.Label (t.configurations[configurationSelected].ToStringShort () + " (original)", EditorStyles.miniLabel);
                GUILayout.Label (t.configurations[configurationSelected].Transform (plane, rotation).ToStringShort () + " (transformed)", EditorStyles.miniLabel);
                GUILayout.EndVertical ();

                for (int i = 0; i < t.configurations.Count; ++i)
                {
                    AreaVolumePointConfiguration configuration = t.configurations[i];
                    EditorGUILayout.BeginHorizontal ();
                    if (GUILayout.Button ("Use", EditorStyles.miniButton, GUILayout.Width (40f)))
                        configurationSelected = i;
                    GUILayout.Label (configuration.ToStringShort (), EditorStyles.miniLabel);
                    EditorGUILayout.EndHorizontal ();
                }
            }
            if (GUILayout.Button ("Add", EditorStyles.miniButton, GUILayout.Width (40f)))
            {
                t.configurations.Add (t.configurations.Count > 0 ? t.configurations[t.configurations.Count - 1] : new AreaVolumePointConfiguration ());
                configurationSelected = t.configurations.Count - 1;
            }

            GUILayout.EndVertical ();

            DrawDefaultInspector ();

            if (drawPrefabOld != drawPrefab || planeOld != plane || rotationOld != rotation || instanceIDOld != t.prefab.GetInstanceID () || configurationSelectionOld != configurationSelected)
            {
                redrawRequired = true;
            }

            if (redrawRequired)
            {
                VisualizeAllVolume (t);
            }
        }

        public void VisualizeAllVolume (TilesetEdgeBlockDefinition t)
        {
            redrawRequired = false;

            if (visualHolder == null)
                visualHolder = GameObject.Find ("TEBD_VisualHolder");
            if (visualHolder == null)
            {
                visualHolder = new GameObject ("TEBD_VisualHolder");
                visualHolder.hideFlags = HideFlags.HideAndDontSave;
            }

            if (t.configurations == null || t.configurations.Count == 0 || t.configurations.Count <= configurationSelected)
                return;

            UtilityGameObjects.ClearChildren (visualHolder.transform);
            visualHolder.transform.position = drawPositionShift;

            AreaVolumePointConfiguration configurationTransformed = t.configurations[configurationSelected].Transform (plane, rotation);

            for (int i = 0; i < 8; ++i)
            {
                Vector3Int pointPosition = TilesetUtility.GetVolumePositionFromIndexInBlock (i);
                Debug.Log (i + " | " + pointPosition);

                AreaVolumePointState pointState = configurationTransformed.GetCornerState (i);

                GameObject pointObject = GameObject.CreatePrimitive (PrimitiveType.Cube);
                pointObject.hideFlags = HideFlags.HideAndDontSave;

                MeshRenderer mr = pointObject.GetComponent<MeshRenderer> ();
                mr.sharedMaterial = GetMaterialFromPointState (pointState);
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                pointObject.transform.parent = visualHolder.transform;
                pointObject.transform.localScale = Vector3.one * GetScaleFromPointState (pointState);
                pointObject.transform.localPosition = new Vector3 (pointPosition.x, -pointPosition.y, pointPosition.z) * TilesetUtility.blockAssetSize;
                pointObject.name = i.ToString ();

                if (visualPoints == null)
                    visualPoints = new GameObject[8];

                if (visualPoints[i] != null)
                {
                    GameObject volumeObjectOld = visualPoints[i];
                    DestroyImmediate (volumeObjectOld);
                }
                visualPoints[i] = pointObject;
            }

            if (drawPrefab && t.prefab != null)
            {
                if (visualPrefab == null)
                    visualPrefab = GameObject.Find ("TEBD_VisualPrefab");
                if (visualPrefab == null)
                {
                    visualPrefab = Instantiate (t.prefab);
                    visualPrefab.name = t.prefab.name;
                    visualPrefab.hideFlags = HideFlags.HideAndDontSave;
                    visualPrefab.transform.parent = visualHolder.transform;
                    visualPrefab.transform.localScale = Vector3.one;

                    MeshRenderer[] mrs = visualPrefab.GetComponentsInChildren<MeshRenderer> ();
                    for (int i = 0; i < mrs.Length; ++i)
                    {
                        mrs[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        Material[] sharedMaterialsCopy = mrs[i].sharedMaterials;
                        for (int m = 0; m < sharedMaterialsCopy.Length; ++m)
                        {
                            sharedMaterialsCopy[m] = GetMaterial (ref materialPrefab, new Color (0.25f, 0.5f, 1.0f, 0.9f), 0);
                        }
                        mrs[i].sharedMaterials = sharedMaterialsCopy;
                    }
                }

                visualPrefab.transform.localPosition = new Vector3 (1f, -1f, 1f) * (TilesetUtility.blockAssetSize / 2f);
                visualPrefab.transform.localRotation = Quaternion.identity; // use -90f as rotation basis
                if (plane == 0)
                {
                    visualPrefab.transform.Rotate (0f, -90f * rotation, 0f);
                }
                if (plane == 1)
                {
                    visualPrefab.transform.Rotate (-90f, 0f, 0f);
                    visualPrefab.transform.Rotate (0f, -90f * rotation, 0f);

                }
                if (plane == 2)
                {
                    visualPrefab.transform.Rotate (0f, 0f, -90f);
                    visualPrefab.transform.Rotate (0f, -90f * rotation, 0f);
                }
                if (plane == 3)
                {
                    visualPrefab.transform.Rotate (90f, 0f, 0f);
                    visualPrefab.transform.Rotate (0f, -90f * rotation, 0f);
                }
                if (plane == 4)
                {
                    visualPrefab.transform.Rotate (0f, 0f, 90f);
                    visualPrefab.transform.Rotate (0f, -90f * rotation, 0f);
                }
                if (plane == 5)
                {
                    visualPrefab.transform.Rotate (-180f, 0f, 0f);
                    visualPrefab.transform.Rotate (0f, -90f * rotation, 0f);
                }
            }
            else
            {
                if (visualPrefab != null)
                    DestroyImmediate (visualPrefab);
            }

            SceneView.RepaintAll ();
        }


        /*
        private void OnSceneGUI (SceneView sceneView)
        {
            if (visualHolder == null || Event.current.type != EventType.MouseDown || !Event.current.alt)
                return;

            Event.current.Use ();
            Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast (worldRay, out hitInfo))
            {
                if (hitInfo.transform.parent != visualHolder.transform)
                    return;

                string name = hitInfo.transform.name;
                int index = 0;
                if (int.TryParse (name, out index))
                {
                    TilesetEdgeBlockDefinition t = target as TilesetEdgeBlockDefinition;
                    if (t.configurations == null || t.configurations.Count <= configurationSelected)
                        return;

                    AreaVolumePointState pointState = t.configurations[configurationSelected].Transform (plane, rotation).GetCornerState (index);
                    int indexOriginal = AreaUtility.GetOriginalCornerIndexOfTransformedConfiguration (index, plane, rotation);

                    if (pointState == AreaVolumePointState.Empty)
                        t.configurations[configurationSelected] = AreaUtility.ReplaceConfigurationCorner (t.configurations[configurationSelected], indexOriginal, AreaVolumePointState.FullDestroyed);
                    else if (pointState == AreaVolumePointState.FullDestroyed)
                        t.configurations[configurationSelected] = AreaUtility.ReplaceConfigurationCorner (t.configurations[configurationSelected], indexOriginal, AreaVolumePointState.Full);
                    else if (pointState == AreaVolumePointState.Full)
                        t.configurations[configurationSelected] = AreaUtility.ReplaceConfigurationCorner (t.configurations[configurationSelected], indexOriginal, AreaVolumePointState.EmptyOrDestroyed);
                    else if (pointState == AreaVolumePointState.EmptyOrDestroyed)
                        t.configurations[configurationSelected] = AreaUtility.ReplaceConfigurationCorner (t.configurations[configurationSelected], indexOriginal, AreaVolumePointState.Irrelevant);
                    else if (pointState == AreaVolumePointState.Irrelevant)
                        t.configurations[configurationSelected] = AreaUtility.ReplaceConfigurationCorner (t.configurations[configurationSelected], indexOriginal, AreaVolumePointState.Empty);

                    redrawRequired = true;
                    VisualizeAllVolume (t);
                    EditorUtility.SetDirty (t);
                }
            }
        }
        */


        private Material GetMaterialFromPointState (AreaVolumePointState state)
        {
            if (state == AreaVolumePointState.Empty)
                return GetMaterial (ref materialVolumeEmpty, Color.gray, 0);
            else if (state == AreaVolumePointState.FullDestroyed)
                return GetMaterial (ref materialVolumeFullDestroyed, new Color (1f, 0.5f, 0.2f, 0.75f), 2);
            else if (state == AreaVolumePointState.Full)
                return GetMaterial (ref materialVolumeFull, new Color (1f, 1f, 1f, 0.66f), 2);
            else
                return GetMaterial (ref materialVolumeIrrelevant, new Color (0.5f, 1f, 0.2f, 0.33f), 2);
        }

        private float GetScaleFromPointState (AreaVolumePointState state)
        {
            if (state == AreaVolumePointState.Full)
                return 1f;
            else
                return 0.5f;
        }

        private static Material GetMaterial (ref Material material, Color color, int mode)
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
                    material.renderQueue = 3000;
                }
                material.SetColor ("_Color", color);
                return material;
            }
            return material;
        }
    }

}