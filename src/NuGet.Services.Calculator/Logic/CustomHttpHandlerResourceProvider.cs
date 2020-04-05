using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace NuGet.Services.Calculator.Logic
{
    public class CustomHttpHandlerResourceProvider : ResourceProvider
    {
        public CustomHttpHandlerResourceProvider() : base(
            typeof(HttpHandlerResource),
            nameof(CustomHttpHandlerResourceProvider),
            nameof(HttpHandlerResourceV3Provider))
        {
        }

        public override Task<Tuple<bool, INuGetResource>> TryCreate(SourceRepository source, CancellationToken token)
        {
            HttpHandlerResourceV3 curResource = null;

            if (source.PackageSource.IsHttp)
            {
                var clientHandler = new HttpClientHandler();

                try
                {
                    clientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                }
                catch (PlatformNotSupportedException)
                {
                    // Assume the platform is handling the decompression.
                }

                var messageHandler = new ServerWarningLogHandler(clientHandler);
                curResource = new HttpHandlerResourceV3(clientHandler, messageHandler);
            }

            return Task.FromResult(new Tuple<bool, INuGetResource>(curResource != null, curResource));
        }
    }
}
