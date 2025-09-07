using System.Diagnostics;

namespace TunCRM.Services
{
    public class ErrorHandlingService
    {
        private readonly ILogger<ErrorHandlingService> _logger;

        public ErrorHandlingService(ILogger<ErrorHandlingService> logger)
        {
            _logger = logger;
        }

        public async Task<T> ExecuteWithErrorHandlingAsync<T>(
            Func<Task<T>> operation, 
            string operationName, 
            T defaultValue = default(T))
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex, operationName);
                return defaultValue;
            }
        }

        public async Task ExecuteWithErrorHandlingAsync(
            Func<Task> operation, 
            string operationName)
        {
            try
            {
                await operation();
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex, operationName);
            }
        }

        public T ExecuteWithErrorHandling<T>(
            Func<T> operation, 
            string operationName, 
            T defaultValue = default(T))
        {
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                LogError(ex, operationName);
                return defaultValue;
            }
        }

        public void ExecuteWithErrorHandling(
            Action operation, 
            string operationName)
        {
            try
            {
                operation();
            }
            catch (Exception ex)
            {
                LogError(ex, operationName);
            }
        }

        private async Task LogErrorAsync(Exception ex, string operationName)
        {
            var errorId = Guid.NewGuid().ToString();
            var stackTrace = ex.StackTrace ?? "No stack trace available";
            
            _logger.LogError(ex, 
                "Error in {OperationName}. ErrorId: {ErrorId}. Message: {Message}", 
                operationName, errorId, ex.Message);

            // Additional error logging logic can be added here
            // For example, saving to database, sending notifications, etc.
            
            await Task.CompletedTask;
        }

        private void LogError(Exception ex, string operationName)
        {
            var errorId = Guid.NewGuid().ToString();
            
            _logger.LogError(ex, 
                "Error in {OperationName}. ErrorId: {ErrorId}. Message: {Message}", 
                operationName, errorId, ex.Message);
        }

        public string GetUserFriendlyMessage(Exception ex)
        {
            return ex switch
            {
                ArgumentException => "Geçersiz veri girişi. Lütfen kontrol ediniz.",
                UnauthorizedAccessException => "Bu işlem için yetkiniz bulunmamaktadır.",
                FileNotFoundException => "Aranan dosya bulunamadı.",
                TimeoutException => "İşlem zaman aşımına uğradı. Lütfen tekrar deneyiniz.",
                InvalidOperationException => "İşlem şu anda gerçekleştirilemiyor.",
                _ => "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyiniz."
            };
        }

        public class ErrorResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public string ErrorId { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; } = DateTime.Now;
        }

        public ErrorResult CreateErrorResult(Exception ex, string operationName)
        {
            var errorId = Guid.NewGuid().ToString();
            
            _logger.LogError(ex, 
                "Error in {OperationName}. ErrorId: {ErrorId}. Message: {Message}", 
                operationName, errorId, ex.Message);

            return new ErrorResult
            {
                Success = false,
                Message = GetUserFriendlyMessage(ex),
                ErrorId = errorId
            };
        }
    }
}
