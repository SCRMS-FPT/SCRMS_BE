using Matching.API.Data.Models;
using Matching.API.Data.Repositories;
using Matching.API.Features.Skills.GetUserSkills;
using Moq;

namespace Matching.Test.Features
{
    public class GetUserSkillsHandlerTests
    {
        private readonly Mock<IUserSkillRepository> _skillRepoMock;
        private readonly GetUserSkillsHandler _handler;

        public GetUserSkillsHandlerTests()
        {
            _skillRepoMock = new Mock<IUserSkillRepository>();
            _handler = new GetUserSkillsHandler(_skillRepoMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenNoSkills()
        {
            _skillRepoMock.Setup(m => m.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<UserSkill>());

            var result = await _handler.Handle(new GetUserSkillsQuery(Guid.NewGuid()), CancellationToken.None);

            Assert.Empty(result);
        }

        [Fact]
        public async Task Handle_ReturnsSingleSkill()
        {
            var userId = Guid.NewGuid();
            var skills = new List<UserSkill> { new UserSkill { UserId = userId, SportId = Guid.NewGuid(), SkillLevel = "beginner" } };
            _skillRepoMock.Setup(m => m.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(skills);

            var result = await _handler.Handle(new GetUserSkillsQuery(userId), CancellationToken.None);

            Assert.Single(result);
            Assert.Equal(skills[0].SportId, result[0].SportId);
        }

        [Fact]
        public async Task Handle_ReturnsMultipleSkills()
        {
            var userId = Guid.NewGuid();
            var skills = new List<UserSkill>
            {
                new UserSkill { UserId = userId, SportId = Guid.NewGuid(), SkillLevel = "beginner" },
                new UserSkill { UserId = userId, SportId = Guid.NewGuid(), SkillLevel = "advanced" }
            };
            _skillRepoMock.Setup(m => m.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(skills);

            var result = await _handler.Handle(new GetUserSkillsQuery(userId), CancellationToken.None);

            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Contains(skills, s => s.SportId == r.SportId));
        }
    }
}
