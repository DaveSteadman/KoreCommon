// <fileheader>

#nullable enable

using System.IO;

using KoreCommon;

namespace KoreGIS;

// Partial class for writing PRJ (projection) files
public static partial class KoreShapefileWriter
{
    // WGS84 projection definition in WKT format.
    private const string Wgs84Prj = @"GEOGCS[""GCS_WGS_1984"",DATUM[""D_WGS_1984"",SPHEROID[""WGS_1984"",6378137.0,298.257223563]],PRIMEM[""Greenwich"",0.0],UNIT[""Degree"",0.0174532925199433]]";

    // Writes the PRJ file with WGS84 definition.
    private static void WritePrj(string prjPath)
    {
        File.WriteAllText(prjPath, Wgs84Prj);
    }
}

