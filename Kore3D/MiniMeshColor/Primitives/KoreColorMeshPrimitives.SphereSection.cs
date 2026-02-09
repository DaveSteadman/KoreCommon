// // <fileheader>

// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.IO;

// namespace KoreCommon;

// // Static class to create KoreMeshData primitives
// // This class is used to create various 3D shapes and meshes in Godot

// public static partial class KoreColorMeshPrimitives
// {
//     // Create a sphere mesh for KoreColorMesh, with a given center, radius, and colormap
//     // - We pick the number of lat/long segments from the color list dimensions
//     // Usage: KoreColorMesh sphereMesh = KoreColorMeshPrimitives.SphereSection(center, azElRange, radius, colormap);

//     public static KoreColorMesh SphereSection(
//         KoreXYZVector center,
//         KoreLLBox llBox,
//         double radius,
//         KoreColorRGB[,] colormap,
//         KoreNumeric2DArray<float> tileEleData)
//     {
//         var mesh = new KoreColorMesh();

//         int lonSegments = colormap.GetLength(1); // longitude segments (horizontal divisions)
//         int latSegments = colormap.GetLength(0); // latitude segments (vertical divisions)

//         // Create a simple double-list that includes poles as duplicated vertices
//         var vertexIdGrid = new List<List<int>>();

//         for (int lat = 0; lat <= latSegments; lat++) // Include both poles
//         {
//             // flip the lat, to go from top to bottom
//             int usedLat = latSegments - lat;
//             double latDegs = llBox.MinLatDegs + (llBox.DeltaLatDegs * usedLat / latSegments);
//             float latFraction = (float)lat / latSegments;

//             var latRow = new List<int>();

//             for (int lon = 0; lon <= lonSegments; lon++)
//             {
//                 double lonDegs = llBox.MinLonDegs + (llBox.DeltaLonDegs * lon / lonSegments);
//                 float lonFraction = (float)lon / lonSegments;

//                 double ele = radius + tileEleData.InterpolatedValue(lonFraction, latFraction);

//                 //KoreCentralLog.AddEntry($"lat: {latDegs:F2}, lon: {lonDegs:F2}, rad: {radius:F2}, ele: {tileEleData.InterpolatedValue(lonFraction, latFraction)}");

//                 KoreLLAPoint rwLLAPointPos = new KoreLLAPoint() { LatDegs = latDegs, LonDegs = lonDegs, RadiusM = ele };
//                 KoreXYZVector rwXYZPointPos = rwLLAPointPos.ToXYZ();

//                 // ---- convert from real-world to game engine ----
//                 rwXYZPointPos = new KoreXYZVector(rwXYZPointPos.X, rwXYZPointPos.Y, -rwXYZPointPos.Z);

//                 KoreXYZVector vertex = rwXYZPointPos;
//                 int vertexId = mesh.AddVertex(vertex);
//                 latRow.Add(vertexId);
//             }

//             vertexIdGrid.Add(latRow);
//         }

//         // Create triangles between all adjacent latitude rows
//         for (int lat = 0; lat < latSegments; lat++)
//         {
//             for (int lon = 0; lon < lonSegments; lon++)
//             {
//                 // Get the four vertices of the current quad
//                 int v1 = vertexIdGrid[lat][lon];         // current lat, current lon
//                 int v2 = vertexIdGrid[lat + 1][lon];     // next lat, current lon
//                 int v3 = vertexIdGrid[lat + 1][lon + 1]; // next lat, next lon
//                 int v4 = vertexIdGrid[lat][lon + 1];     // current lat, next lon

//                 // Use colormap coordinates
//                 KoreColorRGB col = colormap[lat % colormap.GetLength(0), lon % colormap.GetLength(1)];

//                 // Add the quad as two triangles using AddFace helper
//                 KoreColorMeshOps.AddFace(mesh, v1, v4, v3, v2, col);
//             }
//         }

//         return mesh;
//     }

//     // --------------------------------------------------------------------------------------------

//     public static KoreColorMesh CenteredSphereSection(
//         KoreLLBox llBox,
//         double radius,
//         KoreColorRGB[,] colormap,
//         KoreNumeric2DArray<float> tileEleData)
//     {
//         var mesh = new KoreColorMesh();


//         // get the array lengths
//         int lonSegments = colormap.GetLength(1); // longitude segments (horizontal divisions)
//         int latSegments = colormap.GetLength(0); // latitude segments (vertical divisions)

//         // find the real-world center
//         KoreLLPoint rwTileCenter = llBox.CenterPoint;
//         KoreLLAPoint rwTileCenterLLA = new KoreLLAPoint() {
//             LatDegs = rwTileCenter.LatDegs,
//             LonDegs = rwTileCenter.LonDegs,
//             RadiusM = radius
//         };

