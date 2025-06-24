using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RestAPI.Models
{
    public class UserModel
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name"), BsonRepresentation(BsonType.String)]
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = "";

        [BsonElement("email"), BsonRepresentation(BsonType.String)]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = "";

        [BsonElement("password"), BsonRepresentation(BsonType.String)]
        [Required(ErrorMessage = "Password is required")]
        [StringLength(256, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; } = "";

        [BsonElement("role"), BsonRepresentation(BsonType.Int32)]
        [Range(0, int.MaxValue, ErrorMessage = "Role must be a non-negative integer")]
        public int Role { get; set; } = 0;

        [BsonElement("isDeleted"), BsonRepresentation(BsonType.Boolean)]
        public bool IsDeleted { get; set; } = false;

        [BsonElement("createdAt"), BsonRepresentation(BsonType.DateTime)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt"), BsonRepresentation(BsonType.DateTime)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}