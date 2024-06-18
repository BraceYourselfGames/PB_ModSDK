using UnityEngine;
using UnityEditor;

namespace Area
{
    [CustomEditor (typeof (TilesetMultiBlockDefinition))]
    public class TilesetMultiBlockDefinitionInspector : Editor
    {
        private GameObject[] visualPoints;
        private GameObject visualHolder;
        private GameObject visualPrefab;

        private Material materialVolumeEmpty = null;
        private Material materialVolumePivot = null;
        private Material materialVolumeFull = null;
        private Material materialPrefab = null;

        private int drawRotation = 0;
        private bool redrawRequired = true;
        private bool drawPrefab = true;
        private Vector3 drawPositionShift = new Vector3 (0f, 100f, 0f);

        void OnEnable ()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        void OnDisable ()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnDestroy ()
        {
            if (visualHolder != null)
                DestroyImmediate (visualHolder);
        }

        public override void OnInspectorGUI ()
        {
            TilesetMultiBlockDefinition t = target as TilesetMultiBlockDefinition;

            Vector3Int boundsOld = t.bounds;
            Vector3Int pivotPositionOld = t.pivotPosition;
            int drawRotationOld = drawRotation;
            bool drawPrefabOld = drawPrefab;

            GUILayout.BeginVertical ("Box");
            if (GUILayout.Button ("Focus on visualization"))
            {
                if (visualHolder != null && SceneView.lastActiveSceneView != null)
                    SceneView.lastActiveSceneView.LookAt (visualHolder.transform.position);
            }
            drawRotation = EditorGUILayout.IntField ("Preview rotation", drawRotation);
            drawPrefab = EditorGUILayout.Toggle ("Preview prefab", drawPrefab);
            GUILayout.EndVertical ();

            DrawDefaultInspector ();

            if (boundsOld != t.bounds || pivotPositionOld != t.pivotPosition || drawRotationOld != drawRotation || drawPrefabOld != drawPrefab)
            {
                drawRotation = (int)drawRotation % 4;
                redrawRequired = true;
            }

            if (redrawRequired)
            {
                t.UpdateVolume ();
                VisualizeAllVolume (t);
            }
        }

        public void VisualizeAllVolume (TilesetMultiBlockDefinition t)
        {
            redrawRequired = false;

            if (visualHolder == null)
                visualHolder = GameObject.Find ("TMBD_VisualHolder");
            if (visualHolder == null)
            {
                visualHolder = new GameObject ("TMBD_VisualHolder");
                visualHolder.hideFlags = HideFlags.HideAndDontSave;
            }

            UtilityGameObjects.ClearChildren (visualHolder.transform);
            visualHolder.transform.position = drawPositionShift;

            TilesetMultiBlockDefinition.TransformedData data = t.GetTransformedData (drawRotation);

            for (int i = 0; i < data.volumeTransformed.Length; ++i)
            {
                Vector3Int pointPosition = AreaUtility.GetVolumePositionFromIndex (i, data.boundsTransformed, log: false);
                bool full = data.volumeTransformed[i];

                GameObject pointObject = GameObject.CreatePrimitive (PrimitiveType.Cube);
                pointObject.hideFlags = HideFlags.HideAndDontSave;

                MeshRenderer mr = pointObject.GetComponent<MeshRenderer> ();
                mr.sharedMaterial = i == data.pivotIndexTransformed ? GetMaterial (ref materialVolumePivot, new Color (1f, 0.5f, 0.2f, 0.75f), 2) : (full ? GetMaterial (ref materialVolumeFull, new Color (1f, 1f, 1f, 0.66f), 2) : GetMaterial (ref materialVolumeEmpty, Color.gray, 0));
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                pointObject.transform.parent = visualHolder.transform;
                pointObject.transform.localScale = Vector3.one * (full ? 2.5f : 1f);
                pointObject.transform.localPosition = new Vector3 (pointPosition.x, -pointPosition.y, pointPosition.z) * TilesetUtility.blockAssetSize;
                pointObject.name = i.ToString ();

                if (visualPoints == null) visualPoints = new GameObject[data.volumeTransformed.Length];
                if (i > 0 && i < visualPoints.Length)
                {
                    if (visualPoints[i] != null)
                    {
                        GameObject volumeObjectOld = visualPoints[i];
                        DestroyImmediate (volumeObjectOld);
                    }
                    visualPoints[i] = pointObject;
                }
            }

            if (drawPrefab && t.prefab != null)
            {
                if (visualPrefab == null)
                    visualPrefab = GameObject.Find ("TMBD_VisualPrefab");
                if (visualPrefab == null)
                {
                    visualPrefab = Instantiate (t.prefab);
                    visualPrefab.name = "TMBD_VisualPrefab";
                    visualPrefab.hideFlags = HideFlags.HideAndDontSave;
                    visualPrefab.transform.parent = visualHolder.transform;
                    visualPrefab.transform.localRotation = Quaternion.identity;
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

                visualPrefab.transform.localPosition = new Vector3 (data.pivotPositionTransformed.x, -data.pivotPositionTransformed.y, data.pivotPositionTransformed.z) * TilesetUtility.blockAssetSize;
                visualPrefab.transform.localRotation = Quaternion.Euler (new Vector3 (0f, -90f * drawRotation, 0f));
            }
            else
            {
                if (visualPrefab != null)
                    DestroyImmediate (visualPrefab);
            }
        }



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
                    TilesetMultiBlockDefinition t = target as TilesetMultiBlockDefinition;
                    t.volume[index] = !t.volume[index];
                    redrawRequired = true;
                    VisualizeAllVolume (t);
                    EditorUtility.SetDirty (t);
                }
            }
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
