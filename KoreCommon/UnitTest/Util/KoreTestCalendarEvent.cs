// <fileheader>

using System;

using KoreCommon;
namespace KoreCommon.UnitTest;


public static class KoreTestCalendarEvent
{
    // Usage: KoreTestCalendarEvent.RunTests(testLog);
    public static void RunTests(KoreTestLog testLog)
    {
        try
        {
            TestSingleOccurrence(testLog);
            TestHourlyInterval(testLog);
            TestDailyInterval(testLog);
            TestWeeklyInterval(testLog);
            TestMonthlyInterval(testLog);
            TestYearlyInterval(testLog);
            TestCustomDates(testLog);
            TestAdvancePastTime(testLog);
            TestTimeUntilNextOccurrence(testLog);
        }
        catch (Exception ex)
        {
            testLog.AddResult("KoreTestCalendarEvent RunTests // Exception: ", false, ex.Message);
            return;
        }
    }

    // Test single occurrence events
    public static void TestSingleOccurrence(KoreTestLog testLog)
    {
        var eventTime = new DateTime(2026, 3, 15, 14, 30, 0);
        var calEvent = new KoreCalendarEvent(eventTime);

        // Test before the event
        var beforeTime = new DateTime(2026, 3, 15, 14, 0, 0);
        bool isPassed = calEvent.IsOccurrencePassed(beforeTime);
        testLog.AddResult("Single Occurrence - Before Event", !isPassed,
            $"Should not be passed before event time. Result: {isPassed}");

        // Test at exact event time
        isPassed = calEvent.IsOccurrencePassed(eventTime);
        testLog.AddResult("Single Occurrence - At Event Time", isPassed,
            $"Should be passed at event time. Result: {isPassed}");

        // Test after the event
        var afterTime = new DateTime(2026, 3, 15, 15, 0, 0);
        isPassed = calEvent.IsOccurrencePassed(afterTime);
        testLog.AddResult("Single Occurrence - After Event", isPassed,
            $"Should be passed after event time. Result: {isPassed}");

        // Test that single occurrence has no next occurrence
        bool hasNext = calEvent.MoveToNextOccurrence();
        testLog.AddResult("Single Occurrence - No Next", !hasNext,
            $"Single occurrence should not have next occurrence. Result: {hasNext}");
    }

    // Test hourly interval events
    public static void TestHourlyInterval(KoreTestLog testLog)
    {
        var startTime = new DateTime(2026, 3, 1, 10, 0, 0);
        var calEvent = new KoreCalendarEvent(startTime, KoreCalendarEventType.Hourly, 2); // Every 2 hours

        // Test initial occurrence
        testLog.AddComment($"Hourly Event Start: {calEvent.NextOccurrence}");

        // Move to next occurrence
        calEvent.MoveToNextOccurrence();
        var expectedTime = new DateTime(2026, 3, 1, 12, 0, 0);
        bool isCorrect = calEvent.NextOccurrence == expectedTime;
        testLog.AddResult("Hourly Interval - First Next", isCorrect,
            $"Expected: {expectedTime}, Got: {calEvent.NextOccurrence}");

        // Move again
        calEvent.MoveToNextOccurrence();
        expectedTime = new DateTime(2026, 3, 1, 14, 0, 0);
        isCorrect = calEvent.NextOccurrence == expectedTime;
        testLog.AddResult("Hourly Interval - Second Next", isCorrect,
            $"Expected: {expectedTime}, Got: {calEvent.NextOccurrence}");
    }

    // Test daily interval events
    public static void TestDailyInterval(KoreTestLog testLog)
    {
        var startTime = new DateTime(2026, 3, 1, 9, 0, 0);
        var calEvent = new KoreCalendarEvent(startTime, KoreCalendarEventType.Daily, 1); // Every day

        testLog.AddComment($"Daily Event Start: {calEvent.NextOccurrence}");

        // Move through several days
        for (int i = 1; i <= 5; i++)
        {
            calEvent.MoveToNextOccurrence();
            var expectedTime = startTime.AddDays(i);
            bool isCorrect = calEvent.NextOccurrence == expectedTime;
            testLog.AddResult($"Daily Interval - Day {i}", isCorrect,
                $"Expected: {expectedTime}, Got: {calEvent.NextOccurrence}");
        }
    }

