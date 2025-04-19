using System.Security.Claims;

namespace SuperMediaR.Core.Interfaces
{
    public interface IUserContext
    {
        ClaimsPrincipal User { get; }
    }
}