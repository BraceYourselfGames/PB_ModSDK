using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

#if PB_MODSDK
using PhantomBrigade.SDK.ModTools;
#endif

namespace Area
{
    using Scene;

    enum VolumeSelectionStatus
    {
        OK = 0,
        Limited,
        Warning,
    }

    static class AreaSceneModeHelper
    {
        public static void LookDownNorthAligned (AreaVolumePoint cell)
        {
            if (cell == null)
            {
                return;
            }
            var sv = SceneView.lastActiveSceneView;
            var centerPoint = cell.instancePosition + Vector3.up * 25f;
            var rot = Quaternion.LookRotation (Vector3.down, Vector3.forward);
            sv.LookAt (centerPoint, rot);
        }

        public static void InspectCell (AreaSceneBlackboard bb, AreaVolumePoint cell)
        {
            bb.layer = cell.pointPositionIndex.y;
            LookDownNorthAligned (cell);
            bb.lastCellInspected = cell;
            bb.lastCellHovered = null;
        }

        public static void UpdateNeighborState (AreaManager am, int index)
        {
            var bounds = am.boundsFull;
            var layerSize = bounds.x * bounds.z;
            for (var y = -1; y <= 0; y += 1)
            {
                for (var z = -1; z <= 0; z += 1)
                {
                    for (var x = -1; x <= 0; x += 1)
                    {
                        var i = index + x + z * bounds.x + y * layerSize;
                        am.UpdateSpotAtIndex(i, false, false, true);
                    }
                }
            }
        }

        public static void TryRebuildTerrain (AreaSceneBlackboard bb)
        {
            if (!bb.rebuildTerrain)
            {
                return;
            }
            bb.am.UpdateTerrain (true, true);
            bb.rebuildTerrain = false;
        }

        public static bool DisplayVolumeCursor (AreaSceneBlackboard bb, RaycastHit hitInfo, int cursorID, VolumePointChecker pointChecker = null)
        {
            bb.pointerDirection = hitInfo.normal;
            if (!SetPointChecker (bb.gizmos.cursor, cursorID, pointChecker))
            {
                return false;
            }
            bb.gizmos.cursor.SetCursor(cursorID);
            var (pointIndex, _) = AreaSceneHelper.ResolveIndexesFromHit (hitInfo, bb.am.boundsFull, log: bb.enableCursorLogging);
            if (!pointIndex.IsValidIndex (bb.am.points))
            {
                return false;
            }

            var point = bb.am.points[pointIndex];
            bb.lastPointHovered = point;

            var cursor = bb.gizmos.cursor;
            cursor.ShowCursor (point, hitInfo);
            return true;
        }

        static bool SetPointChecker (AreaSceneGizmosCursor gizmosCursor, int cursorID, VolumePointChecker pointChecker)
        {
            var cursor = (AreaSceneVolumeCursor)gizmosCursor.GetCursor (cursorID);
            if (cursor == null)
            {
                return false;
            }
            cursor.pointChecker = pointChecker;
            return true;
        }

        public static void ResetPointer (AreaSceneBlackboard bb)
        {
            var pointer = bb.gizmos.cursor.pointer;
            if (pointer == null)
            {
                return;
            }
            pointer.SetMaterial(pointer.standardMaterialID);
        }

        public static bool DisplaySpotCursor (AreaSceneBlackboard bb, RaycastHit hitInfo, bool showWireframe = true)
        {
            bb.pointerDirection = hitInfo.normal;
            if (!UpdateSpotHovered (bb, hitInfo))
            {
                return false;
            }
            if (showWireframe && bb.lastSpotType == SpotHoverType.Editable)
            {
                bb.gizmos.cursor.HideCursor ();
                bb.gizmos.DrawWireframeForSpot ();
                return true;
            }

            bb.gizmos.cursor.SetCursor (bb.gizmos.cursor.pointerCursorID);
            var cursor = bb.gizmos.cursor;
            var pointer = cursor.pointer;
            pointer.SetMaterial(pointer.standardMaterialID);
            cursor.ShowCursor (bb.lastSpotHovered, hitInfo);
            return true;
        }

        static bool UpdateSpotHovered (AreaSceneBlackboard bb, RaycastHit hitInfo)
        {
            var (_, spotIndex) = AreaSceneHelper.ResolveIndexesFromHit (hitInfo, bb.am.boundsFull, log: bb.enableCursorLogging);
            bb.spotInfo.Update (spotIndex);
            if (!spotIndex.IsValidIndex (bb.am.points))
            {
                return false;
            }
            bb.lastSpotHovered = bb.am.points[spotIndex];
            return true;
        }

