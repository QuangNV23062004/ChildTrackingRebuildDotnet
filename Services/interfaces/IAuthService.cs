using System;
using System.IdentityModel.Tokens.Jwt;
using RestAPI.Helpers;
using RestAPI.Models;

namespace RestAPI.Services.interfaces;

public interface IAuthService
{
    Task<UserModel> Register(UserModel user);

    Task<string> Login(string email, string password);

    Task<UserModel> GetUserInfoByToken(UserInfo userInfo);
}
