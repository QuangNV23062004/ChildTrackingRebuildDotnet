using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestAPI.Dtos;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Services.interfaces;

namespace RestAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsultationMessageController(
        IConsultationMessageService _consultationMessageService
    ) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = "User,Doctor")]
        public async Task<IActionResult> CreateConsultationMessage(
            [FromBody] ConsultationMessageDto.CreateConsultationMessageDto requestDto
        )
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;
                var message = new ConsultationMessageModel
                {
                    Message = requestDto.Message,
                    ConsultationId = requestDto.ConsultationId,
                };
                var result = await _consultationMessageService.CreateConsultationMessage(
                    message,
                    requester
                );
                return StatusCode(
                    StatusCodes.Status201Created,
                    new { result, message = "Consultation message created successfully" }
                );
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "User,Doctor")]
        public async Task<IActionResult> GetConsultationMessageById(string id)
        {
            try
            {
                var result = await _consultationMessageService.GetConsultationMessageById(id);
                return Ok(result);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet("consultation/{id}")]
        [Authorize(Roles = "User,Doctor")]
        public async Task<IActionResult> GetConsultationMessages(
            string id,
            [FromQuery] QueryParams query
        )
        {
            try
            {
                var result = await _consultationMessageService.GetConsultationMessages(id, query);
                return Ok(result);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "User,Doctor")]
        public async Task<IActionResult> DeleteConsultationMessage(string id)
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;
                var result = await _consultationMessageService.DeleteConsultationMessage(
                    id,
                    requester
                );
                return Ok(result);
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}
