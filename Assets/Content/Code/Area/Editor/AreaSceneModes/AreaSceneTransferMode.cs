using PhantomBrigade;

using UnityEditor;
using UnityEngine;

#if PB_MODSDK
using PhantomBrigade.SDK.ModTools;
#endif

namespace Area
{
    using Scene;

    sealed class AreaSceneTransferMode : AreaSceneMode
    {
        public EditingMode EditingMode => EditingMode.Transfer;

        public AreaSceneModePanel Panel { get; }

        public void OnDisable () => Panel.OnDisable ();
        public void OnDestroy ()
        {
            if (transferPreviewInstance != null)
            {
                Object.DestroyImmediate (transferPreviewInstance);
            }
        }

        public int LayerMask => AreaSceneCamera.environmentLayerMask;

        public bool Hover (Event e, RaycastHit hitInfo)
        {
            if (!AreaSceneModeHelper.DisplayVolumeCursor(bb, hitInfo, cursorID))
            {
                return false;
            }
            bb.gizmos.DrawWireframesForVolume (e.shift);

            VisualizeTransferPreview ();

            if (e.shift)
            {
                SetTargetOrigin ();
            }
            var (eventType, button) = AreaSceneModeHelper.ResolveEvent (e);
            switch (eventType)
            {
                case EventType.KeyUp:
                    return CopyPaste(button, e.shift);
                case EventType.MouseDown:
                case EventType.ScrollWheel:
                    return e.shift ? Paste (button) : Copy (button);
            }
            return false;
        }

        public void OnHoverEnd ()
        {
            bb.gizmos.cursor.HideCursor ();
            if (transferPreviewInstance != null)
            {
                Object.DestroyImmediate (transferPreviewInstance);
            }
        }

        public bool HandleSceneUserInput (Event e) => false;

        public void DrawSceneMarkup (Event e, System.Action repaint)
        {
            AreaSceneModeHelper.TryRebuildTerrain (bb);

            var am = bb.am;
            var bounds = am.boundsFull;

            if (AreaSceneBlackboard.clipboardSource == ClipboardSource.Scene)
            {
                var sourcePosA = AreaUtility.GetLocalPositionFromGridPosition (am.clipboardOrigin);
                var sourcePosB = AreaUtility.GetLocalPositionFromGridPosition (am.clipboardOrigin + am.clipboardBoundsRequested + Vector3Int.size1x1x1Neg);
                var sourceCornerAIndex = AreaUtility.GetIndexFromVolumePosition (am.clipboardOrigin, bounds);
                var sourceCornerBIndex = AreaUtility.GetIndexFromVolumePosition (am.clipboardOrigin + am.clipboardBoundsRequested + Vector3Int.size1x1x1Neg, bounds);

                var sourceColorMain = new HSBColor (0.0f, 1f, 1f, 1f).ToColor ();
                var sourceColorCulled = new HSBColor (0.0f, 0.65f, 0.5f, 1f).ToColor ();
                if (sourceCornerAIndex != -1 && sourceCornerBIndex != -1)
                {
                    sourceColorMain = new HSBColor (0.25f, 1f, 1f, 1f).ToColor ();
                    sourceColorCulled = new HSBColor (0.1f, 0.65f, 0.5f, 1f).ToColor ();
                }

                bb.gizmos.DrawZTestVolume (sourcePosA, sourcePosB, sourceColorMain, sourceColorCulled);
                bb.gizmos.DrawVolumeDirection (sourcePosA, sourcePosB, Vector3.right, sourceColorMain);
            }

            if (bb.am.clipboard.IsValid)
            {
                var targetPosA = AreaUtility.GetLocalPositionFromGridPosition (am.targetOrigin);
                var targetPosB = AreaUtility.GetLocalPositionFromGridPosition (am.targetOrigin + am.clipboard.clipboardBoundsSaved + Vector3Int.size1x1x1Neg);
                var targetCornerAIndex = AreaUtility.GetIndexFromVolumePosition (am.targetOrigin, bounds);
                var targetCornerBIndex = AreaUtility.GetIndexFromVolumePosition (am.targetOrigin + am.clipboard.clipboardBoundsSaved + Vector3Int.size1x1x1Neg, bounds);

                var targetColorMain = new HSBColor (0.0f, 1f, 1f, 1f).ToColor ();
                var targetColorCulled = new HSBColor (0.0f, 0.65f, 0.5f, 1f).ToColor ();
                if (targetCornerAIndex != -1 && targetCornerBIndex != -1)
                {
                    targetColorMain = new HSBColor (0.55f, 1f, 1f, 1f).ToColor ();
                    targetColorCulled = new HSBColor (0.6f, 0.65f, 0.5f, 1f).ToColor ();

                }
                bb.gizmos.DrawZTestVolume (targetPosA, targetPosB, targetColorMain, targetColorCulled);
                bb.gizmos.DrawVolumeDirection (targetPosA, targetPosB, am.clipboard.clipboardDirection.ToVector3 (), targetColorMain);
                if (targetCornerBIndex != -1)
                {
                    DrawGroundingStilts (targetPosA, targetPosB);
                }
            }
        }

