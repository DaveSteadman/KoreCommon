// <fileheader>

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using KoreCommon;

namespace KoreGIS;

// Partial class for writing DBF (attribute) files
public static partial class KoreShapefileWriter
{
    // Writes the DBF file.
    private static void WriteDbf(string dbfPath, List<KoreShapefileFeature> features, List<KoreDbfFieldDescriptor> fields)
    {
        using var stream = new FileStream(dbfPath, FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(stream);

        // Calculate header size and record size
        int headerSize = 32 + (fields.Count * 32) + 1; // Header + field descriptors + terminator
        int recordSize = 1; // Deletion flag
        foreach (var field in fields)
        {
            recordSize += field.Length;
        }

        // DBF header
        writer.Write((byte)0x03); // dBASE III
        var now = DateTime.Now;
        writer.Write((byte)(now.Year - 1900));
        writer.Write((byte)now.Month);
        writer.Write((byte)now.Day);
        writer.Write(features.Count); // Number of records
        writer.Write((short)headerSize);
        writer.Write((short)recordSize);
        writer.Write(new byte[20]); // Reserved

        // Field descriptors
        foreach (var field in fields)
        {
            byte[] nameBytes = new byte[11];
            byte[] srcBytes = Encoding.ASCII.GetBytes(field.Name);
            Array.Copy(srcBytes, nameBytes, Math.Min(srcBytes.Length, 11));
            writer.Write(nameBytes);

            writer.Write((byte)field.FieldType);
            writer.Write(new byte[4]); // Reserved
            writer.Write((byte)field.Length);
            writer.Write((byte)field.DecimalCount);
            writer.Write(new byte[14]); // Reserved
        }

        // Header terminator
        writer.Write((byte)0x0D);

        // Records
        foreach (var feature in features)
        {
            // Deletion flag (space = not deleted)
            writer.Write((byte)0x20);

            // Field values
            foreach (var field in fields)
            {
                object? value = feature.Attributes.TryGetValue(field.Name, out var v) ? v : null;
                string strValue = FormatDbfValue(value, field);

                byte[] valueBytes = new byte[field.Length];
                byte[] srcBytes = Encoding.ASCII.GetBytes(strValue);
                int copyLen = Math.Min(srcBytes.Length, field.Length);

                if (field.FieldType == 'N' || field.FieldType == 'F')
                {
                    // Right-align numeric values
                    int startPos = field.Length - copyLen;
                    Array.Copy(srcBytes, 0, valueBytes, startPos, copyLen);
                    // Fill leading with spaces
                    for (int i = 0; i < startPos; i++)
                        valueBytes[i] = 0x20;
                }
                else
                {
                    // Left-align other values
                    Array.Copy(srcBytes, valueBytes, copyLen);
                    // Fill trailing with spaces
                    for (int i = copyLen; i < field.Length; i++)
                        valueBytes[i] = 0x20;
                }

                writer.Write(valueBytes);
            }
        }

        // EOF marker
        writer.Write((byte)0x1A);
    }

    // Formats an attribute value for DBF storage.
    private static string FormatDbfValue(object? value, KoreDbfFieldDescriptor field)
    {
        if (value == null)
            return new string(' ', field.Length);

        switch (field.FieldType)
        {
            case 'N':
            case 'F':
                if (field.DecimalCount > 0)
                {
                    double dval = Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);
                    return dval.ToString($"F{field.DecimalCount}", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    long lval = Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture);
                    return lval.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }

            case 'L':
                bool bval = Convert.ToBoolean(value);
                return bval ? "T" : "F";

            case 'D':
                if (value is DateTime dt)
                    return dt.ToString("yyyyMMdd");
                return new string(' ', 8);

            default:
                return value.ToString() ?? string.Empty;
        }
    }

    // Infers DBF field descriptors from the attributes of all features.
    private static List<KoreDbfFieldDescriptor> InferFieldDescriptors(List<KoreShapefileFeature> features)
    {
        var fieldTypes = new Dictionary<string, Type>();
        var maxLengths = new Dictionary<string, int>();

        foreach (var feature in features)
        {
            foreach (var attr in feature.Attributes)
            {
                if (attr.Value == null)
                    continue;

                var valueType = attr.Value.GetType();

                if (!fieldTypes.ContainsKey(attr.Key))
                {
                    fieldTypes[attr.Key] = valueType;
                    maxLengths[attr.Key] = 1;
                }
                else
                {
                    // Promote type if necessary (e.g., int -> double)
                    var existingType = fieldTypes[attr.Key];
                    if (existingType != valueType)
                    {
                        if (IsNumeric(existingType) && IsNumeric(valueType))
                        {
                            fieldTypes[attr.Key] = typeof(double);
                        }
                        else if (existingType != typeof(string))
                        {
                            fieldTypes[attr.Key] = typeof(string);
                        }
                    }
                }

                // Track max string length
                string strValue = FormatAttributeValue(attr.Value);
                if (strValue.Length > maxLengths[attr.Key])
                {
                    maxLengths[attr.Key] = strValue.Length;
                }
            }
        }

        var descriptors = new List<KoreDbfFieldDescriptor>();
        foreach (var kvp in fieldTypes)
        {
            var descriptor = KoreDbfFieldDescriptor.FromClrType(kvp.Key, kvp.Value, Math.Max(1, maxLengths[kvp.Key]));
            descriptors.Add(descriptor);
        }

        return descriptors;
    }

    // Checks if a type is a numeric type.
    private static bool IsNumeric(Type type)
    {
        return type == typeof(int) || type == typeof(long) || type == typeof(short) ||
               type == typeof(byte) || type == typeof(double) || type == typeof(float) ||
               type == typeof(decimal);
    }

    // Formats an attribute value as a string for length calculation.
    private static string FormatAttributeValue(object value)
    {
        if (value is DateTime dt)
            return dt.ToString("yyyyMMdd");
        if (value is bool b)
            return b ? "T" : "F";
        if (value is double d)
            return d.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
        return value?.ToString() ?? string.Empty;
    }
}

