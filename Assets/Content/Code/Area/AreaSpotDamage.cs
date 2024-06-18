using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Contains the description of a damaged spot, not sure yet why it's needed
/// </summary>

namespace Area
{
    [System.Serializable]
    public class AreaSpotDamage
    {
        public int spotIndex;
        public AreaVolumePointState[] configuration;
        public List<GameObject> instances = null;
    }
}