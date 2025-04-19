﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SuperMediaR.Core.Interfaces;

namespace SuperMediaR.Pipeline.Behaviors
{
    public class EventDispatcherBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EventDispatcherBehavior<TRequest, TResponse>> _logger;

        public EventDispatcherBehavior(IServiceProvider serviceProvider, ILogger<EventDispatcherBehavior<TRequest, TResponse>> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Chama o próximo handler (caso seja uma query ou comando)
            var response = await next();

            // Verifica se o request gerou algum evento
            var events = ExtractEvents(request);

            foreach (var @event in events)
            {
                // Despacha o evento
                await DispatchEvent(@event, cancellationToken);
            }

            return response;
        }

        private IEnumerable<IEvent> ExtractEvents(TRequest request)
        {
            // Aqui você extrai os eventos do request, se houver
            // Caso seu request tenha uma propriedade de eventos ou esteja configurado para gerar eventos
            if (request is IEventSource eventSource)
            {
                return eventSource.Events;
            }

            return Enumerable.Empty<IEvent>();
        }

        private async Task DispatchEvent(IEvent @event, CancellationToken cancellationToken)
        {
            var handlerType = typeof(IEventHandler<>).MakeGenericType(@event.GetType());
            var handlers = _serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                try
                {
                    await ((dynamic)handler).HandleAsync((dynamic)@event, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling event: {EventType}", @event.GetType().Name);
                }
            }
        }
    }
}
