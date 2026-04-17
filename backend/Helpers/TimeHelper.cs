namespace backend.Helpers
{
    public static class TimeHelper
    {
        private static readonly TimeZoneInfo PhilippineTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");

        public static DateTime GetPhilippineTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PhilippineTimeZone);
        }
    }
}