// <fileheader>

#nullable enable

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
    // MARK: Basic Queries
    // --------------------------------------------------------------------------------------------

    public KoreGeoFeature?          GetFeature(string name)          { Features.TryGetValue(name, out var feature);       return feature; }
    public KoreGeoPoint?            GetPoint(string name)            { Points.TryGetValue(name, out var point);           return point; }
    public KoreGeoMultiPoint?       GetMultiPoint(string name)       { MultiPoints.TryGetValue(name, out var multiPoint); return multiPoint; }
    public KoreGeoLineString?       GetLineString(string name)       { LineStrings.TryGetValue(name, out var lineString); return lineString; }
    public KoreGeoMultiLineString?  GetMultiLineString(string name)  { MultiLines.TryGetValue(name, out var multiLine);   return multiLine; }
    public KoreGeoPolygon?          GetPolygon(string name)          { Polygons.TryGetValue(name, out var polygon);       return polygon; }
    public KoreGeoMultiPolygon?     GetMultiPolygon(string name)     { MultiPolygons.TryGetValue(name, out var mulPoly);  return mulPoly; }
    public KoreGeoCircle?           GetCircle(string name)           { Circles.TryGetValue(name, out var circle);         return circle; }

    public List<KoreGeoFeature>         GetAllFeatures()         { return Features.Values.ToList(); }
    public List<KoreGeoPoint>           GetAllPoints()           { return Points.Values.ToList(); }
    public List<KoreGeoMultiPoint>      GetAllMultiPoints()      { return MultiPoints.Values.ToList(); }
    public List<KoreGeoLineString>      GetAllLineStrings()      { return LineStrings.Values.ToList(); }
    public List<KoreGeoMultiLineString> GetAllMultiLineStrings() { return MultiLines.Values.ToList(); }
    public List<KoreGeoPolygon>         GetAllPolygons()         { return Polygons.Values.ToList(); }
    public List<KoreGeoMultiPolygon>    GetAllMultiPolygons()    { return MultiPolygons.Values.ToList(); }
    public List<KoreGeoCircle>          GetAllCircles()          { return Circles.Values.ToList(); }

    // --------------------------------------------------------------------------------------------
    // MARK: Complex Queries
    // --------------------------------------------------------------------------------------------

    public List<KoreGeoFeature> GetFeaturesWithPropertyName(string propertyName)
    {
        return Features.Values.Where(f =>
            f.Properties.ContainsKey(propertyName) &&
            f.Properties[propertyName] != null &&
            f.Properties[propertyName].ToString() != string.Empty).ToList();
    }

    public List<KoreGeoFeature> GetFeaturesWithPropertyValue(string propertyName, object value)
    {
        return Features.Values.Where(f =>
            f.Properties.ContainsKey(propertyName) &&
            f.Properties[propertyName] != null &&
            f.Properties[propertyName].Equals(value)).ToList();
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Filters
    // --------------------------------------------------------------------------------------------

    // Takes a collection of features and returns only those that intersect with the given bounding box

    public static List<KoreGeoFeature> FilterFeaturesByIntersectingBBox(IEnumerable<KoreGeoFeature> featuresToFilter, KoreLLBox bounds)
    {
        var result = new List<KoreGeoFeature>();

        foreach (var feature in featuresToFilter)
        {
            switch (feature)
            {
                case KoreGeoPoint point:
                    if (bounds.Contains(point.Position))
                        result.Add(feature);
                    break;
                case KoreGeoMultiPoint multiPoint:
                    if (multiPoint.Points.Any(p => bounds.Contains(p)))
                        result.Add(feature);
                    break;
                case KoreGeoLineString lineString:
                    if (lineString.Points.Any(p => bounds.Contains(p)))
                        result.Add(feature);
                    break;
                case KoreGeoMultiLineString multiLine:
                    if (multiLine.LineStrings.Any(line => line.Any(p => bounds.Contains(p))))
                        result.Add(feature);
                    break;
                case KoreGeoPolygon polygon:
                    if (polygon.OuterRing.Any(p => bounds.Contains(p)))
                        result.Add(feature);
                    break;
                case KoreGeoMultiPolygon multiPolygon:
                    if (multiPolygon.Polygons.Any(poly => poly.OuterRing.Any(p => bounds.Contains(p))))
                        result.Add(feature);
                    break;
                case KoreGeoCircle circle:
                    // Simple bounding box check for circle center
                    if (bounds.Contains(circle.Center))
                        result.Add(feature);
                    break;
            }
        }
        return result;
    }

    // --------------------------------------------------------------------------------------------

    public static List<KoreGeoFeature> FilterFeaturesByPropertyName(List<KoreGeoFeature> featuresToFilter, string propertyName)
    {
        var result = new List<KoreGeoFeature>();
        foreach (var feature in featuresToFilter)
        {
            if (feature.Properties.ContainsKey(propertyName) &&
                feature.Properties[propertyName] != null &&
                feature.Properties[propertyName].ToString() != string.Empty)
            {
                result.Add(feature);
            }
        }
        return result;
    }

    public static List<KoreGeoFeature> FilterFeaturesByPropertyValue(List<KoreGeoFeature> featuresToFilter, string propertyName, object value)
    {
        var result = new List<KoreGeoFeature>();
        foreach (var feature in featuresToFilter)
        {
            if (feature.Properties.ContainsKey(propertyName) &&
                feature.Properties[propertyName] != null &&
                feature.Properties[propertyName].Equals(value))
            {
                result.Add(feature);
            }
        }
        return result;
    }

}

