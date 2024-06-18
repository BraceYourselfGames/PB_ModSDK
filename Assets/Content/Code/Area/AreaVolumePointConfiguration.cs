namespace Area
{
    [System.Serializable]
    public struct AreaVolumePointConfiguration
    {
        public AreaVolumePointState corner0;
        public AreaVolumePointState corner1;
        public AreaVolumePointState corner2;
        public AreaVolumePointState corner3;
        public AreaVolumePointState corner4;
        public AreaVolumePointState corner5;
        public AreaVolumePointState corner6;
        public AreaVolumePointState corner7;

        public AreaVolumePointConfiguration
        (
            AreaVolumePointState corner0,
            AreaVolumePointState corner1,
            AreaVolumePointState corner2,
            AreaVolumePointState corner3,
            AreaVolumePointState corner4,
            AreaVolumePointState corner5,
            AreaVolumePointState corner6,
            AreaVolumePointState corner7
        )
        {
            this.corner0 = corner0;
            this.corner1 = corner1;
            this.corner2 = corner2;
            this.corner3 = corner3;
            this.corner4 = corner4;
            this.corner5 = corner5;
            this.corner6 = corner6;
            this.corner7 = corner7;
        }

        public AreaVolumePointState GetCornerState (int index)
        {
            if (index == 0)
                return corner0;
            if (index == 1)
                return corner1;
            if (index == 2)
                return corner2;
            if (index == 3)
                return corner3;
            if (index == 4)
                return corner4;
            if (index == 5)
                return corner5;
            if (index == 6)
                return corner6;
            else
                return corner7;
        }

        public override string ToString ()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder ();
            sb.Append ('(');
            sb.Append (corner0.ToString ());
            sb.Append (", ");
            sb.Append (corner1.ToString ());
            sb.Append (", ");
            sb.Append (corner2.ToString ());
            sb.Append (", ");
            sb.Append (corner3.ToString ());
            sb.Append (", ");
            sb.Append (corner4.ToString ());
            sb.Append (", ");
            sb.Append (corner5.ToString ());
            sb.Append (", ");
            sb.Append (corner6.ToString ());
            sb.Append (", ");
            sb.Append (corner7.ToString ());
            sb.Append (')');
            return sb.ToString ();
        }

        public string ToStringShort ()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder ();
            sb.Append ('(');
            sb.Append (((int)corner0).ToString ());
            sb.Append (((int)corner1).ToString ());
            sb.Append (((int)corner2).ToString ());
            sb.Append (((int)corner3).ToString ());
            sb.Append ("-");
            sb.Append (((int)corner4).ToString ());
            sb.Append (((int)corner5).ToString ());
            sb.Append (((int)corner6).ToString ());
            sb.Append (((int)corner7).ToString ());
            sb.Append (')');
            return sb.ToString ();
        }
    }
}