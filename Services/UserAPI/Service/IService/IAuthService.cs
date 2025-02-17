using UserAPI.Models.Dto;

namespace UserAPI.Service.IService
{
    public interface IAuthService
    {
        Task<string> Register(RegistrationRequestDto registrationRequestDto);
        Task<LoginResponseDto> Login(UserAPI.Models.Dto.LoginRequestDto loginRequestDto);
        Task<bool> AssignRole(string email, string roleName);
    }
}
