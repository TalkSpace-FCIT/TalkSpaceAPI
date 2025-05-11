namespace Domain.Results
{
    public class Result<T>
    {
        protected Result(bool isSuccess, T? value, string? error = null)
        {
            this.IsSuccess = isSuccess;
            this.Value = value;
            this.Error = error;
        }

        protected Result(bool isSuccess, string error, ErrorSource errorSource)
        {
            this.IsSuccess = isSuccess;
            this.Error = error;
            this.ErrorSource = errorSource;
        }

        public bool IsSuccess { get; }
        public string? Error { get; }
        public ErrorSource ErrorSource { get; }
        public bool IsFailure => !this.IsSuccess;
        public bool IsEmpty => this.IsSuccess && this.Value is null; 
        public T? Value { get; }

        public static Result<T> Success(T? value) => new Result<T>(true, value);
        public static Result<T> Empty(string? error = null) => new Result<T>(true, default, error);
        public static Result<T> Failure(string error, ErrorSource errorSource = ErrorSource.TalkSpaceAPI) => new Result<T>(false, error, errorSource);

    }
}
