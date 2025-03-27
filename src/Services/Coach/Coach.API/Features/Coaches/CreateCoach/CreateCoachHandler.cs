using BuildingBlocks.Exceptions;
using Coach.API.Data;
using Coach.API.Data.Repositories;
using Coach.API.Services;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Features.Coaches.CreateCoach
{
    public record CreateCoachCommand(
        Guid UserId,
        string FullName,
        string Email,
        string Phone,
        IFormFile? AvatarFile,
        List<IFormFile> ImageFiles,
        string Bio,
        decimal RatePerHour,
        List<Guid> SportIds
    ) : ICommand<CreateCoachResult>;

    public record CreateCoachResult(
        Guid Id,
        string FullName,
        string AvatarUrl,
        List<string> ImageUrls,
        DateTime CreatedAt,
        List<Guid> SportIds);

    public class CreateCoachCommandValidator : AbstractValidator<CreateCoachCommand>
    {
        public CreateCoachCommandValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(255);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
            RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Bio).NotEmpty();
            RuleFor(x => x.RatePerHour).GreaterThan(0);
            RuleFor(x => x.SportIds).NotEmpty().WithMessage("At least one sport required");
        }
    }

    public class CreateCoachCommandHandler : ICommandHandler<CreateCoachCommand, CreateCoachResult>
    {
        private readonly ICoachRepository _coachRepository;
        private readonly ICoachSportRepository _sportRepository;
        private readonly IImageKitService _imageKitService;
        private readonly CoachDbContext _context;

        public CreateCoachCommandHandler(
            ICoachRepository coachRepository,
            ICoachSportRepository sportRepository,
            IImageKitService imageKitService,
            CoachDbContext context)
        {
            _coachRepository = coachRepository;
            _sportRepository = sportRepository;
            _imageKitService = imageKitService;
            _context = context;
        }

        public async Task<CreateCoachResult> Handle(
            CreateCoachCommand command,
            CancellationToken cancellationToken)
        {
            var exists = await _coachRepository.CoachExistsAsync(command.UserId, cancellationToken);
            if (exists)
                throw new AlreadyExistsException("Coach", command.UserId);

            // Upload avatar if provided
            string avatarUrl = string.Empty;
            if (command.AvatarFile != null)
            {
                avatarUrl = await _imageKitService.UploadFileAsync(
                    command.AvatarFile,
                    $"coaches/{command.UserId}/avatar",
                    cancellationToken);
            }

            // Upload all image files
            var imageUrls = await _imageKitService.UploadFilesAsync(
                command.ImageFiles,
                $"coaches/{command.UserId}/images",
                cancellationToken);

            var coach = new Data.Models.Coach
            {
                UserId = command.UserId,
                FullName = command.FullName,
                Email = command.Email,
                Phone = command.Phone,
                Avatar = avatarUrl,
                Bio = command.Bio,
                RatePerHour = command.RatePerHour,
                CreatedAt = DateTime.UtcNow
            };

            // Set image URLs using the helper method
            coach.SetImageUrlsList(imageUrls);

            var coachSports = command.SportIds.Select(sportId => new CoachSport
            {
                CoachId = coach.UserId,
                SportId = sportId,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _coachRepository.AddCoachAsync(coach, cancellationToken);
            foreach (var sport in coachSports)
            {
                await _sportRepository.AddCoachSportAsync(sport, cancellationToken);
            }
            await _context.SaveChangesAsync(cancellationToken);

            return new CreateCoachResult(
                coach.UserId,
                coach.FullName,
                coach.Avatar,
                coach.GetImageUrlsList(),
                coach.CreatedAt,
                coachSports.Select(s => s.SportId).ToList());
        }
    }
}