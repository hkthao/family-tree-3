using MongoDB.Bson;

namespace backend.Application.Common.Interfaces;

public interface IUser
{
    ObjectId? Id { get; }
    List<string>? Roles { get; }

}
