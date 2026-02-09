// <fileheader>

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace KoreCommon;

// Static class to create KoreMeshData primitives
// This class is used to create various 3D shapes and meshes in Godot

public static partial class KoreMeshDataPrimitives
{
    // Create a basic pyramid mesh
    // - apexPoint: The apex point of the pyramid - which will be an offset from the node its ultimately attached to.
    // - apexBaseVector: The vector from the apex to the base center point.
    // - baseForwardVector: The vector, from the base centre point outwards, that defines the forward direction of the base
    // - width and height: relative to the forward or up direction of the base.
    public static KoreMeshData BasicPyramid(
        KoreXYZVector apexPoint, KoreXYZVector apexBaseVector, KoreXYZVector baseForwardVector,
        float width, float height,
        KoreColorRGB linecolor, KoreMeshMaterial material)
    {
        KoreMeshData pyramidMesh = new KoreMeshData();

        KoreColorRGB vertexColor = material.BaseColor;

        // Calculate base center point
        KoreXYZVector baseCenterPoint = apexPoint + apexBaseVector;

        // Ensure baseForwardVector is perpendicular to apexBaseVector
        // If they're not perpendicular, project baseForwardVector onto the plane perpendicular to apexBaseVector
        KoreXYZVector normalizedApexBase = apexBaseVector.Normalize();
        double dotProduct = KoreXYZVector.DotProduct(baseForwardVector, normalizedApexBase);
        KoreXYZVector projectedForward = baseForwardVector - (normalizedApexBase * dotProduct);
        KoreXYZVector normalizedForward = projectedForward.Normalize();

        // Create the right vector perpendicular to both apexBaseVector and baseForwardVector
        KoreXYZVector rightVector = KoreXYZVector.CrossProduct(normalizedForward, normalizedApexBase).Normalize();

        // Calculate the 4 base vertices for a square pyramid
        double halfWidth = width * 0.5;
        double halfHeight = height * 0.5;

        KoreXYZVector baseVertex1 = baseCenterPoint + (normalizedForward * halfHeight) + (rightVector * halfWidth);   // front-right
        KoreXYZVector baseVertex2 = baseCenterPoint + (normalizedForward * halfHeight) - (rightVector * halfWidth);   // front-left
        KoreXYZVector baseVertex3 = baseCenterPoint - (normalizedForward * halfHeight) - (rightVector * halfWidth);   // back-left
        KoreXYZVector baseVertex4 = baseCenterPoint - (normalizedForward * halfHeight) + (rightVector * halfWidth);   // back-right

        // Convert points to vectors for AddVertex (AddVertex expects KoreXYZVector)
        int idxApex  = pyramidMesh.AddCompleteVertex(new KoreXYZVector(apexPoint.X, apexPoint.Y, apexPoint.Z), null, vertexColor);
        int idxBase1 = pyramidMesh.AddCompleteVertex(new KoreXYZVector(baseVertex1.X, baseVertex1.Y, baseVertex1.Z), null, vertexColor);
        int idxBase2 = pyramidMesh.AddCompleteVertex(new KoreXYZVector(baseVertex2.X, baseVertex2.Y, baseVertex2.Z), null, vertexColor);
        int idxBase3 = pyramidMesh.AddCompleteVertex(new KoreXYZVector(baseVertex3.X, baseVertex3.Y, baseVertex3.Z), null, vertexColor);
        int idxBase4 = pyramidMesh.AddCompleteVertex(new KoreXYZVector(baseVertex4.X, baseVertex4.Y, baseVertex4.Z), null, vertexColor);

        // Add base (optional - you might want a solid base)
        pyramidMesh.AddGroupWithMaterial("base", material);
        pyramidMesh.AddGroupWithMaterial("sides", material);

        // Add triangles for the pyramid faces (CW winding from outside)
        pyramidMesh.AddTriangleToGroup(pyramidMesh.AddTriangle(idxApex, idxBase2, idxBase1), "sides"); // front face
        pyramidMesh.AddTriangleToGroup(pyramidMesh.AddTriangle(idxApex, idxBase3, idxBase2), "sides"); // left face
        pyramidMesh.AddTriangleToGroup(pyramidMesh.AddTriangle(idxApex, idxBase4, idxBase3), "sides"); // back face
        pyramidMesh.AddTriangleToGroup(pyramidMesh.AddTriangle(idxApex, idxBase1, idxBase4), "sides"); // right face

        pyramidMesh.AddTriangleToGroup(pyramidMesh.AddTriangle(idxBase1, idxBase2, idxBase3), "base"); // base triangle 1
        pyramidMesh.AddTriangleToGroup(pyramidMesh.AddTriangle(idxBase1, idxBase3, idxBase4), "base"); // base triangle 2

        // Add wireframe lines (match the CW triangle winding)
        pyramidMesh.OutlineTriangle(idxApex, idxBase2, idxBase1, linecolor);
        pyramidMesh.OutlineTriangle(idxApex, idxBase3, idxBase2, linecolor);
        pyramidMesh.OutlineTriangle(idxApex, idxBase4, idxBase3, linecolor);
        pyramidMesh.OutlineTriangle(idxApex, idxBase1, idxBase4, linecolor);

        return pyramidMesh;
    }

