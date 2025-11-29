// <fileheader>

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using KoreCommon;

namespace KoreGIS;

// Partial class for writing SHP (geometry) and SHX (index) files
public static partial class KoreShapefileWriter
{
    private const int ShapefileFileCode = 9994;
    private const int ShapefileVersion = 1000;

    // Writes the SHP and SHX files.
    private static void WriteShpAndShx(string shpPath, string shxPath, KoreShapefileFeatureCollection collection, KoreLLBox? bbox)
    {
        using var shpStream = new MemoryStream();
        using var shxStream = new MemoryStream();
        using var shpWriter = new BinaryWriter(shpStream);
        using var shxWriter = new BinaryWriter(shxStream);

        // Reserve space for headers (100 bytes each)
        shpWriter.Write(new byte[100]);
        shxWriter.Write(new byte[100]);

        int recordNumber = 1;
        foreach (var feature in collection.Features)
        {
            long shpOffset = shpStream.Position / 2; // Offset in 16-bit words

            // Write record to SHP
            int contentLength = WriteShpRecord(shpWriter, feature, recordNumber);

            // Write index entry to SHX
            WriteBigEndianInt32(shxWriter, (int)shpOffset);
            WriteBigEndianInt32(shxWriter, contentLength);

            recordNumber++;
        }

        // Calculate file lengths in 16-bit words
        int shpFileLength = (int)(shpStream.Position / 2);
        int shxFileLength = (int)(shxStream.Position / 2);

        // Write headers
        WriteShpHeader(shpStream, shpFileLength, collection.GeometryType, bbox);
        WriteShpHeader(shxStream, shxFileLength, collection.GeometryType, bbox);

        // Write to files
        File.WriteAllBytes(shpPath, shpStream.ToArray());
        File.WriteAllBytes(shxPath, shxStream.ToArray());
    }

    // Writes the SHP file header.
    private static void WriteShpHeader(MemoryStream stream, int fileLength, ShapefileGeometryType shapeType, KoreLLBox? bbox)
    {
        stream.Position = 0;
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);

        // File code (big-endian)
        WriteBigEndianInt32(writer, ShapefileFileCode);

        // Unused (20 bytes)
        writer.Write(new byte[20]);

        // File length in 16-bit words (big-endian)
        WriteBigEndianInt32(writer, fileLength);

        // Version (little-endian)
        writer.Write(ShapefileVersion);

        // Shape type (little-endian)
        writer.Write((int)shapeType);

        // Bounding box (little-endian doubles)
        double xMin = bbox?.MinLonDegs ?? 0;
        double yMin = bbox?.MinLatDegs ?? 0;
        double xMax = bbox?.MaxLonDegs ?? 0;
        double yMax = bbox?.MaxLatDegs ?? 0;

