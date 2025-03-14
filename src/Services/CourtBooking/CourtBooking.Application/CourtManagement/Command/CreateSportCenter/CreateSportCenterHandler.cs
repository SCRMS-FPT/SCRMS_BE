using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.DTOs;
using CourtBooking.Application.SportCenterManagement.Commands.CreateSportCenter;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace CourtBooking.Application.CourtManagement.Command.CreateSportCenter
{
    public class CreateSportCenterHandler : ICommandHandler<CreateSportCenterCommand, CreateSportCenterResult>
    {
        private readonly ISportCenterRepository _sportCenterRepository;

        public CreateSportCenterHandler(ISportCenterRepository sportCenterRepository)
        {
            _sportCenterRepository = sportCenterRepository;
        }

        public async Task<CreateSportCenterResult> Handle(CreateSportCenterCommand command, CancellationToken cancellationToken)
        {
            var newId = SportCenterId.Of(Guid.NewGuid());
            var sportCenter = SportCenter.Create(
                id: newId,
                ownerId: OwnerId.Of(command.OwnerId),
                name: command.Name,
                phoneNumber: command.PhoneNumber,
                address: Location.Of(command.Location.AddressLine, command.Location.Commune, command.Location.District, command.Location.City),
                location: command.LocationPoint,
                images: command.Images,
                description: command.Description
            );

            foreach (var court in command.Courts)
            {
                var newCourtId = CourtId.Of(Guid.NewGuid());
                var facilitiesJson = JsonSerializer.Serialize(court.Facilities);
                sportCenter.AddCourt(Court.Create(newCourtId, CourtName.Of(court.CourtName), newId, SportId.Of(court.SportId), court.SlotDuration, court.Description, facilitiesJson, court.CourtType));
            }

            await _sportCenterRepository.AddSportCenterAsync(sportCenter, cancellationToken);
            return new CreateSportCenterResult(sportCenter.Id.Value);
        }
    }
}