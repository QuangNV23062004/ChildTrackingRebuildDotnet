using System;
using RestAPI.Models;
using RestAPI.Repositories.Interfaces;
using RestAPI.Services.interfaces;

namespace RestAPI.Repositories.interfaces;

public interface IUserRepository : IBaseRepository<UserModel>
{
    Task<UserModel> GetUserByEmail(string email);
    Task<List<UserModel>> GetDoctorsWithRating();

    Task<List<CountData>> GetNewUsers(int value, string unit);
}
