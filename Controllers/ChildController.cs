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
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User,Admin")] // Only users with role 0 or 1 can access this controller
    public class ChildController(IChildService _childService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateChild([FromBody] ChildDto.CreateChildDto childData)
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;

                var childModel = new ChildModel
                {
                    Name = childData.Name,
                    Gender = (GenderEnum)childData.Gender,
                    BirthDate = childData.BirthDate,
                    Note = childData.Note,
                    FeedingType = childData.FeedingType,
                    Allergies = childData.Allergies,
                    GuardianId = requester!.UserId,
                };

                var created = await _childService.CreateChildAsync(requester!, childModel);

                return StatusCode(
                    StatusCodes.Status201Created,
                    new { message = "Child created successfully", child = created }
                );
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChild(
            string id,
            [FromBody] ChildDto.UpdateChildDto updateData
        )
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;

                var childModel = new ChildModel
                {
                    Id = id,
                    Name = updateData.Name,
                    Gender = (GenderEnum)updateData.Gender,
                    BirthDate = updateData.BirthDate,
                    Note = updateData.Note,
                    FeedingType = updateData.FeedingType,
                    Allergies = updateData.Allergies,
                };

                var updated = await _childService.UpdateChildAsync(id, requester!, childModel);

                return Ok(new { message = "Child updated successfully", updatedChild = updated });
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChild(string id)
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;
                await _childService.DeleteChildAsync(id, requester!);

                return Ok(new { message = "Child deleted successfully" });
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetChildById(string id)
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;
                var child = await _childService.GetChildByIdAsync(id, requester!);

                return Ok(new { message = "Child retrieved successfully", child });
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetChildrenByUserId(
            string id,
            [FromQuery] QueryParams query
        )
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;
                var result = await _childService.GetChildrenByUserIdAsync(id, requester!, query);

                return Ok(
                    new
                    {
                        message = "Children retrieved successfully",
                        data = result.Data,
                        page = result.Page,
                        total = result.Total,
                        totalPages = result.TotalPages,
                    }
                );
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}
