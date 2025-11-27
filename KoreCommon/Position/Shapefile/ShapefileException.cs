// <fileheader>

#nullable enable

using System;

namespace KoreCommon;

// Exception thrown when a Shapefile cannot be read or written.
public class ShapefileException : Exception
{
    public ShapefileException(string message) : base(message) { }
    public ShapefileException(string message, Exception innerException) : base(message, innerException) { }
}
