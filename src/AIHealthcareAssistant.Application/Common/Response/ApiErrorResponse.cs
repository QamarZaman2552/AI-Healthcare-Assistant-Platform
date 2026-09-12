namespace AIHealthcareAssistant.Application.Common.Response
{
    public class ApiErrorResponse
    {
        public bool Success { get; init; } = false;

        public string Message { get; init; } = string.Empty;

        public List<ApiValidationError> Errors { get; init; } = new();

        public static ApiErrorResponse Error(string message)
        {
            return new ApiErrorResponse
            {
                Message = message
            };
        }

        public static ApiErrorResponse Validation(
            string message,
            List<ApiValidationError> errors)
        {
            return new ApiErrorResponse
            {
                Message = message,
                Errors = errors
            };
        }

        public static ApiErrorResponse Unauthorized(string message)
        {
            return new ApiErrorResponse
            {
                Message = message
            };
        }

        public static ApiErrorResponse Forbidden(string message)
        {
            return new ApiErrorResponse
            {
                Message = message
            };
        }
    }
}