using System;
using System.Collections.Generic;
using System.IO;
using NuGet.Packaging;
using NuGet.Versioning;

namespace NuGet.Services.Calculator.Logic
{
    public static class InputValidator
    {
        private const string MustBeAString = "The input must be a string.";

        public static Validated<VersionRange> VersionRange(object value)
        {
            var validatedString = String(value);
            if (!validatedString.IsValid)
            {
                return validatedString.ToType<VersionRange>();
            }

            try
            {
                var output = Versioning.VersionRange.Parse(validatedString.Data);
                return Validated.Valid(output);
            }
            catch (Exception ex)
            {
                return Validated.Invalid<VersionRange>(ex);
            }
        }

        public static Validated<IReadOnlyList<NuGetVersion>> Versions(object value)
        {
            var validatedString = String(value);
            if (!validatedString.IsValid)
            {
                return validatedString.ToType<IReadOnlyList<NuGetVersion>>();
            }

            var output = new List<NuGetVersion>();
            using (var reader = new StringReader(validatedString.Data))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    try
                    {
                        var version = NuGetVersion.Parse(line.Trim());
                        output.Add(version);
                    }
                    catch (Exception ex)
                    {
                        return Validated.Invalid<IReadOnlyList<NuGetVersion>>(ex);
                    }
                }
            }

            return Validated.Valid<IReadOnlyList<NuGetVersion>>(output);
        }

        public static Validated<string> PackageId(object value)
        {
            var validatedString = String(value);
            if (!validatedString.IsValid)
            {
                return validatedString;
            }

            try
            {
                PackageIdValidator.ValidatePackageId(validatedString.Data);
                return validatedString;
            }
            catch (Exception ex)
            {
                return Validated.Invalid<string>(ex);
            }
        }

        private static Validated<string> String(object value)
        {
            if (ReferenceEquals(value, null))
            {
                return Validated.Missing<string>();
            }

            if (!(value is string valueStr))
            {
                return Validated.Invalid<string>(MustBeAString);
            }

            if (string.IsNullOrWhiteSpace(valueStr))
            {
                return Validated.Missing<string>();
            }

            return Validated.Valid(valueStr);
        }
    }
}
