using System;
using UnityEngine;

namespace PhantomBrigade.Data
{
    public enum CullingMode
    {
        None,
        Frustum,
    }

    [Serializable] 
    public class DataContainerRendering : DataContainerUnique
    {
        [Header ("Phantom ECS Hybrid Renderer")]
        public bool drawAllCameras = false;
        public bool enablePhantomRenderer = true;
        public bool enableRendererSync = true;
        public bool allowPartialSync = false;
        public bool forceSyncTimings = false;
        public float syncUpdateRate = 0.0222f;
        
        [Header("Compute Buffer settings")]
        public int computeBufferUpperLimit = 10240;
        public int defaultComputeBufferSize = 1024;
        
        public int computeBufferResizeIncrement = 1024;
        public bool allowPartialIncrements = true;
        public int partialIncrementPadding = 64;

        [Header("Phantom ECS GPU Compute Renderer")]
        public CullingMode gpuCullingMode = CullingMode.Frustum;
        public bool combineShadowPass = false;
        public int maxHiZTextureSizeX = 1024;
        public int maxHiZTextureSizeY = 512;
    }
}

