// <fileheader>

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using KoreCommon;
using KoreCommon.SkiaSharp;
using SkiaSharp;

namespace KoreCommon.UnitTest;

public static partial class KoreTestSkiaSharp
{
    // Test plane projection - draw circles on 3D planes and project back to 2D
    // KoreTestSkiaSharp.TestPlane
    private static void TestImage(KoreTestLog testLog)
    {
        string imagePath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "TestImage_Input.png");

        // check the file exists
        if (!File.Exists(imagePath))
        {
            testLog.AddResult("Test Image", false, "Input image file does not exist.");
            return;
        }

        // Load an image from file
        SKBitmap image = KoreSkiaSharpBitmapOps.LoadBitmap(imagePath);

        // Get the list of colors in the image
        KoreColorList colorList = KoreSkiaSharpBitmapOps.ColorsFromBitmap(image);

        // Reduce the number of colors by merging similar colors
        colorList.ReduceColorCount(70);

        // Apply the color list to the image
        KoreSkiaSharpBitmapOps.ApplyColorListToBitmap(image, colorList);

        // Save the modified image
        string outputPath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "TestImage_Output.png");
        KoreSkiaSharpBitmapOps.SaveBitmapAsPng(image, outputPath);
        testLog.AddComment("Test image saved to " + outputPath);
    }
}
