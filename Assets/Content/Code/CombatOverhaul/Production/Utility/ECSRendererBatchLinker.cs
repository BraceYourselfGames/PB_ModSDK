using Sirenix.OdinInspector;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace CustomRendering
{
    [RequireComponent (typeof (GameObjectEntity))]
    public class ECSRendererBatchLinker : MonoBehaviour
    {
        public bool registerOnStart = true;
        public bool useCompressedGroup = false;
        
        [HideIf ("useCompressedGroup")]
        public bool includeInactive = true;
        
        [HideIf ("useCompressedGroup")]
        public bool destroyRenderers = true;
        
        public bool hideObjectAfterCreation = false;

        private int batchID;
        // private bool batchRegistered = false;

        private Entity parentEntity;
        private bool animating = false;
        private bool started = false;

        
        
        
        [ContextMenu ("Update Animating")]
        public void UpdateAnimating ()
        {
            SetAnimating (animating);
        }

        public void SetAnimating (bool isAnimating)
        {
            animating = isAnimating;
            ECSRenderingBatcher.SetAnimation (batchID, isAnimating);
        }

        [ContextMenu ("Mark Dirty")]
        public void MarkDirty ()
        {
            SyncParentTransform ();
            ECSRenderingBatcher.MarkDirty (batchID);
        }

        void Start ()
        {
            if (Application.isPlaying && registerOnStart)
                CheckRegistration ();
        }

        private void SyncParentTransform ()
        {
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;


            if (parentEntity != Entity.Null)
                manager.SetComponentData (parentEntity, new LocalToWorld { Value = transform.localToWorldMatrix });
        }

        private void Update ()
        {
            if (animating)
                SyncParentTransform ();
        }

        private void CheckRegistration ()
        {
            if (batchID == 0)
                batchID = gameObject.name.GetHashCode ();

            bool batchRegistered = ECSRenderingBatcher.AreBatchInstancesRegistered (batchID);
            if (batchRegistered)
                return;
            
            // var parentObjectLink = GetComponent<GameObjectEntity> ();
            // if (parentObjectLink == null)
            //    parentObjectLink = gameObject.AddComponent<GameObjectEntity> ();

            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            parentEntity = manager.CreateEntity ();

            manager.AddComponent (parentEntity, typeof (LocalToWorld));
            manager.SetComponentData (parentEntity, new LocalToWorld { Value = transform.localToWorldMatrix });

            // Find all child renderers and collect them
            batchID = gameObject.name.GetHashCode ();

            if (useCompressedGroup)
            {
                var compressedHolder = gameObject.GetComponent<CompressedObjectHolder> ();
                if (compressedHolder != null)
                {
                    compressedHolder.CheckReferences ();
                    ECSRenderingBatcher.RegisterBatchInstances (parentEntity, compressedHolder.groups, batchID, gameObject.name);
                }
                else
                    Debug.LogWarning ($"No compressed group found under ECS batch linker object {gameObject.name}", gameObject);
            }
            else
            {
                var renderers = GetComponentsInChildren<MeshRenderer> (includeInactive);
                if (renderers.Length > 0)
                    ECSRenderingBatcher.RegisterBatchInstances (parentEntity, renderers, batchID, destroyRenderers, gameObject.name);
                else
                    Debug.LogWarning ($"No renderers found under ECS batch linker object {gameObject.name}", gameObject);
            }

            // Debug.Log ($"Registering ECS batch linker {gameObject.name}", gameObject);

            if (animating)
                SetAnimating (animating);

            if (hideObjectAfterCreation)
                gameObject.SetActive (false);
        }

        public void OnEnable ()
        {
            CheckRegistration ();
            if (!ECSRenderingBatcher.AreBatchInstancesRegistered (batchID))
                return;

            // Debug.Log ($"Enabling ECS batch linker {gameObject.name}", gameObject);
            ECSRenderingBatcher.ToggleVisibility (true, batchID);
        }

        public void OnDisable ()
        {
            if (!ECSRenderingBatcher.AreBatchInstancesRegistered (batchID))
                return;

            // Debug.Log ($"Disabling ECS batch linker {gameObject.name}", gameObject);
            ECSRenderingBatcher.ToggleVisibility (false, batchID);
        }

        public void OnDestroy ()
        {
            ECSRenderingBatcher.CleanupGroup (batchID);

            //Ensure world is still active
            if (World.DefaultGameObjectInjectionWorld == null) 
                return;

            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;

            manager.DestroyEntity (parentEntity);
        }
    }
}