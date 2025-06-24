using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using RestAPI.Models;
using RestAPI.Repositories.interfaces;
using RestAPI.Repositories.repositories;
using RestAPI.Services.interfaces;

namespace RestAPI.Services.services
{
    public class UserService(IUserRepository _userRepository) : IUserService
    {



        public Task<UserModel> DeleteUser(string id)
        {
            try
            {
                var _user = _userRepository.DeleteAsync(id);
                return _user;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<UserModel> GetUser(string id)
        {
            try
            {
                var _user = _userRepository.GetByIdAsync(id);
                return _user;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<UserModel[]> GetUsers()
        {
            try
            {
                var _users = await _userRepository.GetAllAsync();
                return [.. _users];
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<UserModel> UpdateUser(string id, string Name, string Email)
        {
            try
            {
                var existing_user = await _userRepository.GetByIdAsync(id);
                if (existing_user == null)
                {
                    throw new KeyNotFoundException("User not found");
                }
                existing_user.Name = Name ?? existing_user.Name;
                existing_user.Email = Email ?? existing_user.Email;
                var _user = await _userRepository.UpdateAsync(id, existing_user);

                if (_user == null)
                {
                    throw new InvalidOperationException("Failed to update user.");
                }
                return _user;
            }
            catch (Exception)
            {

                throw;
            }
        }


    }
}