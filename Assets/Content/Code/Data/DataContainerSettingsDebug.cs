using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
    [Serializable] 
    public class DataContainerSettingsDebug : DataContainerUnique
    {
	    [Header ("Testing and Developer Access")]

        [OnValueChanged ("ApplyDeveloperMode")]
        public bool developerMode = false;

        public bool log = true;

        public bool combatScenarioLoaded = false;
        
        [ShowIf ("combatScenarioLoaded")]
        [ValueDropdown ("@DataMultiLinkerScenario.data.Keys")]
        public string combatScenarioLoadedKey = "intro";

        [ShowIf ("combatScenarioLoaded")]
        public int combatScenarioLoadedSeed = 0;
        
        public bool combatScenarioInfo = false;
        public bool combatPropsSkipped = false;
        public bool combatColorSkipped = false;

        public bool allowCombatSaves = false;
        public bool allowCombatDifficulty = false;
        public bool showSplashScreens = true;

        public bool enableAnalytics = true;
        public bool enableAnalyticsInDevMode = false;

        public bool debugAudioEvents = false;
        public bool debugOptions = false;
        public bool debugWeapons = false;

        public bool forceFirstTimeBoot = false;
        public bool forceQuickLoad = false;
        public bool forceWorldGeneration = false;
        public bool forceWorldPreservation = false;
        
        public bool textWarningWhenEmpty = false;
        public bool textFromLibraryWhenEmpty = true;
        public bool pseudolocRestrictedToFinalText = false;

        public bool enableAchievement = true;
        public bool enableInputRemapping = true;
        public bool enableVersionDisplay = false;
        public bool enableVersionAnnouncementDynamic = false;
        public bool enableEquipmentDetailsInCombat = false;
        public bool forceDemoMode = false;

        public override void OnAfterDeserialization ()
        {
            base.OnAfterDeserialization ();
            
            ApplyDeveloperMode ();
        }

        private void ApplyDeveloperMode ()
        {

        }
    }
}

