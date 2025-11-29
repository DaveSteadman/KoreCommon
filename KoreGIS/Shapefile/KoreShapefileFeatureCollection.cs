// <fileheader>

#nullable enable

using System.Collections.Generic;

using KoreCommon;

namespace KoreGIS;

// Represents a collection of features from a Shapefile.
// Corresponds to the contents of a Shapefile layer (.shp, .shx, .dbf, .prj).
public class KoreShapefileFeatureCollection
{
    // The geometry type for all features in this collection.
    // Shapefiles require all features to have the same geometry type.
    public ShapefileGeometryType GeometryType { get; set; }

    // The list of features in this collection.
    public List<KoreShapefileFeature> Features { get; set; } = new List<KoreShapefileFeature>();

    // Bounding box encompassing all features in the collection.
    public KoreLLBox? BoundingBox { get; set; }

    // The projection definition (contents of the .prj file), if available.
    public string? ProjectionWkt { get; set; }

    // Field definitions from the DBF file.
    public List<KoreDbfFieldDescriptor> FieldDescriptors { get; set; } = new List<KoreDbfFieldDescriptor>();

    // Warnings collected during import (e.g., skipped records, projection mismatch).
    public List<string> Warnings { get; set; } = new List<string>();

    // Converts this ShapefileFeatureCollection to a KoreGeoFeatureCollection
    // for interoperability with the existing GeoJSON infrastructure.
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

    // Creates a ShapefileFeatureCollection from a KoreGeoFeatureCollection.
    public static KoreShapefileFeatureCollection FromGeoFeatureCollection(KoreGeoFeatureCollection geoCollection, ShapefileGeometryType geometryType)
    {
        var result = new KoreShapefileFeatureCollection
        {
            GeometryType = geometryType,
            BoundingBox = geoCollection.BoundingBox
        };

        int recordNumber = 1;
        foreach (var feature in geoCollection.Features)
        {
            var shpFeature = new KoreShapefileFeature
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

