using Application.Dtos.Auth;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Interface
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto input);
        Task ChangePassword(ChangePasswordDto input);
        Task ResetPassword(int usetId,string newpassword);
        Task <string>RefreshToken(string refreshToken);
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}
