using System;
using System.Collections;
using UnityEngine;

public static class Co
{
    private static CoRunner instantInstance_ = null;
    private static CoRunner permanentInstance_ = null;

    public static CoRunner InstantRunner
    {
        get
        {
            if (instantInstance_ != null)
            {
                return instantInstance_;
            }
            else
            {
                GameObject go = new GameObject ("[CoRunner.Instant]");
                instantInstance_ = go.AddComponent<CoRunner> ();
                return instantInstance_;
            }
        }
    }

    public static CoRunner PermanentRunner
    {
        get
        {
            if (permanentInstance_ != null)
            {
                return permanentInstance_;
            }
            else
            {
                GameObject go = new GameObject ("[CoRunner.Permanent]");
                GameObject.DontDestroyOnLoad (go);
                permanentInstance_ = go.AddComponent<CoRunner> ();

                return permanentInstance_;
            }
        }
    }

    #region Shortcuts

    public static Coroutine Run (IEnumerator enumerator)
    {
        return InstantRunner.Run (enumerator);
    }

    public static Coroutine Delay (float delay, Action action)
    {
        return InstantRunner.Delay (delay, action);
    }
    
    public static Coroutine Delay (float delay, Action<object> action, object argument)
    {
        return InstantRunner.Delay (delay, action, argument);
    }
    
    public static Coroutine DelayScaled (float delay, Action action)
    {
        return InstantRunner.DelayScaled (delay, action);
    }
    
    public static Coroutine DelayScaled (float delay, Action<object> action, object argument)
    {
        return InstantRunner.DelayScaled (delay, action, argument);
    }
    
    public static Coroutine DelayFrames (int frames, Action action)
    {
        return InstantRunner.DelayFrames (frames, action);
    }
    
    public static Coroutine DelayFrames (int frames, Action<object> action, object argument)
    {
        return InstantRunner.DelayFrames (frames, action, argument);
    }

    public static void Stop (Coroutine coroutine)
    {
        InstantRunner.Stop (coroutine);
    }
    
    public static void StopAndClear (ref Coroutine coroutine)
    {
        InstantRunner.Stop (coroutine);
        coroutine = null;
    }

    public static void StopAll ()
    {
        InstantRunner.StopAll ();
    }

    #endregion Shortcuts
}

public sealed class CoRunner : MonoBehaviour
{
    public Coroutine Run (IEnumerator enumerator)
    {
        return StartCoroutine (enumerator);
    }

    public Coroutine Delay (float delay, Action action)
    {
        return Run (CoDelay (delay, action));
    }
    
    public Coroutine Delay (float delay, Action<object> action, object argument)
    {
        return Run (CoDelay (delay, action, argument));
    }
    
    public Coroutine DelayScaled (float delay, Action action)
    {
        return Run (CoDelayScaled (delay, action));
    }
    
    public Coroutine DelayScaled (float delay, Action<object> action, object argument)
    {
        return Run (CoDelayScaled (delay, action, argument));
    }
    
    public Coroutine DelayFrames (int frames, Action action)
    {
        return Run (CoDelayFrames (frames, action));
    }
    
    public Coroutine DelayFrames (int frames, Action<object> action, object argument)
    {
        return Run (CoDelayFrames (frames, action, argument));
    }
    
    

    private IEnumerator CoDelay (float delay, Action action)
    {
        yield return new WaitForSecondsRealtimeCustom (delay);
        if (action != null)
            action ();
    }
    
    private IEnumerator CoDelay (float delay, Action<object> action, object argument)
    {
        yield return new WaitForSecondsRealtimeCustom (delay);
        if (action != null)
            action (argument);
    }
    
    private IEnumerator CoDelayScaled (float delay, Action action)
    {
        yield return new WaitForSeconds (delay);
        if (action != null)
            action ();
    }
    
    private IEnumerator CoDelayScaled (float delay, Action<object> action, object argument)
    {
        yield return new WaitForSeconds (delay);
        if (action != null)
            action (argument);
    }
    
    private IEnumerator CoDelayFrames (int frames, Action action)
    {
        frames = Mathf.Clamp (frames, 1, 100);
        for (int i = 0; i < frames; ++i)
            yield return null;
        
        if (action != null)
            action ();
    }
    
    private IEnumerator CoDelayFrames (int frames, Action<object> action, object argument)
    {
        frames = Mathf.Clamp (frames, 1, 100);
        for (int i = 0; i < frames; ++i)
            yield return null;
        
        if (action != null)
            action (argument);
    }

    public void Stop (Coroutine coroutine)
    {
        if (coroutine == null)
            return;

        StopCoroutine (coroutine);
    }

    public void StopAll ()
    {
        StopAllCoroutines ();
    }

    public void OnDestroy ()
    {
        if (Application.isPlaying)
            Destroy (gameObject);
    }
}

public class WaitForSecondsRealtimeCustom : CustomYieldInstruction
{
    private float m_WaitUntilTime = -1f;

    /// <summary>
    ///   <para>Creates a yield instruction to wait for a given number of seconds using unscaled time.</para>
    /// </summary>
    /// <param name="time"></param>
    public WaitForSecondsRealtimeCustom (float time)
    {
        this.waitTime = time;
    }

    /// <summary>
    ///   <para>The given amount of seconds that the yield instruction will wait for.</para>
    /// </summary>
    public float waitTime { get; set; }

    public override bool keepWaiting
    {
        get
        {
            if ((double) this.m_WaitUntilTime < 0.0)
                this.m_WaitUntilTime = TimeCustom.unscaledTime + this.waitTime;
            bool flag = (double) TimeCustom.unscaledTime < (double) this.m_WaitUntilTime;
            if (!flag)
                this.m_WaitUntilTime = -1f;
            return flag;
        }
    }
    
    public override void Reset()
    {
        m_WaitUntilTime = -1f;
    }
}