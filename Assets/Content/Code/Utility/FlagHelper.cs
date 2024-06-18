using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class FlagHelper : MonoBehaviour
{
    public bool editable = true;
    public List<GameObject> targets;

    [Button ("Update flags")]
    public void UpdateFlags ()
    {
        for (int i = 0; i < targets.Count; ++i)
        {
            if (targets[i] == gameObject)
            {
                Debug.LogWarning ("Unable to set flags on the host object, this can potentially permanently lock you out of editing this object");
                continue;
            }

            targets[i].hideFlags = editable ? HideFlags.None : HideFlags.NotEditable;
        }
    }
}
