using System;
using Humanizer;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using RestAPI.Helpers;
using RestAPI.Models.SubModels;
using RestAPI.Repositories.database;
using RestAPI.Repositories.Interfaces;

namespace RestAPI.Repositories.repositories;

public class Repository<T> : IBaseRepository<T>
    where T : class
{
    protected readonly IMongoCollection<T> _collection;

    public Repository(IOptions<MongoDBSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        var name = typeof(T).Name;

        if (name.EndsWith("Model"))
            name = name[..^"Model".Length];

        Console.WriteLine("[Repository] Collection name: " + name.Pluralize().ToLower());
        _collection = database.GetCollection<T>(name.Pluralize().ToLower());
    }

    public async Task<T> CreateAsync(T entity)
    {
        try
        {
            await _collection.InsertOneAsync(entity);
            return entity;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create {typeof(T).Name}.", ex);
        }
    }

    public async Task<IEnumerable<T>> GetAllAsync(PopulationModel[]? populationParams = null)
    {
        try
        {
            var facetPipeline = new List<BsonDocument>
            {
                new BsonDocument("$match", new BsonDocument { ["isDeleted"] = false }),
            };

            if (populationParams != null && populationParams.Length > 0)
            {
                foreach (var param in populationParams)
                {
                    facetPipeline.Add(
                        new BsonDocument(
                            "$lookup",
                            new BsonDocument
                            {
                                ["from"] = param.Collection,
                                ["localField"] = param.LocalField,
                                ["foreignField"] = param.ForeignField,
                                ["as"] = param.As,
                            }
                        )
                    );
                    facetPipeline.Add(
                        new BsonDocument("$unwind", new BsonDocument { ["path"] = $"${param.As}" })
                    );
                }
            }
            facetPipeline.Add(
                new BsonDocument(
                    new BsonDocument(
                        "$facet",
                        new BsonDocument
                        {
                            ["count"] = new BsonArray { new BsonDocument("$count", "total") },
                            ["data"] = new BsonArray
                            {
                                new BsonDocument("$sort", new BsonDocument { ["createdAt"] = -1 }),
                                new BsonDocument("$skip", 0),
                            },
                        }
                    )
                )
            );

            var facetResult = await _collection
                .Aggregate<BsonDocument>(facetPipeline)
                .FirstOrDefaultAsync();
            var dataArray = facetResult?["data"]?.AsBsonArray ?? new BsonArray();
            var data = dataArray
                .Select(doc => BsonSerializer.Deserialize<T>(doc.AsBsonDocument))
                .ToList();

            return data;
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to retrieve records.", ex);
        }
    }

    public async Task<PaginationResult<T>> GetAllAsyncWithPagination(
        QueryParams query,
        PopulationModel[]? populationParams = null
    )
    {
        try
        {
            var facetPipeline = new List<BsonDocument>
            {
                new BsonDocument("$match", new BsonDocument { ["isDeleted"] = false }),
            };

            if (populationParams != null && populationParams.Length > 0)
            {
                foreach (var param in populationParams)
                {
                    facetPipeline.Add(
                        new BsonDocument(
                            "$lookup",
                            new BsonDocument
                            {
                                ["from"] = param.Collection,
                                ["localField"] = param.LocalField,
                                ["foreignField"] = param.ForeignField,
                                ["as"] = param.As,
                            }
                        )
                    );
                    facetPipeline.Add(
                        new BsonDocument("$unwind", new BsonDocument { ["path"] = $"${param.As}" })
                    );
                }
            }

            facetPipeline.Add(
                new BsonDocument(
                    new BsonDocument(
                        "$facet",
                        new BsonDocument
                        {
                            ["count"] = new BsonArray { new BsonDocument("$count", "total") },
                            ["data"] = new BsonArray
                            {
                                new BsonDocument("$sort", new BsonDocument { ["createdAt"] = -1 }),
                                new BsonDocument("$skip", (query.Page - 1) * query.Size),
                                new BsonDocument("$limit", query.Size),
                            },
                        }
                    )
                )
            );

            var facetResult = await _collection
                .Aggregate<BsonDocument>(facetPipeline)
                .FirstOrDefaultAsync();
            var total = facetResult?["count"]?.AsBsonArray.FirstOrDefault()?["total"].AsInt32 ?? 0;
            var dataArray = facetResult?["data"]?.AsBsonArray ?? new BsonArray();
            var data = dataArray
                .Select(doc => BsonSerializer.Deserialize<T>(doc.AsBsonDocument))
                .ToList();

            return new PaginationResult<T>
            {
                Page = query.Page,
                Total = total,
                TotalPages = (int)Math.Ceiling((double)total / query.Size),
                Data = data,
            };
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to retrieve paginated records.", ex);
        }
    }

    public async Task<T?> GetByIdAsync(string id, PopulationModel[]? populationParams = null)
    {
        try
        {
            var facetPipeline = new List<BsonDocument>
            {
                new BsonDocument(
                    "$match",
                    new BsonDocument
                    {
                        ["_id"] = new BsonObjectId(ObjectId.Parse(id)),
                        ["isDeleted"] = false,
                    }
                ),
            };

            if (populationParams != null && populationParams.Length > 0)
            {
                foreach (var param in populationParams)
                {
                    facetPipeline.Add(
                        new BsonDocument(
                            "$lookup",
                            new BsonDocument
                            {
                                ["from"] = param.Collection,
                                ["localField"] = param.LocalField,
                                ["foreignField"] = param.ForeignField,
                                ["as"] = param.As,
                            }
                        )
                    );
                    facetPipeline.Add(
                        new BsonDocument("$unwind", new BsonDocument { ["path"] = $"${param.As}" })
                    );
                }
            }

            var result = await _collection.Aggregate<T>(facetPipeline).FirstOrDefaultAsync();
            if (result == null)
            {
                throw new KeyNotFoundException(
                    $"{typeof(T).Name} with ID '{id}' not found or has been deleted."
                );
            }

            return result;
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Invalid ID format.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to retrieve {typeof(T).Name} by ID.", ex);
        }
    }

    public async Task<T?> UpdateAsync(string id, T entity)
    {
        try
        {
            var objectId = new ObjectId(id);
            var result = await _collection.ReplaceOneAsync(
                Builders<T>.Filter.Eq("_id", objectId),
                entity
            );

            if (result.MatchedCount == 0)
                throw new KeyNotFoundException($"{typeof(T).Name} with ID '{id}' not found.");

            return entity;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to update {typeof(T).Name}.", ex);
        }
    }

    public async Task<T?> DeleteAsync(string id)
    {
        try
        {
            var objectId = new ObjectId(id);
            var filter = Builders<T>.Filter.Eq("_id", objectId);
            var update = Builders<T>.Update.Set("isDeleted", true);

            var updated = await _collection.FindOneAndUpdateAsync(
                filter,
                update,
                new FindOneAndUpdateOptions<T> { ReturnDocument = ReturnDocument.After }
            );

            if (updated == null)
                throw new KeyNotFoundException($"{typeof(T).Name} with ID '{id}' not found.");

            return updated;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to delete {typeof(T).Name}.", ex);
        }
    }
}
