using Matching.API.Data.Repositories;
using Matching.API.Features.Suggestions;
using Matching.API.Data.Models;
using Moq;

namespace Matching.Test.Features
{
    public class GetSuggestionsHandlerTests
    {
        private readonly Mock<IUserSkillRepository> _userSkillRepoMock;
        private readonly GetSuggestionsHandler _handler;

        public GetSuggestionsHandlerTests()
        {
            _userSkillRepoMock = new Mock<IUserSkillRepository>();
            _handler = new GetSuggestionsHandler(_userSkillRepoMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenNoSuggestions()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var filters = new List<SportSkillFilter>();

            _userSkillRepoMock.Setup(m => m.GetSuggestionsWithSkillsAsync(userId, 1, 10, filters, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<Guid, List<UserSkill>>());


            // Act
            var result = await _handler.Handle(new GetSuggestionsQuery(1, 10, userId, filters), CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task Handle_ReturnsUserProfiles_WhenSuggestionsExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var suggestedUser1 = Guid.NewGuid();
            var suggestedUser2 = Guid.NewGuid();
            var filters = new List<SportSkillFilter>();

            var mockData = new Dictionary<Guid, List<UserSkill>>
            {
                {
                    suggestedUser1,
                    new List<UserSkill>
                    {
                        new UserSkill { SportId = Guid.NewGuid(), SkillLevel = "Intermediate" }
                    }
                },
                {
                    suggestedUser2,
                    new List<UserSkill>
                    {
                        new UserSkill { SportId = Guid.NewGuid(), SkillLevel = "Advanced" }
                    }
                }
            };

            _userSkillRepoMock.Setup(m => m.GetSuggestionsWithSkillsAsync(userId, 1, 10, filters, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockData);

            // Act
            var result = await _handler.Handle(new GetSuggestionsQuery(1, 10, userId, filters), CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.Id == suggestedUser1);
            Assert.Contains(result, r => r.Id == suggestedUser2);
        }

        [Fact]
        public async Task Handle_RespectsPagination()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var filters = new List<SportSkillFilter>();

            var allSuggestions = new Dictionary<Guid, List<UserSkill>>
            {
                {
                    Guid.NewGuid(),
                    new List<UserSkill> { new UserSkill { SportId = Guid.NewGuid(), SkillLevel = "Beginner" } }
                },
                {
                    Guid.NewGuid(),
                    new List<UserSkill> { new UserSkill { SportId = Guid.NewGuid(), SkillLevel = "Advanced" } }
                },
                {
                    Guid.NewGuid(),
                    new List<UserSkill> { new UserSkill { SportId = Guid.NewGuid(), SkillLevel = "Intermediate" } }
                }
            };

            var pagedSuggestions = allSuggestions.Skip(1).Take(1)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            _userSkillRepoMock.Setup(m => m.GetSuggestionsWithSkillsAsync(userId, 2, 1, filters, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedSuggestions);

            // Act
            var result = await _handler.Handle(new GetSuggestionsQuery(2, 1, userId, filters), CancellationToken.None);

            // Assert
            Assert.Single(result);
            Assert.Equal(pagedSuggestions.First().Key, result[0].Id);
        }
    }
}

