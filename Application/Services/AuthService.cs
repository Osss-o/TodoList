using Application.Dtos.Auth;
using Application.Repositories.Interface;
using Application.Services.Interface;
using Domain.Constants;
using Domain.Entities;
using Domain.Entities.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<RefreshToken> _refreshTokenRepo;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(
            IGenericRepository<User> userRepo,
            IGenericRepository<RefreshToken> refreshTokenRepo,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _userRepo = userRepo;
            _refreshTokenRepo = refreshTokenRepo;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto input)
        {
            var user = await _userRepo.GetAll()
                .FirstOrDefaultAsync(u => u.Email.Trim().ToLower() == input.Username.Trim().ToLower()
               );
            if (user == null)
            {
                return null;
            }

            var passwordHasher = new PasswordHasher<User>();
            var passwordRusult = passwordHasher.VerifyHashedPassword(user, user.Password, input.Password);

            if (passwordRusult == PasswordVerificationResult.Failed)
            {
                return null;
            }
            var jwtSection = _configuration.GetSection("Jwt");
            int accessTokenMinutes = jwtSection.GetValue<int>("AccessTokenMinutes");
            int refreshDays = jwtSection.GetValue<int>("RefreshTokenDays");

            var refreshToken = GenerateRefreshToken();
            var accessToken = GenerateAccessToken(user, accessTokenMinutes);

            await _refreshTokenRepo.Insert(new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(refreshDays)
            });
            await _refreshTokenRepo.SaveChanges();

            return new LoginResponseDto
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Role = user.Role == RoleEnum.Admin ? RolesConst.ADMIN_ROLE : RolesConst.USER_ROLE
            };
        }
        public string GenerateAccessToken(User user, int accessTokenMinutes)
        {
            var jwtSection = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role == RoleEnum.Admin
                ? RolesConst.ADMIN_ROLE : RolesConst.USER_ROLE),
            };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(accessTokenMinutes),
                Issuer = jwtSection["Issuer"],
                Audience = jwtSection["Audience"],
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            };
            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }
        public string GenerateRefreshToken()
        {
            var random = new byte[64];
            RandomNumberGenerator.Fill(random);
            return Convert.ToBase64String(random);
        }

        public async Task<LoginResponseDto> RefreshToken(string refreshToken)
        {
            if (!string.IsNullOrEmpty(refreshToken))
            {
                refreshToken = refreshToken.Trim('"');
            }

            var storedToken = await _refreshTokenRepo.GetAll()
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.ExpiryDate > DateTime.UtcNow);


            if (storedToken == null)
            {
                return null;
            }

            var user = storedToken.User;

            var jwtSection = _configuration.GetSection("Jwt");
            int accessTokenMinutes = jwtSection.GetValue<int>("AccessTokenMinutes");
            int refreshTokenDays = jwtSection.GetValue<int>("RefreshTokenDays");

            var newAccessToken = GenerateAccessToken(user, accessTokenMinutes);
            var newRefreshToken = GenerateRefreshToken();

            storedToken.Token = newRefreshToken;
            storedToken.ExpiryDate = DateTime.UtcNow.AddDays(refreshTokenDays);

            _refreshTokenRepo.Update(storedToken);
            await _refreshTokenRepo.SaveChanges();

            return new LoginResponseDto
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Role = user.Role == RoleEnum.Admin ? RolesConst.ADMIN_ROLE : RolesConst.USER_ROLE,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            };

        }

        public async Task ResetPassword(int userId, string newpassword)
        {
            var user = await _userRepo.GetById(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            if (user.Role == RoleEnum.Admin)
                throw new UnauthorizedAccessException("Cannot reset password for admin users");

            var passwordHasher = new PasswordHasher<User>();
            user.Password = passwordHasher.HashPassword(user, newpassword);

            _userRepo.Update(user);
            await _userRepo.SaveChanges();
        }

        public async Task ChangePassword(ChangePasswordDto input)
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = Convert.ToInt32(userIdClaim);
            var user = await _userRepo.GetById(userId);

            if (user == null)
                throw new KeyNotFoundException("User not found.");
            var passwordHasher = new PasswordHasher<User>();
            var passwordResult = passwordHasher.VerifyHashedPassword(user, user.Password, input.CurrentPassword);

            if (passwordResult == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Current password is incorrect.");
            user.Password = passwordHasher.HashPassword(user, input.NewPassword);


            _userRepo.Update(user);
            await _userRepo.SaveChanges();
        }

    }
}
