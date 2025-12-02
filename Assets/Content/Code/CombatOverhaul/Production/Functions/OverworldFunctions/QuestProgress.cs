using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    public class QuestProgress : OverworldFunctionQuest, IOverworldFunction
    {
        [DropdownReference]
        // [ValueDropdown("$GetStepKeys")]
        public string stepRequired;
        
        [DropdownReference]
        // [ValueDropdown("$GetStepKeys")]
        public string stepNext;
        
        public bool effectOnExit = true;
        public bool effectOnEntry = true;
        
        public void Run ()
        {
            #if !PB_MODSDK
            
            var questState = GetQuestState ();
            if (questState == null)
                return;

            if (string.IsNullOrEmpty (stepNext))
                OverworldQuestUtility.TryProgressingQuest (questState.key, effectOnExit, effectOnEntry, stepRequired);
            else
                OverworldQuestUtility.TryProgressingQuestToStep (questState.key, stepNext, effectOnExit, effectOnEntry, stepRequired);

            #endif
        }
        
        #region Editor
        #if UNITY_EDITOR

        /*
        private IEnumerable<string> GetStepKeys ()
        {
            var questData = DataMultiLinkerOverworldQuest.GetEntry (questKey, false);
            if (questData == null || questData.stepsProc == null)
                return null;

            return questData.stepsProc.Keys;
        }
        */
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public QuestProgress () =>
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
}