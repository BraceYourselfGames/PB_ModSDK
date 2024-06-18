using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Transforms;
using PhantomBrigade.Data;
using Unity.Mathematics;
using UnityEngine.Profiling;

namespace CustomRendering
{
    [AlwaysUpdateSystem]
    [ExecuteAlways]
    [UpdateInGroup (typeof (PresentationSystemGroup))]
    [UpdateBefore (typeof (PhantomIndirectRenderingSystem))]
    public class PhantomRendererSyncSystemV2 : ComponentSystem
    {
        private readonly Dictionary<long, BlockInternalBatch> internalBlockBatches = new Dictionary<long, BlockInternalBatch> ();
        private readonly Dictionary<long, BlockExternalBatch> externalBlockBatches = new Dictionary<long, BlockExternalBatch> ();
        private readonly Dictionary<long, PropBatch> propBatches = new Dictionary<long, PropBatch> ();
        private readonly Dictionary<long, BasicBatch> genericBatches = new Dictionary<long, BasicBatch> ();

        //PropertyArchetypes
        private static ComponentTypeHandle<LocalToWorld> transformType;
        private static ComponentTypeHandle<ScaleShaderProperty> scalePropertyType;
        private static ComponentTypeHandle<HSBOffsetProperty> hsbOffsetPropertyType;
        private static ComponentTypeHandle<PackedPropShaderProperty> packedPropShaderPropertyType;

        private static ComponentTypeHandle<DamageProperty> damagePropertyType;
        private static ComponentTypeHandle<IntegrityProperty> integrityPropertyType;

        private static ComponentTypeHandle<PropertyVersion> propertyVersionType;
        private static ComponentTypeHandle<CullingIndex> cullingIndex;

        //Shared property archetypes
        private static SharedComponentTypeHandle<InstancedMeshRenderer> rendererType;

        //Chunk filters
        private EntityQuery genericChunksQuery;
        private EntityQuery propChunksQuery;
        private EntityQuery blockInternalChunksQuery;
        private EntityQuery blockExternalChunksQuery;

        private readonly List<uint> propChunkVersions = new List<uint> (500);
        private readonly List<uint> genericChunkVersions = new List<uint> (500);
        private readonly List<uint> internalBlockChunkVersions = new List<uint> (500);
        private readonly List<uint> externalBlockChunkVersions = new List<uint> (500);

        //private bool syncInterleaving = false;

        private const int initialVersion = 0;

        private float timer = 0;

        private static PhantomRendererSyncSystemV2 instance;


        public static void CleanupSyncSystem ()
        {
            instance.internalBlockBatches.Clear ();
            instance.externalBlockBatches.Clear ();
            instance.propBatches.Clear ();
            instance.genericBatches.Clear ();
        }


        protected override void OnDestroy ()
        {
            CleanupSyncSystem ();
        }