        void DrawGroundingStilts (Vector3 targetPosA, Vector3 targetPosB)
        {
            var am = bb.am;
            var hc = Handles.color;
            Handles.color = new HSBColor (0.96f, 0.9f, 0.92f, 1f).ToColor ();
            var offsetX = am.clipboard.clipboardBoundsSaved.x - 1;
            var offsetZ = am.clipboard.clipboardBoundsSaved.z - 1;
            var offsets = new[]
            {
                new Vector3 (0f, 0f, 0f),
                new Vector3 (0f, 0f, offsetZ * WorldSpace.BlockSize),
                new Vector3 (offsetX * WorldSpace.BlockSize, 0f, offsetZ * WorldSpace.BlockSize),
                new Vector3 (offsetX * WorldSpace.BlockSize, 0f, 0f),
            };
            var cutoffDistance = (am.clipboard.clipboardBoundsSaved.y - 1) * WorldSpace.BlockSize + 0.1f;
            for (var i = 0; i < offsets.Length; i += 1)
            {
                var offset = offsets[i];
                var groundingRayOrigin = targetPosA + offset;
                var groundingRay = new Ray (groundingRayOrigin, Vector3.down);
                var hit = Physics.Raycast (groundingRay, out var groundingHit, 100f, LayerMasks.environmentMask);
                if (hit && groundingHit.distance > cutoffDistance)
                {
                    var corner = targetPosB - offsets[(i + 2) % 4];
                    Handles.DrawLine (groundingHit.point, corner);
                }
            }
            Handles.color = hc;
        }

        void SetTargetOrigin ()
        {
            var am = bb.am;
            if (!am.clipboard.IsValid)
            {
                return;
            }

            var point = bb.lastPointHovered;
            var y = bb.targetOriginYCursorLock
                ? point.pointPositionIndex.y - am.clipboardBoundsRequested.y + 1
                : am.targetOrigin.y;
            am.targetOrigin = new Vector3Int (point.pointPositionIndex.x, y, point.pointPositionIndex.z);
        }

        bool CopyPaste (KeyCode keyCode, bool shift)
        {
            switch (keyCode)
            {
                case KeyCode.Delete:
                    AreaSceneModeHelper.ClearClipboard (bb);
                    return true;
                case KeyCode.S:
                    if (AreaSceneBlackboard.clipboardSource == ClipboardSource.Snippet)
                    {
                        return true;
                    }
                    ClipboardCopyOperation.ShrinkwrapSource (bb.am);
                    return true;
                case KeyCode.Z when shift:
                    bb.targetOriginYCursorLock = !bb.targetOriginYCursorLock;
                    return true;
                case KeyCode.X:
                    if (AreaSceneBlackboard.clipboardSource == ClipboardSource.Snippet)
                    {
                        return true;
                    }
                    AreaSceneModeHelper.CopyClipboardScene (bb);
                    return true;
                case KeyCode.V:
                    PasteScene (ClipboardPasteOperation.ApplicationMode.Overwrite);
                    return true;
                case KeyCode.B:
                    PasteScene (ClipboardPasteOperation.ApplicationMode.Additive);
                    return true;
                case KeyCode.RightBracket:
                    ChangeTargetOrigin (-1);
                    return true;
                case KeyCode.LeftBracket:
                    ChangeTargetOrigin (1);
                    return true;
                case KeyCode.PageUp:
                    ChangeClipboardOrigin (-1);
                    return true;
                case KeyCode.PageDown:
                    ChangeClipboardOrigin (1);
                    return true;
            }
            return false;
        }

        void PasteScene (ClipboardPasteOperation.ApplicationMode mode)
        {
            if (!bb.am.clipboard.IsValid)
            {
                return;
            }
            #if PB_MODSDK
            if (!DataContainerModData.hasSelectedConfigs)
            {
                return;
            }
            #endif
            ClipboardPasteOperation.applicationMode = mode;
            ClipboardPasteOperation.PasteVolume (bb.am, bb.punchDownTerrainOnPaste, bb.checkPropCompatibilityOnPaste, bb.enableTransferLogging);
        }

