using UnityEngine;

namespace Area
{
    using Scene;

    enum PropOrientationChange
    {
        None = 0,
        Snap,
        RotateClockwise,
        RotateAnticlockwise,
        Flip,
        Reset,
    }

    interface PropEditFunctions
    {
        (byte, bool) GetOrientation ();
        Vector3 GetOffset ();
        Vector4 GetPrimaryColor ();
        Vector4 GetSecondaryColor ();
        void ChangeOrientation (PropOrientationChange change);
        void ChangeOffset (float x, float z);
        void ChangeColor (Vector4 primary, Vector4 secondary);
        void CopyPastePosition (PropCopyPasteReset cpr);
        void CopyPasteColor (PropCopyPasteReset cpr);
    }

    sealed class NewPropFunctions : PropEditFunctions
    {
        public (byte, bool) GetOrientation () => (propEditInfo.Rotation, propEditInfo.Flipped);
        public Vector3 GetOffset () => new Vector3 (propEditInfo.OffsetX, 0, propEditInfo.OffsetZ);
        public Vector4 GetPrimaryColor () => propEditInfo.HSBPrimary;
        public Vector4 GetSecondaryColor () => propEditInfo.HSBSecondary;

        public void ChangeOrientation (PropOrientationChange change)
        {
            switch (change)
            {
                case PropOrientationChange.RotateClockwise:
                    propEditInfo.Rotation = propEditInfo.Rotation.OffsetAndWrap (false, 3);
                    break;
                case PropOrientationChange.RotateAnticlockwise:
                    propEditInfo.Rotation = propEditInfo.Rotation.OffsetAndWrap (true, 3);
                    break;
                case PropOrientationChange.Flip:
                    propEditInfo.Flipped = !propEditInfo.Flipped;
                    break;
                case PropOrientationChange.Reset:
                    propEditInfo.Rotation = 0;
                    propEditInfo.Flipped = false;
                    break;
            }
        }

        public void ChangeOffset (float x, float z)
        {
            propEditInfo.OffsetX = x;
            propEditInfo.OffsetZ = z;
        }

        public void ChangeColor (Vector4 primary, Vector4 secondary)
        {
            propEditInfo.HSBPrimary = primary;
            propEditInfo.HSBSecondary = secondary;
        }

        public void CopyPastePosition (PropCopyPasteReset cpr)
        {
            switch (cpr)
            {
                case PropCopyPasteReset.Copy:
                    propEditInfo.OffsetXClipboard = propEditInfo.OffsetX;
                    propEditInfo.OffsetZClipboard = propEditInfo.OffsetZ;
                    break;
                case PropCopyPasteReset.Paste:
                    propEditInfo.OffsetX = propEditInfo.OffsetXClipboard;
                    propEditInfo.OffsetZ = propEditInfo.OffsetZClipboard;
                    break;
                case PropCopyPasteReset.Reset:
                    propEditInfo.OffsetX = 0f;
                    propEditInfo.OffsetZ = 0f;
                    break;
            }
        }

        public void CopyPasteColor (PropCopyPasteReset cpr)
        {
            switch (cpr)
            {
                case PropCopyPasteReset.Copy:
                    clipboardPropColor.HSBPrimary = propEditInfo.HSBPrimary;
                    clipboardPropColor.HSBSecondary = propEditInfo.HSBSecondary;
                    break;
                case PropCopyPasteReset.Paste:
                    propEditInfo.HSBPrimary = clipboardPropColor.HSBPrimary;
                    propEditInfo.HSBSecondary = clipboardPropColor.HSBSecondary;
                    break;
                case PropCopyPasteReset.Reset:
                    propEditInfo.HSBPrimary = Constants.defaultHSBOffset;
                    propEditInfo.HSBSecondary = Constants.defaultHSBOffset;
                    break;
            }
        }

        public NewPropFunctions (AreaSceneBlackboard bb)
        {
            propEditInfo = bb.propEditInfo;
            clipboardPropColor = bb.clipboardPropColor;
        }

        readonly PropEditInfo propEditInfo;
        readonly ClipboardPropColor clipboardPropColor;
    }

