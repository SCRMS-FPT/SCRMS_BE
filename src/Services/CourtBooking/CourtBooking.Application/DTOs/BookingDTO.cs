using System;
using System.Collections.Generic;

namespace CourtBooking.Application.DTOs
{
    public record BookingDTO(
        Guid Id,
        Guid UserId,
        Guid CourtId,
        DateTime BookingDate,
        TimeSpan StartTime,
        TimeSpan EndTime,
        decimal TotalPrice,
        int Status,
        string? Note,
        string CourtName,
        string SportCenterName,
        string SportName,
        DateTime CreatedAt,
        DateTime? LastModified
    );

    public record BookingDetailDTO(
        Guid Id,
        Guid UserId,
        Guid CourtId,
        DateTime BookingDate,
        TimeSpan StartTime,
        TimeSpan EndTime,
        decimal TotalPrice,
        int Status,
        string? Note,
        CourtDTO Court,
        string UserName,
        string UserPhone,
        DateTime CreatedAt,
        DateTime? LastModified
    );
}