using System;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Data
{
    [Serializable][LabelWidth (180f)]
    public class DataContainerOverworldNameGroup : DataContainer
    {
        public bool prefixID;
        public bool suffixID;
        public string collectionTag;

        #if !PB_MODSDK
        [YamlIgnore]
        [ShowInInspector, DisplayAsString, InlineButton ("RefreshExample", "Generate")]
        private string example = string.Empty;

        private void RefreshExample ()
        {
            var loc = DataManagerText.GetRandomLocStructFromTag (collectionTag);
            var text = loc.text;
            
            int serial = Mathf.RoundToInt (Random.Range (10, 256));
            if (prefixID)
                example = $"{serial}-{text}";
            else if (suffixID)
                example = $"{text}-{serial}";
            else
                example = text;
        }
        #endif
    }
}

