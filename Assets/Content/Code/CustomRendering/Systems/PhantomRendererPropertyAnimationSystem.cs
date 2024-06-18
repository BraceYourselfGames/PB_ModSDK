using Unity.Entities;

namespace CustomRendering
{
    /*
    [UpdateInGroup (typeof(PresentationSystemGroup))]
    [UpdateBefore(typeof(PhantomRendererSyncSystemV2)), AlwaysUpdateSystem]
    [ExecuteAlways]
    public class PhantomRendererPropertyAnimationSystem : JobComponentSystem
    {
        struct PropertyAnimationJob : IJobForEach<AnimatingTag, PropertyVersion>
        {
            public void Execute([ReadOnly] ref AnimatingTag tag, ref PropertyVersion property)
            {
                property.version++; 
            }
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new PropertyAnimationJob ()
            {

            };

            return job.Schedule (this, inputDeps);
        }
    }
    */
    
    [UpdateInGroup (typeof(PresentationSystemGroup))]
    [UpdateBefore(typeof(PhantomRendererSyncSystemV2)), AlwaysUpdateSystem]
    public class PhantomRendererPropertyAnimationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isCompiling)
                return;
#endif
            Entities.WithAll<AnimatingTag> ()
                .ForEach 
                (
                    (ref PropertyVersion property) =>
                    {
                        property.version++; 
                    }
                )
                .Schedule();
        }
    }
}