        void ChangeTargetOrigin (int offset)
        {
            if (!bb.am.clipboard.IsValid || bb.targetOriginYCursorLock)
            {
                return;
            }
            if (offset < 0 && bb.am.targetOrigin.y + bb.am.clipboard.clipboardBoundsSaved.y - 1 <= 0)
            {
                return;
            }
            if (offset > 0 && bb.am.targetOrigin.y + bb.am.clipboard.clipboardBoundsSaved.y - 1 >= bb.am.boundsFull.y - 1)
            {
                return;
            }
            bb.am.targetOrigin.y += offset;
        }

        void ChangeClipboardOrigin (int offset)
        {
            if (AreaSceneBlackboard.clipboardSource == ClipboardSource.Snippet)
            {
                return;
            }
            if (offset < 0 && bb.am.clipboardOrigin.y + bb.am.clipboardBoundsRequested.y - 1 <= 0)
            {
                return;
            }
            if (offset > 0 && bb.am.clipboardOrigin.y + bb.am.clipboardBoundsRequested.y - 1 >= bb.am.boundsFull.y - 1)
            {
                return;
            }
            bb.am.clipboardOrigin.y += offset;
        }

        bool Paste (KeyCode button)
        {
            switch (button)
            {
                case KeyCode.RightBracket:
                    ChangeTargetOrigin (-1);
                    return true;
                case KeyCode.LeftBracket:
                    ChangeTargetOrigin (1);
                    return true;
                case KeyCode.Mouse0:
                    PasteScene (ClipboardPasteOperation.ApplicationMode.Overwrite);
                    return true;
                case KeyCode.Mouse1:
                    PasteScene (ClipboardPasteOperation.ApplicationMode.Additive);
                    return true;
                case KeyCode.Mouse2:
                    if (!bb.am.clipboard.IsValid)
                    {
                        return true;
                    }
                    ClipboardCopyOperation.ShrinkwrapTarget (bb.am);
                    return true;
            }
            return false;
        }

        bool Copy (KeyCode button)
        {
            var point = bb.lastPointHovered;
            switch (button)
            {
                case KeyCode.PageUp:
                    ChangeClipboardOrigin (-1);
                    return true;
                case KeyCode.PageDown:
                    ChangeClipboardOrigin (1);
                    return true;
                case KeyCode.Mouse0:
                    if (AreaSceneBlackboard.clipboardSource == ClipboardSource.Snippet)
                    {
                        return true;
                    }
                    SetClipboardOrigin (point);
                    return true;
                case KeyCode.Mouse2:
                    if (AreaSceneBlackboard.clipboardSource == ClipboardSource.Snippet)
                    {
                        return true;
                    }
                    SetCopyVolumeBounds (point);
                    return true;
                case KeyCode.Mouse1:
                    SetTargetOrigin ();
                    return true;
            }
            return false;
        }

        void SetClipboardOrigin (AreaVolumePoint point)
        {
            var am = bb.am;
            var y = 0;
            if (bb.groundSourceOriginOnClick && am.clipboardBoundsRequested.y < am.boundsFull.y - 1)
            {
                y = point.pointPositionIndex.y - am.clipboardBoundsRequested.y + 1;
            }
            am.clipboardOrigin = new Vector3Int (point.pointPositionIndex.x, y, point.pointPositionIndex.z);
        }

        void SetCopyVolumeBounds (AreaVolumePoint pointStart)
        {
            var am = bb.am;
            var cornerB = new Vector3Int (pointStart.pointPositionIndex.x, 0, pointStart.pointPositionIndex.z);
            var difference = cornerB - am.clipboardOrigin;
            var lowestY = bb.groundSourceOriginOnClick
                ? Mathf.Max (pointStart.pointPositionIndex.y, am.clipboardOrigin.y + am.clipboardBoundsRequested.y - 1)
                : am.boundsFull.y - 1;
            var bounds = new Vector3Int (Mathf.Abs (difference.x), lowestY, Mathf.Abs (difference.z)) + Vector3Int.size1x1x1;

            if (bounds.x < 2 || bounds.z < 2)
            {
                Debug.LogWarning ("Selected area is too small!");
                return;
            }

            if (difference.x < 0)
            {
                am.clipboardOrigin.x = pointStart.pointPositionIndex.x;
            }
            if (difference.z < 0)
            {
                am.clipboardOrigin.z = pointStart.pointPositionIndex.z;
            }
            am.clipboardOrigin.y = 0;
            am.clipboardBoundsRequested = bounds;
        }

