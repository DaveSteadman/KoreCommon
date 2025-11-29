// <fileheader>

using System;

namespace KoreCommon;

public static class KoreXYLineOps
{
    // --------------------------------------------------------------------------------------------
    // #MARK: Distance Values
    // --------------------------------------------------------------------------------------------

    // Get the distance from a point to a line

    public static double ClosestDistanceTo(this KoreXYLine line, KoreXYVector xy)
    {
        double x1 = line.P1.X, y1 = line.P1.Y;
        double x2 = line.P2.X, y2 = line.P2.Y;
        double x3 = xy.X, y3 = xy.Y;

        double dx = x2 - x1;
        double dy = y2 - y1;
        double lengthSq = dx * dx + dy * dy;

        double u = ((x3 - x1) * dx + (y3 - y1) * dy) / lengthSq;
        double x = x1 + u * dx;
        double y = y1 + u * dy;

        double dxToPoint = x - x3;
        double dyToPoint = y - y3;
        return Math.Sqrt(dxToPoint * dxToPoint + dyToPoint * dyToPoint);
    }

    // --------------------------------------------------------------------------------------------
    // #MARK: Line Checks
    // --------------------------------------------------------------------------------------------

    // Its double precision maths, so a point will never *exactly* be on a line, just check that the distance is within a tolerance

    public static bool IsPointOnLine(this KoreXYLine line, KoreXYVector xy, bool limitToLineSegment = true, double tolerance = 1e-6)
    {
        double x1 = line.P1.X;
        double y1 = line.P1.Y;
        double x2 = line.P2.X;
        double y2 = line.P2.Y;
        double x3 = xy.X;
        double y3 = xy.Y;

        // Calculate the projection of the point onto the line (x, y)
        double px = x2 - x1;
        double py = y2 - y1;
        double dAB = px * px + py * py;
        double u = ((x3 - x1) * px + (y3 - y1) * py) / dAB;
        double x = x1 + u * px;
        double y = y1 + u * py;

        // Check if the distance from the projected point to the actual point is within tolerance
        bool isWithinDistance = Math.Sqrt(Math.Pow(x - x3, 2) + Math.Pow(y - y3, 2)) <= tolerance;

        // If limiting to the line segment, check if the projected point is within the segment bounds
        if (limitToLineSegment)
        {
            bool isWithinSegment = u >= 0 && u <= 1;
            return isWithinSegment && isWithinDistance;
        }

        return isWithinDistance;
    }
    // --------------------------------------------------------------------------------------------

    // Find the intersection point between two lines, which could be null.

    // Returns true if the two line segments (l1, l2) properly cross or overlap,
    // excluding the trivial case of exactly coincident endpoints.
    // Uses a small tolerance to guard against floating-point issues.
    public static bool DoesIntersect(KoreXYLine l1, KoreXYLine l2, double tol = 1e-9)
    {
        // 1) Unpack coordinates
        double x1 = l1.P1.X, y1 = l1.P1.Y;
        double x2 = l1.P2.X, y2 = l1.P2.Y;
        double x3 = l2.P1.X, y3 = l2.P1.Y;
        double x4 = l2.P2.X, y4 = l2.P2.Y;

        // 2) Build direction vectors for each segment
        double dx1 = x2 - x1, dy1 = y2 - y1;   // l1 direction
        double dx2 = x4 - x3, dy2 = y4 - y3;   // l2 direction

        // 3) Compute the “denominator” of the intersection formulas:
        //    denom = dx1 * dy2 - dy1 * dx2
        //    If denom == 0, the lines are parallel or colinear.
        double denom = dx1 * dy2 - dy1 * dx2;

        if (Math.Abs(denom) < tol)
        {
            // 3a) Parallel or nearly so.  Check for colinearity by seeing if
            //     (l2.P1 - l1.P1) is also perpendicular to the normal of l1:
            double cross = (x3 - x1) * dy1 - (y3 - y1) * dx1;
            if (Math.Abs(cross) > tol)
            {
                // Truly parallel, no intersection
                return false;
            }

            // 3b) Colinear: now check 1-D bounding-box overlap on X and Y.
            double l1MinX = Math.Min(x1, x2), l1MaxX = Math.Max(x1, x2);
            double l2MinX = Math.Min(x3, x4), l2MaxX = Math.Max(x3, x4);
            double l1MinY = Math.Min(y1, y2), l1MaxY = Math.Max(y1, y2);
            double l2MinY = Math.Min(y3, y4), l2MaxY = Math.Max(y3, y4);

            bool xOverlap = l1MaxX + tol >= l2MinX && l2MaxX + tol >= l1MinX;
            bool yOverlap = l1MaxY + tol >= l2MinY && l2MaxY + tol >= l1MinY;

            // They overlap if both projections overlap
            return xOverlap && yOverlap;
        }

        // 4) Lines are not parallel: solve for parameters t, u such that
        //      L1(t) = P1 + t*(dx1,dy1)
        //      L2(u) = P3 + u*(dx2,dy2)
        //
        //    Intersection ⇒ P1 + t·(dx1,dy1) = P3 + u·(dx2,dy2)
        //    Solve the 2×2 system for t and u:
        double t = ((x3 - x1) * dy2 - (y3 - y1) * dx2) / denom;
        double u = ((x3 - x1) * dy1 - (y3 - y1) * dx1) / denom;

        // 5) The segments intersect (not just their infinite lines) if both
        //    parameters lie strictly between 0 and 1.  Tweak the comparison
        //    if you want to include/exclude endpoint-touching.
        return (t > tol && t < 1 - tol) && (u > tol && u < 1 - tol);
    }

