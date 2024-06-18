using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Area;

public enum AreaLightContributionType
{
    Prop,
    Point
}

[Serializable]
public struct AreaLightContribution
{
    public Vector3 pos;
    public Color color;
    public float intensity;
    public float distance;
    public float multiplier;
    public AreaLightContributionType sourceType;
    public int sourceIndex;
}

[Serializable]
public class AreaLightNode
{
    [PropertyOrder (-1)]
    public Vector3 pos;
    
    [PropertyOrder (1), ListDrawerSettings (DefaultExpandedState = false)]
    public List<AreaLightContribution> contributions;
}

[Serializable]
public class AreaLightSource : AreaLightNode
{
    public Vector3 posAverage;
    
    public Color color;
    public float intensity;
    public bool lightNeeded;
    public bool lightAdded;
    public Light light;
}

[Serializable]
public class AreaTilesetLight
{
    public float offset = 1f;
    public float intensity = 1f;
}


public class AreaLightHelper : MonoBehaviour
{
    public AreaManager am;
    public int elevationSteps = 2;
    public float falloffPower = 1;
    public float falloffMultiplier = 1;
    public float intensityMinimum = 1f;
    public float intensityMaximum = 3f;
    public float lightRadius = 12;
    public float lightSeparation = 6;
    public bool lightPositionAveraging = true;
    
    public Color pointEmissionColor = Color.yellow;
    public float pointSourceRange = 12f;
    public float pointSourceIntensity = 1f;
    
    [ShowInInspector, ListDrawerSettings (ShowPaging = true, DefaultExpandedState = false)]
    private List<AreaLightNode> ambientLightNodes = new List<AreaLightNode> ();
    
    [ShowInInspector, ListDrawerSettings (ShowPaging = true, DefaultExpandedState = false)]
    private List<AreaLightSource> ambientLightNodesFiltered = new List<AreaLightSource> ();
    
    [ShowInInspector, ListDrawerSettings (ShowPaging = true, DefaultExpandedState = false)]
    private List<AreaLightSource> ambientLightNodesFinalized = new List<AreaLightSource> ();
    
    [ShowInInspector, ListDrawerSettings (ShowPaging = true, DefaultExpandedState = false)]
    private List<AreaLightSource> ambientLightNodesUpdated = new List<AreaLightSource> ();

    private Dictionary<int, AreaLightSource> lookupNodesFromPoints = new Dictionary<int, AreaLightSource> ();
    private Dictionary<int, AreaLightSource> lookupNodesFromProps = new Dictionary<int, AreaLightSource> ();

    private static bool featureEnabled = false;

    public void Setup ()
    {
        
    }
    
