// <fileheader>

using System;
using System.Collections.Generic;
using System.Linq;
using KoreCommon;
namespace KoreCommon.UnitTest;

public static class KoreTestMath
{
    public static void RunTests(KoreTestLog testLog)
    {
        try
        {
            TestValueUtilsBool(testLog);
            TestValueUtilsInt(testLog);
            TestValueUtilsFloat(testLog);
            TestFloat1DArray_Basics(testLog);
            TestNumberRange(testLog);
            TestScaleToRange(testLog);
        }
        catch (Exception ex)
        {
            testLog.AddResult("KoreTestMath RunTests", false, ex.Message);
            return;
        }
    }

    public static void TestValueUtilsBool(KoreTestLog testLog)
    {
        testLog.AddResult("BoolToStr(true)",      (KoreValueUtils.BoolToStr(true)    == "True"));
        testLog.AddResult("BoolToStr(false)",     (KoreValueUtils.BoolToStr(false)   == "False"));

        testLog.AddResult("StrToBool(\"True\")",  (KoreValueUtils.StrToBool("True")  == true));
        testLog.AddResult("StrToBool(\"False\")", (KoreValueUtils.StrToBool("False") == false));
        testLog.AddResult("StrToBool(\"true\")",  (KoreValueUtils.StrToBool("true")  == true));
        testLog.AddResult("StrToBool(\"false\")", (KoreValueUtils.StrToBool("false") == false));
        testLog.AddResult("StrToBool(\"y\")",     (KoreValueUtils.StrToBool("y")     == true));
        testLog.AddResult("StrToBool(\"n\")",     (KoreValueUtils.StrToBool("n")     == false));

        testLog.AddResult("StrToBool(\" \")",     (KoreValueUtils.StrToBool(" ")     == false));
        testLog.AddResult("StrToBool(\"1212\")",  (KoreValueUtils.StrToBool("1212")  == false));
    }

    public static void TestValueUtilsInt(KoreTestLog testLog)
    {
        testLog.AddResult("Clamp( 0, 1, 10)", (KoreValueUtils.Clamp( 0, 1, 10) == 1));
        testLog.AddResult("Clamp( 5, 1, 10)", (KoreValueUtils.Clamp( 5, 1, 10) == 5));
        testLog.AddResult("Clamp(11, 1, 10)", (KoreValueUtils.Clamp(11, 1, 10) == 10));

        testLog.AddResult("Wrap( 0, 1, 10)", (KoreValueUtils.Wrap( 0, 1, 10) == 10));
        testLog.AddResult("Wrap( 5, 1, 10)", (KoreValueUtils.Wrap( 5, 1, 10) == 5));
        testLog.AddResult("Wrap(11, 1, 10)", (KoreValueUtils.Wrap(11, 1, 10) == 1));
    }

