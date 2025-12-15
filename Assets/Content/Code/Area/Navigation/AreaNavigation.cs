using UnityEngine;
using System.Collections.Generic;
using System;
using PhantomBrigade;
using PhantomBrigade.Data;

namespace Area
{
    public interface IAreaNavNode
    {
        int GetPointIndex ();
        int GetNodeIndex ();
        Vector3 GetPosition ();
        List<AreaNavLink> GetLinks ();

        void SetPointIndex (int pointIndex);
        void SetNodeIndex (int nodeIndex);
        void SetPosition (Vector3 position);
        void SetLinks (List<AreaNavLink> links);
    }

    public class AreaNavNode : IAreaNavNode
    {
        private int pointIndex;
        private int nodeIndex;
        private Vector3 position;
        private List<AreaNavLink> links;

        public int GetPointIndex () { return pointIndex; }
        public int GetNodeIndex () { return nodeIndex; }
        public Vector3 GetPosition () { return position; }
        public List<AreaNavLink> GetLinks () { return links; }

        public void SetPointIndex (int pointIndex) { this.pointIndex = pointIndex; }
        public void SetNodeIndex (int nodeIndex) { this.nodeIndex = nodeIndex; }
        public void SetPosition (Vector3 position) { this.position = position; }
        public void SetLinks (List<AreaNavLink> links) { this.links = links; }
    }

    public class AreaNavLink
    {
        public AreaNavLinkType type;
        public int destinationIndex;

        public AreaNavLink ()
        {

        }

        public AreaNavLink (AreaNavLinkType type, int destinationIndex)
        {
            this.type = type;
            this.destinationIndex = destinationIndex;
        }

        public override string ToString ()
        {
            return $"(T: {type} / DI: {destinationIndex})";
        }
    }

    public enum AreaNavLinkType
    {
        Horizontal = 0,
        JumpUp = 1,
        JumpDown = 2,
        JumpOverDrop = 3,
        JumpOverClimb = 4,
        Diagonal = 5,
    }

    public enum OverridePolicy
    {
        OverrideAgnostic,
        OverrideProhibited,
        OverrideRequired
    }

    public static class AreaNavUtility
    {
        // Here are some often-used configurations

        // 0                  - empty space
        // 15                 - floor
        // 255                - full space
        // 1, 2, 4, 8         - outward corner floor-to-wall (wall top)
        // 3, 6, 9, 12        - straight floor-to-wall (wall top)
        // 60, 105, 150, 195  - straight wall-to-floor (wall bottom) with straight wall-to-ceiling (wall top) underneath
        // 61, 107, 158, 199  - straight wall-to-floor (wall bottom) with inward corner of wall-to-ceiling (wall top) on the left underneath
        // 62, 109, 151, 203  - straight wall-to-floor (wall bottom) with inward corner of wall-to-ceiling (wall top) on the right underneath
        // 63, 111, 159, 207  - straight wall-to-floor (wall bottom) with nothing else
        // 30, 45, 75, 135    - outward corner wall-to-floor with inward corner of wall-to-ceiling underneath
        // 31, 47, 79, 143    - outward corner wall-to-floor with nothing else

        public const byte configFull = TilesetUtility.configurationFull;
        public const byte configEmpty = TilesetUtility.configurationEmpty;
        public const byte configFloor = TilesetUtility.configurationFloor;

        public const byte configRoofEdgeXPos = 6;
        public const byte configRoofEdgeXNeg = 9;
        public const byte configRoofEdgeZPos = 3;
        public const byte configRoofEdgeZNeg = 12;

        public const byte configRoofCornerOutXPosZNeg = 1;
        public const byte configRoofCornerOutXNegZNeg = 2;
        public const byte configRoofCornerOutXNegZPos = 4;
        public const byte configRoofCornerOutXPosZPos = 8;


        // This class represents a step in the sequence of point-to-point transitions performed to search for valid links
        // Each step tells the algorithm where to go next and what spot configuration mask to use to reject unsuitable neighbours

        public class LinkSearchStep
        {
            public int direction;
            public byte mask;
            public OverridePolicy overridePolicy;

            public LinkSearchStep (int direction, byte mask, OverridePolicy overridePolicy = OverridePolicy.OverrideAgnostic)
            {
                this.direction = direction;
                this.mask = mask;
                this.overridePolicy = overridePolicy;
            }
        }

        // The preset is a complete instruction set for building a given link type over the whole graph
        // In contains a link type used to price the created connections, and an array of arrays of steps
        // In other words, it contains an array of sequences - instructions for similar link searches in different directions

        public class LinkSearchPreset
        {
            public AreaNavLinkType type;
            public OverridePolicy overridePolicyStart;
            public bool strictDestination;
            public LinkSearchStep[][] sequences;

            public LinkSearchPreset
            (
                AreaNavLinkType type,
                OverridePolicy overridePolicyStart,
                bool strictDestination,
                LinkSearchStep[][] sequences
            )
            {
                this.type = type;
                this.overridePolicyStart = overridePolicyStart;
                this.strictDestination = strictDestination;
                this.sequences = sequences;
            }
        }

        // Horizontal link search is the most simple case, as it involves just one step:
        // - Finding a neighbour with a floor configuration along a given direction

        public static LinkSearchPreset searchPresetStraight = new LinkSearchPreset
        (
            AreaNavLinkType.Horizontal,
            OverridePolicy.OverrideAgnostic,
            false,
            new []
            {
                new [] { new LinkSearchStep ((int)PointNeighbourDirection.XPos, configFloor) },
                new [] { new LinkSearchStep ((int)PointNeighbourDirection.XNeg, configFloor) },
                new [] { new LinkSearchStep ((int)PointNeighbourDirection.ZPos, configFloor) },
                new [] { new LinkSearchStep ((int)PointNeighbourDirection.ZNeg, configFloor) },
            }
        );

        // Same deal with diagonal links - they just have a different cost and a different set of directions, but do the same thing:
        // - Finding a neighbour with a floor configuration along a given direction

        public static LinkSearchPreset searchPresetDiagonal = new LinkSearchPreset
        (
            AreaNavLinkType.Diagonal,
            OverridePolicy.OverrideAgnostic,
            false,
            new []
            {
                new [] { new LinkSearchStep ((int)PointNeighbourDirection.XPosZPos, configFloor) },
                new [] { new LinkSearchStep ((int)PointNeighbourDirection.XPosZNeg, configFloor) },
                new [] { new LinkSearchStep ((int)PointNeighbourDirection.XNegZPos, configFloor) },
                new [] { new LinkSearchStep ((int)PointNeighbourDirection.XNegZNeg, configFloor) },
            }
        );

        // Jumping  up (climbing) consists of:
        // - Finding an empty configuration above
        // - Finding a wall-to-roof configuration rotated to you in the facing direction
        // - Finding a floor configuration further along the facing direction

        //   →
        // ↑ ╔[═]
        // ╧═╝

