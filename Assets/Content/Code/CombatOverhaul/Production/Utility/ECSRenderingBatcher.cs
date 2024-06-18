using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine.Rendering;
using Entity = Unity.Entities.Entity;

namespace CustomRendering
{
    public class ECSRenderingBatcher : MonoBehaviour
    {
        private static ECSRenderingBatcher instance;

        public class BatchMeshData
        {
            public Mesh mesh;
            public int subMesh;
            public long id;
            public Material material;
            public ShadowCastingMode shadowCastingMode;
            public bool receiveShadows;
        }

        public class BatchData
        {
            public BatchMeshData MeshData;

            public List<Entity> batchEntities;
        }

        public int batchCutoff = 5;

        private static EntityArchetype rendererArchetype;

        //Batches stored for clean / housekeeping
        private static Dictionary<int, string> allGroups = new Dictionary<int, string> ();

        //Batches that have not yet been emitted to ECS
        private static Dictionary<long, BatchData> pendingBatches = new Dictionary<long, BatchData> ();
        private static EntityQuery rendererLinkQuery;
        private static bool initialized = false;
        private static bool debug = true;

        private void Awake ()
        {
            InitialSetup ();
        }

        private void OnEnable ()
        {
            // InitialSetup ();
        }

        private void InitialSetup ()
        {
            CheckInitialization ();
            
            if (instance != null)
            {
                Destroy (this);
                return;
            }

            instance = this;
        }

        private static void CheckInitialization ()
        {
            if (initialized)
                return;
            initialized = true;

            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            rendererArchetype = entityManager.CreateArchetype
            (
                typeof (PropTag),
                //typeof (InstancedMeshRenderer),
                typeof (LocalToWorld),
                typeof (PropertyVersion),
                typeof (ScaleShaderProperty),
                typeof (HSBOffsetProperty),
                typeof (RendererGroup),
                typeof (PackedPropShaderProperty),
                typeof (Parent),
                typeof (LocalToParent),
                typeof (Translation)
            );

            EntityQueryDesc renderLinkDescription = new EntityQueryDesc 
            {
                All = new ComponentType[]
                {
                    typeof(RendererGroup),
                }
            };
            
            rendererLinkQuery = entityManager.CreateEntityQuery(renderLinkDescription);
        }

        public static void SubmitBatches ()
        {
            instance.EmitBatchesToECS ();
        }

        public static void FullCleanup ()
        {
            CleanupGroups (allGroups);
        }

        public void OnDestroy ()
        {
            FullCleanup ();
        }

        public static int CleanupGroup (int groupID)
        {
            if (World.DefaultGameObjectInjectionWorld == null) return 0;

            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            if (entityManager == null) return 0;
            
            rendererLinkQuery.SetSharedComponentFilter (new RendererGroup { id = groupID });
            var rendererEntities = rendererLinkQuery.ToEntityArray (Allocator.TempJob);

            for (int j = 0; j < rendererEntities.Length; ++j)
            {
                var entity = rendererEntities[j];

                entityManager.DestroyEntity (entity);
            }

            int length = rendererEntities.Length;
            rendererEntities.Dispose ();

            return length;
        }

        private static void CleanupGroups (Dictionary<int, string> renderGroups)
        {
            int groups = renderGroups.Count;
            if (groups == 0) 
                return;
            
            if (debug)
                sb.Clear ();
                
            int entities = 0;

            foreach (var group in renderGroups)
            {
                int entityCount = CleanupGroup (group.Key);

                entities += entityCount;
                
                if (debug)
                    sb.Append ($"Cleaning group {group.Value} with ID {group.Key} found {entityCount} instances ");
                sb.Append ("\n");
            }

            if (debug)
            {
                sb.Append ($"Cleaned up: {entities} total entities");
                Debug.LogWarning (sb.ToString ());
            }

            renderGroups.Clear ();
            pendingBatches.Clear ();
        }


        public static bool AreBatchInstancesRegistered (int rendererLinkID)
        {
            CheckInitialization ();
            return allGroups.ContainsKey (rendererLinkID);
        }

