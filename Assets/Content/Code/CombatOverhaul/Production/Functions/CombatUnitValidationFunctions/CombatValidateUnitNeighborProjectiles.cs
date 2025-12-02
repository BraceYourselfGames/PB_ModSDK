using System;
using System.Collections.Generic;
using PhantomBrigade.Combat.Systems;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    public class DataBlockCombatProjectileCheck
    {
        public DataBlockSubcheckBool guided;
        public DataBlockOverworldEventSubcheckFloat speed;
        public DataBlockScenarioSubcheckUnitRelativeTransform relativeTransform;
    }
    
    public class DataBlockCombatProjectileFilter : DataBlockCombatProjectileCheck
    {
        [PropertyOrder (-2)]
        [SuffixLabel ("@GetSortSuffix ()")]
        public TargetSortMode candidateSort = TargetSortMode.None;
        
        [LabelText ("Max Selections")]
        [PropertyOrder (-2)]
        public int candidateLimit = -1;
        
        [ShowIf ("@candidateLimit > 1")]
        [LabelText ("Repeated Selection")]
        [PropertyOrder (-2)]
        public bool candidateRepeats = false;
        
        private static List<CombatEntity> entitiesFiltered = new List<CombatEntity> ();
        private static List<CombatEntity> entitiesSelected = new List<CombatEntity> ();
        private static List<int> entitiesSelectedIndexes = new List<int> ();
        
        private static Vector3 originPositionCached = default;
        private static Vector3 originDirectionCached = default;
        
        public List<CombatEntity> FilterExternalEntities
        (
            List<CombatEntity> entitiesExternal,
            Vector3 originPosition = default,
            Vector3 originDirection = default
        )
        {
            #if !PB_MODSDK

            if (originDirection.sqrMagnitude.RoughlyEqual (0f))
                originDirection = Vector3.forward;

            originPositionCached = originPosition;
            originDirectionCached = originDirection;
            
            entitiesFiltered.Clear ();
            entitiesFiltered.AddRange (entitiesExternal);
            entitiesSelected.Clear ();

            // Return empty collection if the filter is null or empty
            int filteredCount = entitiesExternal.Count;
            if (filteredCount == 0)
                return entitiesSelected;

            // Return all filtered units if there is no limit on returned number is under it
            if (candidateLimit <= 0 || candidateLimit >= filteredCount)
            {
                entitiesSelected.AddRange (entitiesFiltered);
                return entitiesSelected;
            }

            // Sort units
            if (candidateSort == TargetSortMode.None)
            {
                entitiesSelectedIndexes.Clear ();
                for (int i = 0; i < entitiesFiltered.Count; ++i)
                    entitiesSelectedIndexes.Add (i);
                
                for (int u = 0; u < candidateLimit; ++u)
                {
                    var indexRandom = entitiesSelectedIndexes.GetRandomEntry ();
                    entitiesSelected.Add (entitiesFiltered[indexRandom]);

                    if (!candidateRepeats)
                    {
                        entitiesSelectedIndexes.RemoveAt (indexRandom);
                        if (entitiesSelectedIndexes.Count == 0)
                            break;
                    }
                }
            }
            else
            {
                if (candidateSort == TargetSortMode.Distance)
                    entitiesFiltered.Sort (CompareUnitsByDistance);
                else if (candidateSort == TargetSortMode.DistanceInv)
                    entitiesFiltered.Sort (CompareUnitsByDistanceInv);
                else if (candidateSort == TargetSortMode.Dot)
                    entitiesFiltered.Sort (CompareUnitsByDot);
                else if (candidateSort == TargetSortMode.DotInv)
                    entitiesFiltered.Sort (CompareUnitsByDotInv);

                // var report = unitTargetsFiltered.ToStringFormatted (true, toStringOverride: (x) => $"- {x.ToLog ()}: {Vector3.Magnitude (x.position.v - originPositionCached)}");
                // Debug.Log ($"Sorted targets ({unitTargetsFiltered.Count}) by {sort}:\n{report}");
                
                for (int u = 0; u < candidateLimit; ++u)
                {
                    int index = candidateRepeats ? 0 : u;
                    entitiesSelected.Add (entitiesFiltered[index]);
                }
            }
            
            #endif

            return entitiesSelected;
        }
        
        #if !PB_MODSDK
        
        private int CompareUnitsByDistanceInv (CombatEntity a, CombatEntity b)
        {
            int comp = CompareUnitsByDistance (a, b);
            return -comp;
        }

        private int CompareUnitsByDistance (CombatEntity a, CombatEntity b)
        {
            float aDistanceSqr = 0f;
            float bDistanceSqr = 0f;

            if (a != null && a.hasPosition)
                aDistanceSqr = Vector3.SqrMagnitude (a.position.v - originPositionCached);
            
            if (b != null && b.hasPosition)
                bDistanceSqr = Vector3.SqrMagnitude (b.position.v - originPositionCached);
            
            return aDistanceSqr.CompareTo (bDistanceSqr);
        }
        
        private int CompareUnitsByDotInv (CombatEntity a, CombatEntity b)
        {
            int comp = CompareUnitsByDot (a, b);
            return -comp;
        }
        
        private int CompareUnitsByDot (CombatEntity a, CombatEntity b)
        {
            float aDot = -1f;
            float bDot = -1f;

            if (a != null && a.hasPosition)
                aDot = Vector3.Dot ( (a.position.v - originPositionCached).normalized, originDirectionCached);

            if (b != null && b.hasPosition)
                bDot = Vector3.Dot ( (b.position.v - originPositionCached).normalized, originDirectionCached);
            
            return bDot.CompareTo (aDot);
        }
        
        #endif
        
        #if UNITY_EDITOR

        private string GetSortSuffix ()
        {
            if (candidateSort == TargetSortMode.None)
                return "Unordered proj.";
            if (candidateSort == TargetSortMode.Dot)
                return "Closest proj. to forward dir. first";
            if (candidateSort == TargetSortMode.DotInv)
                return "Farthest proj. from forward dir. first";
            if (candidateSort == TargetSortMode.Distance) 
                return "Closest proj. first";
            if (candidateSort == TargetSortMode.DistanceInv)
                return "Farthest proj. first";
            return "?";
        }
        
        #endif
    }
    
    public class DataBlockSubcheckBoolProjectiles : DataBlockSubcheckBool
    {
        protected override string GetLabel () => present ? "Projectiles should be present" : "Projectiles should be absent";
    }

    public class DataBlockProjectileEffect
    {
        public bool destroy;
        
        [DropdownReference (true)]
        public DataBlockFloat scrambling;
        
        [DropdownReference (true)]
        [ValueDropdown("@AudioEvents.GetKeys ()")]
        public string fxAudio;
        
        [DropdownReference (true)]
        [LabelText ("VFX Origin")]
        [ValueDropdown ("@DataMultiLinkerPartSocket.data.Keys")]
        public string fxTransformSocket;
        
        [DropdownReference]
        [LabelText ("VFX Modifiers")]
        public List<ITargetModifierFunction> fxTransformModifiers;
        
        [DropdownReference]
        [LabelText ("VFX")]
        public List<DataBlockAssetInterpolated> fx;
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockProjectileEffect () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
    
    [Serializable]
    public class CombatValidateUnitNeighborProjectiles : DataBlockSubcheckBoolProjectiles, ICombatUnitValidationFunction
    {
        public float radius = 10f;
        public Vector3 offset;

        public UnitFactionFilter factionFilter = UnitFactionFilter.Any;
        
        [DropdownReference (true)]
        public DataBlockCombatProjectileFilter filter = new DataBlockCombatProjectileFilter ();

        [DropdownReference (true)]
        public DataBlockProjectileEffect effect;
        
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            bool returnValueIfPresent = present;
            
            var unitCombatSource = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombatSource == null || !unitCombatSource.isUnitTag || !unitCombatSource.hasPosition)
                return false;

            Transform transformSource = null;
            var positionSource = unitCombatSource.position.v;
            var directionSource = Vector3.forward;
            
            if (unitCombatSource.hasLocalCenterPoint)
                positionSource += unitCombatSource.GetCenterOffset ();
            
            if (unitCombatSource.hasRotation)
            {
                positionSource += unitCombatSource.rotation.q * offset;
                directionSource = unitCombatSource.rotation.q * Vector3.forward;
            }

            var projectilesSorted = OverlapUtility.GetSortedProjectileOverlaps (positionSource, directionSource, radius, factionFilter, filter, unitCombatSource);
            if (projectilesSorted == null || projectilesSorted.Count == 0)
                return false;

            if (filter != null)
            {
                var unitsFiltered = filter.FilterExternalEntities (projectilesSorted, positionSource, directionSource);
                projectilesSorted = unitsFiltered;
            }

            if (effect != null)
            {
                foreach (var projectile in projectilesSorted)
                {
                    if (effect.destroy)
                        projectile.TriggerProjectile (ProjectileTriggerSource.Intercept);

                    if (projectile.hasProjectileTargetEntity && projectile.hasProjectileGuidanceProgress && effect.scrambling != null && effect.scrambling.f > 0f)
                    {
                        var combat = Contexts.sharedInstance.combat;
                        var time = combat.hasSimulationTime ? combat.simulationTime.f : 0f;
                        var dir = Vector3.forward;
                        if (projectile.hasFacing)
                            dir = projectile.facing.v;
                        else if (projectile.hasRotation)
                            dir = projectile.rotation.q * Vector3.forward;
                        
                        projectile.ReplaceProjectileGuidanceSuspended (time, effect.scrambling.f, -dir);

                        var posOffset = projectile.position.v + dir;
                        if (projectile.hasVelocity)
                            posOffset += projectile.velocity.v * 0.05f;

                        AssetPoolUtility.ActivateInstance ("fx_projectile_guidance_scramble", posOffset, dir);
                    }

                    if (effect.fx != null)
                    {
                        if (!string.IsNullOrEmpty (effect.fxTransformSocket))
                        {
                            var part = EquipmentUtility.GetPartInUnit (unitPersistent, effect.fxTransformSocket);
                            if (part != null)
                            {
                                bool transformFound = part.TryGetPartTransform (false, false, out var transformFx);
                                if (transformFound && transformFx != null)
                                    positionSource = transformFx.position;
                            }
                        }

                        var positionProjectile = projectile.position.v;
                        var directionToProjectile = (positionProjectile - positionSource).normalized;
                        
                        if (effect.fxTransformModifiers != null)
                        {
                            ScenarioUtility.GetTargetModified 
                            (
                                positionSource, 
                                directionToProjectile,
                                effect.fxTransformModifiers, 
                                out var positionModified, 
                                out var directionModified
                            );
                            
                            Debug.DrawLine (positionSource, positionSource + Vector3.up, Color.green, 1f);
                            Debug.DrawLine (positionModified, positionModified + Vector3.up, Color.green, 1f);

                            positionSource = positionModified;
                            directionToProjectile = (positionProjectile - positionSource).normalized;
                        }
                        
                        foreach (var fx in effect.fx)
                        {
                            if (fx == null)
                                continue;
                    
                            fx.GetFXTransform (positionSource, positionProjectile, out var fxPos, out var fxDir, out var fxScale);
                            AssetPoolUtility.ActivateInstance (fx.key, fxPos, fxDir, fxScale, delay: fx.delay);
                        }

                        if (!string.IsNullOrEmpty (effect.fxAudio))
                            AudioUtility.CreateAudioEvent (effect.fxAudio, positionSource);
                    }
                }
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
        
        public CombatValidateUnitNeighborProjectiles () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
}