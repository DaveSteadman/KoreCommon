// <fileheader>

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;

using KoreCommon;

namespace KoreGIS;

// Partial class for reading SHP (geometry) files
public static partial class KoreShapefileReader
{
    // Reads the SHP file for geometry data.
    private static void ReadShpFile(string shpPath, KoreShapefileFeatureCollection collection,
        List<Dictionary<string, object?>> attributes)
    {
        using var stream = new FileStream(shpPath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream);

        // Read main file header (100 bytes)
        int fileCode = ReadBigEndianInt32(reader);
        if (fileCode != ShapefileFileCode)
            throw new KoreShapefileException($"Invalid Shapefile: expected file code {ShapefileFileCode}, got {fileCode}");

        reader.ReadBytes(20); // Unused
        int fileLength = ReadBigEndianInt32(reader) * 2; // File length in 16-bit words
        int version = reader.ReadInt32(); // Little-endian
        int shapeType = reader.ReadInt32(); // Little-endian

        collection.GeometryType = (ShapefileGeometryType)shapeType;

        // Bounding box (little-endian doubles)
        double xMin = reader.ReadDouble();
        double yMin = reader.ReadDouble();
        double xMax = reader.ReadDouble();
        double yMax = reader.ReadDouble();
        double zMin = reader.ReadDouble();
        double zMax = reader.ReadDouble();
        double mMin = reader.ReadDouble();
        double mMax = reader.ReadDouble();

        collection.BoundingBox = new KoreLLBox
        {
            MinLonDegs = xMin,
            MinLatDegs = yMin,
            MaxLonDegs = xMax,
            MaxLatDegs = yMax
        };

        // Read records
        int recordIndex = 0;
        while (stream.Position < fileLength)
        {
            try
            {
                // Record header (big-endian)
                int recordNumber = ReadBigEndianInt32(reader);
                int contentLength = ReadBigEndianInt32(reader) * 2; // In 16-bit words

                long recordStart = stream.Position;

                // Shape type (little-endian)
                int recordShapeType = reader.ReadInt32();

                var feature = new KoreShapefileFeature
                {
                    RecordNumber = recordNumber,
                    GeometryType = (ShapefileGeometryType)recordShapeType
                };

                // Get attributes for this record
                if (recordIndex < attributes.Count)
                {
                    feature.Attributes = attributes[recordIndex];
                }

                // Parse geometry based on shape type
                switch ((ShapefileGeometryType)recordShapeType)
                {
                    case ShapefileGeometryType.Null:
                        // Null shape - no geometry
                        break;

                    case ShapefileGeometryType.Point:
                    case ShapefileGeometryType.PointM:
                    case ShapefileGeometryType.PointZ:
                        ReadPointGeometry(reader, feature, recordShapeType);
                        break;

                    case ShapefileGeometryType.MultiPoint:
                    case ShapefileGeometryType.MultiPointM:
                    case ShapefileGeometryType.MultiPointZ:
                        ReadMultiPointGeometry(reader, feature, recordShapeType);
                        break;

                    case ShapefileGeometryType.PolyLine:
                    case ShapefileGeometryType.PolyLineM:
                    case ShapefileGeometryType.PolyLineZ:
                        ReadPolyLineGeometry(reader, feature, recordShapeType);
                        break;

                    case ShapefileGeometryType.Polygon:
                    case ShapefileGeometryType.PolygonM:
                    case ShapefileGeometryType.PolygonZ:
                        ReadPolygonGeometry(reader, feature, recordShapeType);
                        break;

                    default:
                        collection.Warnings.Add($"Unsupported shape type {recordShapeType} in record {recordNumber}");
                        break;
                }

                collection.Features.Add(feature);

                // Ensure we're at the expected position after reading
                long expectedEnd = recordStart + contentLength;
                if (stream.Position < expectedEnd)
                {
                    stream.Position = expectedEnd;
                }

                recordIndex++;
            }
            catch (EndOfStreamException)
            {
                break; // End of file reached
            }
            catch (Exception ex)
            {
                collection.Warnings.Add($"Failed to read record {recordIndex + 1}: {ex.Message}");
                // Try to continue with next record by skipping remaining bytes
                recordIndex++;
            }
        }
    }

