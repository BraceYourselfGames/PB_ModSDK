using System;
using System.Collections.Generic;
using PhantomBrigade.Combat;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    public class PilotValidateLevel : DataBlockOverworldEventSubcheckInt, IPilotValidationFunction
    {
        public bool IsValid (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK

            if (pilot == null || !pilot.isPilotTag)
                return false;

            var xpCurrent = pilot.hasPilotExperience ? pilot.pilotExperience.current : 0;
            int levelCurrent = PilotUtility.GetLevelFromExperience (xpCurrent);
            return IsPassed (true, levelCurrent);

            #else
            return false;
            #endif
        }
    } 
    
    public class PilotValidateStatValue : DataBlockOverworldEventSubcheckFloat, IPilotValidationFunction
    {
        [LabelText ("Stat"), PropertyOrder (-10)]
        [ValueDropdown ("@DataMultiLinkerPilotStat.data.Keys")]
        public string key;

        [PropertyOrder (-10)]
        public bool normalized;
        
        public bool IsValid (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK

            if (pilot == null || !pilot.isPilotTag)
                return false;

            var statValue = normalized ? pilot.GetPilotStatNormalized (key) : pilot.GetPilotStat (key);
            return IsPassed (true, statValue);

            #else
            return false;
            #endif
        }
    }
    
    public class PilotValidateStatValueRounded : DataBlockOverworldEventSubcheckInt, IPilotValidationFunction
    {
        [LabelText ("Stat"), PropertyOrder (-10)]
        [ValueDropdown ("@DataMultiLinkerPilotStat.data.Keys")]
        public string key;
        
        public bool IsValid (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK

            if (pilot == null || !pilot.isPilotTag)
                return false;

            var statValue = pilot.GetPilotStatRounded (key);
            return IsPassed (true, statValue);

            #else
            return false;
            #endif
        }
    }
    
    public class PilotValidateStatMin : DataBlockOverworldEventSubcheckFloat, IPilotValidationFunction
    {
        [LabelText ("Stat"), PropertyOrder (-10)]
        [ValueDropdown ("@DataMultiLinkerPilotStat.data.Keys")]
        public string key;
        
        public bool IsValid (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK

            if (pilot == null || !pilot.isPilotTag)
                return false;

            bool ranged = PilotUtility.IsPilotStatRanged (pilot, key, out var range);
            return ranged && IsPassed (true, range.x);

            #else
            return false;
            #endif
        }
    }
    
    public class PilotValidateStatMax : DataBlockOverworldEventSubcheckFloat, IPilotValidationFunction
    {
        [LabelText ("Stat"), PropertyOrder (-10)]
        [ValueDropdown ("@DataMultiLinkerPilotStat.data.Keys")]
        public string key;
        
        public bool IsValid (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK

            if (pilot == null || !pilot.isPilotTag)
                return false;

            bool ranged = PilotUtility.IsPilotStatRanged (pilot, key, out var range);
            return ranged && IsPassed (true, range.y);

            #else
            return false;
            #endif
        }
    }
    
    public class PilotValidateStatMinRounded : DataBlockOverworldEventSubcheckInt, IPilotValidationFunction
    {
        [LabelText ("Stat"), PropertyOrder (-10)]
        [ValueDropdown ("@DataMultiLinkerPilotStat.data.Keys")]
        public string key;
        
        public bool IsValid (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK

            if (pilot == null || !pilot.isPilotTag)
                return false;

            bool ranged = PilotUtility.IsPilotStatRanged (pilot, key, out var range);
            return ranged && IsPassed (true, Mathf.RoundToInt (range.x));

            #else
            return false;
            #endif
        }
    }
    
    public class PilotValidateStatMaxRounded : DataBlockOverworldEventSubcheckInt, IPilotValidationFunction
    {
        [LabelText ("Stat"), PropertyOrder (-10)]
        [ValueDropdown ("@DataMultiLinkerPilotStat.data.Keys")]
        public string key;
        
        public bool IsValid (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK

            if (pilot == null || !pilot.isPilotTag)
                return false;

            bool ranged = PilotUtility.IsPilotStatRanged (pilot, key, out var range);
            return ranged && IsPassed (true, Mathf.RoundToInt (range.y));

            #else
            return false;
            #endif
        }
    }
}