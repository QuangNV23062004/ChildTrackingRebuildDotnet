using System;
using Humanizer;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using RestAPI.Repositories.database;
using RestAPI.Repositories.Interfaces;

namespace RestAPI.Repositories.repositories;

public class Repository<T> : IBaseRepository<T> where T : class
{
    protected readonly IMongoCollection<T> _collection;

    public Repository(IOptions<MongoDBSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        var name = typeof(T).Name;

        if (name.EndsWith("Model"))
            name = name[..^"Model".Length];


        _collection = database.GetCollection<T>(name.Pluralize().ToLower());
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            var filter = Builders<T>.Filter.Eq("isDeleted", false);
            return await _collection.Find(filter).ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to retrieve records.", ex);
        }
    }


    public async Task<T?> GetByIdAsync(string id)
    {
        try
        {
            var objectId = new ObjectId(id);
            var filter = Builders<T>.Filter.And(
                Builders<T>.Filter.Eq("_id", objectId),
                Builders<T>.Filter.Eq("isDeleted", false)
            );

            var result = await _collection.Find(filter).FirstOrDefaultAsync();

            if (result == null)
            {
                throw new KeyNotFoundException($"{typeof(T).Name} with ID '{id}' not found or has been deleted.");
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


    public async Task<T> CreateAsync(T entity)
    {
        try
        {
            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty != null && idProperty.PropertyType == typeof(ObjectId?) && idProperty.GetValue(entity) == null)
            {
                // Generate a new ObjectId and set it
                idProperty.SetValue(entity, ObjectId.GenerateNewId());
            }
            await _collection.InsertOneAsync(entity);

            return entity;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create {typeof(T).Name}.", ex);
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
                new FindOneAndUpdateOptions<T>
                {
                    ReturnDocument = ReturnDocument.After
                });

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
