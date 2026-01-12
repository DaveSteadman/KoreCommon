// <fileheader>

using System;
using System.IO;
using System.Collections.Generic;

using SkiaSharp;

using KoreCommon;
namespace KoreCommon.SkiaSharp;

// KoreSkiaSharpBitmapOps: static class of functions around manipulating bitmaps. Uses SkiaSharp, to be universal across any dotnet application.

public static class KoreSkiaSharpBitmapOps
{
    // --------------------------------------------------------------------------------------------
    // MARK: Resize
    // --------------------------------------------------------------------------------------------

    // RotateAndScaleBitmap
    // - Maintains the centre of the bitmap as the anchor position.
    public static SKBitmap RotateAndScaleBitmap(SKBitmap originalBitmap, float angle, float scale)
    {
        int width = (int)(originalBitmap.Width * scale);
        int height = (int)(originalBitmap.Height * scale);
        SKBitmap transformedBitmap = new SKBitmap(width, height);

        using (var canvas = new SKCanvas(transformedBitmap))
        {
            canvas.Clear(SKColors.Transparent);

            // Translate to the centre of the new bitmap
            canvas.Translate(width / 2, height / 2);

            // Apply rotation and scaling
            canvas.RotateDegrees(angle);
            canvas.Scale(scale);

            // Translate back to the top-left of the original bitmap
            canvas.Translate(-originalBitmap.Width / 2, -originalBitmap.Height / 2);

            // Draw the original bitmap
            canvas.DrawBitmap(originalBitmap, 0, 0);
        }
        return transformedBitmap;
    }

    // --------------------------------------------------------------------------------------------

    // Resize the image - simplify the interface to the action so we don't have to deal with the canvas

    public static SKBitmap ResizeImage(SKBitmap originalBitmap, int newWidth, int newHeight)
    {
        // Create a new SKBitmap with the desired dimensions
        SKBitmap resizedBitmap = new SKBitmap(newWidth, newHeight);

        using (SKCanvas canvas = new SKCanvas(resizedBitmap))
        {
            // Set the quality of the resize
            canvas.DrawBitmap(originalBitmap, new SKRect(0, 0, newWidth, newHeight));
        }

        return resizedBitmap;
    }

    public static SKBitmap ResizeImage(SKBitmap originalImage, SKSize newSize) => ResizeImage(originalImage, (int)newSize.Width, (int)newSize.Height);

    // --------------------------------------------------------------------------------------------
    // MARK: Load
    // --------------------------------------------------------------------------------------------

    // SKBitmap.Decode will handle PNG, WebP, JPEG, BMP, GIF, and several other common image formats, as long as SkiaSharp is
    // built with support for those formats (which is true for standard SkiaSharp distributions).
    // Usage: KoreSkiaSharpBitmapOps.LoadBitmap(string)

