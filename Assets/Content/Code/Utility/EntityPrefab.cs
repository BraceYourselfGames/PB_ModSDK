using System;
using System.Collections.Generic;
using CustomRendering;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Sirenix.OdinInspector;

namespace ECS.EntityPrefabs
{

    [Serializable]
    public class EntityPrefabObject
    {
        public string name = string.Empty;
        public int parent = -1;
        public bool rendered = false;
        [ShowIf ("rendered")]
        public int rendererIndex = 0;

        public Vector3 localPosition = Vector3.zero;
        public Quaternion localRotation = Quaternion.identity;
        public Vector3 localScale = Vector3.one;
    }

    [CreateAssetMenu (fileName = "EntityPrefab", menuName = "Other/EntityPrefab", order = 1)]
    public class EntityPrefab : ScriptableObject
    {
        public GameObject source;
        public List<InstancedMeshRenderer> renderers;
        public List<EntityPrefabObject> objects;
        public bool keepOnlyRenderers = false;

        [Button ("Fill from source", ButtonSizes.Large)]
        public void FillFromSource ()
        {
            if (source == null)
                return;

            renderers = new List<InstancedMeshRenderer> ();
            objects = new List<EntityPrefabObject> ();

            var transforms = new List<Transform> ();
            var transformIndices = new Dictionary<Transform, int> ();
            var rendererIdentifierToIndex = new Dictionary<long, int> ();
            var meshes = new List<Mesh> ();
            var materials = new List<Material> ();

            CollectTransformsRecursively (source.transform, transforms, transformIndices);

            // Creating plain object records with names and parenting information
            // and finding all meshes and materials

            for (int i = 0, count = transforms.Count; i < count; ++i)
            {
                var transform = transforms[i];
                var obj = new EntityPrefabObject ();
                objects.Add (obj);

                obj.parent = transform == source.transform ? -1 : transformIndices[transform.parent];
                obj.name = transform.name;
                obj.rendered = false;
                obj.localPosition = transform.localPosition;
                obj.localRotation = transform.localRotation;
                obj.localScale = transform.localScale;

                var filter = transform.GetComponent<MeshFilter> ();
                var renderer = transform.GetComponent<MeshRenderer> ();

                if (!IsUsableForRendering (renderer, filter))
                    continue;

                var mesh = filter.sharedMesh;
                if (!meshes.Contains (mesh))
                    meshes.Add (mesh);

                for (int m = 0, mLimit = renderer.sharedMaterials.Length; m < mLimit; ++m)
                {
                    var material = renderer.sharedMaterials[m];
                    if (!materials.Contains (material))
                        materials.Add (material);
                }
            }

            // Using collected meshes and materials to build a short key 
            // to collection of MeshInstanceRenderer structs

            for (int i = 0, count = transforms.Count; i < count; ++i)
            {
                var transform = transforms[i];
                var obj = objects[i];

                var filter = transform.GetComponent<MeshFilter> ();
                var renderer = transform.GetComponent<MeshRenderer> ();

                if (!IsUsableForRendering (renderer, filter))
                    continue;

                var mesh = filter.sharedMesh;
                int meshIndex = meshes.IndexOf (mesh);

                for (int m = 0, mLimit = renderer.sharedMaterials.Length; m < mLimit; ++m)
                {
                    var material = renderer.sharedMaterials[m];
                    int materialIndex = materials.IndexOf (material);
                    long rendererKey = new PackUnsignedShortsToLong ((ushort)meshIndex, (ushort)materialIndex, (ushort)m, 0).result;

                    if (rendererIdentifierToIndex.ContainsKey (rendererKey))
                        continue;

                    var rendererForEntities = new InstancedMeshRenderer
                    {
                        mesh = filter.sharedMesh,
                        material = material,
                        subMesh = m,
                        castShadows = renderer.shadowCastingMode,
                        receiveShadows = renderer.receiveShadows
                    };

                    rendererIdentifierToIndex.Add (rendererKey, renderers.Count);
                    renderers.Add (rendererForEntities);
                }
            }

            // Generating a rendered entity for each material, 
            // since ECS rendering doesn't support multi-material/multi-submesh entities

            for (int i = 0, count = transforms.Count; i < count; ++i)
            {
                var transform = transforms[i];
                var obj = objects[i];

                var filter = transform.GetComponent<MeshFilter> ();
                var renderer = transform.GetComponent<MeshRenderer> ();

                if (!IsUsableForRendering (renderer, filter))
                    continue;

                var mesh = filter.sharedMesh;
                int meshIndex = meshes.IndexOf (mesh);

                for (int m = 0, mLimit = renderer.sharedMaterials.Length; m < mLimit; ++m)
                {
                    var material = renderer.sharedMaterials[m];
                    int materialIndex = materials.IndexOf (material);

                    long rendererKey = new PackUnsignedShortsToLong ((ushort)meshIndex, (ushort)materialIndex, (ushort)m, 0).result;
                    int rendererIndex = rendererIdentifierToIndex[rendererKey];

                    var objRendered = new EntityPrefabObject ();
                    objRendered.name = string.Format ("{0}_mat{1}", obj.name, m.ToString ());
                    objRendered.parent = i;
                    objRendered.rendered = true;
                    objRendered.rendererIndex = rendererIndex;
                    objRendered.localPosition = Vector3.zero;
                    objRendered.localRotation = Quaternion.identity;
                    objRendered.localScale = Vector3.one;
                    objects.Add (objRendered);
                }
            }

            // Optional optimization - throwing away all empty objects
            // To be on the safe side, we ensure that the root object sits at origin with no parents

            if (keepOnlyRenderers)
            {
                Transform rootParent = source.transform.parent;
                Vector3 rootPosition = source.transform.localPosition;
                Quaternion rootRotation = source.transform.localRotation;
                Vector3 rootScale = source.transform.localScale;

                source.transform.parent = null;
                source.transform.localPosition = Vector3.zero;
                source.transform.localRotation = Quaternion.identity;
                source.transform.localScale = Vector3.one;

                var objectsOptimized = new List<EntityPrefabObject> ();
                var root = new EntityPrefabObject ();
                objectsOptimized.Add (root);

                root.parent = -1;
                root.name = source.name;
                root.rendered = false;
                root.localPosition = Vector3.zero;
                root.localRotation = Quaternion.identity;
                root.localScale = Vector3.one;

                for (int i = 0, count = objects.Count; i < count; ++i)
                {
                    var obj = objects[i];
                    if (!obj.rendered)
                        continue;

                    var transform = transforms[obj.parent];

                    obj.parent = 0;
                    obj.localPosition = transform.position;
                    obj.localRotation = transform.rotation;
                    obj.localScale = transform.lossyScale;
                    objectsOptimized.Add (obj);
                }

                objects = objectsOptimized;

                source.transform.parent = rootParent;
                source.transform.localPosition = rootPosition;
                source.transform.localRotation = rootRotation;
                source.transform.localScale = rootScale;
            }

            // Updating instanced rendering limits

            var instanceLimits = new Dictionary<int, int> ();

            for (int i = 0, count = objects.Count; i < count; ++i)
            {
                var obj = objects[i];
                if (!obj.rendered)
                    continue;

                if (!instanceLimits.ContainsKey (obj.rendererIndex))
                    instanceLimits.Add (obj.rendererIndex, 1);
                else
                    instanceLimits[obj.rendererIndex] += 1;
            }

            for (int i = 0, count = renderers.Count; i < count; ++i)
            {
                var renderer = renderers[i];
                renderer.instanceLimit = instanceLimits[i];
                renderers[i] = renderer;
            }
        }

