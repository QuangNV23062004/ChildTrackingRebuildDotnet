using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestAPI.Dtos;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Services.interfaces;
using RestAPI.Services.services;
using RestAPI.Repositories.interfaces;

namespace RestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController(IUserService userService) : ControllerBase
    {
        [HttpGet("/api")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaginationResult<UserModel>>> GetUsers([FromQuery] QueryParams query)
        {

            try
            {
                var users = await userService.GetUsers(query);
                return StatusCode(StatusCodes.Status200OK, new
                {
                    data = users.Data,
                    total = users.Total,
                    page = users.Page,
                    totalPages = users.TotalPages,
                    message = "Get users successful"
                });
            }
            catch (System.Exception)
            {

                throw;
            }
        }


        [HttpGet("/api/{id}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<UserModel>> GetUser(string id)
        {
            try
            {
                //get data from JWT payload
                string? userId = User.FindFirst("userId")?.Value;
                string? role = User.FindFirst("role")?.Value;

                Console.WriteLine(userId);
                Console.WriteLine(role);
                var user = await userService.GetUser(id);
                return StatusCode(StatusCodes.Status200OK, new
                {
                    user = user,
                    message = "Get user successful"
                });
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPatch("/api/{id}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<UserModel>> UpdateUser(string id, [FromBody] UserDto userDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new
                    {
                        message = "Invalid input data",
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }



                var updatedUser = await userService.UpdateUser(id, userDto.Name, userDto.Email);
                return StatusCode(StatusCodes.Status200OK, new
                {
                    user = updatedUser,
                    message = "Update user successful"
                });
            }
            catch (System.Exception)
            {
                throw;
            }
        }


        [HttpDelete("/api/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserModel>> DeleteUser(string id)
        {
            try
            {
                var user = await userService.DeleteUser(id);

                return StatusCode(StatusCodes.Status200OK, new
                {
                    user,
                    message = "Delete user successfully"
                });
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}
