// <fileheader>

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KoreCommon;

// Writes ESRI Shapefiles (.shp, .shx, .dbf, .prj) from a ShapefileFeatureCollection.
public static class ShapefileWriter
{
    private const int ShapefileFileCode = 9994;
    private const int ShapefileVersion = 1000;

    // WGS84 projection definition in WKT format.
    private const string Wgs84Prj = @"GEOGCS[""GCS_WGS_1984"",DATUM[""D_WGS_1984"",SPHEROID[""WGS_1984"",6378137.0,298.257223563]],PRIMEM[""Greenwich"",0.0],UNIT[""Degree"",0.0174532925199433]]";

    // Writes a ShapefileFeatureCollection to the given path.
    // path: Path to the .shp file or base path without extension.
    // collection: The feature collection to write.
    public static void Write(string path, ShapefileFeatureCollection collection)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));

        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        // Normalize path - remove extension if present
        string basePath = Path.ChangeExtension(path, null);
        string shpPath = basePath + ".shp";
        string shxPath = basePath + ".shx";
        string dbfPath = basePath + ".dbf";
        string prjPath = basePath + ".prj";

        // Ensure directory exists
        string? dir = Path.GetDirectoryName(shpPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        // Infer field descriptors from features if not already set
        var fieldDescriptors = collection.FieldDescriptors.Count > 0
            ? collection.FieldDescriptors
            : InferFieldDescriptors(collection.Features);

        // Calculate bounding box if not set
        var bbox = collection.BoundingBox ?? CalculateBoundingBox(collection.Features);

        // Write all files
        WriteShpAndShx(shpPath, shxPath, collection, bbox);
        WriteDbf(dbfPath, collection.Features, fieldDescriptors);
        WritePrj(prjPath);
    }

    // Infers DBF field descriptors from the attributes of all features.
    private static List<DbfFieldDescriptor> InferFieldDescriptors(List<ShapefileFeature> features)
    {
        var fieldTypes = new Dictionary<string, Type>();
        var maxLengths = new Dictionary<string, int>();

        foreach (var feature in features)
        {
            foreach (var attr in feature.Attributes)
            {
                if (attr.Value == null)
                    continue;

                var valueType = attr.Value.GetType();

                if (!fieldTypes.ContainsKey(attr.Key))
                {
                    fieldTypes[attr.Key] = valueType;
                    maxLengths[attr.Key] = 1;
                }
                else
                {
                    // Promote type if necessary (e.g., int -> double)
                    var existingType = fieldTypes[attr.Key];
                    if (existingType != valueType)
                    {
                        if (IsNumeric(existingType) && IsNumeric(valueType))
                        {
                            fieldTypes[attr.Key] = typeof(double);
                        }
                        else if (existingType != typeof(string))
                        {
                            fieldTypes[attr.Key] = typeof(string);
                        }
                    }
                }

                // Track max string length
                string strValue = FormatAttributeValue(attr.Value);
                if (strValue.Length > maxLengths[attr.Key])
                {
                    maxLengths[attr.Key] = strValue.Length;
                }
            }
        }

        var descriptors = new List<DbfFieldDescriptor>();
        foreach (var kvp in fieldTypes)
        {
            var descriptor = DbfFieldDescriptor.FromClrType(kvp.Key, kvp.Value, Math.Max(1, maxLengths[kvp.Key]));
            descriptors.Add(descriptor);
        }

        return descriptors;
    }

    private static bool IsNumeric(Type type)
    {
        return type == typeof(int) || type == typeof(long) || type == typeof(short) ||
               type == typeof(byte) || type == typeof(double) || type == typeof(float) ||
               type == typeof(decimal);
    }

    // Calculates the bounding box from all features.
    private static KoreLLBox CalculateBoundingBox(List<ShapefileFeature> features)
    {
        double minX = double.MaxValue, minY = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue;

        foreach (var feature in features)
        {
            var points = GetAllPoints(feature.Geometry);
            foreach (var point in points)
            {
                minX = Math.Min(minX, point.LonDegs);
                minY = Math.Min(minY, point.LatDegs);
                maxX = Math.Max(maxX, point.LonDegs);
                maxY = Math.Max(maxY, point.LatDegs);
            }
        }

        if (minX == double.MaxValue)
        {
            return new KoreLLBox { MinLonDegs = 0, MinLatDegs = 0, MaxLonDegs = 0, MaxLatDegs = 0 };
        }

        return new KoreLLBox
        {
            MinLonDegs = minX,
            MinLatDegs = minY,
            MaxLonDegs = maxX,
            MaxLatDegs = maxY
        };
    }

    // Gets all points from a geometry.
    private static List<KoreLLPoint> GetAllPoints(KoreGeoFeature? geometry)
    {
        var points = new List<KoreLLPoint>();
        if (geometry == null)
            return points;

        switch (geometry)
        {
            case KoreGeoPoint point:
                points.Add(point.Position);
                break;
            case KoreGeoMultiPoint multiPoint:
                points.AddRange(multiPoint.Points);
                break;
            case KoreGeoLineString lineString:
                points.AddRange(lineString.Points);
                break;
            case KoreGeoMultiLineString multiLine:
                foreach (var line in multiLine.LineStrings)
                    points.AddRange(line);
                break;
            case KoreGeoPolygon polygon:
                points.AddRange(polygon.OuterRing);
                foreach (var ring in polygon.InnerRings)
                    points.AddRange(ring);
                break;
            case KoreGeoMultiPolygon multiPolygon:
                foreach (var poly in multiPolygon.Polygons)
                {
                    points.AddRange(poly.OuterRing);
                    foreach (var ring in poly.InnerRings)
                        points.AddRange(ring);
                }
                break;
        }

        return points;
    }

    // Writes the SHP and SHX files.
    private static void WriteShpAndShx(string shpPath, string shxPath, ShapefileFeatureCollection collection, KoreLLBox? bbox)
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
    private static int WriteShpRecord(BinaryWriter writer, ShapefileFeature feature, int recordNumber)
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

    // Writes the DBF file.
    private static void WriteDbf(string dbfPath, List<ShapefileFeature> features, List<DbfFieldDescriptor> fields)
    {
        using var stream = new FileStream(dbfPath, FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(stream);

        // Calculate header size and record size
        int headerSize = 32 + (fields.Count * 32) + 1; // Header + field descriptors + terminator
        int recordSize = 1; // Deletion flag
        foreach (var field in fields)
        {
            recordSize += field.Length;
        }

        // DBF header
        writer.Write((byte)0x03); // dBASE III
        var now = DateTime.Now;
        writer.Write((byte)(now.Year - 1900));
        writer.Write((byte)now.Month);
        writer.Write((byte)now.Day);
        writer.Write(features.Count); // Number of records
        writer.Write((short)headerSize);
        writer.Write((short)recordSize);
        writer.Write(new byte[20]); // Reserved

        // Field descriptors
        foreach (var field in fields)
        {
            byte[] nameBytes = new byte[11];
            byte[] srcBytes = Encoding.ASCII.GetBytes(field.Name);
            Array.Copy(srcBytes, nameBytes, Math.Min(srcBytes.Length, 11));
            writer.Write(nameBytes);

            writer.Write((byte)field.FieldType);
            writer.Write(new byte[4]); // Reserved
            writer.Write((byte)field.Length);
            writer.Write((byte)field.DecimalCount);
            writer.Write(new byte[14]); // Reserved
        }

        // Header terminator
        writer.Write((byte)0x0D);

        // Records
        foreach (var feature in features)
        {
            // Deletion flag (space = not deleted)
            writer.Write((byte)0x20);

            // Field values
            foreach (var field in fields)
            {
                object? value = feature.Attributes.TryGetValue(field.Name, out var v) ? v : null;
                string strValue = FormatDbfValue(value, field);

                byte[] valueBytes = new byte[field.Length];
                byte[] srcBytes = Encoding.ASCII.GetBytes(strValue);
                int copyLen = Math.Min(srcBytes.Length, field.Length);

                if (field.FieldType == 'N' || field.FieldType == 'F')
                {
                    // Right-align numeric values
                    int startPos = field.Length - copyLen;
                    Array.Copy(srcBytes, 0, valueBytes, startPos, copyLen);
                    // Fill leading with spaces
                    for (int i = 0; i < startPos; i++)
                        valueBytes[i] = 0x20;
                }
                else
                {
                    // Left-align other values
                    Array.Copy(srcBytes, valueBytes, copyLen);
                    // Fill trailing with spaces
                    for (int i = copyLen; i < field.Length; i++)
                        valueBytes[i] = 0x20;
                }

                writer.Write(valueBytes);
            }
        }

        // EOF marker
        writer.Write((byte)0x1A);
    }

    // Formats an attribute value for DBF storage.
    private static string FormatDbfValue(object? value, DbfFieldDescriptor field)
    {
        if (value == null)
            return new string(' ', field.Length);

        switch (field.FieldType)
        {
            case 'N':
            case 'F':
                if (field.DecimalCount > 0)
                {
                    double dval = Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);
                    return dval.ToString($"F{field.DecimalCount}", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    long lval = Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture);
                    return lval.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }

            case 'L':
                bool bval = Convert.ToBoolean(value);
                return bval ? "T" : "F";

            case 'D':
                if (value is DateTime dt)
                    return dt.ToString("yyyyMMdd");
                return new string(' ', 8);

            default:
                return value.ToString() ?? string.Empty;
        }
    }

    // Formats an attribute value as a string for length calculation.
    private static string FormatAttributeValue(object value)
    {
        if (value is DateTime dt)
            return dt.ToString("yyyyMMdd");
        if (value is bool b)
            return b ? "T" : "F";
        if (value is double d)
            return d.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
        return value?.ToString() ?? string.Empty;
    }

    // Writes the PRJ file with WGS84 definition.
    private static void WritePrj(string prjPath)
    {
        File.WriteAllText(prjPath, Wgs84Prj);
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
