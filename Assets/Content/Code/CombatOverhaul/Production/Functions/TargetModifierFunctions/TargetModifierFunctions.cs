using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class TargetModifierOffsetGlobal : ITargetModifierFunction
    {
        public Vector3 offset;
        
        public void Run (Vector3 originPosition, Vector3 originDirection, ref Vector3 positionModified, ref Vector3 directionModified)
        {
            #if !PB_MODSDK

            positionModified += offset;

            #endif
        }
    }
    
    [Serializable]
    public class TargetModifierOffsetGlobalRandom : ITargetModifierFunction
    {
        public List<Vector3> offsets = new List<Vector3>();
        
        public void Run (Vector3 originPosition, Vector3 originDirection, ref Vector3 positionModified, ref Vector3 directionModified)
        {
            #if !PB_MODSDK

            var offset = offsets.GetRandomEntry ();
            positionModified += offset;

            #endif
        }
    }
    
    [Serializable]
    public class TargetModifierOffsetLocal : ITargetModifierFunction
    {
        public Vector3 offset;
        
        public void Run (Vector3 originPosition, Vector3 originDirection, ref Vector3 positionModified, ref Vector3 directionModified)
        {
            #if !PB_MODSDK

            // Create a quaternion from input direction to allow directional offset application
            var rotationLocal = Quaternion.LookRotation (directionModified, Vector3.up);
            
            // Add local offset to final position using the above rotation
            positionModified += rotationLocal * offset;

            #endif
        }
    }

    [Serializable]
    public class TargetTransform
    {
        public Vector3 offset;
        public Vector3 rotation;
    }
    
    [Serializable]
    public class TargetModifierTransformRandom : ITargetModifierFunction
    {
        public List<TargetTransform> transforms = new List<TargetTransform>();
        
        public void Run (Vector3 originPosition, Vector3 originDirection, ref Vector3 positionModified, ref Vector3 directionModified)
        {
            #if !PB_MODSDK

            // Create a quaternion from input direction to allow directional offset application
            var rotationLocal = Quaternion.LookRotation (directionModified, Vector3.up);
            
            // Add local offset to final position using the above rotation
            var transform = transforms.GetRandomEntry ();
            var rot = Quaternion.Euler (transform.rotation);
            positionModified += (rotationLocal * rot) * transform.offset;

            #endif
        }
    }
    
    [Serializable]
    public class TargetModifierOffsetUnitCircle : ITargetModifierFunction
    {
        public float offset;
        
        public void Run (Vector3 originPosition, Vector3 originDirection, ref Vector3 positionModified, ref Vector3 directionModified)
        {
            #if !PB_MODSDK

            var unitCircleScaled = Random.insideUnitCircle * offset;
            positionModified += new Vector3 (unitCircleScaled.x, 0f, unitCircleScaled.y);

            #endif
        }
    }
    
    [Serializable]
    public class TargetModifierOffsetRandomDirection : ITargetModifierFunction
    {
        public float offset;
        
        public void Run (Vector3 originPosition, Vector3 originDirection, ref Vector3 positionModified, ref Vector3 directionModified)
        {
            #if !PB_MODSDK

            var rotationRandom = Quaternion.Euler (0f, Random.Range (0f, 360f), 0f);
            var offsetRandom = rotationRandom * (Vector3.forward * offset);
            positionModified += offsetRandom;

            #endif
        }
    }

    [Serializable]
    public class TargetModifierSnapToOrigin : ITargetModifierFunction
    {
        public bool position;
        public bool direction;
        
        public void Run (Vector3 originPosition, Vector3 originDirection, ref Vector3 positionModified, ref Vector3 directionModified)
        {
            #if !PB_MODSDK

            if (position)
                positionModified = originPosition;
            
            if (direction)
                directionModified = originDirection;

            #endif
        }
    }
    
    [Serializable]
    public class TargetModifierRotateDirection : ITargetModifierFunction
    {
        public Vector2 rotationRange;
        
        public void Run (Vector3 originPosition, Vector3 originDirection, ref Vector3 positionModified, ref Vector3 directionModified)
        {
            #if !PB_MODSDK

            var angle = Random.Range (rotationRange.x, rotationRange.y);
            var rotationChange = Quaternion.Euler (0f, angle, 0f);
            directionModified = rotationChange * directionModified;

            #endif
        }
    }
    
    [Serializable]
    public class TargetModifierGround : ITargetModifierFunction
    {
        [PropertyRange (0f, 16f)]
        public float groundOffset = 0f;

        public void Run (Vector3 originPosition, Vector3 originDirection, ref Vector3 positionModified, ref Vector3 directionModified)
        {
            #if !PB_MODSDK

            var groundingRayOrigin = positionModified + Vector3.up;
            var groundingRay = new Ray (groundingRayOrigin, Vector3.down);
                                
            if (Physics.Raycast (groundingRay, out var groundingHit, 200f, LayerMasks.environmentMask))
                positionModified = groundingHit.point + Vector3.up * Mathf.Max (0f, groundOffset);

            #endif
        }
    }
}