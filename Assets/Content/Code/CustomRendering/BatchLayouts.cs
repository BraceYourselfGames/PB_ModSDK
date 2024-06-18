using System;
using System.Runtime.InteropServices;
using PhantomBrigade.Data;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRendering
{

    public interface IIndirectLayout
    {
        void Setup(InstancedMeshRenderer renderer);

        void Dispose();
    }

    public interface IResizeable
    {
        void RequestResize (int shortFall);

        bool CheckForResize();
    }

    public class IndirectRenderingBatch : IEquatable<IndirectRenderingBatch>
    {
        public int instanceCount = 0;

        public readonly int batchHash;

        public int batchLayerID = LayerMask.NameToLayer ("Default");

        private ComputeBuffer argsBuffer;
        private uint[] args = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

        private ComputeBuffer visiblityBuffer;

        public ComputeBuffer matrixBuffer;

        public MaterialPropertyBlock properties = new MaterialPropertyBlock ();

        private InstancedMeshRenderer renderer;

        private Bounds rendererBounds = new Bounds ();
        private bool log = false;

        public void SetBatchLayerID(int layer)
        {
            batchLayerID = layer;
        }

        public int GetInstanceLimit()
        {
            return renderer.instanceLimit;
        }

        public void ResetCounts()
        {
            instanceCount = 0;
        }

        public bool IncreaseSize (int bufferShortfall)
        {
            int resizeIncrement = DataLinkerRendering.data.computeBufferResizeIncrement;

            int requestedLimit = renderer.instanceLimit + bufferShortfall;
            
            //Check to make sure the requested limit is less than the absolute upper limit
            requestedLimit = Mathf.Min (requestedLimit, DataLinkerRendering.data.computeBufferUpperLimit);

            if (requestedLimit == renderer.instanceLimit)
            {
                //Nothing to do here, we've already hit the limit, and can't resize again
                return false;
            }

            //Round up the nearest increment, if we don't allow partial incrementation
            int newLimit = DataLinkerRendering.data.allowPartialIncrements ? 
                requestedLimit + DataLinkerRendering.data.partialIncrementPadding : Mathf.CeilToInt (requestedLimit / (float) resizeIncrement) * resizeIncrement; 
            
            
            newLimit = Mathf.Min (newLimit, DataLinkerRendering.data.computeBufferUpperLimit);

            if (log)
                Debug.LogWarning ($"Resizing compute buffer {renderer.mesh.name}::{renderer.id}. From : {renderer.instanceLimit} To : {newLimit}");
            renderer.instanceLimit = newLimit;
            
            return true;
        }

        public void InitializeBuffers ()
        {
            //Instance data
            argsBuffer = new ComputeBuffer (1, RenderingConstants.argsBufferStride,
                ComputeBufferType.IndirectArguments);

            matrixBuffer = new ComputeBuffer (renderer.instanceLimit, RenderingConstants.matrix4x4Stride,
                ComputeBufferType.Structured);

            SyncArgs ();

            properties.SetBuffer (PropertyIDS.matrixDataID, matrixBuffer);
        }

        public IndirectRenderingBatch(InstancedMeshRenderer renderer)
        {
            this.renderer.instanceLimit = renderer.instanceLimit;
            if (this.renderer.instanceLimit == 0) this.renderer.instanceLimit = DataLinkerRendering.data.defaultComputeBufferSize;

            if (this.renderer.instanceLimit > DataLinkerRendering.data.computeBufferUpperLimit)
                this.renderer.instanceLimit = DataLinkerRendering.data.computeBufferUpperLimit;
            
            this.renderer.mesh = renderer.mesh;
            this.renderer.material = renderer.material;
            this.renderer.subMesh = renderer.subMesh;
            this.renderer.castShadows = renderer.castShadows;
            this.renderer.receiveShadows = renderer.receiveShadows;
            batchHash = renderer.id.GetHashCode ();

            InitializeBuffers ();
            
            rendererBounds.center = Vector3.zero;
            rendererBounds.extents = Vector3.one * 9999;
        }
        
        private void SyncArgs()
        {
            if (instanceCount > renderer.instanceLimit)
                instanceCount = renderer.instanceLimit;
            
            int subMesh = renderer.subMesh;
            if (renderer.mesh == null || renderer.material == null)
            {
                // Debug.LogWarning ($"Detected null model ({renderer.mesh == null}) or material ({renderer.material == null})");
            }
            else
            {
                //Visibility / Culling buffer
                args[0] = renderer.mesh.GetIndexCount (subMesh);
                args[1] = (uint) instanceCount;
                args[2] = renderer.mesh.GetIndexStart (subMesh);
                args[3] = renderer.mesh.GetBaseVertex (subMesh);
                args[4] = 0;
                
                //Shadow Buffer
                args[5] = renderer.mesh.GetIndexCount (subMesh);
                args[6] = (uint) instanceCount;
                args[7] = renderer.mesh.GetIndexStart (subMesh);
                args[8] = renderer.mesh.GetBaseVertex (subMesh);
                args[9] = 0;
            }

            argsBuffer.SetData (args);
        }


        public void ReleaseBuffers()
        {
            argsBuffer?.Dispose ();
            matrixBuffer?.Dispose ();
        }
        
        public void DrawAllCameras()
        {
            SyncArgs ();
            //Draw to all cameras
            Graphics.DrawMeshInstancedIndirect(renderer.mesh, renderer.subMesh, renderer.material, rendererBounds,
                argsBuffer, 0, properties,
                renderer.castShadows == ShadowCastingMode.On ? ShadowCastingMode.On : ShadowCastingMode.Off,
                renderer.receiveShadows, batchLayerID);
        }

        public void DrawSingleCamera(Camera targetCamera)
        {
            Graphics.DrawMeshInstancedIndirect(renderer.mesh, renderer.subMesh, renderer.material, rendererBounds,
                argsBuffer, 0, properties,
                renderer.castShadows == ShadowCastingMode.On ? ShadowCastingMode.On : ShadowCastingMode.Off,
                renderer.receiveShadows, batchLayerID, targetCamera);
        }

        public bool Equals (IndirectRenderingBatch other)
        {
            return batchHash == other.batchHash;
        }

        public override int GetHashCode ()
        {
            return batchHash;
        }
    }

    public class BasicBatch : IIndirectLayout, IResizeable
    {
        public IndirectRenderingBatch batch;
        private bool resizeRequested = false;
        private int bufferShortfall = 0;
        
        public void RequestResize (int shortFall)
        {
            bufferShortfall += shortFall;
            resizeRequested = true;
        }
        
        public void Setup(InstancedMeshRenderer renderer)
        {
            if(batch == null) batch = new IndirectRenderingBatch (renderer);
            batch.SetBatchLayerID (RenderingLayerIDs.defaultID);
        }

        public bool CheckForResize ()
        {
            if(batch == null || !resizeRequested) 
                return false;

            if (batch.IncreaseSize (bufferShortfall))
            {
                batch.ReleaseBuffers (); 
                batch.InitializeBuffers ();
            }

            resizeRequested = false;
            bufferShortfall = 0;
            return true;
        }

        public virtual void Dispose()
        {
            batch.ReleaseBuffers ();
            batch = null;
        }
    }


    public class PropBatch : IIndirectLayout, IResizeable
    {
        public ComputeBuffer scaleBuffer;

        public ComputeBuffer hsbBuffer;

        public ComputeBuffer packedPropBuffer;

        public IndirectRenderingBatch batch;

        private bool resizeRequested = false;
        private int bufferShortfall = 0;
        
        public void RequestResize (int shortFall)
        {
            bufferShortfall += shortFall;
            resizeRequested = true;
        }

        public bool CheckForResize ()
        {
            if(batch == null || !resizeRequested) 
                return false;

            if (batch.IncreaseSize (bufferShortfall))
            {
                batch.ReleaseBuffers ();
                ReleaseBuffers ();
                
                batch.InitializeBuffers ();
                InitializeBuffers ();
            }

            resizeRequested = false;
            bufferShortfall = 0;
            return true;
        }

        private void InitializeBuffers ()
        {
            int instanceLimit = batch.GetInstanceLimit ();
            
            scaleBuffer = new ComputeBuffer (instanceLimit, RenderingConstants.halfVector4Stride,
                ComputeBufferType.Default);

            hsbBuffer = new ComputeBuffer(instanceLimit, RenderingConstants.halfVector8Stride,
                ComputeBufferType.Default);

            packedPropBuffer =
                new ComputeBuffer (instanceLimit, RenderingConstants.halfVector4Stride,
                    ComputeBufferType.Default);

            batch.properties.SetBuffer (PropertyIDS.scaleBufferID, scaleBuffer);
            batch.properties.SetBuffer (PropertyIDS.hsbBufferID, hsbBuffer);
            batch.properties.SetBuffer (PropertyIDS.packedPropBufferID, packedPropBuffer);
        }

        public void Setup(InstancedMeshRenderer renderer)
        {
            if(batch == null) batch = new IndirectRenderingBatch (renderer);
            batch.SetBatchLayerID (RenderingLayerIDs.propsID);
            InitializeBuffers ();
        }

        private void ReleaseBuffers ()
        {
            scaleBuffer?.Dispose ();
            hsbBuffer?.Dispose();
            packedPropBuffer?.Dispose ();
        }

        public void Dispose()
        {
            batch.ReleaseBuffers ();
            batch = null;

            ReleaseBuffers ();
        }
    }

    //Data buffers necessary for indirect rendering calls
    public class BlockExternalBatch : IIndirectLayout, IResizeable
    {
        public ComputeBuffer scaleBuffer;

        public ComputeBuffer hsbBuffer;

        public ComputeBuffer damageBuffer;
        
        public ComputeBuffer integrityBuffer;

        public IndirectRenderingBatch batch;

        private bool resizeRequested = false;
        private int bufferShortfall = 0;
        
        public void RequestResize (int shortFall)
        {
            bufferShortfall += shortFall;
            resizeRequested = true;
        }
        
        public void Setup(InstancedMeshRenderer renderer)
        {
            if(batch == null) batch = new IndirectRenderingBatch (renderer);
            batch.SetBatchLayerID (RenderingLayerIDs.environmentID);
            InitializeBuffers ();
        }

        private void InitializeBuffers ()
        {
            int instanceLimit = batch.GetInstanceLimit ();

            scaleBuffer = new ComputeBuffer (instanceLimit, RenderingConstants.halfVector4Stride,
                ComputeBufferType.Structured);

            hsbBuffer = new ComputeBuffer(instanceLimit, RenderingConstants.halfVector8Stride,
                ComputeBufferType.Structured);

            damageBuffer = new ComputeBuffer(instanceLimit, RenderingConstants.halfVector8Stride,
                ComputeBufferType.Structured);

            integrityBuffer = new ComputeBuffer(instanceLimit, RenderingConstants.fixedVector8Stride,
                ComputeBufferType.Structured);

            batch.properties.SetBuffer (PropertyIDS.scaleBufferID, scaleBuffer);
            batch.properties.SetBuffer (PropertyIDS.hsbBufferID, hsbBuffer);
            batch.properties.SetBuffer(PropertyIDS.integrityBufferID, integrityBuffer);
            batch.properties.SetBuffer(PropertyIDS.damageBufferID, damageBuffer);
        }

        public bool CheckForResize ()
        {
            if(batch == null || !resizeRequested) 
                return false;

            if (batch.IncreaseSize (bufferShortfall))
            {
                batch.ReleaseBuffers ();
                ReleaseBuffers ();
                
                batch.InitializeBuffers ();
                InitializeBuffers ();
            }

            resizeRequested = false;
            bufferShortfall = 0;
            return true;
        }

        private void ReleaseBuffers ()
        {
            scaleBuffer?.Dispose ();

            hsbBuffer?.Dispose ();
            
            damageBuffer?.Dispose();
            integrityBuffer?.Dispose();
        }

        public void Dispose()
        {
            batch.ReleaseBuffers ();
            batch = null;

            ReleaseBuffers ();
        }
    }

    public class BlockInternalBatch : IIndirectLayout
    {
        public ComputeBuffer scaleBuffer;

        public ComputeBuffer hsbBuffer;

        public IndirectRenderingBatch batch;

        private bool resizeRequested = false;
        private int bufferShortfall = 0;
        
        public void RequestResize (int shortFall)
        {
            bufferShortfall += shortFall;
            resizeRequested = true;
        }
        public void Setup(InstancedMeshRenderer renderer)
        {
            if(batch == null) batch = new IndirectRenderingBatch (renderer);
            batch.SetBatchLayerID (RenderingLayerIDs.environmentID);
            InitializeBuffers ();
        }

        private void InitializeBuffers ()
        {
            int instanceLimit = batch.GetInstanceLimit ();

            scaleBuffer = new ComputeBuffer (instanceLimit, RenderingConstants.halfVector4Stride,
                ComputeBufferType.Structured);

            hsbBuffer = new ComputeBuffer(instanceLimit, RenderingConstants.halfVector8Stride,
                ComputeBufferType.Structured);

            batch.properties.SetBuffer (PropertyIDS.scaleBufferID, scaleBuffer);
            batch.properties.SetBuffer (PropertyIDS.hsbBufferID, hsbBuffer);
        }

        public bool CheckForResize ()
        {
            if(batch == null || !resizeRequested) 
                return false;

            if (batch.IncreaseSize (bufferShortfall))
            {
                batch.ReleaseBuffers ();
                ReleaseBuffers ();
                
                batch.InitializeBuffers ();
                InitializeBuffers ();
            }

            resizeRequested = false;
            bufferShortfall = 0;
            return true;
        }

        private void ReleaseBuffers ()
        {
            scaleBuffer?.Dispose ();
            hsbBuffer?.Dispose ();
        }

        public void Dispose()
        {
            batch.ReleaseBuffers ();
            batch = null;
            
            ReleaseBuffers ();
        }
    }

    public static class RenderingConstants
    {
        public static readonly int argsBufferStride = sizeof(uint) * 10;

        public static readonly int halfVector4Stride = Marshal.SizeOf(typeof(uint2));
        
        public static readonly int halfVector8Stride = Marshal.SizeOf(typeof(uint4));

        public static readonly int fixedVector4Stride = sizeof(byte) * 4;

        public static readonly int fixedVector8Stride = sizeof(byte) * 8;
        
        public static readonly int matrix4x4Stride = Marshal.SizeOf (typeof(float4x4));
    }

    public static class PropertyIDS
    {
        public static readonly int scaleBufferID = Shader.PropertyToID ("scaleData");

        public static readonly int hsbBufferID = Shader.PropertyToID("hsbData");

        public static readonly int integrityBufferID = Shader.PropertyToID("integrityData");
        
        public static readonly int damageBufferID = Shader.PropertyToID("damageData");

        public static readonly int packedPropBufferID = Shader.PropertyToID ("packedPropData");
        
        public static readonly int matrixDataID = Shader.PropertyToID ("instanceData");
    }
    
    public static class RenderingLayerIDs
    {
        public static int defaultID = LayerMask.NameToLayer ("Default");
        public static int propsID = LayerMask.NameToLayer ("Props");
        public static int environmentID = LayerMask.NameToLayer ("Environment");
    }

}