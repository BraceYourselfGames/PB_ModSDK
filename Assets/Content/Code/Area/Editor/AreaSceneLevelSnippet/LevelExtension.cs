using System.Collections.Generic;

using Content.Code.Utility;

namespace Area
{
    [TypeHinted]
    public interface ILevelExtension { }

    // An extension that changes the serialization and deserialization of level data.
    // This lets a mod change level data before the AreaManager processes it.
    public interface ILevelContentExtension : ILevelExtension { }

    // An extension may want to manipulate the level data after the AreaManager has processed it
    // or otherwise extend the level. An example would be the DataContainerCombatArea fields
    // which don't change level data yet add both visual and interactive elements to levels.
    public interface ILevelSceneExtension : ILevelExtension
    {
        void Apply (LevelData levelData);
    }

    public sealed class LevelData
    {
        public Vector3Int Bounds;
        public List<AreaVolumePoint> Points;
        public List<AreaPlacementProp> Props;
        // See below about area extension data.
        //public List<AreaExtensionData> ExtensionData;
    }

    // This allows a mod to add custom data to that's held by the AreaManager
    // similar to how props are held in AreaManager.indexesOccupiedByProps.
    // In fact, props are merely built-in extension data.
    public abstract class AreaExtensionData { }
}
