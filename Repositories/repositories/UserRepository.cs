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
using RestAPI.Services.interfaces;

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

        public async Task<List<CountData>> GetNewUsers(int value, string unit)
        {
            DateTime startDate,
                endDate;
            string groupBy = "";
            int interval = 0;
            int year = DateTime.Now.Year;

            switch (unit)
            {
                case "month":
                    // value represents month number (1-12, where 1=January, 2=February, etc.)
                    var currentYear = DateTime.Now.Year;
                    var month = value >= 1 && value <= 12 ? value : DateTime.Now.Month;
                    startDate = new DateTime(currentYear, month, 1);
                    endDate = startDate.AddMonths(1).AddDays(-1);
                    interval = endDate.Day; // Number of days in the month
                    groupBy = "%Y-%m-%d";
                    break;
                case "year":
                    // value represents the year
                    year = value > 0 ? value : DateTime.Now.Year;
                    startDate = new DateTime(year, 1, 1);
                    endDate = new DateTime(year, 12, 31);
                    interval = 12; // 12 months
                    groupBy = "%Y-%m-01";
                    break;
                default:
                    throw new ArgumentException("Invalid unit. Use 'month' or 'year'.");
            }

            var pipeline = new List<BsonDocument>
            {
                // Match users created within the date range and not deleted
                new BsonDocument(
                    "$match",
                    new BsonDocument
                    {
                        {
                            "createdAt",
                            new BsonDocument { { "$gte", startDate }, { "$lte", endDate } }
                        },
                        { "isDeleted", false },
                    }
                ),
                // Group by date and count users
                new BsonDocument(
                    "$group",
                    new BsonDocument
                    {
                        {
                            "_id",
                            new BsonDocument(
                                "$dateToString",
                                new BsonDocument { { "format", groupBy }, { "date", "$createdAt" } }
                            )
                        },
                        { "count", new BsonDocument("$sum", 1) },
                    }
                ),
                // Project to final format
                new BsonDocument(
                    "$project",
                    new BsonDocument
                    {
                        { "Date", "$_id" },
                        { "Count", "$count" },
                        { "_id", 0 },
                    }
                ),
                // Sort by date
                new BsonDocument("$sort", new BsonDocument("Date", 1)),
            };

            var result = await _collection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            // Create a dictionary to map dates to counts
            var userCountMap = result.ToDictionary(
                doc => doc["Date"].AsString,
                doc => doc["Count"].AsInt32
            );

            var userCounts = new List<CountData>();

            // Fill in all dates in the interval with counts (0 if no data)
            for (int i = 0; i < interval; i++)
            {
                DateTime currentDate;
                if (unit == "year")
                {
                    // For year, generate month dates (1st of each month)
                    currentDate = new DateTime(year, i + 1, 1);
                }
                else
                {
                    // For month, generate day dates
                    currentDate = startDate.AddDays(i);
                }

                string dateKey = currentDate.ToString("yyyy-MM-dd");
                int count = userCountMap.ContainsKey(dateKey) ? userCountMap[dateKey] : 0;

                userCounts.Add(new CountData(currentDate, count));
            }

            return userCounts;
        }
    }
}
