using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#nullable enable

using KoreCommon;

namespace KoreGIS;

// KoreMapTileCodeOps:
public static class KoreMapTileCodeOps
{
    // With a UV position 0,0 in the top-left, and 1,1 in the bottom-right, get the UV box for the tile
    // Usage: KoreUVBox uvBox = KoreMapTileCodeOps.TileGlobalUVBox(tileCode);
    public static KoreUVBox TileGlobalUVBox(KoreMapTileCode tileCode)
    {
        // Get the LL box for the tile
        KoreLLBox boundsbox = tileCode.LLBox;

        // Get the corner values and step sizes
        double latMaxDegs = boundsbox.MaxLatDegs; // Northern edge
        double latMinDegs = boundsbox.MinLatDegs; // Southern edge
        double lonMinDegs = boundsbox.MinLonDegs; // Western edge
        double lonMaxDegs = boundsbox.MaxLonDegs; // Eastern edge

        // Ensure the latitudes are the correct way round
        if (latMaxDegs < latMinDegs)
        {
            double temp = latMaxDegs;
            latMaxDegs = latMinDegs;
            latMinDegs = temp;
        }

        // Work out the UVs for the tile
        // Horizontally, U goes from 0.0 (west) to 1.0 (east)
        // Vertically, V goes from 0.0 (northpole) to 1.0 (southpole)
        double northV = KoreNumericUtils.ScaleToUncheckedRange(latMaxDegs, 90, -90, 0, 1);
        double southV = KoreNumericUtils.ScaleToUncheckedRange(latMinDegs, 90, -90, 0, 1);
        double westU  = KoreNumericUtils.ScaleToRange(lonMinDegs, -180, 180, 0, 1);
        double eastU  = KoreNumericUtils.ScaleToRange(lonMaxDegs, -180, 180, 0, 1);

        KoreCentralLog.AddEntry($"KoreMapTileCodeOps.TileGlobalUVBox: Tile {tileCode} UVs: W={westU:F4}, E={eastU:F4}, N={northV:F4}, S={southV:F4}");

        return new KoreUVBox(westU, northV, eastU, southV);
    }
}
