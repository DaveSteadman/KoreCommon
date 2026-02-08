using System.Collections;
using SkiaSharp;

namespace KorePlotter.NatoSymbol;

// KoreNatoSymbolDrawOps: Static methods to draw specific shapes for NATO symbols.
// - Functions passed the canvas and the necessary parameters to draw the shape.
// - Static class for utility functions, holds no state.

public static partial class KoreNatoSymbolDrawOps
{
    // Usage: KoreNatoSymbolDrawOps.DrawIcon(canvas, NatoPlatformFunction.Military);
    public static void DrawIcon(KoreNatoSymbolCanvas canvas, NatoPlatformFunction icon)
    {
        SKRect bounds = IconBoundsRect(canvas);
        switch (icon)
        {
            case NatoPlatformFunction.Military:                     DrawTextIcon(canvas, bounds, NatoPlatformFunction.Military);                     break;
            case NatoPlatformFunction.Civilian:                     DrawTextIcon(canvas, bounds, NatoPlatformFunction.Civilian);                     break;
            case NatoPlatformFunction.Attackstrike:                 DrawTextIcon(canvas, bounds, NatoPlatformFunction.Attackstrike);                 break;
            case NatoPlatformFunction.Bomber:                       DrawTextIcon(canvas, bounds, NatoPlatformFunction.Bomber);                       break;
            case NatoPlatformFunction.Cargo:                        DrawTextIcon(canvas, bounds, NatoPlatformFunction.Cargo);                        break;
            case NatoPlatformFunction.Fighter:                      DrawTextIcon(canvas, bounds, NatoPlatformFunction.Fighter);                      break;
            case NatoPlatformFunction.JammerEcm:                    DrawTextIcon(canvas, bounds, NatoPlatformFunction.JammerEcm);                    break;
            case NatoPlatformFunction.Tanker:                       DrawTextIcon(canvas, bounds, NatoPlatformFunction.Tanker);                       break;
            case NatoPlatformFunction.Patrol:                       DrawTextIcon(canvas, bounds, NatoPlatformFunction.Patrol);                       break;
            case NatoPlatformFunction.Reconnaissance:               DrawTextIcon(canvas, bounds, NatoPlatformFunction.Reconnaissance);               break;
            case NatoPlatformFunction.Trainer:                      DrawTextIcon(canvas, bounds, NatoPlatformFunction.Trainer);                      break;
            case NatoPlatformFunction.Utility:                      DrawTextIcon(canvas, bounds, NatoPlatformFunction.Utility);                      break;
            case NatoPlatformFunction.VSTOL:                        DrawTextIcon(canvas, bounds, NatoPlatformFunction.VSTOL);                        break;
            case NatoPlatformFunction.AirborneCommandPost:          DrawTextIcon(canvas, bounds, NatoPlatformFunction.AirborneCommandPost);          break;
            case NatoPlatformFunction.AirborneEarlyWarning:         DrawTextIcon(canvas, bounds, NatoPlatformFunction.AirborneEarlyWarning);         break;
            case NatoPlatformFunction.AntisurfaceWarfare:           DrawTextIcon(canvas, bounds, NatoPlatformFunction.AntisurfaceWarfare);           break;
            case NatoPlatformFunction.AntisubmarineWarfare:         DrawTextIcon(canvas, bounds, NatoPlatformFunction.AntisubmarineWarfare);         break;
            case NatoPlatformFunction.Communications:               DrawTextIcon(canvas, bounds, NatoPlatformFunction.Communications);               break;
            case NatoPlatformFunction.CombatSearchAndRescue:        DrawTextIcon(canvas, bounds, NatoPlatformFunction.CombatSearchAndRescue);        break;
            case NatoPlatformFunction.ElectronicSupportMeasures:    DrawTextIcon(canvas, bounds, NatoPlatformFunction.ElectronicSupportMeasures);    break;
            case NatoPlatformFunction.Government:                   DrawTextIcon(canvas, bounds, NatoPlatformFunction.Government);                   break;
            case NatoPlatformFunction.MineCountermeasures:          DrawTextIcon(canvas, bounds, NatoPlatformFunction.MineCountermeasures);          break;
            case NatoPlatformFunction.PersonnelRecovery:            DrawTextIcon(canvas, bounds, NatoPlatformFunction.PersonnelRecovery);            break;
            case NatoPlatformFunction.Passenger:                    DrawTextIcon(canvas, bounds, NatoPlatformFunction.Passenger);                    break;
            case NatoPlatformFunction.SearchAndRescue:              DrawTextIcon(canvas, bounds, NatoPlatformFunction.SearchAndRescue);              break;
            case NatoPlatformFunction.SupressionOfEnemyAirDefence:  DrawTextIcon(canvas, bounds, NatoPlatformFunction.SupressionOfEnemyAirDefence);  break;
            case NatoPlatformFunction.SpecialOperationsForces:      DrawTextIcon(canvas, bounds, NatoPlatformFunction.SpecialOperationsForces);      break;
            case NatoPlatformFunction.UltraLight:                   DrawTextIcon(canvas, bounds, NatoPlatformFunction.UltraLight);                   break;
            case NatoPlatformFunction.Vip:                          DrawTextIcon(canvas, bounds, NatoPlatformFunction.Vip);                          break;
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: RECT
    // --------------------------------------------------------------------------------------------

    public static SKRect IconBoundsRect(KoreNatoSymbolCanvas canvas)
    {
        float padding = canvas.LDistance * 0.01f; // 10% padding
        return new SKRect(
            canvas.Center.X - (canvas.LDistance * 0.5f) + padding,
            canvas.Center.Y - (canvas.LDistance * 0.2f) + padding,
            canvas.Center.X + (canvas.LDistance * 0.5f) - padding,
            canvas.Center.Y + (canvas.LDistance * 0.2f) - padding
        );
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Icon Draw Ops
    // --------------------------------------------------------------------------------------------

    public static void DrawTextIcon(KoreNatoSymbolCanvas canvas, SKRect bounds, NatoPlatformFunction icon)
    {
        string outText = icon switch
        {
            NatoPlatformFunction.Military => "MIL",
            NatoPlatformFunction.Civilian => "CIV",
            NatoPlatformFunction.Attackstrike => "A",
            NatoPlatformFunction.Bomber => "B",
            NatoPlatformFunction.Cargo => "C",
            NatoPlatformFunction.Fighter => "F",
            NatoPlatformFunction.JammerEcm => "J",
            NatoPlatformFunction.Tanker => "K",
            NatoPlatformFunction.Patrol => "P",
            NatoPlatformFunction.Reconnaissance => "R",
            NatoPlatformFunction.Trainer => "T",
            NatoPlatformFunction.Utility => "U",
            NatoPlatformFunction.VSTOL => "V",
            NatoPlatformFunction.AirborneCommandPost => "ACP",
            NatoPlatformFunction.AirborneEarlyWarning => "AEW",
            NatoPlatformFunction.AntisurfaceWarfare => "ASUW",
            NatoPlatformFunction.AntisubmarineWarfare => "ASW",
            NatoPlatformFunction.Communications => "COM",
            NatoPlatformFunction.CombatSearchAndRescue => "CSAR",
            NatoPlatformFunction.ElectronicSupportMeasures => "ESM",
            NatoPlatformFunction.Government => "GOV",
            NatoPlatformFunction.MineCountermeasures => "MCM",
            NatoPlatformFunction.PersonnelRecovery => "PR",
            NatoPlatformFunction.Passenger => "PX",
            NatoPlatformFunction.SearchAndRescue => "SAR",
            NatoPlatformFunction.SupressionOfEnemyAirDefence => "SEAD",
            NatoPlatformFunction.SpecialOperationsForces => "SOF",
            NatoPlatformFunction.UltraLight => "UL",
            NatoPlatformFunction.Vip => "VIP",
            _ => "UNK"
        };

        // Start with a large font size
        float testFontSize = 100f;
        using var testFont = new SKFont(SKTypeface.Default, testFontSize);

        // Measure the text at the test size
        float textWidth = testFont.MeasureText(outText);
        var fontMetrics = testFont.Metrics;
        float textHeight = fontMetrics.Descent - fontMetrics.Ascent;

        // Calculate scaling factors to fit within bounds (with some padding)
        float paddingFactor = 0.9f; // Use 90% of available space
        float scaleX = (bounds.Width * paddingFactor) / textWidth;
        float scaleY = (bounds.Height * paddingFactor) / textHeight;

        // Use the smaller scale to ensure text fits both dimensions
        float scale = Math.Min(scaleX, scaleY);
        float finalFontSize = testFontSize * scale;

        // Create the final font and paint
        using var font = new SKFont(SKTypeface.Default, finalFontSize);
        using var textPaint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true
        };

        // Measure text with final font to center it
        textWidth = font.MeasureText(outText);
        fontMetrics = font.Metrics;
        textHeight = fontMetrics.Descent - fontMetrics.Ascent;

        // Calculate centered position
        float centeredX = bounds.MidX - (textWidth / 2);
        float centeredY = bounds.MidY - (textHeight / 2) - fontMetrics.Ascent;

        // Draw the text
        canvas.Canvas.DrawText(outText, centeredX, centeredY, font, textPaint);
    }

}