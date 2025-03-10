using System;

namespace CourtBooking.Application.DTOs;

public record SportDTO(
    Guid Id,
    string Name,
    string Description,
    string Icon
);
