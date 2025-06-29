using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestAPI.Dtos;
using RestAPI.Enums;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Repositories.interfaces;
using RestAPI.Services.interfaces;
using RestAPI.Services.services;

namespace RestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController(IUserService userService) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserModel>> CreateUser(
            [FromBody] UserDto.CreateUserDto userDto
        )
        {
            try
            {
                var user = new UserModel
                {
                    Name = userDto.Name,
                    Email = userDto.Email,
                    Role = userDto.Role ?? RoleEnum.User.ToString(),
                    Password = userDto.Password,
                };

                var userObject = await userService.CreateUser(user);
                return StatusCode(
                    StatusCodes.Status201Created,
                    new { user = userObject, message = "Create user successful" }
                );
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaginationResult<UserModel>>> GetUsers(
            [FromQuery] QueryParams query
        )
        {
            try
            {
                var users = await userService.GetUsers(query);
                return StatusCode(
                    StatusCodes.Status200OK,
                    new
                    {
                        data = users.Data,
                        total = users.Total,
                        page = users.Page,
                        totalPages = users.TotalPages,
                        message = "Get users successful",
                    }
                );
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet("{id}")]
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
                return StatusCode(
                    StatusCodes.Status200OK,
                    new { user = user, message = "Get user successful" }
                );
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<UserModel>> UpdateUser(
            string id,
            [FromBody] UserDto.UpdateUserDto userDto
        )
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(
                        StatusCodes.Status400BadRequest,
                        new
                        {
                            message = "Invalid input data",
                            errors = ModelState
                                .Values.SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage),
                        }
                    );
                }

                var updatedUser = await userService.UpdateUser(id, userDto.Name, userDto.Email);
                return StatusCode(
                    StatusCodes.Status200OK,
                    new { user = updatedUser, message = "Update user successful" }
                );
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserModel>> DeleteUser(string id)
        {
            try
            {
                var user = await userService.DeleteUser(id);

                return NoContent();
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}