        public static LinkSearchPreset searchPresetJumpUp = new LinkSearchPreset
        (
            AreaNavLinkType.JumpUp,
            OverridePolicy.OverrideProhibited,
            true,
            new []
            {
                new []
                {
                    new LinkSearchStep ((int)PointNeighbourDirection.YNeg, configEmpty, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.XPos, configRoofEdgeXPos, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.XPos, configFloor, OverridePolicy.OverrideProhibited)
                },
                new []
                {
                    new LinkSearchStep ((int)PointNeighbourDirection.YNeg, configEmpty, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.XNeg, configRoofEdgeXNeg, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.XNeg, configFloor, OverridePolicy.OverrideProhibited)
                },
                new []
                {
                    new LinkSearchStep ((int)PointNeighbourDirection.YNeg, configEmpty, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.ZPos, configRoofEdgeZPos, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.ZPos, configFloor, OverridePolicy.OverrideProhibited)
                },
                new []
                {
                    new LinkSearchStep ((int)PointNeighbourDirection.YNeg, configEmpty, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.ZNeg, configRoofEdgeZNeg, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.ZNeg, configFloor, OverridePolicy.OverrideProhibited)
                },
            }
        );

        // Connecting overrides up
        // - Finding an empty configuration above
        // - Finding a wall-to-roof configuration rotated to you in the facing direction
        // - Finding a floor configuration further along the facing direction

        //   →
        // ↑ ╔[═]
        // ╧═╝

        public static LinkSearchPreset searchPresetOverrideUp = new LinkSearchPreset
        (
            AreaNavLinkType.Diagonal,
            OverridePolicy.OverrideAgnostic,
            true,
            new []
            {
                new []
                {
                    new LinkSearchStep ((int)PointNeighbourDirection.YNeg, configEmpty, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.XPos, configRoofEdgeXPos, OverridePolicy.OverrideRequired)
                },
                new []
                {
                    new LinkSearchStep ((int)PointNeighbourDirection.YNeg, configEmpty, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.XNeg, configRoofEdgeXNeg, OverridePolicy.OverrideRequired)
                },
                new []
                {
                    new LinkSearchStep ((int)PointNeighbourDirection.YNeg, configEmpty, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.ZPos, configRoofEdgeZPos, OverridePolicy.OverrideRequired)
                },
                new []
                {
                    new LinkSearchStep ((int)PointNeighbourDirection.YNeg, configEmpty, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.ZNeg, configRoofEdgeZNeg, OverridePolicy.OverrideRequired)
                },
            }
        );

        // Connecting overrides up
        // - Finding an empty configuration above
        // - Finding a wall-to-roof configuration rotated to you in the facing direction
        // - Finding a floor configuration further along the facing direction

        //   →
        // ↑ ╔[═]
        // ╧═╝

        public static LinkSearchPreset searchPresetOverrideUpCorner = new LinkSearchPreset
        (
            AreaNavLinkType.Diagonal,
            OverridePolicy.OverrideAgnostic,
            true,
            new []
            {
                new []
                {
                    new LinkSearchStep ((int)PointNeighbourDirection.YNeg, configEmpty, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.XPos, configRoofCornerOutXNegZNeg, OverridePolicy.OverrideRequired)
                },
                new []
                {
                    new LinkSearchStep ((int)PointNeighbourDirection.YNeg, configEmpty, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.XPos, configRoofCornerOutXNegZPos, OverridePolicy.OverrideRequired)
                },
                new []
                {
                    new LinkSearchStep ((int)PointNeighbourDirection.YNeg, configEmpty, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.XNeg, configRoofCornerOutXPosZNeg, OverridePolicy.OverrideRequired)
                },
                new []
                {
                    new LinkSearchStep ((int)PointNeighbourDirection.YNeg, configEmpty, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.XNeg, configRoofCornerOutXPosZPos, OverridePolicy.OverrideRequired)
                },
                new []
                {
                    new LinkSearchStep ((int)PointNeighbourDirection.YNeg, configEmpty, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.ZPos, configRoofCornerOutXNegZNeg, OverridePolicy.OverrideRequired)
                },
                new []
                {
                    new LinkSearchStep ((int)PointNeighbourDirection.YNeg, configEmpty, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.ZPos, configRoofCornerOutXPosZNeg, OverridePolicy.OverrideRequired)
                },
                new []
                {
                    new LinkSearchStep ((int)PointNeighbourDirection.YNeg, configEmpty, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.ZNeg, configRoofCornerOutXNegZPos, OverridePolicy.OverrideRequired)
                },
                new []
                {
                    new LinkSearchStep ((int)PointNeighbourDirection.YNeg, configEmpty, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.ZNeg, configRoofCornerOutXPosZPos, OverridePolicy.OverrideRequired)
                }
            }
        );

        // Jumping  down (dropping) consists of:
        // - Finding an empty configuration above
        // - Finding a wall-to-roof configuration rotated to you in the facing direction
        // - Finding a floor configuration further along the facing direction

        //  → →
        // ╧═╗ ↓
        //   ╚[═]

        public static LinkSearchPreset searchPresetJumpDown = new LinkSearchPreset
        (
            AreaNavLinkType.JumpDown,
            OverridePolicy.OverrideProhibited,
            true,
            new []
            {
                new []
                {
                    new LinkSearchStep ((int)PointNeighbourDirection.XPos, configRoofEdgeXNeg, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.XPos, configEmpty, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.YPos, configFloor, OverridePolicy.OverrideProhibited)
                },
                new []
                {
                    new LinkSearchStep ((int)PointNeighbourDirection.XNeg, configRoofEdgeXPos, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.XNeg, configEmpty, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.YPos, configFloor, OverridePolicy.OverrideProhibited)
                },
                new []
                {
                    new LinkSearchStep ((int)PointNeighbourDirection.ZPos, configRoofEdgeZNeg, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.ZPos, configEmpty, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.YPos, configFloor, OverridePolicy.OverrideProhibited)
                },
                new []
                {
                    new LinkSearchStep ((int)PointNeighbourDirection.ZNeg, configRoofEdgeZPos, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.ZNeg, configEmpty, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.YPos, configFloor, OverridePolicy.OverrideProhibited)
                },
            }
        );

        // Connecting overrides down:
        // - Finding an empty configuration
        // - Finding a floor configuration further along the facing direction

        //    →
        // ══╡ ↓
        //   ╚[═]

        public static LinkSearchPreset searchPresetOverrideDown = new LinkSearchPreset
        (
            AreaNavLinkType.Diagonal,
            OverridePolicy.OverrideRequired,
            true,
            new []
            {
                new []
                {
                    new LinkSearchStep ((int)PointNeighbourDirection.XPos, configEmpty, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.YPos, configFloor),
                },
                new []
                {
                    new LinkSearchStep ((int)PointNeighbourDirection.XNeg, configEmpty, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.YPos, configFloor),
                },
                new []
                {
                    new LinkSearchStep ((int)PointNeighbourDirection.ZPos, configEmpty, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.YPos, configFloor),
                },
                new []
                {
                    new LinkSearchStep ((int)PointNeighbourDirection.ZNeg, configEmpty, OverridePolicy.OverrideProhibited),
                    new LinkSearchStep ((int)PointNeighbourDirection.YPos, configFloor),
                },
            }
        );


        // Jumping over drops (narrow gaps) consists of:
        // - Finding a wall-to-roof configuration rotated away from you in the facing direction
        // - Finding a wall-to-roof configuration rotated toward you further along the facing direction
        // - Finding a floor configuration further along the facing direction

        //    →
        // ╧═╗ ╔[═]
        //   ╚═╝

