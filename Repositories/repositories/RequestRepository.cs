using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using RestAPI.Enums;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Repositories.database;
using RestAPI.Repositories.interfaces;
using RestAPI.Services.interfaces;

namespace RestAPI.Repositories.repositories;

public class RequestRepository : Repository<RequestModel>, IRequestRepository
{
    public RequestRepository(IOptions<MongoDBSettings> settings)
        : base(settings) { }

    public async Task<PaginationResult<RequestModel>> GetRequestsByDoctorId(
        string doctorId,
        QueryParams query
    )
    {
        try
        {
            Console.WriteLine("doctorId: " + doctorId);
            var page = Math.Max(query.Page, 1);
            var size = Math.Max(query.Size, 1);
            var skip = (page - 1) * size;

            var facetPipeline = new List<BsonDocument>
            {
                new BsonDocument(
                    "$lookup",
                    new BsonDocument
                    {
                        ["from"] = "users",
                        ["localField"] = "doctorId",
                        ["foreignField"] = "_id",
                        ["as"] = "doctor",
                    }
                ),
                new BsonDocument(
                    "$unwind",
                    new BsonDocument { ["path"] = "$doctor", ["preserveNullAndEmptyArrays"] = true }
                ),
                new BsonDocument(
                    "$lookup",
                    new BsonDocument
                    {
                        ["from"] = "users",
                        ["localField"] = "memberId",
                        ["foreignField"] = "_id",
                        ["as"] = "member",
                    }
                ),
                new BsonDocument(
                    "$unwind",
                    new BsonDocument { ["path"] = "$member", ["preserveNullAndEmptyArrays"] = true }
                ),
                new BsonDocument(
                    "$lookup",
                    new BsonDocument
                    {
                        ["from"] = "children",
                        ["localField"] = "childId",
                        ["foreignField"] = "_id",
                        ["as"] = "child",
                    }
                ),
                new BsonDocument(
                    "$unwind",
                    new BsonDocument { ["path"] = "$child", ["preserveNullAndEmptyArrays"] = true }
                ),
                new BsonDocument(
                    "$match",
                    new BsonDocument
                    {
                        ["doctorId"] = new BsonObjectId(ObjectId.Parse(doctorId)),
                        ["status"] = RequestStatusEnum.Admin_Accepted.ToString(),
                        ["isDeleted"] = false,
                    }
                ),
                // Facet stage to split into count and data
                new BsonDocument(
                    "$facet",
                    new BsonDocument
                    {
                        ["count"] = new BsonArray { new BsonDocument("$count", "total") },
                        ["data"] = new BsonArray
                        {
                            new BsonDocument("$sort", new BsonDocument { ["createdAt"] = -1 }),
                            new BsonDocument("$skip", skip),
                            new BsonDocument("$limit", size),
                        },
                    }
                ),
            };

            var facetResult = await _collection
                .Aggregate<BsonDocument>(facetPipeline)
                .FirstOrDefaultAsync();
            var total = facetResult?["count"]?.AsBsonArray.FirstOrDefault()?["total"].AsInt32 ?? 0;
            var dataArray = facetResult?["data"]?.AsBsonArray ?? new BsonArray();
            var data = dataArray
                .Select(doc => BsonSerializer.Deserialize<RequestModel>(doc.AsBsonDocument))
                .ToList();

            return new PaginationResult<RequestModel>
            {
                Data = data,
                Page = page,
                Total = total,
                TotalPages = (int)Math.Ceiling((double)total / size),
            };
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Invalid Doctor ID format.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to get requests by doctor ID.", ex);
        }
    }

