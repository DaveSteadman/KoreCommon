// <fileheader>

#nullable enable

using System;

using KoreCommon;

namespace KoreGIS;

// Exception thrown when a Shapefile cannot be read or written.
public class KoreShapefileException : Exception
{
    public KoreShapefileException(string message) : base(message) { }
    public KoreShapefileException(string message, Exception innerException) : base(message, innerException) { }
}

