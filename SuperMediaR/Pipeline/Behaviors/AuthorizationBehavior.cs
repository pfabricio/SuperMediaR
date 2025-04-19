using SuperMediaR.Core.Authorization;
using SuperMediaR.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using SuperMediaR.Core.Extensions;

namespace SuperMediaR.Pipeline.Behaviors;

public class AuthorizationBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
{
    private readonly IUserContext _userContext;
    private readonly IServiceProvider _serviceProvider;

    public AuthorizationBehavior(IUserContext userContext, IServiceProvider serviceProvider)
    {
        _userContext = userContext;
        _serviceProvider = serviceProvider;
    }

    public async Task<TResult> Handle(
        TRequest request,
        RequestHandlerDelegate<TResult> next,
        CancellationToken cancellationToken)
    {
        // 🔐 Autorizações baseadas em role
        var authorizeAttributes = request
            .GetType()
            .GetCustomAttributes<AuthorizeAttribute>();

        foreach (var attr in authorizeAttributes)
        {
            if (!_userContext.User.HasRole(attr.Role))
                throw new UnauthorizedAccessException($"Access denied. Required role: {attr.Role}");
        }

        // ⚖️ Regras customizadas
        var ruleType = typeof(IAuthorizationRule<>).MakeGenericType(request.GetType());
        var rules = _serviceProvider.GetServices(ruleType);

        foreach (var rule in rules)
        {
            var isAuthorized = (Task<bool>)ruleType
                .GetMethod("IsAuthorizedAsync")!
                .Invoke(rule, new object[] { request, cancellationToken })!;

            if (!await isAuthorized)
                throw new UnauthorizedAccessException($"Access denied by custom rule: {rule.GetType().Name}");
        }

        return await next();
    }
}