        protected override void OnCreate ()
        {
            instance = this;
            AssertPointerSafety ();

            propChunkVersions.Clear ();
            genericChunkVersions.Clear ();
            internalBlockChunkVersions.Clear ();
            externalBlockChunkVersions.Clear ();

            genericChunksQuery = GetEntityQuery (new EntityQueryDesc
            {
                None = new[]
                {
                    ComponentType.ReadOnly<CulledTag> (),
                    //We have to make sure we're not trying to render something with any of the other tags
                    ComponentType.ReadOnly<PropTag> (),
                    ComponentType.ReadOnly<PointInternalTag> (),
                    ComponentType.ReadOnly<PointExternalTag> ()
                },
                All = new[]
                {
                    ComponentType.ReadOnly<InstancedMeshRenderer> (),
                    ComponentType.ReadOnly<LocalToWorld> (),

                    ComponentType.ReadOnly<PropertyVersion> (),
                }
            });

            propChunksQuery = GetEntityQuery (new EntityQueryDesc
            {
                None = new[]
                {
                    ComponentType.ReadOnly<CulledTag> (),
                    ComponentType.ReadOnly<PointInternalTag> (),
                    ComponentType.ReadOnly<PointExternalTag> ()
                },
                All = new[]
                {
                    ComponentType.ReadOnly<PropTag> (),
                    ComponentType.ReadOnly<InstancedMeshRenderer> (),
                    ComponentType.ReadOnly<LocalToWorld> (),

                    ComponentType.ReadOnly<ScaleShaderProperty> (),

                    ComponentType.ReadOnly<HSBOffsetProperty> (),

                    ComponentType.ReadOnly<PackedPropShaderProperty> (),

                    ComponentType.ReadOnly<PropertyVersion> (),
                }
            });

            blockInternalChunksQuery = GetEntityQuery (new EntityQueryDesc
            {
                None = new[]
                {
                    ComponentType.ReadOnly<CulledTag> (),
                    ComponentType.ReadOnly<PropTag> (),
                    ComponentType.ReadOnly<PointExternalTag> ()
                },
                All = new[]
                {
                    ComponentType.ReadOnly<PointInternalTag> (),
                    ComponentType.ReadOnly<InstancedMeshRenderer> (),
                    ComponentType.ReadOnly<LocalToWorld> (),

                    ComponentType.ReadOnly<ScaleShaderProperty> (),

                    ComponentType.ReadOnly<HSBOffsetProperty> (),

                    ComponentType.ReadOnly<PropertyVersion> (),
                }
            });

            blockExternalChunksQuery = GetEntityQuery (new EntityQueryDesc
            {
                None = new[]
                {
                    ComponentType.ReadOnly<CulledTag> (),
                    ComponentType.ReadOnly<PropTag> (),
                    ComponentType.ReadOnly<PointInternalTag> (),
                },
                All = new[]
                {
                    ComponentType.ReadOnly<PointExternalTag> (),
                    ComponentType.ReadOnly<InstancedMeshRenderer> (),
                    ComponentType.ReadOnly<LocalToWorld> (),

                    ComponentType.ReadOnly<ScaleShaderProperty> (),

                    ComponentType.ReadOnly<HSBOffsetProperty> (),
                    
                    ComponentType.ReadOnly<DamageProperty>(),
                    ComponentType.ReadOnly<IntegrityProperty>(),

                    ComponentType.ReadOnly<PropertyVersion> (),
                }
            });
        }

        private static unsafe void AssertPointerSafety ()
        {
            //These must match the sizes declared in Customproperties.cs 
            //Also ensure that all batch layout and compute buffer strides match if changes to precision are made
            //Refer to Rendering Constants and Compute buffer initialization sections of Batchlayouts.cs
            
            //Half Vector 4's
            Assert.AreEqual (sizeof (uint2), sizeof (ScaleShaderProperty));
            Assert.AreEqual (sizeof (uint2), sizeof (PackedPropShaderProperty));
            
            //Half Vector 8's
            Assert.AreEqual (sizeof (uint4), sizeof(DamageProperty));
            Assert.AreEqual(sizeof (uint4), sizeof(HSBOffsetProperty));
            
            //Fixed Vector 8's
            Assert.AreEqual (sizeof (byte) * 8, sizeof(IntegrityProperty));

        }

        private void SyncInterior ()
        {
            var chunks = blockInternalChunksQuery.CreateArchetypeChunkArray (Allocator.TempJob);
            int chunkCount = chunks.Length;
            bool allowPartialSync = DataShortcuts.render.allowPartialSync;

            if (chunkCount == 0)
            {
                chunks.Dispose ();
                return;
            }

            if (internalBlockChunkVersions.Count != chunkCount)
            {
                internalBlockChunkVersions.Clear ();
                for (int i = 0; i < chunkCount; ++i)
                {
                    internalBlockChunkVersions.Add (initialVersion);
                }
            }

            for (int i = 0; i < chunkCount; ++i)
            {
                var chunk = chunks[i];

                var rendererSharedComponentIndex = chunk.GetSharedComponentIndex (rendererType);

                var currentRenderer =
                    World.EntityManager.GetSharedComponentData<InstancedMeshRenderer> (rendererSharedComponentIndex);

                IndirectRenderingBatch batch = null;
                BlockInternalBatch internalBatch = null;
                long batchIndex = currentRenderer.id;

                //Check for batch existence, and create it if necessary
                if (!internalBlockBatches.ContainsKey (batchIndex))
                {
                    internalBatch = new BlockInternalBatch ();
                    internalBatch.Setup (currentRenderer);
                    internalBlockBatches.Add (batchIndex, internalBatch);

                    batch = internalBatch.batch;

                    PhantomIndirectRenderingSystem.AddBatch (batch, internalBatch);
                }
                else
                {
                    internalBatch = internalBlockBatches[batchIndex];
                    batch = internalBatch.batch;
                }


                int currentInstances = chunk.Count;
                int computeStart = batch.instanceCount;

                int targetEndpoint = computeStart + currentInstances;
                if (targetEndpoint > batch.matrixBuffer.count)
                {
                    int bufferShortfall = Mathf.Abs(batch.matrixBuffer.count - targetEndpoint); 
                    //We must resize the batch at our next opportunity
                    internalBatch.RequestResize (bufferShortfall);

                    currentInstances -= bufferShortfall;
                }

                if (currentInstances <= 0)
                    continue;

                if (!allowPartialSync || chunk.DidChange (propertyVersionType, internalBlockChunkVersions[i]))
                {
                    batch.matrixBuffer.SetData (chunk.GetNativeArray (transformType), 0, computeStart, currentInstances);

                    internalBatch.scaleBuffer.SetData (chunk.GetNativeArray (scalePropertyType), 0,
                        computeStart, currentInstances);
                    
                    internalBatch.hsbBuffer.SetData(chunk.GetNativeArray(hsbOffsetPropertyType), 0, computeStart, currentInstances);
                }

                internalBlockChunkVersions[i] = chunk.GetChangeVersion (propertyVersionType);
                batch.instanceCount += chunk.Count;
            }

            chunks.Dispose ();
        }

