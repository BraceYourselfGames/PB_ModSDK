using UnityEngine;
using Sirenix.OdinInspector;
using Area;

[ExecuteInEditMode]
public class ProceduralMeshTerrain : MonoBehaviour
{
    public MeshFilter mf;
    public MeshRenderer mr;
    public AreaManager am;
    public float offset = 1.5f;

    // [ShowInInspector]
    public int[,] heightfield;

    private void OnEnable ()
    {
        if (Utilities.isPlaymodeChanging)
            return;

        Rebuild ();
    }

    /*
    private void OnDrawGizmosSelected ()
    {
        if (heightfield == null)
            return;

        Vector3 shift = new Vector3 (0f, offset, 0f);

        for (int x = 0, safeLimitX = heightfield.GetLength (0) - 1; x < safeLimitX; ++x)
        {
            for (int z = 0, safeLimitZ = heightfield.GetLength (1) - 1; z < safeLimitZ; ++z)
            {
                Vector3 pos0 = new Vector3 (x, heightfield[x, z], z) * TilesetUtility.blockAssetSize + shift;
                Vector3 pos1 = new Vector3 (x + 1, heightfield[x + 1, z], z) * TilesetUtility.blockAssetSize + shift;
                Vector3 pos2 = new Vector3 (x, heightfield[x, z + 1], z + 1) * TilesetUtility.blockAssetSize + shift;
                Vector3 pos3 = new Vector3 (x + 1, heightfield[x + 1, z + 1], z + 1) * TilesetUtility.blockAssetSize + shift;

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine (pos3, pos2);

                Gizmos.color = Color.cyan;
                Gizmos.DrawLine (pos3, pos1);

                if (z == 0)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine (pos0, pos1);
                }

                if (x == 0)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine (pos0, pos2);
                }
            }
        }
    }
    */

