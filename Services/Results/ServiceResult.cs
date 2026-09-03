namespace BlogApp.Services.Results
{
    public enum ServiceResultStatus
    {
        Ok,
        NotFound,
        Forbidden,
        ValidationError
    }

    // Servislerin HTTP'ye özgü kavramlar (NotFound/Forbid) bilmeden sonuç döndürebilmesi için;
    // controller bu Status'e bakıp uygun IActionResult'a çevirir.
    public class ServiceResult<T>
    {
        public ServiceResultStatus Status { get; init; }
        public T? Data { get; init; }
        public string? ErrorMessage { get; init; }

        public static ServiceResult<T> Ok(T data) =>
            new() { Status = ServiceResultStatus.Ok, Data = data };

        public static ServiceResult<T> NotFound() =>
            new() { Status = ServiceResultStatus.NotFound };

        public static ServiceResult<T> Forbidden() =>
            new() { Status = ServiceResultStatus.Forbidden };

        public static ServiceResult<T> Invalid(string message) =>
            new() { Status = ServiceResultStatus.ValidationError, ErrorMessage = message };
    }
}
