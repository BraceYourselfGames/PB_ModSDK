using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldValidateEntityTags : IOverworldValidationFunction
    {
        [LabelText ("Method")]
        public EntityCheckMethod tagsMethod = EntityCheckMethod.RequireAll;
        
        [ListDrawerSettings (AlwaysAddDefaultValue = true, ShowPaging = false)]
        public List<DataBlockOverworldEventSubcheckTag> tags = new List<DataBlockOverworldEventSubcheckTag> ();
        
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            if (entityPersistent == null || !entityPersistent.isOverworldTag || tags == null || tags.Count == 0)
                return false;

            var entityOverworld = IDUtility.GetLinkedOverworldEntity (entityPersistent);
            var blueprint = entityOverworld != null && entityOverworld.hasDataLinkOverworldEntityBlueprint ? entityOverworld.dataLinkOverworldEntityBlueprint.data : null;

            if (blueprint == null || blueprint.tagsProcessed == null)
                return false;
            
            bool tagsValid = true;
            int tagMatches = 0;
            var tagsOnBlueprint = blueprint.tagsProcessed;
            
            foreach (var tagRequirement in tags)
            {
                bool required = !tagRequirement.not;
                if (tagsOnBlueprint.Contains (tagRequirement.tag) == required)
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