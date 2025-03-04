using CourtBooking.Application.DTOs;
using CourtBooking.Application.SportCenterManagement.Commands.CreateSportCenter;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace CourtBooking.Application.CourtManagement.Command.CreateSportCenter
{
public class CreateSportCenterHandler(IApplicationDbContext _context) 
        : ICommandHandler<CreateSportCenterCommand, CreateSportCenterResult>
{
    public async Task<CreateSportCenterResult> Handle(CreateSportCenterCommand command, CancellationToken cancellationToken)
    {
            //SportCenterId id, Guid ownerId, string name, string phoneNumber,
            //Location address, GeoLocation location, SportCenterImages images, 
            //    string description
            var newId = SportCenterId.Of(Guid.NewGuid());
            var sportCenter = SportCenter.Create
        (
            id: newId,
            ownerId: OwnerId.Of(command.OwnerId),
            name: command.Name,
            phoneNumber: command.PhoneNumber,
            address: Location.Of(command.Location.AddressLine,command.Location.Commune,command.Location.District,
            command.Location.City),
            location: command.LocationPoint,
            images: command.Images,
            description: command.Description
        );
            //foreach (var court in command.Courts)
            //{
            //    sportCenter.AddCourt(Court.Create(CourtName.Of(court.CourtName), newId,
            //        court.SportId, court.SlotDuration,
            //        court.Description, court.Facilities));
            //}


            _context.SportCenters.Add(sportCenter);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateSportCenterResult(sportCenter.Id.Value);
    }
}
    
}
