using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [Serializable][LabelWidth (100f)]
    public class DataContainerPartSocket : DataContainerWithText, IDataContainerTagged
    {
        public bool hidden = false;
        public bool levelCritical = false;
        public bool body;
        
        public int priority = 0;

        [YamlIgnore]
        [LabelText ("Name / Desc.")]
        public string textName = string.Empty;
        
        [YamlIgnore]
        [TextArea (1, 10)][HideLabel]
        public string textDesc = string.Empty;
        
        [DataEditor.SpriteNameAttribute (false, 180f)]
        public string icon = "s_icon_m_link_squad";

        public bool iconFlip = false;

        [ShowIf ("@DataMultiLinkerPartSocket.Presentation.showTags")]
        [ValueDropdown("@DataMultiLinkerPartSocket.tags")]
        public HashSet<string> tags = new HashSet<string> ();

        public HashSet<string> GetTags (bool processed) =>
            tags;
        
        public bool IsHidden () => hidden;
        
        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.equipmentSockets, $"{key}__name");
            textDesc = DataManagerText.GetText (TextLibs.equipmentSockets, $"{key}__text");
        }

        #if UNITY_EDITOR

        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.equipmentSockets, $"{key}__name", textName);
            DataManagerText.TryAddingTextToLibrary (TextLibs.equipmentSockets, $"{key}__text", textDesc);
        }

        #endif
    }
}

