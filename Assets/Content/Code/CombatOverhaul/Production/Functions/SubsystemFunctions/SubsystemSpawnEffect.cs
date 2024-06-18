namespace PhantomBrigade.Functions
{
    // Commented out for now due to need to find an alternative way to pass in hardpoints
    // Unsure this is even a necessary function when you can trigger effects on owner and spawn effects that way
    /*
    [Serializable]
    public class SubsystemSpawnEffect : ISubsystemFunctionGeneral, ISubsystemFunctionTargeted, ISubsystemFunctionAction
    {
        public DataBlockAsset asset = new DataBlockAsset ();

        public void OnPartEventGeneral (EquipmentEntity part, DataContainerSubsystem subsystemBlueprint, string context)
        {
            if (asset == null)
                return;
            
            if (!subsystem.TryGetSubsystemTransform (false, out var t))
                return;

            var position = t.position;
            var direction = t.forward;
            
            AssetPoolUtility.ActivateInstance (asset.key, position, direction, asset.scale);
            Debug.Log ($"{context} | Spawning effect {asset.key} at {position} (general)");
        }

        public void OnPartEventTargeted (EquipmentEntity part, DataContainerSubsystem subsystemBlueprint, string context, Vector3 position, Vector3 direction, Vector3 targetPosition, CombatEntity targetUnitCombat, CombatEntity projectile)
        {
            if (asset == null)
                return;
            
            AssetPoolUtility.ActivateInstance (asset.key, targetPosition, direction, asset.scale);
            Debug.Log ($"{context} | Spawning effect {asset.key} at {position} (targeted)");
        }
        
        public void OnPartEventAction (EquipmentEntity part, DataContainerSubsystem subsystemBlueprint, string context, ActionEntity action)
        {
            if (asset == null)
                return;
            
            if (!subsystem.TryGetSubsystemTransform (false, out var t))
                return;

            var position = t.position;
            var direction = t.forward;
            
            AssetPoolUtility.ActivateInstance (asset.key, position, direction, asset.scale);
            Debug.Log ($"{context} | Spawning effect {asset.key} at {position} (action)");
        }
    }
    */
}