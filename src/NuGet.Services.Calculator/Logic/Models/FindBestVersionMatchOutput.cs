﻿namespace NuGet.Services.Calculator.Logic
{
    public class FindBestVersionMatchOutput
    {
        public InputStatus InputStatus { get; set; }
        public FindBestVersionMatchInput Input { get; set; }
        public BestVersionMatch Result { get; set; }
    }
}
