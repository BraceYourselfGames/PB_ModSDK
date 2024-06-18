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
        
        public bool IsPositionValid (Vector3 position, string context)
        {
            #if !PB_MODSDK
            
            if (checkDistance == null)
            {
                Debug.LogWarning ($"{context} | Can't validate position from state [{stateKey}]: no distance check provided");
                return true;
            }
            
            var combat = Contexts.sharedInstance.combat;
            if (!combat.hasScenarioStateLocations || string.IsNullOrEmpty (stateKey) || !combat.scenarioStateLocations.s.TryGetValue (stateKey, out var locationKey))
            {
                Debug.LogWarning ($"{context} | Can't validate position from state [{stateKey}]: no registered location found");
                return true;
            }
            
            var area = ScenarioUtility.GetCurrentArea ();
            if (area == null || area.locationsProc == null)
            {
                Debug.LogWarning ($"{context} | Can't validate position from state {stateKey} location {locationKey}: no current area");
                return true;
            }

            var locationReferenced = area.GetLocation (locationKey);
            if (locationReferenced == null)
            {
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