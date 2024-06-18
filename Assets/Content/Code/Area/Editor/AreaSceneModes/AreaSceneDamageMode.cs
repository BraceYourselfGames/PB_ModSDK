using UnityEngine;

namespace Area
{
    using Scene;

    sealed class AreaSceneDamageMode : AreaSceneMode
    {
        public EditingMode EditingMode => EditingMode.Damage;

        public AreaSceneModePanel Panel { get; }

        public void OnDisable () => Panel.OnDisable ();
        public void OnDestroy () { }

        public int LayerMask => AreaSceneCamera.environmentLayerMask;

        public bool Hover (Event e, RaycastHit hitInfo)
        {
            if (!AreaSceneModeHelper.DisplayVolumeCursor(bb, hitInfo, cursorID))
            {
                return false;
            }
            bb.gizmos.DrawWireframesForVolume (e.shift);

            var point = bb.lastPointHovered;
            var (eventType, button) = AreaSceneModeHelper.ResolveEvent(e);
            switch (eventType)
            {
                case EventType.MouseDown:
                case EventType.ScrollWheel:
                    Edit (button, point);
                    return true;
                case EventType.KeyUp:
                    return HandleKeyboardInput (button, point);
            }
            return false;
        }

        public void OnHoverEnd () => bb.gizmos.cursor.HideCursor ();

        public bool HandleSceneUserInput (Event e) => false;

        public void DrawSceneMarkup (Event e, System.Action repaint)
        {
            AreaSceneModeHelper.TryRebuildTerrain (bb);

            if (!bb.displayDestructibility)
            {
                return;
            }

            var am = bb.am;
            var bounds = am.boundsFull;
            if (!AreaSceneHelper.ValidateBounds (bounds))
            {
                return;
            }

            // Start at bottom layer
            var startIndex = bounds.x * bounds.z * (bounds.y - 1);
            var points = am.points;
            var damageRestrictionDepth = am.damageRestrictionDepth;
            var damagePenetrationDepth = am.damagePenetrationDepth;

            const float halfBlockPlusCM = WorldSpace.HalfBlockSize + 0.01f;
            var planeRotation = Quaternion.Euler (90f, 0f, 0f);
            var offsetVertical = new Vector3 (0f, halfBlockPlusCM, 0f);

            if (!AreaSceneCamera.Prepare ())
            {
                Debug.LogWarning ("Unable to draw destructibility: no scene camera");
                return;
            }

            // Progress for size of one layer
            var offsetLimit = bounds.x * bounds.z;

            for (var i = 0; i < offsetLimit; ++i)
            {
                var index = i + startIndex;
                var point = points[index];

                if (point == null)
                {
                    continue;
                }

                // Traverse up until you exit damage restriction depth
                var pointAbove = point.pointsWithSurroundingSpots[WorldSpace.SpotNeighbor.Up];
                var pointLast = point;
                AreaVolumePoint pointIndestructibleLast = null;

                while (pointAbove != null && pointAbove.pointPositionIndex.y >= damageRestrictionDepth)
                {
                    pointLast = pointAbove;
                    if (!pointAbove.destructible)
                    {
                        pointIndestructibleLast = pointAbove;
                    }
                    pointAbove = pointAbove.pointsWithSurroundingSpots[WorldSpace.SpotNeighbor.Up];
                }

                if (pointLast.pointState == AreaVolumePointState.Empty)
                {
                    continue;
                }

                var avp = pointAbove;
                while (avp != null)
                {
                    if (!avp.destructible)
                    {
                        pointIndestructibleLast = avp;
                    }
                    avp = avp.pointsWithSurroundingSpots[WorldSpace.SpotNeighbor.Up];
                }

                var pointTop = pointIndestructibleLast ?? point;
                if (!AreaSceneCamera.InView (pointTop.pointPositionLocal, AreaSceneCamera.interactionDistance))
                {
                    continue;
                }

                var pos = offsetVertical + pointLast.pointPositionLocal;
                bb.gizmos.DrawZTestRect (pos, planeRotation, halfBlockPlusCM, Colors.Volume.MainIndestructibleHard, Colors.Volume.FadedIndestructibleHard);
                bb.gizmos.DrawZTestLine (pos, pointLast.pointPositionLocal, Colors.Volume.MainIndestructibleHard, Colors.Volume.FadedIndestructibleHard);

                while (pointAbove != null)
                {
                    if (!pointAbove.destructible)
                    {
                        pos = offsetVertical + pointAbove.pointPositionLocal;
                        bb.gizmos.DrawZTestRect (pos, planeRotation, halfBlockPlusCM, Colors.Volume.MainIndestructibleFlag, Colors.Volume.FadedIndestructibleFlag);
                        bb.gizmos.DrawZTestLine (pos, pointAbove.pointPositionLocal, Colors.Volume.MainIndestructibleFlag, Colors.Volume.FadedIndestructibleFlag);
                    }

                    pointAbove = pointAbove.pointsWithSurroundingSpots[WorldSpace.SpotNeighbor.Up];
                }
            }
        }

