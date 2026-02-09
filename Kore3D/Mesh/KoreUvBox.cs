// <fileheader>


//using System;
//using System.Numerics;

namespace KoreCommon;

// UV corner enumeration for explicit corner access
public enum UVCorner
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

// KoreUVBox: A struct to hold the UV coordinates of a quadrilateral in a texture
// Supports arbitrary quadrilaterals (rectangles, diamonds, rhombuses, etc.)
// Uses bilinear interpolation for fractional UV coordinates within the quad

// DESIGN: Four explicit corner points define any quadrilateral UV region
// - Corner0: Typically "top-left" but can be any orientation
// - Corner1: Typically "top-right"
// - Corner2: Typically "bottom-right"
// - Corner3: Typically "bottom-left"
// - Fractional coordinates (u,v) use bilinear interpolation across the quad

public struct KoreUVBox
{
    // Four corners defining the UV quadrilateral
    public KoreXYVector Corner0; // Typically top-left
    public KoreXYVector Corner1; // Typically top-right
    public KoreXYVector Corner2; // Typically bottom-right
    public KoreXYVector Corner3; // Typically bottom-left

    // Constructor for explicit four-corner quadrilateral
    public KoreUVBox(KoreXYVector corner0, KoreXYVector corner1, KoreXYVector corner2, KoreXYVector corner3)
    {
        Corner0 = corner0;
        Corner1 = corner1;
        Corner2 = corner2;
        Corner3 = corner3;
    }

    // Constructor for axis-aligned rectangle (backward compatibility)
    public KoreUVBox(KoreXYVector topLeft, KoreXYVector bottomRight)
    {
        Corner0 = topLeft;                                              // Top-left
        Corner1 = new KoreXYVector(bottomRight.X, topLeft.Y);          // Top-right
        Corner2 = bottomRight;                                          // Bottom-right
        Corner3 = new KoreXYVector(topLeft.X, bottomRight.Y);          // Bottom-left
    }

    // Constructor for axis-aligned rectangle with explicit coordinates
    public KoreUVBox(double topLeftX, double topLeftY, double bottomRightX, double bottomRightY)
    {
        Corner0 = new KoreXYVector(topLeftX, topLeftY);                 // Top-left
        Corner1 = new KoreXYVector(bottomRightX, topLeftY);             // Top-right
        Corner2 = new KoreXYVector(bottomRightX, bottomRightY);         // Bottom-right
        Corner3 = new KoreXYVector(topLeftX, bottomRightY);             // Bottom-left
    }

    // Backward compatibility properties
    public KoreXYVector TopLeft => Corner0;
    public KoreXYVector TopRight => Corner1;
    public KoreXYVector BottomRight => Corner2;
    public KoreXYVector BottomLeft => Corner3;

    public static KoreUVBox Zero { get { return new KoreUVBox(KoreXYVector.Zero, KoreXYVector.Zero, KoreXYVector.Zero, KoreXYVector.Zero); } }
    public static KoreUVBox Full { get { return new KoreUVBox(KoreXYVector.Zero, KoreXYVector.One); } }

    // --------------------------------------------------------------------------------------------

    // Range properties (for backward compatibility with axis-aligned rectangles)
    public KoreNumericRange<double> UVRangeX
    {
        get
        {
            double minX = KoreValueUtils.Min3(KoreValueUtils.Min3(Corner0.X, Corner1.X, Corner2.X), Corner3.X, Corner0.X);
            double maxX = KoreValueUtils.Max3(KoreValueUtils.Max3(Corner0.X, Corner1.X, Corner2.X), Corner3.X, Corner0.X);
            return new KoreNumericRange<double>(minX, maxX);
        }
    }

    public KoreNumericRange<double> UVRangeY
    {
        get
    {
            double minY = KoreValueUtils.Min3(KoreValueUtils.Min3(Corner0.Y, Corner1.Y, Corner2.Y), Corner3.Y, Corner0.Y);
            double maxY = KoreValueUtils.Max3(KoreValueUtils.Max3(Corner0.Y, Corner1.Y, Corner2.Y), Corner3.Y, Corner0.Y);
            return new KoreNumericRange<double>(minY, maxY);
        }
    }

    // --------------------------------------------------------------------------------------------

    // Get UV coordinates within the quadrilateral using bilinear interpolation
    // uFraction and vFraction should be in the range [0, 1]
    // (0,0) = Corner0, (1,0) = Corner1, (1,1) = Corner2, (0,1) = Corner3

