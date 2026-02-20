namespace Aco228.Runners.Models.Timings;

public enum DelayType
{
    None = -1,
    Seconds = 1,
    Minutes = 2,
    Hours = 3,
    Days = 4,
}

public static class DelayTypeExtension
{
    public static DateTime CreateUTCDelayFrom(this DelayType type, int delayValue)
    {
        if (type == DelayType.Seconds)
            return DateTime.UtcNow.AddSeconds(delayValue);
        if (type == DelayType.Minutes)
            return DateTime.UtcNow.AddMinutes(delayValue);
        if (type == DelayType.Hours)
            return DateTime.UtcNow.AddHours(delayValue).AddMinutes(-2);
        if (type == DelayType.Days)
            return DateTime.UtcNow.AddDays(delayValue).AddHours(-2);
        throw new ArgumentException($"Unknown delay type");
    }
}