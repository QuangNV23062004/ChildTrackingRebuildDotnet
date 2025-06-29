using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RestAPI.Enums;
using RestAPI.Models.SubModels;

namespace RestAPI.Models
{
    public class RequestModel : BaseModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("memberId"), BsonRepresentation(BsonType.ObjectId)]
        [Required(ErrorMessage = "Member ID is required")]
        public string MemberId { get; set; } = "";

        [BsonElement("childId"), BsonRepresentation(BsonType.ObjectId)]
        public string ChildId { get; set; } = "";

        [BsonElement("doctorId"), BsonRepresentation(BsonType.ObjectId)]
        [Required(ErrorMessage = "Doctor ID is required")]
        public string DoctorId { get; set; } = "";

        [BsonElement("status"), BsonRepresentation(BsonType.String)]
        [Required(ErrorMessage = "Status is required")]
        public RequestStatusEnum Status { get; set; } = RequestStatusEnum.Pending;

        [BsonElement("message"), BsonRepresentation(BsonType.String)]
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Message { get; set; } = "";

        [BsonElement("doctor")]
        public UserModel? Doctor { get; set; }

        [BsonElement("member")]
        public UserModel? Member { get; set; }

        [BsonElement("child")]
        public ChildModel? Child { get; set; }
    }
}
