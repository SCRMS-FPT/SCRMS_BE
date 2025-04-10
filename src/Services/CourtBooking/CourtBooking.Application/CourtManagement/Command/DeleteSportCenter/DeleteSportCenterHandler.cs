// DeleteSportCenterHandler.cs
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CourtBooking.Application.CourtManagement.Commands.DeleteSportCenter
{
    public class DeleteSportCenterHandler : ICommandHandler<DeleteSportCenterCommand, DeleteSportCenterResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public DeleteSportCenterHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DeleteSportCenterResult> Handle(DeleteSportCenterCommand request, CancellationToken cancellationToken)
        {
            var sportCenterId = SportCenterId.Of(request.SportCenterId);
            var sportCenter = await _dbContext.SportCenters
                .Include(sc => sc.Courts)
                .FirstOrDefaultAsync(sc => sc.Id == sportCenterId, cancellationToken);

            if (sportCenter == null)
            {
                throw new NotFoundException($"Sport center with ID {request.SportCenterId} not found.");
            }

            // Check if the sport center has any bookings (past or future)
            bool hasBookings = CanDeleteSportCenter(sportCenter, allBookings)

            if (hasBookings)
            {
                // Mark all courts as closed
                foreach (var court in sportCenter.Courts)
                {
                    court.CloseForMaintenance("Sport center was deactivated by admin");
                    court.SetLastModified(DateTime.UtcNow);
                }

                // Mark sport center as inactive (we're assuming we need to add this status to the SportCenter model)
                sportCenter.Deactivate();
                sportCenter.SetLastModified(DateTime.UtcNow);

                await _dbContext.SaveChangesAsync(cancellationToken);

                return new DeleteSportCenterResult(
                    Success: true,
                    Message: "Sport center has existing bookings and was deactivated instead of deleted.",
                    WasDeactivated: true);
            }
            else
            {
                // No bookings, safe to delete entirely
                // First remove all courts and related entities
                _dbContext.CourtSchedules.RemoveRange(
                    sportCenter.Courts.SelectMany(c => _dbContext.CourtSchedules.Where(cs => cs.CourtId == c.Id)));

                _dbContext.CourtPromotions.RemoveRange(
                    sportCenter.Courts.SelectMany(c => _dbContext.CourtPromotions.Where(cp => cp.CourtId == c.Id)));

                _dbContext.Courts.RemoveRange(sportCenter.Courts);

                // Then remove the sport center
                _dbContext.SportCenters.Remove(sportCenter);

                await _dbContext.SaveChangesAsync(cancellationToken);

                return new DeleteSportCenterResult(
                    Success: true,
                    Message: "Sport center and all related data have been permanently deleted.",
                    WasDeactivated: false);
            }
        }
        public bool CanDeleteSportCenter(SportCenter sportCenter)
        {
            var courtIds = sportCenter.Courts.Select(c => c.Id);

            var hasBookings = dbContext.Bookings
                .SelectMany(b => b.BookingDetails)
                .Any(bd => courtIds.Contains(bd.CourtId));

            return !hasBookings;
        }
    }
}