        private void SyncExterior ()
        {
            var chunks = blockExternalChunksQuery.CreateArchetypeChunkArray (Allocator.TempJob);
            int chunkCount = chunks.Length;
            bool allowPartialSync = DataShortcuts.render.allowPartialSync;

            if (chunkCount == 0)
            {
                chunks.Dispose ();
                return;
            }

            if (externalBlockChunkVersions.Count != chunkCount)
            {
                externalBlockChunkVersions.Clear ();
                for (int i = 0; i < chunkCount; ++i)
                {
                    externalBlockChunkVersions.Add (initialVersion);
                }
            }

            for (int i = 0; i < chunkCount; ++i)
            {
                var chunk = chunks[i];

                var rendererSharedComponentIndex = chunk.GetSharedComponentIndex (rendererType);

                var currentRenderer =
                    World.EntityManager.GetSharedComponentData<InstancedMeshRenderer> (rendererSharedComponentIndex);

                IndirectRenderingBatch batch = null;
                BlockExternalBatch externalBatch = null;
                long batchIndex = currentRenderer.id;

                //Check for batch existence, and create it if necessary
                if (!externalBlockBatches.ContainsKey (batchIndex))
                {
                    externalBatch = new BlockExternalBatch ();
                    externalBatch.Setup (currentRenderer);
                    externalBlockBatches.Add (batchIndex, externalBatch);

                    batch = externalBatch.batch;

                    PhantomIndirectRenderingSystem.AddBatch (batch, externalBatch);
                }
                else
                {
                    externalBatch = externalBlockBatches[batchIndex];
                    batch = externalBatch.batch;
                }


                int currentInstances = chunk.Count;
                int computeStart = batch.instanceCount;

                int targetEndpoint = computeStart + currentInstances;
                if (targetEndpoint > batch.matrixBuffer.count)
                {
                    int bufferShortfall = Mathf.Abs(batch.matrixBuffer.count - targetEndpoint); 
                    //We must resize the batch at our next opportunity
                    externalBatch.RequestResize (bufferShortfall);

                    currentInstances -= bufferShortfall;
                }

                if (currentInstances <= 0)
                    continue;

                if (!allowPartialSync || chunk.DidChange (propertyVersionType, externalBlockChunkVersions[i]))
                {
                    //Verify that the batch can actually execute with the offsets
                    batch.matrixBuffer.SetData (chunk.GetNativeArray (transformType), 0, computeStart, currentInstances);

                    externalBatch.scaleBuffer.SetData (chunk.GetNativeArray (scalePropertyType), 0,
                        computeStart, currentInstances);
                    
                    externalBatch.hsbBuffer.SetData(chunk.GetNativeArray(hsbOffsetPropertyType), 0, computeStart, currentInstances);
                    
                    externalBatch.damageBuffer.SetData(chunk.GetNativeArray(damagePropertyType), 0, computeStart, currentInstances);
                    externalBatch.integrityBuffer.SetData(chunk.GetNativeArray(integrityPropertyType), 0, computeStart, currentInstances);
                }

                externalBlockChunkVersions[i] = chunk.GetChangeVersion (propertyVersionType);
                batch.instanceCount += currentInstances;
            }


            chunks.Dispose ();
        }