    [Button ("Rebuild", ButtonSizes.Large)]
    private void Rebuild ()
    {
        if (am == null || am.points.Count < 8)
            return;

        if (mf == null)
            mf = gameObject.GetComponent<MeshFilter> ();

        if (mr == null)
            mr = gameObject.GetComponent<MeshRenderer> ();

        if (mf == null || mr == null)
            return;

        int size = am.boundsFull.x * am.boundsFull.z;
        heightfield = new int[am.boundsFull.x, am.boundsFull.z];

        Debug.Log ("Size: " + size + " | Bounds: " + am.boundsFull);

        for (int i = 0; i < size; ++i)
        {
            var point = am.points[i];

            int iteration = 0;
            while (point.pointState == AreaVolumePointState.Empty)
            {
                point = point.pointsInSpot[4];
                iteration += 1;
                if (iteration > 100)
                {
                    Debug.Log ("Breaking out of while loop, something is wrong");
                    break;
                }
            }

            // Debug.Log (i + " | Position: " + pos);
            heightfield[point.pointPositionIndex.x, point.pointPositionIndex.z] = -point.pointPositionIndex.y;
        }

        Mesh mesh = mf.sharedMesh;
        if (mesh == null)
        {
            mesh = new Mesh ();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.name = "procedural_mesh_terrain";
            mf.sharedMesh = mesh;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        int sizeOnX = heightfield.GetLength (0);
        int sizeOnZ = heightfield.GetLength (1);
        int vertexCount = sizeOnX * sizeOnZ;
        int triangleCount = (sizeOnX - 1) * (sizeOnZ - 1) * 2 * 3;
        var spotShift = new Vector3 (0f, offset, 0f);
        
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[triangleCount];
        Vector3[] normals = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];
        Vector2[] uv2 = new Vector2[vertexCount];
        Quaternion normalRotation = Quaternion.Euler (90f, 0f, 0f);
        Color[] colors = new Color[vertexCount];
        Color vertexColorDefault = Color.white.WithAlpha (1f);

        int triangleIndex = 0;
        for (int x = 0, safeLimitX = sizeOnX - 1; x < safeLimitX; ++x)
        {
            for (int z = 0, safeLimitZ = sizeOnZ - 1; z < safeLimitZ; ++z)
            {
                var posXNegZNeg = new Vector3 (x, heightfield[x, z], z) * TilesetUtility.blockAssetSize + spotShift;
                var posXPosZNeg = new Vector3 (x + 1, heightfield[x + 1, z], z) * TilesetUtility.blockAssetSize + spotShift;
                var posXNegZPos = new Vector3 (x, heightfield[x, z + 1], z + 1) * TilesetUtility.blockAssetSize + spotShift;
                var posXPosZPos = new Vector3 (x + 1, heightfield[x + 1, z + 1], z + 1) * TilesetUtility.blockAssetSize + spotShift;

                int indexXNegZNeg = AreaUtility.GetIndexFromInternalPosition (x, z, sizeOnX, sizeOnZ);
                int indexXPosZNeg = AreaUtility.GetIndexFromInternalPosition (x + 1, z, sizeOnX, sizeOnZ);
                int indexXNegZPos = AreaUtility.GetIndexFromInternalPosition (x, z + 1, sizeOnX, sizeOnZ);
                int indexXPosZPos = AreaUtility.GetIndexFromInternalPosition (x + 1, z + 1, sizeOnX, sizeOnZ);
                int indexBySix = triangleIndex * 6;
                ++triangleIndex;
                
                vertices[indexXNegZNeg] = posXNegZNeg;
                vertices[indexXPosZNeg] = posXPosZNeg;
                vertices[indexXNegZPos] = posXNegZPos;
                vertices[indexXPosZPos] = posXPosZPos;
                
                triangles[indexBySix] = indexXNegZNeg;
                triangles[indexBySix + 1] = indexXNegZPos;
                triangles[indexBySix + 2] = indexXPosZPos;
            
                triangles[indexBySix + 3] = indexXNegZNeg;
                triangles[indexBySix + 4] = indexXPosZPos;
                triangles[indexBySix + 5] = indexXPosZNeg;
            }
        }

        /*
        int totalLimit = (sizeOnX - 1) * (sizeOnZ - 1);
        for (int i = 0; i < totalLimit; ++i)
        {
            int x = i % sizeOnX;
            int z = (i / sizeOnX) % sizeOnZ;
            
            var posXNegZNeg = new Vector3 (x, heightfield[x, z], z) * TilesetUtility.blockAssetSize + spotShift;
            var posXPosZNeg = new Vector3 (x + 1, heightfield[x + 1, z], z) * TilesetUtility.blockAssetSize + spotShift;
            var posXNegZPos = new Vector3 (x, heightfield[x, z + 1], z + 1) * TilesetUtility.blockAssetSize + spotShift;
            var posXPosZPos = new Vector3 (x + 1, heightfield[x + 1, z + 1], z + 1) * TilesetUtility.blockAssetSize + spotShift;
            
            int indexXNegZNeg = i;
            int indexXPosZNeg = i + 1;
            int indexXNegZPos = AreaUtility.GetIndexFromInternalPosition (x, z + 1, sizeOnX, sizeOnZ);
            int indexXPosZPos = AreaUtility.GetIndexFromInternalPosition (x + 1, z + 1, sizeOnX, sizeOnZ);
            int indexBySix = i * 6;
            
            vertices[indexXNegZNeg] = posXNegZNeg;
            vertices[indexXPosZNeg] = posXPosZNeg;
            vertices[indexXNegZPos] = posXNegZPos;
            vertices[indexXPosZPos] = posXPosZPos;

            triangles[indexBySix] = indexXNegZPos;
            triangles[indexBySix + 1] = indexXNegZNeg;
            triangles[indexBySix + 2] = indexXPosZPos;
            
            triangles[indexBySix + 3] = indexXNegZNeg;
            triangles[indexBySix + 4] = indexXPosZNeg;
            triangles[indexBySix + 5] = indexXPosZPos;
        }
        */
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.uv2 = uv2;
        mesh.colors = colors;
        
        mesh.RecalculateNormals ();
        normals = mesh.normals;
    }
}
