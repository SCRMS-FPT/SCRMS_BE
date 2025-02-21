using MediatR;
using CourtBooking.Application.DTOs;

public record GetCourtDetailsQuery(Guid CourtId) : IQuery<GetCourtDetailsResult>;

public record GetCourtDetailsResult(CourtDTO Court);