    // Usage: var pyramidMesh = KoreMeshDataPrimitives.BasicPyramidSharpEdges(apex, baseVector, forwardVector, 2.0f, 2.0f, new KoreColorRGB(255, 0, 0));
    public static KoreMeshData BasicPyramidSharpEdges(
        KoreXYZVector apexVector, KoreXYZVector offsetToBaseCenter, KoreXYZVector baseForwardVector,
        float width, float height,
        KoreColorRGB linecolor, KoreMeshMaterial material)
    {
        KoreMeshData pyramidMesh = new KoreMeshData();

        KoreColorRGB vertexColor = material.BaseColor;

        // Calculate base center point
        KoreXYZVector baseCenter = apexVector + offsetToBaseCenter;

        // Ensure baseForwardVector is perpendicular to apexBaseVector
        // If they're not perpendicular, project baseForwardVector onto the plane perpendicular to apexBaseVector
        KoreXYZVector normalizedApexBase = offsetToBaseCenter.Normalize();
        double dotProduct = KoreXYZVector.DotProduct(baseForwardVector, normalizedApexBase);
        KoreXYZVector projectedForward = baseForwardVector - (normalizedApexBase * dotProduct);
        KoreXYZVector normalizedForward = projectedForward.Normalize();

        // Create the right vector perpendicular to both apexBaseVector and baseForwardVector
        KoreXYZVector rightVector = KoreXYZVector.CrossProduct(normalizedForward, normalizedApexBase).Normalize();

        // Calculate the 4 base vertices for a square pyramid
        double halfWidth = width * 0.5;
        double halfHeight = height * 0.5;

        KoreXYZVector baseVertex1 = offsetToBaseCenter + (normalizedForward * halfHeight) + (rightVector * halfWidth);   // front-right
        KoreXYZVector baseVertex2 = offsetToBaseCenter + (normalizedForward * halfHeight) - (rightVector * halfWidth);   // front-left
        KoreXYZVector baseVertex3 = offsetToBaseCenter - (normalizedForward * halfHeight) - (rightVector * halfWidth);   // back-left
        KoreXYZVector baseVertex4 = offsetToBaseCenter - (normalizedForward * halfHeight) + (rightVector * halfWidth);   // back-right

        // Add base (optional - you might want a solid base)
        pyramidMesh.AddGroupWithMaterial("base", material);
        pyramidMesh.AddGroupWithMaterial("sides", material);

        // Add triangles for the pyramid faces
        pyramidMesh.AddTriangleToGroup(pyramidMesh.AddTriangle(apexVector, baseVertex1, baseVertex2), "sides"); // front face
        pyramidMesh.AddTriangleToGroup(pyramidMesh.AddTriangle(apexVector, baseVertex2, baseVertex3), "sides"); // left face
        pyramidMesh.AddTriangleToGroup(pyramidMesh.AddTriangle(apexVector, baseVertex3, baseVertex4), "sides"); // back face
        pyramidMesh.AddTriangleToGroup(pyramidMesh.AddTriangle(apexVector, baseVertex4, baseVertex1), "sides"); // right face

        pyramidMesh.AddTriangleToGroup(pyramidMesh.AddTriangle(baseVertex1, baseVertex4, baseVertex3), "base"); // base triangle 1
        pyramidMesh.AddTriangleToGroup(pyramidMesh.AddTriangle(baseVertex1, baseVertex3, baseVertex2), "base"); // base triangle 2

        // Add wireframe lines
        int idxApex  = pyramidMesh.AddVertex(apexVector);
        int idxBase1 = pyramidMesh.AddVertex(baseVertex1);
        int idxBase2 = pyramidMesh.AddVertex(baseVertex2);
        int idxBase3 = pyramidMesh.AddVertex(baseVertex3);
        int idxBase4 = pyramidMesh.AddVertex(baseVertex4);

        pyramidMesh.OutlineTriangle(idxApex, idxBase1, idxBase2, linecolor);
        pyramidMesh.OutlineTriangle(idxApex, idxBase2, idxBase3, linecolor);
        pyramidMesh.OutlineTriangle(idxApex, idxBase3, idxBase4, linecolor);
        pyramidMesh.OutlineTriangle(idxApex, idxBase4, idxBase1, linecolor);

        return pyramidMesh;
    }


}
