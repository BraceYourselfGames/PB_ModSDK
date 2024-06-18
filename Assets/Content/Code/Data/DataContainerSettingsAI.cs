using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [Serializable]
    public class DataContainerSettingsAI : DataContainerUnique
    {
        [Header ("Global AI controls")] 
        public bool allowAIControlByPlayer = false;
        public bool aiReplan = true;
        
        [Header("New AI")]
        public float lowHealthSurrenderThreshhold = 0.2f;
		public float strafeAngle = 60.0f;
		public float adjacentDistance = 15.0f;
        public float weaponRangeFalloff = 40f;
        public int aiStrafeDuration = 3;

		[Header("Debugging")]
        public bool debugAIBinding = true;

        [Header ("Keys")]
        
        // Remove this once DataMultiLinker for behaviors exists and modify DataBlockScenarioUnitResolver.GetAIBehaviorKeys to use it
        public HashSet<string> unitBehaviors = new HashSet<string> ();
    }
}