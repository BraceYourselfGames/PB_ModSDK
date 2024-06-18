using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class TimeTrackingSystem : MonoBehaviour
{
    public struct TimeSnapshot
    {
        public float timeRealtime;
        public float timeRecorded;
    }

    public List<TimeSnapshot> snapshotsStandardScaled;
    public List<TimeSnapshot> snapshotsStandardUnscaled;
    public List<TimeSnapshot> snapshotsFixed;
    public List<TimeSnapshot> snapshotsCustomAccumulated;
    public List<TimeSnapshot> snapshotsCustomDirect;
    
    public float graphScaleRealtime = 1f;
    public float graphScaleRecorded = 1f;
    public int graphStepLimit = 500;
    
    public float timeStandardScaled;
    public float timeStandardUnscaled;
    public float timeFixed;
    public float timeCustomAccumulated;
    public float timeCustomDirect;
    
    public Color colorStandardScaled = Color.red;
    public Color colorStandardUnscaled = Color.Lerp (Color.red, Color.yellow, 0.5f);
    public Color colorFixed = Color.green;
    public Color colorCustomAccumulated = Color.cyan;
    public Color colorCustomDirect = Color.Lerp (Color.cyan, Color.blue, 0.5f);

    public const string keyStandardScaled = "Standard/scaled";
    public const string keyStandardUnscaled = "Standard/unscaled";
    public const string keyFixed = "Fixed";
    public const string keyCustomAccumulated = "Custom/accumulated";
    public const string keyCustomDirect = "Custom/direct";
    
    private float timeCustomAtStart = 0;
    private float timeRealtimeAtStart = 0;
    private int graphStep = 0;
    
    private void Awake ()
    {
        snapshotsStandardScaled = new List<TimeSnapshot> (500);
        snapshotsStandardUnscaled = new List<TimeSnapshot> (500);
        snapshotsFixed = new List<TimeSnapshot> (500);
        snapshotsCustomAccumulated = new List<TimeSnapshot> (500);
        snapshotsCustomDirect = new List<TimeSnapshot> (500);
        
        Reset ();
    }

    public void Update ()
    {
        timeStandardScaled += Time.deltaTime;
        timeStandardUnscaled += Time.unscaledDeltaTime;
        timeCustomAccumulated += TimeCustom.unscaledDeltaTime;
        timeCustomDirect = TimeCustom.unscaledTime - timeCustomAtStart;
        
        float tr = Time.realtimeSinceStartup - timeRealtimeAtStart;
        
        snapshotsStandardScaled.Add (new TimeSnapshot { timeRealtime = tr, timeRecorded = timeStandardScaled });
        snapshotsStandardUnscaled.Add (new TimeSnapshot { timeRealtime = tr, timeRecorded = timeStandardUnscaled });
        snapshotsCustomAccumulated.Add (new TimeSnapshot { timeRealtime = tr, timeRecorded = timeCustomAccumulated });
        snapshotsCustomDirect.Add (new TimeSnapshot { timeRealtime = tr, timeRecorded = timeCustomDirect });

        Draw (snapshotsStandardScaled, colorStandardScaled);
        Draw (snapshotsStandardUnscaled, colorStandardUnscaled);
        Draw (snapshotsFixed, colorFixed);
        Draw (snapshotsCustomAccumulated, colorCustomAccumulated);
        Draw (snapshotsCustomDirect, colorCustomDirect);

        graphStep += 1;
        if (graphStep > graphStepLimit)
        {
            Reset ();
        }
    }

    public void FixedUpdate ()
    {
        timeFixed += Time.fixedDeltaTime;
        float tr = Time.realtimeSinceStartup - timeRealtimeAtStart;
        
        snapshotsFixed.Add (new TimeSnapshot { timeRealtime = tr, timeRecorded = timeFixed });
    }

    public void Draw (List<TimeSnapshot> snapshots, Color color)
    {
        if (snapshots.Count <= 1)
            return;

        int count = snapshots.Count;
        for (int i = 1; i < count; ++i)
        {
            var prev = snapshots[i - 1];
            var curr = snapshots[i];
            var from = new Vector3 (0f, prev.timeRecorded * graphScaleRecorded, prev.timeRealtime * graphScaleRealtime) + transform.position;
            var to = new Vector3 (0f, curr.timeRecorded * graphScaleRecorded, curr.timeRealtime * graphScaleRealtime) + transform.position;
            Debug.DrawLine (from, to, color);
        }
    }

    [Button ("Reset", ButtonSizes.Large)]
    public void Reset ()
    {
        snapshotsStandardScaled.Clear ();
        snapshotsStandardUnscaled.Clear ();
        snapshotsFixed.Clear ();
        snapshotsCustomAccumulated.Clear ();
        snapshotsCustomDirect.Clear ();
        
        timeStandardScaled = 0f;
        timeStandardUnscaled = 0f;
        timeFixed = 0f;
        timeCustomAccumulated = 0f;
        
        graphStep = 0;
        timeRealtimeAtStart = Time.realtimeSinceStartup;
        timeCustomAtStart = TimeCustom.unscaledTime;
    }
}
