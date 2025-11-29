// <fileheader>

using System;
using System.Collections.Generic;
using System.Linq;

using KoreCommon;

namespace KoreGIS;

// Library for managing and querying geographic features
// Provides storage, retrieval, and spatial filtering of geo features
public partial class KoreGeoFeatureLibrary
{
    // --------------------------------------------------------------------------------------------
    // MARK: Add/Remove Features
    // --------------------------------------------------------------------------------------------

    public void AddFeature(KoreGeoFeature feature)
    {
        if (string.IsNullOrEmpty(feature.Name))
        {
            throw new ArgumentException("Feature must have a name");
        }

        // Add to main collection
        Features[feature.Name] = feature;

        // Add to type-specific indexes
        switch (feature)
        {
            case KoreGeoPoint point:               Points[feature.Name]        = point;        break;
            case KoreGeoMultiPoint multiPoint:     MultiPoints[feature.Name]   = multiPoint;   break;
            case KoreGeoLineString lineString:     LineStrings[feature.Name]   = lineString;   break;
            case KoreGeoMultiLineString multiLine: MultiLines[feature.Name]    = multiLine;    break;
            case KoreGeoPolygon polygon:           Polygons[feature.Name]      = polygon;      break;
            case KoreGeoMultiPolygon multiPolygon: MultiPolygons[feature.Name] = multiPolygon; break;
            case KoreGeoCircle circle:             Circles[feature.Name]       = circle;       break;
        }
    }

    public void AddFeatures(IEnumerable<KoreGeoFeature> featuresToAdd)
    {
        foreach (var feature in featuresToAdd)
        {
            AddFeature(feature);
        }
    }

    public bool RemoveFeature(string name)
    {
        if (!Features.TryGetValue(name, out var feature))
            return false;

        Features.Remove(name);
        Points.Remove(name);
        MultiPoints.Remove(name);
        LineStrings.Remove(name);
        MultiLines.Remove(name);
        Polygons.Remove(name);
        MultiPolygons.Remove(name);
        Circles.Remove(name);

        return true;
    }

    public void Clear()
    {
        Features.Clear();
        Points.Clear();
        MultiPoints.Clear();
        LineStrings.Clear();
        MultiLines.Clear();
        Polygons.Clear();
        MultiPolygons.Clear();
        Circles.Clear();
    }


    internal string GenerateUniqueFeatureName(string baseName)
    {
        var candidate = (baseName ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(candidate))
        {
            candidate = "Feature";
        }

        if (GetFeature(candidate) is null)
        {
            return candidate;
        }

        var suffix = 2;
        var uniqueCandidate = candidate;
        while (GetFeature(uniqueCandidate) is not null)
        {
            uniqueCandidate = $"{candidate}_{suffix}";
            suffix++;
        }

        return uniqueCandidate;
    }


}

