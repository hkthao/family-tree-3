﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Domain.Common;

public abstract class BaseAuditableEntity : BaseEntity
{
    [BsonElement("Created")]
    public DateTime Created { get; set; } = DateTime.UtcNow;

    public string? CreatedBy { get; set; }

    public DateTime? LastModified { get; set; }

    public string? LastModifiedBy { get; set; }
}
