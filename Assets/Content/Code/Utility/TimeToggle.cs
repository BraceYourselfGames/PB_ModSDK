using UnityEngine;

public class TimeToggle : MonoBehaviour
{
    public bool setOnStart = false;

    [Range (0.001f, 0.5f)]
    public float timeToUse = 0.05f;

    public bool useConstantFramerate = false;
    public int constantFramerate = 30;

    private void Awake ()
    {
        if (!Application.isPlaying)
            return;

        if (useConstantFramerate)
        {
            Time.captureFramerate = constantFramerate;
        }
    }

    private void Start ()
    {
        if (!Application.isPlaying)
            return;

        if (setOnStart)
        {
            Time.timeScale = timeToUse;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }

    }

    // Update is called once per frame
    void Update ()
    {
        if (!Application.isPlaying)
            return;

        if (Input.GetKeyDown (KeyCode.Space))
        {
            if (Time.timeScale == 1f)
            {
                Time.timeScale = timeToUse;
            }
            else
            {
                Time.timeScale = 1f;
            }

            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
	}
}
