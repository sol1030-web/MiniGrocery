namespace MiniGrocery.Web.Utils;

public static class DateTimeExtensions
{
    private static readonly TimeZoneInfo PhTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");

    public static DateTime ToPhTime(this DateTime dt)
    {
        var utc = dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, PhTimeZone);
    }
}
