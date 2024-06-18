using UnityEngine;

namespace Area
{
    using Scene;

    interface AreaSceneMode
    {
        EditingMode EditingMode { get; }
        AreaSceneModePanel Panel { get; }
        void OnDisable ();
        void OnDestroy ();
        int LayerMask { get; }
        bool Hover (Event e, RaycastHit hitInfo);
        void OnHoverEnd ();
        bool HandleSceneUserInput (Event e);
        void DrawSceneMarkup (Event e, System.Action repaint);
    }
}
