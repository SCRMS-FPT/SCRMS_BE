using Coach.API.Data.Models;
using Coach.API.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Features.Schedules.GetMySchedules
{
    // Query definition
    public record GetMySchedulesQuery(Guid CoachId) : IQuery<CoachSchedulesListResponse>;

    // Response model
    public record CoachSchedulesListResponse(List<CoachScheduleDto> Schedules);

    public record CoachScheduleDto(
        Guid Id,
        int DayOfWeek,
        string StartTime,
        string EndTime,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    public class GetMySchedulesQueryHandler : IQueryHandler<GetMySchedulesQuery, CoachSchedulesListResponse>
    {
        private readonly ICoachScheduleRepository _scheduleRepository;

        public GetMySchedulesQueryHandler(ICoachScheduleRepository scheduleRepository)
        {
            _scheduleRepository = scheduleRepository;
        }

        public async Task<CoachSchedulesListResponse> Handle(GetMySchedulesQuery query, CancellationToken cancellationToken)
        {
            var schedules = await _scheduleRepository.GetCoachSchedulesByCoachIdAsync(query.CoachId, cancellationToken);

            var scheduleDtos = schedules.Select(s => new CoachScheduleDto(
                s.Id,
                s.DayOfWeek,
                s.StartTime.ToString("HH:mm"),
                s.EndTime.ToString("HH:mm"),
                s.CreatedAt,
                s.UpdatedAt
            )).ToList();

            return new CoachSchedulesListResponse(scheduleDtos);
        }
    }
}