    // --------------------------------------------------------------------------------------------

    // determine if two lines are parallel

    public static bool IsParallel(KoreXYLine line1, KoreXYLine line2)
    {
        double x1 = line1.P1.X;
        double y1 = line1.P1.Y;
        double x2 = line1.P2.X;
        double y2 = line1.P2.Y;
        double x3 = line2.P1.X;
        double y3 = line2.P1.Y;
        double x4 = line2.P2.X;
        double y4 = line2.P2.Y;

        // Determine the difference in the gradients of the lines
        double d = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

        // Check if the lines are parallel, within the tolerance of the system.
        return KoreValueUtils.EqualsWithinTolerance(d, 0);
    }

    // --------------------------------------------------------------------------------------------
    // #MARK: Line Adjustments
    // --------------------------------------------------------------------------------------------
    // Return a new line object, with adjustments to the end points. +ve extends the line away from the center, -ve reduces it.

    public static KoreXYLine ExtendLine(KoreXYLine line, double p1Dist, double p2Dist)
    {
        // Handle the case where the line has no length
        if (line.Length == 0) return line;

        KoreXYVector p1 = line.P1;
        KoreXYVector p2 = line.P2;

        // Get the delta and normalise it so we apply the delta in the direction of the line
        double dx = (p2.X - p1.X) / line.Length;
        double dy = (p2.Y - p1.Y) / line.Length;
        double lineLength = line.Length;

        double p1x = p1.X - (dx * p1Dist);
        double p1y = p1.Y - (dy * p1Dist);
        double p2x = p2.X + (dx * p2Dist);
        double p2y = p2.Y + (dy * p2Dist);

        return new KoreXYLine(new KoreXYVector(p1x, p1y), new KoreXYVector(p2x, p2y));
    }

    // --------------------------------------------------------------------------------------------

    // Extrapolate the line by a distance. -ve is back from P1, +ve is forward from P2
    // Usage: KoreXYVector extrapolatedPoint = KoreXYLineOps.ExtrapolateDistance(line, distance);
    public static KoreXYVector ExtrapolateDistance(KoreXYLine line, double distance)
    {
        double dx = line.P2.X - line.P1.X;
        double dy = line.P2.Y - line.P1.Y;
        double len = line.Length;

        if (distance >= 0)
        {
            // Extend from P2
            double newX = line.P2.X + (dx * distance / len);
            double newY = line.P2.Y + (dy * distance / len);
            return new KoreXYVector(newX, newY);
        }
        else
        {
            // Backtrack from P1
            double newX = line.P1.X + (dx * distance / len);
            double newY = line.P1.Y + (dy * distance / len);
            return new KoreXYVector(newX, newY);
        }
    }

    // --------------------------------------------------------------------------------------------
    // Usage: KoreXYLineOps.InsetLine(line, insetDist);
    public static KoreXYLine InsetLine(KoreXYLine line, double insetDist)
    {
        // Handle the case where the line has no length
        if (line.Length == 0) return line;

        KoreXYVector p1 = line.P1;
        KoreXYVector p2 = line.P2;

        // Get the delta and normalise it so we apply the delta in the direction of the line
        double dx = (p2.X - p1.X) / line.Length;
        double dy = (p2.Y - p1.Y) / line.Length;

        double p1x = p1.X + (dx * insetDist);
        double p1y = p1.Y + (dy * insetDist);
        double p2x = p2.X - (dx * insetDist);
        double p2y = p2.Y - (dy * insetDist);

        return new KoreXYLine(new KoreXYVector(p1x, p1y), new KoreXYVector(p2x, p2y));
    }

