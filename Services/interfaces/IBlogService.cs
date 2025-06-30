using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestAPI.Enums;
using RestAPI.Helpers;
using RestAPI.Models;

namespace RestAPI.Services.interfaces
{
    public interface IBlogService
    {
        Task<BlogModel> CreateBlog(
            BlogModel blog,
            IFormFile imageUrl,
            IFormFileCollection attachments
        );
        Task<BlogModel> UpdateBlogStatus(string id, BlogStatus status);
        Task<BlogModel> GetBlogById(string id, UserInfo userInfo);
        Task<PaginationResult<BlogModel>> GetBlogs(
            QueryParams query,
            UserInfo userInfo,
            string? status
        );

        Task<BlogModel> DeleteBlog(string id);
    }
}
