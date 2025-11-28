// <fileheader>

using System;
using System.Collections.Generic;
using System.Linq;

namespace KoreGIS;

public sealed class PolygonGeometry : Geometry
{
    // First ring = exterior, remaining rings = holes
    public IReadOnlyList<IReadOnlyList<KoreLLPoint>> Rings { get; }

    public PolygonGeometry(IEnumerable<IEnumerable<KoreLLPoint>> rings)
    {
        if (rings == null)
            throw new ArgumentNullException(nameof(rings));

        var ringsList = rings.Select(r => r?.ToList() ?? throw new ArgumentNullException(nameof(rings))).ToList();
        
        if (ringsList.Count == 0)
            throw new ArgumentException("Polygon must have at least one ring.", nameof(rings));

        foreach (var ring in ringsList)
        {
            if (ring.Count < 4)
                throw new ArgumentException("Each ring must have at least 4 positions (closed ring).", nameof(rings));
        }

        Rings = ringsList.Select(r => (IReadOnlyList<KoreLLPoint>)r).ToList();
    }
}