        public static void DrawVolumeSelectionHandles (AreaSceneBlackboard bb, VolumeSelectionStatus selectionStatus)
        {
            if (!bb.hoverActive)
            {
                return;
            }
            if (AreaManager.editingVolumeBrush == AreaManager.EditingVolumeBrush.Point)
            {
                return;
            }
            if (bb.lastPointHovered == null)
            {
                return;
            }
            var color = AreaSceneGizmos.VolumeSelectionColor.Standard;
            switch (selectionStatus)
            {
                case VolumeSelectionStatus.Limited:
                    color = AreaSceneGizmos.VolumeSelectionColor.Limited;
                    break;
                case VolumeSelectionStatus.Warning:
                    color = AreaSceneGizmos.VolumeSelectionColor.Warning;
                    break;
            }
            bb.gizmos.DrawVolumeSelectionHandles (bb.lastPointHovered.pointPositionLocal, color);
        }

        public static void ChangeVolumeBrush (AreaSceneBlackboard bb, Event e) => ChangeVolumeBrush (bb, e.delta.y > 0f);
        public static void ChangeVolumeBrush (AreaSceneBlackboard bb, bool forward)
        {
            var v = (int)AreaManager.editingVolumeBrush;
            v = v.OffsetAndWrap (forward, 0, (int)AreaManager.EditingVolumeBrush.Circle);
            AreaManager.editingVolumeBrush = (AreaManager.EditingVolumeBrush)v;
            bb.brushChanged = true;
        }

        public static void PopulatePropList (AreaSceneBlackboard bb, List<PropTableEntry> altList = null)
        {
            var useFilter = !string.IsNullOrWhiteSpace (bb.propFilter);
            var propID = -1;
            var filterById = useFilter && int.TryParse (bb.propFilter, out propID);

            collectedProps.Clear ();
            foreach (var prototype in AreaAssetHelper.propsPrototypesList.OrderBy (p => p.id))
            {
                if (filterById)
                {
                    if (prototype.id != propID)
                    {
                        continue;
                    }
                }
                else if (useFilter)
                {
                    if (!prototype.prefab.name.Contains (bb.propFilter))
                    {
                        continue;
                    }
                }

                collectedProps.Add (new PropTableEntry ()
                {
                    prototype = prototype,
                    propEditInfo = bb.propEditInfo,
                });
            }

            altList ??= bb.props;
            altList.Clear ();
            altList.AddRange (collectedProps);
        }

        public static void ClearClipboard (AreaSceneBlackboard bb)
        {
            bb.am.clipboardOrigin = Vector3Int.size0x0x0;
            bb.am.clipboardBoundsRequested = Vector3Int.size2x2x2;
            bb.am.clipboard.Reset ();
            AreaSceneBlackboard.ClearPersistentClipboardInfo ();
        }

        public static void CopyClipboardScene (AreaSceneBlackboard bb)
        {
            ClipboardCopyOperation.CopyVolume (bb.am, bb.enableTransferLogging);
            AreaSceneBlackboard.clipboardSource = ClipboardSource.Scene;
            AreaSceneBlackboard.clipboardAreaKey = bb.am.areaName;
            AreaSceneBlackboard.clipboardSnippetKey = "";
            #if PB_MODSDK
            AreaSceneBlackboard.clipboardModID = DataContainerModData.hasSelectedConfigs ? DataContainerModData.selectedMod.id : AreaSceneModePanelHelper.SDKModPlaceHolder;
            #endif
        }

        public static (EventType, KeyCode) ResolveEvent (Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                case EventType.MouseUp:
                    return (e.type, ResolveMouseButton (e));
                case EventType.ScrollWheel:
                    return (e.type, ResolveScrollWheel(e));
                case EventType.KeyDown:
                case EventType.KeyUp:
                    return (e.type, e.keyCode);
                case EventType.MouseDrag:
                    return (e.type, ResolveMouseButton (e));
            }
            return (e.type, KeyCode.None);
        }

        static KeyCode ResolveScrollWheel (Event e)
        {
            var forward = e.delta.y > 0f;
            if (e.shift)
            {
                return forward ? KeyCode.LeftBracket : KeyCode.RightBracket;
            }
            return forward ? KeyCode.PageDown : KeyCode.PageUp;
        }

        static KeyCode ResolveMouseButton (Event e)
        {
            switch (e.button)
            {
                case 0:
                    return KeyCode.Mouse0;
                case 1:
                    return KeyCode.Mouse1;
                case 2:
                    return KeyCode.Mouse2;
            }
            return KeyCode.None;
        }

        static readonly List<PropTableEntry> collectedProps = new List<PropTableEntry> ();
    }
}
