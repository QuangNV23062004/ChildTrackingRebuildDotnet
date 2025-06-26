using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RestAPI.Models.SubModels;

public class BaseModel
{
    [BsonElement("isDeleted"), BsonRepresentation(BsonType.Boolean)]
    public bool IsDeleted { get; set; } = false;
    [BsonElement("createdAt"), BsonRepresentation(BsonType.DateTime)]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt"), BsonRepresentation(BsonType.DateTime)]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
