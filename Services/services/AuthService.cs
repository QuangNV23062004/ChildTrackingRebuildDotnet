using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Repositories.interfaces;
using RestAPI.Repositories.repositories;
using RestAPI.Services.interfaces;
using Sprache;

namespace RestAPI.Services.services;

//test new render secret

public class AuthService(IUserRepository _userRepository) : IAuthService
{
    private string GenerateJsonWebToken(UserModel user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Name),
            new Claim("userId", user.Id?.ToString() ?? ""),
            new Claim("role", user.Role ?? "User"),
            new Claim("position", user.Role ?? "User"),
        };

        var jwtSecret =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!)
            ) ?? throw new ArgumentNullException("JWT_SECRET is not configured");
        ;
        var creds = new SigningCredentials(jwtSecret, SecurityAlgorithms.HmacSha512);

        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "default_issuer";
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "default_audience";
        var expiresDays = Environment.GetEnvironmentVariable("JWT_EXPIRES_DAYS") ?? "7";

        var TokenProviderDescriptor = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(Int32.Parse(expiresDays)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(TokenProviderDescriptor);
    }

    public async Task<UserModel> Register(UserModel user)
    {
        try
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null");
            }

            var existingUser = await _userRepository.GetUserByEmail(user.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            user.Id = ObjectId.GenerateNewId().ToString();
            user.Password = new PasswordHasher<UserModel>().HashPassword(user, user.Password);
            return await _userRepository.CreateAsync(user);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<string> Login(string email, string password)
    {
        try
        {
            var _user =
                await _userRepository.GetUserByEmail(email)
                ?? throw new Exception("Incorrect email or password");
            bool isCorrectPassword =
                new PasswordHasher<UserModel>().VerifyHashedPassword(
                    _user,
                    _user.Password,
                    password
                ) == PasswordVerificationResult.Success;

            if (!isCorrectPassword)
            {
                throw new UnauthorizedAccessException("Incorrect email or password");
            }

            string token = this.GenerateJsonWebToken(_user);
            return token;
        }
        catch (Exception ex)
        {
            Console.Write(ex);
            throw;
        }
    }

    public async Task<UserModel> GetUserInfoByToken(UserInfo userInfo)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userInfo.UserId);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            return user;
        }
        catch (System.Exception)
        {
            throw;
        }
    }
}
