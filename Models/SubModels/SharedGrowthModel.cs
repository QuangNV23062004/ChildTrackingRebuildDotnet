using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace RestAPI.Models.SubModels
{
    public class PercentileValue
    {
        [BsonElement("percentile")]
        public double Percentile { get; set; }

        [BsonElement("value")]
        public double Value { get; set; }
    }

    public class Percentiles
    {
        [BsonElement("L")]
        public double L { get; set; }

        [BsonElement("M")]
        public double M { get; set; }

        [BsonElement("S")]
        public double S { get; set; }

        [BsonElement("delta")]
        [BsonIgnoreIfNull]
        public double? Delta { get; set; }

        [BsonElement("values")]
        public List<PercentileValue> Values { get; set; } = new List<PercentileValue>();
    }
    public class GrowthVelocityResult
    {
        public string? Period { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public GrowthMetric? Weight { get; set; }
        public GrowthMetric? Height { get; set; }
        public GrowthMetric? HeadCircumference { get; set; }

    }
}