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
}
