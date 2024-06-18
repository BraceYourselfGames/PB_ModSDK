using UnityEngine;

namespace Area
{
    using Scene;

    sealed class AreaSceneVolumeSelectionCursor : AreaSceneCursor
    {
        public bool showGlow = true;

        public void CheckSetup (Transform parent)
        {
            holder = parent;
            LoadMaterials ();
            LoadSelectionObject ();
            LoadSelectionCube ();
            LoadGlowObject ();
        }

        public void OnDestroy ()
        {
            DestroyObjects ();
            DestroyMaterials ();
        }

        public void Update (float timeDelta, AreaSceneCursor.UpdateCursor update)
        {
            if (!update (rcSelectionObject, rcSelectionTargetPosition, ref rcSelectionSmoothPosition, ref rcSelectionVelocityPosition))
            {
                return;
            }
            if (rcGlowObject != null)
            {
                update (rcGlowObject, rcGlowTargetPosition, ref rcGlowSmoothPosition, ref rcGlowVelocityPosition);
            }
        }

        public void Show (AreaVolumePoint point, RaycastHit hitInfo)
        {
            if (rcSelectionObject != null)
            {
                if (!rcSelectionObject.activeSelf)
                {
                    rcSelectionObject.SetActive (true);
                }
                rcSelectionTargetPosition = point.pointPositionLocal;
            }

            if (rcGlowObject == null)
            {
                return;
            }
            if (point == null || !showGlow)
            {
                if (rcGlowObject.activeSelf)
                {
                    rcGlowObject.SetActive (false);
                }
                return;
            }

            var (_, glowPoint) = AreaSceneHelper.GetNeighborFromDirection (point, hitInfo.normal);
            if (glowPoint == null)
            {
                if (rcGlowObject.activeSelf)
                {
                    rcGlowObject.SetActive (false);
                }
                return;
            }

            rcGlowTargetPosition = glowPoint.pointPositionLocal;
            if (!rcGlowObject.activeSelf)
            {
                rcGlowObject.SetActive (true);
            }
        }

        public void Hide ()
        {
            if (rcSelectionObject != null && rcSelectionObject.activeSelf)
            {
                rcSelectionObject.SetActive (false);
            }
            if (rcGlowObject != null && rcGlowObject.activeSelf)
            {
                rcGlowObject.SetActive (false);
            }
        }

        void LoadMaterials ()
        {
            if (rcSelectionMaterial == null)
            {
                var res = Resources.Load<Material> ("Content/Debug/AreaSelection");
                rcSelectionMaterial = new Material (res);
            }
            if (rcGlowMaterial == null)
            {
                var res = Resources.Load<Material> ("Content/Debug/AreaGlow");
                rcGlowMaterial = new Material (res);
            }
            if (rcSelectionCubeMaterial == null)
            {
                var mat = Resources.Load<Material> ("Content/Debug/AreaCursor");
                if (mat != null)
                {
                    rcSelectionCubeMaterial = new Material (mat)
                    {
                        color = Color.red,
                    };
                }
            }
        }

        void LoadSelectionObject ()
        {
            if (rcSelectionObject != null)
            {
                return;
            }

            var rcSelectionTransform = holder.Find (selectionCellName);
            if (rcSelectionTransform != null)
            {
                rcSelectionObject = rcSelectionTransform.gameObject;
                return;
            }

            var prefab = Resources.Load<GameObject> (prefabName);
            if (prefab == null)
            {
                Debug.LogWarning ("Failed to load resource for selection object: " + prefabName);
                return;
            }

            rcSelectionObject = Object.Instantiate (prefab, holder);
            rcSelectionObject.transform.localScale = Vector3.one * 1.01f;
            rcSelectionObject.name = prefab.name;
            // rcSelectionObject.hideFlags = HideFlags.HideInHierarchy;
            var mr = rcSelectionObject.GetComponentInChildren<MeshRenderer> ();
            if (mr != null && rcSelectionMaterial != null)
            {
                mr.sharedMaterial = rcSelectionMaterial;
            }
            rcSelectionObject.SetActive (false);
        }

        void LoadSelectionCube ()
        {
            if (rcSelectionObject == null)
            {
                return;
            }
            if (rcSelectionCube != null)
            {
                return;
            }

            var rcSelectionCubeTransform = rcSelectionObject.transform.Find (selectionCubeName);
            if (rcSelectionCubeTransform == null)
            {
                rcSelectionCube = PrimitiveHelper.CreatePrimitive (PrimitiveType.Cube, false);
                rcSelectionCube.name = selectionCubeName;
                rcSelectionCube.transform.parent = rcSelectionObject.transform;
                rcSelectionCube.transform.localScale = Vector3.one * selectionCubeSize;
                rcSelectionCube.transform.localPosition = Vector3.zero;
                var mr = rcSelectionCube.GetComponent<MeshRenderer> ();
                if (mr != null && rcSelectionCubeMaterial != null)
                {
                    mr.sharedMaterial = rcSelectionCubeMaterial;
                }
            }
            else
            {
                rcSelectionCube = rcSelectionCubeTransform.gameObject;
            }
        }

        void LoadGlowObject ()
        {
            if (rcGlowObject != null)
            {
                return;
            }

            var rcGlowTransform = holder.Find (selectionGlowName);
            if (rcGlowTransform != null)
            {
                rcGlowObject = rcGlowTransform.gameObject;
            }
            else
            {
                rcGlowObject = PrimitiveHelper.CreatePrimitive (PrimitiveType.Quad, false);
                rcGlowObject.transform.localScale = Vector3.one * 6f;
                rcGlowObject.transform.parent = holder;
                rcGlowObject.name = selectionGlowName;
                // rcGlowObject.hideFlags = HideFlags.HideInHierarchy;
                var mr = rcGlowObject.GetComponent<MeshRenderer> ();
                if (mr != null && rcGlowMaterial != null)
                {
                    mr.sharedMaterial = rcGlowMaterial;
                }
                rcGlowObject.SetActive (false);
            }
        }

        void DestroyObjects ()
        {
            if (rcSelectionCube != null)
            {
                Object.DestroyImmediate (rcSelectionCube);
            }
            if (rcSelectionObject != null)
            {
                Object.DestroyImmediate (rcSelectionObject);
            }
            if (rcGlowObject != null)
            {
                Object.DestroyImmediate (rcGlowObject);
            }
        }

        void DestroyMaterials ()
        {
            if (rcSelectionMaterial != null)
            {
                Object.DestroyImmediate (rcSelectionMaterial);
            }
            if (rcGlowMaterial != null)
            {
                Object.DestroyImmediate (rcGlowMaterial);
            }
            if (rcSelectionCubeMaterial != null)
            {
                Object.DestroyImmediate (rcSelectionCubeMaterial);
            }
        }

        Transform holder;
        Material rcSelectionMaterial;
        Material rcGlowMaterial;
        Material rcSelectionCubeMaterial;
        GameObject rcSelectionObject;
        GameObject rcSelectionCube;
        GameObject rcGlowObject;

        Vector3 rcSelectionTargetPosition;
        Vector3 rcSelectionSmoothPosition;
        Vector3 rcSelectionVelocityPosition;

        Vector3 rcGlowTargetPosition;
        Vector3 rcGlowSmoothPosition;
        Vector3 rcGlowVelocityPosition;

        const string selectionCellName = "selection_cell";
        const string prefabName = "Content/Debug/" + selectionCellName;
        const string selectionCubeName = "selection_cube";
        const string selectionGlowName = "selection_glow";

        const float selectionCubeSize = WorldSpace.BlockSize * 0.83f;
    }
}
