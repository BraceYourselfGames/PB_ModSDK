using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatPositionValidationByStateDistance : ICombatPositionValidationFunction
    {
        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerScenario\", \"GetStateKeys\")")]
        public string stateKey;

        public DataBlockOverworldEventSubcheckFloat checkDistance = new DataBlockOverworldEventSubcheckFloat ();
        
        public bool IsPositionValid (CombatDescription cd, Vector3 position, string context)
        {
            #if !PB_MODSDK

            bool log = DataShortcuts.sim.logScenarioGeneration;
            
            if (checkDistance == null)
            {
                if (log)
                    Debug.LogWarning ($"{context} | Can't validate position from state [{stateKey}]: no distance check provided");
                return true;
            }
            
            // This caused PB-15450 [Combat] Mission generation - Mobile base can spawn in incorrect position during base defense missions
            // This function and other operatons like it run during scenario generation for an entire batch of POIs where "combat site" doesn't exist
            // var targetOverworld = ScenarioUtility.GetCombatSite ();
            // var cd = ScenarioUtilityGeneration.GetCombatDescriptionFromEntity (targetOverworld);
            
            // Instead, CD is now passed in explicitly
            var stateLocations = cd?.stateLocations;
            
            if (string.IsNullOrEmpty (stateKey))
            {
                if (log)
                    Debug.LogWarning ($"{context} | Can't validate position from state [{stateKey}]: null or empty key");
                return true;
            }
            
            if (stateLocations == null)
            {
                if (log)
                    Debug.LogWarning ($"{context} | Can't validate position from state [{stateKey}]: state location collection is null");
                return true;
            }
            
            if (!stateLocations.TryGetValue (stateKey, out var locationKey))
            {
                if (log)
                    Debug.LogWarning ($"{context} | Can't validate position from state [{stateKey}]: state location collection has no such entry | Existing entries: {stateLocations.ToStringFormattedKeyValuePairs ()}");
                return true;
            }

            var area = DataMultiLinkerCombatArea.GetEntry (cd.areaKey, false);
            if (area == null || area.locationsProc == null)
            {
                if (log)
                    Debug.LogWarning ($"{context} | Can't validate position from state {stateKey} location {locationKey}: no current area");
                return true;
            }

            var locationReferenced = area.GetLocation (locationKey);
            if (locationReferenced == null)
            {
                if (log)
                    Debug.LogWarning ($"{context} | Can't validate position from state {stateKey} location {locationKey}: no location data found");
                return true;
            }
            
            var distance = (position - locationReferenced.point).magnitude;
            return checkDistance.IsPassed (true, distance);
            
            #else
            return false;
            #endif
        }
    }
}