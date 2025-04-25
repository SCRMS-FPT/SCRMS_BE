using Coach.API.Data;
using Coach.API.Data.Models;
using Coach.API.Data.Repositories;
using Coach.API.Features.Coaches.GetCoaches;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Coach.API.Extensions;
using System.Text.Json;

namespace Coach.API.Features.Coaches.GetMyCoachProfile
{
    public record GetMyCoachProfileQuery(Guid UserId) : IQuery<CoachResponse>;

    public class GetMyCoachProfileQueryValidator : AbstractValidator<GetMyCoachProfileQuery>
    {
        public GetMyCoachProfileQueryValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required");
        }
    }

    public class GetMyCoachProfileQueryHandler : IQueryHandler<GetMyCoachProfileQuery, CoachResponse>
    {
        private readonly ICoachRepository _coachRepository;
        private readonly ICoachSportRepository _sportRepository;
        private readonly ICoachPackageRepository _packageRepository;
        private readonly ICoachScheduleRepository _scheduleRepository;

        public GetMyCoachProfileQueryHandler(
            ICoachRepository coachRepository,
            ICoachSportRepository sportRepository,
            ICoachPackageRepository packageRepository,
            ICoachScheduleRepository scheduleRepository)
        {
            _coachRepository = coachRepository;
            _sportRepository = sportRepository;
            _packageRepository = packageRepository;
            _scheduleRepository = scheduleRepository;
        }

        public async Task<CoachResponse> Handle(GetMyCoachProfileQuery query, CancellationToken cancellationToken)
        {
            var coach = await _coachRepository.GetCoachByIdAsync(query.UserId, cancellationToken);
            if (coach == null)
                throw new CoachNotFoundException(query.UserId);

            var sports = await _sportRepository.GetCoachSportsByCoachIdAsync(coach.UserId, cancellationToken);
            var packages = await _packageRepository.GetCoachPackagesByCoachIdAsync(coach.UserId, cancellationToken);

            // Get coach schedules
            var schedules = await _scheduleRepository.GetCoachSchedulesByCoachIdAsync(coach.UserId, cancellationToken);

            var sportIds = sports.Select(s => s.SportId).ToList();
            var packageResponses = packages.Select(p => new CoachPackageResponse(
                p.Id, p.Name, p.Description, p.Price, p.SessionCount)).ToList();

            // Convert schedules to response format - same as in GetCoachByIdQueryHandler
            var weeklyScheduleResponses = schedules.ToWeeklyScheduleResponses();

            // Parse image URLs correctly
            var imageUrls = new List<string>();
            if (!string.IsNullOrEmpty(coach.ImageUrls))
            {
                // Try parsing as JSON first (new format)
                try
                {
                    var jsonDocument = JsonDocument.Parse(coach.ImageUrls);
                    if (jsonDocument.RootElement.TryGetProperty("Images", out var imagesElement) &&
                        imagesElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var imageElement in imagesElement.EnumerateArray())
                        {
                            if (imageElement.ValueKind == JsonValueKind.String)
                            {
                                imageUrls.Add(imageElement.GetString());
                            }
                        }
                    }
                }
                catch (JsonException)
                {
                    // Fallback to old formats if JSON parsing fails
                    // First try splitting by pipe (|) character 
                    if (coach.ImageUrls.Contains('|'))
                    {
                        imageUrls = coach.ImageUrls.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();
                    }
                    // Also try comma (,) 
                    else if (coach.ImageUrls.Contains(','))
                    {
                        imageUrls = coach.ImageUrls.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                    }
                    else
                    {
                        // If no delimiters found but string is not empty, treat as a single URL
                        imageUrls.Add(coach.ImageUrls);
                    }
                }
            }

            return new CoachResponse(
                coach.UserId,
                coach.FullName,
                coach.Email,
                coach.Phone,
                coach.Avatar,
                imageUrls,
                sportIds,
                coach.Bio,
                coach.RatePerHour,
                coach.CreatedAt,
                packageResponses,
                weeklyScheduleResponses);
        }
    }
}