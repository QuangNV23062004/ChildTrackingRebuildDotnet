using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RestAPI.Models.SubModels;

namespace RestAPI.Models
{
    // Enum for LevelEnum, serialized as string in MongoDB
    public enum LevelEnum
    {
        [BsonRepresentation(BsonType.String)]
        Low,

        [BsonRepresentation(BsonType.String)]
        BelowAverage,

        [BsonRepresentation(BsonType.String)]
        Average,

        [BsonRepresentation(BsonType.String)]
        AboveAverage,

        [BsonRepresentation(BsonType.String)]
        High,

        [BsonRepresentation(BsonType.String)]
        NA,
    }

    // Enum for BmiLevelEnum, serialized as string in MongoDB
    public enum BmiLevelEnum
    {
        [BsonRepresentation(BsonType.String)]
        Underweight,

        [BsonRepresentation(BsonType.String)]
        HealthyWeight,

        [BsonRepresentation(BsonType.String)]
        Overweight,

        [BsonRepresentation(BsonType.String)]
        Obese,

        [BsonRepresentation(BsonType.String)]
        NA,
    }

    public class GrowthResult
    {
        [BsonElement("weight")]
        public GrowthMetric Weight { get; set; } = null!;

        [BsonElement("height")]
        public GrowthMetric Height { get; set; } = null!;

        [BsonElement("bmi")]
        public BmiMetric Bmi { get; set; } = null!;

        [BsonElement("headCircumference")]
        public GrowthMetric HeadCircumference { get; set; } = null!;

        [BsonElement("armCircumference")]
        public GrowthMetric ArmCircumference { get; set; } = null!;

        [BsonElement("weightForLength")]
        public GrowthMetric? WeightForLength { get; set; } = null!;

        [BsonElement("description")]
        public string Description { get; set; } = null!;

        [BsonElement("level")]
        [BsonRepresentation(BsonType.String)]
        public LevelEnum Level { get; set; }
    }

    public class GrowthMetric
    {
        [BsonElement("percentile")]
        public double Percentile { get; set; }

        [BsonElement("description")]
        public string Description { get; set; } = null!;

        [BsonElement("level")]
        [BsonRepresentation(BsonType.String)]
        public LevelEnum Level { get; set; }
    }

    public class BmiMetric
    {
        [BsonElement("percentile")]
        public double Percentile { get; set; }

        [BsonElement("description")]
        public string Description { get; set; } = null!;

        [BsonElement("level")]
        [BsonRepresentation(BsonType.String)]
        public BmiLevelEnum Level { get; set; }
    }

    public class GrowthDataModel : BaseModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("childId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ChildId { get; set; } = null!;

        [BsonElement("inputDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime InputDate { get; set; }

        [BsonElement("weight")]
        public double Weight { get; set; }

        [BsonElement("bmi")]
        public double? Bmi { get; set; }

        [BsonElement("height")]
        public double Height { get; set; }

        [BsonElement("headCircumference")]
        public double? HeadCircumference { get; set; }

        [BsonElement("armCircumference")]
        public double? ArmCircumference { get; set; }

        [BsonElement("growthResult")]
        public GrowthResult? GrowthResult { get; set; }

        [BsonElement("child")]
        public ChildModel? Child { get; set; }
    }
}
