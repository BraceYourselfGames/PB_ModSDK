using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [LabelWidth (120f)]
    public class DataContainerBaseStatGroup : DataContainerWithText
    {
        [LabelText ("Name / Desc.")][YamlIgnore]
        public string textName;

        [HideLabel, TextArea (1, 10)][YamlIgnore]
        public string textDesc;

        [YamlIgnore, ReadOnly, ListDrawerSettings (DefaultExpandedState = false)]
        public List<string> children;

        public List<DataBlockBasePartDependency> partDependencies;
        
        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.baseStatsGroups, $"{key}__name");
            textDesc = DataManagerText.GetText (TextLibs.baseStatsGroups, $"{key}__text");
            
        }

        #region Editor
        #if UNITY_EDITOR
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.baseStatsGroups, $"{key}__name", textName);
            DataManagerText.TryAddingTextToLibrary (TextLibs.baseStatsGroups, $"{key}__text", textDesc);
        }
        
        #endif
        #endregion
    }
}

