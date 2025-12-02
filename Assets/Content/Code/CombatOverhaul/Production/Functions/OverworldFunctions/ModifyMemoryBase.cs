using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class ModifyMemoryBaseAdv : DataBlockMemoryChangeAdv, IOverworldActionFunction, IOverworldFunction, ICombatFunction
    {
        public void Run (OverworldActionEntity source)
        {
            Run ();
        }
        
        public void Run ()
        {
            #if !PB_MODSDK
            
            var basePersistent = IDUtility.playerBasePersistent;
            basePersistent.ApplyEventMemoryChangeAdv (this);
            
            #endif
        }
    }
    
    [Serializable]
    public class ModifyMemoryBase : IOverworldActionFunction, IOverworldFunctionLog, IOverworldFunction, ICombatFunction
    {
        [ListDrawerSettings (CustomAddFunction = "@new DataBlockMemoryChangeFloat ()")]
        public List<DataBlockMemoryChangeFloat> changes = new List<DataBlockMemoryChangeFloat> () { new DataBlockMemoryChangeFloat () };
        
        public void Run (OverworldActionEntity source)
        {
            Run ();
        }
        
        public void Run ()
        {
            #if !PB_MODSDK
            
            var basePersistent = IDUtility.playerBasePersistent;
            basePersistent.ApplyEventMemoryChangeFloat (changes);
            
            #endif
        }

        public string ToLog ()
        {
            if (changes == null || changes.Count == 0)
                return "null";

            return changes.ToStringFormatted ();
        }
    }
    
    [Serializable]
    public class ModifyMemory : IOverworldTargetedFunction
    {
        [ListDrawerSettings (CustomAddFunction = "@new DataBlockMemoryChangeFloat ()")]
        public List<DataBlockMemoryChangeFloat> changes = new List<DataBlockMemoryChangeFloat> () { new DataBlockMemoryChangeFloat () };

        public void Run (OverworldEntity target)
        {
            #if !PB_MODSDK
            
            var targetPersistent = IDUtility.GetLinkedPersistentEntity (target);
            if (targetPersistent != null)
                targetPersistent.ApplyEventMemoryChangeFloat (changes);
            
            #endif
        }
    }
    
    [Serializable]
    public class ModifyMemoryProvinceCurrent : IOverworldFunction, ICombatFunction
    {
        [ListDrawerSettings (CustomAddFunction = "@new DataBlockMemoryChangeFloat ()")]
        public List<DataBlockMemoryChangeFloat> changes = new List<DataBlockMemoryChangeFloat> () { new DataBlockMemoryChangeFloat () };

        public void Run ()
        {
            #if !PB_MODSDK

            bool provinceActiveFound = DataHelperProvince.TryGetProvinceDependenciesActive 
            (
                out var provinceActiveBlueprint, 
                out var provinceActivePersistent, 
                out var provinceActiveOverworld
            );

            if (!provinceActiveFound)
                return;
            
            provinceActivePersistent.ApplyEventMemoryChangeFloat (changes);

            #endif
        }
    }
}