    // Reads a Point geometry.
    private static void ReadPointGeometry(BinaryReader reader, KoreShapefileFeature feature, int shapeType)
    {
        double x = reader.ReadDouble();
        double y = reader.ReadDouble();

        // Skip Z value if present
        if (shapeType == (int)ShapefileGeometryType.PointZ)
        {
            reader.ReadDouble(); // Z
        }

        // Skip M value if present
        if (shapeType == (int)ShapefileGeometryType.PointM || shapeType == (int)ShapefileGeometryType.PointZ)
        {
            reader.ReadDouble(); // M
        }

        var point = new KoreGeoPoint
        {
            Position = new KoreLLPoint { LonDegs = x, LatDegs = y }
        };

        feature.Geometry = point;
        feature.BoundingBox = new KoreLLBox
        {
            MinLonDegs = x,
            MinLatDegs = y,
            MaxLonDegs = x,
            MaxLatDegs = y
        };
    }

    // Reads a MultiPoint geometry.
    private static void ReadMultiPointGeometry(BinaryReader reader, KoreShapefileFeature feature, int shapeType)
    {
        // Bounding box
        double xMin = reader.ReadDouble();
        double yMin = reader.ReadDouble();
        double xMax = reader.ReadDouble();
        double yMax = reader.ReadDouble();

        int numPoints = reader.ReadInt32();

        var multiPoint = new KoreGeoMultiPoint();

        // Read points
        for (int i = 0; i < numPoints; i++)
        {
            double x = reader.ReadDouble();
            double y = reader.ReadDouble();
            multiPoint.Points.Add(new KoreLLPoint { LonDegs = x, LatDegs = y });
        }

        // Skip Z values if present
        if (shapeType == (int)ShapefileGeometryType.MultiPointZ)
        {
            reader.ReadDouble(); // zMin
            reader.ReadDouble(); // zMax
            for (int i = 0; i < numPoints; i++)
                reader.ReadDouble(); // Z values
        }

        // Skip M values if present
        if (shapeType == (int)ShapefileGeometryType.MultiPointM || shapeType == (int)ShapefileGeometryType.MultiPointZ)
        {
            reader.ReadDouble(); // mMin
            reader.ReadDouble(); // mMax
            for (int i = 0; i < numPoints; i++)
                reader.ReadDouble(); // M values
        }

        multiPoint.CalcBoundingBox();
        feature.Geometry = multiPoint;
        feature.BoundingBox = new KoreLLBox
        {
            MinLonDegs = xMin,
            MinLatDegs = yMin,
            MaxLonDegs = xMax,
            MaxLatDegs = yMax
        };
    }

    // Reads a PolyLine geometry.
    private static void ReadPolyLineGeometry(BinaryReader reader, KoreShapefileFeature feature, int shapeType)
    {
        // Bounding box
        double xMin = reader.ReadDouble();
        double yMin = reader.ReadDouble();
        double xMax = reader.ReadDouble();
        double yMax = reader.ReadDouble();

        int numParts = reader.ReadInt32();
        int numPoints = reader.ReadInt32();

        // Read part indices
        int[] parts = new int[numParts];
        for (int i = 0; i < numParts; i++)
        {
            parts[i] = reader.ReadInt32();
        }

        // Read all points
        var allPoints = new KoreLLPoint[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            double x = reader.ReadDouble();
            double y = reader.ReadDouble();
            allPoints[i] = new KoreLLPoint { LonDegs = x, LatDegs = y };
        }

        // Skip Z values if present
        if (shapeType == (int)ShapefileGeometryType.PolyLineZ)
        {
            reader.ReadDouble(); // zMin
            reader.ReadDouble(); // zMax
            for (int i = 0; i < numPoints; i++)
                reader.ReadDouble(); // Z values
        }

        // Skip M values if present
        if (shapeType == (int)ShapefileGeometryType.PolyLineM || shapeType == (int)ShapefileGeometryType.PolyLineZ)
        {
            reader.ReadDouble(); // mMin
            reader.ReadDouble(); // mMax
            for (int i = 0; i < numPoints; i++)
                reader.ReadDouble(); // M values
        }

        // Create multi-line string from parts
        var multiLine = new KoreGeoMultiLineString();

        for (int p = 0; p < numParts; p++)
        {
            int start = parts[p];
            int end = (p + 1 < numParts) ? parts[p + 1] : numPoints;

            var linePoints = new List<KoreLLPoint>();
            for (int i = start; i < end; i++)
            {
                linePoints.Add(allPoints[i]);
            }
            multiLine.LineStrings.Add(linePoints);
        }

        multiLine.CalcBoundingBox();
        feature.Geometry = multiLine;
        feature.BoundingBox = new KoreLLBox
        {
            MinLonDegs = xMin,
            MinLatDegs = yMin,
            MaxLonDegs = xMax,
            MaxLatDegs = yMax
        };
    }

