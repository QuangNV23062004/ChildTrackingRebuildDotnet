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
using RestAPI.Repositories.database;
using RestAPI.Repositories.interfaces;

namespace RestAPI.Repositories.repositories
{
    public class ConsultationRepository : Repository<ConsultationModel>, IConsultationRepository
    {
        public ConsultationRepository(IOptions<MongoDBSettings> settings)
            : base(settings) { }

        public async Task<PaginationResult<ConsultationModel>> GetConsultations(
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
                    // Common stages
                    new BsonDocument(
                        "$lookup",
                        new BsonDocument
                        {
                            ["from"] = "requests",
                            ["localField"] = "requestId",
                            ["foreignField"] = "_id",
                            ["as"] = "request",
                        }
                    ),
                    new BsonDocument(
                        "$unwind",
                        new BsonDocument
                        {
                            ["path"] = "$request",
                            ["preserveNullAndEmptyArrays"] = false,
                        }
                    ),
                    new BsonDocument(
                        "$lookup",
                        new BsonDocument
                        {
                            ["from"] = "users",
                            ["localField"] = "request.doctorId",
                            ["foreignField"] = "_id",
                            ["as"] = "request.doctor",
                        }
                    ),
                    new BsonDocument(
                        "$unwind",
                        new BsonDocument
                        {
                            ["path"] = "$request.doctor",
                            ["preserveNullAndEmptyArrays"] = false,
                        }
                    ),
                    new BsonDocument(
                        "$lookup",
                        new BsonDocument
                        {
                            ["from"] = "users",
                            ["localField"] = "request.memberId",
                            ["foreignField"] = "_id",
                            ["as"] = "request.member",
                        }
                    ),
                    new BsonDocument(
                        "$unwind",
                        new BsonDocument
                        {
                            ["path"] = "$request.member",
                            ["preserveNullAndEmptyArrays"] = false,
                        }
                    ),
                    new BsonDocument(
                        "$lookup",
                        new BsonDocument
                        {
                            ["from"] = "children",
                            ["localField"] = "request.childId",
                            ["foreignField"] = "_id",
                            ["as"] = "request.child",
                        }
                    ),
                    new BsonDocument(
                        "$unwind",
                        new BsonDocument
                        {
                            ["path"] = "$request.child",
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
                var total =
                    facetResult?["count"]?.AsBsonArray.FirstOrDefault()?["total"].AsInt32 ?? 0;
                var dataArray = facetResult?["data"]?.AsBsonArray ?? new BsonArray();
                var data = dataArray
                    .Select(doc =>
                        BsonSerializer.Deserialize<ConsultationModel>(doc.AsBsonDocument)
                    )
                    .ToList();

                return new PaginationResult<ConsultationModel>
                {
                    Data = data,
                    Page = query.Page,
                    Total = (int)total,
                    TotalPages = (int)Math.Ceiling((double)total / query.Size),
                };
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<PaginationResult<ConsultationModel>> GetConsultationsByDoctorId(
            string requesterId,
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
                    // Common stages
                    new BsonDocument(
                        "$lookup",
                        new BsonDocument
                        {
                            ["from"] = "requests",
                            ["localField"] = "requestId",
                            ["foreignField"] = "_id",
                            ["as"] = "request",
                        }
                    ),
                    new BsonDocument(
                        "$unwind",
                        new BsonDocument
                        {
                            ["path"] = "$request",
                            ["preserveNullAndEmptyArrays"] = true,
                        }
                    ),
                    new BsonDocument(
                        "$lookup",
                        new BsonDocument
                        {
                            ["from"] = "users",
                            ["localField"] = "request.doctorId",
                            ["foreignField"] = "_id",
                            ["as"] = "request.doctor",
                        }
                    ),
                    new BsonDocument(
                        "$unwind",
                        new BsonDocument
                        {
                            ["path"] = "$request.doctor",
                            ["preserveNullAndEmptyArrays"] = true,
                        }
                    ),
                    new BsonDocument(
                        "$lookup",
                        new BsonDocument
                        {
                            ["from"] = "users",
                            ["localField"] = "request.memberId",
                            ["foreignField"] = "_id",
                            ["as"] = "request.member",
                        }
                    ),
                    new BsonDocument(
                        "$unwind",
                        new BsonDocument
                        {
                            ["path"] = "$request.member",
                            ["preserveNullAndEmptyArrays"] = true,
                        }
                    ),
                    new BsonDocument(
                        "$lookup",
                        new BsonDocument
                        {
                            ["from"] = "children",
                            ["localField"] = "request.childId",
                            ["foreignField"] = "_id",
                            ["as"] = "request.child",
                        }
                    ),
                    new BsonDocument(
                        "$unwind",
                        new BsonDocument
                        {
                            ["path"] = "$request.child",
                            ["preserveNullAndEmptyArrays"] = false,
                        }
                    ),
                    new BsonDocument(
                        "$match",
                        new BsonDocument
                        {
                            ["request.doctorId"] = new BsonObjectId(ObjectId.Parse(requesterId)),
                            ["status"] = ConsultationStatusEnum.Ongoing.ToString(),
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
                    .Select(doc =>
                        BsonSerializer.Deserialize<ConsultationModel>(doc.AsBsonDocument)
                    )
                    .ToList();

                return new PaginationResult<ConsultationModel>
                {
                    Data = data,
                    Page = query.Page,
                    Total = total,
                    TotalPages = (int)Math.Ceiling((double)total / query.Size),
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<PaginationResult<ConsultationModel>> GetConsultationsByMemberId(
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
                    // Common stages
                    new BsonDocument(
                        "$lookup",
                        new BsonDocument
                        {
                            ["from"] = "requests",
                            ["localField"] = "requestId",
                            ["foreignField"] = "_id",
                            ["as"] = "request",
                        }
                    ),
                    new BsonDocument(
                        "$unwind",
                        new BsonDocument
                        {
                            ["path"] = "$request",
                            ["preserveNullAndEmptyArrays"] = false,
                        }
                    ),
                    new BsonDocument(
                        "$lookup",
                        new BsonDocument
                        {
                            ["from"] = "users",
                            ["localField"] = "request.doctorId",
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
                            ["localField"] = "request.memberId",
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
                            ["localField"] = "request.childId",
                            ["foreignField"] = "_id",
                            ["as"] = "child",
                        }
                    ),
                    new BsonDocument(
                        "$unwind",
                        new BsonDocument
                        {
                            ["path"] = "$child",
                            ["preserveNullAndEmptyArrays"] = false,
                        }
                    ),
                    new BsonDocument(
                        "$match",
                        new BsonDocument
                        {
                            ["request.memberId"] = new BsonObjectId(ObjectId.Parse(memberId)),
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
                    .Select(doc =>
                        BsonSerializer.Deserialize<ConsultationModel>(doc.AsBsonDocument)
                    )
                    .ToList();

                return new PaginationResult<ConsultationModel>
                {
                    Data = data,
                    Page = query.Page,
                    Total = total,
                    TotalPages = (int)Math.Ceiling((double)total / query.Size),
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ConsultationModel> RateConsultationById(string id, int rating)
        {
            try
            {
                var consultation = await _collection.Find(c => c.Id == id).FirstOrDefaultAsync();
                if (consultation == null)
                {
                    throw new Exception("Consultation not found");
                }
                consultation.Rating = rating;
                await _collection.ReplaceOneAsync(c => c.Id == id, consultation);
                return consultation;
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}
