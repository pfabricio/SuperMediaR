using System.Security.Claims;

namespace SuperMediaR.Core.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool HasRole(this ClaimsPrincipal user, string role)
        {
            return user?.IsInRole(role) ?? false;
        }

        public static bool HasAnyRole(this ClaimsPrincipal user, params string[] roles)
        {
            return roles.Any(role => user?.IsInRole(role) ?? false);
        }

        public static string? GetUserId(this ClaimsPrincipal user)
        {
            return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}