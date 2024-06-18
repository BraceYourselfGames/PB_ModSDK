using PhantomBrigade;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

public class DataHelperProvince
{
	static Color outOfBoundsColor = new Color(1f, 0f, 0f);

    public static DataHelperProvince ins;
    private static Texture2D lookupTexture2D;
    private static bool initialized = false;

    private static void CheckInitialization ()
    {
        if (initialized)
            return;

        initialized = true;
        lookupTexture2D = Resources.Load<Texture2D> (DataLinkerSettingsProvinces.data.provinceLookupTextureAssetPath);
    }
    
    private void Awake ()
    {
        ins = this;
        CheckInitialization ();
    }

    private void Start ()
    {
        CheckInitialization ();
    }

    // WARNING This will read from the texture with a GetPixel call, it's quite expensive and should only be done once per frame, or as infrequently as possible
    public static bool GetProvinceColorAtPositionExpensive (Vector3 position, out Color result)
    {
	    CheckInitialization ();
        
	    var lookupTexture = lookupTexture2D;
	    if (lookupTexture == null || DataLinkerSettingsProvinces.data == null || DataLinkerSettingsProvinces.data.definitionsOfProvinces == null || DataLinkerSettingsProvinces.data.definitionsOfProvinces.Count == 0)
	    {
		    result = Color.black;
		    return false;
	    }
        
	    var offsetPosition = position;
	    var worldOffset = DataLinkerSettingsProvinces.data.worldOffset;
	    var worldSize = DataLinkerSettingsProvinces.data.worldSize;
        
	    offsetPosition.x += worldOffset.x;
	    offsetPosition.z += worldOffset.z;

	    var x = Mathf.FloorToInt ((offsetPosition.x / worldSize.x) * lookupTexture.width);
	    var z = Mathf.FloorToInt ((offsetPosition.z / worldSize.z) * lookupTexture.height);

	    result = lookupTexture.GetPixel (x, z);
        return true;
    }

    [Button]
    // WARNING This will read from the texture with a GetPixel call, it's quite expensive and should only be done once per frame, or as infrequently as possible
    // Use GetProvinceKeyAtEntity instead
    public static string GetProvinceKeyAtPositionExpensive (Vector3 position)
    {
	    if(!GetProvinceColorAtPositionExpensive(position, out var color))    
			return null;

        var hsb = new HSBColor (color);
        var hue = hsb.h * 360;

        var closestProvinceKey = string.Empty;
        var closestDifference = Mathf.Infinity;

        foreach (var provinceData in DataLinkerSettingsProvinces.data.definitionsOfProvinces)
        {
            var provinceColour = provinceData.Value;
            var difference = Mathf.Abs (hue - provinceColour);

            if (closestDifference < difference)
            {
                continue;
            }

            closestDifference = difference;
            closestProvinceKey = provinceData.Key;
        }

        // Debug.Log (closestProvinceKey);

        return closestProvinceKey;
    }

    public static bool IsPositionInBounds(Vector3 position)
    {
	    if(!GetProvinceColorAtPositionExpensive(position, out var color))    
		    return false;

		return color != outOfBoundsColor;
    }
}
