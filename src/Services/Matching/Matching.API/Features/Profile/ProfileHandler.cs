using Matching.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Matching.API.Features.Profile
{
    public record GetProfileQuery(Guid UserId) : IRequest<UserMatchInfoResponse>;

    public record UpdateProfileCommand(Guid UserId, string SelfIntroduction) : IRequest;

    public class ProfileHandler :
        IRequestHandler<GetProfileQuery, UserMatchInfoResponse>,
        IRequestHandler<UpdateProfileCommand>
    {
        private readonly MatchingDbContext _context;

        public ProfileHandler(MatchingDbContext context)
        {
            _context = context;
        }

        public async Task<UserMatchInfoResponse> Handle(GetProfileQuery request, CancellationToken cancellationToken)
        {
            var profile = await _context.UserMatchInfos
                .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);
            return profile != null ? new UserMatchInfoResponse(profile.SelfIntroduction) : null;
        }

        public async Task Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var profile = await _context.UserMatchInfos
                .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);

            if (profile == null)
            {
                profile = new UserMatchInfo { UserId = request.UserId, SelfIntroduction = request.SelfIntroduction };
                _context.UserMatchInfos.Add(profile);
            }
            else
            {
                profile.SelfIntroduction = request.SelfIntroduction;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public record UserMatchInfoResponse(string SelfIntroduction);
}