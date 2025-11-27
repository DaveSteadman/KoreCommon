// <fileheader>

#nullable enable

using System;

namespace KoreCommon;

// Exception thrown when a Shapefile cannot be read or written.
public class KoreShapefileException : Exception
{
    public KoreShapefileException(string message) : base(message) { }
    public KoreShapefileException(string message, Exception innerException) : base(message, innerException) { }
}
