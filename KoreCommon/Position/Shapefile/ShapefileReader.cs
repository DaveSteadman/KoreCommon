// <fileheader>

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KoreCommon;

/// <summary>
/// Reads ESRI Shapefiles (.shp, .shx, .dbf, .prj) into a ShapefileFeatureCollection.
/// </summary>
public static class ShapefileReader
{
    private const int ShapefileFileCode = 9994;
    private const int ShapefileVersion = 1000;

    /// <summary>
    /// Reads a Shapefile from the given path.
    /// </summary>
    /// <param name="path">Path to the .shp file or base path without extension.</param>
    /// <returns>A ShapefileFeatureCollection containing all features.</returns>
    public static ShapefileFeatureCollection Read(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));

        // Normalize path - remove extension if present
        string basePath = Path.ChangeExtension(path, null);
        string shpPath = basePath + ".shp";
        string shxPath = basePath + ".shx";
        string dbfPath = basePath + ".dbf";
        string prjPath = basePath + ".prj";

        // Check for case-insensitive file extensions
        shpPath = FindFileWithExtension(basePath, ".shp") ?? shpPath;
        shxPath = FindFileWithExtension(basePath, ".shx") ?? shxPath;
        dbfPath = FindFileWithExtension(basePath, ".dbf") ?? dbfPath;
        prjPath = FindFileWithExtension(basePath, ".prj") ?? prjPath;

        if (!File.Exists(shpPath))
            throw new ShapefileException($"Shapefile not found: {shpPath}");

        var collection = new ShapefileFeatureCollection();

        // Read PRJ file first (optional)
        ReadPrjFile(prjPath, collection);

        // Read DBF file for attributes (if exists)
        var attributes = new List<Dictionary<string, object?>>();
        var fieldDescriptors = new List<DbfFieldDescriptor>();
        if (File.Exists(dbfPath))
        {
            ReadDbfFile(dbfPath, attributes, fieldDescriptors, collection);
            collection.FieldDescriptors = fieldDescriptors;
        }

        // Read SHP file for geometries
        ReadShpFile(shpPath, collection, attributes);

        return collection;
    }

    /// <summary>
    /// Finds a file with the given extension, handling case-insensitive matching.
    /// </summary>
    private static string? FindFileWithExtension(string basePath, string extension)
    {
        string dir = Path.GetDirectoryName(basePath) ?? ".";
        string fileName = Path.GetFileName(basePath);

        // Try exact case first
        string exactPath = Path.Combine(dir, fileName + extension);
        if (File.Exists(exactPath))
            return exactPath;

        // Try uppercase
        string upperPath = Path.Combine(dir, fileName + extension.ToUpperInvariant());
        if (File.Exists(upperPath))
            return upperPath;

        return null;
    }

    /// <summary>
    /// Reads the projection file (.prj) if it exists.
    /// </summary>
    private static void ReadPrjFile(string prjPath, ShapefileFeatureCollection collection)
    {
        if (!File.Exists(prjPath))
            return;

        try
        {
            string wkt = File.ReadAllText(prjPath).Trim();
            collection.ProjectionWkt = wkt;

            // Check if it's WGS84 - look for common identifiers
            bool isWgs84 = wkt.Contains("WGS_1984") ||
                          wkt.Contains("WGS 84") ||
                          wkt.Contains("WGS84") ||
                          wkt.Contains("EPSG:4326") ||
                          wkt.Contains("\"4326\"");

            if (!isWgs84 && !string.IsNullOrEmpty(wkt))
            {
                collection.Warnings.Add($"Projection may not be WGS84. Raw coordinate values will be used without reprojection. PRJ contents: {wkt.Substring(0, Math.Min(100, wkt.Length))}...");
            }
        }
        catch (Exception ex)
        {
            collection.Warnings.Add($"Failed to read PRJ file: {ex.Message}");
        }
    }

    /// <summary>
    /// Reads the DBF file for attribute data.
    /// </summary>
    private static void ReadDbfFile(string dbfPath, List<Dictionary<string, object?>> attributes,
        List<DbfFieldDescriptor> fieldDescriptors, ShapefileFeatureCollection collection)
    {
        try
        {
            using var stream = new FileStream(dbfPath, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(stream);

            // DBF Header
            byte version = reader.ReadByte();
            byte year = reader.ReadByte();
            byte month = reader.ReadByte();
            byte day = reader.ReadByte();
            int recordCount = reader.ReadInt32();
            short headerLength = reader.ReadInt16();
            short recordLength = reader.ReadInt16();
            reader.ReadBytes(20); // Reserved bytes

            // Calculate number of fields: (headerLength - 32 - 1) / 32
            int fieldCount = (headerLength - 33) / 32;

            // Read field descriptors
            for (int i = 0; i < fieldCount; i++)
            {
                var field = new DbfFieldDescriptor();

                byte[] nameBytes = reader.ReadBytes(11);
                field.Name = Encoding.ASCII.GetString(nameBytes).TrimEnd('\0', ' ');
                field.FieldType = (char)reader.ReadByte();
                reader.ReadBytes(4); // Reserved
                field.Length = reader.ReadByte();
                field.DecimalCount = reader.ReadByte();
                reader.ReadBytes(14); // Reserved

                fieldDescriptors.Add(field);
            }

            // Skip header terminator (0x0D)
            reader.ReadByte();

            // Read records
            for (int recordIndex = 0; recordIndex < recordCount; recordIndex++)
            {
                try
                {
                    var record = new Dictionary<string, object?>();

                    // Read deletion flag
                    byte deletionFlag = reader.ReadByte();
                    if (deletionFlag == 0x2A) // '*' = deleted record
                    {
                        reader.ReadBytes(recordLength - 1); // Skip rest of record
                        continue;
                    }

                    // Read field values
                    foreach (var field in fieldDescriptors)
                    {
                        byte[] fieldBytes = reader.ReadBytes(field.Length);
                        string fieldValue = Encoding.ASCII.GetString(fieldBytes).Trim();

                        object? value = ParseDbfValue(fieldValue, field);
                        record[field.Name] = value;
                    }

                    attributes.Add(record);
                }
                catch (Exception ex)
                {
                    collection.Warnings.Add($"Failed to read DBF record {recordIndex + 1}: {ex.Message}");
                    // Try to skip to next record
                    try
                    {
                        long expectedPosition = headerLength + (recordIndex + 1) * recordLength;
                        if (stream.Position < expectedPosition)
                        {
                            stream.Position = expectedPosition;
                        }
                    }
                    catch
                    {
                        // If we can't recover, add empty record
                    }
                    attributes.Add(new Dictionary<string, object?>());
                }
            }
        }
        catch (Exception ex)
        {
            collection.Warnings.Add($"Failed to read DBF file: {ex.Message}");
        }
    }

    /// <summary>
    /// Parses a DBF field value string into the appropriate .NET type.
    /// </summary>
    private static object? ParseDbfValue(string value, DbfFieldDescriptor field)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        try
        {
            switch (field.FieldType)
            {
                case 'C': // Character
                    return value;

                case 'N': // Numeric
                case 'F': // Float
                    if (field.DecimalCount > 0)
                    {
                        if (double.TryParse(value, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double dval))
                            return dval;
                    }
                    else
                    {
                        if (int.TryParse(value, out int ival))
                            return ival;
                        // Try as long if int fails
                        if (long.TryParse(value, out long lval))
                            return (int)lval; // Truncate to int
                        // Fall back to double for very large numbers
                        if (double.TryParse(value, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double dval2))
                            return (int)dval2;
                    }
                    return null;

                case 'L': // Logical
                    char c = value.Length > 0 ? char.ToUpperInvariant(value[0]) : ' ';
                    return c switch
                    {
                        'T' or 'Y' or '1' => true,
                        'F' or 'N' or '0' => false,
                        _ => null
                    };

                case 'D': // Date (YYYYMMDD format)
                    if (value.Length == 8 &&
                        int.TryParse(value.Substring(0, 4), out int year) &&
                        int.TryParse(value.Substring(4, 2), out int month) &&
                        int.TryParse(value.Substring(6, 2), out int day))
                    {
                        try
                        {
                            return new DateTime(year, month, day);
                        }
                        catch
                        {
                            return null;
                        }
                    }
                    return null;

                default:
                    return value;
            }
        }
        catch
        {
            return value; // Return as string if parsing fails
        }
    }

    /// <summary>
    /// Reads the SHP file for geometry data.
    /// </summary>
    private static void ReadShpFile(string shpPath, ShapefileFeatureCollection collection,
        List<Dictionary<string, object?>> attributes)
    {
        using var stream = new FileStream(shpPath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream);

        // Read main file header (100 bytes)
        int fileCode = ReadBigEndianInt32(reader);
        if (fileCode != ShapefileFileCode)
            throw new ShapefileException($"Invalid Shapefile: expected file code {ShapefileFileCode}, got {fileCode}");

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

                var feature = new ShapefileFeature
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

    /// <summary>
    /// Reads a Point geometry.
    /// </summary>
    private static void ReadPointGeometry(BinaryReader reader, ShapefileFeature feature, int shapeType)
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

    /// <summary>
    /// Reads a MultiPoint geometry.
    /// </summary>
    private static void ReadMultiPointGeometry(BinaryReader reader, ShapefileFeature feature, int shapeType)
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

    /// <summary>
    /// Reads a PolyLine geometry.
    /// </summary>
    private static void ReadPolyLineGeometry(BinaryReader reader, ShapefileFeature feature, int shapeType)
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

    /// <summary>
    /// Reads a Polygon geometry.
    /// </summary>
    private static void ReadPolygonGeometry(BinaryReader reader, ShapefileFeature feature, int shapeType)
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

    /// <summary>
    /// Calculates the signed area of a ring (for determining winding direction).
    /// Positive = counter-clockwise, Negative = clockwise
    /// </summary>
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

    /// <summary>
    /// Reads a big-endian 32-bit integer.
    /// </summary>
    private static int ReadBigEndianInt32(BinaryReader reader)
    {
        byte[] bytes = reader.ReadBytes(4);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return BitConverter.ToInt32(bytes, 0);
    }
}
