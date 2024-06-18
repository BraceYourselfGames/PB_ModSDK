using UnityEngine;
using System.Collections.Generic;

namespace Area
{
    [CreateAssetMenu (fileName = "TilesetEdgeBlockDefinition", menuName = "Tileset/Edge Block Definition")]
    public class TilesetEdgeBlockDefinition : ScriptableObject
    {
        public byte id = 0;
        public GameObject prefab = null;

        public bool allowRotationWithinPlane = true;
        public bool allowRotationToOrthogonalPlane = true;
        public bool allowRotationToOppositePlane = true;

        // public AreaVolumePointConfiguration configuration = new AreaVolumePointConfiguration ();
        public List<AreaVolumePointConfiguration> configurations = new List<AreaVolumePointConfiguration> ();
    }
}