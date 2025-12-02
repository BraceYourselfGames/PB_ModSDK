using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldValidateInteractionRecord : DataBlockSubcheckBool, IOverworldEntityValidationFunction
    {
        protected override string GetLabel () => present ? "Must be in history" : "Must be new";
        
        [ValueDropdown ("@DataMultiLinkerOverworldInteraction.data.Keys")]
        public string key;

        public bool global;
        
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            if (string.IsNullOrEmpty (key))
                return false;

            if (global)
            {
                var baseOverworld = IDUtility.playerBaseOverworld;
                var records = baseOverworld.hasInteractionRecords ? baseOverworld.interactionRecords.encountered : null;
                if (records != null && records.Contains (key))
                    return false;
            }
            else
            {
                var provinceKeyActive = DataHelperProvince.GetProvinceKeyActive ();
                var provinceOverworld = IDUtility.GetOverworldEntity (provinceKeyActive);
                var records = provinceOverworld != null && provinceOverworld.hasInteractionRecords ? provinceOverworld.interactionRecords.encountered : null;
                if (records != null && records.Contains (key))
                    return false;
            }

            return true;

            #else
            return false;
            #endif
        }
    }
}