        public static LinkSearchPreset searchPresetJumpOverDrop = new LinkSearchPreset
        (
            AreaNavLinkType.JumpOverDrop,
            OverridePolicy.OverrideAgnostic,
            true,
            new []
            {
            new []
            {
                new LinkSearchStep ((int)PointNeighbourDirection.XPos, configRoofEdgeXNeg, OverridePolicy.OverrideProhibited),
                new LinkSearchStep ((int)PointNeighbourDirection.XPos, configRoofEdgeXPos, OverridePolicy.OverrideProhibited),
                new LinkSearchStep ((int)PointNeighbourDirection.XPos, configFloor),
            },
            new []
            {
                new LinkSearchStep ((int)PointNeighbourDirection.XNeg, configRoofEdgeXPos, OverridePolicy.OverrideProhibited),
                new LinkSearchStep ((int)PointNeighbourDirection.XNeg, configRoofEdgeXNeg, OverridePolicy.OverrideProhibited),
                new LinkSearchStep ((int)PointNeighbourDirection.XNeg, configFloor),
            },
            new []
            {
                new LinkSearchStep ((int)PointNeighbourDirection.ZPos, configRoofEdgeZNeg, OverridePolicy.OverrideProhibited),
                new LinkSearchStep ((int)PointNeighbourDirection.ZPos, configRoofEdgeZPos, OverridePolicy.OverrideProhibited),
                new LinkSearchStep ((int)PointNeighbourDirection.ZPos, configFloor),
            },
            new []
            {
                new LinkSearchStep ((int)PointNeighbourDirection.ZNeg, configRoofEdgeZPos, OverridePolicy.OverrideProhibited),
                new LinkSearchStep ((int)PointNeighbourDirection.ZNeg, configRoofEdgeZNeg, OverridePolicy.OverrideProhibited),
                new LinkSearchStep ((int)PointNeighbourDirection.ZNeg, configFloor),
            },
            }
        );

        // Jumping over low obstacles (vaulting) consists of:
        // - Finding an empty configuration above
        // - Finding a wall-to-roof configuration rotated to you in the facing direction
        // - Finding a wall-to-roof configuration rotated in an opposite way further along the facing direction
        // - Finding an empty configuration further along the facing direction
        // - Finding a floor configuration below

        //    →
        // ↑ ╔═╗ ↓
        // ╧═╝ ╚[═]

        public static LinkSearchPreset searchPresetJumpOverClimb = new LinkSearchPreset
        (
            AreaNavLinkType.JumpOverClimb,
            OverridePolicy.OverrideAgnostic,
            true,
            new []
            {
            new []
            {
                new LinkSearchStep ((int)PointNeighbourDirection.YNeg, configEmpty),
                new LinkSearchStep ((int)PointNeighbourDirection.XPos, configRoofEdgeXPos, OverridePolicy.OverrideProhibited),
                new LinkSearchStep ((int)PointNeighbourDirection.XPos, configRoofEdgeXNeg, OverridePolicy.OverrideProhibited),
                new LinkSearchStep ((int)PointNeighbourDirection.XPos, configEmpty),
                new LinkSearchStep ((int)PointNeighbourDirection.YPos, configFloor),
            },
            new []
            {
                new LinkSearchStep ((int)PointNeighbourDirection.YNeg, configEmpty),
                new LinkSearchStep ((int)PointNeighbourDirection.XNeg, configRoofEdgeXNeg, OverridePolicy.OverrideProhibited),
                new LinkSearchStep ((int)PointNeighbourDirection.XNeg, configRoofEdgeXPos, OverridePolicy.OverrideProhibited),
                new LinkSearchStep ((int)PointNeighbourDirection.XNeg, configEmpty),
                new LinkSearchStep ((int)PointNeighbourDirection.YPos, configFloor),
            },
            new []
            {
                new LinkSearchStep ((int)PointNeighbourDirection.YNeg, configEmpty),
                new LinkSearchStep ((int)PointNeighbourDirection.ZPos, configRoofEdgeZPos, OverridePolicy.OverrideProhibited),
                new LinkSearchStep ((int)PointNeighbourDirection.ZPos, configRoofEdgeZNeg, OverridePolicy.OverrideProhibited),
                new LinkSearchStep ((int)PointNeighbourDirection.ZPos, configEmpty),
                new LinkSearchStep ((int)PointNeighbourDirection.YPos, configFloor),
            },
            new []
            {
                new LinkSearchStep ((int)PointNeighbourDirection.YNeg, configEmpty),
                new LinkSearchStep ((int)PointNeighbourDirection.ZNeg, configRoofEdgeZNeg, OverridePolicy.OverrideProhibited),
                new LinkSearchStep ((int)PointNeighbourDirection.ZNeg, configRoofEdgeZPos, OverridePolicy.OverrideProhibited),
                new LinkSearchStep ((int)PointNeighbourDirection.ZNeg, configEmpty),
                new LinkSearchStep ((int)PointNeighbourDirection.YPos, configFloor),
            },
            }
        );




        private static List<int> pointSearchData;
        private static Queue<PointData> queue;
        #if !PB_MODSDK
        private static List<AreaNavNode> graph;
        #else
        public static List<IAreaNavNode> graph;
        #endif
        private const int defaultNodeIndex = -1;
        private static int lastLevelID;

        private static Color colorAxisXPos = new Color (1f, 0.45f, 0.4f, 1f);
        private static Color colorAxisXNeg = new Color (1f, 0.3f, 0.7f, 1f);

        private static Color colorAxisZPos = new Color (0.4f, 0.6f, 1f, 1f);
        private static Color colorAxisZNeg = new Color (0.2f, 0.8f, 1f, 1f);

        private static Color colorTargetBlend = new Color (0.6f, 0.6f, 1f, 1f);
        private static Color colorTargetAir = new Color (0.4f, 0.7f, 0.9f, 0.75f);
        private static Color colorTargetVelocity = new Color (0.9f, 0.7f, 0.3f, 1f);

        private static Color colorInputYaw = new Color (0.4f, 0.9f, 0.55f, 1f);
        private static Color colorInputPitch = new Color (0.9f, 0.2f, 0.4f, 1f);
        private static Color colorInputThrottle = new Color (0.4f, 0.7f, 0.9f, 1f);
        private static Color colorInputBackground = new Color (0.3f, 0.3f, 0.3f, 1f);



        private static Vector3 navOverrideShiftVertical = Vector3.up * (TilesetUtility.blockAssetSize * 0.5f);
        private static float navOverrideOffsetThresholdPos = 0.3f;
        private static float navOverrideOffsetThresholdNeg = -0.3f;
        private static List<int> navIndexesRemoved = new List<int> ();

        private static bool IsPointUsableAsNavOverrideCenter (AreaVolumePoint point)
        {
            return
                point != null &&
                !point.spotHasDamagedPoints &&
                point.blockTileset == AreaTilesetHelper.idOfTerrain &&
                point.spotConfiguration != configEmpty &&
                point.spotConfiguration != configFull;
        }

        private static bool IsPointUsableAsNavOverrideNeighbor (AreaVolumePoint point)
        {
            return
                point != null &&
                !point.spotHasDamagedPoints &&
                point.blockTileset == AreaTilesetHelper.idOfTerrain &&
                point.spotConfiguration == configFloor;
        }

