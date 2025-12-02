using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldProvinceTagCheck
    {
        [ValueDropdown ("@DataMultiLinkerOverworldProvinceBlueprints.GetTags ()")]
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
    
    [Serializable]
    public class OverworldValidateProvinceTag : IOverworldGlobalValidationFunction
    {
        [LabelText ("Method")]
        public EntityCheckMethod tagsMethod = EntityCheckMethod.RequireAll;
        
        [ListDrawerSettings (AlwaysAddDefaultValue = true, ShowPaging = false)]
        public List<DataBlockOverworldProvinceTagCheck> tags = new List<DataBlockOverworldProvinceTagCheck> ();
        
        public bool IsValid ()
        {
            #if !PB_MODSDK

            bool provinceActiveFound = DataHelperProvince.TryGetProvinceDependenciesActive 
            (
                out var provinceActiveBlueprint, 
                out var provinceActivePersistent, 
                out var provinceActiveOverworld
            );
            
            if (!provinceActiveFound)
                return false;

            var tagsOnBlueprint = provinceActiveBlueprint.tags;
            bool tagsPresent = tagsOnBlueprint != null && tagsOnBlueprint.Count > 0;
            
            bool tagsValid = true;
            int tagMatches = 0;

            foreach (var tagRequirement in tags)
            {
                bool required = !tagRequirement.not;
                bool tagPresent = tagsPresent && tagsOnBlueprint.Contains (tagRequirement.tag);
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
}