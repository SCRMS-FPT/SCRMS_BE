using Matching.API.Data.Repositories;
using Matching.Test.Helper;
using Match = Matching.API.Data.Models.Match;

namespace Matching.Test.Repositories
{
    public class MatchRepositoryTests : HandlerTestBase
    {
        private readonly MatchRepository _repository;

        public MatchRepositoryTests() : base()
        {
            _repository = new MatchRepository(Context);
        }

        [Fact]
        public async Task GetMatchesByUserIdAsync_ReturnsMatches()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var match1 = new Match { Id = Guid.NewGuid(), InitiatorId = userId, MatchedUserId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
            var match2 = new Match { Id = Guid.NewGuid(), InitiatorId = Guid.NewGuid(), MatchedUserId = userId, CreatedAt = DateTime.UtcNow };
            await Context.Matches.AddRangeAsync(match1, match2);
            await Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetMatchesByUserIdAsync(userId, 1, 2, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, m => m.Id == match1.Id);
            Assert.Contains(result, m => m.Id == match2.Id);
        }

        [Fact]
        public async Task GetMatchesByUserIdAsync_RespectsPagination()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var match1 = new Match { Id = Guid.NewGuid(), InitiatorId = userId, MatchedUserId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
            var match2 = new Match { Id = Guid.NewGuid(), InitiatorId = Guid.NewGuid(), MatchedUserId = userId, CreatedAt = DateTime.UtcNow };
            await Context.Matches.AddRangeAsync(match1, match2);
            await Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetMatchesByUserIdAsync(userId, 2, 1, CancellationToken.None);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task AddMatchAsync_AddsMatch()
        {
            // Arrange
            var match = new Match { Id = Guid.NewGuid(), InitiatorId = Guid.NewGuid(), MatchedUserId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };

            // Act
            await _repository.AddMatchAsync(match, CancellationToken.None);
            await Context.SaveChangesAsync();

            // Assert
            var addedMatch = await Context.Matches.FindAsync(match.Id);
            Assert.NotNull(addedMatch);
            Assert.Equal(match.Id, addedMatch.Id);
        }
    }
}
