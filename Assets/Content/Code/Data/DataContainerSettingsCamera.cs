using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhantomBrigade.Data
{
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
    }
}
