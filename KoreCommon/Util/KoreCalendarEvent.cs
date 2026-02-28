// <fileheader>

using System;

namespace KoreCommon;

#nullable enable

// Defines the types of calendar events supported.
public enum KoreCalendarEventType
{
    SingleOccurrence,  // Occurs once at a specific datetime
    Minutely,          // Occurs every N minutes
    Hourly,            // Occurs every N hours
    Daily,             // Occurs every N days
    Weekly,            // Occurs every N weeks
    Monthly,           // Occurs every N months
    Yearly,            // Occurs every N years
    CustomDates        // Occurs at specific irregular datetimes
}

// Represents a calendar event that can occur at specific times or on regular/irregular intervals.
// Supports events over long timespans (weeks, months, years).
public class KoreCalendarEvent
{
    private          DateTime                _nextOccurrence;
    private readonly KoreCalendarEventType   _eventType;
    private readonly int                     _intervalValue;
    private readonly DateTime[]?             _customDates;
    private          int                     _customDateIndex;

    public DateTime              NextOccurrence => _nextOccurrence;
    public KoreCalendarEventType EventType       => _eventType;

    // Constructor for single occurrence events
    public KoreCalendarEvent(DateTime singleOccurrence)
    {
        _eventType       = KoreCalendarEventType.SingleOccurrence;
        _nextOccurrence  = singleOccurrence;
        _intervalValue   = 0;
        _customDates     = null;
        _customDateIndex = 0;
    }

    // Constructor for regular interval events
    public KoreCalendarEvent(DateTime startOccurrence, KoreCalendarEventType eventType, int intervalValue = 1)
    {
        if (eventType == KoreCalendarEventType.SingleOccurrence || eventType == KoreCalendarEventType.CustomDates)
        {
            throw new ArgumentException("Use appropriate constructor for single occurrence or custom dates.", nameof(eventType));
        }

        if (intervalValue <= 0)
        {
            throw new ArgumentException("Interval value must be greater than zero.", nameof(intervalValue));
        }

        _eventType       = eventType;
        _nextOccurrence  = startOccurrence;
        _intervalValue   = intervalValue;
        _customDates     = null;
        _customDateIndex = 0;
    }

    // Constructor for irregular (custom dates) events
    public KoreCalendarEvent(DateTime[] customDates)
    {
        if (customDates == null || customDates.Length == 0)
        {
            throw new ArgumentException("Custom dates array cannot be null or empty.", nameof(customDates));
        }

        // Sort the dates to ensure chronological order
        Array.Sort(customDates);

        _eventType       = KoreCalendarEventType.CustomDates;
        _customDates     = customDates;
        _customDateIndex = 0;
        _nextOccurrence  = _customDates[_customDateIndex];
        _intervalValue   = 0;
    }

    // Checks if the current occurrence has passed given the current time.
    // currentTime: The current time to check against.
    // Returns: True if the occurrence has passed, false otherwise.
    public bool IsOccurrencePassed(DateTime currentTime)
    {
        return currentTime >= _nextOccurrence;
    }

    // Moves to the next occurrence based on the event type.
    // Returns: True if there is a next occurrence, false if the event has no more occurrences.
    public bool MoveToNextOccurrence()
    {
        switch (_eventType)
        {
            case KoreCalendarEventType.SingleOccurrence:
                // Single occurrence events don't have a next occurrence
                return false;

            case KoreCalendarEventType.Minutely:
                _nextOccurrence = _nextOccurrence.AddMinutes(_intervalValue);
                return true;

            case KoreCalendarEventType.Hourly:
                _nextOccurrence = _nextOccurrence.AddHours(_intervalValue);
                return true;

            case KoreCalendarEventType.Daily:
                _nextOccurrence = _nextOccurrence.AddDays(_intervalValue);
                return true;

            case KoreCalendarEventType.Weekly:
                _nextOccurrence = _nextOccurrence.AddDays(_intervalValue * 7);
                return true;

            case KoreCalendarEventType.Monthly:
                _nextOccurrence = _nextOccurrence.AddMonths(_intervalValue);
                return true;

            case KoreCalendarEventType.Yearly:
                _nextOccurrence = _nextOccurrence.AddYears(_intervalValue);
                return true;

            case KoreCalendarEventType.CustomDates:
                if (_customDates != null && _customDateIndex < _customDates.Length - 1)
                {
                    _customDateIndex++;
                    _nextOccurrence = _customDates[_customDateIndex];
                    return true;
                }
                return false;

            default:
                return false;
        }
    }

    // Advances to the next occurrence that is after the given current time.
    // Useful for catching up if multiple occurrences have passed.
    // currentTime: The current time to advance past.
    // Returns: True if an occurrence was found after current time, false otherwise.
    public bool AdvancePastTime(DateTime currentTime)
    {
        while (IsOccurrencePassed(currentTime))
        {
            if (!MoveToNextOccurrence())
            {
                return false;
            }
        }
        return true;
    }

    // Gets the time remaining until the next occurrence from the given current time.
    // currentTime: The current time to measure from.
    // Returns: TimeSpan until next occurrence, or TimeSpan.Zero if already passed.
    public TimeSpan TimeUntilNextOccurrence(DateTime currentTime)
    {
        if (currentTime >= _nextOccurrence)
        {
            return TimeSpan.Zero;
        }
        return _nextOccurrence - currentTime;
    }
}


