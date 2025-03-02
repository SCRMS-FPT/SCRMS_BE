using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CourtBooking.Application.CourtManagement.Command.UpdateCourt;

public class UpdateCourtHandler(IApplicationDbContext _context)
    : IRequestHandler<UpdateCourtCommand, UpdateCourtResult>
{
    public async Task<UpdateCourtResult> Handle(UpdateCourtCommand request, CancellationToken cancellationToken)
    {
        var updatingCourtId = CourtId.Of(request.Court.Id);
        var court = await _context.Courts.FindAsync([updatingCourtId], cancellationToken);
        if (court == null)
        {
            throw new KeyNotFoundException("Court not found");
        }
        var facilitiesJson = JsonSerializer.Serialize(court.Facilities, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        //var location = Location.Of(request.Court.Address.Address, request.Court.Address.Commune, request.Court.Address.District, request.Court.Address.City);
        //court.UpdateCourt(
        //    new CourtName(request.Court.CourtName),
        //    SportId.Of(request.Court.SportId),
        //    location,
        //    request.Court.Description,
        //    facilitiesJson,
        //    request.Court.PricePerHour,
        //    CourtStatus.Open
        //);

        //court.ClearOperatingHours();
        //foreach (var hour in request.Court.OperatingHours)
        //{
        //    court.AddOperatingHour(CourtOperatingHour.Create(
        //        CourtOperatingHourId.Of(Guid.NewGuid()),
        //        court.Id,
        //        hour.OpenTime, 
        //        hour.CloseTime
        //    ));
        //}

        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateCourtResult(true);
    }
}
