

namespace EmployeeManagementSys.BL
{
    internal interface IAuthenticationManager
    {
        Task<APIResult<LoginResponseDto>> LoginAsync(LoginDto loginDto);
        Task<APIResult<ResetPasswordResponseDto>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<APIResult<RefreshTokenResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
    }
}
