using System.Collections.Generic;
using System.Text;

using UnityEngine;

namespace Area
{
    using Scene;

    sealed class AreaSceneSpotInfo
    {
        public void OnDestroy ()
        {
            bb.onLevelLoaded -= OnLevelLoaded;
        }

        public void Update (int spotIndex)
        {
            var (sok, spot) = ValidateHoveredSpot (spotIndex);
            if (!sok)
            {
                lastSpotInfoBuilder.Clear ();
                return;
            }

            if (bb.lastSpotHovered == spot
                && bb.lastSpotTilesetID == spot.blockTileset
                && lastSpotGroup == spot.blockGroup
                && lastSpotSubtype == spot.blockSubtype)
            {
                return;
            }

            Update (spot);
        }

        public void Update (AreaVolumePoint spotHovered)
        {
            var (tok, tileset, definition) = ValidateSpotTileset (spotHovered);
            if (!tok)
            {
                lastSpotInfoBuilder.Clear ();
                return;
            }
            BuildSpotInfo (spotHovered, tileset, definition);
        }

        (bool, AreaVolumePoint) ValidateHoveredSpot (int spotIndex)
        {
            if (!spotIndex.IsValidIndex (bb.am.points))
            {
                ResetHoverInfo ();
                return (false, null);
            }

            var spotHovered = bb.am.points[spotIndex];
            if (spotHovered.spotConfiguration == TilesetUtility.configurationEmpty)
            {
                ResetHoverInfo ();
                return (false, null);
            }

            if (spotHovered.pointState == AreaVolumePointState.Full && spotHovered.spotConfiguration == TilesetUtility.configurationFull)
            {
                spotHovered = spotHovered.pointsWithSurroundingSpots[WorldSpace.SpotNeighbor.Up];
                if (spotHovered == null)
                {
                    ResetHoverInfo ();
                    return (false, null);
                }
            }

            return (true, spotHovered);
        }

        (bool, AreaTileset, AreaBlockDefinition) ValidateSpotTileset (AreaVolumePoint spot)
        {
            ref var lastSpotTilesetID = ref bb.lastSpotTilesetID;
            ref var lastSpotInfoGroups = ref bb.lastSpotInfoGroups;

            lastSpotTilesetID = spot.blockTileset;
            lastSpotGroup = spot.blockGroup;
            lastSpotSubtype = spot.blockSubtype;

            if (!AreaTilesetHelper.database.tilesets.TryGetValue (spot.blockTileset, out var tileset) || tileset == null)
            {
                ResetHoverInfo ();
                return (false, null, null);
            }

            if (AreaManager.IsPointTerrain(spot))
            {
                lastSpotInfoBuilder.Clear ();
                lastSpotInfoBuilder.AppendFormat ("█ {0} ({1})", spot.spotConfiguration, AreaSceneHelper.GetPointConfigurationDisplayString (spot.spotConfiguration));
                lastSpotInfoBuilder.Append ("\n█ ");
                lastSpotInfoBuilder.Append (AreaSceneHelper.GetTilesetDisplayName (spot.blockTileset));
                lastSpotInfoGroups = lastSpotInfoBuilder.ToString ();
                bb.lastSpotHovered = spot;
                bb.lastSpotType = SpotHoverType.Terrain;
                lastSpotTilesetID = spot.blockTileset;
                lastSpotGroup = 0;
                lastSpotSubtype = 0;
                return (false, null, null);
            }

            var definition = tileset.blocks[spot.spotConfiguration];
            if (definition == null)
            {
                ResetHoverInfo ();
                return (false, null, null);
            }

            bb.lastSpotHovered = spot;
            bb.lastSpotType = SpotHoverType.Editable;
            return (true, tileset, definition);
        }

        void ResetHoverInfo ()
        {
            bb.lastSpotInfoGroups = "";
            bb.lastSpotHovered = null;
            bb.lastSpotTilesetID = -1;
            lastSpotGroup = 0;
            lastSpotSubtype = 0;
            bb.lastSpotType = SpotHoverType.Empty;
        }