        writer.Write(xMin);
        writer.Write(yMin);
        writer.Write(xMax);
        writer.Write(yMax);
        writer.Write(0.0); // zMin
        writer.Write(0.0); // zMax
        writer.Write(0.0); // mMin
        writer.Write(0.0); // mMax
    }

    // Writes a single SHP record and returns the content length in 16-bit words.
    private static int WriteShpRecord(BinaryWriter writer, KoreShapefileFeature feature, int recordNumber)
    {
        long contentStart = writer.BaseStream.Position + 8; // After header

        // Record header will be written after we know the content length
        long headerPosition = writer.BaseStream.Position;
        writer.Write(new byte[8]); // Placeholder for header

        // Write shape type
        writer.Write((int)feature.GeometryType);

        // Write geometry
        switch (feature.GeometryType)
        {
            case ShapefileGeometryType.Null:
                // Just shape type, no additional data
                break;

            case ShapefileGeometryType.Point:
                WritePointGeometry(writer, feature.Geometry);
                break;

            case ShapefileGeometryType.MultiPoint:
                WriteMultiPointGeometry(writer, feature.Geometry);
                break;

            case ShapefileGeometryType.PolyLine:
                WritePolyLineGeometry(writer, feature.Geometry);
                break;

            case ShapefileGeometryType.Polygon:
                WritePolygonGeometry(writer, feature.Geometry);
                break;
        }

        long contentEnd = writer.BaseStream.Position;
        int contentLength = (int)((contentEnd - contentStart) / 2); // In 16-bit words

        // Go back and write header
        long currentPosition = writer.BaseStream.Position;
        writer.BaseStream.Position = headerPosition;
        WriteBigEndianInt32(writer, recordNumber);
        WriteBigEndianInt32(writer, contentLength);
        writer.BaseStream.Position = currentPosition;

        return contentLength;
    }

    // Writes a Point geometry.
    private static void WritePointGeometry(BinaryWriter writer, KoreGeoFeature? geometry)
    {
        if (geometry is KoreGeoPoint point)
        {
            writer.Write(point.Position.LonDegs); // X
            writer.Write(point.Position.LatDegs); // Y
        }
        else
        {
            writer.Write(0.0);
            writer.Write(0.0);
        }
    }

    // Writes a MultiPoint geometry.
    private static void WriteMultiPointGeometry(BinaryWriter writer, KoreGeoFeature? geometry)
    {
        var points = new List<KoreLLPoint>();
        if (geometry is KoreGeoMultiPoint multiPoint)
        {
            points = multiPoint.Points;
        }
        else if (geometry is KoreGeoPoint point)
        {
            points.Add(point.Position);
        }

        // Calculate bounding box
        double xMin = double.MaxValue, yMin = double.MaxValue;
        double xMax = double.MinValue, yMax = double.MinValue;
        foreach (var p in points)
        {
            xMin = Math.Min(xMin, p.LonDegs);
            yMin = Math.Min(yMin, p.LatDegs);
            xMax = Math.Max(xMax, p.LonDegs);
            yMax = Math.Max(yMax, p.LatDegs);
        }

        if (points.Count == 0)
        {
            xMin = yMin = xMax = yMax = 0;
        }

        // Write bounding box
        writer.Write(xMin);
        writer.Write(yMin);
        writer.Write(xMax);
        writer.Write(yMax);

        // Write number of points
        writer.Write(points.Count);

        // Write points
        foreach (var p in points)
        {
            writer.Write(p.LonDegs);
            writer.Write(p.LatDegs);
        }
    }

    // Writes a PolyLine geometry.
    private static void WritePolyLineGeometry(BinaryWriter writer, KoreGeoFeature? geometry)
    {
        var parts = new List<List<KoreLLPoint>>();

        if (geometry is KoreGeoMultiLineString multiLine)
        {
            parts = multiLine.LineStrings;
        }
        else if (geometry is KoreGeoLineString lineString)
        {
            parts.Add(lineString.Points);
        }

        WritePartsGeometry(writer, parts);
    }

    // Writes a Polygon geometry.
    private static void WritePolygonGeometry(BinaryWriter writer, KoreGeoFeature? geometry)
    {
        var parts = new List<List<KoreLLPoint>>();

        if (geometry is KoreGeoMultiPolygon multiPolygon)
        {
            foreach (var polygon in multiPolygon.Polygons)
            {
                // Outer ring should be clockwise in Shapefile format
                var outerRing = EnsureClockwise(polygon.OuterRing);
                parts.Add(outerRing);

                // Inner rings (holes) should be counter-clockwise
                foreach (var innerRing in polygon.InnerRings)
                {
                    var hole = EnsureCounterClockwise(innerRing);
                    parts.Add(hole);
                }
            }
        }
        else if (geometry is KoreGeoPolygon polygon)
        {
            var outerRing = EnsureClockwise(polygon.OuterRing);
            parts.Add(outerRing);

            foreach (var innerRing in polygon.InnerRings)
            {
                var hole = EnsureCounterClockwise(innerRing);
                parts.Add(hole);
            }
        }

        WritePartsGeometry(writer, parts);
    }

    // Writes geometry with parts (used by PolyLine and Polygon).
    private static void WritePartsGeometry(BinaryWriter writer, List<List<KoreLLPoint>> parts)
    {
        // Calculate bounding box and total points
        double xMin = double.MaxValue, yMin = double.MaxValue;
        double xMax = double.MinValue, yMax = double.MinValue;
        int totalPoints = 0;

        foreach (var part in parts)
        {
            foreach (var p in part)
            {
                xMin = Math.Min(xMin, p.LonDegs);
                yMin = Math.Min(yMin, p.LatDegs);
                xMax = Math.Max(xMax, p.LonDegs);
                yMax = Math.Max(yMax, p.LatDegs);
            }
            totalPoints += part.Count;
        }

        if (totalPoints == 0)
        {
            xMin = yMin = xMax = yMax = 0;
        }

        // Write bounding box
        writer.Write(xMin);
        writer.Write(yMin);
        writer.Write(xMax);
        writer.Write(yMax);

        // Write number of parts and points
        writer.Write(parts.Count);
        writer.Write(totalPoints);

        // Write part indices
        int index = 0;
        foreach (var part in parts)
        {
            writer.Write(index);
            index += part.Count;
        }

        // Write all points
        foreach (var part in parts)
        {
            foreach (var p in part)
            {
                writer.Write(p.LonDegs);
                writer.Write(p.LatDegs);
            }
        }
    }

    // Ensures a ring is clockwise (for outer rings in Shapefile format).
    private static List<KoreLLPoint> EnsureClockwise(List<KoreLLPoint> ring)
    {
        if (ring.Count < 3)
            return new List<KoreLLPoint>(ring);

        double signedArea = CalculateSignedArea(ring);
        if (signedArea > 0) // Counter-clockwise, need to reverse
        {
            var reversed = new List<KoreLLPoint>(ring);
            reversed.Reverse();
            return reversed;
        }
        return new List<KoreLLPoint>(ring);
    }

    // Ensures a ring is counter-clockwise (for holes in Shapefile format).
    private static List<KoreLLPoint> EnsureCounterClockwise(List<KoreLLPoint> ring)
    {
        if (ring.Count < 3)
            return new List<KoreLLPoint>(ring);

        double signedArea = CalculateSignedArea(ring);
        if (signedArea < 0) // Clockwise, need to reverse
        {
            var reversed = new List<KoreLLPoint>(ring);
            reversed.Reverse();
            return reversed;
        }
        return new List<KoreLLPoint>(ring);
    }

    // Calculates the signed area of a ring.
    // Positive = counter-clockwise, Negative = clockwise
    private static double CalculateSignedArea(List<KoreLLPoint> ring)
    {
        if (ring.Count < 3)
            return 0;

        double area = 0;
        for (int i = 0; i < ring.Count; i++)
        {
            int j = (i + 1) % ring.Count;
            area += ring[i].LonDegs * ring[j].LatDegs;
            area -= ring[j].LonDegs * ring[i].LatDegs;
        }
        return area / 2.0;
    }

    // Writes a big-endian 32-bit integer.
    private static void WriteBigEndianInt32(BinaryWriter writer, int value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        writer.Write(bytes);
    }
}

