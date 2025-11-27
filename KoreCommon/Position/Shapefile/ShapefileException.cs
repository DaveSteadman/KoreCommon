// <fileheader>

#nullable enable

using System;

namespace KoreCommon;

/// <summary>
/// Exception thrown when a Shapefile cannot be read or written.
/// </summary>
public class ShapefileException : Exception
{
    public ShapefileException(string message) : base(message) { }
    public ShapefileException(string message, Exception innerException) : base(message, innerException) { }
}
