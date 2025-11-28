// <fileheader>

using System;
using System.Collections.Generic;
using System.Linq;

namespace KoreGIS;

public sealed class LineStringGeometry : Geometry
{
    public IReadOnlyList<KoreLLPoint> Positions { get; }

    public LineStringGeometry(IEnumerable<KoreLLPoint> positions)
    {
        var list = positions?.ToList() ?? throw new ArgumentNullException(nameof(positions));
        if (list.Count < 2)
            throw new ArgumentException("LineString must have at least 2 positions.", nameof(positions));

        Positions = list;
    }
}
