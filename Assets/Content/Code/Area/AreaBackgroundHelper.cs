using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Area;
using CustomRendering;

public class AreaBackgroundHelper : MonoBehaviour
{
    [Serializable]
    public class BiomeInfo
    {
        public string key;
        public GameObject holder;
        public CompressedObjectHolder props;
    }
    
    public AreaManager am;

    public string testKey;

    public List<BiomeInfo> biomes;

    public Renderer warStateHorizon;

    private Dictionary<string, BiomeInfo> biomeLookup;

    [NonSerialized]
    private int[,] heightfieldScaled;

    public void Setup ()
    {
        biomeLookup = new Dictionary<string, BiomeInfo> ();
        
        if (biomes == null)
            biomes = new List<BiomeInfo> ();
        
        foreach (var biome in biomes)
        {
            if (biome == null || string.IsNullOrEmpty (biome.key) || biomeLookup.ContainsKey (biome.key) || biome.holder == null)
                continue;
            
            biomeLookup.Add (biome.key, biome);

            var cols = biome.holder.GetComponentsInChildren<Collider> (true);
            foreach (var col in cols)
                col.enabled = false;
        }

    }

    [Button ("Rebuild from test key", ButtonSizes.Large), ButtonGroup]
    public void RebuildTest ()
    {
        Rebuild (testKey);
    }
    
    public void RebuildOnlyBoundaryDecal ()
    {
        if (am == null || am.points.Count < 8)
        {
            Debug.LogWarning ($"Failed to load background for area due to no manager reference or points");
            return;
        }
        
        int sizeX = am.boundsFull.x - 1;
        int sizeZ = am.boundsFull.z - 1;
        
        float sizeXScaled = sizeX * TilesetUtility.blockAssetSize;
        float sizeZScaled = sizeZ * TilesetUtility.blockAssetSize;
        
        #if !PB_MODSDK
        WorldUICombat.OnConfigureMapBoundaries (sizeXScaled, sizeZScaled);
        #endif

        foreach (var biome in biomes)
        {
            if (biome == null)
                continue;

            if (biome.holder != null)
                biome.holder.SetActive (false);
        }
    }
    
    public void Rebuild (string biomeKey)
    {
        if (am == null || am.points.Count < 8)
        {
            Debug.LogWarning ($"Failed to load background for area due to no manager reference or points");
            return;
        }
        
        if (string.IsNullOrEmpty (biomeKey) || biomeLookup == null || !biomeLookup.ContainsKey (biomeKey))
        {
            Debug.LogWarning ($"Failed to load background for biome {biomeKey}");
            return;
        }
        
        int sizeX = am.boundsFull.x - 1;
        int sizeZ = am.boundsFull.z - 1;
        
        int heightfieldLength = am.boundsFull.x * am.boundsFull.z;
        heightfieldScaled = new int[am.boundsFull.x, am.boundsFull.z];
        
        for (int i = 0; i < heightfieldLength; ++i)
        {
            var point = am.points[i];
            if (point == null)
            {
                Debug.LogWarning ($"Failed to build boundary mesh due to null point {i} in current area {am.areaName}");
                return;
            }

            int iteration = 0;
            bool emptyFound = false;
            
            while (true)
            {
                if (point.pointState != AreaVolumePointState.Empty)
                    break;

                var pointNext = point.pointsInSpot[4];
                if (pointNext == null)
                    break;

                point = pointNext;
                iteration += 1;
                if (iteration > 100)
                {
                    Debug.Log ("Breaking out of while loop, something is wrong");
                    break;
                }
            }
            
            heightfieldScaled[point.pointPositionIndex.x, point.pointPositionIndex.z] = -point.pointPositionIndex.y * TilesetUtility.blockAssetSize;
        }

        float sizeXScaled = sizeX * TilesetUtility.blockAssetSize;
        float sizeZScaled = sizeZ * TilesetUtility.blockAssetSize;

        var heightXNegZNeg = heightfieldScaled[0, 0];
        var heightXPosZNeg = heightfieldScaled[sizeX, 0];
        var heightXNegZPos = heightfieldScaled[0, sizeZ];
        var heightXPosZPos = heightfieldScaled[sizeX, sizeZ];
        float heightOuterAverage = (heightXNegZNeg + heightXPosZNeg + heightXNegZPos + heightXPosZPos) * 0.25f; // (i % 2) * 6f

        transform.localPosition = new Vector3 (sizeXScaled * 0.5f, heightOuterAverage, sizeZScaled * 0.5f);

        foreach (var biome in biomes)
        {
            if (biome == null)
                continue;

            bool match = biome.key == biomeKey;
            if (biome.holder != null)
            {
                if (biome.holder.activeSelf != match)
                    biome.holder.SetActive (match);
            }
            
            if (biome.props != null)
            {
                var batchLinker = biome.props.GetComponent<ECSRendererBatchLinker> ();
                if (batchLinker != null)
                    batchLinker.MarkDirty ();
            }
        }
    }
}
