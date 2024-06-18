using System.Collections.Generic;
using PhantomBrigade.ModTools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
	public static class ModToolsTextureHelper
	{
		private static bool initialized = false;
		
		public static void Load ()
		{
			initialized = true;
			textureGroup.textures.Clear ();
			textureGroup.textureKeysExposed.Clear ();
			
			var folderPath = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), "ModWindowImages");
			TextureManager.LoadTexturesFrom (textureGroup, folderPath);
		}

		private static TextureGroup textureGroup = new TextureGroup { folderName = "ModWindowImages" };

		public static List<string> GetTextureKeys ()
		{
			if (!initialized)
				Load ();

			return textureGroup?.textureKeysExposed;
		}

		public static Texture2D GetTexture (string key)
		{
			if (!initialized)
				Load ();
			
			if (string.IsNullOrEmpty (key))
				return null;

			if (textureGroup == null || textureGroup.textures == null)
				return null;

			bool found = textureGroup.textures.TryGetValue (key, out var tex);
			return found ? tex : null;
		}
	}
	
	public class DataBlockModToolsButton
	{
		public string label;
		public List<IModToolsFunction> actionsOnClick;
	}
	
	public class DataBlockModToolsButtonConditional : DataBlockModToolsButton
	{
		public bool visibleUnavailable;
		public List<IModToolsCheck> conditions;
	}
	
	public class DataBlockModToolsPageSection
	{
		[ShowIf ("ShowIcons")]
		public SdfIconType icon;
		
		[DropdownReference]
		[ValueDropdown ("GetImageKeys")]
		public string image;
		
        public string hint;
        
        [LabelText ("Header / Desc.")]
        public string header;
        
        [TextArea (1, 10)][HideLabel]
        public string description;
        
        [DropdownReference]
        public List<IModToolsCheck> conditionsVisible;
        
        [DropdownReference]
        public List<IModToolsCheck> conditionsEnabled;
        
        [DropdownReference]
        public List<IModToolsCheck> conditionsComplete;

        [DropdownReference]
        public List<IModToolsFunction> actionsOnClick;
        
        [DropdownReference]
        public List<DataBlockModToolsButton> buttons;

        [DropdownReference (true)]
        public DataContainerModToolsPage childPage;
        
         #if UNITY_EDITOR
    
	    [ShowInInspector]
	    private DataEditor.DropdownReferenceHelper helper;
	    
	    public DataBlockModToolsPageSection () => 
	        helper = new DataEditor.DropdownReferenceHelper (this);
	    
	    private static bool ShowIcons => DataMultiLinkerModToolsPage.Presentation.showIcons;
	    private IEnumerable<string> GetImageKeys => ModToolsTextureHelper.GetTextureKeys ();

	    #endif
	}
	
    public class DataContainerModToolsPage : DataContainer
    {
	    public string buttonLabel = "Open tutorials";
	    
	    [ShowIf ("ShowIcons")]
	    public SdfIconType titleIcon = SdfIconType.Book;
	    
	    [LabelText ("Header / Desc.")]
	    public string titleHeader = "Tutorials";
	    
	    [TextArea (1, 10)][HideLabel]
	    public string titleDesc = "This section contains tutorials. Lorem ipsum dolor sit amet.";

	    [DropdownReference]
	    public List<DataBlockModToolsPageSection> sections;
	    
	    #if UNITY_EDITOR
    
	    [ShowInInspector]
	    private DataEditor.DropdownReferenceHelper helper;
	    
	    public DataContainerModToolsPage () => 
	        helper = new DataEditor.DropdownReferenceHelper (this);

	    private static bool ShowIcons => DataMultiLinkerModToolsPage.Presentation.showIcons;

		#endif
    }
}

