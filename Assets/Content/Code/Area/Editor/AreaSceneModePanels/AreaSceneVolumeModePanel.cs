using System.Text;

using CustomRendering;

using Sirenix.OdinInspector;
using Sirenix.Utilities;

using Unity.Entities;
using UnityEditor;
using UnityEngine;

namespace Area
{
    using Scene;

    sealed class AreaSceneVolumeModePanel : AreaSceneModePanel
    {
        public interface Checks
        {
            bool denyAddBlockTop { get; }
            bool denyRemoveBlock { get; }
            System.Action onWarning { get; set; }
        }

        public GUILayoutOptions.GUILayoutOptionsInstance Width => AreaScenePanelDrawer.ModePanelWidth;
        public string Title => "Structure shape mode";

        public void OnDisable ()
        {
            pointDisplay.OnDisable ();
            brushSelector.OnDisable ();
            controls.OnDisable ();
            tilesetSelection.OnDisable ();
        }

        public void Draw ()
        {
            if (Event.current.type == EventType.Layout)
            {
                pointCached = bb.lastPointHovered;
                cachedHover = bb.hoverActive;
            }

            GUILayout.Space (4f);
            pointDisplay.Draw (pointCached, cachedHover);

            if (pointCached == null)
            {
                return;
            }

            GUILayout.Space (4f);
            controls.Draw ();

            GUILayout.Space (4f);
            GUILayout.BeginVertical ("Box");
            brushSelector.Draw ();
            GUILayout.EndVertical ();

            DrawTilesetInfo (pointCached);
            if (bb.displayVolumeDebugInfo)
            {
                DrawDebugInfo (pointCached);
            }
        }

        public AreaSceneModeHints Hints
        {
            get
            {
                hints.LeaderText = "";
                if (!bb.hoverActive)
                {
                    showWarning = false;
                    hints.HintText = standardHint;
                    return hints;
                }

                hints.HintText = checks.denyAddBlockTop ? deniedAddHint : checks.denyRemoveBlock ? deniedRemoveHint : standardHint;
                if (!showWarning)
                {
                    return hints;
                }

                var elapsedTime = Time.realtimeSinceStartup - warningTimeStart;
                if (elapsedTime > warningDuration)
                {
                    showWarning = false;
                    return hints;
                }

                var interpolant = elapsedTime.RemapTo01 (1.125f, warningDuration);
                var alpha = Mathf.RoundToInt(Mathf.Lerp (byte.MaxValue, byte.MinValue, interpolant));
                var fmt = checks.denyAddBlockTop ? deniedAddWarningFormat : deniedRemoveWarningFormat;
                hints.LeaderText = string.Format (fmt, alpha);
                return hints;
            }
        }

        void DrawTilesetInfo (AreaVolumePoint pointHovered)
        {
            GUILayout.Space (4f);
            GUILayout.BeginVertical ("Box");

            tilesetSelection.Draw ();
            GUILayout.Space (8f);
            GUILayout.Label ("Tilesets");

            var compass = AreaSceneHelper.GetCompassFromDirection (bb.pointerDirection);
            GUILayout.Label ("Face: " + AreaSceneHelper.GetCompassDisplayStringForFace(compass), EditorStyles.miniLabel);

            var cubeFaceSpots = AreaSceneHelper.GetSpotsForBlockFaceWithLabels(bb.am, compass, pointHovered);
            foreach (var (label, spot) in cubeFaceSpots)
            {
                DrawTilesetForSpot (label, spot);
            }

            GUILayout.EndVertical ();
        }

        void DrawTilesetForSpot (string quadrant, AreaVolumePoint spot)
        {
            if (string.IsNullOrEmpty (quadrant))
            {
                return;
            }

            var cachedColor = GUI.contentColor;
            if (spot == null)
            {
                GUI.contentColor = Color.gray;
                GUILayout.Label (quadrant + ": null", EditorStyles.miniLabel);
                GUI.contentColor = cachedColor;
                return;
            }
            if (AreaSceneHelper.IsFreeSpace(spot))
            {
                GUI.contentColor = Color.gray;
                GUILayout.Label (string.Format ("{0} ({1}): empty", quadrant, spot.spotIndex), EditorStyles.miniLabel);
                GUI.contentColor = cachedColor;
                return;
            }

            var spotTileset = string.Format ("{0} ({1}): {2}", quadrant, spot.spotIndex, AreaSceneHelper.GetTilesetDisplayName (spot.blockTileset));
            if (spot.spotConfiguration != TilesetUtility.configurationEmpty && spot.spotConfiguration != TilesetUtility.configurationFull)
            {
                spotTileset += string.Format (" {0} {1}/{2}", spot.spotConfiguration, spot.blockGroup, spot.blockSubtype);
            }
            if (spot.terrainOffset != 0f)
            {
                spotTileset += string.Format (" {0:+0.##;-0.##}", spot.terrainOffset);
            }
            GUILayout.Label (spotTileset, EditorStyles.miniLabel);
        }

