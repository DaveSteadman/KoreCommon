// <fileheader>

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace KoreCommon;

// File of static functions to create KoreMeshData primitives around Bezier curves
// - See KoreNumeric1DArrayOps for Bezier calculations

public static partial class KoreMeshDataPrimitives
{
    // Bezier3Line: Creates a line (not triangles) from three XYZ vectors, a color, and a number of points/divisions.
    // --------------------------------------------------------------------------------------------
    // MARK: Bezier3Line
    // ---------------------------------------------------------------------------------------------

    public static KoreMeshData Bezier3Line(
        KoreXYZVector p1, KoreXYZVector p2, KoreXYZVector p3, KoreColorRGB? lineColor = null, int divisions = 10)
    {
        KoreNumeric1DArray<double> xValues = new KoreNumeric1DArray<double>(3);
        KoreNumeric1DArray<double> yValues = new KoreNumeric1DArray<double>(3);
        KoreNumeric1DArray<double> zValues = new KoreNumeric1DArray<double>(3);

        xValues[0] = p1.X; xValues[1] = p2.X; xValues[2] = p3.X;
        yValues[0] = p1.Y; yValues[1] = p2.Y; yValues[2] = p3.Y;
        zValues[0] = p1.Z; zValues[1] = p2.Z; zValues[2] = p3.Z;

        KoreNumeric1DArray<double> fractionList = KoreNumeric1DArrayOps<double>.ListForRange(0, 1, divisions);

        // debug print the fractions list
        // string fractionsStr = string.Join(", ", fractionList);
        // KoreCentralLog.AddEntry($"Fraction List: {fractionsStr}");


        List<int> pointIds = new List<int>();

        KoreMeshData newMesh = new KoreMeshData();

        foreach (double currVal in fractionList)
        {
            double currX = KoreNumeric1DArrayOps<double>.CalculateBezierPoint(currVal, xValues);
            double currY = KoreNumeric1DArrayOps<double>.CalculateBezierPoint(currVal, yValues);
            double currZ = KoreNumeric1DArrayOps<double>.CalculateBezierPoint(currVal, zValues);

            KoreXYZVector newPoint = new KoreXYZVector(currX, currY, currZ);

            int currPointId = newMesh.AddCompleteVertex(newPoint, null, null, null);
            pointIds.Add(currPointId);
        }


        // Loop through all the points, adding them and making lines.
        for (int i = 0; i < pointIds.Count - 1; i++)
        {
            int startId = pointIds[i];
            int endId = pointIds[i + 1];
            newMesh.AddLine(startId, endId, lineColor);
        }

        KoreColorRGB debugCol = KoreColorPalette.Colors["Red"];
        KoreColorRGB debugCol2 = KoreColorPalette.Colors["White"];

        // Add dotted lines to debug the control points
        newMesh.AddDottedLineByDistance(p1, p2, debugCol, 0.1);
        newMesh.AddDottedLineByDistance(p2, p3, debugCol, 0.1);

        // add small spheres at the control points
        KoreMeshData sphereMeshP1 = KoreMeshDataPrimitives.BasicSphere(p1, 0.01, debugCol, 16);
        KoreMeshData sphereMeshP2 = KoreMeshDataPrimitives.BasicSphere(p2, 0.01, debugCol2, 16);
        KoreMeshData sphereMeshP3 = KoreMeshDataPrimitives.BasicSphere(p3, 0.01, debugCol2, 16);

        // Combine the sphere meshes into the main mesh
        newMesh = KoreMeshData.BasicAppendMesh(newMesh, sphereMeshP1);
        newMesh = KoreMeshData.BasicAppendMesh(newMesh, sphereMeshP2);
        newMesh = KoreMeshData.BasicAppendMesh(newMesh, sphereMeshP3);

        return newMesh;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Bezier4Line
    // --------------------------------------------------------------------------------------------

    public static KoreMeshData Bezier4Line(
        KoreXYZVector p1, KoreXYZVector p2, KoreXYZVector p3, KoreXYZVector p4,
        KoreColorRGB? lineColor = null, int divisions = 10)

    {
        KoreMeshData newMesh = new KoreMeshData();

        // Extract individual points for the bezier processing
        KoreNumeric1DArray<double> xValues = new KoreNumeric1DArray<double>(4);
        KoreNumeric1DArray<double> yValues = new KoreNumeric1DArray<double>(4);
        KoreNumeric1DArray<double> zValues = new KoreNumeric1DArray<double>(4);
        xValues[0] = p1.X; xValues[1] = p2.X; xValues[2] = p3.X; xValues[3] = p4.X;
        yValues[0] = p1.Y; yValues[1] = p2.Y; yValues[2] = p3.Y; yValues[3] = p4.Y;
        zValues[0] = p1.Z; zValues[1] = p2.Z; zValues[2] = p3.Z; zValues[3] = p4.Z;

        // Create the fraction steps to follow along the bezier curve
        KoreNumeric1DArray<double> fractionList = KoreNumeric1DArrayOps<double>.ListForRange(0, 1, divisions);

        // Create the list of point ID along the line
        List<int> pointIds = new List<int>();

        // Loop through the fraction list to calculate the bezier points
        foreach (double currVal in fractionList)
        {
            double currX = KoreNumeric1DArrayOps<double>.CalculateBezierPoint(currVal, xValues);
            double currY = KoreNumeric1DArrayOps<double>.CalculateBezierPoint(currVal, yValues);
            double currZ = KoreNumeric1DArrayOps<double>.CalculateBezierPoint(currVal, zValues);

            KoreXYZVector newPoint = new KoreXYZVector(currX, currY, currZ);

            int currPointId = newMesh.AddCompleteVertex(newPoint, null, null, null);
            pointIds.Add(currPointId);
        }

        // Loop through all the points, adding them and making lines.
        for (int i = 0; i < pointIds.Count - 1; i++)
        {
            int startId = pointIds[i];
            int endId = pointIds[i + 1];
            newMesh.AddLine(startId, endId, lineColor);
        }

        KoreColorRGB debugCol = KoreColorPalette.Colors["Red"];
        KoreColorRGB debugCol2 = KoreColorPalette.Colors["White"];

        // Add dotted lines to debug the control points
        newMesh.AddDottedLineByDistance(p1, p2, debugCol, 0.1);
        newMesh.AddDottedLineByDistance(p2, p3, debugCol, 0.1);
        newMesh.AddDottedLineByDistance(p3, p4, debugCol, 0.1);

        // add small spheres at the control points
        KoreMeshData sphereMeshP1 = KoreMeshDataPrimitives.BasicSphere(p1, 0.01, debugCol, 16);
        KoreMeshData sphereMeshP2 = KoreMeshDataPrimitives.BasicSphere(p2, 0.01, debugCol2, 16);
        KoreMeshData sphereMeshP3 = KoreMeshDataPrimitives.BasicSphere(p3, 0.01, debugCol2, 16);
        KoreMeshData sphereMeshP4 = KoreMeshDataPrimitives.BasicSphere(p4, 0.01, debugCol2, 16);

        // Combine the sphere meshes into the main mesh
        newMesh = KoreMeshData.BasicAppendMesh(newMesh, sphereMeshP1);
        newMesh = KoreMeshData.BasicAppendMesh(newMesh, sphereMeshP2);
        newMesh = KoreMeshData.BasicAppendMesh(newMesh, sphereMeshP3);
        newMesh = KoreMeshData.BasicAppendMesh(newMesh, sphereMeshP4);

        return newMesh;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Bezier Support
    // --------------------------------------------------------------------------------------------

    // Usage : List<KoreXYZVector> points = KoreMeshDataPrimitives.PointsListFromBezier(controlPoints, divisions);
    public static List<KoreXYZVector> PointsListFromBezier(
        List<KoreXYZVector> controlPoints, int divisions = 10)
    {
        List<KoreXYZVector> resultPoints = new List<KoreXYZVector>();

        if (controlPoints.Count < 3)
        {
            throw new ArgumentException("Need at least 3 control points for a Bézier curve");
        }

        if (controlPoints.Count == 3)
        {
            // 3-point Bézier curve
            return CalculateBezier3Points(controlPoints[0], controlPoints[1], controlPoints[2], divisions);
        }
        else if (controlPoints.Count == 4)
        {
            // 4-point Bézier curve
            return CalculateBezier4Points(controlPoints[0], controlPoints[1], controlPoints[2], controlPoints[3], divisions);
        }
        else
        {
            // 5+ points: Create a smooth compound curve with C1 continuity
            // Use overlapping control points with partial t ranges to ensure smooth connections

            int segmentStart = 0;
            bool isFirstSegment = true;

            while (segmentStart + 2 < controlPoints.Count)
            {
                int remainingPoints = controlPoints.Count - segmentStart;
                List<KoreXYZVector> segmentPoints;

                if (remainingPoints >= 4)
                {
                    // Use 4-point Bézier
                    if (isFirstSegment)
                    {
                        // First segment: use full range 0 to 1
                        segmentPoints = CalculateBezier4Points(
                            controlPoints[segmentStart],
                            controlPoints[segmentStart + 1],
                            controlPoints[segmentStart + 2],
                            controlPoints[segmentStart + 3],
                            divisions, 0.0, 1.0);
                        isFirstSegment = false;
                    }
                    else
                    {
                        // Subsequent segments: use partial range to avoid overlap
                        // Use points 2,3,4,5 but only compute t from 0.5 to 1.0
                        segmentPoints = CalculateBezier4Points(
                            controlPoints[segmentStart],
                            controlPoints[segmentStart + 1],
                            controlPoints[segmentStart + 2],
                            controlPoints[segmentStart + 3],
                            divisions, 0.5, 1.0);
                    }

                    // Check if this is the final segment
                    if (remainingPoints == 4)
                    {
                        segmentStart += 4; // Move to end
                    }
                    else
                    {
                        segmentStart += 2; // Advance by 2 for overlap
                    }
                }
                else
                {
                    // 3 points left - use 3-point Bézier
                    if (isFirstSegment)
                    {
                        segmentPoints = CalculateBezier3Points(
                            controlPoints[segmentStart],
                            controlPoints[segmentStart + 1],
                            controlPoints[segmentStart + 2],
                            divisions, 0.0, 1.0);
                    }
                    else
                    {
                        segmentPoints = CalculateBezier3Points(
                            controlPoints[segmentStart],
                            controlPoints[segmentStart + 1],
                            controlPoints[segmentStart + 2],
                            divisions, 0.5, 1.0);
                    }
                    break; // This is the final segment
                }

                // Add points - for first segment add all, for subsequent skip first point
                if (resultPoints.Count == 0)
                {
                    resultPoints.AddRange(segmentPoints);
                }
                else
                {
                    // Skip the first point to avoid duplication at connection
                    resultPoints.AddRange(segmentPoints.Skip(1));
                }
            }
        }

        return resultPoints;
    }

    // Helper method for 3-point Bézier calculation
    private static List<KoreXYZVector> CalculateBezier3Points(KoreXYZVector p1, KoreXYZVector p2, KoreXYZVector p3, int divisions, double tStart = 0.0, double tEnd = 1.0)
    {
        var points = new List<KoreXYZVector>();

        KoreNumeric1DArray<double> xValues = new KoreNumeric1DArray<double>(3);
        KoreNumeric1DArray<double> yValues = new KoreNumeric1DArray<double>(3);
        KoreNumeric1DArray<double> zValues = new KoreNumeric1DArray<double>(3);

        xValues[0] = p1.X; xValues[1] = p2.X; xValues[2] = p3.X;
        yValues[0] = p1.Y; yValues[1] = p2.Y; yValues[2] = p3.Y;
        zValues[0] = p1.Z; zValues[1] = p2.Z; zValues[2] = p3.Z;

        KoreNumeric1DArray<double> fractionList = KoreNumeric1DArrayOps<double>.ListForRange(tStart, tEnd, divisions);

        foreach (double t in fractionList)
        {
            double x = KoreNumeric1DArrayOps<double>.CalculateBezierPoint(t, xValues);
            double y = KoreNumeric1DArrayOps<double>.CalculateBezierPoint(t, yValues);
            double z = KoreNumeric1DArrayOps<double>.CalculateBezierPoint(t, zValues);

            points.Add(new KoreXYZVector(x, y, z));
        }

        return points;
    }

    // Helper method for 4-point Bézier calculation
    private static List<KoreXYZVector> CalculateBezier4Points(KoreXYZVector p1, KoreXYZVector p2, KoreXYZVector p3, KoreXYZVector p4, int divisions, double tStart = 0.0, double tEnd = 1.0)
    {
        var points = new List<KoreXYZVector>();

        KoreNumeric1DArray<double> xValues = new KoreNumeric1DArray<double>(4);
        KoreNumeric1DArray<double> yValues = new KoreNumeric1DArray<double>(4);
        KoreNumeric1DArray<double> zValues = new KoreNumeric1DArray<double>(4);

        xValues[0] = p1.X; xValues[1] = p2.X; xValues[2] = p3.X; xValues[3] = p4.X;
        yValues[0] = p1.Y; yValues[1] = p2.Y; yValues[2] = p3.Y; yValues[3] = p4.Y;
        zValues[0] = p1.Z; zValues[1] = p2.Z; zValues[2] = p3.Z; zValues[3] = p4.Z;

        KoreNumeric1DArray<double> fractionList = KoreNumeric1DArrayOps<double>.ListForRange(tStart, tEnd, divisions);

        foreach (double t in fractionList)
        {
            double x = KoreNumeric1DArrayOps<double>.CalculateBezierPoint(t, xValues);
            double y = KoreNumeric1DArrayOps<double>.CalculateBezierPoint(t, yValues);
            double z = KoreNumeric1DArrayOps<double>.CalculateBezierPoint(t, zValues);

            points.Add(new KoreXYZVector(x, y, z));
        }

        return points;
    }



}
