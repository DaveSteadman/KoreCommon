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
    // Core storage - features indexed by name
    private Dictionary<string, KoreGeoFeature>         Features     = new Dictionary<string, KoreGeoFeature>();

    // Type-specific indexes for faster querying
    private Dictionary<string, KoreGeoPoint>           Points        = new Dictionary<string, KoreGeoPoint>();
    private Dictionary<string, KoreGeoMultiPoint>      MultiPoints   = new Dictionary<string, KoreGeoMultiPoint>();
    private Dictionary<string, KoreGeoLineString>      LineStrings   = new Dictionary<string, KoreGeoLineString>();
    private Dictionary<string, KoreGeoMultiLineString> MultiLines    = new Dictionary<string, KoreGeoMultiLineString>();
    private Dictionary<string, KoreGeoPolygon>         Polygons      = new Dictionary<string, KoreGeoPolygon>();
    private Dictionary<string, KoreGeoMultiPolygon>    MultiPolygons = new Dictionary<string, KoreGeoMultiPolygon>();
    private Dictionary<string, KoreGeoCircle>          Circles       = new Dictionary<string, KoreGeoCircle>();

    public string Name { get; set; } = string.Empty;

    // --------------------------------------------------------------------------------------------
    // MARK: Spatial Queries
    // --------------------------------------------------------------------------------------------

    // Get all points within a bounding box
    public IEnumerable<KoreGeoPoint> GetPointsInBox(KoreLLBox bounds)
    {
        return Points.Values.Where(p => bounds.Contains(p.Position));
    }

    // Get all features that intersect with a bounding box
    public IEnumerable<KoreGeoFeature> GetFeaturesInBox(KoreLLBox bounds)
    {
        var result = new List<KoreGeoFeature>();

        // Add points within bounds
        result.AddRange(GetPointsInBox(bounds));

        // Add line strings with any point in bounds
        foreach (var line in LineStrings.Values)
        {
            if (line.Points.Any(p => bounds.Contains(p)))
                result.Add(line);
        }

        foreach (var multiLine in MultiLines.Values)
        {
            if (multiLine.LineStrings.Any(line => line.Any(p => bounds.Contains(p))))
                result.Add(multiLine);
        }

        // Add polygons with any point in bounds
        foreach (var polygon in Polygons.Values)
        {
            if (polygon.OuterRing.Any(p => bounds.Contains(p)))
                result.Add(polygon);
        }

        foreach (var multiPolygon in MultiPolygons.Values)
        {
            if (multiPolygon.Polygons.Any(poly => poly.OuterRing.Any(p => bounds.Contains(p))))
                result.Add(multiPolygon);
        }

        // Add circles with center in bounds
        foreach (var circle in Circles.Values)
        {
            if (bounds.Contains(circle.Center))
                result.Add(circle);
        }

        foreach (var multiPoint in MultiPoints.Values)
        {
            if (multiPoint.Points.Any(bounds.Contains))
                result.Add(multiPoint);
        }

        return result;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Statistics
    // --------------------------------------------------------------------------------------------

    public int Count => Features.Count;

    // Calculate the bounding box that encompasses all features in the library
    // Used primarily for GeoJSON export metadata
    public KoreLLBox? LibraryBoundingBox()
    {
        if (Features.Count == 0)
            return null;

        var allPoints = new List<KoreLLPoint>();

        // Collect points from all feature types
        foreach (var point in Points.Values)
        {
            allPoints.Add(point.Position);
        }

        foreach (var multiPoint in MultiPoints.Values)
        {
            allPoints.AddRange(multiPoint.Points);
        }

        foreach (var line in LineStrings.Values)
        {
            allPoints.AddRange(line.Points);
        }

        foreach (var multiLine in MultiLines.Values)
        {
            foreach (var line in multiLine.LineStrings)
            {
                allPoints.AddRange(line);
            }
        }

        foreach (var polygon in Polygons.Values)
        {
            allPoints.AddRange(polygon.OuterRing);
            foreach (var innerRing in polygon.InnerRings)
            {
                allPoints.AddRange(innerRing);
            }
        }

        foreach (var multiPolygon in MultiPolygons.Values)
        {
            foreach (var polygon in multiPolygon.Polygons)
            {
                allPoints.AddRange(polygon.OuterRing);
                foreach (var innerRing in polygon.InnerRings)
                {
                    allPoints.AddRange(innerRing);
                }
            }
        }

        foreach (var circle in Circles.Values)
        {
            allPoints.Add(circle.Center);
        }

        // Use KoreLLBox.FromList to calculate the bounding box efficiently
        return allPoints.Count > 0 ? KoreLLBox.FromList(allPoints) : null;
    }

}

