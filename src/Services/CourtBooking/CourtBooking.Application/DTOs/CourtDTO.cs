using CourtBooking.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.DTOs
{
    public record CourtDTO(
    Guid Id,
    string CourtName,
    Guid SportId,
    Guid SportCenterId,
    string Description,
    List<FacilityDTO>? Facilities,
    TimeSpan SlotDuration,
    CourtStatus Status,
    CourtType CourtType,
    decimal MinDepositPercentage,
    string SportName,
    string? SportCenterName,
    DateTime CreatedAt,
    DateTime? LastModified
    );

    public record CourtCreateDTO(
    string CourtName,
    Guid SportId,
    Guid SportCenterId,
    string? Description,
    List<FacilityDTO>? Facilities,
    TimeSpan SlotDuration,
    decimal MinDepositPercentage,
    int CourtType,
    List<CourtScheduleDTO> CourtSchedules,
    int CancellationWindowHours = 24,
    decimal RefundPercentage = 0
    );

    public record CourtUpdateDTO(
    string CourtName,
    Guid SportId,
    string? Description,
    List<FacilityDTO>? Facilities,
    TimeSpan SlotDuration,
    int Status,
    int CourtType,
    decimal MinDepositPercentage,
    int CancellationWindowHours,
    decimal RefundPercentage
    );
}