// <fileheader>

#nullable enable

using System.Collections.Generic;

using KoreCommon;

namespace KoreGIS;

// Base class for all geographic features
public abstract class KoreGeoFeature
{
    public string Name { get; set; } = string.Empty;
    public string? Id { get; set; } // Optional GeoJSON Feature id (RFC 7946 Section 3.2)
    public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
}

