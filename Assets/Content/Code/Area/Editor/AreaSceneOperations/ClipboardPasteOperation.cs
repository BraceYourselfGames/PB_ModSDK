using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Area
{
    static class ClipboardPasteOperation
    {
        public enum ApplicationMode
        {
            Overwrite,
            Additive,
            Subtractive,
        }

        public static ApplicationMode applicationMode = ApplicationMode.Overwrite;

        public static void PasteVolume (AreaManager am, bool punchDown, bool checkPropCompatibility, bool log = false)
        {
            if (!am.clipboard.IsValid)
            {
                Debug.Log ("PasteVolume | Clipboard is empty");
                return;
            }

            var pvi = AnalyzePasteVolume (am, log);
            if (!pvi.OK)
            {
                return;
            }

            // Since we are directly modifying boundary points, some spots outside of captured area would be affected - time to collect all points
            var (affectedPoints, affectedBounds) = GetAffectedPoints (am, pvi.Origin, pvi.Corner, log);
            if (affectedBounds.x == 0 && affectedBounds.y == 0 && affectedBounds.z == 0)
            {
                return;
            }

            if (am.transferVolume)
            {
                var pointsCountTotal = am.points.Count;
                // First, override only point state - we want to update all the configurations before looking into applying other spot-related data
                UpdatePointStateOnPaste (am, pvi.Origin, am.clipboard.clipboardPointsSaved, pointsCountTotal, log);

                // Update all spots
                var topChanged = false;
                var sliceSize = affectedBounds.x * affectedBounds.z;
                var topStop = sliceSize;
                var bottomStart = affectedPoints.Count - sliceSize;
                var bottomConfiguration = new byte[sliceSize];
                for (var i = 0; i < affectedPoints.Count; i += 1)
                {
                    var point = affectedPoints[i];
                    var configuration = point.spotConfiguration;
                    if (bottomStart <= i)
                    {
                        bottomConfiguration[i % sliceSize] = configuration;
                    }
                    am.UpdateSpotAtPoint (point, false, false, true);
                    var changed = configuration != point.spotConfiguration;
                    topChanged |= i < topStop && changed;
                    if (am.debugPasteDrawHighlights)
                    {
                        am.DrawHighlightBox (affectedPoints[i].instancePosition, Color.red, 10f, 0.5f);
                    }
                }

                // Now we can take a look at where spot data can be overwritten
                OverwriteSpotsOnPaste (am, pvi.Origin, pointsCountTotal, log);

                // If the bottom of the paste volume is below the target surface and that surface has terrain tiles,
                // it will tear a hole in the terrain. The punched out terrain tiles have to be replaced with another
                // tileset.
                var phSpec = new PasteHoleSpec ()
                {
                    AreaManager = am,
                    PasteOrigin = pvi.Origin,
                    PasteOriginIndex = pvi.StartIndex,
                    PasteBounds = pvi.Bounds,
                    PasteCorner = pvi.Corner,
                    PasteCornerIndex = pvi.EndIndex,
                    AffectedPoints = affectedPoints,
                    AffectedBounds = affectedBounds,
                    AffectedBottomConfiguration = bottomConfiguration,
                    AffectedTopChanged = topChanged,
                    PunchDown = punchDown,
                    Log = log,
                };
                FillPasteHole (phSpec);


                // Finally, it's time to push full updates
                for (var i = 0; i < affectedPoints.Count; i += 1)
                {
                    am.RebuildBlock (affectedPoints[i], false);
                    am.RebuildCollisionsAroundIndex (affectedPoints[i].spotIndex);
                }
            }

            if (am.transferProps && (applicationMode == ApplicationMode.Overwrite || applicationMode == ApplicationMode.Additive))
            {
                PasteProps (am, pvi.Origin, affectedPoints, checkPropCompatibility, log);
            }

            var sceneHelper = CombatSceneHelper.ins;
            sceneHelper.terrain.Rebuild (true);
        }

        sealed class PasteVolumeInfo
        {
            public bool OK;
            public Vector3Int Origin;
            public Vector3Int Bounds;
            public Vector3Int Corner;
            public int StartIndex;
            public int EndIndex;
        }

        static PasteVolumeInfo AnalyzePasteVolume (AreaManager am, bool log)
        {
            var info = new PasteVolumeInfo ();
            var cornerA = am.targetOrigin;
            var bounds = am.clipboard.clipboardBoundsSaved;
            // Bounds are not positions, they are limits, like array sizes - so we need to subtract 1 from bounds axes to get second corner
            var cornerB = cornerA + bounds + Vector3Int.size1x1x1Neg;
            var indexA = AreaUtility.GetIndexFromVolumePosition (cornerA, am.boundsFull);
            var indexB = AreaUtility.GetIndexFromVolumePosition (cornerB, am.boundsFull);

            if (indexA == -1 || indexB == -1)
            {
                Debug.LogWarningFormat
                (
                    "PasteVolume | Failed to paste due to specified corner {0} or calculated corner {1} falling outside of target level bounds {2}",
                    cornerA,
                    cornerB,
                    am.boundsFull
                );
                return info;
            }

            if (log)
            {
                var gridSize = bounds.x * bounds.z;
                Debug.LogFormat
                (
                    "Paste volume | origin: {0}/{1} | bounds: {2}/{3} | corner: {4}/{5} | grid size: {6}",
                    indexA,
                    cornerA,
                    gridSize * bounds.y,
                    bounds,
                    indexB,
                    cornerB,
                    gridSize);
            }
            info.OK = true;
            info.Origin = cornerA;
            info.Bounds = bounds;
            info.Corner = cornerB;
            info.StartIndex = indexA;
            info.EndIndex = indexB;
            return info;
        }

        static (List<AreaVolumePoint> Points, Vector3Int Bounds) GetAffectedPoints (AreaManager am, Vector3Int cornerA, Vector3Int cornerB, bool log)
        {
            var affectedOriginX = Mathf.Max (0, cornerA.x - 1);
            var affectedOriginY = Mathf.Max (0, cornerA.y - 1);
            var affectedOriginZ = Mathf.Max (0, cornerA.z - 1);
            var cornerAShifted = new Vector3Int (affectedOriginX, affectedOriginY, affectedOriginZ);

            // Bounds are limits, like array sizes - so they will be bigger by 1 than just pure position difference
            // For example, a set of points [(0,0);(1,0);(0,1);(1;1)] has bounds of 2x2, not 1x1!
            var affectedBoundsX = cornerB.x - cornerAShifted.x + 1;
            var affectedBoundsY = cornerB.y - cornerAShifted.y + 1;
            var affectedBoundsZ = cornerB.z - cornerAShifted.z + 1;
            var affectedVolumeLength = affectedBoundsX * affectedBoundsY * affectedBoundsZ;
            var affectedBounds = new Vector3Int (affectedBoundsX, affectedBoundsY, affectedBoundsZ);
            var affectedPoints = new List<AreaVolumePoint> (affectedVolumeLength);

            var lastX = am.boundsFull.x - 1;
            var lastZ = am.boundsFull.z - 1;
            var includesNorthBorder = false;
            var includesEastBorder = false;
            for (var i = 0; i < affectedVolumeLength; i += 1)
            {
                var affectedPointPosition = AreaUtility.GetVolumePositionFromIndex (i, affectedBounds);
                var sourcePointPosition = affectedPointPosition + cornerAShifted;
                var sourcePointIndex = AreaUtility.GetIndexFromVolumePosition (sourcePointPosition, am.boundsFull);
                if (sourcePointIndex == -1)
                {
                    continue;
                }
                includesNorthBorder = sourcePointPosition.z == lastZ;
                includesEastBorder = sourcePointPosition.x == lastX;
                var sourcePoint = am.points[sourcePointIndex];
                affectedPoints.Add (sourcePoint);
                if (am.debugPasteDrawHighlights)
                {
                    Debug.DrawLine (sourcePoint.pointPositionLocal, sourcePoint.instancePosition, Color.white, 10f);
                }
            }
            if (affectedPoints.Count != affectedVolumeLength)
            {
                var count = affectedPoints.Count;
                var x = count % affectedBoundsX;
                var z = count / affectedBoundsX % affectedBoundsZ;
                var y = count / affectedBoundsY;
                affectedBounds = new Vector3Int (x, y, z);
            }
            if (includesNorthBorder)
            {
                affectedBounds.z -= 1;
            }
            if (includesEastBorder)
            {
                affectedBounds.x -= 1;
            }

            if (log)
            {
                Debug.LogFormat
                (
                    "Pasting volume | {0}\nTarget points/origin/bounds/corner: {1}/{2}/{3}/{4}\nAffected points/origin/bounds/corner: {5}/{6}/{7}/{8}",
                    applicationMode,
                    am.clipboard.clipboardPointsSaved.Count,
                    cornerA,
                    am.clipboard.clipboardBoundsSaved,
                    cornerB,
                    affectedPoints.Count,
                    cornerAShifted,
                    affectedBounds,
                    cornerAShifted + affectedBounds + Vector3Int.size1x1x1Neg
                );
            }

            return (affectedPoints, affectedBounds);
        }

        static void UpdatePointStateOnPaste (AreaManager am, Vector3Int cornerA, List<AreaVolumePoint> sourcePoints, int pointsCountTotal, bool log)
        {
            var changedPoints = log ? new List<(AreaVolumePointState State, AreaVolumePoint Point)> () : null;
            for (var i = 0; i < sourcePoints.Count; i += 1)
            {
                var sourcePoint = sourcePoints[i];
                var targetPointPosition = sourcePoint.pointPositionIndex + cornerA;
                var targetPointIndex = AreaUtility.GetIndexFromVolumePosition (targetPointPosition, am.boundsFull);

                if (targetPointIndex < 0 || targetPointIndex >= pointsCountTotal)
                {
                    if (log)
                    {
                        Debug.LogWarningFormat
                        (
                            "Failed to apply source point {0} to area point {1}: out of bounds | Point position: {2} | corner: {3}",
                            i,
                            targetPointIndex,
                            sourcePoint.pointPositionIndex,
                            cornerA
                        );
                    }
                    continue;
                }

                var targetPoint = am.points[targetPointIndex];
                var ts = targetPoint.pointState;
                if (am.debugPasteDrawHighlights && sourcePoint.pointState != AreaVolumePointState.Empty)
                {
                    am.DrawHighlightBox (targetPoint.pointPositionLocal, Color.cyan, 10f);
                }

                switch (applicationMode)
                {
                    case ApplicationMode.Additive:
                    {
                        if (targetPoint.pointState == AreaVolumePointState.Empty && sourcePoint.pointState != AreaVolumePointState.Empty)
                        {
                            targetPoint.pointState = sourcePoint.pointState;
                        }
                        break;
                    }
                    case ApplicationMode.Subtractive:
                    {
                        if (targetPoint.pointState != AreaVolumePointState.Empty && sourcePoint.pointState == AreaVolumePointState.Empty)
                        {
                            targetPoint.pointState = sourcePoint.pointState;
                        }
                        break;
                    }
                    default:
                        targetPoint.pointState = sourcePoint.pointState;
                        break;
                }
                if (log && ts != targetPoint.pointState)
                {
                    changedPoints.Add ((ts, targetPoint));
                }
            }

            if (log && changedPoints.Count != 0)
            {
                Debug.Log
                (
                    "Points with state change (" + applicationMode + "):\n"
                    + changedPoints.Select
                    (
                        x => string.Format
                        (
                            "{0}/{1} {2} --> {3}",
                            x.Point.spotIndex,
                            x.Point.pointPositionIndex,
                            x.State,
                            x.Point.pointState
                        )
                    ).ToStringFormatted (true)
                );
            }
        }

        static void OverwriteSpotsOnPaste (AreaManager am, Vector3Int cornerA, int pointsCountTotal, bool log)
        {
            var overwrittenSpots = log ? new List<AreaVolumePoint> () : null;
            for (var i = 0; i < am.clipboard.clipboardPointsSaved.Count; i += 1)
            {
                var clipboardPoint = am.clipboard.clipboardPointsSaved[i];
                var targetPointPosition = clipboardPoint.pointPositionIndex + cornerA;
                var targetPointIndex = AreaUtility.GetIndexFromVolumePosition (targetPointPosition, am.boundsFull);

                if (targetPointIndex < 0 || targetPointIndex >= pointsCountTotal)
                {
                    if (log)
                    {
                        Debug.LogWarningFormat ("Failed to apply clipboard point {0} to area point {1}: out of bounds", i, targetPointIndex);
                    }
                    continue;
                }

                var targetPoint = am.points[targetPointIndex];

                // We don't want to affect areas outside of clipboard bounds
                // (points on positive edges of bounds cube control spots that lie outside of our volume)
                var inside =
                    clipboardPoint.pointPositionIndex.x < am.clipboard.clipboardBoundsSaved.x - 1 &&
                    clipboardPoint.pointPositionIndex.y < am.clipboard.clipboardBoundsSaved.y - 1 &&
                    clipboardPoint.pointPositionIndex.z < am.clipboard.clipboardBoundsSaved.z - 1;

                // We also don't want to write to empty points or points with different configurations
                var overwriteSpot =
                    inside &&
                    targetPoint.spotPresent &&
                    clipboardPoint.spotConfiguration == targetPoint.spotConfiguration;

                if (!overwriteSpot)
                {
                    if (am.debugPasteDrawHighlights)
                    {
                        am.DrawHighlightBox (targetPoint.instancePosition, Color.yellow, 10f, 1.5f);
                    }
                    continue;
                }

                targetPoint.blockFlippedHorizontally = clipboardPoint.blockFlippedHorizontally;
                targetPoint.blockGroup = clipboardPoint.blockGroup;
                targetPoint.blockRotation = clipboardPoint.blockRotation;
                targetPoint.blockSubtype = clipboardPoint.blockSubtype;
                targetPoint.blockTileset = clipboardPoint.blockTileset;
                targetPoint.customization = clipboardPoint.customization;
                targetPoint.terrainOffset = clipboardPoint.terrainOffset;

                if (am.debugPasteDrawHighlights)
                {
                    Debug.DrawLine (targetPoint.pointPositionLocal, targetPoint.instancePosition, Color.white, 10f);
                }
                if (log)
                {
                    overwrittenSpots.Add (targetPoint);
                }
            }
            if (log && overwrittenSpots.Count != 0)
            {
                Debug.Log ("Spots overwritten: " + overwrittenSpots.Select (pt => pt.spotIndex + "/" + pt.pointPositionIndex).ToStringFormatted ());
            }
        }

        sealed class PasteHoleSpec
        {
            public AreaManager AreaManager;
            public Vector3Int PasteOrigin;
            public int PasteOriginIndex;
            public Vector3Int PasteBounds;
            public Vector3Int PasteCorner;
            public int PasteCornerIndex;
            public List<AreaVolumePoint> AffectedPoints;
            public Vector3Int AffectedBounds;
            public byte[] AffectedBottomConfiguration;
            public bool AffectedTopChanged;
            public bool PunchDown;
            public bool Log;
        }

        static void FillPasteHole (PasteHoleSpec spec)
        {
            var log = spec.Log;
            var affectedPoints = spec.AffectedPoints;
            var affectedBounds = spec.AffectedBounds;
            var gridSize = affectedBounds.x * affectedBounds.z;
            if (log)
            {
                var affectedLast = affectedPoints.Last ();
                Debug.LogFormat
                (
                    "Paste grid ({0}): {1} x {2} | start corner: {3}/{4} | end corner: {5}/{6}\nAffected grid ({7}): {8} x {9} | start corner: {10}/{11} | end corner: {12}/{13}",
                    spec.PasteBounds.x * spec.PasteBounds.z,
                    spec.PasteBounds.x,
                    spec.PasteBounds.z,
                    spec.PasteOriginIndex,
                    spec.PasteOrigin,
                    spec.PasteCornerIndex,
                    spec.PasteCorner,
                    gridSize,
                    affectedBounds.x,
                    affectedBounds.z,
                    affectedPoints[0].spotIndex,
                    affectedPoints[0].pointPositionIndex,
                    affectedLast.spotIndex,
                    affectedLast.pointPositionIndex
                );
            }

            var tdSpec = new TrickleDownSpec ()
            {
                AreaManager = spec.AreaManager,
                AffectedPoints = affectedPoints,
                AffectedBounds = affectedBounds,
                AffectedBottomConfiguration = spec.AffectedBottomConfiguration,
                Log = spec.Log,
            };
            TrickleDown (tdSpec);

            var poSpec = new PunchOutSpec ()
            {
                AreaManager = spec.AreaManager,
                AffectedPoints = affectedPoints,
                TilesetGrid = new int[gridSize],
                TerminalGrid = new int[gridSize],
                GridRowSize = affectedBounds.x,
                GridColumnSize = affectedBounds.z,
                StackedTerrain = new HashSet<int> (),
                Log = log,
            };
            var layerStart = affectedPoints[0].pointPositionIndex.y;
            for (var i = affectedPoints.Count - 1; i >= 0; i -= 1)
            {
                var gridIndex = i % gridSize;
                var gridLayer = i / gridSize;
                poSpec.Layer = layerStart + gridLayer;
                poSpec.Index = i;
                poSpec.GridIndex = gridIndex;
                ReplacePunchOutTile (poSpec);
            }

            if (!spec.AffectedTopChanged || !spec.PunchDown)
            {
                if (log)
                {
                    var msg = "FillPasteHole return on no " + (!spec.AffectedTopChanged ? "top configuration change" : "punch down");
                    Debug.Log (msg);
                }
                return;
            }

            // It's possible to paste a volume deep down. If the top of the volume is below any surrounding
            // surface tiles, we have to fill the hole up to those surface tiles.

            // Propagate the state of the points at the top of the copy volume up to the top of the hole.
            var am = spec.AreaManager;
            var sourcePoints = new List<(int, AreaVolumePoint)> ();
            for (var z = 0; z < am.clipboard.clipboardBoundsSaved.z; z += 1)
            {
                var rowStart = z * am.clipboard.clipboardBoundsSaved.x;
                var gridRowStart = 1 + (1 + z) * affectedBounds.x;
                for (var x = 0; x < am.clipboard.clipboardBoundsSaved.x; x += 1)
                {
                    var gridIndex = x + gridRowStart;
                    sourcePoints.Add ((gridIndex, am.clipboard.clipboardPointsSaved[x + rowStart]));
                }
            }
            poSpec.SourcePoints = sourcePoints;
            poSpec.SourceOrigin = spec.PasteOrigin;

            // Update all the point state until we hit a layer with the region filled by all empty
            // or we reach the top of the level.
            BuildTerminalGrid (poSpec);
            var yStop = UpdateHolePointState (poSpec);

            // Restart at the top of the copy volume and change spots in the hole.
            poSpec.AffectedPoints = am.points;
            poSpec.HoleScan = true;
            var rowSize = am.boundsFull.x;
            var layerSize = rowSize * am.boundsFull.z;
            var anchorIndex = affectedPoints[0].spotIndex;
            var affectedCount = affectedPoints.Count;
            for (var y = affectedPoints[0].pointPositionIndex.y; y > yStop; y -= 1)
            {
                if (log)
                {
                    var startCorner = affectedPoints[0].pointPositionIndex;
                    var endSpot = affectedPoints.Skip (gridSize - 1).First ();
                    var endIndex = endSpot.spotIndex + (y - startCorner.y) * layerSize;
                    var endCorner = endSpot.pointPositionIndex;
                    startCorner.y = y;
                    endCorner.y = y;
                    Debug.LogFormat
                    (
                        "Hole scan | y: {0} | start spot: {1}/{2} | end spot: {3}/{4}",
                        y,
                        anchorIndex,
                        startCorner,
                        endIndex,
                        endCorner
                    );
                }

                for (var z = 0; z < affectedBounds.z; z += 1)
                {
                    var rowStart = z * rowSize;
                    for (var x = 0; x < affectedBounds.x; x += 1)
                    {
                        var index = anchorIndex + x + rowStart;
                        var point = am.points[index];
                        am.UpdateSpotAtPoint (point, false, false, true);
                        affectedPoints.Add (point);
                    }
                }

                for (var z = 0; z < affectedBounds.z; z += 1)
                {
                    var rowStart = z * rowSize;
                    var gridRowStart = z * affectedBounds.x;
                    for (var x = 0; x < affectedBounds.x; x += 1)
                    {
                        poSpec.Layer = y;
                        poSpec.Index = anchorIndex + x + rowStart;
                        poSpec.GridIndex = x + gridRowStart;
                        ReplacePunchOutTile (poSpec);
                    }
                }
                anchorIndex -= layerSize;
            }

            if (poSpec.StackedTerrain.Count != 0)
            {
                // XXX need to handled stacked terrain.
                Debug.LogWarning ("Found stacked terrain tiles | count: " + poSpec.StackedTerrain.Count);
            }

            if (affectedCount == affectedPoints.Count)
            {
                return;
            }
            if (log)
            {
                var addedCount = affectedPoints.Count - affectedCount;
                Debug.Log ("Added points in hole scan (" + addedCount + "): " + affectedPoints.Skip (affectedCount).Select (pt => pt.spotIndex + "/" + pt.pointPositionIndex).ToStringFormatted ());
            }
            affectedPoints.Sort (OrderByIndex);
        }

        static int OrderByIndex (AreaVolumePoint lhs, AreaVolumePoint rhs) => lhs.spotIndex.CompareTo (rhs.spotIndex);

        sealed class TrickleDownSpec
        {
            public AreaManager AreaManager;
            public List<AreaVolumePoint> AffectedPoints;
            public Vector3Int AffectedBounds;
            public byte[] AffectedBottomConfiguration;
            public bool Log;
        }

        static void TrickleDown (TrickleDownSpec spec)
        {
            var points = spec.AffectedPoints;
            var pointCount = points.Count;
            var bottomConfiguration = spec.AffectedBottomConfiguration;
            var pasteBottom = points.Last().pointPositionIndex.y;
            var levelBottom = spec.AreaManager.boundsFull.y;
            var gridSize = spec.AffectedBounds.x * spec.AffectedBounds.z;
            var terminalGrid = new int[gridSize];
            if (spec.Log)
            {
                Debug.LogFormat ("Trickle down scan affected bottom | layer: {0} | index: {1} ", pasteBottom, points[pointCount - gridSize].spotIndex);
            }
            for (var index = pointCount - gridSize; index < pointCount; index += 1)
            {
                var point = points[index];
                var gridIndex = index % gridSize;
                var configuration = bottomConfiguration[gridIndex];
                var unchanged = bottomConfiguration[gridIndex] == point.spotConfiguration;
                var bottomStop = unchanged && point.spotConfiguration == TilesetUtility.configurationFull;
                if (AreaManager.IsPointTerrain (point))
                {
                    AreaManager.ClearTileData (point);
                    if (spec.Log)
                    {
                        Debug.Log ("Trickle down cleared terrain tile in affected bottom | spot: " + point.spotIndex + "/" + point.pointPositionIndex);
                    }
                }
                terminalGrid[gridIndex] = bottomStop ? pasteBottom : levelBottom;
                if (spec.Log)
                {
                    var rx = gridIndex % spec.AffectedBounds.x;
                    var rz = gridIndex / spec.AffectedBounds.z;
                    if (unchanged)
                    {
                        Debug.LogFormat
                        (
                            "{0}/({1}, {2}) {3}/{4} {5} {6}",
                            gridIndex,
                            rx,
                            rz,
                            point.spotIndex,
                            point.pointPositionIndex,
                            TilesetUtility.GetStringFromConfiguration (configuration),
                            bottomStop ? "bs " + pasteBottom : ""
                        );
                    }
                    else
                    {
                        Debug.LogFormat
                        (
                            "{0}/({1}, {2}) {3}/{4} {5} --> {6}",
                            gridIndex,
                            rx,
                            rz,
                            point.spotIndex,
                            point.pointPositionIndex,
                            TilesetUtility.GetStringFromConfiguration (configuration),
                            TilesetUtility.GetStringFromConfiguration (point.spotConfiguration)
                        );
                    }
                }
            }

            var rowSize = spec.AreaManager.boundsFull.x;
            var layerSize = rowSize * spec.AreaManager.boundsFull.z;
            var startIndex = points[pointCount - gridSize].spotIndex + layerSize;
            var yStart = pasteBottom + 1;
            if (spec.Log)
            {
                var gridRowSize = spec.AffectedBounds.x;
                Debug.LogFormat
                (
                    "Trickle down start | layer: {0} | index: {1} | terminals:\n{2}",
                    yStart,
                    startIndex,
                    terminalGrid
                        .Select ((g, i) => string.Format ("{0}/({1}, {2})/{3}", i, i % gridRowSize, i / gridRowSize, g))
                        .ToStringFormatted (true)
                );
            }

            var candidates = new List<AreaVolumePoint> ();
            points = spec.AreaManager.points;
            for (var y = pasteBottom + 1; y < levelBottom; y += 1, startIndex += layerSize)
            {
                var bottomedOut = true;
                candidates.Clear ();
                for (var z = 0; z < spec.AffectedBounds.z; z += 1)
                {
                    var rowStart = startIndex + z * rowSize;
                    var gridRowStart = z * spec.AffectedBounds.x;
                    for (var x = 0; x < spec.AffectedBounds.x; x += 1)
                    {
                        var index = x + rowStart;
                        if (index >= points.Count)
                        {
                            if (spec.Log)
                            {
                                var gridRowSize = spec.AffectedBounds.x;
                                Debug.LogFormat
                                (
                                    "Trickle down reached end of points before level bottom | layer: {0} | terminals:\n{1}",
                                    y,
                                    terminalGrid
                                        .Select ((g, i) => string.Format ("{0}/({1}, {2})/{3}", i, i % gridRowSize, i / gridRowSize, g))
                                        .ToStringFormatted (true)
                                );
                            }
                            return;
                        }

                        var point = points[index];
                        candidates.Add (point);

                        var gridIndex = x + gridRowStart;
                        if (y >= terminalGrid[gridIndex])
                        {
                            continue;
                        }
                        bottomedOut = false;

                        var configuration = point.spotConfiguration;
                        spec.AreaManager.UpdateSpotAtPoint (point, false, false, true);
                        var changed = configuration != point.spotConfiguration;
                        if (!changed && point.spotConfiguration == TilesetUtility.configurationFull)
                        {
                            terminalGrid[gridIndex] = y;
                            continue;
                        }
                        if (!AreaManager.IsPointTerrain (point))
                        {
                            continue;
                        }
                        //if (point.spotConfiguration == TilesetUtility.configurationFloor)
                        if ((point.spotConfiguration & TilesetUtility.configurationTopMask) == 0)
                        {
                            continue;
                        }
                        var indexNeighborUp = point.spotIndex - layerSize;
                        if (indexNeighborUp >= 0)
                        {
                            var neighborUp = points[indexNeighborUp];
                            if (AreaManager.IsPointTerrain (neighborUp) && ((point.spotConfiguration & TilesetUtility.configurationTopMask) >> 4 == (neighborUp.spotConfiguration & TilesetUtility.configurationFloor)))
                            {
                                // This is a slope where the spot above aligns with the spot below.
                                continue;
                            }
                        }
                        AreaManager.ClearTileData (point);
                        if (spec.Log)
                        {
                            Debug.Log ("Trickle down cleared terrain tile | spot: " + point.spotIndex + "/" + point.pointPositionIndex);
                        }
                    }
                }
                if (bottomedOut)
                {
                    if (spec.Log)
                    {
                        var gridRowSize = spec.AffectedBounds.x;
                        Debug.LogFormat
                        (
                            "Trickle down state change stopped at layer | layer: {0} | terminals:\n{1}",
                            y,
                            terminalGrid
                                .Select ((g, i) => string.Format ("{0}/({1}, {2})/{3}", i, i % gridRowSize, i / gridRowSize, g))
                                .ToStringFormatted (true)
                        );
                    }
                    return;
                }
                spec.AffectedPoints.AddRange (candidates);
            }

            if (spec.Log)
            {
                var gridRowSize = spec.AffectedBounds.x;
                Debug.Log ("Trickle down reached level bottom | terminals:\n" +
                    terminalGrid
                        .Select ((g, i) => string.Format ("{0}/({1}, {2})/{3}", i, i % gridRowSize, i / gridRowSize, g))
                        .ToStringFormatted (true));
            }
        }

        sealed class PunchOutSpec
        {
            public AreaManager AreaManager;
            public int Layer;
            public int Index;
            public int GridIndex;
            public bool HoleScan;
            public List<AreaVolumePoint> AffectedPoints;
            public List<(int, AreaVolumePoint)> SourcePoints;
            public Vector3Int SourceOrigin;
            public int[] TilesetGrid;
            public int[] TerminalGrid;
            public int GridRowSize;
            public int GridColumnSize;
            public HashSet<int> StackedTerrain;
            public bool Log;
        }

        const int gridTilesetMask = 0x00FFFFFF;

        static void ReplacePunchOutTile (PunchOutSpec spec)
        {
            var log = spec.Log;
            var grid = spec.TilesetGrid;
            var gridIndex = spec.GridIndex;
            var point = spec.AffectedPoints[spec.Index];
            if (!point.spotPresent)
            {
                return;
            }
            var tilesetID = grid[gridIndex] & gridTilesetMask;
            if (spec.HoleScan && spec.TerminalGrid[gridIndex] >= spec.Layer)
            {
                if (AreaManager.IsPointTerrain (point)
                    && (point.spotConfiguration & TilesetUtility.configurationFloor) != TilesetUtility.configurationFloor
                    && tilesetID != AreaTilesetHelper.idOfTerrain)
                {
                    point.blockTileset = grid[gridIndex];
                    point.terrainOffset = 0f;
                    if (spec.Log)
                    {
                        Debug.LogFormat ("Changed terrain tile above terminal | spot: {0}/{1} | tileset: {2} --> {3}", point.spotIndex, point.pointPositionIndex, AreaTilesetHelper.idOfTerrain, grid[gridIndex]);
                    }
                }
                return;
            }
            if (point.spotConfiguration == TilesetUtility.configurationEmpty || point.spotConfiguration == TilesetUtility.configurationFull)
            {
                if (point.blockTileset != 0)
                {
                    AreaManager.ClearTileData (point);
                    point.terrainOffset = 0f;
                    if (log)
                    {
                        Debug.LogFormat
                        (
                            "Cleared tile data from {0} spot | spot: {1}/{2}",
                            point.spotConfiguration == TilesetUtility.configurationEmpty ? "empty" : "full",
                            point.spotIndex,
                            point.pointPositionIndex
                        );
                    }
                }
                grid[gridIndex] = 0;
                return;
            }
            if (point.blockTileset != 0)
            {
                if (AreaManager.IsPointTerrain (point))
                {
                    TryReplaceTerrainTile (spec, point, tilesetID, gridIndex);
                    return;
                }
                grid[gridIndex] = point.blockTileset;
                return;
            }
            if (tilesetID != 0)
            {
                point.blockTileset = tilesetID;
                if (log)
                {
                    Debug.LogFormat ("Resolved tile | spot: {0}/{1} | tileset: {2}", point.spotIndex, point.pointPositionIndex, tilesetID);
                }
                return;
            }
            point.blockTileset = AreaTilesetHelper.idOfFallback;
            grid[gridIndex] = point.blockTileset;
            if (log)
            {
                var gridRowSize = spec.GridRowSize;
                Debug.LogFormat
                (
                    "Using fallback tileset | spot: {0}/{1} | tileset: {2} | grid: {3}/({4}, {5})",
                    point.spotIndex,
                    point.pointPositionIndex,
                    AreaTilesetHelper.idOfFallback,
                    gridIndex,
                    gridIndex % gridRowSize,
                    gridIndex / gridRowSize
                );
            }
        }

        static void TryReplaceTerrainTile (PunchOutSpec spec, AreaVolumePoint point, int tilesetID, int gridIndex)
        {
            if (tilesetID == 0)
            {
                if ((point.spotConfiguration & TilesetUtility.configurationTopMask) != 0)
                {
                    // Propagate terrain tileset only if it appears to be a slope.
                    spec.TilesetGrid[gridIndex] = point.blockTileset | point.spotConfiguration << 24;
                }
                return;
            }
            if (tilesetID == AreaTilesetHelper.idOfTerrain)
            {
                var spotConfiguration = (byte)(spec.TilesetGrid[gridIndex] >> 24 & 0xFF);
                if ((spotConfiguration & TilesetUtility.configurationTopMask) >> 4 == (point.spotConfiguration & TilesetUtility.configurationFloor))
                {
                    var topless = (point.spotConfiguration & TilesetUtility.configurationTopMask) == 0;
                    if (topless)
                    {
                        spec.TilesetGrid[gridIndex] = 0;
                        return;
                    }
                    // This is a slope where the spot above aligns with the spot below. Replace the configuration in the grid
                    // so we can continue to follow the slope up.
                    spec.TilesetGrid[gridIndex] = tilesetID | point.spotConfiguration << 24;
                    return;
                }
                var added = spec.StackedTerrain.Add (gridIndex);
                if (spec.Log && added)
                {
                    var gridRowSize = spec.GridRowSize;
                    Debug.LogWarningFormat
                    (
                        "Found stacked terrain tile | spot: {0}/{1} | grid: {2}/({3}, {4}) | grid configuration: {5} | point configuration: {6}",
                        point.spotIndex,
                        point.pointPositionIndex,
                        gridIndex,
                        gridIndex % gridRowSize,
                        gridIndex / gridRowSize,
                        TilesetUtility.GetStringFromConfiguration (spotConfiguration),
                        TilesetUtility.GetStringFromConfiguration (point.spotConfiguration)
                    );
                }
                return;
            }
            point.blockTileset = tilesetID;
            point.terrainOffset = 0f;
            if (spec.Log)
            {
                Debug.LogFormat ("Changed terrain tile | spot: {0}/{1} | tileset: {2} --> {3}", point.spotIndex, point.pointPositionIndex, AreaTilesetHelper.idOfTerrain, tilesetID);
            }
        }

        static void BuildTerminalGrid (PunchOutSpec spec)
        {
            const int downEastNeighborBit = 0x04;
            const int eastNeighborBit = 0x40;

            var log = spec.Log;
            var am = spec.AreaManager;
            var rowSize = am.boundsFull.x;
            var layerSize = rowSize * am.boundsFull.z;
            var anchorIndex = spec.AffectedPoints[0].spotIndex - layerSize;
            var xClip = spec.GridRowSize - 1;
            var yStop = spec.AffectedPoints[0].pointPositionIndex.y - 1;
            var terminateDown = true;
            for (var y = yStop; y > 0; y -= 1)
            {
                for (var z = 0; z < spec.GridColumnSize; z += 1)
                {
                    var rowStart = z * rowSize;
                    var gridRowStart = z * spec.GridRowSize;
                    for (var x = 0; x < spec.GridRowSize; x += 1)
                    {
                        var gridIndex = x + gridRowStart;
                        var terminal = spec.TerminalGrid[gridIndex];
                        if (terminal > y)
                        {
                            continue;
                        }
                        var index = anchorIndex + x + rowStart;
                        var point = am.points[index];
                        if (point.spotConfiguration == TilesetUtility.configurationFull)
                        {
                            continue;
                        }
                        if (AreaManager.IsPointTerrain (point))
                        {
                            continue;
                        }
                        if (point.spotConfiguration == TilesetUtility.configurationEmpty && point.blockTileset == 0)
                        {
                            spec.TerminalGrid[gridIndex] = y;
                            if (log)
                            {
                                LogSpotTerminal (point, spec.GridRowSize, gridIndex, spec.TerminalGrid[gridIndex]);
                            }
                            continue;
                        }
                        terminal = terminateDown ? y + 1 : y;
                        spec.TerminalGrid[gridIndex] = terminal;
                        if (log)
                        {
                            LogSpotTerminal (point, spec.GridRowSize, gridIndex, spec.TerminalGrid[gridIndex]);
                        }
                        if (x >= xClip || z == 0)
                        {
                            continue;
                        }
                        var neighborBit = terminateDown ? downEastNeighborBit : eastNeighborBit;
                        if ((point.spotConfiguration & neighborBit) != neighborBit)
                        {
                            continue;
                        }
                        spec.TerminalGrid[gridIndex + 1] = terminal;
                        if (log)
                        {
                            LogSpotTerminal (point, spec.GridRowSize, gridIndex, spec.TerminalGrid[gridIndex]);
                        }
                    }
                }
                terminateDown = false;
            }

            if (log)
            {
                Debug.LogFormat
                (
                    "Terminal scan ({0}):\n{1}",
                    spec.TerminalGrid.Count (g => g != 0),
                    spec.TerminalGrid
                        .Select ((g, i) => new
                        {
                            Index = i,
                            Y = g,
                        })
                        .Where (x => x.Y != 0)
                        .Select (x => string.Format ("{0}/({1}, {2})/{3}", x.Index, x.Index % spec.GridRowSize, x.Index / spec.GridRowSize, x.Y))
                        .ToStringFormatted (true)
                );
            }
        }

        static void LogSpotTerminal (AreaVolumePoint point, int gridRowSize, int gridIndex, int yStop)
        {
            Debug.LogFormat
            (
                "Terminal spot reached in state scan | spot: {0}/{1} | tileset: {2} | configuration: {3} | grid: {4}/({5}, {6}) | stop: {7})",
                point.spotIndex,
                point.pointPositionIndex,
                point.blockTileset,
                TilesetUtility.GetStringFromConfiguration (point.spotConfiguration),
                gridIndex,
                gridIndex % gridRowSize,
                gridIndex / gridRowSize,
                yStop
            );
        }

        static int UpdateHolePointState (PunchOutSpec spec)
        {
            if (spec.AffectedPoints[0].pointPositionIndex.y == 0)
            {
                Debug.Log ("Update hole point state -- early exit because paste volume top is on layer 0");
                return 0;
            }

            var log = spec.Log;
            var rowSize = spec.AreaManager.boundsFull.x;
            var layerSize = rowSize * spec.AreaManager.boundsFull.z;
            var anchorIndex = spec.AffectedPoints[0].spotIndex - layerSize;
            var yStop = spec.AffectedPoints[0].pointPositionIndex.y - 1;
            if (log)
            {
                Debug.LogFormat
                (
                    "Update hole point state | anchor index: {0} | y start: {1} | terminals ({2}):\n{3}",
                    anchorIndex,
                    yStop,
                    spec.TerminalGrid.Count (g => g != 0),
                    spec.TerminalGrid
                        .Select ((g, i) => new
                        {
                            Index = i,
                            Y = g,
                        })
                        .Where (x => x.Y != 0)
                        .Select (x => string.Format ("{0}/({1}, {2})/{3}", x.Index, x.Index % spec.GridRowSize, x.Index / spec.GridRowSize, x.Y))
                        .ToStringFormatted (true)
                );
            }

            var terminals = new HashSet<int> ();
            var sourceOrigin = spec.SourceOrigin;
            var sources = new List<AreaVolumePoint> ();
            while (anchorIndex >= 0)
            {
                for (var z = 0; z < spec.GridColumnSize; z += 1)
                {
                    var gridRowStart = z * spec.GridRowSize;
                    for (var x = 0; x < spec.GridRowSize; x += 1)
                    {
                        var gridIndex = x + gridRowStart;
                        var terminal = spec.TerminalGrid[gridIndex];
                        if (terminal >= yStop)
                        {
                            terminals.Add (gridIndex);
                        }
                    }
                }
                sourceOrigin.y -= 1;
                sources.Clear ();
                foreach (var (gridIndex, point) in spec.SourcePoints)
                {
                    if (terminals.Contains (gridIndex))
                    {
                        continue;
                    }
                    sources.Add (point);
                }
                if (sources.Count == 0)
                {
                    if (log)
                    {
                        Debug.Log ("Update hole point state -- loop exit on terminal condition");
                    }
                    break;
                }
                UpdatePointStateOnPaste (spec.AreaManager, sourceOrigin, sources, spec.AreaManager.points.Count, log);
                anchorIndex -= layerSize;
                yStop -= 1;
            }

            if (log)
            {
                Debug.LogFormat ("anchor: {0} | y stop: {1}", anchorIndex, yStop);
            }

            return yStop;
        }

        static void PasteProps (AreaManager am, Vector3Int cornerA, List<AreaVolumePoint> affectedPoints, bool checkPropCompatibility, bool log)
        {
            if (applicationMode == ApplicationMode.Overwrite)
            {
                for (var i = 0; i < affectedPoints.Count; i += 1)
                {
                    var index = affectedPoints[i].spotIndex;
                    am.RemovePropPlacement (index);
                    if (log && am.indexesOccupiedByProps.TryGetValue(index, out var propList))
                    {
                        Debug.Log ("Removing props on paste overwrite | index: " + index + " | count: " + propList.Count);
                    }
                }
            }

            var placedProps = log ? new List<AreaPlacementProp> () : null;
            for (var i = 0; i < am.clipboard.clipboardPropsSaved.Count; i += 1)
            {
                var savedProp = am.clipboard.clipboardPropsSaved[i];
                var targetPointPosition = savedProp.clipboardPosition + cornerA;
                var targetPointIndex = AreaUtility.GetIndexFromVolumePosition (targetPointPosition, am.boundsFull);
                var placement = new AreaPlacementProp ();
                var pointTargeted = am.points[targetPointIndex];
                var prototype = AreaAssetHelper.GetPropPrototype (savedProp.id);

                placement.id = savedProp.id;
                placement.pivotIndex = targetPointIndex;
                placement.rotation = savedProp.rotation;
                placement.flipped = savedProp.flipped;
                placement.offsetX = savedProp.offsetX;
                placement.offsetZ = savedProp.offsetZ;
                placement.hsbPrimary = savedProp.hsbPrimary;
                placement.hsbSecondary = savedProp.hsbSecondary;

                if (checkPropCompatibility && !am.IsPropPlacementValid (placement, pointTargeted, prototype, false))
                {
                    if (log)
                    {
                        Debug.LogFormat
                        (
                            "Skipping prop paste -- placement isn't valid | index: {0}/{1} | prop ID: {2}",
                            targetPointIndex,
                            targetPointPosition,
                            savedProp.id
                        );
                    }
                    continue;
                }

                if (!am.indexesOccupiedByProps.ContainsKey (targetPointIndex))
                {
                    am.indexesOccupiedByProps.Add (targetPointIndex, new List<AreaPlacementProp> ());
                }
                am.indexesOccupiedByProps[targetPointIndex].Add (placement);
                am.placementsProps.Add (placement);
                am.ExecutePropPlacement (placement);
                if (log)
                {
                    placedProps.Add (placement);
                }
            }
            if (log && placedProps.Count != 0)
            {
                Debug.Log ("Placed pasted props (" + placedProps.Count + "): " + placedProps.Select (p => p.pivotIndex + "/" + p.id).ToStringFormatted ());
            }
        }
    }
}
