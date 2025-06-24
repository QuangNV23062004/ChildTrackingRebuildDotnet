using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestAPI.Dtos;
using RestAPI.Models;
using RestAPI.Services.interfaces;
using RestAPI.Services.services;

namespace RestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController(IUserService userService) : ControllerBase
    {
        [HttpGet("/")]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<UserModel[]>> GetUsers()
        {
            try
            {
                var users = await userService.GetUsers();
                return StatusCode(StatusCodes.Status200OK, new
                {
                    user = users,
                    message = "Get users successful"
                });
            }
            catch (System.Exception ex)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = ex.Message
                });
            }
        }


        [HttpGet("/{id}")]
        [Authorize(Roles = "0,1")]
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
            catch (System.Exception ex)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPatch("/{id}")]
        [Authorize(Roles = "0,1")]
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
            catch (System.Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = ex.Message
                });
            }
        }


        [HttpDelete("/{id}")]
        [Authorize(Roles = "1")]
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
            catch (System.Exception ex)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = ex.Message
                });
            }
        }
    }
}
