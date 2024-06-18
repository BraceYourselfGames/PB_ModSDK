using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [Serializable]
    public class DataContainerSettingsProvinces : DataContainerUnique
    {
        [FilePath (ParentFolder = "Assets/Resources", Extensions = ".png")]
        public string provinceLookupTextureAssetPath;
        public Vector3 worldSize = new Vector3 (3072, 0, 3072);
        public Vector3 worldOffset = new Vector3 (1024, 0, 1024);
        public Dictionary<string, int> definitionsOfProvinces;
    }
}
