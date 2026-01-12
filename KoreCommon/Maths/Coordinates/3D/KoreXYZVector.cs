// <fileheader>

using System;

// KoreXYZVector: A class to hold an XYZ vector. Units are abstract, for the consumer to decide the context.
// This class is immutable, so all operations return a new object.

// An XYZVector is an abstract, relative difference in 3D position. An XYZPoint is an absolute position.
// - So you can't scale a position, but you can scale a vector. etc.

namespace KoreCommon;

public struct KoreXYZVector
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }

    public double Magnitude
    {
        get
        {
            return Math.Sqrt((X * X) + (Y * Y) + (Z * Z));
        }
        set
        {
            double currMag = Magnitude; // store, avoid caclulating twice
            if (currMag < KoreConsts.ArbitrarySmallDouble) // if too close to a div0
            {
                X = value; Y = 0; Z = 0;
            }
            else
            {
                double scaleFactor = value / currMag;
                X *= scaleFactor;
                Y *= scaleFactor;
                Z *= scaleFactor;
            }
        }
    }

    public double Length => Magnitude;

    // --------------------------------------------------------------------------------------------

    public KoreXYZVector(double xm, double ym, double zm)
    {
        this.X = xm;
        this.Y = ym;
        this.Z = zm;
    }

    // Convenience constructor to create a vector from a point
    public KoreXYZVector(KoreXYZVector point)
    {
        this.X = point.X;
        this.Y = point.Y;
        this.Z = point.Z;
    }

    // Return a zero point as a default value
    // Example: KoreXYZVector newPos = KoreXYZVector.Zero();
    public static KoreXYZVector Zero => new KoreXYZVector(0, 0, 0);

    public static KoreXYZVector Right    => new KoreXYZVector(1, 0, 0);
    public static KoreXYZVector Left     => new KoreXYZVector(-1, 0, 0);
    public static KoreXYZVector Up       => new KoreXYZVector(0, 1, 0);
    public static KoreXYZVector Down     => new KoreXYZVector(0, -1, 0);
    public static KoreXYZVector Forward  => new KoreXYZVector(0, 0, 1);
    public static KoreXYZVector Backward => new KoreXYZVector(0, 0, -1);

    // --------------------------------------------------------------------------------------------
    // Basic operations
    // --------------------------------------------------------------------------------------------

    public KoreXYZVector Offset(double x, double y, double z) => new KoreXYZVector(X + x, Y + y, Z + z);
    public KoreXYZVector Offset(KoreXYZVector shiftXYZ) => new KoreXYZVector(X + shiftXYZ.X, Y + shiftXYZ.Y, Z + shiftXYZ.Z);
    public KoreXYZVector Subtract(KoreXYZVector inputXYZ) => new KoreXYZVector(X - inputXYZ.X, Y - inputXYZ.Y, Z - inputXYZ.Z);
    public KoreXYZVector Scale(double scaleFactor) => new KoreXYZVector(X * scaleFactor, Y * scaleFactor, Z * scaleFactor);
    public KoreXYZVector Invert() => new KoreXYZVector(-X, -Y, -Z);

    public KoreXYZVector Normalize()
    {
        double mag = Magnitude;
        if (mag < KoreConsts.ArbitrarySmallDouble)
            return new KoreXYZVector(1, 0, 0);
        else
            return new KoreXYZVector(X / mag, Y / mag, Z / mag);
    }

    public bool IsZero()
    {
        return Math.Abs(X) < KoreConsts.ArbitrarySmallDouble &&
               Math.Abs(Y) < KoreConsts.ArbitrarySmallDouble &&
               Math.Abs(Z) < KoreConsts.ArbitrarySmallDouble;
    }

    public bool IsEqualTo(KoreXYZVector other, double tolerance = KoreConsts.ArbitrarySmallDouble)
    {
        return Math.Abs(X - other.X) < tolerance &&
               Math.Abs(Y - other.Y) < tolerance &&
               Math.Abs(Z - other.Z) < tolerance;
    }


    public KoreXYZVector FlipX() { return new KoreXYZVector(-X, Y, Z); }
    public KoreXYZVector FlipY() { return new KoreXYZVector(X, -Y, Z); }
    public KoreXYZVector FlipZ() { return new KoreXYZVector(X, Y, -Z); }


    // --------------------------------------------------------------------------------------------

    public KoreXYZVector XYZTo(KoreXYZVector remoteXYZ)
    {
        return new KoreXYZVector(remoteXYZ.X - X, remoteXYZ.Y - Y, remoteXYZ.Z - Z);
    }

    public double DistanceTo(KoreXYZVector inputXYZ)
    {
        double diffX = X - inputXYZ.X;
        double diffY = Y - inputXYZ.Y;
        double diffZ = Z - inputXYZ.Z;

        return Math.Sqrt(diffX * diffX + diffY * diffY + diffZ * diffZ);
    }

    // --------------------------------------------------------------------------------------------
    // Polar Vectors
    // --------------------------------------------------------------------------------------------

    public KoreXYZPolarOffset PolarOffsetTo(KoreXYZVector p)
    {
        KoreXYZVector diff = XYZTo(p);

        KoreXYZPolarOffset newOffset = KoreXYZPolarOffset.FromXYZ(diff);

        return newOffset;
    }

    public KoreXYZVector PlusPolarOffset(KoreXYZPolarOffset offset)
    {
        KoreXYZVector diff = offset.ToXYZ();
        return new KoreXYZVector(X + diff.X, Y + diff.Y, Z + diff.Z);
    }

    // --------------------------------------------------------------------------------------------
    // operator overloads
    // --------------------------------------------------------------------------------------------

    // + operator overload
    public static KoreXYZVector operator +(KoreXYZVector a, KoreXYZVector b) { return new KoreXYZVector(a.X + b.X, a.Y + b.Y, a.Z + b.Z); }

    // - operator overload for subtracting points
    public static KoreXYZVector operator -(KoreXYZVector a, KoreXYZVector b) { return new KoreXYZVector(a.X - b.X, a.Y - b.Y, a.Z - b.Z); }

    // * operator overload for scaling a
    public static KoreXYZVector operator *(KoreXYZVector a, double scaleFactor) { return new KoreXYZVector(a.X * scaleFactor, a.Y * scaleFactor, a.Z * scaleFactor); }
    public static KoreXYZVector operator *(double scaleFactor, KoreXYZVector a) { return new KoreXYZVector(a.X * scaleFactor, a.Y * scaleFactor, a.Z * scaleFactor); }

    // / operator overload for scaling a point
    public static KoreXYZVector operator /(KoreXYZVector a, double scaleFactor) { return new KoreXYZVector(a.X / scaleFactor, a.Y / scaleFactor, a.Z / scaleFactor); }
    public static KoreXYZVector operator /(double scaleFactor, KoreXYZVector a) { return new KoreXYZVector(a.X / scaleFactor, a.Y / scaleFactor, a.Z / scaleFactor); }

    // --------------------------------------------------------------------------------------------
    // Conversion
    // --------------------------------------------------------------------------------------------

    public override string ToString()
    {
        return $"({X:F2}, {Y:F2}, {Z:F2})(Mag {Magnitude:F2})";
    }

    // ========================================================================
    // static Ops
    // ========================================================================

    public static KoreXYZVector Diff(KoreXYZVector inputXYZ1, KoreXYZVector inputXYZ2)
    {
        double diffX = inputXYZ2.X - inputXYZ1.X;
        double diffY = inputXYZ2.Y - inputXYZ1.Y;
        double diffZ = inputXYZ2.Z - inputXYZ1.Z;

        return new KoreXYZVector(diffX, diffY, diffZ);
    }

    public static KoreXYZVector Sum(KoreXYZVector inputXYZ1, KoreXYZVector inputXYZ2)
    {
        double sumX = inputXYZ2.X + inputXYZ1.X;
        double sumY = inputXYZ2.Y + inputXYZ1.Y;
        double sumZ = inputXYZ2.Z + inputXYZ1.Z;

        return new KoreXYZVector(sumX, sumY, sumZ);
    }

    public static KoreXYZVector Scale(KoreXYZVector inputXYZ1, double scaleFactor)
    {
        double scaledX = inputXYZ1.X * scaleFactor;
        double scaledY = inputXYZ1.Y * scaleFactor;
        double scaledZ = inputXYZ1.Z * scaleFactor;

        return new KoreXYZVector(scaledX, scaledY, scaledZ);
    }

    // Usage: KoreXYZVector.DotProduct(v1, v2)
    public static double DotProduct(KoreXYZVector inputXYZ1, KoreXYZVector inputXYZ2)
    {
        return (inputXYZ1.X * inputXYZ2.X) + (inputXYZ1.Y * inputXYZ2.Y) + (inputXYZ1.Z * inputXYZ2.Z);
    }

    // Usage: KoreXYZVector.CrossProduct(v1, v2)
    public static KoreXYZVector CrossProduct(KoreXYZVector a, KoreXYZVector b)
    {
        return new KoreXYZVector(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X
        );
    }

    // Create a vector that is perpendicular to this vector.
    // Usage: KoreXYZVector.AngleBetween(v1, v2)
    public KoreXYZVector ArbitraryPerpendicular()
    {
        if (IsZero())
            return new KoreXYZVector(1, 0, 0); // Default for zero input

        KoreXYZVector axis = Math.Abs(X) <= Math.Abs(Y) && Math.Abs(X) <= Math.Abs(Z)
            ? new KoreXYZVector(1, 0, 0)
            : (Math.Abs(Y) <= Math.Abs(Z)
                ? new KoreXYZVector(0, 1, 0)
                : new KoreXYZVector(0, 0, 1));

        var perp = CrossProduct(this, axis);
        if (perp.IsZero())
            perp = CrossProduct(this, new KoreXYZVector(1, 1, 1));  // Last resort

        return perp.Normalize();
    }


}
