using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RestAPI.Enums;
using RestAPI.Models.SubModels;

namespace RestAPI.Models;

public class ChildModel : BaseModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("birthDate")]
    public DateTime BirthDate { get; set; }

    [BsonElement("note")]
    public string Note { get; set; } = "N/A";

    [BsonElement("gender")]
    [BsonRepresentation(BsonType.Int32)]
    public GenderEnum Gender { get; set; }

    [BsonElement("feedingType")]
    [BsonRepresentation(BsonType.String)]
    public FeedingTypeEnum FeedingType { get; set; } = FeedingTypeEnum.NA;

    [BsonElement("allergies")]
    [BsonRepresentation(BsonType.String)]
    public List<AllergyEnum> Allergies { get; set; } = new() { AllergyEnum.NA };

    [BsonElement("guardianId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string GuardianId { get; set; } = null!;

    public List<GrowthVelocityResult> GrowthVelocityResult { get; set; } = new();

    [BsonElement("guardian")]
    public UserModel? Guardian { get; set; }
}
