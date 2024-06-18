using System.Collections.Generic;

using UnityEngine;

namespace Area
{
    sealed class AreaScenePointerCursor : AreaSceneCursor
    {
        public int standardMaterialID { get; private set; }

        public void CheckSetup (Transform parent)
        {
            holder = parent;
            LoadMaterials ();
            LoadObject ();
            SetMaterial (standardMaterialID);
        }

        public void OnDestroy ()
        {
            if (rcCursorObject != null)
            {
                Object.DestroyImmediate (rcCursorObject);
            }
            foreach (var mat in materials)
            {
                if (mat == null)
                {
                    continue;
                }
                Object.DestroyImmediate (mat);
            }
        }

        public void Update (float timeDelta, AreaSceneCursor.UpdateCursor update)
        {
            if (!update (rcCursorObject, rcCursorTargetPosition, ref rcCursorSmoothPosition, ref rcCursorVelocityPosition))
            {
                return;
            }

            rcCursorSmoothRotation = UtilityQuaternion.SmoothDamp (rcCursorSmoothRotation, rcCursorTargetRotation, ref rcCursorVelocityRotation, 0.05f, 100000f, timeDelta);
            rcCursorObject.transform.rotation = rcCursorSmoothRotation;
        }

        public void Show (AreaVolumePoint point, RaycastHit hitInfo)
        {
            if (rcCursorObject == null)
            {
                return;
            }

            if (!rcCursorObject.activeSelf)
            {
                rcCursorObject.SetActive (true);
            }

            var direction = hitInfo.normal;
            rcCursorTargetPosition = hitInfo.point + direction;
            rcCursorTargetRotation = Quaternion.LookRotation (direction);
        }

        public void Hide ()
        {
            if (rcCursorObject == null)
            {
                return;
            }
            if (!rcCursorObject.activeSelf)
            {
                return;
            }
            rcCursorObject.SetActive (false);
        }

        public int RegisterMaterial (Material material)
        {
            if (material == null)
            {
                return -1;
            }

            var crc = material.ComputeCRC ();
            for (var i = 0; i < materials.Count; i += 1)
            {
                if (crc == materials[i].ComputeCRC ())
                {
                    return i;
                }
            }

            var materialID = materials.Count;
            materials.Add (material);
            return materialID;
        }

        public int GetMaterialID () => currentMaterialID;

        public void SetMaterial (int materialID)
        {
            if (!materialID.IsValidIndex (materials))
            {
                return;
            }

            var mat = materials[materialID];
            if (mat == null)
            {
                return;
            }

            var mr = rcCursorObject.GetComponentInChildren<MeshRenderer> ();
            if (mr == null)
            {
                return;
            }

            mr.sharedMaterial = mat;
            currentMaterialID = materialID;
        }

        void LoadMaterials ()
        {
            if (materials.Count != 0)
            {
                return;
            }

            var res = Resources.Load<Material> ("Content/Debug/AreaCursor");
            var mat = new Material (res);
            standardMaterialID = RegisterMaterial(mat);
        }

        void LoadObject ()
        {
            if (rcCursorObject != null)
            {
                return;
            }

            var rcCursorTransform = holder.Find (selectionPointerName);
            if (rcCursorTransform != null)
            {
                rcCursorObject = rcCursorTransform.gameObject;
                return;
            }

            var prefab = Resources.Load<GameObject> (prefabName);
            if (prefab == null)
            {
                Debug.LogWarning ("Failed to load resource for pointer cursor: " + prefabName);
                return;
            }

            rcCursorObject = Object.Instantiate (prefab, holder);
            rcCursorObject.name = prefab.name;
            // rcCursorObject.hideFlags = HideFlags.HideInHierarchy;
            var mr = rcCursorObject.GetComponentInChildren<MeshRenderer> ();
            if (mr != null && materials[standardMaterialID] != null)
            {
                mr.sharedMaterial = materials[standardMaterialID];
            }

            var child = new GameObject ("Light");
            child.transform.parent = rcCursorObject.transform;

            var light = child.AddComponent<Light> ();
            light.gameObject.hideFlags = HideFlags.HideInHierarchy;
            light.transform.localPosition = new Vector3 (0f, 0f, 1f);
            light.color = Color.Lerp (Color.cyan, Color.blue, 0.25f);
            light.intensity = 1f;
            light.shadows = LightShadows.Soft;
        }

        readonly List<Material> materials = new List<Material> ();
        Transform holder;
        GameObject rcCursorObject;
        int currentMaterialID;

        Vector3 rcCursorTargetPosition;
        Vector3 rcCursorSmoothPosition;
        Vector3 rcCursorVelocityPosition;

        Quaternion rcCursorTargetRotation;
        Quaternion rcCursorSmoothRotation;
        Quaternion rcCursorVelocityRotation;

        const string selectionPointerName = "selection_pointer";
        const string prefabName = "Content/Debug/" + selectionPointerName;
    }
}
