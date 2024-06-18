using System;
using System.Collections.Generic;

namespace PhantomBrigade.Data
{
    [Serializable]
    public class DataContainerSettingsEscalation : DataContainerUnique
    {
        public float escalationRate = 1.0f;
        public float deescalationRate = 1.0f;
        
        public float playerWarScoreDecayDelay = 1.0f;
        public float playerWarScoreDecayRate = 1.0f;
        
        public float playerWarScoreRecoveryRate = 1.0f;
        public float playerMaxWarScore = 100.0f;
        public float enemyMaxWarScore = 100.0f;
        public float enemyWarScoreDamageFromObjectives = 1f;
        public float enemyWarScoreDamageFromOthers = 0.25f;

        public int minHope = -5;
        public int maxHope = 5;
        public float hopeWarScoreScale = 0.15f;
        
        public float threatIncreasePerLevel = 35.0f;
        public float threatIncreasePerReputationWithHomeGuard = 30f;
        public int maxReputationWithHomeGuard = 5;

        public bool escalationLevelsOverTime = false;
        public List<float> escalationThresholds = new List<float> () { 50.0f, 100.0f, 150.0f, 200.0f, 250.0f, 300.0f };
        public int warThreshold = 2;

        public float battleSiteDecayMultiplier = 1f;
        public float battleSiteStartDelay = 0f;
        public float battleSiteUpdateInterval = 20.0f;
        public float battleSiteSpawnRate = 0.4f;
        public int battleSiteQuota = 2;
        
        public float warObjectiveStartDelay = 10f;
        public float warObjectiveInterval = 20.0f;
        public float warObjectiveSpawnRate = 0.4f;
        public int warObjectiveQuota = 3;
        public Dictionary<string, float> warObjectiveTagsWeighted = new Dictionary<string, float>
        {
            { "structure", 1 },
            { "squad", 1 },
            { "convoy", 1 }
        };

        public bool warBaseSpawns = false;
        public float warBaseSpawnInterval = 30f;
        public int warBaseSitesMin = 2;
        public bool warBaseObjectivesForced = false;
        
        public float recaptureInterval = 20.0f;
        public float recaptureRate = 0.4f;
        public float recaptureDuration = 100.0f;
        
        public bool unitGroupLimitsActive = false;
        public bool unitGroupCloningUsed = true;
        public bool unitScalingThreatBased = true;
        public bool unitScalingLevelBased = false;
        
        public int embeddedUnitGroupLimitTotal = 5;
        public int embeddedUnitGroupLimitAdded = 1;
        
        public int reinforcementUnitGroupLimitTotal = 5;
        public int reinforcementUnitGroupLimitAdded = 1;
        
        public int friendlyUnitGroupLimitTotal = 5;
        public int friendlyUnitGroupLimitAdded = 1;

        public bool contestWithoutEscalation = true;
        public bool contestWithoutBasePresence = false;
        public bool contestAutomatically = true;
        
        public bool restrictConvoysAtWar = false;
    }
}
