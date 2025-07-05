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
    public class RequestController(IRequestService _requestService) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateRequest(
            [FromBody] RequestDto.CreateRequestDto requestDto
        )
        {
            var requester = HttpContext.Items["UserInfo"] as UserInfo;

            var request = new RequestModel
            {
                ChildId = requestDto.ChildId,
                DoctorId = requestDto.DoctorId,
                Message = requestDto.Message,
            };

            var result = await _requestService.CreateRequest(request, requester);
            return StatusCode(
                StatusCodes.Status201Created,
                new { result, message = "Request created successfully" }
            );
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> DeleteRequest(string id)
        {
            var requester = HttpContext.Items["UserInfo"] as UserInfo;
            var result = await _requestService.DeleteRequest(id, requester);
            return Ok(new { result, message = "Request deleted successfully" });
        }

        // [HttpPut("{id}")]
        // [Authorize(Roles = "User,Admin")]
        // [ApiExplorerSettings(IgnoreApi = true)]
        // public async Task<IActionResult> UpdateRequest(
        //     string id,
        //     [FromBody] RequestDto.UpdateRequestDto requestDto
        // )
        // {
        //     try
        //     {
        //         var requester = HttpContext.Items["UserInfo"] as UserInfo;
        //         var request = new RequestModel
        //         {
        //             ChildId = requestDto.ChildId,
        //             DoctorId = requestDto.DoctorId,
        //             Message = requestDto.Message,
        //         };
        //         var result = await _requestService.UpdateRequest(id, request, requester);
        //         return Ok(new { result, message = "Request updated successfully" });
        //     }
        //     catch (System.Exception)
        //     {
        //         throw;
        //     }
        // }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "User,Doctor,Admin")]
        public async Task<IActionResult> UpdateRequestStatus(
            string id,
            [FromBody] RequestDto.UpdateRequestStatusDto requestDto
        )
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;
                var result = await _requestService.UpdateRequestStatus(
                    id,
                    Enum.Parse<RequestStatusEnum>(requestDto.Status),
                    requester
                );
                return Ok(new { result, message = "Request status updated successfully" });
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "User,Doctor,Admin")]
        public async Task<IActionResult> GetRequestById(string id)
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;
                var result = await _requestService.GetRequestById(id, requester);
                return Ok(new { data = result, message = "Request retrieved successfully" });
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet("member/{id}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetRequestsByMemberId(
            string id,
            [FromQuery] QueryParams query,
            [FromQuery] string? status
        )
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;
                var result = await _requestService.GetRequestsByMemberId(
                    id,
                    query,
                    requester,
                    status
                );
                return Ok(
                    new
                    {
                        data = result.Data,
                        page = result.Page,
                        total = result.Total,
                        totalPages = result.TotalPages,
                        message = "Requests retrieved successfully",
                    }
                );
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet("doctor/{id}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> GetRequestsByDoctorId(
            string id,
            [FromQuery] QueryParams query
        )
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;
                var result = await _requestService.GetRequestsByDoctorId(id, query, requester);
                return Ok(
                    new
                    {
                        data = result.Data,
                        page = result.Page,
                        total = result.Total,
                        totalPages = result.TotalPages,
                        message = "Requests retrieved successfully",
                    }
                );
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllRequests(
            [FromQuery] QueryParams query,
            [FromQuery] string? status
        )
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;
                var result = await _requestService.GetAllRequests(query, requester, status);
                return Ok(
                    new
                    {
                        data = result.Data,
                        page = result.Page,
                        total = result.Total,
                        totalPages = result.TotalPages,
                        message = "Requests retrieved successfully",
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
