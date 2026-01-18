using System;
using UnityEngine;

#if UNITY_EDITOR
#endif

public partial class FXSystem : MonoBehaviour
{
    [Serializable]
    public class StepSystemPlayback : StepBlock
    {
        public bool playOnStart;
        public bool stopOnFinish;
    }
}