    public static SKBitmap LoadBitmap(string filePath)
    {
        using (var stream = File.OpenRead(filePath))
        {
            return SKBitmap.Decode(stream);
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Save As
    // --------------------------------------------------------------------------------------------

    // Usage: KoreSkiaSharpBitmapOps.SaveBitmapAsPng(myBitmap, string)
    public static void SaveBitmapAsPng(SKBitmap bitmap, string filePath)
    {
        using (var image = bitmap.Encode(SKEncodedImageFormat.Png, 100))
        using (var stream = File.OpenWrite(filePath))
        {
            image.SaveTo(stream);
        }
    }

    // --------------------------------------------------------------------------------------------

    // Usage: KoreSkiaSharpBitmapOps.SaveBitmapAsWebp(myBitmap, string)
    public static void SaveBitmapAsWebp(SKBitmap bitmap, string filePath)
    {
        using (var image = bitmap.Encode(SKEncodedImageFormat.Webp, 100))
        using (var stream = File.OpenWrite(filePath))
        {
            image.SaveTo(stream);
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Combine
    // --------------------------------------------------------------------------------------------

    // Combine a 2D grid of bitmaps into a single bitmap:
    // - Traverses the two dimensions of the input array of bitmaps
    // - Uses the size of the first image, and assumes all are present and the same size
    // - Creates a working image of a multiplied up size, the copies the child images into it
    // - Resizes the final image to the required size

    public static SKBitmap CombineBitmaps(SKBitmap[,] bitmaps, Kore2DGridSize requiredSize)
    {
        // Create the array axis sizes
        int rows = bitmaps.GetLength(0);
        int cols = bitmaps.GetLength(1);

        if (rows == 0 || cols == 0 || bitmaps[0, 0] == null)
            throw new ArgumentException("Input bitmap array is empty or contains null elements.");

        // Get the size of the first bitmap to use as a reference, and the combined size
        int imgHeight = bitmaps[0, 0].Height;
        int imgWidth = bitmaps[0, 0].Width;
        int combinedWidth = imgWidth * cols;
        int combinedHeight = imgHeight * rows;

        SKBitmap combinedImage = new SKBitmap(combinedWidth, combinedHeight);
        using (SKCanvas canvas = new SKCanvas(combinedImage))
        {
            canvas.Clear(SKColors.Transparent);

            // Loop through the 2D array and draw each bitmap
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    SKBitmap bitmap = bitmaps[row, col];
                    if (bitmap != null)
                    {
                        int x = col * imgWidth;
                        int y = row * imgHeight;
                        canvas.DrawBitmap(bitmap, x, y);
                    }
                }
            }
        }

        // Resize the combined bitmap to the specified size - Clean up the original combined bitmap
        SKBitmap resizedCombined = ResizeImage(combinedImage, requiredSize.Width, requiredSize.Height);
        combinedImage.Dispose();

        // Return the resized combined bitmap
        return resizedCombined;
    }

    // --------------------------------------------------------------------------------------------

    // DivideBitmap: Creates a 2D array of bitmaps from a single bitmap
    // - First we determine the size of each cell and the number of cells, to find an ideal source image size
    // - We resize the input image to this size
    // - Then we create a 2D array of bitmaps, and copy the pixels out on a 1:1 basis
    // - dispose of the original bitmap to free up memory
    // - return the 2D array of bitmaps

    public static SKBitmap[,] DivideBitmap(SKBitmap bitmap, Kore2DGridSize outputGridSize, Kore2DGridSize outputImageSize)
    {
        // Calc the ideal image size based on the grid size and the image size
        int imageWidth = outputGridSize.Width * outputImageSize.Width;
        int imageHeight = outputGridSize.Height * outputImageSize.Height;

        // Use SKSamplingOptions instead of obsolete SKFilterQuality
        SKSamplingOptions sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
        SKBitmap resized = bitmap.Resize(new SKImageInfo(imageWidth, imageHeight), sampling);

        // Create the 2D array to hold the divided bitmaps
        SKBitmap[,] result = new SKBitmap[outputGridSize.Height, outputGridSize.Width];

        // Copy the pixels from the resized bitmap into the 2D array
        for (int row = 0; row < outputGridSize.Height; row++)
        {
            for (int col = 0; col < outputGridSize.Width; col++)
            {
                int x = col * outputImageSize.Width;
                int y = row * outputImageSize.Height;
                SKBitmap cell = new SKBitmap(outputImageSize.Width, outputImageSize.Height);
                using (SKCanvas canvas = new SKCanvas(cell))
                {
                    canvas.DrawBitmap(resized, new SKRect(x, y, x + outputImageSize.Width, y + outputImageSize.Height));
                }
                result[row, col] = cell;
            }
        }

        // Dispose of the original bitmap to free up memory
        bitmap.Dispose();

        // Return the 2D array of bitmaps
        return result;
    }

    // --------------------------------------------------------------------------------------------

    // Using fractions (like from a calculated map tile subsection) chop a new section out of an image.
    // 0,0 is top-left, 1,1 is bottom-right
    // Usage: KoreSkiaSharpBitmapOps.BitmapSubsection(SKBitmap, float, float, float, float)

    public static SKBitmap BitmapSubsection(SKBitmap bitmap, float minFracX, float minFracY, float maxFracX, float maxFracY)
    {
        // Validate input fractions
        if (minFracX < 0 || minFracX > 1 || minFracY < 0 || minFracY > 1 ||
            maxFracX < 0 || maxFracX > 1 || maxFracY < 0 || maxFracY > 1 ||
            minFracX >= maxFracX || minFracY >= maxFracY)
        {
            throw new ArgumentException("Fraction values must be between 0 and 1 and min values must be less than max values.");
        }

        int startX = (int)(minFracX * bitmap.Width);
        int startY = (int)(minFracY * bitmap.Height);
        int width = (int)((maxFracX - minFracX) * bitmap.Width);
        int height = (int)((maxFracY - minFracY) * bitmap.Height);

        SKBitmap subsection = new SKBitmap(width, height);
        using (SKCanvas canvas = new SKCanvas(subsection))
        {
            canvas.DrawBitmap(bitmap, new SKRect(startX, startY, startX + width, startY + height), new SKRect(0, 0, width, height));
        }

        return subsection;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Color List
    // --------------------------------------------------------------------------------------------

    // Loop through all the pixels in an image, creating a KoreColorList from it
    // Usage: KoreColorList colorList = KoreSkiaSharpBitmapOps.ColorsFromBitmap(SKBitmap);
    public static KoreColorList ColorsFromBitmap(SKBitmap bitmap)
    {
        KoreColorList resultList = new();

        // Loop across the pixels
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                SKColor pixelColor = bitmap.GetPixel(x, y);
                KoreColorRGB koreColor = new KoreColorRGB(pixelColor.Red, pixelColor.Green, pixelColor.Blue);
                resultList.AddWithCheck(koreColor, 5f);
            }
        }

        return resultList;
    }

    // Apply a KoreColorList to an image, replacing each pixel with the closest color from the list
    // Usage: KoreSkiaSharpBitmapOps.ApplyColorListToBitmap(bitmap, colorList);

    public static void ApplyColorListToBitmap(SKBitmap bitmap, KoreColorList colorList)
    {
        // Loop across the pixels
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                // Get the actual pixel color
                SKColor pixelColor = bitmap.GetPixel(x, y);
                KoreColorRGB koreColor = new KoreColorRGB(pixelColor.Red, pixelColor.Green, pixelColor.Blue);
                
                // Find the closest color in the list
                KoreColorRGB closestColor = colorList.ClosestColor(koreColor);
                SKColor skiaClosest = new SKColor(closestColor.R, closestColor.G, closestColor.B, pixelColor.Alpha);
                
                // Apply the new color back to the bitmap
                bitmap.SetPixel(x, y, skiaClosest);
            }
        }
    }

    // returns an x,y array, with 0,0 being top-left

    public static KoreColorRGB[,] SampleBitmapColors(SKBitmap bitmap, int sampleWidth, int sampleHeight)
    {
        KoreColorRGB[,] colorGrid = new KoreColorRGB[sampleHeight, sampleWidth];

        float xStep = (float)bitmap.Width / sampleWidth;
        float yStep = (float)bitmap.Height / sampleHeight;

        for (int y = 0; y < sampleHeight; y++)
        {
            for (int x = 0; x < sampleWidth; x++)
            {
                int pixelX = (int)(x * xStep);
                int pixelY = (int)(y * yStep);

                // Clamp to bitmap dimensions
                pixelX = Math.Min(pixelX, bitmap.Width - 1);
                pixelY = Math.Min(pixelY, bitmap.Height - 1);

                SKColor pixelColor = bitmap.GetPixel(pixelX, pixelY);
                colorGrid[x, y] = new KoreColorRGB(pixelColor.Red, pixelColor.Green, pixelColor.Blue, pixelColor.Alpha);
            }
        }

        return colorGrid;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Effects
    // --------------------------------------------------------------------------------------------

    // function to apply a pixelation effect to a bitmap, by reducing its size by a factor, then restoring the size to
    // its original dimensions.

    public static SKBitmap PixelateImage(SKBitmap originalBitmap, int pixelationFactor)
    {
        // Create a new SKBitmap with the desired dimensions
        SKBitmap pixelatedBitmap = new SKBitmap(originalBitmap.Width / pixelationFactor, originalBitmap.Height / pixelationFactor);

        using (SKCanvas canvas = new SKCanvas(pixelatedBitmap))
        {
            // Set the quality of the resize
            canvas.DrawBitmap(originalBitmap, new SKRect(0, 0, pixelatedBitmap.Width, pixelatedBitmap.Height));
        }

        return ResizeImage(pixelatedBitmap, originalBitmap.Width, originalBitmap.Height);
    }
}