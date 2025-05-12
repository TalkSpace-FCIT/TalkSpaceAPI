namespace Domain.Results
{
    public class Result<T>
    {
        protected Result(bool isSuccess, T? value, string? error = null)
        {
            this.IsSuccess = isSuccess;
            this.Value = value;
            this.Message = error;
        }

        protected Result(bool isSuccess, string error, ErrorSource errorSource)
        {
            this.IsSuccess = isSuccess;
            this.Message = error;
            this.ErrorSource = errorSource;
        }

        public bool IsSuccess { get; }
        public string? Message { get; }
        public ErrorSource ErrorSource { get; }
        public bool IsFailure => !this.IsSuccess;
        public bool IsEmpty => this.IsSuccess && this.Value is null; 
        public T? Value { get; }

        public static Result<T> Success(T? value, string? message) => new Result<T>(true, value, message);
        public static Result<T> Empty(string? message = null) => new Result<T>(true, default, message);
        public static Result<T> Failure(string message, ErrorSource errorSource = ErrorSource.TalkSpaceAPI) => new Result<T>(false, message, errorSource);

    }
}