    sealed class SelectedPropFunctions : PropEditFunctions
    {
        public (byte, bool) GetOrientation () => (placement.rotation, placement.flipped);
        public Vector3 GetOffset () => new Vector3 (placement.offsetX, 0f, placement.offsetZ);
        public Vector4 GetPrimaryColor () => placement.hsbPrimary;
        public Vector4 GetSecondaryColor () => placement.hsbSecondary;

        public void ChangeOrientation (PropOrientationChange change)
        {
            switch (change)
            {
                case PropOrientationChange.Snap:
                    am.SnapPropRotation (placement);
                    return;
                case PropOrientationChange.RotateClockwise:
                    placement.rotation = placement.rotation.OffsetAndWrap (false, 3);
                    break;
                case PropOrientationChange.Flip:
                    placement.flipped = !placement.flipped;
                    break;
                case PropOrientationChange.RotateAnticlockwise:
                    placement.rotation = placement.rotation.OffsetAndWrap (true, 3);
                    break;
                case PropOrientationChange.Reset:
                    placement.rotation = 0;
                    placement.flipped = false;
                    break;
                default:
                    return;
            }

            if (!placement.pivotIndex.IsValidIndex (am.points))
            {
                return;
            }

            var point = am.points[placement.pivotIndex];
            placement.Setup (am, point);
        }

        public void ChangeOffset (float x, float z)
        {
            placement.offsetX = x;
            placement.offsetZ = z;
        }

        public void ChangeColor (Vector4 primary, Vector4 secondary)
        {
            placement.hsbPrimary = primary;
            placement.hsbSecondary = secondary;
            placement.UpdateMaterial_HSBOffsets (placement.hsbPrimary, placement.hsbSecondary);
        }

        public void CopyPastePosition (PropCopyPasteReset cpr)
        {
            switch (cpr)
            {
                case PropCopyPasteReset.Copy:
                    propEditInfo.OffsetXClipboard = placement.offsetX;
                    propEditInfo.OffsetZClipboard = placement.offsetZ;
                    break;
                case PropCopyPasteReset.Paste:
                    placement.offsetX = propEditInfo.OffsetXClipboard;
                    placement.offsetZ = propEditInfo.OffsetZClipboard;
                    am.ExecutePropPlacement (placement);
                    break;
                case PropCopyPasteReset.Reset:
                    placement.offsetX = 0f;
                    placement.offsetZ = 0f;
                    am.ExecutePropPlacement (placement);
                    break;
            }
        }

        public void CopyPasteColor (PropCopyPasteReset cpr)
        {
            switch (cpr)
            {
                case PropCopyPasteReset.Copy:
                    clipboardPropColor.HSBPrimary = placement.hsbPrimary;
                    clipboardPropColor.HSBSecondary = placement.hsbSecondary;
                    Debug.Log ("AM | Prop HSV copied");
                    break;
                case PropCopyPasteReset.Paste:
                    placement.hsbPrimary = clipboardPropColor.HSBPrimary;
                    placement.hsbSecondary = clipboardPropColor.HSBSecondary;
                    placement.UpdateMaterial_HSBOffsets (placement.hsbPrimary, placement.hsbSecondary);
                    Debug.Log ("AM | Prop HSV pasted");
                    break;
                case PropCopyPasteReset.Reset:
                    placement.hsbPrimary = Constants.defaultHSBOffset;
                    placement.hsbSecondary = Constants.defaultHSBOffset;
                    placement.UpdateMaterial_HSBOffsets (placement.hsbPrimary, placement.hsbSecondary);
                    break;
            }
        }

        public readonly AreaPlacementProp placement;

        public SelectedPropFunctions (AreaSceneBlackboard bb, AreaPlacementProp placement)
        {
            this.placement = placement;
            am = bb.am;
            propEditInfo = bb.propEditInfo;
            clipboardPropColor = bb.clipboardPropColor;
        }

        readonly AreaManager am;
        readonly PropEditInfo propEditInfo;
        readonly ClipboardPropColor clipboardPropColor;
    }
}
