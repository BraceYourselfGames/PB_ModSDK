using System.Collections.Generic;

using UnityEngine;

namespace Area
{
    using Scene;

    sealed class AreaSceneSpotMode : AreaSceneMode
    {
        public EditingMode EditingMode => EditingMode.Spot;
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
                case EventType.MouseDown:
                case EventType.ScrollWheel:
                case EventType.KeyUp:
                    Edit (button, e.shift);
                    return true;
            }
            return false;
        }

        public void OnHoverEnd () => bb.gizmos.cursor.HideCursor ();

        public bool HandleSceneUserInput (Event e) => false;

        public void DrawSceneMarkup (Event e, System.Action repaint)
        {
            AreaSceneModeHelper.TryRebuildTerrain (bb);
        }


        void Edit (KeyCode button, bool shift)
        {
            var am = bb.am;
            var startingSpot = bb.lastSpotHovered;

            if (startingSpot.spotConfiguration == TilesetUtility.configurationEmpty || startingSpot.spotConfiguration == TilesetUtility.configurationFull)
            {
                Debug.LogFormat
                (
                    "AM | EditBlockAtIndex | Index {0} has configuration {1} ({2}) and shouldn't be possible to select",
                    startingSpot.spotIndex,
                    startingSpot.spotConfiguration,
                    TilesetUtility.GetStringFromConfiguration (startingSpot.spotConfiguration)
                );
                return;
            }

            // LMB                      - rotate
            // RMB                      - flip
            // MMB                      - copy block

            // ^ (scroll up)            - subtype next
            // v (scroll down)          - subtype prev
            // [ (bracket left)         - group prev
            // ] (bracket right)        - group next
            // < (arrow left / comma)   - paste group, subtype
            // > (arrow right / period) - paste group, subtype, rotation, flipping

            var data = AreaTilesetHelper.database.configurationDataForBlocks[startingSpot.spotConfiguration];

            if (button == KeyCode.Mouse0)
            {
                if (data.customRotationPossible)
                {
                    startingSpot.blockRotation = startingSpot.blockRotation.OffsetAndWrap (true, 3);
                    am.RebuildBlock (startingSpot, false);
                    bb.spotInfo.Update (startingSpot);
                }
                return;
            }

            if (button == KeyCode.Mouse1)
            {
                if (data.customFlippingMode != -1)
                {
                    startingSpot.blockFlippedHorizontally = !startingSpot.blockFlippedHorizontally;
                    am.RebuildBlock (startingSpot, false);
                    bb.spotInfo.Update (startingSpot);
                }
                return;
            }

            if (button == KeyCode.Mouse2)
            {
                var clipboardTilesetInfo = bb.clipboardTilesetInfo;
                clipboardTilesetInfo.Configurations = TilesetUtility.GetConfigurationTransformations (startingSpot.spotConfiguration);
                clipboardTilesetInfo.Tileset = startingSpot.blockTileset;
                clipboardTilesetInfo.Group = startingSpot.blockGroup;
                clipboardTilesetInfo.Subtype = startingSpot.blockSubtype;
                clipboardTilesetInfo.Rotation = startingSpot.blockRotation;
                clipboardTilesetInfo.Flipping = startingSpot.blockFlippedHorizontally;
                clipboardTilesetInfo.Color = startingSpot.customization;

                //AreaManager.pointTestIndex = startingPoint.spotIndex;
                return;
            }

            if (button == KeyCode.V)
            {
                PasteTileset (startingSpot, shift, bb.gizmos.search, bb.currentSearchType);
                return;
            }

            if (button == KeyCode.Q)
            {
                var spotsActedOn = new List<AreaVolumePoint> ();
                bb.gizmos.search.CheckSearchRequirements (am, startingSpot, bb.currentSearchType, ref spotsActedOn);
                // XXX fix this; have method return a result value and check that instead.
                if (spotsActedOn != null)
                {
                    Debug.Log ("AM (I) | EditBlock | Randomizing targeted spots | Spots acted on: " + spotsActedOn.Count);
                    for (var i = 0; i < spotsActedOn.Count; ++i)
                    {
                        var spot = spotsActedOn[i];
                        var configurationData = AreaTilesetHelper.database.configurationDataForBlocks[spot.spotConfiguration];

                        if
                        (
                            AreaTilesetHelper.database.tilesets.ContainsKey (spot.blockTileset) &&
                            AreaTilesetHelper.database.tilesets[spot.blockTileset].blocks[spot.spotConfiguration] != null &&
                            AreaTilesetHelper.database.tilesets[spot.blockTileset].blocks[spot.spotConfiguration].subtypeGroups.ContainsKey (spot.blockGroup)
                        )
                        {
                            var subtypes = AreaTilesetHelper.database.tilesets[spot.blockTileset].blocks[spot.spotConfiguration].subtypeGroups[spot.blockGroup];
                            spot.blockSubtype = subtypes.GetRandomKey ();

                            if (configurationData.customRotationPossible)
                            {
                                spot.blockRotation = (byte)Random.Range (0, 4);
                            }

                            am.RebuildBlock (spot, false);
                        }
                    }
                }

                return;
            }

            if (!AreaTilesetHelper.database.tilesets.TryGetValue(startingSpot.blockTileset, out var tileset))
            {
                Debug.LogWarningFormat ("Missing tileset | index: {0} | tileset ID: {1}", startingSpot.spotIndex, startingSpot.blockTileset);
                return;
            }

            var definition = tileset.blocks[startingSpot.spotConfiguration];
            if (definition == null)
            {
                Debug.LogWarningFormat ("Missing definition for tileset | index: {0} | tileset ID: {1} | configuration: {2}", startingSpot.spotIndex, tileset.id, startingSpot.spotConfiguration);
                return;
            }

            if (button == KeyCode.LeftBracket || button == KeyCode.RightBracket)
            {
                var forward = button == KeyCode.LeftBracket;
                var groupKeyNew = AreaTilesetHelper.OffsetBlockGroup (definition, startingSpot.blockGroup, forward);
                if (groupKeyNew != startingSpot.blockGroup)
                {
                    startingSpot.blockGroup = groupKeyNew;
                    startingSpot.blockSubtype = AreaTilesetHelper.EnsureSubtypeInGroup (definition, startingSpot.blockGroup, startingSpot.blockSubtype);
                    am.RebuildBlock (startingSpot, false);
                }
                return;
            }

            if (button == KeyCode.PageDown || button == KeyCode.PageUp)
            {
                var forward = button == KeyCode.PageUp;
                var subtypeKeyNew = AreaTilesetHelper.OffsetBlockSubtype (definition, startingSpot.blockGroup, startingSpot.blockSubtype, forward);
                if (subtypeKeyNew != startingSpot.blockSubtype)
                {
                    startingSpot.blockSubtype = subtypeKeyNew;
                    am.RebuildBlock (startingSpot, false);
                }
            }
        }

        void PasteTileset (AreaVolumePoint startingSpot, bool shift, AreaSceneSearch search, SpotSearchType searchType)
        {
            var am = bb.am;
            var clipboardTilesetInfo = bb.clipboardTilesetInfo;

            if (shift)
            {
                if (clipboardTilesetInfo.Configurations.Contains (startingSpot.spotConfiguration))
                {
                    startingSpot.blockTileset = clipboardTilesetInfo.Tileset;
                    startingSpot.blockGroup = clipboardTilesetInfo.Group;
                    startingSpot.blockSubtype = clipboardTilesetInfo.Subtype;
                    startingSpot.blockRotation = clipboardTilesetInfo.Rotation;
                    startingSpot.blockFlippedHorizontally = clipboardTilesetInfo.Flipping;
                    am.RebuildBlock (startingSpot, false);
                }
                return;
            }

            var spotsActedOn = new List<AreaVolumePoint> ();
            search.CheckSearchRequirements (am, startingSpot, searchType, ref spotsActedOn);
            if (spotsActedOn == null)
            {
                return;
            }

            Debug.Log ("AM (I) | EditBlock | Replacing targeted spots | Spots acted on: " + spotsActedOn.Count);
            for (var i = 0; i < spotsActedOn.Count; ++i)
            {
                var doRebuild = false;
                var spot = spotsActedOn[i];
                if (clipboardTilesetInfo.MustOverwriteSubtype)
                {
                    if (clipboardTilesetInfo.Configurations.Contains (spot.spotConfiguration))
                    {
                        spot.blockTileset = clipboardTilesetInfo.Tileset;
                        spot.blockGroup = clipboardTilesetInfo.Group;
                        spot.blockSubtype = clipboardTilesetInfo.Subtype;
                        doRebuild = true;
                    }
                }
                else
                {
                    if
                    (
                        AreaTilesetHelper.database.tilesets.ContainsKey (spot.blockTileset) &&
                        AreaTilesetHelper.database.tilesets[spot.blockTileset].blocks[spot.spotConfiguration] != null &&
                        AreaTilesetHelper.database.tilesets[spot.blockTileset].blocks[spot.spotConfiguration].subtypeGroups.ContainsKey (clipboardTilesetInfo.Group)
                    )
                    {
                        spot.blockTileset = clipboardTilesetInfo.Tileset;
                        spot.blockGroup = clipboardTilesetInfo.Group;
                        doRebuild = true;
                    }
                }

                if (clipboardTilesetInfo.OverwriteColor)
                {
                    spot.customization = clipboardTilesetInfo.Color;
                    doRebuild = true;
                }

                if (doRebuild)
                {
                    am.RebuildBlock (spot, false);
                }
            }
        }

        public static AreaSceneMode CreateInstance (AreaSceneBlackboard bb) => new AreaSceneSpotMode (bb);

        AreaSceneSpotMode (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            Panel = new AreaSceneSpotModePanel (bb);
        }

        readonly AreaSceneBlackboard bb;
    }
}
