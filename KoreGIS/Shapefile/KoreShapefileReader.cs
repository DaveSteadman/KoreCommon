// <fileheader>

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;

using KoreCommon;

namespace KoreGIS;

// Reads ESRI Shapefiles (.shp, .shx, .dbf, .prj) into a ShapefileFeatureCollection.
// File-specific handling is in partial class files:
// - ShapefileReader.Shp.cs for geometry
// - ShapefileReader.Dbf.cs for attributes
// - ShapefileReader.Prj.cs for projection
public static partial class KoreShapefileReader
{
    private const int ShapefileFileCode = 9994;
    private const int ShapefileVersion = 1000;

    // Reads a Shapefile from the given path.
    // path: Path to the .shp file or base path without extension.
    // Returns a ShapefileFeatureCollection containing all features.
    public static KoreShapefileFeatureCollection Read(string path)
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
            throw new KoreShapefileException($"Shapefile not found: {shpPath}");

        var collection = new KoreShapefileFeatureCollection();

        // Read PRJ file first (optional)
        ReadPrjFile(prjPath, collection);

        // Read DBF file for attributes (if exists)
        var attributes = new List<Dictionary<string, object?>>();
        var fieldDescriptors = new List<KoreDbfFieldDescriptor>();
        if (File.Exists(dbfPath))
        {
            ReadDbfFile(dbfPath, attributes, fieldDescriptors, collection);
            collection.FieldDescriptors = fieldDescriptors;
        }

        // Read SHP file for geometries
        ReadShpFile(shpPath, collection, attributes);

        return collection;
    }

    // Finds a file with the given extension, handling case-insensitive matching.
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
}

