

namespace EmployeeManagementSys.BL
{
    public interface IAuthenticationManager
    {
        Task<APIResult<LoginResponseDto>> LoginAsync(LoginDto loginDto);
        Task<APIResult<ResetPasswordResponseDto>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<APIResult<RefreshTokenResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
        Task<APIResult<bool>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        Task<APIResult<bool>> ResetPasswordWithTokenAsync(ResetPasswordWithTokenDto resetDto);
    }
}
