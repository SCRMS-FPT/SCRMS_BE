using Coach.API.Data.Repositories;
using BuildingBlocks.Exceptions;
using Coach.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Features.Coaches.RestoreCoach
{
    public class RestoreCoachHandler : ICommandHandler<RestoreCoachCommand, bool>
    {
        private readonly ICoachRepository _coachRepository;
        private readonly CoachDbContext _context;

        public RestoreCoachHandler(ICoachRepository coachRepository, CoachDbContext context)
        {
            _coachRepository = coachRepository;
            _context = context;
        }

        public async Task<bool> Handle(RestoreCoachCommand request, CancellationToken cancellationToken)
        {
            // Tìm coach bằng ID, bao gồm cả các coach đã bị xóa mềm
            var coach = await _context.Coaches
                .FirstOrDefaultAsync(c => c.UserId == request.CoachId, cancellationToken);

            if (coach == null)
                throw new NotFoundException("Coach", request.CoachId);

            if (coach.Status != "deleted")
                throw new BadRequestException("Coach is not in deleted state");

            await _coachRepository.SetCoachStatusAsync(request.CoachId, "active", cancellationToken);
            return true;
        }
    }
}