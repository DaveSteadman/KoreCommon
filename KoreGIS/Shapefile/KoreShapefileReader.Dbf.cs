// <fileheader>

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using KoreCommon;

namespace KoreGIS;

// Partial class for reading DBF (attribute) files
public static partial class KoreShapefileReader
{
    // Reads the DBF file for attribute data.
    private static void ReadDbfFile(string dbfPath, List<Dictionary<string, object?>> attributes,
        List<KoreDbfFieldDescriptor> fieldDescriptors, KoreShapefileFeatureCollection collection)
    {
        try
        {
            using var stream = new FileStream(dbfPath, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(stream);

            // DBF Header
            byte version = reader.ReadByte();
            byte year = reader.ReadByte();
            byte month = reader.ReadByte();
            byte day = reader.ReadByte();
            int recordCount = reader.ReadInt32();
            short headerLength = reader.ReadInt16();
            short recordLength = reader.ReadInt16();
            reader.ReadBytes(20); // Reserved bytes

            // Calculate number of fields: (headerLength - 32 - 1) / 32
            int fieldCount = (headerLength - 33) / 32;

            // Read field descriptors
            for (int i = 0; i < fieldCount; i++)
            {
                var field = new KoreDbfFieldDescriptor();

                byte[] nameBytes = reader.ReadBytes(11);
                field.Name = Encoding.ASCII.GetString(nameBytes).TrimEnd('\0', ' ');
                field.FieldType = (char)reader.ReadByte();
                reader.ReadBytes(4); // Reserved
                field.Length = reader.ReadByte();
                field.DecimalCount = reader.ReadByte();
                reader.ReadBytes(14); // Reserved

                fieldDescriptors.Add(field);
            }

            // Skip header terminator (0x0D)
            reader.ReadByte();

            // Read records
            for (int recordIndex = 0; recordIndex < recordCount; recordIndex++)
            {
                try
                {
                    var record = new Dictionary<string, object?>();

                    // Read deletion flag
                    byte deletionFlag = reader.ReadByte();
                    if (deletionFlag == 0x2A) // '*' = deleted record
                    {
                        reader.ReadBytes(recordLength - 1); // Skip rest of record
                        continue;
                    }

                    // Read field values
                    foreach (var field in fieldDescriptors)
                    {
                        byte[] fieldBytes = reader.ReadBytes(field.Length);
                        string fieldValue = Encoding.ASCII.GetString(fieldBytes).Trim();

                        object? value = ParseDbfValue(fieldValue, field);
                        record[field.Name] = value;
                    }

                    attributes.Add(record);
                }
                catch (Exception ex)
                {
                    collection.Warnings.Add($"Failed to read DBF record {recordIndex + 1}: {ex.Message}");
                    // Try to skip to next record
                    try
                    {
                        long expectedPosition = headerLength + (recordIndex + 1) * recordLength;
                        if (stream.Position < expectedPosition)
                        {
                            stream.Position = expectedPosition;
                        }
                    }
                    catch
                    {
                        // If we can't recover, add empty record
                    }
                    attributes.Add(new Dictionary<string, object?>());
                }
            }
        }
        catch (Exception ex)
        {
            collection.Warnings.Add($"Failed to read DBF file: {ex.Message}");
        }
    }

    // Parses a DBF field value string into the appropriate .NET type.
    private static object? ParseDbfValue(string value, KoreDbfFieldDescriptor field)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        try
        {
            switch (field.FieldType)
            {
                case 'C': // Character
                    return value;

                case 'N': // Numeric
                case 'F': // Float
                    if (field.DecimalCount > 0)
                    {
                        if (double.TryParse(value, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double dval))
                            return dval;
                    }
                    else
                    {
                        if (int.TryParse(value, out int ival))
                            return ival;
                        // Try as long if int fails
                        if (long.TryParse(value, out long lval))
                            return (int)lval; // Truncate to int
                        // Fall back to double for very large numbers
                        if (double.TryParse(value, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double dval2))
                            return (int)dval2;
                    }
                    return null;

                case 'L': // Logical
                    char c = value.Length > 0 ? char.ToUpperInvariant(value[0]) : ' ';
                    return c switch
                    {
                        'T' or 'Y' or '1' => true,
                        'F' or 'N' or '0' => false,
                        _ => null
                    };

                case 'D': // Date (YYYYMMDD format)
                    if (value.Length == 8 &&
                        int.TryParse(value.Substring(0, 4), out int year) &&
                        int.TryParse(value.Substring(4, 2), out int month) &&
                        int.TryParse(value.Substring(6, 2), out int day))
                    {
                        try
                        {
                            return new DateTime(year, month, day);
                        }
                        catch
                        {
                            return null;
                        }
                    }
                    return null;

                default:
                    return value;
            }
        }
        catch
        {
            return value; // Return as string if parsing fails
        }
    }
}

