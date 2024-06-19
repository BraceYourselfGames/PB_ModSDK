using System.Collections.Generic;

using UnityEngine;

namespace Area.Scene
{
    public enum EditingMode
    {
        Volume = 0,
        Tileset,
        Spot,
        Transfer,
        Damage,
        Color,
        Props,
        Navigation,
        Roads,
        RoadCurves,
        TerrainRamp,
        Layers,
        TerrainShape,
        // !!! Put new modes before this line.
        // This is not a mode and should be the last entry.
        Count,
    }

    enum PropEditingMode
    {
        Place,
        Color,
    }

    enum PropEditCommand
    {
        None = 0,
        Snap,
        RotateLeft,
        RotateRight,
        Flip,
        ResetRotation,
        Offset,
        CopyPosition,
        PastePosition,
        ResetPosition,
        ChangeColor,
        CopyColor,
        PasteColor,
        ResetColor,
        ChangeSelected,
        DeleteSelected,
    }

    enum PropCopyPasteReset
    {
        None = 0,
        Copy,
        Paste,
        Reset,
    }

    enum ClipboardSource
    {
        Scene = 0,
        Snippet,
    }

    enum SpotSearchType
    {
        None = 0,
        SameFloor = 1,
        SameFloorIsolated = 2,
        SameConfiguration = 3,
        SameTileset = 4,
        SameEverything = 5,
        AllEmptyNodes = 6,
        SameColor = 7,
    }

    enum SpotCursorType
    {
        Cube = 0,
        Square,
    }

    enum SpotHoverType
    {
        Empty = 0,
        Terrain,
        Editable,
    }

    enum SpotChange
    {
        None = 0,
        Empty,
        Interior,
        State,
        Tileset,
        StateAndTileset,
        InteriorMarkedAll,
        InteriorMarkedLayer,
        FullMarkedAll,
        FullMarkedLayer,
    }

    // The AreaVolumePoint class keeps its neighboring points in two different array.
    // The pointsInSpot array holds neighbors to the north, east and down in world space. This is Point.
    // The pointsWithSurroundingSpots array holds neighbors to the south, west and up in world space. This is Spot.
    enum AVPArray
    {
        Invalid = -1,
        Point,
        Spot,
    }

    static class BoundsSpace
    {
        // This is the native space of AreaVolumePoints. It is a mirror image of world space in the positive Y direction.
        // Because of that, the Y neighbors are flipped so the WorldSpace up neighbor is the BoundsSpace down one.
        // Grid size in BoundsSpace is WorldSpace.BlockSize.

        public static readonly Vector3 SpotOffsetFromPoint = new Vector3 (0.5f, 0.5f, 0.5f);

        public static class PointNeighbor
        {
            public const int East = 1;
            public const int North = 2;
            public const int Up = 4;
        }

        public static class SpotNeighbor
        {
            public const int Down = 3;
            public const int South = 5;
            public const int West = 6;
        }

        public static readonly Vector3Int BottomValue = Vector3Int.size2x2x2;
    }

    static class WorldSpace
    {
        public const float BlockSize = TilesetUtility.blockAssetSize;
        public const float HalfBlockSize = BlockSize / 2f;
        public static readonly Vector3 SpotOffsetFromPoint = BoundsSpace.SpotOffsetFromPoint * BlockSize;

        // The cardinal directions are defined with a birds-eye view looking down on the XZ plane.
        // Blocks are axis-aligned.

        public enum Compass
        {
            North = 0,
            West,
            East,
            South,
            Up,
            Down,
        }

        // Point neighbors in world space extend to the north and east and down.
        // North : positive Z
        // East : postive X
        // Down : negative Y
        public static class PointNeighbor
        {
            public const int East = 1;
            public const int North = 2;
            public const int NorthEast = 3;
            public const int Down = 4;
        }

        // Spot neighbors in world space extend to the south and west and up.
        public static class SpotNeighbor
        {
            public const int UpSouthWest = 0;
            public const int UpSouth = 1;
            public const int UpWest = 2;
            public const int Up = 3;
            public const int SouthWest = 4;
            public const int South = 5;
            public const int West = 6;
            public const int Self = 7;
        }
    }

    class ClipboardTilesetInfo
    {
        public List<byte> Configurations = new List<byte> ();
        public int Tileset = 0;
        public byte Group = 0;
        public byte Subtype = 0;
        public byte Rotation = 0;
        public bool Flipping = false;
        public bool MustOverwriteSubtype = true;
        public bool OverwriteColor = false;
        public TilesetVertexProperties Color = TilesetVertexProperties.defaults;
    }

