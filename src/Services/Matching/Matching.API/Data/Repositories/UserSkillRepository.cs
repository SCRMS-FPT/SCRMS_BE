using Microsoft.EntityFrameworkCore;
using Matching.API.Data.Models;
using Matching.API.Features.Suggestions;
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

        public async Task<Dictionary<Guid, List<UserSkill>>> GetSuggestionsWithSkillsAsync(
    Guid userId,
    int page,
    int limit,
    List<SportSkillFilter> sportSkillFilters,
    CancellationToken cancellationToken)
        {
            // Lấy danh sách ID người dùng đã bị swipe
            var swipedUserIds = await _context.SwipeActions
                .Where(sa => sa.SwiperId == userId)
                .Select(sa => sa.SwipedUserId)
                .ToListAsync(cancellationToken);

            // Tạo query cơ bản
            var query = _context.UserSkills
                .Where(us => us.UserId != userId && !swipedUserIds.Contains(us.UserId));

            // Nếu có bộ lọc, áp dụng chúng
            if (sportSkillFilters != null && sportSkillFilters.Count > 0)
            {
                // Tạo một danh sách các tài khoản người dùng phù hợp với các môn thể thao và kỹ năng
                var userIdsMatchingFilters = await _context.UserSkills
                    .Where(us => sportSkillFilters.Any(
                        filter => us.SportId == filter.SportId &&
                                  string.Equals(us.SkillLevel, filter.SkillLevel, StringComparison.OrdinalIgnoreCase)))
                    .Select(us => us.UserId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                // Lọc các người dùng có ít nhất một kỹ năng phù hợp
                query = query.Where(us => userIdsMatchingFilters.Contains(us.UserId));
            }

            // Lấy danh sách người dùng phân trang
            var userIds = await query
                .Select(us => us.UserId)
                .Distinct()
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync(cancellationToken);

            // Nếu không có người dùng nào, trả về dictionary rỗng
            if (!userIds.Any())
                return new Dictionary<Guid, List<UserSkill>>();

            // Lấy tất cả kỹ năng của các người dùng được chọn
            var allSkills = await _context.UserSkills
                .Where(us => userIds.Contains(us.UserId))
                .ToListAsync(cancellationToken);

            // Nhóm kỹ năng theo người dùng
            var result = allSkills
                .GroupBy(us => us.UserId)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToList());

            return result;
        }
    }
}