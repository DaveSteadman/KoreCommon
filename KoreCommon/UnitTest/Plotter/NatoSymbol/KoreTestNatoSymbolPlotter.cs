// <fileheader>

using System;
using System.IO;
using System.Collections.Generic;

using KoreCommon.Plotter.NatoSymbolGen;

namespace KoreCommon.UnitTest;

public static class KoreTestNatoSymbolPlotter
{
    public static void RunTests(KoreTestLog testLog)
    {
        TestBasicImage(testLog);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: World
    // --------------------------------------------------------------------------------------------

    private static void TestBasicImage(KoreTestLog testLog)
    {
        try
        {
            // Create the canvas, which sets up the layout
            var canvas = new KoreNatoSymbolCanvas(800f);

            var octagonElement = new KoreNatoSymbolElementOctagon();
            octagonElement.DefinePoints(canvas.Layout);
            octagonElement.Render(canvas.Canvas);

            // var octagonBoundsElement = new NatoSymbolElementRect();
            // octagonBoundsElement.CalcPoints(
            //     canvas.Layout.Center,
            //     canvas.Layout.OctagonBounds.Width,
            //     canvas.Layout.OctagonBounds.Height
            // );
            // octagonBoundsElement.Render(canvas.Canvas);

            // var diamondElement = new NatoSymbolElementDiamond();
            // diamondElement.AssignPoints(canvas.Layout.DiamondPoints);
            // diamondElement.Render(canvas.Canvas);

            // Check output directory
            string artefactsDir = KoreTestCenter.TestPath;
            Directory.CreateDirectory(artefactsDir);

            // Save the file
            canvas.SaveToPng(KoreFileOps.JoinPaths(artefactsDir, "octagon_layout_test.png"));
            Console.WriteLine("   📁 Saved: GeneratedSymbols/octagon_layout_test.png");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error generating symbols: {ex.Message}");
        }
    }

}