    public KoreXYVector GetUV(double uFraction, double vFraction)
    {
        // Clamp fractions to [0, 1] range
        uFraction = KoreValueUtils.ClampD(uFraction, 0.0, 1.0);
        vFraction = KoreValueUtils.ClampD(vFraction, 0.0, 1.0);

        // Bilinear interpolation across the quadrilateral
        // First interpolate along top edge (Corner0 to Corner1)
        double topX = KoreValueUtils.Interpolate(Corner0.X, Corner1.X, uFraction);
        double topY = KoreValueUtils.Interpolate(Corner0.Y, Corner1.Y, uFraction);
        KoreXYVector topEdge = new KoreXYVector(topX, topY);

        // Then interpolate along bottom edge (Corner3 to Corner2)
        double bottomX = KoreValueUtils.Interpolate(Corner3.X, Corner2.X, uFraction);
        double bottomY = KoreValueUtils.Interpolate(Corner3.Y, Corner2.Y, uFraction);
        KoreXYVector bottomEdge = new KoreXYVector(bottomX, bottomY);

        // Finally interpolate between the two edge points
        double finalX = KoreValueUtils.Interpolate(topEdge.X, bottomEdge.X, vFraction);
        double finalY = KoreValueUtils.Interpolate(topEdge.Y, bottomEdge.Y, vFraction);
        return new KoreXYVector(finalX, finalY);
    }

    // Get explicit corner coordinates that work correctly for rotated/flipped UV boxes
    // This handles cases where TopLeft/BottomRight might be "backwards"
    public KoreXYVector GetCornerUV(UVCorner corner)
    {
        return corner switch
        {
            UVCorner.TopLeft => TopLeft,
            UVCorner.TopRight => new KoreXYVector(BottomRight.X, TopLeft.Y),
            UVCorner.BottomLeft => new KoreXYVector(TopLeft.X, BottomRight.Y),
            UVCorner.BottomRight => BottomRight,
            _ => TopLeft
        };
    }

    // Get all four corners in a consistent order: TL, TR, BR, BL
    public (KoreXYVector topLeft, KoreXYVector topRight, KoreXYVector bottomRight, KoreXYVector bottomLeft) GetAllCorners()
    {
        return (
            topLeft: TopLeft,
            topRight: new KoreXYVector(BottomRight.X, TopLeft.Y),
            bottomRight: BottomRight,
            bottomLeft: new KoreXYVector(TopLeft.X, BottomRight.Y)
        );
    }

    // --------------------------------------------------------------------------------------------

    // Get a 2D grid of UV coordinates based on the dimensions of a destination point grid
    // Quick UV generation method.
    // Top

    // Usage: KoreXYVector[,] uvGrid = uvBox.GetUVGrid(10, 10);

    public KoreXYVector[,] GetUVGrid(int horizSize, int vertSize)
    {
        var uvGrid = new KoreXYVector[vertSize, horizSize];

        // Get 2 1D arrays to define the values in the range
        // Note: ListForRange should handle reversed ranges correctly (e.g., from 1.0 to 0.0)
        KoreNumeric1DArray<double> uRange = KoreNumeric1DArrayOps<double>.ListForRange(TopLeft.X, BottomRight.X, horizSize);
        KoreNumeric1DArray<double> vRange = KoreNumeric1DArrayOps<double>.ListForRange(TopLeft.Y, BottomRight.Y, vertSize);

        for (int y = 0; y < vertSize; y++)
        {
            for (int x = 0; x < horizSize; x++)
            {
                uvGrid[x, y] = new KoreXYVector(uRange[x], vRange[y]);
            }
        }

        return uvGrid;
    }

    // --------------------------------------------------------------------------------------------

    // create a UV box from a grid
    // (0,0) is top left box, (size-1, size-1) is bottom right box.

    // Creates a UV box for a subtile within the main tile
    public static KoreUVBox BoxFromGrid(KoreXYVector topLeft, KoreXYVector bottomRight, int horizSize, int vertSize, int horizIndex, int vertIndex)
    {
        double horizStep = 1.0f / horizSize;
        double vertStep  = 1.0f / vertSize;

        double leftValue  = topLeft.X + horizIndex * horizStep * (bottomRight.X - topLeft.X);
        double rightValue = topLeft.X + (horizIndex + 1) * horizStep * (bottomRight.X - topLeft.X);
        double topValue   = topLeft.Y + vertIndex * vertStep * (bottomRight.Y - topLeft.Y);
        double botValue   = topLeft.Y + (vertIndex + 1) * vertStep * (bottomRight.Y - topLeft.Y);

        return new KoreUVBox(new KoreXYVector(leftValue, topValue), new KoreXYVector(rightValue, botValue));
    }

    public KoreUVBox BoxFromGrid(KoreNumeric2DPosition<int> innerBoxPos)
    {
        // Calculate the horizontal and vertical step sizes
        double horizStep = (BottomRight.X - TopLeft.X) / innerBoxPos.ExtentX;
        double vertStep  = (BottomRight.Y - TopLeft.Y) / innerBoxPos.ExtentY;

        // Calculate the UV coordinates for the top-left corner of the inner box
        double leftValue   = TopLeft.X + innerBoxPos.PosX * horizStep;
        double rightValue  = TopLeft.X + (innerBoxPos.PosX + 1) * horizStep;
        double topValue    = TopLeft.Y + innerBoxPos.PosY * vertStep;
        double bottomValue = TopLeft.Y + (innerBoxPos.PosY + 1) * vertStep;

        // Return a new KoreUVBox using the calculated values
        return new KoreUVBox(new KoreXYVector(leftValue, topValue), new KoreXYVector(rightValue, bottomValue));
    }

}



