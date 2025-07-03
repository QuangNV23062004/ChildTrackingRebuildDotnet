using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestAPI.Dtos;
using RestAPI.Helpers;
using RestAPI.Models;

namespace RestAPI.Services.interfaces
{
    public interface IUserService
    {
        Task<UserModel> CreateUser(UserModel userDto);
        Task<UserModel> GetUser(string id);
        Task<PaginationResult<UserModel>> GetUsers(QueryParams query);
        Task<UserModel> UpdateUser(string id, string Name, string Email);
        Task<UserModel> DeleteUser(string id);
        Task<List<UserModel>> GetDoctorsWithRating();
    }
}