    // --------------------------------------------------------------------------------------------

    public static KoreXYLine? ClipLineToRect(KoreXYLine line, KoreXYRect rect)
    {
        double x0 = line.P1.X;
        double y0 = line.P1.Y;
        double x1 = line.P2.X;
        double y1 = line.P2.Y;

        double xmin = rect.Left;
        double xmax = rect.Right;
        double ymin = rect.Top;
        double ymax = rect.Bottom;

        double t0 = 0.0;
        double t1 = 1.0;
        double dx = x1 - x0;
        double dy = y1 - y0;

        double[] p = { -dx, dx, -dy, dy };
        double[] q = { x0 - xmin, xmax - x0, y0 - ymin, ymax - y0 };

        for (int i = 0; i < 4; i++)
        {
            if (p[i] == 0)
            {
                if (q[i] < 0)
                {
                    return null; // Parallel line outside the rect
                }
            }
            else
            {
                double t = q[i] / p[i];
                if (p[i] < 0)
                {
                    if (t > t1)
                    {
                        return null; // Line is completely outside
                    }
                    else if (t > t0)
                    {
                        t0 = t; // Line is partially inside
                    }
                }
                else
                {
                    if (t < t0)
                    {
                        return null; // Line is completely outside
                    }
                    else if (t < t1)
                    {
                        t1 = t; // Line is partially inside
                    }
                }
            }
        }

        double clippedX0 = x0 + t0 * dx;
        double clippedY0 = y0 + t0 * dy;
        double clippedX1 = x0 + t1 * dx;
        double clippedY1 = y0 + t1 * dy;

        return new KoreXYLine(new KoreXYVector(clippedX0, clippedY0), new KoreXYVector(clippedX1, clippedY1));
    }

    // OffsetInward() shifts a line segment perpendicular to its direction by a given distance,
    // with the offset direction determined relative to a third point (typically the vertex opposite
    // the edge in a triangle). The function ensures that the offset moves the edge "inward" into
    // the shape, not outward. This is especially useful for triangle insetting, where each edge
    // must be moved inward toward the triangle's centroid while preserving its orientation.

    public static KoreXYLine OffsetInward(KoreXYLine line, KoreXYVector oppositePoint, double inset)
    {
        // Compute the direction vector of the line
        double dx = line.P2.X - line.P1.X;
        double dy = line.P2.Y - line.P1.Y;
        double len = Math.Sqrt(dx * dx + dy * dy);

        // Don't offset degenerate (zero-length) lines
        if (len < 1e-12) return line;

        // Normal vector: rotate the direction vector 90 degrees (clockwise)
        double nx = -dy / len;
        double ny = dx / len;

        // Determine if the normal points toward the triangle's interior
        var mid = line.MidPoint();
        var toC = new KoreXYVector(oppositePoint.X - mid.X, oppositePoint.Y - mid.Y);
        double dot = nx * toC.X + ny * toC.Y;

        // If normal points away from the triangle, flip it
        if (dot < 0)
        {
            nx = -nx;
            ny = -ny;
        }

        // Apply offset in the inward normal direction
        var offset = new KoreXYVector(nx * inset, ny * inset);
        return line.Offset(offset);
    }

    // Look for an intersection point between two lines.
    public static bool TryIntersect(KoreXYLine l1, KoreXYLine l2, out KoreXYVector result)
    {
        double d = (l1.P1.X - l1.P2.X) * (l2.P1.Y - l2.P2.Y) - (l1.P1.Y - l1.P2.Y) * (l2.P1.X - l2.P2.X);
        if (Math.Abs(d) < 1e-12)
        {
            result = KoreXYVector.Zero;
            return false;
        }

        double det1 = l1.P1.X * l1.P2.Y - l1.P1.Y * l1.P2.X;
        double det2 = l2.P1.X * l2.P2.Y - l2.P1.Y * l2.P2.X;

        double px = (det1 * (l2.P1.X - l2.P2.X) - (l1.P1.X - l1.P2.X) * det2) / d;
        double py = (det1 * (l2.P1.Y - l2.P2.Y) - (l1.P1.Y - l1.P2.Y) * det2) / d;

        result = new KoreXYVector(px, py);
        return true;
    }

}