        public static void GenerateNavOverrides
        (
            Dictionary<int, AreaDataNavOverride> navOverrides,
            List<AreaVolumePoint> points,
            bool draw,
            Dictionary<int, AreaDataNavOverride> navOverridesSaved,
            bool navOverridesSavedClearConflicts
        )
        {
            if (points == null || navOverrides == null)
                return;

            navOverrides.Clear ();
            for (int i = 0, count = points.Count; i < count; ++i)
            {
                var point = points[i];
                if (point.spotHasDamagedPoints || point.spotConfiguration == AreaNavUtility.configEmpty || point.spotConfiguration == AreaNavUtility.configFull)
                    continue;

                bool navOverrideFound = AreaTilesetHelper.GetNavOverrideData
                (
                    point.blockTileset,
                    AreaTilesetHelper.assetFamilyBlock,
                    point.spotConfiguration,
                    point.blockGroup,
                    point.blockSubtype,
                    out AreaDataNavOverride navOverride
                );

                if (!navOverrideFound)
                    continue;

                // Debug.Log ($"Found nav override for point with tileset {point.blockTileset}, configuration {point.spotConfiguration}, group {point.blockGroup}, subtype {point.blockSubtype}");
                navOverrides.Add (point.spotIndex, new AreaDataNavOverride { pivotIndex = point.spotIndex, offsetY = navOverride.offsetY});
            }

            for (int i = 0, count = points.Count; i < count; ++i)
            {
                var pointTopSpot = points[i];
                if (!IsPointUsableAsNavOverrideCenter (pointTopSpot))
                    continue;

                var pointBottomSpot = pointTopSpot.pointsInSpot[4];
                if (!IsPointUsableAsNavOverrideCenter (pointBottomSpot))
                    continue;

                if (navOverrides.ContainsKey (pointTopSpot.spotIndex))
                    continue;

                var sc = pointTopSpot.spotConfiguration;

                //    X ---> XZ
                //   /|     /|
                //  0 ---> Z |
                //  | |    | |
                //  | YX --|YXZ
                //  |/     |/
                //  Y ---> YZ

                //        0    1   2   3    4   5    6     7
                // [8]: this, +X, +Z, +XZ, +Y, +YX, +YZ, +XYZ

                // Fetch volume point dependencies for top floor
                var pointTop0 = pointTopSpot.pointsInSpot[0];
                var pointTop1 = pointTopSpot.pointsInSpot[1];
                var pointTop2 = pointTopSpot.pointsInSpot[2];
                var pointTop3 = pointTopSpot.pointsInSpot[3];

                // Fetch volume point dependencies for bottom floor
                var pointBottom0 = pointBottomSpot.pointsInSpot[0];
                var pointBottom1 = pointBottomSpot.pointsInSpot[1];
                var pointBottom2 = pointBottomSpot.pointsInSpot[2];
                var pointBottom3 = pointBottomSpot.pointsInSpot[3];

                // Since some ops are direction agnostic, no point in including them in branches - they'll run off of these reused vars
                AreaVolumePoint pointTopA = null;
                AreaVolumePoint pointTopB = null;
                AreaVolumePoint pointBottomA = null;
                AreaVolumePoint pointBottomB = null;

                // For creating softened override
                AreaVolumePoint pointTopNeighbourSpot = null;
                AreaVolumePoint pointBottomNeighbourSpot = null;

                // Case 1, slopes towards east/X+ (labeled X- because configuration is named for view direction towards the roof, not out of it)
                if (sc == configRoofEdgeXNeg)
                {
                    pointTopA = pointTop0;
                    pointTopB = pointTop2;
                    pointBottomA = pointBottom1;
                    pointBottomB = pointBottom3;

                    pointTopNeighbourSpot = pointTopSpot.pointsWithSurroundingSpots[6];
                    pointBottomNeighbourSpot = pointBottomSpot.pointsInSpot[1];
                }

                // Case 2, slopes towards west/X-
                if (sc == configRoofEdgeXPos)
                {
                    pointTopA = pointTop3;
                    pointTopB = pointTop1;
                    pointBottomA = pointBottom2;
                    pointBottomB = pointBottom0;

                    pointTopNeighbourSpot = pointTopSpot.pointsInSpot[1];
                    pointBottomNeighbourSpot = pointBottomSpot.pointsWithSurroundingSpots[6];
                }

                // Case 3, slopes towards north/Z+ (labeled Z- for same reason as 1)
                else if (sc == configRoofEdgeZNeg)
                {
                    pointTopA = pointTop1;
                    pointTopB = pointTop0;
                    pointBottomA = pointBottom3;
                    pointBottomB = pointBottom2;

                    pointTopNeighbourSpot = pointTopSpot.pointsWithSurroundingSpots[5];
                    pointBottomNeighbourSpot = pointBottomSpot.pointsInSpot[2];
                }

                // Case 4, slopes towards south/Z-
                else if (sc == configRoofEdgeZPos)
                {
                    pointTopA = pointTop2;
                    pointTopB = pointTop3;
                    pointBottomA = pointBottom0;
                    pointBottomB = pointBottom1;

                    pointTopNeighbourSpot = pointTopSpot.pointsInSpot[2];
                    pointBottomNeighbourSpot = pointBottomSpot.pointsWithSurroundingSpots[5];
                }

                if (pointTopA == null || pointTopB == null || pointBottomA == null || pointBottomB == null)
                    continue;

                if (!IsPointUsableAsNavOverrideNeighbor (pointTopNeighbourSpot))
                    continue;

                if (!IsPointUsableAsNavOverrideNeighbor (pointBottomNeighbourSpot))
                    continue;

                var posTopA    = pointTopA.pointPositionLocal - navOverrideShiftVertical;
                var posTopB    = pointTopB.pointPositionLocal - navOverrideShiftVertical;
                var posBottomA = pointBottomA.pointPositionLocal - navOverrideShiftVertical;
                var posBottomB = pointBottomB.pointPositionLocal - navOverrideShiftVertical;

                posTopA    += Vector3.up * (pointTopA.terrainOffset * TilesetUtility.blockAssetSize);
                posTopB    += Vector3.up * (pointTopB.terrainOffset * TilesetUtility.blockAssetSize);
                posBottomA += Vector3.up * (pointBottomA.terrainOffset * TilesetUtility.blockAssetSize);
                posBottomB += Vector3.up * (pointBottomB.terrainOffset * TilesetUtility.blockAssetSize);

                if (draw)
                {
                    Debug.DrawLine (pointTopA.pointPositionLocal, posTopA, Color.green, 5f);
                    Debug.DrawLine (pointTopB.pointPositionLocal, posTopB, Color.green, 5f);
                    Debug.DrawLine (posTopA, posTopB, Color.green, 5f);

                    Debug.DrawLine (pointBottomA.pointPositionLocal, posBottomA, Color.cyan, 5f);
                    Debug.DrawLine (pointBottomB.pointPositionLocal, posBottomB, Color.cyan, 5f);
                    Debug.DrawLine (posBottomA, posBottomB, Color.cyan, 5f);
                }

                if
                (
                    pointTopA.terrainOffset > navOverrideOffsetThresholdNeg ||
                    pointTopB.terrainOffset > navOverrideOffsetThresholdNeg ||
                    pointBottomA.terrainOffset < navOverrideOffsetThresholdPos ||
                    pointBottomB.terrainOffset < navOverrideOffsetThresholdNeg
                )
                    continue;

                if (draw)
                {
                    Debug.DrawLine (posTopA, posBottomB, Color.red, 5f);
                    Debug.DrawLine (posTopB, posBottomA, Color.red, 5f);
                }

                var posAverage = (posTopA + posTopB + posBottomA + posBottomB) * 0.25f;
                var offsetYCenter = posAverage.y - pointTopSpot.pointPositionLocal.y + TilesetUtility.blockAssetSize * 0.5f;
                var offsetYTopNeighbour = offsetYCenter / 3f;
                var offsetYBottomNeighbour = -offsetYTopNeighbour;

                // Debug.Log ($"TN: {offsetYTopNeighbour} | C: {offsetYCenter} | BN: {offsetYBottomNeighbour}");

                if (draw)
                {
                    Debug.DrawLine (pointTopSpot.instancePosition, pointTopSpot.instancePosition + Vector3.up * offsetYCenter, Color.magenta, 5f);
                    Debug.DrawLine (pointTopNeighbourSpot.instancePosition, pointTopNeighbourSpot.instancePosition + Vector3.up * offsetYTopNeighbour, Color.green, 5f);
                    Debug.DrawLine (pointBottomNeighbourSpot.instancePosition, pointBottomNeighbourSpot.instancePosition + Vector3.up * offsetYBottomNeighbour, Color.cyan, 5f);
                }

                TryAddNavOverride (navOverrides, pointTopSpot, offsetYCenter);
                TryAddNavOverride (navOverrides, pointTopNeighbourSpot, offsetYTopNeighbour);
                TryAddNavOverride (navOverrides, pointBottomNeighbourSpot, offsetYBottomNeighbour);
            }

            for (int i = 0, count = points.Count; i < count; ++i)
            {
                var pointTopSpot = points[i];
                if (!IsPointUsableAsNavOverrideCenter (pointTopSpot))
                    continue;

                var pointBottomSpot = pointTopSpot.pointsInSpot[4];
                if (!IsPointUsableAsNavOverrideCenter (pointBottomSpot))
                    continue;

                if (navOverrides.ContainsKey (pointTopSpot.spotIndex))
                    continue;

                var sc = pointTopSpot.spotConfiguration;

                //    X ---> XZ    Top spot
                //   /|     /|
                //  0 ---> Z |
                //  | |    | |
                //  | YX --|YXZ
                //  |/     |/
                //  Y ---> YZ

                //        0    1   2   3    4   5    6     7
                // [8]: this, +X, +Z, +XZ, +Y, +YX, +YZ, +XYZ

                // Spots that will host overrides in addition to already discovered corner spot
                AreaVolumePoint pointTopNeighbourSpot = null;
                AreaVolumePoint pointBottomNeighbourSpot = null;

                // Points that need to be checked for terrain smoothing value
                AreaVolumePoint pointOffsetProviderTop = null;
                AreaVolumePoint pointOffsetProviderBottomA = null;
                AreaVolumePoint pointOffsetProviderBottomB = null;

                // The points that will be used to decide override offset and will be checked for smoothing
                AreaVolumePoint pointTopA = null;
                AreaVolumePoint pointTopB = null;
                AreaVolumePoint pointBottomA = null;
                AreaVolumePoint pointBottomB = null;

                // View from the top
                // TN - top neighbor spot (floor configuration diagonal from center)
                // CR - center spot (corner configuration)
                // BN - bottom neighbor spot (floor configuration diagonal from center)
                //
                //     TN
                //   /    \
                // TA ---- TB
                //   \    /
                //     CR
                //   /    \
                // BA ---- BB
                //   \    /
                //     BN

                if (sc == configRoofCornerOutXPosZNeg)
                {
                    var pointTopNeighborIntermediate = pointTopSpot.pointsWithSurroundingSpots[6];
                    pointTopNeighbourSpot = pointTopNeighborIntermediate?.pointsInSpot[2];

                    var pointBottomNeighborIntermediate = pointBottomSpot.pointsWithSurroundingSpots[5];
                    pointBottomNeighbourSpot = pointBottomNeighborIntermediate?.pointsInSpot[1];

                    pointTopA = pointTopNeighbourSpot?.pointsInSpot[0];
                    pointTopB = pointTopNeighbourSpot?.pointsInSpot[3];
                    pointBottomA = pointBottomSpot.pointsInSpot[0];
                    pointBottomB = pointBottomSpot.pointsInSpot[3];

                    pointOffsetProviderTop = pointTopSpot.pointsInSpot[2];
                    pointOffsetProviderBottomA = pointBottomSpot.pointsInSpot[0];
                    pointOffsetProviderBottomB = pointBottomSpot.pointsInSpot[3];
                }

                else if (sc == configRoofCornerOutXNegZNeg)
                {
                    pointTopNeighbourSpot = pointTopSpot.pointsInSpot[3];
                    pointBottomNeighbourSpot = pointBottomSpot.pointsWithSurroundingSpots[4];

                    pointTopA = pointTopNeighbourSpot?.pointsInSpot[2];
                    pointTopB = pointTopNeighbourSpot?.pointsInSpot[1];
                    pointBottomA = pointBottomSpot.pointsInSpot[2];
                    pointBottomB = pointBottomSpot.pointsInSpot[1];

                    pointOffsetProviderTop = pointTopSpot.pointsInSpot[3];
                    pointOffsetProviderBottomA = pointBottomSpot.pointsInSpot[2];
                    pointOffsetProviderBottomB = pointBottomSpot.pointsInSpot[1];
                }

                else if (sc == configRoofCornerOutXNegZPos)
                {
                    var pointTopNeighborIntermediate = pointTopSpot.pointsWithSurroundingSpots[5];
                    pointTopNeighbourSpot = pointTopNeighborIntermediate?.pointsInSpot[1];

                    var pointBottomNeighborIntermediate = pointBottomSpot.pointsWithSurroundingSpots[6];
                    pointBottomNeighbourSpot = pointBottomNeighborIntermediate?.pointsInSpot[2];

                    pointTopA = pointTopNeighbourSpot?.pointsInSpot[3];
                    pointTopB = pointTopNeighbourSpot?.pointsInSpot[0];
                    pointBottomA = pointBottomSpot.pointsInSpot[3];
                    pointBottomB = pointBottomSpot.pointsInSpot[0];

                    pointOffsetProviderTop = pointTopSpot.pointsInSpot[1];
                    pointOffsetProviderBottomA = pointBottomSpot.pointsInSpot[3];
                    pointOffsetProviderBottomB = pointBottomSpot.pointsInSpot[0];
                }

                else if (sc == configRoofCornerOutXPosZPos)
                {
                    pointTopNeighbourSpot = pointTopSpot.pointsWithSurroundingSpots[4];
                    pointBottomNeighbourSpot = pointBottomSpot.pointsInSpot[3];

                    pointTopA = pointTopNeighbourSpot?.pointsInSpot[1];
                    pointTopB = pointTopNeighbourSpot?.pointsInSpot[2];
                    pointBottomA = pointBottomSpot.pointsInSpot[1];
                    pointBottomB = pointBottomSpot.pointsInSpot[2];

                    pointOffsetProviderTop = pointTopSpot.pointsInSpot[0];
                    pointOffsetProviderBottomA = pointBottomSpot.pointsInSpot[1];
                    pointOffsetProviderBottomB = pointBottomSpot.pointsInSpot[2];
                }

                if (pointTopA == null || pointTopB == null || pointBottomA == null || pointBottomB == null)
                    continue;

                if (!IsPointUsableAsNavOverrideNeighbor (pointTopNeighbourSpot))
                    continue;

                if (!IsPointUsableAsNavOverrideNeighbor (pointBottomNeighbourSpot))
                    continue;

                var posTopA    = pointTopA.pointPositionLocal - navOverrideShiftVertical;
                var posTopB    = pointTopB.pointPositionLocal - navOverrideShiftVertical;
                var posBottomA = pointBottomA.pointPositionLocal - navOverrideShiftVertical;
                var posBottomB = pointBottomB.pointPositionLocal - navOverrideShiftVertical;

                posTopA    += Vector3.up * (pointTopA.terrainOffset * TilesetUtility.blockAssetSize);
                posTopB    += Vector3.up * (pointTopB.terrainOffset * TilesetUtility.blockAssetSize);
                posBottomA += Vector3.up * (pointBottomA.terrainOffset * TilesetUtility.blockAssetSize);
                posBottomB += Vector3.up * (pointBottomB.terrainOffset * TilesetUtility.blockAssetSize);

                if (draw)
                {
                    // Debug.DrawLine (pointTopA.pointPositionLocal, posTopA, Color.green, 5f);
                    // Debug.DrawLine (pointTopB.pointPositionLocal, posTopB, Color.green, 5f);
                    // Debug.DrawLine (posTopA, posTopB, Color.green, 5f);

                    // Debug.DrawLine (pointBottomA.pointPositionLocal, posBottomA, Color.cyan, 5f);
                    // Debug.DrawLine (pointBottomB.pointPositionLocal, posBottomB, Color.cyan, 5f);
                    // Debug.DrawLine (posBottomA, posBottomB, Color.cyan, 5f);

                    Debug.DrawLine (pointOffsetProviderTop.pointPositionLocal, pointOffsetProviderTop.pointPositionLocal + Vector3.up * (pointOffsetProviderTop.terrainOffset * TilesetUtility.blockAssetSize), Color.green, 5f);
                    Debug.DrawLine (pointOffsetProviderBottomA.pointPositionLocal, pointOffsetProviderBottomA.pointPositionLocal + Vector3.up * (pointOffsetProviderBottomA.terrainOffset * TilesetUtility.blockAssetSize), Color.cyan, 5f);
                    Debug.DrawLine (pointOffsetProviderBottomB.pointPositionLocal, pointOffsetProviderBottomB.pointPositionLocal + Vector3.up * (pointOffsetProviderBottomB.terrainOffset * TilesetUtility.blockAssetSize), Color.cyan, 5f);
                }

                if
                (
                    pointOffsetProviderTop.terrainOffset > navOverrideOffsetThresholdNeg ||
                    pointOffsetProviderBottomA.terrainOffset < navOverrideOffsetThresholdPos ||
                    pointOffsetProviderBottomB.terrainOffset < navOverrideOffsetThresholdPos
                )
                    continue;

                if (draw)
                {
                    Debug.DrawLine (posTopA, posBottomB, Color.red, 5f);
                    Debug.DrawLine (posTopB, posBottomA, Color.red, 5f);
                }

                // var posAverage = (posTopA + posTopB + posBottomA + posBottomB) * 0.25f;
                // var offsetYCenter = posAverage.y - pointTopSpot.pointPositionLocal.y + TilesetUtility.blockAssetSize * 0.5f;
                // var offsetYTopNeighbour = offsetYCenter / 3f;
                // var offsetYBottomNeighbour = -offsetYTopNeighbour;

                // TODO: Find a way to extract useful precise offset from points despite slope geometry not aligning nicely with the level
                // Perhaps a terrain collider would be useful here
                var offsetYCenter = -2f;
                var offsetYTopNeighbour = -0.5f;
                var offsetYBottomNeighbour = 0.5f;

                if (draw)
                {
                    Debug.DrawLine (pointTopSpot.instancePosition, pointTopSpot.instancePosition + Vector3.up * offsetYCenter, Color.magenta, 5f);
                    Debug.DrawLine (pointTopNeighbourSpot.instancePosition, pointTopNeighbourSpot.instancePosition + Vector3.up * offsetYTopNeighbour, Color.green, 5f);
                    Debug.DrawLine (pointBottomNeighbourSpot.instancePosition, pointBottomNeighbourSpot.instancePosition + Vector3.up * offsetYBottomNeighbour, Color.cyan, 5f);
                }

                TryAddNavOverride (navOverrides, pointTopSpot, offsetYCenter);
                TryAddNavOverride (navOverrides, pointTopNeighbourSpot, offsetYTopNeighbour);
                // TryAddNavOverride (navOverrides, pointBottomNeighbourSpot, offsetYBottomNeighbour);

                // var pointBottomSpotA = pointOffsetProviderBottomA?.pointsInSpot[3];
                // var pointBottomSpotB = pointOffsetProviderBottomB?.pointsInSpot[3];

                //TryAddNavOverride (navOverrides, pointBottomSpotA, offsetYBottomNeighbour);
                // TryAddNavOverride (navOverrides, pointBottomSpotB, offsetYBottomNeighbour);
            }

            if (navOverridesSaved != null)
            {
                if (navOverridesSavedClearConflicts)
                {
                    navIndexesRemoved.Clear ();
                    foreach (var kvp in navOverridesSaved)
                    {
                        int index = kvp.Key;
                        var navOverride = kvp.Value;
                        if (navOverrides.ContainsKey (index))
                            navIndexesRemoved.Add (index);
                        else
                            navOverrides[index] = navOverride;
                    }

                    if (navIndexesRemoved.Count > 0)
                    {
                        // Removals have to happen in a separate loop to avoid breaking the navOverridesSaved iterator above
                        foreach (var index in navIndexesRemoved)
                            navOverridesSaved.Remove (index);

                        Debug.LogWarning ($"Removed {navIndexesRemoved.Count} saved nav overrides due to conflict with autogenerated nav overrides");
                    }
                }
                else
                {
                    foreach (var kvp in navOverridesSaved)
                    {
                        int index = kvp.Key;
                        var navOverride = kvp.Value;
                        navOverrides[index] = navOverride;
                    }
                }
            }
        }

