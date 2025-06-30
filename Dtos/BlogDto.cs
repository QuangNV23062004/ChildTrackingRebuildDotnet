using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using RestAPI.Enums;

namespace RestAPI.Dtos
{
    public class BlogDto
    {
        public class BlogCreateDto
        {
            public string Title { get; set; }
            public string Content { get; set; }
            public BlogStatus? Status { get; set; }
            public IFormFile ImageUrl { get; set; }
            public IFormFileCollection Attachments { get; set; }
        }

        public class UpdateBlogStatusDto
        {
            public BlogStatus Status { get; set; }
        }
    }
}
