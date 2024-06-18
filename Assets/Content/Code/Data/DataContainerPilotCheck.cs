using System;
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
        
        public bool hidden = false;
        
        public string icon;
        public EntityCheckSeverity severity;
        
        public DataBlockPilotCheck check = new DataBlockPilotCheck ();
        
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

