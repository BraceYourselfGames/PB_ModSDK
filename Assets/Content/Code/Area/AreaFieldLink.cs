using System;
using PhantomBrigade;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

public class AreaFieldLink : MonoBehaviour
{
    public MeshRenderer mr;
    public Collider collider;

    public bool stateMode = false;
    
    [InlineButtonClear] 
    [ValueDropdown("@DataMultiLinkerAssetPools.data.Keys")]
    public string fxEntry;
    public float fxEntryScale = 5f;
    
    [InlineButtonClear]
    [ValueDropdown("@DataMultiLinkerAssetPools.data.Keys")]
    public string fxExit;
    public float fxExitScale = 5f;

    [NonSerialized, ShowInInspector]
    public string type;
    
    [NonSerialized, ShowInInspector]
    public string tag;
    
    public void OnTriggerEnter (Collider colliderOther)
    {
        
    }
    
    public void OnTriggerExit (Collider colliderOther)
    {
        
    }

    public void OnDisable ()
    {
        // Debug.LogWarning ($"Field {name} ({GetInstanceID ()}) disabled (possibly before destruction)");
    }
}
