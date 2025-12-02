using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldPointTagCheck
    {
        [ValueDropdown ("@DataMultiLinkerOverworldPointPreset.GetTags ()")]
        [InlineButton ("Invert", "@GetInversionLabel ()"), GUIColor ("GetInversionColor")]
        public string tag;
        
        [HideInInspector]
        public bool not;

        #if UNITY_EDITOR
        private void Invert () => not = !not;
        private string GetInversionLabel () => not ? "Prohibited" : "Required";
        private Color GetInversionColor () => Color.HSVToRGB (not ? 0f : 0.55f, 0.5f, 1f);
        #endif
    }

    public class OverworldValidatePointFromPreset : DataBlockSubcheckBool, IOverworldEntityValidationFunction
    {
        protected override string GetLabel () => present ? "Should be point w. preset" : "Should not be point w. preset";
        
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            var entityOverworld = IDUtility.GetLinkedOverworldEntity (entityPersistent);
            bool pointWithPreset = entityOverworld != null && entityOverworld.hasDataKeyPointPreset;
            return pointWithPreset == present;

            #else
            return false;
            #endif
        }
    }
    
    public class OverworldValidateGroup :  DataBlockSubcheckBool, IOverworldEntityValidationFunction
    {
        [LabelText ("Method")]
        public EntityCheckMethod type = EntityCheckMethod.RequireAll;

        public List<IOverworldEntityValidationFunction> checks = new List<IOverworldEntityValidationFunction> ();
        
        protected override string GetLabel () => present ? "Result should be true" : "Result should be false";
        
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            if (checks == null)
                return false;

            int validCount = 0;
            foreach (var check in checks)
            {
                if (check == null)
                    continue;
                
                bool valid = check.IsValid (entityPersistent);
                if (valid)
                    validCount += 1;
            }

            bool validBasedOnType = type == EntityCheckMethod.RequireOne ? validCount > 0 : validCount == checks.Count;
            return validBasedOnType == present;

            #else
            return false;
            #endif
        }
    }

    [Serializable]
    public class OverworldValidatePointTags : IOverworldEntityValidationFunction
    {
        [LabelText ("Method")]
        public EntityCheckMethod tagsMethod = EntityCheckMethod.RequireAll;
        
        [ListDrawerSettings (AlwaysAddDefaultValue = true, ShowPaging = false)]
        public List<DataBlockOverworldPointTagCheck> tags = new List<DataBlockOverworldPointTagCheck> ();
        
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            if (entityPersistent == null || !entityPersistent.isOverworldTag || tags == null || tags.Count == 0)
                return false;

            var entityOverworld = IDUtility.GetLinkedOverworldEntity (entityPersistent);
            if (!entityOverworld.hasDataLinkPointPreset)
                return false;

            var tagsOnPreset = entityOverworld.dataLinkPointPreset.data.tagsProc;
            bool tagsPresent = tagsOnPreset != null && tagsOnPreset.Count > 0;
            
            bool tagsValid = true;
            int tagMatches = 0;

            foreach (var tagRequirement in tags)
            {
                bool required = !tagRequirement.not;
                bool tagPresent = tagsPresent && tagsOnPreset.Contains (tagRequirement.tag);
                if (tagPresent == required)
                {
                    tagMatches += 1;
                    if (tagsMethod == EntityCheckMethod.RequireOne)
                        break;
                }
            }
            
            if (tagsMethod == EntityCheckMethod.RequireOne)
                tagsValid = tagMatches > 0;
            else if (tagsMethod == EntityCheckMethod.RequireAll)
                tagsValid = tagMatches == tags.Count;

            return tagsValid;

            #else
            return false;
            #endif
        }
    }
    
    [Serializable]
    public class OverworldValidatePointKey : IOverworldEntityValidationFunction
    {
        [ValueDropdown ("@DataMultiLinkerOverworldPointPreset.data.Keys")]
        public string key;
        
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            if (entityPersistent == null || !entityPersistent.isOverworldTag)
                return false;

            var entityOverworld = IDUtility.GetLinkedOverworldEntity (entityPersistent);
            if (!entityOverworld.hasDataKeyPointPreset)
                return false;

            return string.Equals (entityOverworld.dataKeyPointPreset.s, key);

            #else
            return false;
            #endif
        }
    }
    
    [Serializable]
    public class OverworldValidatePointNameInternal : IOverworldEntityValidationFunction
    {
        public string nameInternal;
        
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            if (entityPersistent == null || !entityPersistent.isOverworldTag)
                return false;

            var entityOverworld = IDUtility.GetLinkedOverworldEntity (entityPersistent);
            if (!entityOverworld.hasNameInternal)
                return false;

            return string.Equals (entityOverworld.nameInternal.s, nameInternal);

            #else
            return false;
            #endif
        }
    }
}