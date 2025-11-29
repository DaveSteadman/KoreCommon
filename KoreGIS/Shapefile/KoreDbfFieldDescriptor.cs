// <fileheader>

#nullable enable

using KoreCommon;

namespace KoreGIS;

// Represents a field (column) descriptor in a DBF file.
public class KoreDbfFieldDescriptor
{
    // Field name (up to 11 characters in DBF format).
    public string Name { get; set; } = string.Empty;

    // DBF field type character:
    // C = Character (string)
    // N = Numeric
    // F = Float
    // L = Logical (boolean)
    // D = Date
    public char FieldType { get; set; }

    // Total field length in bytes.
    public int Length { get; set; }

    // Decimal count for numeric fields.
    public int DecimalCount { get; set; }

    // Work area ID (reserved, usually 0).
    public byte WorkAreaId { get; set; }

    // Gets the .NET type that corresponds to this DBF field type.
    public System.Type GetClrType()
    {
        return FieldType switch
        {
            'C' => typeof(string),
            'N' => DecimalCount > 0 ? typeof(double) : typeof(int),
            'F' => typeof(double),
            'L' => typeof(bool),
            'D' => typeof(System.DateTime),
            _ => typeof(string)
        };
    }

    // Infers the DBF field type from a .NET type.
    public static KoreDbfFieldDescriptor FromClrType(string name, System.Type type, int maxLength = 254)
    {
        var descriptor = new KoreDbfFieldDescriptor { Name = TruncateName(name) };

        if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte))
        {
            descriptor.FieldType = 'N';
            descriptor.Length = 11;
            descriptor.DecimalCount = 0;
        }
        else if (type == typeof(double) || type == typeof(float) || type == typeof(decimal))
        {
            descriptor.FieldType = 'N';
            descriptor.Length = 19;
            descriptor.DecimalCount = 11;
        }
        else if (type == typeof(bool))
        {
            descriptor.FieldType = 'L';
            descriptor.Length = 1;
            descriptor.DecimalCount = 0;
        }
        else if (type == typeof(System.DateTime))
        {
            descriptor.FieldType = 'D';
            descriptor.Length = 8;
            descriptor.DecimalCount = 0;
        }
        else // string or other
        {
            descriptor.FieldType = 'C';
            descriptor.Length = System.Math.Min(maxLength, 254);
            descriptor.DecimalCount = 0;
        }

        return descriptor;
    }

    // Truncates a field name to the DBF maximum of 11 characters.
    private static string TruncateName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "FIELD";
        return name.Length <= 11 ? name : name.Substring(0, 11);
    }
}

