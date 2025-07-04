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
    public class GrowthDataController(IGrowthDataService _growthDataService) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> CreateGrowthData(
            [FromBody] GrowthDataDto.CreateGrowthDataDto growthDataDto,
            string childId
        )
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;

                var growthData = new GrowthDataModel
                {
                    Height = growthDataDto.Height,
                    Weight = growthDataDto.Weight,
                    HeadCircumference = growthDataDto.HeadCircumference,
                    ArmCircumference = growthDataDto.ArmCircumference,
                    InputDate = growthDataDto.InputDate,
                };
                var (createdGrowthData, growthVelocity) =
                    await _growthDataService.CreateGrowthDataAsync(requester!, childId, growthData);
                return Ok(
                    new
                    {
                        growthData = createdGrowthData,
                        growthVelocity = growthVelocity,
                        message = "Growth data created successfully",
                    }
                );
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> UpdateGrowthData(
            string id,
            [FromBody] GrowthDataDto.UpdateGrowthDataDto growthDataDto
        )
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;

                var growthData = new GrowthDataModel
                {
                    Height = growthDataDto.Height,
                    Weight = growthDataDto.Weight,
                    HeadCircumference = growthDataDto.HeadCircumference,
                    ArmCircumference = growthDataDto.ArmCircumference,
                    InputDate = growthDataDto.InputDate,
                };
                var updatedGrowthData = await _growthDataService.UpdateGrowthDataAsync(
                    id,
                    requester!,
                    growthData
                );
                return StatusCode(
                    StatusCodes.Status201Created,
                    new
                    {
                        growthData = updatedGrowthData,
                        message = "Growth data updated successfully",
                    }
                );
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> DeleteGrowthData(string id)
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;
                var deletedGrowthData = await _growthDataService.DeleteGrowthDataAsync(
                    id,
                    requester!
                );
                return Ok(
                    new
                    {
                        growthData = deletedGrowthData,
                        message = "Growth data deleted successfully",
                    }
                );
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "User,Admin,Doctor")]
        public async Task<IActionResult> GetGrowthDataById(string id)
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;
                var growthData = await _growthDataService.GetGrowthDataByIdAsync(id, requester!);
                return Ok(
                    new { growthData = growthData, message = "Growth data fetched successfully" }
                );
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet("child/{id}")]
        [Authorize(Roles = "User,Admin,Doctor")]
        public async Task<IActionResult> GetGrowthDataByChildId(
            string id,
            [FromQuery] QueryParams query
        )
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;
                var growthData = await _growthDataService.GetGrowthDataByChildIdAsync(
                    id,
                    requester!,
                    query
                );
                return Ok(
                    new
                    {
                        data = growthData.Data,
                        page = growthData.Page,
                        total = growthData.Total,
                        totalPages = growthData.TotalPages,
                        message = "Growth data fetched successfully",
                    }
                );
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet("velocity/child/{id}")]
        [Authorize(Roles = "User,Admin,Doctor")]
        public async Task<IActionResult> GetGrowthVelocityByChildId(string id)
        {
            try
            {
                var requester = HttpContext.Items["UserInfo"] as UserInfo;
                var growthVelocity = await _growthDataService.generateGrowthVelocityByChildId(
                    requester!,
                    id
                );
                return Ok(
                    new { data = growthVelocity, message = "Growth velocity fetched successfully" }
                );
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost("public")]
        public async Task<IActionResult> PublicGenerateGrowthData(
            GrowthDataDto.PublicCreateGrowthDataDto growthDataDto
        )
        {
            try
            {
                var growthData = new GrowthDataModel
                {
                    Height = growthDataDto.Height,
                    Weight = growthDataDto.Weight,
                    HeadCircumference = growthDataDto.HeadCircumference,
                    ArmCircumference = growthDataDto.ArmCircumference,
                    InputDate = growthDataDto.InputDate,
                };
                var currentGrowthData = await _growthDataService.PublicGenerateGrowthDataAsync(
                    growthData,
                    growthDataDto.BirthDate,
                    growthDataDto.Gender
                );
                return Ok(
                    new
                    {
                        growthData = currentGrowthData,
                        message = "Growth data fetched successfully",
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
