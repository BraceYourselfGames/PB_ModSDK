using System.Collections.Generic;

using UnityEngine;

namespace Area
{
    public static class BasinHeightmap
    {
        // Preserve a small buffer zone around the perimeter so that the boundary curves are preserved
        // and the level will blend with the area segments. Then slope down to lowest depth level and
        // create a flat layer of terrain.
        //
        // Find the lowest height in the level.
        // Project the level onto a 2D grid on the XZ plane.
        // Propagate the heights of the border cells inward a few cells and use a lookbehind algorithm
        // to ramp down the border to the lowest height. It uses the two neighbors that have already
        // been filled in to get the new height for a cell in the 2D grid which is why the algorithm
        // splits the grid into quadrants.

        // Size (in cells) of the buffer zone. This value should be at least 2.
        const int keepSteps = 3;

        public static void CalculateValues (AreaManager.HeightmapSpec spec)
        {
            var grid = new int[spec.SizeX, spec.SizeZ];
            var lowest = FindLowest (spec.Heightfield, spec.SizeX, spec.SizeZ);
            CopyBufferZone (spec, grid);
            FillSouthWestQuadrant (spec.SizeX, spec.SizeZ, grid, lowest, spec.Store);
            FillNorthWestQuadrant (spec.SizeX, spec.SizeZ, grid, lowest, spec.Store);
            FillNorthEastQuadrant (spec.SizeX, spec.SizeZ, grid, lowest, spec.Store);
            FillSouthEastQuadrant (spec.SizeX, spec.SizeZ, grid, lowest, spec.Store);
        }

        static int FindLowest (int[,] depths, int sizeX, int sizeZ)
        {
            var lowest = 0;
            for (var z = 0; z < sizeZ; z += 1)
            {
                for (var x = 0; x < sizeX; x += 1)
                {
                    var depth = depths[x, z];
                    if (depth > lowest)
                    {
                        lowest = depth;
                    }
                }
            }
            return lowest;
        }

        static void CopyBufferZone (AreaManager.HeightmapSpec spec, int[,] grid)
        {
            var sizeX = spec.SizeX;
            var sizeZ = spec.SizeZ;
            var lastX = sizeX - 1;
            var lastZ = sizeZ - 1;
            var depths = spec.Heightfield;
            var store = spec.Store;
            var points = spec.Points;
            var bounds = spec.Bounds;
            for (var z = 0; z < keepSteps; z += 1)
            {
                for (var x = 0; x < sizeX; x += 1)
                {
                    var depth = depths[x, 0];
                    grid[x, z] = depth;
                    var road = IsRoad (x, 1, depth, points, bounds);
                    store (x, z, depth, byte.MinValue, road ? byte.MaxValue : byte.MinValue);
                }
            }
            for (var z = lastZ; z >= lastZ - keepSteps; z -= 1)
            {
                for (var x = 0; x < sizeX; x += 1)
                {
                    var depth = depths[x, lastZ];
                    grid[x, z] = depth;
                    var road = IsRoad (x, lastZ, depth, points, bounds);
                    store (x, z, depth, byte.MinValue, road ? byte.MaxValue : byte.MinValue);
                }
            }
            for (var x = 0; x < keepSteps; x += 1)
            {
                for (var z = 0; z < sizeZ; z += 1)
                {
                    var depth = depths[0, z];
                    grid[x, z] = depth;
                    var road = IsRoad (1, z, depth, points, bounds);
                    store (x, z, depth, byte.MinValue, road ? byte.MaxValue : byte.MinValue);
                }
            }
            for (var x = lastX; x >= lastX - keepSteps; x -= 1)
            {
                for (var z = 0; z < sizeZ; z += 1)
                {
                    var depth = depths[lastX, z];
                    grid[x, z] = depth;
                    var road = IsRoad (lastX, z, depth, points, bounds);
                    store (x, z, depth, byte.MinValue, road ? byte.MaxValue : byte.MinValue);
                }
            }
        }

        static bool IsRoad (int x, int z, int depth, List<AreaVolumePoint> points, Vector3Int bounds)
        {
            // Roads are marked on the points in the layer below the road surface.
            var position = new Vector3Int (x, depth + 1, z);
            var index = AreaUtility.GetIndexFromVolumePosition (position, bounds, skipBoundsCheck: true);
            var point = points[index];
            return point.road;
        }

        static void FillSouthWestQuadrant (int sizeX, int sizeZ, int[,] grid, int lowest, AreaManager.StoreHeightmapInfo store)
        {
            var halfX = sizeX / 2;
            var halfZ = sizeZ / 2;
            for (var z = keepSteps; z < halfZ; z += 1)
            {
                for (var x = keepSteps; x < halfX; x += 1)
                {
                    var px = grid[x - 1, z];
                    var pz = grid[x, z - 1];
                    if (px < lowest)
                    {
                        px += 1;
                    }
                    if (pz < lowest)
                    {
                        pz += 1;
                    }
                    var v = Mathf.Min (px, pz);
                    grid[x, z] = v;
                    store(x, z, v, byte.MinValue, byte.MinValue);
                }
            }
        }

        static void FillNorthWestQuadrant (int sizeX, int sizeZ, int[,] grid, int lowest, AreaManager.StoreHeightmapInfo store)
        {
            var lastZ = sizeZ - 1;
            var halfX = sizeX / 2;
            var halfZ = sizeZ / 2;
            for (var z = lastZ - keepSteps; z >= halfZ; z -= 1)
            {
                for (var x = keepSteps; x < halfX; x += 1)
                {
                    var px = grid[x - 1, z];
                    var pz = grid[x, z + 1];
                    if (px < lowest)
                    {
                        px += 1;
                    }
                    if (pz < lowest)
                    {
                        pz += 1;
                    }
                    var v = Mathf.Min (px, pz);
                    grid[x, z] = v;
                    store(x, z, v, byte.MinValue, byte.MinValue);
                }
            }
        }

        static void FillNorthEastQuadrant (int sizeX, int sizeZ, int[,] grid, int lowest, AreaManager.StoreHeightmapInfo store)
        {
            var lastX = sizeX - 1;
            var lastZ = sizeZ - 1;
            var halfX = sizeX / 2;
            var halfZ = sizeZ / 2;
            for (var z = lastZ - keepSteps; z >= halfZ; z -= 1)
            {
                for (var x = lastX - keepSteps; x >= halfX; x -= 1)
                {
                    var px = grid[x + 1, z];
                    var pz = grid[x, z + 1];
                    if (px < lowest)
                    {
                        px += 1;
                    }
                    if (pz < lowest)
                    {
                        pz += 1;
                    }
                    var v = Mathf.Min (px, pz);
                    grid[x, z] = v;
                    store(x, z, v, byte.MinValue, byte.MinValue);
                }
            }
        }

        static void FillSouthEastQuadrant (int sizeX, int sizeZ, int[,] grid, int lowest, AreaManager.StoreHeightmapInfo store)
        {
            var lastX = sizeX - 1;
            var halfX = sizeX / 2;
            var halfZ = sizeZ / 2;
            for (var z = keepSteps; z < halfZ; z += 1)
            {
                for (var x = lastX - keepSteps; x >= halfX; x -= 1)
                {
                    var px = grid[x + 1, z];
                    var pz = grid[x, z - 1];
                    if (px < lowest)
                    {
                        px += 1;
                    }
                    if (pz < lowest)
                    {
                        pz += 1;
                    }
                    var v = Mathf.Min (px, pz);
                    grid[x, z] = v;
                    store(x, z, v, byte.MinValue, byte.MinValue);
                }
            }
        }
    }
}
