using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace NuGet.Services.Calculator.Services
{
    public class VersionRangeCalculator
    {
        private readonly Task<FindPackageByIdResource> _resourceTask;

        public VersionRangeCalculator(Task<FindPackageByIdResource> resourceTask)
        {
            _resourceTask = resourceTask ?? throw new ArgumentNullException(nameof(resourceTask));
        }

        public async Task<IEnumerable<NuGetVersion>> GetVersionsAsync(string id)
        {
            using (var cache = new SourceCacheContext())
            {
                ILogger logger = NullLogger.Instance;
                CancellationToken cancellationToken = CancellationToken.None;
                var resource = await _resourceTask;
                return await resource.GetAllVersionsAsync(id, cache, logger, cancellationToken);
            }
        }
    }
}
