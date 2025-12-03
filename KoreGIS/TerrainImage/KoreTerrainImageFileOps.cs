using System;
using System.Collections.Generic;

using KoreCommon;
using KoreCommon.SkiaSharp;
using SkiaSharp;

namespace KoreGIS;

#nullable enable

// Static utility class for operations on map tile images, building on top of the KoreSkiaSharpBitmapOps defined in the common code.

public static class KoreTerrainImageFileOps
{
    // --------------------------------------------------------------------------------------------
    // MARK: Tile Image Size
    // --------------------------------------------------------------------------------------------

    // Load the image for a tile, and reduce the size for tiles nearer the poles. They are distorted, save memory space.
    // - Below 70 degrees latitude, the tile image size is 1024x1024 pixels.
    // - Above 70 degrees latitude, the tile image size is reduced to 512x512 pixels.
    // - Above 80 degrees latitude, the tile image size is reduced to 256x256 pixels.
    // - Above 85 degrees latitude, the tile image size is reduced to 128x128 pixels.
    public static void AdjustTileImageSize(KoreMapTileCode tileCode, bool executeResize = false)
    {
        // Find all the filenames
        KoreMapTileFilepaths filepaths = new KoreMapTileFilepaths(tileCode);

        if (filepaths.WebpFileExists)
        {
            // Load the image
            var tileImage = KoreSkiaSharpBitmapOps.LoadBitmap(filepaths.WebpFilepath);
            if (tileImage == null) return;

            KoreLLBox codeLLBox = tileCode.LLBox;
            double latDegs = codeLLBox.MidLatDegs;

            // Determine the new size based on the latitude
            int newSize = 1024; // Default size
            if (latDegs > 70) newSize = 512;
            if (latDegs > 80) newSize = 256;
            if (latDegs > 85) newSize = 128;

            // Resize the image
            var resizedImage = KoreSkiaSharpBitmapOps.ResizeImage(tileImage, newSize, newSize);

            // Is the resize necessary?
            bool matchWidth = KoreValueUtils.IsEqual(resizedImage.Width, tileImage.Width);
            bool matchHeight = KoreValueUtils.IsEqual(resizedImage.Height, tileImage.Height);

            if (!(matchWidth && matchHeight))
            {
                // Debug print the action
                KoreCentralLog.AddEntry($"AdjustTileImageSize: Resizing tile {tileCode} from {tileImage.Width}x{tileImage.Height} to {newSize}x{newSize}");

                // Save the resized image back to the file
                if (executeResize)
                    KoreSkiaSharpBitmapOps.SaveBitmapAsWebp(resizedImage, filepaths.WebpFilepath);
            }
            else
            {
                // Debug print that no resize was needed
                KoreCentralLog.AddEntry($"AdjustTileImageSize: No resize needed for tile {tileCode} ({tileImage.Width}x{tileImage.Height})");
            }
        }
    }

    // // Function to load all the webp images from the child tiles, combining them into a single image, and then resizing it and
    // // saving it out as the parent tile image.

    // // --------------------------------------------------------------------------------------------

    // // Usage: KoreMapFileOps.CollateChildTileImages("AB");
    // public static void CollateChildTileImages(string strParentTileCode)
    // {
    //     // Create the tile code
    //     // Load the parent tile image as a background to fill in for missing child tiles
    //     KoreMapTileCode      parentTileCode  = new KoreMapTileCode(strParentTileCode);
    //     KoreMapTileFilepaths parentFilepaths = new KoreMapTileFilepaths(parentTileCode);

    //     // Load the child tile images
    //     int numChildTilesX = parentTileCode.ChildCodesWidth();
    //     int numChildTilesY = parentTileCode.ChildCodesHeight();

    //     // Determine the working tile image size (1024 X each dimension)
    //     int workingTileSize = 1024;
    //     int workingImageSizeX = workingTileSize * numChildTilesX;
    //     int workingImageSizeY = workingTileSize * numChildTilesY;

    //     // Create the working image
    //     //Image workingImage = Image.CreateEmpty(workingImageSizeX, workingImageSizeY, false, Image.Format.Rgb8);

    //     var workingImage = KoreSkiaSharpBitmapOps.NewImage(workingImageSizeX, workingImageSizeY);


    //     // Load any parent tile image, resize it to the full working image size, and paste it into the working image
    //     if (parentFilepaths.WebpFileExists)
    //     {
    //         // Load the parent tile image (handles several file extensions)
    //         var parentTileImage = KoreSkiaSharpBitmapOps.LoadBitmap(parentFilepaths.WebpFilepath);

    //         //Image? parentTileImage = GloTextureLoader.LoadDirectWebp(parentFilepaths.WebpFilepath);
    //         if (parentTileImage != null)
    //         {
    //             // Ensure the right image size
    //             var resizedBitmap = KoreSkiaSharpBitmapOps.ResizeImage(parentTileImage, workingImageSizeX, workingImageSizeY);

