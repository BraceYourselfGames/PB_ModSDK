using System;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class FXTween : MonoBehaviour
{
    public static MaterialPropertyBlock mpb;
    
    [OnInspectorGUI ("UpdateInspector")]
    [PropertyRange (0.1f, 1f)]
    public float playbackSpeed = 1f;
    
    [NonSerialized]
    public bool playing = false;
    
    [NonSerialized]
    public float timePlaying = 0f;
    
    [NonSerialized]
    private float timePlayingLast = 0f;
    
    public bool playOnEnable = false;
    public bool clearOnReset = false;
    public bool ignoreRepeatedValues = false;
    
    [ShowInInspector]
    public static bool showInactiveBlocks = true;

    public bool replayAnimation = false;
    public bool manualAnimation = false;

    [NonSerialized, ShowInInspector, ReadOnly, BoxGroup]
    public bool samplingFromEditor = false;

    [NonSerialized, ShowInInspector, ReadOnly, BoxGroup]
    public bool samplingSafetyCheckOverride = false;
    
    [NonSerialized, ShowInInspector, ReadOnly, BoxGroup]
    [PropertyRange (0f, "samplingTimeLimit")]
    public float samplingTime = 0f;
    
    [NonSerialized, ShowInInspector, ReadOnly, BoxGroup]
    public float samplingTimeLimit = 1f;
    
    [NonSerialized, ShowInInspector, ReadOnly, BoxGroup]
    private float samplingTimeLast = -1f;
    
    [NonSerialized, ShowInInspector, ReadOnly, BoxGroup]
    private bool samplingApplied = false;
    
    [NonSerialized, ShowInInspector, ReadOnly, BoxGroup]
    private float timeNormalizedLast = 0f;

    [NonSerialized, ShowInInspector, ReadOnly, BoxGroup]
    private bool initialized = false;

    [NonSerialized, ShowInInspector, ReadOnly, BoxGroup]
    private float timeRealtimeLast;

    [HideReferenceObjectPicker, DisableContextMenu, HideLabel]
    public FXSystem.Step step = new FXSystem.Step ();
    

    
    
    
    public void Awake ()
    {
        CheckInitialization ();
    }

    private bool IsPlaybackDangerous ()
    {
        if (this == null || gameObject == null)
            return true;
        
        #if UNITY_EDITOR
            // Optional override to preview playback inside prefab editing mode, at user's own risk
            if (samplingSafetyCheckOverride)
                return false;

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
        step.parentSystem = null;
        step.parentTween = this;
        step.Initialize (null);
    }

    public void OnEnable ()
    {
        CheckInitialization ();
        if (Application.isPlaying)
        {
            if (playOnEnable)
                Play ();
        }

        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorApplication.update -= OnEditorUpdate;
            UnityEditor.EditorApplication.update += OnEditorUpdate;
        }
        #endif
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
            
            if (step.state == FXSystem.Step.StepState.Playing)
                step.Animate (timePlaying);
        }
        
        #if UNITY_EDITOR
        
        if (samplingFromEditor)
        {
            if (samplingTime != samplingTimeLast)
            {
                CheckInitialization ();
                samplingApplied = true;
                samplingTimeLast = samplingTime;

                // samplingTime needs to be normalized for the AnimateDirectly method
                float samplingTimNormalized = Mathf.InverseLerp (0f, samplingTimeLimit, samplingTime);
                step.AnimateDirectly (samplingTimNormalized);

                UnityEditor.SceneView.RepaintAll ();
            }
        }
        else if (samplingApplied)
        {
            // Debug.Log ("Cleaning up after sampling");
            samplingApplied = false;
            samplingTimeLast = -1f;
            step.Finish ();
        }
        
        #endif
    }

    private void OnDestroy ()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.update -= OnEditorUpdate;
        #endif
    }
    
    
    
    
    public void StopAll ()
    {
        if (IsPlaybackDangerous ())
            return;
        
        step.Finish ();
    }

    public void Play ()
    {
        if (IsPlaybackDangerous ())
            return;
        
        samplingFromEditor = false;
        CheckInitialization ();
        StopAll ();
        step.BeginWait ();
        step.Play ();
        timeNormalizedLast = -1f;

        if (!manualAnimation)
        {
            playing = true;
            timePlaying = 0f;
            timeRealtimeLast = Time.realtimeSinceStartup;
        }
    }
    
    public void Animate (float timeNormalized, bool useSafetyChecks = true)
    {
        if (IsPlaybackDangerous ())
            return;

        if (useSafetyChecks)
        {
            if (!manualAnimation)
                return;
            
            if (ignoreRepeatedValues && timeNormalized == timeNormalizedLast)
                return;
        }

        timeNormalizedLast = timeNormalized;
        step.AnimateDirectly (timeNormalized);
    }
    
    #if UNITY_EDITOR

    private void UpdateInspector ()
    {
        samplingTimeLimit = step.duration;
        step.UpdateInspectorProperties ();
    }

    #endif
}
