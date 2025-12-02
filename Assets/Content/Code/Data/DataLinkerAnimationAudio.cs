using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
    public enum audioAnimState
    {
        None = 0,
        Concussed = 1,
        Overheat = 2,
        Sliding = 3,
        StopToMove = 4,
        RunToSlide = 5,
        SlideToRun = 6,
        SlideToStop = 7,
        WalkJogRunToStop = 8,
        SuddenStop = 9,
        JumpUp = 10,
        JumpDown = 11,
        JumpOverObstacle = 12,
        JumpOverDitch = 13,
        GetUpKnee = 14,
        GetUpProneSupine = 15,
        Dash = 16,
        DashEnd = 17,
        Melee = 18,
        StorageOutOf = 19,
        StorageInto = 20,
        SpawnAirDrop = 21,
        UseEquipmentIntro = 22,
        UseEquipmentOutro = 23,
        FabricatorArmIntro = 24,
        FabricatorArmLoop = 25,
        FabricatorArmOutro = 26,
        SprintToStop = 27,
        StopToSprint = 28,
        MeleeStrike = 29
    }
    
    public class DataLinkerAnimationAudio : DataLinker<DataContainerAnimationAudioSettings>
    {
    }

    [Serializable]
    public class DataContainerAnimationAudioSettings : DataContainerUnique
    {
        [DictionaryKeyDropdown ("@DataContainerAnimationAudioSettings.GetKeys")]
        public Dictionary<string, DataBlockAnimationStateAudioEvent> animationAudioEvents;

        #if UNITY_EDITOR
        private static List<string> keys;
        public static List<string> GetKeys => keys ?? (keys = new List<string>(Enum.GetNames(typeof(audioAnimState))));
        #endif
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