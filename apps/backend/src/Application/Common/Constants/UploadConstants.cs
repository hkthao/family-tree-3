namespace backend.Application.Common.Constants;

public static class UploadConstants
{
    public const string TemporaryUploadsFolder = "temp/uploads";
    public const string FamilyStoryPhotoFolder = "gpv-app/families/{0}/stories/photos"; // Use {0} for string.Format
    public const string FamilyFaceFolder = "gpv-app/families/{0}/faces";
    public const string FamilyAvatarFolder = "gpv-app/families/{0}/avatar";
    public const string MemberAvatarFolder = "gpv-app/families/{0}/member-avatar";
    public const string UserAvatarFolder = "gpv-app/user/avatar";
}
