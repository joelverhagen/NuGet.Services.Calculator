using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Protocol.Core.Types;
using NuGet.Services.Calculator.Support;
using NuGet.Versioning;
using Xunit;
using Xunit.Abstractions;

namespace NuGet.Services.Calculator.Logic
{
    public class VersionRangeCalculatorFacts
    {
        public class TheFindBestVersionMatchMethod : Facts
        {
            public TheFindBestVersionMatchMethod(ITestOutputHelper output) : base(output)
            {
                Input = new FindBestVersionMatchInput
                {
                    VersionRange = "(, )",
                    Versions = string.Join(Environment.NewLine, new List<string>
                    {
                        "4.2.0-beta",
                        "4.2.0",
                        "4.3.0-beta",
                        "4.3.0",
                        "4.3.1-beta",
                        "4.3.1",
                        "4.4.0-beta",
                        "4.4.0",
                        "4.9.0-beta",
                        "4.9.0",
                        "5.0.0-beta",
                        "5.0.0",
                        "5.0.1-beta",
                        "5.0.1",
                    }),
                };
            }

            public FindBestVersionMatchInput Input { get; }

            [Fact]
            public void SelectsBestMatchWithStableRange()
            {
                Input.VersionRange = "[4.3.0, 5.0.0)";

                var output = Target.FindBestVersionMatch(Input);

                Assert.Equal("4.3.0", output.Result.BestMatch.Version.ToNormalizedString());
            }

            [Fact]
            public void SelectsBestMatchWithPrereleaseRange()
            {
                Input.VersionRange = "[4.3.0-z, 5.0.0)";

                var output = Target.FindBestVersionMatch(Input);

                Assert.Equal("4.3.0", output.Result.BestMatch.Version.ToNormalizedString());
            }

            [Fact]
            public void DeterminesIfEachVersionSatisfiesWithStableRange()
            {
                Input.VersionRange = "[4.3.0, 5.0.0)";

                var output = Target.FindBestVersionMatch(Input);

                Assert.Equal(
                    new string[]
                    {
                        "4.3.0",
                        "4.3.1",
                        "4.4.0",
                        "4.9.0",
                    },
                    GetSorted(output, v => v.IsCompatible));
                Assert.Equal(
                    new string[]
                    {
                        "4.2.0-beta",
                        "4.2.0",
                        "4.3.0-beta",
                        "4.3.1-beta",
                        "4.4.0-beta",
                        "4.9.0-beta",
                        "5.0.0-beta",
                        "5.0.0",
                        "5.0.1-beta",
                        "5.0.1",
                    },
                    GetSorted(output, v => !v.IsCompatible));
            }

            [Fact]
            public void DeterminesIfEachVersionSatisfiesWithPrereleaseRange()
            {
                Input.VersionRange = "[4.3.0-z, 5.0.0)";

                var output = Target.FindBestVersionMatch(Input);

                Assert.Equal(
                    new string[]
                    {
                        "4.3.0",
                        "4.3.1-beta",
                        "4.3.1",
                        "4.4.0-beta",
                        "4.4.0",
                        "4.9.0-beta",
                        "4.9.0",
                        "5.0.0-beta",
                    },
                    GetSorted(output, v => v.IsCompatible));
                Assert.Equal(
                    new string[]
                    {
                        "4.2.0-beta",
                        "4.2.0",
                        "4.3.0-beta",
                        "5.0.0",
                        "5.0.1-beta",
                        "5.0.1",
                    },
                    GetSorted(output, v => !v.IsCompatible));
            }

            [Fact]
            public void SortsOutputVersionsByPreferenceWithPrereleaseRange()
            {
                Input.VersionRange = "[4.3.0-z, 5.0.0)";

                var output = Target.FindBestVersionMatch(Input);

                Assert.Equal(
                    new string[]
                    {
                        "4.3.0",
                        "4.3.1-beta",
                        "4.3.1",
                        "4.4.0-beta",
                        "4.4.0",
                        "4.9.0-beta",
                        "4.9.0",
                        "5.0.0-beta",
                        "4.2.0-beta",
                        "4.2.0",
                        "4.3.0-beta",
                        "5.0.0",
                        "5.0.1-beta",
                        "5.0.1",
                    },
                    output
                        .Result
                        .Versions
                        .Select(x => x.Version.ToNormalizedString())
                        .ToArray());
            }

            [Fact]
            public void SortsOutputVersionsByPreferenceWithStableRange()
            {
                Input.VersionRange = "[4.3.0, 5.0.0)";

                var output = Target.FindBestVersionMatch(Input);

                Assert.Equal(
                    new string[]
                    {
                        "4.3.0",
                        "4.3.1",
                        "4.4.0",
                        "4.9.0",
                        "4.2.0-beta",
                        "4.2.0",
                        "4.3.0-beta",
                        "4.3.1-beta",
                        "4.4.0-beta",
                        "4.9.0-beta",
                        "5.0.0-beta",
                        "5.0.0",
                        "5.0.1-beta",
                        "5.0.1",
                    },
                    output
                        .Result
                        .Versions
                        .Select(x => x.Version.ToNormalizedString())
                        .ToArray());
            }

            private static string[] GetSorted(
                FindBestVersionMatchOutput output,
                Func<VersionCompatibility, bool> predicate = null)
            {
                return output
                    .Result
                    .Versions
                    .Where(predicate ?? (v => true))
                    .OrderBy(x => x.Version)
                    .Select(x => x.Version.ToNormalizedString())
                    .ToArray();
            }
        }

        public abstract class Facts
        {
            public Facts(ITestOutputHelper output)
            {
                MetadataResource = new Mock<PackageMetadataResource>();
                Logger = new XunitLogger<VersionRangeCalculator>(output);

                Target = new VersionRangeCalculator(
                    Task.FromResult(MetadataResource.Object),
                    Logger);
            }

            public Mock<FindPackageByIdResource> FindResource { get; }
            public Mock<PackageMetadataResource> MetadataResource { get; }
            public XunitLogger<VersionRangeCalculator> Logger { get; }
            public VersionRangeCalculator Target { get; }
        }
    }
}