        void DrawDebugInfo (AreaVolumePoint pointHovered)
        {
            GUILayout.Space (8f);
            GUILayout.BeginVertical ("Box");

            GUILayout.Label ("Volume position: " + pointHovered.pointPositionIndex, EditorStyles.miniLabel);
            GUILayout.Label ("Local position: " + pointHovered.pointPositionLocal, EditorStyles.miniLabel);
            GUILayout.Label ("Instance position: " + pointHovered.instancePosition, EditorStyles.miniLabel);

            GUILayout.Space (4f);
            var destructibility = pointHovered.indestructibleIndirect
                ? "indestructible (indirect)"
                : pointHovered.destructionUntracked
                    ? "untracked"
                    : pointHovered.destructible
                        ? "destructible"
                        : "indestructible";
            GUILayout.Label ("Destruction: " + destructibility, EditorStyles.miniLabel);
            GUILayout.Label ("Integrity (main): " + pointHovered.integrity.ToString("0.###"), EditorStyles.miniLabel);
            GUILayout.Label ("Integrity (anim.): " + pointHovered.integrityForDestructionAnimation.ToString("0.###"), EditorStyles.miniLabel);
            GUILayout.Label ("Interior (vis.): " + pointHovered.instanceVisibilityInterior.ToString("0.###"), EditorStyles.miniLabel);

            if (AreaManager.world != null && AreaManager.world.EntityManager != null)
            {
                var em = AreaManager.world.EntityManager;

                var entityMainPresent = false;
                if (AreaManager.pointEntitiesMain != null && pointHovered.spotIndex.IsValidIndex (AreaManager.pointEntitiesMain))
                {
                    var entityMain = AreaManager.pointEntitiesMain[pointHovered.spotIndex];
                    entityMainPresent = entityMain != Entity.Null && em.HasComponent<InstancedMeshRenderer> (entityMain);
                }

                var entityInteriorPresent = false;
                if (AreaManager.pointEntitiesInterior != null && pointHovered.spotIndex.IsValidIndex (AreaManager.pointEntitiesInterior))
                {
                    var entityInterior = AreaManager.pointEntitiesInterior[pointHovered.spotIndex];
                    entityInteriorPresent = entityInterior != Entity.Null && em.HasComponent<InstancedMeshRenderer> (entityInterior);
                }

                GUILayout.Space (4f);
                GUILayout.Label ("Entity (main): " + (entityMainPresent ? "present" : "-"), EditorStyles.miniLabel);
                GUILayout.Label ("Entity (interior): " + (entityInteriorPresent ? "present" : "-"), EditorStyles.miniLabel);

                AreaManager.GetShaderDamage (pointHovered, out var damageTop, out var damageBottom, out var damageCritical);

                GUILayout.Space (4f);
                GUILayout.Label ("Sh. integrity (top): " + damageTop, EditorStyles.miniLabel);
                GUILayout.Label ("Sh. integrity (low): " + damageBottom, EditorStyles.miniLabel);
                GUILayout.Label ("Sh. damage (crit.): " + damageCritical, EditorStyles.miniLabel);
            }

            GUILayout.Space (4f);
            GUILayout.Label ("Neighbors", EditorStyles.miniLabel);
            GUILayout.Label (CompactFormat ("C", pointHovered), EditorStyles.miniLabel);
            GUILayout.Label (CompactFormat ("N", pointHovered.pointsInSpot[WorldSpace.PointNeighbor.North]), EditorStyles.miniLabel);
            GUILayout.Label (CompactFormat ("W", pointHovered.pointsWithSurroundingSpots[WorldSpace.SpotNeighbor.West]), EditorStyles.miniLabel);
            GUILayout.Label (CompactFormat ("S", pointHovered.pointsWithSurroundingSpots[WorldSpace.SpotNeighbor.South]), EditorStyles.miniLabel);
            GUILayout.Label (CompactFormat ("E", pointHovered.pointsInSpot[WorldSpace.PointNeighbor.East]), EditorStyles.miniLabel);
            GUILayout.Label (CompactFormat ("SW", pointHovered.pointsWithSurroundingSpots[WorldSpace.SpotNeighbor.SouthWest]), EditorStyles.miniLabel);
            GUILayout.Label (CompactFormat ("NE", pointHovered.pointsInSpot[WorldSpace.PointNeighbor.NorthEast]), EditorStyles.miniLabel);
            GUILayout.Label (CompactFormat ("U", pointHovered.pointsWithSurroundingSpots[WorldSpace.SpotNeighbor.Up]), EditorStyles.miniLabel);
            GUILayout.Label (CompactFormat ("D", pointHovered.pointsInSpot[WorldSpace.PointNeighbor.Down]), EditorStyles.miniLabel);

            GUILayout.EndVertical ();
        }

