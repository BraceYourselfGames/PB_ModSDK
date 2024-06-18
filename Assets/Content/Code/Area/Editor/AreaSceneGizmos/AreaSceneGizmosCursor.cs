using System.Collections.Generic;

using UnityEngine;

namespace Area
{
    sealed class AreaSceneGizmosCursor
    {
        public void CheckSetup ()
        {
            pointer.CheckSetup (bb.am.transform);
            volumeSelection.CheckSetup (bb.am.transform);
            for (var i = 1; i < cursors.Count; i += 1)
            {
                cursors[i].CheckSetup (bb.am.transform);
            }
        }

        public void OnDestroy ()
        {
            activeCursor = null;
            for (var i = 1; i < cursors.Count; i += 1)
            {
                cursors[i].OnDestroy ();
            }
            pointer.OnDestroy ();
            volumeSelection.OnDestroy ();
        }

        public void ShowCursor (AreaVolumePoint point, RaycastHit hitInfo) => activeCursor?.Show (point, hitInfo);
        public void HideCursor () => activeCursor?.Hide ();

        public int RegisterCursor (AreaSceneCursor cursor)
        {
            if (cursor == null)
            {
                return -1;
            }
            for (var i = 0; i < cursors.Count; i += 1)
            {
                if (cursors[i].GetType () == cursor.GetType ())
                {
                    return i;
                }
            }
            var cursorID = cursors.Count;
            cursors.Add (cursor);
            return cursorID;
        }

        public void SetCursor (int cursorID)
        {
            if (!cursorID.IsValidIndex (cursors))
            {
                return;
            }
            if (cursorID == lastActiveCursorID)
            {
                return;
            }
            activeCursor?.Hide ();
            activeCursor = cursors[cursorID];
            lastActiveCursorID = cursorID;
        }

        public AreaSceneCursor GetCursor (int cursorID) => !cursorID.IsValidIndex (cursors) ? null : cursors[cursorID];

        public void Update ()
        {
            updateTimeDelta = Time.realtimeSinceStartup - updateTimeLast;
            updateTimeLast = Time.realtimeSinceStartup;
            activeCursor?.Update (updateTimeDelta, UpdateCursor);
        }

        bool UpdateCursor (GameObject cursorObject, Vector3 targetPosition, ref Vector3 smoothPosition, ref Vector3 velocity)
        {
            if (cursorObject == null)
            {
                return false;
            }
            if (!cursorObject.activeSelf)
            {
                return false;
            }

            smoothPosition = Vector3.SmoothDamp (smoothPosition, targetPosition, ref velocity, 0.05f, 100000f, updateTimeDelta);
            cursorObject.transform.position = smoothPosition;
            return true;
        }

        public int pointerCursorID { get; }
        public AreaScenePointerCursor pointer { get; } = new AreaScenePointerCursor ();
        public AreaSceneCursor volumeSelection { get; } = new AreaSceneVolumeSelectionCursor ();

        public AreaSceneGizmosCursor (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            pointerCursorID = RegisterCursor (pointer);
            activeCursor = pointer;
            lastActiveCursorID = pointerCursorID;
        }

        readonly AreaSceneBlackboard bb;
        readonly List<AreaSceneCursor> cursors = new List<AreaSceneCursor> ();
        AreaSceneCursor activeCursor;
        int lastActiveCursorID;

        float updateTimeLast;
        float updateTimeDelta;
    }
}
