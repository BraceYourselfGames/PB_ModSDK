using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using PhantomBrigade.Overworld.Systems;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
    public class OverworldEntityEscalation : IOverworldTargetedFunction
    {
        public bool offset;
        public int level = 1;
        
        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            if (entityOverworld == null || entityOverworld.isDestroyed)
                return;

            if (offset)
            {
                int levelCurrent = entityOverworld.hasCombatEscalationLevel ? entityOverworld.combatEscalationLevel.level : 0;
                int levelNew = Mathf.Max (levelCurrent + level, 0);
                entityOverworld.ReplaceCombatEscalationLevel (levelNew);
            }
            else
            {
                int levelNew = Mathf.Max (level, 0);
                entityOverworld.ReplaceCombatEscalationLevel (levelNew);
            }
            
            

            #endif
        }
    }
    
    public class OverworldEntityLevelRelative : IOverworldTargetedFunction
    {
        public int levelOffset = 0;
        
        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            if (entityOverworld == null || entityOverworld.isDestroyed)
                return;

            var entityPersistent = IDUtility.GetLinkedPersistentEntity (entityOverworld);
            if (entityPersistent == null)
                return;
            
            var provinceActive = DataHelperProvince.GetProvinceOverworldActive ();
            if (provinceActive == null)
                return;

            var levelBase = provinceActive.hasProvinceLevel ? provinceActive.provinceLevel.level : 1;
            int levelFinal = Mathf.Max (1, levelBase + levelOffset);
            entityPersistent.ReplaceCombatUnitLevel (levelFinal);

            #endif
        }
    }

    public class OverworldEntityExpiration : IOverworldTargetedFunction
    {
        public float duration = 10;
        
        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            if (entityOverworld == null || entityOverworld.isDestroyed)
                return;

            var overworld = Contexts.sharedInstance.overworld;
            var time = overworld.hasSimulationTime ? overworld.simulationTime.f : 0f;

            if (duration <= 0f)
            {
                if (entityOverworld.hasExpirationTimer)
                    entityOverworld.RemoveExpirationTimer ();
            }
            else
                entityOverworld.ReplaceExpirationTimer (time, duration);

            #endif
        }
    }
    
    public class OverworldEntityWeather : IOverworldTargetedFunction
    {
        [PropertyRange (0f, 1f)]
        public float precipitation = 0f;
        
        [PropertyRange (0f, 1f)]
        public float temperature = 1f;
        
        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            if (entityOverworld == null || entityOverworld.isDestroyed)
                return;
            
            entityOverworld.ReplaceCombatWeather (Mathf.Clamp01 (precipitation), temperature);

            #endif
        }
    }
}