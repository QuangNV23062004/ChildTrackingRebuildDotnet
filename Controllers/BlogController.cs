using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestAPI.Dtos;
using RestAPI.Enums;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Services.interfaces;

namespace RestAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogController(IBlogService _blogService) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateBlog([FromForm] BlogDto.BlogCreateDto createBlogDto)
        {
            try
            {
                var userInfo = HttpContext.Items["UserInfo"] as UserInfo;
                if (userInfo == null)
                {
                    return Unauthorized();
                }
                var blog = new BlogModel
                {
                    Title = createBlogDto.Title,
                    Content = createBlogDto.Content,
                    UserId = userInfo.UserId,
                    Status = createBlogDto.Status ?? BlogStatus.Draft,
                };

                var createdBlog = await _blogService.CreateBlog(
                    blog,
                    createBlogDto.ImageUrl,
                    createBlogDto.Attachments
                );
                return StatusCode(
                    StatusCodes.Status201Created,
                    new { blog = createdBlog, message = "Blog created successful" }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBlogStatus(
            string id,
            BlogDto.UpdateBlogStatusDto updateBlogStatusDto
        )
        {
            try
            {
                var blog = await _blogService.UpdateBlogStatus(id, updateBlogStatusDto.Status);
                return Ok(new { blog = blog, message = "Update blog status successful" });
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBlogs(
            [FromQuery] QueryParams query,
            [FromQuery] string? Status
        )
        {
            try
            {
                var UserInfo = HttpContext.Items["UserInfo"] as UserInfo;
                var blogResult = await _blogService.GetBlogs(query, UserInfo, Status);

                return Ok(
                    new
                    {
                        data = blogResult.Data,
                        page = blogResult.Page,
                        total = blogResult.Total,
                        totalPages = blogResult.TotalPages,
                        message = "Get blogs successfully",
                    }
                );
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBlog(string id)
        {
            try
            {
                var UserInfo = HttpContext.Items["UserInfo"] as UserInfo;

                var blog = await _blogService.GetBlogById(id, UserInfo);
                return Ok(new { blog = blog, message = "Get blog successfully" });
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}