    class ClipboardPropColor
    {
        public Vector4 HSBPrimary;
        public Vector4 HSBSecondary;
    }

    class TilesetColorInfo
    {
        public bool ApplyOverlaysOnColorApply = false;
        public bool ApplyMainOnColorApply = true;
        public int SelectedTilesetId = 0;
        public HSBColor SelectedPrimaryColor;
        public HSBColor SelectedSecondaryColor;
        public float OverrideValue = 0f;
    }

    class PropEditInfo
    {
        public AreaPlacementProp PlacementHandled;
        public int PlacementListIndex = -1;
        public int SpotIndexVisualized = -1;
        public int SelectionID;
        public int Index;
        public int IndexVisualized = -1;
        public byte Rotation;
        public int RotationVisualized;
        public bool Flipped;
        public bool FlippedVisualized;
        public float OffsetX;
        public float OffsetZ;
        public float OffsetXVisualized;
        public float OffsetZVisualized;
        public Vector4 HSBPrimary = Constants.defaultHSBOffset;
        public Vector4 HSBSecondary = Constants.defaultHSBOffset;
        public Vector4 HSBPrimaryVisualized = Constants.defaultHSBOffset;
        public Vector4 HSBSecondaryVisualized = Constants.defaultHSBOffset;
        public float OffsetXClipboard = 0f;
        public float OffsetZClipboard = 0f;

        public void Reset ()
        {
            PlacementHandled = default;
            PlacementListIndex = -1;
            SpotIndexVisualized = -1;
            IndexVisualized = -1;
        }
    }

    static class Colors
    {
        public static class Axis
        {
            public static readonly Color XPos = new Color (1f, 0.5f, 0.5f, 1f);
            public static readonly Color XNeg = new Color (1f, 0.4f, 0.8f, 1f);
            public static readonly Color ZPos = new Color (0.6f, 0.8f, 1f, 1f);
            public static readonly Color ZNeg = new Color (0.2f, 0.8f, 1f, 1f);
            public static readonly Color YPos = new Color (0.5f, 1f, 0.6f, 1f);
        }

        public static class Link
        {
            public static readonly Color Horizontal = new Color (1.0f, 0.25f, 0.25f);
            public static readonly Color Diagonal = new Color (1.0f, 0.50f, 0.25f);
            public static readonly Color JumpUp = new Color (0.25f, 1.0f, 0.25f);
            public static readonly Color JumpDown = new Color (0.25f, 0.45f, 0.9f);
            public static readonly Color JumpOverDrop = new Color (0.8f, 0.8f, 0.25f);
            public static readonly Color JumpOverClimb = new Color (0.6f, 0.8f, 0.4f);
        }

        public static class Offset
        {
            public static readonly Color Neutral = new Color (1f, 1f, 1f, 0.25f);
            public static readonly Color Pos = new Color (0.25f, 1.0f, 0.25f, 1f);
            public static readonly Color Neg = new Color (1.0f, 0.50f, 0.25f, 1f);
        }

        public static class Volume
        {
            public static readonly Color FadedIndestructibleFlag = new Color (1f, 0.5f, 0.2f, 0.1f);
            public static readonly Color FadedIndestructibleHard = new Color (1f, 0.35f, 0.55f, 0.1f);

            public static readonly Color MainDestructibleUntracked = new Color (0.65f, 1f, 0.35f, 1f);
            public static readonly Color MainDestructible = new Color (0.5f, 1f, 1f, 1f);
            public static readonly Color MainIndestructibleIndr = new Color (0.7f, 0.7f, 1, 1f);
            public static readonly Color MainIndestructibleFlag = new Color (1f, 0.5f, 0.2f, 1f);
            public static readonly Color MainIndestructibleHard = new Color (1f, 0.35f, 0.55f, 1f);

            public static readonly Color NeighborSecondary = new Color (0.2f, 0.8f, 1f, 0.2f);
            public static readonly Color NeighborPrimaryDestructibleUntracked = new Color (0.65f, 1f, 0.35f, 0.4f);
            public static readonly Color NeighborPrimaryDestructible = new Color (0.5f, 1f, 1f, 0.4f);
            public static readonly Color NeighborPrimaryIndestructibleFlag = new Color (1f, 0.5f, 0.2f, 0.4f);
            public static readonly Color NeighborPrimaryIndestructibleHard = new Color (1f, 0.35f, 0.55f, 0.4f);
        }
    }

