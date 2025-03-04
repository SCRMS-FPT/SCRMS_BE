using Microsoft.EntityFrameworkCore;

namespace Coach.API.Data.Repositories
{
    public class CoachScheduleRepository : ICoachScheduleRepository
    {
        private readonly CoachDbContext _context;

        public CoachScheduleRepository(CoachDbContext context)
        {
            _context = context;
        }

        public async Task AddCoachScheduleAsync(CoachSchedule schedule, CancellationToken cancellationToken)
        {
            await _context.CoachSchedules.AddAsync(schedule, cancellationToken);
        }

        public async Task<CoachSchedule?> GetCoachScheduleByIdAsync(Guid scheduleId, CancellationToken cancellationToken)
        {
            return await _context.CoachSchedules.FirstOrDefaultAsync(s => s.Id == scheduleId, cancellationToken);
        }

        public async Task UpdateCoachScheduleAsync(CoachSchedule schedule, CancellationToken cancellationToken)
        {
            _context.CoachSchedules.Update(schedule);
            await Task.CompletedTask;
        }

        public async Task DeleteCoachScheduleAsync(CoachSchedule schedule, CancellationToken cancellationToken)
        {
            _context.CoachSchedules.Remove(schedule);
            await Task.CompletedTask;
        }

        public async Task<List<CoachSchedule>> GetCoachSchedulesByCoachIdAsync(Guid coachId, CancellationToken cancellationToken)
        {
            return await _context.CoachSchedules.Where(s => s.CoachId == coachId).ToListAsync(cancellationToken);
        }

        public async Task<bool> HasCoachScheduleConflictAsync(Guid coachId, int dayOfWeek, TimeOnly startTime, TimeOnly endTime, CancellationToken cancellationToken)
        {
            return await _context.CoachSchedules.AnyAsync(s =>
                s.CoachId == coachId &&
                s.DayOfWeek == dayOfWeek &&
                (
                    (startTime >= s.StartTime && startTime < s.EndTime) ||
                    (endTime > s.StartTime && endTime <= s.EndTime) ||
                    (startTime <= s.StartTime && endTime >= s.EndTime)
                ), cancellationToken);
        }
    }
}