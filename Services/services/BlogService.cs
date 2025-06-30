using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using RestAPI.Enums;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Models.SubModels;
using RestAPI.Repositories.interfaces;
using RestAPI.Services.interfaces;

namespace RestAPI.Services.services
{
    public class BlogService(IBlogRepository blogRepository, ICloudinary cloudinary) : IBlogService
    {
        public async Task<BlogModel> CreateBlog(
            BlogModel blog,
            IFormFile imageUrl,
            IFormFileCollection attachments
        )
        {
            var uploadedImageUrl = await CloudinaryHelper.UploadImage(
                imageUrl,
                cloudinary,
                "blogImageUrl"
            );

            var uploadedAttachments = new List<string>();
            if (attachments != null && attachments.Count > 0)
            {
                uploadedAttachments = await CloudinaryHelper.UploadImages(
                    attachments,
                    cloudinary,
                    "blogAttachments"
                );
            }

            blog.ImageUrl = uploadedImageUrl;
            blog.ContentImageUrls = uploadedAttachments;
            blog.Content = CloudinaryHelper.FormatBlogContent(blog.Content, uploadedAttachments);

            return await blogRepository.CreateAsync(blog);
        }

        public async Task<BlogModel> DeleteBlog(string id)
        {
            try
            {
                var blog = await blogRepository.DeleteAsync(id);
                if (blog == null)
                {
                    throw new KeyNotFoundException("Blog not found");
                }
                return blog;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<BlogModel> GetBlogById(string id, UserInfo userInfo)
        {
            try
            {
                var blog = await blogRepository.GetByIdAsync(
                    id,
                    new PopulationModel[]
                    {
                        new PopulationModel
                        {
                            LocalField = "userId",
                            ForeignField = "_id",
                            Collection = "users",
                            As = "user",
                        },
                    }
                );

                if (blog == null)
                {
                    throw new KeyNotFoundException("Blog not found");
                }

                if (blog.Status != BlogStatus.Published)
                {
                    throw new KeyNotFoundException("Blog not found");
                }
                return blog;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<PaginationResult<BlogModel>> GetBlogs(
            QueryParams query,
            UserInfo userInfo,
            string? status
        )
        {
            try
            {
                var mode = "User";
                BlogStatus? statusFilter = null;
                if (userInfo != null && Enum.Parse<RoleEnum>(userInfo.Role) == RoleEnum.Admin)
                {
                    mode = "Admin";
                    if (
                        !string.IsNullOrEmpty(status)
                        && Enum.TryParse<BlogStatus>(status, out var parsedStatus)
                    )
                    {
                        statusFilter = parsedStatus;
                    }
                }
                else
                {
                    statusFilter = BlogStatus.Published;
                }
                var blogs = await blogRepository.GetAllAsyncWithPagination(
                    query,
                    new PopulationModel[]
                    {
                        new PopulationModel
                        {
                            LocalField = "userId",
                            ForeignField = "_id",
                            Collection = "users",
                            As = "user",
                        },
                    },
                    statusFilter
                );
                return blogs;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<BlogModel> UpdateBlogStatus(string id, BlogStatus status)
        {
            try
            {
                var checkBlog = await blogRepository.GetByIdAsync(id);
                if (checkBlog == null)
                {
                    throw new KeyNotFoundException("Blog not found");
                }

                checkBlog.Status = status;
                var blog = await blogRepository.UpdateAsync(id, checkBlog);
                if (blog == null)
                {
                    throw new KeyNotFoundException("Blog not found");
                }
                return blog;
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}
