using System.Collections.Generic;

using UnityEngine;

namespace Area
{
    using Scene;

    class AreaSceneSearch
    {
        public void CheckSearchRequirements
        (
            AreaManager am,
            AreaVolumePoint startingSpot,
            SpotSearchType searchType,
            ref List<AreaVolumePoint> spotsActedOn
        )
        {
            if (searchType == SpotSearchType.None)
            {
                lastSearchOrigin = startingSpot;
                lastSearchType = SpotSearchType.None;
                lastSearchResultsAsSpots = new List<AreaVolumePoint> (new[] { startingSpot });
            }

            if (lastSearchOrigin == null || lastSearchOrigin != startingSpot || lastSearchType != searchType)
            {
                SearchForSpots (am, startingSpot, searchType);
            }
            else
            {
                spotsActedOn = lastSearchResultsAsSpots;
                lastSearchResultsAsSpots = null;
                lastSearchResults = null;
                lastSearchOrigin = null;
            }
        }

        bool SearchValidationSameConfiguration (AreaVolumePoint startingSpot, AreaVolumePointSearchData arg) =>
                arg.status == defaultSearchStatus &&
                arg.point.spotPresent &&
                arg.point.spotConfiguration == startingSpot.spotConfiguration;

        bool SearchValidationSameFloor (AreaVolumePoint startingSpot, AreaVolumePointSearchData arg)
        {
            var pairIsNotSeparated =
                arg.directionFromParentCandidate == PointNeighbourDirection.YPos ||
                arg.directionFromParentCandidate == PointNeighbourDirection.YNeg ||
                !TilesetUtility.IsConfigurationPairSeparated
                (
                    arg.parentCandidate.point.spotConfiguration,
                    arg.point.spotConfiguration,
                    arg.directionFromParentCandidate
                );

            return
                arg.status == defaultSearchStatus &&
                arg.point.spotPresent &&
                arg.point.spotConfiguration != AreaNavUtility.configEmpty &&
                arg.point.pointPositionIndex.y == startingSpot.pointPositionIndex.y &&
                pairIsNotSeparated;
        }

        bool SearchValidationSameFloorIsolated (AreaVolumePoint startingPoint, AreaVolumePointSearchData arg)
        {
            var pairIsNotSeparated =
                arg.directionFromParentCandidate == PointNeighbourDirection.YPos ||
                arg.directionFromParentCandidate == PointNeighbourDirection.YNeg ||
                !TilesetUtility.IsConfigurationPairSeparated
                (
                    arg.parentCandidate.point.spotConfiguration,
                    arg.point.spotConfiguration,
                    arg.directionFromParentCandidate
                );

            return
                arg.status == defaultSearchStatus &&
                arg.point.spotPresent &&
                arg.point.spotConfiguration != AreaNavUtility.configEmpty &&
                arg.point.pointPositionIndex.y == startingPoint.pointPositionIndex.y &&
                pairIsNotSeparated &&
                arg.point.spotConfiguration != AreaNavUtility.configFloor;
        }

        bool SearchValidationSameColor (AreaVolumePoint startingPoint, AreaVolumePointSearchData arg) =>
                arg.status == defaultSearchStatus &&
                arg.point.spotPresent &&
                arg.point.spotConfiguration != AreaNavUtility.configEmpty &&
                arg.point.spotConfiguration != AreaNavUtility.configFull &&
                Mathf.Approximately(arg.point.customization.huePrimary, startingPoint.customization.huePrimary) &&
                Mathf.Approximately(arg.point.customization.saturationPrimary, startingPoint.customization.saturationPrimary) &&
                Mathf.Approximately(arg.point.customization.brightnessPrimary, startingPoint.customization.brightnessPrimary) &&
                Mathf.Approximately(arg.point.customization.hueSecondary, startingPoint.customization.hueSecondary) &&
                Mathf.Approximately(arg.point.customization.saturationSecondary, startingPoint.customization.saturationSecondary) &&
                Mathf.Approximately(arg.point.customization.brightnessSecondary, startingPoint.customization.brightnessSecondary);

        bool SearchValidationSameTileset (AreaVolumePoint startingPoint, AreaVolumePointSearchData arg) =>
                arg.status == defaultSearchStatus &&
                arg.point.spotPresent &&
                arg.point.spotConfiguration != AreaNavUtility.configEmpty &&
                arg.point.spotConfiguration != AreaNavUtility.configFull &&
                arg.point.blockTileset == startingPoint.blockTileset;

