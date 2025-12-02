using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using PhantomBrigade.Overworld.Components;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Functions
{
    public class OverworldFunctionQuestBase
    {
        [PropertyOrder (-10)]
        [ValueDropdown ("@DataMultiLinkerOverworldQuest.GetKeys ()")]
        public string questKey;

        #if !PB_MODSDK
        protected QuestState GetQuestState ()
        {
            QuestState questState = null;
            
            if (!string.IsNullOrEmpty (questKey))
                questState = OverworldQuestUtility.GetQuestState (questKey);
            
            return questState;
        }
        #endif

        public override string ToString () => 
            $"{GetType ().Name} / {questKey}";
    }
    
    public class OverworldFunctionQuest
    {
        [PropertyOrder (-10), LabelText ("Main quest"), HorizontalGroup ("Quest")]
        public bool questMain = true;
        
        [HideIf (nameof(questMain))]
        [PropertyOrder (-10), HideLabel, HorizontalGroup ("Quest", 0.65f)]
        [ValueDropdown ("@DataMultiLinkerOverworldQuest.GetKeys ()")]
        public string questKey;

        #if !PB_MODSDK
        protected QuestState GetQuestState ()
        {
            QuestState questState = null;
            
            if (questMain)
            {
                var overworld = Contexts.sharedInstance.overworld;
                var questsActive = overworld.hasQuestsActive ? overworld.questsActive.s : null;
                if (questsActive == null || questsActive.Count == 0)
                    return null;

                foreach (var kvp in questsActive)
                {
                    var questData = DataMultiLinkerOverworldQuest.GetEntry (kvp.Key, false);
                    if (questData == null || questData.coreProc == null || !questData.coreProc.mainType)
                        continue;
                    
                    questState = kvp.Value;
                    break;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty (questKey))
                    questState = OverworldQuestUtility.GetQuestState (questKey);
                
            }
            
            return questState;
        }
        #endif
    }

    public class QuestStart : OverworldFunctionQuestBase, IOverworldFunction
    {
        public void Run ()
        {
            #if !PB_MODSDK
            
            var questState = GetQuestState ();
            if (questState != null)
            {
                Debug.LogWarning ($"Quest {questKey} already in progress");
                return;
            }

            var provinceActiveKey = DataHelperProvince.GetProvinceKeyActive ();
            OverworldQuestUtility.TryEnterQuest (questKey, provinceActiveKey);

            #endif
        }
    }
    
    public class QuestStartWithQuestProvince : OverworldFunctionQuestBase, IOverworldFunction
    {
        [ValueDropdown ("@DataMultiLinkerOverworldQuest.GetKeys ()")]
        public string questKeyOngoing;

        public void Run ()
        {
            #if !PB_MODSDK

            var questState = GetQuestState ();
            if (questState != null)
            {
                Debug.LogWarning ($"Quest {questKey} already in progress");
                return;
            }
            
            if (string.IsNullOrEmpty (questKeyOngoing))
                return;

            var questStateOngoing = OverworldQuestUtility.GetQuestState (questKeyOngoing);
            if (questStateOngoing == null)
            {
                Debug.LogWarning ($"Can't start new quest {questKey} using ongoing quest {questKeyOngoing}: no such active quest found");
                return;
            }

            OverworldQuestUtility.TryEnterQuest (questKey, questStateOngoing.province);

            #endif
        }
    }
    
    /*
    public class QuestStartWithSiteProvince : OverworldFunctionQuest, IOverworldTargetedFunction
    {
        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK
            
            var questState = GetQuestState ();
            if (questState != null)
            {
                Debug.LogWarning ($"Quest {questKey} already in progress");
                return;
            }

            var provinceKey = DataHelperProvince.GetProvinceKeyAtEntity (entityOverworld);
            OverworldQuestUtility.TryEnterQuest (questKey, provinceKey);

            #endif
        }
    }
    */
    
    /*
    public class QuestStartFromFrontline : OverworldFunctionQuest, IOverworldTargetedFunction
    {
        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK
            
            var questState = GetQuestState ();
            if (questState != null)
            {
                Debug.LogWarning ($"Quest {questKey} already in progress");
                return;
            }

            if (entityOverworld == null || !entityOverworld.hasFrontlineBase)
            {
                Debug.LogWarning ($"Entity {entityOverworld.ToLog ()} is not a frontline base, can't be used to initiate a quest!");
                return;
            }

            var provinceKey = entityOverworld.frontlineBase.provinceKeyConnected;

            OverworldQuestUtility.TryEnterQuest (questKey, provinceKey);

            #endif
        }
    }
    */
}