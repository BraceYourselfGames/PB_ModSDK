using System;
using System.Collections;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using PhantomBrigade.Overworld.Systems;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
    public class OverworldEntityMoveRandom : IOverworldTargetedFunction
    {
        public float speed = 10f;
        
        [PropertyOrder (-1)]
        public Vector2 pointRange = new Vector2 (0, 0);	
		
        [PropertyOrder (-1)]
        public PointDistancePriority pointPriority = PointDistancePriority.None;

        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            if (entityOverworld == null || entityOverworld.isDestroyed || !entityOverworld.hasPosition)
                return;

            var provinceKey = DataHelperProvince.GetProvinceKeyActive ();
            var landscapeData = DataHelperProvince.GetLandscapeDataForProvince (provinceKey);
            if (landscapeData == null)
            {
                Debug.LogWarning ($"Can't force movement on entity {entityOverworld.ToLog ()}: province {provinceKey} can't find landscape data");
                return;
            }
            
            var origin = entityOverworld.position.v;
            var pointFound = landscapeData.TryGetPointInRange (origin, pointRange, out var point, distancePriority: pointPriority);
            if (!pointFound)
            {
                Debug.LogWarning ($"Can't force movement on entity {entityOverworld.ToLog ()}: province {provinceKey} could not provide any points in range {pointRange}");
                return;
            }

            int requestID = OverworldUtility.GetNextPathRequestID ();
            entityOverworld.ReplaceMovementSpeedLimit (speed);
            entityOverworld.isMovable = true;
            entityOverworld.ReplacePathfindingRequest (origin, point, requestID);

            #endif
        }
    }
    
    public class OverworldEntityMoveThroughSpawnGroup : IOverworldTargetedFunction
    {
        public bool rawPath = false;
        public int rawPathSubdivision = 2;
        
        public bool destroyOnEnd = false;
        
        public float speed = 10f;
        public string spawnGroupKey = "";
        
        [HideIf (nameof(rawPath))]
        public string indexMemoryKey = "";
        
        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            if (entityOverworld == null || entityOverworld.isDestroyed || !entityOverworld.hasPosition)
                return;
            
            var entityPersistent = IDUtility.GetLinkedPersistentEntity (entityOverworld);
            if (entityPersistent == null)
                return;

            var provinceKey = DataHelperProvince.GetProvinceKeyActive ();
            var landscapeData = DataHelperProvince.GetLandscapeDataForProvince (provinceKey);
            if (landscapeData == null)
            {
                Debug.LogWarning ($"Can't force movement on entity {entityOverworld.ToLog ()}: province {provinceKey} can't find landscape data");
                return;
            }
            
            var pointsInGroup = landscapeData.TryGetPointGroup (spawnGroupKey);
            if (pointsInGroup == null || pointsInGroup.Count < 2)
            {
                Debug.LogWarning ($"Can't force movement on entity {entityOverworld.ToLog ()}: province {provinceKey} has no point group {spawnGroupKey} with 2 or more points");
                return;
            }

            if (rawPath)
            {
                List<Vector3> points;
                if (rawPathSubdivision > 1)
                {
                    int segments = (pointsInGroup.Count - 1) * rawPathSubdivision;
                    var spline = PathUtilitySecondary.MakeCatmullRomSpline (pointsInGroup.ToArray (), segments, 0, false);
                    points = new List<Vector3> (spline);
                }
                else
                    points = new List<Vector3> (pointsInGroup);
                
                var lengthTotal = 0f;
                var lengthsPerSegment = new List<float> (points.Count - 1);

                for (int i = 0, limit = points.Count - 1; i < limit; ++i)
                {
                    var pointStart = points[i];
                    var pointEnd = points[i + 1];
                    var lengthOfSegment = Vector3.Distance (pointStart, pointEnd);
                    lengthTotal += lengthOfSegment;
                    lengthsPerSegment.Add (lengthOfSegment);
                }
                
                entityOverworld.ReplacePath (points, lengthsPerSegment, lengthTotal, 1);
                entityOverworld.ReplaceDestinationPosition (points[1]);
                entityOverworld.ReplaceDestinationNodeId (1);
                Debug.Log ($"Entity {entityOverworld.ToLog ()} gets raw path in province {provinceKey} using spawn group {spawnGroupKey} through full range of {points.Count} points");
                
                entityOverworld.ReplacePosition (points[0]);
                entityOverworld.ReplacePositionTarget (points[0]);

                // Hack to clear VFX
                if (entityOverworld.hasOverworldView)
                {
                    var view = entityOverworld.overworldView.view;
                    view.OnTeleport ();
                }
                
                WorldUIOverworld.OnPathReplaced (entityOverworld);
            }
            else
            {
                int targetIndex = entityPersistent.TryGetMemoryRounded (indexMemoryKey, out int v) ? v : 0;
                int pointCount = pointsInGroup.Count;
                
                if (targetIndex >= pointCount)
                {
                    if (destroyOnEnd)
                    {
                        Debug.LogWarning ($"Entity {entityOverworld.ToLog ()} can't proceed to next point in province {provinceKey} spawn group {spawnGroupKey} at index {targetIndex}, destroying...");
                        OverworldUtility.TryDestroySite (entityOverworld);
                    }
                    else
                    {
                        Debug.Log ($"Entity {entityOverworld.ToLog ()} finished path in province {provinceKey} spawn group {spawnGroupKey} at index {targetIndex}, looping...");
                        targetIndex = 0;
                        entityPersistent.SetMemoryFloat (indexMemoryKey, targetIndex);

                        var pointStart = pointsInGroup[targetIndex];
                        entityOverworld.ReplacePosition (pointStart);
                        entityOverworld.ReplacePositionTarget (pointStart);

                        // Hack to clear VFX
                        if (entityOverworld.hasOverworldView)
                        {
                            var view = entityOverworld.overworldView.view;
                            view.OnTeleport ();
                        }
                    }
                }

                var pointNext = pointsInGroup[targetIndex];
                var origin = entityOverworld.position.v;
                int requestID = OverworldUtility.GetNextPathRequestID ();
                entityOverworld.ReplaceMovementSpeedLimit (speed);
                entityOverworld.isMovable = true;
                entityOverworld.ReplacePathfindingRequest (origin, pointNext, requestID);

                Debug.Log ($"Entity {entityOverworld.ToLog ()} moves to next waypoint in province {provinceKey} spawn group {spawnGroupKey} at index {targetIndex}");
                entityPersistent.SetMemoryFloat (indexMemoryKey, targetIndex + 1);
            }

            #endif
        }
    }
    
    public class OverworldEntityMoveToBase : IOverworldTargetedFunction
    {
        public float speed = 10f;
        
        [PropertyOrder (-1)]
        public Vector2 pointRange = new Vector2 (0, 0);	

        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            if (entityOverworld == null || entityOverworld.isDestroyed || !entityOverworld.hasPosition)
                return;

            var baseOverworld = IDUtility.playerBaseOverworld;
            if (baseOverworld == null)
                return;

            var overworld = Contexts.sharedInstance.overworld;
            if (overworld.hasSimulationLockCountdown)
            {
                var provinceKey = DataHelperProvince.GetProvinceKeyActive ();
                var landscapeData = DataHelperProvince.GetLandscapeDataForProvince (provinceKey);
                if (landscapeData == null)
                {
                    Debug.LogWarning ($"Can't force movement on entity {entityOverworld.ToLog ()}: province {provinceKey} can't find landscape data");
                    return;
                }

                var origin = entityOverworld.position.v;
                var pointRangeFallback = new Vector2 (100, 500);
                var pointFound = landscapeData.TryGetPointInRange (origin, pointRangeFallback, out var point, distancePriority: PointDistancePriority.None);
                if (!pointFound)
                {
                    Debug.LogWarning ($"Can't force movement on entity {entityOverworld.ToLog ()}: province {provinceKey} could not provide any points in range {pointRange}");
                    return;
                }

                int requestID = OverworldUtility.GetNextPathRequestID ();
                entityOverworld.ReplaceMovementSpeedLimit (speed);
                entityOverworld.isMovable = true;
                entityOverworld.ReplacePathfindingRequest (origin, point, requestID);
            }
            else
            {
                var origin = entityOverworld.position.v;
                var point = baseOverworld.position.v;
                
                if (pointRange.y > pointRange.x)
                {
                    var offset = Random.Range (pointRange.x, pointRange.y);
                    var rotation = Quaternion.Euler (0f, 360f * Random.Range (0f, 1f), 0f);
                    point += (rotation * Vector3.forward) * offset;
                }

                int requestID = OverworldUtility.GetNextPathRequestID ();
                entityOverworld.ReplaceMovementSpeedLimit (speed);
                entityOverworld.isMovable = true;
                entityOverworld.ReplacePathfindingRequest (origin, point, requestID);
            }

            #endif
        }
    }
}