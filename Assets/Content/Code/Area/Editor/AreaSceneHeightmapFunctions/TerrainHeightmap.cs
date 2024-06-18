using System.Collections.Specialized;

using UnityEngine;

namespace Area
{
    public static class TerrainHeightmap
    {
        public static void CalculateValues (AreaManager.HeightmapSpec spec)
        {
            // Removes structures and fills holes left by those structures with terrain tiles.

            grid = new BitVector32[spec.SizeX * spec.SizeZ];
            FillGrid (spec);
            var unfilled = FillMissing (spec.SizeX, spec.SizeZ, spec.MaxDepth);
            Debug.Log ("Non-terrain cells: " + unfilled);

            for (var x = 0; x < spec.SizeX; x += 1)
            {
                for (var z = 0; z < spec.SizeZ; z += 1)
                {
                    var gridIndex = z * spec.SizeX + x;
                    var cell = grid[gridIndex];
                    var layer = cell[cellLayer];
                    spec.Store (x, z, layer, cell[cellIsTerrain] == 0 ? byte.MinValue : byte.MaxValue, byte.MinValue);
                }
            }
        }

        static void FillGrid (AreaManager.HeightmapSpec spec)
        {
            var bounds = spec.Bounds;
            var points = spec.Points;
            var sizeX = spec.SizeX;
            var sizeZ = spec.SizeZ;
            for (var y = spec.MaxDepth; y >= 0; y -= 1)
            {
                for (var x = 0; x < sizeX; x += 1)
                {
                    for (var z = 0; z < sizeZ; z += 1)
                    {
                        var position = new Vector3Int (x, y, z);
                        var index = AreaUtility.GetIndexFromVolumePosition (position, bounds, skipBoundsCheck: true);
                        var point = points[index];
                        var gridIndex = x + z * sizeX;
                        var cell = grid[gridIndex];

                        if (cell[cellIsTerrain] == 1)
                        {
                            continue;
                        }

                        if (point.pointState == AreaVolumePointState.Empty)
                        {
                            if (point.spotConfiguration == TilesetUtility.configurationEmpty)
                            {
                                continue;
                            }
                            if ((point.spotConfiguration & configurationTopSelfBit) != 0)
                            {
                                Debug.LogWarningFormat
                                (
                                    "Invalid configuration for empty point -- point state is empty but top configuration bit is 1 | index: {0} {1} | configuration: {2}",
                                    index,
                                    position,
                                    point.spotConfiguration
                                );
                                continue;
                            }

                            if (AreaManager.IsPointTerrain(point))
                            {
                                cell[cellLayer] = y;
                                cell[cellIsTerrain] = 1;
                                grid[gridIndex] = cell;
                            }
                            continue;
                        }

                        if (y == 0)
                        {
                            Debug.LogWarningFormat
                            (
                                "Invalid point in layer 0 -- not empty | index: {0} {1} | state: {2}",
                                index,
                                position,
                                point.pointState
                            );
                        }
                    }
                }
            }
        }

        static int FillMissing (int sizeX, int sizeZ, int maxDepth)
        {
            var filled = 0;
            for (var x = 0; x < sizeX; x += 1)
            {
                for (var z = 0; z < sizeZ; z += 1)
                {
                    var gridIndex = z * sizeX + x;
                    var cell = grid[gridIndex];
                    if (cell[cellIsTerrain] == 1)
                    {
                        filled += 1;
                        continue;
                    }
                    cell[cellLayer] = maxDepth - 1;
                    grid[gridIndex] = cell;
                }
            }
            return sizeX * sizeZ - filled;
        }

        static BitVector32[] grid;

        const byte configurationTopSelfBit = 0x80;
        const int maxLayer = 31;
        static readonly BitVector32.Section cellLayer = BitVector32.CreateSection (maxLayer);
        static readonly BitVector32.Section cellIsTerrain = BitVector32.CreateSection (1, cellLayer);
    }
}
