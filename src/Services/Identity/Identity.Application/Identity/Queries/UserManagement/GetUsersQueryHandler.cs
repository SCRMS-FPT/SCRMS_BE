using Identity.Application.Data.Repositories;
using Identity.Application.Dtos;
using BuildingBlocks.Pagination;
using Mapster;

namespace Identity.Application.Identity.Queries.UserManagement
{
    public class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, PaginatedResult<UserDto>>
    {
        private readonly IUserRepository _userRepository;

        public GetUsersQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<PaginatedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            // Lấy toàn bộ users và loại bỏ những user bị xóa
            var users = await _userRepository.GetAllUserAsync();
            var activeUsers = users.Where(u => !u.IsDeleted).ToList();

            // Lọc theo từ khóa tìm kiếm nếu có (tìm trong firstName, lastName và email)
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                activeUsers = activeUsers.Where(u =>
                    ($"{u.FirstName} {u.LastName}").ToLower().Contains(searchLower)
                    || u.Email.ToLower().Contains(searchLower)
                ).ToList();
            }

            // Lọc theo role nếu có
            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                var filteredUsers = new List<Domain.Models.User>();
                foreach (var user in activeUsers)
                {
                    var roles = await _userRepository.GetRolesAsync(user);
                    if (roles.Any(r => r.Equals(request.Role, StringComparison.OrdinalIgnoreCase)))
                    {
                        filteredUsers.Add(user);
                    }
                }
                activeUsers = filteredUsers;
            }

            // Lấy tổng số kết quả sau khi lọc
            var totalCount = activeUsers.Count;

            // Phân trang dữ liệu
            var skip = request.PaginationRequest.PageIndex * request.PaginationRequest.PageSize;
            var pagedUsers = activeUsers.Skip(skip).Take(request.PaginationRequest.PageSize).ToList();

            // Chuyển đổi sang DTO kèm theo danh sách Roles cho mỗi user
            var userDtos = new List<UserDto>();
            foreach (var user in pagedUsers)
            {
                var roles = await _userRepository.GetRolesAsync(user);
                var userDto = new UserDto(
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
                    user.GetImageUrlsList()
                );
                userDtos.Add(userDto);
            }

            return new PaginatedResult<UserDto>(
                request.PaginationRequest.PageIndex,
                request.PaginationRequest.PageSize,
                totalCount,
                userDtos
            );
        }
    }
}