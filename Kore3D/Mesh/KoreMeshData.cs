// <fileheader>

using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace KoreCommon;

// Minor structs for mesh data components
public record struct KoreMeshLine(int A, int B);
public record struct KoreMeshTriangle(int A, int B, int C);
public record struct KoreMeshLineColour(KoreColorRGB StartColor, KoreColorRGB EndColor);
public record struct KoreMeshTriangleGroup(string MaterialName, List<int> TriangleIds);

// KoreMeshData: A class to hold mesh data for 3D geometry.
// - points, lines, triangles, normals, UVs, vertex colors, line colors, and materials.

// COORDINATE SYSTEM SPECIFICATION:
// - X+: Right, Y+: Up, Z-: Forward (Godot native // Right handed)
// - UVs use top-left origin (Godot/OpenGL style): U (X) incrementing right, V (Y) incrementing down to a 1,1 bottom right
// - Triangle winding: clockwise when viewed from outside (Godot native)

public partial class KoreMeshData
{
    // Vertices by VertexID
    public Dictionary<int, KoreXYZVector> Vertices = [];

    // Normals by VertexID
    public Dictionary<int, KoreXYZVector> Normals = [];

    // UVs by VertexID
    public Dictionary<int, KoreXYVector> UVs = [];

    // Vertex colors by VertexID - for when the mesh is colored by vertex
    public Dictionary<int, KoreColorRGB> VertexColors = [];

    // Lines by LineID, each referencing VertexIDs
    public Dictionary<int, KoreMeshLine> Lines = [];

    // Line colors by LineID
    public Dictionary<int, KoreMeshLineColour> LineColors = [];

    // Triangles by TriangleID, each referencing VertexIDs
    public Dictionary<int, KoreMeshTriangle> Triangles = [];

    // list of Material for this mesh
    public List<KoreMeshMaterial> Materials = [];

    // Named groups, logical/useful sub-divisions of the mesh triangles, with a material.
    // - Non-exclusive inclusion of triangles allows for multiple uses and some manner of hierarchy
    public Dictionary<string, KoreMeshTriangleGroup> NamedTriangleGroups = []; // Tags for grouping triangles

    // Counters for unique IDs
    public int NextVertexId = 0;
    public int NextLineId = 0;
    public int NextTriangleId = 0;

    // --------------------------------------------------------------------------------------------
    // MARK: Constructors
    // --------------------------------------------------------------------------------------------

    // Empty constructor
    public KoreMeshData() { }

    // Deep Copy constructor
    public KoreMeshData(KoreMeshData mesh)
    {
        this.Vertices            = new Dictionary<int, KoreXYZVector>(mesh.Vertices);
        this.Lines               = new Dictionary<int, KoreMeshLine>(mesh.Lines);
        this.Triangles           = new Dictionary<int, KoreMeshTriangle>(mesh.Triangles);
        this.Normals             = new Dictionary<int, KoreXYZVector>(mesh.Normals);
        this.UVs                 = new Dictionary<int, KoreXYVector>(mesh.UVs);
        this.VertexColors        = new Dictionary<int, KoreColorRGB>(mesh.VertexColors);
        this.LineColors          = new Dictionary<int, KoreMeshLineColour>(mesh.LineColors);
        this.Materials           = new List<KoreMeshMaterial>(mesh.Materials);
        this.NamedTriangleGroups = new Dictionary<string, KoreMeshTriangleGroup>(mesh.NamedTriangleGroups);

        this.NextVertexId        = mesh.NextVertexId;
        this.NextLineId          = mesh.NextLineId;
        this.NextTriangleId      = mesh.NextTriangleId;
    }