        private void CollectTransformsRecursively (Transform parent, List<Transform> transforms, Dictionary<Transform, int> transformIndices)
        {
            if (!parent.gameObject.activeSelf)
                return;

            transformIndices.Add (parent, objects.Count);
            transforms.Add (parent);

            for (int i = 0, count = parent.childCount; i < count; ++i)
                CollectTransformsRecursively (parent.GetChild (i), transforms, transformIndices);
        }

        private bool IsUsableForRendering (MeshRenderer renderer, MeshFilter filter)
        {
            if (filter == null || filter.sharedMesh == null || renderer == null || !renderer.enabled || renderer.sharedMaterials.Length == 0)
                return false;
            else
                return true;
        }




        [DisableInEditorMode]
        [Button ("Instantiate entities", ButtonSizes.Large)]
        public void Spawn ()
        {
            if (!Application.isPlaying)
                return;

            EntityPrefabHelper.CheckInitialization ();
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entities = new List<Entity> ();
            var flags = new List<bool> ();

            for (int i = 0; i < objects.Count; ++i)
            {
                var obj = objects[i];
                var entity = entityManager.CreateEntity (EntityPrefabHelper.entityArchetype);
                StringProvider.SetOnEntity (entity, entityManager, obj.name);

                entityManager.SetComponentData (entity, new Translation { Value = obj.localPosition });
                entityManager.SetComponentData (entity, new Rotation { Value = obj.localRotation });
                entityManager.SetComponentData (entity, new NonUniformScale  { Value = obj.localScale });

                if (obj.rendered)
                {
                    var renderer = renderers[obj.rendererIndex];
                    if (renderer.mesh != null && renderer.material != null)
                    {
                        entityManager.AddSharedComponentData (entity, renderer);
                        Debug.Log (string.Format
                        (
                            "{0}: {1} | Renderer: {2} | Mesh: {3} | Material: {4} | Limit: {5}",
                            i,
                            obj.name,
                            obj.rendererIndex,
                            renderer.mesh.name,
                            renderer.material.name,
                            renderer.instanceLimit
                        ));
                    }
                    else
                    {
                        Debug.LogWarning (string.Format
                        (
                            "{0}: {1} | Renderer: {2} | Mesh: {3} | Material: {4} | Skipping rendered object due to missing data",
                            i,
                            obj.name,
                            obj.rendererIndex,
                            renderer.mesh.ToStringNullCheck (),
                            renderer.material.ToStringNullCheck ()
                        ));
                    }
                }

                entities.Add (entity);
            }

            for (int i = 0; i < objects.Count; ++i)
            {
                var obj = objects[i];
                var entity = entities[i];

                if (obj.parent == -1)
                    continue;

                var entityParent = entities[obj.parent];

                entityManager.SetComponentData (entity, new Parent {Value = entityParent});
                entityManager.AddComponent (entity, typeof(LocalToParent));
                //var entityAttachment = entityManager.CreateEntity (EntityPrefabHelper.entityArchetypeAttach);
                //entityManager.SetComponentData (entityAttachment, new Attach { Parent = entityParent, Child = entity });
            }
        }
    }

    public static class EntityPrefabHelper
    {
        private static bool initialized = false;
        public static EntityArchetype entityArchetype;
        //public static EntityArchetype entityArchetypeAttach;

        public static void CheckInitialization ()
        {
            if (!Application.isPlaying || initialized)
                return;

            initialized = true;

            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            entityArchetype = entityManager.CreateArchetype
            (
                typeof (Translation),
                typeof (Rotation),
                typeof (Scale),
                typeof (Static)
            );

           //entityArchetypeAttach = entityManager.CreateArchetype (typeof (Attach));
        }
    }
}
