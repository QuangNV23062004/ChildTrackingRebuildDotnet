using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Repositories.database;
using RestAPI.Repositories.interfaces;

namespace RestAPI.Repositories.repositories
{
    public class GrowthDataRepository : Repository<GrowthDataModel>, IGrowthDataRepository
    {
        public GrowthDataRepository(Microsoft.Extensions.Options.IOptions<MongoDBSettings> settings)
            : base(settings) { }

        public async Task<GrowthDataModel[]> GetAllGrowthDataByChildId(string id)
        {
            try
            {
                var data = await _collection
                    .Find(x => x.ChildId == id && x.IsDeleted == false)
                    .ToListAsync();
                return data.ToArray();
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<PaginationResult<GrowthDataModel>> GetGrowthDataByChildId(
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
                            ["from"] = "children",
                            ["localField"] = "childId",
                            ["foreignField"] = "_id",
                            ["as"] = "child",
                        }
                    ),
                    new BsonDocument(
                        "$unwind",
                        new BsonDocument
                        {
                            ["path"] = "$child",
                            ["preserveNullAndEmptyArrays"] = true,
                        }
                    ),
                    new BsonDocument(
                        "$match",
                        new BsonDocument
                        {
                            ["childId"] = new BsonObjectId(ObjectId.Parse(id)),
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
                var total =
                    facetResult?["count"]?.AsBsonArray.FirstOrDefault()?["total"].AsInt32 ?? 0;
                var dataArray = facetResult?["data"]?.AsBsonArray ?? new BsonArray();
                var data = dataArray
                    .Select(doc => BsonSerializer.Deserialize<GrowthDataModel>(doc.AsBsonDocument))
                    .ToList();

                return new PaginationResult<GrowthDataModel>
                {
                    Data = data,
                    Page = page,
                    Total = total,
                    TotalPages = (int)Math.Ceiling((double)total / size),
                };
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}
