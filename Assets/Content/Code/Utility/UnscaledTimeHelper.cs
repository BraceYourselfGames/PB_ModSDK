using UnityEngine;

public static class TimeCustom
{
    /// <summary>
    /// Unscaled time while playing, without pauses/focus loss
    /// </summary>
    public static float unscaledTime
    {
        get
        {
            if (!initialized)
                Initialize ();
            return unscaledTime_;
        }
    }

    /// <summary>
    /// Unscaled delta while playing, without pauses/focus loss
    /// </summary>
    public static float unscaledDeltaTime
    {
        get
        {
            if (!initialized)
                Initialize ();
            return unscaledDeltaTime_;
        }
    }

    /// <summary>
    /// Time lost out of focus
    /// </summary>
    public static float focusLossTime
    {
        get
        {
            if (!initialized)
                Initialize ();
            return focusLossTime_;
        }
    }

    private static bool initialized = false;
    private static float unscaledTime_ = 0f;
    private static float unscaledDeltaTime_ = 0f;
    private static float focusLossTime_ = 0f;

    private static void Initialize ()
    {
        initialized = true;

        if (UnscaledTimeHelper.ins != null)
            return;

        var go = new GameObject ();
        go.AddComponent<UnscaledTimeHelper> ();
        Debug.Log ("Creating the custom time helper object", go);
    }

    public static void SetUnscaledTime (float unscaledTimeNew, float unscaledDeltaTimeNew)
    {
        unscaledTime_ = unscaledTimeNew;
        unscaledDeltaTime_ = unscaledDeltaTimeNew;
    }

    public static void SetFocusLossTime (float focusLossTimeNew)
    {
        focusLossTime_ = focusLossTimeNew;
    }
}

public class UnscaledTimeHelper : MonoBehaviour
{
    public static UnscaledTimeHelper ins;

    public bool systemTimeMode = false;
    public bool log = false;
    public float systemDeltaRejectionThreshold = 0.25f;

    [Range (0.01f, 0.5f)]
    public float systemTransitionThreshold = 0.1f;

    private float realtimeLast = 0f;
    private float realtimeAtFocusLoss = 0f;
    private float realtimeDelta = 0f;
    private float realtimeDeltaFallback = 1f / 30f;

    private float systemDeltaAverage = 1f / 60f;
    private int systemDeltaHistorySize = 5;
    private int systemDeltaHistoryIndex = 0;
    private float[] systemDeltaHistory;


    private void Awake ()
    {
        if (ins != null)
        {
            Destroy (gameObject);
            return;
        }

        ins = this;
        
        if (transform.parent == null)
            DontDestroyOnLoad (gameObject);
        
        gameObject.name = "~UnscaledTimeHelper";

        TimeCustom.SetUnscaledTime (0f, systemDeltaAverage);
        TimeCustom.SetFocusLossTime (0f);

        systemDeltaHistorySize = Mathf.Clamp (systemDeltaHistorySize, 3, 10);
        systemDeltaHistoryIndex = 0;
        systemDeltaHistory = new float[systemDeltaHistorySize];
        for (int i = 0; i < systemDeltaHistorySize; ++i)
            systemDeltaHistory[i] = systemDeltaAverage;

        if (!systemTimeMode)
        {
            if (log)
                Debug.LogWarning ("Experimental mode enabled on TimeCustom service, using inversion of time scaling instead of true real time");
        }
        else
        {
            Debug.LogError ("Warning! System time mode enabled on TimeCustom service, which could lead to inconsistent animation timings and more issues!");
        }
    }

    private void Update ()
    {
        float systemDelta = Time.unscaledDeltaTime;
        float systemDeltaDifference = Mathf.Abs (systemDelta - systemDeltaAverage);
        if (systemDeltaDifference > systemDeltaRejectionThreshold)
        {
            if (log)
                Debug.LogWarning ($"F: {Time.frameCount} | Rejected unscaled delta from system time with value of {systemDelta}, which was over threshold of {systemDeltaRejectionThreshold}");
        }
        else
        {
            systemDeltaHistory[systemDeltaHistoryIndex] = systemDelta;
            systemDeltaHistoryIndex += 1;
            if (systemDeltaHistoryIndex >= systemDeltaHistorySize)
                systemDeltaHistoryIndex = 0;

            systemDeltaAverage = 0f;
            for (int i = 0; i < systemDeltaHistorySize; ++i)
                systemDeltaAverage += systemDeltaHistory[i];
            systemDeltaAverage /= systemDeltaHistorySize;

            if (log)
                Debug.Log ($"F: {Time.frameCount} | System delta averaged to {systemDeltaAverage} | Last system delta: {systemDelta}");
        }

        if (!systemTimeMode)
        {
            float scaleBasedMultiplier = 1f / Mathf.Clamp (systemTransitionThreshold, 0.01f, 0.5f);
            float scaleBasedInterpolant = Mathf.Clamp01 (Time.timeScale * scaleBasedMultiplier);
            float unscaledDeltaSafe = Time.timeScale.RoughlyEqual (0f) ? Time.unscaledDeltaTime : Time.deltaTime / Time.timeScale;
            float hybridDelta = Mathf.Lerp (systemDeltaAverage, unscaledDeltaSafe, scaleBasedInterpolant);

            if (log)
                Debug.Log ($"F: {Time.frameCount} | Hybrid delta: {hybridDelta} | Derived from system delta average {systemDeltaAverage} and unscaled delta {unscaledDeltaSafe} with interpolant {scaleBasedInterpolant} | DT: {Time.deltaTime} | TS: {Time.timeScale}");
            TimeCustom.SetUnscaledTime (TimeCustom.unscaledTime + hybridDelta, hybridDelta);
        }
        else
        {
            realtimeDelta = Mathf.Min (Time.realtimeSinceStartup - realtimeLast, realtimeDeltaFallback);
            realtimeLast = Time.realtimeSinceStartup;
            TimeCustom.SetUnscaledTime (realtimeLast - TimeCustom.focusLossTime, realtimeDelta);
        }
    }

    private void OnApplicationFocus (bool focus)
    {
        if (focus)
        {
            float outOfFocusTime = Time.realtimeSinceStartup - realtimeAtFocusLoss;
            TimeCustom.SetFocusLossTime (TimeCustom.focusLossTime + outOfFocusTime);
            //Debug.Log ($"Restoring focus to the application, application was out of focus for {outOfFocusTime}, custom unscaled time now {TimeCustom.focusLossTime} behind standard time");
        }
        else
        {
            realtimeAtFocusLoss = Time.realtimeSinceStartup;
        }
    }
}