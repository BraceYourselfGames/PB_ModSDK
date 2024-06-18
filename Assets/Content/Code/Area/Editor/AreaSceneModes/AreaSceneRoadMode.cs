using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace Area
{
    using Scene;

    sealed class AreaSceneRoadMode : AreaSceneMode
    {
        public EditingMode EditingMode => EditingMode.Roads;

        public AreaSceneModePanel Panel { get; }

        public void OnDisable () => Panel.OnDisable ();
        public void OnDestroy () { }

        public int LayerMask => AreaSceneCamera.environmentLayerMask;

        public bool Hover (Event e, RaycastHit hitInfo)
        {
            if (!AreaSceneModeHelper.DisplayVolumeCursor (bb, hitInfo, cursorID))
            {
                lastIndex = -1;
                return false;
            }
            bb.gizmos.DrawWireframesForVolume (e.shift);
            AreaSceneModeHelper.DrawVolumeSelectionHandles (bb, VolumeSelectionStatus.OK);

            var (eventType, button) = AreaSceneModeHelper.ResolveEvent (e);
            switch (eventType)
            {
                case EventType.MouseDown when button == KeyCode.Mouse0 && e.shift && e.control:
                    FloodFill ();
                    break;
                case EventType.MouseDown:
                case EventType.ScrollWheel:
                    Edit (button);
                    return true;
                case EventType.MouseDrag when button == KeyCode.Mouse0 || button == KeyCode.Mouse1:
                    if (lastIndex == bb.lastPointHovered.spotIndex)
                    {
                        return true;
                    }
                    lastIndex = bb.lastPointHovered.spotIndex;
                    bb.am.EditRoad (lastIndex, button == KeyCode.Mouse1 ? AreaManager.RoadEditingOperation.Remove : AreaManager.RoadEditingOperation.Add);
                    return true;
            }
            lastIndex = -1;
            return false;
        }

        public void OnHoverEnd () => bb.gizmos.cursor.HideCursor ();

        public bool HandleSceneUserInput (Event e) => false;

        public void DrawSceneMarkup (Event e, System.Action repaint)
        {
            AreaSceneModeHelper.TryRebuildTerrain (bb);

            var am = bb.am;
            var hc = Handles.color;
            Handles.color = new HSBColor (0.15f, 1f, 1f, 0.5f).ToColor ();

            var offset = new Vector3 (0, WorldSpace.HalfBlockSize, 0f);
            var rotation = Quaternion.Euler (90f, 45f, 0f);
            const float scale = WorldSpace.HalfBlockSize * 0.7075f * 0.5f;

            foreach (var point in am.points)
            {
                if (point == null || !point.road)
                {
                    continue;
                }

                var pos = point.pointPositionLocal + offset;
                Handles.RectangleHandleCap (0, pos, rotation, scale, EventType.Repaint);
            }

            Handles.color = hc;
        }

        void Edit (KeyCode mouseButton)
        {
            var operation = AreaManager.RoadEditingOperation.None;
            switch (mouseButton)
            {
                case KeyCode.Mouse0:
                    operation = AreaManager.RoadEditingOperation.Add;
                    break;
                case KeyCode.Mouse1:
                    operation = AreaManager.RoadEditingOperation.Remove;
                    break;
                case KeyCode.Mouse2:
                    operation = AreaManager.RoadEditingOperation.FloodFill;
                    break;
                case KeyCode.PageUp:
                    operation = AreaManager.RoadEditingOperation.SubtypeNext;
                    break;
                case KeyCode.PageDown:
                    operation = AreaManager.RoadEditingOperation.SubtypePrev;
                    break;
                case KeyCode.LeftBracket:
                case KeyCode.RightBracket:
                    AreaSceneModeHelper.ChangeVolumeBrush(bb, mouseButton == KeyCode.LeftBracket);
                    return;
            }
            lastIndex = bb.lastPointHovered.spotIndex;
            bb.am.EditRoad (lastIndex, operation);
        }

        void FloodFill ()
        {
            var (grid, boundsX, boundsZ) = BuildRoadMap ();
            var markerQueue = new Queue<FillMarker> ();
            var altQueue = new Queue<FillMarker> ();
            var pos = bb.lastPointHovered.pointPositionIndex;
            var marker = new FillMarker ()
            {
                X = pos.x - 1,
                Z = pos.z - 1,
                ZDir = 0,
                SpanEnd = pos.x - 1,
            };
            markerQueue.Enqueue (marker);
            filledIndexes.Clear ();
            ProcessMarkerQueue (grid, boundsX, boundsZ, markerQueue, altQueue);
            FillRoadPoints (pos.y, boundsX);
        }

        (int[], int, int) BuildRoadMap ()
        {
            var bounds = bb.am.boundsFull;
            var lastX = bounds.x - 1;
            var lastZ = bounds.z - 1;
            var grid = new int[(lastX - 1) * (lastZ - 1)];
            var layer = bb.lastPointHovered.pointPositionIndex.y;
            var points = bb.am.points;
            for (var i = layer * bounds.x * bounds.z + bounds.z; i < points.Count; i += 1)
            {
                var point = points[i];
                var pos = point.pointPositionIndex;
                var z = pos.z;
                if (z == lastZ)
                {
                    break;
                }
                var x = pos.x;
                if (x == 0 || x == lastX)
                {
                    continue;
                }
                var index = (z - 1) * (lastX - 1) + x - 1;
                grid[index] = point.road ? border : empty;
            }
            lastX -= 1;
            lastZ -= 1;
            return (grid, lastX, lastZ);
        }

        static void ProcessMarkerQueue(int[] grid, int boundsX, int boundsZ, Queue<FillMarker> markerQueue, Queue<FillMarker> altQueue)
        {
            var spans = new List<(int, int, int)> ();
            while (markerQueue.Count != 0)
            {
                var marker = markerQueue.Dequeue ();
                var mzd = marker.ZDir == 0 ? -1 : marker.ZDir;
                FindMarks (grid, boundsX, marker, spans);
                spans.Sort (OrderByTrailingToLeading);
                foreach (var (spanStart, spanEnd, zdir) in spans)
                {
                    var nextZ = marker.Z + mzd;
                    var addNext = (zdir == 0 || zdir == mzd) && nextZ != -1 && nextZ != boundsZ;
                    var altZ = marker.Z - mzd;
                    var addAlt = (zdir == 0 || zdir != mzd) && altZ != -1 && altZ != boundsZ;
                    if (addNext)
                    {
                        var markerSpan = new FillMarker ()
                        {
                            X = spanStart,
                            Z = nextZ,
                            ZDir = mzd,
                            SpanEnd = spanEnd,
                        };
                        markerQueue.Enqueue (markerSpan);
                    }
                    if (addAlt)
                    {
                        var markerSpan = new FillMarker ()
                        {
                            X = spanStart,
                            Z = altZ,
                            ZDir = -mzd,
                            SpanEnd = spanEnd,
                        };
                        altQueue.Enqueue (markerSpan);
                    }
                }

                if (markerQueue.Count == 0 && altQueue.Count != 0)
                {
                    (markerQueue, altQueue) = (altQueue, markerQueue);
                }
            }
        }

        static void FindMarks (int[] grid, int boundsX, FillMarker marker, List<(int, int, int)> spans)
        {
            spans.Clear ();

            var x = marker.X;
            var z = marker.Z;
            var markIndex = x + z * boundsX;
            var index = markIndex;
            var v = grid[index];
            if (v == filled)
            {
                return;
            }

            x = ScanToStartEdge (grid, x, index, v);
            var spanStart = grid[markIndex] == filled ? x + 1 : -1;
            (x, index, spanStart) = ScanSpan (grid, marker.SpanEnd, marker.X + 1, markIndex + 1, spanStart, marker.X, marker.ZDir, spans);
            if (spanStart == -1)
            {
                return;
            }

            spans.Add ((spanStart, x - 1, spanStart < marker.X ? 0 : marker.ZDir));
            spanStart = x;

            if (x <= marker.SpanEnd || x == boundsX)
            {
                return;
            }

            x = ScanToEndEdge (grid, boundsX, x, index);
            if (spanStart < x)
            {
                spans.Add ((spanStart, x - 1, 0));
            }
        }

        static int ScanToStartEdge (int[] grid, int x, int index, int v)
        {
            while (v == empty)
            {
                grid[index] = filled;
                filledIndexes.Add (index);
                x -= 1;
                index -= 1;
                if (x == -1)
                {
                    break;
                }
                v = grid[index];
            }
            return x;
        }

        static (int, int, int) ScanSpan (int[] grid, int limitX, int x, int index, int spanStart, int markX, int zdir, List<(int, int, int)> spans)
        {
            while (x <= limitX)
            {
                var v = grid[index];
                switch (v)
                {
                    case empty:
                        if (spanStart == -1)
                        {
                            spanStart = x;
                        }
                        grid[index] = filled;
                        filledIndexes.Add (index);
                        break;
                    case border:
                        if (spanStart != -1)
                        {
                            spans.Add ((spanStart, x - 1, spanStart < markX ? 0 : zdir));
                            spanStart = -1;
                        }
                        break;
                    case filled:
                        return (x, index, spanStart);
                }
                x += 1;
                index += 1;
            }
            return (x, index, spanStart);
        }

        static int ScanToEndEdge (int[] grid, int boundsX, int x, int index)
        {
            while (x < boundsX)
            {
                var v = grid[index];
                if (v != empty)
                {
                    break;
                }
                grid[index] = filled;
                filledIndexes.Add (index);
                x += 1;
                index += 1;
            }
            return x;
        }

        static int OrderByTrailingToLeading ((int Start, int End, int ZDir) lhs, (int Start, int End, int ZDir) rhs) => -lhs.Start.CompareTo (rhs.Start);

        void FillRoadPoints (int layer, int boundsX)
        {
            if (filledIndexes.Count == 0)
            {
                return;
            }

            var bounds = bb.am.boundsFull;
            var points = bb.am.points;
            var roadPoints = new List<AreaVolumePoint> ();
            foreach (var index in filledIndexes.OrderBy (i => i))
            {
                var x = index % boundsX + 1;
                var z = index / boundsX + 1;
                var position = new Vector3Int (x, layer, z);
                var indexPoint = AreaUtility.GetIndexFromVolumePosition (position, bounds, skipBoundsCheck: true);
                var point = points[indexPoint];
                roadPoints.Add (point);
            }
            bb.am.EditRoadPoints(roadPoints, AreaManager.RoadEditingOperation.Add);
        }

        public static AreaSceneMode CreateInstance (AreaSceneBlackboard bb) => new AreaSceneRoadMode (bb);

        AreaSceneRoadMode (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            var cursor = new AreaSceneVolumeCursor (bb.gizmos.cursor);
            cursorID = cursor.ID;
            Panel = new AreaSceneRoadModePanel (bb);
        }

        readonly AreaSceneBlackboard bb;
        readonly int cursorID;
        int lastIndex = -1;

        static readonly HashSet<int> filledIndexes = new HashSet<int> ();

        const int empty = 0;
        const int border = 1;
        const int filled = 2;

        struct FillMarker
        {
            public int X;
            public int Z;
            public int ZDir;
            public int SpanEnd;
        }
    }
}
