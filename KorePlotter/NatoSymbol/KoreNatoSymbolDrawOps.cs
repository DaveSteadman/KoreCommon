using System.Collections;
using SkiaSharp;

using KoreCommon;

namespace KorePlotter.NatoSymbol;

// KoreNatoSymbolDrawOps: Static methods to draw specific shapes for NATO symbols.
// - Functions passed the canvas and the necessary parameters to draw the shape.
// - Static class for utility functions, holds no state.

public static partial class KoreNatoSymbolDrawOps
{
    // ----------------------------------------------------------------------------------------
    // MARK: UNKNOWN
    // ----------------------------------------------------------------------------------------

    // Usage: KoreNatoSymbolDrawOps.DrawUnknown(canvas, domain);
    public static void DrawUnknown(KoreNatoSymbolCanvas canvas, NatoSymbolDomain domain)
    {
        float halfRootTwo = (float)(Math.Sqrt(2) / 2.0);
        float s = (float)(1.0 / Math.Sqrt(2)); // sin(45°) = cos(45°) = √2/2

        // Inner square midpoints (where lobes connect)
        SKPoint topMiddle       = canvas.LPoint( 0f,   -0.5f);
        SKPoint rightMiddle     = canvas.LPoint( 0.5f,  0f);
        SKPoint bottomMiddle    = canvas.LPoint( 0f,    0.5f);
        SKPoint leftMiddle      = canvas.LPoint(-0.5f,  0f);

        // Define the inner square’s corners.
        SKPoint topLeft     = canvas.LPoint(-s * 0.5f, -s * 0.5f);
        SKPoint topRight    = canvas.LPoint( s * 0.5f, -s * 0.5f);
        SKPoint bottomRight = canvas.LPoint( s * 0.5f,  s * 0.5f);
        SKPoint bottomLeft  = canvas.LPoint(-s * 0.5f,  s * 0.5f);

        // Outer bulge points (where arcs extend to √2/2 to match diamond bounds)
        SKPoint topBulge        = canvas.LPoint( 0f,          -halfRootTwo);
        SKPoint rightBulge      = canvas.LPoint( halfRootTwo,  0f);
        SKPoint bottomBulge     = canvas.LPoint( 0f,           halfRootTwo);
        SKPoint leftBulge       = canvas.LPoint(-halfRootTwo,  0f);

        // Define the path based on the domain
        SKPath path = new SKPath();

        switch (domain)
        {
            case NatoSymbolDomain.Air:
            case NatoSymbolDomain.Space:
                path.MoveTo(bottomLeft);
                AddArcThroughPoints(path, bottomLeft, topLeft, leftBulge);
                AddArcThroughPoints(path, topLeft, topRight, topBulge);
                AddArcThroughPoints(path, topRight, bottomRight, rightBulge);
                path.Close();
                break;

            case NatoSymbolDomain.Land:
            case NatoSymbolDomain.SeaSurface:
            case NatoSymbolDomain.Equipment:
            case NatoSymbolDomain.Installation:
            case NatoSymbolDomain.Activity:
                path.MoveTo(topLeft);
                AddArcThroughPoints(path, topLeft, topRight, topBulge);
                AddArcThroughPoints(path, topRight, bottomRight, rightBulge);
                AddArcThroughPoints(path, bottomRight, bottomLeft, bottomBulge);
                AddArcThroughPoints(path, bottomLeft, topLeft, leftBulge);
                path.Close();
                break;

            case NatoSymbolDomain.SeaSubsurface:
                path.MoveTo(topRight);
                AddArcThroughPoints(path, topRight, bottomRight, rightBulge);
                AddArcThroughPoints(path, bottomRight, bottomLeft, bottomBulge);
                AddArcThroughPoints(path, bottomLeft, topLeft, leftBulge);
                path.Close();
                break;


            // Other domains can be implemented similarly
            default:
                break;
        }

        // Draw the filled shape
        using SKPaint fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = KoreNatoSymbolColorPalette.BackgroundUnknown,
            IsAntialias = true
        };
        canvas.Canvas.DrawPath(path, fillPaint);

