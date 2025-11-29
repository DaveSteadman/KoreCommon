

using KoreCommon;

namespace KoreGIS;

public class KoreElevationTile
{
    public KoreLLBox        LLBox;
    public KoreMapTileCode  TileCode = new();

    public KoreNumeric2DArray<float> ElevationData = new KoreNumeric2DArray<float>(10, 10);

    // Zero tile
    public static KoreElevationTile Zero
    {
        get { return new KoreElevationTile(); }
    }

    public float GetElevation(double latDegs, double lonDegs)
    {
        KoreLLPoint pos = new() { LatDegs = latDegs, LonDegs = lonDegs };

        if (LLBox.Contains(pos))
        {
            float latFrac;
            float lonFrac;

            (latFrac, lonFrac) = LLBox.GetLatLonFraction(pos);
            float eleAtFraction = ElevationData.InterpolatedValue(lonFrac, latFrac); // lon first, X Y.

            return eleAtFraction;
        }
        return KoreElevationUtils.InvalidEle;
    }


}
