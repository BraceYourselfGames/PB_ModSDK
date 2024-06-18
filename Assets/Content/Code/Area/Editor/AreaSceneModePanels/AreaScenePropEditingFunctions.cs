using UnityEngine;

namespace Area
{
    using Scene;

    interface PropEditFunctions
    {
        (byte, bool) GetOrientation ();
        Vector3 GetOffset ();
        ref Vector4 PrimaryColor { get; }
        ref Vector4 SecondaryColor { get; }
        void ChangeOrientation (bool snap, bool rotateLeft, bool rotateRight, bool flip, bool reset);
        void ChangeOffset (float x, float z);
        void CopyPastePosition (PropCopyPasteReset cpr);
        void OnColorGUIChanged ();
        void CopyPasteColor (PropCopyPasteReset cpr);
    }

    sealed class NewPropFunctions : PropEditFunctions
    {
        public (byte, bool) GetOrientation () => (propEditInfo.Rotation, propEditInfo.Flipped);
        public Vector3 GetOffset () => new Vector3 (propEditInfo.OffsetX, 0, propEditInfo.OffsetZ);
        public ref Vector4 PrimaryColor => ref propEditInfo.HSBPrimary;
        public ref Vector4 SecondaryColor => ref propEditInfo.HSBSecondary;

        public void ChangeOrientation (bool snap, bool rotateLeft, bool rotateRight, bool flip, bool reset)
        {
            if (rotateLeft)
            {
                propEditInfo.Rotation = propEditInfo.Rotation.OffsetAndWrap (false, 3);
            }
            if (rotateRight)
            {
                propEditInfo.Rotation = propEditInfo.Rotation.OffsetAndWrap (true, 3);
            }
            if (flip)
            {
                propEditInfo.Flipped = !propEditInfo.Flipped;
            }
            if (reset)
            {
                propEditInfo.Rotation = 0;
                propEditInfo.Flipped = false;
            }
        }

        public void ChangeOffset (float x, float z)
        {
            propEditInfo.OffsetX = x;
            propEditInfo.OffsetZ = z;
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

        public void OnColorGUIChanged () { }

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
        public ref Vector4 PrimaryColor => ref placement.hsbPrimary;
        public ref Vector4 SecondaryColor => ref placement.hsbSecondary;

        public void ChangeOrientation (bool snap, bool rotateLeft, bool rotateRight, bool flip, bool reset)
        {
            if (snap)
            {
                am.SnapPropRotation (placement);
            }

            if (placement == propEditInfo.PlacementHandled && command == PropEditCommand.RotateLeft)
            {
                rotateLeft = true;
            }
            if (rotateLeft)
            {
                placement.rotation = placement.rotation.OffsetAndWrap (false, 3);
                am.ExecutePropPlacement (placement);
            }

            if (placement == propEditInfo.PlacementHandled && command == PropEditCommand.Flip)
            {
                flip = true;
            }
            if (flip)
            {
                placement.flipped = !placement.flipped;
                am.ExecutePropPlacement (placement);
            }

            if (rotateRight)
            {
                placement.rotation = placement.rotation.OffsetAndWrap (true, 3);
                am.ExecutePropPlacement (placement);
            }

            if (reset)
            {
                placement.rotation = 0;
                placement.flipped = false;
                am.ExecutePropPlacement (placement);
            }
        }

        public void ChangeOffset (float x, float z)
        {
            placement.offsetX = x;
            placement.offsetZ = z;
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

        public void OnColorGUIChanged () => placement.UpdateMaterial_HSBOffsets (placement.hsbPrimary, placement.hsbSecondary);

        public void CopyPasteColor (PropCopyPasteReset cpr)
        {
            var copy = cpr == PropCopyPasteReset.Copy;
            if (placement == propEditInfo.PlacementHandled && command == PropEditCommand.CopyColor)
            {
                copy = true;
            }
            if (copy)
            {
                clipboardPropColor.HSBPrimary = placement.hsbPrimary;
                clipboardPropColor.HSBSecondary = placement.hsbSecondary;
                Debug.Log ("AM | Prop HSV copied");
                return;
            }

            var paste = cpr == PropCopyPasteReset.Paste;
            if (placement == propEditInfo.PlacementHandled && command == PropEditCommand.PasteColor)
            {
                paste = true;
            }
            if (paste)
            {
                placement.hsbPrimary = clipboardPropColor.HSBPrimary;
                placement.hsbSecondary = clipboardPropColor.HSBSecondary;
                placement.UpdateMaterial_HSBOffsets (placement.hsbPrimary, placement.hsbSecondary);
                Debug.Log ("AM | Prop HSV pasted");
                return;
            }

            if (cpr == PropCopyPasteReset.Reset)
            {
                placement.hsbPrimary = Constants.defaultHSBOffset;
                placement.hsbSecondary = Constants.defaultHSBOffset;
                placement.UpdateMaterial_HSBOffsets (placement.hsbPrimary, placement.hsbSecondary);
            }
        }

        public SelectedPropFunctions (AreaSceneBlackboard bb)
        {
            propEditInfo = bb.propEditInfo;
            clipboardPropColor = bb.clipboardPropColor;
        }

        public AreaManager am;
        public PropEditCommand command;
        public AreaPlacementProp placement;

        readonly PropEditInfo propEditInfo;
        readonly ClipboardPropColor clipboardPropColor;
    }
}