    //             // Paste the parent tile image into the working image
    //             KoreSkiaSharpBitmapOps.PasteSection(workingImage, resizedBitmap, new SKPoint(0, 0));
    //         }
    //     }

    //     List<GloMapTileCode> childCodesList = parentTileCode.ChildCodesList();

    //     foreach (GloMapTileCode currCode in childCodesList)
    //     {
    //         GloMapTileFilepaths currFilepaths = new GloMapTileFilepaths(currCode);

    //         if (currFilepaths.WebpFileExists)
    //         {
    //             var childTileImage = KoreSkiaSharpBitmapOps.LoadBitmap(currFilepaths.WebpFilepath);
    //             if (childTileImage != null)
    //             {
    //                 // Resize the child image to the working tile size
    //                 var resizedChildBitmap = KoreSkiaSharpBitmapOps.ResizeImage(childTileImage, workingTileSize, workingTileSize);

    //                 // Calculate the position to paste the child image
    //                 Glo2DGridPos tilePos = currCode.ChildPositionInParent();
    //                 int topLeftX = tilePos.PosX * workingTileSize;
    //                 int topLeftY = tilePos.PosY * workingTileSize;
    //                 SKPoint pastePos = new SKPoint(topLeftX, topLeftY);

    //                 // Paste the child image into the working image
    //                 KoreSkiaSharpBitmapOps.PasteSection(workingImage, resizedChildBitmap, pastePos);
    //             }
    //         }
    //     }

    //     // Save the working image for analysis
    //     var finalResizedImage = KoreSkiaSharpBitmapOps.ResizeImage(workingImage, workingTileSize, workingTileSize);
    //     KoreSkiaSharpBitmapOps.SaveBitmapAsWebp(finalResizedImage, parentFilepaths.WebpFilepath);
    // }

    // // --------------------------------------------------------------------------------------------

    // // Special case, collating the lvl0 tiles into a single world image, saved to a special case filename
    // public static void CollateChildTileImages_Lvl0()
    // {
    //     int numChildTilesX = 12; // 30x30degree tiles
    //     int numChildTilesY = 6;
    //     int workingTileSize = 1024;

    //     int newImageX = numChildTilesX * workingTileSize;
    //     int newImageY = numChildTilesY * workingTileSize;

    //     Image workingImage = Image.CreateEmpty(newImageX, newImageY, false, Image.Format.Rgb8);

    //     // loop through each of the lvl0 tiles.
    //     for (int currXIndex = 0; currXIndex < numChildTilesX; currXIndex++)
    //     {
    //         for (int currYIndex = 0; currYIndex < numChildTilesY; currYIndex++)
    //         {
    //             // Create the tile code
    //             GloMapTileCode parentTileCode = new GloMapTileCode(currXIndex, currYIndex);
    //             GloMapTileFilepaths parentFilepaths = new GloMapTileFilepaths(parentTileCode);

    //             int destXPos = currXIndex * workingTileSize;
    //             int destYPos = currYIndex * workingTileSize;
    //             Vector2I destPos = new Vector2I(destXPos, destYPos);

    //             // Load any parent tile image, resize it to the full working image size, and paste it into the working image
    //             if (parentFilepaths.WebpFileExists)
    //             {
    //                 Image? parentTileImage = GloTextureLoader.LoadDirectWebp(parentFilepaths.WebpFilepath);

    //                 if (parentTileImage != null)
    //                 {
    //                     parentTileImage!.Resize(workingTileSize, workingTileSize);
    //                     workingImage.BlitRect(parentTileImage, new Rect2I(0, 0, workingTileSize, workingTileSize), destPos);
    //                 }
    //             }
    //         }
    //     }

    //     // Save the working image for analysis
    //     string worldTileFilepath = GloMapTileFilepaths.WorldTileFilepath();
    //     workingImage.SaveWebp(worldTileFilepath);

    // }

    // // --------------------------------------------------------------------------------------------

    // // Function to take one high resolution image, and chreate the images for this tile, and the set of child tiles
    // public static void DivideChildTileImages(string strParentTileCode, string parentTileImagePath)
    // {
    //     // Load the source image
    //     Image? workingImage = GloTextureLoader.LoadDirectWebp(parentTileImagePath);
    //     if (workingImage == null) return;

    //     // Determine the size of the source image, such that we can divide it well.
    //     GloMapTileCode parentTileCode = new GloMapTileCode(strParentTileCode);
    //     GloMapTileFilepaths parentFilepaths = new GloMapTileFilepaths(parentTileCode);

    //     // Determine the working tile image size (1024 X each dimension)
    //     int numChildTilesX = parentTileCode.ChildCodesWidth();
    //     int numChildTilesY = parentTileCode.ChildCodesHeight();
    //     int workingTileSize = 1024;
    //     int workingImageSizeX = workingTileSize * numChildTilesX;
    //     int workingImageSizeY = workingTileSize * numChildTilesY;

    //     // resize the working image
    //     workingImage!.Resize(workingImageSizeX, workingImageSizeY);

