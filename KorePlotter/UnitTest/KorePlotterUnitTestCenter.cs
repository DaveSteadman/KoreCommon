
// A static class to manage unit tests for the KoreSim namespace.


using KoreCommon;
using KoreCommon.UnitTest;

namespace KorePlotter.UnitTest;


public static class KorePlotterUnitTestCenter
{
    // Usage: KorePlotterUnitTestCenter.RunAllTests(testLog);
    public static void RunAllTests(KoreTestLog testLog)
    {
        // Run individual test methods
        KoreTestNatoSymbolPlotter.RunTests(testLog);


        // TestKoreElevationManager(testLog);
        // TestKoreElevationTileIO(testLog);
        // Add more test methods as needed
    }
}