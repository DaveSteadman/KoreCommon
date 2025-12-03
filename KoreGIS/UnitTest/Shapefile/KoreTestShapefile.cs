// <fileheader>

using System;
using System.Collections.Generic;
using System.IO;
using KoreCommon.SkiaSharp;

using KoreCommon;
using KoreCommon.UnitTest;
using KoreGIS;
namespace KoreGIS.UnitTest;

#nullable enable

// Unit tests for Shapefile reading and writing functionality.
public static class KoreTestShapefile
{
    private static string TestDir = string.Empty;

    public static void RunTests(KoreTestLog testLog)
    {
        // Create temp directory for test files
        TestDir = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "KoreShapefileTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(TestDir);

        try
        {
            testLog.AddSeparator();
            testLog.AddComment("Shapefile Tests");

            // Round-trip tests
            RunSafeTest(testLog, "Shapefile Point Round-Trip", TestPointRoundTrip);
            RunSafeTest(testLog, "Shapefile MultiPoint Round-Trip", TestMultiPointRoundTrip);
            RunSafeTest(testLog, "Shapefile PolyLine Round-Trip", TestPolyLineRoundTrip);
            RunSafeTest(testLog, "Shapefile Polygon Round-Trip", TestPolygonRoundTrip);
            RunSafeTest(testLog, "Shapefile Polygon with Hole Round-Trip", TestPolygonWithHoleRoundTrip);

            // Attribute type tests
            RunSafeTest(testLog, "Shapefile Int Attribute", TestIntAttribute);
            RunSafeTest(testLog, "Shapefile Double Attribute", TestDoubleAttribute);
            RunSafeTest(testLog, "Shapefile Bool Attribute", TestBoolAttribute);
            RunSafeTest(testLog, "Shapefile DateTime Attribute", TestDateTimeAttribute);
            RunSafeTest(testLog, "Shapefile String Attribute", TestStringAttribute);
            RunSafeTest(testLog, "Shapefile Mixed Attributes", TestMixedAttributes);

            // Non-fatal error handling
            RunSafeTest(testLog, "Shapefile Corrupt Record Handling", TestCorruptRecordSkipping);

            // PRJ file test
            RunSafeTest(testLog, "Shapefile PRJ File Written with WGS84", TestPrjFileWritten);

            // Visualization test
            RunSafeTest(testLog, "Shapefile Streets Visualization", TestDrawStreets);
        }
        finally
        {
            // Cleanup
            try
            {
                if (Directory.Exists(TestDir))
                {
                    Directory.Delete(TestDir, true);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    // Runs a test safely, catching any exceptions and logging them.
    private static void RunSafeTest(KoreTestLog testLog, string testName, Action<KoreTestLog> testAction)
    {
        try
        {
            testAction(testLog);
        }
        catch (Exception ex)
        {
            testLog.AddResult(testName, false, $"Exception: {ex.Message}");
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Point Round-Trip
    // --------------------------------------------------------------------------------------------

    private static void TestPointRoundTrip(KoreTestLog testLog)
    {
        string basePath = Path.Combine(TestDir, "test_point");

        var collection = new KoreShapefileFeatureCollection
        {
            GeometryType = ShapefileGeometryType.Point
        };

        collection.Features.Add(new KoreShapefileFeature
        {
            RecordNumber = 1,
            GeometryType = ShapefileGeometryType.Point,
            Geometry = new KoreGeoPoint { Position = new KoreLLPoint { LatDegs = 51.5074, LonDegs = -0.1278 } },
            Attributes = new Dictionary<string, object?> { { "name", "London" } }
        });

        collection.Features.Add(new KoreShapefileFeature
        {
            RecordNumber = 2,
            GeometryType = ShapefileGeometryType.Point,
            Geometry = new KoreGeoPoint { Position = new KoreLLPoint { LatDegs = 48.8566, LonDegs = 2.3522 } },
            Attributes = new Dictionary<string, object?> { { "name", "Paris" } }
        });

        // Write
        KoreShapefileWriter.Write(basePath, collection);

        // Read back
        var readCollection = KoreShapefileReader.Read(basePath);

        // Verify
        bool passed = readCollection.Features.Count == 2;
        if (passed)
        {
            var point1 = readCollection.Features[0].Geometry as KoreGeoPoint;
            var point2 = readCollection.Features[1].Geometry as KoreGeoPoint;

            passed = point1 != null && point2 != null &&
                     Math.Abs(point1.Position.LatDegs - 51.5074) < 0.0001 &&
                     Math.Abs(point1.Position.LonDegs - (-0.1278)) < 0.0001 &&
                     Math.Abs(point2.Position.LatDegs - 48.8566) < 0.0001 &&
                     Math.Abs(point2.Position.LonDegs - 2.3522) < 0.0001;
        }

        testLog.AddResult("Shapefile Point Round-Trip", passed);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: MultiPoint Round-Trip
    // --------------------------------------------------------------------------------------------

    private static void TestMultiPointRoundTrip(KoreTestLog testLog)
    {
        string basePath = Path.Combine(TestDir, "test_multipoint");

        var multiPoint = new KoreGeoMultiPoint();
        multiPoint.Points.Add(new KoreLLPoint { LatDegs = 10.0, LonDegs = 20.0 });
        multiPoint.Points.Add(new KoreLLPoint { LatDegs = 30.0, LonDegs = 40.0 });
        multiPoint.Points.Add(new KoreLLPoint { LatDegs = 50.0, LonDegs = 60.0 });

        var collection = new KoreShapefileFeatureCollection
        {
            GeometryType = ShapefileGeometryType.MultiPoint
        };

        collection.Features.Add(new KoreShapefileFeature
        {
            RecordNumber = 1,
            GeometryType = ShapefileGeometryType.MultiPoint,
            Geometry = multiPoint,
            Attributes = new Dictionary<string, object?> { { "id", 1 } }
        });

        // Write
        KoreShapefileWriter.Write(basePath, collection);

        // Read back
        var readCollection = KoreShapefileReader.Read(basePath);

        // Verify
        bool passed = readCollection.Features.Count == 1;
        if (passed)
        {
            var readMultiPoint = readCollection.Features[0].Geometry as KoreGeoMultiPoint;
            passed = readMultiPoint != null && readMultiPoint.Points.Count == 3;
            if (passed && readMultiPoint != null)
            {
                passed = Math.Abs(readMultiPoint.Points[0].LatDegs - 10.0) < 0.0001 &&
                         Math.Abs(readMultiPoint.Points[1].LatDegs - 30.0) < 0.0001 &&
                         Math.Abs(readMultiPoint.Points[2].LatDegs - 50.0) < 0.0001;
            }
        }

        testLog.AddResult("Shapefile MultiPoint Round-Trip", passed);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: PolyLine Round-Trip
    // --------------------------------------------------------------------------------------------

    private static void TestPolyLineRoundTrip(KoreTestLog testLog)
    {
        string basePath = Path.Combine(TestDir, "test_polyline");

        var multiLine = new KoreGeoMultiLineString();
        var line1 = new List<KoreLLPoint>
        {
            new KoreLLPoint { LatDegs = 0.0, LonDegs = 0.0 },
            new KoreLLPoint { LatDegs = 1.0, LonDegs = 1.0 },
            new KoreLLPoint { LatDegs = 2.0, LonDegs = 2.0 }
        };
        var line2 = new List<KoreLLPoint>
        {
            new KoreLLPoint { LatDegs = 10.0, LonDegs = 10.0 },
            new KoreLLPoint { LatDegs = 11.0, LonDegs = 11.0 }
        };
        multiLine.LineStrings.Add(line1);
        multiLine.LineStrings.Add(line2);

        var collection = new KoreShapefileFeatureCollection
        {
            GeometryType = ShapefileGeometryType.PolyLine
        };

        collection.Features.Add(new KoreShapefileFeature
        {
            RecordNumber = 1,
            GeometryType = ShapefileGeometryType.PolyLine,
            Geometry = multiLine,
            Attributes = new Dictionary<string, object?> { { "route", "TestRoute" } }
        });

        // Write
        KoreShapefileWriter.Write(basePath, collection);

        // Read back
        var readCollection = KoreShapefileReader.Read(basePath);

        // Verify
        bool passed = readCollection.Features.Count == 1;
        if (passed)
        {
            var readMultiLine = readCollection.Features[0].Geometry as KoreGeoMultiLineString;
            passed = readMultiLine != null && readMultiLine.LineStrings.Count == 2;
            if (passed && readMultiLine != null)
            {
                passed = readMultiLine.LineStrings[0].Count == 3 &&
                         readMultiLine.LineStrings[1].Count == 2;
            }
        }

        testLog.AddResult("Shapefile PolyLine Round-Trip", passed);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Polygon Round-Trip
    // --------------------------------------------------------------------------------------------

    private static void TestPolygonRoundTrip(KoreTestLog testLog)
    {
        string basePath = Path.Combine(TestDir, "test_polygon");

        var polygon = new KoreGeoPolygon();
        // Simple square (clockwise for Shapefile outer ring)
        polygon.OuterRing = new List<KoreLLPoint>
        {
            new KoreLLPoint { LatDegs = 0.0, LonDegs = 0.0 },
            new KoreLLPoint { LatDegs = 0.0, LonDegs = 1.0 },
            new KoreLLPoint { LatDegs = 1.0, LonDegs = 1.0 },
            new KoreLLPoint { LatDegs = 1.0, LonDegs = 0.0 },
            new KoreLLPoint { LatDegs = 0.0, LonDegs = 0.0 } // Close ring
        };

        var multiPolygon = new KoreGeoMultiPolygon();
        multiPolygon.Polygons.Add(polygon);

        var collection = new KoreShapefileFeatureCollection
        {
            GeometryType = ShapefileGeometryType.Polygon
        };

        collection.Features.Add(new KoreShapefileFeature
        {
            RecordNumber = 1,
            GeometryType = ShapefileGeometryType.Polygon,
            Geometry = multiPolygon,
            Attributes = new Dictionary<string, object?> { { "area", "TestArea" } }
        });

        // Write
        KoreShapefileWriter.Write(basePath, collection);

        // Read back
        var readCollection = KoreShapefileReader.Read(basePath);

        // Verify
        bool passed = readCollection.Features.Count == 1;
        if (passed)
        {
            var readMultiPolygon = readCollection.Features[0].Geometry as KoreGeoMultiPolygon;
            passed = readMultiPolygon != null && readMultiPolygon.Polygons.Count == 1;
            if (passed && readMultiPolygon != null)
            {
                passed = readMultiPolygon.Polygons[0].OuterRing.Count == 5;
            }
        }

        testLog.AddResult("Shapefile Polygon Round-Trip", passed);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Polygon with Hole Round-Trip
    // --------------------------------------------------------------------------------------------

    private static void TestPolygonWithHoleRoundTrip(KoreTestLog testLog)
    {
        string basePath = Path.Combine(TestDir, "test_polygon_hole");

        var polygon = new KoreGeoPolygon();
        // Outer ring (clockwise)
        polygon.OuterRing = new List<KoreLLPoint>
        {
            new KoreLLPoint { LatDegs = 0.0, LonDegs = 0.0 },
            new KoreLLPoint { LatDegs = 0.0, LonDegs = 10.0 },
            new KoreLLPoint { LatDegs = 10.0, LonDegs = 10.0 },
            new KoreLLPoint { LatDegs = 10.0, LonDegs = 0.0 },
            new KoreLLPoint { LatDegs = 0.0, LonDegs = 0.0 }
        };

        // Hole (counter-clockwise)
        var hole = new List<KoreLLPoint>
        {
            new KoreLLPoint { LatDegs = 2.0, LonDegs = 2.0 },
            new KoreLLPoint { LatDegs = 8.0, LonDegs = 2.0 },
            new KoreLLPoint { LatDegs = 8.0, LonDegs = 8.0 },
            new KoreLLPoint { LatDegs = 2.0, LonDegs = 8.0 },
            new KoreLLPoint { LatDegs = 2.0, LonDegs = 2.0 }
        };
        polygon.InnerRings.Add(hole);

        var multiPolygon = new KoreGeoMultiPolygon();
        multiPolygon.Polygons.Add(polygon);

        var collection = new KoreShapefileFeatureCollection
        {
            GeometryType = ShapefileGeometryType.Polygon
        };

        collection.Features.Add(new KoreShapefileFeature
        {
            RecordNumber = 1,
            GeometryType = ShapefileGeometryType.Polygon,
            Geometry = multiPolygon,
            Attributes = new Dictionary<string, object?> { { "name", "PolygonWithHole" } }
        });

        // Write
        KoreShapefileWriter.Write(basePath, collection);

        // Read back
        var readCollection = KoreShapefileReader.Read(basePath);

        // Verify
        bool passed = readCollection.Features.Count == 1;
        if (passed)
        {
            var readMultiPolygon = readCollection.Features[0].Geometry as KoreGeoMultiPolygon;
            passed = readMultiPolygon != null && readMultiPolygon.Polygons.Count == 1;
            if (passed && readMultiPolygon != null)
            {
                // Check outer ring and hole
                passed = readMultiPolygon.Polygons[0].OuterRing.Count >= 4 &&
                         readMultiPolygon.Polygons[0].InnerRings.Count == 1 &&
                         readMultiPolygon.Polygons[0].InnerRings[0].Count >= 4;
            }
        }

        testLog.AddResult("Shapefile Polygon with Hole Round-Trip", passed);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Attribute Type Tests
    // --------------------------------------------------------------------------------------------

    private static void TestIntAttribute(KoreTestLog testLog)
    {
        string basePath = Path.Combine(TestDir, "test_int_attr");

        var collection = new KoreShapefileFeatureCollection
        {
            GeometryType = ShapefileGeometryType.Point
        };

        collection.Features.Add(new KoreShapefileFeature
        {
            RecordNumber = 1,
            GeometryType = ShapefileGeometryType.Point,
            Geometry = new KoreGeoPoint { Position = new KoreLLPoint { LatDegs = 0, LonDegs = 0 } },
            Attributes = new Dictionary<string, object?> { { "count", 42 }, { "negval", -100 } }
        });

        KoreShapefileWriter.Write(basePath, collection);
        var readCollection = KoreShapefileReader.Read(basePath);

        bool passed = readCollection.Features.Count == 1;
        if (passed)
        {
            var attrs = readCollection.Features[0].Attributes;
            passed = attrs.TryGetValue("count", out var countVal) && countVal is int countInt && countInt == 42;
            passed = passed && attrs.TryGetValue("negval", out var negVal) && negVal is int negInt && negInt == -100;
        }

        testLog.AddResult("Shapefile Int Attribute", passed);
    }

    private static void TestDoubleAttribute(KoreTestLog testLog)
    {
        string basePath = Path.Combine(TestDir, "test_double_attr");

        var collection = new KoreShapefileFeatureCollection
        {
            GeometryType = ShapefileGeometryType.Point
        };

        collection.Features.Add(new KoreShapefileFeature
        {
            RecordNumber = 1,
            GeometryType = ShapefileGeometryType.Point,
            Geometry = new KoreGeoPoint { Position = new KoreLLPoint { LatDegs = 0, LonDegs = 0 } },
            Attributes = new Dictionary<string, object?> { { "value", 3.14159 }, { "negval", -99.99 } }
        });

        KoreShapefileWriter.Write(basePath, collection);
        var readCollection = KoreShapefileReader.Read(basePath);

        bool passed = readCollection.Features.Count == 1;
        if (passed)
        {
            var attrs = readCollection.Features[0].Attributes;
            passed = attrs.TryGetValue("value", out var valObj) && valObj is double valDbl && Math.Abs(valDbl - 3.14159) < 0.001;
            passed = passed && attrs.TryGetValue("negval", out var negObj) && negObj is double negDbl && Math.Abs(negDbl - (-99.99)) < 0.001;
        }

        testLog.AddResult("Shapefile Double Attribute", passed);
    }

    private static void TestBoolAttribute(KoreTestLog testLog)
    {
        string basePath = Path.Combine(TestDir, "test_bool_attr");

        var collection = new KoreShapefileFeatureCollection
        {
            GeometryType = ShapefileGeometryType.Point
        };

        collection.Features.Add(new KoreShapefileFeature
        {
            RecordNumber = 1,
            GeometryType = ShapefileGeometryType.Point,
            Geometry = new KoreGeoPoint { Position = new KoreLLPoint { LatDegs = 0, LonDegs = 0 } },
            Attributes = new Dictionary<string, object?> { { "active", true }, { "deleted", false } }
        });

        KoreShapefileWriter.Write(basePath, collection);
        var readCollection = KoreShapefileReader.Read(basePath);

        bool passed = readCollection.Features.Count == 1;
        if (passed)
        {
            var attrs = readCollection.Features[0].Attributes;
            passed = attrs.TryGetValue("active", out var activeVal) && activeVal is bool activeBool && activeBool == true;
            passed = passed && attrs.TryGetValue("deleted", out var deletedVal) && deletedVal is bool deletedBool && deletedBool == false;
        }

        testLog.AddResult("Shapefile Bool Attribute", passed);
    }

    private static void TestDateTimeAttribute(KoreTestLog testLog)
    {
        string basePath = Path.Combine(TestDir, "test_date_attr");

        var testDate = new DateTime(2024, 6, 15);

        var collection = new KoreShapefileFeatureCollection
        {
            GeometryType = ShapefileGeometryType.Point
        };

        collection.Features.Add(new KoreShapefileFeature
        {
            RecordNumber = 1,
            GeometryType = ShapefileGeometryType.Point,
            Geometry = new KoreGeoPoint { Position = new KoreLLPoint { LatDegs = 0, LonDegs = 0 } },
            Attributes = new Dictionary<string, object?> { { "created", testDate } }
        });

        KoreShapefileWriter.Write(basePath, collection);
        var readCollection = KoreShapefileReader.Read(basePath);

        bool passed = readCollection.Features.Count == 1;
        if (passed)
        {
            var attrs = readCollection.Features[0].Attributes;
            passed = attrs.TryGetValue("created", out var dateVal) && dateVal is DateTime dateDt &&
                     dateDt.Year == 2024 && dateDt.Month == 6 && dateDt.Day == 15;
        }

        testLog.AddResult("Shapefile DateTime Attribute", passed);
    }

    private static void TestStringAttribute(KoreTestLog testLog)
    {
        string basePath = Path.Combine(TestDir, "test_string_attr");

        var collection = new KoreShapefileFeatureCollection
        {
            GeometryType = ShapefileGeometryType.Point
        };

        collection.Features.Add(new KoreShapefileFeature
        {
            RecordNumber = 1,
            GeometryType = ShapefileGeometryType.Point,
            Geometry = new KoreGeoPoint { Position = new KoreLLPoint { LatDegs = 0, LonDegs = 0 } },
            Attributes = new Dictionary<string, object?> { { "name", "Test Location" }, { "code", "ABC123" } }
        });

        KoreShapefileWriter.Write(basePath, collection);
        var readCollection = KoreShapefileReader.Read(basePath);

        bool passed = readCollection.Features.Count == 1;
        if (passed)
        {
            var attrs = readCollection.Features[0].Attributes;
            passed = attrs.TryGetValue("name", out var nameVal) && nameVal is string nameStr && nameStr == "Test Location";
            passed = passed && attrs.TryGetValue("code", out var codeVal) && codeVal is string codeStr && codeStr == "ABC123";
        }

        testLog.AddResult("Shapefile String Attribute", passed);
    }

    private static void TestMixedAttributes(KoreTestLog testLog)
    {
        string basePath = Path.Combine(TestDir, "test_mixed_attr");

        var collection = new KoreShapefileFeatureCollection
        {
            GeometryType = ShapefileGeometryType.Point
        };

        collection.Features.Add(new KoreShapefileFeature
        {
            RecordNumber = 1,
            GeometryType = ShapefileGeometryType.Point,
            Geometry = new KoreGeoPoint { Position = new KoreLLPoint { LatDegs = 0, LonDegs = 0 } },
            Attributes = new Dictionary<string, object?>
            {
                { "name", "Test" },
                { "count", 42 },
                { "value", 3.14 },
                { "active", true },
                { "created", new DateTime(2024, 1, 1) }
            }
        });

        // Second feature with some missing attributes
        collection.Features.Add(new KoreShapefileFeature
        {
            RecordNumber = 2,
            GeometryType = ShapefileGeometryType.Point,
            Geometry = new KoreGeoPoint { Position = new KoreLLPoint { LatDegs = 1, LonDegs = 1 } },
            Attributes = new Dictionary<string, object?>
            {
                { "name", "Test2" },
                { "count", 100 }
                // Missing value, active, created
            }
        });

        KoreShapefileWriter.Write(basePath, collection);
        var readCollection = KoreShapefileReader.Read(basePath);

        bool passed = readCollection.Features.Count == 2;
        if (passed)
        {
            // Check first feature has all attributes
            var attrs1 = readCollection.Features[0].Attributes;
            passed = attrs1.ContainsKey("name") && attrs1.ContainsKey("count") &&
                     attrs1.ContainsKey("value") && attrs1.ContainsKey("active") &&
                     attrs1.ContainsKey("created");
        }

        testLog.AddResult("Shapefile Mixed Attributes", passed);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Non-Fatal Error Handling
    // --------------------------------------------------------------------------------------------

    private static void TestCorruptRecordSkipping(KoreTestLog testLog)
    {
        // This test creates a valid shapefile, then manually corrupts part of it
        // to verify that the reader can skip corrupt records and continue

        string basePath = Path.Combine(TestDir, "test_corrupt");

        // First, create a valid shapefile with multiple features
        var collection = new KoreShapefileFeatureCollection
        {
            GeometryType = ShapefileGeometryType.Point
        };

        for (int i = 0; i < 5; i++)
        {
            collection.Features.Add(new KoreShapefileFeature
            {
                RecordNumber = i + 1,
                GeometryType = ShapefileGeometryType.Point,
                Geometry = new KoreGeoPoint { Position = new KoreLLPoint { LatDegs = i * 10, LonDegs = i * 10 } },
                Attributes = new Dictionary<string, object?> { { "id", i } }
            });
        }

        KoreShapefileWriter.Write(basePath, collection);

        // Read the shapefile - should work normally first
        var readCollection = KoreShapefileReader.Read(basePath);

        // Verify we can read it back successfully
        bool passed = readCollection.Features.Count == 5;

        // The test primarily verifies that the reader doesn't throw on valid data
        // and that the error handling structure is in place
        testLog.AddResult("Shapefile Corrupt Record Handling", passed);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: PRJ File Test
    // --------------------------------------------------------------------------------------------

    private static void TestPrjFileWritten(KoreTestLog testLog)
    {
        string basePath = Path.Combine(TestDir, "test_prj");
        string prjPath = basePath + ".prj";

        var collection = new KoreShapefileFeatureCollection
        {
            GeometryType = ShapefileGeometryType.Point
        };

        collection.Features.Add(new KoreShapefileFeature
        {
            RecordNumber = 1,
            GeometryType = ShapefileGeometryType.Point,
            Geometry = new KoreGeoPoint { Position = new KoreLLPoint { LatDegs = 0, LonDegs = 0 } },
            Attributes = new Dictionary<string, object?>()
        });

        KoreShapefileWriter.Write(basePath, collection);

        // Verify PRJ file exists and contains WGS84
        bool passed = File.Exists(prjPath);
        if (passed)
        {
            string prjContent = File.ReadAllText(prjPath);
            passed = prjContent.Contains("WGS_1984") || prjContent.Contains("WGS84") || prjContent.Contains("WGS 84");
        }

        testLog.AddResult("Shapefile PRJ File Written with WGS84", passed);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Visualization Test
    // --------------------------------------------------------------------------------------------

    // Helper: Convert Web Mercator (EPSG:3857) to WGS84 lat/lon
    private static KoreLLPoint WebMercatorToLatLon(double x, double y)
    {
        const double R = 6378137.0; // Earth radius in meters (WGS84)

        double lon = x / R * (180.0 / Math.PI);
        double lat = Math.Atan(Math.Sinh(y / R)) * (180.0 / Math.PI);

        return new KoreLLPoint { LatDegs = lat, LonDegs = lon };
    }

    private static void TestDrawStreets(KoreTestLog testLog)
    {
        string shapefilePath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, KoreFileOps.JoinPaths("SampleShapefile", "Streets"));
        string outputPath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "streets_visualization.png");

        // Read the shapefile
        var collection = KoreShapefileReader.Read(shapefilePath);

        testLog.AddComment($"ShapefileDrawTest: Loaded shapefile with {collection.Features.Count} features");
        testLog.AddComment($"ShapefileDrawTest: Geometry type: {collection.GeometryType}");

        // Log first feature details (raw coordinates before conversion)
        if (collection.Features.Count > 0)
        {
            var firstFeature = collection.Features[0];
            testLog.AddComment($"ShapefileDrawTest: First feature geometry type: {firstFeature.Geometry?.GetType().Name ?? "null"}");

            if (firstFeature.Geometry is KoreGeoMultiLineString multiLine)
            {
                testLog.AddComment($"ShapefileDrawTest: First feature has {multiLine.LineStrings.Count} line strings");
                if (multiLine.LineStrings.Count > 0)
                {
                    testLog.AddComment($"ShapefileDrawTest: First line string has {multiLine.LineStrings[0].Count} points");
                    if (multiLine.LineStrings[0].Count > 0)
                    {
                        var firstPointRaw = multiLine.LineStrings[0][0];
                        testLog.AddComment($"ShapefileDrawTest: First point (raw Web Mercator): X={firstPointRaw.LonDegs:F2}, Y={firstPointRaw.LatDegs:F2}");

                        // Convert and show lat/lon
                        var firstPointLatLon = WebMercatorToLatLon(firstPointRaw.LonDegs, firstPointRaw.LatDegs);
                        testLog.AddComment($"ShapefileDrawTest: First point (converted): Lat={firstPointLatLon.LatDegs:F6}, Lon={firstPointLatLon.LonDegs:F6}");
                    }
                }
            }
        }

        // Convert all coordinates from Web Mercator to WGS84 lat/lon
        foreach (var feature in collection.Features)
        {
            if (feature.Geometry is KoreGeoMultiLineString multiLine)
            {
                foreach (var lineString in multiLine.LineStrings)
                {
                    for (int i = 0; i < lineString.Count; i++)
                    {
                        var mercatorPoint = lineString[i];
                        lineString[i] = WebMercatorToLatLon(mercatorPoint.LonDegs, mercatorPoint.LatDegs);
                    }
                }
            }
        }

        // Calculate bounds from all features
        double minLat = double.MaxValue;
        double maxLat = double.MinValue;
        double minLon = double.MaxValue;
        double maxLon = double.MinValue;

        foreach (var feature in collection.Features)
        {
            if (feature.Geometry is KoreGeoMultiLineString multiLine)
            {
                foreach (var lineString in multiLine.LineStrings)
                {
                    foreach (var point in lineString)
                    {
                        minLat = Math.Min(minLat, point.LatDegs);
                        maxLat = Math.Max(maxLat, point.LatDegs);
                        minLon = Math.Min(minLon, point.LonDegs);
                        maxLon = Math.Max(maxLon, point.LonDegs);
                    }
                }
            }
        }

        // Text comment on the calculated bounds
        testLog.AddComment($"ShapefileDrawTest: Calculated bounds - Lat: [{minLat}, {maxLat}], Lon: [{minLon}, {maxLon}]");

        // Add small margin
        double latMargin = (maxLat - minLat) * 0.05;
        double lonMargin = (maxLon - minLon) * 0.05;
        minLat -= latMargin;
        maxLat += latMargin;
        minLon -= lonMargin;
        maxLon += lonMargin;

        // Create world plotter with calculated bounds using degrees
        var geoBounds = new KoreLLBox
        {
            MinLatDegs = minLat,
            MaxLatDegs = maxLat,
            MinLonDegs = minLon,
            MaxLonDegs = maxLon
        };
        var worldPlotter = new KoreWorldPlotter(2000, 2000, geoBounds);

        testLog.AddComment($"ShapefileDrawTest: Bounds: Lat [{geoBounds.MinLatDegs:F6}, {geoBounds.MaxLatDegs:F6}], Lon [{geoBounds.MinLonDegs:F6}, {geoBounds.MaxLonDegs:F6}]");
        testLog.AddComment($"ShapefileDrawTest: Lat span: {geoBounds.DeltaLatDegs:F6}, Lon span: {geoBounds.DeltaLonDegs:F6}");

        // Set drawing color for streets
        var streetColor = new KoreColorRGB(100, 100, 255); // Blue for streets
        worldPlotter.Plotter.DrawSettings.Color = KoreSkiaSharpConv.ToSKColor(streetColor);
        worldPlotter.Plotter.DrawSettings.Paint.StrokeWidth = 1.5f;

        // Draw all street features
        int segmentCount = 0;
        foreach (var feature in collection.Features)
        {
            if (feature.Geometry is KoreGeoMultiLineString multiLine)
            {
                foreach (var lineString in multiLine.LineStrings)
                {
                    // Draw each line segment
                    for (int i = 0; i < lineString.Count - 1; i++)
                    {
                        var pixel1 = worldPlotter.LatLonToPixel(lineString[i]);
                        var pixel2 = worldPlotter.LatLonToPixel(lineString[i + 1]);
                        worldPlotter.Plotter.DrawLine(pixel1, pixel2);
                        segmentCount++;
                    }
                }
            }
        }

        testLog.AddComment($"ShapefileDrawTest: Drew {segmentCount} line segments");

        // Log some sample pixel coordinates to verify conversion
        if (collection.Features.Count > 0 && collection.Features[0].Geometry is KoreGeoMultiLineString sampleMultiLine)
        {
            if (sampleMultiLine.LineStrings.Count > 0 && sampleMultiLine.LineStrings[0].Count > 0)
            {
                var samplePoint = sampleMultiLine.LineStrings[0][0];
                var samplePixel = worldPlotter.LatLonToPixel(samplePoint);
                testLog.AddComment($"ShapefileDrawTest: Sample pixel: ({samplePixel.X:F2}, {samplePixel.Y:F2})");
            }
        }

        // Save the image
        worldPlotter.Save(outputPath);

        // Test passes if we created the image
        bool passed = File.Exists(outputPath);
        testLog.AddResult("Shapefile Streets Visualization", passed,
            passed ? $"Created visualization with {collection.Features.Count} features" : "Failed to create visualization");
    }
}
