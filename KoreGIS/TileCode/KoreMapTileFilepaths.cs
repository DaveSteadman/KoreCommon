using System;
using System.Collections.Generic;
using System.IO;

// KoreMapTileFilepaths class to figure out and store the filepaths of assets for a map tile.

using KoreCommon;

namespace KoreGIS;

public class KoreMapTileFilepaths
{
    public string EleFilepath    { get; set; }
    public string EleArrFilepath { get; set; }
    public string MeshFilepath   { get; set; }
    public string ImageFilepath  { get; set; }
    public string WebpFilepath   { get; set; }
    //public string WebpAnalysisFilepath     { get; set; }

    public bool EleFileExists    { get; set; } = false;
    public bool EleArrFileExists { get; set; } = false;
    public bool MeshFileExists   { get; set; } = false;
    public bool ImageFileExists  { get; set; } = false;
    public bool WebpFileExists   { get; set; } = false;

    public static readonly string[] PathPerLvl = { "Lvl0_30x30", "Lvl1_5x5", "Lvl2_1x1", "Lvl3_0p2x0p2", "Lvl4_0p04x0p04", "Lvl5_0p008x0p008" };

    public KoreMapTileFilepaths(KoreMapTileCode tileCode)
    {
        // Get the tile code name
        string tileCodeName = tileCode.ToString();

        string maplibRootDir = KoreGISHub.GetMapRootDirectory();

        string externalMapLvlFilePath = KoreFileOps.JoinPaths(maplibRootDir, PathPerLvl[tileCode.MapLvl]);
        if (tileCode.MapLvl != 0)
            externalMapLvlFilePath = KoreFileOps.JoinPaths(externalMapLvlFilePath, tileCode.ParentString());

        // Setup the file paths
        EleFilepath    = KoreFileOps.JoinPaths(externalMapLvlFilePath, $"Ele_{tileCodeName}.asc");
        EleArrFilepath = KoreFileOps.JoinPaths(externalMapLvlFilePath, $"Ele_{tileCodeName}.arr");
        MeshFilepath   = KoreFileOps.JoinPaths(externalMapLvlFilePath, $"Mesh_{tileCodeName}.mesh");
        ImageFilepath  = KoreFileOps.JoinPaths(externalMapLvlFilePath, $"Sat_{tileCodeName}.png");
        WebpFilepath   = KoreFileOps.JoinPaths(externalMapLvlFilePath, $"Sat_{tileCodeName}.webp");
        //WebpAnalysisFilepath     = KoreFileOps.JoinPaths(externalMapLvlFilePath, $"Sat_{tileCodeName}_check.webp");

        // Check if the files exist
        EleFileExists    = File.Exists(EleFilepath);
        EleArrFileExists = File.Exists(EleArrFilepath);
        MeshFileExists   = File.Exists(MeshFilepath);
        ImageFileExists  = File.Exists(ImageFilepath);
        WebpFileExists   = File.Exists(WebpFilepath);
    }

    public void InheritImageFilepaths(KoreMapTileFilepaths parentFilepaths)
    {
        ImageFilepath   = parentFilepaths.ImageFilepath;
        WebpFilepath    = parentFilepaths.WebpFilepath;
        ImageFileExists = parentFilepaths.ImageFileExists;
        WebpFileExists  = parentFilepaths.WebpFileExists;
    }

}


