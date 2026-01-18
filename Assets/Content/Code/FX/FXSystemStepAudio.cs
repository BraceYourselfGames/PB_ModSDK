using System;
using System.Collections.Generic;
using PhantomBrigade;
using Sirenix.OdinInspector;
using PhantomBrigade.Game;
using UnityEngine;

public partial class FXSystem : MonoBehaviour
{
    [Serializable]
    public class StepAudio
    {
        public GameObject target;
        
        [ValueDropdown("GetAudioKeys")]
        public string[] events;

        #if UNITY_EDITOR
        private IEnumerable<string> GetAudioKeys () => AudioEvents.GetKeys ();
        #endif

        public void OnPlay ()
        {
            #if !PB_MODSDK
            if (target == null || events == null)
                return;
            
            foreach (var audioEvent in events)
            {
                AudioUtility.CreateLinkedAudioEvent(audioEvent, target.transform);
            }
            #endif
        }
        
    }
}
