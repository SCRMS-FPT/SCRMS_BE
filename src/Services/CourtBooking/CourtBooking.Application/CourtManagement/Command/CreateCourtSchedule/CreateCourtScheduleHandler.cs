using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;

namespace CourtBooking.Application.CourtManagement.Command.CreateCourtSchedule;

public class CreateCourtScheduleHandler : ICommandHandler<CreateCourtScheduleCommand, CreateCourtScheduleResult>
{
    private readonly ICourtRepository _courtRepository;
    private readonly ICourtScheduleRepository _courtScheduleRepository;

    public CreateCourtScheduleHandler(
        ICourtRepository courtRepository,
        ICourtScheduleRepository courtScheduleRepository)
    {
        _courtRepository = courtRepository;
        _courtScheduleRepository = courtScheduleRepository;
    }

    public async Task<CreateCourtScheduleResult> Handle(CreateCourtScheduleCommand request, CancellationToken cancellationToken)
    {
        var courtId = CourtId.Of(request.CourtId);
        var court = await _courtRepository.GetCourtByIdAsync(courtId, cancellationToken);
        if (court == null)
        {
            throw new NotFoundException($"Court with ID {request.CourtId} not found.");
        }

        var newScheduleId = CourtScheduleId.Of(Guid.NewGuid());
        var newSchedule = CourtSchedule.Create(
            newScheduleId,
            courtId,
            DayOfWeekValue.Of(request.DayOfWeek),
            request.StartTime,
            request.EndTime,
            request.PriceSlot
        );
        newSchedule.SetCreatedAt(DateTime.UtcNow);
        newSchedule.SetLastModified(DateTime.UtcNow);

        await _courtScheduleRepository.AddCourtScheduleAsync(newSchedule, cancellationToken);
        return new CreateCourtScheduleResult(newScheduleId.Value);
    }
}