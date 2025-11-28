// <fileheader>

using System;

namespace KoreGIS;

public sealed class PointGeometry : Geometry
{
    public KoreLLPoint Position { get; }

    public PointGeometry(KoreLLPoint position)
    {
        Position = position;
    }
}
