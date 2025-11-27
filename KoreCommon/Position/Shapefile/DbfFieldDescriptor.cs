// <fileheader>

#nullable enable

namespace KoreCommon;

/// <summary>
/// Represents a field (column) descriptor in a DBF file.
/// </summary>
public class DbfFieldDescriptor
{
    /// <summary>
    /// Field name (up to 11 characters in DBF format).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// DBF field type character:
    /// C = Character (string)
    /// N = Numeric
    /// F = Float
    /// L = Logical (boolean)
    /// D = Date
    /// </summary>
    public char FieldType { get; set; }

    /// <summary>
    /// Total field length in bytes.
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// Decimal count for numeric fields.
    /// </summary>
    public int DecimalCount { get; set; }

    /// <summary>
    /// Work area ID (reserved, usually 0).
    /// </summary>
    public byte WorkAreaId { get; set; }

    /// <summary>
    /// Gets the .NET type that corresponds to this DBF field type.
    /// </summary>
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

    /// <summary>
    /// Infers the DBF field type from a .NET type.
    /// </summary>
    public static DbfFieldDescriptor FromClrType(string name, System.Type type, int maxLength = 254)
    {
        var descriptor = new DbfFieldDescriptor { Name = TruncateName(name) };

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

    /// <summary>
    /// Truncates a field name to the DBF maximum of 11 characters.
    /// </summary>
    private static string TruncateName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "FIELD";
        return name.Length <= 11 ? name : name.Substring(0, 11);
    }
}
