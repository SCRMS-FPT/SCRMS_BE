using MediatR;
using CourtBooking.Application.DTOs;

namespace CourtBooking.Application.SportManagement.Queries.GetSports;

public record GetSportsQuery : IRequest<GetSportsResult>;

public record GetSportsResult(List<SportDTO> Sports);
