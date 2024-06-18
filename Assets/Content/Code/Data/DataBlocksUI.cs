using System;

namespace PhantomBrigade.Data
{
    public enum FrameLocation
    {
        TopLeft,
        Top,
        TopRight,
        Left,
        Center,
        Right,
        BottomLeft,
        Bottom,
        BottomRight
    }
    
    public enum FrameGradientMode
    {
        None,
        Top,
        Bottom,
        Left,
        Right
    }
    
    public enum TextLocation
    {
        TopLeft,
        TopRight,
        RightTop,
        RightBottom,
        BottomLeft,
        BottomRight,
        LeftTop,
        LeftBottom
    }
    
    public enum LabelAnchorCorner
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
    
    public enum LabelOffsetDirection
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
    
    public enum ButtonLocation
    {
        None,
        Top,
        Bottom
    }

    public class UIBasicSprite
    {
        public enum Pivot
        {
            TopLeft,
            Top,
            TopRight,
            Left,
            Center,
            Right,
            BottomLeft,
            Bottom,
            BottomRight,
        }
    }
}