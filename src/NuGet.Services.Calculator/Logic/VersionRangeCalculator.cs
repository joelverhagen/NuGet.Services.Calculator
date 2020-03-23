using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace NuGet.Services.Calculator.Logic
{
    public class VersionRangeCalculator
    {
        private readonly Task<FindPackageByIdResource> _resourceTask;

        public VersionRangeCalculator(Task<FindPackageByIdResource> resourceTask)
        {
            _resourceTask = resourceTask ?? throw new ArgumentNullException(nameof(resourceTask));
        }

        public FindBestVersionMatchOutput FindBestVersionMatch(FindBestVersionMatchInput input)
        {
            var outputVersions = new List<VersionSatisfies>();
            var output = new FindBestVersionMatchOutput
            {
                InputStatus = InputStatus.Missing,
                Versions = outputVersions,
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
            output.VersionRange = validatedVersionRange.Data;
            output.BestMatch = validatedVersionRange.Data.FindBestMatch(validatedVersions.Data);

            foreach (var version in validatedVersions.Data)
            {
                outputVersions.Add(new VersionSatisfies(
                    version,
                    validatedVersionRange.Data.Satisfies(version)));
            }

            outputVersions.Sort((a, b) => -1 * Compare(validatedVersionRange.Data, a, b));

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
