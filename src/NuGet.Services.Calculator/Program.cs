using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Services.Calculator.Logic;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace NuGet.Services.Calculator
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddScoped<IHttpCacheUtility, LocalStorageHttpCacheUtility>();
            builder.Services.AddScoped<IConcurrencyUtility, InMemoryConcurrencyUtility>();
            builder.Services.AddScoped(p => Repository.Factory.GetCustomRepository(
                p.GetRequiredService<IHttpCacheUtility>(),
                p.GetRequiredService<IConcurrencyUtility>()));
            builder.Services.AddScoped(p => p.GetRequiredService<SourceRepository>().GetResourceAsync<PackageMetadataResource>());

            builder.Services.AddTransient<VersionRangeCalculator>();

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddHeadElementHelper();
            builder.Services.AddBaseAddressHttpClient();

            await builder.Build().RunAsync();
        }
    }
}
