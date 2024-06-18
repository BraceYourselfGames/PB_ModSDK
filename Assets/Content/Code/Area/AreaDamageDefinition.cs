using UnityEngine;

namespace Area
{
    [System.Serializable]
    public class AreaDamageDefinition
    {
        public int priority = 0;
        public AreaVolumePointState[] configurationCompatible;

        //    X ---> XZ
        //   /|     /|
        //  0 ---> Z |
        //  | |    | |
        //  | YX --|YXZ
        //  |/     |/
        //  Y ---> YZ 

        // public AreaVolumePointState requiredStateAtOrigin = AreaVolumePointState.Irrelevant;
        // public AreaVolumePointState requiredStateAtPosX = AreaVolumePointState.Irrelevant;
        // public AreaVolumePointState requiredStateAtPosZ = AreaVolumePointState.Irrelevant;
        // public AreaVolumePointState requiredStateAtPosXZ = AreaVolumePointState.Irrelevant;

        // public AreaVolumePointState requiredStateAtPosY = AreaVolumePointState.Irrelevant;
        // public AreaVolumePointState requiredStateAtPosYX = AreaVolumePointState.Irrelevant;
        // public AreaVolumePointState requiredStateAtPosYZ = AreaVolumePointState.Irrelevant;
        // public AreaVolumePointState requiredStateAtPosYXZ = AreaVolumePointState.Irrelevant;

        public GameObject prefab;
        public int prefabRotationID = 0;
        public bool prefabFlippedHorizontally = false;
        public bool prefabFlippedVertically = false;
    }
}