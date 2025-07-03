using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RestAPI.Models.SubModels;

namespace RestAPI.Models
{
    public class UserModel : BaseModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
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
        [StringLength(
            256,
            MinimumLength = 8,
            ErrorMessage = "Password must be at least 8 characters"
        )]
        public string Password { get; set; } = "";

        [BsonElement("role"), BsonRepresentation(BsonType.String)]
        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = "User";

        [BsonElement("rating"), BsonRepresentation(BsonType.Double)]
        public double? Rating { get; set; } = 0;
    }
}
