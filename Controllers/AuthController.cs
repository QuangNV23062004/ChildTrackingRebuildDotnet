using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestAPI.Dtos;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Services.interfaces;

namespace RestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<UserModel>> Register(
            [FromBody] AuthDto.RegisterDto registerDto
        )
        {
            var user = new UserModel
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                Password = registerDto.Password,
            };

            try
            {
                var createdUser = await authService.Register(user);
                return StatusCode(
                    StatusCodes.Status201Created,
                    new { user = createdUser, message = "Register successful" }
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception e)
            {
                Console.Write(e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] AuthDto.LoginDto loginDto)
        {
            try
            {
                var token = await authService.Login(loginDto.Email, loginDto.Password);
                return StatusCode(
                    StatusCodes.Status200OK,
                    new { accessToken = token, message = "Login successful" }
                );
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Invalid credentials");
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserModel>> GetUserInfo()
        {
            try
            {
                var UserInfo = HttpContext.Items["UserInfo"] as UserInfo;

                var user = await authService.GetUserInfoByToken(UserInfo);
                return Ok(user);
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}
