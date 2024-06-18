using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatCreateCommsMessageGroup : ICombatFunction
    {
        public bool clearQueue = false;
        
        [ListDrawerSettings (CustomAddFunction = "@new DataBlockScenarioCommLink ()")]
        public List<DataBlockScenarioCommLink> comms = new List<DataBlockScenarioCommLink> ();
        
        public void Run ()
        {
            #if !PB_MODSDK
            
            if (comms != null && comms.Count > 0)
            {
                if (clearQueue)
                    CIViewCombatComms.ClearScheduledMessages ();

                CIViewCombatComms.ScheduleMessageGroup (comms);
            }
            
            #endif
        }
    }
    
    [Serializable]
    public class CombatCreateCommsMessage : ICombatFunction
    {
        [PropertyOrder (1), HideLabel, HorizontalGroup (50f)]
        public float time = 0f;
        
        [GUIColor ("GetContentColor")]
        [HorizontalGroup]
        [ValueDropdown ("@DataMultiLinkerCombatComms.data.Keys")]
        [HideLabel]
        public string key;
        
        public void Run ()
        {
            #if !PB_MODSDK
            
            CIViewCombatComms.ScheduleMessage (key, time);
            
            #endif
        }
        
        #if UNITY_EDITOR

        [ShowInInspector]
        [MultiLineProperty (3), GUIColor (1f, 1f, 1f, 0.5f)]
        [HideLabel, PropertyOrder (10)]
        public string content
        {
            get
            {
                return GetContent ();
            }
            set
            {
                //
            }
        }

        private string GetContent ()
        {
            if (string.IsNullOrEmpty (key))
                return string.Empty;
            
            var data = DataMultiLinkerCombatComms.GetEntry (key, false);
            if (data == null)
                return string.Empty;

            if (data.textContent == null || data.textContent.Count == 0)
                return string.Empty;

            if (data.textContent.Count == 1)
                return data.textContent[0];

            var textContentCombined = data.textContent.ToStringFormatted (true, multilinePrefix: "- ");
            return textContentCombined;
        }

        private Color GetContentColor ()
        {
            if (string.IsNullOrEmpty (key))
                return Color.white;
            
            var data = DataMultiLinkerCombatComms.GetEntry (key, false);
            if (data == null || string.IsNullOrEmpty (data.source))
                return Color.white;

            var source = DataMultiLinkerCombatCommsSource.GetEntry (data.source, false);
            if (source == null)
                return Color.white;

            return source.color;
        }

        [Button, HideInEditorMode, PropertyOrder (-1)]
        private void Test ()
        {
            #if !PB_MODSDK
            
            if (!Application.isPlaying)
                return;

            Run ();
            
            #endif
        }

        #endif
    }
}