namespace Coach.API.Data.Repositories
{
    public interface ICoachRepository
    {
        Task AddCoachAsync(Models.Coach coach, CancellationToken cancellationToken);
        Task<Models.Coach?> GetCoachByIdAsync(Guid coachId, CancellationToken cancellationToken);
        Task UpdateCoachAsync(Models.Coach coach, CancellationToken cancellationToken);
        Task<bool> CoachExistsAsync(Guid coachId, CancellationToken cancellationToken);
        Task<List<Models.Coach>> GetAllCoachesAsync(CancellationToken cancellationToken);

        // Thêm phương thức mới
        Task SetCoachStatusAsync(Guid coachId, string status, CancellationToken cancellationToken);
    }
}