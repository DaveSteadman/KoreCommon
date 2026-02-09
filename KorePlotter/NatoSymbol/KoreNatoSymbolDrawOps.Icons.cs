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
            case NatoPlatformFunction.MilitaryFixedWing:
            {
                using SKPath bowtiePath = BuildBowtieIconPath(bounds, true);
                using SKPaint bowtiePaint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = KoreNatoSymbolColorPalette.BorderColor,
                    IsAntialias = true
                };
                canvas.Canvas.DrawPath(bowtiePath, bowtiePaint);
                break;
            }

            case NatoPlatformFunction.CivilianFixedWing:
            {
                using SKPath bowtiePath = BuildBowtieIconPath(bounds, false);
                using SKPaint bowtiePaint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = KoreNatoSymbolColorPalette.BorderColor,
                    StrokeWidth = StrokeWidthForCanvas(canvas),
                    IsAntialias = true
                };
                canvas.Canvas.DrawPath(bowtiePath, bowtiePaint);
                break;
            }

            case NatoPlatformFunction.MilitaryRotaryWing:
            {
                using SKPath infinityPath = BuildInfinityIconPath(bounds, true, 0.1f);
                using SKPaint infinityPaint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = KoreNatoSymbolColorPalette.BorderColor,
                    IsAntialias = true
                };
                canvas.Canvas.DrawPath(infinityPath, infinityPaint);
                break;
            }

            case NatoPlatformFunction.CivilianRotaryWing:
            {
                using SKPath infinityPath = BuildInfinityIconPath(bounds, false, 0.1f);
                using SKPaint infinityPaint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = KoreNatoSymbolColorPalette.BorderColor,
                    StrokeWidth = StrokeWidthForCanvas(canvas),
                    StrokeCap = SKStrokeCap.Round,
                    StrokeJoin = SKStrokeJoin.Round,
                    IsAntialias = true
                };
                canvas.Canvas.DrawPath(infinityPath, infinityPaint);
                break;
            }

            case NatoPlatformFunction.MilitaryBalloon:
            {
                using SKPath balloonPath = BuildBalloonIconPath(bounds, true);
                using SKPaint balloonPaint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = KoreNatoSymbolColorPalette.BorderColor,
                    IsAntialias = true
                };
                canvas.Canvas.DrawPath(balloonPath, balloonPaint);
                break;
            }

            case NatoPlatformFunction.CivilianBalloon:
            {
                using SKPath balloonPath = BuildBalloonIconPath(bounds, false);
                using SKPaint balloonPaint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = KoreNatoSymbolColorPalette.BorderColor,
                    StrokeWidth = StrokeWidthForCanvas(canvas),
                    IsAntialias = true
                };
                canvas.Canvas.DrawPath(balloonPath, balloonPaint);
                break;
            }

            case NatoPlatformFunction.MilitaryAirship:
            {
                using SKPath airshipPath = BuildAirshipIconPath(bounds, true);
                using SKPaint airshipPaint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = KoreNatoSymbolColorPalette.BorderColor,
                    IsAntialias = true
                };
                canvas.Canvas.DrawPath(airshipPath, airshipPaint);
                break;
            }

            case NatoPlatformFunction.CivilianAirship:
            {
                using SKPath airshipPath = BuildAirshipIconPath(bounds, false);
                using SKPaint airshipPaint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = KoreNatoSymbolColorPalette.BorderColor,
                    StrokeWidth = StrokeWidthForCanvas(canvas),
                    IsAntialias = true
                };
                canvas.Canvas.DrawPath(airshipPath, airshipPaint);
                break;
            }

            case NatoPlatformFunction.UnmannedAerialVehicle:
            {
                using SKPath uavPath = BuildUAVIconPath(bounds, true);
                using SKPaint uavPaint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = KoreNatoSymbolColorPalette.BorderColor,
                    IsAntialias = true
                };
                canvas.Canvas.DrawPath(uavPath, uavPaint);
                break;
            }

            case NatoPlatformFunction.AirDecoy:
            {
                using SKPath airDecoyPath = BuildAirDecoyIconPath(bounds, true);
                using SKPaint airDecoyPaint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = KoreNatoSymbolColorPalette.BorderColor,
                    IsAntialias = true
                };
                canvas.Canvas.DrawPath(airDecoyPath, airDecoyPaint);
                break;
            }

            case NatoPlatformFunction.MedicalEvacuation:
                SKPath medevacPath = BuildMedEvacIconPath(bounds, true);
                using (SKPaint medevacPaint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = KoreNatoSymbolColorPalette.AccentColor,
                    IsAntialias = true
                })
                {
                    canvas.Canvas.DrawPath(medevacPath, medevacPaint);
                }
                medevacPath.Dispose();
                break;

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
    // MARK: Text Icons
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

    // --------------------------------------------------------------------------------------------
    // MARK: Image Icons
    // --------------------------------------------------------------------------------------------

    public static SKPath BuildInfinityIconPath(SKRect bounds, bool isFilled, float insetFraction = 0.10f)
    {
        insetFraction = Math.Clamp(insetFraction, 0f, 0.49f);

        float inset = bounds.Width * insetFraction;
        float left = bounds.Left + inset;
        float right = bounds.Right - inset;
        float top = bounds.Top + (inset * 0.5f);
        float bottom = bounds.Bottom - (inset * 0.5f);

        float midX = (left + right) * 0.5f;
        float midY = (top + bottom) * 0.5f;
        float halfWidth = (right - left) * 0.5f;
        float halfHeight = (bottom - top) * 0.5f;

        if (!isFilled)
        {
            // Civilian: thin centerline infinity.
            SKPath centerPath = new SKPath();
            int samples = 200;
            for (int i = 0; i <= samples; i++)
            {
                float t = (float)(-Math.PI + ((2.0 * Math.PI) * i / samples));
                float s = MathF.Sin(t);
                float c = MathF.Cos(t);

                // Gerono lemniscate: x = sin(t), y = sin(t)cos(t)
                float xNorm = s;
                float yNorm = s * c;

                float x = midX + (xNorm * halfWidth);
                float y = midY + (yNorm * (halfHeight * 0.95f));

                if (i == 0)
                    centerPath.MoveTo(x, y);
                else
                    centerPath.LineTo(x, y);
            }
            return centerPath;
        }

        // Military: explicit closed silhouette (two lobes), not a thick line.
        float w = right - left;
        float h = bottom - top;
        float centerCtrlX = w * 0.24f;
        float centerCtrlY = h * 0.30f;
        float endCtrlY = h * 0.26f;

        SKPath fillPath = new SKPath();

        // Left filled lobe.
        fillPath.MoveTo(midX, midY);
        fillPath.CubicTo(
            midX - centerCtrlX, midY - centerCtrlY,
            left, midY - endCtrlY,
            left, midY
        );
        fillPath.CubicTo(
            left, midY + endCtrlY,
            midX - centerCtrlX, midY + centerCtrlY,
            midX, midY
        );
        fillPath.Close();

        // Right filled lobe.
        fillPath.MoveTo(midX, midY);
        fillPath.CubicTo(
            midX + centerCtrlX, midY - centerCtrlY,
            right, midY - endCtrlY,
            right, midY
        );
        fillPath.CubicTo(
            right, midY + endCtrlY,
            midX + centerCtrlX, midY + centerCtrlY,
            midX, midY
        );
        fillPath.Close();

        fillPath.FillType = SKPathFillType.Winding;
        return fillPath;
    }

    // --------------------------------------------------------------------------------------------

    public static SKPath BuildBowtieIconPath(SKRect bounds, bool isFilled, float insetFraction = 0.1f)
    {
        SKPath path = new SKPath();

        // Inset is expressed as a fraction of rect width to scale across output sizes.
        insetFraction = Math.Clamp(insetFraction, 0f, 0.49f);
        float inset = bounds.Width * insetFraction;
        float left = bounds.Left + inset;
        float right = bounds.Right - inset;
        float top = bounds.Top + (inset / 2f);
        float bottom = bounds.Bottom - (inset / 2f);
        float midX = bounds.MidX;
        float midY = bounds.MidY;

        // Left triangle.
        path.MoveTo(left, top);
        path.LineTo(midX, midY);
        path.LineTo(left, bottom);
        path.Close();

        // Right triangle.
        path.MoveTo(right, top);
        path.LineTo(midX, midY);
        path.LineTo(right, bottom);
        path.Close();

        path.FillType = SKPathFillType.Winding;
        return path;
    }

    // --------------------------------------------------------------------------------------------

    public static SKPath BuildBalloonIconPath(SKRect bounds, bool isFilled)
    {
        SKPath path = new SKPath();

        // Build a simple balloon: circular envelope + centered neck rectangle.
        float inset = Math.Min(bounds.Width, bounds.Height) * 0.08f;
        float left = bounds.Left + inset;
        float right = bounds.Right - inset;
        float top = bounds.Top + inset;
        float bottom = bounds.Bottom - inset;

        float availableWidth = right - left;
        float availableHeight = bottom - top;

        // Keep circle dominant with a small neck below it.
        float circleDiameter = Math.Min(availableWidth, availableHeight * 0.78f);
        float radius = circleDiameter * 0.5f;
        float centerX = (left + right) * 0.5f;
        float circleTop = top;
        float centerY = circleTop + radius;
        float circleBottom = centerY + radius;

        SKRect circleRect = new SKRect(
            centerX - radius,
            centerY - radius,
            centerX + radius,
            centerY + radius
        );
        path.AddOval(circleRect);

        float neckWidth = circleDiameter * 0.32f;
        float neckHeight = Math.Max((bottom - circleBottom), circleDiameter * 0.16f);
        float neckTop = circleBottom;
        float neckBottom = Math.Min(neckTop + neckHeight, bottom);

        SKRect neckRect = new SKRect(
            centerX - (neckWidth * 0.5f),
            neckTop,
            centerX + (neckWidth * 0.5f),
            neckBottom
        );
        path.AddRect(neckRect);

        path.FillType = SKPathFillType.Winding;
        return path;
    }

    // --------------------------------------------------------------------------------------------


    public static SKPath BuildAirshipIconPath(SKRect bounds, bool isFilled)
    {
        float inset = Math.Min(bounds.Width, bounds.Height) * 0.06f;
        float left = bounds.Left + inset;
        float right = bounds.Right - inset;
        float top = bounds.Top + inset;
        float bottom = bounds.Bottom - inset;
        float width = right - left;
        float height = bottom - top;
        float midY = (top + bottom) * 0.5f;

        // Main hull (airship body).
        float hullWidth = width * 0.78f;
        float hullHeight = height * 0.56f;
        float hullLeft = left;
        float hullTop = midY - (hullHeight * 0.5f);
        SKRect hullRect = new SKRect(hullLeft, hullTop, hullLeft + hullWidth, hullTop + hullHeight);
        using SKPath hullPath = new SKPath();
        hullPath.AddOval(hullRect);

        // Tail assembly attached to hull rear.
        float tailAttachX = hullRect.Right - (hullHeight * 0.08f);
        float tailMidX = right - (width * 0.16f);
        float tailOuterX = right - (width * 0.02f);

        float waistHalf = hullHeight * 0.18f;
        float finTipOffset = hullHeight * 0.66f;

        using SKPath tailPath = new SKPath();
        tailPath.MoveTo(tailAttachX, midY - waistHalf);
        tailPath.LineTo(tailMidX, midY - waistHalf);
        tailPath.LineTo(tailOuterX, midY - finTipOffset);
        tailPath.LineTo(tailMidX, midY);
        tailPath.LineTo(tailOuterX, midY + finTipOffset);
        tailPath.LineTo(tailMidX, midY + waistHalf);
        tailPath.LineTo(tailAttachX, midY + waistHalf);
        tailPath.Close();

        // Return a merged silhouette so stroke mode has no interior overlap lines.
        SKPath? merged = hullPath.Op(tailPath, SKPathOp.Union);
        if (merged == null)
        {
            SKPath fallback = new SKPath();
            fallback.AddPath(hullPath);
            fallback.AddPath(tailPath);
            fallback.FillType = SKPathFillType.Winding;
            return fallback;
        }

        merged.FillType = SKPathFillType.Winding;
        return merged;
    }

    // --------------------------------------------------------------------------------------------

    public static SKPath BuildUAVIconPath(SKRect bounds, bool isFilled)
    {
        SKPath path = new SKPath();

        // Large, shallow, downward-facing chevron as a single closed shape.
        float insetX = bounds.Width  * 0.15f;
        float insetY = bounds.Height * 0.18f;

        float left = bounds.Left + insetX;
        float right = bounds.Right - insetX;
        float midX = bounds.MidX;

        float topY = bounds.Top + insetY;
        float bottomY = bounds.MidY + (bounds.Height * 0.14f);

        float endTopY = topY;
        float endBottomY = topY + (bounds.Height * 0.18f);

        float midTopY = bounds.MidY;
        float midBottomY = bounds.Bottom - insetY;

        // float thickness = Math.Max(bounds.Height * 0.18f, 2f);
        // float innerTopY = topY + thickness;
        // float innerBottomY = bottomY + (thickness * 1.2f);

        // Outer V.
        path.MoveTo(left, endTopY);
        path.LineTo(midX, midTopY);
        path.LineTo(right, endTopY);

        // Inner return V (to make a thick chevron band).
        path.LineTo(right, endBottomY);
        path.LineTo(midX, midBottomY);
        path.LineTo(left, endBottomY);
        path.Close();

        path.FillType = SKPathFillType.Winding;
        return path;
    }

    // --------------------------------------------------------------------------------------------

    public static SKPath BuildAirDecoyIconPath(SKRect bounds, bool isFilled)
    {
        SKPath path = new SKPath();

        float insetX = bounds.Width * 0.12f;
        float insetY = bounds.Height * 0.10f;
        float left = bounds.Left + insetX;
        float right = bounds.Right - insetX;
        float top = bounds.Top + insetY;
        float bottom = bounds.Bottom - insetY;
        float width = right - left;
        float height = bottom - top;

        // Baseline bar.
        float barHeight = Math.Max(height * 0.10f, 2f);
        float barTop = bottom - barHeight;
        SKRect barRect = new SKRect(left, barTop, right, bottom);
        path.AddRect(barRect);

        // Three left-pointing triangles above the bar.
        float triRegionTop = top;
        float triRegionBottom = barTop - (height * 0.12f);
        float triHeight = Math.Max(triRegionBottom - triRegionTop, 2f);
        float triWidth = width * 0.23f;
        float triGap = width * 0.035f;

        // Align group to right side like the reference.
        float groupWidth = (3f * triWidth) + (2f * triGap);
        float startX = right - groupWidth;

        for (int i = 0; i < 3; i++)
        {
            float triLeft = startX + i * (triWidth + triGap);
            float triRight = triLeft + triWidth;
            float midY = triRegionTop + (triHeight * 0.5f);

            // Left-pointing filled triangle.
            path.MoveTo(triLeft, midY);
            path.LineTo(triRight, triRegionTop);
            path.LineTo(triRight, triRegionTop + triHeight);
            path.Close();
        }

        path.FillType = SKPathFillType.Winding;
        return path;
    }

    // --------------------------------------------------------------------------------------------

    public static SKPath BuildMedEvacIconPath(SKRect bounds, bool isFilled)
    {
        SKPath path = new SKPath();

        // Build a square-aspect cross that maximizes use of the rect.
        float squareSize = Math.Min(bounds.Width, bounds.Height);
        float left = bounds.MidX - (squareSize * 0.5f);
        float top = bounds.MidY - (squareSize * 0.5f);

        // Arm thickness as a fraction of square size gives a recognisable red-cross shape.
        float armThickness = squareSize * 0.33f;
        float armInset = (squareSize - armThickness) * 0.5f;

        SKRect vertical = new SKRect(
            left + armInset,
            top,
            left + armInset + armThickness,
            top + squareSize
        );

        SKRect horizontal = new SKRect(
            left,
            top + armInset,
            left + squareSize,
            top + armInset + armThickness
        );

        path.AddRect(vertical);
        path.AddRect(horizontal);
        path.FillType = SKPathFillType.Winding;
        return path;
    }

}