    public static void TestValueUtilsFloat(KoreTestLog testLog)
    {
        // Test for Modulo operation
        testLog.AddResult("Modulo 1.1 % 1.0",  KoreValueUtils.EqualsWithinTolerance(KoreValueUtils.Modulo(1.1f, 1.0f), 0.1f));
        testLog.AddResult("Modulo 2.1 % 1.0",  KoreValueUtils.EqualsWithinTolerance(KoreValueUtils.Modulo(2.1f, 1.0f), 0.1f));
        testLog.AddResult("Modulo -0.1 % 1.0", KoreValueUtils.EqualsWithinTolerance(KoreValueUtils.Modulo(-0.1f, 1.0f), 0.9f));

        // Test for LimitToRange operation
        testLog.AddResult("LimitToRange 1.1 in 0-1", KoreValueUtils.EqualsWithinTolerance(KoreValueUtils.LimitToRange(1.1f, 0f, 1f), 1f));
        testLog.AddResult("LimitToRange -5 in 0-1",  KoreValueUtils.EqualsWithinTolerance(KoreValueUtils.LimitToRange(-5f, 0f, 1f), 0f));
        testLog.AddResult("LimitToRange 0.5 in 0-1", KoreValueUtils.EqualsWithinTolerance(KoreValueUtils.LimitToRange(0.5f, 0f, 1f), 0.5f));

        // Test for WrapToRange operation
        testLog.AddResult("WrapToRange 1.1 in 1-2",  KoreValueUtils.EqualsWithinTolerance(KoreValueUtils.WrapToRange(1.1f, 1f, 2f), 1.1f));
        testLog.AddResult("WrapToRange 3.1 in 1-2",  KoreValueUtils.EqualsWithinTolerance(KoreValueUtils.WrapToRange(3.1f, 1f, 2f), 1.1f));
        testLog.AddResult("WrapToRange -1.5 in 1-2", KoreValueUtils.EqualsWithinTolerance(KoreValueUtils.WrapToRange(-1.5f, 1f, 2f), 1.5f));

        // Test for DiffInWrapRange operation
        testLog.AddResult("DiffInWrapRange 1 to 350 in 0-360",  KoreValueUtils.EqualsWithinTolerance(KoreValueUtils.DiffInWrapRange(0f, 360f, 1f, 350f), -11f));
        testLog.AddResult("DiffInWrapRange 1 to 5 in 0-360",    KoreValueUtils.EqualsWithinTolerance(KoreValueUtils.DiffInWrapRange(0f, 360f, 1f, 5f), 4f));
        testLog.AddResult("DiffInWrapRange 340 to 20 in 0-360", KoreValueUtils.EqualsWithinTolerance(KoreValueUtils.DiffInWrapRange(0f, 360f, 340f, 20f), 40f));

        // Test for IndexFromFraction operation
        testLog.AddResult($"IndexFromFraction 0.1 in 0-10",   KoreValueUtils.EqualsWithinTolerance(KoreValueUtils.IndexFromFraction(0.1f, 0, 10), 1));
        testLog.AddResult($"IndexFromFraction 0.2 in 0-100",  KoreValueUtils.EqualsWithinTolerance(KoreValueUtils.IndexFromFraction(0.2f, 0, 100), 20));
        testLog.AddResult($"IndexFromFraction 0.49 in 0-5",   KoreValueUtils.EqualsWithinTolerance(KoreValueUtils.IndexFromFraction(0.49f, 0, 5), 2));
        testLog.AddResult($"IndexFromFraction 0.50 in 0-5",   KoreValueUtils.EqualsWithinTolerance(KoreValueUtils.IndexFromFraction(0.50f, 0, 5), 2));
        testLog.AddResult($"IndexFromFraction 0.6  in 0-5",   KoreValueUtils.EqualsWithinTolerance(KoreValueUtils.IndexFromFraction(0.6f, 0, 5), 3));

        // Test for IndexFromIncrement operation
        testLog.AddResult("IndexFromIncrement 0.1 from 0 increment 1",  KoreValueUtils.EqualsWithinTolerance(KoreValueUtils.IndexFromIncrement(0f, 1f, 0.1f), 0));
        testLog.AddResult("IndexFromIncrement 1.1 from 0 increment 1",  KoreValueUtils.EqualsWithinTolerance(KoreValueUtils.IndexFromIncrement(0f, 1f, 1.1f), 1));
        testLog.AddResult("IndexFromIncrement 13.1 from 0 increment 1", KoreValueUtils.EqualsWithinTolerance(KoreValueUtils.IndexFromIncrement(0f, 1f, 13.1f), 13));

        // Test for IsInRange operation
        testLog.AddResult("IsInRange 0 in 0-1",   KoreValueUtils.IsInRange(0f, 0f, 1f));
        testLog.AddResult("IsInRange 1 in 0-1",   KoreValueUtils.IsInRange(1f, 0f, 1f));
        testLog.AddResult("!IsInRange -1 in 0-1", !KoreValueUtils.IsInRange(-1f, 0f, 1f));
        testLog.AddResult("!IsInRange 2 in 0-1",  !KoreValueUtils.IsInRange(2f, 0f, 1f));

        // Test for Interpolate operation
        testLog.AddResult("Interpolate 0.1 between 0 and 1",    KoreValueUtils.EqualsWithinTolerance(KoreValueUtils.Interpolate(0f, 1f, 0.1f), 0.1f));
        testLog.AddResult("Interpolate 0.9 between 0 and 100",  KoreValueUtils.EqualsWithinTolerance(KoreValueUtils.Interpolate(0f, 100f, 0.9f), 90f));
        testLog.AddResult("Interpolate 1.1 between 0 and 100",  KoreValueUtils.EqualsWithinTolerance(KoreValueUtils.Interpolate(0f, 100f, 1.1f), 110f));
        testLog.AddResult("Interpolate -0.1 between 0 and 100", KoreValueUtils.EqualsWithinTolerance(KoreValueUtils.Interpolate(0f, 100f, -0.1f), -10f));
    }

