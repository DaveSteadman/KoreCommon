// <fileheader>

#nullable enable

using System.Collections.Generic;

namespace KoreCommon;

/// <summary>
/// Represents a single feature in a Shapefile, containing geometry and attributes.
/// Maps to the existing KoreGeoFeature types for geometry representation.
/// </summary>
public class ShapefileFeature
{
    /// <summary>
    /// The record number from the Shapefile (1-based index).
    /// </summary>
    public int RecordNumber { get; set; }

    /// <summary>
    /// The geometry type of this feature.
    /// </summary>
    public ShapefileGeometryType GeometryType { get; set; }

    /// <summary>
    /// The underlying geometry object. Can be one of:
    /// - KoreGeoPoint (for Point)
    /// - KoreGeoMultiPoint (for MultiPoint)
    /// - KoreGeoMultiLineString (for PolyLine - multiple parts)
    /// - KoreGeoMultiPolygon (for Polygon - multiple parts with holes)
    /// </summary>
    public KoreGeoFeature? Geometry { get; set; }

    /// <summary>
    /// Attributes from the DBF file, keyed by field name.
    /// Values are typed appropriately: int, double, string, bool, DateTime, or null.
    /// </summary>
    public Dictionary<string, object?> Attributes { get; set; } = new Dictionary<string, object?>();

    /// <summary>
    /// Bounding box of the geometry (min X, min Y, max X, max Y).
    /// Coordinates are in the Shapefile's coordinate system (assumed WGS84).
    /// </summary>
    public KoreLLBox? BoundingBox { get; set; }
}
