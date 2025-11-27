// <fileheader>

#nullable enable

using System.Collections.Generic;

namespace KoreCommon;

/// <summary>
/// Represents a collection of features from a Shapefile.
/// Corresponds to the contents of a Shapefile layer (.shp, .shx, .dbf, .prj).
/// </summary>
public class ShapefileFeatureCollection
{
    /// <summary>
    /// The geometry type for all features in this collection.
    /// Shapefiles require all features to have the same geometry type.
    /// </summary>
    public ShapefileGeometryType GeometryType { get; set; }

    /// <summary>
    /// The list of features in this collection.
    /// </summary>
    public List<ShapefileFeature> Features { get; set; } = new List<ShapefileFeature>();

    /// <summary>
    /// Bounding box encompassing all features in the collection.
    /// </summary>
    public KoreLLBox? BoundingBox { get; set; }

    /// <summary>
    /// The projection definition (contents of the .prj file), if available.
    /// </summary>
    public string? ProjectionWkt { get; set; }

    /// <summary>
    /// Field definitions from the DBF file.
    /// </summary>
    public List<DbfFieldDescriptor> FieldDescriptors { get; set; } = new List<DbfFieldDescriptor>();

    /// <summary>
    /// Warnings collected during import (e.g., skipped records, projection mismatch).
    /// </summary>
    public List<string> Warnings { get; set; } = new List<string>();

    /// <summary>
    /// Converts this ShapefileFeatureCollection to a KoreGeoFeatureCollection
    /// for interoperability with the existing GeoJSON infrastructure.
    /// </summary>
    public KoreGeoFeatureCollection ToGeoFeatureCollection()
    {
        var result = new KoreGeoFeatureCollection
        {
            BoundingBox = BoundingBox
        };

        foreach (var feature in Features)
        {
            if (feature.Geometry != null)
            {
                // Copy attributes to properties
                foreach (var attr in feature.Attributes)
                {
                    if (attr.Value != null)
                    {
                        feature.Geometry.Properties[attr.Key] = attr.Value;
                    }
                }
                result.Features.Add(feature.Geometry);
            }
        }

        return result;
    }

    /// <summary>
    /// Creates a ShapefileFeatureCollection from a KoreGeoFeatureCollection.
    /// </summary>
    public static ShapefileFeatureCollection FromGeoFeatureCollection(KoreGeoFeatureCollection geoCollection, ShapefileGeometryType geometryType)
    {
        var result = new ShapefileFeatureCollection
        {
            GeometryType = geometryType,
            BoundingBox = geoCollection.BoundingBox
        };

        int recordNumber = 1;
        foreach (var feature in geoCollection.Features)
        {
            var shpFeature = new ShapefileFeature
            {
                RecordNumber = recordNumber++,
                GeometryType = geometryType,
                Geometry = feature
            };

            // Copy properties to attributes
            foreach (var prop in feature.Properties)
            {
                shpFeature.Attributes[prop.Key] = prop.Value;
            }

            result.Features.Add(shpFeature);
        }

        return result;
    }
}
