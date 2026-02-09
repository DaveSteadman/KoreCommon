// <fileheader>

using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using KoreCommon.SkiaSharp;

namespace KoreCommon;

// Static operations for UV coordinate visualization
// Provides tools to visualize UV layouts for debugging mesh UV mapping
public static class KoreMeshDataUvOps
{
    // --------------------------------------------------------------------------------------------
    // MARK: UV Visualization
    // --------------------------------------------------------------------------------------------

    // Creates a visual representation of UV coordinates from mesh data
    // mesh: Mesh containing UV data
    // imageSize: Output image dimensions (square)
    // showPointNumbers: Whether to draw vertex IDs next to points
    // showTriangles: Whether to draw triangle wireframes
    // backgroundColor: Background color for the image
    // returns: SkiaSharp plotter with UV layout drawn
    public static KoreSkiaSharpPlotter CreateUVLayout(
        KoreMeshData mesh,
        int imageSize = 1024,
        bool showPointNumbers = true,
        bool showTriangles = true,
        SKColor? backgroundColor = null)
    {
        return CreateUVLayout(mesh, imageSize, imageSize, showPointNumbers, showTriangles, backgroundColor);
    }

    // Creates a visual representation of UV coordinates from mesh data with custom dimensions
    public static KoreSkiaSharpPlotter CreateUVLayout(
        KoreMeshData mesh,
        int imageWidth,
        int imageHeight,
        bool showPointNumbers = true,
        bool showTriangles = true,
        SKColor? backgroundColor = null)
    {
        var plotter = new KoreSkiaSharpPlotter(imageWidth, imageHeight);

        // Clear background
        var bgColor = backgroundColor ?? SKColors.White;
        plotter.Clear(bgColor);

        // Draw border around UV space (0,0 to 1,1)
        DrawUVBorder(plotter, imageWidth, imageHeight);

        // Draw UV data
        DrawUVData(plotter, mesh, imageWidth, imageHeight, showPointNumbers, showTriangles);

        return plotter;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Save UV Layout
    // --------------------------------------------------------------------------------------------

    // Saves UV layout directly to a file
    public static void SaveUVLayout(
        KoreMeshData mesh,
        string filePath,
        int imageSize = 1024,
        bool showPointNumbers = true,
        bool showTriangles = true)
    {
        var plotter = CreateUVLayout(mesh, imageSize, showPointNumbers, showTriangles);
        plotter.Save(filePath);
    }

    // Saves UV layout with custom dimensions
    public static void SaveUVLayout(
        KoreMeshData mesh,
        string filePath,
        int imageWidth,
        int imageHeight,
        bool showPointNumbers = true,
        bool showTriangles = true)
    {
        var plotter = CreateUVLayout(mesh, imageWidth, imageHeight, showPointNumbers, showTriangles);
        plotter.Save(filePath);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Private Helper Methods
    // --------------------------------------------------------------------------------------------

    private static void DrawUVBorder(KoreSkiaSharpPlotter plotter, int width, int height)
    {
        // Draw main border around UV space (0,0 to 1,1)
        plotter.DrawSettings.Color = SKColors.Black;
        plotter.DrawSettings.LineWidth = 3;
        plotter.DrawSettings.Paint.Style = SKPaintStyle.Stroke;

        var borderRect = new SKRect(0, 0, width, height);
        plotter.DrawRect(borderRect, plotter.DrawSettings.Paint);

        // Draw grid lines for quarters to help visualize UV space
        plotter.DrawSettings.Color = SKColors.LightGray;
        plotter.DrawSettings.LineWidth = 1;

        // Vertical lines at 0.25, 0.5, 0.75
        plotter.DrawLine(new SKPoint(width * 0.25f, 0), new SKPoint(width * 0.25f, height));
        plotter.DrawLine(new SKPoint(width * 0.5f, 0), new SKPoint(width * 0.5f, height));
        plotter.DrawLine(new SKPoint(width * 0.75f, 0), new SKPoint(width * 0.75f, height));

        // Horizontal lines at 0.25, 0.5, 0.75
        plotter.DrawLine(new SKPoint(0, height * 0.25f), new SKPoint(width, height * 0.25f));
        plotter.DrawLine(new SKPoint(0, height * 0.5f), new SKPoint(width, height * 0.5f));
        plotter.DrawLine(new SKPoint(0, height * 0.75f), new SKPoint(width, height * 0.75f));
    }

    private static void DrawUVData(
        KoreSkiaSharpPlotter plotter,
        KoreMeshData mesh,
        int imageWidth,
        int imageHeight,
        bool showPointNumbers,
        bool showTriangles)
    {
        // Convert UV coordinates to screen coordinates
        var uvToScreen = new Dictionary<int, SKPoint>();

        foreach (var kvp in mesh.UVs)
        {
            int vertexId = kvp.Key;
            var uv = kvp.Value;

            // Note: KoreMeshdata UV coordinates are top-left zero - no conversion as this should align with
            //       SKiaSharp top left zero
            float x = (float)(uv.X * imageWidth);
            float y = (float)(uv.Y * imageHeight); // Flip Y for screen coordinates

            uvToScreen[vertexId] = new SKPoint(x, y);
        }

        // Draw triangles first (behind points)
        if (showTriangles)
        {
            DrawUVTriangles(plotter, mesh, uvToScreen);
        }

        // Draw UV points on top
        DrawUVPoints(plotter, uvToScreen, showPointNumbers);
    }

    private static void DrawUVTriangles(
        KoreSkiaSharpPlotter plotter,
        KoreMeshData mesh,
        Dictionary<int, SKPoint> uvToScreen)
    {
        plotter.DrawSettings.Color = SKColors.Blue;
        plotter.DrawSettings.LineWidth = 1;
        plotter.DrawSettings.Paint.Style = SKPaintStyle.Stroke;

        foreach (var triangle in mesh.Triangles.Values)
        {
            // Only draw triangle if all vertices have UV coordinates
            if (uvToScreen.TryGetValue(triangle.A, out var pointA) &&
                uvToScreen.TryGetValue(triangle.B, out var pointB) &&
                uvToScreen.TryGetValue(triangle.C, out var pointC))
            {
                plotter.DrawLine(pointA, pointB);
                plotter.DrawLine(pointB, pointC);
                plotter.DrawLine(pointC, pointA);
            }
        }
    }

    private static void DrawUVPoints(
        KoreSkiaSharpPlotter plotter,
        Dictionary<int, SKPoint> uvToScreen,
        bool showPointNumbers)
    {
        // Draw UV points as small circles
        plotter.DrawSettings.Color = SKColors.Red;
        plotter.DrawSettings.LineWidth = 2;
        plotter.DrawSettings.Paint.Style = SKPaintStyle.Fill;

        foreach (var kvp in uvToScreen)
        {
            int vertexId = kvp.Key;
            var screenPoint = kvp.Value;

            // Draw point as small filled circle
            plotter.DrawPointAsFilledCircle(screenPoint, 3);

            // Draw vertex ID next to the point if requested
            if (showPointNumbers)
            {
                plotter.DrawSettings.Color = SKColors.Black;
                plotter.DrawSettings.Paint.Style = SKPaintStyle.Fill;

                var textPos = new SKPoint(screenPoint.X + 5, screenPoint.Y - 5);
                plotter.DrawText(vertexId.ToString(), textPos, 10);

                plotter.DrawSettings.Color = SKColors.Red;
            }
        }
    }
}
