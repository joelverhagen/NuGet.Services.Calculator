using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;

namespace NuGet.Services.Calculator.Logic
{
    public class InMemoryConcurrencyUtility : IConcurrencyUtility
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks
            = new ConcurrentDictionary<string, SemaphoreSlim>(StringComparer.OrdinalIgnoreCase);

        public async Task<T> ExecuteWithFileLockedAsync<T>(
            string filePath,
            Func<CancellationToken, Task<T>> action,
            CancellationToken token)
        {
            var semaphore = _locks.GetOrAdd(filePath, p => new SemaphoreSlim(1));
            await semaphore.WaitAsync(token);
            try
            {
                return await action(token);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
