using Application.Services.Interface;
using Domain.Constants;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public int UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                return userIdClaim != null ? int.Parse(userIdClaim) : 0;
            }
        }
        public bool IsAdmin
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                return user != null && (user.IsInRole(RolesConst.ADMIN_ROLE) || user.IsInRole(RolesConst.SUPER_ADMIN_ROLE));
            }
        }
    }
}