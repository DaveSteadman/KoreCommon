// <fileheader>

using System;

// Design Decisions:
// - Zero angle is "east" (3 o'clock) and angles increase anti-clockwise.

// Point = a position, absolute.
// Vector = an offset, relative, scalable.

namespace KoreCommon;

public struct KoreXYVector
{
    public double X { get; }
    public double Y { get; }

    public double Length => Math.Sqrt(X * X + Y * Y);
    public double Magnitude => Length;

    public KoreXYVector Normalized => Length == 0 ? new KoreXYVector(0, 0) : new KoreXYVector(X / Length, Y / Length);
    public KoreXYVector UnitVector => Normalized; // Alias for Normalized

    // --------------------------------------------------------------------------------------------
    // MARK: Constructor
    // --------------------------------------------------------------------------------------------

    public KoreXYVector(double x, double y)
    {
        X = x;
        Y = y;
    }

    public KoreXYVector(KoreXYVector xy)
    {
        X = xy.X;
        Y = xy.Y;
    }

    // Zero default constructor
    public static KoreXYVector Zero => new KoreXYVector(0, 0);
    public static KoreXYVector One  => new KoreXYVector(1, 1);

    // --------------------------------------------------------------------------------------------
    // MARK: Public methods
    // --------------------------------------------------------------------------------------------

    // return the angle FROM this point TO the given point - East / Positve X axis is zero
    public double AngleToRads(double x, double y)
    {
        if (KoreValueUtils.EqualsWithinTolerance(x, X) && KoreValueUtils.EqualsWithinTolerance(y, Y))
            return 0;

        return Math.Atan2(y - Y, x - X);
    }

    public double AngleToRads(KoreXYVector xy)
    {
        if (EqualsWithinTolerance(this, xy))
            return 0;

        // return the angle FROM this point TO the given point
        return Math.Atan2(xy.Y - Y, xy.X - X);
    }

    public double AngleToDegs(double x, double y) => KoreValueUtils.RadsToDegs(AngleToRads(x, y));
    public double AngleToDegs(KoreXYVector xy) => KoreValueUtils.RadsToDegs(AngleToRads(xy));

    public double DistanceTo(double x, double y)
    {
        return Math.Sqrt(Math.Pow(X - x, 2) + Math.Pow(Y - y, 2));
    }

    public double DistanceTo(KoreXYVector xy)
    {
        return Math.Sqrt(Math.Pow(X - xy.X, 2) + Math.Pow(Y - xy.Y, 2));
    }

    // --------------------------------------------------------------------------------------------

    // Return a new point offset by an XY amount.

    public KoreXYVector Offset(double x, double y) => new KoreXYVector(X + x, Y + y);
    public KoreXYVector Offset(KoreXYVector xy) => new KoreXYVector(X + xy.X, Y + xy.Y);
    public KoreXYVector Offset(KoreXYPolarOffset o) => Offset(KoreXYPolarOffsetOps.ToXY(o));

    // --------------------------------------------------------------------------------------------
    // MARK: static methods
    // --------------------------------------------------------------------------------------------

    public static KoreXYVector Sum(KoreXYVector a, KoreXYVector b) => new KoreXYVector(a.X + b.X, a.Y + b.Y);
    public static KoreXYVector Diff(KoreXYVector a, KoreXYVector b) => new KoreXYVector(a.X - b.X, a.Y - b.Y);
    public static KoreXYVector Scale(KoreXYVector a, double b) => new KoreXYVector(a.X * b, a.Y * b);

    // Usage: bool matching = KoreXYVector.EqualsWithinTolerance(a, b, tolerance)
    public static bool EqualsWithinTolerance(KoreXYVector a, KoreXYVector b, double tolerance = KoreConsts.ArbitrarySmallDouble)
    {
        return KoreValueUtils.EqualsWithinTolerance(a.X, b.X, tolerance) && KoreValueUtils.EqualsWithinTolerance(a.Y, b.Y, tolerance);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Operator overloads
    // --------------------------------------------------------------------------------------------

    // + operator overload
    public static KoreXYVector operator +(KoreXYVector a, KoreXYVector b) { return new KoreXYVector(a.X + b.X, a.Y + b.Y); }

    // - operator overload for subtracting points
    public static KoreXYVector operator -(KoreXYVector a, KoreXYVector b) { return new KoreXYVector(a.X - b.X, a.Y - b.Y); }

    // * operator overload for scaling point in relation to origin
    public static KoreXYVector operator *(KoreXYVector a, double b) { return new KoreXYVector(a.X * b, a.Y * b); }
    public static KoreXYVector operator *(double b, KoreXYVector a) { return new KoreXYVector(a.X * b, a.Y * b); }

    // / operator overload for scaling point in relation to origin
    public static KoreXYVector operator /(KoreXYVector a, double b) { return new KoreXYVector(a.X / b, a.Y / b); }
    public static KoreXYVector operator /(double b, KoreXYVector a) { return new KoreXYVector(a.X / b, a.Y / b); }

    // MARK: ToString
    public override string ToString()
    {
        return $"KoreXYVector(X: {X:F3}, Y: {Y:F3})";
    }
}