using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Moq;
using NuGet.Protocol.Core.Types;
using NuGet.Services.Calculator.Support;
using Xunit;
using Xunit.Abstractions;

namespace NuGet.Services.Calculator.Logic
{
    public class VersionRangeCalculatorIntegrationTests
    {
        public class TheFindBestPackageVersionMatchMethod : Facts
        {
            public TheFindBestPackageVersionMatchMethod(ITestOutputHelper output) : base(output)
            {
            }

            [Fact]
            public async Task FindBestPackageVersionMatch()
            {
                var input = new FindBestPackageVersionMatchInput
                {
                    PackageId = "NuGet.Versioning",
                    VersionRange = "[4.3.0, 5.0.0)",
                    ShowUnlisted = false,
                };

                var output = await Target.FindBestPackageVersionMatchAsync(input, Token);

                Assert.Equal("4.3.0", output.Result.BestMatch.Version.ToNormalizedString());
            }

            [Theory]
            [InlineData(false)]
            [InlineData(true)]
            public async Task UnlistedVersionIsBestMatch(bool showUnlisted)
            {
                var input = new FindBestPackageVersionMatchInput
                {
                    PackageId = "BaseTestPackage.Unlisted",
                    VersionRange = "1.1.0",
                    ShowUnlisted = showUnlisted,
                };

                var output = await Target.FindBestPackageVersionMatchAsync(input, Token);

                Assert.Equal("1.1.0", output.Result.BestMatch.Version.ToNormalizedString());
            }

            [Fact]
            public async Task CanFilterOutUnlistedThatAreNotBestMatch()
            {
                var input = new FindBestPackageVersionMatchInput
                {
                    PackageId = "BaseTestPackage.Unlisted",
                    VersionRange = "1.2.0",
                    ShowUnlisted = false,
                };

                var output = await Target.FindBestPackageVersionMatchAsync(input, Token);

                Assert.Empty(output.Result.Versions);
                Assert.Null(output.Result.BestMatch);
            }

            [Fact]
            public async Task CanIncludeUnlistedThatAreNotBestMatch()
            {
                var input = new FindBestPackageVersionMatchInput
                {
                    PackageId = "BaseTestPackage.Unlisted",
                    VersionRange = "1.2.0",
                    ShowUnlisted = true,
                };

                var output = await Target.FindBestPackageVersionMatchAsync(input, Token);

                Assert.Contains("1.1.0", output.Result.Versions.Select(v => v.Version.ToNormalizedString()));
            }
        }

        public abstract class Facts
        {
            public Facts(ITestOutputHelper output)
            {
                StorageService = new InMemoryStorageService();
                HttpCacheUtility = new LocalStorageHttpCacheUtility(StorageService);
                ConcurrencyUtility = new InMemoryConcurrencyUtility();
                SourceRepository = Repository.Factory.GetCustomRepository(HttpCacheUtility, ConcurrencyUtility);
                Logger = new XunitLogger<VersionRangeCalculator>(output);

                Token = CancellationToken.None;

                Target = new VersionRangeCalculator(
                    SourceRepository.GetResourceAsync<PackageMetadataResource>(),
                    Logger);
            }

            public Mock<FindPackageByIdResource> FindResource { get; }
            public Mock<PackageMetadataResource> MetadataResource { get; }
            public VersionRangeCalculator Target { get; }
            public InMemoryStorageService StorageService { get; }
            public LocalStorageHttpCacheUtility HttpCacheUtility { get; }
            public InMemoryConcurrencyUtility ConcurrencyUtility { get; }
            public SourceRepository SourceRepository { get; }
            public XunitLogger<VersionRangeCalculator> Logger { get; }
            public CancellationToken Token { get; }
        }

        public class InMemoryStorageService : ILocalStorageService
        {
            public event EventHandler<ChangingEventArgs> Changing
            {
                add { }
                remove { }
            }

            public event EventHandler<ChangedEventArgs> Changed
            {
                add { }
                remove { }
            }

            private readonly Dictionary<string, object> _objects = new Dictionary<string, object>();

            public Task ClearAsync() => throw new NotImplementedException();
            public Task<bool> ContainKeyAsync(string key) => throw new NotImplementedException();
            public Task<string> KeyAsync(int index) => throw new NotImplementedException();
            public Task<int> LengthAsync() => throw new NotImplementedException();
            public Task RemoveItemAsync(string key) => throw new NotImplementedException();

            public Task<T> GetItemAsync<T>(string key)
            {
                if (_objects.TryGetValue(key, out var value))
                {
                    return Task.FromResult((T)value);
                }

                return Task.FromResult(default(T));
            }

            public Task SetItemAsync(string key, object data)
            {
                _objects[key] = data;
                return Task.CompletedTask;
            }
        }
    }
}
