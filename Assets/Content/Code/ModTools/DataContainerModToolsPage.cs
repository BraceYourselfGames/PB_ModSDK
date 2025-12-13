using System.Collections.Generic;
using System.Text;
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
	    private static StringBuilder sb = new StringBuilder ();
	    
	    [Button ("Export to Markdown"), PropertyOrder (-1)]
	    private void ExportToMarkdown ()
	    {
		    sb.Clear ();
		    sb.Append ("# ");
		    sb.Append (titleHeader);
		    sb.Append ("\n");
		    sb.Append (titleDesc);

		    sb.Append ("\n\n> This tutorial is available directly in the SDK under `PB Mod SDK/Getting Started/Tutorials/");
		    sb.Append (titleHeader);
		    sb.Append ("`. The SDK version can include interactive elements such as buttons automatically selecting project assets. ");
		    sb.Append ("Make sure you are in the right scene (game_main_sdk or game_extended_sdk). You should see DataModel and ModManager in the Hierarchy window. ");
		    sb.Append ("If you're unsure how to find the right scene, use the scene buttons in the Getting Started window.");
		    
		    if (sections != null)
		    {
			    foreach (var section in sections)
			    {
				    if (section == null)
					    continue;

				    sb.Append ("\n\n");
				    sb.Append ("## ");
				    sb.Append (section.header);

				    if (!string.IsNullOrEmpty (section.image))
				    {
					    // ![t1_01_create.png](https://github.com/BraceYourselfGames/PB_ModSDK/blob/main/ModWindowImages/t1_01_create.png)
					    sb.Append ("\n![");
					    sb.Append (section.image);
					    sb.Append (".png](https://github.com/BraceYourselfGames/PB_ModSDK/blob/main/ModWindowImages/");
						sb.Append (section.image);
						sb.Append (".png)\n");
				    }

				    sb.Append ("\n");
				    sb.Append (section.description);
			    }
		    }

		    Debug.Log ($"Exported article {titleHeader} to system copy buffer. Ctrl+V in the text editor to paste...");
		    GUIUtility.systemCopyBuffer = sb.ToString ();
	    }

		#endif
    }
}

