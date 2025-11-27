// <fileheader>

using System;
using System.IO;


using KoreCommon;
namespace KoreCommon.UnitTest;



public static class KoreTestDatabase
{
    // KoreTestDatabase.RunTests(testLog)
    public static void RunTests(KoreTestLog testLog)
    {
        try
        {
            TestDatabaseReadWrite(testLog);
            TestDatabaseMesh(testLog);
        }
        catch (Exception ex)
        {
            testLog.AddResult("KoreTestDatabase.RunTests", false, $"Exception: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private static void TestDatabaseReadWrite(KoreTestLog testLog)
    {
        string dbPath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "test_db.sqlite");
        if (File.Exists(dbPath))
            File.Delete(dbPath);

        // Create a new database
        var db = new KoreBinaryDataManager(dbPath);
        testLog.AddResult("Database Created", File.Exists(dbPath));

        // Prepare test values
        int    testInt    = 123456;
        float  testFloat  = 3.14159f;
        double testDouble = 2.718281828459;
        bool   testBool   = true;
        string testString = "Hello, world!";
        byte[] testBytes  = new byte[] { 1, 2, 3, 4, 5 };

        // Write values
        var writer = new KoreByteArrayWriter();
        writer.WriteInt(testInt);
        writer.WriteFloat(testFloat);
        writer.WriteDouble(testDouble);
        writer.WriteBool(testBool);
        writer.WriteString(testString);
        writer.WriteBytes(testBytes);
        byte[] data = writer.ToArray();

        bool addResult = db.Set("test", data);
        testLog.AddResult("KoreBinaryDataManager Add()", addResult);

        // Read back
        byte[] readData   = db.Get("test");
        var    reader     = new KoreByteArrayReader(readData);
        int    readInt    = reader.ReadInt();
        float  readFloat  = reader.ReadFloat();
        double readDouble = reader.ReadDouble();
        bool   readBool   = reader.ReadBool();
        string readString = reader.ReadString();
        byte[] readBytes  = reader.ReadBytes(testBytes.Length);

        testLog.AddResult("KoreBinaryDataManager Int Read/Write", readInt == testInt);
        testLog.AddResult("KoreBinaryDataManager Float Read/Write", Math.Abs(readFloat - testFloat) < 1e-5);
        testLog.AddResult("KoreBinaryDataManager Double Read/Write", Math.Abs(readDouble - testDouble) < 1e-10);
        testLog.AddResult("KoreBinaryDataManager Bool Read/Write", readBool == testBool);
        testLog.AddResult("KoreBinaryDataManager String Read/Write", readString == testString);
        testLog.AddResult("KoreBinaryDataManager Bytes Read/Write", readBytes.Length == testBytes.Length && System.Linq.Enumerable.SequenceEqual(readBytes, testBytes));
    }

    private static void TestDatabaseMesh(KoreTestLog testLog)
    {
        string dbPath = "test_db_mesh.sqlite";
        if (File.Exists(dbPath))
            File.Delete(dbPath);

        // Create a basic cube mesh
        var mesh = KoreMeshDataPrimitives.BasicCube(1.0f, KoreMeshMaterialPalette.DefaultMaterial);

        // Serialise to bytes
        byte[] meshBytes = KoreMeshDataIO.ToBytes(mesh);

        // Write to DB
        // var db = new KoreBinaryDataManager(dbPath);
        // bool addResult = db.Add("cube", meshBytes);
        // testLog.AddResult("KoreBinaryDataManager Mesh Add()", addResult);

        // // Read back
        // byte[] readBytes = db.Get("cube");
        var mesh2 = KoreMeshDataIO.FromBytes(meshBytes);

        // Check vertex and triangle counts
        bool vertsMatch = mesh2.Vertices.Count == mesh.Vertices.Count;
        bool trisMatch  = mesh2.Triangles.Count == mesh.Triangles.Count;

        testLog.AddComment($"Mesh Buffer Size: {meshBytes.Length} bytes");

        testLog.AddResult($"KoreBinaryDataManager Mesh Vertices Count ({mesh.Vertices.Count} -> {mesh2.Vertices.Count})", vertsMatch);
        testLog.AddResult($"KoreBinaryDataManager Mesh Triangles Count ({mesh.Triangles.Count} -> {mesh2.Triangles.Count})", trisMatch);
    }
}
