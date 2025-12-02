using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [HideInInspector]
    public class DataBlockMechCamera
    {
        public enum MechCameraFollowMode
        {
            Locked,
            Orbital
        }

        public enum MechCameraTargetMode
        {
            Torso,
            WeaponLeft,
            WeaponRight
        }

        public MechCameraTargetMode target;
        public MechCameraFollowMode followMode;
        public float FOV;
        public Vector3 offset;
        public float movementDamping;
        public float aimDamping;

        [ShowIf ("@followMode == MechCameraFollowMode.Orbital")]
        public float orbitalAngle;
    }

    [Serializable]
    public class DataContainerSettingsCamera : DataContainerUnique
    {
        public float cameraTelemetrySmoothing = 7.5f;
        public float heightMultiplierForListenerTakeoff = 0.02f;

        public bool disableCombatCameraHeightSensor = false;

        [Header ("Actual Shadow Distances")]
        public int shadowDistanceOverworld = 250;
        public int shadowDistanceCombat = 250;
        public int shadowDistanceBase = 50;
        public SortedDictionary<string, int> shadowDistanceBasePerCamera = new SortedDictionary<string, int> ();

        [Header ("Frustum Shadow Distance Offsets (from actual shadows)")]
        public int shadowOffsetOverworld = -60;
        public int shadowOffsetCombat = -60;
        public int shadowOffsetBase = -10;

        [Header ("Mech-Mounted Camera Presets")]
        public SortedDictionary<string, DataBlockMechCamera> mechMountedCameraPresets = new SortedDictionary<string, DataBlockMechCamera> ();
    }
}
