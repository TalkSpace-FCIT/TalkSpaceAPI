namespace Domain.Results
{
    public class QueryResult<T> : Result<T>
    {
        public QueryResult(bool isFound, bool isSuccess, T? value) : base(isSuccess, value)
        {
            this.IsFound = isFound;
        }

        public QueryResult(bool isFound, bool isSuccess, string error, ErrorSource errorSource) : base(isSuccess, error, errorSource)
        {
            this.IsFound = isFound;
        }
        public bool IsFound { get; }
        public bool IsNotFound => !this.IsFound;

        public static new QueryResult<T> Success(T value) => 
            new QueryResult<T>(true, true, value);

        public static QueryResult<T> Failure(string error) =>
            new QueryResult<T>(false, false, error, ErrorSource.Database);

        public static QueryResult<T> NotFound() =>
            new QueryResult<T>(false, true, default);
    }
}