    //     // Get the child tiles, and iterate through them saving out the new tiles
    //     List<GloMapTileCode> childCodesList = parentTileCode.ChildCodesList();
    //     foreach (GloMapTileCode currCode in childCodesList)
    //     {
    //         GloMapTileFilepaths currFilepaths = new GloMapTileFilepaths(currCode);

    //         // Calculate the position to paste the child image
    //         Glo2DGridPos tilePos = currCode.ChildPositionInParent();
    //         int topLeftX = tilePos.PosX * workingTileSize;
    //         int topLeftY = tilePos.PosY * workingTileSize;
    //         Rect2I copyRect = new Rect2I(topLeftX, topLeftY, workingTileSize, workingTileSize);

    //         // Create the child image
    //         Image childImage = workingImage.GetRegion(copyRect);

    //         // Save the child image
    //         childImage.SaveWebp(currFilepaths.WebpFilepath);
    //     }

    //     // Resize the working image and save it out save the parent tile image
    //     workingImage.Resize(workingTileSize, workingTileSize);
    //     workingImage.SaveWebp(parentFilepaths.WebpFilepath);
    // }

    // // --------------------------------------------------------------------------------------------

    // // Special case: Takes a new 2:1 aspect ratio world image and divides it into the 12x6 lvl0 tiles
    // // Usage: GloMapFileOperations.DivideChildTileImages_Lvl0(worldimagePath);

    // public static void DivideChildTileImages_Lvl0(string parentTileImagePath)
    // {
    //     // Load the source image
    //     Image? workingImage = GloTextureLoader.LoadDirectWebp(parentTileImagePath);
    //     if (workingImage == null) return;

    //     // Determine the size of the source image, such that we can divide it well.
    //     int numChildTilesX = 12; // 30x30degree tiles
    //     int numChildTilesY = 6;
    //     int workingTileSize = 1024;
    //     int workingImageSizeX = workingTileSize * numChildTilesX;
    //     int workingImageSizeY = workingTileSize * numChildTilesY;

    //     // resize the working image, to make the division cleaner
    //     workingImage!.Resize(workingImageSizeX, workingImageSizeY);

    //     // Get the child tiles, and iterate through them saving out the new tiles
    //     for (int currXIndex = 0; currXIndex < numChildTilesX; currXIndex++)
    //     {
    //         for (int currYIndex = 0; currYIndex < numChildTilesY; currYIndex++)
    //         {
    //             GloMapTileCode currCode = new GloMapTileCode(currXIndex, currYIndex);

    //             // Calculate the position to paste the child image
    //             Glo2DGridPos tilePos = currCode.ChildPositionInParent();
    //             int topLeftX = tilePos.PosX * workingTileSize;
    //             int topLeftY = tilePos.PosY * workingTileSize;
    //             Rect2I copyRect = new Rect2I(topLeftX, topLeftY, workingTileSize, workingTileSize);

    //             // Create the child image
    //             Image childImage = workingImage.GetRegion(copyRect);

    //             // Save the child image
    //             GloMapTileFilepaths currFilepaths = new GloMapTileFilepaths(currCode);
    //             childImage.SaveWebp(currFilepaths.WebpFilepath);
    //         }
    //     }
    // }

    // // --------------------------------------------------------------------------------------------

    // // Function to load the image for this tile, and the set of child tiles
    // // Usage: Image[,] childImages = GloMapFileOperations.DivideIntoChildTileImages(tileCode, parentImage);

    // public static Image[,] DivideIntoChildTileImages(GloMapTileCode parentTileCode, Image parentImage)
    // {
    //     // Determine the working tile image size (1024 X each dimension)
    //     int numChildTilesX = parentTileCode.ChildCodesWidth();
    //     int numChildTilesY = parentTileCode.ChildCodesHeight();
    //     int workingTileSize = 1024;
    //     int workingImageSizeX = workingTileSize * numChildTilesX;
    //     int workingImageSizeY = workingTileSize * numChildTilesY;

    //     // Resize the working image to a multiple of the output tile size, so the division is cleaner
    //     parentImage.Resize(workingImageSizeX, workingImageSizeY);

    //     // Get the child tiles, and iterate through them saving out the new tiles
    //     List<GloMapTileCode> childCodesList = parentTileCode.ChildCodesList();
    //     Image[,] childImages = new Image[numChildTilesX, numChildTilesY];

    //     foreach (GloMapTileCode currCode in childCodesList)
    //     {
    //         // Calculate the position to paste the child image
    //         Glo2DGridPos tilePos = currCode.ChildPositionInParent();
    //         int topLeftX = tilePos.PosX * workingTileSize;
    //         int topLeftY = tilePos.PosY * workingTileSize;
    //         Rect2I copyRect = new Rect2I(topLeftX, topLeftY, workingTileSize, workingTileSize);

    //         // Create the child image
    //         Image childImage = parentImage.GetRegion(copyRect);
    //         childImages[tilePos.PosX, tilePos.PosY] = childImage;
    //     }

    //     return childImages;
    // }

    // // --------------------------------------------------------------------------------------------

}


