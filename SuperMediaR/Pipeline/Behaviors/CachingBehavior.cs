using Microsoft.Extensions.Caching.Memory;
using SuperMediaR.Core.Interfaces;

namespace SuperMediaR.Pipeline.Behaviors
{
    public class CachingBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
        where TRequest : IQuery<TResult>
    {
        private readonly IMemoryCache _cache;

        public CachingBehavior(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<TResult> Handle(
            TRequest request,
            RequestHandlerDelegate<TResult> next,
            CancellationToken cancellationToken)
        {
            if (request is not ICachableQuery<TResult> cachable)
                return await next();

            if (_cache.TryGetValue(cachable.CacheKey, out TResult response))
                return response;

            response = await next();

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cachable.Expiration ?? TimeSpan.FromMinutes(5)
            };

            _cache.Set(cachable.CacheKey, response, options);

            return response;
        }
    }
}