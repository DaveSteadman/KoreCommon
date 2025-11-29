// <fileheader>

using System;
using System.Collections.Generic;

using KoreCommon;

namespace KoreGIS;

// A common class that represents a route. Each leg on the route could be from a separate
// class, so we will use common interface for each, but their implementation will be different.

// Design Decisions:
// - We will use a list of legs to represent the route.

public class KoreRoute
{
    public List<IKoreRouteLeg> Legs;

    // --------------------------------------------------------------------------------------------
    // MARK: Constructors
    // --------------------------------------------------------------------------------------------

    public KoreRoute()
    {
        Legs = new List<IKoreRouteLeg>();
    }

    public KoreRoute(List<IKoreRouteLeg> legs)
    {
        Legs = legs;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Basic Leg Management
    // --------------------------------------------------------------------------------------------

    public int NumLegs() => Legs.Count;
    public void AppendLeg(IKoreRouteLeg leg) => Legs.Add(leg);
    public void InsertLeg(int index, IKoreRouteLeg leg) => Legs.Insert(index, leg);
    public void RemoveLeg(int index) => Legs.RemoveAt(index);
    public void ClearLegs() => Legs.Clear();

    public IKoreRouteLeg GetLeg(int index) => Legs[index];
    public IKoreRouteLeg LastLeg() => Legs[Legs.Count - 1];

    // --------------------------------------------------------------------------------------------
    // MARK: Route traversal
    // --------------------------------------------------------------------------------------------

    public double TotalStraightLineDistanceM()
    {
        double summedDistanceM = 0;
        foreach (IKoreRouteLeg leg in Legs)
            summedDistanceM += leg.GetStraightLineDistanceM();
        return summedDistanceM;
    }

    public double TotalCalculatedDistanceM()
    {
        double summedCalculatedDistanceM = 0;
        foreach (IKoreRouteLeg leg in Legs)
            summedCalculatedDistanceM += leg.GetCalculatedDistanceM();
        return summedCalculatedDistanceM;
    }

    public double TotalDurationSeconds()
    {
        double summedDurationSeconds = 0;
        foreach (IKoreRouteLeg leg in Legs)
            summedDurationSeconds += leg.GetDurationS();
        return summedDurationSeconds;
    }

    // ------------------------------------------------------------------------------------------------

    public IKoreRouteLeg LegAtRouteTime(double timeS)
    {
        double timeRemainingS = timeS;
        foreach (IKoreRouteLeg leg in Legs)
        {
            if (timeRemainingS < leg.GetDurationS())
                return leg;

            timeRemainingS -= leg.GetDurationS();
        }
        return Legs[Legs.Count - 1]; // Return the last leg if time exceeds total duration
    }

    public double RouteTimeForFraction(double fraction)
    {
        if ((fraction < 0) || (NumLegs() == 0))
            return 0;

        double totalDurationS = TotalDurationSeconds();

        if (fraction > 1)
            return totalDurationS;

        return totalDurationS * fraction;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Complex Methods
    // --------------------------------------------------------------------------------------------

    public KoreLLAPoint PositionAtRouteTime(double timeS)
    {
        // Setup the start time for the first leg
        double currLegStartSecs = 0;

        foreach (IKoreRouteLeg leg in Legs)
        {
            // Find the times for the current leg duration, the current leg end time, and the current leg time
            double currLegDurationSecs = leg.GetDurationS();
            double currLegEndSecs = currLegStartSecs + currLegDurationSecs;
            double currLegTime = timeS - currLegStartSecs;

            // If the current leg time fits in the current leg's duration
            if ((currLegTime > 0) && (currLegTime < currLegDurationSecs))
            {
                // If the current time is within the current leg's duration, return the position
                return leg.PositionAtLegTime(currLegTime);
            }

            // Set the start time for the next leg
            currLegStartSecs = currLegEndSecs;
        }

        // If we get to the end of the logic without finding a leg, return a default position
        return KoreLLAPoint.Zero;
    }

    // ------------------------------------------------------------------------------------------------

    // Get the route, in a list of points.
    // - Throw if there are no legs, or less than two points (the start and end points are required).

    public List<KoreLLAPoint> RoutePositions(int numPoints)
    {
        if (NumLegs() == 0 || numPoints < 2)
            throw new InvalidOperationException("Route must have at least two points.");

        // Determine the fractions and number of seconds between points
        double totalDurationS = TotalDurationSeconds();
        double secondsPerPoint = totalDurationS / (numPoints - 1);

        List<KoreLLAPoint> points = new List<KoreLLAPoint>();

        for (int i = 0; i < numPoints; i++)
        {
            double currTime = i * secondsPerPoint;
            points.Add(PositionAtRouteTime(currTime));
        }

        return points;
    }
}