    // Test weekly interval events
    public static void TestWeeklyInterval(KoreTestLog testLog)
    {
        var startTime = new DateTime(2026, 3, 1, 9, 0, 0);
        var calEvent = new KoreCalendarEvent(startTime, KoreCalendarEventType.Weekly, 2); // Every 2 weeks

        testLog.AddComment($"Weekly Event Start: {calEvent.NextOccurrence}");

        calEvent.MoveToNextOccurrence();
        var expectedTime = startTime.AddDays(14);
        bool isCorrect = calEvent.NextOccurrence == expectedTime;
        testLog.AddResult("Weekly Interval - First Occurrence", isCorrect,
            $"Expected: {expectedTime}, Got: {calEvent.NextOccurrence}");

        calEvent.MoveToNextOccurrence();
        expectedTime = startTime.AddDays(28);
        isCorrect = calEvent.NextOccurrence == expectedTime;
        testLog.AddResult("Weekly Interval - Second Occurrence", isCorrect,
            $"Expected: {expectedTime}, Got: {calEvent.NextOccurrence}");
    }

    // Test monthly interval events
    public static void TestMonthlyInterval(KoreTestLog testLog)
    {
        var startTime = new DateTime(2026, 1, 15, 10, 0, 0);
        var calEvent = new KoreCalendarEvent(startTime, KoreCalendarEventType.Monthly, 1); // Every month

        testLog.AddComment($"Monthly Event Start: {calEvent.NextOccurrence}");

        // Test several months
        calEvent.MoveToNextOccurrence();
        var expectedTime = new DateTime(2026, 2, 15, 10, 0, 0);
        bool isCorrect = calEvent.NextOccurrence == expectedTime;
        testLog.AddResult("Monthly Interval - February", isCorrect,
            $"Expected: {expectedTime}, Got: {calEvent.NextOccurrence}");

        calEvent.MoveToNextOccurrence();
        expectedTime = new DateTime(2026, 3, 15, 10, 0, 0);
        isCorrect = calEvent.NextOccurrence == expectedTime;
        testLog.AddResult("Monthly Interval - March", isCorrect,
            $"Expected: {expectedTime}, Got: {calEvent.NextOccurrence}");

        // Test across year boundary
        var yearEndEvent = new KoreCalendarEvent(new DateTime(2026, 11, 15, 10, 0, 0),
            KoreCalendarEventType.Monthly, 3); // Every 3 months
        yearEndEvent.MoveToNextOccurrence();
        expectedTime = new DateTime(2027, 2, 15, 10, 0, 0);
        isCorrect = yearEndEvent.NextOccurrence == expectedTime;
        testLog.AddResult("Monthly Interval - Across Year", isCorrect,
            $"Expected: {expectedTime}, Got: {yearEndEvent.NextOccurrence}");
    }

    // Test yearly interval events
    public static void TestYearlyInterval(KoreTestLog testLog)
    {
        var startTime = new DateTime(2026, 3, 15, 10, 0, 0);
        var calEvent = new KoreCalendarEvent(startTime, KoreCalendarEventType.Yearly, 1); // Every year

        testLog.AddComment($"Yearly Event Start: {calEvent.NextOccurrence}");

        // Test several years
        for (int i = 1; i <= 3; i++)
        {
            calEvent.MoveToNextOccurrence();
            var expectedTime = new DateTime(2026 + i, 3, 15, 10, 0, 0);
            bool isCorrect = calEvent.NextOccurrence == expectedTime;
            testLog.AddResult($"Yearly Interval - Year {2026 + i}", isCorrect,
                $"Expected: {expectedTime}, Got: {calEvent.NextOccurrence}");
        }
    }

