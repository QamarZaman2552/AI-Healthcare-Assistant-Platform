namespace AIHealthcareAssistant.Application.Common.Response
{
    public class ApiResponse<T>
    {
        public bool Success { get; init; }

        public string Message { get; init; } = string.Empty;

        public T? Data { get; init; }
        public static ApiResponse<T> Ok(T data, string message = " Request Completed Successfully.")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data

            };
        }
    }
}
