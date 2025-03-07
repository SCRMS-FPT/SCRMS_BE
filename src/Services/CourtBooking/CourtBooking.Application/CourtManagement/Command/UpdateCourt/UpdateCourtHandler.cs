using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CourtBooking.Application.CourtManagement.Command.UpdateCourt;

public class UpdateCourtHandler(IApplicationDbContext _context)
    : IRequestHandler<UpdateCourtCommand, UpdateCourtResult>
{
    public async Task<UpdateCourtResult> Handle(UpdateCourtCommand request, CancellationToken cancellationToken)
    {
        var updatingCourtId = CourtId.Of(request.Court.Id);
        var court = await _context.Courts.FindAsync(updatingCourtId, cancellationToken);
        if (court == null)
        {
            throw new KeyNotFoundException("Court not found");
        }

        // Update court details
        court.UpdateCourt(
            CourtName.Of(request.Court.CourtName),
            SportId.Of(request.Court.SportId),
            request.Court.SlotDuration,
            request.Court.Description,
            JsonSerializer.Serialize(request.Court.Facilities, new JsonSerializerOptions { WriteIndented = true }),
            (CourtStatus)request.Court.Status,
            (CourtType)request.Court.CourtType
        );

        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateCourtResult(true);
    }
}
