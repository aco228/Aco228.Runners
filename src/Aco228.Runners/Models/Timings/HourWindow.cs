using Aco228.Common.Extensions;

namespace Aco228.Runners.Models.Timings;

public class HourWindow
{
    public int Hour { get; set; }
    public int Minute { get; set; } = 0;
    public bool IsUtc { get; set; } = false;

    public static HourWindow DayStart => new(0);
    public static HourWindow DayEnd => new(23,59);

    public HourWindow () { }
    public HourWindow (int hour, int minute = 0, bool isUtc = false)
    {
        Hour = hour;
        Minute = minute;
        IsUtc = isUtc;
    }

    public override string ToString() => $"{Hour.WithZeroPrefix()}:{Minute.WithZeroPrefix()}";
}

public class DelayWindow
{
    public int Value { get; set; }
    public DelayType Type { get; set; } = DelayType.Minutes;

    public DelayWindow ()
    {
    }

    public DelayWindow (int value, DelayType delayType = DelayType.Minutes)
    {
        Value = value;
        Type = delayType;
    }

    public override string ToString() => $"{Value.WithZeroPrefix()} {Type.ToString()}";
}

public static class HourWindowExtensions
{
    public static bool IsTimeOkay(HourWindow from, HourWindow to)
    {
        var currentTimeFrom = from.IsUtc ? DateTime.UtcNow : DateTime.Now;
        var currentTimeTo = to.IsUtc ? DateTime.UtcNow : DateTime.Now;

        var afterFrom = currentTimeFrom.Hour > from.Hour || (currentTimeFrom.Hour == from.Hour && currentTimeFrom.Minute >= from.Minute);
        var beforeTo = currentTimeTo.Hour < to.Hour || (currentTimeTo.Hour == to.Hour && currentTimeTo.Minute < to.Minute);

        return afterFrom && beforeTo;
    }
    
    public static bool IsLessThan(this HourWindow window, DateTime currentTime)
    {
        return currentTime.Hour == window.Hour && window.Minute < currentTime.Minute || window.Hour < currentTime.Hour;
    }
    
    public static bool IsMoreThan(this HourWindow window, DateTime currentTime)
    {
        return currentTime.Hour == window.Hour && window.Minute > currentTime.Minute || window.Hour > currentTime.Hour;
    }
}