using NuGet.Versioning;

namespace NuGet.Services.Calculator.Logic
{
    public class VersionCompatibility
    {
        public VersionCompatibility(NuGetVersion version, bool isCompatible)
        {
            Version = version;
            IsCompatible = isCompatible;
        }

        public NuGetVersion Version { get; }
        public bool IsCompatible { get; }
    }
}
