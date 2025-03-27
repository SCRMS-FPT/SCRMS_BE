using Coach.API.Data.Models;
using Coach.API.Features.Coaches.GetCoaches;

namespace Coach.API.Extensions
{
    public static class CoachScheduleExtensions
    {
        public static List<CoachWeeklyScheduleResponse> ToWeeklyScheduleResponses(this List<CoachSchedule> schedules)
        {
            return schedules.Select(s =>
            {
                string dayName = s.DayOfWeek switch
                {
                    1 => "Sunday",
                    2 => "Monday",
                    3 => "Tuesday",
                    4 => "Wednesday",
                    5 => "Thursday",
                    6 => "Friday",
                    7 => "Saturday",
                    _ => "Unknown"
                };

                return new CoachWeeklyScheduleResponse(
                    s.DayOfWeek,
                    dayName,
                    s.StartTime.ToString("HH:mm:ss"),
                    s.EndTime.ToString("HH:mm:ss"),
                    s.Id
                );
            }).ToList();
        }
    }
}