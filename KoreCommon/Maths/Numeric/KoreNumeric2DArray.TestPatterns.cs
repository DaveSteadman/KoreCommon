// <fileheader>


// global using KoreFloat2DArray  = KoreNumeric2DArray<float>;
// global using KoreDouble2DArray = KoreNumeric2DArray<double>;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using KoreGIS;

namespace KoreCommon;

public partial class KoreNumeric2DArray<T> where T : struct, INumber<T>
{
    // Create a test pattern array, to detail the specific LL values in an interpolated grid, so we can start to see where calculations are going wrong
    // Odd lines are longitude and even lines are latitude, interpolated across the llbox values
    //
    // Usage example: KoreNumeric2DArray<float> testPattern = KoreNumeric2DArray<float>.LLATestPattern(4008, 2004, new KoreLLBox(-180.0, -90.0, 180.0, 90.0));
    //
    // Note: Need to have the array upside down, so the top left corner is the top left corner of the map
    static public KoreNumeric2DArray<T> LLATestPattern(int width, int height, KoreLLBox llBox)
    {
        KoreNumeric2DArray<T> retGrid = new KoreNumeric2DArray<T>(width, height);

        double minLatDegs = llBox.MinLatDegs;
        double latIncDegs = llBox.DeltaLatDegs / (height - 1);

        double minLonDegs = llBox.MinLonDegs;
        double lonIncDegs = llBox.DeltaLonDegs / (width - 1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Check if y is odd or even
                if (y % 2 == 0)
                    retGrid[x, y] = T.CreateChecked(minLatDegs + (y * latIncDegs));
                else
                    retGrid[x, y] = T.CreateChecked(minLonDegs + (x * lonIncDegs));
            }
        }

        KoreNumeric2DArray<T> retGridrev = retGrid.ReverseRows();

        return retGridrev;
    }


    static public KoreNumeric2DArray<T> LatTestPattern(int width, int height, KoreLLBox llBox)
    {
        KoreNumeric2DArray<T> retGrid = new KoreNumeric2DArray<T>(width, height);

        double minLatDegs = llBox.MinLatDegs;
        double latIncDegs = llBox.DeltaLatDegs / (height - 1);

        double minLonDegs = llBox.MinLonDegs;
        double lonIncDegs = llBox.DeltaLonDegs / (width - 1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                retGrid[x, y] = T.CreateChecked(minLatDegs + (y * latIncDegs));
            }
        }

        KoreNumeric2DArray<T> retGridrev = retGrid.ReverseRows();

        return retGridrev;
    }

    static public KoreNumeric2DArray<T> LonTestPattern(int width, int height, KoreLLBox llBox)
    {
        KoreNumeric2DArray<T> retGrid = new KoreNumeric2DArray<T>(width, height);

        double minLatDegs = llBox.MinLatDegs;
        double latIncDegs = llBox.DeltaLatDegs / (height - 1);

        double minLonDegs = llBox.MinLonDegs;
        double lonIncDegs = llBox.DeltaLonDegs / (width - 1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                retGrid[x, y] = T.CreateChecked(minLonDegs + (x * lonIncDegs));
            }
        }

        KoreNumeric2DArray<T> retGridrev = retGrid.ReverseRows();

        return retGridrev;
    }


}
