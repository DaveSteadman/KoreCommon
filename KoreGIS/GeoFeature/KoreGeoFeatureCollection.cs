// <fileheader>

#nullable enable

using System.Collections.Generic;

using KoreCommon;

namespace KoreGIS;

// A collection of geographic features
public class KoreGeoFeatureCollection
{
    public List<KoreGeoFeature> Features { get; set; } = new List<KoreGeoFeature>();
    public KoreLLBox? BoundingBox { get; set; }
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
}

