using Microsoft.Extensions.DependencyInjection;
using SuperMediaR.Core.Interfaces;

namespace SuperMediaR;

public class SuperMediator : ISuperMediator
{
    private readonly IServiceProvider _serviceProvider;

    public SuperMediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
        dynamic handler = _serviceProvider.GetRequiredService(handlerType);
        return await handler.HandleAsync((dynamic)command, cancellationToken);
    }

    public async Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        dynamic handler = _serviceProvider.GetRequiredService(handlerType);
        return await handler.HandleAsync((dynamic)query, cancellationToken);
    }

    private Task<TResult> ExecutePipeline<TRequest, TResult>(TRequest request, CancellationToken cancellationToken)
    {
        var behaviors = _serviceProvider
            .GetServices<IPipelineBehavior<TRequest, TResult>>()
            .Reverse()
            .ToList();

        RequestHandlerDelegate<TResult> handler = async () =>
        {
            var handlerType = typeof(ICommandHandler<,>);
            if (typeof(IQuery<TResult>).IsAssignableFrom(typeof(TRequest)))
                handlerType = typeof(IQueryHandler<,>);

            var genericHandlerType = handlerType.MakeGenericType(typeof(TRequest), typeof(TResult));
            dynamic handlerInstance = _serviceProvider.GetRequiredService(genericHandlerType);
            return await handlerInstance.HandleAsync((dynamic)request, cancellationToken);
        };

        foreach (var behavior in behaviors)
        {
            var next = handler;
            handler = () => behavior.Handle(request, next, cancellationToken);
        }

        return handler();
    }

    public async Task PublishAsync(IEvent @event, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IEventHandler<>).MakeGenericType(@event.GetType());
        var handlers = _serviceProvider.GetServices(handlerType);

        foreach (object handler in handlers)
        {
            await ((dynamic)handler).HandleAsync((dynamic)@event, cancellationToken);
        }
    }
}