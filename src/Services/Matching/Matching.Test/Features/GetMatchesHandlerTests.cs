using Matching.API.Data.Repositories;
using Matching.API.Data.Models;
using Matching.API.Features.Matches.GetMatches;
using Moq;
using Match = Matching.API.Data.Models.Match;

namespace Matching.Test.Features
{
    public class GetMatchesHandlerTests
    {
        private readonly Mock<IMatchRepository> _matchRepoMock;
        private readonly GetMatchesHandler _handler;

        public GetMatchesHandlerTests()
        {
            _matchRepoMock = new Mock<IMatchRepository>();
            _handler = new GetMatchesHandler(_matchRepoMock.Object);
        }

        //[Fact]
        //public async Task Handle_ReturnsEmptyList_WhenNoMatches()
        //{
        //    _matchRepoMock.Setup(m => m.GetMatchesByUserIdAsync(It.IsAny<Guid>(), 1, 10, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(new List<Match>());

        //    var result = await _handler.Handle(new GetMatchesQuery(1, 10, Guid.NewGuid()), CancellationToken.None);

        //    Assert.Empty(result);
        //}

        [Fact]
        public async Task Handle_ReturnsCorrectMatches_WhenUserIsInitiator()
        {
            var userId = Guid.NewGuid();
            var matches = new List<Match> { new Match { Id = Guid.NewGuid(), InitiatorId = userId, MatchedUserId = Guid.NewGuid(), MatchTime = DateTime.UtcNow } };
            _matchRepoMock.Setup(m => m.GetMatchesByUserIdAsync(userId, 1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(matches);

            var result = await _handler.Handle(new GetMatchesQuery(1, 10, userId), CancellationToken.None);

            Assert.Single(result);
            Assert.Equal(matches[0].MatchedUserId, result[0].PartnerId);
        }

        [Fact]
        public async Task Handle_ReturnsCorrectMatches_WhenUserIsMatched()
        {
            var userId = Guid.NewGuid();
            var matches = new List<Match> { new Match { Id = Guid.NewGuid(), InitiatorId = Guid.NewGuid(), MatchedUserId = userId, MatchTime = DateTime.UtcNow } };
            _matchRepoMock.Setup(m => m.GetMatchesByUserIdAsync(userId, 1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(matches);

            var result = await _handler.Handle(new GetMatchesQuery(1, 10, userId), CancellationToken.None);

            Assert.Single(result);
            Assert.Equal(matches[0].InitiatorId, result[0].PartnerId);
        }
    }
}
