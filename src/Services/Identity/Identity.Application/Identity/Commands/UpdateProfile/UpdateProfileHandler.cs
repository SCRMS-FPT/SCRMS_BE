using Identity.Application.Data.Repositories;
using Identity.Application.Services;
using Identity.Domain.Exceptions;

namespace Identity.Application.Identity.Commands.UpdateProfile
{
    public sealed class UpdateProfileHandler : ICommandHandler<UpdateProfileCommand, UserDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IImageKitService _imageKitService;

        public UpdateProfileHandler(
            IUserRepository userRepository,
            IImageKitService imageKitService)
        {
            _userRepository = userRepository;
            _imageKitService = imageKitService;
        }

        public async Task<UserDto> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByIdAsync(command.UserId);
            if (user == null)
                throw new DomainException("User not found");

            // Update basic properties
            user.FirstName = command.FirstName;
            user.LastName = command.LastName;
            user.PhoneNumber = command.Phone;
            user.BirthDate = DateTime.SpecifyKind(command.BirthDate, DateTimeKind.Utc);
            user.Gender = Enum.Parse<Gender>(command.Gender, true);
            user.SelfIntroduction = command.SelfIntroduction;

            // Process images

            // 1. Get the current list of images
            var currentImages = user.GetImageUrlsList();

            // 2. Delete images that are requested to be removed
            if (command.ImagesToDelete != null && command.ImagesToDelete.Any())
            {
                foreach (var imageUrl in command.ImagesToDelete)
                {
                    if (currentImages.Contains(imageUrl))
                    {
                        currentImages.Remove(imageUrl);
                        await _imageKitService.DeleteFileAsync(imageUrl, cancellationToken);
                    }
                }
            }

            // 3. Keep the existing images (that were not deleted)
            var updatedImages = new List<string>();
            if (command.ExistingImageUrls != null)
            {
                foreach (var imageUrl in command.ExistingImageUrls)
                {
                    if (currentImages.Contains(imageUrl) &&
                        (command.ImagesToDelete == null || !command.ImagesToDelete.Contains(imageUrl)))
                    {
                        updatedImages.Add(imageUrl);
                    }
                }
            }

            // 4. Upload new avatar if provided and make it the first image
            if (command.NewAvatarFile != null)
            {
                // Upload avatar using ImageKit
                var avatarUrl = await _imageKitService.UploadFileAsync(
                    command.NewAvatarFile,
                    $"users/{user.Id}/avatar",
                    cancellationToken);

                // Add avatar as the first image in the list
                updatedImages.Insert(0, avatarUrl);
            }

            // 5. Upload other new images
            if (command.NewImageFiles != null && command.NewImageFiles.Any())
            {
                var newImageUrls = await _imageKitService.UploadFilesAsync(
                    command.NewImageFiles,
                    $"users/{user.Id}/images",
                    cancellationToken);

                updatedImages.AddRange(newImageUrls);
            }

            // 6. Update the user's image list
            user.SetImageUrlsList(updatedImages);
            user.LastModifiedAt = DateTime.UtcNow;

            var result = await _userRepository.UpdateUserAsync(user);
            if (!result.Succeeded)
            {
                throw new DomainException($"Failed to update profile: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            // Retrieve roles to include in the response
            var roles = await _userRepository.GetRolesAsync(user);

            return new UserDto(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                user.BirthDate,
                user.Gender.ToString(),
                user.SelfIntroduction,
                user.CreatedAt,
                roles.ToList(),
                updatedImages
            );
        }
    }
}
