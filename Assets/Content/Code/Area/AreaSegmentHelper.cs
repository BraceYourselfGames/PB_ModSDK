using System.Collections.Generic;
using CustomRendering;
using PhantomBrigade.Data;
using UnityEngine;

public class AreaSegmentHelper : MonoBehaviour
{
    public Transform holder;

    public void LoadSegments (List<DataBlockEnvironmentSegment> segments)
    {
        UtilityGameObjects.ClearChildren (holder);
        gameObject.SetActive (true);
        
        #if !PB_MODSDK
        if (segments == null)
            return;

        for (int i = 0, count = segments.Count; i < count; ++i)
        {
            var segment = segments[i];
            if (segment == null || segment.prefab == null)
            {
                // Debug.LogWarning ($"Skipping environment segment {i} that is null or has a null prefab reference");
                continue;
            }
                
            #if UNITY_EDITOR
            var segmentObject = UnityEditor.PrefabUtility.InstantiatePrefab (segment.prefab, holder) as GameObject;
            #else
            var segmentObject = GameObject.Instantiate (segment.prefab, holder) as GameObject;
            #endif
            
            if (segmentObject == null)
                continue;
            
            segmentObject.transform.SetLocalTransformationToZero ();
            segmentObject.name = segment.prefab.name;

            if (Application.isPlaying)
            {
                var childBatchLinkers = segmentObject.GetComponentsInChildren<ECSRendererBatchLinker> (true);
                foreach (var batchLinker in childBatchLinkers)
                    batchLinker.MarkDirty ();
            }
        }
        #endif
    }

    public void ClearSegments ()
    {
        gameObject.SetActive (false);
        UtilityGameObjects.ClearChildren (holder);
    }
}
