using UnityEngine;
using System.Collections.Generic;
 
namespace Area
{
    [System.Serializable]
    public class AreaBlockDefinition
    {
        public SortedDictionary<byte, SortedDictionary<byte, GameObject>> subtypeGroups = new SortedDictionary<byte, SortedDictionary<byte, GameObject>> ();

        // public int prefabRotationID = 0;
        // public bool prefabFlipped = false;
        // public bool customRotationPossible = false;
        // public int customFlippingAxis = -1;
    }

    [System.Serializable]
    public class AreaConfigurationData
    {
        public string configurationAsString = string.Empty;

        /// <summary>
        /// Cached just in case you'll need to recheck configuration without knowing how you came to reference the given configuration dataset
        /// </summary>
        public byte configuration = 0;

        /// <summary>
        /// Describes the mandatory rotation of a prefab, which is necessary because prefab might've been authored for a different configuration
        /// </summary>
        public int requiredRotation = 0;

        /// <summary>
        /// Whether it's mandatory to invert the prefab on Z axis, which is necessary because prefab might've been authored for a different configuration
        /// </summary>
        public bool requiredFlippingZ = false;

        /// <summary>
        /// Custom rotation is possible on configurations with exactly the same top 4 and bottom 4 corner values, which makes all prefab rotations valid
        /// </summary>
        public bool customRotationPossible = false;

        /// <summary>
        /// Some configurations can be flipped without breaking the prefab adherence to the grid
        /// Value of -1 indicates no flipping allowed, 0 indicates it's allowed on X, 1 indicates it's allowed on Z
        /// </summary>
        public int customFlippingMode = -1;
    }
}