        private static void TryAddNavOverride (Dictionary<int, AreaDataNavOverride> navOverrides, AreaVolumePoint point, float offset)
        {
            if (point == null)
                return;

            var index = point.spotIndex;
            if (navOverrides.ContainsKey (index))
                return;

            navOverrides.Add
            (
                index,
                new AreaDataNavOverride
                {
                    pivotIndex = index,
                    offsetY = offset
                }
            );
        }

        public static void RebuildPointSearchData (AreaManager am)
        {
            PointData.SetBounds (am.boundsFull);
	        pointSearchData = new List<int> (am.points.Count);
	        for (int i = 0; i < am.points.Count; ++i)
	        {
                pointSearchData.Add (defaultNodeIndex);
	        }
        }

        public static void OnLevelUnload ()
        {
            pointSearchData = null;
        }

        public static void GetNavigationNodes (ref List<IAreaNavNode> externalGraph, AreaManager am = null)
        {
            if (am == null)
                am = CombatSceneHelper.ins.areaManager;

            if (am == null)
            {
                Debug.Log ("AN | AreaManager not found");
                return;
            }

            // Debug.Log ("AN | GetNavigationNodes | Starting graph generation");

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch ();
            timer.Start ();

            int areaNameHash = am.areaName.GetHashCode ();
            bool rebuildPointSearchData = pointSearchData == null || pointSearchData.Count != am.points.Count || lastLevelID != areaNameHash;

            // Always rebuild search data in edit mode, in case we did some big changes to same level
            if (!Application.isPlaying)
                rebuildPointSearchData = true;

            if (rebuildPointSearchData)
            {
                lastLevelID = areaNameHash;
                RebuildPointSearchData (am);
            }

            if (externalGraph == null)
            {
                var appoximateCapacity = am.boundsFull.x * am.boundsFull.z + 100;
                externalGraph = new List<IAreaNavNode> (appoximateCapacity);
            }
            else
                externalGraph.Clear ();

            var overrides = am.navOverrides;
            int linkIndex = 0;

            for (int i = pointSearchData.Count - 1; i >= 0; --i)
            {
                var point = am.points[i];

                // if (point.spotConfiguration != point.spotConfigurationWithDamage)
                //     Debug.Log ($"{point.spotIndex} | SC != SCD: {point.spotConfiguration}, {point.spotConfigurationWithDamage}");

                // Overrides are only valid if there is no damage to involved volume
                bool overridePresentAndValid =
                    overrides.ContainsKey (point.spotIndex) &&
                    !point.spotHasDamagedPoints;

                // Only floors can house nodes, and only if they are not neighbored by tall walls
                bool configValidAndClear =
                    point.spotConfigurationWithDamage == configFloor &&
                    IsNotNeighbouringWalls (point.spotIndex, am.points);

                // If tileset defines restriction for a given group/subtype, take this back
                if (configValidAndClear)
                {
                    bool navigationAllowedByTileset = AreaTilesetHelper.IsNavigationAllowed
                    (
                        point.blockTileset,
                        point.blockGroup,
                        point.blockSubtype
                    );

                    if (!navigationAllowedByTileset)
                        configValidAndClear = false;
                }

                bool nodeNeeded = overridePresentAndValid || configValidAndClear;

                if (!nodeNeeded)
                {
                    pointSearchData[i] = defaultNodeIndex;
                    continue;
                }

                float offset = 0f;
                if (overridePresentAndValid)
                    offset = am.navOverrides[point.spotIndex].offsetY;

                var node = new AreaNavNode ();
                node.SetPointIndex (point.spotIndex);
                node.SetPosition (offset != 0f ? point.instancePosition + Vector3.up * offset : point.instancePosition);
                node.SetNodeIndex (externalGraph.Count);

                pointSearchData[i] = node.GetNodeIndex ();
                externalGraph.Add (node);
            }

            // Debug.Log ("Graph size: " + graph.Count);

            int nodeCount = externalGraph.Count;
            linkCount = 0;

            for (int i = 0; i < nodeCount; ++i)
            {
	            IAreaNavNode node = externalGraph[i];
	            var pointIndex = node.GetPointIndex ();

	            SearchForLink (am, searchPresetStraight, pointIndex, externalGraph, overrides);
	            SearchForLink (am, searchPresetDiagonal, pointIndex, externalGraph, overrides);

                SearchForLink (am, searchPresetOverrideUp, pointIndex, externalGraph, overrides);
                SearchForLink (am, searchPresetOverrideUpCorner, pointIndex, externalGraph, overrides);
                SearchForLink (am, searchPresetOverrideDown, pointIndex, externalGraph, overrides);

	            SearchForLink (am, searchPresetJumpUp, pointIndex, externalGraph, overrides);
	            SearchForLink (am, searchPresetJumpDown, pointIndex, externalGraph, overrides);
	            SearchForLink (am, searchPresetJumpOverDrop, pointIndex, externalGraph, overrides);
	            SearchForLink (am, searchPresetJumpOverClimb, pointIndex, externalGraph, overrides);
            }

            timer.Stop ();

            if (nodeCount != nodeCountLast || linkCount != linkCountLast)
                Debug.Log ($"AN | GetNavigationNodes | Graph rebuild completed in {timer.Elapsed.Milliseconds} ms | Node count: {nodeCountLast} > {nodeCount} | Link count: {linkCountLast} > {linkCount}");
            else
                Debug.Log ($"AN | GetNavigationNodes | Graph rebuild completed in {timer.Elapsed.Milliseconds} ms | No changes to node/link count: {nodeCount}/{linkCount}");

            nodeCountLast = nodeCount;
            linkCountLast = linkCount;
        }

