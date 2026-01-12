// <fileheader>

using System;

namespace KoreCommon;

public struct KoreXYZBox
{
    public KoreXYZVector Center { get; set; } = KoreXYZVector.Zero;

    public enum EnumFace   { Top, Bottom, Left, Right, Front, Back }
    public enum EnumCorner { TopLeftFront, TopRightFront, BottomLeftFront, BottomRightFront, TopLeftBack, TopRightBack, BottomLeftBack, BottomRightBack }
    public enum EnumEdge   { TopFront, TopBack, TopLeft, TopRight, BottomFront, BottomBack, BottomLeft, BottomRight, FrontLeft, FrontRight, BackLeft, BackRight }

    public double Width  { get; set; } = 0;
    public double Height { get; set; } = 0;
    public double Length { get; set; } = 0;

    public double Volume => Width * Height * Length;

    // --------------------------------------------------------------------------------------------
    // MARK: Constructors
    // --------------------------------------------------------------------------------------------

    public KoreXYZBox()
    {
        Center = KoreXYZVector.Zero;

        Width  = 0;
        Height = 0;
        Length = 0;
    }

    public KoreXYZBox(KoreXYZVector center, double width, double height, double length)
    {
        Center = center;
        Width  = width;
        Height = height;
        Length = length;
    }

    // --------------------------------------------------------------------------------------------

    // Constructors for common sizes

    public static KoreXYZBox Zero
    {
        get { return new KoreXYZBox(KoreXYZVector.Zero, 0, 0, 0); }
    }

    public static KoreXYZBox One
    {
        get { return new KoreXYZBox(KoreXYZVector.Zero, 1, 1, 1); }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Box Edits
    // --------------------------------------------------------------------------------------------

    // Scale the box by a factor - Treat the box as immutable and return a new box.

    public KoreXYZBox Scale(double scale)
    {
        return new KoreXYZBox(Center, Width * scale, Height * scale, Length * scale);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Offset methods
    // --------------------------------------------------------------------------------------------

    public double OffsetForwards  { get { return (Length / 2) - Center.Z; } }
    public double OffsetBackwards { get { return (Length / 2) + Center.Z; } }
    public double OffsetLeft      { get { return (Width  / 2) - Center.X; } }
    public double OffsetRight     { get { return (Width  / 2) + Center.X; } }
    public double OffsetUp        { get { return (Height / 2) - Center.Y; } }
    public double OffsetDown      { get { return (Height / 2) + Center.Y; } }

    public double LongestOffset()
    {
        double longestDimension = Math.Max(Width, Math.Max(Height, Length));
        return longestDimension / 2;
    }

    public double LongestDimension { get { return Math.Max(Width, Math.Max(Height, Length)); } }

    // --------------------------------------------------------------------------------------------
    // MARK: Corner methods
    // --------------------------------------------------------------------------------------------

    // Get the corner of the box - considering the width, height and length

    public KoreXYZVector Corner(EnumCorner corner)
    {
        double halfWidth  = Width / 2;
        double halfHeight = Height / 2;
        double halfLength = Length / 2;

        switch (corner)
        {
            case EnumCorner.TopLeftFront:     return new KoreXYZVector(Center.X - halfWidth, Center.Y + halfHeight, Center.Z - halfLength);
            case EnumCorner.TopRightFront:    return new KoreXYZVector(Center.X + halfWidth, Center.Y + halfHeight, Center.Z - halfLength);
            case EnumCorner.BottomLeftFront:  return new KoreXYZVector(Center.X - halfWidth, Center.Y - halfHeight, Center.Z - halfLength);
            case EnumCorner.BottomRightFront: return new KoreXYZVector(Center.X + halfWidth, Center.Y - halfHeight, Center.Z - halfLength);
            case EnumCorner.TopLeftBack:      return new KoreXYZVector(Center.X - halfWidth, Center.Y + halfHeight, Center.Z + halfLength);
            case EnumCorner.TopRightBack:     return new KoreXYZVector(Center.X + halfWidth, Center.Y + halfHeight, Center.Z + halfLength);
            case EnumCorner.BottomLeftBack:   return new KoreXYZVector(Center.X - halfWidth, Center.Y - halfHeight, Center.Z + halfLength);
            case EnumCorner.BottomRightBack:  return new KoreXYZVector(Center.X + halfWidth, Center.Y - halfHeight, Center.Z + halfLength);
            default:
                return Center;
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Edge methods
    // --------------------------------------------------------------------------------------------

    // Get the edge of the box - considering the width, height and length.

    public KoreXYZLine Edge(EnumEdge edge)
    {
        switch (edge)
        {
            case EnumEdge.TopFront:    return new KoreXYZLine(Corner(EnumCorner.TopLeftFront), Corner(EnumCorner.TopRightFront));
            case EnumEdge.TopBack:     return new KoreXYZLine(Corner(EnumCorner.TopLeftBack), Corner(EnumCorner.TopRightBack));
            case EnumEdge.TopLeft:     return new KoreXYZLine(Corner(EnumCorner.TopLeftFront), Corner(EnumCorner.TopLeftBack));
            case EnumEdge.TopRight:    return new KoreXYZLine(Corner(EnumCorner.TopRightFront), Corner(EnumCorner.TopRightBack));
            case EnumEdge.BottomFront: return new KoreXYZLine(Corner(EnumCorner.BottomLeftFront), Corner(EnumCorner.BottomRightFront));
            case EnumEdge.BottomBack:  return new KoreXYZLine(Corner(EnumCorner.BottomLeftBack), Corner(EnumCorner.BottomRightBack));
            case EnumEdge.BottomLeft:  return new KoreXYZLine(Corner(EnumCorner.BottomLeftFront), Corner(EnumCorner.BottomLeftBack));
            case EnumEdge.BottomRight: return new KoreXYZLine(Corner(EnumCorner.BottomRightFront), Corner(EnumCorner.BottomRightBack));
            case EnumEdge.FrontLeft:   return new KoreXYZLine(Corner(EnumCorner.TopLeftFront), Corner(EnumCorner.BottomLeftFront));
            case EnumEdge.FrontRight:  return new KoreXYZLine(Corner(EnumCorner.TopRightFront), Corner(EnumCorner.BottomRightFront));
            case EnumEdge.BackLeft:    return new KoreXYZLine(Corner(EnumCorner.TopLeftBack), Corner(EnumCorner.BottomLeftBack));
            case EnumEdge.BackRight:   return new KoreXYZLine(Corner(EnumCorner.TopRightBack), Corner(EnumCorner.BottomRightBack));
            default:
                return new KoreXYZLine(Center, Center);
        }
    }

    // --------------------------------------------------------------------------------------------

    // Override the ToString method to return a string representation of the box.

    public override string ToString()
    {
        return $"Center:{Center}, Width:{Width:F2}, Height:{Height:F2}, Length:{Length:F2}";
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Inset
    // --------------------------------------------------------------------------------------------

    // Inset the box by a given amount - in each dimension.

    public KoreXYZBox Inset(double insetWidth, double insetHeight, double insetLength)
    {
        return new KoreXYZBox(Center, Width - insetWidth, Height - insetHeight, Length - insetLength);
    }

}