using System;
using System.Collections.Generic;
using System.Text;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{

    public class DataBlockOverworldEventActorCheck
    {
        [DropdownReference(true)]
        [ListDrawerSettings (AlwaysAddDefaultValue = true, ShowPaging = false)]
        public DataBlockOverworldMemoryCheckGroup eventMemory;
        
        protected static StringBuilder sb = new StringBuilder ();
        
        public override string ToString ()
        {
            sb.Clear ();

            if (eventMemory != null)
            {
                sb.Append ("\nMemory: ");
                sb.Append (eventMemory);
            }

            return sb.ToString ();
        }
        
        #region Editor
        #if UNITY_EDITOR

        #endif
        #endregion
    }
    
    public class DataBlockOverworldEventActorWorldCheck : DataBlockOverworldEventActorCheck
    {
	    [DropdownReference(true)]
	    public DataBlockOverworldEventSubcheckInt provinceDistance;
        
	    [DropdownReference(true)]
	    [ValueDropdown ("@DataMultiLinkerOverworldProvinceBlueprints.data.Keys")]
	    public string provinceKey;

        [DropdownReference (true)]
        [ListDrawerSettings (AlwaysAddDefaultValue = true, ShowPaging = false)]
        public DataBlockOverworldMemoryCheckGroup provinceMemory;
        
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckBool provinceHostile;

	    [DropdownReference(true)]
	    public DataBlockOverworldEventSubcheckFloat radius;

	    [DropdownReference(true)]
	    public DataBlockOverworldEventSubcheckFaction faction;
        
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckBool resupplyPoint;
        
        [DropdownReference]
        public DataBlockOverworldEventSubcheckAI ai;

	    [ShowIf ("@tags != null && tags.Count > 0")]
	    public EntityCheckMethod tagsMethod = EntityCheckMethod.RequireAll;

	    [DropdownReference]
	    [ListDrawerSettings (AlwaysAddDefaultValue = true, ShowPaging = false)]
	    public List<DataBlockOverworldEventSubcheckTag> tags;
        
        public bool IsIterationOrderReversed()
        {
            return radius != null && radius.check == FloatCheckMode.Greater;
        }

        #region Editor

#if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldEventActorWorldCheck () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

#endif

        #endregion
    }

    public class DataBlockOverworldEventActorUnitCheck : DataBlockOverworldEventActorCheck
    {
        [DropdownReference]
        public DataBlockOverworldEventSubcheckBool squad;

        [DropdownReference]
        public DataBlockOverworldEventSubcheckFloat integrityAverage;
        
        [DropdownReference]
        public DataBlockOverworldEventSubcheckFloat integrityLowest;
        
        public override string ToString ()
        {
            var textMain = base.ToString ();
        
            sb.Clear ();
            sb.Append (textMain);

            if (squad != null)
            {
                sb.Append ("\n- In squad: ");
                sb.Append (squad);
            }
            
            if (integrityAverage != null)
            {
                sb.Append ("\n- Average integrity: ");
                sb.Append (integrityAverage);
            }
            
            if (integrityLowest != null)
            {
                sb.Append ("\n- Lowest integrity: ");
                sb.Append (integrityLowest);
            }
            
            if (eventMemory != null && eventMemory.checks != null && eventMemory.checks.Count > 0)
            {
                sb.Append ($"\n- Memory ({(eventMemory.method == EntityCheckMethod.RequireAll ? "all must match" : "any of the following")}):");
                foreach (var subcheck in eventMemory.checks)
                {
                    sb.Append ("\n  - ");
                    sb.Append (subcheck.ToString ());
                }
            }

            return sb.ToString ();
        }
        

        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockOverworldEventActorUnitCheck () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }

    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldEventActorUnitSubcheckPilot
    {
        public bool required;
    }
    
    public class DataBlockOverworldEventActorPilotCheck : DataBlockOverworldEventActorCheck
    {
        [DropdownReference]
        public DataBlockOverworldEventSubcheckFloat health;
        
        [DropdownReference]
        public DataBlockOverworldEventSubcheckFloat healthNormalized;
        
        [DropdownReference]
        public DataBlockOverworldEventSubcheckFloat healthLimit;
        
        [DropdownReference]
        public DataBlockOverworldEventSubcheckFloat concussionOffset;
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockOverworldEventActorPilotCheck () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
        
        public override string ToString ()
        {
            var textMain = base.ToString ();
        
            sb.Clear ();
            sb.Append (textMain);

            if (health != null)
            {
                sb.Append ("\n- Health: ");
                sb.Append (health);
            }
            
            if (eventMemory != null && eventMemory.checks != null && eventMemory.checks.Count > 0)
            {
                sb.Append ($"\n- Memory ({(eventMemory.method == EntityCheckMethod.RequireAll ? "all must match" : "any of the following")}):");
                foreach (var subcheck in eventMemory.checks)
                {
                    sb.Append ("\n  - ");
                    sb.Append (subcheck.ToString ());
                }
            }

            return sb.ToString ();
        }
    }
}