using UnityEngine;

public static class Constants
{
    public static bool debugSaving = false;

    public static uint gridSize = 3;

    //If you change the layers you have to change it here too! 8 is the layer in the tags and layers manager for environment
    public const int EnvironmentLayer = 8;
    #if PB_MODSDK
    public const int volumeCollidersLayer = 11;
    #endif
    public const int debrisLayer = 21;
    public const int propLayer = 24;
    public const int tileMask = 1 << 17;


    public static Vector4 defaultHSBOffset = new Vector4 (0f, 0.5f, 0.5f, 0f);
}