    public static void TestFloat1DArray_Basics(KoreTestLog testLog)
    {
        KoreNumeric1DArray<float> array = new KoreNumeric1DArray<float>(5);
        array[0] = 1.0f;
        array[1] = 2.0f;
        array[2] = 3.0f;
        array[3] = 4.0f;
        array[4] = 5.0f;

        // Test Length
        testLog.AddResult("Array Length", array.Length == 5);

        // Test individual element access
        testLog.AddResult("Array[0]", KoreValueUtils.EqualsWithinTolerance(array[0], 1.0f));
        testLog.AddResult("Array[1]", KoreValueUtils.EqualsWithinTolerance(array[1], 2.0f));
        testLog.AddResult("Array[2]", KoreValueUtils.EqualsWithinTolerance(array[2], 3.0f));
        testLog.AddResult("Array[3]", KoreValueUtils.EqualsWithinTolerance(array[3], 4.0f));
        testLog.AddResult("Array[4]", KoreValueUtils.EqualsWithinTolerance(array[4], 5.0f));

        // Test Max
        testLog.AddResult("Array Max", KoreValueUtils.EqualsWithinTolerance(array.Max(), 5.0f));

        // Test Min
        testLog.AddResult("Array Min", KoreValueUtils.EqualsWithinTolerance(array.Min(), 1.0f));

        // Test Average
        testLog.AddResult("Array Average", KoreValueUtils.EqualsWithinTolerance(array.Average(), 3.0f));

        // Test Sum
        testLog.AddResult("Array Sum", KoreValueUtils.EqualsWithinTolerance(array.Sum(), 15.0f));

        // Additional tests for boundary conditions and invalid inputs
        // Assuming KoreFloat1DArray handles negative indices or out-of-bound indices gracefully
        // testLog.Add("Array[-1]", KoreValueUtils.EqualsWithinTolerance(array[-1], /* expected value */));
        // testLog.Add("Array[5]", KoreValueUtils.EqualsWithinTolerance(array[5], /* expected value */));
    }

    private static void TestNumberRange(KoreTestLog testLog)
    {
        {
            List<double> testList = KoreValueUtils.CreateRangeList(10, -3, +3);

            // concatenate the list into a CSV string, formatted F3
            string csvStr = string.Join(", ", testList.Select(x => x.ToString("F3")));
            testLog.AddComment($"Number Range: {csvStr}");
        }
    }

    private static void TestScaleToRange(KoreTestLog testLog)
    {
        {
            double startVal = 5.0;
            double startRangeMin = 0.0;
            double startRangeMax = 10.0;

            double targetVal = 150.0;
            double targetRangeMin = 100.0;
            double targetRangeMax = 200.0;

            double scaledVal = KoreNumericUtils.ScaleToRange(startVal, startRangeMin, startRangeMax, targetRangeMin, targetRangeMax);
            testLog.AddResult("ScaleToRange", KoreValueUtils.EqualsWithinTolerance(scaledVal, targetVal), "Simple halfway 1");
        }

        {
            double startVal = 0.0;
            double startRangeMin = -10.0;
            double startRangeMax = 10.0;

            double targetVal = 150.0;
            double targetRangeMin = 100.0;
            double targetRangeMax = 200.0;

            double scaledVal = KoreNumericUtils.ScaleToRange(startVal, startRangeMin, startRangeMax, targetRangeMin, targetRangeMax);
            testLog.AddResult("ScaleToRange", KoreValueUtils.EqualsWithinTolerance(scaledVal, targetVal), "Simple halfway 2");
        }

        {
            double startVal = 45.0;
            double startRangeMax = 90.0;
            double startRangeMin = -90.0;

            double targetVal = 0.25;
            double targetRangeMax = 0.0;
            double targetRangeMin = 1.0;

            double scaledVal = KoreNumericUtils.ScaleToUncheckedRange(startVal, startRangeMin, startRangeMax, targetRangeMin, targetRangeMax);
            testLog.AddResult("ScaleToUncheckedRange", KoreValueUtils.EqualsWithinTolerance(scaledVal, targetVal), "ScaleToUncheckedRange Flipped Range");
        }

    }
}


