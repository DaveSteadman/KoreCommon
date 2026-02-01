// // <fileheader>

// using System;

// // --- Environment Creation: ---
// // dotnet new console -n UnitTest
// // dotnet add package Microsoft.Data.Sqlite
// // dotnet add package SkiaSharp
// // dotnet new console

// // --- Environment update: ---
// // dotnet workload list
// // dotnet list package --outdated
// // dotnet workload update

// // dotnet remove package System.Data.SQLite

// using KoreCommon;
// using KoreCommon.UnitTest;
// using KoreGIS.UnitTest;

// class Program
// {
//     static void Main()
//     {
//         // Uncomment to run the bulk rename (use with caution!)
//         // RenameKoreToKoreInFiles(".");
//         KoreTestLog testLog = new KoreTestLog();
//         KoreTestCenter.RunTests(testLog);
//         // KoreGISTestCenter.RunTests(testLog);

//         // Adhoc tests - a function designed to be reworked to consider issues-of-the-day
//         //KoreTestLog testres = KoreTestCenter.RunAdHocTests();

//         // Get the test reports
//         // Add default statements if no tests passed or failed
//         string fullReport = testLog.FullReport();
//         string failReport = testLog.FailReport();

//         string separator = new string('-', 80);
//         Console.WriteLine(separator);
//         if (testLog.OverallPass())
//             Console.WriteLine(fullReport);
//         else
//             Console.WriteLine(failReport);
//         Console.WriteLine(separator);
//         Console.WriteLine(testLog.OneLineReport());
//         Console.WriteLine(separator);
//     }
// }
