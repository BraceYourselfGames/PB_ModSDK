using System;
using UnityEngine;
using Unity.Entities;
using UnityEngine.Rendering;

namespace CustomRendering
{
    /// <summary>
    /// Render Mesh with Material (must be instanced material) by object to world matrix.
    /// Specified by the LocalToWorld associated with Entity.
    /// </summary>
    [Serializable]
    public struct InstancedMeshRenderer : ISharedComponentData, IEquatable<InstancedMeshRenderer>
    {
        public Mesh mesh;
        public Material material;
        public int subMesh;
        
        public int instanceLimit;

        public ShadowCastingMode castShadows;
        public bool receiveShadows;
        public long id;

        public bool Equals(InstancedMeshRenderer other) =>
            other.id == id;

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
    }
}