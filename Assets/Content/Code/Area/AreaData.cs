using System;
using System.Collections.Generic;

namespace Area
{
    [Serializable]
    public class AreaDataCore
    {
        public Vector3Int bounds;
        public int damageRestrictionDepth;
        public int damagePenetrationDepth;
        public int gradientOffsetBottom;
        public int gradientOffsetTop;
    }

    [Serializable]
    public struct AreaDataSpot
    {
        public int index;
        public int tileset;
        public byte group;
        public byte subtype;
        public byte rotation;
        public bool flip;
        public byte offset;
    }

    [Serializable]
    public struct AreaDataCustomization
    {
        public int index;
        public float h1;
        public float s1;
        public float b1;
        public float h2;
        public float s2;
        public float b2;
        public float emission;
    }

    [Serializable]
    public struct AreaDataIntegrity
    {
        public int index;
        public float integrity;
        public bool destructible;
    }

    [Serializable]
    public struct AreaDataProp
    {
        public int id;
        public int pivotIndex;
        public byte rotation;
        public bool flip;
        public byte status;
        public float offsetX;
        public float offsetZ;
        public float h1;
        public float s1;
        public float b1;
        public float h2;
        public float s2;
        public float b2;
    }
    
    [Serializable]
    public struct AreaTilesetNavOverride
    {
        public byte config;
        public byte group;
        public byte subtype;
        public float offsetY;
    }
    
    [Serializable]
    public struct AreaTilesetNavRestriction
    {
        public List<byte> subtypes;
    }

    [Serializable]
    public struct AreaDataNavOverride
    {
        public int pivotIndex;
        public float offsetY;
    }

    public enum AreaVolumePointState
    {
        Empty = 0,
        FullDestroyed = 1,
        Full = 2
    }

    //    X ---> XZ
    //   /|     /|
    //  0 ---> Z |
    //  | |    | |
    //  | YX --|YXZ
    //  |/     |/
    //  Y ---> YZ 

    public enum PointNeighbourDirection
    {
        XPos = 0, // 1,
        XNeg = 2, // 3,
        YPos = 4,
        YNeg = 5,
        ZPos = 3, // 0,
        ZNeg = 1, // 2,
        XPosZPos = 6,
        XPosZNeg = 7,
        XNegZPos = 8,
        XNegZNeg = 9
    }

    public class AreaVolumePointSearchData
    {
        public AreaVolumePoint point;
        public int status;

        public AreaVolumePointSearchData parent;
        public AreaVolumePointSearchData parentCandidate;

        public PointNeighbourDirection directionFromParent;
        public PointNeighbourDirection directionFromParentCandidate;

        public AreaVolumePointSearchData neighbourXPos;
        public AreaVolumePointSearchData neighbourXNeg;
        public AreaVolumePointSearchData neighbourZPos;
        public AreaVolumePointSearchData neighbourZNeg;
        public AreaVolumePointSearchData neighbourYPos;
        public AreaVolumePointSearchData neighbourYNeg;

        public AreaVolumePointSearchData neighbourXPosZPos;
        public AreaVolumePointSearchData neighbourXPosZNeg;
        public AreaVolumePointSearchData neighbourXNegZPos;
        public AreaVolumePointSearchData neighbourXNegZNeg;
    }
}