        private void SyncProps ()
        {
            var chunks = propChunksQuery.CreateArchetypeChunkArray (Allocator.TempJob);
            int chunkCount = chunks.Length;
            bool allowPartialSync = DataShortcuts.render.allowPartialSync;

            if (chunkCount == 0)
            {
                chunks.Dispose ();
                return;
            }

            if (propChunkVersions.Count != chunkCount)
            {
                propChunkVersions.Clear ();
                for (int i = 0; i < chunkCount; ++i)
                {
                    propChunkVersions.Add (initialVersion);
                }
            }

            for (int i = 0; i < chunkCount; ++i)
            {
                var chunk = chunks[i];

                var rendererSharedComponentIndex = chunk.GetSharedComponentIndex (rendererType);

                var currentRenderer =
                    World.EntityManager.GetSharedComponentData<InstancedMeshRenderer> (rendererSharedComponentIndex);

                IndirectRenderingBatch batch = null;
                PropBatch propBatch = null;
                long batchIndex = currentRenderer.id;

                //Check for batch existence, and create it if necessary
                if (!propBatches.ContainsKey (batchIndex))
                {
                    propBatch = new PropBatch ();
                    propBatch.Setup (currentRenderer);
                    propBatches.Add (batchIndex, propBatch);

                    batch = propBatch.batch;

                    PhantomIndirectRenderingSystem.AddBatch (batch, propBatch);
                }
                else
                {
                    propBatch = propBatches[batchIndex];
                    batch = propBatch.batch;
                }

                int computeStartIndex = batch.instanceCount;
                int currentInstances = chunk.Count;
                int targetEndpoint = computeStartIndex + currentInstances;
                
                if (targetEndpoint > batch.matrixBuffer.count)
                {
                    int bufferShortfall = Mathf.Abs(batch.matrixBuffer.count - targetEndpoint); 
                    //We must resize the batch at our next opportunity
                    propBatch.RequestResize (bufferShortfall);

                    currentInstances -= bufferShortfall;
                }

                if (currentInstances <= 0)
                    continue;


                if (!allowPartialSync || chunk.DidChange (propertyVersionType, propChunkVersions[i]))
                {
                    batch.matrixBuffer.SetData (chunk.GetNativeArray (transformType), 0, computeStartIndex, currentInstances);

                    propBatch.scaleBuffer.SetData (chunk.GetNativeArray (scalePropertyType), 0,
                        computeStartIndex, currentInstances);
                    
                    propBatch.hsbBuffer.SetData(chunk.GetNativeArray(hsbOffsetPropertyType), 0, computeStartIndex, currentInstances);

                    propBatch.packedPropBuffer.SetData (chunk.GetNativeArray (packedPropShaderPropertyType), 0,
                        computeStartIndex, currentInstances);
                }

                propChunkVersions[i] = chunk.GetChangeVersion (propertyVersionType);
                batch.instanceCount += currentInstances;
            }


            chunks.Dispose ();
        }
        

