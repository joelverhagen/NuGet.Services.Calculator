using NuGet.Versioning;

namespace NuGet.Services.Calculator.Logic
{
    public class VersionSatisfies
    {
        public VersionSatisfies(NuGetVersion version, bool satisfies)
        {
            Version = version;
            Satisfies = satisfies;
        }

        public NuGetVersion Version { get; }
        public bool Satisfies { get; }
    }
}
