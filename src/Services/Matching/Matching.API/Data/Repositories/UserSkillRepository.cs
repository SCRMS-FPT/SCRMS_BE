using Microsoft.EntityFrameworkCore;
using Matching.API.Data.Models;

namespace Matching.API.Data.Repositories
{
    public class UserSkillRepository : IUserSkillRepository
    {
        private readonly MatchingDbContext _context;

        public UserSkillRepository(MatchingDbContext context)
        {
            _context = context;
        }

        public async Task<UserSkill?> GetByUserIdAndSportIdAsync(Guid userId, Guid sportId, CancellationToken cancellationToken)
        {
            return await _context.UserSkills.FirstOrDefaultAsync(us => us.UserId == userId && us.SportId == sportId, cancellationToken);
        }

        public async Task<List<UserSkill>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.UserSkills.Where(us => us.UserId == userId).ToListAsync(cancellationToken);
        }

        public async Task AddUserSkillAsync(UserSkill userSkill, CancellationToken cancellationToken)
        {
            await _context.UserSkills.AddAsync(userSkill, cancellationToken);
        }

        public Task UpdateUserSkillAsync(UserSkill userSkill, CancellationToken cancellationToken)
        {
            _context.UserSkills.Update(userSkill);
            return Task.CompletedTask;
        }

        public async Task<List<Guid>> GetSuggestionUserIdsAsync(Guid userId, int page, int limit, CancellationToken cancellationToken)
        {
            // Lấy danh sách ID người dùng đã bị swipe
            var swipedUserIds = await _context.SwipeActions
                .Where(sa => sa.SwiperId == userId)
                .Select(sa => sa.SwipedUserId)
                .ToListAsync(cancellationToken);

            // Lấy danh sách ID người dùng chưa bị swipe, không phải userId, và phân trang
            var suggestionUserIds = await _context.UserSkills
                .Where(us => us.UserId != userId && !swipedUserIds.Contains(us.UserId))
                .Select(us => us.UserId)
                .Distinct() // Loại bỏ trùng lặp
                .Skip((page - 1) * limit) // Bỏ qua các bản ghi trước đó
                .Take(limit) // Lấy số bản ghi theo limit
                .ToListAsync(cancellationToken);

            return suggestionUserIds;
        }
    }
}