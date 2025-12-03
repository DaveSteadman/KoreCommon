// <fileheader>

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using KoreCommon;

namespace KoreGIS;

// GeoJSONKore extensions for KoreGeoRoute
// Provides serialization to/from JSON in a format compatible with GeoJSON
// Extended with Kore-specific properties for curved route legs
public static class KoreGeoRouteGeoJSONKore
{
    // --------------------------------------------------------------------------------------------
    // MARK: Export to GeoJSONKore
    // --------------------------------------------------------------------------------------------

    // Export a route to GeoJSONKore format
    // Creates a FeatureCollection with LineString geometries and Kore-specific properties
    public static string ToGeoJSONKore(this KoreGeoRoute route, int pointsPerLeg = 50)
    {
        var features = new List<object>();

        // Export each leg as a separate feature with metadata
        for (int i = 0; i < route.Legs.Count; i++)
        {
            var leg = route.Legs[i];
            var legPoints = leg.GeneratePoints(pointsPerLeg);

            // Build properties object with leg metadata
            var properties = new Dictionary<string, object>
            {
                { "leg_index", i },
                { "leg_type", leg.GetType().Name },
                { "line_width", leg.LineWidth },
                { "color_r", leg.Color.R },
                { "color_g", leg.Color.G },
                { "color_b", leg.Color.B }
            };

            // Add leg-specific properties
            switch (leg)
            {
                case KoreGeoRouteLegStraight:
                    properties["geometry_type"] = "straight";
                    break;

                case KoreGeoRouteLegGreatCircle:
                    properties["geometry_type"] = "great_circle";
                    break;

                case KoreGeoRouteLegBezier bezier:
                    properties["geometry_type"] = "bezier";
                    properties["control_point_count"] = bezier.ControlPoints.Count;
                    // Store control points as separate coordinates
                    var controlPointCoords = new List<double[]>();
                    foreach (var cp in bezier.ControlPoints)
                    {
                        controlPointCoords.Add(new[] { cp.LonDegs, cp.LatDegs });
                    }
                    properties["control_points"] = controlPointCoords;
                    break;

                case KoreGeoRouteLegFlexibleJoin flexJoin:
                    properties["geometry_type"] = "flexible_join";
                    properties["start_control_distance_m"] = flexJoin.StartControlPointDistanceM;
                    properties["end_control_distance_m"] = flexJoin.EndControlPointDistanceM;
                    // Store calculated control points
                    var calculatedPoints = flexJoin.GetControlPoints();
                    var calcPointCoords = new List<double[]>();
                    foreach (var cp in calculatedPoints)
                    {
                        calcPointCoords.Add(new[] { cp.LonDegs, cp.LatDegs });
                    }
                    properties["calculated_control_points"] = calcPointCoords;
                    break;
            }

            // Add feature properties from base KoreGeoFeature
            if (!string.IsNullOrEmpty(leg.Name))
                properties["name"] = leg.Name;

            if (!string.IsNullOrEmpty(leg.Id))
                properties["id"] = leg.Id;

            foreach (var prop in leg.Properties)
            {
                properties[$"user_{prop.Key}"] = prop.Value;
            }

            // Create LineString geometry with generated points
            var geometry = new
            {
                type = "LineString",
                coordinates = legPoints.ConvertAll(p => new[] { p.LonDegs, p.LatDegs })
            };

            var feature = new
            {
                type = "Feature",
                properties,
                geometry
            };

            features.Add(feature);
        }

        // Calculate bounding box for the entire route
        var allRoutePoints = route.GeneratePoints(20);
        KoreLLBox? bbox = allRoutePoints.Count > 0 ? KoreLLBox.FromList(allRoutePoints) : (KoreLLBox?)null;

        // Build route-level properties
        var routeProperties = new Dictionary<string, object>
        {
            { "route_name", route.Name },
            { "route_leg_count", route.Legs.Count },
            { "route_waypoint_count", route.WaypointCount },
            { "route_line_width", route.LineWidth },
            { "route_color_r", route.Color.R },
            { "route_color_g", route.Color.G },
            { "route_color_b", route.Color.B },
            { "kore_type", "route" },
            { "kore_version", "1.0" }
        };

        if (!string.IsNullOrEmpty(route.Id))
            routeProperties["route_id"] = route.Id;

        foreach (var prop in route.Properties)
        {
            routeProperties[$"route_{prop.Key}"] = prop.Value;
        }

        // Build the FeatureCollection
        object featureCollection;
        if (bbox.HasValue)
        {
            featureCollection = new
            {
                type = "FeatureCollection",
                bbox = new[] { bbox.Value.MinLonDegs, bbox.Value.MinLatDegs, bbox.Value.MaxLonDegs, bbox.Value.MaxLatDegs },
                properties = routeProperties,
                features
            };
        }
        else
        {
            featureCollection = new
            {
                type = "FeatureCollection",
                properties = routeProperties,
                features
            };
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        return JsonSerializer.Serialize(featureCollection, options);
    }

    // Save a route to a GeoJSONKore file
    public static void SaveToGeoJSONKore(this KoreGeoRoute route, string filePath, int pointsPerLeg = 50)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        var json = route.ToGeoJSONKore(pointsPerLeg);
        File.WriteAllText(filePath, json);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Import from GeoJSONKore
    // --------------------------------------------------------------------------------------------

    // Import a route from GeoJSONKore format
    // Reconstructs curved legs from stored metadata
    public static KoreGeoRoute FromGeoJSONKore(string geoJsonKoreString)
    {
        if (string.IsNullOrWhiteSpace(geoJsonKoreString))
            throw new ArgumentException("GeoJSONKore string cannot be null or empty", nameof(geoJsonKoreString));

        using var document = JsonDocument.Parse(geoJsonKoreString);
        var root = document.RootElement;

        var type = root.GetProperty("type").GetString();
        if (!string.Equals(type, "FeatureCollection", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Expected FeatureCollection, got {type}");

        var route = new KoreGeoRoute();

        // Parse route-level properties
        if (root.TryGetProperty("properties", out var routeProps))
        {
            if (routeProps.TryGetProperty("route_name", out var routeName))
                route.Name = routeName.GetString() ?? string.Empty;

            if (routeProps.TryGetProperty("route_id", out var routeId))
                route.Id = routeId.GetString();

            if (routeProps.TryGetProperty("route_line_width", out var routeLineWidth))
                route.LineWidth = routeLineWidth.GetDouble();

            // Parse route color
            if (routeProps.TryGetProperty("route_color_r", out var r) &&
                routeProps.TryGetProperty("route_color_g", out var g) &&
                routeProps.TryGetProperty("route_color_b", out var b))
            {
                route.Color = new KoreColorRGB((byte)r.GetInt32(), (byte)g.GetInt32(), (byte)b.GetInt32());
            }

            // Parse user properties (prefixed with route_)
            foreach (var prop in routeProps.EnumerateObject())
            {
                if (prop.Name.StartsWith("route_") &&
                    !prop.Name.StartsWith("route_name") &&
                    !prop.Name.StartsWith("route_id") &&
                    !prop.Name.StartsWith("route_leg") &&
                    !prop.Name.StartsWith("route_waypoint") &&
                    !prop.Name.StartsWith("route_line") &&
                    !prop.Name.StartsWith("route_color"))
                {
                    string key = prop.Name.Substring(6); // Remove "route_" prefix
                    route.Properties[key] = prop.Value.ToString();
                }
            }
        }

        // Parse features (legs)
        if (!root.TryGetProperty("features", out var featuresElement))
            throw new InvalidDataException("FeatureCollection missing features array");

        var legsList = new List<(int index, KoreGeoRouteLeg leg)>();

        foreach (var featureElement in featuresElement.EnumerateArray())
        {
            if (!featureElement.TryGetProperty("properties", out var props))
                continue;

            if (!props.TryGetProperty("leg_index", out var legIndexProp))
                continue;

            int legIndex = legIndexProp.GetInt32();

            // Parse geometry to get start and end points
            if (!featureElement.TryGetProperty("geometry", out var geometry))
                continue;

            if (!geometry.TryGetProperty("coordinates", out var coords))
                continue;

            var coordArray = coords.EnumerateArray().ToList();
            if (coordArray.Count < 2)
                continue;

            var firstCoord = coordArray[0].EnumerateArray().ToList();
            var lastCoord = coordArray[coordArray.Count - 1].EnumerateArray().ToList();

            var startPoint = new KoreLLPoint { LonDegs = firstCoord[0].GetDouble(), LatDegs = firstCoord[1].GetDouble() };
            var endPoint = new KoreLLPoint { LonDegs = lastCoord[0].GetDouble(), LatDegs = lastCoord[1].GetDouble() };

            // Get leg type and create appropriate leg
            KoreGeoRouteLeg? leg = null;

            if (props.TryGetProperty("geometry_type", out var geomType))
            {
                string geometryType = geomType.GetString() ?? "straight";

                switch (geometryType)
                {
                    case "straight":
                        leg = new KoreGeoRouteLegStraight
                        {
                            StartPoint = startPoint,
                            EndPoint = endPoint
                        };
                        break;

                    case "great_circle":
                        leg = new KoreGeoRouteLegGreatCircle
                        {
                            StartPoint = startPoint,
                            EndPoint = endPoint
                        };
                        break;

                    case "bezier":
                        var bezierLeg = new KoreGeoRouteLegBezier
                        {
                            StartPoint = startPoint,
                            EndPoint = endPoint
                        };

                        // Reconstruct control points
                        if (props.TryGetProperty("control_points", out var controlPointsProp))
                        {
                            var controlPoints = new List<KoreLLPoint>();
                            foreach (var cpCoord in controlPointsProp.EnumerateArray())
                            {
                                var cpArray = cpCoord.EnumerateArray().ToList();
                                controlPoints.Add(new KoreLLPoint { LonDegs = cpArray[0].GetDouble(), LatDegs = cpArray[1].GetDouble() });
                            }
                            bezierLeg.SetControlPoints(controlPoints);
                        }

                        leg = bezierLeg;
                        break;

                    case "flexible_join":
                        var flexJoin = new KoreGeoRouteLegFlexibleJoin
                        {
                            StartPoint = startPoint,
                            EndPoint = endPoint
                        };

                        if (props.TryGetProperty("start_control_distance_m", out var startDist))
                            flexJoin.StartControlPointDistanceM = startDist.GetDouble();

                        if (props.TryGetProperty("end_control_distance_m", out var endDist))
                            flexJoin.EndControlPointDistanceM = endDist.GetDouble();

                        // Note: PreviousLeg and NextLeg will need to be set after all legs are loaded
                        leg = flexJoin;
                        break;
                }
            }

            if (leg == null)
            {
                // Default to straight line
                leg = new KoreGeoRouteLegStraight
                {
                    StartPoint = startPoint,
                    EndPoint = endPoint
                };
            }

            // Parse common leg properties
            if (props.TryGetProperty("name", out var nameProp))
                leg.Name = nameProp.GetString() ?? string.Empty;

            if (props.TryGetProperty("id", out var idProp))
                leg.Id = idProp.GetString();

            if (props.TryGetProperty("line_width", out var lineWidth))
                leg.LineWidth = lineWidth.GetDouble();

            if (props.TryGetProperty("color_r", out var legR) &&
                props.TryGetProperty("color_g", out var legG) &&
                props.TryGetProperty("color_b", out var legB))
            {
                leg.Color = new KoreColorRGB((byte)legR.GetInt32(), (byte)legG.GetInt32(), (byte)legB.GetInt32());
            }

            // Parse user properties (prefixed with user_)
            foreach (var prop in props.EnumerateObject())
            {
                if (prop.Name.StartsWith("user_"))
                {
                    string key = prop.Name.Substring(5); // Remove "user_" prefix
                    leg.Properties[key] = prop.Value.ToString();
                }
            }

            legsList.Add((legIndex, leg));
        }

        // Sort legs by index and add to route
        legsList.Sort((a, b) => a.index.CompareTo(b.index));
        foreach (var (_, leg) in legsList)
        {
            route.AddLeg(leg);
        }

        // Wire up flexible joins with their previous and next legs
        for (int i = 0; i < route.Legs.Count; i++)
        {
            if (route.Legs[i] is KoreGeoRouteLegFlexibleJoin flexJoin)
            {
                if (i > 0)
                    flexJoin.PreviousLeg = route.Legs[i - 1];

                if (i < route.Legs.Count - 1)
                    flexJoin.NextLeg = route.Legs[i + 1];
            }
        }

        // Calculate bounding boxes for each leg (must be after wiring up FlexibleJoins)
        foreach (var leg in route.Legs)
        {
            leg.CalcBoundingBox();
        }

        // Calculate bounding box for entire route
        route.CalcBoundingBox();

        return route;
    }

    // Load a route from a GeoJSONKore file
    public static KoreGeoRoute LoadFromGeoJSONKore(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("GeoJSONKore file not found", filePath);

        var json = File.ReadAllText(filePath);
        return FromGeoJSONKore(json);
    }
}