    // Test custom (irregular) dates
    public static void TestCustomDates(KoreTestLog testLog)
    {
        var customDates = new DateTime[]
        {
            new DateTime(2026, 3, 15, 10, 0, 0),
            new DateTime(2026, 5, 22, 14, 30, 0),
            new DateTime(2026, 7, 4, 9, 0, 0),
            new DateTime(2027, 1, 1, 0, 0, 0)
        };

        var calEvent = new KoreCalendarEvent(customDates);

        testLog.AddComment($"Custom Dates Event Start: {calEvent.NextOccurrence}");

        // Test each custom date
        for (int i = 0; i < customDates.Length - 1; i++)
        {
            var currentExpected = customDates[i];
            bool isCorrect = calEvent.NextOccurrence == currentExpected;
            testLog.AddResult($"Custom Dates - Date {i + 1}", isCorrect,
                $"Expected: {currentExpected}, Got: {calEvent.NextOccurrence}");

            calEvent.MoveToNextOccurrence();
        }

        // Test last date
        bool isLastCorrect = calEvent.NextOccurrence == customDates[customDates.Length - 1];
        testLog.AddResult("Custom Dates - Last Date", isLastCorrect,
            $"Expected: {customDates[customDates.Length - 1]}, Got: {calEvent.NextOccurrence}");

        // Test that there's no next occurrence after last custom date
        bool hasNext = calEvent.MoveToNextOccurrence();
        testLog.AddResult("Custom Dates - No More Occurrences", !hasNext,
            $"Should not have next occurrence after last date. Result: {hasNext}");
    }

    // Test advance past time functionality
    public static void TestAdvancePastTime(KoreTestLog testLog)
    {
        var startTime = new DateTime(2026, 3, 1, 10, 0, 0);
        var calEvent = new KoreCalendarEvent(startTime, KoreCalendarEventType.Daily, 1);

        testLog.AddComment($"Advance Past Time Start: {calEvent.NextOccurrence}");

        // Advance past several days
        var currentTime = new DateTime(2026, 3, 5, 15, 0, 0);
        bool advanced = calEvent.AdvancePastTime(currentTime);

        var expectedTime = new DateTime(2026, 3, 6, 10, 0, 0);
        bool isCorrect = advanced && calEvent.NextOccurrence == expectedTime;
        testLog.AddResult("Advance Past Time - Skip Multiple Days", isCorrect,
            $"Advanced: {advanced}, Expected: {expectedTime}, Got: {calEvent.NextOccurrence}");

        // Test with single occurrence event (should fail to advance)
        var singleEvent = new KoreCalendarEvent(new DateTime(2026, 3, 1, 10, 0, 0));
        currentTime = new DateTime(2026, 3, 5, 15, 0, 0);
        advanced = singleEvent.AdvancePastTime(currentTime);
        testLog.AddResult("Advance Past Time - Single Event Fails", !advanced,
            $"Single event should not advance. Result: {advanced}");
    }

    // Test time until next occurrence
    public static void TestTimeUntilNextOccurrence(KoreTestLog testLog)
    {
        var eventTime = new DateTime(2026, 3, 15, 14, 30, 0);
        var calEvent = new KoreCalendarEvent(eventTime);

        // Test time until occurrence
        var currentTime = new DateTime(2026, 3, 15, 12, 0, 0);
        var timeUntil = calEvent.TimeUntilNextOccurrence(currentTime);
        var expectedTimeSpan = TimeSpan.FromHours(2.5);

        bool isCorrect = timeUntil == expectedTimeSpan;
        testLog.AddResult("Time Until Occurrence - Before Event", isCorrect,
            $"Expected: {expectedTimeSpan}, Got: {timeUntil}");

        // Test after occurrence (should be zero)
        currentTime = new DateTime(2026, 3, 15, 15, 0, 0);
        timeUntil = calEvent.TimeUntilNextOccurrence(currentTime);
        isCorrect = timeUntil == TimeSpan.Zero;
        testLog.AddResult("Time Until Occurrence - After Event", isCorrect,
            $"Expected: Zero, Got: {timeUntil}");
    }
}
