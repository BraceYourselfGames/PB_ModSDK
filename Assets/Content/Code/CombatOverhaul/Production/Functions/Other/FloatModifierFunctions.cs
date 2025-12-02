using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    public class FloatOperationSet : IFloatOperation
    {
        [LabelText ("=")]
        public float value;
        
        public float Apply (float input)
        {
            return value;
        }
    }
    
    public class FloatOperationSetResolvedInt : IFloatOperation
    {
        [PropertyOrder (-1)]
        public IOverworldIntValueFunction resolver;
        
        public float Apply (float input)
        {
            #if !PB_MODSDK
            var baseOverworld = IDUtility.playerBaseOverworld;
            if (baseOverworld == null || resolver == null)
                return input;
            
            int valueResolved = resolver.Resolve (baseOverworld);
            return valueResolved;
            #else
            return input;
            #endif
        }
    }
    
    public class FloatOperationAdd : IFloatOperation
    {
        [LabelText ("-")]
        public float value;
        
        public float Apply (float input)
        {
            return input + value;
        }
    }
    
    public class FloatOperationMultiply : IFloatOperation
    {
        [LabelText ("x")]
        public float value;
        
        public float Apply (float input)
        {
            return input * value;
        }
    }
    
    public class FloatOperationDivide : IFloatOperation
    {
        [LabelText ("/")]
        public float value;
        
        public float Apply (float input)
        {
            return Mathf.Abs (value) > 0.0001f ? input / value : value;
        }
    }
    
    public class FloatOperationPower : IFloatOperation
    {
        [LabelText ("^")]
        public float value;
        
        public float Apply (float input)
        {
            return Mathf.Pow (input, value);
        }
    }
    
    public class FloatOperationSubtractFrom : IFloatOperation
    {
        public float value;
        
        public float Apply (float input)
        {
            return value - input;
        }
    }
    
    public class FloatOperationMin : IFloatOperation
    {
        public float value;
        
        public float Apply (float input)
        {
            return Mathf.Min (input, value);
        }
    }
    
    public class FloatOperationMax : IFloatOperation
    {
        public float value;
        
        public float Apply (float input)
        {
            return Mathf.Max (input, value);
        }
    }
    
    public class FloatOperationClamp : IFloatOperation
    {
        public float min = 0f;
        public float max = 1f;
        
        public float Apply (float input)
        {
            return Mathf.Clamp (input, min, max);
        }
    }
    
    public class FloatOperationRoundTo : IFloatOperation
    {
        [Min (1)]
        public int step = 1;
        
        public float Apply (float input)
        {
            int roundingIncrement = Mathf.RoundToInt (Mathf.Max (1f, step));
            return Mathf.RoundToInt (input / roundingIncrement) * roundingIncrement;
        }
    }
    
    public class FloatOperationAddResolvedInt : IFloatOperation
    {
        [PropertyOrder (-1)]
        public IOverworldIntValueFunction resolver;
        
        public float Apply (float input)
        {
            #if !PB_MODSDK
            var baseOverworld = IDUtility.playerBaseOverworld;
            if (baseOverworld == null || resolver == null)
                return input;
            
            int valueResolved = resolver.Resolve (baseOverworld);
            return input + valueResolved;
            #else
            return input;
            #endif
        }
    }
    
    public class FloatOperationSubtractResolvedInt : IFloatOperation
    {
        [PropertyOrder (-1)]
        public IOverworldIntValueFunction resolver;
        
        public float Apply (float input)
        {
            #if !PB_MODSDK
            var baseOverworld = IDUtility.playerBaseOverworld;
            if (baseOverworld == null || resolver == null)
                return input;
            
            int valueResolved = resolver.Resolve (baseOverworld);
            return input + valueResolved;
            #else
            return input;
            #endif
        }
    }
    
    public class FloatOperationMultiplyResolvedInt : IFloatOperation
    {
        [PropertyOrder (-1)]
        public IOverworldIntValueFunction resolver;
        
        public float Apply (float input)
        {
            #if !PB_MODSDK
            var baseOverworld = IDUtility.playerBaseOverworld;
            if (baseOverworld == null || resolver == null)
                return input;
            
            int valueResolved = resolver.Resolve (baseOverworld);
            return input * valueResolved;
            #else
            return input;
            #endif
        }
    }
    
    public class FloatOperationSubtractFromResolvedInt : IFloatOperation
    {
        [PropertyOrder (-1)]
        public IOverworldIntValueFunction resolver;
        
        public float Apply (float input)
        {
            #if !PB_MODSDK
            var baseOverworld = IDUtility.playerBaseOverworld;
            if (baseOverworld == null || resolver == null)
                return input;
            
            int valueResolved = resolver.Resolve (baseOverworld);
            return valueResolved - input;
            #else
            return input;
            #endif
        }
    }
}