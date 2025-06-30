using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestAPI.Enums;
using RestAPI.Models.SubModels;

namespace RestAPI.Models
{
    public class BlogModel : BaseModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public List<string> ContentImageUrls { get; set; } = new List<string>();
        public BlogStatus Status { get; set; }
        public string UserId { get; set; }
        public UserModel? User { get; set; }
    }
}
