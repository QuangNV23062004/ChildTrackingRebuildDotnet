using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RestAPI.Models.SubModels;

namespace RestAPI.Models
{
    public class ConsultationMessageModel : BaseModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string? Id { get; set; }

        [Required]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("consultationId")]
        public string ConsultationId { get; set; } = "";

        [Required]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("senderId")]
        public string SenderId { get; set; } = "";

        [Required]
        [BsonElement("message")]
        public string Message { get; set; } = "";

        [BsonElement("sender")]
        public UserModel? Sender { get; set; }
    }
}
