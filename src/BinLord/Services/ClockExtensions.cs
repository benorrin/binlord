using BinLord.Models;

namespace BinLord.Services;

/// <summary>
/// Computes "now"/"today" in the time zone configured in <see cref="AppSettings"/>,
/// rather than the server's local zone.
/// </summary>
public static class ClockExtensions
{
    public static DateTime GetNow(this AppSettings settings)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ResolveTimeZone(settings.TimeZoneId));
    }

    public static DateOnly GetToday(this AppSettings settings)
    {
        return DateOnly.FromDateTime(settings.GetNow());
    }

    private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.Utc;
        }
    }
}
