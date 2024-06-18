using UnityEngine;

public class PhysicsHelper : MonoBehaviour
{
    public float timeScale = 1;
    public bool enablePhysics = false;

    [ContextMenu("Set Time Scale")]
    public void SetTimeScale()
    {
        timeScale = Mathf.Max (0, timeScale);
        
        Time.timeScale = timeScale;
    }


    [ContextMenu("Toggle Physics Simulation")]
    public void TogglePhysics()
    {
        enablePhysics = !enablePhysics;
        Physics.autoSimulation = enablePhysics;
    }

}