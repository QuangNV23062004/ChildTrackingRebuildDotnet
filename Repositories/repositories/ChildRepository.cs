using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Repositories.database;
using RestAPI.Repositories.interfaces;

namespace RestAPI.Repositories.repositories;

public class ChildRepository : Repository<ChildModel>, IChildRepository
{
    public ChildRepository(IOptions<MongoDBSettings> settings)
        : base(settings) { }

    public async Task<PaginationResult<ChildModel>> GetChildrenByUserId(
        string id,
        QueryParams query
    )
    {
        try
        {
            var page = Math.Max(query.Page, 1);
            var size = Math.Max(query.Size, 1);
            var skip = (page - 1) * size;

            var facetPipeline = new List<BsonDocument>
            {
                // Common stages
                new BsonDocument(
                    "$lookup",
                    new BsonDocument
                    {
                        ["from"] = "users",
                        ["localField"] = "guardianId",
                        ["foreignField"] = "_id",
                        ["as"] = "guardian",
                    }
                ),
                new BsonDocument(
                    "$unwind",
                    new BsonDocument
                    {
                        ["path"] = "$guardian",
                        ["preserveNullAndEmptyArrays"] = true,
                    }
                ),
                new BsonDocument(
                    "$match",
                    new BsonDocument
                    {
                        ["guardianId"] = new BsonObjectId(ObjectId.Parse(id)),
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
                .Select(doc => BsonSerializer.Deserialize<ChildModel>(doc.AsBsonDocument))
                .ToList();

            return new PaginationResult<ChildModel>
            {
                Page = page,
                Total = total,
                TotalPages = (int)Math.Ceiling((double)total / size),
                Data = data,
            };
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Invalid Guardian ID format.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to get children by guardian ID.", ex);
        }
    }
}
