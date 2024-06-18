using Area;
using UnityEngine;
using Sirenix.OdinInspector;

public class DataManagerTilesets : MonoBehaviour
{
    [Button ("Load", ButtonSizes.Large), ButtonGroup, PropertyOrder (-1)]
    private void Load () => AreaTilesetHelper.LoadDatabase ();
    
    [Button ("Save", ButtonSizes.Large), ButtonGroup, PropertyOrder (-1)]
    private void Save () => AreaTilesetHelper.SaveDatabase ();

    [ShowInInspector, BoxGroup, HideLabel, HideReferenceObjectPicker]
    private AreaTilesetDatabase data
    {
        get
        {
            return AreaTilesetHelper.database;
        }
        set
        {
            AreaTilesetHelper.database = value;
        }
    }
}