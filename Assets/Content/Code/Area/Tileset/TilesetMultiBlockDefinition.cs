using UnityEngine;

namespace Area
{
    [CreateAssetMenu (fileName = "TilesetMultiBlockDefinition", menuName = "Tileset/Multi-Block Definition")]
    public class TilesetMultiBlockDefinition : ScriptableObject
    {
        public int tileset = 0;
        public string name = string.Empty;

        public bool[] volume = new bool[27];
        public Vector3Int bounds;
        public Vector3Int pivotPosition = Vector3Int.size0x0x0;
        public GameObject prefab;
        public bool useAsBrush = false;

        public void UpdateVolume ()
        {
            int volumeLength = bounds.x * bounds.y * bounds.z;
            if (volume == null || volume.Length == 0 || volume.Length != volumeLength)
            {
                bounds = new Vector3Int ((int)Mathf.Max (bounds.x, 2), (int)Mathf.Max (bounds.y, 2), (int)Mathf.Max (bounds.z, 2));
                volume = new bool[volumeLength];
            }
        }

        [System.Serializable]
        public struct TransformedData
        {
            public bool[] volumeTransformed;
            public Vector3Int boundsTransformed;
            public Vector3Int pivotPositionTransformed;
            public int pivotIndexTransformed;

            public TransformedData (bool[] volumeTransformed, Vector3Int boundsTransformed, Vector3Int pivotPositionTransformed, int pivotIndexTransformed)
            {
                this.volumeTransformed = volumeTransformed;
                this.boundsTransformed = boundsTransformed;
                this.pivotPositionTransformed = pivotPositionTransformed;
                this.pivotIndexTransformed = pivotIndexTransformed;
            }
        }

        public TransformedData GetTransformedData (int rotation)
        {
            int length = volume.Length;
            bool[] volumeTransformed = new bool[length];
            System.Array.Copy (volume, volumeTransformed, length);

            Vector3Int boundsTransformed = bounds;
            Vector3Int pivotPositionTransformed = pivotPosition;
            int pivotIndexTransformed = TilesetUtility.GetIndexFromVolumePosition (pivotPosition, bounds);

            for (int r = 0; r < rotation; ++r)
            {
                bool[] resultClone = new bool[length];
                System.Array.Copy (volumeTransformed, resultClone, length);

                boundsTransformed = new Vector3Int (boundsTransformed.z, boundsTransformed.y, boundsTransformed.x);
                bool pivotCopiedOnThisRotation = false;

                for (int i = 0; i < volumeTransformed.Length; ++i)
                {
                    int x = i % boundsTransformed.x;
                    int y = i / (boundsTransformed.z * boundsTransformed.x);
                    int z = (i / boundsTransformed.x) % boundsTransformed.z;

                    int indexToCopyFrom = z + (boundsTransformed.x - x - 1) * boundsTransformed.z + y * boundsTransformed.x * boundsTransformed.z;
                    volumeTransformed[i] = resultClone[indexToCopyFrom];

                    if (indexToCopyFrom == pivotIndexTransformed && !pivotCopiedOnThisRotation)
                    {
                        pivotIndexTransformed = i;
                        pivotCopiedOnThisRotation = true;
                    }
                }
            }

            pivotPositionTransformed = TilesetUtility.GetVolumePositionFromIndex (pivotIndexTransformed, boundsTransformed);

            return new TransformedData
            (
                volumeTransformed,
                boundsTransformed,
                pivotPositionTransformed,
                pivotIndexTransformed
            );
        }
    }
}