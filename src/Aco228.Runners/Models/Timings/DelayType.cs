namespace Aco228.Runners.Models.Timings;

public enum DelayType
{
    None = -1,
    Minutes = 1,
    Hours = 2,
    Days = 3,
}

public static class DelayTypeExtension
{
    public static DateTime CreateUTCDelayFrom(this DelayType type, int delayValue)
    {
        if (type == DelayType.Minutes)
            return DateTime.UtcNow.AddMinutes(delayValue);
        if (type == DelayType.Hours)
            return DateTime.UtcNow.AddHours(delayValue).AddMinutes(-2);
        if (type == DelayType.Days)
            return DateTime.UtcNow.AddDays(delayValue).AddHours(-2);
        throw new ArgumentException($"Unknown delay type");
    }
}