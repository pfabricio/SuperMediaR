namespace SuperMediaR.Core.Interfaces;

public interface IAuthorizationRule<in TRequest>
{
    Task<bool> IsAuthorizedAsync(TRequest request, CancellationToken cancellationToken);
}