    [Button (ButtonSizes.Large), PropertyOrder (-1)]
    public void Rebuild ()
    {
        if (!featureEnabled)
            return;
        
        ambientLightNodes.Clear ();

        lookupNodesFromPoints.Clear ();
        lookupNodesFromProps.Clear ();
        
        UtilityGameObjects.ClearChildren (gameObject);
        
        if (am == null || am.points.Count < 8)
        {
            Debug.LogWarning ($"Failed to load background for area due to no manager reference or points");
            return;
        }

        elevationSteps = Mathf.Clamp (elevationSteps, 1, 5);
        
        var points = am.points;
        for (int i = 0, limit = points.Count; i < limit; ++i)
        {
            var p0 = points[i];
            if (p0 == null || p0.spotConfiguration != AreaNavUtility.configFloor)
                continue;

            bool valid = true;
            var pe = p0;
            for (int e = 0; e < elevationSteps; ++e)
            {
                pe = pe.pointsWithSurroundingSpots[3];
                if (pe == null || pe.spotConfiguration != AreaNavUtility.configEmpty)
                {
                    valid = false;
                    break;
                }
            }
            
            if (!valid)
                continue;
            
            ambientLightNodes.Add (new AreaLightNode
            {
                pos = pe.instancePosition
            });
        }

        var nodeCount = ambientLightNodes.Count;
        
        var props = am.placementsProps;
        for (int i = 0, limit = props.Count; i < limit; ++i)
        {
            var prop = props[i];
            if (prop == null || prop.prototype == null || prop.prototype.prefab == null)
                continue;

            var prefab = prop.prototype.prefab;
            if (prefab.activeLights == null || prefab.activeLights.Count == 0)
                continue;
            
            var propPosition = (Vector3)prop.state.cachedRootPosition;
            var propRotation = (Quaternion)prop.state.cachedRootRotation;
            
            for (int l = 0, lLimit = prefab.activeLights.Count; l < lLimit; ++l)
            {
                var source = prefab.activeLights[l];
                if (source == null || source.intensity < 0.1f || source.range < 1f)
                    continue;
                
                var sourcePosition = propPosition + propRotation * source.transform.localPosition;
                var sourceRangeSqr = source.range;
                sourceRangeSqr *= sourceRangeSqr;
                
                for (int s = 0; s < nodeCount; ++s)
                {
                    var node = ambientLightNodes[s];
                    var distanceSqr = Vector3.SqrMagnitude (node.pos - sourcePosition);
                    if (distanceSqr > sourceRangeSqr)
                        continue;

                    if (node.contributions == null)
                        node.contributions = new List<AreaLightContribution> ();

                    var distance = Vector3.Distance (node.pos, sourcePosition);
                    var divisor = Mathf.Pow (distance, falloffPower); 
                    
                    node.contributions.Add (new AreaLightContribution
                    {
                        pos = sourcePosition,
                        color = source.color,
                        intensity = source.intensity * falloffMultiplier / divisor,
                        distance = distance,
                        sourceType = AreaLightContributionType.Prop,
                        sourceIndex = i
                    });
                }
            }
        }

        // Consider config on tileset blocks
        var pointSourceRangeSqr = pointSourceRange * pointSourceRange;
        
        for (int i = 0, limit = points.Count; i < limit; ++i)
        {
            var point = points[i];
            if (point == null)
                continue;
            
            // Skip empty points
            if (point.spotConfiguration == AreaNavUtility.configEmpty || point.spotConfiguration == AreaNavUtility.configFull)
                continue;
            
            // Skip damaged points
            if (point.integrity < 1f || point.spotHasDamagedPoints)
                continue;
            
            // Skip points not painted to emit
            if (point.customization == null || point.customization.overrideIndex < 1)
                continue;
            
            if (point.lightData == null)
                continue;

            var source = point.lightData;
            if (source.intensity < 0.1f)
                continue;
            
            var direction = AreaAssetHelper.GetSurfaceDirection (point.spotConfiguration);
            var sourcePosition = point.instancePosition + direction * source.offset;
            var sourceIntensity = source.intensity * pointSourceIntensity;
            
            Debug.DrawLine (point.instancePosition, sourcePosition, Color.white, 5f);
            
            for (int s = 0; s < nodeCount; ++s)
            {
                var node = ambientLightNodes[s];
                var distanceSqr = Vector3.SqrMagnitude (node.pos - sourcePosition);
                if (distanceSqr > pointSourceRangeSqr)
                    continue;

                if (node.contributions == null)
                    node.contributions = new List<AreaLightContribution> ();

                var distance = Vector3.Distance (node.pos, sourcePosition);
                var divisor = Mathf.Pow (distance, falloffPower); 
                    
                node.contributions.Add (new AreaLightContribution
                {
                    pos = sourcePosition,
                    color = pointEmissionColor,
                    intensity = sourceIntensity * falloffMultiplier / divisor,
                    distance = distance,
                    sourceType = AreaLightContributionType.Point,
                    sourceIndex = i
                });
            }
        }
        
        
        
        
        // Clear filtered list
        ambientLightNodesFiltered.Clear ();

        // Promote nodes with at least one contribution to the filtered list
        for (int s = 0; s < nodeCount; ++s)
        {
            var node = ambientLightNodes[s];
            if (node.contributions == null || node.contributions.Count == 0)
                continue;

            ambientLightNodesFiltered.Add (new AreaLightSource
            {
                pos = node.pos,
                contributions = node.contributions
            });
        }

        
        
        
        // Count promoted nodes, initialize final collection
        int filteredCount = ambientLightNodesFiltered.Count;
        ambientLightNodesFinalized.Clear ();
        
        // Update average position, color and intensity under filtered nodes
        // Promote nodes with sufficient intensity to final list
        for (int s = 0; s < filteredCount; ++s)
        {
            var node = ambientLightNodesFiltered[s];
            var contributionCount = node.contributions.Count;

            node.posAverage = node.pos;
            node.color = Color.black;
            node.intensity = 0f;
            
            // This count will not start at 0, but might drop to 0 from one-by-one invalidations on destruction
            if (contributionCount == 0)
            {
                node.lightNeeded = false;
                continue;
            }
            
            var colorVector = Vector3.zero;
            var posAverage = Vector3.zero;
            
            for (int c = 0; c < contributionCount; ++c)
            {
                var contribution = node.contributions[c];
                var clr = contribution.color;
                colorVector += new Vector3 (clr.r, clr.g, clr.b) * contribution.intensity;
                posAverage += contribution.pos;
            }

            var colorVectorNormalized = colorVector.normalized;
            var intensity = colorVector.magnitude;
            
            if (intensity < intensityMinimum)
            {
                node.lightNeeded = false;
                continue;
            }
            
            var color = new Color (colorVectorNormalized.x, colorVectorNormalized.y, colorVectorNormalized.z);
            posAverage /= contributionCount;

            node.posAverage = posAverage;
            node.color = color;
            node.intensity = intensity;
            node.lightNeeded = true;
            
            ambientLightNodesFinalized.Add (node);
        }

        // Cull weaker lights from finalized list where multiple overlap
        float distanceLimitSqr = Mathf.Pow (lightSeparation, 2);

        for (int s1 = ambientLightNodesFinalized.Count - 1; s1 >= 0; --s1)
        {
            var lightActive1 = ambientLightNodesFinalized[s1];
            for (int s2 = ambientLightNodesFinalized.Count - 1; s2 >= 0; --s2)
            {
                if (s2 == s1)
                    continue;
                
                var lightActive2 = ambientLightNodesFinalized[s2];
                var posDelta = lightActive1.pos - lightActive2.pos;
                var distanceSqr = posDelta.sqrMagnitude;
                
                if (distanceSqr > distanceLimitSqr)
                    continue;

                if (lightActive2.intensity < lightActive1.intensity)
                    continue;
                
                ambientLightNodesFinalized.RemoveAt (s1);
                break;
            }
        }

        ApplyNodes (ambientLightNodesFinalized);
    }

