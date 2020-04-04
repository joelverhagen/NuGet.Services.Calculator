using System.Collections.Generic;
using NuGet.Versioning;

namespace NuGet.Services.Calculator.Logic
{
    public class BestVersionMatch
    {
        public VersionRange VersionRange { get; set; }
        public IReadOnlyList<VersionCompatibility> Versions { get; set; }
        public NuGetVersion BestMatch { get; set; }
    }
}
