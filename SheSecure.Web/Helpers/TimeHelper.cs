namespace SheSecure.Web.Helpers
{
    public static class TimeHelper
    {
        public static DateTime ToIST(string timeStr)
        {
            if (string.IsNullOrEmpty(timeStr)) 
                return DateTime.UtcNow.AddHours(5).AddMinutes(30);

            var dt = DateTime.Parse(timeStr);
            if (dt.Kind == DateTimeKind.Unspecified)
            {
                dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            }
            else
            {
                dt = dt.ToUniversalTime();
            }

            try {
                // For Windows
                var tz = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(dt, tz);
            } catch {
                // Fallback
                return dt.AddHours(5).AddMinutes(30);
            }
        }
    }
}
