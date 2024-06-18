using UnityEngine;

public class UtilityCameraLinker : MonoBehaviour
{
    public static UtilityCameraLinker ins;
    private bool initialized = false;

    private void Setup ()
    {
        if (initialized)
            return;

        initialized = true;
        ins = this;
    }
    
    private void Update ()
    {
        if (!initialized)
            Setup ();
    }

    public static void OnNightfall (bool nightfall)
    {
        if (ins == null)
        {
            Debug.Log ($"Failed to apply nightfall status to shadows: camera linker instance is missing"); 
            return;
        }
        
        if (!ins.initialized)
            ins.Setup ();
    }

    public static void OnShadowDistanceChange (int distance, int frustumOffset)
    {
        QualitySettings.shadowDistance = distance;
    }
}
