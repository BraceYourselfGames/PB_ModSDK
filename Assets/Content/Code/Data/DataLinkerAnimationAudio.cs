using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
    public class DataLinkerAnimationAudio : DataLinker<DataContainerAnimationAudioSettings>
    {
    }

    [Serializable]
    public class DataContainerAnimationAudioSettings : DataContainerUnique
    {
        [DictionaryKeyDropdown ("@DataContainerAnimationAudioSettings.GetKeys")]
        public Dictionary<string, DataBlockAnimationStateAudioEvent> animationAudioEvents;

    }

    public class DataBlockAnimationStateAudioEvent
    {
        public bool fireEventOnEntry;
        [ShowIf("fireEventOnEntry")]
        [ValueDropdown("@AudioEvents.GetKeys()")]
        public string onEnterEventName;

        public bool fireEventOnExit;
        [ShowIf("fireEventOnExit")]
        [ValueDropdown("@AudioEvents.GetKeys()")]
        public string onExitEventName;
        
    }
}