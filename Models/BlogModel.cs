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
    public class BlogModel : BaseModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("title")]
        [Required]
        public string Title { get; set; } = null!;

        [BsonElement("content")]
        [Required]
        public string Content { get; set; }

        [BsonElement("imageUrl")]
        public string ImageUrl { get; set; }

        [BsonElement("contentImageUrls")]
        public List<string> ContentImageUrls { get; set; } = new List<string>();

        [BsonElement("status")]
        public BlogStatus Status { get; set; } = BlogStatus.Draft;

        [BsonElement("userId")]
        [Required]
        public string UserId { get; set; }

        [BsonElement("user")]
        public UserModel? User { get; set; }
    }
}
