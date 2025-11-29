using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#nullable enable

using KoreCommon;

namespace KoreGIS;

// KoreMapTileCode: A tile code class, dealing solely with the definition of a hierachical grid format.

public struct KoreLevelCodeElement
{
    public int LatIndex;
    public int LonIndex;

    public string CodeString()
    {
        int latId = KoreValueUtils.Clamp(LatIndex, 0, 19);
        int lonId = KoreValueUtils.Clamp(LonIndex, 0, 19);
        return $"{KoreMapTileCode.LetterLookup[latId]}{KoreMapTileCode.LetterLookup[lonId]}";
    }
}

public class KoreMapTileCode
{
    // --------------------------------------------------------------------------------------------
    // Constants
    // --------------------------------------------------------------------------------------------

    public static readonly int MaxMapLvl = 5;

    public static readonly int[]    NumTilesVertPerLvl  = {   6, 6, 5, 5, 5, 5};
    public static readonly int[]    NumTilesHorizPerLvl = {  12, 6, 5, 5, 5, 5};
    public static readonly double[] TileSizeDegsPerLvl  = {  30, 5, 1, 0.2, 0.04, 0.008 };

    // Tile code string constants - Letter vertical from top-left down, Letter horizontal from top-left right. eg AB_CD
    public static readonly int MinTileCodeStrLen =  2;
    public static readonly int MaxTileCodeStrLen = 20;
    public static readonly char CodeSeparator  = '_';
    public static readonly char[] LetterLookup = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J' , 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T'};

    public static string InvalidTileCode = "ZEROCODE";

    // --------------------------------------------------------------------------------------------
    // MARK: Properties
    // --------------------------------------------------------------------------------------------

    public List<KoreLevelCodeElement> CodeList { get; private set; } = new();

    public bool IsValid()
    {
        if (CodeList.Count == 0) return false;
        if (ToString() == InvalidTileCode) return false;

        return true;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Derived Properties
    // --------------------------------------------------------------------------------------------

    public int MapLvl     => (CodeList.Count - 1);

    public int HorizCount => NumTilesHorizPerLvl[MapLvl];
    public int VertCount  => NumTilesVertPerLvl[MapLvl];

    public int HorizIndex => CodeList[MapLvl].LonIndex;
    public int VertIndex  => CodeList[MapLvl].LatIndex;

    public Kore2DGridPos GridPos => new Kore2DGridPos(HorizCount, VertCount, HorizIndex, VertIndex);

    public KoreLLBox LLBox => LLBoxForCode(this);
    public string TileCode => ToString();

    // --------------------------------------------------------------------------------------------
    // MARK: Constructor - Basic
    // --------------------------------------------------------------------------------------------

    public KoreMapTileCode() {}

    public KoreMapTileCode(int x, int y)
    {
        AddLevelCode(x, y, 0);
    }

    public KoreMapTileCode(KoreMapTileCode parentCode)
    {
        CodeList = new List<KoreLevelCodeElement>(parentCode.CodeList);
    }

    public KoreMapTileCode(string inStrCode)
    {
        KoreMapTileCode newTileCode = TileCodeFromString(inStrCode);
        CodeList = new List<KoreLevelCodeElement>(newTileCode.CodeList);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Constructor - Default
    // --------------------------------------------------------------------------------------------

    public static KoreMapTileCode UndefinedTileCode()
    {
        return new KoreMapTileCode();
    }

    public static KoreMapTileCode Zero
    {
        get { return new KoreMapTileCode(); }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Constructor - Lvl Num
    // --------------------------------------------------------------------------------------------

    public static KoreMapTileCode? CodeToLvl(KoreMapTileCode sourceCode, int toLvl)
    {
        if (sourceCode.IsValid() == false) return null;
        if (sourceCode.MapLvl > toLvl) return null;

        KoreMapTileCode retCode = new KoreMapTileCode();

        for (int i = 0; i < toLvl; i++)
        {
            KoreLevelCodeElement l = sourceCode.CodeList[i];
            retCode.CodeList.Add(l);

        }
        return retCode;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Constructor - Lvl XY
    // --------------------------------------------------------------------------------------------

    // We add the next level to the internal list, the level is used only for validation.

    public void AddLevelCode(int x, int y, int level)
    {
        // if (x < 0) x = 0;
        // if (y < 0) y = 0;
        // if (x >= NumTilesHorizPerLvl[level]) x = NumTilesHorizPerLvl[level] - 1;
        // if (y >= NumTilesVertPerLvl[level])  y = NumTilesVertPerLvl[level] - 1;

        KoreLevelCodeElement l = new KoreLevelCodeElement() { LatIndex = y, LonIndex = x };
        CodeList.Add(l);
    }

    public void AddLevelCode(int x, int y)
    {
        int nextMapLvl = MapLvl + 1;
        AddLevelCode(x, y, nextMapLvl);
    }

    public void SetLevelCode(int x, int y, int level)
    {
        // Basic level check - Levels are 0-based, so inputs will be values 0, 1, 2, 3, 4. 0 == 1 level in the code.
        if (CodeList.Count < (level+1)) throw new InvalidOperationException("Invalid level");

        if (x < 0) x = 0;
        if (y < 0) y = 0;
        if (x >= NumTilesHorizPerLvl[level]) x = NumTilesHorizPerLvl[level] - 1;
        if (y >= NumTilesVertPerLvl[level])  y = NumTilesVertPerLvl[level] - 1;

        CodeList[level] = new KoreLevelCodeElement() { LatIndex = y, LonIndex = x };
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Constructor - String
    // --------------------------------------------------------------------------------------------

    public static KoreMapTileCode TileCodeFromString(string inStrCode)
    {
        // Create a blank tilecode.
        KoreMapTileCode newTileCode = new();

        // Trim off any leading/trailing whitespace and remove non-alphanumeric and non-underscore characters
        inStrCode = Regex.Replace(inStrCode.Trim(), @"[^A-Za-z0-9_]", "");

        // A tile code is typically in the format AA_AA_AA. Split the string into its main level parts.
        string[] parts = inStrCode.Split('_');

        // Each part has to be two characters long
        foreach (var part in parts)
        {
            if (part.Length != 2)
            {
                KoreCentralLog.AddEntry($"Failed to parse: <{inStrCode}> // <{part}>");
                return newTileCode;
            }
        }

        // Each character has to be a letter we turn into an index.
        foreach (var part in parts)
        {
            // Convert and validate on the first two characters being letters
            int firstIndex  = KoreStringOps.NumberForLetter(part[0]);
            int secondIndex = KoreStringOps.NumberForLetter(part[1]);
            if (firstIndex == -1 || secondIndex == -1)
                return newTileCode;

            newTileCode.AddLevelCode(secondIndex, firstIndex); // Note the letter ordering is reversed. BF is 2nd row (Y), then 6th column (X)
        }
        return newTileCode;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Constructor - Lat Lon
    // --------------------------------------------------------------------------------------------

    public KoreMapTileCode(double latDegs, double lonDegs, int mapLevel)
    {
        if (mapLevel < 0 || mapLevel >= MaxMapLvl)
            throw new ArgumentOutOfRangeException(nameof(mapLevel), $"Map level must be between 0 and {MaxMapLvl - 1}");

        double tileSizeDegs = TileSizeDegsPerLvl[0];
        double topLeftLatDegs = 90;
        double topLeftLonDegs = -180;

        for (int level = 0; level <= mapLevel; level++)
        {
            int latIndex = (int)Math.Floor((topLeftLatDegs - latDegs) / TileSizeDegsPerLvl[level]);
            int lonIndex = (int)Math.Floor((lonDegs - topLeftLonDegs) / TileSizeDegsPerLvl[level]);

            latIndex = Math.Clamp(latIndex, 0, NumTilesVertPerLvl[level]  - 1);
            lonIndex = Math.Clamp(lonIndex, 0, NumTilesHorizPerLvl[level] - 1);

            AddLevelCode(lonIndex, latIndex, level);

            topLeftLatDegs -= latIndex * TileSizeDegsPerLvl[level];
            topLeftLonDegs += lonIndex * TileSizeDegsPerLvl[level];
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Parent / Child Codes
    // --------------------------------------------------------------------------------------------

    public KoreMapTileCode ChildCode(int x, int y)
    {
        KoreMapTileCode newCode = new KoreMapTileCode(this);

        int newLvl = newCode.CodeList.Count;

        if (newLvl >= MaxMapLvl) throw new InvalidOperationException("Maximum map level exceeded");

        newCode.AddLevelCode(x, y, newLvl);

        return newCode;
    }

    public KoreMapTileCode ParentCode()
    {
        if (CodeList.Count == 0) return new KoreMapTileCode();

        KoreMapTileCode newCode = new KoreMapTileCode(this);
        newCode.CodeList.RemoveAt(newCode.CodeList.Count - 1);

        return newCode;
    }

    // --------------------------------------------------------------------------------------------

    // Create a list of child tile codes for this code, to help the creation of a tiles' children.

    public List<KoreMapTileCode> ChildCodesList()
    {
        List<KoreMapTileCode> childCodes = new List<KoreMapTileCode>();
        int newLvl = CodeList.Count;

        // We're just returning an empty list if we're at the max level
        if (newLvl <= MaxMapLvl)
        {
            for (int y = 0; y < NumTilesVertPerLvl[newLvl]; y++)
            {
                for (int x = 0; x < NumTilesHorizPerLvl[newLvl]; x++)
                {
                    KoreMapTileCode newCode = new KoreMapTileCode(this);
                    newCode.AddLevelCode(x, y, newLvl);
                    childCodes.Add(newCode);
                }
            }
        }
        return childCodes;
    }

    // Get the dimensions of the child codes list

    public int ChildCodesWidth()
    {
        if (MapLvl >= MaxMapLvl) return 0;
        return NumTilesHorizPerLvl[MapLvl + 1];
    }

    public int ChildCodesHeight()
    {
        if (MapLvl >= MaxMapLvl) return 0;
        return NumTilesVertPerLvl[MapLvl + 1];
    }

    public Kore2DGridPos ChildPositionInParent()
    {
        return new Kore2DGridPos(HorizCount, VertCount, HorizIndex, VertIndex);
    }

    // --------------------------------------------------------------------------------------------

    // Utility function to create all the lvl0 codes
    // Usage: List<KoreMapTileCode> lvl0Codes = KoreMapTileCode.Lvl0Codes();
    public static List<KoreMapTileCode> Lvl0Codes()
    {
        List<KoreMapTileCode> lvl0Codes = new List<KoreMapTileCode>();

        // loop through the lat and long ranges, adding each tilecode to the list
        for (int latId = 0; latId < NumTilesVertPerLvl[0]; latId++)
        {
            for (int lonId = 0; lonId < NumTilesHorizPerLvl[0]; lonId++)
                lvl0Codes.Add(new KoreMapTileCode(lonId, latId));
        }

        return lvl0Codes;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: String outputs
    // --------------------------------------------------------------------------------------------

    public override string ToString()
    {
        if (CodeList.Count == 0)
            return "ZEROCODE";
        return string.Join(CodeSeparator.ToString(), CodeList.ConvertAll(c => c.CodeString()));
    }

    public string ParentString()
    {
        if (CodeList.Count == 0) return "";
        return string.Join(CodeSeparator.ToString(), CodeList.GetRange(0, CodeList.Count - 1).ConvertAll(c => c.CodeString()));
    }

    // KoreMapTileCode.CodeForIndex(0, 0) => "AA"
    public static string CodeForIndex(int y, int x)
    {
        return $"{LetterLookup[y]}{LetterLookup[x]}";
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Lat Longs
    // --------------------------------------------------------------------------------------------

    // Usage: KoreLLBox tileBounds = KoreMapTileCode.LLBoxForCode(tileCode);

    public static KoreLLBox LLBoxForCode(KoreMapTileCode tileCode)
    {
        KoreLLBox llbox = new KoreLLBox();

        double topLeftLatDegs = 90;
        double topLeftLonDegs = -180;

        int currLvl = 0;
        foreach (KoreLevelCodeElement currCodeLevel in tileCode.CodeList)
        {
            double deltaLatDegs = TileSizeDegsPerLvl[currLvl] * currCodeLevel.LatIndex;
            double deltaLonDegs = TileSizeDegsPerLvl[currLvl] * currCodeLevel.LonIndex;

            topLeftLatDegs -= deltaLatDegs;
            topLeftLonDegs += deltaLonDegs;

            currLvl++;
        }

        double finalTileSizeDegs = TileSizeDegsPerLvl[tileCode.MapLvl];
        llbox.MaxLatDegs = topLeftLatDegs;
        llbox.MinLatDegs = topLeftLatDegs - finalTileSizeDegs;
        llbox.MinLonDegs = topLeftLonDegs;
        llbox.MaxLonDegs = topLeftLonDegs + finalTileSizeDegs;

        return llbox;
    }
}
