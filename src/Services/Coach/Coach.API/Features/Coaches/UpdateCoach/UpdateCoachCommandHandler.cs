using Coach.API.Data;
using Coach.API.Data.Repositories;
using Coach.API.Services;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Features.Coaches.UpdateCoach
{
    public record UpdateCoachCommand(
        Guid CoachId,
        string FullName,
        string Email,
        string Phone,
        IFormFile? NewAvatarFile,
        List<IFormFile> NewImageFiles,
        List<string> ExistingImageUrls,
        List<string> ImagesToDelete,
        string Bio,
        decimal RatePerHour,
        List<Guid> SportIds) : ICommand<Unit>;

    public class UpdateCoachCommandValidator : AbstractValidator<UpdateCoachCommand>
    {
        public UpdateCoachCommandValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(255);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
            RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Bio).NotEmpty().WithMessage("Bio is required");
            RuleFor(x => x.CoachId).NotEmpty().WithMessage("Coach id is required");
            RuleFor(x => x.RatePerHour).GreaterThan(0).WithMessage("Rate per hour must greater than 0");
        }
    }

    public class UpdateCoachCommandHandler : ICommandHandler<UpdateCoachCommand, Unit>
    {
        private readonly CoachDbContext _context;
        private readonly IImageKitService _imageKitService; // Changed from IBackblazeService
        private readonly ICoachRepository _coachRepository;

        public UpdateCoachCommandHandler(
            CoachDbContext context,
            IImageKitService imageKitService, // Changed from IBackblazeService
            ICoachRepository coachRepository)
        {
            _context = context;
            _imageKitService = imageKitService;
            _coachRepository = coachRepository;
        }

        public async Task<Unit> Handle(
            UpdateCoachCommand command,
            CancellationToken cancellationToken)
        {
            var coach = await _context.Coaches.Include(p => p.CoachSports)
                .FirstOrDefaultAsync(c => c.UserId == command.CoachId, cancellationToken);

            if (coach == null)
            {
                throw new CoachNotFoundException(command.CoachId);
            }

            // 1. Update avatar if a new one is provided
            if (command.NewAvatarFile != null)
            {
                // Delete old avatar if it exists
                if (!string.IsNullOrEmpty(coach.Avatar))
                {
                    await _imageKitService.DeleteFileAsync(coach.Avatar, cancellationToken);
                }

                // Upload new avatar using ImageKit
                var avatarUrl = await _imageKitService.UploadFileAsync(
                    command.NewAvatarFile,
                    $"coaches/{coach.UserId}/avatar",
                    cancellationToken);

                coach.Avatar = avatarUrl;
            }

            // 2. Handle images to delete
            var currentImages = coach.GetImageUrlsList();
            foreach (var imageUrl in command.ImagesToDelete)
            {
                if (currentImages.Contains(imageUrl))
                {
                    currentImages.Remove(imageUrl);
                    await _imageKitService.DeleteFileAsync(imageUrl, cancellationToken);
                }
            }

            // 3. Add existing images (that weren't deleted)
            var updatedImages = new List<string>();
            foreach (var imageUrl in command.ExistingImageUrls)
            {
                if (currentImages.Contains(imageUrl) && !command.ImagesToDelete.Contains(imageUrl))
                {
                    updatedImages.Add(imageUrl);
                }
            }

            // 4. Upload new images using ImageKit
            if (command.NewImageFiles.Any())
            {
                var newImageUrls = await _imageKitService.UploadFilesAsync(
                    command.NewImageFiles,
                    $"coaches/{coach.UserId}/images",
                    cancellationToken);

                updatedImages.AddRange(newImageUrls);
            }

            // Update basic coach properties
            coach.FullName = command.FullName;
            coach.Email = command.Email;
            coach.Phone = command.Phone;
            coach.Bio = command.Bio;
            coach.RatePerHour = command.RatePerHour;

            // Update image URLs using the helper method
            coach.SetImageUrlsList(updatedImages);

            // 5. Handle sports
            // Remove old sports
            var listOldSport = coach.CoachSports.ToList();
            _context.CoachSports.RemoveRange(listOldSport);

            // Add new sports
            var listNewSport = command.SportIds.Select(p => new CoachSport
            {
                CoachId = coach.UserId,
                SportId = p,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _context.CoachSports.AddRangeAsync(listNewSport, cancellationToken);

            // Save all changes
            await _coachRepository.UpdateCoachAsync(coach, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}