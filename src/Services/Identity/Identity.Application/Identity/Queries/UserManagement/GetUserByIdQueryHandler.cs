﻿using Identity.Application.Data;
using Identity.Application.Data.Repositories;
using Mapster;

namespace Identity.Application.Identity.Queries.UserManagement
{
    public class GetUserByIdQueryHandler :
        IQueryHandler<GetUserByIdQuery, UserDto?>,
        IQueryHandler<GetUserProfileByIdQuery, UserProfileDto?>
    {
        private readonly IUserRepository _userRepository;

        public GetUserByIdQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // Handler for GetUserByIdQuery, returning UserDto
        public async Task<UserDto?> Handle(
            GetUserByIdQuery request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByIdAsync(request.UserId);
            if (user == null || user.IsDeleted)
            {
                return null;
            }

            var roles = await _userRepository.GetRolesAsync(user);
            var userDto = user.Adapt<UserDto>() with
            {
                Roles = roles.ToList(),
                ImageUrls = user.GetImageUrlsList()
            };
            return userDto;
        }

        // Handler for GetUserProfileByIdQuery, returning UserProfileDto
        public async Task<UserProfileDto?> Handle(
            GetUserProfileByIdQuery request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByIdAsync(request.UserId);
            if (user == null || user.IsDeleted)
            {
                return null;
            }

            // Lấy profile của người dùng và thêm danh sách ảnh
            var profileDto = user.Adapt<UserProfileDto>();

            // Tạo đối tượng mới với danh sách ảnh
            return profileDto with
            {
                ImageUrls = user.GetImageUrlsList()
            };
        }
    }
}