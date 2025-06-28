using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RestAPI.Models.SubModels;

namespace RestAPI.Models
{
    // Enum for GrowthVelocityEnum, serialized as string in MongoDB
    public enum GrowthVelocityEnum
    {
        [BsonRepresentation(BsonType.String)]
        WV,

        [BsonRepresentation(BsonType.String)]
        HV,

        [BsonRepresentation(BsonType.String)]
        BV,

        [BsonRepresentation(BsonType.String)]
        HCV,

        [BsonRepresentation(BsonType.String)]
        ACV
    }

    public class Interval
    {
        [BsonElement("inMonths")]
        public double InMonths { get; set; }

        [BsonElement("inWeeks")]
        public double InWeeks { get; set; }

        [BsonElement("inDays")]
        public double InDays { get; set; }
    }




    public class GrowthVelocityModel : BaseModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("firstInterval")]
        public Interval FirstInterval { get; set; } = null!;

        [BsonElement("lastInterval")]
        public Interval LastInterval { get; set; } = null!;

        [BsonElement("gender")]
        [BsonRepresentation(BsonType.Int32)]
        public GenderEnum Gender { get; set; }

        [BsonElement("type")]
        [BsonRepresentation(BsonType.String)]
        public GrowthVelocityEnum Type { get; set; }

        [BsonElement("percentiles")]
        public Percentiles Percentiles { get; set; } = null!;
    }
}