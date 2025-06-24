using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RestAPI.Models;
using RestAPI.Repositories.database;
using RestAPI.Repositories.interfaces;

namespace RestAPI.Repositories.repositories
{
    public class UserRepository : Repository<UserModel>, IUserRepository
    {
        public UserRepository(IOptions<MongoDBSettings> settings) : base(settings)
        {
        }

        public async Task<UserModel> GetUserByEmail(string email)
        {
            try
            {
                var filter = Builders<UserModel>.Filter.And(
                    Builders<UserModel>.Filter.Eq(u => u.Email, email),
                    Builders<UserModel>.Filter.Eq(u => u.IsDeleted, false)
                );

                var user = await _collection.Find(filter).FirstOrDefaultAsync();

                return user;
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("Invalid email format.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the user by email.", ex);
            }
        }

    }
}