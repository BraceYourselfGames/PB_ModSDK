using UnityEngine;

namespace PhantomBrigade.Data
{
    public class ScenarioAddUnitsFromSite : ICombatScenarioGenStep
    {
        public void Run (DataContainerScenario scenario, int seed)
        {
            #if !PB_MODSDK

            var targetPersistent = ScenarioUtility.GetCombatSite ();
            var targetOverworld = IDUtility.GetLinkedOverworldEntity (targetPersistent);
            if (targetOverworld == null || !targetOverworld.hasDataLinkOverworldEntityBlueprint)
            {
                Debug.LogWarning ($"Skipping adding units from site: {targetOverworld.ToLog ()} has no blueprint");
                return;
            }

            var targetBlueprint = targetOverworld.dataLinkOverworldEntityBlueprint.data;
            var blocks = targetBlueprint.scenarioUnitsProcessed;
            if (blocks == null || blocks.Count == 0)
            {
                Debug.LogWarning ($"Skipping adding units from site: {targetOverworld.ToLog ()} has no embedded units");
                return;
            }

            var context = $"site blueprint {targetBlueprint.key}";
            ScenarioUtilityGeneration.InsertUnitBlocks (scenario, blocks, context);
            
            #endif
        }
    }
}