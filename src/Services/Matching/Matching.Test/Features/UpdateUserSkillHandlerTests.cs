using Matching.API.Data;
using Matching.API.Data.Models;
using Matching.API.Data.Repositories;
using Matching.API.Features.Skills.UpdateUserSkill;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Matching.Test.Features
{
    public class UpdateUserSkillHandlerTests
    {
        private readonly Mock<IUserSkillRepository> _skillRepoMock;
        private readonly Mock<MatchingDbContext> _contextMock;
        private readonly UpdateUserSkillHandler _handler;

        public UpdateUserSkillHandlerTests()
        {
            _skillRepoMock = new Mock<IUserSkillRepository>();
            var options = new DbContextOptionsBuilder<MatchingDbContext>()
                .UseInMemoryDatabase("TestDB")
                .Options;
            _contextMock = new Mock<MatchingDbContext>(options);
            _handler = new UpdateUserSkillHandler(_skillRepoMock.Object, _contextMock.Object);
        }

        [Fact]
        public async Task Handle_AddsNewSkill_WhenNotExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var sportId = Guid.NewGuid();
            _skillRepoMock.Setup(m => m.GetByUserIdAndSportIdAsync(userId, sportId, It.IsAny<CancellationToken>())).ReturnsAsync((UserSkill)null);
            _contextMock.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            await _handler.Handle(new UpdateUserSkillCommand(userId, sportId, "intermediate"), CancellationToken.None);

            // Assert
            _skillRepoMock.Verify(m => m.AddUserSkillAsync(It.Is<UserSkill>(us =>
                us.UserId == userId &&
                us.SportId == sportId &&
                us.SkillLevel == "intermediate"), It.IsAny<CancellationToken>()), Times.Once());
            _contextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task Handle_UpdatesExistingSkill()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var sportId = Guid.NewGuid();
            var skill = new UserSkill { UserId = userId, SportId = sportId, SkillLevel = "beginner" };
            _skillRepoMock.Setup(m => m.GetByUserIdAndSportIdAsync(userId, sportId, It.IsAny<CancellationToken>())).ReturnsAsync(skill);
            _contextMock.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            await _handler.Handle(new UpdateUserSkillCommand(userId, sportId, "advanced"), CancellationToken.None);

            // Assert
            Assert.Equal("advanced", skill.SkillLevel);
            _skillRepoMock.Verify(m => m.UpdateUserSkillAsync(skill, It.IsAny<CancellationToken>()), Times.Once());
            _skillRepoMock.Verify(m => m.AddUserSkillAsync(It.IsAny<UserSkill>(), It.IsAny<CancellationToken>()), Times.Never());
            _contextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }
    }
}

