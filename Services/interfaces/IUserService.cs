using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestAPI.Models;

namespace RestAPI.Services.interfaces
{
    public interface IUserService
    {

        Task<UserModel> GetUser(string id);
        Task<UserModel[]> GetUsers();
        Task<UserModel> UpdateUser(string id, string Name, string Email);
        Task<UserModel> DeleteUser(string id);
    }
}