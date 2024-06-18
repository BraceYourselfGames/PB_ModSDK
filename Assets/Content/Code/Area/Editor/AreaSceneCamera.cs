using UnityEditor;
using UnityEngine;

namespace Area
{
    using Scene;

    static class AreaSceneCamera
    {
        public static bool Prepare ()
        {
            cam = SceneView.currentDrawingSceneView.camera;
            hasCamera = cam != null;
            if (!hasCamera)
            {
                facing = Vector3.zero;
                nearCenter = Vector3.zero;
                return false;
            }

            var t = cam.transform;
            facing = t.forward;
            nearCenter = t.position + facing * cam.nearClipPlane;

            return true;
        }

        public static bool InView (Vector3 point, float cutoffDistance, bool showOccluded = true)
        {
            if (!hasCamera)
            {
                return false;
            }

            var direction = Utilities.GetDirection (nearCenter, point);
            var dot = Vector3.Dot (facing, direction);
            if (dot <= 0f)
            {
                return false;
            }

            if (Vector3.SqrMagnitude (nearCenter - point) > cutoffDistance)
            {
                return false;
            }

            var vp = cam.WorldToViewportPoint (point);
            if (vp.x < 0f || vp.x > 1f)
            {
                return false;
            }
            if (vp.y < 0f || vp.y > 1f)
            {
                return false;
            }

            if (showOccluded)
            {
                return true;
            }

            return !Physics.Linecast (nearCenter, point - direction * WorldSpace.HalfBlockSize, environmentLayerMask);
        }

        static Camera cam;
        static bool hasCamera;
        static Vector3 facing;
        static Vector3 nearCenter;

        public const float interactionDistance = 2100f;
        public const float interactionDistanceNavigation = 3000f;
        public static readonly Vector3 spotRaycastHitOffset = new Vector3 (-1.5f, 1.5f, -1.5f);
        public const int environmentLayerMask = 1 << Constants.environmentLayer;
        public const int volumeCollidersLayerMask = 1 << Constants.volumeCollidersLayer;
    }
}
