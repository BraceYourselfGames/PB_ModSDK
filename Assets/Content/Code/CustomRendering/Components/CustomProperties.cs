using UnityEngine;
using Unity.Entities;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace CustomRendering
{

    [StructLayout(LayoutKind.Explicit)]
    public struct HalfVector4 //Total stride should be 64 bits, 2 32 bit Uints, that each contain two 16 bit, packed fp16's
    {
        //Each Uint can take 32 bits of data
        [FieldOffset(0)] //0 - 3
        public uint x;
        [FieldOffset(4)] //4 - 7
        public uint y;

        [FieldOffset(0)] //0 - 1
        public ushort x1;
        [FieldOffset(2)] //2 - 3
        public ushort x2;
        
        [FieldOffset(4)] //4 - 5
        public ushort y1;
        [FieldOffset(6)] //6 - 7
        public ushort y2;
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HalfVector4(float4 vector)
        {
            x = 0;
            y = 0;
            x1 = (ushort)math.f32tof16(vector.x);
            x2 = (ushort)math.f32tof16(vector.y);
            y1 = (ushort)math.f32tof16(vector.z);
            y2 = (ushort)math.f32tof16(vector.w);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HalfVector4(Vector4 vector)
        {
            x = 0;
            y = 0;
            x1 = (ushort)math.f32tof16(vector.x);
            x2 = (ushort)math.f32tof16(vector.y);
            y1 = (ushort)math.f32tof16(vector.z);
            y2 = (ushort)math.f32tof16(vector.w);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HalfVector4(float x, float y, float z, float w)
        {
            this.x = 0;
            this.y = 0;
            x1 = (ushort)math.f32tof16(x);
            x2 = (ushort)math.f32tof16(y);
            y1 = (ushort)math.f32tof16(z);
            y2 = (ushort)math.f32tof16(w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(HalfVector4 lhs, HalfVector4 rhs)
        {
            return (lhs.x == rhs.x) && (lhs.y == rhs.y);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(HalfVector4 lhs, HalfVector4 rhs)
        {
            return (lhs.x != rhs.x) || (lhs.y != rhs.y);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct HalfVector8 //Total stride should be 128bits (64 + 64)
    {
        [FieldOffset(0)]
        public HalfVector4 primary;
        [FieldOffset(8)] //The stride of a Packed Vector4 should be 64bits (32 + 32), for the two internal uint32s
        public HalfVector4 secondary;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HalfVector8(float4 primary, float4 secondary)
        {
            this.primary = new HalfVector4(primary);
            this.secondary = new HalfVector4(secondary);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HalfVector8(HalfVector4 primary, HalfVector4 secondary)
        {
            this.primary = primary;
            this.secondary = secondary;
        }
    }

    //32 bits / four values at one byte each
    [StructLayout(LayoutKind.Explicit)]
    public struct FixedVector4
    {
        [FieldOffset(0)]
        public uint data;
        
        [FieldOffset(0)] 
        public byte x;
        [FieldOffset(1)]
        public byte y;
        [FieldOffset(2)]
        public byte z;
        [FieldOffset(3)]
        public byte w;

        public FixedVector4(float4 value)
        {
            data = 0;
            x = (byte)Mathf.Round(Mathf.Clamp01(value.x) * byte.MaxValue);
            y = (byte)Mathf.Round(Mathf.Clamp01(value.y) * byte.MaxValue);
            z = (byte)Mathf.Round(Mathf.Clamp01(value.z) * byte.MaxValue);
            w = (byte)Mathf.Round(Mathf.Clamp01(value.w) * byte.MaxValue);
        }
        
        //This will only work with normalized 0-1 floats
        public FixedVector4(float x, float y, float z, float w)
        {
            data = 0;
            this.x = (byte)Mathf.Round(Mathf.Clamp01(x) * byte.MaxValue);
            this.y = (byte)Mathf.Round(Mathf.Clamp01(y) * byte.MaxValue);
            this.z = (byte)Mathf.Round(Mathf.Clamp01(z) * byte.MaxValue);
            this.w = (byte)Mathf.Round(Mathf.Clamp01(w) * byte.MaxValue);
        }

        public FixedVector4(byte x, byte y, byte z, byte w)
        {
            data = 0;
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(FixedVector4 lhs, FixedVector4 rhs)
        {
            return (lhs.x == rhs.x) && (lhs.y == rhs.y);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(FixedVector4 lhs, FixedVector4 rhs)
        {
            return (lhs.x != rhs.x) || (lhs.y != rhs.y);
        }
    }

    //64 bits / two 32 bit values, containing four one byte values each
    [StructLayout(LayoutKind.Explicit)]
    public struct FixedVector8
    {
        [FieldOffset(0)] //0, 1, 2, 3
        public FixedVector4 primary;
        [FieldOffset(4)] //4, 5, 6, 7
        public FixedVector4 secondary;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedVector8(float4 primary, float4 secondary)
        {
            this.primary = new FixedVector4(primary);
            this.secondary = new FixedVector4(secondary);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedVector8(FixedVector4 primary, FixedVector4 secondary)
        {
            this.primary = primary;
            this.secondary = secondary;
        }
    }
    
    //If we make changes to the rendering properties, there's multiple places that must be changed to match
    //First, ensure all structured buffer formats align in Instancing_Shared.cginc
    //Second, Ensure pointer safety checks are correct in PhantomRendererSyncSystemV2.cs AssertPointerSafety()
    //Third, Ensure that the compute buffer strides match in BatchLayout.cs

    //64 bit / 8 byte values, fixed precision
    public struct HSBOffsetProperty : IComponentData
    {
        public HalfVector8 property;
    }

    //128 bit / 8 fp16 values, half precision
    public struct DamageProperty : IComponentData
    {
        //Primary is top, Secondary is bottom
        public HalfVector8 property;
    }

    //64 bit / 8 byte values, fixed precision
    public struct IntegrityProperty : IComponentData
    {
        //Primary is top, Secondary is bottom
        public FixedVector8 property;
    }
    
    public struct ScaleShaderProperty : IComponentData
    {
        public HalfVector4 property;
    }

    public struct PackedPropShaderProperty : IComponentData
    {
        public HalfVector4 property;
    }
    
    [Serializable]
    public struct RendererGroup : ISharedComponentData
    {
        public int id;
    }

    public struct CullingIndex : IComponentData
    {
        public int index;
    }

    public struct CulledTag : IComponentData { }


    public struct PropTag : IComponentData { }

    public struct PointInternalTag : IComponentData { }

    public struct PointExternalTag : IComponentData { }

    public struct AnimatingTag : IComponentData
    {
    }

    public struct PropertyVersion : IComponentData
    {
        public int version;
    }
}