// <fileheader>

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;

using KoreCommon;

namespace KoreGIS;

// Writes ESRI Shapefiles (.shp, .shx, .dbf, .prj) from a ShapefileFeatureCollection.
// File-specific handling is in partial class files:
// - ShapefileWriter.Shp.cs for geometry
// - ShapefileWriter.Dbf.cs for attributes
// - ShapefileWriter.Prj.cs for projection
public static partial class KoreShapefileWriter
{
    // Writes a ShapefileFeatureCollection to the given path.
    // path: Path to the .shp file or base path without extension.
    // collection: The feature collection to write.
    public static void Write(string path, KoreShapefileFeatureCollection collection)
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

    // Calculates the bounding box from all features.
    private static KoreLLBox CalculateBoundingBox(List<KoreShapefileFeature> features)
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
}

