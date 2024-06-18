using System.Runtime.InteropServices;

[StructLayout (LayoutKind.Explicit)]
public struct PackBytesToInt
{
    [FieldOffset (0)]
    public byte byte0;
    [FieldOffset (1)]
    public byte byte1;
    [FieldOffset (2)]
    public byte byte2;
    [FieldOffset (3)]
    public byte byte3;

    [FieldOffset (0)]
    public int result;

    public PackBytesToInt 
    (
        byte byte0,
        byte byte1,
        byte byte2,
        byte byte3
    )
    {
        result = 0;
        this.byte0 = byte0;
        this.byte1 = byte1;
        this.byte2 = byte2;
        this.byte3 = byte3;
    }
}

[StructLayout (LayoutKind.Explicit)]
public struct PackBytesToLong
{
    [FieldOffset (0)]
    public byte byte0;
    [FieldOffset (1)]
    public byte byte1;
    [FieldOffset (2)]
    public byte byte2;
    [FieldOffset (3)]
    public byte byte3;
    [FieldOffset (4)]
    public byte byte4;
    [FieldOffset (5)]
    public byte byte5;
    [FieldOffset (6)]
    public byte byte6;
    [FieldOffset (7)]
    public byte byte7;

    [FieldOffset (0)]
    public long result;

    public PackBytesToLong
    (
        byte byte0,
        byte byte1,
        byte byte2,
        byte byte3,
        byte byte4,
        byte byte5,
        byte byte6,
        byte byte7
    )
    {
        result = 0;
        this.byte0 = byte0;
        this.byte1 = byte1;
        this.byte2 = byte2;
        this.byte3 = byte3;
        this.byte4 = byte4;
        this.byte5 = byte5;
        this.byte6 = byte6;
        this.byte7 = byte7;
    }
}

[StructLayout (LayoutKind.Explicit)]
public struct PackIntegerAndBytesToLong
{
    [FieldOffset (0)]
    public int integer;
    [FieldOffset (4)]
    public byte byte0;
    [FieldOffset (5)]
    public byte byte1;
    [FieldOffset (6)]
    public byte byte2;
    [FieldOffset (7)]
    public byte byte3;

    [FieldOffset (0)]
    public long result;

    public PackIntegerAndBytesToLong
    (
        int integer,
        byte byte0,
        byte byte1,
        byte byte2,
        byte byte3
    )
    {
        result = 0;
        this.integer = integer;
        this.byte0 = byte0;
        this.byte1 = byte1;
        this.byte2 = byte2;
        this.byte3 = byte3;
    }
}

[StructLayout (LayoutKind.Explicit)]
public struct PackIntegersToLong
{
    [FieldOffset (0)]
    public int integer0;
    [FieldOffset (4)]
    public int integer1;

    [FieldOffset (0)]
    public long result;

    public PackIntegersToLong
    (
        int integer0,
        int integer1
    )
    {
        result = 0;
        this.integer0 = integer0;
        this.integer1 = integer1;
    }
}

[StructLayout (LayoutKind.Explicit)]
public struct PackUnsignedShortsToLong
{
    [FieldOffset (0)]
    public ushort ushort0;
    [FieldOffset (2)]
    public ushort ushort1;
    [FieldOffset (4)]
    public ushort ushort2;
    [FieldOffset (6)]
    public ushort ushort3;

    [FieldOffset (0)]
    public long result;

    public PackUnsignedShortsToLong
    (
        ushort ushort0,
        ushort ushort1,
        ushort ushort2,
        ushort ushort3
    )
    {
        result = 0;
        this.ushort0 = ushort0;
        this.ushort1 = ushort1;
        this.ushort2 = ushort2;
        this.ushort3 = ushort3;
    }
}