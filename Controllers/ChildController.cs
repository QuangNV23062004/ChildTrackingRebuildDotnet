using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> CreateChild([FromBody] ChildModel childData)
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;
                var created = await _childService.CreateChildAsync(requester!, childData);

                return Ok(new
                {
                    message = "Child created successfully",
                    child = created
                });
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        [HttpPut("{childId}")]
        public async Task<IActionResult> UpdateChild(string childId, [FromBody] ChildModel updateData)
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;
                var updated = await _childService.UpdateChildAsync(childId, requester!, updateData);

                return Ok(new
                {
                    message = "Child updated successfully",
                    updatedChild = updated
                });
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        [HttpDelete("{childId}")]
        public async Task<IActionResult> DeleteChild(string childId)
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;
                await _childService.DeleteChildAsync(childId, requester!);

                return Ok(new
                {
                    message = "Child deleted successfully"
                });
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        [HttpGet("{childId}")]
        public async Task<IActionResult> GetChildById(string childId)
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;
                var child = await _childService.GetChildByIdAsync(childId, requester!);

                return Ok(new
                {
                    message = "Child retrieved successfully",
                    child
                });
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetChildrenByUserId(string id, [FromQuery] QueryParams query)
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;
                var result = await _childService.GetChildrenByUserIdAsync(id, requester!, query);

                return Ok(new
                {
                    message = "Children retrieved successfully",
                    children = result.Data,
                    page = result.Page,
                    total = result.Total,
                    totalPages = result.TotalPages
                });
            }
            catch (System.Exception)
            {

                throw;
            }
        }
    }
}