        public static void RegisterBatchInstances
        (
            Entity parentEntity,
            List<CompressedObjectGroup> objectGroups,
            int rendererLinkID,
            string groupName
        )
        {
            CheckInitialization ();
            if (AreBatchInstancesRegistered (rendererLinkID))
                return;
            
            if (debug)
            {
                sb.Clear ();
                sb.Append ($"Registering {objectGroups.Count} compressed object groups for batching... | Renderer link ID: {rendererLinkID}");
            }
        
            // Logging moved to a condensed report
            // if (instance.debug)
            //     Debug.Log ($"Registering {meshRenderers.Length} batch instances with link id {rendererLinkID} : destroying renderers : {destroyRenderers.ToString ()}");
            
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            RendererGroup rendererGroup = new RendererGroup { id = rendererLinkID }; 
            allGroups.Add (rendererLinkID, groupName);
            

            for (int i = 0; i < objectGroups.Count; ++i)
            {
                var group = objectGroups[i];
                
                var sharedMesh = group.mesh;
                if (sharedMesh == null)
                {
                    Debug.LogWarning ($"Skipping batching of compressed group {groupName}/{rendererLinkID} due to null mesh reference");
                    continue;
                }
                
                var sharedMeshID = sharedMesh.GetInstanceID ();
                var sharedMaterials = group.materials;
                if (sharedMaterials == null)
                {
                    Debug.LogWarning ($"Skipping batching of compressed group {groupName}/{rendererLinkID} due to null materials reference");
                    continue;
                }

                if (debug)
                {
                    sb.Append ("\n");
                    sb.Append ($"- {i} | Group {groupName}/{rendererLinkID} | Mesh: {sharedMesh.name} | Count: {group.placements.Count}");
                }

                // Check to make sure the materials can be instanced
                for (int j = 0; j < sharedMaterials.Length; ++j)
                {
                    var currentMaterial = sharedMaterials[j];
                    if (currentMaterial == null)
                        continue;

                    if (currentMaterial.enableInstancing != true)
                        continue;
                    
                    long batchID = Utilities.MakeLong (sharedMeshID, ($"{currentMaterial.name}_ecs{j}").GetHashCode ());
                    BatchData batchData = null;

                    if (pendingBatches.ContainsKey (batchID))
                        batchData = pendingBatches[batchID];
                    else
                    {
                        batchData = new BatchData
                        {
                            MeshData = new BatchMeshData
                            {
                                mesh = sharedMesh,
                                id = batchID,
                                material = currentMaterial,
                                shadowCastingMode = ShadowCastingMode.On,
                                receiveShadows = true,
                                subMesh = j
                            }
                        };

                        pendingBatches.Add (batchID, batchData);
                        batchData.batchEntities = new List<Entity> ();
                    }

                    for (int x = 0; x < group.placements.Count; ++x)
                    {
                        var placement = group.placements[x];
                        var trs = Matrix4x4.TRS (placement.position, placement.rotation, placement.scale);
                        var s = placement.scale.y;
                        
                        Entity rendererEntity = entityManager.CreateEntity (rendererArchetype);

                        entityManager.SetComponentData (rendererEntity, new Parent { Value = parentEntity });
                        entityManager.SetComponentData (rendererEntity, new Translation { Value = placement.position });
                        entityManager.SetComponentData (rendererEntity, new PropertyVersion { version = 1 });
                        entityManager.SetComponentData (rendererEntity, new ScaleShaderProperty { property = new HalfVector4 (s, s, s, 1) });
                        entityManager.SetComponentData (rendererEntity,
                            new HSBOffsetProperty
                            {
                                property = new HalfVector8(new HalfVector4(0f, 0.5f, 0.5f, 1),
                                    new HalfVector4(0f, 0.5f, 0.5f, 1))
                            });
                        entityManager.SetComponentData (rendererEntity, new PackedPropShaderProperty { property = new HalfVector4(1, 0, 1, 0) });
                        entityManager.SetComponentData (rendererEntity, new LocalToWorld { Value = trs });
                        entityManager.SetSharedComponentData (rendererEntity, rendererGroup);

                        batchData.batchEntities.Add (rendererEntity);
                    }
                }
            }
            
            if (debug)
                Debug.Log (sb.ToString ());
        }

