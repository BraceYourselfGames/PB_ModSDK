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

        [YamlIgnore]
        [ShowInInspector, DisplayAsString, InlineButton ("RefreshExample", "Generate")]
        private string example = string.Empty;

        private void RefreshExample ()
        {
            var collection = DataManagerText.GetTextCollectionByTag (collectionTag);
            int collectionCount = collection != null ? collection.Count : 0;
            if (collectionCount <= 0)
            {
                Debug.LogWarning ($"Failed to get random name index of a site using text collection tag {collectionTag}");
                return;
            }

            var nameBase = collection.GetRandomEntry ();
            int serial = Mathf.RoundToInt (Random.Range (10, 256));
            if (prefixID)
                example = $"{serial}-{nameBase}";
            else if (suffixID)
                example = $"{nameBase}-{serial}";
            else
                example = nameBase;
        }
    }
}

