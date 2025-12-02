using System.Collections.Generic;
using CustomRendering;
using PhantomBrigade.Data;
using UnityEngine;
using Area;

public static class AssetPackageHelper
{
    public static string levelAssetURL = "https://cdn.braceyourselfgames.com/PB/PB_ModSDK_AssetPackage_V20A.unitypackage";
    public static string levelAssetURLCaption = "Download asset package";
        
    public static string levelAssetWarning = "Level previews are not available: assets (tilesets and/or props) not found. Ensure you have the asset package installed.";
    public static string levelAssetTilesetsWarning = "Level previews are not available: tileset assets not found. Ensure you have the asset package installed: these assets are not included in the core Mod SDK repository.\n\nVerify the following folders:\n- Assets/Resources/Content/Objects/Tilesets\n- Configs/Tilesets";
    public static string levelAssetPropsWarning = "Level previews are not available: prop assets not found. Ensure you have the asset package installed: these assets are not included in the core Mod SDK repository.\n\nVerify the following folder:\n- Assets/Resources/Content/Props";
        
    public static bool AreLevelAssetsInstalled ()
    {
        if (!ResourceDatabaseManager.IsDatabaseAvailable ())
            return false;
        
        AreaTilesetHelper.CheckResources ();
        AreaAssetHelper.CheckResources ();

        return AreaTilesetHelper.AreAssetsPresent () && AreaAssetHelper.AreAssetsPresent ();
    }
    
    public static string unitAssetWarning = "Unit previews are not available: assets not found. Ensure you have the asset package installed.";

    public static bool AreUnitAssetsInstalled ()
    {
        if (!ResourceDatabaseManager.IsDatabaseAvailable ())
            return false;
        
        ItemHelper.CheckDatabase ();
        return ItemHelper.AreAssetsPresent ();
    }
    
    public static bool AreTextureAssetsInstalled ()
    {
        return TextureManager.AreAssetsPresent ();
    }
}