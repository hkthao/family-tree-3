using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.UserPushTokens.Specifications;

public class UserPushTokenOrderingSpecification : Specification<UserPushToken>
{
    public UserPushTokenOrderingSpecification(string? sortBy, string? sortOrder)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            Query.OrderBy(upt => upt.Created); // Default sort
            return;
        }

        switch (sortBy.ToLower())
        {
            case "expopushtoken":
                if (sortOrder?.ToLower() == "desc")
                {
                    Query.OrderByDescending(upt => upt.ExpoPushToken);
                }
                else
                {
                    Query.OrderBy(upt => upt.ExpoPushToken);
                }
                break;
            case "platform":
                if (sortOrder?.ToLower() == "desc")
                {
                    Query.OrderByDescending(upt => upt.Platform);
                }
                else
                {
                    Query.OrderBy(upt => upt.Platform);
                }
                break;
            case "deviceid":
                if (sortOrder?.ToLower() == "desc")
                {
                    Query.OrderByDescending(upt => upt.DeviceId);
                }
                else
                {
                    Query.OrderBy(upt => upt.DeviceId);
                }
                break;
            case "userid":
                if (sortOrder?.ToLower() == "desc")
                {
                    Query.OrderByDescending(upt => upt.UserId);
                }
                else
                {
                    Query.OrderBy(upt => upt.UserId);
                }
                break;
            case "created": // From BaseAuditableEntity
                if (sortOrder?.ToLower() == "desc")
                {
                    Query.OrderByDescending(upt => upt.Created);
                }
                else
                {
                    Query.OrderBy(upt => upt.Created);
                }
                break;
            default:
                Query.OrderBy(upt => upt.Created); // Fallback to default sort
                break;
        }
    }
}
