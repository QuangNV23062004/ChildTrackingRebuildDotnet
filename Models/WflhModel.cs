using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using RestAPI.Enums;
using RestAPI.Models.SubModels;
namespace RestAPI.Models
{
    public class WflhModel : BaseModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("height")]
        [BsonRequired] // Enforce required field
        public double Height { get; set; }

        [BsonElement("gender")]
        [BsonRequired] // Enforce required field
        public GenderEnum Gender { get; set; }

        [BsonElement("percentiles")]
        [BsonRequired] // Enforce required field
        public Percentiles Percentiles { get; set; } = null!; // Non-nullable
    }
}