//         // Zero Lon Center - so we create a common tile, then rotate into position later
//         // - also means we can create the tile from relative angles, which is easier to manage

//         // Define zero longitude center, so we can create the tile from relative (not absolute) angles and
//         // more intuitively rotate the tile to the absolute longitude later.
//         KoreLLAPoint rwLLAZeroLonCenter = new KoreLLAPoint()
//         {
//             LatDegs = rwTileCenterLLA.LatDegs,
//             LonDegs = 0,
//             RadiusM = radius
//         };
//         KoreXYZVector rwXYZZeroLonCenter = rwLLAZeroLonCenter.ToXYZ();

//         double boxWidthDegs = llBox.DeltaLonDegs;
//         double boxHalfWidthDegs = boxWidthDegs / 2.0;

//         // Setup the loop control values
//         // List<double> lonZeroListDegs = KoreValueUtils.CreateRangeList(lonSegments, -boxHalfWidthDegs, boxHalfWidthDegs); // Relative azimuth - left to right (low to high longitude)
//         // List<double> latListDegs = KoreValueUtils.CreateRangeList(latSegments, llBox.MaxLatDegs, llBox.MinLatDegs); // Max to min +90 -> -90. Start at top of tile

//         // Create a simple double-list for the vertex IDs
//         var vertexIdGrid = new List<List<int>>();



//         double stepLonDegs = llBox.DeltaLonDegs / lonSegments;
//         double stepLatDegs = llBox.DeltaLatDegs / latSegments;



//         for (int lat = 0; lat <= latSegments; lat++) // Include both poles
//         {
//             // flip the lat, to go from top to bottom
//             int usedLat = lat;

//             double latDegs = llBox.MaxLatDegs - (stepLatDegs * lat);
//             //latDegs = latListDegs[lat];
//             float latFraction =  ((float)lat / latSegments);

//             var latRow = new List<int>();

//             for (int lon = 0; lon <= lonSegments; lon++)
//             {
//                 double lonDegs = -boxHalfWidthDegs + (stepLonDegs * lon);

//                 float lonFraction =  (float)lon / lonSegments;

//                 double eleAmplifier = 1;

//                 // Get real-world radius with elevation
//                 double realWorldRadiusWithElevation = KoreWorldConsts.EarthRadiusM + (eleAmplifier * tileEleData.InterpolatedValue(lonFraction, latFraction));

//                 // Scale to game engine radius
//                 double gameEngineRadius = (realWorldRadiusWithElevation / KoreWorldConsts.EarthRadiusM) * radius;

//                 //KoreCentralLog.AddEntry($"lat: {latDegs:F2}, lon: {lonDegs:F2}, rad: {radius:F2}, ele: {tileEleData.InterpolatedValue(lonFraction, latFraction)}");

//                 KoreLLAPoint rwLLAPointPos = new KoreLLAPoint() { LatDegs = latDegs, LonDegs = lonDegs, RadiusM = gameEngineRadius };
//                 KoreXYZVector rwXYZPointPos = rwLLAPointPos.ToXYZ();

//                 KoreXYZVector rwXYZCenterOffset = rwXYZZeroLonCenter.XYZTo(rwXYZPointPos);

//                 // ---- convert from real-world to game engine ----
//                 rwXYZPointPos = new KoreXYZVector(rwXYZCenterOffset.X, rwXYZCenterOffset.Y, rwXYZCenterOffset.Z);
//                 KoreXYZVector geVector = new KoreXYZVector(rwXYZPointPos.X, rwXYZPointPos.Y, -rwXYZPointPos.Z);

//                 // KoreXYZVector vertex = rwXYZPointPos;
//                 int vertexId = mesh.AddVertex(geVector);
//                 latRow.Add(vertexId);
//             }

//             vertexIdGrid.Add(latRow);
//         }

//         // colormap is x,y 0,0 top left

//         // Create triangles between all adjacent latitude rows
//         for (int lat = 0; lat < latSegments; lat++)
//         {
//             int yid =  lat % colormap.GetLength(1);

//             for (int lon = 0; lon < lonSegments; lon++)
//             {
//                 int xid =  lon % colormap.GetLength(0);

//                 // Get the four vertices of the current quad
//                 int v1 = vertexIdGrid[lat][lon];         // current lat, current lon
//                 int v2 = vertexIdGrid[lat + 1][lon];     // next lat, current lon
//                 int v3 = vertexIdGrid[lat + 1][lon + 1]; // next lat, next lon
//                 int v4 = vertexIdGrid[lat][lon + 1];     // current lat, next lon

//                 // Use colormap coordinates
//                 KoreColorRGB col = colormap[xid, yid];

//                 // Add the quad as two triangles using AddFace helper
//                 KoreColorMeshOps.AddFace(mesh, v1, v4, v3, v2, col);
//             }
//         }

//         return mesh;
//     }


// }
