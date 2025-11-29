// <fileheader>

using System;
using KoreCommon.SkiaSharp;

using KoreCommon;
namespace KoreGIS;


public static class KoreWorldPlotterOps
{
    // --------------------------------------------------------------------------------------------
    // MARK: Grid Overlays
    // --------------------------------------------------------------------------------------------

    public static void DrawGrid(
        KoreWorldPlotter worldPlotter,
        int latSpacingDegs = 15,
        int lonSpacingDegs = 15,
        KoreColorRGB? gridColor = null,
        double lineWidth = 1.0)
    {
        // Default color
        KoreColorRGB color = gridColor ?? new KoreColorRGB(200, 200, 200); // Light gray

        var plotter = worldPlotter.Plotter;
        plotter.DrawSettings.Color = KoreSkiaSharpConv.ToSKColor(color);
        plotter.DrawSettings.LineWidth = (float)lineWidth;

        // Draw latitude lines (horizontal)
        for (int lat = -90; lat <= 90; lat += latSpacingDegs)
        {
            var start = worldPlotter.LatLonToPixel(lat, worldPlotter.MinLonDegs);
            var end = worldPlotter.LatLonToPixel(lat, worldPlotter.MaxLonDegs);
            plotter.DrawLine(start, end);
        }

        // Draw longitude lines (vertical)
        for (int lon = -180; lon <= 180; lon += lonSpacingDegs)
        {
            var start = worldPlotter.LatLonToPixel(worldPlotter.MinLatDegs, lon);
            var end = worldPlotter.LatLonToPixel(worldPlotter.MaxLatDegs, lon);
            plotter.DrawLine(start, end);
        }
    }

    // --------------------------------------------------------------------------------------------

    public static void DrawGridWithLabels(
        KoreWorldPlotter worldPlotter,
        int latSpacingDegs = 30,
        int lonSpacingDegs = 30,
        KoreColorRGB? gridColor = null,
        KoreColorRGB? labelColor = null,
        double lineWidth = 1.0,
        int fontSize = 10)
    {
        // Draw the grid first
        DrawGrid(worldPlotter, latSpacingDegs, lonSpacingDegs, gridColor, lineWidth);

        KoreColorRGB textColor = labelColor ?? new KoreColorRGB(80, 80, 80);

        var plotter = worldPlotter.Plotter;
        var originalColor = plotter.DrawSettings.Color;
        plotter.DrawSettings.Color = KoreSkiaSharpConv.ToSKColor(textColor);

        double minLat = worldPlotter.MinLatDegs;
        double maxLat = worldPlotter.MaxLatDegs;
        double minLon = worldPlotter.MinLonDegs;
        double maxLon = worldPlotter.MaxLonDegs;

        double lonSpan = Math.Max(maxLon - minLon, 0.0001);
        double latSpan = Math.Max(maxLat - minLat, 0.0001);

        double latLabelOffset = Math.Min(5.0, lonSpan * 0.05); // degrees inside the map from the edge
        double latLabelLon = minLon + Math.Min(latLabelOffset, lonSpan / 2.0);

        double lonLabelOffset = Math.Min(5.0, latSpan * 0.05);
        double lonLabelLat = maxLat - Math.Min(lonLabelOffset, latSpan / 2.0);

        // Label latitude lines that fall within the current bounds
        for (int lat = -90; lat <= 90; lat += latSpacingDegs)
        {
            if (lat < minLat || lat > maxLat)
            {
                continue;
            }

            string label = lat == 0 ? "0°" : $"{Math.Abs(lat)}°{(lat > 0 ? "N" : "S" )}";

            var position = new KoreLLPoint
            {
                LatDegs = lat,
                LonDegs = latLabelLon
            };

            worldPlotter.DrawTextAtPosition(label, position, KoreXYRectPosition.LeftCenter, fontSize);
        }

        // Label longitude lines that fall within the current bounds
        for (int lon = -180; lon <= 180; lon += lonSpacingDegs)
        {
            if (lon < minLon || lon > maxLon)
            {
                continue;
            }

            string label = lon switch
            {
                0 => "0°",
                180 or -180 => "180°",
                _ => $"{Math.Abs(lon)}°{(lon > 0 ? "E" : "W" )}"
            };

            var position = new KoreLLPoint
            {
                LatDegs = lonLabelLat,
                LonDegs = lon
            };

            worldPlotter.DrawTextAtPosition(label, position, KoreXYRectPosition.TopCenter, fontSize);
        }

        plotter.DrawSettings.Color = originalColor;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Major Line drawing
    // --------------------------------------------------------------------------------------------

    public static void DrawEquator(
        KoreWorldPlotter worldPlotter,
        KoreColorRGB? lineColor = null,
        double lineWidth = 2.0)
    {
        KoreColorRGB color = lineColor ?? KoreColorPalette.Colors["Red"];

        var plotter = worldPlotter.Plotter;
        plotter.DrawSettings.Color = KoreSkiaSharpConv.ToSKColor(color);
        plotter.DrawSettings.LineWidth = (float)lineWidth;

        var start = worldPlotter.LatLonToPixel(0, worldPlotter.MinLonDegs);
        var end = worldPlotter.LatLonToPixel(0, worldPlotter.MaxLonDegs);
        plotter.DrawLine(start, end);
    }

    private static void DrawArcticCircle(
        KoreWorldPlotter worldPlotter,
        KoreColorRGB? lineColor = null,
        double lineWidth = 2.0)
    {
        KoreColorRGB color = lineColor ?? KoreColorPalette.Colors["Blue"];

        var plotter = worldPlotter.Plotter;
        plotter.DrawSettings.Color = KoreSkiaSharpConv.ToSKColor(color);
        plotter.DrawSettings.LineWidth = (float)lineWidth;

        var lat = 66.5; // Approximate latitude of Arctic Circle
        var start = worldPlotter.LatLonToPixel(lat, worldPlotter.MinLonDegs);
        var end = worldPlotter.LatLonToPixel(lat, worldPlotter.MaxLonDegs);
        plotter.DrawLine(start, end);
    }

    // --------------------------------------------------------------------------------------------
    // Border Drawing
    // --------------------------------------------------------------------------------------------

    public static void DrawBorder(
        KoreWorldPlotter worldPlotter,
        KoreColorRGB? borderColor = null,
        double borderWidth = 2.0)
    {
        KoreColorRGB color = borderColor ?? KoreColorRGB.Black;
        var plotter = worldPlotter.Plotter;

        // Store the linewidth and color
        var originalColor = plotter.DrawSettings.Color;
        var originalLineWidth = plotter.DrawSettings.LineWidth;

        // Apply the new values
        plotter.DrawSettings.Color = KoreSkiaSharpConv.ToSKColor(color);
        plotter.DrawSettings.LineWidth = (float)(borderWidth * 2.0); // Double width to compensate for edge clipping

        // Draw rectangle border at the outer edge
        var topLeft = new KoreXYVector(0, 0);
        var topRight = new KoreXYVector(worldPlotter.Width, 0);
        var bottomRight = new KoreXYVector(worldPlotter.Width, worldPlotter.Height);
        var bottomLeft = new KoreXYVector(0, worldPlotter.Height);

        plotter.DrawLine(topLeft, topRight);
        plotter.DrawLine(topRight, bottomRight);
        plotter.DrawLine(bottomRight, bottomLeft);
        plotter.DrawLine(bottomLeft, topLeft);

        // Restore original settings
        plotter.DrawSettings.Color = originalColor;
        plotter.DrawSettings.LineWidth = originalLineWidth;
    }
}
