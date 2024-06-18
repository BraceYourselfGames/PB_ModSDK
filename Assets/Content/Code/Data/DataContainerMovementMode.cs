using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
	public class DataBlockMovementModeUI
	{
		public string textName;
		public Color color;
		
		[DataEditor.SpriteNameAttribute (true, 32f)]
		public string icon;
	}
	
    public class DataContainerMovementMode : DataContainer
    {
	    [ValueDropdown ("@DataMultiLinkerBaseAction.data.Keys")]
	    [InlineButtonClear]
	    public string uiFromAbility;

	    [BoxGroup ("Stats")]
	    [ValueDropdown ("GetStatKeys"), InlineButtonClear]
	    public string statMovementSpeed;
	    
	    [BoxGroup ("Stats")]
	    [ValueDropdown ("GetStatKeys"), InlineButtonClear]
	    public string statEnergyChangeMoving;
	    
	    [BoxGroup ("Stats")]
	    [ValueDropdown ("GetStatKeys"), InlineButtonClear]
	    public string statEnergyChangeStationary;
	    
	    [BoxGroup ("Stats")]
	    [ValueDropdown ("GetStatKeys"), InlineButtonClear]
	    public string statVisionEnemy;
	    
	    [BoxGroup ("Stats")]
	    [ValueDropdown ("GetStatKeys"), InlineButtonClear]
	    public string statVisionOwn;
	    

        [PropertyTooltip ("Does this mode pause base actions like repair and construction?")]
        public bool haltBaseActions;

        private IEnumerable<string> GetStatKeys => DataMultiLinkerBaseStat.data.Keys;
    }
}

