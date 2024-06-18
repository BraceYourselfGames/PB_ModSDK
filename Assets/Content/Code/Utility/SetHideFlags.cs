using UnityEngine;
using System.Collections.Generic;

public class SetHideFlags : MonoBehaviour
{
    [System.Serializable]
    public class ObjectFlagPair
    {
        public Transform gameObject;
        public HideFlags flag;
    }

    public List<ObjectFlagPair> pairs = new List<ObjectFlagPair> ();

    private void OnEnable ()
    {
        Apply ();
    }

    [ContextMenu ("Apply")]
    public void Apply ()
    {
        for (int i = 0; i < pairs.Count; ++i)
        {
            if (pairs[i].gameObject == null)
                continue;

            pairs[i].gameObject.hideFlags = pairs[i].flag;
        }
    }
}