        void VisualizeTransferPreview ()
        {
            return;
            /*
            if
            (
                am.clipboardPointsSaved == null ||
                am.clipboardPointsSaved.Count == 0 ||
                am.targetOrigin == Vector3Int.size0x0x0
            )
            {
                // Debug.Log ("B1");
                HideTransferPreview ();
                return;
            }

            if
            (
                transferPreviewInstance != null &&
                transferPreviewVersionVisualized == transferPreviewVersion
            )
            {
                // Debug.Log ("B3");
                return;
            }

            transferPreviewVersionVisualized = transferPreviewVersion;
            if (transferPreviewInstance == null)
                transferPreviewInstance = new GameObject ("TransferPreviewInstance");
            else
                UtilityGameObjects.ClearChildren (transferPreviewInstance);

            // Bounds are not positions, they are limits, like array sizes - so we need to subtract 1 from bounds axes to get second corner
            Vector3Int cornerA = am.targetOrigin;
            Vector3Int cornerB = cornerA + am.clipboardBoundsSaved + Vector3Int.size1x1x1Neg;

            int indexA = AreaUtility.GetIndexFromInternalPosition (cornerA, am.boundsFull);
            int indexB = AreaUtility.GetIndexFromInternalPosition (cornerB, am.boundsFull);

            if (indexA == -1 || indexB == -1)
            {
                Debug.LogWarning (string.Format
                (
                    "PasteVolume | Failed to paste due to specified corner {0} or calculated corner {1} falling outside of target level bounds {2}",
                    cornerA,
                    cornerB,
                    am.boundsFull
                ));
                return;
            }

            // Since we are directly modifying boundary points, some spots outside of captured area would be affected - time to collect all points
            int affectedOriginX = Mathf.Max (0, cornerA.x - 1);
            int affectedOriginZ = Mathf.Max (0, cornerA.z - 1);
            var cornerAShifted = new Vector3Int (affectedOriginX, 0, affectedOriginZ);

            // Bounds are limits, like array sizes - so they will be bigger by 1 than just pure position difference
            // For example, a set of points [(0,0);(1,0);(0,1);(1;1)] has bounds of 2x2, not 1x1!
            int affectedBoundsX = cornerB.x - cornerAShifted.x + 1;
            int affectedBoundsZ = cornerB.z - cornerAShifted.z + 1;
            int affectedVolumeLength = affectedBoundsX * am.clipboardBoundsSaved.y * affectedBoundsZ;
            var affectedBounds = new Vector3Int (affectedBoundsX, am.clipboardBoundsSaved.y, affectedBoundsZ);
            transferPointsAffected = new List<AreaVolumePoint> (affectedVolumeLength);

            for (int i = 0; i < affectedVolumeLength; ++i)
            {
                Vector3Int affectedPointPosition = AreaUtility.GetVolumePositionFromIndex (i, affectedBounds);
                Vector3Int sourcePointPosition = affectedPointPosition + cornerAShifted;
                int sourcePointIndex = AreaUtility.GetIndexFromInternalPosition (sourcePointPosition, am.boundsFull);
                var sourcePoint = am.points[sourcePointIndex];
                transferPointsAffected.Add (sourcePoint);
                // Debug.DrawLine (sourcePoint.pointPositionLocal, sourcePoint.instancePosition, Color.white, 10f);
            }

            for (int i = 0; i < am.clipboardPointsSaved.Count; ++i)
            {
                var clipboardPoint = am.clipboardPointsSaved[i];
                var targetPointPosition = clipboardPoint.pointPositionIndex + cornerA;
                int targetPointIndex = AreaUtility.GetIndexFromInternalPosition (targetPointPosition, am.boundsFull);
                var targetPoint = am.points[targetPointIndex];
                if (clipboardPoint.pointState == AreaVolumePointState.Empty)
                    continue;

                var go = PrimitiveHelper.CreatePrimitive (PrimitiveType.Cube, false);
                go.transform.localScale = Vector3.one * 3.05f;
                go.transform.localPosition = targetPoint.pointPositionLocal;
                go.transform.parent = transferPreviewInstance.transform;
            }

            Material material = PrimitiveHelper.GetDefaultMaterial ();
            UtilityMaterial.SetMaterialsOnRenderers (transferPreviewInstance.gameObject, material);
            */
        }

        void HideTransferPreview ()
        {
            if (transferPreviewInstance != null)
            {
                transferPreviewInstance.gameObject.SetActive (false);
            }
        }

        public static AreaSceneMode CreateInstance (AreaSceneBlackboard bb) => new AreaSceneTransferMode (bb);

        AreaSceneTransferMode (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            var cursor = new AreaSceneVolumeCursor (bb.gizmos.cursor);
            cursorID = cursor.ID;
            Panel = AreaSceneTransferModePanel.Create (bb);
        }

        readonly AreaSceneBlackboard bb;
        readonly int cursorID;
        GameObject transferPreviewInstance;
    }
}