    /*
    public void OnPointStateChange (AreaVolumePoint point)
    {
        int index = point.spotIndex;
        OnSourceChange (index, AreaLightContributionType.Point);
    }
    
    public void OnPropChange (int index)
    {
        OnSourceChange (index, AreaLightContributionType.Prop);
    }
    
    public void OnSourceChange (int index, AreaLightContributionType type)
    {
        ambientLightNodesUpdated.Clear ();

        for (int i = 0, iLimit = ambientLightNodesFinalized.Count; i < iLimit; ++i)
        {
            var node = ambientLightNodesFinalized[i];
            if (node == null || node.contributions == null)
                continue;
            
            for (int c = 0, cLimit = node.contributions.Count; c < cLimit; ++c)
            {
                var contribution = node.contributions[c];
                if (contribution.sourceType != type)
                    continue;

                if (contribution.sourceIndex != index)
                    continue;
                
                ambientLightNodesUpdated.Add (node);
                break;
            }
        }

        ApplyNodes (ambientLightNodesUpdated);
    }
    */

    private void ApplyNodes (List<AreaLightSource> nodes)
    {
        if (nodes == null)
            return;

        int nodeCount = nodes.Count;
        if (nodeCount == 0)
            return;
        
        var rootTransform = transform;
        
        for (int s = 0; s < nodeCount; ++s)
        {
            var node = nodes[s];
            bool lightComponentEnabled = node.lightNeeded && node.intensity > 0.1f;

            if (lightComponentEnabled)
            {
                Light lightComponent = null;
                if (node.lightAdded)
                    lightComponent = node.light;
                else
                {
                    var lightObject = new GameObject ($"AreaAmbientLight_{s}");
                    var lightTransform = lightObject.transform;
                    lightTransform.parent = rootTransform;
                    lightTransform.position = lightPositionAveraging ? node.posAverage : node.pos;

                    lightComponent = lightObject.AddComponent<Light> ();
                    node.light = lightComponent;
                    node.lightAdded = true;
                }

                lightComponent.range = lightRadius;
                lightComponent.color = node.color;
                lightComponent.intensity = Mathf.Min (intensityMaximum, node.intensity);

                if (!lightComponent.enabled)
                    lightComponent.enabled = true;
            }
            else
            {
                if (node.lightAdded)
                {
                    var lightComponent = node.light;
                    if (lightComponent.enabled)
                        node.light.enabled = false;
                }
                    
            }
        }
    }
}
