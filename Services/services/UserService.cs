using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using RestAPI.Enums;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Repositories.interfaces;
using RestAPI.Repositories.repositories;
using RestAPI.Services.interfaces;

namespace RestAPI.Services.services
{
    public class UserService(IUserRepository _userRepository) : IUserService
    {
        public async Task<UserModel> CreateUser(UserModel user)
        {
            try
            {
                var existingUser = await _userRepository.GetUserByEmail(user.Email);
                if (existingUser != null)
                {
                    throw new Exception("User already exists");
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

        public async Task<UserModel> DeleteUser(string id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    throw new KeyNotFoundException("User not found");
                }
                if (user.Role == RoleEnum.Admin.ToString())
                {
                    throw new Exception("Cannot delete admin user");
                }

                var _user = await _userRepository.DeleteAsync(id);
                if (_user == null)
                {
                    throw new KeyNotFoundException("User not found");
                }
                return _user;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<UserModel> GetUser(string id)
        {
            try
            {
                var _user = await _userRepository.GetByIdAsync(id);
                if (_user == null)
                {
                    throw new KeyNotFoundException("User not found");
                }
                return _user;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<PaginationResult<UserModel>> GetUsers(QueryParams query)
        {
            try
            {
                var _users = await _userRepository.GetAllAsyncWithPagination(query);
                return _users;
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

        public async Task<List<UserModel>> GetDoctorsWithRating()
        {
            try
            {
                var _doctors = await _userRepository.GetDoctorsWithRating();
                return _doctors;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
