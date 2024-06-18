using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace Area
{
    using Scene;

    sealed class AreaScenePropMode : AreaSceneMode
    {
        public EditingMode EditingMode => EditingMode.Props;

        public AreaSceneModePanel Panel { get; }

        public void OnDisable () => Panel.OnDisable ();
        public void OnDestroy ()
        {
            if (propHolder != null)
            {
                Object.DestroyImmediate (propHolder);
            }
            if (propCursorInstance != null)
            {
                Object.DestroyImmediate (propCursorInstance);
            }
        }

        public int LayerMask => AreaSceneCamera.environmentLayerMask;

        public bool Hover (Event e, RaycastHit hitInfo)
        {
            if (!AreaSceneModeHelper.DisplaySpotCursor (bb, hitInfo, showWireframe: false))
            {
                return false;
            }

            VisualizeProp (e, hitInfo);

            var (eventType, button) = AreaSceneModeHelper.ResolveEvent (e);
            switch (eventType)
            {
                case EventType.KeyUp:
                    return ChangePropOrientation (button);
                case EventType.ScrollWheel:
                    ChangeSelectedProp (e);
                    return true;
                case EventType.MouseDown:
                    Edit (bb.lastSpotHovered.spotIndex, button, e.shift);
                    return true;
            }
            return false;
        }

        public void OnHoverEnd ()
        {
            bb.gizmos.cursor.HideCursor ();
            if (propCursorInstance != null && propCursorInstance.activeSelf)
            {
                propCursorInstance.SetActive (false);
            }
            HideProp ();
        }

        public bool HandleSceneUserInput (Event e)
        {
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Delete)
            {
                // Pressing the delete key will delete the Area game object. That's bad.
                // In prop mode, though, it seems natural to want to press delete to
                // get rid of a selected prop. This key is handled by the Unity editor
                // on key down so we can't wait until key up as with the other keys.
                bb.propEditCommand = PropEditCommand.DeleteSelected;
                return true;
            }
            if (e.type != EventType.KeyUp || !e.shift || e.control)
            {
                bb.propEditCommand = PropEditCommand.None;
                return false;
            }

            switch (e.keyCode)
            {
                case KeyCode.F:
                    bb.propEditCommand = PropEditCommand.RotateLeft;
                    return true;
                case KeyCode.G:
                    bb.propEditCommand = PropEditCommand.Flip;
                    return true;
                case KeyCode.Z:
                    bb.propEditCommand = PropEditCommand.CopyColor;
                    return true;
                case KeyCode.X:
                    bb.propEditCommand = PropEditCommand.PasteColor;
                    return true;
                default:
                    bb.propEditCommand = PropEditCommand.None;
                    return false;
            }
        }

        public void DrawSceneMarkup (Event e, System.Action repaint)
        {
            AreaSceneModeHelper.TryRebuildTerrain (bb);

            if (bb.hoverActive)
            {
                return;
            }

            var am = bb.am;
            var propEditInfo = bb.propEditInfo;

            if (!AreaSceneCamera.Prepare ())
            {
                Debug.LogWarning ("Unable to display prop handles: no scene camera");
                return;
            }

            foreach (var placement in am.placementsProps)
            {
                if (placement == null) // || !placement.prototype.prefab.allowPositionOffset)
                {
                    continue;
                }

                var placementPositionCurrent = (Vector3)placement.state.cachedRootPosition;
                if (!AreaSceneCamera.InView (placementPositionCurrent, AreaSceneCamera.interactionDistance, bb.showOccludedPropHandles))
                {
                    continue;
                }
                if (AreaSceneHelper.OverlapsUI (bb, placementPositionCurrent))
                {
                    continue;
                }

                // Important to make button size constant and distance-independent
                // Otherwise there will be bugs with prop selection, off-screen props get selected if you click on an empty spot, etc.
                var size = HandleUtility.GetHandleSize (placementPositionCurrent);

                Handles.color = Color.white.WithAlpha (0.35f);
                var handleUsed = Handles.Button (placementPositionCurrent, Quaternion.identity, size * handleSize, size * pickSize, Handles.DotHandleCap);

                if (handleUsed)
                {
                    propEditInfo.PlacementHandled = placement;
                    propEditInfo.PlacementListIndex = placement.pivotIndex;
                    repaint ();
                }

                if (propEditInfo.PlacementHandled != placement)
                {
                    continue;
                }
                if (!am.indexesOccupiedByProps.ContainsKey (propEditInfo.PlacementListIndex))
                {
                    continue;
                }

                // Draw origin pivot box
                var point = am.points[placement.pivotIndex];
                Handles.color = Color.white;
                Handles.DrawWireCube(point.instancePosition, handleSizeCube);

                EditorGUI.BeginChangeCheck ();
                var placementPositionModified = Handles.DoPositionHandle (placementPositionCurrent, Quaternion.Euler (0f, -90f * placement.rotation, 0f)); // Quaternion.LookRotation (Vector3.Normalize (point - ...));
                if (!EditorGUI.EndChangeCheck ())
                {
                    continue;
                }

                // Update point variable again to prevent UI selection bugs
                point = am.points[placement.pivotIndex];
                var differenceCounterRotated = Quaternion.Euler (0f, 90f * placement.rotation, 0f) * (placementPositionModified - placementPositionCurrent);
                var differenceInLocalSpace = (Quaternion)placement.state.cachedRootRotation * (placementPositionModified - placementPositionCurrent);

                if (placement.prototype.prefab.compatibility == AreaProp.Compatibility.Floor)
                {
                    placement.offsetX = Mathf.Clamp (placement.offsetX + differenceCounterRotated.x, -1.5f, 1.5f);
                    placement.offsetZ = Mathf.Clamp (placement.offsetZ + differenceCounterRotated.z, -1.5f, 1.5f);
                }
                else if (placement.prototype.prefab.compatibility == AreaProp.Compatibility.WallStraightMiddle)
                {
                    //placement.offsetX = Mathf.Clamp (placement.offsetX + differenceInLocalSpace.x, -1.5f, 1.5f);
                    placement.offsetX = Mathf.Clamp (placement.offsetX + differenceCounterRotated.x, -1.5f, 1.5f);
                    placement.offsetZ = Mathf.Clamp (placement.offsetZ + differenceCounterRotated.y, -1.5f, 1.5f);
                }
                else if
                (
                    placement.prototype.prefab.compatibility == AreaProp.Compatibility.WallStraightBottomToFloor ||
                    placement.prototype.prefab.compatibility == AreaProp.Compatibility.WallStraightTopToFloor
                )
                {
                    //placement.offsetX = Mathf.Clamp (placement.offsetX + differenceInLocalSpace.x, -1.5f, 1.5f);
                    placement.offsetZ = Mathf.Clamp (placement.offsetZ + differenceCounterRotated.z, -1.5f, 1.5f);
                }

                placement.offsetX = placement.offsetX.Truncate (2);
                placement.offsetZ = placement.offsetZ.Truncate (2);

                // Debug.LogWarning ("In-editor prop offset update not implemented yet");
                // placement.instanceLegacy.transform.position = point.pointPositionLocal + new Vector3 (1f, -1f, 1f) * (TilesetUtility.blockAssetSize / 2f) + AreaUtility.GetPropOffsetAsVector (placement);

                placement.UpdateOffsets (am);
                UtilityECS.ScheduleUpdate ();
            }
        }

        bool ChangePropOrientation (KeyCode keyCode)
        {
            // Hotkeys to rotate and flip a prop in preview mode (before placement)
            if (keyCode != KeyCode.LeftBracket && keyCode != KeyCode.RightBracket)
            {
                return false;
            }

            var propEditInfo = bb.propEditInfo;
            var forward = keyCode == KeyCode.RightBracket;
            if (forward)
            {
                propEditInfo.Rotation = propEditInfo.Rotation.OffsetAndWrap (true, 3);
            }
            else
            {
                propEditInfo.Flipped = !propEditInfo.Flipped;
            }
            return true;
        }

        void ChangeSelectedProp (Event e)
        {
            var forward = e.delta.y > 0f;
            var propEditInfo = bb.propEditInfo;
            AreaAssetHelper.CheckResources ();
            propEditInfo.Index = propEditInfo.Index.OffsetAndWrap (forward, 0, AreaAssetHelper.propsPrototypesList.Count - 1);
            propEditInfo.SelectionID = AreaAssetHelper.propsPrototypesList[propEditInfo.Index].id;
        }

        void Edit (int spotIndex, KeyCode mouseButton, bool shift)
        {
            if (AreaAssetHelper.propsPrototypesList == null || AreaAssetHelper.propsPrototypesList.Count == 0)
            {
                Debug.Log ("AM (I) | EditProp | Early exit due to missing prop library");
                return;
            }

            bb.propEditInfo.PlacementListIndex = spotIndex;
            bb.am.indexesOccupiedByProps.TryGetValue (spotIndex, out var placements);

            if (bb.propEditingMode == PropEditingMode.Color)
            {
                EditPropColor (placements, mouseButton);
                return;
            }

            switch (mouseButton)
            {
                case KeyCode.Mouse0:
                    Debug.LogFormat
                    (
                        "AM (I) | EditProp | {0} | Attempting to place new prop at index {1}, placements there: {2}",
                        mouseButton,
                        spotIndex,
                        placements == null ? "none" : placements.Count.ToString ()
                    );
                    AddProp (spotIndex);
                    return;
                case KeyCode.Mouse1 when shift:
                    CopyProp (placements);
                    return;
                case KeyCode.Mouse2:
                    RemoveProp (placements);
                    return;
            }
        }

        void AddProp (int indexUnderCursor)
        {
            var am = bb.am;
            var propEditInfo = bb.propEditInfo;
            var prototype = AreaAssetHelper.GetPropPrototype (propEditInfo.SelectionID);
            if (prototype == null)
            {
                prototype = AreaAssetHelper.propsPrototypesList[0];
                propEditInfo.SelectionID = prototype.id;
            }

            var placement = new AreaPlacementProp
            {
                id = propEditInfo.SelectionID,
                pivotIndex = indexUnderCursor,
                rotation = propEditInfo.Rotation,
                flipped = propEditInfo.Flipped,
                offsetX = propEditInfo.OffsetX,
                offsetZ = propEditInfo.OffsetZ,
                hsbPrimary = propEditInfo.HSBPrimary,
                hsbSecondary = propEditInfo.HSBSecondary
            };

            var pointTargeted = am.points[indexUnderCursor];
            if (!am.IsPropPlacementValid (placement, pointTargeted, prototype, bb.checkPropConfiguration))
            {
                return;
            }

            if (!am.indexesOccupiedByProps.ContainsKey (indexUnderCursor))
            {
                am.indexesOccupiedByProps.Add (indexUnderCursor, new List<AreaPlacementProp> ());
            }

            am.indexesOccupiedByProps[indexUnderCursor].Add (placement);
            am.placementsProps.Add (placement);

            am.ExecutePropPlacement (placement);
            am.SnapPropRotation (placement);

            propEditInfo.PlacementHandled = placement;
            propEditInfo.PlacementListIndex = placement.pivotIndex;
        }

        void CopyProp (List<AreaPlacementProp> placements)
        {
            if (placements == null)
            {
                return;
            }
            if (placements.Count == 0)
            {
                return;
            }

            var propEditInfo = bb.propEditInfo;
            // By default choose the first prop in the cell
            var propToCopy = placements[0];
            // If user has a prop selected, check if it's on the same cell and then copy that prop
            foreach (var placement in placements)
            {
                if (placement == propEditInfo.PlacementHandled)
                {
                    propToCopy = placement;
                    break;
                }
            }

            propEditInfo.SelectionID = propToCopy.prototype.id;
            propEditInfo.Index = AreaAssetHelper.propsPrototypesList.IndexOf (propToCopy.prototype);

            propEditInfo.Rotation = propToCopy.rotation;
            propEditInfo.Flipped = propToCopy.flipped;

            propEditInfo.OffsetX = propToCopy.offsetX;
            propEditInfo.OffsetZ = propToCopy.offsetZ;
            propEditInfo.HSBPrimary = propToCopy.hsbPrimary;
            propEditInfo.HSBSecondary = propToCopy.hsbSecondary;
        }

        void RemoveProp (List<AreaPlacementProp> placements)
        {
            if (placements == null)
            {
                return;
            }
            if (placements.Count == 0)
            {
                return;
            }

            var propEditInfo = bb.propEditInfo;
            // By default choose the first prop in the cell
            var propToDelete = placements[0];
            // If user has a prop selected, check if it's on the same cell and then delete that prop
            if (propEditInfo.PlacementHandled != null)
            {
                foreach (var placement in placements)
                {
                    if (placement == propEditInfo.PlacementHandled)
                    {
                        propToDelete = placement;
                        break;
                    }
                }
            }

            bb.am.RemovePropPlacement (propToDelete);
        }

        void EditPropColor (List<AreaPlacementProp> placements, KeyCode mouseButton)
        {
            if (placements == null)
            {
                return;
            }
            if (placements.Count == 0)
            {
                return;
            }

            AreaPlacementProp placementActedOn = null;
            var warn = false;
            foreach (var placement in placements)
            {
                if (placement == null)
                {
                    continue;
                }
                if (placement.prototype == null)
                {
                    continue;
                }
                if (!placement.prototype.prefab.allowTinting)
                {
                    continue;
                }

                if (placementActedOn == null)
                {
                    placementActedOn = placement;
                }
                else
                {
                    warn = true;
                }
            }

            if (warn)
            {
                Debug.LogWarning ("AM (I) | EditProp | More than one colorable prop occupies this cell, selecting the first one: " + placementActedOn.prototype.name);
            }

            if (placementActedOn == null)
            {
                Debug.LogWarning ("AM (I) | EditProp | No placements were found on the current point, aborting");
                return;
            }

            var clipboardPropColor = bb.clipboardPropColor;
            switch (mouseButton)
            {
                case KeyCode.Mouse0:
                    placementActedOn.UpdateMaterial_HSBOffsets (clipboardPropColor.HSBPrimary, clipboardPropColor.HSBSecondary);
                    return;
                case KeyCode.Mouse1:
                    placementActedOn.UpdateMaterial_HSBOffsets (Constants.defaultHSBOffset, Constants.defaultHSBOffset);
                    return;
                case KeyCode.Mouse2:
                    clipboardPropColor.HSBPrimary = placementActedOn.hsbPrimary;
                    clipboardPropColor.HSBSecondary = placementActedOn.hsbSecondary;
                    return;
            }
        }

        void VisualizeProp (Event e, RaycastHit hitInfo)
        {
            if (bb.propEditingMode != PropEditingMode.Place)
            {
                return;
            }
            if (e.shift)
            {
                HideProp ();
                return;
            }

            var am = bb.am;
            if (am == null)
            {
                Debug.Log ("VisualizeProp - AM is null");
                return;
            }

            var position = hitInfo.point + -hitInfo.normal * 0.5f;

            LoadObjects ();
            if (propCursorInstance != null)
            {
                propCursorInstance.transform.position = position;
                if (!propCursorInstance.activeSelf)
                {
                    propCursorInstance.SetActive (true);
                }
            }

            if (AreaAssetHelper.propsPrototypesList == null || AreaAssetHelper.propsPrototypesList.Count == 0)
            {
                HideProp ();
                return;
            }

            var spotIndex = AreaUtility.GetIndexFromWorldPosition (position + AreaSceneCamera.spotRaycastHitOffset, am.GetHolderColliders ().position, am.boundsFull);
            if (spotIndex == AreaUtility.invalidIndex)
            {
                HideProp ();
                Debug.Log("VisualizeProp - Point index not found");
                return;
            }

            var point = am.points[spotIndex];
            if (!point.spotPresent
                || point.spotConfiguration == TilesetUtility.configurationEmpty
                || point.spotConfiguration == TilesetUtility.configurationFull)
            {
                HideProp ();
                Debug.Log("VisualizeProp - Point not present");
                return;
            }

            var propEditInfo = bb.propEditInfo;
            if (propEditInfo.IndexVisualized != propEditInfo.Index)
            {
                // Only re-instantiate prop preview when prop index changes
                if (!InstantiatePreviewObject (point))
                {
                    return;
                }
            }
            else
            {
                ShowPreviewObject (point);
            }

            propEditInfo.SpotIndexVisualized = spotIndex;
            propEditInfo.RotationVisualized = propEditInfo.Rotation;
            propEditInfo.FlippedVisualized = propEditInfo.Flipped;
            propEditInfo.OffsetXVisualized = propEditInfo.OffsetX;
            propEditInfo.OffsetZVisualized = propEditInfo.OffsetZ;
        }

        void LoadObjects ()
        {
            if (propHolder == null)
            {
                propHolder = GameObject.Find ("AreaManager_PropPreviewHolder");
            }
            if (propHolder == null)
            {
                propHolder = new GameObject ("AreaManager_PropPreviewHolder");
                propHolder.hideFlags = HideFlags.HideAndDontSave;
            }
            if (propCursorInstance == null && bb.am.debugPropVisualCursor != null)
            {
                propCursorInstance = Object.Instantiate (bb.am.debugPropVisualCursor);
                propCursorInstance.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        void HideProp ()
        {
            if (propPreviewInstance != null)
            {
                propPreviewInstance.gameObject.SetActive (false);
            }
        }

        bool InstantiatePreviewObject (AreaVolumePoint point)
        {
            var propEditInfo = bb.propEditInfo;
            if (!propEditInfo.Index.IsValidIndex (AreaAssetHelper.propsPrototypesList))
            {
                propEditInfo.Index = 0;
                HideProp ();
                Debug.Log("VisualizeProp - Prop index invalid, bailing on instantiation");
                return false;
            }

            var prototype = AreaAssetHelper.propsPrototypesList[propEditInfo.Index];
            if (prototype == null)
            {
                HideProp ();
                Debug.Log("VisualizeProp - Prop prototype data is null, bailing on instantiation");
                return false;
            }

            UtilityGameObjects.ClearChildren (propHolder);
            propPreviewInstance = Object.Instantiate (prototype.prefab.gameObject).GetComponent<AreaProp> ();
            propPreviewInstance.transform.name = prototype.prefab.transform.name;

            SnapPropRotationToTileConfiguration(prototype, prototype.prefab, point);
            propPreviewInstance.gameObject.SetFlags (HideFlags.HideAndDontSave);

            UpdatePropHSBOffsets (propPreviewInstance);

            propEditInfo.IndexVisualized = propEditInfo.Index;
            return true;
        }

        void SnapPropRotationToTileConfiguration (AreaPropPrototypeData prototype, AreaProp instance, AreaVolumePoint point)
        {
            var propEditInfo = bb.propEditInfo;
            // Snap previewed prop rotation to tile's configuration
            if (instance.linkRotationToConfiguration)
            {
                var configurationMask = AreaAssetHelper.GetPropMask (instance.compatibility);
                propEditInfo.Rotation = (byte)configurationMask.IndexOf (point.spotConfiguration);
            }
            var t = propPreviewInstance.transform;
            t.rotation = Quaternion.Euler (new Vector3 (0f, -90f * propEditInfo.Rotation, 0f));
            t.position = point.pointPositionLocal
                + new Vector3 (1f, -1f, 1f) * WorldSpace.HalfBlockSize
                + AreaUtility.GetPropOffsetAsVector (prototype, propEditInfo.OffsetX, propEditInfo.OffsetZ, propEditInfo.Rotation, t.localRotation);
            t.localScale = instance.mirrorOnZAxis
                ? new Vector3 (1f, 1f, propEditInfo.Flipped ? -1f : 1f)
                : new Vector3 (propEditInfo.Flipped ? -1f : 1f, 1f, 1f);
            t.localScale = instance.scaleRandomly
                ? Vector3.Lerp (instance.scaleMin, instance.scaleMax, 0.5f)
                : t.localScale;
            t.parent = propHolder.transform;
        }

        void UpdatePropHSBOffsets (AreaProp previewInstance)
        {
            if (previewInstance == null)
            {
                return;
            }
            if (!previewInstance.allowTinting)
            {
                return;
            }

            if (propMPB == null)
            {
                propMPB = new MaterialPropertyBlock ();
            }

            var visPropRenderersNew = previewInstance.renderers;
            foreach (var renderer in visPropRenderersNew)
            {
                if (renderer.mode != AreaProp.RendererMode.ActiveWhenDestroyed)
                {
                    propMPB.Clear ();
                    renderer.renderer.GetPropertyBlock (propMPB);
                    // Toggle HSV override to ignore instance array data
                    propMPB.SetFloat (ID_InstancePropsOverride, 1.0f);
                    // Set HSV offsets to preview prop colors
                    propMPB.SetVector (ID_HsbOffsetsPrimary, bb.propEditInfo.HSBPrimary);
                    propMPB.SetVector (ID_HsbOffsetsSecondary, bb.propEditInfo.HSBSecondary);
                    // Set packed prop data to defaults just in case
                    propMPB.SetVector (ID_PackedPropData, packedPropDataDefault);
                    renderer.renderer.SetPropertyBlock (propMPB);
                }
                else
                {
                    // Additionally hide meshes showing destroyed state, since we are iterating on renderers
                    renderer.renderer.gameObject.SetActive (false);
                }
            }
        }

        void ShowPreviewObject (AreaVolumePoint point)
        {
            var propEditInfo = bb.propEditInfo;
            if (propPreviewInstance == null)
            {
                propEditInfo.IndexVisualized = -1;
                return;
            }

            if (propPreviewInstance.gameObject.activeSelf == false)
            {
                propPreviewInstance.gameObject.SetActive (true);
            }

            if (propEditInfo.SpotIndexVisualized != point.spotIndex
                || propEditInfo.RotationVisualized != propEditInfo.Rotation
                || propEditInfo.FlippedVisualized != propEditInfo.Flipped
                || !propEditInfo.OffsetXVisualized.RoughlyEqual (propEditInfo.OffsetX)
                || !propEditInfo.OffsetZVisualized.RoughlyEqual (propEditInfo.OffsetZ))
            {
                var prototype = AreaAssetHelper.propsPrototypesList[propEditInfo.Index];
                if (prototype == null)
                {
                    HideProp ();
                    return;
                }
                SnapPropRotationToTileConfiguration (prototype, propPreviewInstance, point);
            }

            if (propEditInfo.HSBPrimaryVisualized != propEditInfo.HSBPrimary || propEditInfo.HSBSecondaryVisualized != propEditInfo.HSBSecondary)
            {
                UpdatePropHSBOffsets (propPreviewInstance);
            }
        }

        public static AreaSceneMode CreateInstance (AreaSceneBlackboard bb) => new AreaScenePropMode (bb);

        AreaScenePropMode (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            Panel = AreaScenePropModePanel.Create (bb);
        }

        readonly AreaSceneBlackboard bb;

        MaterialPropertyBlock propMPB;

        GameObject propCursorInstance;
        AreaProp propPreviewInstance;
        GameObject propHolder;

        const float handleSize = 0.1f;
        const float pickSize = 0.4f;

        static readonly Vector3 handleSizeCube = new Vector3 (0.25f, 0.25f, 0.25f);

        static readonly int ID_InstancePropsOverride = Shader.PropertyToID ("_InstancePropsOverride");
        static readonly int ID_HsbOffsetsPrimary = Shader.PropertyToID ("_HSBOffsetsPrimary");
        static readonly int ID_HsbOffsetsSecondary = Shader.PropertyToID ("_HSBOffsetsSecondary");
        static readonly int ID_PackedPropData = Shader.PropertyToID ("_PackedPropData");
        static readonly Vector4 packedPropDataDefault = new Vector4 (1.0f, 0.0f, 0.0f, 1.0f);
    }
}