    // Reads a Polygon geometry.
    private static void ReadPolygonGeometry(BinaryReader reader, KoreShapefileFeature feature, int shapeType)
    {
        // Bounding box
        double xMin = reader.ReadDouble();
        double yMin = reader.ReadDouble();
        double xMax = reader.ReadDouble();
        double yMax = reader.ReadDouble();

        int numParts = reader.ReadInt32();
        int numPoints = reader.ReadInt32();

        // Read part indices
        int[] parts = new int[numParts];
        for (int i = 0; i < numParts; i++)
        {
            parts[i] = reader.ReadInt32();
        }

        // Read all points
        var allPoints = new KoreLLPoint[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            double x = reader.ReadDouble();
            double y = reader.ReadDouble();
            allPoints[i] = new KoreLLPoint { LonDegs = x, LatDegs = y };
        }

        // Skip Z values if present
        if (shapeType == (int)ShapefileGeometryType.PolygonZ)
        {
            reader.ReadDouble(); // zMin
            reader.ReadDouble(); // zMax
            for (int i = 0; i < numPoints; i++)
                reader.ReadDouble(); // Z values
        }

        // Skip M values if present
        if (shapeType == (int)ShapefileGeometryType.PolygonM || shapeType == (int)ShapefileGeometryType.PolygonZ)
        {
            reader.ReadDouble(); // mMin
            reader.ReadDouble(); // mMax
            for (int i = 0; i < numPoints; i++)
                reader.ReadDouble(); // M values
        }

        // Create multi-polygon from parts
        // In Shapefile, outer rings are clockwise and holes are counter-clockwise
        var multiPolygon = new KoreGeoMultiPolygon();
        var rings = new List<List<KoreLLPoint>>();

        for (int p = 0; p < numParts; p++)
        {
            int start = parts[p];
            int end = (p + 1 < numParts) ? parts[p + 1] : numPoints;

            var ringPoints = new List<KoreLLPoint>();
            for (int i = start; i < end; i++)
            {
                ringPoints.Add(allPoints[i]);
            }
            rings.Add(ringPoints);
        }

        // Determine which rings are outer rings and which are holes
        // Using signed area: positive = counter-clockwise (hole in Shapefile), negative = clockwise (outer)
        // Note: In Shapefile spec, outer rings are clockwise
        KoreGeoPolygon? currentPolygon = null;

        foreach (var ring in rings)
        {
            double signedArea = CalculateSignedArea(ring);

            if (signedArea < 0) // Clockwise = outer ring in Shapefile
            {
                // Start a new polygon
                if (currentPolygon != null)
                {
                    multiPolygon.Polygons.Add(currentPolygon);
                }
                currentPolygon = new KoreGeoPolygon { OuterRing = ring };
            }
            else // Counter-clockwise = hole
            {
                if (currentPolygon != null)
                {
                    currentPolygon.InnerRings.Add(ring);
                }
                else
                {
                    // Hole without outer ring - treat as outer ring
                    currentPolygon = new KoreGeoPolygon { OuterRing = ring };
                }
            }
        }

        if (currentPolygon != null)
        {
            multiPolygon.Polygons.Add(currentPolygon);
        }

        multiPolygon.CalcBoundingBox();
        feature.Geometry = multiPolygon;
        feature.BoundingBox = new KoreLLBox
        {
            MinLonDegs = xMin,
            MinLatDegs = yMin,
            MaxLonDegs = xMax,
            MaxLatDegs = yMax
        };
    }

    // Calculates the signed area of a ring (for determining winding direction).
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

    // Reads a big-endian 32-bit integer.
    private static int ReadBigEndianInt32(BinaryReader reader)
    {
        byte[] bytes = reader.ReadBytes(4);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return BitConverter.ToInt32(bytes, 0);
    }
}

