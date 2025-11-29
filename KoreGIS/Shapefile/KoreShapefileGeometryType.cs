// <fileheader>

#nullable enable

using KoreCommon;

namespace KoreGIS;

// ESRI Shapefile geometry types as defined in the Shapefile specification.
public enum ShapefileGeometryType
{
    Null = 0,
    Point = 1,
    PolyLine = 3,
    Polygon = 5,
    MultiPoint = 8,
    PointZ = 11,
    PolyLineZ = 13,
    PolygonZ = 15,
    MultiPointZ = 18,
    PointM = 21,
    PolyLineM = 23,
    PolygonM = 25,
    MultiPointM = 28
}

