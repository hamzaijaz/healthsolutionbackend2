using System;
using System.Collections.Generic;
using System.Text;

namespace MyHealthSolution.Service.Application.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime? ToAEST(this DateTime? inputTime)
        {
            if (!inputTime.HasValue)
                return null;

            return ToAEST(inputTime);
        }

        public static DateTime ToAEST(this DateTime inputTime)
        {
            TimeZoneInfo aestZone;
            try
            {
                aestZone = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                aestZone = TimeZoneInfo.FindSystemTimeZoneById("Australia/Melbourne");
            }
            return TimeZoneInfo.ConvertTimeFromUtc(inputTime, aestZone);
        }
    }
}
