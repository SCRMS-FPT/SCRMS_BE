namespace Coach.API.Data.Repositories
{
    public interface ICoachBookingRepository
    {
        Task AddCoachBookingAsync(CoachBooking booking, CancellationToken cancellationToken);

        Task<CoachBooking?> GetCoachBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken);

        Task UpdateCoachBookingAsync(CoachBooking booking, CancellationToken cancellationToken);

        Task<List<CoachBooking>> GetCoachBookingsByCoachIdAsync(Guid coachId, CancellationToken cancellationToken);

        Task<List<CoachBooking>> GetCoachBookingsByUserIdAsync(Guid userId, CancellationToken cancellationToken);

        Task<bool> HasOverlappingCoachBookingAsync(Guid coachId, DateOnly bookingDate, TimeOnly startTime, TimeOnly endTime, CancellationToken cancellationToken);
        IQueryable<CoachBooking> GetCoachBookingsByUserIdQueryable(Guid userId);

        IQueryable<CoachBooking> GetCoachBookingsByCoachIdQueryable(Guid coachId); // Thêm phương thức mới
    }
}