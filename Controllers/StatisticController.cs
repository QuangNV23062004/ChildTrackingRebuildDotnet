using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestAPI.Models;
using RestAPI.Services.interfaces;

namespace RestAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticController(IStatisticService statisticService) : ControllerBase
    {
        [HttpGet("new-users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetNewUsers([FromQuery] int value, [FromQuery] string unit)
        {
            try
            {
                var users = await statisticService.GetNewUsers(value, unit);
                return Ok(users);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet("new-requests")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetNewRequests(
            [FromQuery] int value,
            [FromQuery] string unit
        )
        {
            var requests = await statisticService.GetNewRequests(value, unit);
            return Ok(requests);
        }
    }
}
