using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Repositories.database;
using RestAPI.Repositories.interfaces;

namespace RestAPI.Repositories.repositories
{
    public class ConsultationMessageRepository
        : Repository<ConsultationMessageModel>,
            IConsultationMessageRepository
    {
        public ConsultationMessageRepository(IOptions<MongoDBSettings> settings)
            : base(settings) { }

        public async Task<PaginationResult<ConsultationMessageModel>> GetConsultationMessages(
            string consultationId,
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
                    new BsonDocument(
                        "$lookup",
                        new BsonDocument
                        {
                            ["from"] = "users",
                            ["localField"] = "senderId",
                            ["foreignField"] = "_id",
                            ["as"] = "sender",
                        }
                    ),
                    new BsonDocument(
                        "$unwind",
                        new BsonDocument
                        {
                            ["path"] = "$sender",
                            ["preserveNullAndEmptyArrays"] = true,
                        }
                    ),
                    new BsonDocument(
                        "$match",
                        new BsonDocument
                        {
                            ["consultationId"] = new BsonObjectId(ObjectId.Parse(consultationId)),
                            ["isDeleted"] = false,
                        }
                    ),
                    new BsonDocument(
                        "$facet",
                        new BsonDocument
                        {
                            ["data"] = new BsonArray
                            {
                                new BsonDocument("$sort", new BsonDocument { ["createdAt"] = -1 }),
                                new BsonDocument("$skip", skip),
                                new BsonDocument("$limit", size),
                            },
                            ["count"] = new BsonArray { new BsonDocument("$count", "total") },
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
                    .Select(doc =>
                        BsonSerializer.Deserialize<ConsultationMessageModel>(doc.AsBsonDocument)
                    )
                    .ToList();

                return new PaginationResult<ConsultationMessageModel>
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
