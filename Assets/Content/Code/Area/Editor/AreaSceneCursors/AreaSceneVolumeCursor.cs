using UnityEngine;

namespace Area
{
    sealed class AreaSceneVolumeCursor : AreaSceneCursor
    {
        public int ID { get; }

        public int standardMaterialID => pointer.standardMaterialID;
        public int limitedMaterialID { get; private set; } = -1;
        public int warningMaterialID { get; private set; } = -1;

        public VolumePointChecker pointChecker
        {
            get => pointerCheckerInternal;
            set
            {
                pointerCheckerInternal = value;
                lastMaterialID = pointer.GetMaterialID ();
            }
        }

        public bool showWarning;
        public float warningTimeStart;

        public void CheckSetup (Transform parent)
        {
            if (limitedMaterialID != -1)
            {
                return;
            }

            var mat = Resources.Load<Material> ("Content/Debug/AreaCursor");
            var limited = new Material (mat)
            {
                color = new HSBColor (0f, 0f, 0.5f).ToColor (),
            };
            limitedMaterialID = pointer.RegisterMaterial (limited);
            var warning = new Material (mat)
            {
                color = new HSBColor (0.02083f, 1f, 0.8f).ToColor (),
            };
            warningMaterialID = pointer.RegisterMaterial (warning);
        }

        public void OnDestroy () { }

        public void Update (float timeDelta, AreaSceneCursor.UpdateCursor update)
        {
            if (pointChecker != null)
            {
                var materialID = pointChecker.CurrentMaterialID;
                if (materialID != lastMaterialID)
                {
                    pointer.SetMaterial (materialID);
                    lastMaterialID = materialID;
                }
            }
            pointer.Update (timeDelta, update);
            selection.Update (timeDelta, update);
        }

        public void Show (AreaVolumePoint point, RaycastHit hitInfo)
        {
            pointer.Show (point, hitInfo);
            selection.Show (point, hitInfo);
            pointChecker?.Check (point, hitInfo.normal);
        }

        public void Hide ()
        {
            pointer.Hide ();
            selection.Hide ();
        }

        public AreaSceneVolumeCursor (AreaSceneGizmosCursor gizmosCursor)
        {
            pointer = gizmosCursor.pointer;
            selection = gizmosCursor.volumeSelection;
            ID = gizmosCursor.RegisterCursor (this);
            lastMaterialID = standardMaterialID;
        }

        readonly AreaScenePointerCursor pointer;
        readonly AreaSceneCursor selection;
        VolumePointChecker pointerCheckerInternal;

        int lastMaterialID;
    }
}