    public async Task<PaginationResult<RequestModel>> GetRequestsByMemberId(
        string memberId,
        QueryParams query,
        string? status
    )
    {
        try
        {
            var page = Math.Max(query.Page, 1);
            var size = Math.Max(query.Size, 1);
            var skip = (page - 1) * size;

            var facetPipeline = new List<BsonDocument>
            {
                new BsonDocument(
                    "$lookup",
                    new BsonDocument
                    {
                        ["from"] = "users",
                        ["localField"] = "doctorId",
                        ["foreignField"] = "_id",
                        ["as"] = "doctor",
                    }
                ),
                new BsonDocument(
                    "$unwind",
                    new BsonDocument { ["path"] = "$doctor", ["preserveNullAndEmptyArrays"] = true }
                ),
                new BsonDocument(
                    "$lookup",
                    new BsonDocument
                    {
                        ["from"] = "users",
                        ["localField"] = "memberId",
                        ["foreignField"] = "_id",
                        ["as"] = "member",
                    }
                ),
                new BsonDocument(
                    "$unwind",
                    new BsonDocument { ["path"] = "$member", ["preserveNullAndEmptyArrays"] = true }
                ),
                new BsonDocument(
                    "$lookup",
                    new BsonDocument
                    {
                        ["from"] = "children",
                        ["localField"] = "childId",
                        ["foreignField"] = "_id",
                        ["as"] = "child",
                    }
                ),
                new BsonDocument(
                    "$unwind",
                    new BsonDocument { ["path"] = "$child", ["preserveNullAndEmptyArrays"] = true }
                ),
                new BsonDocument(
                    "$match",
                    new BsonDocument
                    {
                        ["memberId"] = new BsonObjectId(ObjectId.Parse(memberId)),
                        ["isDeleted"] = false,
                    }
                ),
                // Facet stage to split into count and data
                new BsonDocument(
                    "$facet",
                    new BsonDocument
                    {
                        ["count"] = new BsonArray { new BsonDocument("$count", "total") },
                        ["data"] = new BsonArray
                        {
                            new BsonDocument("$sort", new BsonDocument { ["createdAt"] = -1 }),
                            new BsonDocument("$skip", skip),
                            new BsonDocument("$limit", size),
                        },
                    }
                ),
            };

            if (status != null && status != "")
            {
                facetPipeline.Add(
                    new BsonDocument("$match", new BsonDocument { ["status"] = status })
                );
            }

            Console.WriteLine("facetPipeline: " + facetPipeline.ToJson());

            var facetResult = await _collection
                .Aggregate<BsonDocument>(facetPipeline)
                .FirstOrDefaultAsync();
            var total = facetResult?["count"]?.AsBsonArray.FirstOrDefault()?["total"].AsInt32 ?? 0;
            var dataArray = facetResult?["data"]?.AsBsonArray ?? new BsonArray();
            var data = dataArray
                .Select(doc => BsonSerializer.Deserialize<RequestModel>(doc.AsBsonDocument))
                .ToList();

            return new PaginationResult<RequestModel>
            {
                Data = data,
                Page = page,
                Total = total,
                TotalPages = (int)Math.Ceiling((double)total / size),
            };
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Invalid Member ID format.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to get requests by member ID.", ex);
        }
    }

    public async Task<PaginationResult<RequestModel>> GetAllRequestWithPagination(
        QueryParams query,
        string? status
    )
    {
        try
        {
            var page = Math.Max(query.Page, 1);
            var size = Math.Max(query.Size, 1);
            var skip = (page - 1) * size;

            var facetPipeline = new List<BsonDocument>
            {
                new BsonDocument(
                    "$lookup",
                    new BsonDocument
                    {
                        ["from"] = "users",
                        ["localField"] = "doctorId",
                        ["foreignField"] = "_id",
                        ["as"] = "doctor",
                    }
                ),
                new BsonDocument(
                    "$unwind",
                    new BsonDocument
                    {
                        ["path"] = "$doctor",
                        ["preserveNullAndEmptyArrays"] = false,
                    }
                ),
                new BsonDocument(
                    "$lookup",
                    new BsonDocument
                    {
                        ["from"] = "users",
                        ["localField"] = "memberId",
                        ["foreignField"] = "_id",
                        ["as"] = "member",
                    }
                ),
                new BsonDocument(
                    "$unwind",
                    new BsonDocument
                    {
                        ["path"] = "$member",
                        ["preserveNullAndEmptyArrays"] = false,
                    }
                ),
                new BsonDocument(
                    "$lookup",
                    new BsonDocument
                    {
                        ["from"] = "children",
                        ["localField"] = "childId",
                        ["foreignField"] = "_id",
                        ["as"] = "child",
                    }
                ),
                new BsonDocument(
                    "$unwind",
                    new BsonDocument { ["path"] = "$child", ["preserveNullAndEmptyArrays"] = false }
                ),
                new BsonDocument("$match", new BsonDocument { ["isDeleted"] = false }),
                // Facet stage to split into count and data
                new BsonDocument(
                    "$facet",
                    new BsonDocument
                    {
                        ["count"] = new BsonArray { new BsonDocument("$count", "total") },
                        ["data"] = new BsonArray
                        {
                            new BsonDocument("$sort", new BsonDocument { ["createdAt"] = -1 }),
                            new BsonDocument("$skip", skip),
                            new BsonDocument("$limit", size),
                        },
                    }
                ),
            };

            var facetResult = await _collection
                .Aggregate<BsonDocument>(facetPipeline)
                .FirstOrDefaultAsync();
            var total = facetResult?["count"]?.AsBsonArray.FirstOrDefault()?["total"].AsInt32 ?? 0;
            var dataArray = facetResult?["data"]?.AsBsonArray ?? new BsonArray();
            var data = dataArray
                .Select(doc => BsonSerializer.Deserialize<RequestModel>(doc.AsBsonDocument))
                .ToList();

            return new PaginationResult<RequestModel>
            {
                Data = data,
                Page = page,
                Total = total,
                TotalPages = (int)Math.Ceiling((double)total / size),
            };
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Invalid status format.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to get paginated requests.", ex);
        }
    }

    public async Task<List<CountData>> GetNewRequests(int value, string unit)
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
            // Match requests created within the date range and not deleted
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
            // Group by date and count requests
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
        var requestCountMap = result.ToDictionary(
            doc => doc["Date"].AsString,
            doc => doc["Count"].AsInt32
        );

        var requestCounts = new List<CountData>();

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
            int count = requestCountMap.ContainsKey(dateKey) ? requestCountMap[dateKey] : 0;

            requestCounts.Add(new CountData(currentDate, count));
        }

        return requestCounts;
    }
}
