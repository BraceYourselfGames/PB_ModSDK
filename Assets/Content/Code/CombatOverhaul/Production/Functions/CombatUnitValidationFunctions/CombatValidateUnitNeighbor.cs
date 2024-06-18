using System;
using System.Collections.Generic;
using System.Linq;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    public class DataBlockBlackboardLocalExport
    {
        public string key;
        public bool indexed;
    }

    public class DataBlockSubcheckBoolUnits : DataBlockSubcheckBool
    {
        protected override string GetLabel () => present ? "Units should be present" : "Units should be absent";
    }
    
    [Serializable]
    public class CombatValidateUnitNeighbor : DataBlockSubcheckBoolUnits, ICombatUnitValidationFunction
    {
        public float radius = 10f;
        public Vector3 offset;
        
        public UnitFactionFilter faction = UnitFactionFilter.Any;
        
        [DropdownReference (true)]
        public DataBlockScenarioSubcheckUnitFilter filter = new DataBlockScenarioSubcheckUnitFilter ();

        [DropdownReference (true)]
        public DataBlockBlackboardLocalExport blackboardExport;
        
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            bool returnValueIfPresent = present;
            
            var unitCombatSource = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombatSource == null || !unitCombatSource.isUnitTag || !unitCombatSource.hasPosition)
                return false;

            var positionSource = unitCombatSource.position.v;
            var directionSource = Vector3.forward;
            
            if (unitCombatSource.hasLocalCenterPoint)
                positionSource += unitCombatSource.GetCenterOffset ();
            
            if (unitCombatSource.hasRotation)
            {
                positionSource += unitCombatSource.rotation.q * offset;
                directionSource = unitCombatSource.rotation.q * Vector3.forward;
            }

            var units = OverlapUtility.GetSortedUnitOverlaps (positionSource, radius, filter, unitCombatSource, faction);
            if (units == null || units.Count == 0)
                return false;

            if (filter != null)
            {
                var unitsFiltered = filter.FilterExternalUnits (units, filter.sort, filter.unitLimit, filter.unitRepeats, positionSource, directionSource);
                units = unitsFiltered;
            }
            
            if (blackboardExport != null && !string.IsNullOrEmpty (blackboardExport.key))
            {
                var blackboardKey = blackboardExport.key;
                if (blackboardExport.indexed)
                    ScenarioUtility.SaveEntitiesToUnitBlackboard (unitCombatSource, blackboardKey, units);
                else
                    ScenarioUtility.SaveEntityToUnitBlackboard (unitCombatSource, blackboardKey, units.FirstOrDefault ());
            }

            return returnValueIfPresent;

            #else
            return false;
            #endif
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public CombatValidateUnitNeighbor () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
    
    [Serializable]
    public class CombatValidateUnitNeighborWithEffect : ICombatUnitValidationFunction
    {
        public float radius = 10f;
        public Vector3 offset;
        
        public UnitFactionFilter faction = UnitFactionFilter.Any;
        
        [BoxGroup ("F", false)]
        public DataBlockScenarioSubcheckUnitFilter filter = new DataBlockScenarioSubcheckUnitFilter ();

        public List<DataBlockAssetInterpolated> fxOnHit;

        public List<ICombatFunctionTargeted> functionsOnHit = new List<ICombatFunctionTargeted> ();
        
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            var unitCombatSource = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombatSource == null || !unitCombatSource.isUnitTag || !unitCombatSource.hasPosition)
                return false;

            var positionSource = unitCombatSource.position.v;
            var directionSource = Vector3.forward;
            
            if (unitCombatSource.hasLocalCenterPoint)
                positionSource += unitCombatSource.GetCenterOffset ();
            
            if (unitCombatSource.hasRotation)
            {
                positionSource += unitCombatSource.rotation.q * offset;
                directionSource = unitCombatSource.rotation.q * Vector3.forward;
            }

            var units = OverlapUtility.GetSortedUnitOverlaps (positionSource, radius, filter, unitCombatSource, faction);
            if (units == null || units.Count == 0)
                return false;

            if (filter != null)
            {
                var unitsFiltered = filter.FilterExternalUnits (units, filter.sort, filter.unitLimit, filter.unitRepeats, positionSource, directionSource);
                units = unitsFiltered;
            }

            if (functionsOnHit != null)
            {
                foreach (var unitCombatHit in units)
                {
                    var unitPersistentHit = IDUtility.GetLinkedPersistentEntity (unitCombatHit);
                    if (unitPersistentHit == null)
                        continue;

                    foreach (var function in functionsOnHit)
                    {
                        if (function != null)
                            function.Run (unitPersistentHit);
                    }

                    if (fxOnHit != null)
                    {
                        var positionHit = unitCombatHit.GetCenterPoint ();
                        var directionHit = (positionHit - positionSource).normalized;
                        
                        foreach (var fx in fxOnHit)
                        {
                            if (fx == null)
                                continue;

                            fx.GetFXTransform (positionSource, positionHit, out var fxPos, out var fxDir, out var fxScale);
                            AssetPoolUtility.ActivateInstance (fx.key, fxPos, fxDir, fxScale, delay: fx.delay);
                        }
                    }
                }
            }

            return true;

            #else
            return false;
            #endif
        }
    }
}