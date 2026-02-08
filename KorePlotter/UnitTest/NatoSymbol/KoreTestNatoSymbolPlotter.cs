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

            float imageUnit = 250f;
            float halfUnit = imageUnit / 2f;

            // Pick a random icon for testing
            var allIcons = Enum.GetValues(typeof(NatoPlatformFunction));
            var randomIcon = (NatoPlatformFunction)allIcons.GetValue(new Random().Next(allIcons.Length))!;

            KoreSkiaSharpPlotter unitTestCanvas = new((int)imageUnit * 10, (int)imageUnit * 10); // 1000x1000 pixels

            // - - - - -

            float xPos = imageUnit;
            float yPos = 0f;

            foreach (NatoSymbolDomain domain in Enum.GetValues(typeof(NatoSymbolDomain)))
            {
                // Get domain name for header
                string domainName = NatoSymbolUtils.NatoSymbolDomainToString(domain);
                unitTestCanvas.DrawTextCentered(domainName, new KoreXYVector(xPos + halfUnit, yPos + halfUnit));
                xPos += imageUnit;
            }

            // - - - - -

            xPos = 0f;
            yPos += imageUnit;


            unitTestCanvas.DrawTextCentered("Unknown", new KoreXYVector(halfUnit, yPos + halfUnit));
            xPos += imageUnit;

            foreach (NatoSymbolDomain domain in Enum.GetValues(typeof(NatoSymbolDomain)))
            {
                // Create the canvas, which sets up the layout
                var symbolCanvas = new KoreNatoSymbolCanvas(imageUnit);
                KoreNatoSymbolDrawOps.DrawUnknown(symbolCanvas, domain);
                KoreNatoSymbolDrawOps.DrawOctagon(symbolCanvas, DrawMode.Stroke);

                randomIcon = (NatoPlatformFunction)allIcons.GetValue(new Random().Next(allIcons.Length))!;

                KoreNatoSymbolDrawOps.DrawIcon(symbolCanvas, randomIcon);

                unitTestCanvas.PasteBitmap(symbolCanvas.ToBitmap(), xPos, yPos);

                symbolCanvas.Clear();
                xPos += imageUnit;
            }

            // - - - - -

            xPos = 0f;
            yPos += imageUnit;

            unitTestCanvas.DrawTextCentered("Neutral", new KoreXYVector(halfUnit, yPos + halfUnit));
            xPos += imageUnit;

            foreach (NatoSymbolDomain domain in Enum.GetValues(typeof(NatoSymbolDomain)))
            {
                // Create the canvas, which sets up the layout
                var symbolCanvas = new KoreNatoSymbolCanvas(imageUnit);
                KoreNatoSymbolDrawOps.DrawNeutral(symbolCanvas, domain);
                KoreNatoSymbolDrawOps.DrawOctagon(symbolCanvas, DrawMode.Stroke);

                unitTestCanvas.PasteBitmap(symbolCanvas.ToBitmap(), xPos, yPos);

                symbolCanvas.Clear();
                xPos += imageUnit;
            }

            // - - - - -

            xPos = 0f;
            yPos += imageUnit;


            unitTestCanvas.DrawTextCentered("Friendly", new KoreXYVector(halfUnit, yPos + halfUnit));
            xPos += imageUnit;



            foreach (NatoSymbolDomain domain in Enum.GetValues(typeof(NatoSymbolDomain)))
            {
                // Create the canvas, which sets up the layout
                var symbolCanvas = new KoreNatoSymbolCanvas(imageUnit);
                KoreNatoSymbolDrawOps.DrawFriend(symbolCanvas, domain);
                KoreNatoSymbolDrawOps.DrawOctagon(symbolCanvas, DrawMode.Stroke);
                unitTestCanvas.PasteBitmap(symbolCanvas.ToBitmap(), xPos, yPos);

                symbolCanvas.Clear();
                xPos += imageUnit;
            }


            // - - - - -

            xPos = 0f;
            yPos += imageUnit;


            unitTestCanvas.DrawTextCentered("Hostile", new KoreXYVector(halfUnit, yPos + halfUnit));
            xPos += imageUnit;

            foreach (NatoSymbolDomain domain in Enum.GetValues(typeof(NatoSymbolDomain)))
            {
                // Create the canvas, which sets up the layout
                var symbolCanvas = new KoreNatoSymbolCanvas(imageUnit);
                KoreNatoSymbolDrawOps.DrawHostile(symbolCanvas, domain);
                KoreNatoSymbolDrawOps.DrawOctagon(symbolCanvas, DrawMode.Stroke);
                unitTestCanvas.PasteBitmap(symbolCanvas.ToBitmap(), xPos, yPos);

                symbolCanvas.Clear();
                xPos += imageUnit;
            }


            // - - - - -

            xPos = 0f;
            yPos += imageUnit;


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
