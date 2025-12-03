// <fileheader>

using System.IO;
using KoreCommon;
using KoreCommon.SkiaSharp;

using SkiaSharp;

namespace KoreGIS;

#nullable enable

public class KoreTerrainImageTile
{
    public KoreLLBox LLBox = KoreLLBox.Zero;
    public string ImagePath = string.Empty;
    SKBitmap? TileImage = null;

    // Width resolution of the image, a ranking factor in the wider collection
    // of satellite images, with higher resolution images preferred when requesting
    // a value.
    public float PixelsPerDegree = 0.0f;

    // --------------------------------------------------------------------------------------------

    public KoreTerrainImageTile(KoreLLBox llBox, string imagePath)
    {
        LLBox = llBox;
        ImagePath = KoreFileOps.StandardizePath(imagePath);
        LoadImage(ImagePath);
    }

    public KoreTerrainImageTile()
    {
        // Default constructor - zero values
    }

    // Zero tile - rely on the default constructor
    public static KoreTerrainImageTile Zero
    {
        get { return new KoreTerrainImageTile(); }
    }

    // --------------------------------------------------------------------------------------------

    public void LoadImage(string fullFilePath)
    {
        string imagePath = KoreFileOps.StandardizePath(fullFilePath);

        if (File.Exists(imagePath))
        {
            TileImage = KoreSkiaSharpBitmapOps.LoadBitmap(imagePath);

            if (TileImage != null)
            {
                float imageWidthPixels = TileImage.Width;
                float imageWidthDegrees = (float)LLBox.DeltaLonDegs;

                // More pixels per degree is better resolution
                PixelsPerDegree = imageWidthPixels / imageWidthDegrees;

                KoreCentralLog.AddEntry($" - Loaded image: {imagePath} size {TileImage.Width}x{TileImage.Height} // pixels per degree {PixelsPerDegree:F1}");
            }
        }
    }

    // --------------------------------------------------------------------------------------------

    public void UnloadImage()
    {
        // Dispose of the SKBitmap properly (if it exists)
        TileImage?.Dispose();
        TileImage = null;
    }

    // --------------------------------------------------------------------------------------------

    public bool IsValid()
    {
        return (TileImage != null);
    }

    public bool ContainsPoint(KoreLLPoint checkPos)
    {
        return LLBox.Contains(checkPos);
    }

    public KoreColorRGB GetTerrainColor(KoreLLPoint checkPos)
    {
        if (LLBox.Contains(checkPos) && (TileImage != null))
        {
            // Use the lat/lon values to get fractions across the image
            double fracBottomToTop = LLBox.LatRangeDegs.FractionInRange(checkPos.LatDegs);
            double fracLeftToRight = LLBox.LonRangeDegs.FractionInRange(checkPos.LonDegs);
            double fracTopToBottom = 1 - fracBottomToTop;

            // Determine the pixel position we want to query
            int tileImageWidth = TileImage.Width;
            int tileImageHeight = TileImage.Height;
            int pixelX = (int)(fracLeftToRight * (tileImageWidth - 1));
            int pixelY = (int)(fracTopToBottom * (tileImageHeight - 1));

            // Check we're in-bounds on the pixel positions
            if (pixelX < 0) pixelX = 0;
            if (pixelX >= tileImageWidth) pixelX = tileImageWidth - 1;
            if (pixelY < 0) pixelY = 0;
            if (pixelY >= tileImageHeight) pixelY = tileImageHeight - 1;

            // Get and return the pixel color
            SKColor pixelColor = TileImage.GetPixel(pixelX, pixelY);
            KoreColorRGB returnColor = KoreSkiaSharpConv.ToKoreColorRGB(pixelColor);

            KoreCentralLog.AddEntry($" - {checkPos} = frac {fracLeftToRight:F2},{fracTopToBottom:F2} pixel {pixelX},{pixelY} color {KoreColorOps.ColorName(returnColor)}");

            return returnColor;
        }
        return KoreColorRGB.Zero;
    }


}

