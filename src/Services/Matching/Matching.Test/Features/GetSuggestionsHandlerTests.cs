using Matching.API.Data.Repositories;
using Matching.API.Features.Suggestions;
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
            _userSkillRepoMock.Setup(m => m.GetSuggestionUserIdsAsync(userId, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Guid>());

            // Act
            var result = await _handler.Handle(new GetSuggestionsQuery(1, 10, userId), CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task Handle_ReturnsUserProfiles_WhenSuggestionsExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var suggestionIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            _userSkillRepoMock.Setup(m => m.GetSuggestionUserIdsAsync(userId, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(suggestionIds);

            // Act
            var result = await _handler.Handle(new GetSuggestionsQuery(1, 10, userId), CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, profile => Assert.Contains(profile.Id, suggestionIds));
        }

        [Fact]
        public async Task Handle_RespectsPagination()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var suggestionIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            _userSkillRepoMock.Setup(m => m.GetSuggestionUserIdsAsync(userId, 2, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(suggestionIds.Skip(1).Take(1).ToList());

            // Act
            var result = await _handler.Handle(new GetSuggestionsQuery(2, 1, userId), CancellationToken.None);

            // Assert
            Assert.Single(result);
            Assert.Equal(suggestionIds[1], result[0].Id);
        }
    }
}