        string CompactFormat (string label, AreaVolumePoint avp)
        {
            sbDebug.Clear ();
            sbDebug.AppendFormat ("  {0}:", label);
            if (avp == null)
            {
                sbDebug.Append (" null");
                return sbDebug.ToString ();
            }

            sbDebug.AppendFormat
            (
                " {0} {1}{2}{3}{4}{5}",
                avp.spotIndex,
                avp.pointState == AreaVolumePointState.Empty ? "E" : "F",
                avp.spotPresent ? "S" : "-",
                avp.instanceCollider == null ? "-" : avp.instanceCollider.activeSelf ? "C" : "I",
                avp.road ? "R" : "-",
                avp.spotHasDamagedPoints ? "D" : "-"
            );
            if (avp.spotConfiguration == TilesetUtility.configurationEmpty)
            {
                sbDebug.Append (" empty");
            }
            else if (avp.spotConfiguration == TilesetUtility.configurationFull)
            {
                sbDebug.Append (" full");
            }
            else
            {
                var configuration = avp.spotHasDamagedPoints ? avp.spotConfigurationWithDamage : avp.spotConfiguration;
                sbDebug.AppendFormat (" {0} ({1})", configuration, AreaSceneHelper.GetPointConfigurationDisplayString (configuration));
            }
            if (avp.blockTileset != 0)
            {
                sbDebug.AppendFormat
                (
                    "  {0} ({1}/{2}/{3}/{4})",
                    avp.blockTileset,
                    avp.blockGroup,
                    avp.blockSubtype,
                    avp.blockRotation,
                    avp.blockFlippedHorizontally ? "F" : "-"
                );
            }
            if (avp.terrainOffset != 0f)
            {
                sbDebug.AppendFormat (" {0:F2}", avp.terrainOffset);
            }
            return sbDebug.ToString ();
        }

        void OnWarning ()
        {
            showWarning = true;
            warningTimeStart = Time.realtimeSinceStartup;
        }

        public AreaSceneVolumeModePanel (AreaSceneBlackboard bb, Checks checks)
        {
            this.bb = bb;
            brushSelector = new BrushSelector (bb);
            controls = new VolumeModeGeneralControls (bb);
            tilesetSelection = new TilesetSelection (bb, EditingMode.Volume);
            this.checks = checks;
            checks.onWarning += OnWarning;
        }

        readonly AreaSceneBlackboard bb;
        readonly AreaSceneModeHints hints = new AreaSceneModeHints ();
        readonly VolumeModePanelPointDisplay pointDisplay = new VolumeModePanelPointDisplay ();
        readonly BrushSelector brushSelector;
        readonly VolumeModeGeneralControls controls;
        readonly TilesetSelection tilesetSelection;
        readonly Checks checks;
        AreaVolumePoint pointCached;
        bool cachedHover;
        bool showWarning;
        float warningTimeStart;

        static readonly StringBuilder sbDebug = new StringBuilder ();

        const string standardHint = "[LMB] - Add block     [RMB] - Remove block     [MW▲▼] - Change tileset     [Shift + MW▲▼] - Change brush";
        const string deniedAddHint = "<color=#66666699>[LMB] - Add block</color>     [RMB] - Remove block     [MW▲▼] - Change tileset     [Shift + MW▲▼] - Change brush";
        const string deniedRemoveHint = "[LMB] - Add block     <color=#66666699>[RMB] - Remove block</color>     [MW▲▼] - Change tileset     [Shift + MW▲▼] - Change brush";
        const string deniedAddWarningFormat = "<color=#eeee11{0:x2}>!!! Blocks cannot be added above the current block !!!</color>";
        const string deniedRemoveWarningFormat = "<color=#eeee11{0:x2}>!!! Block cannot be removed !!!</color>";

        const float warningDuration = 1.75f;
    }

    sealed class VolumeModePanelPointDisplay : SelfDrawnGUI
    {
        [ShowInInspector]
        [GUIColor (nameof(color))]
        [HideLabel, DisplayAsString, EnableGUI]
        public string pointDisplay => hasPoint
            ? (hover ? "Point: " : "Last point: ") + point.spotIndex
            : "Point: —";

        public void Draw (AreaVolumePoint pointHovered, bool hoverActive)
        {
            point = pointHovered;
            hover = hoverActive;

            GUILayout.BeginVertical ("Box");
            base.Draw ();
            GUILayout.EndVertical ();
        }

        Color color => point == null ? Color.gray : hover ? Color.white : Color.yellow;
        bool hasPoint => point != null;

        AreaVolumePoint point;
        bool hover;
    }

    sealed class VolumeModeGeneralControls : SelfDrawnGUI
    {
        [ShowInInspector]
        [PropertySpace (8f)]
        [ToggleLeft]
        public bool swapTileset
        {
            get => bb.swapTilesetOnVolumeEdits;
            set => bb.swapTilesetOnVolumeEdits = value;
        }

        [ShowInInspector]
        [ToggleLeft]
        public bool overrideTerrainAndRoadTilesets
        {
            get => bb.overrideTerrainAndRoadTilesetsOnVolumeEdits;
            set => bb.overrideTerrainAndRoadTilesetsOnVolumeEdits = value;
        }

        public override void Draw ()
        {
            GUILayout.BeginVertical ("Box");
            base.Draw ();
            GUILayout.EndVertical ();
        }

        public VolumeModeGeneralControls (AreaSceneBlackboard bb)
        {
            this.bb = bb;
        }

        readonly AreaSceneBlackboard bb;
    }
}
