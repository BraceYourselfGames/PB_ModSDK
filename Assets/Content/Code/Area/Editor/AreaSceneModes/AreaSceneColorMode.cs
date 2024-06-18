using System.Collections.Generic;

using UnityEngine;

namespace Area
{
    using Scene;

    sealed class AreaSceneColorMode : AreaSceneMode
    {
        public EditingMode EditingMode => EditingMode.Color;

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
                    Edit (button);
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

        void Edit (KeyCode mouseButton)
        {
            var am = bb.am;
            var spotsActedOn = new List<AreaVolumePoint> ();
            var startingSpot = bb.lastSpotHovered;
            bb.gizmos.search.CheckSearchRequirements (am, startingSpot, bb.currentSearchType, ref spotsActedOn);
            if (spotsActedOn == null)
            {
                return;
            }

            if (spotsActedOn.Count > 1)
            {
                Debug.Log ("AM (I) | EditColor | Spots acted on: " + spotsActedOn.Count);
            }

            var tilesetColor = bb.tilesetColor;
            if (mouseButton == KeyCode.Mouse0)
            {
                foreach (var spot in spotsActedOn)
                {
                    var properties = spot.customization;

                    if (tilesetColor.ApplyOverlaysOnColorApply)
                    {
                        properties.overrideIndex = tilesetColor.OverrideValue;
                    }

                    if (tilesetColor.ApplyMainOnColorApply)
                    {
                        properties.huePrimary = tilesetColor.SelectedPrimaryColor.h;
                        properties.saturationPrimary = tilesetColor.SelectedPrimaryColor.s;
                        properties.brightnessPrimary = tilesetColor.SelectedPrimaryColor.b;
                        properties.hueSecondary = tilesetColor.SelectedSecondaryColor.h;
                        properties.saturationSecondary = tilesetColor.SelectedSecondaryColor.s;
                        properties.brightnessSecondary = tilesetColor.SelectedSecondaryColor.b;
                    }

                    am.ApplyShaderPropertiesAtPoint (spot, properties, true, false, false);
                }
            }
            else if (mouseButton == KeyCode.Mouse1)
            {
                foreach (var spot in spotsActedOn)
                {
                    am.ApplyShaderPropertiesAtPoint (spot, TilesetVertexProperties.defaults, true, false, false);
                }
            }
            else if (mouseButton == KeyCode.Mouse2)
            {
                /*if(absoluteColorMode)
                {
                    var tileColorPrimary = AreaTilesetHelper.GetTileset(startingPoint.blockTileset).primaryColor;
                    var tileColorSecondary = AreaTilesetHelper.GetTileset(startingPoint.blockTileset).secondaryColor;

                    var colorPrimary = HSBColor.FromColor(new Color(tileColorPrimary.x, tileColorPrimary.y, tileColorPrimary.z));
                    var colorSecondary = HSBColor.FromColor(new Color(tileColorSecondary.x, tileColorSecondary.y, tileColorSecondary.z));

                    var shiftPrimary = new HSBColor(startingPoint.customization.huePrimary, startingPoint.customization.saturationPrimary, startingPoint.customization.brightnessPrimary);
                    var shiftSecondary = new HSBColor(startingPoint.customization.hueSecondary, startingPoint.customization.saturationSecondary, startingPoint.customization.brightnessSecondary);

                    selectedPrimaryColor = ApplyColorShift(colorPrimary, shiftPrimary);
                    selectedSecondaryColor = ApplyColorShift(colorSecondary, shiftSecondary);
                }
                else
                {*/
                    tilesetColor.SelectedPrimaryColor = new HSBColor(startingSpot.customization.huePrimary, startingSpot.customization.saturationPrimary, startingSpot.customization.brightnessPrimary);
                    tilesetColor.SelectedSecondaryColor = new HSBColor(startingSpot.customization.hueSecondary, startingSpot.customization.saturationSecondary, startingSpot.customization.brightnessSecondary);
                //}

                tilesetColor.SelectedTilesetId = startingSpot.blockTileset;
                tilesetColor.OverrideValue = startingSpot.customization.overrideIndex;
            }
        }

        public static AreaSceneMode CreateInstance (AreaSceneBlackboard bb) => new AreaSceneColorMode (bb);

        AreaSceneColorMode (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            Panel = new AreaSceneColorModePanel (bb);
        }

        readonly AreaSceneBlackboard bb;
    }
}
