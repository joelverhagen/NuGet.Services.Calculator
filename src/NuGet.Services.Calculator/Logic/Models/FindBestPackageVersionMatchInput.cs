namespace NuGet.Services.Calculator.Logic
{
    public class FindBestPackageVersionMatchInput
    {
        [PackageIdValidation]
        public string PackageId { get; set; }

        [VersionRangeValidation]
        public string VersionRange { get; set; }

        public bool ShowUnlisted { get; set; }
    }
}
