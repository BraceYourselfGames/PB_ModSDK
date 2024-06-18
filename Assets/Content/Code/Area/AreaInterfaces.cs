using UnityEngine;

namespace Area
{
    public interface IAreaAccess
    {
        void ApplyDamageToPosition (Vector3 position, int damage);
    }
}