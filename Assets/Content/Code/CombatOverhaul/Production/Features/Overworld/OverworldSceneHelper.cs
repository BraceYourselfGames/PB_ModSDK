using System;
using System.Collections.Generic;
using System.Text;
using CustomRendering;
using Knife.DeferredDecals;
using PhantomBrigade;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;


[ExecuteInEditMode]
public class OverworldSceneHelper : MonoBehaviour
{
    public static OverworldSceneHelper ins;
    public OverworldLandscapeManager landscapeManager;

    [NonSerialized]
    private bool activeLast;

    private void Awake ()
    {
        ins = this;
        // SetActive (false);
    }

    private void OnEnable ()
    {
        if (!Application.isPlaying)
        {
            ins = this;
        }
    }

    public static bool IsActive ()
    {
        if (!Application.isPlaying || ins == null)
            return false;
        return ins.activeLast;
    }
    
    #if UNITY_EDITOR

    [Button ("Activate"), PropertyOrder (-1), HideIf ("activeLast"), HideInEditorMode]
    private void SetActiveTrue () => 
        SetActive (true);
    
    [Button ("Deactivate"), PropertyOrder (-1), ShowIf ("activeLast"), HideInEditorMode]
    private void SetActiveFalse () => 
        SetActive (false);
    
    #endif
    
    public static void SetActive (bool active)
    {
        if (!Application.isPlaying || ins == null)
            return;

        ins.SetActiveDirectly (active);
    }

    public void SetActiveDirectly (bool active)
    {
        // Debug.LogWarning ($"Setting overworld scene {(active ? "active" : "hidden")}");
        activeLast = active;
        
        if (landscapeManager != null)
            landscapeManager.gameObject.SetActive (active);
        else if (OverworldLandscapeManager.ins != null)
            OverworldLandscapeManager.ins.gameObject.SetActive (active);

        if (Application.isPlaying)
        {
            // propHolder.gameObject.SetActive (active);
            // SetOverworldVFXActive(active);
            // overworldUI.SetActive(active);
            
            if (active)
                ECSRenderingBatcher.SubmitBatches ();
        }
    }
}
