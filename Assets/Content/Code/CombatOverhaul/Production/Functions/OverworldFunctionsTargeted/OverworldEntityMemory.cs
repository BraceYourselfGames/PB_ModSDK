using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using PhantomBrigade.Overworld.Systems;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
    public class OverworldEntityMemoryBase
    {
        [PropertyOrder (-1)]
        [GUIColor ("@OverworldUtility.GetMemoryEditorColor (memoryKey)")]
        [ValueDropdown ("@DataMultiLinkerOverworldMemory.data.Keys")]
        public string memoryKey;
    }
    
    public class OverworldEntityMemorySet : OverworldEntityMemoryBase, IOverworldTargetedFunction
    {
        public int value;
        
        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK
            
            var entityPersistent = IDUtility.GetLinkedPersistentEntity (entityOverworld);
            if (entityPersistent == null || string.IsNullOrEmpty (memoryKey))
                return;

            entityPersistent.SetMemoryFloat (memoryKey, value);

            #endif
        }
    }
    
    public class OverworldEntityMemorySetRandom : OverworldEntityMemoryBase, IOverworldTargetedFunction
    {
        public Vector2 valueRange;
        
        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK
            
            var entityPersistent = IDUtility.GetLinkedPersistentEntity (entityOverworld);
            if (entityPersistent == null || string.IsNullOrEmpty (memoryKey))
                return;

            var value = Mathf.RoundToInt (Random.Range (valueRange.x, valueRange.y));
            entityPersistent.SetMemoryFloat (memoryKey, value);

            #endif
        }
    }
    
    public class OverworldEntityMemoryOffset : OverworldEntityMemoryBase, IOverworldTargetedFunction
    {
        public int value;
        
        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            var entityPersistent = IDUtility.GetLinkedPersistentEntity (entityOverworld);
            if (entityPersistent == null || string.IsNullOrEmpty (memoryKey))
                return;
            
            entityPersistent.OffsetMemoryFloat (memoryKey, value);

            #endif
        }
    }
    
    public class OverworldEntityMemoryRemove : OverworldEntityMemoryBase, IOverworldTargetedFunction
    {
        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK
            
            var entityPersistent = IDUtility.GetLinkedPersistentEntity (entityOverworld);
            if (entityPersistent == null || string.IsNullOrEmpty (memoryKey))
                return;

            entityPersistent.RemoveMemoryFloat (memoryKey);

            #endif
        }
    }
}