
// Static utility class to read and write Kore elevation patches to file.


using System;
using System.IO;
using System.Text;
using System.Linq;

using KoreCommon;

namespace KoreGIS;

#nullable enable

public static class KoreElevationPatchIO
{
    // --------------------------------------------------------------------------------------------
    // MARK: Constants
    // --------------------------------------------------------------------------------------------

    public const string ElevationTileExtension = ".elepatch";

    // --------------------------------------------------------------------------------------------
    // MARK: Text File IO
    // --------------------------------------------------------------------------------------------

    // KoreElevationPatchIO.WriteToTextFile

    public static void WriteToTextFile(KoreElevationPatch patch, string filePath)
    {
        try
        {
            string content = WriteToString(patch);
            File.WriteAllText(filePath, content);
        }
        catch (Exception ex)
        {
            KoreCentralLog.AddEntry($"Error writing to text file: {ex.Message}");
        }
    }

    public static KoreElevationPatch? ReadFromTextFile(string filePath)
    {
        try
        {
            string content = File.ReadAllText(filePath);
            return ReadFromString(content);
        }
        catch (Exception ex)
        {
            KoreCentralLog.AddEntry($"Error reading from text file: {ex.Message}");
            return null;
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: String IO
    // --------------------------------------------------------------------------------------------

    public static string WriteToString(KoreElevationPatch patch)
    {
        StringBuilder sb = new StringBuilder();

        // Write bounding box
        sb.AppendLine($"BoundingBox: {patch.LLBox.MinLatDegs:F5}, {patch.LLBox.MinLonDegs:F5}, {patch.LLBox.MaxLatDegs:F5}, {patch.LLBox.MaxLonDegs:F5}");

        // Write resolution
        sb.AppendLine($"Resolution: {patch.ElevationData.Width}, {patch.ElevationData.Height}");

        // Write elevation data (2D grid of values, and limit to 2dp for readability and space)
        int numrows = patch.ElevationData.Height;
        for (int i = 0; i < numrows; i++)
        {
            var row = patch.ElevationData.GetRow(i);
            for (int j = 0; j < row.Length; j++)
            {
                sb.Append(row[j].ToString("F2"));
                if (j < row.Length - 1) sb.Append(", ");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    // --------------------------------------------------------------------------------------------

    public static KoreElevationPatch? ReadFromString(string content)
    {
        try
        {
            string[] lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            // Read bounding box
            string[] l1parts = lines[0].Split(':'); // Split on the ":" to remove the row title
            if (l1parts.Length != 2)
            {
                KoreCentralLog.AddEntry("Invalid bounding box format in elevation patch string.");
                return null;
            }
            string[] l1values = l1parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries); // split on "," to get the 4 BBox values
            if (l1values.Length != 4)
            {
                KoreCentralLog.AddEntry("Invalid bounding box values in elevation patch string.");
                return null;
            }

            // Parse the bounding box values out of the strings
            double minLatDegs = double.Parse(l1values[0]);
            double minLonDegs = double.Parse(l1values[1]);
            double maxLatDegs = double.Parse(l1values[2]);
            double maxLonDegs = double.Parse(l1values[3]);

            KoreLLBox llBox = new KoreLLBox()
            {
                MinLatDegs = minLatDegs,
                MinLonDegs = minLonDegs,
                MaxLatDegs = maxLatDegs,
                MaxLonDegs = maxLonDegs
            };

            // Read resolution
            string[] l2parts = lines[1].Split(':'); // Split the ":" to remove the title
            if (l2parts.Length != 2)
            {
                KoreCentralLog.AddEntry("Invalid resolution format in elevation patch string.");
                return null;
            }
            string[] l2values = l2parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries); // split to get the lat and long point counts
            if (l2values.Length != 2)
            {
                KoreCentralLog.AddEntry("Invalid resolution values in elevation patch string.");
                return null;
            }

            int horizRes = int.Parse(l2values[0]);
            int vertRes  = int.Parse(l2values[1]);

            // Read elevation data
            int numActualDataRows = lines.Length - 2;
            KoreFloat2DArray elevData = new KoreFloat2DArray(horizRes, vertRes);

            // Check we have enough rows to traverse: numActualDataRows + 2 is the number of data rows plus the header
            if (vertRes != numActualDataRows)
            {
                KoreCentralLog.AddEntry($"Invalid number of elevation data rows in elevation patch string: {numActualDataRows} vs {vertRes}");
                return null;
            }

            for (int i = 0; i < vertRes; i++)
            {
                string[] values = lines[i + 2].Split(',', StringSplitOptions.RemoveEmptyEntries);

                //KoreCentralLog.AddEntry($"Line: {i} // {i + 2} of {numActualDataRows} // Values: {values.Length} vs {horizRes}");

                // Check we have the right number of values in each row
                if (values.Length != horizRes)
                {
                    KoreCentralLog.AddEntry($"Invalid number of elevation values in row {i + 2} of elevation patch string: {values.Length} vs {horizRes}");
                    return null;
                }

                KoreNumeric1DArray<float> row = new KoreNumeric1DArray<float>(values.Length);
                for (int j = 0; j < values.Length; j++)
                {
                    row[j] = float.Parse(values[j]);
                }
                elevData.SetRow(i, row);
            }

            return new KoreElevationPatch() { ElevationData = elevData, LLBox = llBox };
        }
        catch (Exception)
        {
            KoreCentralLog.AddEntry("Error parsing elevation patch string.");
            return null; // Return null in case of any errors during parsing.
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Large String IO
    // --------------------------------------------------------------------------------------------

    // Could be too big to put into a string, so append to a file incrementally

    public static void WriteToTextFileIncrementally(KoreElevationPatch patch, string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // Write bounding box
            writer.WriteLine($"BoundingBox: {patch.LLBox.MinLatDegs:F5}, {patch.LLBox.MinLonDegs:F5}, {patch.LLBox.MaxLatDegs:F5}, {patch.LLBox.MaxLonDegs:F5}");

            // Write resolution
            writer.WriteLine($"Resolution: {patch.ElevationData.Width}, {patch.ElevationData.Height}");

            // Write elevation data (streaming row by row to avoid high memory usage)
            int numRows = patch.ElevationData.Height;
            for (int i = 0; i < numRows; i++)
            {
                var row = patch.ElevationData.GetRow(i);

                // Format the numbers as F2 (cm resolution on the elevation) and join them with commas
                writer.WriteLine(string.Join(", ", row.Select(value => value.ToString("F2"))));
            }
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Arc ASII Grid
    // --------------------------------------------------------------------------------------------

    /*
        ncols         315
        nrows         270
        xllcorner     -491880.41
        yllcorner     5849212.228
        cellsize      200
        nodata_value  -9999.0
        <space separated data>
    */

    // Usage: KoreFloat2DArray asciiArcArry = KoreFloat2DArrayIO.LoadFromArcASIIGridFile(<filename>);

    public static KoreFloat2DArray LoadFromArcASIIGridFile(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);

        // Parse header
        int inwidth = int.Parse(lines[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]);
        int inheight = int.Parse(lines[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]);
        float xllcorner = float.Parse(lines[2].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]);
        float yllcorner = float.Parse(lines[3].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]);
        float cellsize = float.Parse(lines[4].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]);
        float nodataValue = float.Parse(lines[5].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]);

        KoreFloat2DArray grid = new KoreFloat2DArray(inwidth, inheight);

        // Start reading data from line 6 (after the header)
        for (int i = 6; i < lines.Length; i++)
        {
            int lineIndex = i - 6;

            string[] elevationValues = lines[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int j = 0; j < inwidth; j++)
            {
                float elevation = float.Parse(elevationValues[j]);

                // Check for no data value
                if (elevation == nodataValue)
                {
                    // Handle no data value if needed
                    elevation = 0; // Example: set to 0 or some other appropriate value
                }
                // X, Y top to bottom as in the file
                grid[j, lineIndex] = elevation;
            }
        }
        return grid;
    }


    // ******
    // NOTE: Don't save to ArcASCIIGrid - we may be able to get some of the syntax right, but we can't calculate the corner and cell size values appropriately.
    // ******

    // Write a file back out to something like an ASCII ArcGrid file for later reading.
    // We don't have the maths/geography libraries to do this accurately, so many of the params are expected to be deafults.

    // Usage: KoreFloat2DArrayIO.SaveToArcASIIGridFile(asciiArcArry, <filename>);

    // public static void SaveToArcASIIGridFile(
    //         KoreFloat2DArray array, string filePath,
    //         float xllcorner = -1f, float yllcorner = -1f,
    //         float cellsize = -1f, float nodataValue = -1f)
    // {
    //     using (StreamWriter writer = new StreamWriter(filePath))
    //     {
    //         writer.WriteLine($"ncols        {array.Width}");
    //         writer.WriteLine($"nrows        {array.Height}");
    //         writer.WriteLine($"xllcorner    {xllcorner}");
    //         writer.WriteLine($"yllcorner    {yllcorner}");
    //         writer.WriteLine($"cellsize     {cellsize}");
    //         writer.WriteLine($"nodata_value {nodataValue}");


    //         for (int j = 0; j < array.Height; j++)
    //         {
    //             for (int i = 0; i < array.Width; i++)
    //             {
    //                 // X, Y, top to bottom as in the array
    //                 writer.Write(array[i, j] + " ");
    //             }
    //             writer.WriteLine();
    //         }
    //     }
    // }

    // --------------------------------------------------------------------------------------------
    // MARK: Binary File IO
    // --------------------------------------------------------------------------------------------

    public static void WriteToBinaryFile(KoreElevationPatch patch, string filePath)
    {
        try
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
            {
                // Write bounding box
                writer.Write(patch.LLBox.MinLatDegs);
                writer.Write(patch.LLBox.MinLonDegs);
                writer.Write(patch.LLBox.MaxLatDegs);
                writer.Write(patch.LLBox.MaxLonDegs);

                // Write resolution
                writer.Write(patch.ElevationData.Width);
                writer.Write(patch.ElevationData.Height);

                // Write elevation data
                int numRows = patch.ElevationData.Height;
                for (int i = 0; i < numRows; i++)
                {
                    var row = patch.ElevationData.GetRow(i);
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

    public static KoreElevationPatch? ReadFromBinaryFile(string filePath)
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

                return new KoreElevationPatch() { ElevationData = elevData, LLBox = llBox };
            }
        }
        catch (Exception ex)
        {
            KoreCentralLog.AddEntry($"Error reading from binary file: {ex.Message}");
            return null;
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Byte
    // --------------------------------------------------------------------------------------------

    // Use the KoreCommon KoreByteArrayWriter for efficient storage

    public static byte[] WriteToByteArray(KoreElevationPatch patch)
    {
        KoreByteArrayWriter writer = new KoreByteArrayWriter();

        // Write bounding box
        writer.WriteDouble(patch.LLBox.MinLatDegs);
        writer.WriteDouble(patch.LLBox.MinLonDegs);
        writer.WriteDouble(patch.LLBox.MaxLatDegs);
        writer.WriteDouble(patch.LLBox.MaxLonDegs);

        // Write resolution
        writer.WriteInt(patch.ElevationData.Width);
        writer.WriteInt(patch.ElevationData.Height);

        // Write elevation data
        int numRows = patch.ElevationData.Height;
        for (int i = 0; i < numRows; i++)
        {
            var row = patch.ElevationData.GetRow(i);
            foreach (var value in row)
            {
                writer.WriteFloat(value);
            }
        }

        return writer.ToArray();
    }


    public static KoreElevationPatch? ReadFromByteArray(byte[] data)
    {
        try
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms))
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

                return new KoreElevationPatch() { ElevationData = elevData, LLBox = llBox };
            }
        }
        catch (Exception ex)
        {
            KoreCentralLog.AddEntry($"Error reading from byte array: {ex.Message}");
            return null;
        }
    }

}