        bool SearchValidationSameEverything (AreaVolumePoint startingPoint, AreaVolumePointSearchData arg) =>
                arg.status == defaultSearchStatus &&
                arg.point.spotPresent &&
                arg.point.spotConfiguration == startingPoint.spotConfiguration &&
                arg.point.blockTileset == startingPoint.blockTileset &&
                arg.point.blockGroup == startingPoint.blockGroup &&
                arg.point.blockSubtype == startingPoint.blockSubtype;

        bool SearchValidationAllEmptyNodes (AreaVolumePoint startingPoint, AreaVolumePointSearchData arg) =>
                arg.status == defaultSearchStatus &&
                arg.point.spotPresent &&
                (arg.point.spotConfiguration == AreaNavUtility.configEmpty || arg.point.spotConfiguration == AreaNavUtility.configFull);

        void SearchForSpots
        (
            AreaManager am,
            AreaVolumePoint startingSpot,
            SpotSearchType searchType
        )
        {
            if (startingSpot == null || !startingSpot.spotPresent || startingSpot.spotConfiguration == 0)
            {
                Debug.Log ("Bailing out of search for spots due to starting spot being null, not present, or empty");
                return;
            }

            lastSearchOrigin = startingSpot;
            lastSearchType = searchType;

            if (searchType == SpotSearchType.None)
            {
                lastSearchResultsAsSpots = new List<AreaVolumePoint> (new[] { startingSpot });
            }
            else
            {
                // Debug.Log ("Beginning search with mode " + searchType);
                SearchValidationRoutine validation = null;
                switch (searchType)
                {
                    case SpotSearchType.SameConfiguration:
                        validation = SearchValidationSameConfiguration;
                        break;
                    case SpotSearchType.SameFloor:
                        validation = SearchValidationSameFloor;
                        break;
                    case SpotSearchType.SameFloorIsolated:
                        validation = SearchValidationSameFloorIsolated;
                        break;
                    case SpotSearchType.SameTileset:
                        validation = SearchValidationSameTileset;
                        break;
                    case SpotSearchType.SameEverything:
                        validation = SearchValidationSameEverything;
                        break;
                    case SpotSearchType.AllEmptyNodes:
                        validation = SearchValidationAllEmptyNodes;
                        break;
                    case SpotSearchType.SameColor:
                        validation = SearchValidationSameColor;
                        break;
                }

                if (pointSearchData == null || pointSearchData.Count != am.points.Count || pointSearchDataChangeTracker != am.ChangeTracker)
                {
                    pointSearchData = new List<AreaVolumePointSearchData> (am.points.Count);
                    pointSearchDataChangeTracker = am.ChangeTracker;

                    for (var i = 0; i < am.points.Count; ++i)
                    {
                        var psd = new AreaVolumePointSearchData
                        {
                            point = am.points[i]
                        };
                        pointSearchData.Add (psd);
                    }

                    foreach (var psd in pointSearchData)
                    {
                        // spotpoints: 1 (X+) 2 (Z+) 4 (Y+)
                        // spotsAroundThisPoint: 3 (Y-), 5 (Z-), 6 (X-)

                        var pointYPos = psd.point.pointsInSpot[BoundsSpace.PointNeighbor.Up];
                        if (pointYPos != null)
                        {
                            psd.neighbourYPos = pointSearchData[pointYPos.spotIndex];
                        }

                        var pointXPos = psd.point.pointsInSpot[BoundsSpace.PointNeighbor.East];
                        if (pointXPos != null)
                        {
                            psd.neighbourXPos = pointSearchData[pointXPos.spotIndex];
                        }

                        var pointXNeg = psd.point.pointsWithSurroundingSpots[BoundsSpace.SpotNeighbor.West];
                        if (pointXNeg != null)
                        {
                            psd.neighbourXNeg = pointSearchData[pointXNeg.spotIndex];
                        }

                        var pointZPos = psd.point.pointsInSpot[BoundsSpace.PointNeighbor.North];
                        if (pointZPos != null)
                        {
                            psd.neighbourZPos = pointSearchData[pointZPos.spotIndex];
                        }

                        var pointZNeg = psd.point.pointsWithSurroundingSpots[BoundsSpace.SpotNeighbor.South];
                        if (pointZNeg != null)
                        {
                            psd.neighbourZNeg = pointSearchData[pointZNeg.spotIndex];
                        }

                        var pointYNeg = psd.point.pointsWithSurroundingSpots[BoundsSpace.SpotNeighbor.Down];
                        if (pointYNeg != null)
                        {
                            psd.neighbourYNeg = pointSearchData[pointYNeg.spotIndex];
                        }
                    }
                }

                foreach (var psd in pointSearchData)
                {
                    psd.status = defaultSearchStatus;
                }

                GetPointsConnected (am, startingSpot, validation);

                lastSearchResults = new List<AreaVolumePointSearchData> ();
                lastSearchResultsAsSpots = new List<AreaVolumePoint> ();

                foreach (var psd in pointSearchData)
                {
                    if (psd.status == defaultSearchStatus)
                    {
                        continue;
                    }
                    lastSearchResults.Add (psd);
                    lastSearchResultsAsSpots.Add (psd.point);
                }

                // Debug.Log ("Search results: " + lastSearchResults.Count);
            }
        }

