using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.ValueObjects;
using FluentValidation;
using System.Linq;

namespace CourtBooking.Application.CourtManagement.Command.DeleteSportCenter;

public class DeleteSportCenterHandler : ICommandHandler<DeleteSportCenterCommand, DeleteSportCenterResult>
{
    private readonly ISportCenterRepository _sportCenterRepository;
    private readonly ICourtRepository _courtRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ICourtScheduleRepository _courtScheduleRepository;
    private readonly ICourtPromotionRepository _courtPromotionRepository;

    public DeleteSportCenterHandler(
        ISportCenterRepository sportCenterRepository,
        ICourtRepository courtRepository,
        IBookingRepository bookingRepository,
        ICourtScheduleRepository courtScheduleRepository,
        ICourtPromotionRepository courtPromotionRepository)
    {
        _sportCenterRepository = sportCenterRepository;
        _courtRepository = courtRepository;
        _bookingRepository = bookingRepository;
        _courtScheduleRepository = courtScheduleRepository;
        _courtPromotionRepository = courtPromotionRepository;
    }

    public async Task<DeleteSportCenterResult> Handle(DeleteSportCenterCommand command, CancellationToken cancellationToken)
    {
        var sportCenterId = SportCenterId.Of(command.SportCenterId);
        var sportCenter = await _sportCenterRepository.GetSportCenterByIdAsync(sportCenterId, cancellationToken);

        if (sportCenter == null)
        {
            throw new NotFoundException($"Sport center with ID {command.SportCenterId} not found.");
        }

        // Get all courts associated with this sport center
        var courts = await _courtRepository.GetCourtsBySportCenterIdAsync(sportCenterId, cancellationToken);

        // Check if any courts have active bookings
        foreach (var court in courts)
        {
            var activeBookings = await _bookingRepository.GetActiveBookingsForCourtAsync(
                court.Id,
                new[] { BookingStatus.Deposited, BookingStatus.Completed },
                DateTime.UtcNow,
                cancellationToken);

            if (activeBookings.Any())
            {
                throw new ValidationException("Không thể xóa trung tâm thể thao vì có lịch đặt sân còn hoạt động.");
            }
        }

        // Delete all courts and their related entities
        foreach (var court in courts)
        {
            // Delete court promotions
            var promotions = await _courtPromotionRepository.GetPromotionsByCourtIdAsync(court.Id, cancellationToken);
            foreach (var promotion in promotions)
            {
                await _courtPromotionRepository.DeleteAsync(promotion.Id, cancellationToken);
            }

            // Delete court schedules
            var schedules = await _courtScheduleRepository.GetSchedulesByCourtIdAsync(court.Id, cancellationToken);
            foreach (var schedule in schedules)
            {
                await _courtScheduleRepository.DeleteCourtScheduleAsync(schedule.Id, cancellationToken);
            }

            // Delete the court
            await _courtRepository.DeleteCourtAsync(court.Id, cancellationToken);
        }

        // Delete the sport center
        await _sportCenterRepository.DeleteSportCenterAsync(sportCenterId, cancellationToken);

        return new DeleteSportCenterResult(true);
    }
}