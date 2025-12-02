using UnityEngine;
using System.Collections.Generic;

namespace Area
{
    // Uncomment this to inspect point state in inspectors, but don't leave serialization enabled in live versions - forced filling of nullable fields from serialization can break things
    // [System.Serializable]
    public class AreaVolumePoint
    {
        // Every volume point is, at the same time, responsible for a specific point in space (which can be full/empty/destroyed), 
        // and a so-called spot - information about 2x2x2 group of points (which determines the shape of level geometry)

        // Some operations require knowing the state of volume points comprising a spot group with the current point - instead of calculating their indexes and fetching them every time,
        // we can use the cached references provided below. Those are typically used when you need to know a detailed spot state (e.g. for damage) from a given volume point in negative XYZ corner of 2x2x2 group.

        //    X ---> XZ
        //   /|     /|
        //  0 ---> Z |
        //  | |    | |
        //  | YX --|YXZ
        //  |/     |/
        //  Y ---> YZ 

        //        0    1   2   3    4   5    6     7
        // [8]: this, +X, +Z, +XZ, +Y, +YX, +YZ, +XYZ
        public AreaVolumePoint[] pointsInSpot;

        // Another set of cached references to other points goes into a negative direction and is typically used in operations where you need to work with 8 spots surrounding a given (current) point
        // Since spots are attached to negative XYZ corner, our current point describes positive XYZ corner of 2x2x2 group of spots, and info about other spots around can be reached by going one step back on XYZ axes

        //   YZ <--- Y
        //   /|     /|
        // YXZ <-- YX|
        //  | |    | |
        //  | Z <--| 0
        //  |/     |/
        // XZ <--- X 

        //        0    1    2    3   4    5   6   7
        // [8]: -XYZ, -YZ, -XY, -Y, -XZ, -Z, -X, this
        public AreaVolumePoint[] pointsWithSurroundingSpots;

        public AreaVolumePointState pointState = AreaVolumePointState.Empty; 
        public Vector3Int pointPositionIndex = new Vector3Int (-1, -1, -1);
        public Vector3 pointPositionLocal = Vector3.zero;

        public bool spotPresent = true;
        public byte spotConfiguration = 0;
        public byte spotConfigurationWithDamage = 0;
        public int spotIndex = 0;
        public bool spotHasDamagedPoints = false;

        public int blockTileset = 0;
        public byte blockGroup = 0;
        public byte blockSubtype = 0;
        public byte blockRotation = 0;
        public bool blockFlippedHorizontally = false;

        public bool destructible = true;
        public bool indestructibleIndirect = false;
        public bool indestructibleAny => indestructibleIndirect | !destructible;
        
        public bool destructionUntracked = false;
        
        public float integrity = 1f;
        public float integrityForDestructionAnimation = 1f;

        public float terrainOffset = 0f;
        
        public int structuralValue = 0;
        public int structuralGroup = 0;
        public AreaVolumePoint structuralParent = null;
        public AreaVolumePoint structuralParentCandidate = null;
        public bool structuralChildrenPresent = false;

        public GameObject instanceCollider = null;

        public Vector3 instancePosition;
        public AreaTilesetLight lightData = null;

        public Quaternion instanceMainRotation;
        public Quaternion instanceInteriorRotation;
        // public List<Quaternion> instanceDamageRotations;

        public Vector4 instanceMainScaleAndSpin = Vector4.one;
        public Vector4 instanceInteriorScaleAndSpin = Vector4.one;

        public float instanceVisibilityInterior = 1f;
        
        public TilesetVertexProperties customization = TilesetVertexProperties.defaults;

        public AreaSimulatedChunk simulatedHelper;
        public bool road = false;

        public AreaVolumePoint ()
        {

        }

        public void RecheckDamage ()
        {
            if (!spotPresent)
                spotHasDamagedPoints = false;
            else
            {
                for (int i = 0; i < 8; ++i)
                {
                    AreaVolumePoint neighbourPoint = pointsInSpot[i];
                    if (neighbourPoint != null && neighbourPoint.pointState == AreaVolumePointState.FullDestroyed)
                    {
                        spotHasDamagedPoints = true;
                        break;
                    }
                }
            }
        }

        public void RecheckRubbleHosting ()
        {
            
        }

        public AreaVolumePointConfiguration GetConfigurationStruct ()
        {
            return new AreaVolumePointConfiguration
            (
                pointsInSpot[0]?.pointState ?? AreaVolumePointState.Empty,
                pointsInSpot[1]?.pointState ?? AreaVolumePointState.Empty,
                pointsInSpot[2] != null ? pointsInSpot[3].pointState : AreaVolumePointState.Empty,
                pointsInSpot[3] != null ? pointsInSpot[2].pointState : AreaVolumePointState.Empty,
                pointsInSpot[4]?.pointState ?? AreaVolumePointState.Empty,
                pointsInSpot[5]?.pointState ?? AreaVolumePointState.Empty,
                pointsInSpot[6] != null ? pointsInSpot[7].pointState : AreaVolumePointState.Empty,
                pointsInSpot[7] != null ? pointsInSpot[6].pointState : AreaVolumePointState.Empty
            );
        }

        public bool IsSurroundedByFullPoints ()
        {
            AreaVolumePoint pointNeighbourXPos = pointsInSpot[1];
            AreaVolumePoint pointNeighbourZPos = pointsInSpot[2];
            AreaVolumePoint pointNeighbourXNeg = pointsWithSurroundingSpots[6];
            AreaVolumePoint pointNeighbourZNeg = pointsWithSurroundingSpots[5];

            return
            (
                (pointNeighbourXPos == null || pointNeighbourXPos.pointState == AreaVolumePointState.Full) &&
                (pointNeighbourZPos == null || pointNeighbourZPos.pointState == AreaVolumePointState.Full) &&
                (pointNeighbourXNeg == null || pointNeighbourXNeg.pointState == AreaVolumePointState.Full) &&
                (pointNeighbourZNeg == null || pointNeighbourZNeg.pointState == AreaVolumePointState.Full)
            );
        }
        
        
    }
}