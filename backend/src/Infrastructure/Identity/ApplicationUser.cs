using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson;

namespace backend.Infrastructure.Identity;

public class ApplicationUser : MongoIdentityUser<ObjectId>
{
}