        void BuildSpotInfo(AreaVolumePoint lastSpotHovered, AreaTileset tileset, AreaBlockDefinition definition)
        {
            lastSpotInfoBuilder.Clear ();
            var configuration = lastSpotHovered.spotHasDamagedPoints ? lastSpotHovered.spotConfigurationWithDamage : lastSpotHovered.spotConfiguration;
            lastSpotInfoBuilder.AppendFormat ("█ {0} ({1})", configuration, AreaSceneHelper.GetPointConfigurationDisplayString (configuration));
            if (lastSpotHovered.blockRotation != 0)
            {
                lastSpotInfoBuilder.AppendFormat (" rot={0}", lastSpotHovered.blockRotation);
            }
            if (lastSpotHovered.blockFlippedHorizontally)
            {
                lastSpotInfoBuilder.Append (" flipped");
            }
            lastSpotInfoBuilder.Append ("\n█ ");
            lastSpotInfoBuilder.Append (AreaSceneHelper.GetTilesetDisplayName(tileset));
            lastSpotInfoBuilder.Append ("\n");

            var identifiersPresent = tileset.groupIdentifiers != null;
            foreach (var kvp in definition.subtypeGroups)
            {
                if (UnfinishedTiles.TryGetValue (tileset.id, out var matches) && matches.Contains (configuration << 8 | kvp.Key))
                {
                    continue;
                }
                BuildGroupSubtypeInfo (tileset, identifiersPresent, kvp.Key, kvp.Value);
            }

            bb.lastSpotInfoGroups = lastSpotInfoBuilder.ToString ();
        }

        void BuildGroupSubtypeInfo (AreaTileset tileset, bool identifiersPresent, byte group, SortedDictionary<byte, GameObject> map)
        {
            lastSpotInfoBuilder.Append ("\n");

            var lastSpotHovered = bb.lastSpotHovered;
            var groupMatch = group == lastSpotHovered.blockGroup;
            if (groupMatch)
            {
                lastSpotInfoBuilder.AppendFormat ("<b><color=#00ffffff>{0}</color></b>", group);
            }
            else
            {
                lastSpotInfoBuilder.Append (group);
            }

            lastSpotInfoBuilder.Append (": ");
            var first = true;
            foreach (var subtype in map.Keys)
            {
                if (!first)
                {
                    lastSpotInfoBuilder.Append (" - ");
                }

                var subtypeMatch = groupMatch && subtype == lastSpotHovered.blockSubtype;
                if (subtypeMatch)
                {
                    lastSpotInfoBuilder.AppendFormat ("<b><color=#00ffffff>{0}</color></b>", subtype);
                }
                else
                {
                    lastSpotInfoBuilder.Append (subtype);
                }

                first = false;
            }

            var identifierFound = identifiersPresent && tileset.groupIdentifiers.ContainsKey (group);
            if (identifierFound)
            {
                lastSpotInfoBuilder.Append (" <size=9>");
                if (groupMatch)
                {
                    lastSpotInfoBuilder.AppendFormat ("<b>{0}</b>", tileset.groupIdentifiers[group]);
                }
                else
                {
                    lastSpotInfoBuilder.Append (tileset.groupIdentifiers[group]);
                }
                lastSpotInfoBuilder.Append ("</size>");
            }
        }

        void OnLevelLoaded ()
        {
            ResetHoverInfo ();
        }

        public static void CreateInstance (AreaSceneBlackboard bb) => bb.spotInfo = new AreaSceneSpotInfo (bb);

        AreaSceneSpotInfo (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            bb.onLevelLoaded += OnLevelLoaded;
        }

        readonly AreaSceneBlackboard bb;
        readonly StringBuilder lastSpotInfoBuilder = new StringBuilder ();
        int lastSpotGroup;
        int lastSpotSubtype;

        public static readonly Dictionary<int, HashSet<int>> UnfinishedTiles = new Dictionary<int, HashSet<int>> ()
        {
            [10] = new HashSet<int> ()
            {
                TilesetUtility.configurationFloor << 8 | 1,
            },
            [30] = new HashSet<int> ()
            {
                TilesetUtility.configurationFloor << 8 | 1,
            },
            [50] = new HashSet<int> ()
            {
                TilesetUtility.configurationFloor << 8 | 1,
            },
        };
    }
}
