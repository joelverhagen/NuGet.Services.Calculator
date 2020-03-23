using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace NuGet.Services.Calculator.Logic
{
    public class CustomHttpSourceResourceProvider : ResourceProvider
    {
        private readonly ConcurrentDictionary<PackageSource, HttpSourceResource> _cache
            = new ConcurrentDictionary<PackageSource, HttpSourceResource>();

        private readonly IHttpCacheUtility _httpCacheUtility;
        private readonly IConcurrencyUtility _concurrencyUtility;

        public CustomHttpSourceResourceProvider(
            IHttpCacheUtility httpCacheUtility,
            IConcurrencyUtility concurrencyUtility) : base(
                typeof(HttpSourceResource),
                nameof(CustomHttpSourceResourceProvider),
                nameof(HttpSourceResource))
        {
            _httpCacheUtility = httpCacheUtility ?? throw new ArgumentNullException(nameof(httpCacheUtility));
            _concurrencyUtility = concurrencyUtility ?? throw new ArgumentNullException(nameof(concurrencyUtility));
        }

        public override Task<Tuple<bool, INuGetResource>> TryCreate(SourceRepository source, CancellationToken token)
        {
            HttpSourceResource curResource = null;

            if (source.PackageSource.IsHttp)
            {
                curResource = _cache.GetOrAdd(
                    source.PackageSource,
                    packageSource => new HttpSourceResource(HttpSource.Create(
                        source,
                        NullThrottle.Instance,
                        _httpCacheUtility,
                        _concurrencyUtility)));
            }

            return Task.FromResult(new Tuple<bool, INuGetResource>(curResource != null, curResource));
        }
    }
}
