// <fileheader>

using System;
using System.Collections.Generic;

#nullable enable

namespace KoreCommon;


// KoreMeshDataEditOps: A static class to hold functions to edit a mesh

public static partial class KoreMeshDataEditOps
{
    // --------------------------------------------------------------------------------------------
    // MARK: Bounding Box
    // --------------------------------------------------------------------------------------------

    // Take a mesh and two triangle Id from a face. inset new points and triangles from that, returning the two most-inset triangle IDs.

    public static List<int> InsetFace(KoreMeshData meshData, int triId1, int triId2, double insetDist)
    {
        // Create new triangles from the inset points
        List<int> newTriIds = new List<int>();

        // If either triangle is missing, bail and return an empty list
        if (!meshData.HasTriangle(triId1) || !meshData.HasTriangle(triId2))
            return newTriIds;

        // Read all the details
        KoreMeshTriangle tri1 = meshData.Triangles[triId1];
        KoreMeshTriangle tri2 = meshData.Triangles[triId2];

        int pntAid = tri1.A;
        int pntBid = tri1.B;
        int pntCid = tri1.C;
        int pntDid = tri2.A;

        KoreXYZVector pntA = meshData.Vertices[pntAid];
        KoreXYZVector pntB = meshData.Vertices[pntBid];
        KoreXYZVector pntC = meshData.Vertices[pntCid];
        KoreXYZVector pntD = meshData.Vertices[pntDid];

        // Delete the two old triangles
        KoreMeshDataEditOps.DeleteTriangle(meshData, triId1);
        KoreMeshDataEditOps.DeleteTriangle(meshData, triId2);

        // Construct and add the new points
        KoreXYZVector pntInsetA = KoreXYZVectorOps.InsetPoint(pntD, pntA, pntB, insetDist);
        KoreXYZVector pntInsetB = KoreXYZVectorOps.InsetPoint(pntA, pntB, pntC, insetDist);
        KoreXYZVector pntInsetC = KoreXYZVectorOps.InsetPoint(pntB, pntC, pntD, insetDist);
        KoreXYZVector pntInsetD = KoreXYZVectorOps.InsetPoint(pntB, pntC, pntD, insetDist);

        int pntInsetAid = meshData.AddVertex(pntInsetA);
        int pntInsetBid = meshData.AddVertex(pntInsetB);
        int pntInsetCid = meshData.AddVertex(pntInsetC);
        int pntInsetDid = meshData.AddVertex(pntInsetD);

        // Add the two new inset triangles - so we know they are the first two in the returned list
        newTriIds.Add(meshData.AddTriangle(new KoreMeshTriangle(pntInsetAid, pntInsetBid, pntInsetCid)));
        newTriIds.Add(meshData.AddTriangle(new KoreMeshTriangle(pntInsetAid, pntInsetCid, pntInsetDid)));

        // Add the eight new triangles surrounding the new center two
        newTriIds.Add(meshData.AddTriangle(new KoreMeshTriangle(pntAid, pntBid, pntInsetAid)));
        newTriIds.Add(meshData.AddTriangle(new KoreMeshTriangle(pntInsetAid, pntBid, pntInsetBid)));
        newTriIds.Add(meshData.AddTriangle(new KoreMeshTriangle(pntBid, pntCid, pntInsetBid)));
        newTriIds.Add(meshData.AddTriangle(new KoreMeshTriangle(pntInsetBid, pntCid, pntInsetCid)));
        newTriIds.Add(meshData.AddTriangle(new KoreMeshTriangle(pntCid, pntDid, pntInsetCid)));
        newTriIds.Add(meshData.AddTriangle(new KoreMeshTriangle(pntInsetCid, pntDid, pntInsetDid)));
        newTriIds.Add(meshData.AddTriangle(new KoreMeshTriangle(pntDid, pntAid, pntInsetDid)));
        newTriIds.Add(meshData.AddTriangle(new KoreMeshTriangle(pntInsetDid, pntAid, pntInsetAid)));

        return newTriIds;
    }

}
