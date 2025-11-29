
// Static utility class to read and write Kore elevation tiles to file.

using System;
using System.IO;
using System.Text;

using KoreCommon;

namespace KoreGIS;

#nullable enable

public static class KoreElevationTileIO
{
    // --------------------------------------------------------------------------------------------
    // MARK: Text File IO
    // --------------------------------------------------------------------------------------------

    // KoreElevationPatchIO.WriteToTextFile

    public static void WriteToTextFile(KoreElevationTile tile, string filePath)
    {
        try
        {
            string content = WriteToString(tile);
            File.WriteAllText(filePath, content);
        }
        catch (Exception ex)
        {
            KoreCentralLog.AddEntry($"Error writing to text file: {ex.Message}");
        }
    }

    public static KoreElevationTile? ReadFromTextFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                KoreCentralLog.AddEntry($"E10000: File Not Found: {filePath}");
                return null;
            }
            else
            {
                string content = File.ReadAllText(filePath);

                if (string.IsNullOrEmpty(content))
                {
                    KoreCentralLog.AddEntry($"E10001: File Content Not Found: {filePath}");
                    return null;
                }
                return ReadFromString(content);
            }
        }
        catch (Exception ex)
        {
            KoreCentralLog.AddEntry($"EXCEPTION: X1000: Error reading from text file: {ex.Message}");
            return null;
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: String IO
    // --------------------------------------------------------------------------------------------

    public static string WriteToString(KoreElevationTile tile)
    {
        StringBuilder sb = new StringBuilder();

        // Write Tilecode
        sb.AppendLine($"TileCode: {tile.TileCode.ToString()}");
        // Write bounding box
        sb.AppendLine($"BoundingBox: {tile.LLBox.MinLatDegs}, {tile.LLBox.MinLonDegs}, {tile.LLBox.MaxLatDegs}, {tile.LLBox.MaxLonDegs}");
        // Write resolution
        sb.AppendLine($"Resolution: {tile.ElevationData.Width}, {tile.ElevationData.Height}");

        // Write elevation data (2D grid of values, and limit to 2dp for readability and space)
        int numrows = tile.ElevationData.Height;
        for (int i = 0; i < numrows; i++)
        {
            var row = tile.ElevationData.GetRow(i);

            for (int j = 0; j < row.Length; j++)
            {
                sb.Append(row[j].ToString("F2")); // F2 = 2DP on meters of elevation = 1cm accuracy.
                if (j < row.Length - 1) sb.Append(",");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    // --------------------------------------------------------------------------------------------

    public static KoreElevationTile? ReadFromString(string content)
    {
        try
        {
            string[] lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 4) { KoreCentralLog.AddEntry($"ELE: Poor Content: {content}"); return null; }

            // Read the header lines and check the construction / remove the titles
            string[] l1parts = lines[0].Split(':');
            string[] l2parts = lines[1].Split(':');
            string[] l3parts = lines[2].Split(':');

            if (l1parts.Length != 2) { KoreCentralLog.AddEntry($"ELE: Failed Line 1: {lines[0]}"); return null; }
            if (l2parts.Length != 2) { KoreCentralLog.AddEntry($"ELE: Failed Line 2: {lines[1]}"); return null; }
            if (l3parts.Length != 2) { KoreCentralLog.AddEntry($"ELE: Failed Line 3: {lines[2]}"); return null; }

            // Process Line 1 ----------------------
            string tilecodeStr = l1parts[1].Trim();
            //KoreCentralLog.AddEntry($"Line1: {l1parts[0]} // {l1parts[1]}");
            KoreMapTileCode tileCode = KoreMapTileCode.TileCodeFromString(tilecodeStr);
            if (!tileCode.IsValid()) { KoreCentralLog.AddEntry($"Invalid Tilecode"); return null; }
            ;

            // Process Line 2 ----------------------
            string[] l2values = l2parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (l2values.Length != 4) { KoreCentralLog.AddEntry($"ELE: Failed Line 2 Values: {lines[1]}"); return null; }

            double minLatDegs = double.Parse(l2values[0]);
            double minLonDegs = double.Parse(l2values[1]);
            double maxLatDegs = double.Parse(l2values[2]);
            double maxLonDegs = double.Parse(l2values[3]);

            KoreLLBox llBox = new()
            {
                MinLatDegs = minLatDegs,
                MinLonDegs = minLonDegs,
                MaxLatDegs = maxLatDegs,
                MaxLonDegs = maxLonDegs
            };

            // Process Line 3 ----------------------
            string[] l3values = l3parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (l3values.Length != 2) { KoreCentralLog.AddEntry($"ELE: Failed Line 3 Values: {lines[2]}"); return null; }

            int horizRes = int.Parse(l3values[0]);
            int vertRes = int.Parse(l3values[1]);

            // Process data grid -------------------

            // Read elevation data
            int numRows = vertRes; // lines.Length - 3;
            KoreFloat2DArray elevData = new KoreFloat2DArray(horizRes, vertRes);

            for (int i = 0; i < numRows; i++)
            {
                string[] values = lines[i + 3].Split(',', StringSplitOptions.RemoveEmptyEntries);
                KoreNumeric1DArray<float> row = new KoreNumeric1DArray<float>(values.Length);

                for (int j = 0; j < values.Length; j++)
                {
                    row[j] = float.Parse(values[j]);
                }
                elevData.SetRow(i, row);
            }

            return new KoreElevationTile() { ElevationData = elevData, LLBox = llBox, TileCode = tileCode };
        }
        catch (Exception)
        {
            KoreCentralLog.AddEntry($"EXCEPTION: ReadFromString");
            return null; // Return null in case of any errors during parsing.
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Binary File IO
    // --------------------------------------------------------------------------------------------

    public static void WriteToBinaryFile(KoreElevationTile tile, string filePath)
    {
        try
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
            {
                // Write bounding box
                writer.Write(tile.LLBox.MinLatDegs);
                writer.Write(tile.LLBox.MinLonDegs);
                writer.Write(tile.LLBox.MaxLatDegs);
                writer.Write(tile.LLBox.MaxLonDegs);

                // Write resolution
                writer.Write(tile.ElevationData.Width);
                writer.Write(tile.ElevationData.Height);

                // Write elevation data
                int numRows = tile.ElevationData.Height;
                for (int i = 0; i < numRows; i++)
                {
                    var row = tile.ElevationData.GetRow(i);
                    for (int j = 0; j < row.Length; j++)
                    {
                        writer.Write(row[j]);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            KoreCentralLog.AddEntry($"Error writing to binary file: {ex.Message}");
        }
    }

    // --------------------------------------------------------------------------------------------

    public static KoreElevationTile? ReadFromBinaryFile(string filePath)
    {
        try
        {
            using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                // Read bounding box
                double minLat = reader.ReadDouble();
                double minLon = reader.ReadDouble();
                double maxLat = reader.ReadDouble();
                double maxLon = reader.ReadDouble();
                KoreLLBox llBox = new KoreLLBox(minLat, minLon, maxLat, maxLon);

                // Read resolution
                int horizRes = reader.ReadInt32();
                int vertRes = reader.ReadInt32();

                // Read elevation data
                KoreFloat2DArray elevData = new KoreFloat2DArray(horizRes, vertRes);
                for (int i = 0; i < vertRes; i++)
                {
                    KoreNumeric1DArray<float> row = new KoreNumeric1DArray<float>(horizRes);
                    for (int j = 0; j < horizRes; j++)
                    {
                        row[j] = reader.ReadSingle();
                    }
                    elevData.SetRow(i, row);
                }

                return new KoreElevationTile() { ElevationData = elevData, LLBox = llBox };
            }
        }
        catch (Exception ex)
        {
            KoreCentralLog.AddEntry($"Error reading from binary file: {ex.Message}");
            return null;
        }
    }

}



