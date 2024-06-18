using UnityEngine;

public class StringToID : MonoBehaviour
{
    public string queryString;
    public int id;
    
    [ContextMenu("Get ID For Query")]
    private void GetIDForQuery()
    {
        id = queryString.GetHashCode ();
    }
}
