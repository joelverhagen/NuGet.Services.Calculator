using System.Collections.Generic;
using NuGet.Versioning;

namespace NuGet.Services.Calculator.Logic
{
    public class FindBestVersionMatchOutput
    {
        public InputStatus InputStatus { get; set; }
        public FindBestVersionMatchInput Input { get; set; }
        public VersionRange VersionRange { get; set; }
        public IReadOnlyList<VersionSatisfies> Versions { get; set; }
        public NuGetVersion BestMatch { get; set; }
    }
}
