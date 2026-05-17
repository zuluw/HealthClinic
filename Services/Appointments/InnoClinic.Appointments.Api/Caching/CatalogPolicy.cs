using Microsoft.AspNetCore.OutputCaching;

namespace InnoClinic.Appointments.Api.Caching
{
    public sealed class CatalogPolicy(IConfiguration configuration) : IOutputCachePolicy
    {
        private readonly bool _cacheNonSuccess = configuration.GetValue<bool>("CacheSettings:CacheNonSuccessResponses");

        public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellationToken)
        {
            var request = context.HttpContext.Request;

            if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method))
            {
                context.EnableOutputCaching = false;
                context.AllowCacheLookup = false;
                return ValueTask.CompletedTask;
            }

            context.EnableOutputCaching = true;
            context.AllowCacheLookup = true;
            context.AllowLocking = true;

            context.CacheVaryByRules.QueryKeys = "*";

            return ValueTask.CompletedTask;
        }

        public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellationToken) 
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellationToken)
        {
            var response = context.HttpContext.Response;

            if (!_cacheNonSuccess && response.StatusCode != StatusCodes.Status200OK)
            {
                context.EnableOutputCaching = false;
            }

            return ValueTask.CompletedTask;
        }
    }
}