    static class SpotsByFace
    {
        // When removing a block, we need to start with the spots opposite the targeted face. That's because those
        // spots might be interior spots. Interior spots don't have a tileset ID and they will now that they're
        // exposed and no longer interior. If the swapTileset flag isn't set, then the tilesets from the face spots
        // get pushed down to the interior spots. However, the face spots may become empty after the remove operation
        // which clears their tileset so we have to process the interior spots before the face spots to make sure we
        // don't lose the face spot tilesets.
        public const int FaceSpotStartIndex = 4;
        public static readonly Dictionary<int, int[]> Map = new Dictionary<int, int[]> ()
        {
            [(int)WorldSpace.Compass.Up] = new[]
            {
                WorldSpace.SpotNeighbor.Self,
                WorldSpace.SpotNeighbor.West,
                WorldSpace.SpotNeighbor.SouthWest,
                WorldSpace.SpotNeighbor.South,
                WorldSpace.SpotNeighbor.Up,
                WorldSpace.SpotNeighbor.UpWest,
                WorldSpace.SpotNeighbor.UpSouthWest,
                WorldSpace.SpotNeighbor.UpSouth,
            },
            [(int)WorldSpace.Compass.Down] = new[]
            {
                WorldSpace.SpotNeighbor.UpWest,
                WorldSpace.SpotNeighbor.Up,
                WorldSpace.SpotNeighbor.UpSouth,
                WorldSpace.SpotNeighbor.UpSouthWest,
                WorldSpace.SpotNeighbor.West,
                WorldSpace.SpotNeighbor.Self,
                WorldSpace.SpotNeighbor.South,
                WorldSpace.SpotNeighbor.SouthWest,
            },
            [(int)WorldSpace.Compass.North] = new[]
            {
                WorldSpace.SpotNeighbor.UpSouthWest,
                WorldSpace.SpotNeighbor.UpSouth,
                WorldSpace.SpotNeighbor.South,
                WorldSpace.SpotNeighbor.SouthWest,
                WorldSpace.SpotNeighbor.UpWest,
                WorldSpace.SpotNeighbor.Up,
                WorldSpace.SpotNeighbor.Self,
                WorldSpace.SpotNeighbor.West,
            },
            [(int)WorldSpace.Compass.West] = new[]
            {
                WorldSpace.SpotNeighbor.UpSouthWest,
                WorldSpace.SpotNeighbor.UpWest,
                WorldSpace.SpotNeighbor.West,
                WorldSpace.SpotNeighbor.SouthWest,
                WorldSpace.SpotNeighbor.UpSouth,
                WorldSpace.SpotNeighbor.Up,
                WorldSpace.SpotNeighbor.Self,
                WorldSpace.SpotNeighbor.South,
            },
            [(int)WorldSpace.Compass.East] = new[]
            {
                WorldSpace.SpotNeighbor.UpWest,
                WorldSpace.SpotNeighbor.UpSouthWest,
                WorldSpace.SpotNeighbor.SouthWest,
                WorldSpace.SpotNeighbor.West,
                WorldSpace.SpotNeighbor.Up,
                WorldSpace.SpotNeighbor.UpSouth,
                WorldSpace.SpotNeighbor.South,
                WorldSpace.SpotNeighbor.Self,

            },
            [(int)WorldSpace.Compass.South] = new[]
            {
                WorldSpace.SpotNeighbor.Up,
                WorldSpace.SpotNeighbor.UpWest,
                WorldSpace.SpotNeighbor.West,
                WorldSpace.SpotNeighbor.Self,
                WorldSpace.SpotNeighbor.UpSouth,
                WorldSpace.SpotNeighbor.UpSouthWest,
                WorldSpace.SpotNeighbor.SouthWest,
                WorldSpace.SpotNeighbor.South,
            },
        };
    }

    // Fetch 3 surrounding points (exclude direction you came from)
    // For each result:
    // - check if configuration isn't 0
    // - check if it's not present in the results array yet
    // - add it to results array

    // AreaVolumePoint neighbour reference arrays:

    //    X ---> XZ
    //   /|     /|
    //  0 ---> Z |
    //  | |    | |
    //  | YX --|YXZ
    //  |/     |/
    //  Y ---> YZ

    //        0    1   2    3   4    5    6    7
    // [8]: this, +X, +Z, +XZ, +Y, +YX, +YZ, +XYZ
    // AreaVolumePoint[] spotPoints;

    //   YZ <--- Y
    //   /|     /|
    // YXZ <-- YX|
    //  | |    | |
    //  | Z <--| 0
    //  |/     |/
    // XZ <--- X

    //        0    1    2    3    4   5   6   7
    // [8]: -XYZ, -YZ, -XY, -Y, -XZ, -Z, -X, this
    // AreaVolumePoint[] spotsAroundThisPoint;
}
