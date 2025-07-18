using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using Razor.Templating.Core;
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

    private string GenerateVerifyToken(UserModel user)
    {
        if (
            string.IsNullOrWhiteSpace(user.Name)
            || string.IsNullOrWhiteSpace(user.Email)
            || string.IsNullOrWhiteSpace(user.Password)
        )
        {
            throw new ArgumentException("Name, Email, and Password are required");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Name),
            new Claim("name", user.Name),
            new Claim("email", user.Email),
            new Claim("mail", user.Email),
            new Claim("password", user.Password),
        };

        var jwtSecret =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!)
            ) ?? throw new ArgumentNullException("JWT_SECRET is not configured");
        ;
        var creds = new SigningCredentials(jwtSecret, SecurityAlgorithms.HmacSha512);

        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "default_issuer";
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "default_audience";
        var expiresDays =
            Environment.GetEnvironmentVariable("VERIFICATION_TOKEN_EXPIRES_DAY") ?? "7";

        var TokenProviderDescriptor = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(Int32.Parse(expiresDays)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(TokenProviderDescriptor);
    }

    private async Task SendVerificationEmail(string email, string token, string name)
    {
        try
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email is required");
            }

            var baseUrl =
                Environment.GetEnvironmentVariable("SERVER_URL") ?? "http://localhost:5264";
            var verificationUrl = $"{baseUrl}/api/Auth/verify?verificationToken={token}";

            var body = await RazorTemplateEngine.RenderAsync(
                "/Pages/EmailTemplates/VerifyModel.cshtml",
                verificationUrl
            );

            var smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;

            var senderMail = Environment.GetEnvironmentVariable("EMAIL_USERNAME")!;
            var password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD")!;
            smtpClient.Credentials = new NetworkCredential(senderMail, password);

            var message = new MailMessage
            {
                From = new MailAddress(senderMail),
                Subject = "GrowthGuardian Email Verification",
                Body = body,
                IsBodyHtml = true,
            };
            message.To.Add(email);

            await smtpClient.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending email: {ex.Message}");
            throw;
        }
    }

    private ClaimsPrincipal ValidateAndDecodeToken(string token)
    {
        try
        {
            // Get the secret key from environment variables
            var secret =
                Environment.GetEnvironmentVariable("JWT_SECRET")
                ?? throw new ArgumentNullException("JWT_SECRET is not configured");

            // Convert the secret to a SymmetricSecurityKey
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            // Configure validation parameters based on your token generation logic
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true, // Enable if you set JWT_ISSUER
                ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "default_issuer",
                ValidateAudience = true, // Enable if you set JWT_AUDIENCE
                ValidAudience =
                    Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "default_audience",
                ValidateLifetime = true, // Check token expiration
                ClockSkew = TimeSpan.Zero, // No tolerance for clock skew
            };

            // Validate the token and get the claims principal
            var handler = new JwtSecurityTokenHandler();
            var claimsPrincipal = handler.ValidateToken(
                token,
                validationParameters,
                out var validatedToken
            );

            // Optional: Log or inspect the token for debugging
            var jwtToken = handler.ReadJwtToken(token);
            Console.WriteLine("Decoded Payload: " + jwtToken.Payload.SerializeToJson());

            return claimsPrincipal;
        }
        catch (SecurityTokenException ex)
        {
            Console.WriteLine($"Token validation failed: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error decoding token: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> Register(UserModel user)
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

            var token = this.GenerateVerifyToken(user);

            await SendVerificationEmail(user.Email, token, user.Name);
            return true;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<UserModel> VerifyUser(string token)
    {
        try
        {
            var claimsPrincipal = ValidateAndDecodeToken(token);
            //use mail because email + role claims will be null
            var emailClaim = claimsPrincipal.FindFirst("mail")?.Value;
            var nameClaim = claimsPrincipal.FindFirst("name")?.Value;
            var passwordClaim = claimsPrincipal.FindFirst("password")?.Value;
            var user = new UserModel { Name = nameClaim!, Email = emailClaim! };
            user.Id = ObjectId.GenerateNewId().ToString();
            user.Password = new PasswordHasher<UserModel>().HashPassword(user, passwordClaim!);
            return await _userRepository.CreateAsync(user);
        }
        catch (System.Exception)
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
