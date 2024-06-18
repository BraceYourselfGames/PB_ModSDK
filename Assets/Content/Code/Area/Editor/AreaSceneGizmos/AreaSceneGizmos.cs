using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Area
{
    using Scene;

    class AreaSceneGizmos
    {
        public void CheckSetup () => cursor.CheckSetup ();
        public void OnDestroy () => cursor.OnDestroy ();
        public void Update () => cursor.Update ();

        public void DrawSceneMarkup ()
        {
            DrawAreaBorder ();
            DrawSearchResults (search.lastSearchResults);
        }

        public void DrawStructuralAnalysis ()
        {
            var am = bb.am;
            var hc = Handles.color;
            var colorNonIsolated = new HSBColor (0f, 0f, 0.8f, 1f).ToColor ();

            foreach (var p in am.points)
            {
                if (p == null)
                {
                    continue;
                }
                if (p.pointPositionIndex.y >= am.damageRestrictionDepth)
                {
                    continue;
                }
                if (p.structuralParent == null)
                {
                    continue;
                }

                if (p.structuralGroup == 0)
                {
                    Handles.color = colorNonIsolated;
                }
                else
                {
                    var structuralGroupValue = p.structuralGroup / 7.8423f;
                    Handles.color = new HSBColor (structuralGroupValue % 1f, 1f, 1f, 1f).ToColor ();
                }

                Handles.DrawAAPolyLine (4f, p.pointPositionLocal, p.structuralParent.pointPositionLocal);
                Handles.CubeHandleCap (0, p.pointPositionLocal, Quaternion.identity, 0.5f, EventType.Repaint);

                if (p.structuralGroup != 0)
                {
                    Handles.color = Color.white;
                    Handles.Label (p.pointPositionLocal, p.structuralGroup.ToString (), EditorStyles.whiteLabel);
                }
            }

            Handles.color = hc;
        }

        public void DrawVolumeSelectionHandles (Vector3 position, Color color)
        {

            var hc = Handles.color;
            Handles.color = color.WithAlpha (0.5f);

            if (AreaManager.editingVolumeBrush == AreaManager.EditingVolumeBrush.Circle || AreaManager.editingVolumeBrush == AreaManager.EditingVolumeBrush.Square3x3)
            {
                // X+
                Handles.CubeHandleCap (0, position + new Vector3 (WorldSpace.BlockSize, 0f, 0f), Quaternion.identity, 0.5f, EventType.Repaint);
                // Z+
                Handles.CubeHandleCap (0, position + new Vector3 (0f, 0f, WorldSpace.BlockSize), Quaternion.identity, 0.5f, EventType.Repaint);
                // X-
                Handles.CubeHandleCap (0, position + new Vector3 (-WorldSpace.BlockSize, 0f, 0f), Quaternion.identity, 0.5f, EventType.Repaint);
                // Z-
                Handles.CubeHandleCap (0, position + new Vector3 (0f, 0f, -WorldSpace.BlockSize), Quaternion.identity, 0.5f, EventType.Repaint);
            }

            if (AreaManager.editingVolumeBrush == AreaManager.EditingVolumeBrush.Square3x3)
            {
                // X+ & Z+
                Handles.CubeHandleCap (0, position + new Vector3 (WorldSpace.BlockSize, 0f, WorldSpace.BlockSize), Quaternion.identity, 0.5f, EventType.Repaint);
                // X- & Z+
                Handles.CubeHandleCap (0, position + new Vector3 (-WorldSpace.BlockSize, 0f, WorldSpace.BlockSize), Quaternion.identity, 0.5f, EventType.Repaint);
                // X- & Z-
                Handles.CubeHandleCap (0, position + new Vector3 (-WorldSpace.BlockSize, 0f, -WorldSpace.BlockSize), Quaternion.identity, 0.5f, EventType.Repaint);
                // X+ & Z-
                Handles.CubeHandleCap (0, position + new Vector3 (WorldSpace.BlockSize, 0f, -WorldSpace.BlockSize), Quaternion.identity, 0.5f, EventType.Repaint);
            }

            if (AreaManager.editingVolumeBrush == AreaManager.EditingVolumeBrush.Square2x2)
            {
                // X-
                Handles.CubeHandleCap (0, position + new Vector3 (-WorldSpace.BlockSize, 0f, 0f), Quaternion.identity, 0.5f, EventType.Repaint);
                // Z-
                Handles.CubeHandleCap (0, position + new Vector3 (0f, 0f, -WorldSpace.BlockSize), Quaternion.identity, 0.5f, EventType.Repaint);
                // X- & Z-
                Handles.CubeHandleCap (0, position + new Vector3 (-WorldSpace.BlockSize, 0f, -WorldSpace.BlockSize), Quaternion.identity, 0.5f, EventType.Repaint);
            }

            Handles.color = hc;
        }

        public void DrawWireframeForSpot ()
        {
            var lastSpotHovered = bb.lastSpotHovered;
            if (lastSpotHovered == null)
            {
                Debug.LogWarning ("Null lastSpotHovered with editable spot type");
                return;
            }

            switch (bb.currentSpotCursorType)
            {
                case SpotCursorType.Cube:
                    DrawWireframeCubeForSpot (lastSpotHovered);
                    break;
                case SpotCursorType.Square:
                    DrawWireframeQuadForSpot (lastSpotHovered);
                    break;
            }
            DrawSurfaceDirectionIndicator(lastSpotHovered);
        }

        public void DrawWireframesForVolume (bool shift)
        {
            var lastPointHovered = bb.lastPointHovered;
            if (lastPointHovered == null)
            {
                return;
            }

            if (!bb.showVolumeWireFrames)
            {
                return;
            }

            const float cubeShrink = WorldSpace.HalfBlockSize * 0.93f;
            const float bracketShrink = WorldSpace.HalfBlockSize * 0.87f;
            var halfExtentsCube = Vector3.one * cubeShrink;
            var halfExtentsBracket = Vector2.one * bracketShrink;

            DrawAAWireCube (lastPointHovered.pointPositionLocal, halfExtentsCube, AreaSceneHelper.GetColorFromMain (lastPointHovered));

            if (shift)
            {
                var neighborUp = lastPointHovered.pointsWithSurroundingSpots[WorldSpace.SpotNeighbor.Up];
                if (neighborUp != null)
                {
                    var surfacePos = lastPointHovered.pointPositionLocal + Vector3.up * WorldSpace.HalfBlockSize;
                    var offset = neighborUp.terrainOffset * WorldSpace.BlockSize;

                    DrawAAWireSquare (surfacePos, halfExtentsBracket, Colors.Offset.Neutral);
                    if (Mathf.Abs (offset) > 0.05f)
                    {
                        var color = offset > 0f ? Colors.Offset.Pos : Colors.Offset.Neg;
                        DrawAAWireSquare (surfacePos + Vector3.up * offset, halfExtentsBracket, color);
                        DrawAALine (surfacePos, surfacePos + Vector3.up * offset, WorldSpace.BlockSize, color);
                    }
                }
                return;
            }

            DrawWireCubesForVolume (halfExtentsCube);
        }

        public void DrawVolumeDirection(Vector3 posA, Vector3 posB, Vector3 dir, Color colorMain)
        {
            var colorMainTransparent = colorMain.WithAlpha (0.15f);

            var hc = Handles.color;

            var centerPt = new Vector3((posA.x + posB.x) * 0.5f, posB.y, (posA.z + posB.z) * 0.5f);
            var halfSize = new Vector3((posB.x - posA.x) * 0.5f, 0f, (posB.z - posA.z) * 0.5f);

            var fwdStep = Vector3.Scale(dir, halfSize);
            var sideStep = new Vector3(fwdStep.z,  0f, -fwdStep.x);
            var edgePt = centerPt + fwdStep;

            Handles.color = colorMainTransparent;
            Handles.DrawAAConvexPolygon(edgePt + sideStep.normalized, edgePt + fwdStep.normalized, edgePt - sideStep.normalized);

            Handles.color = colorMain;
            Handles.DrawPolyLine(edgePt + sideStep.normalized, edgePt + fwdStep.normalized, edgePt - sideStep.normalized);

            Handles.color = hc;
        }

        public void DrawZTestVolume (Vector3 posA, Vector3 posB, Color colorMain, Color colorCulled)
        {
            Vector3 difference = posB - posA;
            difference.y = -difference.y;

            var hc = Handles.color;
            var zt = Handles.zTest;

            var colorMainTransparent = colorMain.WithAlpha (0.15f);
            var colorCulledTransparent = colorCulled.WithAlpha (0.15f);

            Handles.zTest = CompareFunction.LessEqual;
            Handles.color = colorMain;
            Handles.CubeHandleCap (0, posA, Quaternion.identity, 0.5f, EventType.Repaint);
            Handles.CubeHandleCap (1, posB, Quaternion.identity, 0.5f, EventType.Repaint);

            Handles.zTest = CompareFunction.Greater;
            Handles.color = colorCulled;
            Handles.CubeHandleCap (0, posA, Quaternion.identity, 0.5f, EventType.Repaint);
            Handles.CubeHandleCap (1, posB, Quaternion.identity, 0.5f, EventType.Repaint);

            Handles.color = Color.white.WithAlpha (1f);
            transferPreviewVerts[0] = new Vector3 (posA.x, posB.y, posA.z);
            transferPreviewVerts[1] = new Vector3 (posA.x, posA.y, posA.z);
            transferPreviewVerts[2] = new Vector3 (posB.x, posA.y, posA.z);
            transferPreviewVerts[3] = new Vector3 (posB.x, posB.y, posA.z);
            Handles.zTest = CompareFunction.LessEqual;
            Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorMainTransparent, colorMain);
            Handles.zTest = CompareFunction.Greater;
            Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorCulledTransparent, colorCulled);

            transferPreviewVerts[0] = new Vector3 (posB.x, posB.y, posA.z);
            transferPreviewVerts[1] = new Vector3 (posB.x, posA.y, posA.z);
            transferPreviewVerts[2] = new Vector3 (posB.x, posA.y, posB.z);
            transferPreviewVerts[3] = new Vector3 (posB.x, posB.y, posB.z);
            Handles.zTest = CompareFunction.LessEqual;
            Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorMainTransparent, colorMain);
            Handles.zTest = CompareFunction.Greater;
            Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorCulledTransparent, colorCulled);

            transferPreviewVerts[0] = new Vector3 (posB.x, posB.y, posB.z);
            transferPreviewVerts[1] = new Vector3 (posB.x, posA.y, posB.z);
            transferPreviewVerts[2] = new Vector3 (posA.x, posA.y, posB.z);
            transferPreviewVerts[3] = new Vector3 (posA.x, posB.y, posB.z);
            Handles.zTest = CompareFunction.LessEqual;
            Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorMainTransparent, colorMain);
            Handles.zTest = CompareFunction.Greater;
            Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorCulledTransparent, colorCulled);

            transferPreviewVerts[0] = new Vector3 (posA.x, posB.y, posB.z);
            transferPreviewVerts[1] = new Vector3 (posA.x, posA.y, posB.z);
            transferPreviewVerts[2] = new Vector3 (posA.x, posA.y, posA.z);
            transferPreviewVerts[3] = new Vector3 (posA.x, posB.y, posA.z);
            Handles.zTest = CompareFunction.LessEqual;
            Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorMainTransparent, colorMain);
            Handles.zTest = CompareFunction.Greater;
            Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorCulledTransparent, colorCulled);

            Handles.color = hc;
            Handles.zTest = zt;
        }

        public void DrawZTestLine (Vector3 a, Vector3 b, Color colorMain, Color colorCulled)
        {
            var hc = Handles.color;
            var zt = Handles.zTest;

            Handles.zTest = CompareFunction.LessEqual;
            Handles.color = colorMain;
            Handles.DrawLine (a, b);

            Handles.zTest = CompareFunction.Greater;
            Handles.color = colorCulled;
            Handles.DrawLine (a, b);

            Handles.color = hc;
            Handles.zTest = zt;
        }

        public void DrawZTestCube (Vector3 center, Quaternion rotation, float size, Color colorMain, Color colorCulled)
        {
            var hc = Handles.color;
            var zt = Handles.zTest;

            Handles.zTest = CompareFunction.LessEqual;
            Handles.color = colorMain;
            Handles.CubeHandleCap (0, center, rotation, size, EventType.Repaint);

            Handles.zTest = CompareFunction.Greater;
            Handles.color = colorCulled;
            Handles.CubeHandleCap(0, center, rotation, size, EventType.Repaint);

            Handles.color = hc;
            Handles.zTest = zt;
        }

        public void DrawZTestRect (Vector3 center, Quaternion rotation, float size, Color colorMain, Color colorCulled)
        {
            var hc = Handles.color;
            var zt = Handles.zTest;

            Handles.zTest = CompareFunction.LessEqual;
            Handles.color = colorMain;
            Handles.RectangleHandleCap(0, center, rotation, size, EventType.Repaint);

            Handles.zTest = CompareFunction.Greater;
            Handles.color = colorCulled;
            Handles.RectangleHandleCap (0, center, rotation, size, EventType.Repaint);

            Handles.color = hc;
            Handles.zTest = zt;
        }

        void DrawSearchResults (List<AreaVolumePointSearchData> searchResults)
        {
            if (search.lastSearchResults == null)
            {
                return;
            }
            if (!bb.showLastSearchResults)
            {
                return;
            }

            var hc = Handles.color;
            var colorIsolated = new HSBColor (0f, 0f, 0.8f, 1f).ToColor ();
            var spotOffset = new Vector3 (1f, -1f, 1f) * (TilesetUtility.blockAssetSize / 2f);
            var rotation = Quaternion.Euler (90f, 0f, 0f);
            var maskFloor = AreaNavUtility.configFloor;

            for (var i = 0; i < searchResults.Count; ++i)
            {
                var psd = searchResults[i];
                if (psd != null && psd.status < 1000)
                {
                    var parented = psd.parent != null;
                    var flat = psd.point.spotConfiguration == maskFloor;

                    Handles.color = parented ? new HSBColor ((psd.status / 734.8423f) % 1f, 1f, 1f, 1f).ToColor () : colorIsolated;

                    if (flat)
                    {
                        Handles.RectangleHandleCap (0, psd.point.pointPositionLocal + spotOffset, rotation, 1.5f, EventType.Repaint);
                    }

                    if (parented)
                    {
                        Handles.CubeHandleCap (0, psd.point.pointPositionLocal + spotOffset, Quaternion.identity, 0.5f, EventType.Repaint);
                        Handles.DrawAAPolyLine (4f, psd.point.pointPositionLocal + spotOffset, psd.parent.point.pointPositionLocal + spotOffset);
                    }
                    else
                    {
                        Handles.CubeHandleCap (0, psd.point.pointPositionLocal + spotOffset, Quaternion.identity, 1f, EventType.Repaint);
                    }
                }
            }

            Handles.color = hc;
        }

        void DrawAreaBorder ()
        {
            if (!bb.showAreaBorder)
            {
                return;
            }

            var boundsScaled = bb.am.boundsFull * TilesetUtility.blockAssetSize;
            var boundsMidY = -boundsScaled.y * 0.5f;
            var bc1 = new Vector3 (0f, boundsMidY, 0f);
            var bc2 = new Vector3 (boundsScaled.x, boundsMidY, 0f);
            var bc3 = new Vector3 (boundsScaled.x, boundsMidY, boundsScaled.z);
            var bc4 = new Vector3 (0f, boundsMidY, boundsScaled.z);

            var hc = Handles.color;
            Handles.color = Color.gray;
            Handles.DrawLine (bc1, bc2);
            Handles.DrawLine (bc2, bc3);
            Handles.DrawLine (bc3, bc4);
            Handles.DrawLine (bc4, bc1);
            Handles.color = hc;
        }

        void DrawWireframeQuadForSpot (AreaVolumePoint spot)
        {
            GetQuadCornersForSpot (spot, bb.pointerDirection);

            var hc = Handles.color;
            Handles.color = Color.white.Opaque ();
            Handles.DrawPolyLine(spotQuadCorners);
            Handles.color = hc;
        }

        void DrawWireframeCubeForSpot(AreaVolumePoint lastSpotHovered)
        {
            DrawZTestWireCube (lastSpotHovered.instancePosition, Color.white.Opaque (), Color.cyan.WithAlpha (0.5f));

            const float positiveLineLength = WorldSpace.BlockSize * 1.25f;
            var offset = Vector3.up * 0.1f;
            var origin = lastSpotHovered.pointPositionLocal + offset;

            var neighborEast = lastSpotHovered.pointsInSpot[WorldSpace.PointNeighbor.East];
            if (neighborEast != null)
            {
                DrawZTestLine (origin, neighborEast.pointPositionLocal + offset, Colors.Axis.XPos);
                DrawZTestLine (neighborEast.pointPositionLocal + offset, neighborEast.pointPositionLocal + offset * positiveLineLength, Colors.Axis.XPos);
            }

            var neighborNorth = lastSpotHovered.pointsInSpot[WorldSpace.PointNeighbor.North];
            if (neighborNorth != null)
            {
                DrawZTestLine (origin, neighborNorth.pointPositionLocal + offset, Colors.Axis.ZPos);
                DrawZTestLine (neighborNorth.pointPositionLocal + offset, neighborNorth.pointPositionLocal + offset * positiveLineLength, Colors.Axis.ZPos);
            }

            var neighborWest = lastSpotHovered.pointsWithSurroundingSpots[WorldSpace.SpotNeighbor.West];
            if (neighborWest != null)
            {
                DrawZTestLine (origin, neighborWest.pointPositionLocal + offset, Colors.Axis.XNeg);
            }

            var neighborSouth = lastSpotHovered.pointsWithSurroundingSpots[WorldSpace.SpotNeighbor.South];
            if (neighborSouth != null)
            {
                DrawZTestLine (origin, neighborSouth.pointPositionLocal + offset, Colors.Axis.ZNeg);
            }
        }

        void DrawSurfaceDirectionIndicator (AreaVolumePoint lastSpotHovered)
        {
            var direction = AreaAssetHelper.GetSurfaceDirection (lastSpotHovered.spotConfiguration);
            var dirColor = direction == Vector3.up ? Colors.Axis.YPos : Colors.Axis.ZPos;
            DrawZTestLine (lastSpotHovered.instancePosition, lastSpotHovered.instancePosition + direction * WorldSpace.BlockSize, dirColor);
            DrawZTestWireCube (lastSpotHovered.instancePosition, Color.white.Opaque (), Color.cyan.WithAlpha (0.5f), 0.5f);
        }

        void DrawWireCubesForVolume (Vector3 halfExtentsCube)
        {
            var lastPointHovered = bb.lastPointHovered;
            var neighborEast = lastPointHovered.pointsInSpot[WorldSpace.PointNeighbor.East];
            var neighborWest = lastPointHovered.pointsWithSurroundingSpots[WorldSpace.SpotNeighbor.West];

            if (neighborEast != null && neighborEast.pointState != AreaVolumePointState.Empty)
            {
                DrawAAWireCube (neighborEast.pointPositionLocal, halfExtentsCube, AreaSceneHelper.GetColorFromNeighbor (neighborEast));
            }

            if (neighborWest != null && neighborWest.pointState != AreaVolumePointState.Empty)
            {
                DrawAAWireCube (neighborWest.pointPositionLocal, halfExtentsCube, AreaSceneHelper.GetColorFromNeighbor (neighborWest));
            }

            var neighborNorth = lastPointHovered.pointsInSpot[WorldSpace.PointNeighbor.North];
            var neighborSouth = lastPointHovered.pointsWithSurroundingSpots[WorldSpace.SpotNeighbor.South];

            if (neighborNorth != null && neighborNorth.pointState != AreaVolumePointState.Empty)
            {
                DrawAAWireCube (neighborNorth.pointPositionLocal, halfExtentsCube, AreaSceneHelper.GetColorFromNeighbor (neighborNorth));
            }

            if (neighborSouth != null && neighborSouth.pointState != AreaVolumePointState.Empty)
            {
                DrawAAWireCube (neighborSouth.pointPositionLocal, halfExtentsCube, AreaSceneHelper.GetColorFromNeighbor (neighborSouth));
            }

            var neighborDown = lastPointHovered.pointsInSpot[WorldSpace.PointNeighbor.Down];
            var neighborUp = lastPointHovered.pointsWithSurroundingSpots[WorldSpace.SpotNeighbor.Up];

            if (neighborDown != null && neighborDown.pointState != AreaVolumePointState.Empty)
            {
                DrawAAWireCube (neighborDown.pointPositionLocal, halfExtentsCube, AreaSceneHelper.GetColorFromNeighbor (neighborDown));
            }

            if (neighborUp != null && neighborUp.pointState != AreaVolumePointState.Empty)
            {
                DrawAAWireCube (neighborUp.pointPositionLocal, halfExtentsCube, AreaSceneHelper.GetColorFromNeighbor (neighborUp));
            }
        }

        void DrawAAWireCube (Vector3 center, Vector3 halfExtents, Color color, float width = 3f)
        {
            var hc = Handles.color;
            Handles.color = color;

            var t1 = center + new Vector3 (-halfExtents.x, halfExtents.y, -halfExtents.z);
            var t2 = center + new Vector3 (halfExtents.x, halfExtents.y, -halfExtents.z);
            var t3 = center + new Vector3 (halfExtents.x, halfExtents.y, halfExtents.z);
            var t4 = center + new Vector3 (-halfExtents.x, halfExtents.y, halfExtents.z);

            var b1 = center + new Vector3 (-halfExtents.x, -halfExtents.y, -halfExtents.z);
            var b2 = center + new Vector3 (halfExtents.x, -halfExtents.y, -halfExtents.z);
            var b3 = center + new Vector3 (halfExtents.x, -halfExtents.y, halfExtents.z);
            var b4 = center + new Vector3 (-halfExtents.x, -halfExtents.y, halfExtents.z);

            DrawAALine (b1, t1, width);
            DrawAALine (b2, t2, width);
            DrawAALine (b3, t3, width);
            DrawAALine (b4, t4, width);

            DrawAALine (t1, t2, width);
            DrawAALine (t2, t3, width);
            DrawAALine (t3, t4, width);
            DrawAALine (t4, t1, width);

            DrawAALine (b1, b2, width);
            DrawAALine (b2, b3, width);
            DrawAALine (b3, b4, width);
            DrawAALine (b4, b1, width);

            Handles.color = hc;
        }

        void DrawAAWireSquare (Vector3 center, Vector2 halfExtents, Color color, float width = 3f)
        {
            var hc = Handles.color;
            Handles.color = color;

            var t1 = center + new Vector3 (-halfExtents.x, 0, -halfExtents.y);
            var t2 = center + new Vector3 (halfExtents.x, 0, -halfExtents.y);
            var t3 = center + new Vector3 (halfExtents.x, 0, halfExtents.y);
            var t4 = center + new Vector3 (-halfExtents.x, 0, halfExtents.y);

            DrawAALine (t1, t2, width);
            DrawAALine (t2, t3, width);
            DrawAALine (t3, t4, width);
            DrawAALine (t4, t1, width);

            Handles.color = hc;
        }

        void DrawAALine (Vector3 a, Vector3 b, float width)
        {
            polylinePointsTemp[0] = a;
            polylinePointsTemp[1] = b;
            Handles.DrawAAPolyLine (width, polylinePointsTemp);
        }

        void DrawAALine (Vector3 a, Vector3 b, float width, Color color)
        {
            var hc = Handles.color;
            Handles.color = color;

            polylinePointsTemp[0] = a;
            polylinePointsTemp[1] = b;
            Handles.DrawAAPolyLine (width, polylinePointsTemp);

            Handles.color = hc;
        }

        void DrawZTestWireCube (Vector3 center, Color colorMain, Color colorCulled, float size = 3f)
        {
            var hc = Handles.color;
            var zt = Handles.zTest;

            Handles.zTest = CompareFunction.LessEqual;
            Handles.color = colorMain;
            Handles.DrawWireCube (center, Vector3.one * size);

            Handles.zTest = CompareFunction.Greater;
            Handles.color = colorCulled;
            Handles.DrawWireCube (center, Vector3.one * size);

            Handles.color = hc;
            Handles.zTest = zt;
        }

        void DrawZTestLine (Vector3 a, Vector3 b, Color colorMain) => DrawZTestLine (a, b, colorMain, colorMain.WithAlpha (0.5f));

        void GetQuadCornersForSpot (AreaVolumePoint spot, Vector3 direction)
        {
            System.Array.Clear (spotQuadCorners, 0, spotQuadCorners.Length);
            var compass = AreaSceneHelper.GetCompassFromDirection (direction);
            var corners = spotQuadCornerMap[(int)compass];
            var center = spot.instancePosition;
            for (var i = 0; i < corners.Length; i += 1)
            {
                spotQuadCorners[i] = center + corners[i];
            }
            spotQuadCorners[spotQuadCorners.Length - 1] = spotQuadCorners[0];
        }

        public AreaSceneGizmosCursor cursor { get; }
        public AreaSceneSearch search { get; } = new AreaSceneSearch ();

        public static void CreateInstance (AreaSceneBlackboard bb) => bb.gizmos = new AreaSceneGizmos (bb);

        AreaSceneGizmos (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            cursor = new AreaSceneGizmosCursor (bb);
        }

        readonly AreaSceneBlackboard bb;

        static readonly Vector3[] polylinePointsTemp = new Vector3[2];
        static readonly Vector3[] transferPreviewVerts = new Vector3[4];

        static readonly Vector3[] spotQuadCorners = new Vector3[5];
        static readonly Dictionary<int, Vector3[]> spotQuadCornerMap = new Dictionary<int, Vector3[]> ()
        {
            [(int)WorldSpace.Compass.Up] = new[]
            {
                new Vector3 (WorldSpace.HalfBlockSize, 0f, WorldSpace.HalfBlockSize),
                new Vector3 (-WorldSpace.HalfBlockSize, 0f, WorldSpace.HalfBlockSize),
                new Vector3 (-WorldSpace.HalfBlockSize, 0f, -WorldSpace.HalfBlockSize),
                new Vector3 (WorldSpace.HalfBlockSize, 0f, -WorldSpace.HalfBlockSize),
            },
            [(int)WorldSpace.Compass.Down] = new[]
            {
                new Vector3 (-WorldSpace.HalfBlockSize, 0f, WorldSpace.HalfBlockSize),
                new Vector3 (WorldSpace.HalfBlockSize, 0f, WorldSpace.HalfBlockSize),
                new Vector3 (WorldSpace.HalfBlockSize, 0f, -WorldSpace.HalfBlockSize),
                new Vector3 (-WorldSpace.HalfBlockSize, 0f, -WorldSpace.HalfBlockSize),
            },
            [(int)WorldSpace.Compass.North] = new[]
            {
                new Vector3 (-WorldSpace.HalfBlockSize, WorldSpace.HalfBlockSize, 0f),
                new Vector3 (WorldSpace.HalfBlockSize, WorldSpace.HalfBlockSize, 0f),
                new Vector3 (WorldSpace.HalfBlockSize, -WorldSpace.HalfBlockSize, 0f),
                new Vector3 (-WorldSpace.HalfBlockSize, -WorldSpace.HalfBlockSize, 0f),
            },
            [(int)WorldSpace.Compass.West] = new[]
            {
                new Vector3 (0f, WorldSpace.HalfBlockSize, WorldSpace.HalfBlockSize),
                new Vector3 (0f, WorldSpace.HalfBlockSize, -WorldSpace.HalfBlockSize),
                new Vector3 (0f, -WorldSpace.HalfBlockSize, -WorldSpace.HalfBlockSize),
                new Vector3 (0f, -WorldSpace.HalfBlockSize, WorldSpace.HalfBlockSize),
            },
            [(int)WorldSpace.Compass.South] = new[]
            {
                new Vector3 (WorldSpace.HalfBlockSize, WorldSpace.HalfBlockSize, 0f),
                new Vector3 (-WorldSpace.HalfBlockSize, WorldSpace.HalfBlockSize, 0f),
                new Vector3 (-WorldSpace.HalfBlockSize, -WorldSpace.HalfBlockSize, 0f),
                new Vector3 (WorldSpace.HalfBlockSize, -WorldSpace.HalfBlockSize, 0f),
            },
            [(int)WorldSpace.Compass.East] = new[]
            {
                new Vector3 (0f, WorldSpace.HalfBlockSize, -WorldSpace.HalfBlockSize),
                new Vector3 (0f, WorldSpace.HalfBlockSize, WorldSpace.HalfBlockSize),
                new Vector3 (0f, -WorldSpace.HalfBlockSize, WorldSpace.HalfBlockSize),
                new Vector3 (0f, -WorldSpace.HalfBlockSize, -WorldSpace.HalfBlockSize),
            },
        };

        public static class VolumeSelectionColor
        {
            public static readonly Color Standard = Color.white;
            public static readonly Color Limited = Color.gray;
            public static readonly Color Warning = Color.red;
        }
    }
}
