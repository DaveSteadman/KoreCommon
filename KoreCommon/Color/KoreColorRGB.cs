// <fileheader>

// Custom colour class, protable between frameworks.

using System;

#nullable enable

namespace KoreCommon;

public struct KoreColorRGB : IEquatable<KoreColorRGB>
{
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }
    public byte A { get; set; }

    public float Rf
    {
        get => KoreColorIO.ByteToFloat(R);
        set => R = KoreColorIO.FloatToByte(value);
    }

    public float Gf
    {
        get => KoreColorIO.ByteToFloat(G);
        set => G = KoreColorIO.FloatToByte(value);
    }

    public float Bf
    {
        get => KoreColorIO.ByteToFloat(B);
        set => B = KoreColorIO.FloatToByte(value);
    }

    public float Af
    {
        get => KoreColorIO.ByteToFloat(A);
        set => A = KoreColorIO.FloatToByte(value);
    }

    public bool IsTransparent => (A < KoreColorIO.MaxByte);

    // --------------------------------------------------------------------------------------------
    // Constructors
    // --------------------------------------------------------------------------------------------

    public KoreColorRGB(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public KoreColorRGB(byte r, byte g, byte b)
    {
        R = r;
        G = g;
        B = b;
        A = KoreColorIO.MaxByte;
    }

    public KoreColorRGB(float r, float g, float b, float a)
    {
        R = KoreColorIO.FloatToByte(r);
        G = KoreColorIO.FloatToByte(g);
        B = KoreColorIO.FloatToByte(b);
        A = KoreColorIO.FloatToByte(a);
    }

    public KoreColorRGB(float r, float g, float b)
    {
        R = KoreColorIO.FloatToByte(r);
        G = KoreColorIO.FloatToByte(g);
        B = KoreColorIO.FloatToByte(b);
        A = KoreColorIO.MaxByte;
    }

    public static readonly KoreColorRGB Zero = new KoreColorRGB(KoreColorIO.MinByte, KoreColorIO.MinByte, KoreColorIO.MinByte, KoreColorIO.MinByte);
    public static readonly KoreColorRGB White = new KoreColorRGB(KoreColorIO.MaxByte, KoreColorIO.MaxByte, KoreColorIO.MaxByte, KoreColorIO.MaxByte);
    public static readonly KoreColorRGB Black = new KoreColorRGB(0, 0, 0, KoreColorIO.MaxByte);

    // --------------------------------------------------------------------------------------------
    // MARK: Equality Operations
    // --------------------------------------------------------------------------------------------

    public override bool Equals(object? obj)
    {
        return obj is KoreColorRGB other && Equals(other);
    }

    public bool Equals(KoreColorRGB other)
    {
        return R == other.R && G == other.G && B == other.B && A == other.A;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(R, G, B, A);
    }

    public static bool operator ==(KoreColorRGB left, KoreColorRGB right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(KoreColorRGB left, KoreColorRGB right)
    {
        return !left.Equals(right);
    }

}