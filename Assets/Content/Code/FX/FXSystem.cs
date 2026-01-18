using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public partial class FXSystem : MonoBehaviour
{
    public static MaterialPropertyBlock mpb;
    
    [InfoBox("$playbackReport", InfoMessageType.None)]
    [OnInspectorGUI ("UpdateInspector")]
    [PropertyRange (0.1f, 1f)]
    public float playbackSpeed = 1f;
    
    [NonSerialized]
    public bool playing = false;
    
    [NonSerialized]
    public float timePlaying = 0f;
    
    [NonSerialized]
    private float timePlayingLast = 0f;
    
    [NonSerialized]
    public int stepsFinished = 0;

    public bool playOnEnable = false;
    public bool clearOnReset = false;
    
    [ShowInInspector]
    public static bool showInactiveBlocks = true;
    
    [ShowInInspector]
    public static bool showCurveStats = false;
    
    [HideInInspector]
    public bool sampling = false;
    
    [PropertyRange (0f, "samplingTimeLimit")][HideInInspector]
    public float samplingTime = 1f;
    
    [NonSerialized]
    public float samplingTimeLimit = 1f;
    
    [NonSerialized]
    private float samplingTimeLast = -1f;
    
    [NonSerialized]
    private bool samplingApplied = false;



    [NonSerialized]
    private bool initialized = false;

    [NonSerialized]
    private float timeRealtimeLast;
    
    [ListDrawerSettings (DefaultExpandedState = true, NumberOfItemsPerPage = 1, ShowIndexLabels = true)]
    public List<Step> steps = new List<Step> ();
    
    
    
    
    #if UNITY_EDITOR

    [NonSerialized]
    private string playbackReport = string.Empty;

    [NonSerialized]
    private bool playbackReportEmpty = false;

    [NonSerialized]
    private StringBuilder playbackBuilder = new StringBuilder ();
    
    private void UpdateInspector ()
    {
        samplingTimeLimit = 0f;
        bool danger = IsPlaybackDangerous ();
        if (!danger && steps != null && steps.Count > 0)
        {
            playbackBuilder.Clear ();
            for (int i = 0; i < steps.Count; ++i)
            {
                var step = steps[i];
                if (i > 0)
                    playbackBuilder.Append ("\n");
                playbackBuilder.Append (i);
                playbackBuilder.Append (": ");
                playbackBuilder.Append (step.name);
                playbackBuilder.Append (" | ");
                playbackBuilder.Append (step.state);
                playbackBuilder.Append (" | ");
                playbackBuilder.Append (Mathf.RoundToInt (step.timeNormalized * 100f));
                playbackBuilder.Append ("%");

                step.UpdateInspectorProperties ();
                var end = step.startTime + step.duration;
                if (samplingTimeLimit < end)
                    samplingTimeLimit = end;
            }
            
            playbackReport = playbackBuilder.ToString ();
            playbackReportEmpty = false;
        }
        else if (!playbackReportEmpty)
        {
            if (danger)
                playbackReport = "Playback and stat display is not supported in prefab stage or on prefab instances";
            else
                playbackReport = "Add steps to the system to see report on status of the steps here";
            playbackReportEmpty = true;
        }
    }
    
    #endif

    
    
    
    public void Awake ()
    {
        CheckInitialization ();
    }

    private bool IsPlaybackDangerous ()
    {
        if (gameObject == null)
            return true;
        
        #if UNITY_EDITOR
            var mainStage = UnityEditor.SceneManagement.StageUtility.GetMainStageHandle ();
            var currentStage = UnityEditor.SceneManagement.StageUtility.GetStageHandle (gameObject);
            if (currentStage == mainStage)
            {
                // return PrefabUtility.IsPartOfPrefabInstance (gameObject) || PrefabUtility.IsPartOfPrefabAsset (gameObject);
                return PrefabUtility.IsPartOfPrefabAsset (gameObject);
            }
            else
                return true;
        #else
            return false;
        #endif
    }
    
    private bool IsSamplingUndesirable ()
    {
        if (gameObject == null)
            return true;
        
        #if UNITY_EDITOR
            var mainStage = UnityEditor.SceneManagement.StageUtility.GetMainStageHandle ();
            var currentStage = UnityEditor.SceneManagement.StageUtility.GetStageHandle (gameObject);
            if (currentStage == mainStage)
                return PrefabUtility.IsPartOfPrefabInstance (gameObject) || PrefabUtility.IsPartOfPrefabAsset (gameObject);
            else
                return true;
        #else
            return false;
        #endif
    }

    public void CheckInitialization ()
    {
        if (initialized)
            return;

        initialized = true;

        if (IsPlaybackDangerous ())
            return;

        for (int i = 0; i < steps.Count; ++i)
        {
            var step = steps[i];
            step.parentSystem = this;
            step.parentTween = null;
            step.Initialize (OnStepComplete);
        }
    }

    public void OnEnable ()
    {
        CheckInitialization ();
        if (Application.isPlaying)
        {
            if (playOnEnable && !playing)
                Play ();
        }
        
        #if UNITY_EDITOR
        else
        {
            UnityEditor.EditorApplication.update -= OnEditorUpdate;
            UnityEditor.EditorApplication.update += OnEditorUpdate;
        }
        #endif
    }

    public void OnDisable ()
    {
        if (Application.isPlaying)
        {
            if (playOnEnable && playing)
                StopAll ();
        }
    }

    public void Update ()
    {
        if (Application.isPlaying)
            OnSafeUpdate ();
    }

    private void OnEditorUpdate ()
    {
        #if UNITY_EDITOR
        if (!Application.isPlaying)
            OnSafeUpdate ();
        #endif
    }

    private void OnSafeUpdate ()
    {
        if (IsPlaybackDangerous ())
            return;

        if (playing)
        {
            if (Application.isPlaying)
                timePlaying += Time.deltaTime * playbackSpeed;
            else
            {
                timePlaying += (Time.realtimeSinceStartup - timeRealtimeLast) * playbackSpeed;
                timeRealtimeLast = Time.realtimeSinceStartup;
            }
        
            for (int i = 0; i < steps.Count; ++i)
            {
                var step = steps[i];
                if (step.state == Step.StepState.Playing)
                    step.Animate (timePlaying);
                else if (step.state == Step.StepState.Waiting && timePlaying > step.startTime)
                    step.Play ();
            }
        }
        
        #if UNITY_EDITOR
        
        else if (sampling)
        {
            if (samplingTime != samplingTimeLast)
            {
                CheckInitialization ();
                samplingApplied = true;
                samplingTimeLast = samplingTime;

                for (int i = 0; i < steps.Count; ++i)
                {
                    var step = steps[i];
                    var end = step.startTime + step.duration;
                    if (samplingTime >= step.startTime)
                    {
                        if (samplingTime < end)
                        {
                            if (step.state != Step.StepState.Playing)
                                step.Play ();
                            step.Animate (samplingTime);
                        }
                        else if (step.state != Step.StepState.Finished)
                            step.Finish ();
                    }
                    else if (step.state != Step.StepState.Waiting)
                        step.BeginWait ();
                }
                
                UnityEditor.SceneView.RepaintAll ();
            }
        }
        else if (samplingApplied)
        {
            Debug.Log ("Cleaning up after sampling");
            samplingApplied = false;
            samplingTimeLast = -1f;
            
            for (int i = 0; i < steps.Count; ++i)
                steps[i].Finish (0f);
        }
        
        #endif
    }

    public void Animate (float timeNormalized, int stepIndex = 0)
    {
        if (steps == null || !stepIndex.IsValidIndex (steps))
            return;

        var step = steps[stepIndex];
        step?.Animate (timeNormalized, true);
    }

    [Button ("Stop at 0"), HideInEditorMode, ButtonGroup ("Test")]
    private void StopAllAt0 () => StopAll (0f);
    
    
    [Button ("Stop at 1"), HideInEditorMode, ButtonGroup ("Test")]
    private void StopAllAt1 () => StopAll (1f);
    
    public void StopAll (float timeNormalizedForced = 1f)
    {
        if (IsPlaybackDangerous ())
            return;
        
        if (timeNormalizedForced.RoughlyEqual (0f))
        {
            for (int i = steps.Count - 1; i >= 0; --i)
                steps[i].Finish (0f);
        }
        else
        {
            for (int i = 0; i < steps.Count; ++i)
                steps[i].Finish (timeNormalizedForced);
        }
    }
    
    public void Play ()
    {
        if (steps.Count == 0 || IsPlaybackDangerous ())
            return;

        sampling = false;
        CheckInitialization ();
        StopAll ();

        for (int i = 0; i < steps.Count; ++i)
            steps[i].BeginWait ();

        playing = true;
        stepsFinished = 0;
        timePlaying = 0f;
        timeRealtimeLast = Time.realtimeSinceStartup;
    }

    private void OnStepComplete ()
    {
        if (!playing)
            return;
        
        stepsFinished += 1; 
        for (int i = 0; i < steps.Count; ++i)
        {
            if (steps[i].state != Step.StepState.Finished)
                return;
        }
        
        playing = false;
        StopAll ();
    }

    public bool IsPlaying ()
    {
        return playing;
    }

    private void OnDestroy ()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.update -= OnEditorUpdate;
        #endif
    }
}
