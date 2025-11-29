// <fileheader>

#nullable enable

using System;
using System.IO;

using KoreCommon;

namespace KoreGIS;

// Partial class for reading PRJ (projection) files
public static partial class KoreShapefileReader
{
    // Reads the projection file (.prj) if it exists.
    private static void ReadPrjFile(string prjPath, KoreShapefileFeatureCollection collection)
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
}

