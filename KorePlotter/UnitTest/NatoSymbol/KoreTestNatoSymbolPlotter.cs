// <fileheader>

using System;
using System.IO;
using System.Collections.Generic;

using SkiaSharp;

using KoreCommon;
using KoreCommon.UnitTest;
using KorePlotter.NatoSymbol;
using KoreCommon.SkiaSharp;

namespace KorePlotter.UnitTest;

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
            // Unit test canvas - a grid of 100x100 pixel squares, into which we are going to
            // draw a grid of each symbol and domain, with labels.

            KoreSkiaSharpPlotter unitTestCanvas = new(1000, 1000); // 1000x1000 pixels

            // - - - - -

            float xPos = 100f;
            float yPos = 0f;

            foreach (NatoSymbolDomain domain in Enum.GetValues(typeof(NatoSymbolDomain)))
            {
                // Get domain name for header
                string domainName = NatoSymbolUtils.NatoSymbolDomainToString(domain);
                unitTestCanvas.DrawTextCentered(domainName, new KoreXYVector(xPos + 50f, yPos + 50f));
                xPos += 100f;
            }

            // - - - - -

            xPos = 0f;
            yPos += 100f;


            unitTestCanvas.DrawTextCentered("Unknown", new KoreXYVector(50f, yPos + 50f));
            xPos += 100f;

            foreach (NatoSymbolDomain domain in Enum.GetValues(typeof(NatoSymbolDomain)))
            {
                // Create the canvas, which sets up the layout
                var symbolCanvas = new KoreNatoSymbolCanvas(100f);
                KoreNatoSymbolDrawOps.DrawUnknown(symbolCanvas, domain);
                KoreNatoSymbolDrawOps.DrawOctagon(symbolCanvas, DrawMode.Stroke);

                unitTestCanvas.PasteBitmap(symbolCanvas.ToBitmap(), xPos, yPos);

                symbolCanvas.Clear();
                xPos += 100f;
            }

            // - - - - -

            xPos = 0f;
            yPos += 100f;

            unitTestCanvas.DrawTextCentered("Neutral", new KoreXYVector(50f, yPos + 50f));
            xPos += 100f;

            foreach (NatoSymbolDomain domain in Enum.GetValues(typeof(NatoSymbolDomain)))
            {
                // Create the canvas, which sets up the layout
                var symbolCanvas = new KoreNatoSymbolCanvas(100f);
                KoreNatoSymbolDrawOps.DrawNeutral(symbolCanvas, domain);
                KoreNatoSymbolDrawOps.DrawOctagon(symbolCanvas, DrawMode.Stroke);

                unitTestCanvas.PasteBitmap(symbolCanvas.ToBitmap(), xPos, yPos);

                symbolCanvas.Clear();
                xPos += 100f;
            }

            // - - - - -

            xPos = 0f;
            yPos += 100f;


            unitTestCanvas.DrawTextCentered("Friendly", new KoreXYVector(50f, yPos + 50f));
            xPos += 100f;



            foreach (NatoSymbolDomain domain in Enum.GetValues(typeof(NatoSymbolDomain)))
            {
                // Create the canvas, which sets up the layout
                var symbolCanvas = new KoreNatoSymbolCanvas(100f);
                KoreNatoSymbolDrawOps.DrawFriend(symbolCanvas, domain);
                KoreNatoSymbolDrawOps.DrawOctagon(symbolCanvas, DrawMode.Stroke);
                unitTestCanvas.PasteBitmap(symbolCanvas.ToBitmap(), xPos, yPos);

                symbolCanvas.Clear();
                xPos += 100f;
            }


            // - - - - -

            xPos = 0f;
            yPos += 100f;


            unitTestCanvas.DrawTextCentered("Hostile", new KoreXYVector(50f, yPos + 50f));
            xPos += 100f;

            foreach (NatoSymbolDomain domain in Enum.GetValues(typeof(NatoSymbolDomain)))
            {
                // Create the canvas, which sets up the layout
                var symbolCanvas = new KoreNatoSymbolCanvas(100f);
                KoreNatoSymbolDrawOps.DrawHostile(symbolCanvas, domain);
                KoreNatoSymbolDrawOps.DrawOctagon(symbolCanvas, DrawMode.Stroke);
                unitTestCanvas.PasteBitmap(symbolCanvas.ToBitmap(), xPos, yPos);

                symbolCanvas.Clear();
                xPos += 100f;
            }


            // - - - - -

            xPos = 0f;
            yPos += 100f;


            // Check output directory
            string artefactsDir = KoreTestCenter.TestPath;
            Directory.CreateDirectory(artefactsDir);

            // Save the file
            unitTestCanvas.Save(KoreFileOps.JoinPaths(artefactsDir, "octagon_layout_test.png"));
            Console.WriteLine("   📁 Saved: GeneratedSymbols/octagon_layout_test.png");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error generating symbols: {ex.Message}");
        }
    }

}
