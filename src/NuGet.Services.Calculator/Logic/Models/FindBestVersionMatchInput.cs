namespace NuGet.Services.Calculator.Logic
{
    public class FindBestVersionMatchInput
    {
        [VersionRangeValidation]
        public string VersionRange { get; set; }

        [VersionListValidation]
        public string Versions { get; set; }
    }
}
