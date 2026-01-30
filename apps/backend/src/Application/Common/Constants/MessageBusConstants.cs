namespace backend.Application.Common.Constants;

public static class MessageBusConstants
{
    public static class Exchanges
    {
        public const string MemberFace = "face_exchange";
        // Add other exchanges here
    }

    public static class RoutingKeys
    {
        public const string MemberFaceAdded = "face.add";
        public const string MemberFaceDeleted = "face.delete";
        // Add other routing keys here
    }
}
