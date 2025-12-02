namespace backend.Application.Common.Constants;

public static class ErrorMessages
{
    public const string AccessDenied = "Access denied. You do not have permission to perform this action.";
    public const string NotFound = "{0} not found.";
    public const string Unauthorized = "User is not authenticated.";
    public const string EventNotFound = "Event with ID {0} not found.";
    public const string FamilyNotFound = "Family with ID {0} not found.";
    public const string UserProfileNotFound = "User profile not found.";
    public const string NoAIResponse = "AI did not return a response.";
    public const string MultipleFamiliesFound = "Multiple families found with the given name or code. Please specify.";
    public const string MultipleMembersFound = "Multiple members found with the given name or code. Please specify.";
    public const string InvalidAIResponse = "AI generated invalid response: {0}";
    public const string UnexpectedError = "An unexpected error occurred: {0}";
    public const string FileEmpty = "File is empty.";
    public const string FileSizeExceedsLimit = "File size exceeds the maximum limit of {0} MB.";
    public const string InvalidFileType = "Invalid file type. Only JPG, JPEG, PNG, PDF, DOCX are allowed.";
    public const string FileUploadFailed = "File upload failed.";
    public const string FileUploadNullUrl = "File upload succeeded but returned a null URL.";
    public const string InvalidBase64 = "Invalid Base64 string for image data.";
    public const string FaceThumbnailUploadFailed = "Face thumbnail upload failed.";
    public const string ExternalIdNotFound = "External ID (sub claim) not found in claims.";
    public const string InvalidUserIdFormat = "Invalid user ID format.";
}
