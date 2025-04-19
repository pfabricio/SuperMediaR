using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SuperMediaR;
using SuperMediaR.Core.Interfaces;
using SuperMediaR.Pipeline.Behaviors;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSuperMediaR(this IServiceCollection services)
    {
        // Core
        services.AddScoped<ISuperMediator, SuperMediator>();

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionScopeBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(EventDispatcherBehavior<,>));

        services.Scan(scan => scan
            .FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
            .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(c => c.AssignableTo(typeof(IEventHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(c => c.AssignableTo(typeof(IValidator<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(c => c.AssignableTo(typeof(IAuthorizationRule<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}

