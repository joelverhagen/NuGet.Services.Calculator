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
        private readonly Task<FindPackageByIdResource> _resourceTask;
        private readonly StandardToNuGetLogger _nuGetLogger;

        public VersionRangeCalculator(Task<FindPackageByIdResource> resourceTask, ILogger<VersionRangeCalculator> logger)
        {
            _resourceTask = resourceTask ?? throw new ArgumentNullException(nameof(resourceTask));

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
            using (var cache = new SourceCacheContext())
            {
                var resource = await _resourceTask;
                versions = await resource.GetAllVersionsAsync(input.PackageId, cache, _nuGetLogger, cancellationToken);
            }

            output.InputStatus = InputStatus.Valid;
            output.Result = GetBestVersionMatch(validatedVersionRange.Data, versions.ToList());

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
            output.Result = GetBestVersionMatch(validatedVersionRange.Data, validatedVersions.Data);

            return output;
        }

        private BestVersionMatch GetBestVersionMatch(
            VersionRange versionRange,
            IReadOnlyList<NuGetVersion> inputVersions)
        {
            var outputVersions = new List<VersionSatisfies>();
            var output = new BestVersionMatch
            {
                BestMatch = versionRange.FindBestMatch(inputVersions),
                VersionRange = versionRange,
                Versions = outputVersions,
            };

            foreach (var version in inputVersions)
            {
                outputVersions.Add(new VersionSatisfies(
                    version,
                    versionRange.Satisfies(version)));
            }

            outputVersions.Sort((a, b) => -1 * Compare(versionRange, a, b));

            return output;
        }
             
        private int Compare(VersionRange versionRange, VersionSatisfies a, VersionSatisfies b)
        {
            var satisfiesComparison = a.Satisfies.CompareTo(b.Satisfies);
            if (satisfiesComparison != 0)
            {
                return satisfiesComparison;
            }
            else if (versionRange.IsBetter(a.Version, b.Version))
            {
                return -1;
            }
            else if (versionRange.IsBetter(b.Version, a.Version))
            {
                return 1;
            }
            else
            {
                return a.Version.CompareTo(b.Version);
            }
        }

        public async Task<IEnumerable<NuGetVersion>> GetVersionsAsync(string packageId, CancellationToken cancellationToken)
        {
            using (var cache = new SourceCacheContext())
            {
                var resource = await _resourceTask;
                return await resource.GetAllVersionsAsync(packageId, cache, _nuGetLogger, cancellationToken);
            }
        }
    }
}
