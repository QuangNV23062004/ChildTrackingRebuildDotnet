using System;
using RestAPI.Models;
using RestAPI.Repositories.Interfaces;

namespace RestAPI.Repositories.interfaces;

public interface IUserRepository : IBaseRepository<UserModel>
{
    Task<UserModel> GetUserByEmail(string email);
    Task<List<UserModel>> GetDoctorsWithRating();
}