        private static int linkCount = 0;

        private static int nodeCountLast = 0;
        private static int linkCountLast = 0;

        private static LinkSearchStep[] searchSequence;
        private static LinkSearchStep searchStep;
        private static AreaNavNode nodeRuntime;
        private static List<AreaNavLink> reusedLinks;

        private static void SearchForLink
        (
            AreaManager am,
            LinkSearchPreset preset,
            int indexOrigin,
            List<IAreaNavNode> graphUsed,
            Dictionary<int, AreaDataNavOverride> overrides
        )
        {
            var searchNeighbour = indexOrigin;

            var overridePolicyStart = preset.overridePolicyStart;
            if (overridePolicyStart == OverridePolicy.OverrideProhibited || overridePolicyStart == OverridePolicy.OverrideRequired)
            {
                bool startIsOverride = overrides.ContainsKey (indexOrigin);
                bool startOverrideRequired = overridePolicyStart == OverridePolicy.OverrideRequired;

                // Bail if we don't satisfy start policy
                if (startIsOverride != startOverrideRequired)
                    return;
            }

            // Each search preset contains multiple search sequences, so we iterate through preset.steps to get each
            for (var searchIndexOfSequence = 0; searchIndexOfSequence < preset.sequences.Length; searchIndexOfSequence += 1)
            {
                // We retrieve the sequence to a reused variable to avoid getting the element from preset.sequences every time
                // A search sequence is an array of step objects each containing a direction and a configuration mask
                searchSequence = preset.sequences[searchIndexOfSequence];

                // It's important to reset the origin at each step, since it's modified in the process of sequence evaluation
                // Each individual step is using it to continue to neighbours, so it's changed in each subsequent step to progress the chain
                var searchOrigin = indexOrigin;
                var searchSequenceSuccessful = false;

                // We'll iterate through the steps until we're through
                for (var searchIndexOfStep = 0; searchIndexOfStep < searchSequence.Length; searchIndexOfStep += 1)
                {
                    // We retrieve the step to a reused variable to avoid getting the element from searchSequence every time
                    searchStep = searchSequence[searchIndexOfStep];
                    bool lastStep = searchIndexOfStep == searchSequence.Length - 1;

                    // The step is considered successful when we encounter a match for configuration mask and verify vertical clearance
                    var searchStepSuccessful = false;

                    // Origin point might not have a neighbour in a direction our step requires
                    // If neighbour is present, we retrieve it to a reused variable to avoid getting the element from searchOrigin.neighbours every time
                    var point = PointData.GetNeighbourPoint (searchOrigin, searchStep.direction, am.points);
                    if (point != null)
                    {
                        searchNeighbour = point.spotIndex;
                        // We need to verify if we have a match to currently examined neighbour configuration
                        bool match = point.spotConfigurationWithDamage == searchStep.mask;
                        if (match || (lastStep && !preset.strictDestination))
                        {
                            // Got a match! Next, we need to verify that the point has enough vertical clearance for the mechs: two empty spots above the examined one
                            // We take over the reused variable, since current step won't have any further use for it in it's original context
                            point = PointData.GetNeighbourPoint (searchNeighbour, (int)PointNeighbourDirection.YNeg, am.points);
                            if (point != null)
                            {
                                if (point.spotConfigurationWithDamage == configEmpty)
                                {
                                    // Great, we verified that we have 1 empty spot above the potential link destination - time to verify we have second half of empty space further up
                                    // We take over the reused variable yet again (originally it was at the floor, then it was in an empty cell above it, now it's 1 step up again)
                                    point = PointData.GetNeighbourPoint (point.spotIndex, (int)PointNeighbourDirection.YNeg, am.points);
                                    if (point != null)
                                    {
                                        if (point.spotConfigurationWithDamage == configEmpty)
                                        {
                                            // Awesome - we're not on top of the level, but we had two empty cells above the floor point in this step - vertical clearance is guaranteed
                                            searchStepSuccessful = true;
                                        }
                                    }
                                    else
                                    {
                                        // We had one empty cell and a level boundary above the floor point in this step - vertical clearance is guaranteed
                                        searchStepSuccessful = true;
                                    }
                                }
                            }
                            else
                            {
                                // If there is no point above, then we're at top edge of the map and vertical clearance is guaranteed
                                searchStepSuccessful = true;
                            }
                        }
                    }

                    // Before we can continue (or finish, if this is last step), we need to verify if this destination satisfies override policy
                    if (searchStepSuccessful)
                    {
                        var overridePolicyDestination = searchStep.overridePolicy;
                        if (overridePolicyDestination == OverridePolicy.OverrideProhibited || overridePolicyDestination == OverridePolicy.OverrideRequired)
                        {
                            bool destinationIsOverride = overrides.ContainsKey (searchNeighbour);
                            bool destinationOverrideRequired = overridePolicyDestination == OverridePolicy.OverrideRequired;

                            // Bail if we don't satisfy destination policy
                            if (destinationIsOverride != destinationOverrideRequired)
                                searchStepSuccessful = false;
                        }
                    }

                    if (searchStepSuccessful)
                    {
                        if (lastStep)
                        {
                            // If we're here and last step was successful, we don't need any further operations with search origin and can exit straight to sequence success
                            // No point breaking the iteration, since we're at the very last loop if we're here
                            searchSequenceSuccessful = true;
                        }
                        else
                        {
                            // If we're here, it's time to hand off the search origin variable to currently evaluated neighbour before we enter next step
                            searchOrigin = searchNeighbour;
                        }
                    }
                    else
                    {
                        // If search step was unsuccessful, there is no point continuing with a given sequence - let's break and go up
                        break;
                    }
                }

                var nodeIndexNeighbour = pointSearchData[searchNeighbour];
                if (searchSequenceSuccessful && nodeIndexNeighbour != defaultNodeIndex)
                {
                    // Next, we fetch the start and destination nodes to prepare for last checks
                    var nodeDestination = graphUsed[nodeIndexNeighbour];
                    var nodeStart = graphUsed[pointSearchData[indexOrigin]];
                    var searchLinkValidated = true;

                    // If the starting node (one attached to search origin point) has no link collection, we're safe to add a new node to newly created collection
                    if (nodeStart.GetLinks () == null)
                        nodeStart.SetLinks (new List<AreaNavLink> (8));

                    // If the collection already existed, we need to verify it doesn't contain exact same link before adding it
                    else
                    {
                        reusedLinks = nodeStart.GetLinks ();
                        for (var searchLinkIndex = 0; searchLinkIndex < reusedLinks.Count; searchLinkIndex += 1)
                        {
                            var searchLinkExamined = reusedLinks[searchLinkIndex];
                            if (searchLinkExamined.type == preset.type && searchLinkExamined.destinationIndex == nodeDestination.GetNodeIndex ())
                            {
                                searchLinkValidated = false;
                                break;
                            }
                        }
                    }

                    if (searchLinkValidated)
                    {
                        reusedLinks = nodeStart.GetLinks ();
                        reusedLinks.Add (new AreaNavLink (preset.type, nodeDestination.GetNodeIndex ()));
                    }

                    linkCount += 1;
                }
            }
        }



