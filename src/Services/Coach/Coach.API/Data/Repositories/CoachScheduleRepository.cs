using Coach.API.Data.Models;
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
            var errorMessages = new List<string>();

            // Validate CoachId
            if (schedule.CoachId == Guid.Empty)
            {
                errorMessages.Add("CoachId is required");
            }

            // Validate DayOfWeek
            if (schedule.DayOfWeek < 1 || schedule.DayOfWeek > 7) // Assuming DayOfWeek is between 1 and 7
            {
                errorMessages.Add("DayOfWeek is required and must be between 1 and 7");
            }

            // If there are any validation errors, throw an exception
            if (errorMessages.Any())
            {
                throw new ArgumentException(string.Join(", ", errorMessages));
            }

            // Proceed with the original logic if validation is successful
            await _context.CoachSchedules.AddAsync(schedule, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
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

        public async Task<bool> HasCoachScheduleConflictExcludingCurrentAsync(Guid scheduleId, Guid coachId, int dayOfWeek, TimeOnly startTime, TimeOnly endTime, CancellationToken cancellationToken)
        {
            return await _context.CoachSchedules.AnyAsync(s =>
                s.CoachId == coachId &&
                s.DayOfWeek == dayOfWeek &&
                s.Id != scheduleId && // Exclude the current schedule being updated
                (
                    (startTime >= s.StartTime && startTime < s.EndTime) ||
                    (endTime > s.StartTime && endTime <= s.EndTime) ||
                    (startTime <= s.StartTime && endTime >= s.EndTime)
                ), cancellationToken);
        }
    }
}