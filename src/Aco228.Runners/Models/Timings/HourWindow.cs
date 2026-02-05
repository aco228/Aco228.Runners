using Aco228.Common.Extensions;

namespace Aco228.Runners.Models.Timings;

public class HourWindow
{
    public int Hour { get; set; }
    public int Minute { get; set; } = 0;

    public static HourWindow DayStart => new(0);
    public static HourWindow DayEnd => new(23,59);

    public HourWindow () { }
    public HourWindow (int hour, int minute = 0)
    {
        Hour = hour;
        Minute = minute;
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
    public static bool IsTimeOkay(HourWindow from, HourWindow to, DateTime currentTime)
    {
        return (from.Hour == currentTime.Hour && currentTime.Minute > from.Minute || currentTime.Hour > from.Hour)
               && (currentTime.Hour == to.Hour && currentTime.Minute < to.Minute || currentTime.Hour < to.Hour);
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