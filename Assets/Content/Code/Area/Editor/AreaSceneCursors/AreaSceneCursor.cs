using UnityEngine;

namespace Area
{
    interface AreaSceneCursor
    {
        delegate bool UpdateCursor (GameObject cursorObject, Vector3 targetPosition, ref Vector3 smoothPosition, ref Vector3 velocity);
        void CheckSetup (Transform parent);
        void OnDestroy ();
        void Update (float timeDelta, UpdateCursor update);
        void Show (AreaVolumePoint point, RaycastHit hitInfo);
        void Hide ();
    }
}
