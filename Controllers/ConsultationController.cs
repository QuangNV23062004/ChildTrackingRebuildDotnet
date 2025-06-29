using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RestAPI.Dtos;
using RestAPI.Helpers;
using RestAPI.Services.interfaces;

namespace RestAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsultationController(IConsultationService _consultationService) : ControllerBase
    {
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> UpdateConsultationStatus(
            string id,
            [FromBody] ConsultationDto.UpdateConsultationStatusDto requestDto
        )
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;
                var result = await _consultationService.UpdateConsultationStatus(
                    id,
                    requestDto.Status,
                    requester
                );
                return Ok(new { result, message = "Consultation status updated successfully" });
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "User,Doctor,Admin")]
        public async Task<IActionResult> GetConsultationById(string id)
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;
                var consultation = await _consultationService.GetConsultationById(id, requester);
                return Ok(new { consultation, message = "Consultation retrieved successfully" });
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet("doctor/{id}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> getConsultationByDoctorId(
            string id,
            [FromQuery] QueryParams query,
            [FromQuery] string? status
        )
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;
                var consultations = await _consultationService.GetConsultationsByDoctorId(
                    id,
                    query,
                    status
                );
                return Ok(new { consultations, message = "Consultations retrieved successfully" });
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet("member/{id}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> getConsultationByMemberId(
            string id,
            [FromQuery] QueryParams query,
            [FromQuery] string? status
        )
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;
                var consultations = await _consultationService.GetConsultationsByMemberId(
                    id,
                    query,
                    status
                );
                return Ok(new { consultations, message = "Consultations retrieved successfully" });
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> getAllConsultations(
            [FromQuery] QueryParams query,
            [FromQuery] string? status
        )
        {
            try
            {
                var consultations = await _consultationService.GetConsultations(query, status);
                return Ok(new { consultations, message = "Consultations retrieved successfully" });
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPatch("{id}/rating")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> rateConsultation(
            string id,
            [FromBody] ConsultationDto.RateConsultationDto requestDto
        )
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;
                var result = await _consultationService.RateConsultationById(
                    id,
                    requestDto.Rating,
                    requester
                );
                return Ok(new { result, message = "Consultation rated successfully" });
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}
