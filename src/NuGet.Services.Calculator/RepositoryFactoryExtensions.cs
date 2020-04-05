using System;
using System.Linq;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Services.Calculator.Logic;

namespace NuGet.Services.Calculator
{
    public static class RepositoryFactoryExtensions
    {
        public static SourceRepository GetCustomRepository(
            this Repository.RepositoryFactory factory,
            IHttpCacheUtility httpCacheUtility,
            IConcurrencyUtility concurrencyUtility)
        {
            var providers = Repository
                .Provider
                .GetCoreV3()
                .Concat(new[]
                {
                        new Lazy<INuGetResourceProvider>(() => new CustomHttpHandlerResourceProvider()),
                        new Lazy<INuGetResourceProvider>(() => new CustomHttpSourceResourceProvider(
                            httpCacheUtility,
                            concurrencyUtility))
                });

            return Repository.CreateSource(providers, "https://api.nuget.org/v3/index.json");
        }
    }
}
