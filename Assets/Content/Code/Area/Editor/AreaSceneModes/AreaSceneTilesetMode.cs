using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Area
{
    using Scene;

    sealed class AreaSceneTilesetMode : AreaSceneMode
    {
        public EditingMode EditingMode => EditingMode.Tileset;

        public AreaSceneModePanel Panel { get; }

        public void OnDisable () => Panel.OnDisable ();
        public void OnDestroy () { }

        public int LayerMask => AreaSceneCamera.environmentLayerMask;

        public bool Hover (Event e, RaycastHit hitInfo)
        {
            if (!AreaSceneModeHelper.DisplaySpotCursor (bb, hitInfo))
            {
                return false;
            }
            var (eventType, button) = AreaSceneModeHelper.ResolveEvent (e);
            switch (eventType)
            {
                case EventType.ScrollWheel:
                    ChangeSelectedTileset (e);
                    return true;
                case EventType.KeyUp:
                    return ChangeSearchType (button);
                case EventType.MouseDown:
                    Edit (button);
                    return true;
            }
            return false;
        }

        public void OnHoverEnd () => bb.gizmos.cursor.HideCursor ();

        public bool HandleSceneUserInput (Event e) => false;

        public void DrawSceneMarkup (Event e, Action repaint)
        {
            AreaSceneModeHelper.TryRebuildTerrain (bb);
        }

        void ChangeSelectedTileset (Event e)
        {
            var forward = e.delta.y > 0f;
            ref var editingTilesetSelected = ref bb.spotTilesetSelected;
            var tilesetKeyNew = AreaTilesetHelper.OffsetBlockTileset (editingTilesetSelected.id, forward);
            if (tilesetKeyNew != editingTilesetSelected.id)
            {
                editingTilesetSelected = AreaTilesetHelper.database.tilesets[tilesetKeyNew];
            }
        }

        bool ChangeSearchType (KeyCode button)
        {
            if (button != KeyCode.LeftBracket && button != KeyCode.RightBracket)
            {
                return false;
            }

            var forward = button == KeyCode.RightBracket;
            var currentSearchTypeInt = (int)bb.currentSearchType;
            currentSearchTypeInt = currentSearchTypeInt.OffsetAndWrap (forward, 6);
            bb.currentSearchType = (SpotSearchType)currentSearchTypeInt;
            return true;
        }

        void Edit (KeyCode button)
        {
            var spot = bb.lastSpotHovered;
            switch (button)
            {
                case KeyCode.Mouse0:
                    ReplaceTile (spot);
                    break;
                case KeyCode.Mouse2:
                    bb.spotTilesetSelected = AreaTilesetHelper.database.tilesets[spot.blockTileset];
                    break;
            }
        }

        void ReplaceTile (AreaVolumePoint startingSpot)
        {
            var am = bb.am;
            ref var editingTilesetSelected = ref bb.spotTilesetSelected;
            if (editingTilesetSelected == null)
            {
                return;
            }

            var spotsActedOn = new List<AreaVolumePoint> ();
            bb.gizmos.search.CheckSearchRequirements (am, startingSpot, bb.currentSearchType, ref spotsActedOn);
            // XXX fix this; have method return a result value and check that instead.
            if (spotsActedOn == null || spotsActedOn.Count == 0)
            {
                return;
            }

            // XXX remove once search is fixed.
            //Debug.LogFormat ("Edit tileset | start index: {0} | search: {1} | found: {2}", startingSpot.spotIndex, bb.currentSearchType, spotsActedOn.Count);
            var usingTerrain = editingTilesetSelected.id == AreaTilesetHelper.idOfTerrain;
            var terrainModified = false;
            foreach (var spot in spotsActedOn)
            {
                if (spot.blockTileset == editingTilesetSelected.id)
                {
                    continue;
                }

                #if PB_MODSDK
                if (AreaManager.IsSpotRoad (spot) && spot.spotConfiguration == TilesetUtility.configurationFloor)
                {
                    UpdateRoadPointWithSpot (spot);
                }
                #endif
                terrainModified |= usingTerrain || AreaManager.IsPointTerrain (spot);
                if (usingTerrain)
                {
                    EnsureSingleTerrainSpotInVerticalStack (spot);
                }
                spot.blockTileset = editingTilesetSelected.id;
                am.RebuildBlock (spot, false);
            }

            if (terrainModified)
            {
                var sceneHelper = CombatSceneHelper.ins;
                sceneHelper.terrain.Rebuild (true);
            }
        }

        void EnsureSingleTerrainSpotInVerticalStack (AreaVolumePoint spot)
        {
            // The given spot is the one being changed to a terrain spot. If there is another spot that
            // is using the terrain tileset in the vertical stack, either above or below, change that
            // spot to use the fallback tileset.

            if ((spot.spotConfiguration & TilesetUtility.configurationBitmaskSelf) == 0)
            {
                return;
            }

            var am = bb.am;
            var bounds = am.boundsFull;
            var maxY = bounds.y;
            var volumePosition = spot.pointPositionIndex;
            var targetY = volumePosition.y;
            for (var y = 0; y < maxY; y += 1)
            {
                if (y == targetY)
                {
                    continue;
                }

                volumePosition.y = y;
                var index = AreaUtility.GetIndexFromVolumePosition (volumePosition, bounds, skipBoundsCheck: true);
                spot = am.points[index];
                if (!AreaManager.IsPointTerrain (spot))
                {
                    continue;
                }

                spot.blockTileset = AreaTilesetHelper.idOfFallback;
                am.RebuildBlock (spot, false);
                break;
            }
        }

        void UpdateRoadPointWithSpot (AreaVolumePoint spot)
        {
            var position = spot.pointPositionIndex;
            var clearedRoadPoints = roadPointOffsets.Where (rpo => rpo.Spots.All(rso => IsRoadSpotCleared (position, rso)));
            foreach (var rpo in clearedRoadPoints)
            {
                var rpp = position + rpo.Point;
                var index = AreaUtility.GetIndexFromVolumePosition (rpp, bb.am.boundsFull);
                if (!index.IsValidIndex (bb.am.points))
                {
                    continue;
                }
                var point = bb.am.points[index];
                point.road = false;
            }
        }

        bool IsRoadSpotCleared (Vector3Int position, Vector3Int offset)
        {
            var rsp = position + offset;
            var index = AreaUtility.GetIndexFromVolumePosition (rsp, bb.am.boundsFull);
            if (!index.IsValidIndex (bb.am.points))
            {
                return true;
            }
            var point = bb.am.points[index];
            return point.blockTileset != AreaTilesetHelper.idOfRoad;
        }

        public static AreaSceneMode CreateInstance (AreaSceneBlackboard bb) => new AreaSceneTilesetMode (bb);

        AreaSceneTilesetMode (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            Panel = AreaSceneTilesetModePanel.Create (bb);
        }

        readonly AreaSceneBlackboard bb;

        static readonly RoadPoint[] roadPointOffsets =
        {
            new RoadPoint ()
            {
                Point = new Vector3Int (0, 1, 0),
                Spots = new[]
                {
                    new Vector3Int (0, 0, -1),
                    new Vector3Int (-1, 0, 0),
                    new Vector3Int (-1, 0, -1),
                },
            },
            new RoadPoint ()
            {
                Point = new Vector3Int (1, 1, 0),
                Spots = new[]
                {
                    new Vector3Int (0, 0, -1),
                    new Vector3Int (1, 0, 0),
                    new Vector3Int (1, 0, -1),
                },
            },
            new RoadPoint ()
            {
                Point = new Vector3Int (1, 1, 1),
                Spots = new[]
                {
                    new Vector3Int (0, 0, 1),
                    new Vector3Int (1, 0, 0),
                    new Vector3Int (1, 0, 1),
                },
            },
            new RoadPoint ()
            {
                Point = new Vector3Int (0, 1, 1),
                Spots = new[]
                {
                    new Vector3Int (0, 0, 1),
                    new Vector3Int (-1, 0, 0),
                    new Vector3Int (-1, 0, 1),
                },
            },
        };

        sealed class RoadPoint
        {
            public Vector3Int Point;
            public Vector3Int[] Spots;
        }
    }
}
