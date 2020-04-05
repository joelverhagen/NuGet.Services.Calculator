using NuGet.Versioning;

namespace NuGet.Services.Calculator.Logic
{
    public class VersionCompatibility
    {
        public VersionCompatibility(NuGetVersion version, bool isCompatible, bool? isListed)
        {
            Version = version;
            IsCompatible = isCompatible;
            IsListed = isListed;
        }

        public NuGetVersion Version { get; }
        public bool IsCompatible { get; }
        public bool? IsListed { get; }
    }
}
