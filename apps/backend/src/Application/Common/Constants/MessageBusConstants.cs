namespace backend.Application.Common.Constants;

public static class MessageBusConstants
{
    public static class Exchanges
    {
        public const string MemberFace = "member-face-exchange";
        // Add other exchanges here
    }

    public static class RoutingKeys
    {
        public const string MemberFaceDeleted = "member-face.deleted";
        // Add other routing keys here
    }
}
