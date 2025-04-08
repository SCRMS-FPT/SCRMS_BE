using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CourtBooking.Application.CourtManagement.Command.UpdateCourt;

public class UpdateCourtHandler : IRequestHandler<UpdateCourtCommand, UpdateCourtResult>
{
    private readonly ICourtRepository _courtRepository;

    public UpdateCourtHandler(ICourtRepository courtRepository)
    {
        _courtRepository = courtRepository;
    }

    public async Task<UpdateCourtResult> Handle(UpdateCourtCommand request, CancellationToken cancellationToken)
    {
        var courtId = CourtId.Of(request.Id);
        var court = await _courtRepository.GetCourtByIdAsync(courtId, cancellationToken);
        if (court == null)
        {
            throw new KeyNotFoundException("Court not found");
        }

        court.UpdateCourt(
            CourtName.Of(request.Court.CourtName),
            SportId.Of(request.Court.SportId),
            request.Court.SlotDuration,
            request.Court.Description,
            JsonSerializer.Serialize(request.Court.Facilities),
            (CourtStatus)request.Court.Status,
            (CourtType)request.Court.CourtType,
            request.Court.MinDepositPercentage,
            request.Court.CancellationWindowHours,
            request.Court.RefundPercentage
        );

        await _courtRepository.UpdateCourtAsync(court, cancellationToken);
        return new UpdateCourtResult(true);
    }
}