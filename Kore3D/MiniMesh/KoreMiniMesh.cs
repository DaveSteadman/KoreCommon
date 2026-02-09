// <fileheader>

using System;
using System.Collections.Generic;

#nullable enable

namespace KoreCommon;

// KoreMiniMesh: A minimalist mesh definition, stripping away textures and UVs to favour exclusively colored materials.
// - Normals are calculated on the fly for a triangle surface.

// COORDINATE SYSTEM SPECIFICATION:
// - X+: Right, Y+: Up, Z-: Forward (Godot native)
// - Triangle winding: clockwise when viewed from outside (Godot native)

public readonly record struct KoreMiniMeshLine(int A, int B, int ColorId);
public readonly record struct KoreMiniMeshTri(int A, int B, int C);
public readonly record struct KoreMiniMeshGroup(string MaterialName, List<int> TriIdList);

public partial class KoreMiniMesh
{
    // Core attribute buffers (deduplicated)
    public Dictionary<int, KoreXYZVector> Vertices = []; // unique XYZ
    public Dictionary<int, KoreColorRGB> Colors = []; // vertex/loop colors

    // Topology
    public Dictionary<int, KoreMiniMeshLine> Lines = []; // optional edges (debug/wire)
    public Dictionary<int, KoreMiniMeshTri> Triangles = []; // per-face corner refs

    // Rendering
    public List<KoreMiniMeshMaterial> Materials = []; // Materials list - they contain their own name
    public Dictionary<string, KoreMiniMeshGroup> Groups = []; // Tags for grouping triangles under names/colors

    // Counters for unique IDs
    public int NextVertexId   = 0;
    public int NextColorId    = 0;
    public int NextLineId     = 0;
    public int NextTriangleId = 0;
}