        public static void RegisterBatchInstances (Entity parentEntity, MeshRenderer[] meshRenderers, int rendererLinkID, bool destroyRenderers, string groupName)
        {
            CheckInitialization ();
            
            // Logging moved to a condensed report
            // if (instance.debug)
            //     Debug.Log ($"Registering {meshRenderers.Length} batch instances with link id {rendererLinkID} : destroying renderers : {destroyRenderers.ToString ()}");

            if (debug)
            {
                sb.Clear ();
                sb.Append ($"Registering {meshRenderers.Length} mesh renderers for batching... | Renderer link ID: {rendererLinkID} | Group name: {groupName} | Destroy source: {destroyRenderers}");
            }
            
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            RendererGroup rendererGroup = new RendererGroup { id = rendererLinkID };

            if (!allGroups.ContainsKey (rendererLinkID))
                allGroups.Add (rendererLinkID, groupName);

            for (int i = 0; i < meshRenderers.Length; ++i)
            {
                var currentRenderer = meshRenderers[i];

                // Check to make sure the materials can be instanced
                for (int j = 0; j < currentRenderer.materials.Length; ++j)
                {
                    var currentMaterial = currentRenderer.materials[j];
                    if (currentMaterial == null)
                        continue;

                    if (currentMaterial.enableInstancing != true)
                        continue;

                    var meshFilter = currentRenderer.GetComponent<MeshFilter> ();
                    if (meshFilter == null || meshFilter.sharedMesh == null)
                    {
                        Debug.LogWarning ($"Invalid mesh configuration on {currentRenderer.gameObject.name}");
                        continue;
                    }

                    var sharedMesh = meshFilter.sharedMesh;
                    var sharedMeshID = meshFilter.sharedMesh.GetInstanceID ();
                    
                    if (debug)
                    {
                        sb.Append ("\n");
                        sb.Append ($"- {i}.{j} | Mesh: {sharedMesh.name} | Material: {currentMaterial.name}");
                    }

                    long batchID = Utilities.MakeLong (sharedMeshID, ($"{currentMaterial.name}_ecs{j}").GetHashCode ());
                    BatchData batchData = null;

                    if (pendingBatches.ContainsKey (batchID))
                        batchData = pendingBatches[batchID];
                    else
                    {
                        batchData = new BatchData
                        {
                            MeshData = new BatchMeshData
                            {
                                mesh = meshFilter.sharedMesh,
                                id = batchID,
                                material = currentMaterial,
                                shadowCastingMode = currentRenderer.shadowCastingMode,
                                receiveShadows = currentRenderer.receiveShadows,
                                subMesh = j
                            }
                        };

                        pendingBatches.Add (batchID, batchData);
                        batchData.batchEntities = new List<Entity> ();
                    }

                    Entity rendererEntity = entityManager.CreateEntity (rendererArchetype);

                    var currentTransform = currentRenderer.transform;
                    
                    var trs = Matrix4x4.TRS (currentTransform.position, currentTransform.rotation, currentTransform.lossyScale);

                    entityManager.SetComponentData (rendererEntity, new Parent { Value = parentEntity });
                    entityManager.SetComponentData (rendererEntity, new Translation { Value = currentRenderer.transform.localPosition });
                    entityManager.SetComponentData (rendererEntity, new PropertyVersion { version = 1 });
                    entityManager.SetComponentData (rendererEntity, new ScaleShaderProperty { property = new HalfVector4 (1, 1, 1, 1) });
                    entityManager.SetComponentData (rendererEntity,
                        new HSBOffsetProperty
                        {
                            property = new HalfVector8(new HalfVector4(0f, 0.5f, 0.5f, 1),
                                new HalfVector4(0f, 0.5f, 0.5f, 1))
                        });
                    entityManager.SetComponentData (rendererEntity, new PackedPropShaderProperty { property = new HalfVector4 (1, 0, 1, 0) });
                    entityManager.SetComponentData (rendererEntity, new LocalToWorld { Value = trs });
                    entityManager.SetSharedComponentData (rendererEntity, rendererGroup);

                    batchData.batchEntities.Add (rendererEntity);
                }
            }

            if (destroyRenderers)
            {
                for (int i = 0; i < meshRenderers.Length; ++i)
                    Destroy (meshRenderers[i]);
            }
            else
            {
                for (int i = 0; i < meshRenderers.Length; ++i)
                    meshRenderers[i].enabled = false;
            }
            
            if (debug)
                Debug.Log (sb.ToString ());
        }

        private static void VerifyLinkQuery ()
        {
            if (rendererLinkQuery == null)
            {
                rendererLinkQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery
                (
                    typeof (RendererGroup)
                );
            }
        }

        public static void MarkDirty (int rendererLinkID)
        {
            if (World.DefaultGameObjectInjectionWorld == null) return;

            VerifyLinkQuery ();

            rendererLinkQuery.SetSharedComponentFilter (new RendererGroup { id = rendererLinkID });
            var rendererEntities = rendererLinkQuery.ToEntityArray (Allocator.TempJob);

            if (!rendererEntities.IsCreated) return;

            var propertyComponent = typeof (PropertyVersion);

            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            for (int i = 0; i < rendererEntities.Length; ++i)
            {
                var entity = rendererEntities[i];

                if (!entityManager.HasComponent (entity, propertyComponent)) continue;

                var currentVersion = entityManager.GetComponentData<PropertyVersion> (entity);

                entityManager.SetComponentData (entity, new PropertyVersion
                {
                    version = currentVersion.version + 1
                });
            }


            rendererEntities.Dispose ();
        }

