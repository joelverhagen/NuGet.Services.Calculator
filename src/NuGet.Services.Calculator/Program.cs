using System;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Services.Calculator.Services;

namespace NuGet.Services.Calculator
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddTransient<IHttpCacheUtility, LocalStorageHttpCacheUtility>();
            builder.Services.AddSingleton<IConcurrencyUtility, InMemoryConcurrencyUtilities>();
            builder.Services.AddTransient<VersionRangeCalculator>();
            builder.Services.AddSingleton(p =>
            {
                var providers = Repository
                    .Provider
                    .GetCoreV3()
                    .Concat(new[]
                    {
                        new Lazy<INuGetResourceProvider>(() => new CustomHttpHandlerResourceProvider()),
                        new Lazy<INuGetResourceProvider>(() => new CustomHttpSourceResourceProvider(
                            p.GetRequiredService<IHttpCacheUtility>(),
                            p.GetRequiredService<IConcurrencyUtility>()))
                    });

                return Repository.CreateSource(providers, "https://api.nuget.org/v3/index.json");
            });
            builder.Services.AddSingleton(p => p
                .GetRequiredService<SourceRepository>()
                .GetResourceAsync<FindPackageByIdResource>());

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddBaseAddressHttpClient();

            await builder.Build().RunAsync();
        }
    }
}
