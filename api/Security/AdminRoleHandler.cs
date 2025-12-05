using MediaMatch.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;

namespace MediaMatch.Security
{
    public class AdminRoleHandler : AuthorizationHandler<AdminRoleRequirement>
    {
        private readonly MediaMatchContext _context;

        public AdminRoleHandler(MediaMatchContext context)
        {
            _context = context;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRoleRequirement requirement)
        {
            var idClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim)) return;
            if (!int.TryParse(idClaim, out var userId)) return;

            var roleName = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Include(ur => ur.Role)
                .Select(ur => ur.Role.Name)
                .FirstOrDefaultAsync();

            if (string.Equals(roleName, "Membro", StringComparison.OrdinalIgnoreCase)) roleName = "user";

            if (string.Equals(roleName, "admin", StringComparison.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
            }
        }
    }
}
