using Content.Code.Utility;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    public interface IOverworldFunctionLog
    {
        public string ToLog ();
    }
    
    public interface IFunctionLocalizedText
    {
        public string GetLocalizedText ();
    }
    
    [InterfaceInfo ("Used to execute arbitrary logic involving an overworld action. Signature `void Run (OverworldActionEntity source)`")]
    [TypeHinted]
    public interface IOverworldActionFunction
    {
        public void Run (OverworldActionEntity source);
    }
    
    public interface IOverworldActionParent
    {
        public DataContainerOverworldAction ParentAction { get; set; }
    }
    
    [InterfaceInfo ("Used to execute arbitrary logic involving a specific unit in combat. Signature `void Run (PersistentEntity unitPersistent)`")]
    [LabelWidth (160f)]
    [TypeHinted]
    public interface ICombatFunctionTargeted
    {
        public void Run (PersistentEntity unitPersistent);
    }
    
    [InterfaceInfo ("Used to execute arbitrary logic involving a specific unit in overworld. Signature `void Run (PersistentEntity unitPersistent)`")]
    [LabelWidth (160f)]
    [TypeHinted]
    public interface IOverworldUnitFunction
    {
        public void Run (PersistentEntity unitPersistent);
    }
    
    [TypeHinted]
    public interface ICombatFunctionSpatial
    {
        public void Run (Vector3 position, Vector3 direction);
    }
    
    [InterfaceInfo ("General purpose combat functions are typically about affecting global combat state. Check unit-targeted functions for state changes involving specific units. Signature `void Run ()`")]
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
    
    /*
    [InterfaceInfo ("Modifies generated scenario. Used for complex arbitrary changes that can't be accomplished with existing data blocks. Signature `void Modify (DataContainerScenario scenarioGenerated)`")]
    [TypeHinted]
    public interface IScenarioGenerationFunction
    {
        public void Modify (DataContainerScenario scenarioGenerated);
    }
    */

    [InterfaceInfo ("Used to execute arbitrary logic not requiring a target entity in the overworld context. Signature `void Run ()`")]
    [LabelWidth (160f)]
    [TypeHinted]
    public interface IOverworldFunction
    {
        public void Run ();
    }
    
    [InterfaceInfo ("Used to execute arbitrary logic based on a subsystem during general unit events such as crashes, damage etc. Signature `void OnPartEventGeneral (EquipmentEntity part, DataContainerSubsystem subsystemBlueprint, string context)`")]
    [LabelWidth (160f)]
    [TypeHinted]
    public interface ISubsystemFunctionGeneral
    {
        public void OnPartEventGeneral (EquipmentEntity part, DataContainerSubsystem subsystemBlueprint, string context);
    }
    
    [InterfaceInfo ("Used to execute arbitrary logic based on a subsystem during unit-to-unit interactions such as projectile hits. Signature `void OnPartEventTargeted (EquipmentEntity part, DataContainerSubsystem subsystemBlueprint, string context, Vector3 position, Vector3 direction, Vector3 targetPosition, CombatEntity targetUnitCombat, CombatEntity projectile)`")]
    [LabelWidth (160f)]
    [TypeHinted]
    public interface ISubsystemFunctionTargeted
    {
        public void OnPartEventTargeted (EquipmentEntity part, DataContainerSubsystem subsystemBlueprint, string context, Vector3 position, Vector3 direction, Vector3 targetPosition, CombatEntity targetUnitCombat, CombatEntity projectile);
    }
    
    [InterfaceInfo ("Used to execute arbitrary logic based on a subsystem when a certain action event happens to a parent part. Signature `void OnPartEventAction (EquipmentEntity part, DataContainerSubsystem subsystemBlueprint, string context, ActionEntity action)`")]
    [LabelWidth (160f)]
    [TypeHinted]
    public interface ISubsystemFunctionAction
    {
        public void OnPartEventAction (EquipmentEntity part, DataContainerSubsystem subsystemBlueprint, string context, ActionEntity action);
    }
    
    [InterfaceInfo ("Used to modify position and direction, used when creating AI attacks, offsetting special effects etc. Signature `void Run (Vector3 originPosition, Vector3 originDirection, ref Vector3 positionModified, ref Vector3 directionModified)`")]
    [LabelWidth (160f)]
    [TypeHinted]
    public interface ITargetModifierFunction
    {
        public void Run (Vector3 originPosition, Vector3 originDirection, ref Vector3 positionModified, ref Vector3 directionModified);
    }
    
    [InterfaceInfo ("Used to modify float values. Signature `void float Apply (float input)`")]
    [LabelWidth (160f)]
    [TypeHinted]
    public interface IFloatOperation
    {
        public float Apply (float input);
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
    
    [InterfaceInfo ("Used to validate whether a scenario state is true, enabling arbitrary scenario logic. Signature `bool IsValid (string stateKey, DataBlockScenarioState stateDefinition)`")]
    [LabelWidth (160f)]
    [TypeHinted]
    public interface ICombatStateValidationFunction
    {
        public bool IsValid (string stateKey, DataBlockScenarioState stateDefinition);
    }
    
    [InterfaceInfo ("Used to validate the state of overworld in general. Utilized in scenarios to check the combat site, player base or the world. Signature `bool IsValid ()`")]
    [LabelWidth (160f)]
    [TypeHinted]
    public interface IOverworldGlobalValidationFunction
    {
        public bool IsValid ();
    }
    
    [InterfaceInfo ("Used to validate the state of overworld in general or state of a specific entity in it. Utilized in scenarios to check the combat site, player base or the world. Signature `bool IsValid (PersistentEntity entityPersistent)`")]
    [LabelWidth (160f)]
    [TypeHinted]
    public interface IOverworldEntityValidationFunction
    {
        public bool IsValid (PersistentEntity entityPersistent);
    }
    
    [InterfaceInfo ("Used to modify an overworld entity, similar to unit targeted functions. Signature `void Run (OverworldEntity entityOverworld)`")]
    [LabelWidth (160f)]
    [TypeHinted]
    public interface IOverworldTargetedFunction
    {
        public void Run (OverworldEntity entityOverworld);
    }
    
    [InterfaceInfo ("Used to modify a persistent pilot entity, similar to unit targeted functions. Signature `void Run (PersistentEntity pilot)`")]
    [LabelWidth (160f)]
    [TypeHinted]
    public interface IPilotTargetedFunction
    {
        public void Run (PersistentEntity pilot, PersistentEntity entityPersistentLinked);
    }
    
    [InterfaceInfo ("Used to validate whether a pilot passes a condition in a variety of contexts. Signature `bool IsValid (PersistentEntity pilot)`")]
    [LabelWidth (160f)]
    [TypeHinted]
    public interface IPilotValidationFunction
    {
        public bool IsValid (PersistentEntity pilot, PersistentEntity entityPersistentLinked);
    }

    
    [InterfaceInfo ("Used to get a int value from an overworld unit: typically a memory, a stat or some constant. Signature `int Resolve (OverworldEntity entityOverworld)`")]
    [LabelWidth (160f)]
    [TypeHinted]
    public interface IOverworldIntValueFunction
    {
        public int Resolve (OverworldEntity entityOverworld);
    }
    
    [InterfaceInfo ("Used to validate positions in some specialized contexts in combat, such as filtering unit spawn positions. Signature `bool IsPositionValid (Vector3 position, string context)`")]
    [LabelWidth (160f)]
    [TypeHinted]
    public interface ICombatPositionValidationFunction
    {
        public bool IsPositionValid (CombatDescription cd, Vector3 position, string context);
    }
    
    [InterfaceInfo ("Used to validate whether a unit passes a condition in a variety of contexts in combat: scenario checks, subsystem functions and status effects. Signature `bool IsValid (PersistentEntity unitPersistent)`")]
    [LabelWidth (160f)]
    [TypeHinted]
    public interface ICombatUnitValidationFunction
    {
        public bool IsValid (PersistentEntity unitPersistent);
    }
    
    [InterfaceInfo ("Used to validate whether a unit passes a condition in a variety of contexts in overworld. Signature `bool IsValid (PersistentEntity unitPersistent)`")]
    [LabelWidth (160f)]
    [TypeHinted]
    public interface IOverworldUnitValidationFunction
    {
        public bool IsValid (PersistentEntity unitPersistent);
    }
    
    [InterfaceInfo ("Used to get a float value from a unit: typically a memory, a stat or some constant. Signature `float Resolve (PersistentEntity unitPersistent)`")]
    [LabelWidth (160f)]
    [TypeHinted]
    public interface ICombatUnitValueResolver
    {
        public float Resolve (PersistentEntity unitPersistent);
    }
    
    // Not type hinted as it's not used in configs
    // [InterfaceInfo ("Used to modify a scenario being generated. Only used via hardcoded lists. Signature `void Run (OverworldEntity siteOverworld, DataContainerScenario scenario, int seed)`")]
    public interface ICombatScenarioGenStep
    {
        void Run (OverworldEntity siteOverworld, DataContainerScenario scenario, int seed);
    }
}