    // Initialises the mesh data with empty dictionaries
    public void ClearAllData()
    {
        Vertices.Clear();
        Lines.Clear();
        Triangles.Clear();
        Normals.Clear();
        UVs.Clear();
        VertexColors.Clear();
        LineColors.Clear();
        Materials.Clear();
        NamedTriangleGroups.Clear();

        NextVertexId = 0;
        NextLineId = 0;
        NextTriangleId = 0;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Populate: Max Ids
    // --------------------------------------------------------------------------------------------

    // Reset the Next IDs, looking for the max values in the current lists - Note that after numerous
    // operations, the IDs can be non-sequential, so we need to find the max value in each list.

    public void ResetMaxIDs()
    {
        // Reset the next IDs based on the current max values in the dictionaries
        NextVertexId   = Vertices.Count  > 0 ? Vertices.Keys.Max()  + 1 : 0;
        NextLineId     = Lines.Count     > 0 ? Lines.Keys.Max()     + 1 : 0;
        NextTriangleId = Triangles.Count > 0 ? Triangles.Keys.Max() + 1 : 0;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Vertices
    // --------------------------------------------------------------------------------------------

    public int AddVertex(KoreXYZVector vertex)
    {
        int id = NextVertexId++;
        Vertices[id] = vertex;
        return id;
    }

    public void SetVertex(int id, KoreXYZVector vertex)
    {
        Vertices[id] = vertex;
    }

    // Add a vertex and return its ID
    public int AddCompleteVertex(KoreXYZVector vertex, KoreXYZVector? normal = null, KoreColorRGB? color = null, KoreXYVector? uv = null)
    {
        int id = NextVertexId++;
        Vertices[id] = vertex;

        if (normal.HasValue) Normals[id] = normal.Value;
        if (color.HasValue) VertexColors[id] = color.Value;
        if (uv.HasValue) UVs[id] = uv.Value;

        return id;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Normals
    // --------------------------------------------------------------------------------------------

    public void SetNormal(int vertexId, KoreXYZVector normal)
    {
        // Need to have the normal tied to the vertex ID
        if (!Vertices.ContainsKey(vertexId))
            throw new ArgumentOutOfRangeException(nameof(vertexId), "Vertex ID is not found.");

        Normals[vertexId] = normal;
    }

    public void ClearAllNormals()
    {
        Normals.Clear();
    }

    // --------------------------------------------------------------------------------------------
    // MARK: UVs
    // --------------------------------------------------------------------------------------------

    public void SetUV(int vertexId, KoreXYVector uv)
    {
        // We want to throw here, because we have a unique ID concept and random new additions break this
        if (!Vertices.ContainsKey(vertexId))
            throw new ArgumentOutOfRangeException(nameof(vertexId), "Vertex ID is not found.");

        UVs[vertexId] = uv;
    }

    public void ClearAllUVs()
    {
        UVs.Clear();
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Vertex Colors
    // --------------------------------------------------------------------------------------------

    public void SetVertexColor(int vertexId, KoreColorRGB color)
    {
        // Can only set a vertex colour for a valid vertex ID
        if (!Vertices.ContainsKey(vertexId))
            throw new ArgumentOutOfRangeException(nameof(vertexId), "Vertex ID is not found.");

        VertexColors[vertexId] = color;
    }

    public void SetAllVertexColors(KoreColorRGB color)
    {
        foreach (var vertexId in Vertices.Keys)
            SetVertexColor(vertexId, color);
    }

    public void ClearAllVertexColors()
    {
        VertexColors.Clear();
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Lines
    // --------------------------------------------------------------------------------------------

    public int AddLine(int vertexIdA, int vertexIdB)
    {
        // Check if the line already exists
        int prevLineId = LineID(vertexIdA, vertexIdB);
        if (prevLineId != -1)
            return prevLineId;

        int id = NextLineId++;
        Lines[id] = new KoreMeshLine(vertexIdA, vertexIdB);
        return id;
    }

    public int AddLine(KoreXYZVector startPos, KoreXYZVector endPos)
    {
        int idxA = AddVertex(startPos);
        int idxB = AddVertex(endPos);
        return AddLine(idxA, idxB);
    }

    public bool DoesLineExist(int vertexIdA, int vertexIdB)
    {
        var line = new KoreMeshLine(vertexIdA, vertexIdB);
        return Lines.Values.Any(l => l.Equals(line));
    }

    public int LineID(int vertexIdA, int vertexIdB)
    {
        foreach (var kvp in Lines)
        {
            int id = kvp.Key;
            KoreMeshLine line = kvp.Value;

            if (line.A == vertexIdA && line.B == vertexIdB)
                return id;
        }
        return -1;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Line Color
    // --------------------------------------------------------------------------------------------

    public void SetLineColor(int lineId, KoreColorRGB lineColor)
    {
        // We want to throw here, because we have a unique ID concept and random new additions break this
        if (!Lines.ContainsKey(lineId))
            throw new ArgumentOutOfRangeException(nameof(lineId), "Line ID is not found.");

        LineColors[lineId] = new KoreMeshLineColour(lineColor, lineColor);
    }

    public void SetLineColor(int lineId, KoreColorRGB lineStartColor, KoreColorRGB lineEndColor)
    {
        // We want to throw here, because we have a unique ID concept and random new additions break this
        if (!Lines.ContainsKey(lineId))
            throw new ArgumentOutOfRangeException(nameof(lineId), "Line ID is not found.");

        LineColors[lineId] = new KoreMeshLineColour(lineStartColor, lineEndColor);
    }

    public void SetAllLineColors(KoreColorRGB startColor, KoreColorRGB endColor)
    {
        foreach (var lineId in Lines.Keys)
            SetLineColor(lineId, startColor, endColor);
    }

    public void SetAllLineColors(KoreColorRGB color) => SetAllLineColors(color, color);

    // --------------------------------------------------------------------------------------------
    // MARK: Line Shortcuts
    // --------------------------------------------------------------------------------------------

    public int AddLine(int vertexIdA, int vertexIdB, KoreColorRGB colLine)
    {
        int id = NextLineId++;
        Lines[id] = new KoreMeshLine(vertexIdA, vertexIdB);
        LineColors[id] = new KoreMeshLineColour(colLine, colLine);
        return id;
    }

    // Add a line and return its ID
    public int AddLine(int vertexIdA, int vertexIdB, KoreColorRGB? colStart = null, KoreColorRGB? colEnd = null)
    {
        int id = NextLineId++;
        Lines[id] = new KoreMeshLine(vertexIdA, vertexIdB);

        if (colStart.HasValue && colEnd.HasValue)
            LineColors[id] = new KoreMeshLineColour(colStart.Value, colEnd.Value);
        return id;
    }

    public int AddLine(KoreMeshLine line, KoreColorRGB? colStart = null, KoreColorRGB? colEnd = null)
    {
        return AddLine(line.A, line.B, colStart, colEnd);
    }

    public int AddLine(KoreXYZVector start, KoreXYZVector end, KoreColorRGB colLine) => AddLine(start, end, colLine, colLine);

    public int AddLine(KoreXYZVector start, KoreXYZVector end, KoreColorRGB colStart, KoreColorRGB colEnd)
    {
        int idxA = AddCompleteVertex(start, null, colStart);
        int idxB = AddCompleteVertex(end, null, colEnd);
        return AddLine(idxA, idxB, colStart, colEnd);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Outline
    // --------------------------------------------------------------------------------------------

    public void OutlineTriangle(int v0, int v1, int v2, KoreColorRGB linecolor)
    {
        // Outline the triangle by adding lines between its vertices
        int l1Id = AddLine(v0, v1);
        int l2Id = AddLine(v1, v2);
        int l3Id = AddLine(v2, v0);

        SetLineColor(l1Id, linecolor);
        SetLineColor(l2Id, linecolor);
        SetLineColor(l3Id, linecolor);
    }

    public void OutlineTriangle(int triId, KoreColorRGB linecolor)
    {
        if (Triangles.ContainsKey(triId))
        {
            KoreMeshTriangle triangle = Triangles[triId];
            OutlineTriangle(triangle.A, triangle.B, triangle.C, linecolor);
        }
    }

    // --------------------------------------------------------------------------------------------

    public void OutlineFace(int v0, int v1, int v2, int v3, KoreColorRGB linecolor)
    {
        // Outline the triangle by adding lines between its vertices
        int l1Id = AddLine(v0, v1);
        int l2Id = AddLine(v1, v2);
        int l3Id = AddLine(v2, v3);
        int l4Id = AddLine(v3, v0);

        SetLineColor(l1Id, linecolor);
        SetLineColor(l2Id, linecolor);
        SetLineColor(l3Id, linecolor);
        SetLineColor(l4Id, linecolor);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Dotted Lines
    // --------------------------------------------------------------------------------------------

    public void AddDottedLineByDistance(KoreXYZVector start, KoreXYZVector end, KoreColorRGB colLine, double dotSpacing)
    {
        int p1 = AddCompleteVertex(start, null, colLine);
        int p2 = AddCompleteVertex(end, null, colLine);
        AddDottedLineByDistance(p1, p2, colLine, dotSpacing);
    }

    public void AddDottedLineByDistance(int vertexIdA, int vertexIdB, KoreColorRGB colLine, double dotSpacing)
    {
        // Calculate the distance between the two vertices
        KoreXYZVector pntA = new KoreXYZVector(Vertices[vertexIdA]);
        KoreXYZVector pntB = new KoreXYZVector(Vertices[vertexIdB]);

        double distance = pntA.DistanceTo(pntB);

        double currDist = 0.0;
        double dotLength = dotSpacing * 0.5; // Each dot is half the spacing

        while (currDist < distance)
        {
            // Calculate the start point of this dot
            double tStart = currDist / distance;
            KoreXYZVector dotStart = new KoreXYZVector(
                pntA.X + (pntB.X - pntA.X) * tStart,
                pntA.Y + (pntB.Y - pntA.Y) * tStart,
                pntA.Z + (pntB.Z - pntA.Z) * tStart
            );

            // Calculate the end point of this dot
            double dotEndDist = Math.Min(currDist + dotLength, distance);
            double tEnd = dotEndDist / distance;
            KoreXYZVector dotEnd = new KoreXYZVector(
                pntA.X + (pntB.X - pntA.X) * tEnd,
                pntA.Y + (pntB.Y - pntA.Y) * tEnd,
                pntA.Z + (pntB.Z - pntA.Z) * tEnd
            );

            // Add the line segment (the dot)
            AddLine(new KoreXYZVector(dotStart), new KoreXYZVector(dotEnd), colLine);

            // Move to the next dot position (including the gap)
            currDist += dotSpacing;
        }

        // Not returning a value since we're not creating a single returnable lineId
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Poly Line
    // --------------------------------------------------------------------------------------------

    // Add a list of points and connect them all with lines.
    public void AddPolyLine(List<KoreXYZVector> points, KoreColorRGB colLine)
    {
        if (points.Count < 2)
            throw new ArgumentException("At least two points are required to create a polyline.");

        // Add all the points and record the ids in a list
        List<int> pointIds = new List<int>();

        foreach (KoreXYZVector pnt in points)
        {
            int id = AddCompleteVertex(pnt, null, colLine);
            pointIds.Add(id);
        }

        for (int i = 0; i < pointIds.Count - 1; i++)
        {
            AddLine(pointIds[i], pointIds[i + 1], colLine);
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Triangle
    // --------------------------------------------------------------------------------------------

    // Triangles wind CW

    // Add a triangle and return its ID
    public int AddTriangle(int vertexIdA, int vertexIdB, int vertexIdC)
    {
        int id = NextTriangleId++;
        Triangles[id] = new KoreMeshTriangle(vertexIdA, vertexIdB, vertexIdC);

        return id;
    }

    public int AddTriangle(KoreMeshTriangle triangle)
    {
        return AddTriangle(triangle.A, triangle.B, triangle.C);
    }

    // Add a completely independent triangle with vertices and optional color.
    public int AddTriangle(KoreXYZVector a, KoreXYZVector b, KoreXYZVector c)
    {
        int idxA = AddVertex(a);
        int idxB = AddVertex(b);
        int idxC = AddVertex(c);

        int triId = AddTriangle(idxA, idxB, idxC);

        return triId;
    }

    // Add an independent triangle with vertices and UV positions
    // Uses CW winding for the triangle.
    public void AddTriangle(KoreXYZVector a, KoreXYZVector b, KoreXYZVector c,
                           KoreXYVector uvA, KoreXYVector uvB, KoreXYVector uvC,
                           string? groupName = null)
    {
        int idxA = AddVertex(a);
        int idxB = AddVertex(b);
        int idxC = AddVertex(c);

        SetUV(idxA, uvA);
        SetUV(idxB, uvB);
        SetUV(idxC, uvC);

        // Create the triangle
        int triId = AddTriangle(idxA, idxB, idxC);

        // Add to the group
        if (groupName != null)
            AddTriangleToGroup(triId, groupName);
    }

    public bool HasTriangle(int triId)
    {
        return Triangles.ContainsKey(triId);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Face
    // --------------------------------------------------------------------------------------------

    // a ---- b
    // |      |
    // d ---- c

    // Add a new isolated face as two triangles with automatically calculated normals for sharp edges.
    // Creates four separate vertices (no sharing) with proper face normals for crisp rendering.
    public void AddFace(KoreXYZVector a, KoreXYZVector b, KoreXYZVector c, KoreXYZVector d)
    {
        // Calculate the face normal using cross product
        KoreXYZVector ab = b - a;  // Vector from A to B
        KoreXYZVector ac = c - a;  // Vector from A to C

        // Cross product gives us the face normal (right-hand rule)
        KoreXYZVector faceNormal = KoreXYZVector.CrossProduct(ab, ac);

        // Add three separate vertices with the same face normal for sharp edges
        int idxA = AddCompleteVertex(a, faceNormal);
        int idxB = AddCompleteVertex(b, faceNormal);
        int idxC = AddCompleteVertex(c, faceNormal);
        int idxD = AddCompleteVertex(d, faceNormal);

        // Add the triangles with CW winding
        int triId1 = AddTriangle(idxA, idxC, idxB);
        int triId2 = AddTriangle(idxA, idxD, idxC);

        // Set the normals for the triangles
        Normals[triId1] = KoreMeshDataEditOps.NormalForTriangle(this, triId1);
        Normals[triId2] = KoreMeshDataEditOps.NormalForTriangle(this, triId2);
    }

    public void AddFace(int aId, int bId, int cId, int dId)
    {
        // Create two triangles from the face with CW winding
        AddTriangle(aId, cId, bId);
        AddTriangle(aId, dId, cId);
    }

    public void AddFace(KoreXYZVector a, KoreXYZVector b, KoreXYZVector c, KoreXYZVector d, KoreUVBox uvBox)
    {
        int idxA = AddVertex(a);
        int idxB = AddVertex(b);
        int idxC = AddVertex(c);
        int idxD = AddVertex(d);

        SetUV(idxA, uvBox.TopLeft);
        SetUV(idxB, uvBox.TopRight);
        SetUV(idxC, uvBox.BottomRight);
        SetUV(idxD, uvBox.BottomLeft);

        // Create two triangles from the face with CW winding
        AddTriangle(idxA, idxB, idxC);
        AddTriangle(idxA, idxC, idxD);
    }

    // Explicit UV mapping for rotated/oriented faces
    // Maps: a?uvA, b?uvB, c?uvC, d?uvD directly
    public void AddFace(KoreXYZVector a, KoreXYZVector b, KoreXYZVector c, KoreXYZVector d,
                       KoreXYVector uvA, KoreXYVector uvB, KoreXYVector uvC, KoreXYVector uvD,
                       string? groupName = null)
    {
        int idxA = AddVertex(a);
        int idxB = AddVertex(b);
        int idxC = AddVertex(c);
        int idxD = AddVertex(d);

        SetUV(idxA, uvA);
        SetUV(idxB, uvB);
        SetUV(idxC, uvC);
        SetUV(idxD, uvD);

        // Create two triangles from the face with CW winding
        int tri1Id = AddTriangle(idxA, idxB, idxC);
        int tri2Id = AddTriangle(idxA, idxC, idxD);

        // add triangles to group if specified
        if (!string.IsNullOrEmpty(groupName))
        {
            AddTriangleToGroup(tri1Id, groupName);
            AddTriangleToGroup(tri2Id, groupName);
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Materials
    // --------------------------------------------------------------------------------------------

    // The mesh class will look to add materials given any opportunity, and use a cull Broken materials function
    // to remove them later if needed.

    public void AddMaterial(KoreMeshMaterial material)
    {
        // Search the Materials list for a matching material (case insensitive)
        foreach (KoreMeshMaterial existingMaterial in Materials)
        {
            if (string.Equals(existingMaterial.Name, material.Name, StringComparison.OrdinalIgnoreCase))
            {
                // remove the material
                Materials.Remove(existingMaterial);
                break;
            }
        }

        // Material not found - add a new one using its name
        Materials.Add(material);
    }

    // Get the material, or return the default material if not setup
    public KoreMeshMaterial GetMaterial(string materialName)
    {
        // Loop through the existing materials, and return if we find one with a matching name (case insensitive)
        foreach (KoreMeshMaterial existingMaterial in Materials)
        {
            if (string.Equals(existingMaterial.Name, materialName, StringComparison.OrdinalIgnoreCase))
                return existingMaterial;
        }
        return KoreMeshMaterialPalette.DefaultMaterial;
    }

    public List<string> AllMaterialNames()
    {
        List<string> materialNames = new List<string>();
        foreach (KoreMeshMaterial material in Materials)
        {
            materialNames.Add(material.Name);
        }
        return materialNames;
    }

    public void AddDefaultMaterial()
    {
        AddMaterial(KoreMeshMaterialPalette.DefaultMaterial);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Groups
    // --------------------------------------------------------------------------------------------

    public void AddNamedGroup(string newName)
    {
        if (!NamedTriangleGroups.ContainsKey(newName))
        {
            KoreMeshTriangleGroup newGroup = new KoreMeshTriangleGroup
            {
                MaterialName = "",
                TriangleIds = new List<int>()
            };
            NamedTriangleGroups[newName] = newGroup;
        }
    }

    public bool HasNamedGroup(string groupName)
    {
        return NamedTriangleGroups.ContainsKey(groupName);
    }

    public KoreMeshTriangleGroup? NamedGroup(string groupName)
    {
        if (NamedTriangleGroups.ContainsKey(groupName))
        {
            return NamedTriangleGroups[groupName];
        }
        return null;
    }


    // --------------------------------------------------------------------------------------------

    public void AddGroupWithMaterial(string groupName, KoreMeshMaterial material)
    {
        AddMaterial(material);

        if (!NamedTriangleGroups.ContainsKey(groupName))
        {
            NamedTriangleGroups[groupName] = new KoreMeshTriangleGroup
            {
                MaterialName = material.Name,
                TriangleIds = new List<int>()
            };
        }
    }

    public void SetGroupMaterialName(string groupName, string materialName)
    {
        if (NamedTriangleGroups.ContainsKey(groupName))
        {
            KoreMeshTriangleGroup currGroup = NamedTriangleGroups[groupName];
            NamedTriangleGroups[groupName] = currGroup with { MaterialName = materialName };
        }
        else
        {
            KoreMeshTriangleGroup newGroup = new KoreMeshTriangleGroup
            {
                MaterialName = materialName,
                TriangleIds = new List<int>()
            };
            NamedTriangleGroups[groupName] = newGroup;
        }
    }


    public KoreMeshMaterial MaterialForGroup(string groupName)
    {
        if (NamedTriangleGroups.ContainsKey(groupName))
        {
            string materialName = NamedTriangleGroups[groupName].MaterialName;
            return GetMaterial(materialName);
        }
        return KoreMeshMaterialPalette.DefaultMaterial; // No material for this group, or no group found
    }

    // --------------------------------------------------------------------------------------------

    public void AddTriangleToGroup(int triangleId, string groupName)
    {
        if (NamedTriangleGroups.ContainsKey(groupName))
        {
            NamedTriangleGroups[groupName].TriangleIds.Add(triangleId);
        }
        else
        {
            KoreMeshTriangleGroup newGroup = new KoreMeshTriangleGroup
            {
                MaterialName = KoreMeshMaterialPalette.DefaultMaterialName,
                TriangleIds = new List<int> { triangleId }
            };
            AddDefaultMaterial();
            NamedTriangleGroups[groupName] = newGroup;
        }
    }

    public void AddAllTrianglesToGroup(string groupName)
    {
        if (!NamedTriangleGroups.ContainsKey(groupName))
        {
            AddNamedGroup(groupName);
        }

        KoreMeshTriangleGroup group = NamedTriangleGroups[groupName];
        foreach (var triangle in Triangles)
        {
            group.TriangleIds.Add(triangle.Key);
        }
        NamedTriangleGroups[groupName] = group;
    }

    public void AddTrianglesToGroup(List<int> triangleIds, string groupName)
    {
        if (!NamedTriangleGroups.ContainsKey(groupName))
        {
            AddNamedGroup(groupName);
        }

        KoreMeshTriangleGroup group = NamedTriangleGroups[groupName];
        foreach (int triId in triangleIds)
        {
            if (Triangles.ContainsKey(triId) && !group.TriangleIds.Contains(triId))
            {
                group.TriangleIds.Add(triId);
            }
        }
        NamedTriangleGroups[groupName] = group;
    }

    public HashSet<KoreXYZVector> NamedGroupVertices(string groupName)
    {
        HashSet<KoreXYZVector> vertices = new HashSet<KoreXYZVector>();

        if (NamedTriangleGroups.ContainsKey(groupName))
        {
            KoreMeshTriangleGroup group = NamedTriangleGroups[groupName];
            foreach (int triangleId in group.TriangleIds)
            {
                if (Triangles.ContainsKey(triangleId))
                {
                    KoreMeshTriangle triangle = Triangles[triangleId];
                    vertices.Add(Vertices[triangle.A]);
                    vertices.Add(Vertices[triangle.B]);
                    vertices.Add(Vertices[triangle.C]);
                }
            }
        }

        return vertices;
    }


    // --------------------------------------------------------------------------------------------
    // MARK: Spatial Queries
    // --------------------------------------------------------------------------------------------

    // Find all vertices within a specified distance of a given point
    // Useful for identifying a shared vertex location between triangles
    public List<int> FindVerticesWithinDistance(KoreXYZVector targetPoint, double maxDistance)
    {
        List<int> nearbyVertices = new List<int>();
        double maxDistanceSquared = maxDistance * maxDistance; // Use squared distance for performance

        foreach (var kvp in Vertices)
        {
            int vertexId = kvp.Key;
            KoreXYZVector vertex = kvp.Value;

            // Calculate squared distance to avoid expensive sqrt operation
            double deltaX = vertex.X - targetPoint.X;
            double deltaY = vertex.Y - targetPoint.Y;
            double deltaZ = vertex.Z - targetPoint.Z;
            double distanceSquared = deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ;

            if (distanceSquared <= maxDistanceSquared)
            {
                nearbyVertices.Add(vertexId);
            }
        }

        return nearbyVertices;
    }

    // Find all vertices within a specified distance of a vertex by ID
    public List<int> FindVerticesWithinDistance(int targetVertexId, double maxDistance)
    {
        if (!Vertices.ContainsKey(targetVertexId))
            throw new ArgumentOutOfRangeException(nameof(targetVertexId), "Vertex ID is not found.");

        return FindVerticesWithinDistance(Vertices[targetVertexId], maxDistance);
    }

}
