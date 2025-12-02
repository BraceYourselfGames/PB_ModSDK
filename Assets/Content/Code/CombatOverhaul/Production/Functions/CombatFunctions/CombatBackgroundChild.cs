using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatBackgroundChild
    {
        [PropertyOrder (-1)]
        public string segment;
        
        [PropertyOrder (-1)]
        public string child;
    }
    
    [Serializable]
    public class CombatBackgroundChildActivation : CombatBackgroundChild, ICombatFunction
    {
        public bool active;
        
        public void Run ()
        {
            #if !PB_MODSDK

            var linker = AreaSegmentHelper.GetSegmentLinker (segment);
            if (linker == null)
                return;

            if (active)
                linker.SetChildActive (child);
            else
                linker.SetChildInactive (child);

            #endif
        }
    }
    
    [Serializable]
    public class CombatBackgroundChildTween : CombatBackgroundChild, ICombatFunction
    {
        public bool active;
        
        [ShowIf (nameof(active))]
        public float duration;
        
        [ShowIf (nameof(active))]
        public Vector2 range;
        
        public void Run ()
        {
            #if !PB_MODSDK

            var linker = AreaSegmentHelper.GetSegmentLinker (segment);
            if (linker == null)
                return;

            if (active)
                linker.StartChildTween (child, duration, range);
            else
                linker.StopChildTween (child);

            #endif
        }
    }
    
    [Serializable]
    public class CombatBackgroundChildParticle : CombatBackgroundChild, ICombatFunction
    {
        public bool active;
        
        public void Run ()
        {
            #if !PB_MODSDK

            var linker = AreaSegmentHelper.GetSegmentLinker (segment);
            if (linker == null)
                return;

            if (active)
                linker.StartChildParticleSystem (child);
            else
                linker.StopChildParticleSystem (child);

            #endif
        }
    }
}