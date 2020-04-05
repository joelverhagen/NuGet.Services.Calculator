using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace NuGet.Services.Calculator.Logic
{
    public class VersionRangeCalculator
    {
        private readonly Task<PackageMetadataResource> _metadataResourceTask;
        private readonly StandardToNuGetLogger _nuGetLogger;

        public VersionRangeCalculator(
            Task<PackageMetadataResource> metadataResourceTask,
            ILogger<VersionRangeCalculator> logger)
        {
            _metadataResourceTask = metadataResourceTask ?? throw new ArgumentNullException(nameof(metadataResourceTask));

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _nuGetLogger = new StandardToNuGetLogger(logger);
        }

        public async Task<FindBestPackageVersionMatchOutput> FindBestPackageVersionMatchAsync(
            FindBestPackageVersionMatchInput input,
            CancellationToken cancellationToken)
        {
            var output = new FindBestPackageVersionMatchOutput
            {
                InputStatus = InputStatus.Missing,
                Input = input,
            };

            if (input == null)
            {
                return output;
            }

            var validatedPackageId = InputValidator.PackageId(input.PackageId);
            var validatedVersionRange = InputValidator.VersionRange(input.VersionRange);

            if (validatedPackageId.IsMissing || validatedPackageId.IsMissing)
            {
                return output;
            }

            if (validatedVersionRange.IsInvalid || validatedVersionRange.IsInvalid)
            {
                output.InputStatus = InputStatus.Invalid;
                return output;
            }

            IEnumerable<NuGetVersion> versions;
            HashSet<NuGetVersion> listed;
            using (var cache = new SourceCacheContext())
            {
                var resource = await _metadataResourceTask;
                var metadata = await resource.GetMetadataAsync(
                    input.PackageId,
                    includePrerelease: true,
                    includeUnlisted: true,
                    cache,
                    _nuGetLogger,
                    cancellationToken);

                listed = new HashSet<NuGetVersion>(metadata.Where(x => x.IsListed).Select(x => x.Identity.Version));
                versions = metadata.Select(x => x.Identity.Version).ToList();
            }

            output.InputStatus = InputStatus.Valid;
            output.PackageId = validatedPackageId.Data;
            output.ShowUnlisted = input.ShowUnlisted;
            output.Result = GetBestVersionMatch(validatedVersionRange.Data, versions.ToList(), listed, output.ShowUnlisted);

            return output;
        }

        public FindBestVersionMatchOutput FindBestVersionMatch(FindBestVersionMatchInput input)
        {
            var output = new FindBestVersionMatchOutput
            {
                InputStatus = InputStatus.Missing,
                Input = input,
            };

            if (input == null)
            {
                return output;
            }

            var validatedVersionRange = InputValidator.VersionRange(input.VersionRange);
            var validatedVersions = InputValidator.Versions(input.Versions);

            if (validatedVersionRange.IsMissing || validatedVersions.IsMissing)
            {
                return output;
            }

            if (validatedVersionRange.IsInvalid || validatedVersions.IsInvalid)
            {
                output.InputStatus = InputStatus.Invalid;
                return output;
            }

            output.InputStatus = InputStatus.Valid;
            output.Result = GetBestVersionMatch(
                validatedVersionRange.Data,
                validatedVersions.Data,
                listed: null,
                showUnlisted: true);

            return output;
        }

        private BestVersionMatch GetBestVersionMatch(
            VersionRange versionRange,
            IReadOnlyList<NuGetVersion> inputVersions,
            HashSet<NuGetVersion> listed,
            bool showUnlisted)
        {
            var outputVersions = new List<VersionCompatibility>();
            var output = new BestVersionMatch
            {
                VersionRange = versionRange,
                Versions = outputVersions,
            };

            var remainingVersions = new List<NuGetVersion>(inputVersions);
            while (remainingVersions.Any())
            {
                var version = versionRange.FindBestMatch(remainingVersions);
                if (version == null)
                {
                    break;
                }

                var compatibility = new VersionCompatibility(
                    version,
                    isCompatible: true,
                    isListed: listed?.Contains(version));

                if (output.BestMatch == null)
                {
                    output.BestMatch = compatibility;
                    outputVersions.Add(compatibility);
                }
                else if (showUnlisted || compatibility.IsListed != false)
                {
                    outputVersions.Add(compatibility);
                }

                remainingVersions.Remove(version);
            }

            outputVersions.AddRange(remainingVersions
                .OrderBy(version => version)
                .Select(version => new VersionCompatibility(
                    version,
                    isCompatible: false,
                    isListed: listed?.Contains(version)))
                .Where(version => showUnlisted || version.IsListed != false));

            return output;
        }
    }
}
