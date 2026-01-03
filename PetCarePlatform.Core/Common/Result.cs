namespace PetCarePlatform.Core.Common
{
    /// <summary>
    /// Represents the result of an operation that can either succeed or fail.
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; private set; }
        public bool IsFailure => !IsSuccess;
        public string ErrorMessage { get; private set; } = string.Empty;
        public string? ErrorCode { get; private set; }

        protected Result(bool isSuccess, string errorMessage, string? errorCode = null)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }

        public static Result Success() => new Result(true, string.Empty);
        
        public static Result Failure(string errorMessage, string? errorCode = null) 
            => new Result(false, errorMessage, errorCode);

        public static Result<T> Success<T>(T value) => Result<T>.Success(value);
        
        public static Result<T> Failure<T>(string errorMessage, string? errorCode = null) 
            => Result<T>.Failure(errorMessage, errorCode);
    }

    /// <summary>
    /// Represents the result of an operation that returns a value.
    /// </summary>
    public class Result<T> : Result
    {
        public T? Value { get; private set; }

        private Result(bool isSuccess, T? value, string errorMessage, string? errorCode = null)
            : base(isSuccess, errorMessage, errorCode)
        {
            Value = value;
        }

        public static Result<T> Success(T value) => new Result<T>(true, value, string.Empty);
        
        public static Result<T> Failure(string errorMessage, string? errorCode = null) 
            => new Result<T>(false, default, errorMessage, errorCode);

        public static implicit operator Result<T>(T value) => Success(value);
    }
}
