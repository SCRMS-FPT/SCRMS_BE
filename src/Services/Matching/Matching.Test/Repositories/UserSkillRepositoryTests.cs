using Matching.API.Data.Models;
using Matching.API.Data.Repositories;
using Matching.Test.Helper;

namespace Matching.Test.Repositories
{
    public class UserSkillRepositoryTests : HandlerTestBase
    {
        private readonly UserSkillRepository _repository;

        public UserSkillRepositoryTests() : base()
        {
            _repository = new UserSkillRepository(Context);
        }

        [Fact]
        public async Task GetByUserIdAndSportIdAsync_ReturnsSkill_WhenExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var sportId = Guid.NewGuid();
            var skill = new UserSkill { UserId = userId, SportId = sportId, SkillLevel = "beginner" };
            await Context.UserSkills.AddAsync(skill);
            await Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByUserIdAndSportIdAsync(userId, sportId, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("beginner", result.SkillLevel);
        }

        [Fact]
        public async Task GetByUserIdAndSportIdAsync_ReturnsNull_WhenNotExists()
        {
            // Act
            var result = await _repository.GetByUserIdAndSportIdAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsSkills()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var skills = new List<UserSkill>
            {
                new UserSkill { UserId = userId, SportId = Guid.NewGuid(), SkillLevel = "beginner" },
                new UserSkill { UserId = userId, SportId = Guid.NewGuid(), SkillLevel = "advanced" }
            };
            await Context.UserSkills.AddRangeAsync(skills);
            await Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByUserIdAsync(userId, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task AddUserSkillAsync_AddsSkill()
        {
            // Arrange
            var skill = new UserSkill { UserId = Guid.NewGuid(), SportId = Guid.NewGuid(), SkillLevel = "beginner" };

            // Act
            await _repository.AddUserSkillAsync(skill, CancellationToken.None);
            await Context.SaveChangesAsync();

            // Assert
            var addedSkill = await Context.UserSkills.FindAsync(skill.UserId, skill.SportId);
            Assert.NotNull(addedSkill);
            Assert.Equal("beginner", addedSkill.SkillLevel);
        }

        [Fact]
        public async Task UpdateUserSkillAsync_UpdatesSkill()
        {
            // Arrange
            var skill = new UserSkill { UserId = Guid.NewGuid(), SportId = Guid.NewGuid(), SkillLevel = "beginner" };
            await Context.UserSkills.AddAsync(skill);
            await Context.SaveChangesAsync();
            skill.SkillLevel = "advanced";

            // Act
            await _repository.UpdateUserSkillAsync(skill, CancellationToken.None);
            await Context.SaveChangesAsync();

            // Assert
            var updatedSkill = await Context.UserSkills.FindAsync(skill.UserId, skill.SportId);
            Assert.Equal("advanced", updatedSkill.SkillLevel);
        }

        [Fact]
        public async Task GetSuggestionUserIdsAsync_ReturnsFilteredUsers()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var swipedUserId = Guid.NewGuid();
            await Context.SwipeActions.AddAsync(new SwipeAction { SwiperId = userId, SwipedUserId = swipedUserId, Decision = "Test decision" });
            var userSkills = new List<UserSkill>
            {
                new UserSkill { UserId = Guid.NewGuid(), SportId = Guid.NewGuid(), SkillLevel = "intermediate" },
                new UserSkill { UserId = Guid.NewGuid(), SportId = Guid.NewGuid(), SkillLevel = "beginner" }
            };
            await Context.UserSkills.AddRangeAsync(userSkills);
            await Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetSuggestionUserIdsAsync(userId, 1, 10, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, id => Assert.NotEqual(swipedUserId, id));
        }
    }
}
