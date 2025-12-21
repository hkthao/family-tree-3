namespace backend.Application.Common.Constants;

public static class ErrorSources
{
    public const string Forbidden = "Forbidden";
    public const string NotFound = "NotFound";
    public const string Authentication = "Authentication";
    public const string Database = "Database";
    public const string Exception = "Exception";
    public const string FileStorage = "FileStorage";
    public const string FileUpload = "FileUpload";
    public const string Validation = "Validation";
    public const string NoContent = "NoContent";
    public const string BadRequest = "BadRequest";
    public const string Conflict = "Conflict"; // NEW
    public const string ExternalServiceError = "ExternalServiceError";
    public const string InternalError = "InternalError";
}
