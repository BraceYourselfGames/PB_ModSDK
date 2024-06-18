using UnityEngine;
using System;

[System.Serializable]
public struct Vector3Int : IComparable<Vector3Int>
{
    public int x;
    public int y;
    public int z;

    public static readonly Vector3Int size0x0x0 = new Vector3Int (0, 0, 0);
    public static readonly Vector3Int size1x1x1 = new Vector3Int (1, 1, 1);
    public static readonly Vector3Int size1x1x1Neg = new Vector3Int (-1, -1, -1);
    public static readonly Vector3Int size2x2x2 = new Vector3Int (2, 2, 2);

    public static readonly Vector3Int size2x2x2Neg = new Vector3Int (-2, -2, -2);
    public static readonly Vector3Int size3x3x3 = new Vector3Int (3, 3, 3);
    public static Vector3Int size3x1x3 = new Vector3Int (3, 1, 3);

    public Vector3Int (int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3Int (Vector3 vector3Float)
    {
        this.x = Mathf.RoundToInt (vector3Float.x);
        this.y = Mathf.RoundToInt (vector3Float.y);
        this.z = Mathf.RoundToInt (vector3Float.z);
    }

    public override string ToString () => "(" + x + ", " + y + ", " + z + ")";

    public Vector3Int Invert ()
    {
        return new Vector3Int (-x, -y, -z);
    }

    public Vector3 ToVector3 ()
    {
        return new Vector3 (x, y, z);
    }

    public override int GetHashCode ()
    {
        return x.GetHashCode () ^ y.GetHashCode () ^ z.GetHashCode ();
    }

    public override bool Equals (object obj)
    {
        return obj is Vector3Int && this == (Vector3Int)obj;
    }

    public static bool operator == (Vector3Int a, Vector3Int b)
    {
        return a.x == b.x && a.y == b.y && a.z == b.z;
    }

    public static bool operator != (Vector3Int a, Vector3Int b)
    {
        return a.x != b.x || a.y != b.y || a.z != b.z;
    }

    public static Vector3Int operator * (Vector3Int a, Vector3Int b)
    {
        return new Vector3Int (a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public static Vector3Int operator * (Vector3Int a, int b)
    {
        return new Vector3Int (a.x * b, a.y * b, a.z * b);
    }

    public static Vector3Int operator / (Vector3Int a, int b)
    {
	    return new Vector3Int (a.x / b, a.y / b, a.z / b);
    }

    public static Vector3Int operator + (Vector3Int a, Vector3Int b)
    {
        return new Vector3Int (a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static Vector3Int operator - (Vector3Int a, Vector3Int b)
    {
        return new Vector3Int (a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public int CompareTo (Vector3Int other)
    {
        return x.CompareTo (other.x) + y.CompareTo (other.y) + z.CompareTo (other.z);
    }
}

public static class Vector3IntExtensions
{
    public static Vector3Int RotateByIndex (this Vector3Int v, int rotationIndex)
    {
        rotationIndex = rotationIndex % 4;
        if (rotationIndex == 0)
        {
            return v;
        }
        else if (rotationIndex == 1)
        {
            return new Vector3Int (-v.z, v.y, v.x);
        }
        else if (rotationIndex == 2)
        {
            return new Vector3Int (-v.x, v.y, -v.z);
        }
        else
        {
            return new Vector3Int (v.z, v.y, -v.x);
        }
    }

    public static Vector3Int FlipOnX (this Vector3Int v)
    {
        return new Vector3Int (-v.x, v.y, v.z);
    }

    public static Vector3Int FlipOnZ (this Vector3Int v)
    {
        return new Vector3Int (v.x, v.y, -v.z);
    }
}