        void GetPointsConnected
        (
            AreaManager am,
            AreaVolumePoint startingSpot,
            SearchValidationRoutine validation
        )
        {
            if (startingSpot == null || !startingSpot.spotPresent || startingSpot.spotConfiguration == 0 || validation == null)
            {
                return;
            }

            var q = new Queue<AreaVolumePointSearchData> (am.points.Count);
            var iterations = 0;
            var limit = am.points.Count + 1000;

            //AreaVolumePointSearchData psd;
            foreach (var psd in pointSearchData)
            {
                psd.status = defaultSearchStatus;
                psd.parent = null;
                psd.parentCandidate = null;
            }

            q.Enqueue (pointSearchData[startingSpot.spotIndex]);
            while (q.Count > 0)
            {
                var psd = q.Dequeue ();

                if (q.Count > limit)
                {
                    // XXX can this alogrithm be deterministic?
                    throw new System.Exception ("The algorithm is probably looping. Queue size: " + q.Count);
                }

                if (psd.status != defaultSearchStatus)
                {
                    continue;
                }

                psd.status = iterations;
                psd.parent = psd.parentCandidate;
                psd.directionFromParent = psd.directionFromParentCandidate;

                if (CheckSearchStepValidity (startingSpot, psd.neighbourYPos, psd, PointNeighbourDirection.YPos, validation))
                {
                    q.Enqueue (psd.neighbourYPos);
                }
                if (CheckSearchStepValidity (startingSpot, psd.neighbourXPos, psd, PointNeighbourDirection.XPos, validation))
                {
                    q.Enqueue (psd.neighbourXPos);
                }
                if (CheckSearchStepValidity (startingSpot, psd.neighbourXNeg, psd, PointNeighbourDirection.XNeg, validation))
                {
                    q.Enqueue (psd.neighbourXNeg);
                }
                if (CheckSearchStepValidity (startingSpot, psd.neighbourZPos, psd, PointNeighbourDirection.ZPos, validation))
                {
                    q.Enqueue (psd.neighbourZPos);
                }
                if (CheckSearchStepValidity (startingSpot, psd.neighbourZNeg, psd, PointNeighbourDirection.ZNeg, validation))
                {
                    q.Enqueue (psd.neighbourZNeg);
                }
                if (CheckSearchStepValidity (startingSpot, psd.neighbourYNeg, psd, PointNeighbourDirection.YNeg, validation))
                {
                    q.Enqueue (psd.neighbourYNeg);
                }

                iterations++;
            }
        }

        bool CheckSearchStepValidity
        (
            AreaVolumePoint startingSpot,
            AreaVolumePointSearchData psd,
            AreaVolumePointSearchData parentCandidate,
            PointNeighbourDirection direction,
            SearchValidationRoutine validation
        )
        {
            if (psd == null || validation == null)
            {
                // Debug.Log ("Bailing out of search step due to data being null or validation function being null");
                return false;
            }

            psd.directionFromParentCandidate = direction;
            psd.parentCandidate = parentCandidate;
            return validation (startingSpot, psd); // psd.status == -1 && psd.point.spotConfiguration != 0;
        }

        delegate bool SearchValidationRoutine (AreaVolumePoint startingSpot, AreaVolumePointSearchData arg);

        public List<AreaVolumePointSearchData> lastSearchResults;

        int pointSearchDataChangeTracker;
        List<AreaVolumePointSearchData> pointSearchData;
        List<AreaVolumePoint> lastSearchResultsAsSpots;
        AreaVolumePoint lastSearchOrigin;
        SpotSearchType lastSearchType;

        const int defaultSearchStatus = -1;
    }
}
