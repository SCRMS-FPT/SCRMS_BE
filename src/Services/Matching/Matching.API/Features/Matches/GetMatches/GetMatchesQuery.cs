using Matching.API.Data;
using Matching.API.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Matching.API.Features.Matches.GetMatches
{
    public record GetMatchesQuery(int Page, int Limit, Guid UserId) : IRequest<List<MatchResponse>>;

    public class GetMatchesHandler : IRequestHandler<GetMatchesQuery, List<MatchResponse>>
    {
        private readonly IMatchRepository _matchRepository;

        public GetMatchesHandler(IMatchRepository matchRepository)
        {
            _matchRepository = matchRepository;
        }

        public async Task<List<MatchResponse>> Handle(GetMatchesQuery request, CancellationToken cancellationToken)
        {
            var matches = await _matchRepository.GetMatchesByUserIdAsync(request.UserId, request.Page, request.Limit, cancellationToken);
            return matches.Select(m => new MatchResponse
            {
                Id = m.Id,
                PartnerId = m.InitiatorId == request.UserId ? m.MatchedUserId : m.InitiatorId,
                MatchTime = m.MatchTime
            }).ToList();
        }
    }

    public class MatchResponse
    {
        public Guid Id { get; set; }
        public Guid PartnerId { get; set; }
        public DateTime MatchTime { get; set; }
    }
}