        private static Dictionary<AreaNavLinkType, uint> navCosts;
        private static bool builtNavCosts = false;

        private static void BuildNavCosts ()
        {
            navCosts = new Dictionary<AreaNavLinkType, uint>();
            var data = DataLinkerSettingsArea.data;

            navCosts.Add (AreaNavLinkType.Horizontal, (uint)data.horizontalCost);
            navCosts.Add (AreaNavLinkType.Diagonal, (uint)data.diagonalCost);
            navCosts.Add (AreaNavLinkType.JumpDown, (uint)data.jumpDownCost);
            navCosts.Add (AreaNavLinkType.JumpOverClimb, (uint)data.jumpOverClimbCost);
            navCosts.Add (AreaNavLinkType.JumpOverDrop, (uint)data.jumpOverDropCost);
            navCosts.Add (AreaNavLinkType.JumpUp, (uint)data.jumpUpCost);
            builtNavCosts = true;
        }

        public static uint GetNodeCost (AreaNavLinkType nodeType)
        {
            if (!builtNavCosts)
                BuildNavCosts();

            return navCosts[nodeType];
        }





        private static readonly int[] wallCheckDirections = new int[]
        {
            (int)PointNeighbourDirection.XPos,
            (int)PointNeighbourDirection.XNeg,
            (int)PointNeighbourDirection.ZPos,
            (int)PointNeighbourDirection.ZNeg
        };

        private static bool IsNotNeighbouringWalls (int index, List<AreaVolumePoint> points)
        {
            for (int i = 0; i < wallCheckDirections.Length; ++i)
            {
                // We grab a horizontal direction from a list of used directions (contains 4 for now, but subject to change, maybe some cases will need diagonals)
                var wallCheckDirection = wallCheckDirections[i];

                // Retrieving a horizontal neighbour into a temporary variable - we'll check whether it houses the bottom of some wall
                var point = PointData.GetNeighbourPoint (index, wallCheckDirection, points);
                if (point != null)
                {
                    point = PointData.GetNeighbourPoint (point.spotIndex, (int)PointNeighbourDirection.YNeg, points);
                    if (point != null)
                    {
                        var topEmpty = (point.spotConfigurationWithDamage & TilesetUtility.configurationTopMask) == 0;
                        if (!topEmpty)
                            return false;
                    }
                }
            }

            return true;
        }
    }

}
