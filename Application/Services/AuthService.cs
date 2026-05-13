using Application.Dtos.Auth;
using Application.Repositories.Interface;
using Application.Services.Interface;
using Application.Specifications;
using Domain.Entities;
using Domain.Entities.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<RefreshToken> _refreshTokenRepo;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGenerateAccessToken _generateAccessToken;

        public AuthService(
            IGenericRepository<User> userRepo,
            IGenericRepository<RefreshToken> refreshTokenRepo,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IGenerateAccessToken generateAccessToken)
        {
            _userRepo = userRepo;
            _refreshTokenRepo = refreshTokenRepo;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _generateAccessToken = generateAccessToken;
        }
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto input)
        {
            var spec = new SpecificationBuilder<User>()
                .Where(u=>u.Email.ToLower() == input.Username.Trim().ToLower())
                .Build();
            var user = await _userRepo.GetEntityWithSpec(spec);
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
            var accessToken = _generateAccessToken.AccessTokenGenerator(user, accessTokenMinutes);

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
                Role = user.Role.ToString()
            };
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
            var spec = new SpecificationBuilder<RefreshToken>()
                .Where(rt=>rt.Token ==refreshToken&&rt.ExpiryDate > DateTime.UtcNow )
                .Include(rt => rt.User)
                .Build();
          var storedToken = await _refreshTokenRepo.GetEntityWithSpec(spec);

            if (storedToken == null)
            {
                return null;
            }

            var user = storedToken.User;

            var jwtSection = _configuration.GetSection("Jwt");
            int accessTokenMinutes = jwtSection.GetValue<int>("AccessTokenMinutes");
            int refreshTokenDays = jwtSection.GetValue<int>("RefreshTokenDays");

            var newAccessToken = _generateAccessToken.AccessTokenGenerator(user, accessTokenMinutes);
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
                Role = user.Role.ToString(),
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            };

        }

        public async Task ResetPassword(int userId, string newpassword)
        {
            var user = await _userRepo.GetById(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            if (user.Role == RoleEnum.Admin || user.Role == RoleEnum.SuperAdmin)
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
