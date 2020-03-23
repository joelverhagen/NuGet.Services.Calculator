using System;

namespace NuGet.Services.Calculator.Logic
{
    public class Validated<T>
    {
        public Validated(InputStatus inputStatus, string errorMessage, T data)
        {
            InputStatus = inputStatus;
            ErrorMessage = errorMessage;
            Data = data;
        }

        public InputStatus InputStatus { get; }
        public string ErrorMessage { get; }
        public T Data { get; }

        public bool IsMissing => InputStatus == InputStatus.Missing;
        public bool IsInvalid => InputStatus == InputStatus.Invalid;
        public bool IsValid => InputStatus == InputStatus.Valid;

        public Validated<TOutput> ToType<TOutput>(TOutput data)
        {
            return new Validated<TOutput>(InputStatus, ErrorMessage, data);
        }

        public Validated<TOutput> ToType<TOutput>()
        {
            return ToType<TOutput>(default);
        }
    }

    public static class Validated
    {
        public static Validated<T> Missing<T>()
        {
            return new Validated<T>(InputStatus.Missing, "The input is required.", default);
        }

        public static Validated<T> Invalid<T>(string errorMessage)
        {
            if (errorMessage == null)
            {
                throw new ArgumentNullException(nameof(errorMessage));
            }

            return new Validated<T>(InputStatus.Invalid, errorMessage, default);
        }

        public static Validated<T> Valid<T>(T data)
        {
            return new Validated<T>(InputStatus.Valid, errorMessage: null, data: data);
        }

        public static Validated<T> Invalid<T>(Exception ex)
        {
            var errorMessage = ex.Message;
            if (ex is ArgumentException argEx)
            {
                var suffix = "Parameter name: " + argEx.ParamName;
                if (errorMessage.EndsWith(suffix))
                {
                    errorMessage = errorMessage.Substring(0, ex.Message.Length - suffix.Length);
                }
            }

            return Invalid<T>(errorMessage.Trim());
        }
    }
}
