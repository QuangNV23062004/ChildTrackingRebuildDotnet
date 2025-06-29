using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RestAPI.Enums;
using RestAPI.Models.SubModels;

namespace RestAPI.Models
{
    public class ConsultationModel : BaseModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("requestId"), BsonRepresentation(BsonType.ObjectId)]
        [Required(ErrorMessage = "request ID is required")]
        public string RequestId { get; set; } = "";

        [BsonElement("status"), BsonRepresentation(BsonType.String)]
        [Required(ErrorMessage = "status is required")]
        public ConsultationStatusEnum Status { get; set; } = ConsultationStatusEnum.Ongoing;

        [BsonElement("rating"), BsonRepresentation(BsonType.Int32)]
        public int Rating { get; set; } = 0;

        [BsonElement("request")]
        public RequestModel? Request { get; set; }
    }
}