        void Edit (KeyCode button, AreaVolumePoint point)
        {
            var am = bb.am;

            if (Application.isPlaying)
            {
                if (button == KeyCode.Mouse1 && point.pointState == AreaVolumePointState.Full)
                {
                    am.ApplyDamageToPoint (point, 1000);
                }
                return;
            }

            switch (button)
            {
                case KeyCode.Mouse2:
                    ToggleDestructible (point);
                    break;
                case KeyCode.Mouse0 when point.pointState == AreaVolumePointState.FullDestroyed:
                case KeyCode.Mouse1 when point.pointState == AreaVolumePointState.Full:
                    if (button == KeyCode.Mouse0)
                    {
                        point.pointState = AreaVolumePointState.Full;
                        point.integrity = point.integrityForDestructionAnimation = 1f;
                    }
                    else
                    {
                        var indestructible = AreaManager.IsPointIndestructible (point, true, true, true, true, true);
                        if (!indestructible || bb.allowIndestructibleDestruction)
                        {
                            point.pointState = AreaVolumePointState.FullDestroyed;
                            point.integrity = point.integrityForDestructionAnimation = 0f;
                        }
                        else
                        {
                            Debug.LogWarning ("Destruction blocked by point being indestructible: try enabling the option allowing removal of indestructible points");
                            return;
                        }
                    }

                    am.UpdateSpotsAroundIndex (point.spotIndex, false);
                    am.RebuildBlocksAroundIndex (point.spotIndex);
                    am.UpdateDamageAroundIndex (point.spotIndex);
                    am.RebuildCollisionsAroundIndex (point.spotIndex);
                    break;
                case KeyCode.PageUp:
                case KeyCode.PageDown:
                    point.integrity = Mathf.Clamp (point.integrity + (button == KeyCode.PageUp ? 0.1f : -0.1f), 0.1f, 1f);
                    foreach (var spot in point.pointsWithSurroundingSpots)
                    {
                        am.ApplyShaderPropertiesAtPoint (spot, AreaManager.ShaderOverwriteMode.None, true, true, true);
                    }
                    break;
            }
        }

        void ToggleDestructible (AreaVolumePoint point)
        {
            // Do not modify points with hard indestructibility from height, edges or tilesets to avoid wasting data
            if (AreaManager.IsPointIndestructible (point, false, true, true, true, true))
            {
                if (bb.enableDamageLogging)
                {
                    Debug.LogFormat ("Point {0} is already indirectly indestructible due to height, adjacency or tileset", point.spotIndex);
                }
                return;
            }

            point.destructible = !point.destructible;
            if (!bb.propagateDestructibilityDown)
            {
                if (bb.enableDamageLogging)
                {
                    Debug.LogFormat
                    (
                        "Point {0} is now {1}",
                        point.spotIndex,
                        point.destructible ? "destructible" : "indestructible"
                    );
                }
                return;
            }

            var pointNeighborDown = point.pointsInSpot[WorldSpace.PointNeighbor.Down];
            var pointsChanged = 0;
            while (pointNeighborDown != null)
            {
                // Do not modify points with hard indestructibility from height, edges or tilesets to avoid wasting data
                if (AreaManager.IsPointIndestructible (pointNeighborDown, false, true, true, true, true))
                {
                    break;
                }

                pointNeighborDown.destructible = point.destructible;
                pointNeighborDown = pointNeighborDown.pointsInSpot[WorldSpace.PointNeighbor.Down];
                pointsChanged += 1;
            }

            if (bb.enableDamageLogging)
            {
                Debug.LogFormat
                (
                    "Point {0} is now {1} along with {2} points under it",
                    point.spotIndex,
                    point.destructible ? "destructible" : "indestructible",
                    pointsChanged
                );
            }
        }

        void ToggleDestructionTracking (AreaVolumePoint point)
        {
            point.destructionUntracked = !point.destructionUntracked;
            if (!bb.propagateDestructibilityDown)
            {
                if (bb.enableDamageLogging)
                {
                    Debug.LogFormat
                    (
                        "Point {0} is now {1}",
                        point.spotIndex,
                        point.destructible ? "destructible" : "indestructible"
                    );
                }
                return;
            }

            var pointNeighborDown = point.pointsInSpot[WorldSpace.PointNeighbor.Down];
            var pointsChanged = 0;
            while (pointNeighborDown != null)
            {
                // Do not modify points with hard indestructibility from height, edges or tilesets to avoid wasting data
                if (AreaManager.IsPointIndestructible (pointNeighborDown, false, true, true, true, true))
                {
                    break;
                }

                pointNeighborDown.destructionUntracked = point.destructionUntracked;
                pointNeighborDown = pointNeighborDown.pointsInSpot[WorldSpace.PointNeighbor.Down];
                pointsChanged += 1;
            }

            if (bb.enableDamageLogging)
            {
                Debug.LogFormat
                (
                    "Point {0} destruction is now {1} along with {2} points under it",
                    point.spotIndex,
                    point.destructionUntracked ? "untracked" : "tracked",
                    pointsChanged
                );
            }
        }

        bool HandleKeyboardInput (KeyCode button, AreaVolumePoint point)
        {
            switch (button)
            {
                case KeyCode.PageUp:
                case KeyCode.PageDown:
                    Edit (button, point);
                    return true;
                case KeyCode.Q:
                    ToggleDestructionTracking (point);
                    return true;
            }
            return false;
        }

        public static AreaSceneMode CreateInstance (AreaSceneBlackboard bb) => new AreaSceneDamageMode (bb);

        AreaSceneDamageMode (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            var cursor = new AreaSceneVolumeCursor (bb.gizmos.cursor);
            cursorID = cursor.ID;
            Panel = AreaSceneDamageModePanel.Create (bb);
        }

        readonly AreaSceneBlackboard bb;
        readonly int cursorID;
    }
}
