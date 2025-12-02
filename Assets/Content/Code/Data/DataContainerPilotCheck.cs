using System;
using System.Collections.Generic;
using PhantomBrigade.Functions;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [Serializable]
    public class DataBlockPilotCheck
    {
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckFloat healthNormalized;

        [DropdownReference (true)]
        public DataBlockOverworldMemoryCheckGroup eventMemory;

        #if !PB_MODSDK
        public bool IsPassed (PersistentEntity pilot)
        {
            if (pilot == null || !pilot.isPilotTag)
                return false;
            
            bool healthNormalizedValid = true;
            if (healthNormalized != null)
            {
                var healthNormalized = pilot.GetPilotStatNormalized (PilotStatKeys.hp);
                healthNormalizedValid = this.healthNormalized.IsPassed (true, healthNormalized);
            }

            bool memoryValid = true;
            if (eventMemory != null)
                memoryValid = eventMemory.IsPassed (pilot);

            return healthNormalizedValid && memoryValid;
        }
        #endif

        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockPilotCheck () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
    
    [Serializable][LabelWidth (150f)]
    public class DataContainerPilotCheck : DataContainerWithText
    {
        [YamlIgnore]
        [LabelText ("Name / Desc.")]
        public string textName;
        
        [YamlIgnore]
        [HideLabel, TextArea]
        public string textDesc;
        
        public bool textDescInTooltips = false;
        public bool hidden = false;
        public bool displayInCombat = false;
        
        public string icon;
        public EntityCheckSeverity severity;

        public List<IPilotValidationFunction> checks = new List<IPilotValidationFunction> ();

        #if !PB_MODSDK
        public bool IsTriggered (PersistentEntity pilot)
        {
            if (checks == null || checks.Count == 0)
                return false;

            if (pilot == null || !pilot.isPilotTag || pilot.isDestroyed)
                return false;

            foreach (var check in checks)
            {
                if (check == null)
                    continue;

                var valid = check.IsValid (pilot, null);
                if (!valid)
                    return false;
            }
            
            return true;
        }
        #endif
        
        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.pilotChecks, $"{key}__name");
            textDesc = DataManagerText.GetText (TextLibs.pilotChecks, $"{key}__desc");
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.pilotChecks, $"{key}__name", textName);
            DataManagerText.TryAddingTextToLibrary (TextLibs.pilotChecks, $"{key}__desc", textDesc);
        }
        
        #endif
        #endregion
    }
}

