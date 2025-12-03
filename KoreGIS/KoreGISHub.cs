// <fileheader>

using System;
using System.IO;

namespace KoreGIS;

public static class KoreGISHub
{
    private static readonly object SyncLock = new object();
    private static bool _isInitialized;

    // Directories and paths
    private static string _mapRootDirectory = string.Empty;
    private static string _elevationDataDirectory = string.Empty;

    // --------------------------------------------------------------------------------------------

    // Patch and Tile Elevation systems
    // Access: KoreGISHub.EleManager
    public static KoreElevationManager EleManager = new KoreElevationManager();

    // Terrain Image Manager
    // Access: KoreGISHub.ImageManager
    public static KoreTerrainImageManager ImageManager = new KoreTerrainImageManager();

    // --------------------------------------------------------------------------------------------

    public static bool IsInitialized
    {
        get
        {
            lock (SyncLock)
            {
                return _isInitialized;
            }
        }
    }

    // --------------------------------------------------------------------------------------------

    public static void Initialize(string mapRootDirectory, string elevationDataDirectory)
    {
        if (string.IsNullOrWhiteSpace(mapRootDirectory))
            throw new ArgumentException("Map root directory cannot be null or empty.", nameof(mapRootDirectory));
        if (string.IsNullOrWhiteSpace(elevationDataDirectory))
            throw new ArgumentException("Elevation data directory cannot be null or empty.", nameof(elevationDataDirectory));

        string resolvedMapPath = Path.GetFullPath(mapRootDirectory);
        string resolvedElevationPath = Path.GetFullPath(elevationDataDirectory);

        if (!Directory.Exists(resolvedMapPath))
            throw new DirectoryNotFoundException($"Map root directory does not exist: {resolvedMapPath}");
        if (!Directory.Exists(resolvedElevationPath))
            throw new DirectoryNotFoundException($"Elevation data directory does not exist: {resolvedElevationPath}");

        lock (SyncLock)
        {
            _mapRootDirectory = resolvedMapPath;
            _elevationDataDirectory = resolvedElevationPath;
            _isInitialized = true;
        }
    }

    // --------------------------------------------------------------------------------------------

    // Exception if not initialized, returns the map root directory.
    // Usage: KoreGISHub.GetMapRootDirectory()

    public static string GetMapRootDirectory()
    {
        lock (SyncLock)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("KoreGISHub has not been initialised with a map root directory yet.");

            return _mapRootDirectory;
        }
    }

    // Exception if not initialized, returns the elevation data directory.
    // Usage: KoreGISHub.GetElevationDataDirectory()

    public static string GetElevationDataDirectory()
    {
        lock (SyncLock)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("KoreGISHub has not been initialised with an elevation data directory yet.");

            return _elevationDataDirectory;
        }
    }
}