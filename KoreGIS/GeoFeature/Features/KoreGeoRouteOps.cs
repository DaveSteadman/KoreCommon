// <fileheader>

#nullable enable

using System;
using System.Collections.Generic;

using KoreCommon;

namespace KoreGIS;

// Operations for editing and querying KoreGeoRoute objects
public static class KoreGeoRouteOps
{
    // --------------------------------------------------------------------------------------------
    // MARK: Waypoint Access
    // --------------------------------------------------------------------------------------------

    // Get all waypoints from a route, covering the first leg start and the end of every leg after that
    // route: Route to extract waypoints from
    // returns: List of waypoints in order
    public static List<KoreLLPoint> GetWaypoints(this KoreGeoRoute route)
    {
        var waypoints = new List<KoreLLPoint>();

        if (route.Legs.Count == 0)
            return waypoints;

        // First waypoint is the start of the first leg
        waypoints.Add(route.Legs[0].StartPoint);

        // Subsequent waypoints are the end points of each leg
        foreach (var leg in route.Legs)
        {
            waypoints.Add(leg.EndPoint);
        }

        return waypoints;
    }

    // Get a specific waypoint by index
    // route: Route being queried
    // waypointIndex: Zero-based waypoint index (0 = start of first leg)
    // returns: Waypoint at the specified index
    public static KoreLLPoint GetWaypoint(this KoreGeoRoute route, int waypointIndex)
    {
        if (route.Legs.Count == 0)
            throw new InvalidOperationException("Route has no legs");

        if (waypointIndex < 0 || waypointIndex >= route.WaypointCount)
            throw new ArgumentOutOfRangeException(nameof(waypointIndex),
                $"Waypoint index {waypointIndex} is out of range (0 to {route.WaypointCount - 1})");

        // Waypoint 0 is the start of the first leg
        if (waypointIndex == 0)
            return route.Legs[0].StartPoint;

        // Other waypoints are the end points of legs
        // Waypoint 1 = end of leg 0, waypoint 2 = end of leg 1, etc.
        return route.Legs[waypointIndex - 1].EndPoint;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Waypoint Editing
    // --------------------------------------------------------------------------------------------

    // Set a waypoint to a new position, updating the surrounding legs appropriately
    // route: Route to modify
    // waypointIndex: Zero-based waypoint index
    // newPosition: New position for the waypoint
    public static void SetWaypoint(this KoreGeoRoute route, int waypointIndex, KoreLLPoint newPosition)
    {
        if (route.Legs.Count == 0)
            throw new InvalidOperationException("Route has no legs");

        if (waypointIndex < 0 || waypointIndex >= route.WaypointCount)
            throw new ArgumentOutOfRangeException(nameof(waypointIndex),
                $"Waypoint index {waypointIndex} is out of range (0 to {route.WaypointCount - 1})");

        // Waypoint 0 is the start of the first leg
        if (waypointIndex == 0)
        {
            route.Legs[0].StartPoint = newPosition;

            // If this leg is a bezier, update its control points array too
            if (route.Legs[0] is KoreGeoRouteLegBezier bezier && bezier.ControlPoints.Count > 0)
            {
                bezier.ControlPoints[0] = newPosition;
            }

            // Invalidate bounding box for affected leg
            route.Legs[0].CalcBoundingBox();
        }
        // Last waypoint is the end of the last leg
        else if (waypointIndex == route.WaypointCount - 1)
        {
            int lastLegIndex = route.Legs.Count - 1;
            route.Legs[lastLegIndex].EndPoint = newPosition;

            // If this leg is a bezier, update its control points array too
            if (route.Legs[lastLegIndex] is KoreGeoRouteLegBezier bezier && bezier.ControlPoints.Count > 0)
            {
                bezier.ControlPoints[bezier.ControlPoints.Count - 1] = newPosition;
            }

            // Invalidate bounding box for affected leg
            route.Legs[lastLegIndex].CalcBoundingBox();
        }
        // Middle waypoints affect two legs
        else
        {
            int prevLegIndex = waypointIndex - 1;
            int nextLegIndex = waypointIndex;

            // Update the end of the previous leg
            route.Legs[prevLegIndex].EndPoint = newPosition;
            if (route.Legs[prevLegIndex] is KoreGeoRouteLegBezier prevBezier && prevBezier.ControlPoints.Count > 0)
            {
                prevBezier.ControlPoints[prevBezier.ControlPoints.Count - 1] = newPosition;
            }

            // Update the start of the next leg
            route.Legs[nextLegIndex].StartPoint = newPosition;
            if (route.Legs[nextLegIndex] is KoreGeoRouteLegBezier nextBezier && nextBezier.ControlPoints.Count > 0)
            {
                nextBezier.ControlPoints[0] = newPosition;
            }

            // For FlexibleJoin legs, invalidate their calculated control points
            if (route.Legs[prevLegIndex] is KoreGeoRouteLegFlexibleJoin prevFlexJoin)
            {
                // Force recalculation by clearing the cached control points
                prevFlexJoin.GetControlPoints(); // This will trigger recalculation
            }

            if (route.Legs[nextLegIndex] is KoreGeoRouteLegFlexibleJoin nextFlexJoin)
            {
                // Force recalculation by clearing the cached control points
                nextFlexJoin.GetControlPoints(); // This will trigger recalculation
            }

            // Invalidate bounding boxes for affected legs
            route.Legs[prevLegIndex].CalcBoundingBox();
            route.Legs[nextLegIndex].CalcBoundingBox();
        }

        // Invalidate route's bounding box
        route.CalcBoundingBox();
    }

    // Set all waypoints at once; list must match the route's waypoint count
    // route: Route to modify
    // waypoints: New waypoint positions
    public static void SetWaypoints(this KoreGeoRoute route, List<KoreLLPoint> waypoints)
    {
        if (route.Legs.Count == 0)
            throw new InvalidOperationException("Route has no legs");

        if (waypoints.Count != route.WaypointCount)
            throw new ArgumentException(
                $"Waypoint count mismatch: expected {route.WaypointCount}, got {waypoints.Count}",
                nameof(waypoints));

        // Apply each waypoint
        for (int i = 0; i < waypoints.Count; i++)
        {
            SetWaypoint(route, i, waypoints[i]);
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Waypoint Info
    // --------------------------------------------------------------------------------------------

    // Get information about which legs are connected to a waypoint
    // route: Route being queried
    // waypointIndex: Zero-based waypoint index
    // returns: Tuple of (previous leg index or -1, next leg index or -1)
    public static (int prevLegIndex, int nextLegIndex) GetWaypointConnections(this KoreGeoRoute route, int waypointIndex)
    {
        if (route.Legs.Count == 0)
            throw new InvalidOperationException("Route has no legs");

        if (waypointIndex < 0 || waypointIndex >= route.WaypointCount)
            throw new ArgumentOutOfRangeException(nameof(waypointIndex));

        // First waypoint: no previous leg, next is leg 0
        if (waypointIndex == 0)
            return (-1, 0);

        // Last waypoint: previous is last leg, no next leg
        if (waypointIndex == route.WaypointCount - 1)
            return (route.Legs.Count - 1, -1);

        // Middle waypoints: previous leg ends here, next leg starts here
        return (waypointIndex - 1, waypointIndex);
    }

    // Check if moving a waypoint will affect FlexibleJoin legs and return affected leg indices
    // route: Route to inspect
    // waypointIndex: Zero-based waypoint index
    // returns: List of FlexibleJoin leg indices impacted by the waypoint
    public static List<int> GetAffectedFlexibleJoins(this KoreGeoRoute route, int waypointIndex)
    {
        var affected = new List<int>();

        if (route.Legs.Count == 0)
            return affected;

        var (prevLegIndex, nextLegIndex) = GetWaypointConnections(route, waypointIndex);

        // Check previous leg
        if (prevLegIndex >= 0 && route.Legs[prevLegIndex] is KoreGeoRouteLegFlexibleJoin)
            affected.Add(prevLegIndex);

        // Check next leg
        if (nextLegIndex >= 0 && route.Legs[nextLegIndex] is KoreGeoRouteLegFlexibleJoin)
            affected.Add(nextLegIndex);

        // Also check legs adjacent to FlexibleJoins (since FlexibleJoin depends on prev/next legs)
        if (prevLegIndex > 0 && route.Legs[prevLegIndex - 1] is KoreGeoRouteLegFlexibleJoin)
            affected.Add(prevLegIndex - 1);

        if (nextLegIndex >= 0 && nextLegIndex < route.Legs.Count - 1 &&
            route.Legs[nextLegIndex + 1] is KoreGeoRouteLegFlexibleJoin)
            affected.Add(nextLegIndex + 1);

        return affected;
    }
}

