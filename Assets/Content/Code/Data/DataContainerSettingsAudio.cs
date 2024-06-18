using System;

namespace PhantomBrigade.Data
{
    [Serializable]
    public class DataContainerSettingsAudio : DataContainerUnique
    {
        public bool replayAudioUsed = false;
        public bool cameraMovementSyncs = true;
		public float mechMassMin = 50f;
        public float mechMassMax = 100f;
        public float lastPlayDistanceThreshold = 1f;
        public float lastPlayUnscaledTimeThreshold = 0.1f;
    }
}