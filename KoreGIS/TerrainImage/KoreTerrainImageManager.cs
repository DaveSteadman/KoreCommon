// <fileheader>

using System.Collections.Generic;
using System.IO;
using KoreCommon;

namespace KoreGIS;

public class KoreTerrainImageManager
{
    public List<KoreTerrainImageTile> TerrainImageTileList = new();

    public static KoreColorRGB DefaultColor = KoreColorPalette.Find("Magenta");

    // --------------------------------------------------------------------------------------------

    public (bool, string) LoadTile(KoreLLBox llBox, string imageFilename)
    {
        string imagePath = KoreFileOps.StandardizePath(imageFilename);

        if (!File.Exists(imagePath))
        {
            return (false, $"File not found: {imagePath}");
        }

        KoreTerrainImageTile newTile = new(llBox, imagePath);
        if (newTile.IsValid())
        {
            TerrainImageTileList.Add(newTile);
            SortTilesByResolution();
            return (true, $"- Loaded image: {imagePath}");
        }

        return (false, $"- Failed to load image: {imagePath}");
    }

    // --------------------------------------------------------------------------------------------

    // Sort the tiles by resolution (pixels per degree), highest first
    public void SortTilesByResolution()
    {
        TerrainImageTileList.Sort((a, b) => b.PixelsPerDegree.CompareTo(a.PixelsPerDegree));

        // debug print the tiles in order
        foreach (KoreTerrainImageTile tile in TerrainImageTileList)
        {
            string resStr = $"{tile.PixelsPerDegree:0.00} ppd - {tile.ImagePath}";
            KoreCentralLog.AddEntry(resStr);
        }
    }

    // --------------------------------------------------------------------------------------------

    public KoreColorRGB ColorForPoint(KoreLLPoint llPoint)
    {
        // Loop through the tiles. They are maintained in reolsution order, so the first match
        // with the highest pixels per degree is the best match.
        foreach (KoreTerrainImageTile tile in TerrainImageTileList)
        {
            if (tile.ContainsPoint(llPoint))
            {
                KoreColorRGB tileColor = tile.GetTerrainColor(llPoint);
                return tileColor;
            }
        }

        // Return an obvious no-data-found color
        return DefaultColor;
    }

}


