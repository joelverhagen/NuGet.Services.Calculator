namespace NuGet.Services.Calculator.Logic
{
    public class FindBestPackageVersionMatchOutput
    {
        public InputStatus InputStatus { get; set; }
        public FindBestPackageVersionMatchInput Input { get; set; }
        public string PackageId { get; set; }
        public bool ShowUnlisted { get; set; }
        public BestVersionMatch Result { get; set; }
    }
}
