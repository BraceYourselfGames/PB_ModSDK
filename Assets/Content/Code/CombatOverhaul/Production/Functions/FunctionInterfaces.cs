using Content.Code.Utility;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [TypeHinted]
    public interface IOverworldEventFunction
    {
        public void Run (OverworldEntity target, DataContainerOverworldEvent eventData);
    }

    public interface IOverworldEventFunctionEarly
    {
        public bool Early ();
    }

    public interface IOverworldEventParent
    {
        public DataContainerOverworldEvent ParentEvent { get; set; }
    }
    
    public interface IOverworldFunctionLog
    {
        public string ToLog ();
    }
    
    [TypeHinted]
    public interface IOverworldActionFunction
    {
        public void Run (OverworldActionEntity source);
    }
    
    public interface IOverworldActionParent
    {
        public DataContainerOverworldAction ParentAction { get; set; }
    }
    
    [LabelWidth (160f)]
    [TypeHinted]
    public interface ICombatFunctionTargeted
    {
        public void Run (PersistentEntity unitPersistent);
    }
    
    [TypeHinted]
    public interface ICombatFunctionSpatial
    {
        public void Run (Vector3 position, Vector3 direction);
    }
    
    [LabelWidth (160f)]
    [TypeHinted]
    public interface ICombatFunction
    {
        public void Run ();
    }
    
    public interface ICombatFunctionDelayed
    {
        public bool IsDelayed ();
    }

    [LabelWidth (160f)]
    [TypeHinted]
    public interface IOverworldFunction
    {
        public void Run ();
    }
    
    [LabelWidth (160f)]
    [TypeHinted]
    public interface ISubsystemFunctionGeneral
    {
        public void OnPartEventGeneral (EquipmentEntity part, DataContainerSubsystem subsystemBlueprint, string context);
    }
    
    [LabelWidth (160f)]
    [TypeHinted]
    public interface ISubsystemFunctionTargeted
    {
        public void OnPartEventTargeted (EquipmentEntity part, DataContainerSubsystem subsystemBlueprint, string context, Vector3 position, Vector3 direction, Vector3 targetPosition, CombatEntity targetUnitCombat, CombatEntity projectile);
    }
    
    [LabelWidth (160f)]
    [TypeHinted]
    public interface ISubsystemFunctionAction
    {
        public void OnPartEventAction (EquipmentEntity part, DataContainerSubsystem subsystemBlueprint, string context, ActionEntity action);
    }
    
    [LabelWidth (160f)]
    [TypeHinted]
    public interface ITargetModifierFunction
    {
        public void Run (Vector3 originPosition, Vector3 originDirection, ref Vector3 positionModified, ref Vector3 directionModified);
    }
    
    [LabelWidth (160f)]
    [TypeHinted]
    public interface ICombatActionExecutionFunction
    {
        public void Run (CombatEntity unitCombat, ActionEntity action);
    }
    
    [LabelWidth (160f)]
    [TypeHinted]
    public interface ICombatActionValidationFunction
    {
        public bool IsValid (CombatEntity unitCombat);
    }
    
    [LabelWidth (160f)]
    [TypeHinted]
    public interface ICombatStateValidationFunction
    {
        public bool IsValid (string stateKey, DataBlockScenarioState stateDefinition);
    }
    
    [LabelWidth (160f)]
    [TypeHinted]
    public interface IOverworldValidationFunction
    {
        public bool IsValid (PersistentEntity entityPersistent);
    }
    
    [LabelWidth (160f)]
    [TypeHinted]
    public interface ICombatPositionValidationFunction
    {
        public bool IsPositionValid (Vector3 position, string context);
    }
    
    [LabelWidth (160f)]
    [TypeHinted]
    public interface ICombatUnitValidationFunction
    {
        public bool IsValid (PersistentEntity unitPersistent);
    }
    
    [LabelWidth (160f)]
    [TypeHinted]
    public interface ICombatUnitValueResolver
    {
        public float Resolve (PersistentEntity unitPersistent);
    }
}

// Non-type hinted, non serialized
public interface ICombatScenarioGenStep
{
    void Run (DataContainerScenario scenario, int seed);
}