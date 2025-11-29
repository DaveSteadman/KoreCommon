// <fileheader>

#nullable enable

using System.Collections.Generic;

using KoreCommon;

namespace KoreGIS;

// Represents a single feature in a Shapefile, containing geometry and attributes.
// Maps to the existing KoreGeoFeature types for geometry representation.
public class KoreShapefileFeature
{
    // The record number from the Shapefile (1-based index).
    public int RecordNumber { get; set; }

    // The geometry type of this feature.
    public ShapefileGeometryType GeometryType { get; set; }

    // The underlying geometry object. Can be one of:
    // - KoreGeoPoint (for Point)
    // - KoreGeoMultiPoint (for MultiPoint)
    // - KoreGeoMultiLineString (for PolyLine - multiple parts)
    // - KoreGeoMultiPolygon (for Polygon - multiple parts with holes)
    public KoreGeoFeature? Geometry { get; set; }

    // Attributes from the DBF file, keyed by field name.
    // Values are typed appropriately: int, double, string, bool, DateTime, or null.
    public Dictionary<string, object?> Attributes { get; set; } = new Dictionary<string, object?>();

    // Bounding box of the geometry (min X, min Y, max X, max Y).
    // Coordinates are in the Shapefile's coordinate system (assumed WGS84).
    public KoreLLBox? BoundingBox { get; set; }
}

