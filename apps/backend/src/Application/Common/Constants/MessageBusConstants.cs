namespace backend.Application.Common.Constants;

public static class MessageBusConstants
{
    public static class Exchanges
    {
        public const string MemberFace = "face_exchange";
        public const string FileUpload = "file_upload_exchange"; // NEW
        // Add other exchanges here
    }

    public static class RoutingKeys
    {
        public const string MemberFaceAdded = "face.add";
        public const string MemberFaceDeleted = "face.delete";
        public const string FileUploadRequested = "file.upload.requested"; // NEW
        public const string FileUploadCompleted = "file.upload.completed";
        public const string FileDeletionRequested = "file.deletion.requested"; // NEW
        public const string FileDeletionCompleted = "file.deletion.completed"; // NEW
        // Add other routing keys here
    }
}