        // Draw the border
        using SKPaint paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = KoreNatoSymbolColorPalette.BorderColor,
            StrokeWidth = StrokeWidthForCanvas(canvas),
            IsAntialias = true
        };
        canvas.Canvas.DrawPath(path, paint);
    }

    // ----------------------------------------------------------------------------------------
    // MARK: NEUTRAL
    // ----------------------------------------------------------------------------------------

    // Usage: KoreNatoSymbolDrawOps.DrawNeutral(canvas, domain);
    public static void DrawNeutral(KoreNatoSymbolCanvas canvas, NatoSymbolDomain domain)
    {
        float halfRootTwo = (float)(Math.Sqrt(2) / 2.0);
        float s = (float)(1.0 / Math.Sqrt(2)); // sin(45°) = cos(45°) = √2/2

        // define the square points in L coordinates
        SKPoint topLeft         = canvas.LPoint(-0.5f, -0.5f);
        SKPoint topRight        = canvas.LPoint( 0.5f, -0.5f);
        SKPoint bottomLeft      = canvas.LPoint(-0.5f,  0.5f);
        SKPoint bottomRight     = canvas.LPoint( 0.5f,  0.5f);

        // Fill is always a square
        SKPath fillpath = new SKPath();
        fillpath.MoveTo(topLeft);
        fillpath.LineTo(topRight);
        fillpath.LineTo(bottomRight);
        fillpath.LineTo(bottomLeft);
        fillpath.LineTo(topLeft);
        fillpath.Close();

        SKPath strokepath = new SKPath();

        switch (domain)
        {
            case NatoSymbolDomain.Air:
            case NatoSymbolDomain.Space:
                strokepath.MoveTo(bottomLeft);
                strokepath.LineTo(topLeft);
                strokepath.LineTo(topRight);
                strokepath.LineTo(bottomRight);
                break;

            case NatoSymbolDomain.Land:
            case NatoSymbolDomain.SeaSurface:
            case NatoSymbolDomain.Equipment:
            case NatoSymbolDomain.Installation:
            case NatoSymbolDomain.Activity:
                strokepath.MoveTo(topLeft);
                strokepath.LineTo(topRight);
                strokepath.LineTo(bottomRight);
                strokepath.LineTo(bottomLeft);
                strokepath.LineTo(topLeft);
                break;

            case NatoSymbolDomain.SeaSubsurface:
                strokepath.MoveTo(topRight);
                strokepath.LineTo(bottomRight);
                strokepath.LineTo(bottomLeft);
                strokepath.LineTo(topLeft);
                break;
        }

        // Draw the filled shape
        using SKPaint fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = KoreNatoSymbolColorPalette.BackgroundNeutral,
            IsAntialias = true
        };
        canvas.Canvas.DrawPath(fillpath, fillPaint);

        // Draw the border
        using SKPaint paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = KoreNatoSymbolColorPalette.BorderColor,
            StrokeWidth = StrokeWidthForCanvas(canvas),
            IsAntialias = true
        };
        canvas.Canvas.DrawPath(strokepath, paint);

    }

    // ----------------------------------------------------------------------------------------
    // MARK: FRIEND
    // ----------------------------------------------------------------------------------------

    // Usage: KoreNatoSymbolDrawOps.DrawFriend(canvas, domain);
    public static void DrawFriend(KoreNatoSymbolCanvas canvas, NatoSymbolDomain domain)
    {
        // define the square points in L coordinates
        SKPoint topLeft         = canvas.LPoint(-0.55f, -0.5f);
        SKPoint topRight        = canvas.LPoint( 0.55f, -0.5f);
        SKPoint bottomLeft      = canvas.LPoint(-0.55f,  0.5f);
        SKPoint bottomRight     = canvas.LPoint( 0.55f,  0.5f);

        // Peak Values
        SKPoint peakTop         = canvas.LPoint( 0f,   -1.0f);
        SKPoint peakBottom      = canvas.LPoint( 0f,    1.0f);

        float fudgeX = 0.5f;
        float fudgeY = 0.05f;

        SKPaint fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = KoreNatoSymbolColorPalette.BackgroundFriendly,
            IsAntialias = true
        };
        SKPaint paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = KoreNatoSymbolColorPalette.BorderColor,
            StrokeWidth = StrokeWidthForCanvas(canvas),
            IsAntialias = true
        };



        switch (domain)
        {
            case NatoSymbolDomain.Air:
            case NatoSymbolDomain.Space:

                KoreNumeric1DArray<float> xPoints = new KoreNumeric1DArray<float>(new float[]
                {
                    bottomLeft.X,
                    canvas.LPoint(-fudgeX, 0).X,
                    canvas.LPoint(0.0f, 0).X,
                    canvas.LPoint(fudgeX, 0).X,
                    bottomRight.X
                });
                KoreNumeric1DArray<float> yPoints = new KoreNumeric1DArray<float>(new float[]
                {
                    bottomLeft.Y,
                    peakTop.Y + fudgeY,
                    peakTop.Y,
                    peakTop.Y + fudgeY,
                    bottomRight.Y
                });

                SKPath path = new SKPath();

                int numpoints = 50;
                float tInc = 1.0f / (numpoints - 1);
                for (int i = 0; i < numpoints; i++)
                {
                    float t = i * tInc;
                    float x = KoreNumeric1DArrayOps<float>.CalculateBezierPoint(t, xPoints);
                    float y = KoreNumeric1DArrayOps<float>.CalculateBezierPoint(t, yPoints);
                    SKPoint pt = new SKPoint(x, y);

                    if (i == 0)
                        path.MoveTo(pt);
                    else
                        path.LineTo(pt);
                }

                // Draw the filled shape
                canvas.Canvas.DrawPath(path, fillPaint);
                canvas.Canvas.DrawPath(path, paint);

                break;

            case NatoSymbolDomain.SeaSubsurface:

                KoreNumeric1DArray<float> xPoints2 = new KoreNumeric1DArray<float>(new float[]
                {
                    topLeft.X,
                    canvas.LPoint(-fudgeX, 0).X,
                    canvas.LPoint(0.0f, 0).X,
                    canvas.LPoint(fudgeX, 0).X,
                    topRight.X
                });
                KoreNumeric1DArray<float> yPoints2 = new KoreNumeric1DArray<float>(new float[]
                {
                    topLeft.Y,
                    peakBottom.Y - fudgeY,
                    peakBottom.Y,
                    peakBottom.Y - fudgeY,
                    topRight.Y
                });

                SKPath path2 = new SKPath();

                int numpoints2 = 50;
                float tInc2 = 1.0f / (numpoints2 - 1);
                for (int i = 0; i < numpoints2; i++)
                {
                    float t = i * tInc2;
                    float x = KoreNumeric1DArrayOps<float>.CalculateBezierPoint(t, xPoints2);
                    float y = KoreNumeric1DArrayOps<float>.CalculateBezierPoint(t, yPoints2);
                    SKPoint pt = new SKPoint(x, y);

                    if (i == 0)
                        path2.MoveTo(pt);
                    else
                        path2.LineTo(pt);
                }

                // Draw the filled shape
                canvas.Canvas.DrawPath(path2, fillPaint);
                canvas.Canvas.DrawPath(path2, paint);

                break;

            case NatoSymbolDomain.SeaSurface:
            case NatoSymbolDomain.Equipment:

                // draw a cirlce around the octagon
                SKPoint center = canvas.LPoint(0f, 0f);
                float radius = canvas.LDistance * 0.5f;
                canvas.Canvas.DrawCircle(center, radius, fillPaint);
                canvas.Canvas.DrawCircle(center, radius, paint);

                break;

            case NatoSymbolDomain.Land:
            case NatoSymbolDomain.Installation:
            case NatoSymbolDomain.Activity:

                // Setup Wide Rect
                SKPoint wideTopLeft         = canvas.LPoint(-0.75f, -0.5f);
                SKPoint wideTopRight        = canvas.LPoint( 0.75f, -0.5f);
                SKPoint wideBottomLeft      = canvas.LPoint(-0.75f,  0.5f);
                SKPoint wideBottomRight     = canvas.LPoint( 0.75f,  0.5f);

                SKRect wideRect = new SKRect(wideTopLeft.X, wideTopLeft.Y, wideBottomRight.X, wideBottomRight.Y);

                canvas.Canvas.DrawRect(wideRect, fillPaint);
                canvas.Canvas.DrawRect(wideRect, paint);

                break;



        }





    }

    // ----------------------------------------------------------------------------------------
    // MARK: HOSTILE
    // ----------------------------------------------------------------------------------------

    // Usage: KoreNatoSymbolDrawOps.DrawHostile(canvas, domain);
    public static void DrawHostile(KoreNatoSymbolCanvas canvas, NatoSymbolDomain domain)
    {
        float halfRootTwo = (float)(Math.Sqrt(2) / 2.0);
        float notchOffset = halfRootTwo - 0.5f;

        // define the diamond points in L coordinates
        SKPoint topMiddle       = canvas.LPoint(0f,          -halfRootTwo);
        SKPoint rightMiddle     = canvas.LPoint(halfRootTwo,  0f);
        SKPoint bottomMiddle    = canvas.LPoint(0f,           halfRootTwo);
        SKPoint leftMiddle      = canvas.LPoint(-halfRootTwo, 0f);

        // define the square points in L coordinates
        SKPoint topLeft         = canvas.LPoint(-0.5f, -0.5f);
        SKPoint topRight        = canvas.LPoint( 0.5f, -0.5f);
        SKPoint bottomLeft      = canvas.LPoint(-0.5f,  0.5f);
        SKPoint bottomRight     = canvas.LPoint( 0.5f,  0.5f);

        // Notch points
        SKPoint notchUpperLeft  = canvas.LPoint(-0.5f, -notchOffset);
        SKPoint notchUpperRight = canvas.LPoint( 0.5f, -notchOffset);
        SKPoint notchLowerRight = canvas.LPoint( 0.5f,  notchOffset);
        SKPoint notchLowerLeft  = canvas.LPoint(-0.5f,  notchOffset);

        // Define the path based on the domain
        SKPath path = new SKPath();

        switch (domain)
        {
            case NatoSymbolDomain.Land:
            case NatoSymbolDomain.SeaSurface:
            case NatoSymbolDomain.Equipment:
            case NatoSymbolDomain.Installation:
            case NatoSymbolDomain.Activity:
                // Draw diamond with notches for Land domain
                path.MoveTo(topMiddle);
                path.LineTo(rightMiddle);
                path.LineTo(bottomMiddle);
                path.LineTo(leftMiddle);
                path.LineTo(topMiddle);
                path.Close();
                break;

            case NatoSymbolDomain.Air:
            case NatoSymbolDomain.Space:
                path.MoveTo(bottomLeft);
                path.LineTo(notchUpperLeft);
                path.LineTo(topMiddle);
                path.LineTo(notchUpperRight);
                path.LineTo(bottomRight);
                path.Close();
                break;

            case NatoSymbolDomain.SeaSubsurface:
                path.MoveTo(topLeft);
                path.LineTo(notchLowerLeft);
                path.LineTo(bottomMiddle);
                path.LineTo(notchLowerRight);
                path.LineTo(topRight);
                break;

            // Other domains can be implemented similarly
            default:
                break;
        }

        // Draw the filled shape
        using SKPaint fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = KoreNatoSymbolColorPalette.BackgroundHostile,
            IsAntialias = true
        };
        canvas.Canvas.DrawPath(path, fillPaint);

        // Draw the border
        using SKPaint paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = KoreNatoSymbolColorPalette.BorderColor,
            StrokeWidth = StrokeWidthForCanvas(canvas),
            IsAntialias = true
        };
        canvas.Canvas.DrawPath(path, paint);

    }

    // ----------------------------------------------------------------------------------------
    // MARK: HELPER METHODS
    // ----------------------------------------------------------------------------------------

    // Helper: adds a circular arc from point A to B that passes through point C.
    // In degenerate (nearly collinear) cases, simply draws a line.
    private static void AddArcThroughPoints(SKPath path, SKPoint A, SKPoint B, SKPoint C)
    {
        float x1 = A.X, y1 = A.Y;
        float x2 = B.X, y2 = B.Y;
        float x3 = C.X, y3 = C.Y;

        // Calculate circle center via determinant.
        float d = 2 * (x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2));
        if (Math.Abs(d) < 1e-6)
        {
            // Points nearly collinear; just draw a line.
            path.LineTo(B);
            return;
        }

        float x1Sq = x1 * x1 + y1 * y1;
        float x2Sq = x2 * x2 + y2 * y2;
        float x3Sq = x3 * x3 + y3 * y3;
        float centerX = (x1Sq * (y2 - y3) + x2Sq * (y3 - y1) + x3Sq * (y1 - y2)) / d;
        float centerY = (x1Sq * (x3 - x2) + x2Sq * (x1 - x3) + x3Sq * (x2 - x1)) / d;
        SKPoint center = new SKPoint(centerX, centerY);

        float radius = (float)Math.Sqrt((A.X - centerX) * (A.X - centerX) + (A.Y - centerY) * (A.Y - centerY));
        SKRect oval = new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius);

        // Compute start, mid, and end angles (in degrees) relative to the circle's center.
        double startAngle = Math.Atan2(A.Y - center.Y, A.X - center.X) * 180.0 / Math.PI;
        double midAngle   = Math.Atan2(C.Y - center.Y, C.X - center.X) * 180.0 / Math.PI;
        double endAngle   = Math.Atan2(B.Y - center.Y, B.X - center.X) * 180.0 / Math.PI;

        double Normalize(double angle)
        {
            double a = angle % 360;
            if (a < 0)
                a += 360;
            return a;
        }

        startAngle = Normalize(startAngle);
        midAngle   = Normalize(midAngle);
        endAngle   = Normalize(endAngle);

        // Compute sweep angle so that the arc passes through midAngle.
        double sweepCW = endAngle - startAngle;
        if (sweepCW < 0)
            sweepCW += 360;
        double sweepCCW = sweepCW - 360;
        double diff = midAngle - startAngle;
        if (diff < 0)
            diff += 360;
        double sweepAngle = (diff <= sweepCW) ? sweepCW : sweepCCW;

        // Append the arc. Using ArcTo (with forceMoveTo false) joins seamlessly.
        path.ArcTo(oval, (float)startAngle, (float)sweepAngle, false);
    }


}