        private void SyncGeneric ()
        {
            var chunks = genericChunksQuery.CreateArchetypeChunkArray (Allocator.TempJob);
            int chunkCount = chunks.Length;
            bool allowPartialSync = DataShortcuts.render.allowPartialSync;

            if (chunkCount == 0)
            {
                chunks.Dispose ();
                return;
            }


            if (genericChunkVersions.Count != chunkCount)
            {
                genericChunkVersions.Clear ();
                for (int i = 0; i < chunkCount; ++i)
                {
                    genericChunkVersions.Add (initialVersion);
                }
            }

            for (int i = 0; i < chunkCount; ++i)
            {
                var chunk = chunks[i];

                var rendererSharedComponentIndex = chunk.GetSharedComponentIndex (rendererType);

                var currentRenderer =
                    World.EntityManager.GetSharedComponentData<InstancedMeshRenderer> (rendererSharedComponentIndex);

                IndirectRenderingBatch batch = null;
                BasicBatch basicBatch = null;
                long batchIndex = currentRenderer.id;

                //Check for batch existence, and create it if necessary
                if (!genericBatches.ContainsKey (batchIndex))
                {
                    basicBatch = new BasicBatch ();
                    basicBatch.Setup (currentRenderer);
                    genericBatches.Add (batchIndex, basicBatch);

                    batch = basicBatch.batch;

                    PhantomIndirectRenderingSystem.AddBatch (batch, basicBatch);
                }
                else
                {
                    basicBatch = genericBatches[batchIndex];
                    batch = basicBatch.batch;
                }


                int currentInstances = chunk.Count;
                int computeStart = batch.instanceCount;

                int targetEndpoint = computeStart + currentInstances;
                if (targetEndpoint > batch.matrixBuffer.count)
                {
                    int bufferShortfall = Mathf.Abs(batch.matrixBuffer.count - targetEndpoint); 
                    //We must resize the batch at our next opportunity
                    basicBatch.RequestResize (bufferShortfall);

                    currentInstances -= bufferShortfall;
                }

                if (currentInstances <= 0)
                    continue;

                if (!allowPartialSync || chunk.DidChange (propertyVersionType, genericChunkVersions[i]))
                {
                    batch.matrixBuffer.SetData (chunk.GetNativeArray (transformType), 0, computeStart, currentInstances);
                }

                genericChunkVersions[i] = chunk.GetChangeVersion (propertyVersionType);
                batch.instanceCount += currentInstances;
            }


            chunks.Dispose ();
        }

        private void ResetChunkVersions (List<uint> chunkVersions)
        {
            for (int i = 0; i < chunkVersions.Count; ++i)
            {
                chunkVersions[i] = initialVersion;
            }
        }

        private void VerifyBatches ()
        {
            bool versionsDirty = false;
            foreach (var batch in genericBatches.Values)
            {
                if (batch.CheckForResize ())
                {
                    versionsDirty = true;
                }
            }

            if (versionsDirty)
            {
                ResetChunkVersions (genericChunkVersions);
                versionsDirty = false;
            }

            foreach (var batch in externalBlockBatches.Values)
            {
                if (batch.CheckForResize ())
                {
                    versionsDirty = true;
                }
            }

            if (versionsDirty)
            {
                ResetChunkVersions (externalBlockChunkVersions);
                versionsDirty = false;
            }

            foreach (var batch in internalBlockBatches.Values)
            {
                if (batch.CheckForResize ())
                {
                    versionsDirty = true;
                }
            }

            if (versionsDirty)
            {
                ResetChunkVersions (internalBlockChunkVersions);
                versionsDirty = false;
            }

            foreach (var batch in propBatches.Values)
            {
                if (batch.CheckForResize ())
                {
                    versionsDirty = true;
                }
            }

            if (versionsDirty)
            {
                ResetChunkVersions (propChunkVersions);
                versionsDirty = false;
            }
        }

        private void Sync ()
        {
            bool enableRendererSync = true;

            timer += Time.DeltaTime;

            if (DataShortcuts.render.forceSyncTimings && timer < DataShortcuts.render.syncUpdateRate) return;

            timer = 0;

            if (DataLinkerRendering.data != null)
                enableRendererSync = DataLinkerRendering.data.enableRendererSync;

            if (!enableRendererSync)
                return;

            Profiler.BeginSample ("Sync v2");
            PhantomIndirectRenderingSystem.syncLocked = true;

            rendererType = GetSharedComponentTypeHandle<InstancedMeshRenderer> ();
            transformType = GetComponentTypeHandle<LocalToWorld> ();
            damagePropertyType = GetComponentTypeHandle<DamageProperty>();
            integrityPropertyType = GetComponentTypeHandle<IntegrityProperty>();
            packedPropShaderPropertyType = GetComponentTypeHandle<PackedPropShaderProperty> ();
            scalePropertyType = GetComponentTypeHandle<ScaleShaderProperty> ();
            hsbOffsetPropertyType = GetComponentTypeHandle<HSBOffsetProperty>();
            propertyVersionType = GetComponentTypeHandle<PropertyVersion> ();


            VerifyBatches ();
            PhantomIndirectRenderingSystem.ResetCounts ();

            SyncExterior ();
            SyncInterior ();
            SyncGeneric ();
            SyncProps ();
            
            PhantomIndirectRenderingSystem.syncLocked = false;
            Profiler.EndSample ();
        }

        protected override void OnUpdate ()
        {
            Sync ();
        }
    }
}