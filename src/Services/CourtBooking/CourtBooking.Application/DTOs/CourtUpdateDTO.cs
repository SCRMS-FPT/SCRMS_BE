using CourtBooking.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.DTOs;

public record CourtUpdateDTO(
    Guid Id,
    string CourtName,
    Guid SportId,
    string Description,
    List<FacilityDTO> Facilities,
    TimeSpan SlotDuration,
    int Status
    //List<CourtScheduleDTO> CourtSlots
    );
