using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using RestAPI.Enums;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Models.SubModels;
using RestAPI.Repositories.database;
using RestAPI.Repositories.interfaces;

namespace RestAPI.Repositories.repositories
{
    public class BlogRepository : Repository<BlogModel>, IBlogRepository
    {
        public BlogRepository(IOptions<MongoDBSettings> settings)
            : base(settings) { }

        public async Task<PaginationResult<BlogModel>> GetAllAsyncWithPagination(
            QueryParams query,
            PopulationModel[]? populationParams = null,
            BlogStatus? status = null
        )
        {
            var facetPipeline = new List<MongoDB.Bson.BsonDocument>
            {
                new MongoDB.Bson.BsonDocument(
                    "$match",
                    new MongoDB.Bson.BsonDocument { ["isDeleted"] = false }
                ),
            };
            if (status != null)
            {
                facetPipeline[0]["$match"].AsBsonDocument.Add("status", (int)status.Value);
            }

            if (populationParams != null && populationParams.Length > 0)
            {
                foreach (var param in populationParams)
                {
                    facetPipeline.Add(
                        new MongoDB.Bson.BsonDocument(
                            "$lookup",
                            new MongoDB.Bson.BsonDocument
                            {
                                ["from"] = param.Collection,
                                ["localField"] = param.LocalField,
                                ["foreignField"] = param.ForeignField,
                                ["as"] = param.As,
                            }
                        )
                    );
                    facetPipeline.Add(
                        new MongoDB.Bson.BsonDocument(
                            "$unwind",
                            new MongoDB.Bson.BsonDocument
                            {
                                ["path"] = "$" + param.As,
                                ["preserveNullAndEmptyArrays"] = true,
                            }
                        )
                    );
                }
            }
            facetPipeline.Add(
                new MongoDB.Bson.BsonDocument(
                    new MongoDB.Bson.BsonDocument(
                        "$facet",
                        new MongoDB.Bson.BsonDocument
                        {
                            ["count"] = new MongoDB.Bson.BsonArray
                            {
                                new MongoDB.Bson.BsonDocument("$count", "total"),
                            },
                            ["data"] = new MongoDB.Bson.BsonArray
                            {
                                new MongoDB.Bson.BsonDocument(
                                    "$sort",
                                    new MongoDB.Bson.BsonDocument { ["createdAt"] = -1 }
                                ),
                                new MongoDB.Bson.BsonDocument(
                                    "$skip",
                                    (query.Page - 1) * query.Size
                                ),
                                new MongoDB.Bson.BsonDocument("$limit", query.Size),
                            },
                        }
                    )
                )
            );

            var facetResult = await _collection
                .Aggregate<MongoDB.Bson.BsonDocument>(facetPipeline)
                .FirstOrDefaultAsync();
            var total = facetResult?["count"]?.AsBsonArray.FirstOrDefault()?["total"].AsInt32 ?? 0;
            var dataArray = facetResult?["data"]?.AsBsonArray ?? new MongoDB.Bson.BsonArray();
            var data = dataArray
                .Select(doc =>
                    MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BlogModel>(
                        doc.AsBsonDocument
                    )
                )
                .ToList();
            return new PaginationResult<BlogModel>
            {
                Page = query.Page,
                Total = total,
                TotalPages = (int)Math.Ceiling((double)total / query.Size),
                Data = data,
            };
        }
    }
}
