using Unity.Entities;


namespace Area.ECS
{
    public struct BlockID : IComponentData
    {
        public int id;
    }

    public struct BlockSimulated : IComponentData
    {
        public int indexOfHelper;
    }

    public struct SimulatedChunkRoot : IComponentData
    {
        public int id;
    }


    public struct BlockDamageEdge : IComponentData
    {
        public int indexOfPoint;
    }

    public struct PropRoot : IComponentData
    {
        public int id;
        public int pivotIndex;
    }

    public struct PropChild : IComponentData
    {
        public int id;
        public int pivotIndex;
        public int subObjectIndex;
    }
}