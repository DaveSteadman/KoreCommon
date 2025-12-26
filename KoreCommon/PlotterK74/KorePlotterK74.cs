
using SkiaSharp;

using KoreCommon;
using KoreCommon.SkiaSharp;


namespace KoreCommon.PlotterK74;

// KorePlotterK74: 2D plotting functionality using SkiaSharp
// - Based on KoreSkiaSharpPlotter from KoreCommon.SkiaSharp
// - Exists to draw in the K74 aesthetic/style
//   - Tiles, bold lines, specific colors, etc.
//   - A grid based layout.

public partial class KorePlotterK74
{
    public readonly int CellSizePixels;
    public readonly int PixelWidth;
    public readonly int PixelHeight;

    public SKBitmap K74Bitmap;
    public SKCanvas K74Canvas;
    public KoreSkiaSharpPlotterDrawSettings DrawSettings;

    public KoreSkiaSharpPlotter KorePlotter;

    public KorePlotterK74(int widthcells, int heightcells, int cellSizePixels)
    {
        this.CellSizePixels = cellSizePixels;
        this.PixelWidth     = widthcells * cellSizePixels;
        this.PixelHeight    = heightcells * cellSizePixels;

        K74Bitmap = new SKBitmap(PixelWidth, PixelHeight);
        K74Canvas = new SKCanvas(K74Bitmap);

        // Set default draw settings for K74 style
        DrawSettings = new KoreSkiaSharpPlotterDrawSettings();
        DrawSettings.ResetToDefaults();

        KorePlotter = new KoreSkiaSharpPlotter(PixelWidth, PixelHeight);
    }

    // --------------------------------------------------------------------------------------------

    public SKRect RectForCell(int cellX, int cellY)
    {
        return new SKRect(
            cellX * CellSizePixels,
            cellY * CellSizePixels,
            (cellX + 1) * CellSizePixels,
            (cellY + 1) * CellSizePixels
        );
    }

    public SKPoint PointForCell(int cellX, int cellY, KoreXYRectPosition cellPoint)
    {
        KoreXYRect cellRect = new KoreXYRect(
            cellX * CellSizePixels,
            cellY * CellSizePixels,
            (cellX + 1) * CellSizePixels,
            (cellY + 1) * CellSizePixels
        );

        KoreXYVector point = cellRect.PointFromPosition(cellPoint);
        return new SKPoint((float)point.X, (float)point.Y);
    }

    // --------------------------------------------------------------------------------------------

    public void FillCell(int cellX, int cellY, SKColor color)
    {
        using var paint = new SKPaint
        {
            Color = color,
            Style = SKPaintStyle.Fill,
            IsAntialias = false
        };

        SKRect cellRect = RectForCell(cellX, cellY);
        KorePlotter.DrawRect(cellRect, paint);
    }

    public void DrawCellRect(int cellLeftX, int cellTopY, int cellRightX, int cellBottomY, SKColor color)
    {
        // top line
        for (int x = cellLeftX; x <= cellRightX; x++)
            FillCell(x, cellTopY, color);

        // bottom line
        for (int x = cellLeftX; x <= cellRightX; x++)
            FillCell(x, cellBottomY, color);

        // left line
        for (int y = cellTopY; y <= cellBottomY; y++)
            FillCell(cellLeftX, y, color);

        // right line
        for (int y = cellTopY; y <= cellBottomY; y++)
            FillCell(cellRightX, y, color);
    }

    // --------------------------------------------------------------------------------------------

    // clip rect operations, to create a subset of cells to draw into
    // startX/Y are inclusive, everything counted from top left
    // numCellsX/Y are counts of cells to include, all inclusive
    // Usage: ClipRectForCells(1, 1, 3, 3) will clip to cells (1,1) to (3,3) inclusive
    public void ClipRectForCells(int startCellX, int startCellY, int numCellsX, int numCellsY)
    {
        SKPoint tlPoint = PointForCell(
            startCellX,
            startCellY,
            KoreXYRectPosition.TopLeft);

        SKPoint brPoint = PointForCell(
            startCellX + numCellsX,
            startCellY + numCellsY,
            KoreXYRectPosition.BottomRight
        );

        var clipRect = new SKRect(
            tlPoint.X,
            tlPoint.Y,
            brPoint.X,
            brPoint.Y
        );

        KorePlotter.ClipToRect(clipRect);
    }

    public void ClearAllClips()
    {
        KorePlotter.ClearAllClips();
    }

    // --------------------------------------------------------------------------------------------

    // Draw arcs at the cell width size, centered on a cell point
    public void DrawArcAtCell(
        int cellX, int cellY, KoreXYRectPosition cellPos,
        int cellSizeInnerRadius, int cellSizeOuterRadius,
        float startAngleDegs, float sweepAngleDegs,
        SKColor color)
    {
        SKPoint centerPoint = PointForCell(cellX, cellY, cellPos);
        float innerRadius = cellSizeInnerRadius * CellSizePixels;
        float outerRadius = cellSizeOuterRadius * CellSizePixels;

        using var paint = new SKPaint
        {
            Color = color,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = outerRadius - innerRadius,
            IsAntialias = false
        };

        float radius = innerRadius + (paint.StrokeWidth / 2);

        SKRect arcRect = new SKRect(
            centerPoint.X - radius,
            centerPoint.Y - radius,
            centerPoint.X + radius,
            centerPoint.Y + radius
        );

        KoreXYAnnularSector arcBox = new KoreXYAnnularSector(
            new KoreXYVector(centerPoint.X, centerPoint.Y),
            innerRadius,
            outerRadius,
            KoreValueUtils.DegsToRads(startAngleDegs),
            KoreValueUtils.DegsToRads(sweepAngleDegs)
        );

        KorePlotter.DrawArcBoxFilled(arcBox, color);
    }
}