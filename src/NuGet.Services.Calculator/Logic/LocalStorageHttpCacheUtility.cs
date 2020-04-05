using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace NuGet.Services.Calculator.Logic
{
    public class LocalStorageHttpCacheUtility : IHttpCacheUtility
    {
        private readonly ILocalStorageService _localStorageService;

        public LocalStorageHttpCacheUtility(ILocalStorageService localStorageService)
        {
            _localStorageService = localStorageService ?? throw new ArgumentNullException(nameof(localStorageService));
        }

        public async Task CreateCacheFileAsync(
            HttpCacheResult result,
            HttpResponseMessage response,
            Action<Stream> ensureValidContents,
            CancellationToken cancellationToken)
        {
            var memoryStream = new MemoryStream();
            await response.Content.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            ensureValidContents?.Invoke(memoryStream);
            await _localStorageService.SetItemAsync(
                GetCacheKey(result.CacheFile),
                new CacheEntry { Added = DateTimeOffset.Now, Contents = memoryStream.ToArray() });
            memoryStream.Position = 0;
            result.Stream = memoryStream;
        }

        private static string GetCacheKey(string cacheFile)
        {
            return Path.GetFullPath(cacheFile).ToLowerInvariant();
        }

        public HttpCacheResult InitializeHttpCacheResult(string httpCacheDirectory, Uri sourceUri, string cacheKey, HttpSourceCacheContext context)
        {
            return HttpCacheUtility.InitializeHttpCacheResult(
                httpCacheDirectory,
                sourceUri,
                cacheKey,
                context);
        }

        public async Task<Stream> TryReadCacheFileAsync(TimeSpan maxAge, string cacheFile)
        {
            var entry = await _localStorageService.GetItemAsync<CacheEntry>(GetCacheKey(cacheFile));
            if (entry != null && (DateTimeOffset.Now - entry.Added) <= maxAge)
            {
                return new MemoryStream(entry.Contents);
            }

            return null;
        }

        private class CacheEntry
        {
            public DateTimeOffset Added { get; set; }
            public byte[] Contents { get; set; }
        }
    }
}
