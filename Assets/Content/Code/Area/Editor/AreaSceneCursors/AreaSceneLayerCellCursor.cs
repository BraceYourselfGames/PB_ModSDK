using UnityEngine;

namespace Area
{
    sealed class AreaSceneLayerCellCursor : AreaSceneCursor
    {
        public int ID { get; }

        public void CheckSetup (Transform parent)
        {
            holder = parent;
            LoadCellCursorObject ();
        }

        public void OnDestroy ()
        {
            if (cellCursor != null)
            {
                Object.DestroyImmediate (cellCursor);
            }
            if (cellCursorMaterial != null)
            {
                Object.DestroyImmediate (cellCursorMaterial);
            }
            if (cellCursorEmptyMaterial != null)
            {
                Object.DestroyImmediate (cellCursorEmptyMaterial);
            }
        }

        public void Update (float timeDelta, AreaSceneCursor.UpdateCursor update)
        {
            update (cellCursor, cellTargetPosition, ref cellSmoothPosition, ref cellVelocityPosition);
        }

        public void Show (AreaVolumePoint spot, RaycastHit hitInfo)
        {
            if (cellCursor == null)
            {
                return;
            }
            if (spot == null)
            {
                return;
            }
            if (!cellCursor.activeSelf)
            {
                cellCursor.SetActive (true);
            }

            var reset = bb.lastCellHovered == null;
            ref var lastCellEmpty = ref bb.cellEmpty;

            cellTargetPosition = spot.instancePosition;
            if (AreaSceneHelper.IsFreeSpace (spot))
            {
                if (lastCellEmpty && !reset)
                {
                    return;
                }
                SetMaterialFreeSpace ();
                lastCellEmpty = true;
                return;
            }

            if (!lastCellEmpty && !reset)
            {
                return;
            }
            SetMaterialOccupied ();
            lastCellEmpty = false;
        }

        public void Hide ()
        {
            if (cellCursor == null)
            {
                return;
            }
            if (!cellCursor.activeSelf)
            {
                return;
            }
            cellCursor.SetActive (false);
        }

        void LoadCellCursorObject ()
        {
            if (cellCursor != null)
            {
                return;
            }

            var cellCursorTransform = holder.Find (cellCursorName);
            if (cellCursorTransform != null)
            {
                cellCursor = cellCursorTransform.gameObject;
                LoadMaterials ();
                return;
            }

            var prefab = Resources.Load<GameObject> (prefabName);
            if (prefab == null)
            {
                Debug.LogWarning ("Failed to load resource for pointer cursor: " + prefabName);
                return;
            }

            cellCursor = Object.Instantiate (prefab, holder);
            cellCursor.name = prefab.name;
            cellCursor.SetActive (false);

            LoadMaterials ();
        }

        void LoadMaterials ()
        {
            var mr = cellCursor.GetComponentInChildren<MeshRenderer> ();
            if (mr == null)
            {
                return;
            }

            if (cellCursorMaterial == null)
            {
                cellCursorMaterial = new Material (mr.sharedMaterial);
                cellCursorMaterial.SetColor (tintColorShaderID, Color.cyan);
            }
            if (cellCursorEmptyMaterial == null)
            {
                cellCursorEmptyMaterial = new Material (mr.sharedMaterial);
                cellCursorEmptyMaterial.SetColor (tintColorShaderID, Color.yellow);
            }
            mr.sharedMaterial = cellCursorEmptyMaterial;
        }

        void SetMaterialFreeSpace ()
        {
            if (cellCursorEmptyMaterial == null)
            {
                return;
            }

            var mr = cellCursor.GetComponentInChildren<MeshRenderer> ();
            if (mr != null)
            {
                mr.sharedMaterial = cellCursorEmptyMaterial;
            }
        }

        void SetMaterialOccupied ()
        {
            if (cellCursorMaterial == null)
            {
                return;
            }
            var mr = cellCursor.GetComponentInChildren<MeshRenderer> ();
            if (mr != null)
            {
                mr.sharedMaterial = cellCursorMaterial;
            }
        }

        public AreaSceneLayerCellCursor (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            ID = bb.gizmos.cursor.RegisterCursor (this);
        }

        readonly AreaSceneBlackboard bb;

        Transform holder;

        GameObject cellCursor;
        Material cellCursorMaterial;
        Material cellCursorEmptyMaterial;
        Vector3 cellTargetPosition;
        Vector3 cellSmoothPosition;
        Vector3 cellVelocityPosition;

        const string cellCursorName = "block_cell";
        const string prefabName = "Editor/AreaVisuals/" + cellCursorName;

        static readonly int tintColorShaderID = Shader.PropertyToID ("_TintColor");
    }
}