        public static void SetAnimation (int rendererLinkID, bool animating)
        {
            if (World.DefaultGameObjectInjectionWorld == null) return;

            VerifyLinkQuery ();

            rendererLinkQuery.SetSharedComponentFilter (new RendererGroup { id = rendererLinkID });
            var rendererEntities = rendererLinkQuery.ToEntityArray (Allocator.TempJob);

            if (!rendererEntities.IsCreated) return;

            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            if (animating)
            {
                for (int i = 0; i < rendererEntities.Length; ++i)
                {
                    var entity = rendererEntities[i];

                    if (entityManager.HasComponent<AnimatingTag> (entity)) continue;

                    entityManager.AddComponent<AnimatingTag> (entity);
                }
            }
            else
            {
                for (int i = 0; i < rendererEntities.Length; ++i)
                {
                    var entity = rendererEntities[i];

                    if (!entityManager.HasComponent<AnimatingTag> (entity)) continue;

                    entityManager.RemoveComponent<AnimatingTag> (entity);
                }
            }


            rendererEntities.Dispose ();
        }

        public static void ToggleVisibility (bool visible, int rendererLinkID)
        {
            if (World.DefaultGameObjectInjectionWorld == null) return;

            VerifyLinkQuery ();

            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            rendererLinkQuery.SetSharedComponentFilter (new RendererGroup { id = rendererLinkID });
            var rendererEntities = rendererLinkQuery.ToEntityArray (Allocator.TempJob);

            if (!rendererEntities.IsCreated) return;

            if (visible)
            {
                for (int i = 0; i < rendererEntities.Length; ++i)
                {
                    var entity = rendererEntities[i];

                    if (!entityManager.HasComponent<CulledTag> (entity)) continue;

                    entityManager.RemoveComponent<CulledTag> (entity);
                }
            }
            else
            {
                for (int i = 0; i < rendererEntities.Length; ++i)
                {
                    var entity = rendererEntities[i];

                    if (entityManager.HasComponent<CulledTag> (entity)) continue;

                    entityManager.AddComponent<CulledTag> (entity);
                }
            }

            rendererEntities.Dispose ();
        }

        private static StringBuilder sb = new StringBuilder ();

        private void EmitBatchesToECS ()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            int batchCount = pendingBatches.Count;
            int totalInstanceCount = 0;

            if (debug)
                sb.Clear ();

            int a = 0;

            foreach (var batchData in pendingBatches)
            {
                int indexCount = batchData.Value.batchEntities.Count;
                var batchMeshData = batchData.Value.MeshData;

                var batchRenderer = new InstancedMeshRenderer
                {
                    mesh = batchMeshData.mesh,
                    id = batchMeshData.id,
                    material = batchMeshData.material,
                    castShadows = batchMeshData.shadowCastingMode,
                    receiveShadows = batchMeshData.receiveShadows,
                    subMesh = batchMeshData.subMesh,
                    instanceLimit = indexCount
                };

                if (debug)
                {
                    sb.Append ("\n");
                    sb.Append (a);
                    sb.Append (" | ");
                    sb.Append (indexCount);
                    sb.Append (" instances of mesh ");
                    sb.Append (batchRenderer.mesh.name);
                }

                if (indexCount <= batchCutoff)
                    continue;

                totalInstanceCount += indexCount;
                bool entitiesMissing = false;
                int missingCount = 0;

                for (int i = 0; i < indexCount; ++i)
                {
                    var rendererEntity = batchData.Value.batchEntities[i];
                    if (!entityManager.Exists (rendererEntity))
                    {
                        // Debug.LogWarning ($"Entity {i} in batch doesn't exist!");
                        entitiesMissing = true;
                        missingCount += 1;
                        continue;
                    }
                    
                    entityManager.AddSharedComponentData (rendererEntity, batchRenderer);
                }

                if (entitiesMissing)
                {
                    Debug.LogWarning ($"Entities missing in batch {a} (mesh {batchRenderer.mesh.name} | Missing {missingCount} out of {indexCount})");
                }

                batchData.Value.batchEntities.Clear ();
                a += 1;
            }

            if (debug)
                Debug.LogWarning ($"Emitted {batchCount} batches with a total of {totalInstanceCount} instances. Details: " + sb.ToString ());

            pendingBatches.Clear ();
        }
    }
}