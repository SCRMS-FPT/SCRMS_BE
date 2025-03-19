using MediatR;
using CourtBooking.Application.DTOs;

namespace CourtBooking.Application.CourtManagement.Queries.GetCourtDetails;

public record GetCourtDetailsQuery(Guid CourtId) : IQuery<GetCourtDetailsResult>;

public record GetCourtDetailsResult(CourtDTO Court);