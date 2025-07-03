using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using RestAPI.Enums;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Repositories.database;
using RestAPI.Repositories.interfaces;

namespace RestAPI.Repositories.repositories
{
    public class UserRepository : Repository<UserModel>, IUserRepository
    {
        public UserRepository(IOptions<MongoDBSettings> settings)
            : base(settings) { }

        public async Task<List<UserModel>> GetDoctorsWithRating()
        {
            var pipeline = new List<BsonDocument>
            {
                // 1. Match doctors
                new BsonDocument(
                    "$match",
                    new BsonDocument
                    {
                        { "role", RoleEnum.Doctor.ToString() },
                        { "isDeleted", false },
                    }
                ),
                // 2. Lookup requests
                new BsonDocument(
                    "$lookup",
                    new BsonDocument
                    {
                        { "from", "requests" },
                        { "localField", "_id" },
                        { "foreignField", "doctorId" },
                        { "as", "requests" },
                    }
                ),
                // 3. Lookup consultations for each request (filtering for completed and rated only)
                new BsonDocument(
                    "$lookup",
                    new BsonDocument
                    {
                        { "from", "consultations" },
                        { "let", new BsonDocument("requestIds", "$requests._id") },
                        {
                            "pipeline",
                            new BsonArray
                            {
                                new BsonDocument(
                                    "$match",
                                    new BsonDocument
                                    {
                                        {
                                            "$expr",
                                            new BsonDocument(
                                                "$in",
                                                new BsonArray { "$requestId", "$$requestIds" }
                                            )
                                        },
                                    }
                                ),
                                new BsonDocument(
                                    "$match",
                                    new BsonDocument
                                    {
                                        { "status", "Completed" },
                                        { "rating", new BsonDocument("$gt", 0) },
                                    }
                                ),
                            }
                        },
                        { "as", "consultations" },
                    }
                ),
                // 4. Add averageRating field, set to 0 if no consultations
                new BsonDocument(
                    "$addFields",
                    new BsonDocument
                    {
                        {
                            "averageRating",
                            new BsonDocument(
                                "$cond",
                                new BsonArray
                                {
                                    new BsonDocument(
                                        "$gt",
                                        new BsonArray
                                        {
                                            new BsonDocument("$size", "$consultations"),
                                            0,
                                        }
                                    ),
                                    new BsonDocument("$avg", "$consultations.rating"),
                                    0,
                                }
                            )
                        },
                    }
                ),
                // 5. Project only needed fields
                new BsonDocument(
                    "$project",
                    new BsonDocument
                    {
                        { "_id", 1 },
                        { "name", 1 },
                        { "email", 1 },
                        { "role", 1 },
                        { "averageRating", 1 },
                    }
                ),
            };

            var result = await _collection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            var doctors = result
                .Select(doc => new UserModel
                {
                    Id = doc["_id"].ToString(),
                    Name = doc["name"].AsString,
                    Email = doc["email"].AsString,
                    Role = doc["role"].AsString,
                    Rating =
                        doc.Contains("averageRating") && !doc["averageRating"].IsBsonNull
                            ? doc["averageRating"].ToDouble()
                            : 0,
                })
                .ToList();

            return doctors;
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
