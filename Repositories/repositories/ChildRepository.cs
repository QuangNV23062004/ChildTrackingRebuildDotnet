using System;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Repositories.database;
using RestAPI.Repositories.interfaces;

namespace RestAPI.Repositories.repositories;

public class ChildRepository : Repository<ChildModel>, IChildRepository
{
    public ChildRepository(IOptions<MongoDBSettings> settings) : base(settings)
    {
    }
    public async Task<PaginationResult<ChildModel>> GetChildrenByUserId(string id, QueryParams query)
    {
        try
        {
            var filter = Builders<ChildModel>.Filter.And(
                Builders<ChildModel>.Filter.Eq("GuardianId", new ObjectId(id)),
                Builders<ChildModel>.Filter.Eq(c => c.IsDeleted, false)
            );

            var total = await _collection.CountDocumentsAsync(filter);

            var page = Math.Max(query.Page, 1);
            var size = Math.Max(query.Size, 1);
            var skip = (page - 1) * size;

            var sortBy = !string.IsNullOrWhiteSpace(query.SortBy) ? query.SortBy : "createdAt";
            var isDescending = query.Order?.ToLower() == "descending";

            var sort = isDescending
                ? Builders<ChildModel>.Sort.Descending(sortBy)
                : Builders<ChildModel>.Sort.Ascending(sortBy);

            var data = await _collection.Find(filter)
                                        .Sort(sort)
                                        .Skip(skip)
                                        .Limit(size)
                                        .ToListAsync();
            
            Console.WriteLine(data);

            return new PaginationResult<ChildModel>
            {
                Page = page,
                Total = (int)total,
                TotalPages = (int)Math.Ceiling((double)total / size),
                Data = data
            };
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to get children by guardian ID", ex);
        }
    }
}
