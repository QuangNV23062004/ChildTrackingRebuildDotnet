using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RestAPI.Models.SubModels;

namespace RestAPI.Models
{
    // Enum for GrowthMetricsForAgeEnum, serialized as string in MongoDB
    public enum GrowthMetricsForAgeEnum
    {
        [BsonRepresentation(BsonType.String)]
        WFA,

        [BsonRepresentation(BsonType.String)]
        LHFA,

        [BsonRepresentation(BsonType.String)]
        BFA,

        [BsonRepresentation(BsonType.String)]
        HCFA,

        [BsonRepresentation(BsonType.String)]
        ACFA,
    }

    public class Age
    {
        [BsonElement("inMonths")]
        public double InMonths { get; set; }

        [BsonElement("inDays")]
        public double InDays { get; set; }
    }



    public class GrowthMetricForAgeModel : BaseModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("age")]
        public Age Age { get; set; } = null!;

        [BsonElement("gender")]
        [BsonRepresentation(BsonType.Int32)]
        public GenderEnum Gender { get; set; }

        [BsonElement("type")]
        [BsonRepresentation(BsonType.String)]
        public GrowthMetricsForAgeEnum Type { get; set; }

        [BsonElement("percentiles")]
        public Percentiles Percentiles { get; set; } = null!;
    }


}