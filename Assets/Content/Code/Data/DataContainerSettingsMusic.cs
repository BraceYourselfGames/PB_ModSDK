using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [Serializable]
    public class DataContainerSettingsMusic : DataContainerUnique
    {
		public struct MusicReactThreshold
		{
			public float powerBalanceThreshold;
            public int progress;
            public string mood;

            public bool IsValid => mood.Length > 0;
		}

        [Header ("Reactive Music")]
        
        public int startingMusicProgress = 0;
        public int maxStep = 1;
        public int minProgressWhileTeamIntact = -1;
        public int teamIntactThreshold = 2;
        public List<MusicReactThreshold> reactiveMusicThresholds = new List<MusicReactThreshold>();
    }
}