namespace CarSharing.Extensions
{
    internal static class DateTimeExtension
    {
        public static DateTime RoundSeconds(this DateTime self)
        {
            return new DateTime(self.Year, self.Month, self.Day, self.Hour, self.Minute, self.Second);
        }
    }
}
