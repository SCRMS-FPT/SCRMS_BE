using Coach.API.Data.Models;

namespace Coach.API.Data.Repositories
{
    public interface ICoachScheduleRepository
    {
        Task AddCoachScheduleAsync(CoachSchedule schedule, CancellationToken cancellationToken);

        Task<CoachSchedule?> GetCoachScheduleByIdAsync(Guid scheduleId, CancellationToken cancellationToken);

        Task UpdateCoachScheduleAsync(CoachSchedule schedule, CancellationToken cancellationToken);

        Task DeleteCoachScheduleAsync(CoachSchedule schedule, CancellationToken cancellationToken);

        Task<List<CoachSchedule>> GetCoachSchedulesByCoachIdAsync(Guid coachId, CancellationToken cancellationToken);

        Task<bool> HasCoachScheduleConflictAsync(Guid coachId, int dayOfWeek, TimeOnly startTime, TimeOnly endTime, CancellationToken cancellationToken);

        Task<bool> HasCoachScheduleConflictExcludingCurrentAsync(Guid scheduleId, Guid coachId, int dayOfWeek, TimeOnly startTime, TimeOnly endTime, CancellationToken cancellationToken);
    }
}